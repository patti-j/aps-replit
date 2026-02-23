using PT.APSCommon;

namespace PT.Scheduler.Schedule.InventoryManagement;

/// <summary>
/// Defines information used when transfering inventory from one warehouse to another warehouse.
/// </summary>
public class InventoryTransferRule : IPTSerializable
{
    private class ReferenceInfo
    {
        internal ReferenceInfo(BaseId a_fromWarehouseId, BaseId a_toWarehouseId, BaseId? a_itemId)
        {
            FromWarehouseId = a_fromWarehouseId;
            ToWarehouseId = a_toWarehouseId;
            ItemId = a_itemId;
        }

        internal readonly BaseId FromWarehouseId;
        internal readonly BaseId ToWarehouseId;
        internal readonly BaseId? ItemId;
    }

    private ReferenceInfo m_referenceInfo;

    private Warehouse m_fromWarehouse;

    public Warehouse FromWarehouse
    {
        get => m_fromWarehouse;
        private set => m_fromWarehouse = value;
    }

    private Warehouse m_toWarehouse;

    public Warehouse ToWarehouse
    {
        get => m_toWarehouse;
        private set => m_toWarehouse = value;
    }

    private Item m_item;

    /// <summary>
    /// Optional. Rules applies to a specific Item in the From/To Warehouses.
    /// </summary>
    public Item Item
    {
        get => m_item;
        private set => m_item = value;
    }

    private TimeSpan m_transferSpan;

    public TimeSpan TransferSpan
    {
        get => m_transferSpan;
        private set => m_transferSpan = value;
    }

    public InventoryTransferRule(Warehouse a_fromWarehouse, Warehouse a_toWarehouse, Item a_item, TimeSpan a_transferSpan)
    {
        //TODO: Item cannot be set yet
        //if (a_item == null)
        //{
        //    throw new APSCommon.PTValidationException("2907");
        //}
        if (a_fromWarehouse == null || a_toWarehouse == null)
        {
            throw new PTValidationException("2906", new object[] { a_item.ExternalId, a_item.Name });
        }

        if (a_item != null && (!a_fromWarehouse.Inventories.Contains(a_item.Id) || !a_toWarehouse.Inventories.Contains(a_item.Id)))
        {
            throw new PTValidationException("2908", new object[] { a_item.ExternalId, a_item.Name });
        }

        FromWarehouse = a_fromWarehouse;
        ToWarehouse = a_toWarehouse;
        Item = a_item;
        TransferSpan = a_transferSpan;
    }

    public InventoryTransferRule(Warehouse a_fromWarehouse, Warehouse a_toWarehouse, TimeSpan a_transferSpan)
        : this(a_fromWarehouse, a_toWarehouse, null, a_transferSpan) { }

    public InventoryTransferRule(IReader a_reader)
    {
        long idVal;

        a_reader.Read(out idVal);
        BaseId fromWarehouseId = new (idVal);

        a_reader.Read(out idVal);
        BaseId toWarehouseId = new (idVal);

        bool hasItem = false;
        a_reader.Read(out hasItem);
        BaseId? itemId = null;
        if (hasItem)
        {
            a_reader.Read(out idVal);
            itemId = new BaseId(idVal);
        }

        m_referenceInfo = new ReferenceInfo(fromWarehouseId, toWarehouseId, itemId);

        a_reader.Read(out m_transferSpan);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(FromWarehouse.Id.Value);
        a_writer.Write(ToWarehouse.Id.Value);
        a_writer.Write(Item != null);
        if (Item != null)
        {
            a_writer.Write(Item.Id.Value);
        }

        a_writer.Write(TransferSpan);
    }

    public void RestoreReferences(ScenarioDetail a_sd)
    {
        FromWarehouse = a_sd.WarehouseManager.GetById(m_referenceInfo.FromWarehouseId);
        ToWarehouse = a_sd.WarehouseManager.GetById(m_referenceInfo.ToWarehouseId);
        if (m_referenceInfo.ItemId != null)
        {
            Item = a_sd.ItemManager.GetById(m_referenceInfo.ItemId.Value);
        }

        m_referenceInfo = null;
    }

    public const int UNIQUE_ID = 803;

    public int UniqueId => UNIQUE_ID;
}