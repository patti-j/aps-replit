using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

public class PurchaseToStockEditT : ScenarioIdBaseT, IPTSerializable, IEnumerable<PurchaseToStockEdit>
{
    #region PT Serialization
    private readonly List<PurchaseToStockEdit> m_poEdits = new ();
    public static int UNIQUE_ID => 1042;

    public PurchaseToStockEditT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                PurchaseToStockEdit node = new (a_reader);
                m_poEdits.Add(node);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_poEdits);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public PurchaseToStockEditT() { }
    public PurchaseToStockEditT(BaseId a_scenarioId) : base(a_scenarioId) { }

    public PurchaseToStockEdit this[int i] => m_poEdits[i];

    public void Validate()
    {
        foreach (PurchaseToStockEdit poEdit in m_poEdits)
        {
            poEdit.Validate();
        }
    }

    public override string Description => string.Format("Purchase orders updated ({0})".Localize(), m_poEdits.Count);

    public IEnumerator<PurchaseToStockEdit> GetEnumerator()
    {
        return m_poEdits.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(PurchaseToStockEdit a_pts)
    {
        m_poEdits.Add(a_pts);
    }
}

/// <summary>
/// A standard Item to be purchased for stock.  The received Item will go to stock for use by any Job requiring the Item.
/// </summary>
public class PurchaseToStockEdit : PTObjectBaseEdit, IPTSerializable
{
    #region PT Serialization
    public PurchaseToStockEdit(IReader a_reader)
        : base(a_reader)
    {
        #region 12530
        if (a_reader.VersionNumber >= 12530)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out m_qtyOrdered);
            a_reader.Read(out m_qtyReceived);
            a_reader.Read(out m_scheduledReceiptDate);
            a_reader.Read(out m_buyerExternalId);
            a_reader.Read(out m_vendorExternalId);

            a_reader.Read(out m_itemExternalId);
            a_reader.Read(out m_warehouseExternalId);

            a_reader.Read(out m_unloadSpan);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out int tmp);
            m_maintenanceMethod = (PurchaseToStockDefs.EMaintenanceMethod)tmp;
            a_reader.Read(out m_lotCode);
            a_reader.Read(out m_actualReceiptDate);
            a_reader.Read(out m_storageAreaExternalId);
        }
        #endregion
        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out m_qtyOrdered);
            a_reader.Read(out m_qtyReceived);
            a_reader.Read(out m_scheduledReceiptDate);
            a_reader.Read(out m_buyerExternalId);
            a_reader.Read(out m_vendorExternalId);

            a_reader.Read(out m_itemExternalId);
            a_reader.Read(out m_warehouseExternalId);

            a_reader.Read(out m_unloadSpan);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out int tmp);
            m_maintenanceMethod = (PurchaseToStockDefs.EMaintenanceMethod)tmp;
            a_reader.Read(out m_lotCode);
            a_reader.Read(out m_actualReceiptDate);
        }
        #endregion
    }

    public new void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_bools.Serialize(writer);
        m_setBools.Serialize(writer);

        writer.Write(m_qtyOrdered);
        writer.Write(m_qtyReceived);
        writer.Write(m_scheduledReceiptDate);
        writer.Write(m_buyerExternalId);
        writer.Write(m_vendorExternalId);

        writer.Write(m_itemExternalId);
        writer.Write(m_warehouseExternalId);

        writer.Write(m_unloadSpan);
        writer.Write(m_transferSpan);
        writer.Write((int)m_maintenanceMethod);
        writer.Write(m_lotCode);
        writer.Write(m_actualReceiptDate);
        writer.Write(m_storageAreaExternalId);
    }

    public new int UniqueId => 1042;
    #endregion

    public PurchaseToStockEdit(BaseId a_poId)
    {
        Id = a_poId;
        m_externalId = null; //Clear other id without triggering IsSet
    }

    public PurchaseToStockEdit(string a_externalId)
    {
        ExternalId = a_externalId;
        m_id = BaseId.NULL_ID;
    }

    #region Shared Properties
    private long m_scheduledReceiptDate;

    public DateTime ScheduledReceiptDate
    {
        get => new (m_scheduledReceiptDate);
        set
        {
            m_scheduledReceiptDate = value.Ticks;
            m_setBools[c_scheduledReceiptDateSetIdx] = true;
        }
    }

    private long m_actualReceiptDate;

    public DateTime ActualReceiptDate
    {
        get => new (m_actualReceiptDate);
        set
        {
            m_actualReceiptDate = value.Ticks;
            m_setBools[c_actualReceiptDateSetIdx] = true;
        }
    }

    private decimal m_qtyOrdered;

    public decimal QtyOrdered
    {
        get => m_qtyOrdered;
        set
        {
            m_qtyOrdered = value;
            m_setBools[c_qtyOrderedSetIdx] = true;
        }
    }

    private decimal m_qtyReceived;

    public decimal QtyReceived
    {
        get => m_qtyReceived;
        set
        {
            m_qtyReceived = value;
            m_setBools[c_qtyReceivedSetIdx] = true;
        }
    }

    private string m_vendorExternalId;

    /// <summary>
    /// The company the parts are ordered from.
    /// </summary>
    public string VendorExternalId
    {
        get => m_vendorExternalId;
        set
        {
            m_vendorExternalId = value;
            m_setBools[c_vendorExternalIdSetIdx] = true;
        }
    }

    private string m_buyerExternalId;

    /// <summary>
    /// The individual responsible for this purchase.
    /// </summary>
    public string BuyerExternalId
    {
        get => m_buyerExternalId;
        set
        {
            m_buyerExternalId = value;
            m_setBools[c_buyerExternalIdSetIdx] = true;
        }
    }

    private string m_warehouseExternalId;

    /// <summary>
    /// The Warehouse where the items will be delivered when received.
    /// </summary>
    [Required(true)]
    public string WarehouseExternalId
    {
        get => m_warehouseExternalId;
        set
        {
            m_warehouseExternalId = value;
            m_setBools[c_warehouseExternalIdSetIdx] = true;
        }
    }

    private TimeSpan m_unloadSpan;

    /// <summary>
    /// If scheduling Docks, this is used to specify the amount of time it will take to unload the items.
    /// </summary>
    public TimeSpan UnloadSpan
    {
        get => m_unloadSpan;
        set
        {
            m_unloadSpan = value;
            m_setBools[c_unloadSpanSetIdx] = true;
        }
    }

    private TimeSpan m_transferSpan;

    /// <summary>
    /// Material is not considered usable in production until this time has passed after the Scheduled Receipt Date.
    /// </summary>
    public TimeSpan TransferSpan
    {
        get => m_transferSpan;
        set
        {
            m_transferSpan = value;
            m_setBools[c_transferSpanSetIdx] = true;
        }
    }

    private string m_itemExternalId;

    /// <summary>
    /// The Item to be ordered.
    /// </summary>
    public string ItemExternalId
    {
        get => m_itemExternalId;
        set
        {
            m_itemExternalId = value;
            m_setBools[c_itemExternalIdSetIdx] = true;
        }
    }
    private string m_storageAreaExternalId;
    public string StorageAreaExternalId
    {
        get => m_storageAreaExternalId;
        set
        {
            m_storageAreaExternalId = value;
            m_setBools[c_storageAreaExternalIdSetIdx] = true;
        }
    }
    private BoolVector32 m_bools;
    private const short c_firmIdx = 0;
    private const short c_closedIdx = 1;
    private const short c_overrideStorageConstraintIdx = 2;
    private const short c_requireEmptyStorageAreaIdx = 3;

    private BoolVector32 m_setBools;
    private const short c_firmSetIdx = 1;
    private const short c_closedSetIdx = 2;
    private const short c_transferSpanSetIdx = 3;
    private const short c_unloadSpanSetIdx = 4;
    private const short c_scheduledReceiptDateSetIdx = 5;
    private const short c_actualReceiptDateSetIdx = 6;
    private const short c_qtyOrderedSetIdx = 7;
    private const short c_qtyReceivedSetIdx = 8;
    private const short c_lotCodeSetIdx = 9;
    private const short c_itemExternalIdSetIdx = 10;
    private const short c_warehouseExternalIdSetIdx = 11;
    private const short c_vendorExternalIdSetIdx = 12;
    private const short c_buyerExternalIdSetIdx = 13;
    private const short c_maintenanceMethodSetIdx = 14;
    private const short c_overrideStorageConstraintSetIdx = 15;
    private const short c_requireEmptyStorageAreaSetIdx = 16;
    private const short c_storageAreaExternalIdSetIdx = 17;

    public bool FirmSet => m_setBools[c_firmSetIdx];
    public bool ClosedSet => m_setBools[c_closedSetIdx];
    public bool TransferSpanSet => m_setBools[c_transferSpanSetIdx];
    public bool UnloadSpanSet => m_setBools[c_unloadSpanSetIdx];
    public bool ScheduledReceiptDateSet => m_setBools[c_scheduledReceiptDateSetIdx];
    public bool ActualReceiptDateSet => m_setBools[c_actualReceiptDateSetIdx];
    public bool QtyOrderedSet => m_setBools[c_qtyOrderedSetIdx];
    public bool QtyReceivedSet => m_setBools[c_qtyReceivedSetIdx];
    public bool LotCodeSet => m_setBools[c_lotCodeSetIdx];
    public bool ItemExternalIdSet => m_setBools[c_itemExternalIdSetIdx];
    public bool WarehouseExternalIdSet => m_setBools[c_warehouseExternalIdSetIdx];
    public bool VendorExternalIdSet => m_setBools[c_vendorExternalIdSetIdx];
    public bool BuyerExternalIdSet => m_setBools[c_buyerExternalIdSetIdx];
    public bool MaintenanceMethodSet => m_setBools[c_maintenanceMethodSetIdx];
    public bool OverrideStorageConstraintSet => m_setBools[c_overrideStorageConstraintSetIdx];
    public bool RequireEmptyStorageAreaSet => m_setBools[c_requireEmptyStorageAreaSetIdx];
    public bool StorageAreaExternalIdSet => m_setBools[c_storageAreaExternalIdSetIdx];

    public bool RequireEmptyStorageArea
    {
        get => m_bools[c_requireEmptyStorageAreaIdx];
        set
        {
            m_bools[c_requireEmptyStorageAreaIdx] = value;
            m_setBools[c_requireEmptyStorageAreaSetIdx] = true;
        }
    }
    /// <summary>
    /// If the Purchase is Firm then the MRP logic will not modify or delete it.
    /// Users can still change Firm Purchases and imports can affect them.
    /// </summary>
    public bool Firm
    {
        get => m_bools[c_firmIdx];
        set
        {
            m_bools[c_firmIdx] = value;
            m_setBools[c_firmSetIdx] = true;
        }
    }
    /// <summary>
    /// Whether this material will store in excess of the storage areas max quantity when received.
    /// If false, any material that can't be stored will be discarded.
    /// </summary>
    public bool OverrideStorageConstraint
    {
        get => m_bools[c_overrideStorageConstraintIdx];
        set
        {
            m_bools[c_overrideStorageConstraintIdx] = value;
            m_setBools[c_overrideStorageConstraintSetIdx] = true;
        }
    }

    /// <summary>
    /// If true then the Purchase has no effect on the plan.
    /// </summary>
    public bool Closed
    {
        get => m_bools[c_closedIdx];
        set
        {
            m_bools[c_closedIdx] = value;
            m_setBools[c_closedSetIdx] = true;
        }
    }

    private PurchaseToStockDefs.EMaintenanceMethod m_maintenanceMethod = PurchaseToStockDefs.EMaintenanceMethod.Manual;

    public PurchaseToStockDefs.EMaintenanceMethod MaintenanceMethod
    {
        get => m_maintenanceMethod;
        set => m_maintenanceMethod = value;
    }

    private string m_lotCode;

    public string LotCode
    {
        get => m_lotCode;
        set
        {
            m_lotCode = value;
            m_setBools[c_lotCodeSetIdx] = true;
        }
    }
    #endregion Shared Properties

    public override bool HasEdits => base.HasEdits || m_setBools.AnyFlagsSet;

    public void Validate()
    {
        if (ItemExternalIdSet && string.IsNullOrEmpty(ItemExternalId))
        {
            throw new ValidationException("2115");
        }

        if (WarehouseExternalIdSet && string.IsNullOrEmpty(WarehouseExternalId))
        {
            throw new ValidationException("2116");
        }

        if (QtyOrderedSet && QtyOrdered <= 0 && QtyReceivedSet && QtyReceived <= 0)
        {
            throw new ValidationException("2936");
        }
    }
}