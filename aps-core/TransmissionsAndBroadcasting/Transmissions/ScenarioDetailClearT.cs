using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Transmission for expediting a list of Jobs.
/// </summary>
public class ScenarioDetailClearT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 589;

    #region IPTSerializable Members
    public ScenarioDetailClearT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 13011)
        {
            m_bools = new BoolVector32(a_reader);
            m_bools2 = new BoolVector32(a_reader);
        }
        else if (a_reader.VersionNumber >= 1)
        {
            m_bools = new BoolVector32(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_bools2.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailClearT() { }

    public ScenarioDetailClearT(BaseId a_scenarioId)
        : base(a_scenarioId) { }

    #region Bools
    private BoolVector32 m_bools;

    private const int c_jobsIdx = 0;
    private const int c_plantsIdx = 1;
    private const int c_departmentIdx = 2;
    private const int c_resourcesIdx = 3;
    private const int c_capacityIntervalsIdx = 4;
    private const int c_recurringCapacityIntervalsIdx = 5;
    private const int c_balancedCompositeRulesIdx = 6;
    private const int c_cellsIdx = 7;
    private const int c_itemsIdx = 8;
    private const int c_warehousesIdx = 9;
    private const int c_inventoriesIdx = 10;
    private const int c_purchaseToStocksIdx = 11;
    private const int c_capabilitiesIdx = 12;
    private const int c_resourcesWithoutCapabilitiesIdx = 13;
    private const int c_salesOrdersIdx = 14;
    private const int c_forecastsIdx = 15;
    private const int c_transferOrdersIdx = 16;
    private const int c_productRulesIdx = 17;
    //priva constly int c_setupCodeTablesIdx = 18; deprecated
    private const int c_attributeCodeTablesIdx = 19;
    private const int c_attributeNumberRangeTablesIdx = 20;
    private const int c_resourceConnectorsIdx = 21;
    private const int c_allowedHelpersIdx = 22;
    private const int c_generalizeInsteadOfClearIdx = 23;
    private const int c_clearCustomersIdx = 24;
    private const int c_clearCleanOutIdx = 25;
    private const int c_clearAttributesIdx = 26;
    private const int c_compatibilityCodeTableManagerIdx = 27;
    private const int c_storageAreaIdx = 28;
    private const int c_itemStorageIdx = 29;
    private const int c_lotIdx = 30;
    private const int c_storageAreaConnectorsIdx = 31;

    private BoolVector32 m_bools2;

    private const int c_lotStorageIdx = 0;
    private const int c_userUdfsIdx = 1;
    private const int c_plantsUdfsIdx = 2;
    private const int c_departmentsUdfsIdx = 3;
    private const int c_resourcesUdfsIdx = 4;
    private const int c_cellsUdfsIdx = 5;
    private const int c_capacityIntervalsUdfsIdx = 6;
    private const int c_productRulesUdfsIdx = 7;
    private const int c_resourceConnectorsUdfsIdx = 8;
    private const int c_itemsUdfsIdx = 9;
    private const int c_warehousesUdfsIdx = 10;
    private const int c_salesOrdersUdfsIdx = 11;
    private const int c_forecastsUdfsIdx = 12;
    private const int c_purchasesToStockUdfsIdx = 13;
    private const int c_transferOrdersUdfsIdx = 14;
    private const int c_jobsUdfsIdx = 15;
    private const int c_manufacturingOrdersUdfsIdx = 16;
    private const int c_resourceOperationsUdfsIdx = 17;
    private const int c_customersUdfsIdx = 18;
    private const int c_lotsUdfsIdx = 19;
    private const int c_storageAreaUdfsIdx = 21;
    private const int c_itemStorageUdfsIdx = 22;
    private const int c_storageAreaConnectorsUdfsIdx = 23;

    public bool ClearJobs
    {
        get => m_bools[c_jobsIdx];
        set => m_bools[c_jobsIdx] = value;
    }

    public bool ClearPlants
    {
        get => m_bools[c_plantsIdx];
        set => m_bools[c_plantsIdx] = value;
    }

    public bool ClearDepartments
    {
        get => m_bools[c_departmentIdx];
        set => m_bools[c_departmentIdx] = value;
    }

    public bool ClearResources
    {
        get => m_bools[c_resourcesIdx];
        set => m_bools[c_resourcesIdx] = value;
    }

    public bool ClearCapabilities
    {
        get => m_bools[c_capabilitiesIdx];
        set => m_bools[c_capabilitiesIdx] = value;
    }

    public bool ClearCapacityIntervals
    {
        get => m_bools[c_capacityIntervalsIdx];
        set => m_bools[c_capacityIntervalsIdx] = value;
    }

    public bool ClearRecurringCapacityIntervals
    {
        get => m_bools[c_recurringCapacityIntervalsIdx];
        set => m_bools[c_recurringCapacityIntervalsIdx] = value;
    }

    public bool ClearBalancedCompositeRules
    {
        get => m_bools[c_balancedCompositeRulesIdx];
        set => m_bools[c_balancedCompositeRulesIdx] = value;
    }

    public bool ClearCells
    {
        get => m_bools[c_cellsIdx];
        set => m_bools[c_cellsIdx] = value;
    }

    public bool ClearItems
    {
        get => m_bools[c_itemsIdx];
        set => m_bools[c_itemsIdx] = value;
    }

    public bool ClearWarehouses
    {
        get => m_bools[c_warehousesIdx];
        set => m_bools[c_warehousesIdx] = value;
    }

    public bool ClearInventories
    {
        get => m_bools[c_inventoriesIdx];
        set => m_bools[c_inventoriesIdx] = value;
    }

    public bool ClearPurchaseToStocks
    {
        get => m_bools[c_purchaseToStocksIdx];
        set => m_bools[c_purchaseToStocksIdx] = value;
    }

    public bool ClearResourcesWithoutCapabilities
    {
        get => m_bools[c_resourcesWithoutCapabilitiesIdx];
        set => m_bools[c_resourcesWithoutCapabilitiesIdx] = value;
    }

    public bool ClearSalesOrders
    {
        get => m_bools[c_salesOrdersIdx];
        set => m_bools[c_salesOrdersIdx] = value;
    }

    public bool ClearForecasts
    {
        get => m_bools[c_forecastsIdx];
        set => m_bools[c_forecastsIdx] = value;
    }

    public bool ClearTransferOrders
    {
        get => m_bools[c_transferOrdersIdx];
        set => m_bools[c_transferOrdersIdx] = value;
    }

    public bool ClearProductRules
    {
        get => m_bools[c_productRulesIdx];
        set => m_bools[c_productRulesIdx] = value;
    }

    public bool ClearAttributeCodeTables
    {
        get => m_bools[c_attributeCodeTablesIdx];
        set => m_bools[c_attributeCodeTablesIdx] = value;
    }

    public bool ClearAttributeNumberRangeTables
    {
        get => m_bools[c_attributeNumberRangeTablesIdx];
        set => m_bools[c_attributeNumberRangeTablesIdx] = value;
    }

    public bool ClearResourceConnectors
    {
        get => m_bools[c_resourceConnectorsIdx];
        set => m_bools[c_resourceConnectorsIdx] = value;
    }

    public bool ClearAllowedHelpers
    {
        get => m_bools[c_allowedHelpersIdx];
        set => m_bools[c_allowedHelpersIdx] = value;
    }

    public bool GeneralizeDataInsteadOfClear
    {
        get => m_bools[c_generalizeInsteadOfClearIdx];
        set => m_bools[c_generalizeInsteadOfClearIdx] = value;
    }

    public bool ClearCustomers
    {
        get => m_bools[c_clearCustomersIdx];
        set => m_bools[c_clearCustomersIdx] = value;
    }
    public bool ClearAttributes
    {
        get => m_bools[c_clearAttributesIdx];
        set => m_bools[c_clearAttributesIdx] = value;
    }
    public bool ClearCleanOutTables
    {
        get => m_bools[c_clearCleanOutIdx];
        set => m_bools[c_clearCleanOutIdx] = value;
    }

    public bool ClearCompatibilityCodeTables
    {
        get => m_bools[c_compatibilityCodeTableManagerIdx];
        set => m_bools[c_compatibilityCodeTableManagerIdx] = value;
    }

    public bool ClearStorageAreas
    {
        get => m_bools[c_storageAreaIdx];
        set => m_bools[c_storageAreaIdx] = value;
    }

    public bool ClearStorageAreaConnectors
    {
        get => m_bools[c_storageAreaConnectorsIdx];
        set => m_bools[c_storageAreaConnectorsIdx] = value;
    }

    public bool ClearItemStorages
    {
        get => m_bools[c_itemStorageIdx];
        set => m_bools[c_itemStorageIdx] = value;
    }

    public bool ClearLots
    {
        get => m_bools[c_lotIdx];
        set => m_bools[c_lotIdx] = value;
    }

    public bool ClearUserUdfs
    {
        get => m_bools2[c_userUdfsIdx];
        set => m_bools2[c_userUdfsIdx] = value;
    }

    public bool ClearPlantUdfs
    {
        get => m_bools2[c_plantsUdfsIdx];
        set => m_bools2[c_plantsUdfsIdx] = value;
    }

    public bool ClearDepartmentUdfs
    {
        get => m_bools2[c_departmentsUdfsIdx];
        set => m_bools2[c_departmentsUdfsIdx] = value;
    }

    public bool ClearResourceUdfs
    {
        get => m_bools2[c_resourcesUdfsIdx];
        set => m_bools2[c_resourcesUdfsIdx] = value;
    }

    public bool ClearCellUdfs
    {
        get => m_bools2[c_cellsUdfsIdx];
        set => m_bools2[c_cellsUdfsIdx] = value;
    }

    public bool ClearCapacityIntervalUdfs
    {
        get => m_bools2[c_capacityIntervalsUdfsIdx];
        set => m_bools2[c_capacityIntervalsUdfsIdx] = value;
    }

    public bool ClearProductRulesUdfs
    {
        get => m_bools2[c_productRulesUdfsIdx];
        set => m_bools2[c_productRulesUdfsIdx] = value;
    }

    public bool ClearResourceConnectorUdfs
    {
        get => m_bools2[c_resourceConnectorsUdfsIdx];
        set => m_bools2[c_resourceConnectorsUdfsIdx] = value;
    }

    public bool ClearItemUdfs
    {
        get => m_bools2[c_itemsUdfsIdx];
        set => m_bools2[c_itemsUdfsIdx] = value;
    }

    public bool ClearWarehouseUdfs
    {
        get => m_bools2[c_warehousesUdfsIdx];
        set => m_bools2[c_warehousesUdfsIdx] = value;
    }

    public bool ClearSalesOrderUdfs
    {
        get => m_bools2[c_salesOrdersUdfsIdx];
        set => m_bools2[c_salesOrdersUdfsIdx] = value;
    }

    public bool ClearForecastUdfs
    {
        get => m_bools2[c_forecastsUdfsIdx];
        set => m_bools2[c_forecastsUdfsIdx] = value;
    }

    public bool ClearPurchasesToStockUdfs
    {
        get => m_bools2[c_purchasesToStockUdfsIdx];
        set => m_bools2[c_purchasesToStockUdfsIdx] = value;
    }

    public bool ClearTransferOrderUdfs
    {
        get => m_bools2[c_transferOrdersUdfsIdx];
        set => m_bools2[c_transferOrdersUdfsIdx] = value;
    }

    public bool ClearJobUdfs
    {
        get => m_bools2[c_jobsUdfsIdx];
        set => m_bools2[c_jobsUdfsIdx] = value;
    }

    public bool ClearManufacturingOrderUdfs
    {
        get => m_bools2[c_manufacturingOrdersUdfsIdx];
        set => m_bools2[c_manufacturingOrdersUdfsIdx] = value;
    }

    public bool ClearResourceOperationUdfs
    {
        get => m_bools2[c_resourceOperationsUdfsIdx];
        set => m_bools2[c_resourceOperationsUdfsIdx] = value;
    }

    public bool ClearCustomerUdfs
    {
        get => m_bools2[c_customersUdfsIdx];
        set => m_bools2[c_customersUdfsIdx] = value;
    }

    public bool ClearLotsUdfs
    {
        get => m_bools2[c_lotsUdfsIdx];
        set => m_bools2[c_lotsUdfsIdx] = value;
    }

    public bool ClearStorageAreaUdfs
    {
        get => m_bools2[c_storageAreaUdfsIdx];
        set => m_bools2[c_storageAreaUdfsIdx] = value;
    }

    public bool ClearItemStorageUdfs
    {
        get => m_bools2[c_itemStorageUdfsIdx];
        set => m_bools2[c_itemStorageUdfsIdx] = value;
    }

    public bool ClearStorageAreaConnectorUdfs
    {
        get => m_bools2[c_storageAreaConnectorsUdfsIdx];
        set => m_bools2[c_storageAreaConnectorsUdfsIdx] = value;
    }

    /// <summary>
    /// Returns true if any of the udf objects are selected to be deleted.
    /// </summary>
    public bool ClearUserFields
    {
        get
        {
            return m_bools2.AnyFlagSetInRange(c_userUdfsIdx, c_storageAreaConnectorsUdfsIdx);
        }
    }

    //don't need this for now
    //public bool ClearLotStorages
    //{
    //    get => m_bools2[c_lotStorageIdx];
    //    set => m_bools2[c_lotStorageIdx] = value;
    //}
    #endregion Bools

    public override string Description => "Scenario Data cleared";
}