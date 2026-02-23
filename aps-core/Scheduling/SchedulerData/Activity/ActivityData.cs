using PT.APSCommon;
using PT.Database;
using PT.PackageDefinitions.Settings.PublishOptions;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.SchedulerExtensions.Activity;
using PT.SchedulerExtensions.Block;
using static PT.ERPTransmissions.ResourceT;

namespace PT.SchedulerData.Activity;

public static class ActivityData
{
    public static void PtDbPopulate(this InternalActivityManager a_manager, ScenarioDetail a_sd, ref PtDbDataSet dataSet, PtDbDataSet.JobOperationsRow jobOpRow, PTDatabaseHelper a_dbHelper)
    {
        for (int i = 0; i < a_manager.Count; i++)
        {
            InternalActivity activity = a_manager.GetByIndex(i);
            if (!(activity.Scheduled && activity.ScheduledStartDate.Ticks > a_dbHelper.MaxPublishDate.Ticks))
            {
                activity.PtDbPopulate(a_sd, ref dataSet, jobOpRow, a_dbHelper);
            }
        }
    }

    public static void PtDbPopulate(this InternalActivity a_act, ScenarioDetail a_sd, ref PtDbDataSet r_dataSet, PtDbDataSet.JobOperationsRow a_jobOpRow, PTDatabaseHelper a_dbHelper)
    {
        ScenarioPublishDataLimits scenarioPublishDataLimits = new ScenarioPublishDataLimits();
        using (a_sd.Scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
        {
            scenarioPublishDataLimits = ss.ScenarioSettings.LoadSetting(scenarioPublishDataLimits);
        }

        bool includeBlocks = scenarioPublishDataLimits.PublishBlocks || a_dbHelper.PublishAllData;
        decimal scheduledResourceOptimizeScoreTotal = 0;
        string optimizeScoreDetailsString = String.Empty;
        
        BaseId scheduledResId = BaseId.NULL_ID;
        if (a_act.Batch != null)
        {
            scheduledResId = a_act.Batch.PrimaryResource.Id;
            optimizeScoreDetailsString = a_act.GetScheduledResourceScoreDetails(scheduledResId);
        }

        IReadOnlyList<FactorScore> factorScoresList = a_act.GetFactorScoresForResource(scheduledResId);
        if (factorScoresList.Count > 0)
        {
            scheduledResourceOptimizeScoreTotal = factorScoresList.Sum(f => f.Score);
        }

        //Values that don't exist in Scheduler.InternalActivity            
        //Add Activity row
        PtDbDataSet.JobActivitiesRow jobActivitiesRow = r_dataSet.JobActivities.AddJobActivitiesRow(
            a_jobOpRow.PublishDate,
            a_jobOpRow.InstanceId,
            a_act.Operation.ManufacturingOrder.Job.Id.ToBaseType(),
            a_act.Operation.ManufacturingOrder.Id.ToBaseType(),
            a_act.Operation.Id.ToBaseType(),
            a_act.Id.ToBaseType(),
            a_act.ProductionStatus.ToString(),
            a_act.RequiredFinishQty,
            a_act.ReportedGoodQty,
            a_act.ReportedScrapQty,
            a_act.ReportedRunSpan.TotalHours,
            a_act.ReportedSetupSpan.TotalHours,
            a_act.ReportedPostProcessingSpan.TotalHours,
            a_act.ReportedStorageSpan.TotalHours,
            a_act.ReportedCleanSpan.TotalHours,
            a_act.ReportedCleanoutGrade,
            a_dbHelper.AdjustPublishTime(a_act.ReportedEndOfPostProcessingDate),
            a_dbHelper.AdjustPublishTime(a_act.ReportedEndOfStorageDate),
            a_act.Batch?.ZeroLength ?? false,
            a_act.ExternalId,
            a_act.Locked.ToString(),
            a_act.Anchored,
            a_dbHelper.AdjustPublishTime(a_act.AnchorStartDate),
            a_act.AnchorDrift.TotalHours,
            a_dbHelper.AdjustPublishTime(a_act.ScheduledStartDate),
            a_dbHelper.AdjustPublishTime(a_act.Batch?.SetupFinishedDateTime ?? PTDateTime.MinDateTime),
            a_dbHelper.AdjustPublishTime(a_act.ScheduledEndOfRunDate),
            a_dbHelper.AdjustPublishTime(a_act.ScheduledEndDate),
            a_act.ScheduledSetupSpan.TotalHours,
            a_act.ScheduledProductionSpan.TotalHours,
            a_act.ScheduledPostProcessingSpan.TotalHours,
            a_dbHelper.AdjustPublishTime(a_act.ReportedFinishDate),
            a_act.ActivityManualUpdateOnly,
            a_act.Paused,
            a_act.NbrOfPeople,
            a_act.PeopleUsage.ToString(),
            a_act.Comments,
            a_act.Comments2,
            a_act.ExpectedFinishQty,
            a_dbHelper.AdjustPublishTime(a_act.JitStartDate),
            a_act.Scheduled,
            a_act.WorkContent.TotalHours,
            a_act.PercentFinished,
            a_act.Bottleneck,
            a_act.CalendarDuration.TotalHours,
            a_dbHelper.AdjustPublishTime(a_act.EndOfRunDate),
            a_act.ExpectedScrapQty,
            a_act.LaborCost,
            a_act.Late,
            a_act.LeftLeeway.TotalHours,
            a_act.MachineCost,
            a_dbHelper.AdjustPublishTime(a_act.MaxDelayRequiredStartBy),
            a_act.MaxDelaySlack.TotalDays,
            a_act.MaxDelayViolation,
            a_act.NbrOfPeopleAdjustedWorkContent.TotalHours,
            a_act.Queue.TotalDays,
            a_act.RemainingQty,
            a_dbHelper.AdjustPublishTime(a_act.ReportedEndOfRunDate),
            new TimeSpan(a_act.ReportedMaterialPostProcessing).TotalHours,
            a_dbHelper.AdjustPublishTime(a_act.ReportedStartDate),
            a_act.RequiredStartQty,
            a_act.ResourcesUsed,
            a_act.ResourceTransferSpan.TotalHours,
            a_act.RightLeeway.TotalHours,
            a_act.Slack.TotalDays,
            a_act.SplitId,
            a_act.Started,
            a_act.Timing.ToString(),
            a_act.Batched,
            TimeSpan.FromTicks(a_act.ScheduledOrDefaultProductionInfo.CycleSpanTicks).TotalHours,
            a_act.ScheduledOrDefaultProductionInfo.QtyPerCycle,
            TimeSpan.FromTicks(a_act.ScheduledOrDefaultProductionInfo.SetupSpanTicks).TotalHours,
            TimeSpan.FromTicks(a_act.ScheduledOrDefaultProductionInfo.PostProcessingSpanTicks).TotalHours,
            TimeSpan.FromTicks(a_act.ScheduledOrDefaultProductionInfo.CleanSpanTicks).TotalHours,
            a_act.ScheduledOrDefaultProductionInfo.CleanoutGrade,
            a_act.ScheduledOrDefaultProductionInfo.PlanningScrapPercent,
            a_act.ScheduledOrDefaultProductionInfo.OnlyAllowManualUpdatesToCycleSpan,
            a_act.ScheduledOrDefaultProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle,
            a_act.ScheduledOrDefaultProductionInfo.OnlyAllowManualUpdatesToSetupSpan,
            a_act.ScheduledOrDefaultProductionInfo.OnlyAllowManualUpdatesToPostProcessingSpan,
            a_act.ScheduledOrDefaultProductionInfo.OnlyAllowManualUpdatesToCleanSpan,
            a_act.ScheduledOrDefaultProductionInfo.OnlyAllowManualUpdatesToPlanningScrapPercent,
            scheduledResourceOptimizeScoreTotal,
            optimizeScoreDetailsString,
            a_act.Batch != null ? a_act.Batch.SetupCapacitySpan.Overrun : false,
            a_act.Batch != null ? a_act.Batch.ProcessingCapacitySpan.Overrun : false,
            a_act.Batch != null ? a_act.Batch.PostProcessingSpan.Overrun : false,
            a_dbHelper.AdjustPublishTime(a_act.ReportedProcessingStartDateTime),
            a_act.BatchAmount,
            a_act.ActualResourcesUsed != null ? a_act.ActualResourcesUsed.ToResourceExternalIdsCSV(a_sd) : "",
            a_act.ScheduledCleanSpan.Hours,
            a_act.TotalSetupCost,
            a_act.TotalCleanCost,
            a_dbHelper.AdjustPublishTime(a_act.DbrJitStartDate)
        );

        if (includeBlocks && a_act.Batch != null) //scheduled
        {
            for (int i = 0; i < a_act.Batch.BlockCount; i++)
            {
                ResourceBlock block = a_act.Batch.BlockAtIndex(i);
                if (block != null && block.Activity.Id == a_act.Id)
                {
                    block.PtDbPopulate(a_sd, ref r_dataSet, jobActivitiesRow, a_dbHelper);
                }
            }
        }
    }

    /// <summary>
    /// If scheduled, returns the sum of scheduled spans
    /// Otherwise, 0
    /// </summary>
    public static TimeSpan ScheduledRemainingWorkSpan(this InternalActivity a_act)
    {
        if (a_act.Scheduled)
        {
            return a_act.ScheduledSetupSpan + a_act.ScheduledProductionSpan + a_act.ScheduledPostProcessingSpan;
        }

        return TimeSpan.Zero;
    }

    /// <summary>
    /// Returns the standard work span minus reported spans
    /// Minimum of zero
    /// </summary>
    public static TimeSpan StandardRemainingWorkSpan(this InternalActivity a_act)
    {
        return PTDateTime.Max(TimeSpan.Zero, a_act.StandardWorkSpan() - a_act.ReportedSetupSpan - a_act.ReportedRunSpan - a_act.ReportedPostProcessingSpan);
    }

    /// <summary>
    /// If scheduled, returns the sum of the batch start and end times
    /// Otherwise, zero
    /// </summary>
    public static TimeSpan CalendarRemainingWorkSpan(this InternalActivity a_act)
    {
        if (a_act.Batch != null)
        {
            return a_act.Batch.CalendarSetupSpan() + a_act.Batch.CalendarRunSpan() + a_act.Batch.CalendarPostProcessingSpan();
        }

        return TimeSpan.Zero;
    }

    /// <summary>
    /// If scheduled, returns the sum of scheduled and reported spans
    /// Otherwise, zero
    /// </summary>
    public static TimeSpan ScheduledWorkSpan(this InternalActivity a_act)
    {
        if (a_act.Scheduled)
        {
            return ScheduledRemainingWorkSpan(a_act) + a_act.ReportedSetupSpan + a_act.ReportedRunSpan + a_act.ReportedPostProcessingSpan;
        }

        return TimeSpan.Zero;
    }

    /// <summary>
    /// Returns the sum of standard and reported spans
    /// </summary>
    public static TimeSpan StandardWorkSpan(this InternalActivity a_act)
    {
        return a_act.StandardSetupSpan() + a_act.StandardRunSpan() + a_act.Operation.PostProcessingSpan;
    }

    /// <summary>
    /// If scheduled, returns the sum of scheduled calendar spans and reported spans
    /// If not scheduled, returns zero
    /// </summary>
    public static TimeSpan ReportedWorkSpan(this InternalActivity a_act)
    {
        if (a_act.Batch != null)
        {
            return a_act.ReportedSetupSpan + a_act.ReportedRunSpan + a_act.ReportedPostProcessingSpan + a_act.ReportedCleanSpan + a_act.ReportedStorageSpan;
        }

        return TimeSpan.Zero;
    }

    public static bool CanActivitySupplyLotControlledMaterial(this InternalActivity a_activity, MaterialRequirement a_materialRequirement)
    {
        foreach (Product product in a_activity.Operation.Products)
        {
            if (a_materialRequirement.Item == product.Item
                && !string.IsNullOrEmpty(product.LotCode)
                && a_materialRequirement.ContainsEligibleLot(product.LotCode))
            {
                return true;
            }
        }

        return false;
    }
}