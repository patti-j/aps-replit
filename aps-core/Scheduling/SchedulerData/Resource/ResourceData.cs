using PT.APSCommon;
using PT.Database;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

public static class ResourceData
{
    #region PT Database
    public static void PtDbPopulate(this Resource a_resource, ref PtDbDataSet r_dataSet, PtDbDataSet.DepartmentsRow a_deptRow, ScenarioDetail a_sd, PTDatabaseHelper a_dbHelper)
    {
        //Add Resource row            
        PtDbDataSet.ResourcesRow resRow = r_dataSet.Resources.AddResourcesRow(
            a_deptRow.PublishDate,
            a_deptRow.InstanceId,
            a_deptRow.PlantId,
            a_deptRow.DepartmentId,
            a_resource.Id.ToBaseType(),
            a_resource.Name,
            a_resource.Description,
            a_resource.Notes,
            a_resource.Bottleneck,
            a_resource.BufferSpan.TotalHours,
            a_resource.CapacityType.ToString(),
            a_resource.OvertimeHourlyCost,
            a_resource.StandardHourlyCost,
            a_resource.ExperimentalDispatcherIdOne.ToBaseType(),
            a_resource.ExperimentalDispatcherIdTwo.ToBaseType(),
            a_resource.ExperimentalDispatcherIdThree.ToBaseType(),
            a_resource.ExperimentalDispatcherIdFour.ToBaseType(),
            a_resource.NormalDispatcherId.ToBaseType(),
            a_resource.Workcenter,
            a_resource.CanOffload,
            a_resource.CanPreemptMaterials,
            a_resource.CanPreemptPredecessors,
            a_resource.CanWorkOvertime,
            a_resource.CycleEfficiencyMultiplier,
            a_resource.HeadStartSpan.TotalHours,
            a_resource.PostActivityRestSpan.TotalHours,
            a_resource.Stage,
            a_resource.TransferSpan.TotalHours,
            a_resource.ConsecutiveSetupTimes,
            a_resource.ActivitySetupEfficiencyMultiplier,
            a_resource.ChangeoverSetupEfficiencyMultiplier,
            a_resource.SetupSpan.TotalHours,
            a_resource.UseResourceSetupTime,
            a_resource.UseOperationSetupTime,
            a_resource.UseSequencedSetupTime,
            a_resource.ResourceSetupCost,
            a_resource.Active,
            a_resource.DiscontinueSameCellScheduling,
            a_resource.ResourceType.ToString(),
            a_resource.ExternalId,
            false,
            a_resource.AttributeCodeTableName,
            Convert.ToDouble(a_resource.BottleneckPercent),
            a_resource.BufferSpan.TotalHours,
            a_resource.CellName,
            a_resource.DisallowDragAndDrops,
            a_resource.ExcludeFromGantts,
            a_resource.ExperimentalOptimizeRuleOne,
            a_resource.ExperimentalOptimizeRuleTwo,
            a_resource.ExperimentalOptimizeRuleThree,
            a_resource.ExperimentalOptimizeRuleFour,
            a_resource.GanttRowHeightFactor,
            a_resource.HeadStartSpan.TotalDays,
            a_resource.ImageFileName,
            a_resource.MaxQty,
            a_resource.MaxQtyPerCycle,
            a_resource.MinQty,
            a_resource.MinQtyPerCycle,
            a_resource.NbrCapabilities,
            a_resource.NormalOptimizeRule,
            a_resource.OverlappingOnlineIntervals,
            a_resource.Sequential,
            a_resource.SetupSpan.TotalHours,
            a_resource.ShopViewUsersCount,
            a_resource.TransferSpan.TotalHours,
            a_resource.WorkcenterExternalId,
            a_resource.MaxCumulativeQty,
            a_resource.ManualSchedulingOnly,
            a_resource.IsTank,
            a_resource.MinNbrOfPeople,
            a_resource.BatchType.ToString(),
            a_resource.BatchVolume,
            a_resource.AutoJoinSpan.TotalHours,
            a_resource.OmitSetupOnFirstActivity,
            a_resource.OmitSetupOnFirstActivityInShift,
            a_resource.MinVolume,
            a_resource.MaxVolume,
            TimeSpan.FromTicks(a_resource.StandardCleanSpanTicks).TotalHours,
            a_resource.StandardCleanoutGrade,
            a_resource.UseResourceCleanout,
            a_resource.UseOperationCleanout,
            a_resource.UseAttributeCleanouts,
            a_resource.ResourceCleanoutCost,
            a_resource.OperationCountCleanoutTriggerTableName,
            a_resource.ProductionUnitsCleanoutTriggerTableName,
            a_resource.TimeCleanoutTriggerTableName,
            a_resource.Priority,
            a_resource.StorageAreaResource
        ); //Get rid of the last field -- used to be PostProcessingUsesResource.

        a_resource.PtDbPopulateUserFields(resRow, a_sd, a_dbHelper.UserFieldDefinitions);
    }

    //DB Export
    public static void PtDbPopulate(this ResourceManager a_resource, ref PtDbDataSet r_dataSet, PtDbDataSet.DepartmentsRow a_deptRow, bool a_limitToResources, HashSet<BaseId> a_resourceIds, ScenarioDetail a_sd, PTDatabaseHelper a_dbHelper)
    {
        for (int i = 0; i < a_resource.Count; i++)
        {
            if (a_limitToResources && !a_resourceIds.Contains(a_resource[i].Id))
            {
                continue;
            }

            a_resource[i].PtDbPopulate(ref r_dataSet, a_deptRow, a_sd, a_dbHelper);
        }
    }

    /// <summary>
    /// Calculate fields that need to be calculated by Resource, prior to exporting to PT database.
    /// </summary>
    public static void PreProcessWorkForPtDbPopulate(this Resource a_res)
    {
        ResourceBlockList.Node node = a_res.Blocks.First;
        int seq = 1;
        int runNbr = 0;
        DateTimeOffset curDate = PTDateTime.MinValue;
        while (node != null)
        {
            ResourceBlock block = node.Data;
            if (block.StartDateTime.ToDisplayTime().ToDateNoTime() > curDate) //in a new day
            {
                runNbr = 1;
                curDate = block.StartDateTime.ToDisplayTime().ToDateNoTime();
            }
            else
            {
                runNbr++;
            }

            block.RunNbr = runNbr;
            block.Sequence = seq;
            seq++;
            node = node.Next;
        }
    }
    #endregion

    /// <summary>
    /// Returns the efficiency % based on the multiplier
    /// </summary>
    public static decimal GetSetupEfficiencyPercent(this Resource a_resource)
    {
        return 100m / a_resource.ActivitySetupEfficiencyMultiplier;
    }

    /// <summary>
    /// Returns the efficiency % based on the multiplier
    /// </summary>
    public static decimal GetCycleEfficiencyPercent(this Resource a_resource)
    {
        return 100m / a_resource.CycleEfficiencyMultiplier;
    }

    /// <summary>
    /// Gets total available capacity for scheduled (online - offline - already scheduled).
    /// Assumes single tasking.
    /// </summary>
    /// <param name="a_start"></param>
    /// <param name="a_end"></param>
    /// <returns></returns>
    public static TimeSpan GetAvailableCapacityBetweenDates(this Resource a_resource, DateTime a_start, DateTime a_end)
    {
        TimeSpan online = a_resource.ResourceCapacityIntervals.GetTotalSchedulableCapacityBetweenDates(a_start, a_end);
        ResourceBlockList.Node node = a_resource.Blocks.First;
        while (node != null)
        {
            Batch batch = node.Data.Batch;
            if ((batch.StartDateTime <= a_start && batch.EndDateTime > a_start) || (batch.StartDateTime >= a_start && batch.StartDateTime < a_end))
            {
                DateTime offlineStart = batch.StartDateTime <= a_start ? a_start : batch.StartDateTime;
                DateTime offlineEnd = batch.EndDateTime > a_end ? a_end : batch.EndDateTime;
                TimeSpan offlineDuration = offlineEnd.Subtract(offlineStart);
                if (offlineDuration > online)
                {
                    online = TimeSpan.Zero;
                    break;
                }

                online = online.Subtract(offlineDuration);
            }

            node = node.Next;
        }

        return online <= TimeSpan.Zero ? TimeSpan.Zero : online;
    }

    /// <summary>
    /// Gets total number of activities scheduled on a resource.
    /// </summary>
    /// <param name="a_resource"></param>
    /// <returns></returns>
    public static int GetNbrActivitiesScheduled(this Resource a_resource)
    {
        if (a_resource.Blocks.Count == 0)
        {
            return 0;
        }

        ResourceBlockList.Node node = a_resource.Blocks.First;
        int scheduledActivityCount = 0;
        while (node != null)
        {
            ResourceBlock block = node.Data;
            scheduledActivityCount += block.Batch.ActivitiesCount;
            node = node.Next;
        }

        return scheduledActivityCount;
    }
}