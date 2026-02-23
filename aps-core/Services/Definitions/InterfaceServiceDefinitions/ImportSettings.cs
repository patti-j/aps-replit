using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ImportDefintions;

/// <summary>
/// Stores the settings for the interface, including mappings and all options.
/// </summary>
public class ImportSettings : ICloneable
{
    public void InitializeSettings()
    {
        UserSettings= new MapStepSettings(typeof(PtImportDataSet.UsersDataTable), this);
        PlantSettings = new MapStepSettings(typeof(PtImportDataSet.PlantsDataTable), this);
        DepartmentSettings = new MapStepSettings(typeof(PtImportDataSet.DepartmentsDataTable), this);
        CellsSettings = new MapStepSettings(typeof(PtImportDataSet.CellsDataTable), this);
        MachineSettings = new MapStepSettings(typeof(ResourceTDataSet.ResourceDataTable), this);
        CapacityIntervalSettings = new MapStepSettings(typeof(CapacityIntervalTDataSet.CapacityIntervalDataTable), this);
        CapacityIntervalResourceSettings = new MapStepSettings(typeof(CapacityIntervalTDataSet.ResourcesDataTable), this);
        ProductRulesSettings = new MapStepSettings(typeof(ProductRulesTDataSet.ProductRulesDataTable), this);
        AttributeSettings = new MapStepSettings(typeof(PtImportDataSet.PTAttributesDataTable), this);
        SetupTableAttSettings = new MapStepSettings(typeof(LookupAttributeNumberRangeDataSet.TableListDataTable), this);
        SetupTableAttNameSettings = new MapStepSettings(typeof(LookupAttributeNumberRangeDataSet.AttributeRangesDataTable), this);
        SetupTableAttFromSettings = new MapStepSettings(typeof(LookupAttributeNumberRangeDataSet.From_RangesDataTable), this);
        SetupTableAttToSettings = new MapStepSettings(typeof(LookupAttributeNumberRangeDataSet.To_RangesDataTable), this);
        SetupTableAttResourceSettings = new MapStepSettings(typeof(LookupAttributeNumberRangeDataSet.AssignedResourcesDataTable), this);
        AttributeCodeTableSetting = new MapStepSettings(typeof(LookupAttributeCodeTableDataSet.TableListDataTable), this);
        AttributeCodeTableAttributeExternalIdSetting = new MapStepSettings(typeof(LookupAttributeCodeTableDataSet.AttributeExternalIdDataTable), this);
        AttributeCodeTableAttributeCodesSetting = new MapStepSettings(typeof(LookupAttributeCodeTableDataSet.AttributeCodesDataTable), this);
        AttributeCodeTableAssignedResourcesSetting = new MapStepSettings(typeof(LookupAttributeCodeTableDataSet.AssignedResourcesDataTable), this);

        RecurringCapacityIntervalSettings = new MapStepSettings(typeof(RecurringCapacityIntervalTDataSet.RecurringCapacityIntervalsDataTable), this);
        CapabilityAssignmentSettings = new MapStepSettings(typeof(ResourceTDataSet.CapabilityAssignmentsDataTable), this);
        CapabilitySettings = new MapStepSettings(typeof(PtImportDataSet.CapabilitiesDataTable), this);
        AllowedHelperResourcesSettings = new MapStepSettings(typeof(ResourceTDataSet.AllowedHelperResourcesDataTable), this);
        WarehouseSettings = new MapStepSettings(typeof(PtImportDataSet.WarehousesDataTable), this);
        PlantWarehouseSettings = new MapStepSettings(typeof(PtImportDataSet.SuppliedPlantsDataTable), this);
        InventorySettings = new MapStepSettings(typeof(PtImportDataSet.InventoriesDataTable), this);
        LotsSettings = new MapStepSettings(typeof(PtImportDataSet.LotsDataTable), this);
        ItemSettings = new MapStepSettings(typeof(PtImportDataSet.ItemsDataTable), this);
        CustomerSettings = new MapStepSettings(typeof(PtImportDataSet.CustomerDataTable), this);
        PurchaseToStockSettings = new MapStepSettings(typeof(PtImportDataSet.PurchaseToStocksDataTable), this);
        SalesOrderSettings = new MapStepSettings(typeof(SalesOrderTDataSet.SalesOrderDataTable), this);
        SalesOrderLineSettings = new MapStepSettings(typeof(SalesOrderTDataSet.SalesOrderLineDataTable), this);
        SalesOrderLineDistSettings = new MapStepSettings(typeof(SalesOrderTDataSet.SalesOrderLineDistDataTable), this);
        ForecastSettings = new MapStepSettings(typeof(ForecastTDataSet.ForecastsDataTable), this);
        ForecastShipmentSettings = new MapStepSettings(typeof(ForecastTDataSet.ForecastShipmentsDataTable), this);
        TransferOrderSettings = new MapStepSettings(typeof(TransferOrderTDataSet.TransferOrderDataTable), this);
        TransferOrderDistributionSettings = new MapStepSettings(typeof(TransferOrderTDataSet.TransferOrderDistributionDataTable), this);

        JobSettings = new MapStepSettings(typeof(JobDataSet.JobDataTable), this);
        MoSettings = new MapStepSettings(typeof(JobDataSet.ManufacturingOrderDataTable), this);
        SuccessorMoSettings = new MapStepSettings(typeof(JobDataSet.SuccessorMODataTable), this);
        PathSettings = new MapStepSettings(typeof(JobDataSet.AlternatePathDataTable), this);
        PathNodeSettings = new MapStepSettings(typeof(JobDataSet.AlternatePathNodeDataTable), this);
        CustomerConnectionSettings = new MapStepSettings(typeof(JobDataSet.CustomerDataTable), this);
        ResourceOperationSettings = new MapStepSettings(typeof(JobDataSet.ResourceOperationDataTable), this);
        ResourceRequirementSettings = new MapStepSettings(typeof(JobDataSet.ResourceRequirementDataTable), this);
        RequiredCapabilitySettings = new MapStepSettings(typeof(JobDataSet.CapabilityDataTable), this);
        MaterialSettings = new MapStepSettings(typeof(JobDataSet.MaterialRequirementDataTable), this);
        InternalActivitySettings = new MapStepSettings(typeof(JobDataSet.ActivityDataTable), this);
        ProductSettings = new MapStepSettings(typeof(JobDataSet.ProductDataTable), this);
        OpAttributeSettings = new MapStepSettings(typeof(JobDataSet.ResourceOperationAttributesDataTable), this);
        CleanoutTriggerTablesSettings = new MapStepSettings(typeof(CleanoutTriggerTablesDataSet.TableListDataTable), this);
        CleanoutTriggerTablesAssignedResourcesSettings = new MapStepSettings(typeof(CleanoutTriggerTablesDataSet.AssignedResourcesDataTable), this);
        StorageAreaItemCleanoutTablesSettings = new MapStepSettings(typeof(LookupItemCleanoutTableDataSet.TableListDataTable), this);
        OperationCountCleanoutTriggersSettings = new MapStepSettings(typeof(CleanoutTriggerTablesDataSet.OperationCountCleanoutTriggersDataTable), this);
        ProductionUnitCleanoutTriggersSettings = new MapStepSettings(typeof(CleanoutTriggerTablesDataSet.ProductionUnitsCleanoutTriggersDataTable), this);
        TimeCleanoutTriggersSettings = new MapStepSettings(typeof(CleanoutTriggerTablesDataSet.TimeCleanoutTriggersDataTable), this);

        StorageAreaItemCleanoutTablesSettings = new MapStepSettings(typeof(LookupItemCleanoutTableDataSet.TableListDataTable), this);
        StorageAreaCleanAssignmentSettings = new MapStepSettings(typeof(LookupItemCleanoutTableDataSet.AssignedResourcesDataTable), this);
        StorageAreaItemCleanoutsSettings = new MapStepSettings(typeof(LookupItemCleanoutTableDataSet.ItemCleanoutsDataTable), this);

        ResourceConnectorsSettings = new MapStepSettings(typeof(ResourceConnectorDataSet.ResourceConnectorsDataTable), this);
        ResourceConnectionSettings = new MapStepSettings(typeof(ResourceConnectorDataSet.ResourceConnectionDataTable), this);

        CompatibilityCodeTablesSettings = new MapStepSettings(typeof(CompatibilityCodeTableDataSet.CompatibilityCodeTableListDataTable), this);
        CompatibilityCodeTablesAssignedResourcesSettings = new MapStepSettings(typeof(CompatibilityCodeTableDataSet.CompatibilityCodeTableAssignedResourcesDataTable), this);
        CompatibilityCodesSettings = new MapStepSettings(typeof(CompatibilityCodeTableDataSet.CompatibilityCodesDataTable), this);
        UserFieldSettings = new MapStepSettings(typeof(PtImportDataSet.UserFieldDefinitionsDataTable), this);

        StorageAreaSettings = new MapStepSettings(typeof(PtImportDataSet.StorageAreasDataTable), this);
        StorageAreaConnectorSettings = new MapStepSettings(typeof(PtImportDataSet.StorageAreaConnectorDataTable), this);
        StorageAreaConnectorInSettings = new MapStepSettings(typeof(PtImportDataSet.StorageAreaConnectorInDataTable), this);
        StorageAreaConnectorOutSettings = new MapStepSettings(typeof(PtImportDataSet.StorageAreaConnectorOutDataTable), this);
        ResourceStorageAreaConnectorInSettings = new MapStepSettings(typeof(PtImportDataSet.ResourceStorageAreaConnectorInDataTable), this);
        ResourceStorageAreaConnectorOutSettings = new MapStepSettings(typeof(PtImportDataSet.ResourceStorageAreaConnectorOutDataTable), this);

        ItemStorageSettings  = new MapStepSettings(typeof(PtImportDataSet.ItemStorageDataTable), this);
        ItemStorageLotsSettings  = new MapStepSettings(typeof(PtImportDataSet.ItemStorageLotsDataTable), this);
    }

    /// <summary>
    /// This can be used if we change the format of this file.
    /// </summary>
    public int VERSION { get; set; } = -1;

    public bool UseCustomInterface { get; set; }

    /// <summary>
    /// Text to wrap around column names to avoid reserved word usage when building queries.
    /// </summary>

    public string ReservedWordsWrapper { get; set; }

    #region WELCOME Step
    public bool ShowWelcomeStep { get; set; } = true;
    #endregion

        #region RESOURCES Step

        public bool IncludeUserFields { get; set; } = true;

        public bool IncludeUsers { get; set; } = true;

    public bool IncludePlants { get; set; } = true;

    public bool IncludeDepartments { get; set; } = true;

    public bool IncludeCells { get; set; } = false;

    public bool IncludeResources { get; set; } = true;

    public bool IncludeCapabilityAssignments { get; set; } = true;

    public bool IncludeAllowedHelperResources { get; set; } = false;

    public bool IncludeCapabilities { get; set; } = true;

    public bool IncludeCapacityIntervals { get; set; } = true;

    public bool IncludeRecurringCapacityIntervals { get; set; } = true;

    public bool IncludeWarehouses { get; set; } = false;

    public bool IncludeItems { get; set; } = false;

    public bool IncludeInventories { get; set; } = false;

    public bool IncludeLots { get; set; } = false;

    public bool IncludeStorageArea { get; set; } = false;
    public bool IncludeItemStorage { get; set; } = false;
    public bool IncludeItemStorageLots { get; set; } = false;
    public bool IncludeStorageAreaConnectors { get; set; } = false;
    public bool IncludeStorageAreaConnectorIn { get; set; } = false;
    public bool IncludeStorageAreaConnectorOut { get; set; } = false;
    public bool IncludeResourceStorageAreaConnectorIn { get; set; } = false;
    public bool IncludeResourceStorageAreaConnectorOut { get; set; } = false;

    public bool IncludeCustomers { get; set; } = false;

    public bool IncludePurchasesToStock { get; set; } = false;

    public bool IncludeSalesOrders { get; set; } = false;

    public bool IncludeForecasts { get; set; } = false;

    public bool IncludeTransferOrders { get; set; } = false;

    public bool IncludeProductRules { get; set; } = false;

    public bool IncludeAttributes { get; set; } = false;

    public bool IncludeAttributeSetupTables { get; set; } = false;

    public bool IncludeAttributeCodeTables { get; set; } = false;

    public bool IncludeCleanoutTriggerTables { get; set; } = false;
    
    public bool IncludeResourceConnectors { get; set; } = false;
    public bool IncludeCompatibilityCodeTables { get; set; } = false;
    public bool IncludeStorageAreaItemCleanoutTables { get; set; } = false;

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
    public MapStepSettings PlantSettings { get; set; }
    public MapStepSettings UserSettings { get; set; }

    public MapStepSettings DepartmentSettings { get; set; }

    public MapStepSettings MachineSettings { get; set; }

    public MapStepSettings CapacityIntervalSettings { get; set; }

    public MapStepSettings CapacityIntervalResourceSettings { get; set; }

    public MapStepSettings ProductRulesSettings { get; set; }

    public MapStepSettings AttributeSettings { get; set; }

    public MapStepSettings SetupTableAttSettings { get; set; }

    public MapStepSettings SetupTableAttNameSettings { get; set; }

    public MapStepSettings SetupTableAttFromSettings { get; set; }

    public MapStepSettings SetupTableAttToSettings { get; set; }

    public MapStepSettings SetupTableAttResourceSettings { get; set; }

    //Attribute Code Tables

    public MapStepSettings AttributeCodeTableSetting { get; set; }

    public MapStepSettings AttributeCodeTableAttributeExternalIdSetting { get; set; }

    public MapStepSettings AttributeCodeTableAttributeCodesSetting { get; set; }

    public MapStepSettings AttributeCodeTableAssignedResourcesSetting { get; set; }

    public MapStepSettings RecurringCapacityIntervalSettings { get; set; }

    public MapStepSettings CapabilityAssignmentSettings { get; set; }

    public MapStepSettings CapabilitySettings { get; set; }

    public MapStepSettings AllowedHelperResourcesSettings { get; set; }

    public MapStepSettings WarehouseSettings { get; set; }

    public MapStepSettings PlantWarehouseSettings { get; set; }

    public MapStepSettings InventorySettings { get; set; }

    public MapStepSettings LotsSettings { get; set; }

    public MapStepSettings ItemSettings { get; set; }

    public MapStepSettings PurchaseToStockSettings { get; set; }

    public MapStepSettings SalesOrderSettings { get; set; }

    public MapStepSettings SalesOrderLineSettings { get; set; }

    public MapStepSettings SalesOrderLineDistSettings { get; set; }

    public MapStepSettings ForecastSettings { get; set; }

    public MapStepSettings ForecastShipmentSettings { get; set; }

    public MapStepSettings TransferOrderSettings { get; set; }

    public MapStepSettings TransferOrderDistributionSettings { get; set; }

    public MapStepSettings JobSettings { get; set; }

    public MapStepSettings MoSettings { get; set; }

    public MapStepSettings SuccessorMoSettings { get; set; }

    public MapStepSettings PathSettings { get; set; }

    public MapStepSettings PathNodeSettings { get; set; }

    public MapStepSettings CustomerConnectionSettings { get; set; }

    public MapStepSettings ResourceOperationSettings { get; set; }

    public MapStepSettings ResourceRequirementSettings { get; set; }

    public MapStepSettings RequiredCapabilitySettings { get; set; }

    public MapStepSettings MaterialSettings { get; set; }

    public MapStepSettings InternalActivitySettings { get; set; }

    public MapStepSettings ProductSettings { get; set; }

    public MapStepSettings OpAttributeSettings { get; set; }

    public MapStepSettings CellsSettings { get; set; }

    public MapStepSettings CustomerSettings { get; set; }
    public MapStepSettings CleanoutTriggerTablesSettings { get; set; }
    public MapStepSettings CleanoutTriggerTablesAssignedResourcesSettings { get; set; }
    public MapStepSettings OperationCountCleanoutTriggersSettings { get; set; }
    public MapStepSettings ProductionUnitCleanoutTriggersSettings { get; set; }
    public MapStepSettings TimeCleanoutTriggersSettings { get; set; }

    public MapStepSettings StorageAreaItemCleanoutTablesSettings { get; set; }
    public MapStepSettings StorageAreaCleanAssignmentSettings { get; set; }
    public MapStepSettings StorageAreaItemCleanoutsSettings { get; set; }

    public MapStepSettings ResourceConnectorsSettings { get; set; }
    public MapStepSettings ResourceConnectionSettings { get; set; }
    public MapStepSettings CompatibilityCodeTablesSettings { get; set; }
    public MapStepSettings CompatibilityCodeTablesAssignedResourcesSettings { get; set; }
    public MapStepSettings CompatibilityCodesSettings { get; set; }
    public MapStepSettings UserFieldSettings { get; set; }
    public MapStepSettings StorageAreaSettings { get; set; }
    public MapStepSettings StorageAreaConnectorSettings { get; set; }
    public MapStepSettings StorageAreaConnectorInSettings { get; set; }
    public MapStepSettings StorageAreaConnectorOutSettings { get; set; }
    public MapStepSettings ResourceStorageAreaConnectorInSettings { get; set; }
    public MapStepSettings ResourceStorageAreaConnectorOutSettings { get; set; }
    public MapStepSettings ItemStorageSettings { get; set; }
    public MapStepSettings ItemStorageLotsSettings { get; set; }
    #endregion

    #region ICloneable Members
    object ICloneable.Clone()
    {
        return Clone();
    }

    public ImportSettings Clone()
    {
        ImportSettings clone = (ImportSettings)MemberwiseClone();
        CloneParentSettingReferences(clone);
        return clone;
    }

    /// <summary>
    /// Set the parent ImportSettings reference
    /// </summary>
    /// <param name="a_clonedSettings"></param>
    private static void CloneParentSettingReferences(ImportSettings a_clonedSettings)
    {
        a_clonedSettings.UserSettings.ImportSettings = a_clonedSettings;
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
        a_clonedSettings.StorageAreaItemCleanoutTablesSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.OperationCountCleanoutTriggersSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.ProductionUnitCleanoutTriggersSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.TimeCleanoutTriggersSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.StorageAreaItemCleanoutTablesSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.StorageAreaCleanAssignmentSettings.ImportSettings = a_clonedSettings;
        a_clonedSettings.StorageAreaItemCleanoutsSettings.ImportSettings = a_clonedSettings;
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
    #endregion

    public void OnDeserialized(StreamingContext a_streamingContext)
    {
        PropertyInfo[] propInfos = GetType().GetProperties();
        foreach (PropertyInfo pi in propInfos)
        {
            if (pi.PropertyType == typeof(MapStepSettings))
            {
                ((MapStepSettings)pi.GetValue(this)).ImportSettings = this;
            }
        }

        //UserSettings.InterfaceSettings = this;
        //PlantSettings.InterfaceSettings = this;
        //DepartmentSettings.InterfaceSettings = this;
        //MachineSettings.InterfaceSettings = this;
        //CapacityIntervalSettings.InterfaceSettings = this;
        //CapacityIntervalResourceSettings.InterfaceSettings = this;
        //ProductRulesSettings.InterfaceSettings = this;
        //SetupTableAttSettings.InterfaceSettings = this;
        //SetupTableAttNameSettings.InterfaceSettings = this;
        //SetupTableAttFromSettings.InterfaceSettings = this;
        //SetupTableAttToSettings.InterfaceSettings = this;
        //SetupTableAttResourceSettings.InterfaceSettings = this;
        //AttributeCodeTableSetting.InterfaceSettings = this;
        //AttributeCodeTableAttributeNameSetting.InterfaceSettings = this;
        //AttributeCodeTableAttributeCodesSetting.InterfaceSettings = this;
        //AttributeCodeTableAssignedResourcesSetting.InterfaceSettings = this;

        //SetupCodeTableSetting.InterfaceSettings = this;
        //SetupCodeTableSetupCodesSetting.InterfaceSettings = this;
        //SetupCodeTableAssignedResourcesSetting.InterfaceSettings = this;

        //RecurringCapacityIntervalSettings.InterfaceSettings = this;
        //CapabilityAssignmentSettings.InterfaceSettings = this;
        //CapabilitySettings.InterfaceSettings = this;
        //ResourceConnectorSettings.InterfaceSettings = this;
        //AllowedHelperResourcesSettings.InterfaceSettings = this;
        //WarehouseSettings.InterfaceSettings = this;
        //PlantWarehouseSettings.InterfaceSettings = this;
        //InventorySettings.InterfaceSettings = this;
        //LotsSettings.InterfaceSettings = this;
        //ItemSettings.InterfaceSettings = this;
        //PurchaseToStockSettings.InterfaceSettings = this;
        //SalesOrderSettings.InterfaceSettings = this;
        //SalesOrderLineSettings.InterfaceSettings = this;
        //SalesOrderLineDistSettings.InterfaceSettings = this;
        //ForecastSettings.InterfaceSettings = this;
        //ForecastShipmentSettings.InterfaceSettings = this;
        //TransferOrderSettings.InterfaceSettings = this;
        //TransferOrderDistributionSettings.InterfaceSettings = this;

        //JobSettings.InterfaceSettings = this;
        //MoSettings.InterfaceSettings = this;
        //SuccessorMoSettings.InterfaceSettings = this;
        //PathSettings.InterfaceSettings = this;
        //PathNodeSettings.InterfaceSettings = this;
        //ResourceOperationSettings.InterfaceSettings = this;
        //ResourceRequirementSettings.InterfaceSettings = this;
        //RequiredCapabilitySettings.InterfaceSettings = this;
        //MaterialSettings.InterfaceSettings = this;
        //InternalActivitySettings.InterfaceSettings = this;
        //ProductSettings.InterfaceSettings = this;
        //OpAttributeSettings.InterfaceSettings = this;
    }
}

public class MapStepSettings : ICloneable
{
    private const string c_displayOnlyPrefix = "_";
    private const string c_blankNextExpression = "''";
    private const string c_select = "SELECT ";

    internal ImportSettings ImportSettings { get; set; }

    public MapStepSettings() { }

    public MapStepSettings(Type objectType, ImportSettings a_aImportSettings)
    {
        SetMapInfosForType(objectType); //Load the Property info for the object type.
        TypeName = objectType.Name;
        ImportSettings = a_aImportSettings;
    }

    public string TypeName { get; set; }

    public bool AutoDelete { get; set; }

    public bool DistinctRows { get; set; }

    public string FromjoinExpression { get; set; } = "";

    public string WhereExpression { get; set; } = "";

    private MapInfoArray mapInfos; //One string for each map expression
    //[ReadOnly(true)] //Keeps it from going to xml settings file.

    public MapInfoArray MapInfos
    {
        get => mapInfos;
        set
        {
            mapInfos = value;
            //Backwards compatibility: Remove distinct from column infos. There is now a distinct option in the wizard
            for (int i = 0; i < mapInfos.Count; i++)
            {
                if (mapInfos[i].SourceExpression.ToLower().StartsWith("distinct"))
                {
                    mapInfos[i].SourceExpression = Regex.Replace(mapInfos[i].SourceExpression, "distinct", "", RegexOptions.IgnoreCase).TrimStart();
                    DistinctRows = true;
                }
            }
        }
    }

    /// <summary>
    /// Set the Property names from the object using Reflection.
    /// </summary>
    /// <param name="type"></param>
    private void SetMapInfosForType(Type type)
    {
        //Transitioning to use datasets instead of readers.
        if (type.Name.EndsWith("Table"))
        {
            DataTable table = (DataTable)Activator.CreateInstance(type);
            SetMapInfosForTableType(table);
        }
        else //use reflection
        {
            MapInfos = new MapInfoArray();
            MapInfo map;

            int propCount = TypeDescriptor.GetProperties(type).Count;
            PropertyDescriptor pd;
            for (int i = 0; i < propCount; i++)
            {
                pd = TypeDescriptor.GetProperties(type).Sort()[i]; //.Sort(GetSortStringArray(type))[i]; //sort by default sort specified in the object
                if (!pd.IsReadOnly && pd.IsBrowsable)
                {
                    map = new MapInfo();
                    map.PropertyName = pd.Name;
                    map.PropertyType = pd.PropertyType.Name;
                    map.PropertyDescription = pd.Description;
                    //Store whether the property is required or not.
                    RequiredAttribute requiredAttribute = (RequiredAttribute)pd.Attributes[typeof(RequiredAttribute)];
                    if (requiredAttribute != null)
                    {
                        map.Required = requiredAttribute.Required;
                    }
                    else
                    {
                        map.Required = false;
                    }

                    MapInfos.Add(map);
                }
            }
        }
    }

    /// <summary>
    /// Set the Property names from the table object using Reflection.
    /// </summary>
    private void SetMapInfosForTableType(DataTable table)
    {
        MapInfos = new MapInfoArray();
        MapInfo map;

        int columnCount = table.Columns.Count;

        for (int i = 0; i < columnCount; i++)
        {
            DataColumn col = table.Columns[i];
            bool markedForImport = col.ExtendedProperties["Non-Importable"] == null;

            if (markedForImport) //Some fields are for use in the user interface as display only -- can't import them.
            {
                map = new MapInfo();
                map.PropertyName = col.ColumnName; //Must match dataset col name
                map.PropertyType = col.DataType.Name;
                //map.PropertyDescription = pd.Description;
                //Store whether the property is required or not.
                map.Required = !col.AllowDBNull;
                MapInfos.Add(map);
            }
        }
    }

    public void Update(MapStepSettings newSettings)
    {
        ImportSettings = newSettings.ImportSettings;
        AutoDelete = newSettings.AutoDelete;
        FromjoinExpression = newSettings.FromjoinExpression;
        WhereExpression = newSettings.WhereExpression;
        DistinctRows = newSettings.DistinctRows;
        MapInfos = newSettings.MapInfos;
    }

    public string GetCommandText(bool a_usingNewTableQuery, int a_rowLimit = 0)
    {
        string cText = c_select;

        if (DistinctRows)
        {
            cText += " DISTINCT ";
        }

        if (a_rowLimit > 0)
        {
            cText += $" TOP {a_rowLimit} ";
        }

        if (a_usingNewTableQuery)
        {
            string leftWrapChar = "";
            string rightWrapChar = "";
            if (ImportSettings.ReservedWordsWrapper != null && ImportSettings.ReservedWordsWrapper.Length >= 2)
            {
                leftWrapChar = ImportSettings.ReservedWordsWrapper.Substring(0, 1);
                rightWrapChar = ImportSettings.ReservedWordsWrapper.Substring(1, 1);
            }

            //FIELDS
            bool addedFirstField = false;
            for (int i = 0; i < MapInfos.Count; i++)
            {
                MapInfo mapInfo = MapInfos[i];
                string nextExpression = mapInfo.SourceExpression;

                if (!string.IsNullOrWhiteSpace(nextExpression))
                {
                    if (addedFirstField) //already have a field so need a comma
                    {
                        cText = cText + ",";
                    }

                    if (nextExpression.Trim().ToUpperInvariant() == mapInfo.PropertyName.Trim().ToUpperInvariant())
                    {
                        cText = $"{cText} {nextExpression}"; //Can't have AS with the same alias as the field name.  Returns error: Circular reference caused by alias '<field name>' in query definition's SELECT list.                             
                    }
                    else
                    {
                        nextExpression = RemoveAsFromExpression(nextExpression);
                        cText = $"{cText} {nextExpression} as {leftWrapChar}{mapInfo.PropertyName}{rightWrapChar}"; //For MISYS Prospect running Pervasive, removed brackets that were around {2}.  Was giving <<???>> error.
                    }

                    addedFirstField = true;
                }
            }

            //FROM
            string fromExpression = $" FROM {FromjoinExpression}";

            //WHERE
            string tempWhereExpression = "";
            if (WhereExpression.Trim().Length > 0)
            {
                tempWhereExpression = $" WHERE {WhereExpression.Trim()}";
            }

            return cText + fromExpression + tempWhereExpression;
        }
        else
        {
            string externalId = "";
            string plantExternalId = "";
            string departmentExternalId = "";
            string resourceExternalId = "";

            //FIELDS
            for (int i = 0; i < MapInfos.Count; i++)
            {
                MapInfo mapInfo = MapInfos[i];
                string nextExpression = mapInfo.SourceExpression;
                if (nextExpression.Trim().Length == 0)
                {
                    nextExpression = c_blankNextExpression; //Can't have blanks in sql
                }

                if (cText.Length > c_select.Length) //already have a field so need a comma
                {
                    cText = cText + ",";
                }

                cText = cText + " " + nextExpression;

                //Get values for ORDER BY clause for ResourceT Capability Assignment                        
                if (mapInfo.PropertyName == "ExternalId")
                {
                    externalId = nextExpression;
                }

                if (mapInfo.PropertyName == "PlantExternalId")
                {
                    plantExternalId = nextExpression;
                }

                if (mapInfo.PropertyName == "DepartmentExternalId")
                {
                    departmentExternalId = nextExpression;
                }

                if (mapInfo.PropertyName == "ResourceExternalId")
                {
                    resourceExternalId = nextExpression;
                }
            }

            string orderByExpression = "";

            orderByExpression = AddOrderBy(orderByExpression, externalId); //this has to be before the PlantExternalId, DeptExternalId, WarehouseEternalId or else it will be ingored since it's a substring of them.

            //ResourceT
            orderByExpression = AddOrderBy(orderByExpression, plantExternalId);
            orderByExpression = AddOrderBy(orderByExpression, departmentExternalId);
            orderByExpression = AddOrderBy(orderByExpression, resourceExternalId);

            orderByExpression = RemoveAsFromExpression(orderByExpression);

            string tempOrderBy = orderByExpression;
            orderByExpression = orderByExpression.Replace(",,", ","); //get rid of blanks in order by
            if (orderByExpression.Length > 0)
            {
                tempOrderBy = " ORDER BY " + orderByExpression + " ASC";
            }

            //FROM
            string fromExpression = " FROM " + FromjoinExpression;

            //WHERE
            string tempWhereExpression = "";
            if (WhereExpression.Trim().Length > 0)
            {
                tempWhereExpression = " WHERE " + WhereExpression.Trim();
            }

            //ORDER BY
            cText = cText + fromExpression + tempWhereExpression + tempOrderBy;

            return cText;
        }
    }

    private string AddOrderBy(string originalExpression, string addOnExpression)
    {
        string output;
        if (addOnExpression == c_blankNextExpression)
        {
            output = originalExpression;
        }
        else
        {
            addOnExpression = RemoveAsFromExpression(addOnExpression); //can't have 'AS alias' in Order By clause
            if (originalExpression.Length > 0)
            {
                //Don't add it if it's already in the list since this is not allowed. Using "," below to avoid cases where the new field is a part of the name of some other field.
                if (originalExpression.Contains(addOnExpression))
                {
                    output = originalExpression;
                }
                else
                {
                    output = string.Format("{0},{1}", originalExpression, addOnExpression);
                }
            }
            else
            {
                output = addOnExpression;
            }
        }

        return output;
    }

    /// <summary>
    /// Returns the express minus the ' as alias' suffix if any.
    /// </summary>
    private string RemoveAsFromExpression(string expression)
    {
        string[] result = Regex.Split(expression, " AS ", RegexOptions.IgnoreCase);
        if (result[0].IndexOf("cast(", StringComparison.CurrentCultureIgnoreCase) == -1)
        {
            return result[0];
        }

        return expression;
    }

    public bool HasNonEmptySourceExpression()
    {
        for (int i = 0; i < MapInfos.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(MapInfos[i].SourceExpression))
            {
                return true;
            }
        }

        return false;
    }

    #region ICloneable Members
    object ICloneable.Clone()
    {
        return Clone();
    }

    public MapStepSettings Clone()
    {
        return (MapStepSettings)MemberwiseClone();
    }
    #endregion
}

public class MapInfo
{
    public string PropertyName { get; set; } = "";

    public string PropertyDescription { get; set; } = "";

    public string PropertyType { get; set; }

    /// <summary>
    /// The SQL string to be used to retrieve data for this Property from the ERP.  
    /// </summary>
    public string SourceExpression { get; set; } = "";

    /// <summary>
    /// Whether the user must specify a Source Expression for this value.
    /// </summary>

    public bool Required { get; set; }
}

public class MapInfoArray
{
    public List<MapInfo> MapInfos { get; set; } = new ();

    public void RemoveMapInfoForProperty(string a_propertyName)
    {
        MapInfos.RemoveAll(m => m.PropertyName == a_propertyName);
    }

    public void Add(MapInfo mapInfo)
    {
        MapInfos.Add(mapInfo);
    }

    public MapInfo this[int i] => MapInfos?[i];

    public int Count => MapInfos?.Count ?? 0;

    public void Clear()
    {
        MapInfos.Clear();
    }

    public void RemoveAt(int a_i)
    {
        MapInfos.RemoveAt(a_i);
    }
}