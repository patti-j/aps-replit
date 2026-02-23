using PT.Scheduler;

namespace PT.SchedulerData.ResourceBlockListData;

public static class ResourceBlockListNodeData
{
    /// <summary>
    /// Checks if two resource blocks can be joined based on Base CanAutoJoin logic (resource LimitAutoJoinToSameCapacityInterval setting and blocks's having different MO, same AutoJoinGroup code, Lateness
    /// matching, and MO Cleanout Separation)
    /// </summary>
    /// <param name="a_blockNode"></param>
    /// <param name="a_sd"></param>
    /// <param name="a_resource"></param>
    /// <param name="a_previousBlock"></param>
    /// <param name="isCleanoutFalse"></param>
    /// <returns></returns>
    public static bool CanAutoJoinWithBaseLogic(this ResourceBlockList.Node a_blockNode, ScenarioDetail a_sd, Resource a_resource, ResourceBlock a_previousBlock, out bool isCleanoutFalse)
    {
        bool canAutoJoin = false;
        isCleanoutFalse = false;

        long curCiStart = DateTime.MinValue.Date.Ticks;
        long curCiEnd = DateTime.MinValue.Date.Ticks;

        if (a_resource.LimitAutoJoinToSameCapacityInterval)
        {
            LinkedListNode<ResourceCapacityInterval> ciNode = a_resource.FindRCIForward(a_blockNode.Data.StartTicks);
            if (ciNode.Value.StartDate != curCiStart) //in a new Capacity Interval so no autojoin on this one.
            {
                curCiStart = ciNode.Value.StartDate;
                curCiEnd = ciNode.Value.EndDate;
                return false; //break
            }
        }

        if (a_previousBlock != null &&
            (!a_resource.LimitAutoJoinToSameCapacityInterval || a_blockNode.Data.EndDateTime.Ticks <= curCiEnd) // only check this if limiting to same shift.
            &&
            a_previousBlock.Batch.FirstActivity.Operation.ManufacturingOrder.Id != a_blockNode.Data.Batch.FirstActivity.Operation.ManufacturingOrder.Id &&
            !string.IsNullOrEmpty(a_previousBlock.Batch.FirstActivity.Operation.ManufacturingOrder.AutoJoinGroup) &&
            a_previousBlock.Batch.FirstActivity.Operation.ManufacturingOrder.AutoJoinGroup == a_blockNode.Data.Batch.FirstActivity.Operation.ManufacturingOrder.AutoJoinGroup &&
            a_previousBlock.Batch.FirstActivity.Late == a_blockNode.Data.Batch.FirstActivity.Late &&
            (a_previousBlock.Batch.FirstActivity.Late ||
             a_resource.LimitAutoJoinToSameCapacityInterval || //Allow the job to become late since they are joining by shift
             (!a_resource.LimitAutoJoinToSameCapacityInterval && !a_previousBlock.Batch.FirstActivity.Late && a_previousBlock.Batch.FirstActivity.NeedDate > a_blockNode.Data.Batch.FirstActivity.ScheduledEndDate))) // don't join if it will make it late.
        {
            // don't join mos that are separated by cleanout
            ResourceCapacityInterval cleanout = a_resource.ResourceCapacityIntervals.FindFirstOnlineIntervalNotUsedForRunAfterDate(a_previousBlock.EndDateTime);
            if (cleanout != null && a_blockNode.Data.StartDateTime >= cleanout.EndDateTime)
            {
                isCleanoutFalse = true;
            }
            else
            {
                canAutoJoin = true;
            }
        }

        return canAutoJoin;
    }
}