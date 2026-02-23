using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

using PT.APIDefinitions;
using PT.Common.Debugging;
using PT.Common.File;
using PT.ERPTransmissions;
using PT.ImportDefintions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.PlanetTogetherAPI.Importing
{
    /// <summary>
    /// An individual step of the import process. Injects a <see cref="ImporterV2"/> to handle connections, as well as the <see cref="NewImportSettings"/> that define communication with the database.
    /// Each known step subclasses out from this one, keeping track of the SQL tables it needs to pull from in <see cref="TablesForStep"/>.
    /// </summary>
    public class ImportStep
    {
        protected Type m_outputTransmissionType;
        protected NewImportSettings m_settings;

        /// <summary>
        /// Pre-built queries cached so that no calculation is needed at import time
        /// </summary>
        private bool m_include;

        /// <summary>
        /// How much this step weighs in overall import progress. Default is 50.
        /// </summary>
        public virtual double ProgressPoints => 50;

    /// <summary>
    /// How to classify the status of this import step.
    /// </summary>
        public virtual ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Misc;

        public ImportStep(Type a_type, NewImportSettings a_settings, bool a_include = true)
        {
            m_settings = a_settings;
            m_outputTransmissionType = a_type;
            if (!m_outputTransmissionType.IsSubclassOf(typeof(ERPTransmission)) && // general case
                m_outputTransmissionType != typeof(JobT.InternalActivity)) // special case for activities
            {
                DebugException.ThrowInDebug($"Importer attempted to create a step for non-ERP tranmission type {m_outputTransmissionType.Name}");
            }

            m_include = a_include;
        }

        protected IDbCommand GetCommand(IDbConnection connection, string sqlText)
        {
            IDbCommand cmd;
            cmd = new SqlCommand(sqlText, (SqlConnection)connection);
            cmd.CommandTimeout = 0;
            return cmd;
        }

        protected Dictionary<string, ImportTableSettings> CreateTableLookupForImportStep(List<ImportTableSettings> a_tableSettings)
        {
            Dictionary<string, ImportTableSettings> importTablesToUse = new Dictionary<string, ImportTableSettings>();

            foreach (ImportTableSettings importTableSettings in a_tableSettings)
            {
                importTablesToUse.Add(importTableSettings.TableName, importTableSettings);
            }

            return importTablesToUse;
        }

        /// <summary>
        /// Gets references to the tables used in this import step from the overall settings.
        /// </summary>
        protected virtual Dictionary<string, ImportTableSettings> TablesForStep => new Dictionary<string, ImportTableSettings>();


        public virtual Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            DebugException.ThrowInDebug("Attempted to run an import step with no associated tables.");
            return null;
        }


        public bool ShouldStepBeSkipped()
        {
            if (!m_include)
            {
                return true;
            }

            if (TablesForStep.Values.All(x => x.MapInfos.Count == 0))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Iterates over a transmission and clears props that have been marked to be Cleared in the associated data table.
        /// This base method can be used for any transmission that only works with one data table, and has no nested entities.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a_tableName"></param>
        /// <param name="a_t"></param>
        protected virtual void ClearTPropsForTable<T>(string a_tableName, ERPMaintenanceTransmission<T> a_t)
        {
            List<IntegrationProperty> propsToClear = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(a_tableName).ToList();
            foreach (T entity in a_t.Nodes)
            {
                ClearPropertyInternal(propsToClear, entity);
            }
        }

        protected static void ClearPropertyInternal<T>(IEnumerable<IntegrationProperty> a_propsToClear, T a_entity)
        {
            foreach (IntegrationProperty prop in a_propsToClear)
            {
                PropertyInfo propInfo = typeof(T).GetProperty(prop.ColumnName, BindingFlags.Public | BindingFlags.Instance);
                if (propInfo == null)
                {
                    DebugException.ThrowInDebug($"Property on {a_entity.GetType().Name} does not match property {prop.ColumnName}. " +
                                                $"These should be aligned, or custom logic is needed in an override of this method.");
                    continue;
                }
                    
                // The transmissions follow a convention where calling the Set method flags the property as set,
                // and only set properties are updated when the corresponding manager's Update method is called.
                // By explicitly setting the Cleared property to its default value, we ensure that default is used on update.
                // TODO: Bug watch - the import should always be importing Cleared columns as null, which means the Get value on the transmission should be the default.
                // TODO: But if that convention isn't followed, this may break. Probably best to fix the convention, but if we have issues,
                // TODO: we may want to call Activator.CreateInstance() and use a new object to get the default GetMethod value (this would have performance implications).
                propInfo.SetMethod.Invoke(a_entity, [propInfo.GetMethod.Invoke(a_entity, [])]);
            }
        }

        public enum EImportStepTErrorReason { GetDataError, ValidationError }

        public struct ImportStepTError
        {
            public EImportStepTErrorReason ErrorReason { get; private set; }
            public ExceptionDescriptionInfo[] Exceptions { get; private set; }
            //public string[] ErrorMessages { get; private set; }

            public ImportStepTError(EImportStepTErrorReason a_reason, Exception[] a_errorMessages)
            {
                ErrorReason = a_reason;
                Exceptions = a_errorMessages.Select(err => new ExceptionDescriptionInfo(err)).ToArray();
            }

            public ImportStepTError(EImportStepTErrorReason a_reason, ApplicationExceptionList a_applicationExceptionList)
            {
                ErrorReason = a_reason;
                Exceptions = a_applicationExceptionList.Select(s => s.Data).ToArray();
            }
        }
    }

    public class ImportPlantsStep : ImportStep
    {
        public ImportPlantsStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(PlantT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.PlantSettings,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Plants;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string sqlText = m_settings.PlantSettings.GetCommandText(true);
                    IDbCommand command = GetCommand(connection, sqlText);

                    PlantT plantT = new();
                    plantT.AutoDeleteMode = m_settings.PlantSettings.AutoDelete;
                    plantT.Fill(command);
                    ClearTPropsForTable(m_settings.PlantSettings.TableName, plantT);
                    return new(plantT);
                }
                catch (Exception err)
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }
    }

    public class ImportDepartmentsStep : ImportStep
    {
        public ImportDepartmentsStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(DepartmentT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.DepartmentSettings,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Departments;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string sqlText = m_settings.DepartmentSettings.GetCommandText(true);
                    IDbCommand command = GetCommand(connection, sqlText);
                    DepartmentT deptT = new();
                    deptT.AutoDeleteMode = m_settings.DepartmentSettings.AutoDelete;
                    deptT.Fill(command);
                    ClearTPropsForTable(m_settings.DepartmentSettings.TableName, deptT);
                    return new(deptT);
                }
                catch (Exception err)
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }
    }

    public class ImportCustomersStep : ImportStep
    {
        public ImportCustomersStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(CustomerT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.CustomerSettings,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Customers;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string sqlTable = m_settings.CustomerSettings.GetCommandText(true);
                    IDbCommand cmdTable = GetCommand(connection, sqlTable);
                    CustomerT customerT = new();

                    customerT.Fill(cmdTable);
                    ClearTPropsForTable(m_settings.CustomerSettings.TableName, customerT);
                    customerT.AutoDeleteMode = m_settings.CustomerSettings.AutoDelete;
                    return new(customerT);
                }
                catch (Exception err)
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }
    }

    public class ImportUDFStep : ImportStep
    {
        public ImportUDFStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(CustomerT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.UserFieldSettings,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.UserFields;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string sqlTable = m_settings.UserFieldSettings.GetCommandText(true);
                    IDbCommand cmdTable = GetCommand(connection, sqlTable);
                    UserFieldDefinitionT userFieldDefinitionT = new();

                    userFieldDefinitionT.Fill(cmdTable);
                    ClearTPropsForTable(m_settings.UserFieldSettings.TableName, userFieldDefinitionT);
                    userFieldDefinitionT.AutoDeleteMode = m_settings.UserFieldSettings.AutoDelete;
                    return new(userFieldDefinitionT);
                }
                catch (Exception err)
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }
    }
    

    public class ImportCapabilitiesStep : ImportStep
    {
        public ImportCapabilitiesStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(CapabilityT), a_importSettings, a_include) { }

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Capabilities;

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.CapabilitySettings,
        });


        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string sqlText = m_settings.CapabilitySettings.GetCommandText(true);
                    IDbCommand command = GetCommand(connection, sqlText);
                    CapabilityT capT = new();
                    capT.Fill(command);
                    ClearTPropsForTable(m_settings.CapabilitySettings.TableName, capT);
                    capT.AutoDeleteMode = m_settings.CapabilitySettings.AutoDelete;
                    return new(capT);
                }
                catch (Exception err)
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }
    }

    public class ImportCellsStep : ImportStep
    {
        public ImportCellsStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(CellT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.CellsSettings,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Cells;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string cellSqlText = m_settings.CellsSettings.GetCommandText(true);
                    IDbCommand cellsCmd = GetCommand(connection, cellSqlText);

                    CellT cellT = new();

                    cellT.Fill(cellsCmd);
                    ClearTPropsForTable(m_settings.CellsSettings.TableName, cellT);

                    cellT.AutoDeleteMode = m_settings.CellsSettings.AutoDelete;
                    return new(cellT);
                }
                catch (Exception err)
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }
    }

    public class ImportResourcesStep : ImportStep
    {

        public ImportResourcesStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(ResourceT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.MachineSettings,
            m_settings.CapabilityAssignmentSettings,
            m_settings.AllowedHelperResourcesSettings

        });
        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Resources;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string machSqlText = m_settings.MachineSettings.GetCommandText(true);
                    IDbCommand machCommand = GetCommand(connection, machSqlText);

                    string machCapabAssSqlText = m_settings.CapabilityAssignmentSettings.GetCommandText(true);
                    string allowedHelpersSqlText = m_settings.AllowedHelperResourcesSettings.GetCommandText(true);
                    IDbCommand machCapabAssCommand = GetCommand(connection, machCapabAssSqlText);
                    IDbCommand allowedHelpersCommand = GetCommand(connection, allowedHelpersSqlText);

                    ResourceT resourceT = new();
                    resourceT.AutoDeleteMode = m_settings.MachineSettings.AutoDelete;
                    resourceT.AutoDeleteCapabilityAssociations = m_settings.CapabilityAssignmentSettings.AutoDelete;
                    resourceT.AutoDeleteAllowedHelpers = m_settings.AllowedHelperResourcesSettings.AutoDelete;
                    resourceT.UpdateAllowedHelpers = m_settings.AllowedHelperResourcesSettings.HasNonEmptySourceExpression();
                    resourceT.Fill(machCommand, machCapabAssCommand, m_settings.CapabilityAssignmentSettings.HasNonEmptySourceExpression(), m_settings.AllowedHelperResourcesSettings.HasNonEmptySourceExpression(), allowedHelpersCommand);
                    ClearTPropsForTable(null, resourceT);

                    return new(resourceT);
                }
                catch (Exception err)
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }

        protected override void ClearTPropsForTable<T>(string _, ERPMaintenanceTransmission<T> a_t)
        {
            if (a_t is not ResourceT resourceT)
            {
                throw new ArgumentException($"Error in {nameof(ImportResourcesStep)} overload of {nameof(ClearTPropsForTable)}");
            }

            IEnumerable<IntegrationProperty> resourceProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.MachineSettings.TableName).ToList();
            IEnumerable<IntegrationProperty> allowedHelperProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.AllowedHelperResourcesSettings.TableName);
            // Capabilities props not actually used currently (just FK on resource). TODO: Do we need to clear on this?
            //IEnumerable<IntegrationProperty> CapProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.CapabilityAssignmentSettings.TableName);

            foreach (ResourceT.Resource node in resourceT.Nodes)
            {
                ClearPropertyInternal(resourceProps, node);

                foreach (BaseResource.AllowedHelper helper in node.AllowedHelpers)
                {
                    ClearPropertyInternal(allowedHelperProps, helper);
                }
            }
        }
    }

    public class ImportResourceConnectorsStep : ImportStep
    {
        public ImportResourceConnectorsStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(ResourceConnectorsT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.ResourceConnectionSettings,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.ResourceConnectors;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string sqlTable = m_settings.ResourceConnectorsSettings.GetCommandText(true);
                    IDbCommand cmdTables = GetCommand(connection, sqlTable);
                    string sqlResourceConnection = m_settings.ResourceConnectionSettings.GetCommandText(true);
                    IDbCommand cmdResourceConnection = GetCommand(connection, sqlResourceConnection);
                    ResourceConnectorsT resConnectorsT = new ();

                    ApplicationExceptionList errors = new ();
                    resConnectorsT.Fill(cmdTables, cmdResourceConnection, errors);
                    ClearTPropsForTable(null, resConnectorsT);

                    if (errors.Any())
                    {
                        return new (new ImportStepTError(EImportStepTErrorReason.GetDataError, errors));
                    }

                    resConnectorsT.AutoDeleteMode = m_settings.ResourceConnectorsSettings.AutoDelete;
                    resConnectorsT.AutoDeleteConnections = m_settings.ResourceConnectionSettings.AutoDelete;
                    return new (resConnectorsT);
                }
                catch (Exception err)
                {
                    return new (new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }

        protected override void ClearTPropsForTable<T>(string _, ERPMaintenanceTransmission<T> a_t)
        {
            if (a_t is not ResourceConnectorsT resConnectorsT)
            {
                throw new ArgumentException($"Error in {nameof(ImportResourceConnectorsStep)} overload of {nameof(ClearTPropsForTable)}");
            }

            List<IntegrationProperty> connectorProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.ResourceConnectorsSettings.TableName).ToList();
            //List<IntegrationProperty> connectionProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.ResourceConnectionSettings.TableName).ToList();

            foreach (ResourceConnectorsT.ResourceConnector node in resConnectorsT.Nodes)
            {
                ClearPropertyInternal(connectorProps, node);

                // TODO: ResourceConnection has no settable props. Any additional work needed?
                //node.FromResources = node.FromResources;
                //node.ToResources = node.ToResources;
            }
        }
    }

    // TODO: This isn't referenced anywhere. I have a feeling this isn't actually possible to do, but we should investigate.
    public class ImportItemsOnlyStep : ImportStep
    {
        public ImportItemsOnlyStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(WarehouseT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            // TODO: as above, this doesn't seem like the right list of tables for an items-only import
            m_settings.WarehouseSettings,
            m_settings.PlantWarehouseSettings,
            m_settings.InventorySettings,
            m_settings.LotsSettings,
            m_settings.ItemSettings,
            m_settings.CleanoutTriggerTablesSettings,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Items;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string sqlText = m_settings.ItemSettings.GetCommandText(true);
                    IDbCommand command = GetCommand(connection, sqlText);

                    WarehouseT warehouseT = new();
                    warehouseT.AutoDeleteItems = m_settings.ItemSettings.AutoDelete;
                    warehouseT.Fill(null, null, null, null, command, null, null, null ,null, null, null, null,null);
                    ClearTPropsForTable(m_settings.ItemSettings.TableName, warehouseT);
                    return new(warehouseT);
                }
                catch (Exception err)
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }
    }


    public class ImportWarehousesStep : ImportStep
    {
        public ImportWarehousesStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(WarehouseT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.WarehouseSettings,
            m_settings.PlantWarehouseSettings,
            m_settings.InventorySettings,
            m_settings.LotsSettings,
            m_settings.ItemSettings,
            m_settings.StorageAreaSettings,
            m_settings.ItemStorageSettings,
            m_settings.ItemStorageLotsSettings,
            m_settings.StorageAreaConnectorSettings,
            m_settings.StorageAreaConnectorOutSettings,
            m_settings.StorageAreaConnectorInSettings,
            m_settings.ResourceStorageAreaConnectorInSettings,
            m_settings.ResourceStorageAreaConnectorOutSettings,
        });

        public override double ProgressPoints
        {
            get
            {
                double progressPoints = 50;
                if (m_settings.LotsSettings.HasNonEmptySourceExpression())
                {
                    progressPoints += 50;
                }
                if (m_settings.InventorySettings.HasNonEmptySourceExpression())
                {
                    progressPoints += 50;
                }
                if (m_settings.ItemSettings.HasNonEmptySourceExpression())
                {
                    progressPoints += 50;
                }

                return progressPoints;
            }
        }

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Warehouses;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                using (IDbConnection plantWarehouseConnection = a_importer.OpenNewConnection())
                {
                    string warehouseSqlText = m_settings.WarehouseSettings.GetCommandText(true);
                    IDbCommand warehouseCommand = GetCommand(connection, warehouseSqlText);

                    string plantWarehouseSqlText = m_settings.PlantWarehouseSettings.GetCommandText(true);
                    IDbCommand plantWarehouseCommand = GetCommand(plantWarehouseConnection, plantWarehouseSqlText);

                    IDbCommand inventoryCommand = null;
                    IDbCommand itemsCommand = null;
                    IDbCommand lotsCommand = null;
                    IDbCommand storageAreaCommand = null;
                    IDbCommand storageAreaConnectorCommand = null;
                    IDbCommand storageAreaConnectorsInCommand = null;
                    IDbCommand storageAreaConnectorsOutCommand = null;
                    IDbCommand resourceStorageAreaConnectorsInCommand = null;
                    IDbCommand resourceStorageAreaConnectorsOutCommand = null;
                    IDbCommand itemStorageCommand = null;
                    IDbCommand itemStorageLotsCommand = null;

                    IDbConnection inventoryConnection = null;
                    IDbConnection itemsConnection = null;
                    IDbConnection lotsConnection = null;
                    IDbConnection storageAreaConnection = null;
                    IDbConnection storageAreaConnectorConnection = null;
                    IDbConnection storageAreaConnectorsInConnection = null;
                    IDbConnection storageAreaConnectorsOutConnection = null;
                    IDbConnection resourceStorageAreaConnectorsInConnection = null;
                    IDbConnection resourceStorageAreaConnectorsOutConnection = null;
                    IDbConnection itemStorageConnection = null;
                    IDbConnection itemStorageLotsConnection = null;
                    try
                    {
                        WarehouseT warehouseT = new();
                        warehouseT.AutoDeleteMode = m_settings.WarehouseSettings.AutoDelete;

                        //Inventories
                        if (m_settings.InventorySettings.HasNonEmptySourceExpression())
                        {
                            inventoryConnection = a_importer.OpenNewConnection();
                            string inventorySqlText = m_settings.InventorySettings.GetCommandText(true);
                            inventoryCommand = GetCommand(inventoryConnection, inventorySqlText);
                            warehouseT.AutoDeleteInventories = m_settings.InventorySettings.AutoDelete;

                            //Lots
                            if (m_settings.LotsSettings.HasNonEmptySourceExpression())
                            {
                                lotsConnection = a_importer.OpenNewConnection();
                                string lotsSqlText = m_settings.LotsSettings.GetCommandText(true);
                                lotsCommand = GetCommand(lotsConnection, lotsSqlText);
                                warehouseT.AutoDeleteLots = m_settings.LotsSettings.AutoDelete;
                            }
                        }

                        //Items
                        if (m_settings.ItemSettings.HasNonEmptySourceExpression())
                        {
                            itemsConnection = a_importer.OpenNewConnection();
                            string itemsSqlText = m_settings.ItemSettings.GetCommandText(true);
                            itemsCommand = GetCommand(itemsConnection, itemsSqlText);
                            warehouseT.AutoDeleteItems = m_settings.ItemSettings.AutoDelete;
                        }
                        
                        //Storage Area
                        if (m_settings.StorageAreaSettings.HasNonEmptySourceExpression())
                        {
                            storageAreaConnection = a_importer.OpenNewConnection();
                            string storageAreaSqlText = m_settings.StorageAreaSettings.GetCommandText(true);
                            storageAreaCommand = GetCommand(storageAreaConnection, storageAreaSqlText);
                            warehouseT.AutoDeleteStorageAreas = m_settings.StorageAreaSettings.AutoDelete;
                        }
                            
                        //Item Storage
                        if (m_settings.ItemStorageSettings.HasNonEmptySourceExpression())
                        {
                            itemStorageConnection = a_importer.OpenNewConnection();
                            string itemStorageSqlText = m_settings.ItemStorageSettings.GetCommandText(true);
                            itemStorageCommand = GetCommand(itemStorageConnection, itemStorageSqlText);
                            warehouseT.AutoDeleteItemStorage = m_settings.ItemStorageSettings.AutoDelete;
                        }

                        //Item Storage Lots
                        if (m_settings.ItemStorageLotsSettings.HasNonEmptySourceExpression())
                        {
                            itemStorageLotsConnection = a_importer.OpenNewConnection();
                            string itemStorageLotsSqlText = m_settings.ItemStorageLotsSettings.GetCommandText(true);
                            itemStorageLotsCommand = GetCommand(itemStorageLotsConnection, itemStorageLotsSqlText);
                            warehouseT.AutoDeleteItemStorageLots = m_settings.ItemStorageLotsSettings.AutoDelete;
                        }

                        //Storage Area Connectors
                        if (m_settings.StorageAreaConnectorSettings.HasNonEmptySourceExpression())
                        {
                            storageAreaConnectorConnection = a_importer.OpenNewConnection();
                            string storageAreaConnectorSqlText = m_settings.StorageAreaConnectorSettings.GetCommandText(true);
                            storageAreaConnectorCommand = GetCommand(storageAreaConnectorConnection, storageAreaConnectorSqlText);
                            warehouseT.AutoDeleteStorageAreaConnectors = m_settings.StorageAreaConnectorSettings.AutoDelete;

                            //Storage Area Connectors In
                            if (m_settings.StorageAreaConnectorInSettings.HasNonEmptySourceExpression())
                            {
                                storageAreaConnectorsInConnection = a_importer.OpenNewConnection();
                                string storageAreaConnectorsInSqlText = m_settings.StorageAreaConnectorInSettings.GetCommandText(true);
                                storageAreaConnectorsInCommand = GetCommand(storageAreaConnectorsInConnection, storageAreaConnectorsInSqlText);
                                warehouseT.AutoDeleteStorageAreaConnectorsIn = m_settings.StorageAreaConnectorInSettings.AutoDelete;
                            }

                            //Storage Area Connectors Out
                            if (m_settings.StorageAreaConnectorOutSettings.HasNonEmptySourceExpression())
                            {
                                storageAreaConnectorsOutConnection = a_importer.OpenNewConnection();
                                string storageAreaConnectorsOutSqlText = m_settings.StorageAreaConnectorOutSettings.GetCommandText(true);
                                storageAreaConnectorsOutCommand = GetCommand(storageAreaConnectorsOutConnection, storageAreaConnectorsOutSqlText);
                                warehouseT.AutoDeleteStorageAreaConnectorsOut = m_settings.StorageAreaConnectorOutSettings.AutoDelete;
                            }

                            //Resource Storage Area Connectors In
                            if (m_settings.ResourceStorageAreaConnectorInSettings.HasNonEmptySourceExpression())
                            {
                                resourceStorageAreaConnectorsInConnection = a_importer.OpenNewConnection();
                                string resourceStorageAreaConnectorsInSqlText = m_settings.ResourceStorageAreaConnectorInSettings.GetCommandText(true);
                                resourceStorageAreaConnectorsInCommand = GetCommand(resourceStorageAreaConnectorsInConnection, resourceStorageAreaConnectorsInSqlText);
                                warehouseT.AutoDeleteResourceStorageAreaConnectorIn = m_settings.ResourceStorageAreaConnectorInSettings.AutoDelete;
                            }

                            //Resource Storage Area Connectors Out
                            if (m_settings.ResourceStorageAreaConnectorOutSettings.HasNonEmptySourceExpression())
                            {
                                resourceStorageAreaConnectorsOutConnection = a_importer.OpenNewConnection();
                                string resourceStorageAreaConnectorsOutSqlText = m_settings.ResourceStorageAreaConnectorOutSettings.GetCommandText(true);
                                resourceStorageAreaConnectorsOutCommand = GetCommand(resourceStorageAreaConnectorsOutConnection, resourceStorageAreaConnectorsOutSqlText);
                                warehouseT.AutoDeleteResourceStorageAreaConnectorOut = m_settings.ResourceStorageAreaConnectorOutSettings.AutoDelete;
                            }
                        }

                        warehouseT.Fill(warehouseCommand, plantWarehouseCommand, inventoryCommand, lotsCommand, itemsCommand, storageAreaCommand, itemStorageCommand, itemStorageLotsCommand, storageAreaConnectorCommand, storageAreaConnectorsInCommand, storageAreaConnectorsOutCommand,resourceStorageAreaConnectorsInCommand,resourceStorageAreaConnectorsOutCommand);
                        // TODO: handle storage lots clear
                        ClearTPropsForTable(null, warehouseT);
                        return new(warehouseT);

                    }
                    catch (Exception err)
                    {
                        return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                    }
                    finally
                    {
                        //Need to dispose of manually since not in Using clause.
                        if (inventoryConnection != null)
                        {
                            if (inventoryConnection.State != ConnectionState.Closed)
                            {
                                inventoryConnection.Close();
                            }

                            inventoryConnection.Dispose();
                        }

                        if (itemsConnection != null)
                        {
                            if (itemsConnection.State != ConnectionState.Closed)
                            {
                                itemsConnection.Close();
                            }

                            itemsConnection.Dispose();
                        }

                        if (lotsConnection != null)
                        {
                            if (lotsConnection.State != ConnectionState.Closed)
                            {
                                lotsConnection.Close();
                            }

                            lotsConnection.Dispose();
                        }
                    }
                }
            }
        }

        protected override void ClearTPropsForTable<T>(string _, ERPMaintenanceTransmission<T> a_t)
        {
            if (a_t is not WarehouseT warehouseT)
            {
                throw new ArgumentException($"Error in {nameof(ImportWarehousesStep)} overload of {nameof(ClearTPropsForTable)}");
            }

            List<IntegrationProperty> warehouseProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.WarehouseSettings.TableName).ToList();
            foreach (WarehouseT.Warehouse node in warehouseT.Nodes)
            {
                ClearPropertyInternal(warehouseProps, node);
            }

            List<IntegrationProperty> itemProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.ItemSettings.TableName).ToList();
            foreach (WarehouseT.Item node in warehouseT.ItemsList)
            {
                ClearPropertyInternal(itemProps, node);
            }
        }
    }

    public class ImportJobsStep : ImportStep
    {
        private bool m_include;
        JobDataSet jobDataSet = null;
        private HashSet<string> jobTActivityUniqueIds = null;

        public ImportJobsStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(JobT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.JobSettings,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Jobs;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            a_importer.jobDataSet = a_importer.FetchJobDataSet(a_importer);
            a_importer.jobTActivityUniqueIds = new(); // this holds ERPTransmissions.Activity.GetUniqueKey() for every Activity that's added to JobT. Used to determine if ActivityUpdateT should be sent.

            JobT jobT = new();
            jobT.AutoDeleteMode = m_settings.JobSettings.AutoDelete;
            try
            {
                ApplicationExceptionList errors = new();
                jobT.Fill(ref errors, a_importer.jobDataSet, !m_settings.PathSettings.HasNonEmptySourceExpression(), a_importer.jobTActivityUniqueIds);
                ClearTPropsForTable(null, jobT);

                if (jobT.Count <= 0)
                {
                    errors.Add(new ImportException("2719"));
                }

                if (errors.Any())
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, errors));
                }

                return new(jobT);
            }
            catch (ImportException interfaceErr)
            {
                return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [interfaceErr]));
            }
            catch (Exception err)
            {
                return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
            }
        }

        protected override void ClearTPropsForTable<T>(string _, ERPMaintenanceTransmission<T> a_t)
        {
            if (a_t is not JobT jobT)
            {
                throw new ArgumentException($"Error in {nameof(ImportJobsStep)} overload of {nameof(ClearTPropsForTable)}");
            }

            List<IntegrationProperty> jobProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.JobSettings.TableName).ToList();
            List<IntegrationProperty> moProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.MoSettings.TableName).ToList();
            List<IntegrationProperty> opProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.ResourceOperationSettings.TableName).ToList();
            List<IntegrationProperty> reqProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.ResourceRequirementSettings.TableName).ToList();
            List<IntegrationProperty> actProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.InternalActivitySettings.TableName).ToList();
            List<IntegrationProperty> matProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.MaterialSettings.TableName).ToList();
            List<IntegrationProperty> prodProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.ProductSettings.TableName).ToList();
            List<IntegrationProperty> attProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.OpAttributeSettings.TableName).ToList();
            List<IntegrationProperty> pathProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.PathSettings.TableName).ToList();
            List<IntegrationProperty> sucProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.SuccessorMoSettings.TableName).ToList();

            foreach (JobT.Job node in jobT.Nodes)
            {
                ClearPropertyInternal(jobProps, node);

                for (int i = 0; i < node.ManufacturingOrderCount; i++)
                {
                    JobT.ManufacturingOrder mo = node[i];
                    ClearPropertyInternal(moProps, mo);

                    for (int j = 0; j < mo.PathCount; j++)
                    {
                        JobT.AlternatePath path = mo.GetAlternatePath(j);
                        ClearPropertyInternal(pathProps, path);
                    }

                    for (var j = 0; j < mo.SuccessorMOs.Count; j++)
                    {
                        JobT.SuccessorMO suc = mo.SuccessorMOs[j];
                        ClearPropertyInternal(sucProps, suc);
                    }

                    for (int j = 0; j < mo.OperationCount; j++)
                    {
                        JobT.ResourceOperation op = (JobT.ResourceOperation)mo.GetOperation(i);
                        ClearPropertyInternal(opProps, op);

                        for (int k = 0; k < op.ResourceRequirementCount; k++)
                        {
                            JobT.InternalOperation.ResourceRequirement req = op.GetResourceRequirement(k);
                            ClearPropertyInternal(reqProps, req);
                        }

                        for (int k = 0; k < op.InternalActivityCount; k++)
                        {
                            JobT.InternalActivity act = op.GetInternalActivity(k);
                            ClearPropertyInternal(actProps, act);
                        }

                        for (int k = 0; k < op.MaterialRequirementCount; k++)
                        {
                            JobT.MaterialRequirement mat = op.GetMaterialRequirement(k);
                            ClearPropertyInternal(matProps, mat);
                        }

                        for (int k = 0; k < op.ProductCount; k++)
                        {
                            JobT.Product prod = op.GetProduct(k);
                            ClearPropertyInternal(prodProps, prod);
                        }

                        foreach (OperationAttribute att in op.ResourceAttributes)
                        {
                            ClearPropertyInternal(attProps, att);
                        }
                    }
                }
            }
        }
    }

    public class ImportInternalActivityStep : ImportStep
    {
        public ImportInternalActivityStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(JobT.InternalActivity), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.InternalActivitySettings,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Activities;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    ActivityUpdateT actUpdateT = a_importer.GetActivityUpdateT(a_importer.jobDataSet, a_importer.jobTActivityUniqueIds);
                    if (actUpdateT != null && actUpdateT.Count > 0)
                    {
                        // TODO: This Transmission has no settable properties, so Update should work as intended. Review if this changes.
                        //ClearTPropsForTable(m_settings.InternalActivitySettings.TableName, actUpdateT);
                        actUpdateT.Validate();
                        return new(actUpdateT);
                    }

                    // TODO: Unclear what outcome should be here. In original importer, it simply wouldn't add to the importT but would move on.
                    return new(null);

                }
                catch (Exception err)
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }
    }


    public class ImportCapacityIntervalsStep : ImportStep
    {
        public ImportCapacityIntervalsStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(CapacityIntervalT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.CapacityIntervalSettings,
            m_settings.CapacityIntervalResourceSettings,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Capacity;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string ciSqlTxt = m_settings.CapacityIntervalSettings.GetCommandText(true);
                    string resSqlTxt = m_settings.CapacityIntervalResourceSettings.GetCommandText(true);
                    IDbCommand ciCmd = GetCommand(connection, ciSqlTxt);
                    IDbCommand resCmd = GetCommand(connection, resSqlTxt);
                    CapacityIntervalT cit = new();
                    cit.AutoDeleteMode = m_settings.CapacityIntervalSettings.AutoDelete;
                    cit.AutoDeleteResourceAssociations = m_settings.CapacityIntervalResourceSettings.AutoDelete;
                    cit.Fill(ciCmd, resCmd);
                    // TODO: This might need an overload with both CapacityIntervalSettings and CapacityIntervalResourceSettings managed
                    ClearTPropsForTable(null, cit);
                    return new(cit);
                }
                catch (Exception err)
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }

        protected override void ClearTPropsForTable<T>(string a_tableName, ERPMaintenanceTransmission<T> a_t)
        {
            if (a_t is not CapacityIntervalT cit)
            {
                throw new ArgumentException($"Error in {nameof(ImportCapacityIntervalsStep)} overload of {nameof(ClearTPropsForTable)}");
            }

            List<IntegrationProperty> citProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.CapacityIntervalSettings.TableName).ToList();
            // TODO: Review CapacityIntervalResourceSettings for any Clear props needing implementation
            foreach (CapacityIntervalT.CapacityIntervalDef node in cit.Nodes)
            {
                ClearPropertyInternal(citProps, node);
            }
        }
    }

    public class ImportRecurringCapacityIntervalsStep : ImportStep
    {
        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Capacity;

        public ImportRecurringCapacityIntervalsStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(RecurringCapacityIntervalT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.RecurringCapacityIntervalSettings,
            m_settings.CapacityIntervalResourceSettings,
        });


        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string sqlText = m_settings.RecurringCapacityIntervalSettings.GetCommandText(true);
                    string resSqlTxt = m_settings.CapacityIntervalResourceSettings.GetCommandText(true);

                    IDbCommand command = GetCommand(connection, sqlText);
                    IDbCommand resCmd = GetCommand(connection, resSqlTxt);
                    RecurringCapacityIntervalT rciT = new();

                    rciT.AutoDeleteMode = m_settings.RecurringCapacityIntervalSettings.AutoDelete;
                    rciT.AutoDeleteResourceAssociations = m_settings.CapacityIntervalResourceSettings.AutoDelete;
                    rciT.Fill(command, resCmd);
                    ClearTPropsForTable(null, rciT);
                    return new(rciT);
                }
                catch (Exception err)
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }

        protected override void ClearTPropsForTable<T>(string a_tableName, ERPMaintenanceTransmission<T> a_t)
        {
            if (a_t is not RecurringCapacityIntervalT rciT)
            {
                throw new ArgumentException($"Error in {nameof(ImportResourcesStep)} overload of {nameof(ClearTPropsForTable)}");
            }

            // TODO: Handle Resource Settings if Clearable
            List<IntegrationProperty> rcitProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.RecurringCapacityIntervalSettings.TableName).ToList();
            foreach (RecurringCapacityIntervalDef node in rciT.Nodes)
            {
                ClearPropertyInternal(rcitProps, node);
            }
        }
    }

    public class ImportProductRulesStep : ImportStep
    {
        public ImportProductRulesStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(ProductRulesT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.ProductRulesSettings,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.ProductRules;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string sqlText = m_settings.ProductRulesSettings.GetCommandText(true);
                    IDbCommand command = GetCommand(connection, sqlText);
                    ProductRulesT prt = new();
                    prt.Fill(command);
                    ClearTPropsForTable(m_settings.ProductRulesSettings.TableName, prt);
                    prt.AutoDeleteMode = m_settings.ProductRulesSettings.AutoDelete;
                    return new(prt);
                }
                catch (Exception err)
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }
    }

    public class ImportAttributesStep : ImportStep
    {
        public ImportAttributesStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(PTAttributeT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.AttributeSettings,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Attributes;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string sqlText = m_settings.AttributeSettings.GetCommandText(true);
                    IDbCommand command = GetCommand(connection, sqlText);
                    PTAttributeT ptAttributeT = new();
                    ptAttributeT.Fill(command);
                    ClearTPropsForTable(m_settings.AttributeSettings.TableName, ptAttributeT);
                    ptAttributeT.AutoDeleteMode = m_settings.AttributeSettings.AutoDelete;
                    return new(ptAttributeT);
                }
                catch (Exception err)
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }
    }

    public class ImportAttributesSetupTableStep : ImportStep
    {
        public ImportAttributesSetupTableStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(LookupAttributeNumberRangeT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.SetupTableAttSettings,
            m_settings.SetupTableAttFromSettings,
            m_settings.SetupTableAttToSettings,
            m_settings.SetupTableAttResourceSettings,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Attributes;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string sqlTable = m_settings.SetupTableAttSettings.GetCommandText(true);
                    IDbCommand cmdTable = GetCommand(connection, sqlTable);
                    string sqlAtt = m_settings.SetupTableAttNameSettings.GetCommandText(true);
                    IDbCommand cmdAtt = GetCommand(connection, sqlAtt);
                    string sqlFrom = m_settings.SetupTableAttFromSettings.GetCommandText(true);
                    IDbCommand cmdFrom = GetCommand(connection, sqlFrom);
                    string sqlTo = m_settings.SetupTableAttToSettings.GetCommandText(true);
                    IDbCommand cmdTo = GetCommand(connection, sqlTo);
                    string sqlRes = m_settings.SetupTableAttResourceSettings.GetCommandText(true);
                    IDbCommand cmdRes = GetCommand(connection, sqlRes);
                    LookupAttributeNumberRangeT prt = new();
                    prt.Fill(cmdTable, cmdAtt, cmdFrom, cmdTo, cmdRes);
                    ClearTPropsForTable(null, prt);
                    prt.AutoDeleteMode = m_settings.SetupTableAttSettings.AutoDelete;
                    return new(prt);
                }
                catch (Exception err)
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }

        protected override void ClearTPropsForTable<T>(string _, ERPMaintenanceTransmission<T> a_t)
        {
            if (a_t is not LookupAttributeNumberRangeT prt)
            {
                throw new ArgumentException($"Error in {nameof(ImportAttributesSetupTableStep)} overload of {nameof(ClearTPropsForTable)}");
            }

            // TODO: Handle additional tables?
            List<IntegrationProperty> attProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.SetupTableAttSettings.TableName).ToList();
            foreach (LookupAttributeNumberRangeT.LookupAttributeNumberRangeTable node in prt.Nodes)
            {
                ClearPropertyInternal(attProps, node);
            }
        }
    }

    public class ImportAttributeCodeTableStep : ImportStep
    {
        public ImportAttributeCodeTableStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(LookupAttributeCodeTableT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.AttributeCodeTableSetting,
            m_settings.AttributeCodeTableAttributeExternalIdSetting,
            m_settings.AttributeCodeTableAttributeCodesSetting,
            m_settings.AttributeCodeTableAssignedResourcesSetting,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Attributes;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string sqlTable = m_settings.AttributeCodeTableSetting.GetCommandText(true);
                    IDbCommand cmdTable = GetCommand(connection, sqlTable);
                    string sqlAtt = m_settings.AttributeCodeTableAttributeExternalIdSetting.GetCommandText(true);
                    IDbCommand cmdAtt = GetCommand(connection, sqlAtt);
                    string sqlCodes = m_settings.AttributeCodeTableAttributeCodesSetting.GetCommandText(true);
                    IDbCommand cmdCodes = GetCommand(connection, sqlCodes);
                    string sqlRes = m_settings.AttributeCodeTableAssignedResourcesSetting.GetCommandText(true);
                    IDbCommand cmdRes = GetCommand(connection, sqlRes);
                    LookupAttributeCodeTableT prt = new();
                    prt.Fill(cmdTable, cmdAtt, cmdCodes, cmdRes);
                    ClearTPropsForTable(null, prt);

                    prt.AutoDeleteMode = m_settings.AttributeCodeTableSetting.AutoDelete;
                    return new(prt);
                }
                catch (Exception err)
                {
                    return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }

        protected override void ClearTPropsForTable<T>(string a_tableName, ERPMaintenanceTransmission<T> a_t)
        {
            if (a_t is not LookupAttributeCodeTableT prt)
            {
                throw new ArgumentException($"Error in {nameof(ImportAttributeCodeTableStep)} overload of {nameof(ClearTPropsForTable)}");
            }

            // TODO: Clear additional tables?
            List<IntegrationProperty> attProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.AttributeCodeTableSetting.TableName).ToList();
            foreach (AttributeCodeTable node in prt.Nodes)
            {
                ClearPropertyInternal(attProps, node);

            }
        }
    }

    // TODO: Determine how this works/ how to make it work without FromJoin (or whether we need FromJoin still)
    public class ImportCleanoutTriggerTableStep : ImportStep
    {
        private bool m_include;

        public ImportCleanoutTriggerTableStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(CleanoutTriggerTablesT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.CleanoutTriggerTablesSettings,
            m_settings.CleanoutTriggerTablesAssignedResourcesSettings,
            m_settings.OperationCountCleanoutTriggersSettings,
            m_settings.ProductionUnitCleanoutTriggersSettings,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.CleanoutIntervals;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            // In theory, should be able to uncomment and test (other than Clear)
            DebugException.ThrowInDebug("This needs to be figured out");
            return null;


            //    using (IDbConnection connection = a_importer.OpenNewConnection())
            //    {
            //        try
            //        {
            //            bool importOpCountCleanoutTriggersHasValidSelectAndFromExpressions = true;
            //            bool importProdUnitsCleanoutTriggersHasValidSelectAndFromExpressions = true;
            //            bool importTimeCleanoutTriggersHasValidSelectAndFromExpressions = true;

            //            string sqlTable = m_settings.CleanoutTriggerTablesSettings.GetCommandText(true);
            //            IDbCommand cmdTables = GetCommand(connection, sqlTable);

            //            string sqlCleanoutTblAssignedResources = m_settings.CleanoutTriggerTablesAssignedResourcesSettings.GetCommandText(true);
            //            IDbCommand cmdCleanoutTblAssignedResources = GetCommand(connection, sqlCleanoutTblAssignedResources);

            //            string sqlOpCountCleanoutTriggers = m_settings.OperationCountCleanoutTriggersSettings.GetCommandText(true);
            //            IDbCommand cmdOpCountCleanoutTriggers = GetCommand(connection, sqlOpCountCleanoutTriggers);
            //            if (!m_settings.OperationCountCleanoutTriggersSettings.HasNonEmptySourceExpression()
            //                && m_settings.OperationCountCleanoutTriggersSettings.FromjoinExpression.Trim() == string.Empty)
            //            {
            //                importOpCountCleanoutTriggersHasValidSelectAndFromExpressions = false;
            //            }

            //            string sqlProdUnitsCleanoutTriggers = m_settings.ProductionUnitCleanoutTriggersSettings.GetCommandText(true);
            //            IDbCommand cmdProdUnitsCleanoutTriggers = GetCommand(connection, sqlProdUnitsCleanoutTriggers);
            //            if (!m_settings.ProductionUnitCleanoutTriggersSettings.HasNonEmptySourceExpression()
            //                && m_settings.ProductionUnitCleanoutTriggersSettings.FromjoinExpression.Trim() == string.Empty)
            //            {
            //                importProdUnitsCleanoutTriggersHasValidSelectAndFromExpressions = false;
            //            }

            //            string sqlTimeCleanoutTriggers = m_settings.TimeCleanoutTriggersSettings.GetCommandText(true);
            //            IDbCommand cmdTimeCleanoutTriggers = GetCommand(connection, sqlTimeCleanoutTriggers);
            //            if (!m_settings.TimeCleanoutTriggersSettings.HasNonEmptySourceExpression()
            //                && m_settings.TimeCleanoutTriggersSettings.FromjoinExpression.Trim() == string.Empty)
            //            {
            //                importTimeCleanoutTriggersHasValidSelectAndFromExpressions = false;
            //            }

            //            CleanoutTriggerTablesT ctt = new();
            //            ctt.Fill(cmdTables, cmdCleanoutTblAssignedResources, cmdOpCountCleanoutTriggers, importOpCountCleanoutTriggersHasValidSelectAndFromExpressions, cmdTimeCleanoutTriggers, importTimeCleanoutTriggersHasValidSelectAndFromExpressions, cmdProdUnitsCleanoutTriggers, importProdUnitsCleanoutTriggersHasValidSelectAndFromExpressions, errors);
            //            ctt.AutoDelete = m_settings.CleanoutTriggerTablesSettings.AutoDelete;
            //            ctt.AutoDeleteOperationCountCleanoutTriggers = m_settings.OperationCountCleanoutTriggersSettings.AutoDelete;
            //            ctt.AutoDeleteProductionUnitsCleanoutTriggers = m_settings.ProductionUnitCleanoutTriggersSettings.AutoDelete;
            //            ctt.AutoDeleteTimeCleanoutTriggers = m_settings.TimeCleanoutTriggersSettings.AutoDelete;

            //            // TODO: clear props. Outlandishly, CleanoutTriggerTablesT doesn't follow the same ERP transmission pattern as others

            //            return new(ctt);
            //        }
            //        catch (Exception err)
            //        {
            //            return new(new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
            //        }
            //    }
        }
    }

    public class ImportCompatibilityCodeTableStep : ImportStep
    {
        public ImportCompatibilityCodeTableStep(NewImportSettings a_importSettings, bool a_include)
            : base(typeof(CompatibilityCodeTableT), a_importSettings, a_include) { }

        protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
        {
            m_settings.CompatibilityCodeTablesSettings,
            m_settings.CompatibilityCodeTablesAssignedResourcesSettings,
        });

        public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Compatibility;

        public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                try
                {
                    string sqlTable = m_settings.CompatibilityCodeTablesSettings.GetCommandText(true);
                    IDbCommand cmdTables = GetCommand(connection, sqlTable);
                    // TODO: Wrong tablesetting ref?
                    string sqlCompatibilityCodesAssignedResources = m_settings.CompatibilityCodeTablesSettings.GetCommandText(true);
                    IDbCommand cmdCompatibilityCodesTblAssignedResources = GetCommand(connection, sqlCompatibilityCodesAssignedResources);
                    string sqlCompatibilityCodes = m_settings.CompatibilityCodesSettings.GetCommandText(true);
                    IDbCommand cmdCompatibilityCodes = GetCommand(connection, sqlCompatibilityCodes);
                    CompatibilityCodeTableT cctt = new ();
                    cctt.Fill(cmdTables, cmdCompatibilityCodes, cmdCompatibilityCodesTblAssignedResources);
                    ClearTPropsForTable(null, cctt);
                    cctt.AutoDeleteMode = m_settings.CompatibilityCodeTablesSettings.AutoDelete;
                    return new (cctt);
                }
                catch (Exception err)
                {
                    return new (new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                }
            }
        }

        protected override void ClearTPropsForTable<T>(string a_tableName, ERPMaintenanceTransmission<T> a_t)
        {
            if (a_t is not CompatibilityCodeTableT cctt)
            {
                throw new ArgumentException($"Error in {nameof(ImportCompatibilityCodeTableStep)} overload of {nameof(ClearTPropsForTable)}");
            }

            // TODO: handle multiple tables
            List<IntegrationProperty> comProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.CompatibilityCodeTablesSettings.TableName).ToList();
            foreach (CompatibilityCodeTable node in cctt.Nodes)
            {
                ClearPropertyInternal(comProps, node);
            }
        }

        public class ImportPurchaseToStockStep : ImportStep
        {
            public ImportPurchaseToStockStep(NewImportSettings a_importSettings, bool a_include)
                : base(typeof(PurchaseToStockT), a_importSettings, a_include) { }

            protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
            {
                m_settings.PurchaseToStockSettings,
            });

            public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.PurchaseToStock;

            public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
            {
                using (IDbConnection connection = a_importer.OpenNewConnection())
                {
                    try
                    {
                        string sqlText = m_settings.PurchaseToStockSettings.GetCommandText(true);
                        IDbCommand command = GetCommand(connection, sqlText);
                        PurchaseToStockT purchaseT = new ();
                        purchaseT.Fill(command, PurchaseToStockDefs.EMaintenanceMethod.ERP);
                        ClearTPropsForTable(m_settings.PurchaseToStockSettings.TableName, purchaseT);
                        purchaseT.AutoDeleteMode = m_settings.PurchaseToStockSettings.AutoDelete;
                        return new (purchaseT);
                    }
                    catch (Exception err)
                    {
                        return new (new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                    }
                }
            }
        }

        public class ImportSalesOrdersStep : ImportStep
        {
            public ImportSalesOrdersStep(NewImportSettings a_importSettings, bool a_include)
                : base(typeof(SalesOrderT), a_importSettings, a_include) { }

            protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
            {
                m_settings.SalesOrderSettings,
                m_settings.SalesOrderLineSettings,
                m_settings.SalesOrderLineDistSettings,
            });

            public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.SalesOrders;

            public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
            {
                using (IDbConnection connection = a_importer.OpenNewConnection())
                {
                    try
                    {
                        string soSqlText = m_settings.SalesOrderSettings.GetCommandText(true);
                        IDbCommand soCommand = GetCommand(connection, soSqlText);
                        string soLineSqlText = m_settings.SalesOrderLineSettings.GetCommandText(true);
                        IDbCommand soLineCommand = GetCommand(connection, soLineSqlText);
                        string soLineDistSqlText = m_settings.SalesOrderLineDistSettings.GetCommandText(true);
                        IDbCommand soLineDistCommand = GetCommand(connection, soLineDistSqlText);

                        SalesOrderT soT = new ();
                        soT.Fill(soCommand, soLineCommand, soLineDistCommand);
                        ClearTPropsForTable(null, soT);
                        soT.AutoDeleteMode = m_settings.SalesOrderSettings.AutoDelete;
                        return new (soT);
                    }
                    catch (Exception err)
                    {
                        return new (new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                    }
                }
            }

            protected override void ClearTPropsForTable<T>(string _, ERPMaintenanceTransmission<T> a_t)
            {
                if (a_t is not SalesOrderT soT)
                {
                    throw new ArgumentException($"Error in {nameof(ImportSalesOrdersStep)} overload of {nameof(ClearTPropsForTable)}");
                }

                List<IntegrationProperty> soProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.SalesOrderSettings.TableName).ToList();
                List<IntegrationProperty> solProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.SalesOrderLineSettings.TableName).ToList();
                List<IntegrationProperty> soldProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.SalesOrderLineDistSettings.TableName).ToList();
                foreach (SalesOrderT.SalesOrder node in soT.Nodes)
                {
                    ClearPropertyInternal(soProps, node);

                    foreach (SalesOrderT.SalesOrder.SalesOrderLine line in node.SalesOrderLines)
                    {
                        ClearPropertyInternal(solProps, line);

                        foreach (SalesOrderT.SalesOrder.SalesOrderLine.SalesOrderLineDistribution dist in line.LineDistributions)
                        {
                            ClearPropertyInternal(soldProps, dist);
                        }
                    }
                }
            }
        }

        public class ImportForecastsStep : ImportStep
        {
            public ImportForecastsStep(NewImportSettings a_importSettings, bool a_include)
                : base(typeof(ForecastT), a_importSettings, a_include) { }

            protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
            {
                m_settings.ForecastSettings,
                m_settings.ForecastShipmentSettings,
            });

            public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.Forecasts;

            public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
            {
                using (IDbConnection connection = a_importer.OpenNewConnection())
                {
                    try
                    {
                        string forecastSqlText = m_settings.ForecastSettings.GetCommandText(true);
                        IDbCommand forecastCommand = GetCommand(connection, forecastSqlText);
                        string shipmentSqlText = m_settings.ForecastShipmentSettings.GetCommandText(true);
                        IDbCommand shipmentCommand = GetCommand(connection, shipmentSqlText);
                        ForecastT forecastT = new ();
                        ApplicationExceptionList errors = new (); // TODO: This would be better to incorporate into the exception process
                        forecastT.Fill(forecastCommand, shipmentCommand, errors);
                        ClearTPropsForTable(null, forecastT);
                        forecastT.AutoDeleteMode = m_settings.ForecastSettings.AutoDelete;

                        if (errors.Any())
                        {
                            return new (new ImportStepTError(EImportStepTErrorReason.GetDataError, errors));
                        }

                        return new (forecastT);
                    }
                    catch (Exception err)
                    {
                        return new (new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                    }
                }
            }

            protected override void ClearTPropsForTable<T>(string a_tableName, ERPMaintenanceTransmission<T> a_t)
            {
                if (a_t is not ForecastT forecastT)
                {
                    throw new ArgumentException($"Error in {nameof(ImportForecastsStep)} overload of {nameof(ClearTPropsForTable)}");
                }

                List<IntegrationProperty> foreProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.ForecastSettings.TableName).ToList();
                List<IntegrationProperty> shipProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.ForecastShipmentSettings.TableName).ToList();

                foreach (ForecastT.Forecast node in forecastT.Nodes)
                {
                    ClearPropertyInternal(foreProps, node);

                    foreach (ForecastT.ForecastShipment ship in node.Shipments)
                    {
                        ClearPropertyInternal(shipProps, ship);

                    }
                }
            }
        }

        public class ImportTransferOrderStep : ImportStep
        {
            public ImportTransferOrderStep(NewImportSettings a_importSettings, bool a_include)
                : base(typeof(TransferOrderT), a_importSettings, a_include) { }

            protected override Dictionary<string, ImportTableSettings> TablesForStep => CreateTableLookupForImportStep(new List<ImportTableSettings>()
            {
                m_settings.TransferOrderSettings,
                m_settings.TransferOrderDistributionSettings,
            });

            public override ImportStatuses.EImportProgressStep ProgressStep => ImportStatuses.EImportProgressStep.TransferOrders;

            public override Result<ERPTransmission, ImportStepTError> GetTransmission(ImporterV2 a_importer)
            {
                using (IDbConnection connection = a_importer.OpenNewConnection())
                {
                    try
                    {
                        string transferOrderSqlText = m_settings.TransferOrderSettings.GetCommandText(true);
                        IDbCommand transferOrderCommand = GetCommand(connection, transferOrderSqlText);
                        string transferOrderDistSqlText = m_settings.TransferOrderDistributionSettings.GetCommandText(true);
                        IDbCommand transferOrderDistCommand = GetCommand(connection, transferOrderDistSqlText);
                        TransferOrderT transferOrderT = new ();
                        transferOrderT.Fill(transferOrderCommand, transferOrderDistCommand, JobDefs.EMaintenanceMethod.ERP);
                        ClearTPropsForTable(null, transferOrderT);
                        transferOrderT.AutoDeleteMode = m_settings.TransferOrderSettings.AutoDelete;
                        return new (transferOrderT);
                    }
                    catch (Exception err)
                    {
                        return new (new ImportStepTError(EImportStepTErrorReason.GetDataError, [err]));
                    }
                }
            }

            protected override void ClearTPropsForTable<T>(string a_tableName, ERPMaintenanceTransmission<T> a_t)
            {
                if (a_t is not TransferOrderT transferOrderT)
                {
                    throw new ArgumentException($"Error in {nameof(ImportTransferOrderStep)} overload of {nameof(ClearTPropsForTable)}");
                }

                List<IntegrationProperty> tranProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.TransferOrderSettings.TableName).ToList();
                List<IntegrationProperty> distProps = m_settings.FeaturesSettings.GetClearValuePropertiesUsingTable(m_settings.TransferOrderDistributionSettings.TableName).ToList();
                foreach (TransferOrderT.TransferOrder node in transferOrderT.Nodes)
                {
                    ClearPropertyInternal(tranProps, node);

                    foreach (TransferOrderT.TransferOrder.TransferOrderDistribution dist in node.Distributions)
                    {
                        ClearPropertyInternal(distProps, dist);

                    }
                }
            }
        }
    }
}
