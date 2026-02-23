using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Debugging;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerExtensions.Operations;

public static class BaseOperationExtensions
{
    public static TimeSpan? CommitmentStartDrift(this BaseOperation a_baseOperation)
    {
        return CheckBeforeGrabbingTimeSpanDifference(a_baseOperation.ScheduledStartDate, a_baseOperation.CommitStartDate, a_baseOperation.Scheduled);
    }

    public static TimeSpan? CommitmentEndDrift(this BaseOperation a_baseOperation)
    {
        return CheckBeforeGrabbingTimeSpanDifference(a_baseOperation.ScheduledEndDate, a_baseOperation.CommitEndDate, a_baseOperation.Scheduled);
    }

    private static TimeSpan? CheckBeforeGrabbingTimeSpanDifference(DateTime a_dateStart, DateTime a_dateEnd, bool a_operationScheduled)
    {
        if (!a_operationScheduled)
        {
            return null;
        }

        if (!PTDateTime.IsValidDateTimeBetweenMinMax(a_dateEnd))
        {
            return null;
        }

        try
        {
            return a_dateStart - a_dateEnd;
        }
        catch (Exception e)
        {
            DebugException.ThrowInDebug(e.Message);
        }

        return null;
    }

    /// <summary>
    /// Browsable list of all Material Requirements for the Operation that are not Available.
    /// </summary>
    public static string GetMaterialsNotPlannedWithProjectedInventory(this BaseOperation a_op, ScenarioDetail a_sd, BaseOperation a_projectedOp)
    {
        System.Text.StringBuilder builder = new ();
        for (int i = 0; i < a_op.MaterialRequirements.Count; i++)
        {
            MaterialRequirement mr = a_op.MaterialRequirements[i];
            if (!mr.Planned && !mr.Available)
            {
                if (builder.Length > 0)
                {
                    builder.Append(Environment.NewLine);
                }

                decimal leadTimeDays = -1;
                DateTimeOffset nowPlusLeadTimeLocal = PTDateTime.MinValue;
                if (mr.Item != null)
                {
                    decimal projectInvAtOpStart = 0;
                    if (mr.Warehouse != null)
                    {
                        if (mr.Warehouse.Inventories.Contains(mr.Item.Id))
                        {
                            Scheduler.Inventory inv = mr.Warehouse.Inventories[mr.Item.Id];
                            projectInvAtOpStart = inv.GetProjectedInventoryAtDate(a_projectedOp.StartDateTime, a_sd);
                            leadTimeDays = (decimal)inv.LeadTime.TotalDays;
                            nowPlusLeadTimeLocal = a_sd.ClockDate.Add(inv.LeadTime).ToDisplayTime();
                        }
                    }
                    else
                    {
                        //Check all warehouses
                        for (int wI = 0; wI < a_sd.WarehouseManager.Count; wI++)
                        {
                            Warehouse warehouse = a_sd.WarehouseManager.GetByIndex(wI);
                            if (warehouse.Inventories.Contains(mr.Item.Id))
                            {
                                Scheduler.Inventory inv = warehouse.Inventories[mr.Item.Id];
                                decimal warehouseInvProj = inv.GetProjectedInventoryAtDate(a_projectedOp.StartDateTime, a_sd);
                                if (warehouseInvProj > 0)
                                {
                                    projectInvAtOpStart = projectInvAtOpStart + warehouseInvProj;
                                }
                            }
                        }

                        leadTimeDays = (decimal)mr.Item.DefaultLeadTime.TotalDays;
                        nowPlusLeadTimeLocal = a_sd.ClockDate.Add(mr.Item.DefaultLeadTime).ToDisplayTime();
                    }

                    string txt = string.Format("{0} {1}:  UnIssued Qty: {2}, {3} Projected OnHand: {4}, Lead Time Days: {5}, Now+LeadTime: {6}".Localize(), mr.MaterialName, mr.MaterialDescription, Math.Round(mr.UnIssuedQty, 2), mr.UOM, Math.Round(projectInvAtOpStart, 2), leadTimeDays, nowPlusLeadTimeLocal);
                    builder.Append(txt);
                }
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gets material status, ignoring unknown and planned sources for item types not included in the provided item types list
    /// </summary>
    public static BaseOperation.MaterialStatuses GetMaterialStatusWithTypes(this BaseOperation a_op, DateTime a_clockDate, List<ItemDefs.itemTypes> a_typesToInclude)
    {
        int availMrCount = 0;
        MaterialRequirementsCollection materials = a_op.MaterialRequirements;
        int filteredMaterialCount = 0;
        bool atLeastOneSupplyIsPlanned = false; // there at least one supply that is a PO or Job that is planned or estimate (or not firm for PO).
        InternalOperation op = a_op as InternalOperation;
        //Some materials have not been fully issued.
        for (int mrI = 0; mrI < materials.Count; mrI++)
        {
            MaterialRequirement mr = materials[mrI];

            if (mr.IssuedComplete || mr.Available)
            {
                availMrCount++;
            }
            else if (mr.Item != null && !a_typesToInclude.Contains(mr.Item.ItemType))
            {
                filteredMaterialCount++;
            }
            else if (op.Scheduled && mr.GetUnavailableQty(a_clockDate, op, op.GetLeadActivity()) > 0 && 
                     ((mr.ConstraintType == MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByAvailableDate) ||
                    (mr.ConstraintType == MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByEarlierOfLeadTimeOrAvailableDate && a_op.ScheduledEndDate > a_clockDate.Add(mr.LeadTimeSpan))))
            {
                return BaseOperation.MaterialStatuses.MaterialsIgnoredConstraintViolation;
            }
            else if (mr.BuyDirect || !mr.Planned)
            {
                return BaseOperation.MaterialStatuses.MaterialSourcesUnknown;
            }
            else
            {
                atLeastOneSupplyIsPlanned = atLeastOneSupplyIsPlanned || mr.AreAnySuppliesPlanned();
            }
        }

        if (availMrCount + filteredMaterialCount == materials.Count)
        {
            return BaseOperation.MaterialStatuses.MaterialsAvailable;
        }

        if (atLeastOneSupplyIsPlanned)
        {
            return BaseOperation.MaterialStatuses.MaterialSourcesPlanned;
        }

        return BaseOperation.MaterialStatuses.MaterialSourcesFirmed;
    }

    /// <summary>
    /// Returns whether the specified id is an operation successor
    /// </summary>
    public static bool ContainsOpAsSuccessor(this BaseOperation a_op, BaseId a_possibleSuccessorId)
    {
        for (int i = 0; i < a_op.Successors.Count; i++)
        {
            AlternatePath.Association opSuccessor = a_op.Successors[i];
            BaseOperation operation = opSuccessor?.Successor?.Operation;
            if (operation != null)
            {
                if (operation.Id == a_possibleSuccessorId)
                {
                    return true;
                }

                if (ContainsOpAsSuccessor(operation, a_possibleSuccessorId))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Returns whether the specified id is an operation predecessor
    /// </summary>
    public static bool ContainsOpAsPredecessor(this BaseOperation a_op, BaseId a_possibleSuccessorId)
    {
        for (int i = 0; i < a_op.Predecessors.Count; i++)
        {
            AlternatePath.Association opSuccessor = a_op.Predecessors[i];
            BaseOperation operation = opSuccessor?.Predecessor?.Operation;
            if (operation != null)
            {
                if (operation.Id == a_possibleSuccessorId)
                {
                    return true;
                }

                if (ContainsOpAsPredecessor(operation, a_possibleSuccessorId))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the datetime and description of the latest material receipt
    /// </summary>
    public static Tuple<DateTime, string> LatestMaterialReceipt(this ResourceOperation a_op, ScenarioDetail a_sd)
    {
        Tuple<DateTime, string> latestSource = new (DateTime.MinValue, null);
        for (int i = 0; i < a_op.MaterialRequirements.Count; i++)
        {
            MaterialRequirement mr = a_op.MaterialRequirements[i];
            Tuple<DateTime, string> supplyDateTime = mr.CalcExpectedMaterialReceipt(a_sd, a_op);
            if (supplyDateTime.Item1 > latestSource.Item1)
            {
                latestSource = supplyDateTime;
            }
        }

        return latestSource;
    }

    public static bool IsCapable(this InternalOperation a_op, Resource a_res)
    {
        for (int i = 0; i < a_op.ResourceRequirements.Count; i++)
        {
            ResourceRequirement rr = a_op.ResourceRequirements.GetByIndex(i);

            //Check res active
            if (!a_res.Active)
            {
                return false;
            }

            //TODO: Does this work?
            //Check capabilities
            foreach (Capability cap in rr.CapabilityManager)
            {
                if (!a_res.IsCapable(cap.Id))
                {
                    return false;
                }
            }

            //MinQty
            if (a_op.RequiredFinishQty < a_res.MinQty)
            {
                return false;
            }

            if (a_op.QtyPerCycle < a_res.MinQtyPerCycle)
            {
                return false;
            }

            //MaxQty
            if (a_op.RequiredFinishQty > a_res.MaxQty)
            {
                return false;
            }

            if (a_op.QtyPerCycle > a_res.MaxQtyPerCycle)
            {
                return false;
            }

            //Attribute Ranges
            if (a_res.FromToRanges != null)
            {
                for (int attI = 0; attI < a_op.Attributes.Count; attI++)
                {
                    OperationAttribute attr = a_op.Attributes[attI];
                    Scheduler.RangeLookup.FromRangeSet fromRangeSet = a_res.FromToRanges.Find(attr.PTAttribute.Name);
                    if (fromRangeSet != null && fromRangeSet.EligibilityConstraint)
                    {
                        Scheduler.RangeLookup.FromRange fromRange = fromRangeSet.Find(attr.Number);
                        if (fromRange == null)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }
}