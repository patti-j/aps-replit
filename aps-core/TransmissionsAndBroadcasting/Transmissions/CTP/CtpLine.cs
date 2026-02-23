using PT.APSCommon;

namespace PT.Transmissions.CTP;

public class CtpLine : IPTSerializable
{
    public const int UNIQUE_ID = 619;

    #region IPTSerializable Members
    public CtpLine(IReader reader)
    {
        if (reader.VersionNumber >= 704)
        {
            reader.Read(out m_usingIdKeys);
            if (m_usingIdKeys)
            {
                m_inventoryKey = new SchedulerDefinitions.InventoryKey(reader);
            }
            else
            {
                m_inventoryExternalIdKey = new SchedulerDefinitions.InventoryExternalIdKey(reader);
            }

            reader.Read(out requiredQty);
            reader.Read(out needDate);
            requiredPathId = new BaseId(reader);
        }
        else if (reader.VersionNumber >= 522)
        {
            BaseId warehouseId = new (reader);
            BaseId itemId = new (reader);
            m_inventoryKey = new SchedulerDefinitions.InventoryKey(warehouseId.Value, itemId.Value);
            m_usingIdKeys = true;
            reader.Read(out requiredQty);
            reader.Read(out needDate);
            requiredPathId = new BaseId(reader);
        }
        else
        {
            /*this.m_itemId = */
            new BaseId(reader);
            reader.Read(out requiredQty);
            reader.Read(out needDate);
            requiredPathId = new BaseId(reader);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_usingIdKeys);
        if (m_usingIdKeys)
        {
            m_inventoryKey.Serialize(a_writer);
        }
        else
        {
            m_inventoryExternalIdKey.Serialize(a_writer);
        }

        a_writer.Write(requiredQty);
        a_writer.Write(needDate);
        requiredPathId.Serialize(a_writer);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public CtpLine(BaseId a_warehouseId, BaseId a_itemId, decimal a_requiredQty, DateTime a_moNeedDate)
    {
        m_usingIdKeys = true;
        m_inventoryKey = new SchedulerDefinitions.InventoryKey(a_warehouseId.Value, a_itemId.Value);
        requiredQty = a_requiredQty;
        needDate = a_moNeedDate;
    }

    public CtpLine(string a_warehouseExternald, string a_itemExternalId, decimal a_requiredQty, DateTime a_moNeedDate)
    {
        m_usingIdKeys = false;
        m_inventoryExternalIdKey = new SchedulerDefinitions.InventoryExternalIdKey(a_warehouseExternald, a_itemExternalId);
        requiredQty = a_requiredQty;
        needDate = a_moNeedDate;
    }

    private readonly bool m_usingIdKeys;

    /// <summary>
    /// whether Inventory is specified using internal Ids. If true, InventoryKey
    /// should be used to find the inventory, otherwise InventoryExternalIdKey
    /// </summary>
    public bool UsingIdKey => m_usingIdKeys;

    private SchedulerDefinitions.InventoryKey m_inventoryKey;
    public SchedulerDefinitions.InventoryKey InventoryKey => m_inventoryKey;

    private SchedulerDefinitions.InventoryExternalIdKey m_inventoryExternalIdKey;
    public SchedulerDefinitions.InventoryExternalIdKey InventoryExternalIdKey => m_inventoryExternalIdKey;

    private decimal requiredQty;

    /// <summary>
    /// The qty of the Item requested.
    /// </summary>
    public decimal RequiredQty
    {
        get => requiredQty;
        set => requiredQty = value;
    }

    private DateTime needDate;

    /// <summary>
    /// The Need Date for the Manufacturing Order corrsponding to the CTP Line.
    /// </summary>
    public DateTime NeedDate
    {
        get => needDate;
        set => needDate = value;
    }

    private BaseId requiredPathId = BaseId.NULL_ID;

    /// <summary>
    /// The Id of the Alternate Path that MUST be used.  NullId if no Required Path was specified by the user.
    /// </summary>
    public BaseId RequiredPathId
    {
        get => requiredPathId;
        set => requiredPathId = value;
    }
}