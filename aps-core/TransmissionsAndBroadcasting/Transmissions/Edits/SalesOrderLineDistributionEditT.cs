using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

public class SalesOrderLineDistributionEdit : PTObjectBaseEdit, IPTSerializable
{
    #region PT Serialization
    public SalesOrderLineDistributionEdit(IReader a_reader)
        : base(a_reader)
    {
        #region 12000
        if (a_reader.VersionNumber >= 12000)
        {
            SalesOrderLineDistributionId = new BaseId(a_reader);
            SalesOrderLineId = new BaseId(a_reader);
            SalesOrderId = new BaseId(a_reader);

            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out m_qtyOrdered);
            a_reader.Read(out m_qtyShipped);
            a_reader.Read(out m_requiredAvailableDate);
            a_reader.Read(out m_shipToZone);
            a_reader.Read(out m_salesRegion);
            a_reader.Read(out m_closed);
            a_reader.Read(out m_hold);
            a_reader.Read(out m_holdReason);
            a_reader.Read(out m_priority);
            a_reader.Read(out m_maximumLateness);
            a_reader.Read(out m_allowPartialAllocations);
            a_reader.Read(out m_minAllocationQty);

            a_reader.Read(out int stockShortageRuleInt);
            m_stockShortageRule = (SalesOrderDefs.StockShortageRules)stockShortageRuleInt;
            a_reader.Read(out m_mustSupplyFromWarehouseExternalId);
            a_reader.Read(out int val);
            for (int i = 0; i < val; i++)
            {
                a_reader.Read(out string lotCode);
                m_allowedLotCodes.Add(lotCode);
            }

            a_reader.Read(out val);
            m_materialAllocation = (ItemDefs.MaterialAllocation)val;
            a_reader.Read(out m_minSourceQty);
            a_reader.Read(out m_maxSourceQty);
            a_reader.Read(out val);
            m_materialSourcing = (ItemDefs.MaterialSourcing)val;
        }
        #endregion
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        SalesOrderLineDistributionId.Serialize(a_writer);
        SalesOrderLineId.Serialize(a_writer);
        SalesOrderId.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_setBools.Serialize(a_writer);

        a_writer.Write(m_qtyOrdered);
        a_writer.Write(m_qtyShipped);
        a_writer.Write(m_requiredAvailableDate);
        a_writer.Write(m_shipToZone);
        a_writer.Write(m_salesRegion);
        a_writer.Write(m_closed);
        a_writer.Write(m_hold);
        a_writer.Write(m_holdReason);
        a_writer.Write(m_priority);
        a_writer.Write(m_maximumLateness);
        a_writer.Write(m_allowPartialAllocations);
        a_writer.Write(m_minAllocationQty);

        a_writer.Write((int)m_stockShortageRule);
        a_writer.Write(m_mustSupplyFromWarehouseExternalId);
        a_writer.Write(m_allowedLotCodes.Count);
        foreach (string allowedLotCode in m_allowedLotCodes)
        {
            a_writer.Write(allowedLotCode);
        }

        a_writer.Write((int)m_materialAllocation);
        a_writer.Write(m_minSourceQty);
        a_writer.Write(m_maxSourceQty);
        a_writer.Write((int)m_materialSourcing);
    }

    public new int UniqueId => 1047;
    #endregion

    public SalesOrderLineDistributionEdit(BaseId a_salesOrderId, BaseId a_salesOrderLineId, BaseId a_salesOrderLineDistributionId)
    {
        SalesOrderLineDistributionId = a_salesOrderLineDistributionId;
        SalesOrderLineId = a_salesOrderLineId;
        SalesOrderId = a_salesOrderId;
    }

    public override bool HasEdits => base.HasEdits || m_setBools.AnyFlagsSet;

    #region Shared Properties
    public readonly BaseId SalesOrderLineDistributionId;
    public readonly BaseId SalesOrderLineId;
    public readonly BaseId SalesOrderId;

    private BoolVector32 m_bools;
    private const int c_useMustSupplyFromWarehouseExternalIdIdx = 0;

    private BoolVector32 m_setBools;

    private const int c_mustSupplyFromWarehouseExternalIdIsSetIdx = 0;
    private const int c_useMustSupplyFromWarehouseExternalIdIsSetIdx = 1;
    private const int c_qtyOrderedIsSetIdx = 2;
    private const int c_qtyShippedSetIdx = 3;
    private const int c_minSourceQtySetIdx = 4;
    private const int c_maxSourceQtySetIdx = 5;
    private const int c_materialSourcingSetIdx = 6;
    private const int c_requiredAvailableDateSetIdx = 7;
    private const int c_shipToZoneSetIdx = 8;
    private const int c_salesRegionSetIdx = 9;
    private const int c_closedSetIdx = 10;
    private const int c_holdSetIdx = 11;
    private const int c_holdReasonSetIdx = 12;
    private const int c_prioritySetIdx = 13;
    private const int c_stockShortageRuleSetIdx = 14;
    private const int c_maximumLatenessSetIdx = 15;
    private const int c_allowPartialAllocationsSetIdx = 16;
    private const int c_minAllocationQtySetIdx = 17;
    private const int c_allowedLotCodesSetIdx = 18;
    private const int c_materialAllocationSetIdx = 19;

    private string m_mustSupplyFromWarehouseExternalId;

    /// <summary>
    /// The demand must be satisfied by the specified Warehouse only.
    /// </summary>
    public string MustSupplyFromWarehouseExternalId
    {
        get => m_mustSupplyFromWarehouseExternalId;
        set
        {
            m_mustSupplyFromWarehouseExternalId = value;
            m_setBools[c_mustSupplyFromWarehouseExternalIdIsSetIdx] = true;
        }
    }

    public bool MustSupplyFromWarehouseExternalIdSet => m_setBools[c_mustSupplyFromWarehouseExternalIdIsSetIdx];

    public bool UseMustSupplyFromWarehouseExternalId
    {
        get => m_bools[c_useMustSupplyFromWarehouseExternalIdIdx];
        set
        {
            m_bools[c_useMustSupplyFromWarehouseExternalIdIdx] = value;
            m_setBools[c_useMustSupplyFromWarehouseExternalIdIsSetIdx] = true;
        }
    }

    public bool UseMustSupplyFromWarehouseExternalIdSet => m_setBools[c_useMustSupplyFromWarehouseExternalIdIsSetIdx];

    private decimal m_qtyOrdered;

    /// <summary>
    /// The total qty for this Line Item Distribution on the order.  This remains the same even if there is a partial shipment made.
    /// </summary>
    public decimal QtyOrdered
    {
        get => m_qtyOrdered;
        set
        {
            m_qtyOrdered = value;
            m_setBools[c_qtyOrderedIsSetIdx] = true;
        }
    }

    public bool QtyOrderedSet => m_setBools[c_qtyOrderedIsSetIdx];

    private decimal m_qtyShipped;

    /// <summary>
    /// This is the remaining qty that must be planned for.  If a partial shipment is made then this is the QtyOrdered minus partial shipments.
    /// </summary>
    public decimal QtyShipped
    {
        get => m_qtyShipped;
        set
        {
            m_qtyShipped = value;
            m_setBools[c_qtyShippedSetIdx] = true;
        }
    }

    public bool QtyShippedSet => m_setBools[c_qtyShippedSetIdx];

    private decimal m_minSourceQty;

    public decimal MinSourceQty
    {
        get => m_minSourceQty;
        set
        {
            m_minSourceQty = value;
            m_setBools[c_minSourceQtySetIdx] = true;
        }
    }

    public bool MinSourceQtySet => m_setBools[c_minSourceQtySetIdx];

    private decimal m_maxSourceQty;

    public decimal MaxSourceQty
    {
        get => m_maxSourceQty;
        set
        {
            m_maxSourceQty = value;
            m_setBools[c_maxSourceQtySetIdx] = true;
        }
    }

    public bool MaxSourceQtySet => m_setBools[c_maxSourceQtySetIdx];

    private ItemDefs.MaterialSourcing m_materialSourcing = ItemDefs.MaterialSourcing.NotSet;

    public ItemDefs.MaterialSourcing MaterialSourcing
    {
        get => m_materialSourcing;
        set
        {
            m_materialSourcing = value;
            m_setBools[c_materialSourcingSetIdx] = true;
        }
    }

    public bool MaterialSourcingSet => m_setBools[c_materialSourcingSetIdx];

    private DateTime m_requiredAvailableDate;

    /// <summary>
    /// The date when the material must be available in stock in order to reach the customer by the Promised Delivery Date.
    /// </summary>
    public DateTime RequiredAvailableDate
    {
        get => m_requiredAvailableDate;
        set
        {
            m_requiredAvailableDate = value;
            m_setBools[c_requiredAvailableDateSetIdx] = true;
        }
    }

    public bool RequiredAvailableDateSet => m_setBools[c_requiredAvailableDateSetIdx];

    private string m_shipToZone;

    /// <summary>
    /// Specifies the geographic are where the shipment is going.  For information only.
    /// </summary>
    public string ShipToZone
    {
        get => m_shipToZone;
        set
        {
            m_shipToZone = value;
            m_setBools[c_shipToZoneSetIdx] = true;
        }
    }

    public bool ShipToZoneSet => m_setBools[c_shipToZoneSetIdx];

    private string m_salesRegion;

    /// <summary>
    /// The geographic region for the shipment.  For information only.
    /// </summary>
    public string SalesRegion
    {
        get => m_salesRegion;
        set
        {
            m_salesRegion = value;
            m_setBools[c_salesRegionSetIdx] = true;
        }
    }

    public bool SalesRegionSet => m_setBools[c_salesRegionSetIdx];

    private bool m_closed;

    /// <summary>
    /// If marked as Closed then the it no longer affects planning, regardless of the QtyOpenToShip.
    /// </summary>
    public bool Closed
    {
        get => m_closed;
        set
        {
            m_closed = value;
            m_setBools[c_closedSetIdx] = true;
        }
    }

    public bool ClosedSet => m_setBools[c_closedSetIdx];

    private bool m_hold;

    /// <summary>
    /// If true then the shipment is ignored in planning.
    /// </summary>
    public bool Hold
    {
        get => m_hold;
        set
        {
            m_hold = value;
            m_setBools[c_holdSetIdx] = true;
        }
    }

    public bool HoldSet => m_setBools[c_holdSetIdx];

    private string m_holdReason;

    public string HoldReason
    {
        get => m_holdReason;
        set
        {
            m_holdReason = value;
            m_setBools[c_holdReasonSetIdx] = true;
        }
    }

    public bool HoldReasonSet => m_setBools[c_holdReasonSetIdx];

    private int m_priority;

    /// <summary>
    /// Indicates the importance of the shipment.  Used during the allocation process to determine which requirements to allocate to first.
    /// Shipments with lower numbers are considered more important and receive allocation before shipments with higher numbers.
    /// Allocation is based upon the Allocation Rule specified during the time a Simulation is performed.
    /// </summary>
    public int Priority
    {
        get => m_priority;
        set
        {
            m_priority = value;
            m_setBools[c_prioritySetIdx] = true;
        }
    }

    public bool PrioritySet => m_setBools[c_prioritySetIdx];

    private SalesOrderDefs.StockShortageRules m_stockShortageRule;

    /// <summary>
    /// Specifies what should be done in stock planning when the shipments full QtyOpenToShip cannot be satisfied.
    /// This can also be overridden during Optimizes with a global rule.
    /// </summary>
    public SalesOrderDefs.StockShortageRules StockShortageRule
    {
        get => m_stockShortageRule;
        set
        {
            m_stockShortageRule = value;
            m_setBools[c_stockShortageRuleSetIdx] = true;
        }
    }

    public bool StockShortageRuleSet => m_setBools[c_stockShortageRuleSetIdx];

    private TimeSpan m_maximumLateness;

    /// <summary>
    /// If using StockShortageRule of PushLater and the demand has been pushed this amount past the Required Available Date then it is marked as a Missed Sale.
    /// </summary>
    public TimeSpan MaximumLateness
    {
        get => m_maximumLateness;
        set
        {
            m_maximumLateness = value;
            m_setBools[c_maximumLatenessSetIdx] = true;
        }
    }

    public bool MaximumLatenessSet => m_setBools[c_maximumLatenessSetIdx];

    private bool m_allowPartialAllocations;

    /// <summary>
    /// If true then if there is not enough stock to satisfy a shipment, a portion of the requirement can't be met from stock and a portion from
    /// either Backlog or MissedSale.  If false, then the entire qty must be allocated.
    /// </summary>
    public bool AllowPartialAllocations
    {
        get => m_allowPartialAllocations;
        set
        {
            m_allowPartialAllocations = value;
            m_setBools[c_allowPartialAllocationsSetIdx] = true;
        }
    }

    public bool AllowPartialAllocationsSet => m_setBools[c_allowPartialAllocationsSetIdx];

    private decimal m_minAllocationQty;

    /// <summary>
    /// If AllowPartialAllocations is true then this is the minimum amount that must be allocated to the shipment.
    /// If this amount is not available then the shipment's
    /// </summary>
    public decimal MinAllocationQty
    {
        get => m_minAllocationQty;
        set
        {
            m_minAllocationQty = value;
            m_setBools[c_minAllocationQtySetIdx] = true;
        }
    }

    public bool MinAllocationQtySet => m_setBools[c_minAllocationQtySetIdx];

    private List<string> m_allowedLotCodes = new ();

    public List<string> AllowedLotCodes
    {
        get => m_allowedLotCodes;
        set
        {
            m_allowedLotCodes = value ?? new List<string>();
            m_setBools[c_allowedLotCodesSetIdx] = true;
        }
    }

    public bool AllowedLotCodesSet => m_setBools[c_allowedLotCodesSetIdx];

    private ItemDefs.MaterialAllocation m_materialAllocation = ItemDefs.MaterialAllocation.NotSet;

    /// <summary>
    /// How material should be used either use the earliest created or the latest created.
    /// </summary>
    public ItemDefs.MaterialAllocation MaterialAllocation
    {
        get => m_materialAllocation;
        set
        {
            m_materialAllocation = value;
            m_setBools[c_materialAllocationSetIdx] = true;
        }
    }

    public bool MaterialAllocationSet => m_setBools[c_materialAllocationSetIdx];
    #endregion Shared Properties
}