using PT.APSCommon.Extensions;
using PT.Common.Localization;
using PT.Scheduler;
using PT.SchedulerData.InventoryManagement;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

public static class BaseOperationData
{
    ///// <summary>
    ///// Returns the Prededecessor information needed to publish to the Web Portal.
    ///// Format: predecesosrId, predecessorId+offsetHrs                
    ///// </summary>
    /////<example>Op -000001 is a predecessor starting 8.5 hrs before the successor ('SS' is start-to-start)  and -000002 is another predecessor with an end-to-start relation.   -000001SS+8.5,-000002</example>
    ///// <returns></returns>
    //public static string GetPortalPredecessorIndices(this BaseOperation a_baseOp)
    //{
    //    if (a_baseOp.AlternatePathNode != null)
    //    {
    //        System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();

    //        for (int i = 0; i < a_baseOp.AlternatePathNode.Predecessors.Count; i++)
    //        {
    //            AlternatePath.Association association = a_baseOp.AlternatePathNode.Predecessors[i];
    //            BaseOperation pred = association.Predecessor.Operation;
    //            if (i > 0)
    //                strBuilder.Append(", ");
    //            strBuilder.Append(pred.Id.ToString());
    //            long opStartDTToUse;
    //            long predStartDTToUse;
    //            if (pred.Finished && pred is InternalOperation)
    //            {
    //                InternalOperation intOp = (InternalOperation)pred;
    //                intOp.GetReportedStartDate(out predStartDTToUse);
    //            }
    //            else
    //                predStartDTToUse = pred.StartDateTime.Ticks;

    //            if (a_baseOp.Finished && a_baseOp is InternalOperation)
    //            {
    //                InternalOperation intOp = (InternalOperation)a_baseOp;
    //                intOp.GetReportedStartDate(out opStartDTToUse);
    //            }
    //            else
    //                opStartDTToUse = a_baseOp.StartDateTime.Ticks;

    //            double offsetFromPred = Math.Round(new TimeSpan(opStartDTToUse - predStartDTToUse).TotalHours, 1, MidpointRounding.AwayFromZero);
    //            if (offsetFromPred != 0)
    //                strBuilder.Append(String.Format("SS+{0}", offsetFromPred.ToString())); //SS is start-to-start; can be negative or positive
    //        }

    //        return strBuilder.ToString();
    //    }
    //    return String.Empty;
    //}

    /// <summary>
    /// Calculates the earliest date for which all materials are available.
    /// This value is the latest LatestSourceDate of all MaterialRequirements (MRs) if none of the MRs
    /// have a set item or warehouse. This value is the latest AvailableDate of MRs with an item
    /// or warehouse reference if one or more of the MRs have an item unless the AvailableDate is later
    /// than the Operation's StartDate, in which case, it returns the Operation's StartDate
    /// </summary>
    public static DateTime CalculateEarliestMaterialAvailability(this BaseOperation a_op, ScenarioDetail a_sd)
    {
        List<MaterialRequirement> requirementsList = new ();
        //Track non issued requirements
        for (int i = 0; i < a_op.MaterialRequirements.Count; i++)
        {
            if (!a_op.MaterialRequirements[i].IssuedComplete)
            {
                requirementsList.Add(a_op.MaterialRequirements[i]);
            }
        }

        if (requirementsList.Count == 0)
        {
            return PTDateTime.MinDateTime;
        }

        DateTime maxDate = a_op.StartDateTime;
        //Note: Non constraint (shortages) can have availability after schedule date.
        //if (a_op.Scheduled)
        //{
        //maxDate = a_op.ScheduledStartDate;
        //}

        //If there is only 1 requirement, just use the simple calculation
        if (requirementsList.Count == 1)
        {
            return requirementsList[0].CalculateAvailableDate(PTDateTime.MinDateTime, maxDate, a_sd);
        }

        DateTime currentWorkingDate = DateTime.MinValue;
        for (int i = requirementsList.Count - 1; i >= 0; i--)
        {
            MaterialRequirement materialRequirement = requirementsList[i];
            bool usingBaseDate;
            DateTime calculateAvailableDate = materialRequirement.CalculateAvailableDate(currentWorkingDate, maxDate, a_sd, out usingBaseDate);
            if (usingBaseDate)
            {
                //This MR does not have an inventory reference and is assumed available after this date.
                currentWorkingDate = PTDateTime.Max(calculateAvailableDate, currentWorkingDate);
                requirementsList.RemoveAt(i);
            }
        }

        if (requirementsList.Count == 0)
        {
            //None of the MRs had an inventory reference or a warehouse reference
            return currentWorkingDate;
        }

        //Keep looping through all remaining requirements until all are available at the same time.
        //If at any point, the material is available after the max date, just return the max date.
        bool conflictFound;
        do
        {
            conflictFound = false;
            currentWorkingDate = requirementsList[0].CalculateAvailableDate(currentWorkingDate, maxDate, a_sd);
            if (currentWorkingDate >= maxDate)
            {
                return maxDate;
                ;
            }

            for (int i = 1; i < requirementsList.Count; i++)
            {
                DateTime availableDate = requirementsList[i].CalculateAvailableDate(currentWorkingDate, maxDate, a_sd);
                if (availableDate > maxDate)
                {
                    return maxDate;
                }

                if (availableDate > currentWorkingDate)
                {
                    conflictFound = true;
                    currentWorkingDate = availableDate;
                }
            }
        } while (conflictFound);

        return currentWorkingDate;
    }

    /// <summary>
    /// Calculate the value for the MaterialAvailability column.
    /// </summary>
    public static string CalculateMaterialAvailability(this BaseOperation a_currentOp)
    {
        string availability = "";

        if (a_currentOp.MaterialRequirements.Count > 0)
        {
            int availableMRs = 0;
            int onHandMRs = 0;
            int shortageMRs = 0;
            int leadTimeMRs = 0;
            for (int i = 0; i < a_currentOp.MaterialRequirements.Count; i++)
            {
                MaterialRequirement currentMr = a_currentOp.MaterialRequirements[i];
                bool endOfPlanningHorizon = currentMr.MRSupply.SourcesFrom(false, true, InventoryDefs.EAdjustmentType.PastPlanningHorizon);
                bool shortage = currentMr.MRSupply.SourcesFrom(false, true, InventoryDefs.EAdjustmentType.Shortage);
                bool onlyOnHandInventory = currentMr.MRSupply.SourcesFrom(true, true, InventoryDefs.EAdjustmentType.OnHand);

                if (currentMr.Available)
                {
                    availableMRs++;
                    if (onlyOnHandInventory)
                    {
                        onHandMRs++;
                    }
                }

                if (!currentMr.Planned)
                {
                    leadTimeMRs++;
                }

                if (endOfPlanningHorizon || shortage)
                {
                    shortageMRs++;
                }
            }

            if (shortageMRs > 0)
            {
                if (shortageMRs == a_currentOp.MaterialRequirements.Count)
                {
                    availability = Localizer.GetString("Shortage");
                }
                else
                {
                    availability = Localizer.GetString("Shortage (Partial)");
                }
            }
            else if (leadTimeMRs > 0)
            {
                if (leadTimeMRs == a_currentOp.MaterialRequirements.Count)
                {
                    availability = Localizer.GetString("Lead Time");
                }
                else
                {
                    availability = Localizer.GetString("Lead Time (Partial)");
                }
            }
            else if (availableMRs == a_currentOp.MaterialRequirements.Count)
            {
                if (onHandMRs == availableMRs)
                {
                    availability = Localizer.GetString("On-Hand");
                }
                else
                {
                    availability = Localizer.GetString("All Available");
                }
            }
            else
            {
                availability = Localizer.GetString("Planned");
            }
        }

        return availability;
    }

    /// <summary>
    /// Returns a list of the Material Names for all Material Requirements that are Buy Direct.
    /// </summary>
    public static string GetBuyDirectMaterialsList(this BaseOperation a_op)
    {
        System.Text.StringBuilder list = new ();
        MaterialRequirementsCollection materials = a_op.MaterialRequirements;

        for (int mrI = 0; mrI < materials.Count; mrI++)
        {
            MaterialRequirement mr = materials[mrI];
            if (mr.BuyDirect)
            {
                if (list.Length > 0)
                {
                    list.Append(", ");
                }

                list.Append(string.Format("{0}", mr.MaterialName));
            }
        }

        return list.ToString();
    }

    /// <summary>
    /// Returns a list of the Material Names for all Material Requirements that are Buy Direct
    /// where the material requirement is not Available.
    public static string GetBuyDirectMaterialsNotAvailableList(this BaseOperation a_op)
    {
        System.Text.StringBuilder list = new ();
        MaterialRequirementsCollection materials = a_op.MaterialRequirements;
        for (int mrI = 0; mrI < materials.Count; mrI++)
        {
            MaterialRequirement mr = materials[mrI];
            if (mr.BuyDirect && !mr.Available)
            {
                if (list.Length > 0)
                {
                    list.Append(", ");
                }

                list.Append(string.Format("{0}", mr.MaterialName));
            }
        }

        return list.ToString();
    }

    /// <summary>
    /// The latest Scheduled End Date of all of the Operation's Predecessors.  If there are no Predecessors then this is MinDate.
    /// </summary>
    public static DateTime GetLatestPredecessorFinishDate(this BaseOperation a_op)
    {
        if (a_op.NotPartOfCurrentRouting())
        {
            return PTDateTime.MaxDateTime;
        }

        if (a_op.HasPredecessors())
        {
            AlternatePath.AssociationCollection predecessorCollection = a_op.AlternatePathNode.Predecessors;
            DateTime latestPredFinishSoFar = PTDateTime.MinDateTime;

            for (int predecessorI = 0; predecessorI < predecessorCollection.Count; predecessorI++)
            {
                BaseOperation predecessorOperation = predecessorCollection[predecessorI].Predecessor.Operation;
                if (predecessorOperation.Scheduled && predecessorOperation.ScheduledEndDate > latestPredFinishSoFar)
                {
                    latestPredFinishSoFar = predecessorOperation.ScheduledEndDate;
                }
            }

            return latestPredFinishSoFar;
        }

        return PTDateTime.MinDateTime;
    }

    /// <summary>
    /// Browsable list of all Material Requirements for the Operation that are not Available.
    /// </summary>
    public static string GetMaterialsNotAvailable(this BaseOperation a_op)
    {
        System.Text.StringBuilder builder = new ();
        for (int i = 0; i < a_op.MaterialRequirements.Count; i++)
        {
            MaterialRequirement mr = a_op.MaterialRequirements[i];

            if (!mr.Available)
            {
                if (builder.Length > 0)
                {
                    builder.Append(Environment.NewLine);
                }

                string txt = string.Format("{0} due {1}, {2}".Localize(), mr.MaterialName, mr.LatestSourceDateTime.ToDisplayTime(), mr.Source);
                builder.Append(txt);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Calculate a list of all Material Requirements for the Operation that are not Available, including leadtime
    /// </summary>
    public static string CalculateMaterialsNotAvailable(this BaseOperation a_op, ScenarioDetail a_sd)
    {
        System.Text.StringBuilder builder = new ();
        for (int i = 0; i < a_op.MaterialRequirements.Count; i++)
        {
            MaterialRequirement mr = a_op.MaterialRequirements[i];

            if (!mr.Available)
            {
                DateTime availableDate = mr.LatestSourceDateTime;
                //Check for leadtime if available date is not set
                if (!mr.BuyDirect)
                {
                    if (availableDate <= PTDateTime.MinDateTime)
                    {
                        TimeSpan leadtime = mr.Warehouse != null ? mr.Warehouse.Inventories[mr.Item.Id].LeadTime : mr.Item.DefaultLeadTime;
                        //TODO: potentially loop through warehouses to find the earliest leadtime
                        availableDate = a_sd.ClockDate.Add(leadtime);
                    }
                }

                if (builder.Length > 0)
                {
                    builder.Append(Environment.NewLine);
                }

                string txt = string.Format("{0} due {1}, {2}".Localize(), mr.MaterialName, availableDate.ToDisplayTime(), mr.Source);
                builder.Append(txt);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Browsable list of all Material Requirements for the Operation that are not Planned.
    /// </summary>
    public static string GetMaterialsNotPlanned(this BaseOperation a_op)
    {
        System.Text.StringBuilder builder = new ();
        for (int i = 0; i < a_op.MaterialRequirements.Count; i++)
        {
            MaterialRequirement mr = a_op.MaterialRequirements[i];

            if (!mr.Planned)
            {
                if (builder.Length > 0)
                {
                    builder.Append(Environment.NewLine);
                }

                string txt = string.Format("{0} {1}:  {2} {3}", mr.MaterialName, mr.MaterialDescription, Math.Round(mr.TotalRequiredQty, 2), mr.UOM);
                builder.Append(txt);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Specifies whether the Operation has Material Requirements that are not Allocated.
    /// </summary>
    public static BaseOperation.MaterialStatuses GetMaterialStatus(this BaseOperation a_op, DateTime a_clockDate)
    {
        int availMrCount = 0;
        bool atLeastOneSourceIsPlanned = false;
        MaterialRequirementsCollection materials = a_op.MaterialRequirements;
        InternalOperation op = a_op as InternalOperation;

        //Some materials have not been fully issued.
        for (int mrI = 0; mrI < a_op.MaterialRequirements.Count; mrI++)
        {
            MaterialRequirement mr = a_op.MaterialRequirements[mrI];

            if (mr.IssuedComplete || mr.Available)
            {
                availMrCount++;
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
                atLeastOneSourceIsPlanned = atLeastOneSourceIsPlanned || mr.AreAnySuppliesPlanned();
            }
        }

        if (availMrCount == a_op.MaterialRequirements.Count)
        {
            return BaseOperation.MaterialStatuses.MaterialsAvailable;
        }

        if (atLeastOneSourceIsPlanned)
        {
            return BaseOperation.MaterialStatuses.MaterialSourcesPlanned;
        }

        return BaseOperation.MaterialStatuses.MaterialSourcesFirmed;
    }

    /// <summary>
    /// The number of Materials that are not Planned.
    /// </summary>
    public static int GetUnplannedMaterialsCount(this BaseOperation a_op)
    {
        int count = 0;
        for (int i = 0; i < a_op.MaterialRequirements.Count; i++)
        {
            MaterialRequirement mr = a_op.MaterialRequirements[i];
            if (!mr.Planned)
            {
                count++;
            }
        }

        return count;
    }
}