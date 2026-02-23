using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.ERPTransmissions;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler
{
    public class UserFieldDefinitionManager : BaseObjectManager<UserFieldDefinition>, IEnumerable<UserFieldDefinition>, IUserFieldDefinitionManager
    {
        #region IPTSerializable
        public int UniqueId => 857;

        public UserFieldDefinitionManager(IReader a_reader, BaseIdGenerator a_idGen) : base(a_idGen)
        {
            if (a_reader.VersionNumber >= 12503)
            {
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    UserFieldDefinition udfDef = new(a_reader);
                    Add(udfDef);
                }
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);
        }
        #endregion

        #region Construction
        public UserFieldDefinitionManager(BaseIdGenerator a_idGen) : base(a_idGen) { }
        #endregion

        #region Declarations
        public class UserFieldManagerException : PTException
        {
            public UserFieldManagerException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
                : base(a_message, a_stringParameters, a_appendHelpUrl) { }
        }

        #endregion

        #region RestoreReferences
        private readonly object m_restoreLock = new object();
        //For backwards compatibility, add new UserFieldDefinitions for any existing UserFields in the object.
        internal void RestoreReferences(BaseObject a_ptObject, UserField.EUDFObjectType a_udfObjectType)
        {
            RestoreReferences(a_ptObject.UserFields, a_udfObjectType);
        }

        //For backwards compatibility, add new UserFieldDefinitions for any existing UserFields in the object.
        internal void RestoreReferences(UserFieldList a_userFields, UserField.EUDFObjectType a_udfObjectType)
        {
            if (a_userFields == null)
            {
                return;
            }
            
            lock (m_restoreLock)
            {
                InitFastLookupByExternalId();

                for (int i = 0; i < a_userFields.Count; i++)
                {
                    UserField udf = a_userFields[i];

                    string externalId = string.Format("{0}@{1}", a_udfObjectType.ToString().Localize(), udf.ExternalId);

                    UserFieldDefinition udfDef = GetByExternalId(externalId);
                    if (udfDef == null) //If there was one already found, we will ignore this new one.
                    {
                        UserFieldDefinition newUDFDef = new (NextID(), externalId);
                        newUDFDef.DisplayInUI = !udf.ExternalId.EndsWith("(Hidden)");
                        newUDFDef.Publish = true;
                        newUDFDef.KeepValue = false;
                        newUDFDef.Name = udf.ExternalId;
                        newUDFDef.ObjectType = a_udfObjectType;
                        newUDFDef.UDFDataType = udf.UDFDataType;
                        newUDFDef.DefaultValue = udf.DataValue;
                        newUDFDef.Description = "";

                        Add(newUDFDef);
                    }

                    udf.ExternalId = externalId;
                }

                DeInitFastLookupByExternalId();
            }
        }

        #endregion

        #region IAfterRestoreReferences

        public void AfterRestoreReferences_1(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
        {
            //PT.Scheduler.AfterRestoreReferences.Helpers.SortedIdListHelperFor_AfterRestoreReferences_1(serializationVersionNbr, _idGen, _activitiesList, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        }

        public void AfterRestoreReferences_2(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
        {
            //PT.Scheduler.AfterRestoreReferences.Helpers.SortedIdListHelperFor_AfterRestoreReferences_2(serializationVersionNbr, _activitiesList, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        }

        #endregion

        #region IEnumerable
        public IEnumerator<UserFieldDefinition> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return GetByIndex(i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region ICopyTable
        public override Type ElementType => typeof(UserFieldDefinition);
        #endregion

        internal UserFieldDefinition Add(UserFieldDefinition a_udfDef)
        {
            return base.Add(a_udfDef);
        }

        public void AddNewDefinition(string a_externalId,
                                     string a_name,
                                     string a_description,
                                     UserField.UDFTypes a_udfType,
                                     UserField.EUDFObjectType a_objectType,
                                     bool a_display,
                                     bool a_publish)
        {
            UserFieldDefinition newDef = new UserFieldDefinition(IdGen.NextID(), a_externalId)
            {
                Name = a_name,
                Description = a_description,
                UDFDataType = a_udfType,
                ObjectType = a_objectType,
                DisplayInUI = a_display,
                Publish = a_publish
            };

            base.Add(newDef);
        }

        #region Transmissions

        private UserFieldDefinition AddDefault(UserFieldDefinitionDefaultT a_t)
        {
            UserFieldDefinition newUdfDef = new(NextID(), NextExternalId("User Field Definition".Localize()));
            ValidateAdd(newUdfDef);
            return Add(newUdfDef);
        }

        private UserFieldDefinition AddCopy(UserFieldDefinitionCopyT a_t)
        {
            ValidateCopy(a_t);
            UserFieldDefinition sourceUDFDef = GetById(a_t.OriginalId);
            UserFieldDefinition copyUDFDef = new(sourceUDFDef, NextID());
            copyUDFDef.ExternalId = NextExternalId("User Field Definition");
            copyUDFDef.Name = MakeCopyName(sourceUDFDef.Name);
            return AddCopy(sourceUDFDef, copyUDFDef);
        }

        private void ValidateAdd(UserFieldDefinition a_userFieldDefinition)
        {
            if (Contains(a_userFieldDefinition))
            {
                throw new UserFieldManagerException("3064", new object[] { a_userFieldDefinition.ExternalId });
            }
        }

        private void ValidateCopy(UserFieldDefinitionCopyT a_t)
        {
            ValidateExistence(a_t.OriginalId);
        }

        /// <summary>
        /// Handles the deletion of UserFieldDefinitions as well as their related user fields on PT objects.
        /// </summary>
        /// <param name="a_udfDefId"></param>
        private void Delete(ScenarioDetail a_sd, BaseId a_udfDefId)
        {
            UserFieldDefinition udfDef = GetById(a_udfDefId);
            if (udfDef != null)
            {
                Remove(udfDef.Id); //Now remove it from the Manager.

                //Remove the udfs from the objects that reference them
                DeleteUserFieldReferencesFromScenarios(a_sd, udfDef);
            }
        }

        private void Delete(BaseId a_udfDefId)
        {
            if (a_udfDefId != BaseId.NULL_ID)
            {
                Remove(a_udfDefId);
            }
        }

        /// <summary>
        /// Deletes the specified user fields from objects in the specified scenario.
        /// </summary>
        /// <param name="a_sd"></param>
        /// <param name="a_udfDef"></param>
        /// <param name="a_dataChanges"></param>
        private void DeleteUserFieldReferencesFromScenarios(ScenarioDetail a_sd, UserFieldDefinition a_udfDef)
        {
            if (a_udfDef.ObjectType == UserField.EUDFObjectType.Plants)
            {
                a_sd.PlantManager.DeleteUserFieldByExternalId(a_udfDef.ExternalId);
            }
            else if (a_udfDef.ObjectType == UserField.EUDFObjectType.Departments)
            {
                foreach (Plant plant in a_sd.PlantManager)
                {
                    plant.Departments.DeleteUserFieldByExternalId(a_udfDef.ExternalId);
                }
            }
            else if (a_udfDef.ObjectType == UserField.EUDFObjectType.Resources)
            {
                foreach (Plant plant in a_sd.PlantManager)
                {
                    foreach (Department dept in plant.Departments)
                    {
                        dept.Resources.DeleteUserFieldByExternalId(a_udfDef.ExternalId);
                    }
                }
            }
            else if (a_udfDef.ObjectType == UserField.EUDFObjectType.Cells)
            {
                a_sd.CellManager.DeleteUserFieldByExternalId(a_udfDef.ExternalId);
            }
            else if (a_udfDef.ObjectType == UserField.EUDFObjectType.CapacityIntervals)
            {
                a_sd.CapacityIntervalManager.DeleteUserFieldByExternalId(a_udfDef.ExternalId);
                a_sd.RecurringCapacityIntervalManager.DeleteUserFieldByExternalId(a_udfDef.ExternalId);
            }
            else if (a_udfDef.ObjectType == UserField.EUDFObjectType.ProductRules)
            {
                foreach (Plant plant in a_sd.PlantManager)
                {
                    foreach (Department dept in plant.Departments)
                    {
                        foreach (Resource res in dept.Resources)
                        {
                            foreach (ProductRule productRule in a_sd.ProductRuleManager.EnumerateForResource(res.Id))
                            {
                                productRule.ProductRulesUserFields.Remove(a_udfDef.ExternalId);
                            }
                        }
                    }
                }
            }
            else if (a_udfDef.ObjectType == UserField.EUDFObjectType.ResourceConnectors)
            {
                a_sd.ResourceConnectorManager.DeleteUserFieldByExternalId(a_udfDef.ExternalId);
            }
            else if (a_udfDef.ObjectType == UserField.EUDFObjectType.Items)
            {
                a_sd.ItemManager.DeleteUserFieldByExternalId(a_udfDef.ExternalId);
            }
            else if (a_udfDef.ObjectType == UserField.EUDFObjectType.Warehouses)
            {
                a_sd.WarehouseManager.DeleteUserFieldByExternalId(a_udfDef.ExternalId);
            }
            else if (a_udfDef.ObjectType == UserField.EUDFObjectType.SalesOrders)
            {
                a_sd.SalesOrderManager.DeleteUserFieldByExternalId(a_udfDef.ExternalId);
            }
            else if (a_udfDef.ObjectType == UserField.EUDFObjectType.Forecasts)
            {
                foreach (Warehouse warehouse in a_sd.WarehouseManager)
                {
                    foreach (Inventory itemInventory in warehouse.Inventories)
                    {
                        if (itemInventory.ForecastVersions == null || itemInventory.ForecastVersions.Versions == null)
                        {
                            continue;
                        }

                        foreach (ForecastVersion forecastVersion in itemInventory.ForecastVersions?.Versions)
                        {
                            foreach (Forecast forecast in forecastVersion.Forecasts)
                            {
                                forecast.UserFields.Remove(a_udfDef.ExternalId);
                            }
                        }
                    }
                }
            }
            else if (a_udfDef.ObjectType == UserField.EUDFObjectType.PurchasesToStock)
            {
                a_sd.PurchaseToStockManager.DeleteUserFieldByExternalId(a_udfDef.ExternalId);
            }
            else if (a_udfDef.ObjectType == UserField.EUDFObjectType.TransferOrders)
            {
                a_sd.TransferOrderManager.DeleteUserFieldByExternalId(a_udfDef.ExternalId);
            }
            else if (a_udfDef.ObjectType == UserField.EUDFObjectType.Jobs)
            {
                a_sd.JobManager.DeleteUserFieldByExternalId(a_udfDef.ExternalId);
            }
            else if (a_udfDef.ObjectType == UserField.EUDFObjectType.ManufacturingOrders)
            {
                foreach (Job job in a_sd.JobManager)
                {
                    job.ManufacturingOrders.DeleteUserFieldByExternalId(a_udfDef.ExternalId);
                }
            }
            else if (a_udfDef.ObjectType == UserField.EUDFObjectType.ResourceOperations)
            {
                foreach (Job job in a_sd.JobManager)
                {
                    bool removed = false;
                    foreach (ManufacturingOrder mo in job.ManufacturingOrders)
                    {
                        mo.OperationManager.DeleteUserFieldByExternalId(a_udfDef.ExternalId);
                    }
                }
            }
        }

        private void DeleteUserFieldReferencesFromScenariosByType(ScenarioManager a_sm, ScenarioDetail a_sd, UserField.EUDFObjectType a_udfObjectType)
        {
            if (a_udfObjectType == UserField.EUDFObjectType.Plants)
            {
                foreach (Plant plant in a_sd.PlantManager)
                {
                    plant.UserFields.Clear();
                }
            }
            else if (a_udfObjectType == UserField.EUDFObjectType.Departments)
            {
                foreach (Plant plant in a_sd.PlantManager)
                {
                    foreach (Department dept in plant.Departments)
                    {
                        dept.UserFields.Clear();
                    }
                }
            }
            else if (a_udfObjectType == UserField.EUDFObjectType.Resources)
            {
                foreach (Plant plant in a_sd.PlantManager)
                {
                    foreach (Department dept in plant.Departments)
                    {
                        foreach (Resource res in dept.Resources)
                        {
                            res.UserFields.Clear();
                        }
                    }
                }
            }
            else if (a_udfObjectType == UserField.EUDFObjectType.Cells)
            {
                foreach (Cell cell in a_sd.CellManager)
                {
                    cell.UserFields.Clear();
                }
            }
            else if (a_udfObjectType == UserField.EUDFObjectType.CapacityIntervals)
            {
                foreach (CapacityInterval ci in a_sd.CapacityIntervalManager)
                {
                    ci.UserFields.Clear();
                }

                foreach (RecurringCapacityInterval rci in a_sd.RecurringCapacityIntervalManager)
                {
                    rci.UserFields.Clear();
                }
            }
            else if (a_udfObjectType == UserField.EUDFObjectType.ProductRules)
            {
                foreach (Plant plant in a_sd.PlantManager)
                {
                    foreach (Department dept in plant.Departments)
                    {
                        foreach (Resource res in dept.Resources)
                        {
                            foreach (ProductRule productRule in a_sd.ProductRuleManager.EnumerateForResource(res.Id))
                            {
                                productRule.ProductRulesUserFields.Clear();
                            }
                        }
                    }
                }
            }
            else if (a_udfObjectType == UserField.EUDFObjectType.ResourceConnectors)
            {
                foreach (ResourceConnector resourceConnector in a_sd.ResourceConnectorManager)
                {
                    resourceConnector.UserFields.Clear();
                }
            }
            else if (a_udfObjectType == UserField.EUDFObjectType.Items)
            {
                foreach (Item item in a_sd.ItemManager)
                {
                    item.UserFields.Clear();
                }
            }
            else if (a_udfObjectType == UserField.EUDFObjectType.Warehouses)
            {
                foreach (Warehouse warehouse in a_sd.WarehouseManager)
                {
                    warehouse.UserFields.Clear();
                }
            }
            else if (a_udfObjectType == UserField.EUDFObjectType.SalesOrders)
            {
                foreach (SalesOrder so in a_sd.SalesOrderManager)
                {
                    so.UserFields.Clear();
                }
            }
            else if (a_udfObjectType == UserField.EUDFObjectType.Forecasts)
            {
                foreach (Warehouse warehouse in a_sd.WarehouseManager)
                {
                    foreach (Inventory itemInventory in warehouse.Inventories)
                    {
                        if (itemInventory.ForecastVersions == null || itemInventory.ForecastVersions.Versions == null)
                        {
                            continue;
                        }

                        foreach (ForecastVersion forecastVersion in itemInventory.ForecastVersions.Versions)
                        {
                            foreach (Forecast forecast in forecastVersion.Forecasts)
                            {
                                forecast.UserFields.Clear();
                            }
                        }
                    }
                }
            }
            else if (a_udfObjectType == UserField.EUDFObjectType.PurchasesToStock)
            {
                foreach (PurchaseToStock purchaseToStock in a_sd.PurchaseToStockManager)
                {
                    purchaseToStock.UserFields.Clear();
                }
            }
            else if (a_udfObjectType == UserField.EUDFObjectType.TransferOrders)
            {
                foreach (TransferOrder transferOrder in a_sd.TransferOrderManager)
                {
                    transferOrder.UserFields.Clear();
                }
            }
            else if (a_udfObjectType == UserField.EUDFObjectType.Jobs)
            {
                foreach (Job job in a_sd.JobManager)
                {
                    job.UserFields.Clear();
                }
            }
            else if (a_udfObjectType == UserField.EUDFObjectType.ManufacturingOrders)
            {
                foreach (Job job in a_sd.JobManager)
                {
                    foreach (ManufacturingOrder mo in job.ManufacturingOrders)
                    {
                        mo.UserFields.Clear();
                    }
                }
            }
            else if (a_udfObjectType == UserField.EUDFObjectType.ResourceOperations)
            {
                foreach (Job job in a_sd.JobManager)
                {
                    bool removed = false;
                    foreach (ManufacturingOrder mo in job.ManufacturingOrders)
                    {
                        for (int i = 0; i < mo.OperationManager.Count; i++)
                        {
                            BaseOperation op = mo.OperationManager.GetByIndex(i);
                            op.UserFields.Clear();
                        }
                    }
                }
            }

            a_sm.FireUDFDataChangedEvent(a_udfObjectType);
        }

        public void Receive(ScenarioManager a_scenarioManager, ScenarioDetail a_sd, ScenarioDetailClearT a_clearT)
        {
            if (a_clearT.ClearUserUdfs)
            {
                using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, AutoExiter.THREAD_TRY_WAIT_MS))
                {
                    foreach (User user in um)
                    {
                        user.UserFields.Clear();
                    }

                    a_scenarioManager.FireUDFDataChangedEvent(UserField.EUDFObjectType.Users);
                }
            }
            if (a_clearT.ClearPlantUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.Plants);
            }
            if (a_clearT.ClearDepartmentUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.Departments);
            }
            if (a_clearT.ClearResourceUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.Resources);
            }
            if (a_clearT.ClearCellUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.Cells);
            }
            if (a_clearT.ClearCapacityIntervalUdfs || a_clearT.ClearRecurringCapacityIntervals)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.CapacityIntervals);
            }
            if (a_clearT.ClearProductRulesUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.ProductRules);
            }
            if (a_clearT.ClearResourceConnectorUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.ResourceConnectors);
            }
            if (a_clearT.ClearItemUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.Items);
            }
            if (a_clearT.ClearWarehouseUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.Warehouses);
            }
            if (a_clearT.ClearSalesOrderUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.SalesOrders);
            }
            if (a_clearT.ClearForecastUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.Forecasts);
            }
            if (a_clearT.ClearPurchasesToStockUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.PurchasesToStock);
            }
            if (a_clearT.ClearTransferOrderUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.TransferOrders);
            }
            if (a_clearT.ClearJobUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.Jobs);
            }
            if (a_clearT.ClearManufacturingOrderUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.ManufacturingOrders);
            }
            if (a_clearT.ClearResourceOperationUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.ResourceOperations);
            }
            if (a_clearT.ClearCustomerUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.Customers);
            }
            if (a_clearT.ClearLotsUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.Lots);
            }
            if (a_clearT.ClearLotsUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.Lots);
            }
            if (a_clearT.ClearStorageAreaUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.StorageArea);
            }
            if (a_clearT.ClearItemStorageUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.ItemStorage);
            }
            if (a_clearT.ClearStorageAreaConnectorUdfs)
            {
                DeleteUserFieldReferencesFromScenariosByType(a_scenarioManager, a_sd, UserField.EUDFObjectType.StorageAreaConnectors);
            }
        }

        public void Receive(ScenarioManager a_scenarioManager, UserFieldDefinitionBaseT a_t)
        {
            UserFieldDefinition udfDef;
            if (a_t is UserFieldDefinitionDefaultT udfDefinitionDefaultT)
            {
                udfDef = AddDefault(udfDefinitionDefaultT);
                a_scenarioManager.FireUDFDataChangedEvent(udfDef.ObjectType);
            }
            else if (a_t is UserFieldDefinitionCopyT udfDefinitionCopyT)
            {
                udfDef = AddCopy(udfDefinitionCopyT);
                a_scenarioManager.FireUDFDataChangedEvent(udfDef.ObjectType);
            }
        }

        public void Receive(ScenarioManager a_scenarioManager, ScenarioDetail a_sd, UserFieldDefinitionBaseT a_t)
        {
            UserFieldDefinition udfDef;

            //We probably only want to fire the UDFChangedEvent once per object type, so keep a list of unique object types.
            List<UserField.EUDFObjectType> deletedUDFObjectTypes = new ();

            if (a_t is UserFieldDefinitionDeleteT udfDefinitionDeleteT)
            {
                for (int i = 0; i < udfDefinitionDeleteT.UserFieldDefinitionIds.Count; i++)
                {
                    BaseId userFieldDefinitionId = udfDefinitionDeleteT.UserFieldDefinitionIds[i];
                    udfDef = GetById(userFieldDefinitionId);
                    if (udfDef == null)
                    {
                        throw new ValidationException("3068", new object[] { "User Field Definition".Localize(), userFieldDefinitionId }, a_appendHelp: true, a_logToSentry: true);
                    }

                    Delete(a_sd, udfDef.Id);
                    deletedUDFObjectTypes.AddIfNew(udfDef.ObjectType);
                }
            }
            else if (a_t is UserFieldDefinitionDeleteAllT)
            {
                for (int i = Count - 1; i >= 0; i--)
                {
                    udfDef = GetByIndex(i);
                    Delete(a_sd, udfDef.Id);
                    deletedUDFObjectTypes.AddIfNew(udfDef.ObjectType);
                }
            }

            foreach (UserField.EUDFObjectType objectType in deletedUDFObjectTypes)
            {
                a_scenarioManager.FireUDFDataChangedEvent(objectType);
            }
        }

        public void Receive(ScenarioManager a_scenarioManager, UserFieldDefinitionT a_t)
        {
            HashSet<BaseId> affectedUdfs = new();

            InitFastLookupByExternalId();
            HashSet<UserField.EUDFObjectType> objectTypes = new();

            for (int i = 0; i < a_t.Count; ++i)
            {
                UserFieldDefinitionT.UserFieldDefinition userFieldDefinitionNode = a_t[i];

                UserFieldDefinition udfDef;
                if (userFieldDefinitionNode.IdSet)
                {
                    udfDef = GetById(userFieldDefinitionNode.Id);
                    if (udfDef == null)
                    {
                        throw new ValidationException("3068", new object[] { "User Field Definition".Localize(), userFieldDefinitionNode.Id });
                    }
                }
                else
                {
                    udfDef = GetByExternalId(userFieldDefinitionNode.ExternalId);
                }

                if (udfDef == null)
                {
                    udfDef = new UserFieldDefinition(NextID(), userFieldDefinitionNode);
                    Add(udfDef);
                }

                udfDef.Update(userFieldDefinitionNode, a_t);
                affectedUdfs.Add(udfDef.Id);
                objectTypes.AddIfNew(udfDef.ObjectType);
            }

            foreach (UserField.EUDFObjectType udfObjectType in objectTypes)
            {
                a_scenarioManager.FireUDFDataChangedEvent(udfObjectType);
            }

            if (a_t.AutoDeleteMode)
            {
                objectTypes.Clear();

                for (int i = Count - 1; i >= 0; i--)
                {
                    UserFieldDefinition udfDef = GetByIndex(i);
                    if (!affectedUdfs.Contains(udfDef.Id))
                    {
                        Delete(udfDef.Id);
                        objectTypes.AddIfNew(udfDef.ObjectType);
                    }
                }

                foreach (UserField.EUDFObjectType udfObjectType in objectTypes)
                {
                    a_scenarioManager.FireUDFDataChangedEvent(udfObjectType);
                }
            }

            DeInitFastLookupByExternalId();
        }
        #endregion

        /// <summary>
        /// Create a shallow copy of the underlying collection to use in a readonly context
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, IUserFieldDefinition> GenerateFastLookupByExternalId()
        {
            Dictionary<string, IUserFieldDefinition> newExternalIdDictionary = new (StringComparers.CaseSensitiveComparer);

            foreach (UserFieldDefinition o in this)
            {
                newExternalIdDictionary.Add(o.ExternalId, o);
            }

            return newExternalIdDictionary;
        }

        public IUserFieldDefinition GetUdfDataType(string a_defExternalId)
        {
            return GetByExternalId(a_defExternalId);
        }
    }
}
