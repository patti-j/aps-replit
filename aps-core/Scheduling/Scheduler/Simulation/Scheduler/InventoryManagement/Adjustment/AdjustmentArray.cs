using PT.APSCommon;
using PT.APSCommon.Collections;
using PT.Common.Debugging;

namespace PT.Scheduler;

/// <summary>
/// Don't be confused about this array. It has nothing to do with overlap or material overlap or
/// keeping track of inventory. It is used only to plot and track inventory levels in UI and reports.
/// A set of adjustments to a specific inventory (within a warehouse).
/// </summary>
public partial class AdjustmentArray
{
    public override void Add(Adjustment a_adjustment)
    {
        //#if DEBUG
        //if (Sorted)
        //{
        //    throw new DebugException("AdjustmentArray cannot be added to once it has been sorted.");
        //}
        //#endif
        if (a_adjustment.Id == BaseId.NEW_OBJECT_ID)
        {
            a_adjustment.Id = m_adjustmentIdGenerator.NextID();
        }

        base.Add(a_adjustment);
    }

    internal void Add(AdjustmentArray a_adjustmentArray)
    {
        #if DEBUG
        if (Sorted)
        {
            throw new DebugException("AdjustmentArray cannot be added to once it has been sorted.");
        }
        #endif
        foreach (Adjustment adjustment in a_adjustmentArray)
        {
            Add(adjustment);
        }
    }

    //TODO: SA should be replaced by looping through adjustments and not in the array
    //internal void UpdateLineDistributionSourceQtys()
    //{
    //    decimal invQty = 0;

    //    for (int adjI = 0; adjI < Count; ++adjI)
    //    {
    //        Adjustment adj = this[adjI];

    //        if (adj.Reason is Inventory)
    //        {
    //            invQty += adj.ChangeQty;
    //        }
    //        else if (adj.ChangeQty < 0)
    //        {
    //            decimal unallocatedQty = adj.ChangeQty;
    //            decimal qtyAllocatedFromInv = 0;

    //            if (invQty > 0 && unallocatedQty > 0)
    //            {
    //                if (invQty >= unallocatedQty)
    //                {
    //                    qtyAllocatedFromInv = unallocatedQty;
    //                    unallocatedQty = 0;
    //                    invQty -= qtyAllocatedFromInv;
    //                }
    //                else
    //                {
    //                    qtyAllocatedFromInv = invQty;
    //                    unallocatedQty -= qtyAllocatedFromInv;
    //                    invQty = 0;
    //                }
    //            }

    //            if (adj.Reason is Demand.SalesOrderLineDistribution)
    //            {
    //                Demand.SalesOrderLineDistribution dist = (Demand.SalesOrderLineDistribution)adj.Reason;
    //                dist.QtyAllocatedFromOnHandInventory = qtyAllocatedFromInv;
    //                dist.QtyAllocatedFromProjectedInventory = unallocatedQty;
    //            }
    //        }
    //    }
    //}

    /// <summary>
    /// checks wether this array includes an adjustment with a date prior to provided cut off.
    /// </summary>
    /// <param name="a_cutoffTicks"></param>
    /// <returns>true if first adjustment if any is strictly smaller than a_cutoffTicks</returns>
    public bool HasAdjustmentBeforeDate(long a_cutoffTicks)
    {
        Sort();
        return Count > 0 && this[0].AdjDate.Ticks < a_cutoffTicks;
    }
}

public class ReadOnlyAdjutmentArray
{
    internal ReadOnlyAdjutmentArray(AdjustmentArray a_adjustments)
    {
        m_adjustments = a_adjustments;
    }

    private readonly AdjustmentArray m_adjustments;

    /// <summary>
    /// The number of elements in the AdjustmentArray.
    /// </summary>
    public int Count => m_adjustments.Count;

    public Adjustment this[int a_index] => m_adjustments[a_index];
}