using System.Collections;

using PT.Database;
using PT.PackageDefinitions.Settings.PublishOptions;
using PT.Scheduler;
using PT.SchedulerData.Activity;
using PT.SchedulerData.Resources;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

public static class ResourceOperationData
{
    public static void PtDbPopulate(this ResourceOperation a_resourceOperation,
                                    ScenarioDetail a_sd,
                                    ref PtDbDataSet a_dataSet,
                                    PtDbDataSet.ManufacturingOrdersRow a_moRow,
                                    bool a_publishInventory,
                                    Hashtable a_msProjectIndexesHash,
                                    PTDatabaseHelper a_dbHelper)
    {
        ScenarioPublishDataLimits scenarioPublishDataLimits = new ScenarioPublishDataLimits();
        using (a_sd.Scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
        {
            scenarioPublishDataLimits = ss.ScenarioSettings.LoadSetting(scenarioPublishDataLimits);
        }
        bool includeActivities = scenarioPublishDataLimits.PublishActivities || a_dbHelper.PublishAllData;

        System.Text.StringBuilder predBuilder = new ();
        //Create a list of all this operation's predecessors
        if (a_resourceOperation.AlternatePathNode != null && a_resourceOperation.AlternatePathNode.Predecessors != null)
        {
            for (int predI = 0; predI < a_resourceOperation.AlternatePathNode.Predecessors.Count; predI++)
            {
                AlternatePath.Association association = a_resourceOperation.AlternatePathNode.Predecessors[predI];
                BaseOperation predOp = association.Predecessor.Operation;
                if (predOp.Omitted == BaseOperationDefs.omitStatuses.NotOmitted) //don't want to show Omitted ops in the projects
                {
                    int predRowIdx = (int)a_msProjectIndexesHash[predOp.Id] + 2; //MS Project is 1-based indexes plus the Job summary-task row adds another 1.
                    string msOverlap;
                    if (association.OverlapType == InternalOperationDefs.overlapTypes.NoOverlap)
                    {
                        msOverlap = "FS"; //Finish to Start
                    }
                    else
                    {
                        msOverlap = "SS"; //Start to Start
                    }

                    if (predBuilder.Length > 0)
                    {
                        predBuilder.Append(","); //MAY NEED TO USE REGIONAL LIST SEPARATOR CHARACTER
                    }

                    predBuilder.Append(string.Format("{0}{1}", predRowIdx, msOverlap));
                }
            }
        }
        
        a_resourceOperation.GetReportedStartDate(out long reportedStartTicks);
        a_resourceOperation.GetReportedStartDate(out long reportedEndTicks);

        PtDbDataSet.JobOperationsRow jobOpRow = a_dataSet.JobOperations.AddJobOperationsRow(
            a_moRow.PublishDate,
            a_moRow.InstanceId,
            a_resourceOperation.ManufacturingOrder.Job.Id.ToBaseType(),
            a_resourceOperation.ManufacturingOrder.Id.ToBaseType(),
            a_resourceOperation.Id.ToBaseType(),
            a_resourceOperation.Name,
            a_resourceOperation.Description,
            a_resourceOperation.SetupSpan.TotalHours,
            a_resourceOperation.ProductionSetupCost,
            a_resourceOperation.RequiredStartQty,
            a_resourceOperation.RequiredFinishQty,
            a_resourceOperation.QtyPerCycle,
            a_resourceOperation.CycleSpan.TotalMinutes,
            a_resourceOperation.PostProcessingSpan.TotalHours,
            a_resourceOperation.CleanSpan.TotalHours,
            a_resourceOperation.CleanoutCost,
            a_resourceOperation.OverlapTransferQty,
            a_resourceOperation.CanPause,
            a_resourceOperation.CanSubcontract,
            a_resourceOperation.CarryingCost,
            a_resourceOperation.CompatibilityCode,
            a_resourceOperation.BatchCode,
            a_resourceOperation.SetupNumber,
            a_resourceOperation.DeductScrapFromRequired,
            a_resourceOperation.SuccessorProcessing.ToString(),
            a_resourceOperation.KeepSuccessorsTimeLimit.TotalHours,
            a_dbHelper.AdjustPublishTime(a_resourceOperation.HoldUntil),
            a_resourceOperation.HoldReason,
            a_resourceOperation.Omitted.ToString(),
            a_resourceOperation.Finished,
            a_resourceOperation.OnHold,
            a_resourceOperation.IsRework,
            Convert.ToDouble(a_resourceOperation.PlanningScrapPercent),
            a_resourceOperation.UOM,
            a_resourceOperation.Notes,
            a_resourceOperation.AutoSplit,
            a_resourceOperation.WholeNumberSplits,
            a_resourceOperation.ResourceRequirements.PrimaryResourceRequirement.Id.ToBaseType(),
            a_resourceOperation.SetupColor.Name,
            a_resourceOperation.TimeBasedReporting,
            a_resourceOperation.ExternalId,
            a_resourceOperation.Scheduled,
            a_dbHelper.AdjustPublishTime(a_resourceOperation.StartDateTime),
            a_dbHelper.AdjustPublishTime(a_resourceOperation.ScheduledEndDate),
            a_dbHelper.AdjustPublishTime(new DateTime(reportedStartTicks)),
            a_dbHelper.AdjustPublishTime(new DateTime(reportedEndTicks)),
            a_dbHelper.AdjustPublishTime(a_resourceOperation.NeedDate),
            a_resourceOperation.Locked.ToString(),
            a_resourceOperation.Anchored.ToString(),
            a_resourceOperation.GetScheduledPrimaryWorkCenterExternalId(),
            a_resourceOperation.AutoReportProgress,
            a_resourceOperation.ExpectedFinishQty,
            a_resourceOperation.UseExpectedFinishQty,
            a_resourceOperation.SchedulingHours,
            a_resourceOperation.OutputName,
            a_resourceOperation.UseCompatibilityCode,
            a_resourceOperation.WorkContent.TotalHours,
            a_resourceOperation.PercentFinished,
            predBuilder.ToString(),
            a_resourceOperation.Attributes.AttributesSummary,
            a_resourceOperation.AutoFinish,
            a_resourceOperation.Bottleneck,
            a_resourceOperation.GetBuyDirectMaterialsList(),
            a_resourceOperation.GetBuyDirectMaterialsNotAvailableList(),
            a_resourceOperation.CarryingCost,
            a_resourceOperation.Cycles,
            a_resourceOperation.CycleSpan.TotalHours,
            a_dbHelper.AdjustPublishTime(a_resourceOperation.EndOfMatlPostProcDate),
            a_dbHelper.AdjustPublishTime(a_resourceOperation.EndOfResourceTransferTimeDate),
            a_dbHelper.AdjustPublishTime(a_resourceOperation.EndOfRunDate),
            a_resourceOperation.ExpectedRunHours,
            a_resourceOperation.ExpectedScrapQty,
            a_resourceOperation.ExpectedSetupHours,
            a_dbHelper.AdjustPublishTime(a_resourceOperation.JitStartDate),
            a_resourceOperation.KeepSuccessorsTimeLimit.TotalHours,
            a_resourceOperation.LaborCost,
            a_resourceOperation.Late,
            a_resourceOperation.LatestConstraint,
            a_dbHelper.AdjustPublishTime(a_resourceOperation.LatestConstraintDate),
            a_dbHelper.AdjustPublishTime(a_resourceOperation.GetLatestPredecessorFinishDate()),
            a_resourceOperation.MachineCost,
            a_resourceOperation.MaterialCost,
            a_resourceOperation.MaterialList,
            a_resourceOperation.CalculateMaterialsNotAvailable(a_sd),
            a_resourceOperation.GetMaterialsNotPlanned(),
            a_resourceOperation.GetMaterialStatus(a_sd.ClockDate).ToString(),
            a_dbHelper.AdjustPublishTime(a_resourceOperation.MaxDelayRequiredStartBy),
            a_resourceOperation.ProductsList,
            a_resourceOperation.RemainingFinishQty,
            a_resourceOperation.ReportedGoodQty,
            a_resourceOperation.ReportedRunHours,
            a_resourceOperation.ReportedScrapQty,
            a_resourceOperation.ReportedSetupHours,
            a_resourceOperation.ReportedPostProcessingHours,
            a_resourceOperation.RequiredCapabilities,
            a_resourceOperation.ResourcesUsed,
            a_resourceOperation.RunSpan.TotalHours,
            a_resourceOperation.SchedulingHours,
            a_resourceOperation.SetupColor.ToString(),
            a_resourceOperation.Split,
            a_resourceOperation.StandardRunSpan.TotalHours,
            a_resourceOperation.StandardSetupSpan.TotalHours,
            a_resourceOperation.Started,
            a_resourceOperation.StockMaterialsList,
            a_resourceOperation.StockMaterialsListAwaitingAllocation,
            a_dbHelper.AdjustPublishTime(a_resourceOperation.CommitStartDate),
            a_dbHelper.AdjustPublishTime(a_resourceOperation.CommitEndDate),
            a_resourceOperation.ProductionInfo.OnlyAllowManualUpdatesToCycleSpan,
            a_resourceOperation.ProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle,
            a_resourceOperation.ProductionInfo.OnlyAllowManualUpdatesToSetupSpan,
            a_resourceOperation.ProductionInfo.OnlyAllowManualUpdatesToPostProcessingSpan,
            a_resourceOperation.ProductionInfo.OnlyAllowManualUpdatesToPlanningScrapPercent,
            a_resourceOperation.ProductionInfoBaseOperation.OnlyAllowManualupdates.Materials,
            a_resourceOperation.ProductionInfoBaseOperation.OnlyAllowManualupdates.Products,
            a_resourceOperation.ProductionInfo.OnlyAllowManualUpdatesToResourceRequirements,
            TimeSpan.FromTicks(a_resourceOperation.ProductionInfo.StorageSpanTicks).TotalHours,
            Convert.ToDouble(a_resourceOperation.CurrentBufferPenetrationPercent),
            Convert.ToDouble(a_resourceOperation.ScheduledBufferPenetrationPercent),
            a_resourceOperation.StandardOperationBufferDays,
            a_resourceOperation.PlannedScrapQty,
            a_resourceOperation.AutoSplitInfo.AutoSplitType.ToString(),
            a_resourceOperation.AutoSplitInfo.KeepSplitsOnSameResource,
            a_resourceOperation.AutoSplitInfo.MinAutoSplitAmount,
            a_resourceOperation.AutoSplitInfo.MaxAutoSplitAmount,
            a_resourceOperation.SetupSplitType.ToString(),
            a_resourceOperation.PreventSplitsFromIncurringSetup,
            a_resourceOperation.PreventSplitsFromIncurringClean,
            a_resourceOperation.TotalSetupCost,
            a_resourceOperation.SequenceHeadStartDays,
            a_resourceOperation.AllowSameLotInNonEmptyStorageArea,
            a_dbHelper.AdjustPublishTime(a_resourceOperation.JitStartDate)
        );

        a_resourceOperation.PtDbPopulateUserFields(jobOpRow, a_sd, a_dbHelper.UserFieldDefinitions);

        //Add Resource Requirements.  Must do before adding Activities since the Blocks have a relation to the Resource Reqts in the Dataset.
        a_resourceOperation.ResourceRequirements.PtDbPopulate(ref a_dataSet, jobOpRow);

        //Add Activities
        if (includeActivities)
        {
            a_resourceOperation.Activities.PtDbPopulate(a_sd, ref a_dataSet, jobOpRow, a_dbHelper);
        }

        if (a_publishInventory)
        {
            //Add Materials
            a_resourceOperation.MaterialRequirements.PtDbPopulate(ref a_dataSet, a_resourceOperation, jobOpRow, a_dbHelper);
        }

        a_resourceOperation.PtDbPopulate(ref a_dataSet, a_moRow, jobOpRow, a_publishInventory, a_dbHelper);
    }
}