using PT.APIDefinitions.RequestsAndResponses.Webapp;
using PT.Common.Debugging;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using System.Diagnostics;

using PT.APSCommon;
using PT.Transmissions;

using Debug = PT.Common.Testing.Debug;

namespace PT.ImportDefintions;

/// <summary>
/// New Version of the import settings that uses Features. The actual feature logic is handled in the <see cref="ImportTableSettings "/>themselves.
/// </summary>
public class NewImportSettings : ICloneable
{
    /// <summary>
    /// This value is similar to the Serialization version in that it is used by the Importing Service to determine whether configs stored on the webapp are outdated and
    /// need to be upgraded. This value is also used in a few other places for filtering which configs to show in the ui.
    ///
    /// Whenever any schema changes are made to the import data please bump this version by one and update the respective classes in IntegrationFeatureBase.cs
    ///
    /// You can test missing Feature columns, depreciated columns, and type mismatches by uncommenting Test_IsFeatureDataSetInSync() in ImportTableSettings.cs
    /// </summary>
    public const int c_CONFIG_VERSION_NUMBER = 0; //set to 1 whenever this leaves dev
    public ImportFeaturesSettings FeaturesSettings { get; set; }
    public List<ImportTableSettings> m_ImportTableSettings { get; set; }

    #region Webapp Identifiers
    public string Name { get; set; } = "New Integration";

    public const int c_ID_NOT_ON_WEBAPP = 0;

    public int WebAppId { get; set; } = c_ID_NOT_ON_WEBAPP;
    #endregion    

    public void InitializeSettings()
    {
        m_ImportTableSettings = new List<ImportTableSettings>();
        FeaturesSettings = new ImportFeaturesSettings();

        PlantSettings = new ImportTableSettings(typeof(PtImportDataSet.PlantsDataTable), this, "Plants");
        m_ImportTableSettings.Add(PlantSettings);
        DepartmentSettings = new ImportTableSettings(typeof(PtImportDataSet.DepartmentsDataTable), this, "Departments");
        m_ImportTableSettings.Add(DepartmentSettings);
        CellsSettings = new ImportTableSettings(typeof(PtImportDataSet.CellsDataTable), this, "Cells");
        m_ImportTableSettings.Add(CellsSettings);
        MachineSettings = new ImportTableSettings(typeof(ResourceTDataSet.ResourceDataTable), this, "Resources");
        m_ImportTableSettings.Add(MachineSettings);
        CapacityIntervalSettings = new ImportTableSettings(typeof(CapacityIntervalTDataSet.CapacityIntervalDataTable), this, "CapacityIntervals");
        m_ImportTableSettings.Add(CapacityIntervalSettings);
        CapacityIntervalResourceSettings = new ImportTableSettings(typeof(CapacityIntervalTDataSet.ResourcesDataTable), this, "CapacityIntervalResources");
        m_ImportTableSettings.Add(CapacityIntervalResourceSettings);
        ProductRulesSettings = new ImportTableSettings(typeof(ProductRulesTDataSet.ProductRulesDataTable), this, "ProductRules");
        m_ImportTableSettings.Add(ProductRulesSettings);
        AttributeSettings = new ImportTableSettings(typeof(PtImportDataSet.PTAttributesDataTable), this, "Attributes");
        m_ImportTableSettings.Add(AttributeSettings);
        SetupTableAttSettings = new ImportTableSettings(typeof(LookupAttributeNumberRangeDataSet.TableListDataTable), this, "AttributeRangeTables");
        m_ImportTableSettings.Add(SetupTableAttSettings);
        SetupTableAttNameSettings = new ImportTableSettings(typeof(LookupAttributeNumberRangeDataSet.AttributeRangesDataTable), this, "AttributeRangeTableAttrNames");
        m_ImportTableSettings.Add(SetupTableAttNameSettings);
        SetupTableAttFromSettings = new ImportTableSettings(typeof(LookupAttributeNumberRangeDataSet.From_RangesDataTable), this, "AttributeRangeTableFrom");
        m_ImportTableSettings.Add(SetupTableAttFromSettings);
        SetupTableAttToSettings = new ImportTableSettings(typeof(LookupAttributeNumberRangeDataSet.To_RangesDataTable), this, "AttributeRangeTableTo");
        m_ImportTableSettings.Add(SetupTableAttToSettings);
        SetupTableAttResourceSettings = new ImportTableSettings(typeof(LookupAttributeNumberRangeDataSet.AssignedResourcesDataTable), this, "AttributeRangeTableResources");
        m_ImportTableSettings.Add(SetupTableAttResourceSettings);
        AttributeCodeTableSetting = new ImportTableSettings(typeof(LookupAttributeCodeTableDataSet.TableListDataTable), this, "AttributeCodeTables");
        m_ImportTableSettings.Add(AttributeCodeTableSetting);
        AttributeCodeTableAttributeExternalIdSetting = new ImportTableSettings(typeof(LookupAttributeCodeTableDataSet.AttributeExternalIdDataTable), this, "AttributeCodeTableAttrNames");
        m_ImportTableSettings.Add(AttributeCodeTableAttributeExternalIdSetting);
        AttributeCodeTableAttributeCodesSetting = new ImportTableSettings(typeof(LookupAttributeCodeTableDataSet.AttributeCodesDataTable), this, "AttributeCodeTableAttrCodes");
        m_ImportTableSettings.Add(AttributeCodeTableAttributeCodesSetting);
        AttributeCodeTableAssignedResourcesSetting = new ImportTableSettings(typeof(LookupAttributeCodeTableDataSet.AssignedResourcesDataTable), this, "AttributeCodeTableResources");
        m_ImportTableSettings.Add(AttributeCodeTableAssignedResourcesSetting);
        
        RecurringCapacityIntervalSettings = new ImportTableSettings(typeof(RecurringCapacityIntervalTDataSet.RecurringCapacityIntervalsDataTable), this, "RecurringCapacityIntervals");
        m_ImportTableSettings.Add(RecurringCapacityIntervalSettings);
        CapabilityAssignmentSettings = new ImportTableSettings(typeof(ResourceTDataSet.CapabilityAssignmentsDataTable), this, "ResourceCapabilities");
        m_ImportTableSettings.Add(CapabilityAssignmentSettings);
        CapabilitySettings = new ImportTableSettings(typeof(PtImportDataSet.CapabilitiesDataTable), this, "Capabilities");
        m_ImportTableSettings.Add(CapabilitySettings);
        AllowedHelperResourcesSettings = new ImportTableSettings(typeof(ResourceTDataSet.AllowedHelperResourcesDataTable), this, "AllowedHelpers");
        m_ImportTableSettings.Add(AllowedHelperResourcesSettings);
        WarehouseSettings = new ImportTableSettings(typeof(PtImportDataSet.WarehousesDataTable), this, "Warehouses");
        m_ImportTableSettings.Add(WarehouseSettings);
        PlantWarehouseSettings = new ImportTableSettings(typeof(PtImportDataSet.SuppliedPlantsDataTable), this, "PlantWarehouses");
        m_ImportTableSettings.Add(PlantWarehouseSettings);
        InventorySettings = new ImportTableSettings(typeof(PtImportDataSet.InventoriesDataTable), this, "Inventories");
        m_ImportTableSettings.Add(InventorySettings);
        LotsSettings = new ImportTableSettings(typeof(PtImportDataSet.LotsDataTable), this, "Lots");
        m_ImportTableSettings.Add(LotsSettings);
        ItemSettings = new ImportTableSettings(typeof(PtImportDataSet.ItemsDataTable), this, "Items");
        m_ImportTableSettings.Add(ItemSettings);
        CustomerSettings = new ImportTableSettings(typeof(PtImportDataSet.CustomerDataTable), this, "Customers");
        m_ImportTableSettings.Add(CustomerSettings);
        PurchaseToStockSettings = new ImportTableSettings(typeof(PtImportDataSet.PurchaseToStocksDataTable), this, "PurchasesToStock");
        m_ImportTableSettings.Add(PurchaseToStockSettings);
        SalesOrderSettings = new ImportTableSettings(typeof(SalesOrderTDataSet.SalesOrderDataTable), this, "SalesOrders");
        m_ImportTableSettings.Add(SalesOrderSettings);
        SalesOrderLineSettings = new ImportTableSettings(typeof(SalesOrderTDataSet.SalesOrderLineDataTable), this, "SalesOrderLines");
        m_ImportTableSettings.Add(SalesOrderLineSettings);
        SalesOrderLineDistSettings = new ImportTableSettings(typeof(SalesOrderTDataSet.SalesOrderLineDistDataTable), this, "SalesOrderLineDistributions");
        m_ImportTableSettings.Add(SalesOrderLineDistSettings);
        ForecastSettings = new ImportTableSettings(typeof(ForecastTDataSet.ForecastsDataTable), this, "Forecasts");
        m_ImportTableSettings.Add(ForecastSettings);
        ForecastShipmentSettings = new ImportTableSettings(typeof(ForecastTDataSet.ForecastShipmentsDataTable), this, "ForecastShipments");
        m_ImportTableSettings.Add(ForecastShipmentSettings);
        TransferOrderSettings = new ImportTableSettings(typeof(TransferOrderTDataSet.TransferOrderDataTable), this, "TransferOrders");
        m_ImportTableSettings.Add(TransferOrderSettings);
        TransferOrderDistributionSettings = new ImportTableSettings(typeof(TransferOrderTDataSet.TransferOrderDistributionDataTable), this, "TransferOrderDistributions");
        m_ImportTableSettings.Add(TransferOrderDistributionSettings);
        
        JobSettings = new ImportTableSettings(typeof(JobDataSet.JobDataTable), this, "Jobs");
        m_ImportTableSettings.Add(JobSettings);
        MoSettings = new ImportTableSettings(typeof(JobDataSet.ManufacturingOrderDataTable), this, "ManufacturingOrders");
        m_ImportTableSettings.Add(MoSettings);
        SuccessorMoSettings = new ImportTableSettings(typeof(JobDataSet.SuccessorMODataTable), this, "JobSuccessorManufacturingOrders");
        m_ImportTableSettings.Add(SuccessorMoSettings);
        PathSettings = new ImportTableSettings(typeof(JobDataSet.AlternatePathDataTable), this, "JobPaths");
        m_ImportTableSettings.Add(PathSettings);
        PathNodeSettings = new ImportTableSettings(typeof(JobDataSet.AlternatePathNodeDataTable), this, "JobPathNodes");
        m_ImportTableSettings.Add(PathNodeSettings);
        CustomerConnectionSettings = new ImportTableSettings(typeof(JobDataSet.CustomerDataTable), this, "CustomerConnections");
        m_ImportTableSettings.Add(CustomerConnectionSettings);
        ResourceOperationSettings = new ImportTableSettings(typeof(JobDataSet.ResourceOperationDataTable), this, "JobOperations");
        m_ImportTableSettings.Add(ResourceOperationSettings);
        ResourceRequirementSettings = new ImportTableSettings(typeof(JobDataSet.ResourceRequirementDataTable), this, "JobResources");
        m_ImportTableSettings.Add(ResourceRequirementSettings);
        RequiredCapabilitySettings = new ImportTableSettings(typeof(JobDataSet.CapabilityDataTable), this, "JobResourceCapabilities");
        m_ImportTableSettings.Add(RequiredCapabilitySettings);
        MaterialSettings = new ImportTableSettings(typeof(JobDataSet.MaterialRequirementDataTable), this, "JobMaterials");
        m_ImportTableSettings.Add(MaterialSettings);
        InternalActivitySettings = new ImportTableSettings(typeof(JobDataSet.ActivityDataTable), this, "JobActivities");
        m_ImportTableSettings.Add(InternalActivitySettings);
        ProductSettings = new ImportTableSettings(typeof(JobDataSet.ProductDataTable), this, "JobProducts"); 
        m_ImportTableSettings.Add(ProductSettings);
        OpAttributeSettings = new ImportTableSettings(typeof(JobDataSet.ResourceOperationAttributesDataTable), this, "JobOperationAttributes"); 
        m_ImportTableSettings.Add(OpAttributeSettings);
        CleanoutTriggerTablesSettings = new ImportTableSettings(typeof(CleanoutTriggerTablesDataSet.TableListDataTable), this, "CleanoutTriggerTables");
        m_ImportTableSettings.Add(CleanoutTriggerTablesSettings);
        CleanoutTriggerTablesAssignedResourcesSettings = new ImportTableSettings(typeof(CleanoutTriggerTablesDataSet.AssignedResourcesDataTable), this, "CleanoutTriggerTableResources");
        m_ImportTableSettings.Add(CleanoutTriggerTablesAssignedResourcesSettings);
        OperationCountCleanoutTriggersSettings = new ImportTableSettings(typeof(CleanoutTriggerTablesDataSet.OperationCountCleanoutTriggersDataTable), this, "CleanoutTriggerTableOpCount");
        m_ImportTableSettings.Add(OperationCountCleanoutTriggersSettings);
        ProductionUnitCleanoutTriggersSettings = new ImportTableSettings(typeof(CleanoutTriggerTablesDataSet.ProductionUnitsCleanoutTriggersDataTable), this, "CleanoutTriggerTableProdUnits");
        m_ImportTableSettings.Add(ProductionUnitCleanoutTriggersSettings);
        TimeCleanoutTriggersSettings = new ImportTableSettings(typeof(CleanoutTriggerTablesDataSet.TimeCleanoutTriggersDataTable), this, "CleanoutTriggerTableTime");
        m_ImportTableSettings.Add(TimeCleanoutTriggersSettings);
        
        ResourceConnectorsSettings = new ImportTableSettings(typeof(ResourceConnectorDataSet.ResourceConnectorsDataTable), this, "ResourceConnectors");
        m_ImportTableSettings.Add(ResourceConnectorsSettings);
        ResourceConnectionSettings = new ImportTableSettings(typeof(ResourceConnectorDataSet.ResourceConnectionDataTable), this, "ResourceConnections");
        m_ImportTableSettings.Add(ResourceConnectionSettings);
        
        CompatibilityCodeTablesSettings = new ImportTableSettings(typeof(CompatibilityCodeTableDataSet.CompatibilityCodeTableListDataTable), this, "CompatibilityCodeTables");
        m_ImportTableSettings.Add(CompatibilityCodeTablesSettings);
        CompatibilityCodeTablesAssignedResourcesSettings = new ImportTableSettings(typeof(CompatibilityCodeTableDataSet.CompatibilityCodeTableAssignedResourcesDataTable), this, "CompatibilityCodeTableResources");
        m_ImportTableSettings.Add(CompatibilityCodeTablesAssignedResourcesSettings);
        CompatibilityCodesSettings = new ImportTableSettings(typeof(CompatibilityCodeTableDataSet.CompatibilityCodesDataTable), this, "CompatibilityCodes");
        m_ImportTableSettings.Add(CompatibilityCodesSettings);

        UserFieldSettings = new ImportTableSettings(typeof(PtImportDataSet.UserFieldDefinitionsDataTable), this, "UserFieldDefinitions");
        m_ImportTableSettings.Add(UserFieldSettings);
        StorageAreaSettings = new ImportTableSettings(typeof(PtImportDataSet.StorageAreasDataTable), this, "StorageAreas");
        m_ImportTableSettings.Add(StorageAreaSettings);
        StorageAreaConnectorSettings = new ImportTableSettings(typeof(PtImportDataSet.StorageAreaConnectorDataTable), this, "StorageAreaConnectors");
        m_ImportTableSettings.Add(StorageAreaConnectorSettings);
        StorageAreaConnectorInSettings = new ImportTableSettings(typeof(PtImportDataSet.StorageAreaConnectorInDataTable), this, "StorageAreaConnectorsIn");
        m_ImportTableSettings.Add(StorageAreaConnectorInSettings);
        StorageAreaConnectorOutSettings = new ImportTableSettings(typeof(PtImportDataSet.StorageAreaConnectorOutDataTable), this, "StorageAreaConnectorsOut");
        m_ImportTableSettings.Add(StorageAreaConnectorOutSettings);
        ResourceStorageAreaConnectorInSettings = new ImportTableSettings(typeof(PtImportDataSet.ResourceStorageAreaConnectorInDataTable), this, "ResourceStorageAreaConnectorsIn");
        m_ImportTableSettings.Add(ResourceStorageAreaConnectorInSettings);
        ResourceStorageAreaConnectorOutSettings = new ImportTableSettings(typeof(PtImportDataSet.ResourceStorageAreaConnectorOutDataTable), this, "ResourceStorageAreaConnectorsOut");
        m_ImportTableSettings.Add(ResourceStorageAreaConnectorOutSettings);
        ItemStorageSettings  = new ImportTableSettings(typeof(PtImportDataSet.ItemStorageDataTable), this, "ItemStorage");
        m_ImportTableSettings.Add(ItemStorageSettings);
        ItemStorageLotsSettings = new ImportTableSettings(typeof(PtImportDataSet.ItemStorageLotsDataTable), this, "ItemStorageLots");
        m_ImportTableSettings.Add(ItemStorageLotsSettings);
    }

    public void ReloadList()
    {
        m_ImportTableSettings = new List<ImportTableSettings>();
        m_ImportTableSettings.Add(PlantSettings);
        m_ImportTableSettings.Add(DepartmentSettings);
        m_ImportTableSettings.Add(CellsSettings);
        m_ImportTableSettings.Add(MachineSettings);
        m_ImportTableSettings.Add(CapacityIntervalSettings);
        m_ImportTableSettings.Add(CapacityIntervalResourceSettings);
        m_ImportTableSettings.Add(ProductRulesSettings);
        m_ImportTableSettings.Add(AttributeSettings);
        m_ImportTableSettings.Add(SetupTableAttSettings);
        m_ImportTableSettings.Add(SetupTableAttNameSettings);
        m_ImportTableSettings.Add(SetupTableAttFromSettings);
        m_ImportTableSettings.Add(SetupTableAttToSettings);
        m_ImportTableSettings.Add(SetupTableAttResourceSettings);
        m_ImportTableSettings.Add(AttributeCodeTableSetting);
        m_ImportTableSettings.Add(AttributeCodeTableAttributeExternalIdSetting);
        m_ImportTableSettings.Add(AttributeCodeTableAttributeCodesSetting);
        m_ImportTableSettings.Add(AttributeCodeTableAssignedResourcesSetting);
        m_ImportTableSettings.Add(RecurringCapacityIntervalSettings);
        m_ImportTableSettings.Add(CapabilityAssignmentSettings);
        m_ImportTableSettings.Add(CapabilitySettings);
        m_ImportTableSettings.Add(AllowedHelperResourcesSettings);
        m_ImportTableSettings.Add(WarehouseSettings);
        m_ImportTableSettings.Add(PlantWarehouseSettings);
        m_ImportTableSettings.Add(InventorySettings);
        m_ImportTableSettings.Add(LotsSettings);
        m_ImportTableSettings.Add(ItemSettings);
        m_ImportTableSettings.Add(CustomerSettings);
        m_ImportTableSettings.Add(PurchaseToStockSettings);
        m_ImportTableSettings.Add(SalesOrderSettings);
        m_ImportTableSettings.Add(SalesOrderLineSettings);
        m_ImportTableSettings.Add(SalesOrderLineDistSettings);
        m_ImportTableSettings.Add(ForecastSettings);
        m_ImportTableSettings.Add(ForecastShipmentSettings);
        m_ImportTableSettings.Add(TransferOrderSettings);
        m_ImportTableSettings.Add(TransferOrderDistributionSettings);
        m_ImportTableSettings.Add(JobSettings);
        m_ImportTableSettings.Add(MoSettings);
        m_ImportTableSettings.Add(SuccessorMoSettings);
        m_ImportTableSettings.Add(PathSettings);
        m_ImportTableSettings.Add(PathNodeSettings);
        m_ImportTableSettings.Add(CustomerConnectionSettings);
        m_ImportTableSettings.Add(ResourceOperationSettings);
        m_ImportTableSettings.Add(ResourceRequirementSettings);
        m_ImportTableSettings.Add(RequiredCapabilitySettings);
        m_ImportTableSettings.Add(MaterialSettings);
        m_ImportTableSettings.Add(InternalActivitySettings);
        m_ImportTableSettings.Add(ProductSettings);
        m_ImportTableSettings.Add(OpAttributeSettings);
        m_ImportTableSettings.Add(CleanoutTriggerTablesSettings);
        m_ImportTableSettings.Add(CleanoutTriggerTablesAssignedResourcesSettings);
        m_ImportTableSettings.Add(OperationCountCleanoutTriggersSettings);
        m_ImportTableSettings.Add(ProductionUnitCleanoutTriggersSettings);
        m_ImportTableSettings.Add(TimeCleanoutTriggersSettings);
        m_ImportTableSettings.Add(ResourceConnectorsSettings);
        m_ImportTableSettings.Add(ResourceConnectionSettings);
        m_ImportTableSettings.Add(CompatibilityCodeTablesSettings);
        m_ImportTableSettings.Add(CompatibilityCodeTablesAssignedResourcesSettings);
        m_ImportTableSettings.Add(CompatibilityCodesSettings);
        m_ImportTableSettings.Add(UserFieldSettings);
        m_ImportTableSettings.Add(StorageAreaSettings);
        m_ImportTableSettings.Add(StorageAreaConnectorSettings);
        m_ImportTableSettings.Add(StorageAreaConnectorInSettings);
        m_ImportTableSettings.Add(StorageAreaConnectorOutSettings);
        m_ImportTableSettings.Add(ResourceStorageAreaConnectorInSettings);
        m_ImportTableSettings.Add(ResourceStorageAreaConnectorOutSettings);
        m_ImportTableSettings.Add(ItemStorageSettings);
        m_ImportTableSettings.Add(ItemStorageLotsSettings);
    }

    public ImportTableSettings GetTableByName(string a_name)
    {
        ReloadList();
        ImportTableSettings table = m_ImportTableSettings.FirstOrDefault(t => t.TableName == a_name);
        if (table == null)
        {
            if (Debugger.IsAttached)
            {
                throw new DebugException();
            }

            return null;
        }

        return table;
    }

    public string ReservedWordsWrapper { get; set; }
     #region RESOURCES Step
    public bool IncludePlants { get; set; } = true;

    public bool IncludeDepartments { get; set; } = true;

    public bool IncludeCells { get; set; } = false;

    public bool IncludeResources { get; set; } = true;

    public bool IncludeLastRunActivities { get; set; } = false;

    public bool IncludeCapabilityAssignments { get; set; } = true;

    public bool IncludeAllowedHelperResources { get; set; } = false;

    public bool IncludeCapabilities { get; set; } = true;

    public bool IncludeCapacityIntervals { get; set; } = true;

    public bool IncludeRecurringCapacityIntervals { get; set; } = true;

    public bool IncludeWarehouses { get; set; } = false;

    public bool IncludeItems { get; set; } = false;

    public bool IncludeInventories { get; set; } = false;

    public bool IncludeLots { get; set; } = false;

    public bool IncludeCustomers { get; set; } = false;

    public bool IncludePurchasesToStock { get; set; } = false;

    public bool IncludeSalesOrders { get; set; } = false;

    public bool IncludeForecasts { get; set; } = false;

    public bool IncludeTransferOrders { get; set; } = false;

    public bool IncludeProductRules { get; set; } = false;

    public bool IncludeAttributes { get; set; } = false;

    public bool IncludeAttributeSetupTables { get; set; } = false;

    public bool IncludeAttributeCodeTables { get; set; } = false;

    public bool IncludeSetupCodeTables { get; set; } = false;
    public bool IncludeCleanoutTriggerTables { get; set; } = false;
    
    public bool IncludeResourceConnectors { get; set; } = false;
    public bool IncludeCompatibilityCodeTables { get; set; } = false;

    #endregion
    
    #region JOBS Step
    public bool IncludeJobs { get; set; } = true;

    public bool IncludeManufacturingOrders { get; set; } = true;

    public bool IncludeResourceOperations { get; set; } = true;

    public bool IncludeResourceRequirements { get; set; } = true;

    public bool IncludeRequiredCapabilities { get; set; } = true;

    public bool IncludeInternalActivity { get; set; } = true;

    public bool IncludeMaterials { get; set; } = true;

    public bool IncludeSuccessorMOs { get; set; } = true;

    public bool IncludePaths { get; set; } = false;

    public bool IncludeProducts { get; set; } = false;

    public bool IncludeOpAttributes { get; set; } = false;
    #endregion
    
    #region MAPPINGS Steps
    //!!!!!!!!NOTE:  If adding/removing these, also update SettingsSaver.SaveSettings() and LoadSettings().!!!!
    
    public ImportTableSettings PlantSettings { get; set; }
    
    public ImportTableSettings DepartmentSettings { get; set; }
    
    public ImportTableSettings MachineSettings { get; set; }
    
    public ImportTableSettings CapacityIntervalSettings { get; set; }
    
    public ImportTableSettings CapacityIntervalResourceSettings { get; set; }
    
    public ImportTableSettings ProductRulesSettings { get; set; }
    
    public ImportTableSettings AttributeSettings { get; set; }
    
    public ImportTableSettings SetupTableAttSettings { get; set; }
    
    public ImportTableSettings SetupTableAttNameSettings { get; set; }
    
    public ImportTableSettings SetupTableAttFromSettings { get; set; }
    
    public ImportTableSettings SetupTableAttToSettings { get; set; }
    
    public ImportTableSettings SetupTableAttResourceSettings { get; set; }
    
    
    
    public ImportTableSettings AttributeCodeTableSetting { get; set; }
    
    public ImportTableSettings AttributeCodeTableAttributeExternalIdSetting { get; set; }
    
    public ImportTableSettings AttributeCodeTableAttributeCodesSetting { get; set; }
    
    public ImportTableSettings AttributeCodeTableAssignedResourcesSetting { get; set; }
    
    
    public ImportTableSettings RecurringCapacityIntervalSettings { get; set; }
    
    public ImportTableSettings CapabilityAssignmentSettings { get; set; }
    
    public ImportTableSettings CapabilitySettings { get; set; }
    
    public ImportTableSettings AllowedHelperResourcesSettings { get; set; }
    
    public ImportTableSettings WarehouseSettings { get; set; }
    
    public ImportTableSettings PlantWarehouseSettings { get; set; }
    
    public ImportTableSettings InventorySettings { get; set; }
    
    public ImportTableSettings LotsSettings { get; set; }
    
    public ImportTableSettings ItemSettings { get; set; }
    
    public ImportTableSettings PurchaseToStockSettings { get; set; }
    
    public ImportTableSettings SalesOrderSettings { get; set; }
    
    public ImportTableSettings SalesOrderLineSettings { get; set; }
    
    public ImportTableSettings SalesOrderLineDistSettings { get; set; }
    
    public ImportTableSettings ForecastSettings { get; set; }
    
    public ImportTableSettings ForecastShipmentSettings { get; set; }
    
    public ImportTableSettings TransferOrderSettings { get; set; }
    
    public ImportTableSettings TransferOrderDistributionSettings { get; set; }
    
    public ImportTableSettings JobSettings { get; set; }
    
    public ImportTableSettings MoSettings { get; set; }
    
    public ImportTableSettings SuccessorMoSettings { get; set; }
    
    public ImportTableSettings PathSettings { get; set; }
    
    public ImportTableSettings PathNodeSettings { get; set; }
    
    public ImportTableSettings CustomerConnectionSettings { get; set; }
    
    public ImportTableSettings ResourceOperationSettings { get; set; }
    
    public ImportTableSettings ResourceRequirementSettings { get; set; }
    
    public ImportTableSettings RequiredCapabilitySettings { get; set; }
    
    public ImportTableSettings MaterialSettings { get; set; }
    
    public ImportTableSettings InternalActivitySettings { get; set; }
    
    public ImportTableSettings ProductSettings { get; set; }
    
    public ImportTableSettings OpAttributeSettings { get; set; }
    
    public ImportTableSettings CellsSettings { get; set; }
    
    public ImportTableSettings CustomerSettings { get; set; }
    public ImportTableSettings CleanoutTriggerTablesSettings { get; set; }
    public ImportTableSettings CleanoutTriggerTablesAssignedResourcesSettings { get; set; }
    public ImportTableSettings OperationCountCleanoutTriggersSettings { get; set; }
    public ImportTableSettings ProductionUnitCleanoutTriggersSettings { get; set; }
    public ImportTableSettings TimeCleanoutTriggersSettings { get; set; }
    
    public ImportTableSettings ResourceConnectorsSettings { get; set; }
    public ImportTableSettings ResourceConnectionSettings { get; set; }
    public ImportTableSettings CompatibilityCodeTablesSettings { get; set; }
    public ImportTableSettings CompatibilityCodeTablesAssignedResourcesSettings { get; set; }
    public ImportTableSettings CompatibilityCodesSettings { get; set; }
    public ImportTableSettings UserFieldSettings { get; set; }
    
    public ImportTableSettings StorageAreaSettings { get; set; }
    
    public ImportTableSettings StorageAreaConnectorSettings { get; set; }
    
    public ImportTableSettings StorageAreaConnectorInSettings { get; set; }
    
    public ImportTableSettings StorageAreaConnectorOutSettings { get; set; }
    
    public ImportTableSettings ResourceStorageAreaConnectorInSettings { get; set; }
    
    public ImportTableSettings ResourceStorageAreaConnectorOutSettings { get; set; }
    
    public ImportTableSettings ItemStorageSettings { get; set; }
    public ImportTableSettings ItemStorageLotsSettings { get; set; }
    
    #endregion

    object ICloneable.Clone()
    {
        return Clone();
    }

    public NewImportSettings Clone()
    {
        NewImportSettings clone = (NewImportSettings)MemberwiseClone();
        CloneParentSettingReferences(clone);
        return clone;
    }

    /// <summary>
    /// Set the parent ImportSettings reference
    /// </summary>
    /// <param name="a_clonedSettings"></param>
    private static void CloneParentSettingReferences(NewImportSettings a_clonedSettings)
    {
        a_clonedSettings.PlantSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.DepartmentSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.CellsSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.MachineSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.CapacityIntervalSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.CapacityIntervalResourceSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.AttributeSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.ProductRulesSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.SetupTableAttSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.SetupTableAttNameSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.SetupTableAttFromSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.SetupTableAttToSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.SetupTableAttResourceSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.AttributeCodeTableSetting.ImportSettings = a_clonedSettings;
        a_clonedSettings.AttributeCodeTableAttributeExternalIdSetting.ImportSettings = a_clonedSettings;
        a_clonedSettings.AttributeCodeTableAttributeCodesSetting.ImportSettings = a_clonedSettings;
        a_clonedSettings.AttributeCodeTableAssignedResourcesSetting.ImportSettings = a_clonedSettings;
        a_clonedSettings.RecurringCapacityIntervalSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.CapabilityAssignmentSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.CapabilitySettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.AllowedHelperResourcesSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.WarehouseSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.PlantWarehouseSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.InventorySettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.LotsSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.ItemSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.CustomerSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.PurchaseToStockSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.SalesOrderSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.SalesOrderLineSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.SalesOrderLineDistSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.ForecastSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.ForecastShipmentSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.TransferOrderSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.TransferOrderDistributionSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.JobSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.MoSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.SuccessorMoSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.PathSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.PathNodeSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.CustomerConnectionSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.ResourceOperationSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.ResourceRequirementSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.RequiredCapabilitySettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.MaterialSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.InternalActivitySettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.ProductSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.OpAttributeSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.CleanoutTriggerTablesSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.CleanoutTriggerTablesAssignedResourcesSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.OperationCountCleanoutTriggersSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.ProductionUnitCleanoutTriggersSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.TimeCleanoutTriggersSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.ResourceConnectorsSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.ResourceConnectionSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.CompatibilityCodeTablesSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.CompatibilityCodeTablesAssignedResourcesSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.CompatibilityCodesSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.UserFieldSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.StorageAreaSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.StorageAreaConnectorSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.StorageAreaConnectorInSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.StorageAreaConnectorOutSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.ResourceStorageAreaConnectorInSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.ResourceStorageAreaConnectorOutSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.ItemStorageSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.ItemStorageLotsSettings.ImportSettings = a_clonedSettings;
    }

    /// <summary>
    /// Rebuilds all table settings, using the latest set of updated features. This only needs to be done once per update of the overall settings.
    /// TODO: It might be nice to only do this for updated settings rather than all of them.
    /// </summary>
    public void UpdateTableSettings(NewImportSettings a_importSettings)
    {
        PlantSettings = a_importSettings.PlantSettings;
        DepartmentSettings = a_importSettings.DepartmentSettings;
        CellsSettings = a_importSettings.CellsSettings;
        MachineSettings = a_importSettings.MachineSettings;
        CapacityIntervalSettings = a_importSettings.CapacityIntervalSettings;
        CapacityIntervalResourceSettings = a_importSettings.CapacityIntervalResourceSettings;
        AttributeSettings = a_importSettings.AttributeSettings;
        ProductRulesSettings = a_importSettings.ProductRulesSettings;
        SetupTableAttSettings = a_importSettings.SetupTableAttSettings;
        SetupTableAttNameSettings = a_importSettings.SetupTableAttNameSettings;
        SetupTableAttFromSettings = a_importSettings.SetupTableAttFromSettings;
        SetupTableAttToSettings = a_importSettings.SetupTableAttToSettings;
        SetupTableAttResourceSettings = a_importSettings.SetupTableAttResourceSettings;
        AttributeCodeTableSetting = a_importSettings.AttributeCodeTableSetting;
        AttributeCodeTableAttributeExternalIdSetting = a_importSettings.AttributeCodeTableAttributeExternalIdSetting;
        AttributeCodeTableAttributeCodesSetting = a_importSettings.AttributeCodeTableAttributeCodesSetting;
        AttributeCodeTableAssignedResourcesSetting = a_importSettings.AttributeCodeTableAssignedResourcesSetting;
        RecurringCapacityIntervalSettings = a_importSettings.RecurringCapacityIntervalSettings;
        CapabilityAssignmentSettings = a_importSettings.CapabilityAssignmentSettings;
        CapabilitySettings = a_importSettings.CapabilitySettings;
        AllowedHelperResourcesSettings = a_importSettings.AllowedHelperResourcesSettings;
        WarehouseSettings = a_importSettings.WarehouseSettings;
        PlantWarehouseSettings = a_importSettings.PlantWarehouseSettings;
        InventorySettings = a_importSettings.InventorySettings;
        LotsSettings = a_importSettings.LotsSettings;
        ItemSettings = a_importSettings.ItemSettings;
        CustomerSettings = a_importSettings.CustomerSettings;
        PurchaseToStockSettings = a_importSettings.PurchaseToStockSettings;
        SalesOrderSettings = a_importSettings.SalesOrderSettings;
        SalesOrderLineSettings = a_importSettings.SalesOrderLineSettings;
        SalesOrderLineDistSettings = a_importSettings.SalesOrderLineDistSettings;
        ForecastSettings = a_importSettings.ForecastSettings;
        ForecastShipmentSettings = a_importSettings.ForecastShipmentSettings;
        TransferOrderSettings = a_importSettings.TransferOrderSettings;
        TransferOrderDistributionSettings = a_importSettings.TransferOrderDistributionSettings;
        JobSettings = a_importSettings.JobSettings;
        MoSettings = a_importSettings.MoSettings;
        SuccessorMoSettings = a_importSettings.SuccessorMoSettings;
        PathSettings = a_importSettings.PathSettings;
        PathNodeSettings = a_importSettings.PathNodeSettings;
        CustomerConnectionSettings = a_importSettings.CustomerConnectionSettings;
        ResourceOperationSettings = a_importSettings.ResourceOperationSettings;
        ResourceRequirementSettings = a_importSettings.ResourceRequirementSettings;
        RequiredCapabilitySettings = a_importSettings.RequiredCapabilitySettings;
        MaterialSettings = a_importSettings.MaterialSettings;
        InternalActivitySettings = a_importSettings.InternalActivitySettings;
        ProductSettings = a_importSettings.ProductSettings;
        OpAttributeSettings = a_importSettings.OpAttributeSettings;
        CleanoutTriggerTablesSettings = a_importSettings.CleanoutTriggerTablesSettings;
        CleanoutTriggerTablesAssignedResourcesSettings = a_importSettings.CleanoutTriggerTablesAssignedResourcesSettings;
        OperationCountCleanoutTriggersSettings = a_importSettings.OperationCountCleanoutTriggersSettings;
        ProductionUnitCleanoutTriggersSettings = a_importSettings.ProductionUnitCleanoutTriggersSettings;
        TimeCleanoutTriggersSettings = a_importSettings.TimeCleanoutTriggersSettings;
        ResourceConnectorsSettings = a_importSettings.ResourceConnectorsSettings;
        ResourceConnectionSettings = a_importSettings.ResourceConnectionSettings;
        CompatibilityCodeTablesSettings = a_importSettings.CompatibilityCodeTablesSettings;
        CompatibilityCodeTablesAssignedResourcesSettings = a_importSettings.CompatibilityCodeTablesAssignedResourcesSettings;
        CompatibilityCodesSettings = a_importSettings.CompatibilityCodesSettings;
        UserFieldSettings = a_importSettings.UserFieldSettings;
        StorageAreaSettings = a_importSettings.StorageAreaSettings;
        StorageAreaConnectorSettings = a_importSettings.StorageAreaConnectorSettings;
        StorageAreaConnectorInSettings = a_importSettings.StorageAreaConnectorInSettings;
        StorageAreaConnectorOutSettings = a_importSettings.StorageAreaConnectorOutSettings;
        ResourceStorageAreaConnectorInSettings = a_importSettings.ResourceStorageAreaConnectorInSettings;
        ResourceStorageAreaConnectorOutSettings = a_importSettings.ResourceStorageAreaConnectorOutSettings;
        ItemStorageSettings = a_importSettings.ItemStorageSettings;
        ItemStorageLotsSettings = a_importSettings.ItemStorageLotsSettings;

        PlantSettings.RebuildImportData(this);
        DepartmentSettings.RebuildImportData(this);
        CellsSettings.RebuildImportData(this);
        MachineSettings.RebuildImportData(this);
        CapacityIntervalSettings.RebuildImportData(this);
        CapacityIntervalResourceSettings.RebuildImportData(this);
        AttributeSettings.RebuildImportData(this);
        ProductRulesSettings.RebuildImportData(this);
        SetupTableAttSettings.RebuildImportData(this);
        SetupTableAttNameSettings.RebuildImportData(this);
        SetupTableAttFromSettings.RebuildImportData(this);
        SetupTableAttToSettings.RebuildImportData(this);
        SetupTableAttResourceSettings.RebuildImportData(this);
        AttributeCodeTableSetting.RebuildImportData(this);
        AttributeCodeTableAttributeExternalIdSetting.RebuildImportData(this);
        AttributeCodeTableAttributeCodesSetting.RebuildImportData(this);
        AttributeCodeTableAssignedResourcesSetting.RebuildImportData(this);
        RecurringCapacityIntervalSettings.RebuildImportData(this);
        CapabilityAssignmentSettings.RebuildImportData(this);
        CapabilitySettings.RebuildImportData(this);
        AllowedHelperResourcesSettings.RebuildImportData(this);
        WarehouseSettings.RebuildImportData(this);
        PlantWarehouseSettings.RebuildImportData(this);
        InventorySettings.RebuildImportData(this);
        LotsSettings.RebuildImportData(this);
        ItemSettings.RebuildImportData(this);
        CustomerSettings.RebuildImportData(this);
        PurchaseToStockSettings.RebuildImportData(this);
        SalesOrderSettings.RebuildImportData(this);
        SalesOrderLineSettings.RebuildImportData(this);
        SalesOrderLineDistSettings.RebuildImportData(this);
        ForecastSettings.RebuildImportData(this);
        ForecastShipmentSettings.RebuildImportData(this);
        TransferOrderSettings.RebuildImportData(this);
        TransferOrderDistributionSettings.RebuildImportData(this);
        JobSettings.RebuildImportData(this);
        MoSettings.RebuildImportData(this);
        SuccessorMoSettings.RebuildImportData(this);
        PathSettings.RebuildImportData(this);
        PathNodeSettings.RebuildImportData(this);
        CustomerConnectionSettings.RebuildImportData(this);
        ResourceOperationSettings.RebuildImportData(this);
        ResourceRequirementSettings.RebuildImportData(this);
        RequiredCapabilitySettings.RebuildImportData(this);
        MaterialSettings.RebuildImportData(this);
        InternalActivitySettings.RebuildImportData(this);
        ProductSettings.RebuildImportData(this);
        OpAttributeSettings.RebuildImportData(this);
        CleanoutTriggerTablesSettings.RebuildImportData(this);
        CleanoutTriggerTablesAssignedResourcesSettings.RebuildImportData(this);
        OperationCountCleanoutTriggersSettings.RebuildImportData(this);
        ProductionUnitCleanoutTriggersSettings.RebuildImportData(this);
        TimeCleanoutTriggersSettings.RebuildImportData(this);
        ResourceConnectorsSettings.RebuildImportData(this);
        ResourceConnectionSettings.RebuildImportData(this);
        CompatibilityCodeTablesSettings.RebuildImportData(this);
        CompatibilityCodeTablesAssignedResourcesSettings.RebuildImportData(this);
        CompatibilityCodesSettings.RebuildImportData(this);
        UserFieldSettings.RebuildImportData(this);
        StorageAreaSettings.RebuildImportData(this);
        StorageAreaConnectorSettings.RebuildImportData(this);
        StorageAreaConnectorInSettings.RebuildImportData(this);
        StorageAreaConnectorOutSettings.RebuildImportData(this);
        ResourceStorageAreaConnectorInSettings.RebuildImportData(this);
        ResourceStorageAreaConnectorOutSettings.RebuildImportData(this);
        ItemStorageSettings.RebuildImportData(this);
        ItemStorageLotsSettings.RebuildImportData(this);
    }

    public void UpdateFeatureSettings(NewImportSettings a_settings)
    {
        foreach (KeyValuePair<string, IntegrationFeatureBase> incomingFeature in a_settings.FeaturesSettings.AllFeatures)
        {
            // TODO: if the incoming feature has a feature not in the current one, create a new feature with its properties (since model will be the supertype, but we want subtype)

            // Update all changeable fields
            FeaturesSettings.AllFeatures[incomingFeature.Key].Enabled = incomingFeature.Value.Enabled;
            FeaturesSettings.AllFeatures[incomingFeature.Key].Properties = incomingFeature.Value.Properties;
        }
    }

    public bool? IsFeatureDistinct(string a_featureName)
    {
        IntegrationFeatureBase feature = FeaturesSettings.GetFeatureByName(a_featureName);
        if (!feature.IsBaseTableFeature)
        {
            return null;
        }
        else
        {
            ImportTableSettings importTableSettings = m_ImportTableSettings.FirstOrDefault(table => table.TableName == feature.FeatureName);
            if (importTableSettings == null)
            {
                DebugException.ThrowInDebug("Table Feature 'FeatureName' does not match an ImportTableSettings");
                return null;
            }

            return importTableSettings.DistinctRows;
        }
    }

    public bool? IsFeatureAutoDelete(string a_featureName)
    {
        IntegrationFeatureBase feature = FeaturesSettings.GetFeatureByName(a_featureName);
        if (!feature.IsBaseTableFeature || !feature.CanSetAutoDelete)
        {
            return null;
        }
        else
        {
            ImportTableSettings importTableSettings = m_ImportTableSettings.FirstOrDefault(table => table.TableName == feature.FeatureName);
            if (importTableSettings == null)
            {
                DebugException.ThrowInDebug("Table Feature 'FeatureName' does not match an ImportTableSettings");
                return null;
            }

            return importTableSettings.AutoDelete;
        }
    }

    public void InitializeSettings(IntegrationConfigDTO a_integrationConfigDto)
    {
        InitializeSettings(); // set baseline

        WebAppId = a_integrationConfigDto.Id;
        Name = a_integrationConfigDto.Name;

        Dictionary<PropertyDTO.PropertyLookupKey, PropertyDTO> propertyDtoLookup = BuildPropertyLookup(a_integrationConfigDto);

        foreach (FeatureDTO featureDto in a_integrationConfigDto.Features)
        {
            if (FeaturesSettings.AllFeatures.TryGetValue(featureDto.Name, out IntegrationFeatureBase featureBaseline))
            {
                featureBaseline.Update(featureDto, propertyDtoLookup);

                if (featureBaseline.IsBaseTableFeature)
                {
                    ImportTableSettings tableSettings = GetTableForTableFeature(featureBaseline.FeatureName);
                    tableSettings.Update(featureDto, this);
                }
            }
        }
    }

    private static Dictionary<PropertyDTO.PropertyLookupKey, PropertyDTO> BuildPropertyLookup(IntegrationConfigDTO a_integrationConfigDto)
    {
        Dictionary<PropertyDTO.PropertyLookupKey, PropertyDTO> propertyDtoLookup = new Dictionary<PropertyDTO.PropertyLookupKey, PropertyDTO>();
        foreach (var foo in a_integrationConfigDto.Properties)
        {
            PropertyDTO.PropertyLookupKey propertyLookupKey = new PropertyDTO.PropertyLookupKey(foo.TableName, foo.ColumnName);
            if (propertyDtoLookup.ContainsKey(propertyLookupKey))
            {
                // A property can be in multiple features - but this is a good breakpoint to confirm it *should* be, as we sort out the data
            }
            else
            {
                propertyDtoLookup.Add(propertyLookupKey, foo);
            }
        }

        return propertyDtoLookup;
    }

    public IntegrationConfigDTO ToConfigDto()
    {
        return new IntegrationConfigDTO()
        {
            Id = WebAppId,
            Name = Name,
            Features = FeaturesSettings.AllFeatures.Values
                      .Select(feature => new FeatureDTO()
                      {
                          Name = feature.FeatureName,
                          Id = feature.WebAppId,
                          Enabled = feature.Enabled,
                          Distinct = IsFeatureDistinct(feature.FeatureName),
                          AutoDelete = IsFeatureAutoDelete(feature.FeatureName),
                      }).ToList(),

            Properties = FeaturesSettings.AllFeatures.Values
                        .SelectMany(feature => feature.Properties)
                        .Distinct() // no need to store duplicates, they can be redistributed when the data is loaded
                        .Select(prop => new PropertyDTO()
                        {
                            Id = prop.WebAppId,
                            ColumnName = prop.ColumnName,
                            TableName = prop.TableName,
                            DataType = prop.DataType,
                            FixedValue = prop.FixedValue,
                            SourceOption = prop.SourceOption
                        }).ToList(),
            VersionNumber = c_CONFIG_VERSION_NUMBER.ToString()
};
    }
    /// <summary>
    /// Finds the baseline feature that represents a provided table.
    /// </summary>
    /// <param name="a_atableTableName"></param>
    /// <returns></returns>
    private ImportTableSettings GetTableForTableFeature(string a_featureName)
    {
        if (FeaturesSettings.AllFeatures.TryGetValue(a_featureName, out IntegrationFeatureBase feature))
        {
            if (!feature.IsBaseTableFeature)
            {
                return null;
            }

            // TODO: once all name changes are done, confirm the ImportSettings TableName always matches the BaseTableFeature name
            ImportTableSettings importTableSettings = m_ImportTableSettings.FirstOrDefault(tableSettings => tableSettings.TableName == a_featureName);
            if (importTableSettings == null)
            {
                DebugException.ThrowInDebug("Feature is tagged as being for Base Table, but no TableSettings exist with the same Table Name");
            }

            return importTableSettings;
        }
        
        DebugException.ThrowInDebug("Feature with given name does not exist");
        return null;
    }


}