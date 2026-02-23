using System.ComponentModel;
using System.Diagnostics;

using PT.APSCommon;
using PT.APSCommon.Serialization;
using PT.Database;
using PT.ERPTransmissions;
using PT.Scheduler.Demand;
using PT.Scheduler.Schedule.Storage;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Specifies an Item that will be produced by an Operation.
/// </summary>
public partial class Product : ExternalBaseIdObject, IPTSerializable
{
    // Some testing code used to assign a unique number to products. At the time of this test, this object's
    // Ids weren't unique across the system. 
    private static long s_maxInstanceNbr;
    internal long m_instanceNbr;

    [Conditional("TEST")]
    internal void Init()
    {
        s_maxInstanceNbr++;
        m_instanceNbr = s_maxInstanceNbr;
    }

    public new const int UNIQUE_ID = 535;

    #region IPTSerializable Members
    public Product(IReader a_reader)
        : base(a_reader)                           
    {
        m_restoreInfo = new RestoreInfo();
        if (a_reader.VersionNumber >= 12511)
        {
            m_bools = new BoolVector32(a_reader);
            m_restoreInfo = new RestoreInfo(a_reader);
            a_reader.Read(out m_totalOutputQty);

            a_reader.Read(out int val);
            m_inventoryAvailableTiming = (ProductDefs.EInventoryAvailableTimings)val;

            a_reader.Read(out bool haveDemands);
            if (haveDemands)
            {
                m_demands = new DemandCollection(a_reader);
            }

            a_reader.Read(out m_materialPostProcessingTicks);
            a_reader.Read(out m_lotCode);
            a_reader.Read(out bool hasGeneratedLotIds);
            if (hasGeneratedLotIds)
            {
                BaseIdClassFactory cf = new();
                a_reader.Read(cf, out m_generatedLotIds);
            }

            a_reader.Read(out m_completedQty);
            a_reader.Read(out m_unitVolumeOverride);

            a_reader.Read(out m_wearDurability);
        }
        else if (a_reader.VersionNumber >= 12106)
        {
            m_restoreInfo.ItemId = new BaseId(a_reader);
            a_reader.Read(out m_restoreInfo.HasWarehouse);
            if (m_restoreInfo.HasWarehouse)
            {
                m_restoreInfo.WarehouseId = new BaseId(a_reader);
            }

            //Set the Warehouse and Inventory reference
            a_reader.Read(out m_totalOutputQty);

            a_reader.Read(out int val);
            m_inventoryAvailableTiming = (ProductDefs.EInventoryAvailableTimings)val;

            // [TANK_CODE]
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out bool haveDemands);
            if (haveDemands)
            {
                m_demands = new DemandCollection(a_reader);
            }

            a_reader.Read(out m_materialPostProcessingTicks);
            a_reader.Read(out m_lotCode);
            a_reader.Read(out bool hasGeneratedLotIds);
            if (hasGeneratedLotIds)
            {
                BaseIdClassFactory cf = new ();
                a_reader.Read(cf, out m_generatedLotIds);
            }

            a_reader.Read(out m_completedQty);

            a_reader.Read(out m_unitVolumeOverride);
        }
        else if (a_reader.VersionNumber >= 740)
        {
            m_restoreInfo.ItemId = new BaseId(a_reader);
            a_reader.Read(out m_restoreInfo.HasWarehouse);
            if (m_restoreInfo.HasWarehouse)
            {
                m_restoreInfo.WarehouseId = new BaseId(a_reader);
            }

            //Set the Warehouse and Inventory reference
            a_reader.Read(out m_totalOutputQty);

            a_reader.Read(out int val);
            m_inventoryAvailableTiming = (ProductDefs.EInventoryAvailableTimings)val;

            // [TANK_CODE]
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out bool haveDemands);
            if (haveDemands)
            {
                m_demands = new DemandCollection(a_reader);
            }

            a_reader.Read(out m_materialPostProcessingTicks);
            a_reader.Read(out m_lotCode);
            a_reader.Read(out bool hasGeneratedLotIds);
            if (hasGeneratedLotIds)
            {
                BaseIdClassFactory cf = new ();
                a_reader.Read(cf, out m_generatedLotIds);
            }

            a_reader.Read(out m_completedQty);
        }
    }

    private RestoreInfo m_restoreInfo;

    private class RestoreInfo
    {
        public BaseId ItemId = BaseId.NULL_ID;
        public bool HasWarehouse = true; // Products used to always have Warehouses.
        public BaseId WarehouseId = BaseId.NULL_ID;
        public bool HasStorageArea;
        public BaseId StorageAreaId = BaseId.NULL_ID;
        public BaseId ProducedLotId = BaseId.NULL_ID;

        public RestoreInfo(IReader a_reader)
        {
            ItemId = new BaseId(a_reader);
            a_reader.Read(out HasWarehouse);
            if (HasWarehouse)
            {
                WarehouseId = new BaseId(a_reader);
            }

            if (a_reader.VersionNumber >= 12511)
            {
                a_reader.Read(out HasStorageArea);
                if (HasStorageArea)
                {
                    StorageAreaId = new BaseId(a_reader);
                }
            }

            ProducedLotId = new BaseId(a_reader);
        }

        public RestoreInfo() { }

        public static void Serialize(IWriter a_writer, BaseId a_itemId, Warehouse a_warehouse, StorageArea a_storageArea, Lot a_producedLot)
        {
            a_itemId.Serialize(a_writer);
            a_writer.Write(a_warehouse != null);
            if (a_warehouse != null)
            {
                a_warehouse.Id.Serialize(a_writer);
            }

            a_writer.Write(a_storageArea != null);
            if (a_storageArea != null)
            {
                a_storageArea.Id.Serialize(a_writer);
            }

            if (a_producedLot == null)
            {
                BaseId.NULL_ID.Serialize(a_writer);
            }
            else
            {
                a_producedLot.Id.Serialize(a_writer);
            }
        }
    }

    internal void RestoreReferences(BaseOperationManager a_opManager, WarehouseManager a_warehouses, ItemManager a_items, SalesOrderManager a_salesOrderManager, TransferOrderManager a_transferOrderManager, BaseIdGenerator a_idGen)
    {
        m_item = a_items.GetById(m_restoreInfo.ItemId);
        if (m_restoreInfo.HasWarehouse)
        {
            m_warehouse = a_warehouses.GetById(m_restoreInfo.WarehouseId);
            m_inventory = m_warehouse.Inventories[m_restoreInfo.ItemId];
        }

        if (m_restoreInfo.HasStorageArea)
        {
            if (m_warehouse.StorageAreas.TryGetValue(m_restoreInfo.StorageAreaId, out StorageArea storageArea))
            {
                m_storageArea = storageArea;
            }
        }

        if (Demands != null)
        {
            Demands.RestoreReferences(a_salesOrderManager, Inventory, a_transferOrderManager, a_warehouses);
        }

        if (m_restoreInfo.ProducedLotId != BaseId.NULL_ID)
        {
            m_producedLot = m_inventory.Lots.GetById(m_restoreInfo.ProducedLotId);
            m_producedLot?.RestoreSource(this);
        }

        m_restoreInfo = null;

        m_idGen = a_idGen;
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
        RestoreInfo.Serialize(a_writer, m_item.Id, Warehouse, StorageArea, m_producedLot);

        a_writer.Write(m_totalOutputQty);
        a_writer.Write((int)m_inventoryAvailableTiming);

        a_writer.Write(Demands != null);
        if (Demands != null)
        {
            Demands.Serialize(a_writer);
        }

        a_writer.Write(m_materialPostProcessingTicks);
        a_writer.Write(m_lotCode);
        bool hasGeneratedLotIds = m_generatedLotIds != null;
        a_writer.Write(hasGeneratedLotIds);
        if (hasGeneratedLotIds)
        {
            a_writer.Write(m_lotSerializer, m_generatedLotIds);
        }

        a_writer.Write(m_completedQty);

        a_writer.Write(m_unitVolumeOverride);

        a_writer.Write(m_wearDurability);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    /// <summary>
    /// Specified a quantity of Items to be made for an Operation.
    /// </summary>
    /// <param name="id">Unige identifier within the Operation's Products.</param>
    /// <param name="product">Defines the Product.</param>
    /// <param name="warehouses">List of the Warehouse where the parts will be delivered when complete.</param>
    /// <param name="items">List of all Items in the system.</param>
    public Product(BaseId a_id, JobT.Product a_product, ScenarioDetail a_sd, ItemManager a_items, ScenarioOptions a_so)
        : base(a_product.ExternalId, a_id)
    {
        if (string.IsNullOrEmpty(a_product.ItemExternalId))
        {
            throw new PTValidationException("2932", new object[] { a_product.ExternalId });
        }

        m_item = a_items.GetByExternalId(a_product.ItemExternalId);
        if (m_item == null)
        {
            throw new PTValidationException("2178", new object[] { a_product.ItemExternalId });
        }

        m_warehouse = a_sd.WarehouseManager.GetByExternalId(a_product.WarehouseExternalId);
        if (m_warehouse == null)
        {
            throw new PTValidationException("2179", new object[] { a_product.WarehouseExternalId });
        }

        m_inventory = m_warehouse.Inventories[m_item.Id];
        if (m_inventory == null)
        {
            throw new PTValidationException("2180", new object[] { a_product.ItemExternalId, a_product.WarehouseExternalId });
        }

        if (string.IsNullOrEmpty(a_product.StorageAreaExternalId))
        {
            bool anyCanStore = false;
            foreach (StorageArea storageArea in m_warehouse.StorageAreas)
            {
                if (storageArea.CanStoreItem(m_item.Id))
                {
                    anyCanStore = true;
                    break;
                }
            }
            if (!anyCanStore)
            {
                throw new PTValidationException("3116", new object[] { a_product.ItemExternalId });
            }
        }
        else
        {
            m_storageArea = m_warehouse.StorageAreas.GetByExternalId(a_product.StorageAreaExternalId);
            if (m_storageArea == null)
            {
                throw new PTValidationException("3127", new object[] { a_product.ExternalId, a_product.StorageAreaExternalId });
            }

            if (!m_storageArea.CanStoreItem(m_item.Id))
            {
                throw new PTValidationException("3091", new object[] { m_item.ExternalId, a_product.StorageAreaExternalId });
            }
        }


        TotalOutputQty = a_so.RoundQty(a_product.TotalOutputQty);
        InventoryAvailableTiming = a_product.InventoryAvailableTiming;

        SetWarehouseDuringMRP = a_product.SetWarehouseDuringMRP;
        MaterialPostProcessingTicks = a_product.MaterialPostProcessingTicks;
        FixedQty = a_product.FixedQty;

        LotCode = a_product.LotCode;
        CompletedQty = a_product.CompletedQty;

        if (a_product.UseLimitMatlSrcToEligibleLots)
        {
            UseLimitMatlSrcToEligibleLots = a_product.UseLimitMatlSrcToEligibleLots;
            LimitMatlSrcToEligibleLots = a_product.LimitMatlSrcToEligibleLots;
        }
        else
        {
            //Default
            UseLimitMatlSrcToEligibleLots = false;
            LimitMatlSrcToEligibleLots = true;
        }

        RequireEmptyStorageArea = a_product.RequireEmptyStorageArea;

        UnitVolumeOverride = a_product.UnitVolumeOverride;

        m_idGen = a_sd.IdGen;
        //Init();
    }
    
    public Product(BaseId a_id, Product a_originalProduct)
        : base(a_originalProduct.ExternalId, a_id)
    {
        m_totalOutputQty = a_originalProduct.TotalOutputQty;
        m_completedQty = a_originalProduct.CompletedQty;
        m_inventoryAvailableTiming = a_originalProduct.InventoryAvailableTiming;
        m_item = a_originalProduct.Item;
        m_inventory = a_originalProduct.Inventory;
        m_warehouse = a_originalProduct.Warehouse;
        SetWarehouseDuringMRP = a_originalProduct.SetWarehouseDuringMRP;
        MaterialPostProcessingTicks = a_originalProduct.MaterialPostProcessingTicks;
        FixedQty = a_originalProduct.FixedQty;
        LotCode = a_originalProduct.LotCode;
        m_idGen = a_originalProduct.m_idGen;
        m_unitVolumeOverride = a_originalProduct.m_unitVolumeOverride;
        m_storageArea = a_originalProduct.StorageArea;
        //Init();
    }
    #endregion

    #region Shared Properties
    /// <summary>
    /// If true TotalOutputQty will not change if the Required Quantity of the Manufacturing Order is changed for any reason (MRP, Split, etc).
    /// </summary>
    public bool FixedQty
    {
        get => m_bools[c_fixedQtyIdx];
        internal set => m_bools[c_fixedQtyIdx] = value;
    }

    private decimal m_totalOutputQty;

    /// <summary>
    /// The total amount of the specified Item to be made by the Operation.
    /// </summary>
    public decimal TotalOutputQty
    {
        get => m_totalOutputQty;
        internal set => m_totalOutputQty = value;
    }

    private decimal m_completedQty;

    /// <summary>
    /// The total amount of the specified Item that has already been produced.
    /// </summary>
    public decimal CompletedQty
    {
        get => m_completedQty;
        internal set => m_completedQty = value;
    }

    public bool LimitMatlSrcToEligibleLots
    {
        get => m_bools[c_limitMatlSrcToEligibleLotsIdx];
        internal set => m_bools[c_limitMatlSrcToEligibleLotsIdx] = value;
    }

    public bool UseLimitMatlSrcToEligibleLots
    {
        get => m_bools[c_useLimitMatlSrcToEligibleLotsIdx];
        set => m_bools[c_useLimitMatlSrcToEligibleLotsIdx] = value;
    }

    /// <summary>
    /// The remaining amount of the specified Item that will still be made.
    /// </summary>
    public decimal RemainingOutputQty =>
        //Limit to zero in case of rounding issues.
        Math.Max(0, m_totalOutputQty - m_completedQty);

    private ProductDefs.EInventoryAvailableTimings m_inventoryAvailableTiming = ProductDefs.EInventoryAvailableTimings.AtOperationResourcePostProcessingEnd;

    /// <summary>
    /// Controls when inventory that is created is considered available in stock for use by a consuming Material Requirement.
    /// </summary>
    public ProductDefs.EInventoryAvailableTimings InventoryAvailableTiming
    {
        get => m_inventoryAvailableTiming;
        internal set => m_inventoryAvailableTiming = value;
    }
    
    /// <summary>
    /// If true then during MRP the Warehouse is set to the Warehouse where the demand occurrs (provided that the Item is stocked in the demand's Warehouse).
    /// </summary>
    public bool SetWarehouseDuringMRP
    {
        get => m_bools[SetWarehouseDuringMRPIdx];
        internal set => m_bools[SetWarehouseDuringMRPIdx] = value;
    }

    private long m_materialPostProcessingTicks;

    /// <summary>
    /// This is similar to Operation.MaterialPostProcessing. It's added to the time when the product would become available.
    /// </summary>
    public long MaterialPostProcessingTicks
    {
        get => m_materialPostProcessingTicks;
        set => m_materialPostProcessingTicks = value;
    }

    public TimeSpan MaterialPostProcessingSpan => new (MaterialPostProcessingTicks);
    #endregion Shared Properties

    #region Properties
    private Item m_item;
    public Item Item => m_item;

    private Inventory m_inventory;

    /// <summary>
    /// The Inventory to be updated when the parts are made.
    /// </summary>
    public Inventory Inventory => m_inventory;

    private Warehouse m_warehouse;

    /// <summary>
    /// The Warehouse where the produced Items will be sent.
    /// </summary>
    public Warehouse Warehouse => m_warehouse;

    private StorageArea m_storageArea;
    /// <summary>
    /// If specified, use the specified Storage Area
    /// </summary>
    public StorageArea StorageArea
    {
        get => m_storageArea;
        set => m_storageArea = value;
    }
    
    private decimal m_unitVolumeOverride;

    public decimal UnitVolumeOverride
    {
        get => m_unitVolumeOverride;
        set => m_unitVolumeOverride = value;
    }
    

    /// <summary>
    /// The volume this product will take up during production
    /// This is based on the remaining quantity to produce and the item volume.
    /// This value can be overridden with VolumeOverride
    /// </summary>
    public decimal TotalRequiredVolume
    {
        get
        {
            if (m_unitVolumeOverride != 0m)
            {
                return TotalOutputQty * m_unitVolumeOverride;
            }

            //Calculate based in item and product
            decimal itemVolumePerUnit = Item.UnitVolume;
            return itemVolumePerUnit * TotalOutputQty;
        }
    }

    /// <summary>
    /// The volume this product will take up during production
    /// This is based on the remaining quantity to produce and the item volume.
    /// This value can be overridden with VolumeOverride
    /// </summary>
    public decimal RemainingVolume
    {
        get
        {
            if (m_unitVolumeOverride != 0m)
            {
                return RemainingOutputQty * m_unitVolumeOverride;
            }

            //Calculate based in item and product
            decimal itemVolumePerUnit = Item.UnitVolume;
            return itemVolumePerUnit * RemainingOutputQty;
        }
    }

    public bool RequireEmptyStorageArea
    {
        get => m_bools[c_requireEmptyStorageIdx];
        set => m_bools[c_requireEmptyStorageIdx] = value;
    }
    
    public bool StoreDuringPostProcessing
    {
        get => m_bools[c_storeDuringPostProcessing];
        set => m_bools[c_storeDuringPostProcessing] = value;
    }

    #region Lots
    private string m_lotCode; //Defaulting m_lotCode to null helps to prevent the Update method from overwriting an existing LotCode when importing a NULL value.

    /// <summary>
    /// If the item is lot controlled, this is the value that will be used as the
    /// lot's ExternalId when activity is scheduled and the lot is created.
    /// </summary>
    public string LotCode
    {
        get => m_lotCode;
        private set
        {
            if (value != null)
            {
                m_lotCode = value;
            }
        }
    }

    internal void SetLotCode(string a_code)
    {
        LotCode = a_code;
    }

    protected BaseIdGenerator m_idGen;

    /// <summary>
    /// Dictionary of previously created LotIds. Each LotId is associated with an activity
    /// </summary>
    private Dictionary<BaseId, BaseId> m_generatedLotIds = new ();

    private readonly BaseIdBaseIdSerializer m_lotSerializer = new ();

    //TODO: SA. Why is this here but not used. The activity version is used instead...
    /// <summary>
    /// Returns a BaseId to use to create a Lot. If an activity has created a lot during another simulation,
    /// the same Id will be returned, otherwise a new BaseId will be generated.
    /// </summary>
    /// <returns></returns>
    internal BaseId GetLotIdForActivityIdx(BaseId a_sourceActivityId)
    {
        if (m_generatedLotIds == null)
        {
            m_generatedLotIds = new Dictionary<BaseId, BaseId>();
        }

        if (m_generatedLotIds.TryGetValue(a_sourceActivityId, out BaseId lotId))
        {
            return lotId;
        }

        lotId = m_idGen.NextID();
        m_generatedLotIds.Add(a_sourceActivityId, lotId);
        return lotId;
    }

    /// <summary>
    /// Remove id references to save memory and clear Lot Code
    /// </summary>
    internal void ClearLotData()
    {
        m_generatedLotIds = null;
        SetLotCode(string.Empty);
    }
    #endregion
    #endregion

    #region Update
    internal bool Update(Product a_updatedProduct, bool a_erpUpdate, BaseOperation a_op, ScenarioOptions a_so, IScenarioDataChanges a_dataChanges)
    {
        bool updated = base.Update(a_updatedProduct);
        bool flagConstraintChanges = false;
        bool flagProductionChanges = false;

        //Determine if the Item or Qty changed and therefore requires an update of the Inventory.
        if (TotalOutputQty != a_updatedProduct.TotalOutputQty)
        {
            if (!(a_erpUpdate && a_op.ManufacturingOrder.PreserveRequiredQty))
            {
                if (TotalOutputQty < a_updatedProduct.TotalOutputQty)
                {
                    //Producing more so we need to potentially flag production changes
                    flagProductionChanges = true;
                }
                else
                {
                    //Producing less so we need to potentially flag constraint changes
                    flagConstraintChanges = true;
                }

                TotalOutputQty = a_so.RoundQty(a_updatedProduct.TotalOutputQty);
                FlagItemForNetChangeMRP();
                updated = true;
            }
        }

        if (Item != a_updatedProduct.Item)
        {
            m_item = a_updatedProduct.Item;
            FlagItemForNetChangeMRP();
            flagConstraintChanges = true;
            updated = true;
        }
        
        if (Warehouse != a_updatedProduct.Warehouse)
        {
            m_warehouse = a_updatedProduct.Warehouse;
            FlagItemForNetChangeMRP();
            flagConstraintChanges = true;
            updated = true;
        }
        
        if (Inventory != a_updatedProduct.Inventory)
        {
            m_inventory = a_updatedProduct.Inventory;
            FlagItemForNetChangeMRP();
            flagConstraintChanges = true;
            updated = true;
        }

        if (InventoryAvailableTiming != a_updatedProduct.InventoryAvailableTiming)
        {
            if (InventoryAvailableTiming < a_updatedProduct.InventoryAvailableTiming)
            {
                //Product is available at a later date, flag constraint
                flagConstraintChanges = true;
            }
            else
            {
                //Product available earlier, flag production
                flagProductionChanges = true;
            }

            InventoryAvailableTiming = a_updatedProduct.InventoryAvailableTiming;
            updated = true;
        }

        if (SetWarehouseDuringMRP != a_updatedProduct.SetWarehouseDuringMRP)
        {
            SetWarehouseDuringMRP = a_updatedProduct.SetWarehouseDuringMRP;
            FlagItemForNetChangeMRP();
            updated = true;
        }

        if (WearDurability != a_updatedProduct.WearDurability)
        {
            WearDurability = a_updatedProduct.WearDurability;
            updated = true;
        }

        if (MaterialPostProcessingTicks != a_updatedProduct.MaterialPostProcessingTicks)
        {
            if (MaterialPostProcessingTicks < a_updatedProduct.MaterialPostProcessingTicks)
            {
                //Product is available at a later date, flag constraint
                flagConstraintChanges = true;
            }
            else
            {
                //Product available earlier, flag production
                flagProductionChanges = true;
            }

            MaterialPostProcessingTicks = a_updatedProduct.MaterialPostProcessingTicks;
            updated = true;
        }

        if (FixedQty != a_updatedProduct.FixedQty)
        {
            FixedQty = a_updatedProduct.FixedQty;
            updated = true;
        }
        
        if (LotCode != a_updatedProduct.LotCode)
        {
            if (a_updatedProduct.LotCode == string.Empty)
            {
                //Clear value when setting to empty string. This allows for not clearing during import of null but does allow clearing from the UI.
                m_lotCode = null;
            }
            else if (!string.IsNullOrEmpty(a_updatedProduct.LotCode))
            {
                LotCode = a_updatedProduct.LotCode;
                flagConstraintChanges = true;
            }
        
            updated = true;
        }
        
        if (CompletedQty != a_updatedProduct.CompletedQty)
        {
            if (Item.ItemType == ItemDefs.itemTypes.Tool && CompletedQty != TotalOutputQty)
            {
                //TODO: Localization create new exception code
                throw new PTValidationException("Tool type products cannot be partially completed");
            }

            if (CompletedQty < a_updatedProduct.CompletedQty)
            {
                //More qty has been marked complete, flag constraint
                flagConstraintChanges = true;
            }
            else
            {
                //Qty marked complete has been reduced, flag production
                flagProductionChanges = true;
            }

            CompletedQty = a_updatedProduct.CompletedQty;
            updated = true;
        }

        if (UseLimitMatlSrcToEligibleLots != a_updatedProduct.UseLimitMatlSrcToEligibleLots)
        {
            UseLimitMatlSrcToEligibleLots = a_updatedProduct.UseLimitMatlSrcToEligibleLots;
            updated = true;
        }

        if (UseLimitMatlSrcToEligibleLots)
        {
            if (LimitMatlSrcToEligibleLots != a_updatedProduct.LimitMatlSrcToEligibleLots)
            {
                LimitMatlSrcToEligibleLots = a_updatedProduct.LimitMatlSrcToEligibleLots;
                flagConstraintChanges = true;
                updated = true;
            }
        }
        else
        {
            if (!LimitMatlSrcToEligibleLots)
            {
                LimitMatlSrcToEligibleLots = true;
                flagProductionChanges = true;
                updated = true;
            }
        }

        if (UnitVolumeOverride != a_updatedProduct.UnitVolumeOverride)
        {
            UnitVolumeOverride = a_updatedProduct.UnitVolumeOverride;
            flagConstraintChanges = true;
            updated = true;
        }

        if (StorageArea != a_updatedProduct.StorageArea)
        {
            StorageArea = a_updatedProduct.StorageArea;
            flagConstraintChanges = true;
            updated = true;
        }

        if (RequireEmptyStorageArea != a_updatedProduct.RequireEmptyStorageArea)
        {
            RequireEmptyStorageArea = a_updatedProduct.RequireEmptyStorageArea;

            if (RequireEmptyStorageArea)
            {
                flagConstraintChanges = true;
            }
           
            updated = true;
        }

        if (a_op.Scheduled)
        {
            if (flagProductionChanges)
            {
                a_dataChanges.FlagProductionChanges(a_op.Job.Id);
            }

            if (flagConstraintChanges)
            {
                a_dataChanges.FlagConstraintChanges(a_op.Job.Id);
            }
        }

        return updated;
    }

    internal void FlagItemForNetChangeMRP()
    {
        if (Inventory != null)
        {
            Inventory.IncludeInNetChangeMRP = true;
        }
    }

    /// <summary>
    /// Set a new value for the Warehouse and Inventory.
    /// </summary>
    /// <param name="warehouse"></param>
    internal void SetWarehouseAndInventory(Inventory aNewInventory)
    {
        m_item = aNewInventory.Item;
        m_warehouse = aNewInventory.Warehouse;
        m_inventory = aNewInventory;
    }
    #endregion

    internal void PopulateJobDataSet(ref JobDataSet r_dataSet, BaseOperation a_operation)
    {
        JobDataSet.ProductRow row = r_dataSet.Product.NewProductRow();
        row.JobExternalId = a_operation.ManufacturingOrder.Job.ExternalId;
        row.MoExternalId = a_operation.ManufacturingOrder.ExternalId;
        row.OpExternalId = a_operation.ExternalId;
        row.ExternalId = ExternalId;
        row.Id = Id.ToBaseType();
        row.ItemDescription = Item.Description;
        row.ItemExternalId = Item.ExternalId;
        row.TotalOutputQty = TotalOutputQty;
        row.WarehouseExternalId = Warehouse == null ? "" : Warehouse.ExternalId;
        row.InventoryAvailableTiming = InventoryAvailableTiming.ToString();
        row.SetWarehouseDuringMRP = SetWarehouseDuringMRP;
        row.MaterialPostProcessingHrs = MaterialPostProcessingSpan.TotalHours;
        row.FixedQty = FixedQty;
        row.LotCode = LotCode;
        row.CompletedQty = CompletedQty;
        row.UseLimitMatlSrcToEligibleLots = UseLimitMatlSrcToEligibleLots;
        row.LimitMatlSrcToEligibleLots = LimitMatlSrcToEligibleLots;
        row.UnitVolumeOverride = UnitVolumeOverride;
        row.StorageAreaExternalId = StorageArea?.ExternalId ?? "";
        row.RequireEmptyStorageArea = RequireEmptyStorageArea;
        
        r_dataSet.Product.AddProductRow(row);
    }

    #region PT Database
    public void PtDbPopulate(ref PtDbDataSet r_dataSet, BaseOperation a_op, PtDbDataSet.ManufacturingOrdersRow a_moRow, bool a_publishInventory, PTDatabaseHelper a_dbHelper)
    {
        PtDbDataSet.JobProductsRow jobProductRow = r_dataSet.JobProducts.AddJobProductsRow(
            a_moRow.PublishDate,
            a_moRow.InstanceId,
            a_op.ManufacturingOrder.Job.Id.ToBaseType(),
            a_op.ManufacturingOrder.Id.ToBaseType(),
            a_op.Id.ToBaseType(),
            Id.ToBaseType(),
            TotalOutputQty,
            CompletedQty,
            Item.Id.ToBaseType(),
            Warehouse != null ? Warehouse.Id.ToBaseType() : BaseId.NULL_ID.ToBaseType(),
            StorageArea != null ? StorageArea.ExternalId : "",
            ExternalId,
            InventoryAvailableTiming.ToString(),
            SetWarehouseDuringMRP,
            MaterialPostProcessingSpan.TotalHours,
            FixedQty,
            LotCode,
            RequireEmptyStorageArea
            );

        if (a_publishInventory)
        {
            if (Demands != null)
            {
                Demands.PtDbPopulate(ref r_dataSet, jobProductRow, a_dbHelper);
            }
        }
    }
    #endregion

    #region Cloning
    /// <summary>
    /// Creates a deep copy of the product.
    /// </summary>
    /// <param name="a_id"></param>
    /// <param name="a_originalProduct"></param>
    /// <returns></returns>
    internal virtual Product DeepCopy()
    {
        return new Product(m_idGen.NextID(), this);
    }
    #endregion

    #region Bools
    private BoolVector32 m_bools;

    //private const int StoreInTankIdx = 0; Deprecated
    private const int SetWarehouseDuringMRPIdx = 1;
    private const int c_fixedQtyIdx = 2;
    private const int c_limitMatlSrcToEligibleLotsIdx = 3;
    private const int c_useLimitMatlSrcToEligibleLotsIdx = 4;
    private const int c_requireEmptyStorageIdx = 5;
    private const int c_storeDuringPostProcessing = 6;
    #endregion

    #region MRP
    private DemandCollection m_demands;

    /// <summary>
    /// Specifies the demands for which the Product was created.
    /// Null if not specified.
    /// </summary>
    public DemandCollection Demands
    {
        get => m_demands;
        internal set => m_demands = value;
    }

    private int m_wearDurability;

    public int WearDurability
    {
        get => m_wearDurability;
        internal set => m_wearDurability = value;
    }
    #endregion MRP

    /// <summary>
    /// Returns the qty that this product is supplying to the specified MaterialRequirement
    /// </summary>
    /// <param name="a_mr"></param>
    /// <returns></returns>
    public decimal GetSupplyQtyForMr(MaterialRequirement a_mr)
    {
        decimal supplyQty = 0m;
        foreach (Adjustment adjustment in m_producedLot.GetAdjustmentArray())
        {
            if (adjustment is MaterialRequirementAdjustment mrAdjustment)
            {
                if (mrAdjustment.Material == a_mr)
                {
                    supplyQty += mrAdjustment.Qty;
                }
            }
        }

        return supplyQty;
    }

    /// <summary>
    /// Returns the qty that this product is supplying to the specified Operation
    /// </summary>
    public decimal GetSupplyQtyForOp(InternalOperation a_op)
    {
        decimal supplyQty = 0m;
        foreach (Adjustment adjustment in m_producedLot.GetAdjustmentArray())
        {
            if (adjustment is MaterialRequirementAdjustment mrAdjustment)
            {
                if (a_op.MaterialRequirements.Contains(mrAdjustment.Material))
                {
                    supplyQty += mrAdjustment.Qty;
                }
            }
        }

        return supplyQty;
    }

    internal void ValidateStorageAreaDelete(BaseOperation a_op, StorageArea a_saToDelete, StorageAreasDeleteProfile a_saDeleteProfile)
    {
        if (!a_saToDelete.CanStoreItem(Item.Id))
        {
            //can delete, this was not a storage area that could have stored the item to begin with
            return;
        }

        if (!ValidateStorageAreaDelete(a_op, StorageArea, Warehouse, a_saToDelete, a_saDeleteProfile))
        {
            return;
        }
    }

    private bool ValidateStorageAreaDelete(BaseOperation a_op, StorageArea a_sa, Warehouse a_wh, StorageArea a_saToDelete, StorageAreasDeleteProfile a_saDeleteProfile)
    {
        if (a_sa == null)
        {
            //Check at least one SA in the Warehouse can store the item, if the give SA is deleted
            foreach (StorageArea sa in a_wh.StorageAreas)
            {
                if (sa.Id == a_saToDelete.Id || a_saDeleteProfile.ContainsStorageArea(sa.Id))
                {
                    continue;
                }

                if (sa.CanStoreItem(Item.Id))
                {
                    return true;
                }
            }

            a_saDeleteProfile.AddValidationException(a_saToDelete, new PTValidationException("3100", new object[] { a_saToDelete.ExternalId, a_op.ManufacturingOrder.Job.ExternalId, a_op.ManufacturingOrder.ExternalId, a_op.ExternalId, ExternalId }));
            return false;
        }

        if (a_sa.Id == a_saToDelete.Id)
        {
            a_saDeleteProfile.AddValidationException(a_saToDelete, new PTValidationException("3100", new object[] { a_saToDelete.ExternalId, a_op.ManufacturingOrder.Job.ExternalId, a_op.ManufacturingOrder.ExternalId, a_op.ExternalId, ExternalId }));
            return false;
        }

        return true;
    }
}