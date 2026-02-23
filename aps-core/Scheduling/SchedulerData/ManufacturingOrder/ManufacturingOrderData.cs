using PT.APSCommon;
using PT.Database;
using PT.PackageDefinitions.Settings.PublishOptions;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

public static class ManufacturingOrderData
{
    public static void PtDbPopulate(this ManufacturingOrder a_manufacturingOrder,
                                    ScenarioDetail sd,
                                    ref PtDbDataSet dataSet,
                                    PtDbDataSet.JobsRow jobRow,
                                    bool publishInventory,
                                    bool limitToResourceList,
                                    HashSet<BaseId> resourceIds,
                                    PTDatabaseHelper a_dbHelper)
    {
        ScenarioPublishDataLimits scenarioPublishDataLimits = new ScenarioPublishDataLimits();
        using (sd.Scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
        {
            scenarioPublishDataLimits = ss.ScenarioSettings.LoadSetting(scenarioPublishDataLimits);
        }

        bool includeOperations = scenarioPublishDataLimits.PublishOperations || a_dbHelper.PublishAllData;

        long lockedPlantId = -1;
        if (a_manufacturingOrder.LockedPlant != null)
        {
            lockedPlantId = a_manufacturingOrder.LockedPlant.Id.ToBaseType();
        }

        //Add MO row
        PtDbDataSet.ManufacturingOrdersRow moRow = dataSet.ManufacturingOrders.AddManufacturingOrdersRow(
            jobRow.PublishDate,
            jobRow.InstanceId,
            jobRow.JobId,
            a_manufacturingOrder.Id.ToBaseType(),
            a_manufacturingOrder.Name,
            a_manufacturingOrder.RequiredQty,
            a_manufacturingOrder.ExpectedFinishQty,
            a_manufacturingOrder.Description,
            a_manufacturingOrder.ProductName,
            a_manufacturingOrder.ProductDescription,
            a_manufacturingOrder.Notes,
            a_manufacturingOrder.Scheduled,
            a_dbHelper.AdjustPublishTime(a_manufacturingOrder.ScheduledStartDate.ToDateTimeOffsetUtc()).ToDateTime(),
            a_dbHelper.AdjustPublishTime(a_manufacturingOrder.ScheduledEndDate.ToDateTimeOffsetUtc()).ToDateTime(),
            a_manufacturingOrder.CanSpanPlants,
            a_manufacturingOrder.CurrentPath.Id.ToBaseType(),
            a_manufacturingOrder.DefaultPath.Id.ToBaseType(),
            a_manufacturingOrder.Family,
            a_manufacturingOrder.Finished,
            a_manufacturingOrder.Anchored.ToString(),
            a_manufacturingOrder.OnHold.ToString(),
            a_manufacturingOrder.HoldReason,
            a_manufacturingOrder.Late,
            a_manufacturingOrder.Lateness.TotalDays,
            a_manufacturingOrder.LeadTime.TotalDays,
            a_manufacturingOrder.Locked.ToString(),
            lockedPlantId,
            a_manufacturingOrder.UOM,
            a_manufacturingOrder.ExternalId,
            a_manufacturingOrder.Anchored.ToString(),
            a_manufacturingOrder.BreakOffSourceMoName,
            a_manufacturingOrder.CopyRoutingFromTemplate,
            a_dbHelper.AdjustPublishTime(a_manufacturingOrder.HoldUntil.ToDateTimeOffsetUtc()).ToDateTime(),
            a_manufacturingOrder.IsBreakOff,
            a_dbHelper.AdjustPublishTime(a_manufacturingOrder.NeedDate.ToDateTimeOffsetUtc()).ToDateTime(),
            a_manufacturingOrder.PercentFinished,
            a_manufacturingOrder.ProductColor.Name,
            a_manufacturingOrder.MoNeedDate,
            a_manufacturingOrder.StandardHours,
            a_manufacturingOrder.Bottlenecks,
            a_manufacturingOrder.DefaultPathName,
            a_manufacturingOrder.ExpectedRunHours,
            a_manufacturingOrder.ExpectedSetupHours,
            a_manufacturingOrder.Hold,
            a_manufacturingOrder.LaborCost,
            a_manufacturingOrder.LockedPlantName,
            a_manufacturingOrder.MachineCost,
            a_manufacturingOrder.MaterialCost,
            a_manufacturingOrder.MoNeedDate,
            a_manufacturingOrder.PreserveRequiredQty,
            a_manufacturingOrder.ReportedRunHours,
            a_manufacturingOrder.ReportedSetupHours,
            a_manufacturingOrder.RequestedQty,
            a_manufacturingOrder.SchedulingHours,
            a_manufacturingOrder.AutoJoinGroup,
            a_manufacturingOrder.Split,
            a_manufacturingOrder.SplitCount,
            a_manufacturingOrder.SplitFromManufacturingOrderId.ToBaseType(),
            a_manufacturingOrder.Started,
            a_manufacturingOrder.LockToCurrentAlternatePath,
            a_manufacturingOrder.ShippingBufferOverride.HasValue ? a_manufacturingOrder.ShippingBufferOverride.Value.Ticks : 0,
            a_dbHelper.AdjustPublishTime(a_manufacturingOrder.GetReleaseDate().ToDateTimeOffsetUtc()).ToDateTime(),
            a_dbHelper.AdjustPublishTime(a_manufacturingOrder.ShippingDueDate.ToDateTimeOffsetUtc()).ToDateTime(),
            Convert.ToDouble(a_manufacturingOrder.ShippingBufferCurrentPenetrationPercent),
            Convert.ToDouble(a_manufacturingOrder.ShippingBufferProjectedPenetrationPercent),
            a_manufacturingOrder.ResizeForStorage
        );

        a_manufacturingOrder.PtDbPopulateUserFields(moRow, sd, a_dbHelper.UserFieldDefinitions);

        //Add Operations
        if (includeOperations)
        {
            //PtDbPopulate(sd, ref dataSet, moRow, maxPublishDate, publishInventory, limitToResourceList, resourceIds, a_includeAllData);
            a_manufacturingOrder.OperationManager.PtDbPopulate(sd, ref dataSet, moRow, publishInventory, limitToResourceList, resourceIds, a_dbHelper);
        }

        //Paths
        for (int pathI = 0; pathI < a_manufacturingOrder.AlternatePaths.Count; pathI++)
        {
            a_manufacturingOrder.AlternatePaths[pathI].PtDbPopulate(ref dataSet, moRow);
        }

        //Add Successor MOs
        for (int i = 0; i < a_manufacturingOrder.SuccessorMOs.Count; i++)
        {
            SuccessorMO sucMO = a_manufacturingOrder.SuccessorMOs[i];
            if (sucMO.SuccessorManufacturingOrder != null)
            {
                BaseId altPathId = new (-1);
                BaseId sucOpId = new (-1);
                if (sucMO.AlternatePath != null)
                {
                    altPathId = sucMO.AlternatePath.Id;
                }

                if (sucMO.Operation != null)
                {
                    sucOpId = sucMO.Operation.Id;
                }

                dataSet.JobSuccessorManufacturingOrders.AddJobSuccessorManufacturingOrdersRow(
                    jobRow.PublishDate,
                    jobRow.InstanceId,
                    a_manufacturingOrder.Job.Id.ToBaseType(),
                    a_manufacturingOrder.Id.ToBaseType(),
                    sucMO.SuccessorManufacturingOrder.Job.Id.ToBaseType(),
                    sucMO.SuccessorManufacturingOrder.Id.ToBaseType(),
                    altPathId.ToBaseType(),
                    sucOpId.ToBaseType(),
                    TimeSpan.FromTicks(sucMO.TransferSpan).TotalHours,
                    sucMO.UsageQtyPerCycle
                );
            }
        }
    }

    #region PT Database
    public static void PtDbPopulate(this ManufacturingOrderManager a_manufacturingOrderManager, ScenarioDetail sd, ref PtDbDataSet dataSet, PtDbDataSet.JobsRow jobRow, bool publishInventory, bool limitToResourceList, HashSet<BaseId> resourceIds, PTDatabaseHelper a_dbHelper)
    {
        for (int i = 0; i < a_manufacturingOrderManager.Count; i++)
        {
            ManufacturingOrder mo = a_manufacturingOrderManager[i];
            if (!(mo.Scheduled && mo.GetScheduledStartDate().Ticks > a_dbHelper.MaxPublishDate.Ticks))
            {
                mo.PtDbPopulate(sd, ref dataSet, jobRow, publishInventory, limitToResourceList, resourceIds, a_dbHelper);
            }
        }
    }
    #endregion
}