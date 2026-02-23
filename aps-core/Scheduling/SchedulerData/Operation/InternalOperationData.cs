using PT.APSCommon.Extensions;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

public static class InternalOperationData
{
    /// <summary>
    /// Get the reported start date of the operation if it is finished.
    /// </summary>
    /// <param name="a_intOp"></param>
    /// <param name="o_startDate">This OUT argument is only valid if the operation has been finished.</param>
    /// <returns>Whether the operation has been finished.</returns>
    public static bool GetReportedStartDate(this InternalOperation a_intOp, out long o_startDate)
    {
        o_startDate = PTDateTime.MaxDateTimeTicks;

        for (int activityI = 0; activityI < a_intOp.Activities.Count; ++activityI)
        {
            InternalActivity internalActivity = a_intOp.Activities.GetByIndex(activityI);
            if (internalActivity.ProductionStatus == InternalActivityDefs.productionStatuses.Finished)
            {
                if (internalActivity.ReportedStartDateTicks < o_startDate)
                {
                    o_startDate = internalActivity.ReportedStartDateTicks;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public static Tuple<DateTime, string> CalculateLatestConstraint(this InternalOperation a_op, ScenarioDetail a_sd)
    {
        if (!a_op.Scheduled)
        {
            return new Tuple<DateTime, string>(PTDateTime.MinDateTime, "Not Scheduled".Localize());
        }

        DateTime startDate = a_op.ScheduledStartDate;
        //DateTime earliestAnchorDate = EarliestAnchorDate(a_op);
        //if (startDate == earliestAnchorDate)
        //{
        //    return new Tuple<DateTime, string>(startDate, "Anchored");
        //}

        if (a_op.LatestConstraintInternal == InternalOperation.LatestConstraintEnum.AnchorDate)
        {
            return new Tuple<DateTime, string>(a_op.LatestConstraintDate, a_op.LatestConstraint);
        }

        //Material.
        DateTime availability = a_op.CalculateEarliestMaterialAvailability(a_sd);
        if (availability == startDate)
        {
            //Check POs first
            for (int i = 0; i < a_op.MaterialRequirements.Count; i++)
            {
                MaterialRequirement requirement = a_op.MaterialRequirements[i];
                //Skip non-constraint
                if (requirement.NonConstraint)
                {
                    continue;
                }
                
                foreach (PurchaseToStock order in requirement.SupplyingOrders)
                {
                    if (order.ScheduledReceiptDate == startDate)
                    {
                        return new Tuple<DateTime, string>(startDate, string.Format("Expected supply from: {0}".Localize(), order.Name));
                    }
                }
            }

            //Then check supplying activities
            for (int i = 0; i < a_op.MaterialRequirements.Count; i++)
            {
                MaterialRequirement requirement = a_op.MaterialRequirements[i];
                //Skip non-constraint
                if (requirement.NonConstraint)
                {
                    continue;
                }
                
                foreach (InternalActivity activity in requirement.SupplyingActivities)
                {
                    Product product = activity.Operation.Products.GetByItemId(requirement.Item.Id);
                    DateTime materialAvailabilityDate;
                    switch (product.InventoryAvailableTiming)
                    {
                        case ProductDefs.EInventoryAvailableTimings.AtOperationRunStart:
                            materialAvailabilityDate = activity.ScheduledStartDate;
                            break;
                        case ProductDefs.EInventoryAvailableTimings.AtOperationRunEnd:
                        case ProductDefs.EInventoryAvailableTimings.ByProductionCycle:
                            //TODO: How should we implement this??
                            materialAvailabilityDate = activity.ScheduledEndOfRunDate;
                            break;
                        case ProductDefs.EInventoryAvailableTimings.DuringPostProcessing:
                            materialAvailabilityDate = activity.Batch.PostProcessingEndDateTime;
                            break;
                        case ProductDefs.EInventoryAvailableTimings.AtOperationResourcePostProcessingEnd:
                            materialAvailabilityDate = activity.Batch.PostProcessingEndDateTime;
                            break;
                        case ProductDefs.EInventoryAvailableTimings.DuringStorage:
                            materialAvailabilityDate = activity.Batch.PostProcessingEndDateTime;
                            break;
                        case ProductDefs.EInventoryAvailableTimings.AtStorageEnd:
                            materialAvailabilityDate = activity.Batch.PostProcessingEndDateTime;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    materialAvailabilityDate += product.MaterialPostProcessingSpan;
                    if (materialAvailabilityDate == startDate)
                    {
                        return new Tuple<DateTime, string>(startDate, string.Format("Expected supply from: {0}".Localize(), activity.Operation.ManufacturingOrder.Job.Name));
                    }
                }
            }

            //Finally check leadtimes and horizon
            for (int i = 0; i < a_op.MaterialRequirements.Count; i++)
            {
                MaterialRequirement requirement = a_op.MaterialRequirements[i];
                //Skip non-constraint
                if (requirement.NonConstraint)
                {
                    continue;
                }
                
                foreach (Adjustment adj in requirement.MRSupply)
                {
                    if (adj.AdjustmentType == InventoryDefs.EAdjustmentType.PastPlanningHorizon)
                    {
                        return new Tuple<DateTime, string>(a_sd.GetPlanningHorizonEnd(), string.Format("No Supply: {0}, {1}".Localize(), requirement.Item.Name, requirement.Item.Description).TrimEnd(' ', ','));
                    }

                    if (adj.AdjustmentType == InventoryDefs.EAdjustmentType.LeadTime)
                    {
                        return new Tuple<DateTime, string>(startDate, string.Format("Lead Time: {0}, {1}".Localize(), requirement.Item.Name, requirement.Item.Description).TrimEnd(' ', ','));
                    }
                }
            }
        }

        if (a_op.LatestConstraintInternal == InternalOperation.LatestConstraintEnum.MaterialRequirement)
        {
            //scheduled as soon as material was available during simulation. This must be lead time or expected inventory
            DateTime latestConstraintTime = availability;

            MaterialRequirement mrConstraint = a_op.LatestConstraintMaterialRequirement;
            string latestConstraint = string.Empty;

            if (mrConstraint == null)
            {
                return new Tuple<DateTime, string>(a_op.LatestConstraintDate, latestConstraint);
            }

            if (mrConstraint.BuyDirect)
            {
                latestConstraint = string.Format("Buy-Direct: {0}".Localize(), mrConstraint.MaterialName);
            }
            else
            {
                latestConstraint = string.Format("{0} ({1})", mrConstraint.Item.Name, mrConstraint.UnIssuedQty);
            }

            //Add more details if the warehouse is available
            if (mrConstraint.Warehouse != null)
            {
                Inventory inv = mrConstraint.Warehouse.Inventories[mrConstraint.Item.Id];
                foreach (Adjustment adj in mrConstraint.MRSupply)
                {
                    if (adj.AdjustmentType == InventoryDefs.EAdjustmentType.PastPlanningHorizon)
                    {
                        latestConstraintTime = a_sd.GetPlanningHorizonEnd();
                        latestConstraint = string.Format("No Supply: {0}, {1}".Localize(), mrConstraint.Item.Name, mrConstraint.Item.Description).TrimEnd(' ', ',');
                        break;
                    }

                    if (adj.AdjustmentType == InventoryDefs.EAdjustmentType.LeadTime)
                    {
                        if (a_sd.ClockDate.Add(inv.LeadTime) >= latestConstraintTime)
                        {
                            latestConstraintTime = a_sd.ClockDate.Add(inv.LeadTime);
                            latestConstraint = string.Format("Lead Time: {0}, {1}".Localize(), mrConstraint.Item.Name, mrConstraint.Item.Description).TrimEnd(' ', ',');
                        }
                    }
                }

                foreach (InternalActivity activity in mrConstraint.SupplyingActivities)
                {
                    if (activity.ScheduledEndDate > latestConstraintTime)
                    {
                        latestConstraintTime = activity.ScheduledEndDate;
                        latestConstraint = string.Format("Expected supply from: {0}".Localize(), activity.Operation.ManufacturingOrder.Job.Name);
                    }
                }
                //TODO: Uncomment when MaterialRequirement.cs is available
                //foreach (PurchaseToStock order in requirement.SupplyingOrders)
                //{
                //    if (order.ScheduledReceiptDate > latestConstraintTime)
                //    {
                //        latestConstraintTime = order.ScheduledReceiptDate;
                //        latestConstraint = String.Format("Expected supply from: {0}".Localize(), order.Name);
                //    }
                //}
            }

            return new Tuple<DateTime, string>(a_op.LatestConstraintDate, latestConstraint);
        }

        return new Tuple<DateTime, string>(a_op.LatestConstraintDate, a_op.LatestConstraint);
    }

    public static DateTime EarliestAnchorDate(this InternalOperation a_op)
    {
        DateTime maxDate = PTDateTime.MaxDateTime;
        for (int i = 0; i < a_op.Activities.Count; i++)
        {
            InternalActivity activity = a_op.Activities.GetByIndex(i);
            if (activity.Anchored && activity.Scheduled)
            {
                maxDate = PTDateTime.Min(maxDate, activity.AnchorDate);
            }
        }

        return maxDate;
    }
}