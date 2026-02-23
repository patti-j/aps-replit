using System.ComponentModel;
using System.Text;

using PT.APSCommon;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule.Demand;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Simulation;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler.Demand;

public partial class SalesOrderLineDistribution : BaseIdObject, IMaterialAllocation
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 641;

    internal SalesOrderLineDistribution(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_reader)
    {
        m_restoreInfo = new RestoreInfo();
        m_simAdjustments = new AdjustmentArray(a_idGen);

        if (a_reader.VersionNumber >= 12551)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_qtyOrdered);
            a_reader.Read(out m_qtyShipped);
            a_reader.Read(out m_requiredAvailableDateTicks);
            a_reader.Read(out m_shipToZone);
            a_reader.Read(out m_salesRegion);
            a_reader.Read(out m_closed);
            a_reader.Read(out m_hold);
            a_reader.Read(out m_holdReason);
            a_reader.Read(out m_priority);
            a_reader.Read(out m_maximumLatenessSpanTicks);
            a_reader.Read(out m_allowPartialAllocations);
            a_reader.Read(out m_minAllocationQty);

            a_reader.Read(out int stockShortageRuleInt);
            m_stockShortageRule = (SalesOrderDefs.StockShortageRules)stockShortageRuleInt;
            m_restoreInfo.wId = new BaseId(a_reader);
            m_eligibleLots = new EligibleLots(a_reader);
            a_reader.Read(out m_actualAvailableTicks);
            a_reader.Read(out int val);
            MaterialAllocation = (ItemDefs.MaterialAllocation)val;
            a_reader.Read(out m_minSourceQty);
            a_reader.Read(out m_maxSourceQty);
            a_reader.Read(out val);
            m_materialSourcing = (ItemDefs.MaterialSourcing)val;
            m_demandAdjustments = new AdjustmentArray(a_reader, a_idGen);
        }
        else if (a_reader.VersionNumber >= 739)
        {
            m_demandAdjustments = new (a_idGen);

            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_qtyOrdered);
            a_reader.Read(out m_qtyShipped);
            a_reader.Read(out m_requiredAvailableDateTicks);
            a_reader.Read(out m_shipToZone);
            a_reader.Read(out m_salesRegion);
            a_reader.Read(out m_closed);
            a_reader.Read(out m_hold);
            a_reader.Read(out m_holdReason);
            a_reader.Read(out m_priority);
            a_reader.Read(out m_maximumLatenessSpanTicks);
            a_reader.Read(out m_allowPartialAllocations);
            a_reader.Read(out m_minAllocationQty);

            a_reader.Read(out int stockShortageRuleInt);
            m_stockShortageRule = (SalesOrderDefs.StockShortageRules)stockShortageRuleInt;
            m_restoreInfo.wId = new BaseId(a_reader);
            m_eligibleLots = new EligibleLots(a_reader);
            a_reader.Read(out m_actualAvailableTicks);
            a_reader.Read(out int val);
            MaterialAllocation = (ItemDefs.MaterialAllocation)val;
            a_reader.Read(out m_minSourceQty);
            a_reader.Read(out m_maxSourceQty);
            a_reader.Read(out val);
            m_materialSourcing = (ItemDefs.MaterialSourcing)val;
        }
    }

    private readonly RestoreInfo m_restoreInfo;

    private class RestoreInfo
    {
        internal BaseId wId;
    }

    internal void RestoreReferences(ScenarioDetail a_sd, WarehouseManager a_warehouseManager, SalesOrderLine a_SOL)
    {
        m_salesOrderLine = a_SOL;
        m_mustSupplyFromWarehouse = a_warehouseManager.GetById(m_restoreInfo.wId);

        //Simulation adjustments are restored from their stored source
        m_demandAdjustments.RestoreReferences(a_sd);
    }

    //Sort the data so we don't have to worry about sort timing when accessing the collection first after loading.
    internal void AfterRestoreAdjustmentReferences()
    {
        m_demandAdjustments.Sort();
        m_simAdjustments.Sort();
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        a_writer.Write(m_qtyOrdered);
        a_writer.Write(m_qtyShipped);
        a_writer.Write(m_requiredAvailableDateTicks);
        a_writer.Write(m_shipToZone);
        a_writer.Write(m_salesRegion);
        a_writer.Write(m_closed);
        a_writer.Write(m_hold);
        a_writer.Write(m_holdReason);
        a_writer.Write(m_priority);
        a_writer.Write(m_maximumLatenessSpanTicks);
        a_writer.Write(m_allowPartialAllocations);
        a_writer.Write(m_minAllocationQty);

        a_writer.Write((int)m_stockShortageRule);

        if (m_mustSupplyFromWarehouse == null)
        {
            BaseId.NULL_ID.Serialize(a_writer);
        }
        else
        {
            m_mustSupplyFromWarehouse.Id.Serialize(a_writer);
        }

        m_eligibleLots.Serialize(a_writer);
        a_writer.Write(m_actualAvailableTicks);

        a_writer.Write((int)MaterialAllocation);
        a_writer.Write(m_minSourceQty);
        a_writer.Write(m_maxSourceQty);
        a_writer.Write((int)m_materialSourcing);
        m_demandAdjustments.Serialize(a_writer);
    }

    private SalesOrderLine m_salesOrderLine;

    public SalesOrderLine SalesOrderLine => m_salesOrderLine;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion IPTSerializable

    internal SalesOrderLineDistribution(string salesOrderExternalId, SalesOrderT.SalesOrder.SalesOrderLine.SalesOrderLineDistribution dis, WarehouseManager warehouses, SalesOrderLine aSOL, BaseId aId, BaseIdGenerator a_idGen)
        : base(aId)
    {
        m_salesOrderLine = aSOL;
        m_demandAdjustments = new (a_idGen);
        m_simAdjustments = new (a_idGen);
        Update(dis, warehouses, salesOrderExternalId, true);
    }

    internal SalesOrderLineDistribution(SalesOrderLine aSOL, BaseId aId, Warehouse aMustSupplyFromWarehouse, decimal aQtyOrdered, decimal aQtyShipped, DateTime aRequiredDate, ItemDefs.MaterialAllocation a_materialAllocation, WarehouseManager aWarehouseManager, HashSet<string> a_eligibleLots, BaseIdGenerator a_idGen)
        : base(aId)
    {
        m_salesOrderLine = aSOL;
        m_mustSupplyFromWarehouse = aMustSupplyFromWarehouse;
        SetNetChangeMrpFlag(aWarehouseManager);
        m_qtyOrdered = aQtyOrdered;
        QtyShipped = aQtyShipped;
        m_requiredAvailableDateTicks = aRequiredDate.Ticks;
        m_eligibleLots.SetEligibleLots(a_eligibleLots, this);
        m_matlAlloc = a_materialAllocation;
        m_demandAdjustments = new (a_idGen);
        m_simAdjustments = new (a_idGen);
    }

    internal void Update(SalesOrderT.SalesOrder.SalesOrderLine.SalesOrderLineDistribution a_tDist, WarehouseManager a_warehouses, string a_salesOrderExternalId, bool a_itemChanged)
    {
        bool salesOrderDistributionChanged = false;
        if (a_tDist.UseMustSupplyFromWarehouseExternalId)
        {
            if (a_tDist.MustSupplyFromWarehouseExternalId == null)
            {
                if (MustSupplyFromWarehouse != null)
                {
                    MustSupplyFromWarehouse = null;
                    salesOrderDistributionChanged = true;
                }
            }
            else
            {
                if (MustSupplyFromWarehouse == null || MustSupplyFromWarehouse.ExternalId != a_tDist.MustSupplyFromWarehouseExternalId)
                {
                    Warehouse whs = a_warehouses.GetByExternalId(a_tDist.MustSupplyFromWarehouseExternalId);
                    if (whs == null)
                    {
                        throw new PTValidationException("2163", new object[] { a_salesOrderExternalId, a_tDist.MustSupplyFromWarehouseExternalId });
                    }

                    MustSupplyFromWarehouse = whs;
                    salesOrderDistributionChanged = true;
                }
            }
        }
        else
        {
            if (MustSupplyFromWarehouse != null)
            {
                MustSupplyFromWarehouse = null;
                salesOrderDistributionChanged = true;
            }
        }

        if (UseMustSupplyFromWarehouse != a_tDist.UseMustSupplyFromWarehouseExternalId)
        {
            UseMustSupplyFromWarehouse = a_tDist.UseMustSupplyFromWarehouseExternalId;
        }

        if (a_itemChanged)
        {
            if (MustSupplyFromWarehouse == null)
            {
                bool itemExists = false;
                foreach (Warehouse warehouse in a_warehouses)
                {
                    if (warehouse.Inventories.Contains(SalesOrderLine.Item.Id))
                    {
                        itemExists = true;
                        break;
                    }
                }

                if (!itemExists)
                {
                    throw new PTValidationException("3004", new object[] { a_salesOrderExternalId, SalesOrderLine.Item.ExternalId });
                }
            }
            else
            {
                if (!MustSupplyFromWarehouse.Inventories.Contains(SalesOrderLine.Item.Id))
                {
                    throw new PTValidationException("2164", new object[] { a_salesOrderExternalId, a_tDist.MustSupplyFromWarehouseExternalId, SalesOrderLine.Item.ExternalId });
                }
            }
        }

        if (AllowPartialAllocations != a_tDist.AllowPartialAllocations)
        {
            AllowPartialAllocations = a_tDist.AllowPartialAllocations;
            salesOrderDistributionChanged = true;
        }

        if (Closed != a_tDist.Closed)
        {
            Closed = a_tDist.Closed;
            salesOrderDistributionChanged = true;
        }

        if (Hold != a_tDist.Hold)
        {
            Hold = a_tDist.Hold;
        }

        if (HoldReason != a_tDist.HoldReason)
        {
            HoldReason = a_tDist.HoldReason;
        }

        if (MaximumLateness != a_tDist.MaximumLateness)
        {
            MaximumLateness = a_tDist.MaximumLateness;
        }

        if (MinAllocationQty != a_tDist.MinAllocationQty)
        {
            MinAllocationQty = a_tDist.MinAllocationQty;
            salesOrderDistributionChanged = true;
        }

        if (Priority != a_tDist.Priority)
        {
            m_priority = a_tDist.Priority;
            salesOrderDistributionChanged = true;
        }

        if (QtyShipped != a_tDist.QtyShipped)
        {
            QtyShipped = a_tDist.QtyShipped;
            salesOrderDistributionChanged = true;
        }

        if (QtyOrdered != a_tDist.QtyOrdered)
        {
            QtyOrdered = a_tDist.QtyOrdered;
            salesOrderDistributionChanged = true;
        }

        if (RequiredAvailableDate.Ticks != a_tDist.RequiredAvailableDate.Ticks)
        {
            RequiredAvailableDate = a_tDist.RequiredAvailableDate;
            salesOrderDistributionChanged = true;
        }

        if (SalesRegion != a_tDist.SalesRegion)
        {
            SalesRegion = a_tDist.SalesRegion;
        }

        if (ShipToZone != a_tDist.ShipToZone)
        {
            ShipToZone = a_tDist.ShipToZone;
        }

        if (StockShortageRule != a_tDist.StockShortageRule)
        {
            StockShortageRule = a_tDist.StockShortageRule;
        }

        m_eligibleLots.Clear();
        foreach (string allowedLotCode in a_tDist.AllowedLotCodes)
        {
            m_eligibleLots.Add(allowedLotCode, this);
        }

        if (MaterialAllocation != a_tDist.MaterialAllocation)
        {
            m_matlAlloc = a_tDist.MaterialAllocation;
            salesOrderDistributionChanged = true;
        }

        if (MinSourceQty != a_tDist.MinSourceQty)
        {
            MinSourceQty = a_tDist.MinSourceQty;
            salesOrderDistributionChanged = true;
        }

        if (MaxSourceQty != a_tDist.MaxSourceQty)
        {
            MaxSourceQty = a_tDist.MaxSourceQty;
            salesOrderDistributionChanged = true;
        }

        if (MaterialSourcing != a_tDist.MaterialSourcing)
        {
            MaterialSourcing = a_tDist.MaterialSourcing;
            salesOrderDistributionChanged = true;
        }

        if (salesOrderDistributionChanged)
        {
            SetNetChangeMrpFlag(a_warehouses);
        }
    }

    internal void SetNetChangeMrpFlag(WarehouseManager aWarehouseManager)
    {
        if (MustSupplyFromWarehouse != null && MustSupplyFromWarehouse.Inventories.Contains(SalesOrderLine.Item.Id))
        {
            Inventory inv = MustSupplyFromWarehouse.Inventories[SalesOrderLine.Item.Id];
            inv.IncludeInNetChangeMRP = true;
            inv.SalesOrdersChangedSinceLastForecast = true;
        }
        else
        {
            for (int w = 0; w < aWarehouseManager.Count; w++)
            {
                Warehouse warehosue = aWarehouseManager.GetByIndex(w);
                if (warehosue.Inventories.Contains(SalesOrderLine.Item.Id))
                {
                    Inventory inv = warehosue.Inventories[SalesOrderLine.Item.Id];
                    inv.IncludeInNetChangeMRP = true;
                    inv.SalesOrdersChangedSinceLastForecast = true;
                }
            }
        }
    }

    #region Shared Properties
    private BoolVector32 m_bools;
    private const short c_useMustSupplyFromWarehouseIdx = 0;

    private Warehouse m_mustSupplyFromWarehouse;

    /// <summary>
    /// Can be used to force the Line to be supplied from the specified Warehouse.
    /// If not specified then the material can come from any Warehouse.
    /// </summary>
    public Warehouse MustSupplyFromWarehouse
    {
        get => m_mustSupplyFromWarehouse;
        private set => m_mustSupplyFromWarehouse = value;
    }

    public bool UseMustSupplyFromWarehouse
    {
        get => m_bools[c_useMustSupplyFromWarehouseIdx];
        set => m_bools[c_useMustSupplyFromWarehouseIdx] = value;
    }

    private decimal m_qtyOrdered;

    /// <summary>
    /// The total qty for this Line Item Distribution on the order.  This remains the same even if there is a partial shipment made.
    /// </summary>
    public decimal QtyOrdered
    {
        get => m_qtyOrdered;
        set => m_qtyOrdered = value;
    }

    private decimal m_qtyShipped;

    /// <summary>
    /// This is the total shipped so far for this distribution.
    /// </summary>
    public decimal QtyShipped
    {
        get => m_qtyShipped;
        set => m_qtyShipped = value;
    }

    /// <summary>
    /// This is the remaining qty that must be planned for.  If a partial shipment is made then this is the QtyOrdered minus partial shipments.
    /// </summary>
    public decimal QtyOpenToShip => Math.Max(0, QtyOrdered - QtyShipped);

    private long m_requiredAvailableDateTicks;

    internal long RequiredAvailableDateTicks
    {
        get => m_requiredAvailableDateTicks;

        private set => m_requiredAvailableDateTicks = value;
    }

    private long m_actualAvailableTicks;

    /// <summary>
    /// When the material is actually available.
    /// </summary>
    public long ActualAvailableTicks
    {
        get => m_actualAvailableTicks;
        set => m_actualAvailableTicks = value;
    }

    /// <summary>
    /// The date when the material must be available in stock in order to reach the customer by the Promised Delivery Date.
    /// </summary>
    public DateTime RequiredAvailableDate
    {
        get => new (RequiredAvailableDateTicks);
        set => RequiredAvailableDateTicks = value.Ticks;
    }

    private string m_shipToZone;

    /// <summary>
    /// Specifies the geographic area where the shipment is going.  For information only.
    /// </summary>
    public string ShipToZone
    {
        get => m_shipToZone;
        set => m_shipToZone = value;
    }

    private string m_salesRegion;

    /// <summary>
    /// The geographic region for the shipment.  For information only.
    /// </summary>
    public string SalesRegion
    {
        get => m_salesRegion;
        set => m_salesRegion = value;
    }

    private bool m_closed;

    /// <summary>
    /// If marked as Closed then it no longer affects planning, regardless of the QtyOpenToShip.
    /// </summary>
    public bool Closed
    {
        get => m_closed;
        set => m_closed = value;
    }

    private bool m_hold;

    /// <summary>
    /// If true then the shipment is ignored in planning.
    /// </summary>
    public bool Hold
    {
        get => m_hold;
        set => m_hold = value;
    }

    private string m_holdReason;

    public string HoldReason
    {
        get => m_holdReason;
        set => m_holdReason = value;
    }

    private int m_priority;

    /// <summary>
    /// Indicates the importance of the shipment.  Used during the allocation process to determine which requirements to allocate to first.
    /// Shipments with lower numbers are considered more important and receive allocation before shipments with higher numbers.
    /// Allocation is based upon the Allocation Rule specified during the time a Simulation is performed.
    /// </summary>
    public int Priority
    {
        get => m_priority;
        set => m_priority = value;
    }

    private SalesOrderDefs.StockShortageRules m_stockShortageRule;

    /// <summary>
    /// Specifies what should be done in stock planning when the shipments full QtyOpenToShip cannot be satisfied.
    /// This can also be overridden during Optimizes with a global rule.
    /// </summary>
    public SalesOrderDefs.StockShortageRules StockShortageRule
    {
        get => m_stockShortageRule;
        set => m_stockShortageRule = value;
    }

    private long m_maximumLatenessSpanTicks;

    internal long MaximumLatenessSpanTicks
    {
        get => m_maximumLatenessSpanTicks;

        private set => m_maximumLatenessSpanTicks = value;
    }

    /// <summary>
    /// If using StockShortageRule of PushLater and the demand has been pushed this amount past the Required Available Date then it is marked as a Missed Sale.
    /// </summary>
    public TimeSpan MaximumLateness
    {
        get => new (MaximumLatenessSpanTicks);
        set => MaximumLatenessSpanTicks = value.Ticks;
    }

    private bool m_allowPartialAllocations;

    /// <summary>
    /// If true then if there is not enough stock to satisfy a shipment, a portion of the requirement can't be met from stock and a portion from
    /// either Backlog or MissedSale.  If false, then the entire qty must be allocated.
    /// </summary>
    public bool AllowPartialAllocations
    {
        get => m_allowPartialAllocations;
        set => m_allowPartialAllocations = value;
    }

    private decimal m_minAllocationQty;

    /// <summary>
    /// If AllowPartialAllocations is true then this is the minimum amount that must be allocated to the shipment.
    /// If this amount is not available then the shipment's
    /// </summary>
    public decimal MinAllocationQty
    {
        get => m_minAllocationQty;
        set => m_minAllocationQty = value;
    }

    private readonly EligibleLots m_eligibleLots = new ();

    /// <summary>
    /// The set of lots eligible to fulfill this distribution. If not specified, any part or lot can be used.
    /// </summary>
    internal EligibleLots EligibleLots => m_eligibleLots;

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

    /// <summary>
    /// Whether the lot can be used to fulfill this material requirement.
    /// </summary>
    /// <param name="a_lot"></param>
    /// <returns></returns>
    public bool IsLotElig(Lot a_lot, object a_data)
    {
        if (a_lot == null && MustUseEligLot)
        {
            return false;
        }

        return m_eligibleLots.IsLotElig(a_lot, (EligibleLots)a_data);
    }

    /// <summary>
    /// Whether the MaterialRequirement must be supplied by the lots specified as Eligible Lots.
    /// </summary>
    public bool MustUseEligLot => m_eligibleLots.Count > 0;

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

    #region Values set during Simulations
    private Warehouse m_supplyingWarehouse;

    /// <summary>
    /// The Warehouse from which the material will be supplied.
    /// </summary>
    public Warehouse SupplyingWarehouse
    {
        get => m_supplyingWarehouse;
        set => m_supplyingWarehouse = value;
    }

    private decimal m_qtyAllocatedFromOnHandInventory;

    /// <summary>
    /// The quantity that is currently planned to be satisfied from OnHand inventory.
    /// </summary>
    public decimal QtyAllocatedFromOnHandInventory
    {
        get => m_qtyAllocatedFromOnHandInventory;
        set => m_qtyAllocatedFromOnHandInventory = value;
    }

    private decimal m_qtyAllocatedFromProjectedInventory;

    /// <summary>
    /// The quantity that is currently planned to be satisfied from future projected inventory. (not including OnHand inventory)
    /// </summary>
    public decimal QtyAllocatedFromProjectedInventory
    {
        get => m_qtyAllocatedFromProjectedInventory;
        set => m_qtyAllocatedFromProjectedInventory = value;
    }
    #endregion Values set during Simulations

    #region Allocation calculations
    /// <summary>
    /// The remaining portion of the QtyOpenToShip that is not currently covered by OnHand or Projected Inventory allocations.
    /// </summary>
    public decimal QtyNotAllocated => QtyOpenToShip - QtyAllocatedFromOnHandInventory - QtyAllocatedFromProjectedInventory;

    /// <summary>
    /// The qty that is not currently planeed to be satisfied and will therefore result in lost sales.
    /// Depends upon the Stock Shortage Rule.
    /// </summary>
    public decimal MissedSaleQty
    {
        get
        {
            if (m_stockShortageRule == SalesOrderDefs.StockShortageRules.MissedSale)
            {
                return QtyNotAllocated;
            }

            return 0;
        }
    }

    /// <summary>
    /// The qty that is not currently planeed to be satisfied and will therefore result in an inventory Backlog (carry-forward of the requirement to future periods).
    /// Depends upon the Stock Shortage Rule.
    /// </summary>
    public decimal BacklogQty
    {
        get
        {
            if (m_stockShortageRule == SalesOrderDefs.StockShortageRules.Backlog)
            {
                return QtyNotAllocated;
            }

            return 0;
        }
    }
    #endregion Allocation calculations

    public void Edit(ScenarioDetail a_sd, SalesOrderLineDistributionEdit a_edit, bool a_itemChanged)
    {
        bool salesOrderDistributionChanged = false;
        if (a_edit.MustSupplyFromWarehouseExternalIdSet)
        {
            if (a_edit.UseMustSupplyFromWarehouseExternalId)
            {
                //TODO: There is no way to set this to null
                if (a_edit.MustSupplyFromWarehouseExternalId == null)
                {
                    if (MustSupplyFromWarehouse != null)
                    {
                        MustSupplyFromWarehouse = null;
                        salesOrderDistributionChanged = true;
                    }
                }
                else
                {
                    if (MustSupplyFromWarehouse == null || MustSupplyFromWarehouse.ExternalId != a_edit.MustSupplyFromWarehouseExternalId)
                    {
                        Warehouse whs = a_sd.WarehouseManager.GetByExternalId(a_edit.MustSupplyFromWarehouseExternalId);
                        if (whs == null)
                        {
                            throw new PTValidationException("2163", new object[] { SalesOrderLine.SalesOrder.ExternalId, a_edit.MustSupplyFromWarehouseExternalId });
                        }

                        MustSupplyFromWarehouse = whs;
                        salesOrderDistributionChanged = true;
                    }
                }
            }
            else
            {
                if (MustSupplyFromWarehouse != null)
                {
                    MustSupplyFromWarehouse = null;
                    salesOrderDistributionChanged = true;
                }
            }
        }

        if (a_itemChanged)
        {
            if (MustSupplyFromWarehouse == null)
            {
                bool itemExists = false;
                foreach (Warehouse warehouse in a_sd.WarehouseManager)
                {
                    if (warehouse.Inventories.Contains(SalesOrderLine.Item.Id))
                    {
                        itemExists = true;
                        break;
                    }

                    if (!itemExists)
                    {
                        throw new PTValidationException("3004", new object[] { SalesOrderLine.SalesOrder.ExternalId, SalesOrderLine.Item.ExternalId });
                    }
                }
            }
            else
            {
                if (!MustSupplyFromWarehouse.Inventories.Contains(SalesOrderLine.Item.Id))
                {
                    throw new PTValidationException("2164", new object[] { SalesOrderLine.SalesOrder.ExternalId, a_edit.MustSupplyFromWarehouseExternalId, SalesOrderLine.Item.ExternalId });
                }
            }
        }

        if (a_edit.AllowPartialAllocationsSet /*AllowPartialAllocations != a_edit.AllowPartialAllocations*/)
        {
            AllowPartialAllocations = a_edit.AllowPartialAllocations;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.ClosedSet /*Closed != a_edit.Closed*/)
        {
            Closed = a_edit.Closed;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.HoldSet /*Hold != a_edit.Hold*/)
        {
            Hold = a_edit.Hold;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.HoldReasonSet /*HoldReason != a_edit.HoldReason*/)
        {
            HoldReason = a_edit.HoldReason;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.MaximumLatenessSet /*MaximumLateness != a_edit.MaximumLateness*/)
        {
            MaximumLateness = a_edit.MaximumLateness;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.MinAllocationQtySet /*MinAllocationQty != a_edit.MinAllocationQty*/)
        {
            MinAllocationQty = a_edit.MinAllocationQty;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.PrioritySet /*Priority != a_edit.Priority*/)
        {
            m_priority = a_edit.Priority;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.QtyShippedSet /*QtyShipped != a_edit.QtyShipped*/)
        {
            QtyShipped = a_edit.QtyShipped;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.QtyOrderedSet /*QtyOrdered != a_edit.QtyOrdered*/)
        {
            QtyOrdered = a_edit.QtyOrdered;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.RequiredAvailableDateSet /*RequiredAvailableDate.Ticks != a_edit.RequiredAvailableDate.Ticks*/)
        {
            RequiredAvailableDate = a_edit.RequiredAvailableDate;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.SalesRegionSet /*SalesRegion != a_edit.SalesRegion*/)
        {
            SalesRegion = a_edit.SalesRegion;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.ShipToZoneSet /*ShipToZone != a_edit.ShipToZone*/)
        {
            ShipToZone = a_edit.ShipToZone;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.StockShortageRuleSet /*StockShortageRule != a_edit.StockShortageRule*/)
        {
            StockShortageRule = a_edit.StockShortageRule;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.AllowedLotCodesSet)
        {
            m_eligibleLots.Clear();
            foreach (string allowedLotCode in a_edit.AllowedLotCodes)
            {
                m_eligibleLots.Add(allowedLotCode, this);
            }
        }


        if (a_edit.MaterialAllocationSet /*MaterialAllocation != a_edit.MaterialAllocation*/)
        {
            m_matlAlloc = a_edit.MaterialAllocation;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.MinSourceQtySet)
        {
            MinSourceQty = a_edit.MinSourceQty;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.MaxSourceQtySet /*MaxSourceQty != a_edit.MaxSourceQty*/)
        {
            MaxSourceQty = a_edit.MaxSourceQty;
            salesOrderDistributionChanged = true;
        }

        if (a_edit.MaterialSourcingSet /*MaterialSourcing != a_edit.MaterialSourcing*/)
        {
            MaterialSourcing = a_edit.MaterialSourcing;
            salesOrderDistributionChanged = true;
        }

        if (salesOrderDistributionChanged)
        {
            SetNetChangeMrpFlag(a_sd.WarehouseManager);
        }
    }

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

    public String Source(ScenarioDetail a_sd)
    {
        Dictionary<(BaseId, BaseId), MRSupply.CondensedSupplyDescription> supplyDescriptions = new();

        if (Closed || QtyOpenToShip == 0)
        {
            //Distribution is finished
            return "";
        }

        foreach (Adjustment adjustment in Adjustments)
        {
            if (adjustment.HasLotStorage)
            {
                //This adjustment pulled from a known source
                Lot sourceLot = adjustment.Storage.Lot;
                BaseId storageAreaId = adjustment.Storage.StorageArea.Id;
                if (sourceLot.Reason is InternalActivity activity)
                {
                    MRSupply.CondensedSupplyDescription desc;
                    if (!supplyDescriptions.TryGetValue((activity.Id, storageAreaId), out desc))
                    {
                        desc = new MRSupply.CondensedSupplyDescription(activity);
                        desc.Add(adjustment);
                        supplyDescriptions.Add((activity.Id, storageAreaId), desc);
                        continue;
                    }

                    desc.Add(adjustment);
                }
                else if (sourceLot.Reason is PurchaseToStock po)
                {
                    if (!supplyDescriptions.TryGetValue((po.Id, storageAreaId), out MRSupply.CondensedSupplyDescription desc))
                    {
                        desc = new MRSupply.CondensedSupplyDescription(po);
                        desc.Add(adjustment);
                        supplyDescriptions.Add((po.Id, storageAreaId), desc);
                        continue;
                    }

                    desc.Add(adjustment);
                }
                else if (sourceLot.Reason is TransferOrderDistribution orderDemand)
                {
                    if (!supplyDescriptions.TryGetValue((orderDemand.Id, storageAreaId), out MRSupply.CondensedSupplyDescription tODesc))
                    {
                        tODesc = new MRSupply.CondensedSupplyDescription(orderDemand);
                        tODesc.Add(adjustment);
                        supplyDescriptions.Add((orderDemand.Id, storageAreaId), tODesc);
                        continue;
                    }

                    tODesc.Add(adjustment);
                }
                else
                {
                    Scheduler.Inventory inv = adjustment.Inventory;

                    MRSupply.CondensedSupplyDescription desc;
                    if (!supplyDescriptions.TryGetValue((inv.Id, storageAreaId), out desc))
                    {
                        desc = new MRSupply.CondensedSupplyDescription(inv);
                        desc.Add(adjustment);
                        supplyDescriptions.Add((inv.Id, storageAreaId), desc);
                        continue;
                    }

                    desc.Add(adjustment);
                }
            }
            else
            {
                //Could be leadtime
                if (adjustment is MaterialRequirementLeadTimeAdjustment)
                {
                    Scheduler.Inventory inv = adjustment.Inventory;

                    MRSupply.CondensedSupplyDescription desc;
                    if (!supplyDescriptions.TryGetValue((inv.Id, BaseId.NULL_ID), out desc))
                    {
                        desc = new MRSupply.CondensedSupplyDescription(inv);
                        desc.Add(adjustment);
                        supplyDescriptions.Add((inv.Id, BaseId.NULL_ID), desc);
                        continue;
                    }

                    desc.Add(adjustment);
                }
            }
        }

        StringBuilder sb = new();

        supplyDescriptions.Values.OrderByDescending(d => d.FirstTime).ToList().ForEach(desc =>
        {
            if (sb.Length > 0)
            {
                sb.Append("; ");
            }
            sb.Append(desc.GetDescription());
        });

        return sb.ToString();
    }

    public DateTime EarliestShipDate(ScenarioDetail a_sd)
    {
        DateTime earliestShipDate = PTDateTime.MaxDateTime;

        if (Closed || QtyOpenToShip == 0)
        {
            //Distribution is finished
            return earliestShipDate;
        }

        foreach (SalesOrderAdjustment adjustment in Adjustments)
        {
            if (Math.Abs(adjustment.ChangeQty) == adjustment.Distribution.QtyOpenToShip && adjustment.AdjDate < earliestShipDate)
            {
                earliestShipDate = adjustment.AdjDate;
            }
        }

        return earliestShipDate;
    }
}