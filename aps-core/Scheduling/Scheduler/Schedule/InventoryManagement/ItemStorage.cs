using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;

namespace PT.Scheduler;

/// <summary>
/// Inventory object data that is related to item storage. The Inventory object contains planning properties while this class contains storage and scheduling fields.
/// </summary>
public partial class ItemStorage : BaseObject, IPTSerializable, IComparable<ItemStorage>
{
    #region IPTSerializable Members
    internal ItemStorage(BaseId a_newId, Item a_item, StorageArea a_storageArea, Inventory a_inv)
        : base(a_newId)
    {
        m_item = a_item;
        m_storageArea = a_storageArea;
        m_inventory = a_inv;
    }

    internal ItemStorage(BaseId a_newId, Item a_item, StorageArea a_storageArea)
        : base(a_newId)
    {
        m_item = a_item;
        m_storageArea = a_storageArea;
        m_inventory = a_storageArea.Warehouse.Inventories[m_item.Id];
    }

    internal ItemStorage(IReader a_reader)
        : base(a_reader)
    {
        m_boolVector32 = new BoolVector32(a_reader);
        m_itemId = new BaseId(a_reader);
        a_reader.Read(out m_maxQty);
        a_reader.Read(out m_disposalQty);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_boolVector32.Serialize(a_writer);
        m_item.Id.Serialize(a_writer);
        a_writer.Write(m_maxQty);
        a_writer.Write(m_disposalQty);
    }

    public override int UniqueId => 1206;
    public override string DefaultNamePrefix => "Item Storage".Localize();
    #endregion IPTSerializable Members

    private readonly BaseId m_itemId = BaseId.NULL_ID;
    private Item m_item;

    public Item Item => m_item;

    private Warehouse m_warehouse;

    public Warehouse Warehouse => m_warehouse;

    private Inventory m_inventory;
    public Inventory Inventory => m_inventory;

    public StorageArea StorageArea => m_storageArea;

    private decimal m_maxQty;
     
    /// <summary>
    /// The maximum qty of this item that can be stored at any point in time.
    /// </summary>
    public decimal MaxQty => m_maxQty;

    public bool UnConstrained => m_maxQty <= 0;


    private decimal m_disposalQty;

    /// <summary>
    /// Represents the minimum limit to maintain material in the Storage Area. If the qty goes below this quantity, the remainder will be disposed.
    /// </summary>
    public decimal DisposalQty => m_disposalQty;

    //private TimeSpan m_disposalTimeMin;

    ///// <summary>
    ///// How long the material will be kept before it is eligible to be disposed.
    ///// This is different than expiration, because it will only be disposed if 
    ///// </summary>
    //public TimeSpan DisposalTimeMin => m_disposalTimeMin;

    //private TimeSpan m_disposalTimeMax;

    ///// <summary>
    ///// The maximum time the material can be kept before it is automatically disposed.
    ///// </summary>
    //public TimeSpan DisposalTimeMax => m_disposalTimeMax;

    private StorageArea m_storageArea;

    private BoolVector32 m_boolVector32;

    private const short c_disposeImmediatelyIdx = 0;

    /// <summary>
    /// Whether the material will be disposed of immediately upon meeting the disposal requirement (time or qty).
    /// If false, the material will only be disposed if needed to make room for other storage.
    /// </summary>
    public bool DisposeImmediately
    {
        get => m_boolVector32[c_disposeImmediatelyIdx];
        internal set => m_boolVector32[c_disposeImmediatelyIdx] = value;
    }

    public int CompareTo(ItemStorage a_other)
    {
        return Id.CompareTo(a_other.Id);
    }

    internal void RestoreReferences(CustomerManager a_cm, ItemManager a_itemManager, Warehouse a_warehouse, StorageArea a_storageArea, ISystemLogger a_errorReporter)
    {
        m_item = a_itemManager.GetById(m_itemId);
        m_warehouse = a_warehouse;
        m_inventory = m_warehouse.Inventories[m_itemId];
        m_storageArea = a_storageArea;
    }

    public override BaseId GetKey()
    {
        return m_item?.Id ?? m_itemId;
    }

    internal void Update(WarehouseT.ItemStorage a_itemStorage, LotManager a_lots, UserFieldDefinitionManager a_udfManager)
    {
        UpdateUserFields(a_itemStorage.UserFields, a_udfManager, UserField.EUDFObjectType.ItemStorage);
        m_maxQty = a_itemStorage.MaxQty;
        m_disposalQty = a_itemStorage.DisposalQty;
        DisposeImmediately = a_itemStorage.DisposeImmediately;
    }

    public override string ToString()
    {
        return $"{Inventory.Item.Name}; SA={StorageArea}";
    }

    internal void ValidateItemDelete(ItemDeleteProfile a_items)
    {
        if (a_items.ContainsItem(Item.Id))
        {
            PTValidationException validationException = new("3104", new object[] { Item.Name, Name });
            a_items.AddValidationException(Item, validationException);
        }
    }
}

