using System.ComponentModel;
using PT.APSCommon;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Schedule.Storage;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.Demand;

public partial class TransferOrderDistribution : BaseObject, IMaterialAllocation
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 649;

    internal TransferOrderDistribution(IReader a_reader, TransferOrder a_transferOrder, BaseIdGenerator a_idGen)
        : base(a_reader)
    {
        m_simAdjustments = new AdjustmentArray(a_idGen);
        
        if (a_reader.VersionNumber >= 12551)
        {
            m_bools = new BoolVector32(a_reader);
            m_transferOrder = a_transferOrder;
            a_reader.Read(out m_qtyOrdered);
            a_reader.Read(out m_qtyShipped);
            a_reader.Read(out m_qtyReceived);
            a_reader.Read(out m_scheduledShipDateTicks);
            a_reader.Read(out m_scheduledReceiptTicks);

            m_restoreInfo = new RestoreInfo();

            m_restoreInfo.ItemId = new BaseId(a_reader);
            m_restoreInfo.FromWarehouseId = new BaseId(a_reader);
            m_restoreInfo.ToWarehouseId = new BaseId(a_reader);
            m_restoreInfo.ToStorageAreaId = new BaseId(a_reader);
            m_restoreInfo.FromStorageAreaId = new BaseId(a_reader);
            m_eligibleLots = new Schedule.Demand.EligibleLots(a_reader);
            a_reader.Read(out m_lotCode);
            a_reader.Read(out m_actualShipTicks);
            a_reader.Read(out m_actualReceiptTicks);
            a_reader.Read(out int val);
            m_matlAlloc = (ItemDefs.MaterialAllocation)val;
            a_reader.Read(out m_minSourceQty);
            a_reader.Read(out m_maxSourceQty);
            a_reader.Read(out val);
            m_materialSourcing = (ItemDefs.MaterialSourcing)val;

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new(a_reader);
                BaseId lotId = new(a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }

            m_restoreInfo.ProducedLotId = new BaseId(a_reader);
            m_demandAdjustments = new (a_reader, a_idGen);
        }
        else if(a_reader.VersionNumber >= 12055) //Added generatedLotIds
        {
            m_demandAdjustments = new (a_idGen);
            m_transferOrder = a_transferOrder;
            a_reader.Read(out m_qtyOrdered);
            a_reader.Read(out m_qtyShipped);
            a_reader.Read(out m_qtyReceived);
            a_reader.Read(out m_scheduledShipDateTicks);
            a_reader.Read(out m_scheduledReceiptTicks);
            a_reader.Read(out bool m_closed);
            Closed = m_closed;

            m_restoreInfo = new RestoreInfo();

            m_restoreInfo.ItemId = new BaseId(a_reader);
            m_restoreInfo.FromWarehouseId = new BaseId(a_reader);
            m_restoreInfo.ToWarehouseId = new BaseId(a_reader);
            m_eligibleLots = new Schedule.Demand.EligibleLots(a_reader);
            a_reader.Read(out m_lotCode);
            a_reader.Read(out m_actualShipTicks);
            a_reader.Read(out m_actualReceiptTicks);
            a_reader.Read(out int val);
            m_matlAlloc = (ItemDefs.MaterialAllocation)val;
            a_reader.Read(out m_minSourceQty);
            a_reader.Read(out m_maxSourceQty);
            a_reader.Read(out val);
            m_materialSourcing = (ItemDefs.MaterialSourcing)val;

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new (a_reader);
                BaseId lotId = new (a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }
        }
        else if (a_reader.VersionNumber >= 12000) //731 reader for v12 backwards compatibility
        {
            m_demandAdjustments = new (a_idGen);
            m_transferOrder = a_transferOrder;
            a_reader.Read(out m_qtyOrdered);
            a_reader.Read(out m_qtyShipped);
            a_reader.Read(out m_qtyReceived);
            a_reader.Read(out m_scheduledShipDateTicks);
            a_reader.Read(out m_scheduledReceiptTicks);
            a_reader.Read(out bool m_closed);
            Closed = m_closed;

            m_restoreInfo = new RestoreInfo();

            m_restoreInfo.ItemId = new BaseId(a_reader);
            m_restoreInfo.FromWarehouseId = new BaseId(a_reader);
            m_restoreInfo.ToWarehouseId = new BaseId(a_reader);
            m_eligibleLots = new Schedule.Demand.EligibleLots(a_reader);
            a_reader.Read(out m_lotCode);
            a_reader.Read(out m_actualShipTicks);
            a_reader.Read(out m_actualReceiptTicks);
            a_reader.Read(out int val);
            m_matlAlloc = (ItemDefs.MaterialAllocation)val;
            a_reader.Read(out m_minSourceQty);
            a_reader.Read(out m_maxSourceQty);
            a_reader.Read(out val);
            m_materialSourcing = (ItemDefs.MaterialSourcing)val;
        }
        else if (a_reader.VersionNumber >= 756) //Added generatedLotIds
        {
            m_transferOrder = a_transferOrder;
            a_reader.Read(out m_qtyOrdered);
            a_reader.Read(out m_qtyShipped);
            a_reader.Read(out m_qtyReceived);
            a_reader.Read(out m_scheduledShipDateTicks);
            a_reader.Read(out m_scheduledReceiptTicks);
            a_reader.Read(out bool m_closed);
            Closed = m_closed;

            m_restoreInfo = new RestoreInfo();

            m_restoreInfo.ItemId = new BaseId(a_reader);
            m_restoreInfo.FromWarehouseId = new BaseId(a_reader);
            m_restoreInfo.ToWarehouseId = new BaseId(a_reader);
            m_eligibleLots = new Schedule.Demand.EligibleLots(a_reader);
            a_reader.Read(out m_lotCode);
            a_reader.Read(out m_actualShipTicks);
            a_reader.Read(out m_actualReceiptTicks);
            a_reader.Read(out int val);
            m_matlAlloc = (ItemDefs.MaterialAllocation)val;
            a_reader.Read(out m_minSourceQty);
            a_reader.Read(out m_maxSourceQty);
            a_reader.Read(out val);
            m_materialSourcing = (ItemDefs.MaterialSourcing)val;

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new (a_reader);
                BaseId lotId = new (a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }
        }
        else if (a_reader.VersionNumber >= 731)
        {
            m_transferOrder = a_transferOrder;
            a_reader.Read(out m_qtyOrdered);
            a_reader.Read(out m_qtyShipped);
            a_reader.Read(out m_qtyReceived);
            a_reader.Read(out m_scheduledShipDateTicks);
            a_reader.Read(out m_scheduledReceiptTicks);
            a_reader.Read(out bool m_closed);
            Closed = m_closed;

            m_restoreInfo = new RestoreInfo();

            m_restoreInfo.ItemId = new BaseId(a_reader);
            m_restoreInfo.FromWarehouseId = new BaseId(a_reader);
            m_restoreInfo.ToWarehouseId = new BaseId(a_reader);
            m_eligibleLots = new Schedule.Demand.EligibleLots(a_reader);
            a_reader.Read(out m_lotCode);
            a_reader.Read(out m_actualShipTicks);
            a_reader.Read(out m_actualReceiptTicks);
            a_reader.Read(out int val);
            m_matlAlloc = (ItemDefs.MaterialAllocation)val;
            a_reader.Read(out m_minSourceQty);
            a_reader.Read(out m_maxSourceQty);
            a_reader.Read(out val);
            m_materialSourcing = (ItemDefs.MaterialSourcing)val;
        }
    }

    private class RestoreInfo
    {
        internal BaseId ItemId;
        internal BaseId FromWarehouseId;
        internal BaseId ToWarehouseId;
        internal BaseId ToStorageAreaId = BaseId.NULL_ID;
        internal BaseId FromStorageAreaId = BaseId.NULL_ID;
        internal BaseId ProducedLotId = BaseId.NULL_ID;
    }

    private RestoreInfo m_restoreInfo;

    internal Inventory ToInventory;

    internal void RestoreReferences(WarehouseManager a_warehouses, ItemManager a_items)
    {
        m_item = a_items.GetById(m_restoreInfo.ItemId);
        m_fromWarehouse = a_warehouses.GetById(m_restoreInfo.FromWarehouseId);
        m_toWarehouse = a_warehouses.GetById(m_restoreInfo.ToWarehouseId);
        ToInventory = m_toWarehouse.Inventories[m_item.Id];

        if (m_restoreInfo.ProducedLotId != BaseId.NULL_ID)
        {
            m_producedLot = ToInventory.Lots.GetById(m_restoreInfo.ProducedLotId);
            //TODO:  we need to handle clearing the produced lot when deleting Lots
            m_producedLot?.RestoreSource(this);
        }

        if (m_restoreInfo.FromStorageAreaId != BaseId.NULL_ID)
        {
            m_fromStorageArea = m_fromWarehouse.StorageAreas.GetValue(m_restoreInfo.FromStorageAreaId);
        }
        
        if (m_restoreInfo.ToStorageAreaId != BaseId.NULL_ID)
        {
            m_toStorageArea = m_toWarehouse.StorageAreas.GetValue(m_restoreInfo.ToStorageAreaId);
        }

        m_restoreInfo = null;
    }

    //Sort the data so we don't have to worry about sort timing when accessing the collection first after loading.
    internal void AfterRestoreAdjustmentReferences()
    {
        m_simAdjustments.Sort();
        m_demandAdjustments.Sort();
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        a_writer.Write(m_qtyOrdered);
        a_writer.Write(m_qtyShipped);
        a_writer.Write(m_qtyReceived);
        a_writer.Write(m_scheduledShipDateTicks);
        a_writer.Write(m_scheduledReceiptTicks);

        m_item.Id.Serialize(a_writer);
        m_fromWarehouse.Id.Serialize(a_writer);
        m_toWarehouse.Id.Serialize(a_writer);

        if (m_toStorageArea != null)
        {
            m_toStorageArea.Id.Serialize(a_writer);
        }
        else
        {
            BaseId.NULL_ID.Serialize(a_writer);
        }

        if (m_fromStorageArea != null)
        {
            m_fromStorageArea.Id.Serialize(a_writer);
        }
        else
        {
            BaseId.NULL_ID.Serialize(a_writer);
        }

        m_eligibleLots.Serialize(a_writer);
        a_writer.Write(m_lotCode);
        a_writer.Write(m_actualShipTicks);
        a_writer.Write(m_actualReceiptTicks);
        a_writer.Write((int)m_matlAlloc);
        a_writer.Write(m_minSourceQty);
        a_writer.Write(m_maxSourceQty);
        a_writer.Write((int)m_maxSourceQty);

        a_writer.Write(m_generatedLotIds.Count);
        foreach (KeyValuePair<BaseId, BaseId> ids in m_generatedLotIds)
        {
            ids.Key.Serialize(a_writer);
            ids.Value.Serialize(a_writer);
        }

        if (m_producedLot != null)
        {
            m_producedLot.Id.Serialize(a_writer);
        }
        else
        {
            BaseId.NULL_ID.Serialize(a_writer);
        }
        m_demandAdjustments.Serialize(a_writer);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion IPTSerializable

    #region Construction
    internal TransferOrderDistribution(BaseId a_id, string a_transferOrderExternalId, ERPTransmissions.TransferOrderT.TransferOrder.TransferOrderDistribution a_tod, ItemManager a_itemsManager, WarehouseManager a_wm, TransferOrder a_transferOrder, BaseIdGenerator a_idGen)
        : base(a_id, a_tod)
    {
        m_transferOrder = a_transferOrder;
        m_demandAdjustments = new (a_idGen);
        m_simAdjustments = new (a_idGen);
        Update(a_transferOrderExternalId, a_tod, a_itemsManager, a_wm);
    }
    #endregion

    private readonly TransferOrder m_transferOrder;

    public TransferOrder TransferOrder => m_transferOrder;

    private Item m_item;

    /// <summary>
    /// The Item whose inventory will be transferred between Warehouses.
    /// </summary>
    public Item Item => m_item;

    private Warehouse m_fromWarehouse;

    /// <summary>
    /// The Warehouse from which the Inventory will be subtracted.
    /// </summary>
    public Warehouse FromWarehouse => m_fromWarehouse;

    private Warehouse m_toWarehouse;

    /// <summary>
    /// The Warehouse to which the Inventory will be added.
    /// </summary>
    public Warehouse ToWarehouse => m_toWarehouse;

    private StorageArea m_toStorageArea;

    /// <summary>
    /// The StorageArea to which the Inventory will be added (optional).
    /// </summary>
    public StorageArea ToStorageArea
    {
        get => m_toStorageArea;
        private set { m_toStorageArea = value; }
    }

    private StorageArea m_fromStorageArea;

    /// <summary>
    /// The Warehouse to which the Inventory will be added.
    /// </summary>
    public StorageArea FromStorageArea
    {
        get => m_fromStorageArea;
        private set { m_fromStorageArea = value; }
    }

    private string m_lotCode;

    public bool LimitMatlSrcToEligibleLots => m_eligibleLots.Count > 0;

    public int WearDurability => -1; //TOs don't use this

    /// <summary>
    /// The lot code to associate with available material
    /// </summary>
    public string LotCode
    {
        get => m_lotCode;
        internal set => m_lotCode = value;
    }

    public long MaterialPostProcessingTicks => 0; //TOs don't use this. //TODO: Consider adding

    private readonly PT.Scheduler.Schedule.Demand.EligibleLots m_eligibleLots = new ();

    internal PT.Scheduler.Schedule.Demand.EligibleLots EligibleLots => m_eligibleLots;

    public IEnumerable<string> EligibleLotCodesEnumerator
    {
        get
        {
            foreach (string lotCode in m_eligibleLots.LotCodesEnumerator)
            {
                yield return lotCode;
            }
        }
    }

    internal void Update(string a_transferOrderExternalId, TransferOrderT.TransferOrder.TransferOrderDistribution a_tod, ItemManager a_itemManager, WarehouseManager a_warehouseManager)
    {
        bool transferOrderDistributionChanged = false;

        if (FromWarehouse == null || FromWarehouse.ExternalId != a_tod.FromWarehouseExternalId)
        {
            if (a_warehouseManager.GetByExternalId(a_tod.FromWarehouseExternalId) is not Warehouse warehouse)
            {
                throw new PTValidationException("2167", new object[] { a_transferOrderExternalId, a_tod.FromWarehouseExternalId });
            }

            m_fromWarehouse = warehouse;
            transferOrderDistributionChanged = true;
        }

        if (ToWarehouse == null || ToWarehouse.ExternalId != a_tod.ToWarehouseExternalId)
        {
            if (a_warehouseManager.GetByExternalId(a_tod.ToWarehouseExternalId) is not Warehouse warehouse)
            {
                throw new PTValidationException("2168", new object[] { a_transferOrderExternalId, a_tod.ToWarehouseExternalId });
            }

            m_toWarehouse = warehouse;
            transferOrderDistributionChanged = true;
        }

        if (Item == null || Item.ExternalId != a_tod.ItemExternalId)
        {
            Item item = a_itemManager.GetByExternalId(a_tod.ItemExternalId);
            if (item == null)
            {
                throw new PTValidationException("2169", new object[] { a_transferOrderExternalId, a_tod.ItemExternalId });
            }

            // both warehouses should have inventory records
            if (!m_fromWarehouse.Inventories.Contains(item.Id))
            {
                throw new PTValidationException("2877", new object[] { a_transferOrderExternalId, a_tod.ItemExternalId, a_tod.FromWarehouseExternalId });
            }

            if (!m_toWarehouse.Inventories.Contains(item.Id))
            {
                throw new PTValidationException("2878", new object[] { a_transferOrderExternalId, a_tod.ItemExternalId, a_tod.ToWarehouseExternalId });
            }

            m_item = item;
            ToInventory = m_toWarehouse.Inventories[m_item.Id];
            transferOrderDistributionChanged = true;
        }

        if (Closed != a_tod.Closed)
        {
            Closed = a_tod.Closed;
            transferOrderDistributionChanged = true;
        }

        if (QtyOrdered != a_tod.QtyOrdered)
        {
            QtyOrdered = a_tod.QtyOrdered;
            transferOrderDistributionChanged = true;
        }

        if (QtyReceived != a_tod.QtyReceived)
        {
            QtyReceived = a_tod.QtyReceived;
            transferOrderDistributionChanged = true;
        }

        if (QtyShipped != a_tod.QtyShipped)
        {
            QtyShipped = a_tod.QtyShipped;
            transferOrderDistributionChanged = true;
        }

        if (ScheduledReceiveDate != a_tod.ScheduledReceiveDate)
        {
            m_scheduledReceiptTicks = a_tod.ScheduledReceiveDate.Ticks;
            transferOrderDistributionChanged = true;
        }

        if (ScheduledShipDate != a_tod.ScheduledShipDate)
        {
            m_scheduledShipDateTicks = a_tod.ScheduledShipDate.Ticks;
            transferOrderDistributionChanged = true;
        }

        if (QtyReceived > QtyShipped)
        {
            throw new PTValidationException("2876", new object[] { QtyReceived, QtyShipped });
        }

        if (MaterialAllocation != a_tod.MaterialAllocation)
        {
            transferOrderDistributionChanged = true;
            MaterialAllocation = a_tod.MaterialAllocation;
        }

        if (MinSourceQty != a_tod.MinSourceQty)
        {
            transferOrderDistributionChanged = true;
            MinSourceQty = a_tod.MinSourceQty;
        }

        if (MaxSourceQty != a_tod.MaxSourceQty)
        {
            transferOrderDistributionChanged = true;
            MaxSourceQty = a_tod.MaxSourceQty;
        }

        if (MaterialSourcing != a_tod.MaterialSourcing)
        {
            transferOrderDistributionChanged = true;
            MaterialSourcing = a_tod.MaterialSourcing;
        }

        if (PreferEmptyStorageArea != a_tod.PreferEmptyStorageArea)
        {
            transferOrderDistributionChanged = true;
            PreferEmptyStorageArea = a_tod.PreferEmptyStorageArea;
        }

        if (OverrideStorageConstraint != a_tod.OverrideStorageConstraint)
        {
            transferOrderDistributionChanged = true;
            OverrideStorageConstraint = a_tod.OverrideStorageConstraint;
        }

        if (AllowPartialAllocations != a_tod.AllowPartialAllocations)
        {
            transferOrderDistributionChanged = true;
            AllowPartialAllocations = a_tod.AllowPartialAllocations;
        }

        if (a_tod.FromStorageAreaExternalIdIsSet)
        {
            if (m_fromStorageArea == null || m_fromStorageArea.ExternalId != a_tod.FromStorageAreaExternalId)
            {
                //If from warehouse is set, validate that the storage area exists in that warehouse, otherwise check all warehouses
                if (m_fromWarehouse != null)
                {
                    if (!ValidateStorageAreaExists(a_tod.FromStorageAreaExternalId, m_fromWarehouse, a_warehouseManager, out StorageArea fromStorageArea))
                    {
                        throw new PTValidationException("3117", new object[] { a_transferOrderExternalId, m_fromWarehouse.ExternalId, a_tod.FromStorageAreaExternalId });
                    }

                    FromStorageArea = fromStorageArea;
                }
                else
                {
                    if (!ValidateStorageAreaExists(a_tod.FromStorageAreaExternalId, null, a_warehouseManager, out StorageArea fromStorageArea))
                    {
                        throw new PTValidationException("3118", new object[] { a_transferOrderExternalId, a_tod.FromStorageAreaExternalId });
                    }

                    FromStorageArea = fromStorageArea;
                }

                transferOrderDistributionChanged = true;
            }
        }

        if (a_tod.ToStorageAreaExternalIdIsSet)
        {
            if (m_toStorageArea == null || m_toStorageArea.ExternalId != a_tod.ToStorageAreaExternalId)
            {
                //If to warehouse is set, validate that the storage area exists in that warehouse, otherwise check all warehouses
                if (m_toWarehouse != null)
                {
                    if (!ValidateStorageAreaExists(a_tod.ToStorageAreaExternalId, m_toWarehouse, a_warehouseManager, out StorageArea toStorageArea))
                    {
                        throw new PTValidationException("3119", new object[] { a_transferOrderExternalId, m_toWarehouse.ExternalId, a_tod.ToStorageAreaExternalId });
                    }

                    ToStorageArea = toStorageArea;
                }
                else
                {
                    if (!ValidateStorageAreaExists(a_tod.ToStorageAreaExternalId, null, a_warehouseManager, out StorageArea toStorageArea))
                    {
                        throw new PTValidationException("3118", new object[] { a_transferOrderExternalId, a_tod.ToStorageAreaExternalId });
                    }

                    ToStorageArea = toStorageArea;
                }

                transferOrderDistributionChanged = true;
            }
        }

        if (!ValidateItemCanBeStored(FromStorageArea, FromWarehouse))
        {
            if (FromStorageArea != null)
            {
                throw new PTValidationException("3120", new object[] { a_transferOrderExternalId, FromStorageArea.ExternalId, Item.ExternalId });
            }
            else
            {
                throw new PTValidationException("3121", new object[] { a_transferOrderExternalId, Item.ExternalId });
            }
        }

        //Validate to storage area is able to store item  if defined
        if (!ValidateItemCanBeStored(ToStorageArea, ToWarehouse))
        {
            if (ToStorageArea != null)
            {
                throw new PTValidationException("3120", new object[] { a_transferOrderExternalId, ToStorageArea.ExternalId, Item.ExternalId });
            }
            else
            {
                throw new PTValidationException("3121", new object[] { a_transferOrderExternalId, Item.ExternalId });
            }
        }
        
        if (transferOrderDistributionChanged)
        {
            SetInventoryNetChangeMrpFlag();
        }
    }

    private static bool ValidateStorageAreaExists(string a_storageAreaExternalId, Warehouse a_warehouseToCheck, WarehouseManager a_wm, out StorageArea o_storageArea)
    {
        o_storageArea = null;
        //If warehouse to check is set, validate that the storage area exists in that warehouse, otherwise check all warehouses
        if (a_warehouseToCheck != null)
        {
            o_storageArea = a_warehouseToCheck.StorageAreas.GetByExternalId(a_storageAreaExternalId);
            return o_storageArea != null;
        }

        foreach (Warehouse warehouse in a_wm)
        {
            o_storageArea = warehouse.StorageAreas.GetByExternalId(a_storageAreaExternalId);

            if (o_storageArea != null)
            {
                return true;
            }
        }

        return false;
    }

    internal bool ValidateItemCanBeStored(StorageArea a_storageArea, Warehouse a_wh)
    {
        //if Storage Area is defined check if it can store item, otherwise check all storage areas
        if (a_storageArea != null)
        {
            return a_storageArea.CanStoreItem(Item.Id);
        }

        foreach (StorageArea sa in a_wh.StorageAreas)
        {
            if (sa.CanStoreItem(Item.Id))
            {
                return true;
            }
        }

        return false;
    }

    internal void ValidateStorageAreaDelete(StorageArea a_storageArea, StorageAreasDeleteProfile a_saDeleteProfile)
    {
        if (!a_storageArea.CanStoreItem(Item.Id))
        {
            //can delete, this was not a storage area that could have stored the item to begin with
            return;
        }

        if (!ValidateStorageAreaDelete(FromStorageArea, FromWarehouse, a_storageArea, a_saDeleteProfile))
        {
            return;
        }

        if (!ValidateStorageAreaDelete(ToStorageArea, ToWarehouse, a_storageArea, a_saDeleteProfile))
        {
            return;
        }
    }

    private bool ValidateStorageAreaDelete(StorageArea a_sa, Warehouse a_wh, StorageArea a_saToDelete, StorageAreasDeleteProfile a_saDeleteProfile)
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

            a_saDeleteProfile.AddValidationException(a_saToDelete, new PTValidationException("3126", new object[] { a_saToDelete.Name, Item.Id, TransferOrder.Name }));
            return false;
        }

        if (a_sa.Id == a_saToDelete.Id)
        {
            a_saDeleteProfile.AddValidationException(a_saToDelete, new PTValidationException("3122", new object[] { a_saToDelete.Name, TransferOrder.Name }));
            return false;
        }

        return true;
    }

    internal void SetInventoryNetChangeMrpFlag()
    {
        //Check that the inventory exists, it may have been deleted.
        Inventory inv = FromWarehouse.Inventories[Item.Id];
        if (inv != null)
        {
            inv.IncludeInNetChangeMRP = true;
        }

        inv = ToWarehouse.Inventories[Item.Id];
        if (inv != null)
        {
            inv.IncludeInNetChangeMRP = true;
        }
    }

    #region Shared Properties
    private long m_scheduledShipDateTicks;

    public long ScheduledShipDateTicks => m_scheduledShipDateTicks;

    /// <summary>
    /// The date/time when the inventory is planned to be removed from the From Warehouse.
    /// </summary>
    public DateTime ScheduledShipDate => new (m_scheduledShipDateTicks);

    private long m_actualShipTicks;

    public long ActualShipTicks
    {
        get => m_actualShipTicks;
        internal set => m_actualShipTicks = value;
    }

    private long m_scheduledReceiptTicks;

    /// <summary>
    /// When the material is actually received.
    /// </summary>
    public long ActualAvailableTicks
    {
        get => m_actualReceiptTicks;
        set { ; }
    }

    public long ScheduledReceiveDateTicks
    {
        get => m_scheduledReceiptTicks;
        internal set => m_scheduledReceiptTicks = value;
    }

    private long m_actualReceiptTicks;

    public long ActualReceiptTicks
    {
        get => m_actualReceiptTicks;
        internal set => m_actualReceiptTicks = value;
    }

    /// <summary>
    /// The date/time when the inventory is planned to be added to the To Warehouse.
    /// </summary>
    public DateTime ScheduledReceiveDate => new (m_scheduledReceiptTicks);

    private decimal m_qtyOrdered;

    /// <summary>
    /// The total quantity expected to be shipped.
    /// </summary>
    public decimal QtyOrdered
    {
        get => m_qtyOrdered;
        private set => m_qtyOrdered = value;
    }

    private decimal m_qtyShipped;

    /// <summary>
    /// The quantity already removed from the inventory of the From Warehouse.
    /// </summary>
    public decimal QtyShipped
    {
        get => m_qtyShipped;
        private set => m_qtyShipped = value;
    }

    /// <summary>
    /// The quantity that remains to be shipped.
    /// </summary>
    public decimal QtyOpenToShip => QtyOrdered - QtyShipped;

    private decimal m_qtyReceived;

    /// <summary>
    /// The quantity already received into the To Warehouse.
    /// </summary>
    public decimal QtyReceived
    {
        get => m_qtyReceived;
        private set => m_qtyReceived = value;
    }

    private BoolVector32 m_bools;

    private const short c_closedIdx = 0;
    private const short c_preferEmptyStorageAreaIdx = 1;
    private const short c_overrideStorageConstraintIdx = 2;
    private const short c_allowPartialAllocationsIdx = 3;
    
    /// <summary>
    /// If Closed then the Transfer Order Distribution has no further affect on the Inventory Plan.
    /// </summary>
    public bool Closed
    {
        get => m_bools[c_closedIdx];
        private set => m_bools[c_closedIdx] = value;
    }

    /// <summary>
    /// Whether the material needs to be stored in an Empty Storage Area
    /// </summary>
    public bool PreferEmptyStorageArea
    {
        get => m_bools[c_preferEmptyStorageAreaIdx];
        private set => m_bools[c_preferEmptyStorageAreaIdx] = value;
    }

    /// <summary>
    /// Whether this material will store in excess of the storage areas max quantity when received.
    /// If false, any material that can't be stored will be discarded.
    /// </summary>
    public bool OverrideStorageConstraint
    {
        get => m_bools[c_overrideStorageConstraintIdx];
        private set => m_bools[c_overrideStorageConstraintIdx] = value;
    }
    
    /// <summary>
    /// Whether this transfer can source from multiple partial supplies.
    /// </summary>
    public bool AllowPartialAllocations
    {
        get => m_bools[c_allowPartialAllocationsIdx];
        private set => m_bools[c_allowPartialAllocationsIdx] = value;
    }

    private ItemDefs.MaterialAllocation m_matlAlloc = ItemDefs.MaterialAllocation.NotSet;

    public ItemDefs.MaterialAllocation MaterialAllocation
    {
        get => m_matlAlloc;
        private set => m_matlAlloc = value;
    }

    /// <summary>
    /// If specified, each source of material must have at least this much quantity available to be used to fulfill this material requirement.
    /// </summary>
    private decimal m_minSourceQty;

    public decimal MinSourceQty
    {
        get => m_minSourceQty;
        private set => m_minSourceQty = value;
    }

    private decimal m_maxSourceQty;

    /// <summary>
    /// If specified, each source of material must have at least this much quantity available to be used to fulfil this material requirement.
    /// </summary>
    public decimal MaxSourceQty
    {
        get => m_maxSourceQty;
        private set => m_maxSourceQty = value;
    }

    private ItemDefs.MaterialSourcing m_materialSourcing;

    /// <summary>
    /// You can set this value to control certain aspects of how material is sourced.
    /// </summary>
    public ItemDefs.MaterialSourcing MaterialSourcing
    {
        get => m_materialSourcing;
        private set => m_materialSourcing = value;
    }

    public BaseId DemandId => Id;
    #endregion Shared Properties

    #region Miscellaneous
    public override string DefaultNamePrefix => "TransferOrderDistribution";
    #endregion

    /// <summary>
    /// Remove all links to supply sources and inventory signals
    /// </summary>
    internal void Deleting()
    {
        foreach (Adjustment demandAdjustment in m_demandAdjustments)
        {
            if (demandAdjustment.HasLotStorage)
            {
                demandAdjustment.Storage.Lot.DeleteDemand(demandAdjustment);
            }
            else
            {
                demandAdjustment.Inventory.DeleteDemand(demandAdjustment);
            }
        }

        foreach (Adjustment demandAdjustment in m_simAdjustments)
        {
            if (demandAdjustment.HasLotStorage)
            {
                demandAdjustment.Storage.Lot.DeleteDemand(demandAdjustment);
            }
            else
            {
                demandAdjustment.Inventory.DeleteDemand(demandAdjustment);
            }
        }
    }
}