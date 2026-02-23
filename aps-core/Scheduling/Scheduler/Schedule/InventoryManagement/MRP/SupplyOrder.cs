using PT.Scheduler.MRP;

namespace PT.Scheduler.Schedule.InventoryManagement.MRP;

/// <summary>
/// This is an (intermediary) output of Mrp. SupplyOrder is a demand (or justification) for Job/PO to
/// be created and should define the necessary parameters for doing so.
/// The main motivation for this is to avoid adjusting (up or down) Mrp generated Job Qtys.
/// SupplyOrder is for creating one Job/PO only so when batching it's final SupplyQty should be adjusted
/// to meet the BatchSize, MinNbrBatches, MaxNbrBatches, etc...
/// </summary>
public class SupplyOrder
{
    private decimal m_totalDemandQty; // the actual sum of all demand qtys that have been added here
    private decimal m_allocated; // the amount that's allocated to future demand which can't be directly added here
    private decimal m_supplyGenerated; // the quantity that was created for this order

    private readonly Inventory m_inventory;
    public Inventory Inventory => m_inventory;

    private readonly List<DemandPart> m_demandParts = new ();

    /// <summary>
    /// A list of DemandParts (adjustment and Qty)
    /// </summary>
    public List<DemandPart> DemandParts => m_demandParts;

    private long m_needDateTicks = PTDateTime.MaxDateTime.Ticks;

    /// <summary>
    /// the earliest demand date
    /// </summary>
    public long NeedDateTicks => m_needDateTicks;

    private int m_priority = int.MaxValue;

    /// <summary>
    /// highest (smallest integer value) priority amongst demands.
    /// </summary>
    public int Priority => m_priority;

    public bool IsEmpty => m_demandParts.Count == 0;

    public SupplyOrder(Inventory a_inv)
    {
        m_inventory = a_inv;
    }

    /// <summary>
    /// demand with adjustment date greater than this cannot be added to this supply order.
    /// </summary>
    /// <returns></returns>
    public long GetMaxDemandDateTicks()
    {
        long end = PTDateTime.MaxDateTime.Ticks;
        if (m_demandParts.Count > 0 && m_inventory.Item.CanBatch)
        {
            return Math.Min(PTDateTime.MaxDateTime.Ticks, m_inventory.Item.BatchWindowTicks + m_needDateTicks);
        }

        return end;
    }

    internal bool IsOutSideBatchWindow(DateTime a_adjDate)
    {
        if (m_demandParts.Count == 0)
        {
            return false; // no window has been established, it's inside
        }

        return m_inventory.Item.BatchWindowTicks <= 0 || a_adjDate.Ticks < m_needDateTicks || a_adjDate.Ticks > GetMaxDemandDateTicks();
    }

    internal decimal QuantityToAccept(ScenarioDetail a_sd, MrpDemand a_demand, DateTime a_adjustmentDate, decimal a_qty, bool a_allowPartialSupply)
    {
        a_qty = Math.Abs(a_qty);

        if (!m_batchingClosed)
        {
            //This order hasn't been closed yet, see if we can accept more within the window.
            if (IsOutSideBatchWindow(a_adjustmentDate))
            {
                return 0;
            }
        }

        if (a_sd.ExtensionController.RunMrpExtension)
        {
            bool? canBatch = a_sd.ExtensionController.CanBatch(a_sd, this, a_demand, a_qty, a_allowPartialSupply);
            if (canBatch.HasValue && !canBatch.Value)
            {
                return 0;
            }
        }

        decimal availableQty = GetAvailableQty(a_sd, a_demand);

        if (m_inventory.PreventSharedBatchOverflow)
        {
            if (BatchOverflow)
            {
                //This batch already has a demand that wasn't fully satisfied from another batch. Don't add another batch to it.
                return 0;
            }

            //This batch would not satisfy the entire demand and we would get split usage.
            if (m_demandParts.Count > 0 && a_qty > availableQty)
            {
                //Don't use this batch, make a new one
                return 0;
            }
        }

        decimal qtyToAccept = Math.Min(a_qty, availableQty);
        if (qtyToAccept <= 0)
        {
            return 0; // can't accept it at all.
        }

        if (!a_allowPartialSupply && qtyToAccept < a_qty) // can't accept partial
        {
            //TODO: Find a better way to show this message, this spot can produce false warnings.
            //a_mrpWarnings.AddPartialSupplyViolation(a_adj);
            return 0;
        }

        return qtyToAccept;
    }

    /// <summary>
    /// Add an adjustment to this supply qty.
    /// </summary>
    /// <param name="a_demand"></param>
    /// <returns>Qty that was requested minus Qty that was accepted (what remains)</returns>
    internal decimal AddDemand(ScenarioDetail a_sd, MrpDemand a_demand, DateTime a_adjustmentDate, decimal a_qty, bool a_allowPartialSupply, bool a_useJitAdjUsage, ScenarioDetail.MrpWarnings a_mrpWarnings)
    {
        decimal qtyToAccept = QuantityToAccept(a_sd, a_demand, a_adjustmentDate, a_qty, a_allowPartialSupply);
        if (qtyToAccept > 0m)
        {
            m_totalDemandQty = m_totalDemandQty + qtyToAccept;
            m_needDateTicks = Math.Min(a_adjustmentDate.Ticks, m_needDateTicks);
            m_priority = Math.Min(a_demand.Priority, m_priority);
            m_demandParts.Add(new DemandPart(a_demand, qtyToAccept));
        }

        return Math.Max(0, a_qty - qtyToAccept);
    }

    /// <summary>
    /// Demand with qty greater than this cannot be added to this supply order.
    /// </summary>
    /// <returns></returns>
    internal decimal GetAvailableQty(ScenarioDetail a_sd, MrpDemand a_demand = null)
    {
        if (m_batchingClosed)
        {
            return GetAvailableQtyFromOverproduction(a_sd, a_demand);
        }

        return GetAvailableQtyForBatch(a_sd, a_demand);
    }

    private decimal GetAvailableQtyFromOverproduction(ScenarioDetail a_sd, MrpDemand a_demand = null)
    {
        if (m_inventory.ReplenishmentMin > 0)
        {
            //We can accept up to the maximum (limited by Max Order and Replenishment
            return Math.Min(m_inventory.Item.MaxOrderQty, m_inventory.ReplenishmentMax) - m_totalDemandQty;
        }

        if (m_inventory.Item.CanBatch)
        {
            decimal producing = GetOrderQty();

            decimal? overrideOrderQty = a_sd.ExtensionController.GetUsableBatchedOverproductionQty(a_sd, this, producing, m_totalDemandQty, a_demand);
            if (overrideOrderQty.HasValue)
            {
                return overrideOrderQty.Value;
            }

            //Accept the remainder of the existing batch, but don't produce another batch
            return producing - m_totalDemandQty;
        }
        else
        {
            decimal? overrideOrderQty = a_sd.ExtensionController.GetUsableNonBatchOverproductionQty(a_sd, this, m_totalDemandQty, a_demand);
            if (overrideOrderQty.HasValue)
            {
                return overrideOrderQty.Value;
            }

            //Don't accept more, this order is closed
            return 0;
        }
    }

    private decimal GetAvailableQtyForBatch(ScenarioDetail a_sd, MrpDemand a_mrpDemand = null)
    {
        decimal maxOrderQty = m_inventory.Item.MaxOrderQty;
        decimal? overrideOrderQty = a_sd.ExtensionController.GetMaxOrderQty(a_sd, this, a_mrpDemand);
        if (overrideOrderQty.HasValue)
        {
            maxOrderQty = overrideOrderQty.Value;

            if (maxOrderQty == 0)
            {
                return 0;
            }
        }

        if (m_inventory.ReplenishmentMin > 0)
        {
            //We can accept up to the maximum (limited by Max Order and Replenishment
            return Math.Min(maxOrderQty, m_inventory.ReplenishmentMax - m_totalDemandQty);
        }

        if (m_inventory.Item.CanBatch)
        {
            decimal ratio = maxOrderQty / m_inventory.Item.BatchSize;
            if (ratio % 1 != 0)
            {
                maxOrderQty = m_inventory.Item.BatchSize * (int)ratio;
            }

            return maxOrderQty - m_totalDemandQty;
        }

        return m_demandParts.Count == 0 ? decimal.MaxValue : 0;
    }

    private int NbrOfBatches()
    {
        decimal totalProductionQty = Math.Max(m_inventory.ReplenishmentMax, m_totalDemandQty);
        totalProductionQty = Math.Max(totalProductionQty, m_inventory.Item.MinOrderQty);
        totalProductionQty = Math.Min(totalProductionQty, m_inventory.Item.MaxOrderQty);
        return (int)Math.Ceiling(totalProductionQty / m_inventory.Item.BatchSize); // round up
    }

    /// <summary>
    /// returns the qty of a supply order needed to satisfy demand defined so far.
    /// out parameter determines if the qty meets batching/MinOrderQty criteria.
    /// </summary>
    /// <param name="o_valid"></param>
    /// <returns></returns>
    public decimal GetOrderQty(out bool o_valid)
    {
        o_valid = IsValid();
        return GetOrderQty();
    }

    private decimal GetOrderQty()
    {
        decimal qty;

        if (m_inventory.ReplenishmentMin > 0)
        {
            //TODO: Should this just be ReplenishmentMax?
            qty = Math.Max(m_totalDemandQty, m_inventory.ReplenishmentMax);
        }
        else if (m_inventory.Item.CanBatch)
        {
            qty = NbrOfBatches() * m_inventory.Item.BatchSize;
        }
        else
        {
            qty = m_totalDemandQty;
        }

        // if MinOrderQty is not met but it could be met with RoundUpLimit, then set the qty to MinOrderQty
        if (qty < m_inventory.Item.MinOrderQty && qty + m_inventory.Item.MinOrderQtyRoundupLimit >= m_inventory.Item.MinOrderQty)
        {
            qty = m_inventory.Item.MinOrderQty;
        }

        return qty;
    }

    /// <summary>
    /// is MinOrderQty met?
    /// </summary>
    /// <returns></returns>
    private bool IsValid()
    {
        return GetOrderQty() >= m_inventory.Item.MinOrderQty;
    }

    /// <summary>
    /// If this supply will be making any extra, it this will allocate some of that extra
    /// qty return the remainder that the supply couldn't accept
    /// </summary>
    /// <param name="a_qty"></param>
    /// <returns></returns>
    internal decimal Allocate(ScenarioDetail a_sd, decimal a_qty, bool a_allowPartialSupply, MrpDemand a_demand)
    {
        bool? canAllocate = a_sd.ExtensionController.CanAllocate(a_sd, this, a_demand, a_qty, a_allowPartialSupply);
        if (canAllocate.HasValue && !canAllocate.Value)
        {
            return a_qty;
        }

        a_qty = Math.Abs(a_qty);
        decimal qtyToAllocate = Math.Min(a_qty, GetUnallocatedQty());
        if (qtyToAllocate <= 0 || (!a_allowPartialSupply && qtyToAllocate < a_qty))
        {
            return a_qty;
        }

        m_allocated = m_allocated + qtyToAllocate;
        m_demandParts.Add(new DemandPart(a_demand, qtyToAllocate));
        return Math.Max(0, a_qty - qtyToAllocate);
    }

    /// <summary>
    /// OrderQty - totalDemandQty - allocatedQty if it's possitive
    /// </summary>
    /// <returns></returns>
    public decimal GetUnallocatedQty()
    {
        return Math.Max(0, GetOrderQty() - m_totalDemandQty - m_allocated);
    }

    /// <summary>
    /// The total supply produces as a result of this order
    /// </summary>
    public decimal ExcessSupplyGenerated => TotalSupplyGenerated - m_allocated - TotalDemand;

    /// <summary>
    /// The total supply produces as a result of this order
    /// </summary>
    public decimal TotalSupplyGenerated => m_supplyGenerated;

    /// <summary>
    /// Total quantity of all demand parts
    /// </summary>
    internal decimal TotalDemand => m_totalDemandQty;

    /// <summary>
    /// Report supply has been generated
    /// </summary>
    /// <param name="a_qty"></param>
    internal void SupplyGenerated(decimal a_qty)
    {
        m_supplyGenerated += a_qty;
    }

    //TODO: Replenishment code
    /// <summary>
    /// similar to add demand, except it adds unallocated qty which can be allocated to future
    /// demands.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_qty"></param>
    /// <param name="a_needDate"></param>
    /// <returns></returns>
    //internal decimal AddReplenishment(ScenarioDetail a_sd, decimal a_qty, DateTime a_needDate)
    //{
    //    a_qty = Math.Abs(a_qty);
    //    if (m_demandParts.Count > 0 && (a_needDate.Ticks < m_needDateTicks || a_needDate.Ticks > GetMaxDemandDateTicks())) return a_qty;
    //    decimal qtyToAccept = Math.Min(a_qty, GetAvailableQty(a_sd));
    //    if (qtyToAccept <= 0)
    //    {
    //        return a_qty; // can't accept it at all.
    //    }
    //    m_replenishmentQty = m_replenishmentQty + qtyToAccept;
    //    m_needDateTicks = Math.Min(a_needDate.Ticks, m_needDateTicks);
    //    m_priority = Math.Min(m_inventory.SafetyStockJobPriority, m_priority);
    //    return Math.Max(0, qtyToAccept);
    //}

    /// <summary>
    /// If using replenishment or safety stock, this function determines if current inventory level is below
    /// required levels and if so creates Supply Orders accordingly.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_inv"></param>
    /// <param name="a_runningTotal">current inventory level (during MRP run)</param>
    /// <param name="a_supplyOrders">list of created supply orders</param>
    /// <param name="a_needDate">time of the last adjustment that changed running total</param>
    /// <param name="a_adjWrappers">list of all AdjustWrappers</param>
    /// <param name="a_adjIndex">Index of the last adjustment that changed running total</param>
    //private void GenerateSupplyForReplenishment(ScenarioDetail a_sd, Inventory a_inv, decimal a_runningTotal, List<SupplyOrder> a_supplyOrders, DateTime a_needDate, List<AdjWrapper> a_adjWrappers, int a_adjIndex)
    //{
    //    decimal runningTotalPlusNewSupplyQty = a_runningTotal + GetTotalNewSupplyOutput(a_supplyOrders);
    //    decimal replenishmentNeeded = GetQuantityNeeded(a_inv, runningTotalPlusNewSupplyQty);
    //    if (a_sd.ScenarioOptions.IsStrictlyGreaterThanZero(replenishmentNeeded))
    //    {
    //        // see if inventory level goes above ReplenishmentToLevel within allowed days.
    //        if (a_inv.ReplenishmentContractDays > 0)
    //        {
    //            DateTime allowedUnderReplenishmentUntilDate = a_needDate.AddDays(a_inv.ReplenishmentContractDays);
    //            decimal futureRunningTotal = runningTotalPlusNewSupplyQty;
    //            for (int i = a_adjIndex + 1; i < a_adjWrappers.Count; i++)
    //            {
    //                AdjWrapper futureAdj = a_adjWrappers[i];
    //                if (futureAdj.AdjDate > allowedUnderReplenishmentUntilDate)
    //                {
    //                    break;
    //                }
    //                runningTotalPlusNewSupplyQty += futureAdj.Adjustment.ChangeQty;
    //                if (runningTotalPlusNewSupplyQty > a_inv.ReplenishmentMax)
    //                {
    //                    replenishmentNeeded = 0;
    //                    break;
    //                }
    //            }
    //        }
    //        // batch with previous supplies
    //        if (a_sd.ScenarioOptions.IsStrictlyGreaterThanZero(replenishmentNeeded))
    //        {
    //            replenishmentNeeded = BatchReplenishmentExistingSupplyOrders(a_sd, replenishmentNeeded, a_supplyOrders, a_needDate);
    //        }
    //        // create new supply
    //        if (a_sd.ScenarioOptions.IsStrictlyGreaterThanZero(replenishmentNeeded))
    //        {
    //            while (!a_sd.ScenarioOptions.IsApproximatelyZeroOrLess(replenishmentNeeded))
    //            {
    //                SupplyOrder supplyOrder = new SupplyOrder(a_inv);
    //                decimal acceptedQty = supplyOrder.AddReplenishment(a_sd, replenishmentNeeded, a_needDate);
    //                if (a_sd.ScenarioOptions.IsApproximatelyZero(acceptedQty))
    //                {
    //                    // TODO: log an error, we're in an infinite loop.
    //                    // this can possibly happen if AllowedPartialSupply is false.
    //                    break;
    //                }
    //                a_supplyOrders.Add(supplyOrder);
    //                replenishmentNeeded -= acceptedQty;
    //            }
    //        }
    //    }
    //}

    //Once batching is closed, we can only accept qty that will be overproduced by the batch.
    private bool m_batchingClosed;

    internal bool IsBatchingClosed => m_batchingClosed;

    /// <summary>
    /// Whether this batch was an overflow from a single demand that already filled another batch.
    /// This is used with the PreventBatchOverflow feature to prevent multiple jobs from having the multiple lot peggings which can cause incorrect inventory allocations
    /// </summary>
    internal bool BatchOverflow { get; set; }

    public void CloseBatching(DateTime a_adjDate)
    {
        if (!m_batchingClosed && IsOutSideBatchWindow(a_adjDate))
        {
            m_batchingClosed = true;
        }
    }
}

/// <summary>
/// A negative adjustment plus a qty. this is used to break down
/// demand into multiple part that could be supplied from different
/// SupplyOrders
/// </summary>
public struct DemandPart
{
    /// <summary>
    /// Full Demand
    /// </summary>
    /// <param name="a_demand"></param>
    internal DemandPart(MrpDemand a_demand)
    {
        Demand = a_demand;
        Qty = a_demand.DemandQty;
    }

    /// <summary>
    /// partial
    /// </summary>
    /// <param name="a_demand"></param>
    /// <param name="a_qty"></param>
    internal DemandPart(MrpDemand a_demand, decimal a_qty)
    {
        a_qty = Math.Abs(a_qty);
        Demand = a_demand;
        Qty = a_qty;
    }

    public readonly MrpDemand Demand;
    public readonly decimal Qty;
}