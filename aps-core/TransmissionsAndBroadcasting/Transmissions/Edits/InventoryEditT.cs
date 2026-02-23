using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;

using static PT.SchedulerDefinitions.ItemDefs;

namespace PT.Transmissions;

public class InventoryEditT : ScenarioIdBaseT, IPTSerializable
{
    #region PT Serialization
    public static int UNIQUE_ID => 1051;

    public InventoryEditT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                InventoryEdit node = new (a_reader);
                m_inventoryEdits.Add(node);
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                ItemEdit node = new (a_reader);
                m_itemEdits.Add(node);
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                WarehouseEdit node = new (a_reader);
                m_warehouseEdits.Add(node);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_inventoryEdits);
        writer.Write(m_itemEdits);
        writer.Write(m_warehouseEdits);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public InventoryEditT() { }
    public InventoryEditT(BaseId a_scenarioId) : base(a_scenarioId) { }

    public void Validate() { }

    public override string Description => string.Format("Warehouses updated ({0}); Inventories updated ({1}); Items updated ({2})".Localize(), m_warehouseEdits.Count, m_inventoryEdits.Count, m_itemEdits.Count);

    private readonly List<InventoryEdit> m_inventoryEdits = new ();
    public List<InventoryEdit> InventoryEdits => m_inventoryEdits;

    private readonly List<ItemEdit> m_itemEdits = new ();
    public List<ItemEdit> ItemEdits => m_itemEdits;

    private readonly List<WarehouseEdit> m_warehouseEdits = new ();
    public List<WarehouseEdit> WarehouseEdits => m_warehouseEdits;
}

public class InventoryEdit : PTObjectBaseEdit, IPTSerializable
{
    public BaseId InventoryId;
    public BaseId ItemId;
    public BaseId WarehouseId;

    #region PT Serialization
    public InventoryEdit(IReader a_reader)
        : base(a_reader)
    {
        #region 12410

        if (a_reader.VersionNumber >= 12547)
        {
            WarehouseId = new BaseId(a_reader);
            InventoryId = new BaseId(a_reader);
            ItemId = new BaseId(a_reader);
            PurchaseOrderSupplyStorageAreaId = new BaseId(a_reader);

            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out m_leadTime);
            a_reader.Read(out m_safetyStock);
            a_reader.Read(out m_safetyStockWarningLevel);
            a_reader.Read(out m_maxInventory);
            a_reader.Read(out m_plannerExternalId);
            a_reader.Read(out m_onHandQty);
            a_reader.Read(out m_safetyStockJobPriority);
            a_reader.Read(out int value);
            m_mrpProcessing = (InventoryDefs.MrpProcessing)value;
            a_reader.Read(out value);
            m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)value;
            a_reader.Read(out m_templateJobExternalId);
            a_reader.Read(out m_replenishmentMin);
            a_reader.Read(out m_replenishmentMax);
            a_reader.Read(out m_numberOfIntervalsToForecast);
            a_reader.Read(out value);
            m_forecastInterval = (DateInterval.EInterval)value;
            a_reader.Read(out m_shippingBuffer);
            a_reader.Read(out m_receivingBuffer);
            a_reader.Read(out m_bufferStock);


            a_reader.Read(out short maValue);
            m_materialAllocation = (ItemDefs.MaterialAllocation)maValue;
        }

        #endregion
        #region 12410

        else if (a_reader.VersionNumber >= 12410)
        {
            WarehouseId = new BaseId(a_reader);
            InventoryId = new BaseId(a_reader);
            ItemId = new BaseId(a_reader);

            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out m_leadTime);
            a_reader.Read(out m_safetyStock);
            a_reader.Read(out m_safetyStockWarningLevel);
            a_reader.Read(out m_maxInventory);
            a_reader.Read(out m_plannerExternalId);
            a_reader.Read(out m_onHandQty);
            a_reader.Read(out m_safetyStockJobPriority);
            a_reader.Read(out int value);
            m_mrpProcessing = (InventoryDefs.MrpProcessing)value;
            a_reader.Read(out value);
            m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)value;
            a_reader.Read(out m_templateJobExternalId);
            a_reader.Read(out m_replenishmentMin);
            a_reader.Read(out m_replenishmentMax);
            a_reader.Read(out m_numberOfIntervalsToForecast);
            a_reader.Read(out value);
            m_forecastInterval = (DateInterval.EInterval)value;
            a_reader.Read(out m_shippingBuffer);
            a_reader.Read(out m_receivingBuffer);
            a_reader.Read(out m_bufferStock);


            a_reader.Read(out short maValue);
            m_materialAllocation = (ItemDefs.MaterialAllocation)maValue;
        }

        #endregion

        #region 12000

        else if (a_reader.VersionNumber >= 12000)
        {
            WarehouseId = new BaseId(a_reader);
            InventoryId = new BaseId(a_reader);
            ItemId = new BaseId(a_reader);

            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out m_leadTime);
            a_reader.Read(out m_safetyStock);
            a_reader.Read(out m_safetyStockWarningLevel);
            a_reader.Read(out m_maxInventory);
            a_reader.Read(out m_plannerExternalId);
            a_reader.Read(out m_onHandQty);
            a_reader.Read(out m_safetyStockJobPriority);
            a_reader.Read(out int value);
            m_mrpProcessing = (InventoryDefs.MrpProcessing)value;
            a_reader.Read(out value);
            m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)value;
            a_reader.Read(out m_templateJobExternalId);
            a_reader.Read(out m_replenishmentMin);
            a_reader.Read(out m_replenishmentMax);
            a_reader.Read(out m_numberOfIntervalsToForecast);
            a_reader.Read(out value);
            m_forecastInterval = (DateInterval.EInterval)value;
            a_reader.Read(out m_shippingBuffer);
            a_reader.Read(out m_receivingBuffer);
            a_reader.Read(out m_bufferStock);
        }

        #endregion
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        WarehouseId.Serialize(a_writer);
        InventoryId.Serialize(a_writer);
        ItemId.Serialize(a_writer);
        PurchaseOrderSupplyStorageAreaId.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_setBools.Serialize(a_writer);

        a_writer.Write(m_leadTime);
        a_writer.Write(m_safetyStock);
        a_writer.Write(m_safetyStockWarningLevel);
        a_writer.Write(m_maxInventory);
        a_writer.Write(m_plannerExternalId);
        a_writer.Write(m_onHandQty);
        a_writer.Write(m_safetyStockJobPriority);
        a_writer.Write((int)m_mrpProcessing);
        a_writer.Write((int)m_mrpExcessQuantityAllocation);
        a_writer.Write(m_templateJobExternalId);
        a_writer.Write(m_replenishmentMin);
        a_writer.Write(m_replenishmentMax);
        a_writer.Write(m_numberOfIntervalsToForecast);
        a_writer.Write((int)m_forecastInterval);
        a_writer.Write(m_shippingBuffer);
        a_writer.Write(m_receivingBuffer);
        a_writer.Write(m_bufferStock);
        a_writer.Write((short)m_materialAllocation);
    }

    public override bool HasEdits => base.HasEdits || m_setBools.AnyFlagsSet;

    public new int UniqueId => 1052;
    #endregion

    public InventoryEdit(BaseId a_warehouseId, BaseId a_inventoryId, BaseId a_itemId)
    {
        InventoryId = a_inventoryId;
        WarehouseId = a_warehouseId;
        ItemId = a_itemId;
        m_externalId = null;
    }

    #region Shared Properties
    private BoolVector32 m_bools;
    private const short c_autoGenerateForecastsIdx = 0;
    private const short c_preventSharedBatchOverflowIdx = 1;

    private BaseId m_purchaseOrderSupplyStorageAreaId;
    public BaseId PurchaseOrderSupplyStorageAreaId
    {
        get => m_purchaseOrderSupplyStorageAreaId;
        set
        {
            m_purchaseOrderSupplyStorageAreaId = value;
            m_setBools[c_poSupplyStorageAreaIdSetIdx] = true;
        }
    }

    private TimeSpan m_leadTime;

    public TimeSpan LeadTime
    {
        get => m_leadTime;
        set
        {
            m_leadTime = value;
            m_setBools[c_leadTimeSetIdx] = true;
        }
    }

    private decimal m_safetyStock;

    public decimal SafetyStock
    {
        get => m_safetyStock;
        set
        {
            m_safetyStock = value;
            m_setBools[c_safetyStockSetIdx] = true;
        }
    }

    private decimal m_safetyStockWarningLevel;

    public decimal SafetyStockWarningLevel
    {
        get => m_safetyStockWarningLevel;
        set
        {
            m_safetyStockWarningLevel = value;
            m_setBools[c_safetyStockWarningLevelSetIdx] = true;
        }
    }

    private decimal m_maxInventory;

    public decimal MaxInventory
    {
        get => m_maxInventory;
        set
        {
            m_maxInventory = value;
            m_setBools[c_maxInventorySetIdx] = true;
        }
    }

    private string m_plannerExternalId;

    public string PlannerExternalId
    {
        get => m_plannerExternalId;
        set
        {
            m_plannerExternalId = value;
            m_setBools[c_plannerExternalIdSetIdx] = true;
        }
    }

    private decimal m_onHandQty;

    public decimal OnHandQty
    {
        get => m_onHandQty;
        set
        {
            m_onHandQty = value;
            m_setBools[c_onHandQtySetIdx] = true;
        }
    }

    private int m_safetyStockJobPriority;

    public int SafetyStockJobPriority
    {
        get => m_safetyStockJobPriority;
        set
        {
            m_safetyStockJobPriority = value;
            m_setBools[c_safetyStockJobPrioritySetIdx] = true;
        }
    }

    private InventoryDefs.MrpProcessing m_mrpProcessing;

    public InventoryDefs.MrpProcessing MrpProcessing
    {
        get => m_mrpProcessing;
        set
        {
            m_mrpProcessing = value;
            m_setBools[c_mrpProcessingSetIdx] = true;
        }
    }

    private InventoryDefs.MrpExcessQuantityAllocation m_mrpExcessQuantityAllocation;

    public InventoryDefs.MrpExcessQuantityAllocation MrpExcessQuantityAllocation
    {
        get => m_mrpExcessQuantityAllocation;
        set
        {
            m_mrpExcessQuantityAllocation = value;
            m_setBools[c_mrpExcessQuantityAllocationSetIdx] = true;
        }
    }

    private string m_templateJobExternalId;

    public string TemplateJobExternalId
    {
        get => m_templateJobExternalId;
        set
        {
            m_templateJobExternalId = value;
            m_setBools[c_templateExternalIdSetIdx] = true;
        }
    }

    private decimal m_replenishmentMin;

    public decimal ReplenishmentMin
    {
        get => m_replenishmentMin;
        set
        {
            m_replenishmentMin = value;
            m_setBools[c_replenishmentMinSetIdx] = true;
        }
    }

    private decimal m_replenishmentMax;

    public decimal ReplenishmentMax
    {
        get => m_replenishmentMax;
        set
        {
            m_replenishmentMax = value;
            m_setBools[c_replenishmentMaxSetIdx] = true;
        }
    }

    private double m_replenishmentContractDays;

    public double ReplenishmentContractDays
    {
        get => m_replenishmentContractDays;
        set
        {
            m_replenishmentContractDays = value;
            m_setBools[c_replenishmentContractDaysSetIdx] = true;
        }
    }

    private int m_numberOfIntervalsToForecast;

    public int NumberOfIntervalsToForecast
    {
        get => m_numberOfIntervalsToForecast;
        set
        {
            m_numberOfIntervalsToForecast = value;
            m_setBools[c_numberOfIntervalsToForecastSetIdx] = true;
        }
    }

    private DateInterval.EInterval m_forecastInterval;

    public DateInterval.EInterval ForecastInterval
    {
        get => m_forecastInterval;
        set
        {
            m_forecastInterval = value;
            m_setBools[c_forecastIntervalSetIdx] = true;
        }
    }

    public bool AutoGenerateForecasts
    {
        get => m_bools[c_autoGenerateForecastsIdx];
        set
        {
            m_bools[c_autoGenerateForecastsIdx] = value;
            m_setBools[c_autoGenerateForecastsSetIdx] = true;
        }
    }

    private TimeSpan m_shippingBuffer;

    public TimeSpan ShippingBuffer
    {
        get => m_shippingBuffer;
        set
        {
            m_shippingBuffer = value;
            m_setBools[c_shippingBufferSetIdx] = true;
        }
    }

    private TimeSpan m_receivingBuffer;

    public TimeSpan ReceivingBuffer
    {
        get => m_receivingBuffer;
        set
        {
            m_receivingBuffer = value;
            m_setBools[c_receivingBufferSetIdx] = true;
        }
    }

    private decimal m_bufferStock;


    public decimal BufferStock
    {
        get => m_bufferStock;
        set
        {
            m_bufferStock = value;
            m_setBools[c_bufferStockSetIdx] = true;
        }
    }

    public bool PreventSharedBatchOverflow
    {
        get => m_bools[c_preventSharedBatchOverflowIdx];
        set
        {
            m_bools[c_preventSharedBatchOverflowIdx] = value;
            m_setBools[c_preventSharedBatchOverflowSetIdx] = true;
        }
    }

    private MaterialAllocation m_materialAllocation;
    public MaterialAllocation MaterialAllocation
    {
        get => m_materialAllocation;
        set
        {
            m_materialAllocation = value;
            m_setBools[c_materialAllocationSetIdx] = true;
        }
    }
    #endregion

    #region Set Bools
    private BoolVector32 m_setBools;
    private const short c_leadTimeSetIdx = 0;
    private const short c_safetyStockSetIdx = 1;
    private const short c_safetyStockWarningLevelSetIdx = 2;
    private const short c_maxInventorySetIdx = 3;
    private const short c_plannerExternalIdSetIdx = 4;
    private const short c_onHandQtySetIdx = 5;
    private const short c_safetyStockJobPrioritySetIdx = 6;
    private const short c_mrpProcessingSetIdx = 7;
    private const short c_mrpExcessQuantityAllocationSetIdx = 8;
    private const short c_templateExternalIdSetIdx = 9;
    private const short c_replenishmentMinSetIdx = 10;
    private const short c_replenishmentMaxSetIdx = 11;
    private const short c_replenishmentContractDaysSetIdx = 12;
    private const short c_numberOfIntervalsToForecastSetIdx = 13;
    private const short c_forecastIntervalSetIdx = 14;
    private const short c_autoGenerateForecastsSetIdx = 15;
    private const short c_shippingBufferSetIdx = 16;
    private const short c_receivingBufferSetIdx = 17;
    private const short c_bufferStockSetIdx = 18;
    private const short c_preventSharedBatchOverflowSetIdx = 19;
    private const short c_materialAllocationSetIdx = 20;
    private const short c_poSupplyStorageAreaIdSetIdx = 21;

    public bool LeadTimeSet => m_setBools[c_leadTimeSetIdx];
    public bool SafetyStockSet => m_setBools[c_safetyStockSetIdx];
    public bool SafetyStockWarningLevelSet => m_setBools[c_safetyStockWarningLevelSetIdx];
    public bool MaxInventorySet => m_setBools[c_maxInventorySetIdx];
    public bool PlannerExternalIdSet => m_setBools[c_plannerExternalIdSetIdx];
    public bool OnHandQtySet => m_setBools[c_onHandQtySetIdx];
    public bool SafetyStockJobPrioritySet => m_setBools[c_safetyStockJobPrioritySetIdx];
    public bool MrpProcessingSet => m_setBools[c_mrpProcessingSetIdx];
    public bool MrpExcessQuantityAllocationSet => m_setBools[c_mrpExcessQuantityAllocationSetIdx];
    public bool TemplateJobExternalIdSet => m_setBools[c_templateExternalIdSetIdx];
    public bool ReplenishmentMinSet => m_setBools[c_replenishmentMinSetIdx];
    public bool ReplenishmentMaxSet => m_setBools[c_replenishmentMaxSetIdx];
    public bool ReplenishmentContractDaysSet => m_setBools[c_replenishmentContractDaysSetIdx];
    public bool NumberOfIntervalsToForecastSet => m_setBools[c_numberOfIntervalsToForecastSetIdx];
    public bool ForecastIntervalSet => m_setBools[c_forecastIntervalSetIdx];
    public bool AutoGenerateForecastsSet => m_setBools[c_autoGenerateForecastsSetIdx];
    public bool ShippingBufferSet => m_setBools[c_shippingBufferSetIdx];
    public bool ReceivingBufferSet => m_setBools[c_receivingBufferSetIdx];
    public bool BufferStockSet => m_setBools[c_bufferStockSetIdx];
    public bool PreventSharedBatchOverflowSet => m_bools[c_preventSharedBatchOverflowSetIdx];
    public bool MaterialAllocationSet => m_setBools[c_materialAllocationSetIdx];
    public bool PurchaseOrderSupplyStorageAreaSet => m_setBools[c_poSupplyStorageAreaIdSetIdx];
    #endregion
}

public class WarehouseEdit : PTObjectBaseEdit, IPTSerializable
{
    public BaseId InventoryId;
    public BaseId ItemId;
    public BaseId WarehouseId;

    #region PT Serialization
    public WarehouseEdit(IReader a_reader)
        : base(a_reader)
    {
        #region 12000
        if (a_reader.VersionNumber >= 12507)
        {
            WarehouseId = new BaseId(a_reader);
            InventoryId = new BaseId(a_reader);
            ItemId = new BaseId(a_reader);

            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out m_storageCapacity);
            a_reader.Read(out m_annualPercentageRate);
            a_reader.Read(out m_tankWarehouse);
        }
        #endregion
        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            WarehouseId = new BaseId(a_reader);
            InventoryId = new BaseId(a_reader);
            ItemId = new BaseId(a_reader);

            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out int nbrDockObsolete);
            a_reader.Read(out m_storageCapacity);
            a_reader.Read(out m_annualPercentageRate);
            a_reader.Read(out m_tankWarehouse);
        }
        #endregion
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        WarehouseId.Serialize(a_writer);
        InventoryId.Serialize(a_writer);
        ItemId.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_setBools.Serialize(a_writer);

        a_writer.Write(m_storageCapacity);
        a_writer.Write(m_annualPercentageRate);
        a_writer.Write(m_tankWarehouse);
    }

    public override bool HasEdits => base.HasEdits || m_setBools.AnyFlagsSet;

    public new int UniqueId => 1051;
    #endregion

    public WarehouseEdit(BaseId a_warehouseId, BaseId a_inventoryId, BaseId a_itemId)
    {
        InventoryId = a_inventoryId;
        WarehouseId = a_warehouseId;
        ItemId = a_itemId;
        m_externalId = null;
    }

    #region Shared Properties
    private BoolVector32 m_bools;
    
    private decimal m_storageCapacity;

    public decimal StorageCapacity
    {
        get => m_storageCapacity;
        set
        {
            m_storageCapacity = value;
            m_setBools[c_storageCapacitySetIdx] = true;
        }
    }

    private decimal m_annualPercentageRate;

    public decimal AnnualPercentageRate
    {
        get => m_annualPercentageRate;
        set
        {
            m_annualPercentageRate = value;
            m_setBools[c_annualPercentageRateSetIdx] = true;
        }
    }

    private bool m_tankWarehouse;

    public bool TankWarehouse
    {
        get => m_tankWarehouse;
        set
        {
            m_tankWarehouse = value;
            m_setBools[c_tankWarehouseSetIdx] = true;
        }
    }
    #endregion

    #region Set bools
    private BoolVector32 m_setBools;
    private const short c_nbrOfDocksSetIdx = 0; // Remove NbrOfDocks 
    private const short c_storageCapacitySetIdx = 1;
    private const short c_annualPercentageRateSetIdx = 2;
    private const short c_tankWarehouseSetIdx = 3;

    public bool StorageCapacitySet => m_setBools[c_storageCapacitySetIdx];
    public bool AnnualPercentageRateSet => m_setBools[c_annualPercentageRateSetIdx];
    public bool TankWarehouseSet => m_setBools[c_tankWarehouseSetIdx];
    #endregion
}

public class ItemEdit : PTObjectBaseEdit, IPTSerializable
{
    public BaseId InventoryId;
    public BaseId ItemId;
    public BaseId WarehouseId;

    #region PT Serialization
    public ItemEdit(IReader a_reader)
        : base(a_reader)
    {
        #region 12511
        if (a_reader.VersionNumber >= 12511)
        {
            WarehouseId = new BaseId(a_reader);
            InventoryId = new BaseId(a_reader);
            ItemId = new BaseId(a_reader);

            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out int value);
            m_source = (sources)value;
            a_reader.Read(out value);
            m_itemType = (itemTypes)value;
            a_reader.Read(out m_itemGroup);
            a_reader.Read(out m_batchSize);
            a_reader.Read(out m_maxOrderQty);
            a_reader.Read(out m_minOrderQty);
            a_reader.Read(out m_batchWindow);
            a_reader.Read(out m_minOrderQtyRoundupLimit);
            a_reader.Read(out m_jobAutoSplitQty);
            a_reader.Read(out m_shelfLife);
            a_reader.Read(out m_defaultLeadTime);
            a_reader.Read(out m_transferQty);
            a_reader.Read(out m_cost);

            a_reader.Read(out m_unitVolume);
            a_reader.Read(out m_shelfLifeWarningHrs);
        }
        #endregion        
        #region 12506
        else if (a_reader.VersionNumber >= 12506)
        {
            WarehouseId = new BaseId(a_reader);
            InventoryId = new BaseId(a_reader);
            ItemId = new BaseId(a_reader);

            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out int value);
            m_source = (sources)value;
            a_reader.Read(out value);
            m_itemType = (itemTypes)value;
            a_reader.Read(out m_itemGroup);
            a_reader.Read(out m_batchSize);
            a_reader.Read(out m_maxOrderQty);
            a_reader.Read(out m_minOrderQty);
            a_reader.Read(out m_batchWindow);
            a_reader.Read(out m_minOrderQtyRoundupLimit);
            a_reader.Read(out m_jobAutoSplitQty);
            a_reader.Read(out int lotUsabilityVal);
            a_reader.Read(out m_shelfLife);
            a_reader.Read(out m_defaultLeadTime);
            a_reader.Read(out m_transferQty);
            a_reader.Read(out m_cost);

            a_reader.Read(out m_unitVolume);
            a_reader.Read(out m_shelfLifeWarningHrs);
        }
        #endregion
        #region 12104
        else if (a_reader.VersionNumber >= 12106)
        {
            WarehouseId = new BaseId(a_reader);
            InventoryId = new BaseId(a_reader);
            ItemId = new BaseId(a_reader);

            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out int value);
            m_source = (sources)value;
            a_reader.Read(out value);
            m_itemType = (itemTypes)value;
            a_reader.Read(out m_itemGroup);
            a_reader.Read(out m_batchSize);
            a_reader.Read(out m_maxOrderQty);
            a_reader.Read(out m_minOrderQty);
            a_reader.Read(out m_batchWindow);
            a_reader.Read(out m_minOrderQtyRoundupLimit);
            a_reader.Read(out m_jobAutoSplitQty);
            a_reader.Read(out int lotUsabilityVal);
            a_reader.Read(out m_shelfLife);
            a_reader.Read(out m_defaultLeadTime);
            a_reader.Read(out m_transferQty);
            a_reader.Read(out m_cost);

            a_reader.Read(out m_unitVolume);
        }
        #endregion

        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            WarehouseId = new BaseId(a_reader);
            InventoryId = new BaseId(a_reader);
            ItemId = new BaseId(a_reader);

            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out int value);
            m_source = (sources)value;
            a_reader.Read(out value);
            m_itemType = (itemTypes)value;
            a_reader.Read(out m_itemGroup);
            a_reader.Read(out m_batchSize);
            a_reader.Read(out m_maxOrderQty);
            a_reader.Read(out m_minOrderQty);
            a_reader.Read(out m_batchWindow);
            a_reader.Read(out m_minOrderQtyRoundupLimit);
            a_reader.Read(out m_jobAutoSplitQty);
            a_reader.Read(out int lotUsabilityVal);
            a_reader.Read(out m_shelfLife);
            a_reader.Read(out m_defaultLeadTime);
            a_reader.Read(out m_transferQty);
            a_reader.Read(out m_cost);
        }
        #endregion
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        WarehouseId.Serialize(a_writer);
        InventoryId.Serialize(a_writer);
        ItemId.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_setBools.Serialize(a_writer);

        a_writer.Write((int)m_source);
        a_writer.Write((int)m_itemType);
        a_writer.Write(m_itemGroup);
        a_writer.Write(m_batchSize);
        a_writer.Write(m_maxOrderQty);
        a_writer.Write(m_minOrderQty);
        a_writer.Write(m_batchWindow);
        a_writer.Write(m_minOrderQtyRoundupLimit);
        a_writer.Write(m_jobAutoSplitQty);
        a_writer.Write(m_shelfLife);
        a_writer.Write(m_defaultLeadTime);
        a_writer.Write(m_transferQty);
        a_writer.Write(m_cost);

        a_writer.Write(m_unitVolume);
        a_writer.Write(m_shelfLifeWarningHrs);
    }

    public override bool HasEdits => base.HasEdits || m_setBools.AnyFlagsSet;

    public new int UniqueId => 1053;
    #endregion

    public ItemEdit(BaseId a_warehouseId, BaseId a_inventoryId, BaseId a_itemId)
    {
        InventoryId = a_inventoryId;
        WarehouseId = a_warehouseId;
        ItemId = a_itemId;
        m_externalId = null;
    }

    #region Shared Properties
    private BoolVector32 m_bools;
    private const short c_planInventoryIdx = 0;

    private sources m_source;

    public sources Source
    {
        get => m_source;
        set
        {
            m_source = value;
            m_setBools[c_sourceSetIdx] = true;
        }
    }

    private itemTypes m_itemType;

    public itemTypes ItemType
    {
        get => m_itemType;
        set
        {
            m_itemType = value;
            m_setBools[c_itemTypeSetIdx] = true;
        }
    }

    private string m_itemGroup;

    public string ItemGroup
    {
        get => m_itemGroup;
        set
        {
            m_itemGroup = value;
            m_setBools[c_itemGroupSetIdx] = true;
        }
    }

    private decimal m_batchSize;

    public decimal BatchSize
    {
        get => m_batchSize;
        set
        {
            m_batchSize = value;
            m_setBools[c_batchSizeSetIdx] = true;
        }
    }

    private decimal m_maxOrderQty;

    public decimal MaxOrderQty
    {
        get => m_maxOrderQty;
        set
        {
            m_maxOrderQty = value;
            m_setBools[c_maxOrderQtySetIdx] = true;
        }
    }

    private decimal m_minOrderQty;

    public decimal MinOrderQty
    {
        get => m_minOrderQty;
        set
        {
            m_minOrderQty = value;
            m_setBools[c_minOrderQtySetIdx] = true;
        }
    }

    private TimeSpan m_batchWindow;

    public TimeSpan BatchWindow
    {
        get => m_batchWindow;
        set
        {
            m_batchWindow = value;
            m_setBools[c_batchWindowSetIdx] = true;
        }
    }

    private decimal m_minOrderQtyRoundupLimit;

    public decimal MinOrderQtyRoundupLimit
    {
        get => m_minOrderQtyRoundupLimit;
        set
        {
            m_minOrderQtyRoundupLimit = value;
            m_setBools[c_minOrderQtyRoundupLimitSetIdx] = true;
        }
    }

    private decimal m_jobAutoSplitQty;

    public decimal JobAutoSplitQty
    {
        get => m_jobAutoSplitQty;
        set
        {
            m_jobAutoSplitQty = value;
            m_setBools[c_jobAutoSplitQtySetIdx] = true;
        }
    }

    private TimeSpan m_shelfLife;

    public TimeSpan ShelfLife
    {
        get => m_shelfLife;
        set
        {
            m_shelfLife = value;
            m_setBools[c_shelfLifeSetIdx] = true;
        }
    }

    private TimeSpan m_defaultLeadTime;

    public TimeSpan DefaultLeadTime
    {
        get => m_defaultLeadTime;
        set
        {
            m_defaultLeadTime = value;
            m_setBools[c_defaultLeadTimeSetIdx] = true;
        }
    }

    public bool PlanInventory
    {
        get => m_bools[c_planInventoryIdx];
        set
        {
            m_bools[c_planInventoryIdx] = value;
            m_setBools[c_planInventorySetIdx] = true;
        }
    }

    private decimal m_transferQty;

    public decimal TransferQty
    {
        get => m_transferQty;
        set
        {
            m_transferQty = value;
            m_setBools[c_transferQtySetIdx] = true;
        }
    }

    private decimal m_cost;

    public decimal Cost
    {
        get => m_cost;
        set
        {
            m_cost = value;
            m_setBools[c_costIsSetIdx] = true;
        }
    }

    private decimal m_unitVolume;

    public decimal UnitVolume
    {
        get => m_unitVolume;
        set
        {
            m_unitVolume = value;
            m_setBools[c_unitVolumeIsSetIdx] = true;
        }
    }
    
    private double m_shelfLifeWarningHrs;

    public double ShelfLifeWarningHrs
    {
        get => m_shelfLifeWarningHrs;
        set
        {
            m_shelfLifeWarningHrs = value;
            m_setBools[c_shelfLifeWarningHrsIsSetIdx] = true;
        }
    }
    #endregion

    #region Set Bools
    private BoolVector32 m_setBools;
    private const short c_sourceSetIdx = 0;
    private const short c_itemTypeSetIdx = 1;
    private const short c_itemGroupSetIdx = 2;
    private const short c_batchSizeSetIdx = 3;
    private const short c_maxOrderQtySetIdx = 4;
    private const short c_minOrderQtySetIdx = 5;
    private const short c_batchWindowSetIdx = 6;
    private const short c_minOrderQtyRoundupLimitSetIdx = 7;
    private const short c_jobAutoSplitQtySetIdx = 8;
    //private const short c_lotUsabilitySetIdx = 9;
    private const short c_shelfLifeSetIdx = 10;
    private const short c_defaultLeadTimeSetIdx = 11;
    private const short c_planInventorySetIdx = 12;
    private const short c_transferQtySetIdx = 13;
    private const short c_costIsSetIdx = 14;
    private const short c_unitVolumeIsSetIdx = 15;
    private const short c_shelfLifeWarningHrsIsSetIdx = 16;

    public bool SourceSet => m_setBools[c_sourceSetIdx];
    public bool ItemTypeSet => m_setBools[c_itemTypeSetIdx];
    public bool ItemGroupSet => m_setBools[c_itemGroupSetIdx];
    public bool BatchSizeSet => m_setBools[c_batchSizeSetIdx];
    public bool MaxOrderQtySet => m_setBools[c_maxOrderQtySetIdx];
    public bool MinOrderQtySet => m_setBools[c_minOrderQtySetIdx];
    public bool BatchWindowSet => m_setBools[c_batchWindowSetIdx];
    public bool MinOrderQtyRoundupLimitSet => m_setBools[c_minOrderQtyRoundupLimitSetIdx];
    public bool JobAutoSplitQtySet => m_setBools[c_jobAutoSplitQtySetIdx];
    public bool ShelfLifeSet => m_setBools[c_shelfLifeSetIdx];
    public bool DefaultLeadTimeSet => m_setBools[c_defaultLeadTimeSetIdx];
    public bool PlanInventorySet => m_setBools[c_planInventorySetIdx];
    public bool TransferQtySet => m_setBools[c_transferQtySetIdx];
    public bool CostIsSet => m_setBools[c_costIsSetIdx];
    public bool UnitVolumeIsSet => m_setBools[c_unitVolumeIsSetIdx];
    public bool ShelfLifeWarningHrsIsSet => m_setBools[c_shelfLifeWarningHrsIsSetIdx];
    #endregion
}