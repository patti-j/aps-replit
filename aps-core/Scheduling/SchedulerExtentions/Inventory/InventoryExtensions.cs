using PT.APSCommon;
using PT.Scheduler;
using PT.SchedulerExtensions.Operations;

namespace PT.SchedulerExtensions.Inventory;

public static class InventoryExtensions
{
    /// <summary>
    /// Checks the Inventory's Adjustment Array for production from an Activity.
    /// </summary>
    /// <param name="windowStartInclusive">Searches starting from here.</param>
    /// <param name="windowEndExclusive">Searches up to here.</param>
    /// <param name="a_excludeSuccessorId"></param>
    /// <returns>The first Internal Activity producing in this window.  Returns null if none is found.</returns>
    public static InternalActivity FindFirstSupplyingActivityInWindow(this Scheduler.Inventory a_inv, DateTime windowStartInclusive, DateTime windowEndExclusive, BaseId a_excludeSuccessorId)
    {
        AdjustmentArray adjustments = a_inv.GetAdjustmentArray();

        for (int i = 0; i < adjustments.Count; i++)
        {
            Adjustment adj = adjustments[i];
            if (adj.AdjDate.Ticks >= windowStartInclusive.Ticks && adj.AdjDate.Ticks < windowEndExclusive.Ticks && adj.ChangeQty > 0)
            {
                if (adj is ActivityAdjustment actAdj && (a_excludeSuccessorId == BaseId.NULL_ID || !actAdj.Activity.Operation.ContainsOpAsSuccessor(a_excludeSuccessorId)))
                {
                    return actAdj.Activity; //found the first Activity.
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Condense adjustments that have the same date, reason, and type.
    /// </summary>
    /// <param name="a_array"></param>
    /// <returns></returns>
    //public static AdjustmentArray CondenseAdjustmentsWithSameTimeAndReason(this AdjustmentArray a_array)
    //{
    //    AdjustmentArray newArray = new AdjustmentArray(null);

    //    Adjustment previousAdjustment = null;
    //    foreach (Adjustment adjustment in a_array)
    //    {
    //        if (previousAdjustment == null)
    //        {
    //            // First item – just hold onto it for now.
    //            previousAdjustment = adjustment;
    //            continue;
    //        }

    //        // Same key?  Roll the values together.
    //        if (previousAdjustment.AdjDate == adjustment.AdjDate
    //            && previousAdjustment.GetReason().Id == adjustment.GetReason().Id
    //            && previousAdjustment.AdjustmentType == adjustment.AdjustmentType)
    //        {
    //            previousAdjustment.Condense(adjustment);
    //        }
    //        else
    //        {
    //            // Different key – push the finished one and start a new group.
    //            newArray.Add(previousAdjustment);
    //            previousAdjustment = adjustment;
    //        }
    //    }

    //    // Push the final pending adjustment (if any).
    //    if (previousAdjustment != null)
    //    {
    //        newArray.Add(previousAdjustment);
    //    }

    //    return newArray;
    //}
}