using System.Text;

using PT.APSCommon;
using PT.Common.Sql.SqlServer;
using PT.Database;
using PT.PackageDefinitions.Settings.PublishOptions;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.SchedulerExtensions.Job;
using PT.SchedulerExtensions.Operations;

using static PT.SchedulerDefinitions.BaseOperationDefs;

namespace PT.SchedulerData;

public static class JobData
{
    #region PT Database
    public static void PtDbPopulate(this Job a_job,
                                    ScenarioDetail sd,
                                    ref PtDbDataSet dataSet,
                                    PtDbDataSet.SchedulesRow schedulesRow,
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
        bool includeManufacturingOrders = scenarioPublishDataLimits.PublishManufacturingOrders || a_dbHelper.PublishAllData;

        //Values that don't exist in Scheduler.Job
        string jobNotes = a_job.Notes;
        if (jobNotes != null && jobNotes.Length > SqlConstants.MAX_SQL_STRING_LENGTH)
        {
            jobNotes = jobNotes.Substring(0, SqlConstants.MAX_SQL_STRING_LENGTH);
        }

        PtDbDataSet.JobsRow jobRow = dataSet.Jobs.AddJobsRow(
            schedulesRow,
            schedulesRow.InstanceId,
            a_job.Id.ToBaseType(),
            a_job.Customers.GetCustomerExternalIdsList(),
            a_dbHelper.AdjustPublishTime(a_job.EntryDate.ToDateTime()),
            a_dbHelper.AdjustPublishTime(a_job.NeedDateTime),
            a_job.Classification.ToString(),
            a_job.Commitment.ToString(),
            a_job.Hot,
            a_job.HotReason,
            a_job.Importance,
            a_job.Cancelled,
            a_job.LatePenaltyCost,
            a_job.MaxEarlyDeliverySpan.TotalDays,
            a_job.Priority,
            a_job.Type,
            a_job.Revenue,
            a_job.Profit,
            a_job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled,
            a_dbHelper.AdjustPublishTime(a_job.ScheduledStartDate),
            a_dbHelper.AdjustPublishTime(a_job.ScheduledEndDate),
            a_job.GetLeadResource()?.ExternalId,
            a_job.CalculateStartsInDays(sd),
            a_job.Lateness.TotalDays,
            a_job.Late,
            a_job.Overdue,
            a_job.Description,
            jobNotes,
            a_job.Finished,
            a_job.Name,
            a_job.DoNotDelete,
            a_job.ExternalId,
            a_job.Locked.ToString(),
            a_job.Anchored.ToString(),
            a_job.OrderNumber,
            a_job.OnHold.ToString(),
            a_job.HoldReason,
            a_job.Template,
            a_job.CustomerEmail,
            a_job.AgentEmail,
            a_job.DoNotSchedule,
            ColorUtils.ConvertColorToHexString(a_job.ColorCode),
            a_job.MaintenanceMethod.ToString(),
            a_dbHelper.AdjustPublishTime(a_job.HoldUntil),
            a_job.PercentFinished,
            a_job.ScheduledStatus.ToString(),
            a_job.StandardHours,
            a_job.Bottlenecks,
            a_job.CanSpanPlants,
            a_job.CommitmentPreserved,
            a_job.DoNotDeletePreserved,
            a_job.DoNotSchedulePreserved,
            a_dbHelper.AdjustPublishTime(a_job.EarliestDelivery),
            a_job.GetEnteredToday(),
            a_job.ExpectedRunHours,
            a_job.ExpectedSetupHours,
            a_job.DetermineFailedToScheduleReason(sd),
            a_job.Hold,
            a_job.HoldReason,
            a_job.LaborCost,
            a_job.MachineCost,
            a_job.MaterialCost,
            a_job.OverdueSpan.TotalDays,
            a_job.PercentOfStandardHrs,
            a_job.PercentOverStandardHrs,
            a_job.Product,
            a_job.ProductDescription,
            a_job.Qty,
            a_job.ReportedRunHours,
            a_job.ReportedSetupHours,
            a_job.SchedulingHours,
            a_job.Started,
            a_job.Throughput,
            a_job.ShippingCost,
            a_job.ExpectedLatePenaltyCost,
            a_job.SubcontractCost,
            a_job.TotalCost,
            a_job.Printed,
            a_job.Invoiced,
            a_job.Shipped.ToString(),
            a_job.Destination,
            a_job.Reviewed,
            a_job.PercentOfMaterialsAvailable,
            a_job.SuccessorOrderNumbers,
            a_job.ResourceNames,
            a_job.LowLevelCode
        ); //Analysis is unused because it can be too long and cause memory overflows

        a_job.PtDbPopulateUserFields(jobRow, sd, a_dbHelper.UserFieldDefinitions);

        //Add Manufacturing Orders
        if (includeManufacturingOrders)
        {
            a_job.ManufacturingOrders.PtDbPopulate(sd, ref dataSet, jobRow, publishInventory, limitToResourceList, resourceIds, a_dbHelper);
        }
    }

    public static void PtDbPopulate(this JobManager a_jobManager,
                                    ScenarioDetail sd,
                                    ref PtDbDataSet dataSet,
                                    PlantManager plants,
                                    PtDbDataSet.SchedulesRow schedulesRow,
                                    bool publishInventory,
                                    bool limitToJobIdList,
                                    HashSet<BaseId> jobIdsToInclude,
                                    bool limitToResourceList,
                                    HashSet<BaseId> resourceIds,
                                    PTDatabaseHelper a_dbHelper)
    {
        ScenarioPublishDataLimits scenarioPublishDataLimits = new ScenarioPublishDataLimits();
        using (sd.Scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
        {
            scenarioPublishDataLimits = ss.ScenarioSettings.LoadSetting(scenarioPublishDataLimits);
        }
        plants.PreProcessWorkForPtDbPopulate();

        bool includeTemplates = scenarioPublishDataLimits.PublishTemplates || a_dbHelper.PublishAllData;

        for (int i = 0; i < a_jobManager.Count; i++)
        {
            Job job = a_jobManager[i];
            if (!includeTemplates && job.Template)
            {
                continue;
            }

            if ((!limitToJobIdList || jobIdsToInclude.Contains(job.Id)) &&
                !((job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled) & (job.ScheduledStartDate.Ticks > a_dbHelper.MaxPublishDate.Ticks)))
            {
                bool includeThisJob;
                if (limitToResourceList)
                {
                    includeThisJob = false;
                    List<InternalResource> resourcesUsed = job.GetResourcesScheduled();
                    for (int resI = 0; resI < resourcesUsed.Count; resI++)
                    {
                        if (resourceIds.Contains(resourcesUsed[resI].Id))
                        {
                            includeThisJob = true;
                            break;
                        }
                    }
                }
                else
                {
                    includeThisJob = true;
                }

                if (includeThisJob)
                {
                    job.PtDbPopulate(sd, ref dataSet, schedulesRow, publishInventory, limitToResourceList, resourceIds, a_dbHelper);
                }
            }
        }
    }
    #endregion

    public static string GetLeadEligibleResources(this Job a_job, ScenarioDetail a_sd)
    {
        StringBuilder list = new ();
        List<Resource> resourceList = a_sd.PlantManager.GetResourceList();

        ManufacturingOrder firstMO = a_job.GetFirstMO(); //Should we be getting the lead MO instead?

        foreach (AlternatePath path in firstMO.AlternatePaths)
        {
            list.Append(path.GetLeadEligibleResources());
        }

        return list.ToString();
    }

    public static string GetLeadCapabilities(this Job a_job)
    {
        List<InternalOperation> leadOps = new ();
        InternalOperation op = (InternalOperation)a_job.GetFirstMO().GetLeadOperation();

        if (op == null)
        {
            //Try getting unscheduled lead op
            IEnumerable<BaseOperation> ops = a_job.GetFirstMO().GetLeadUnscheduledOperations();
            leadOps.AddRange(ops.Cast<InternalOperation>());
        }
        else
        {
            leadOps.Add(op);
        }

        if (leadOps.Count == 0)
        {
            //This job cannot be scheduled
            return string.Empty;
        }

        return leadOps[0].RequiredCapabilities;
    }

    public static string GetEligibility(this Job a_job, ScenarioDetail a_sd)
    {
        StringBuilder jobStringBuilder = new ();

        //Checks if system setting requires all alternate paths to be eligible
        bool allPathsRequired = a_sd.ScenarioOptions.UnsatisfiableJobPathHandling == ScenarioOptions.EUnsatisfiableJobPathHandlingEnum.ExcludeJob;
        foreach (ManufacturingOrder manufacturingOrder in a_job.ManufacturingOrders)
        {
            if (!manufacturingOrder.Scheduled)
            {
                StringBuilder moStringBuilder = new ();
                if (manufacturingOrder.IsFinishedOrOmitted)
                {
                    jobStringBuilder.Append(string.Format("MO '{0}' eligibility issues:  did not schedule because all Ops were marked omitted. ".Localize(), manufacturingOrder.Name));
                    continue;
                }

                bool pathIneligible = false;
                AlternatePath currentPath = manufacturingOrder.CurrentPath;
                moStringBuilder.Append(string.Format("MO '{0}' eligibility issues: ".Localize(), manufacturingOrder.Name));

                //Validate path validity dates
                if (currentPath.ValidityStartDate.Ticks >= a_sd.GetPlanningHorizonEnd().Ticks)
                {
                    pathIneligible = true;

                    if (allPathsRequired)
                    {
                        //short-circuit and return because the MO cannot be scheduled and the system setting requires all paths to be eligible
                        moStringBuilder.Append(string.Format("Validity start date for Path '{0}' after the planning horizon and the option to schedule MO's as long as at least one path is eligible is disabled. ".Localize(), currentPath.Name));

                        return moStringBuilder.ToString();


                    }
                    else
                    {
                        moStringBuilder.Append(string.Format("Validity start date for Path '{0}' after the planning horizon. ".Localize(), currentPath.Name));
                    }
                }

                if (currentPath.ValidityEndDate.Ticks <= a_sd.ClockDate.Ticks)
                {
                    pathIneligible = true;

                    if (allPathsRequired)
                    {
                        //short-circuit and return because the MO cannot be scheduled and the system setting requires all paths to be eligible
                        moStringBuilder.Append(string.Format("Validity end date for Path '{0}' before the clock date and the option to schedule MO's as long as at least one path is eligible is disabled. ".Localize(), currentPath.Name));

                        return moStringBuilder.ToString();
                    }
                    else
                    {
                        moStringBuilder.Append(string.Format("Validity end date for Path '{0}' before the clock date. ".Localize(), currentPath.Name));
                    }
                }

                if (!pathIneligible)
                {
                    for (int pathIndex = 0; pathIndex < currentPath.NodeCount; pathIndex++)
                    {
                        InternalOperation altOp = currentPath.GetNodeByIndex(pathIndex).Operation;
                        StringBuilder opStringBuilder = new();
                        opStringBuilder.Append(string.Format("Operation '{0}' eligibility issues: ".Localize(), altOp.Name));

                        if (OperationIsIneligible(altOp, a_sd.ProductRuleManager, opStringBuilder))
                        {
                            pathIneligible = true;
                            //Op is ineligible, if all paths are required to be eligible, append a msg to show the user that the setting to allow jobs with ineligible paths
                            //to schedule as long as at least one path is satisfiable is disabled
                            if (allPathsRequired)
                            {
                                moStringBuilder.Append(string.Format("Path '{0}' is ineligible and the option to schedule MO's as long as at least one path is eligible is disabled. ".Localize(), currentPath.Name));
                                moStringBuilder.Append(opStringBuilder.ToString());

                                return moStringBuilder.ToString();
                            }
                            else
                            {
                                moStringBuilder.Append(string.Format("Path '{0}' is ineligible. ".Localize(), currentPath.Name));
                                moStringBuilder.Append(opStringBuilder.ToString());
                            }
                        }
                    }
                }

                if (!pathIneligible)
                {
                    //Clear so that the MO header string doesn't get added twice unnecessarily.
                    moStringBuilder.Clear();
                }

                //Current path is not eligible, evaluate alternate paths
                foreach (AlternatePath alternatePath in manufacturingOrder.AlternatePaths)
                {
                    if (currentPath.ExternalId.Equals(alternatePath.ExternalId))
                    {
                        continue;
                    }

                    moStringBuilder.Append(string.Format("MO '{0}' eligibility issues: ".Localize(), manufacturingOrder.Name));

                    //Validate path validity dates
                    if (alternatePath.ValidityStartDate.Ticks >= a_sd.GetPlanningHorizonEnd().Ticks)
                    {
                        pathIneligible = true;

                        if (allPathsRequired)
                        {
                            //short-circuit and return because the MO cannot be scheduled and the system setting requires all paths to be eligible
                            moStringBuilder.Append(string.Format("Validity start date for Path '{0}' after the planning horizon and the option to schedule MO's as long as at least one path is eligible is disabled. ".Localize(), alternatePath.Name));

                            return moStringBuilder.ToString();
                        }
                        else
                        {
                            moStringBuilder.Append(string.Format("Validity start date for Path '{0}' after the planning horizon. ".Localize(), alternatePath.Name));
                        }
                    }

                    if (alternatePath.ValidityEndDate.Ticks <= a_sd.ClockDate.Ticks)
                    {
                        pathIneligible = true;

                        if (allPathsRequired)
                        {
                            //short-circuit and return because the MO cannot be scheduled and the system setting requires all paths to be eligible
                            moStringBuilder.Append(string.Format("Validity end date for Path '{0}' before the clock date and the option to schedule MO's as long as at least one path is eligible is disabled. ".Localize(), alternatePath.Name));

                            return moStringBuilder.ToString();
                        }
                        else
                        {
                            moStringBuilder.Append(string.Format("Validity end date for Path '{0}' before the clock date. ".Localize(), alternatePath.Name));
                        }
                    }

                    if (!pathIneligible)
                    {
                        for (int pathIndex = 0; pathIndex < alternatePath.NodeCount; pathIndex++)
                        {
                            InternalOperation altOp = alternatePath.GetNodeByIndex(pathIndex).Operation;
                            StringBuilder opStringBuilder = new();
                            opStringBuilder.Append(string.Format("Operation '{0}' eligibility issues: ".Localize(), altOp.Name));

                            if (OperationIsIneligible(altOp, a_sd.ProductRuleManager, opStringBuilder))
                            {
                                pathIneligible = true;

                                //Op is ineligible, if all paths are required to be eligible, append a msg to show the user that the setting to allow jobs with ineligible paths
                                //to schedule as long as at least one path is satisfiable is disabled
                                if (allPathsRequired)
                                {
                                    moStringBuilder.Append(string.Format("Path '{0}' is ineligible and the option to schedule MO's as long as at least one path is eligible is disabled. ".Localize(), alternatePath.Name));
                                    moStringBuilder.Append(opStringBuilder.ToString());

                                    return moStringBuilder.ToString();
                                }
                                else
                                {
                                    moStringBuilder.Append(string.Format("Path '{0}' is ineligible. ".Localize(), alternatePath.Name));
                                    moStringBuilder.Append(opStringBuilder.ToString());
                                }
                            }
                        }
                    }

                    if (!pathIneligible)
                    {
                        //This MO is simply unscheduled, nothing to add in the eligibility string.
                        moStringBuilder.Clear();
                    }
                }

                if (moStringBuilder.Length > 0)
                {
                    jobStringBuilder.Append(moStringBuilder.ToString());
                }
            }
        }

        return jobStringBuilder.ToString();
    }
    /// <summary>
    /// Evaluates if an operation is eligible, appends ineligibility reason if any, and returns true if there is a reason an operation cannot
    /// be scheduled and the system requires all paths to be satisfiable to be eligible for scheduling
    /// </summary>
    private static bool OperationIsIneligible(InternalOperation a_operation, ProductRuleManager a_productRuleManager, StringBuilder a_opStringBuilder)
    {
        //Skips if operation is either finished or omitted
        if (a_operation.Finished || a_operation.Omitted != omitStatuses.NotOmitted)
        {
            return false;
        }

        //If the system does not require all paths to be eligible for a job to schedule,
        //check and return if at least one resource can schedule the op
        foreach (ResourceRequirement requirement in a_operation.ResourceRequirements.Requirements)
        {
            if (requirement.CapabilityManager.Count == 0)
            {
                //short-circuit, no capabilities mapped to the requirement
                a_opStringBuilder.Append(string.Format("Resource Requirement '{0}' has no Capabilities mapped.".Localize(), requirement.ExternalId));
                return true;
            }

            foreach (Capability capabilityObj in requirement.CapabilityManager)
            {
                InternalResourceList resList = capabilityObj.Resources;
                if (resList.Count == 0)
                {
                    //short-circuit, no resources mapped to the capability
                    a_opStringBuilder.Append(string.Format("Capability '{0}' has no mapped Resources.".Localize(), capabilityObj.Name));
                    return true;
                }

                bool nonManualScheduledResourceFound = false;
                for (int i = 0; i < resList.Count; i++)
                {
                    if (resList.GetByIndex(i) is Resource resource)
                    {
                        if (!resource.ManualSchedulingOnly)
                        {
                            nonManualScheduledResourceFound = true;
                            break;
                        }
                    }
                }

                if (!nonManualScheduledResourceFound)
                {
                    //short-circuit, all resources mapped to the capability are manual schedule only
                    a_opStringBuilder.Append(string.Format("All Resources mapped to Capability '{0}' are marked ManualScheduleOnly.".Localize(), capabilityObj.Name));
                    return true;
                }

                bool eligibleResourceFound = false;
                for (var i = 0; i < resList.Count; i++)
                {
                    if (resList.GetByIndex(i) is Resource resource)
                    {
                        StringBuilder currentReasonStringBuilder = new();
                        if (!resource.Active)
                        {
                            a_opStringBuilder.Append(string.Format("Resource '{0}' mapped to Capability '{1}' is not Active.".Localize(), resource.Name, capabilityObj.Name));
                            continue;
                        }

                        if (!resource.ResourceCapacityIntervals.HasOnlineCapacity())
                        {
                            a_opStringBuilder.Append(string.Format("Resource '{0}' mapped to Capability '{1}' has no Online intervals.".Localize(), resource.Name, capabilityObj.Name));
                            continue;
                        }

                        if (!requirement.IsEligible(resource, a_productRuleManager, currentReasonStringBuilder))
                        {
                            a_opStringBuilder.Append(currentReasonStringBuilder.ToString());
                        }
                        else
                        {
                            //Resource is eligible, no need to check further
                            eligibleResourceFound = true;
                            break;
                        }

                    }
                }

                if (!eligibleResourceFound)
                {
                    //short-circuit, no eligible resources found for this capability
                    return true;
                }
            }
        }

        //All resource requirements satisfied
        return false;
    }
}