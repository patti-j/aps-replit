using System.Collections;
using System.Drawing;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Extensions;
using PT.Common.Localization;
using PT.Scheduler;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;
using PT.SchedulerExtensions.Operations;

namespace PT.SchedulerExtensions.Job;

public static class JobExtensions
{
    /// <summary>
    /// Don't use this field for simulation purposes becuase it uses Datetime.Now.
    /// True if the EntryDate (ignoring time of day) of the Job is the same as the date on the client PC.
    /// </summary>
    public static bool GetEnteredToday(this Scheduler.Job a_job)
    {
        return a_job.EntryDate.ToDisplayTime().ToDateNoTime() == PTDateTime.UserDateTimeNow.ToDateNoTime();
    }

    public static string DetermineFailedToScheduleReason(this Scheduler.Job a_job, ScenarioDetail a_sd)
    {
        if (a_job.ScheduledStatus != JobDefs.scheduledStatuses.FailedToSchedule)
        {
            return string.Empty;
        }

        if (!string.IsNullOrEmpty(a_job.FailedToScheduleReason))
        {
            return a_job.FailedToScheduleReason;
        }

        //Check Resource Eligibility
        foreach (InternalActivity act in a_job.GetActivities())
        {
            //Verify Capability Eligibility
            List<Resource> capableResources = new ();
            for (int i = 0; i < act.Operation.ResourceRequirements.Count; i++)
            {
                ResourceRequirement rr = act.Operation.ResourceRequirements.GetByIndex(i);

                foreach (Resource res in a_sd.PlantManager.GetResourceList())
                {
                    if (act.Operation.IsResourceCapable(res, rr))
                    {
                        capableResources.Add(res);
                    }
                }

                if (capableResources.Count == 0)
                {
                    return string.Format("Operation '{0}' Resource Requirement '{1}' is not eligible on any resource".Localize(), act.Operation.Name, rr.ExternalId);
                }

                if (!capableResources.Any(r => r.Active))
                {
                    return string.Format("Eligible resources for Operation '{0}' Resource Requirement '{1}' are not active".Localize(), act.Operation.Name, rr.ExternalId);
                }

                //This isn't reliable for all simulation types and might override a real reason.
                //if (!capableResources.All(r => r.ManualSchedulingOnly))
                //{
                //    return string.Format("Eligible resources for Operation '{0}' Resource Requirement '{1}' are not active".Localize(), act.Operation.Name, rr.ExternalId);
                //}

                if (capableResources.All(r => r.MinQty > act.RequiredFinishQty || r.MaxQty < act.RequiredFinishQty))
                {
                    return string.Format("Eligible resources for Operation '{0}' Resource Requirement '{1}' can not run required quantity: {2}".Localize(), act.Operation.Name, rr.ExternalId, act.RequiredFinishQty);
                }

                if (capableResources.All(r => r.MinQtyPerCycle > act.Operation.QtyPerCycle || r.MaxQtyPerCycle < act.Operation.QtyPerCycle))
                {
                    return string.Format("Eligible resources for Operation '{0}' Resource Requirement '{1}' can not run required quantity per cycle : {2}".Localize(), act.Operation.Name, rr.ExternalId, act.Operation.QtyPerCycle);
                }

                foreach (Resource resource in capableResources)
                {
                    if (!string.IsNullOrEmpty(act.Operation.GetOutOfRangeAttributes(resource)))
                    {
                        break;
                    }
                }
            }
        }

        foreach (ManufacturingOrder mo in a_job.ManufacturingOrders)
        {
            foreach (AlternatePath path in mo.AlternatePaths)
            {
                if (path.ExcludedBySchedulingDueToValidity)
                {
                    //TODO: This is making an assumption. It's possible other paths were valid and it failed for some other reason.
                    return "Paths were excluded due to validity date ranges".Localize();
                }
            }

            List<ManufacturingOrder> predecessorMOs = mo.GetPredecessorMOs(a_sd.JobManager);

            foreach (ManufacturingOrder manufacturingOrder in predecessorMOs)
            {
                if (!manufacturingOrder.Finished && !manufacturingOrder.Scheduled)
                {
                    return string.Format("Predecessor MO with External Id:{0} and Job External Id:{1} failed to schedule".Localize(), manufacturingOrder.ExternalId, manufacturingOrder.Job.ExternalId);
                }
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Calculates the number of days that the Scheduled Start Date is relative to the Clock Date
    /// </summary>
    /// <param name="a_job"></param>
    /// <param name="a_sd"></param>
    /// <returns>Number of days from Clock Date or int.MaxValue if Job is not Scheduled</returns>
    public static int CalculateStartsInDays(this PT.Scheduler.Job a_job, ScenarioDetail a_sd)
    {
        if (!a_job.ScheduledOrPartiallyScheduled)
        {
            return int.MaxValue;
        }

        int numberOfStartDays = Convert.ToInt32(Math.Ceiling((a_job.ScheduledStartDate - a_sd.ClockDate).TotalDays));

        return numberOfStartDays;
    }

    /// <summary>
    /// Returns the Names of all Jobs that are of Classification SalesOrder and are supplied directly or indirectly from this Job.
    /// </summary>
    public static string GetSuppliedSalesOrderJobs(this Scheduler.Job a_job)
    {
        List<Scheduler.Job> suppliedJobs = GetSuppliedJobs(a_job);
        if (suppliedJobs.Count > 0)
        {
            System.Text.StringBuilder sb = new ();
            foreach (Scheduler.Job job in suppliedJobs)
            {
                if (job.Classification == JobDefs.classifications.SalesOrder)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(Environment.NewLine);
                    }

                    sb.Append(job.Name);
                }
            }

            return sb.ToString();
        }

        return "";
    }

    /// <summary>
    /// Returns the Names of all Jobs that are of Classification SalesOrder and are supplied directly or indirectly from this Job.
    /// </summary>
    public static string GetSuppliedSalesOrderJobDetails(this Scheduler.Job a_job)
    {
        List<PT.Scheduler.Job> suppliedJobs = GetSuppliedJobs(a_job);
        if (suppliedJobs.Count > 0)
        {
            System.Text.StringBuilder sb = new ();
            foreach (Scheduler.Job job in suppliedJobs)
            {
                if (job.Classification == JobDefs.classifications.SalesOrder)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(Environment.NewLine);
                    }

                    string timing;
                    if (job.Overdue)
                    {
                        timing = Localizer.GetString("OVERDUE");
                    }
                    else if (job.Late)
                    {
                        timing = Localizer.GetString("LATE");
                    }
                    else if (job.TooEarly)
                    {
                        timing = Localizer.GetString("Too Early");
                    }
                    else
                    {
                        timing = Localizer.GetString("On-Time");
                    }

                    sb.Append(string.Format("[{0}]  [{1}]  [{2}]  [{3}]  [{4}]", job.Name, job.Customers.GetCustomerNamesList(), job.Product, timing, job.NeedDateTime));
                }
            }

            return sb.ToString();
        }

        return "";
    }

    /// <summary>
    /// The Current Path Names for all MOs for the Job.
    /// </summary>
    public static string CurrentPath(this Scheduler.Job a_job)
    {
        System.Text.StringBuilder builder = new ();
        for (int i = 0; i < a_job.ManufacturingOrderCount; i++)
        {
            ManufacturingOrder mo = a_job.ManufacturingOrders[i];
            if (mo.CurrentPath != null)
            {
                if (builder.Length > 0)
                {
                    builder.Append(",");
                }

                builder.Append(mo.CurrentPathName);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Color to indicate the lateness of the Job.
    /// Overdue=Red, Late=Orange, TooEarly=LightGreen, AlmostLate==Yellow, On-Time=White.
    /// </summary>
    public static Color GetTimingColor(this Scheduler.Job a_job)
    {
        if (a_job.Overdue)
        {
            return ColorUtils.ColorCodes.OverdueColor;
        }

        if (a_job.Late)
        {
            return ColorUtils.ColorCodes.LateColor;
        }

        if (a_job.TooEarly)
        {
            return ColorUtils.ColorCodes.TooEarlyColor;
        }

        if (a_job.AlmostLate)
        {
            return ColorUtils.ColorCodes.AlmostLateColor;
        }

        return ColorUtils.ColorCodes.OnTimeColor;
    }

    /// <summary>
    /// Returns a color based on the Priority.
    /// Priority Less than <=1=Red, 2=Orange, 3=Yellow.  Greater than 3 is White.
    /// </summary>
    public static Color GetPriorityColor(this Scheduler.Job a_job)
    {
        //TODO: need to load the PrioritySettings colors picked by the user in the priority segment settings control

        if (a_job.Priority < 1)
        {
            return ColorUtils.ColorCodes.Priority1;
        }

        if (a_job.Priority == 1)
        {
            return ColorUtils.ColorCodes.Priority1;
        }

        if (a_job.Priority == 2)
        {
            return ColorUtils.ColorCodes.Priority2;
        }

        if (a_job.Priority == 3)
        {
            return ColorUtils.ColorCodes.Priority3;
        }

        return ColorUtils.ColorCodes.PriorityHigherThan3;
    }

    /// <summary>
    /// Can be used to indicate whether a less preferred Path is in use.
    /// The highest of the Current Path Preferences for all MOs for the Job.
    /// </summary>
    public static int MaxCurrentPathPreference(this Scheduler.Job a_job)
    {
        int maxPathPreference = -999;
        for (int i = 0; i < a_job.ManufacturingOrderCount; i++)
        {
            ManufacturingOrder mo = a_job.ManufacturingOrders[i];
            if (mo.CurrentPath != null && mo.CurrentPath.Preference > maxPathPreference)
            {
                maxPathPreference = mo.CurrentPath.Preference;
            }
        }

        return maxPathPreference;
    }

    /// <summary>
    /// Returns the earliest operation JIT start date. Finished and omitted jobs are excluded.
    /// </summary>
    public static DateTime EarliestOpJitStart(this Scheduler.Job a_job)
    {
        DateTime minJit = PTDateTime.MaxDateTime;
        foreach (InternalOperation op in a_job.GetOperations())
        {
            if (!op.Finished && op.Omitted == BaseOperationDefs.omitStatuses.NotOmitted)
            {
                minJit = PTDateTime.Min(minJit, op.DbrJitStartDate);
            }
        }

        return minJit;
    }

    /// <summary>
    /// Returns the earliest operation JIT start date. Finished and omitted jobs are excluded.
    /// </summary>
    public static JobDefs.EMoReleasedStatus MoReleased(this Scheduler.Job a_job)
    {
        int totalMos = a_job.ManufacturingOrderCount;
        int holdMOs = 0;
        foreach (ManufacturingOrder manufacturingOrder in a_job.ManufacturingOrders)
        {
            if (manufacturingOrder.Finished)
            {
                totalMos--;
            }
            else if (manufacturingOrder.Hold)
            {
                holdMOs++;
            }
        }

        if (totalMos == holdMOs)
        {
            return JobDefs.EMoReleasedStatus.Released;
        }

        if (holdMOs > 0)
        {
            return JobDefs.EMoReleasedStatus.Partially;
        }

        return JobDefs.EMoReleasedStatus.None;
    }

    /// <summary>
    /// Gets the Earliest Release Date out of this Job's MO's.
    /// </summary>
    /// <param name="a_job"></param>
    /// <returns>Earliest Release Date or PTDateTime.MinDateTime if all MO's are finished</returns>
    public static DateTime EarliestRelease(this Scheduler.Job a_job)
    {
        DateTime earliestHold = PTDateTime.MinDateTime;
        foreach (ManufacturingOrder mo in a_job.ManufacturingOrders)
        {
            if (!mo.Finished)
            {
                earliestHold = PTDateTime.Max(earliestHold, mo.HoldUntil);
            }
        }

        return earliestHold;
    }

    /// <summary>
    /// Gets a list of Plant Ids from this Job's used Resources. If Job is not scheduled, Plant Ids are based on the MO's Locked Plant Id field if defined.
    /// </summary>
    /// <param name="a_job"></param>
    /// <returns></returns>
    public static List<BaseId> ScheduledPlantIds(this Scheduler.Job a_job)
    {
        List<BaseId> plantList = new ();
        if (a_job.ScheduledOrPartiallyScheduled)
        {
            List<InternalResource> resourcesUsed = a_job.GetResourcesScheduled();
            for (int resI = 0; resI < resourcesUsed.Count; resI++)
            {
                BaseId plantId = resourcesUsed[resI].Department.PlantId;
                if (!plantList.Contains(plantId))
                {
                    plantList.Add(plantId);
                }
            }
        }
        else //Use MO Locked Plant if specified
        {
            for (int moI = 0; moI < a_job.ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = a_job.ManufacturingOrders[moI];
                if (mo.LockedPlant != null)
                {
                    BaseId plantId = mo.LockedPlant.Id;
                    if (!plantList.Contains(plantId))
                    {
                        plantList.Add(plantId);
                    }
                }
            }
        }

        return plantList;
    }

    /// <summary>
    /// The percent of Material Requirements for all Manufacturing Orders that are issued.
    /// </summary>
    public static int PercentOfMaterialsIssued(this Scheduler.Job a_job)
    {
        int materialCount = 0;
        int materialAvailableCount = 0;
        for (int i = 0; i < a_job.ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = a_job.ManufacturingOrders[i];
            if (!mo.Scheduled)
            {
                continue; // no supply or availability is available for unscheduled Operations. Do not include these in the calaculation
            }

            for (int nodeI = 0; nodeI < mo.CurrentPath.NodeCount; nodeI++)
            {
                AlternatePath.Node node = mo.CurrentPath.GetNodeByIndex(nodeI);
                BaseOperation bOp = node.Operation;
                if (!bOp.Scheduled)
                {
                    continue;
                }

                for (int matI = 0; matI < bOp.MaterialRequirements.Count; matI++)
                {
                    MaterialRequirement mr = bOp.MaterialRequirements[matI];
                    materialCount++;
                    if (mr.IssuedComplete)
                    {
                        materialAvailableCount++;
                    }
                }
            }
        }

        if (materialCount == 0)
        {
            return 100;
        }

        return (int)(materialAvailableCount / (float)materialCount * 100);
    }

    /// <summary>
    /// Gets a list of Material Names from all Operation Material Requirements on all MO's
    /// </summary>
    /// <param name="a_job"></param>
    /// <returns></returns>
    public static string MaterialList(this Scheduler.Job a_job)
    {
        HashSet<string> fullList = new ();
        System.Text.StringBuilder stringBuilder = new ();
        for (int i = 0; i < a_job.ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = a_job.ManufacturingOrders[i];
            HashSet<string> materialList = mo.MaterialList();
            fullList.MergeWith(materialList);
        }

        foreach (string s in fullList)
        {
            stringBuilder.Append(s + ", ");
        }

        return stringBuilder.ToString().TrimEnd(',', ' ');
    }

    /// <summary>
    /// Gets Product from final MO. If SuccessorMOs are not defined, the first MO's Product will be returned.
    /// </summary>
    /// <param name="a_job"></param>
    /// <returns></returns>
    public static Product GetFinalPrimaryProduct(this Scheduler.Job a_job)
    {
        List<Product> products = new ();
        List<ManufacturingOrder> mosWithProduct = new ();

        for (int i = 0; i < a_job.ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = a_job.ManufacturingOrders[i];
            if (mo.GetPrimaryProduct() != null)
            {
                mosWithProduct.Add(mo);
            }
        }

        foreach (ManufacturingOrder mo in mosWithProduct)
        {
            if (mo.SuccessorMOs.Count != 0)
            {
                continue;
            }

            return mo.GetPrimaryProduct();
        }

        return null;
    }

    /// <summary>
    /// The total number of Sales Orders satisfied by the Manufacturing Order's Products.
    /// </summary>
    public static int SalesOrderCount(this Scheduler.Job a_job)
    {
        int count = 0;
        for (int i = 0; i < a_job.ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = a_job.ManufacturingOrders[i];
            count += mo.SalesOrderCount;
        }

        return count;
    }

    /// <summary>
    /// The total number of Forecasts satisfied by the Manufacturing Order's Products.
    /// </summary>
    public static int ForecastCount(this Scheduler.Job a_job)
    {
        int count = 0;
        for (int i = 0; i < a_job.ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = a_job.ManufacturingOrders[i];
            count += mo.ForecastCount;
        }

        return count;
    }

    /// <summary>
    /// The total of all Required Qties for Sales Orders Demands.
    /// </summary>
    public static decimal QtyToSalesOrders(this Scheduler.Job a_job)
    {
        decimal qty = 0;
        for (int i = 0; i < a_job.ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = a_job.ManufacturingOrders[i];
            List<Product> products = mo.GetProducts(true);
            foreach (Product product in products)
            {
                if (product.Demands != null)
                {
                    decimal moQty = product.Demands.QtyToSalesOrders;
                    qty += Math.Min(product.TotalOutputQty, moQty);
                }
            }
        }

        return qty;
    }

    /// <summary>
    /// The Names of all Original MRP Sales Orders the Job was created to supply.
    /// </summary>
    public static string OriginalSalesOrders(this Scheduler.Job a_job)
    {
        Dictionary<SalesOrder, List<SalesOrderLine>> demandDict = new ();
        for (int i = 0; i < a_job.ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = a_job.ManufacturingOrders[i];
            List<Product> products = mo.GetProducts(true);
            foreach (Product product in products)
            {
                if (product.Demands != null && product.Demands.SalesOrderDemands != null)
                {
                    for (int soI = 0; soI < product.Demands.SalesOrderDemands.Count; soI++)
                    {
                        SalesOrderDemand soDemand = product.Demands.SalesOrderDemands.GetByIndex(soI);
                        List<SalesOrderLine> lineList;
                        if (demandDict.TryGetValue(soDemand.SalesOrderLineDistribution.SalesOrderLine.SalesOrder, out lineList))
                        {
                            lineList.Add(soDemand.SalesOrderLineDistribution.SalesOrderLine);
                        }
                        else
                        {
                            lineList = new List<SalesOrderLine>();
                            lineList.Add(soDemand.SalesOrderLineDistribution.SalesOrderLine);
                            demandDict.Add(soDemand.SalesOrderLineDistribution.SalesOrderLine.SalesOrder, lineList);
                        }
                    }
                }
            }
        }

        //Build the string in format:   SOName (line1, line2), SOName2 (line1, line2)
        System.Text.StringBuilder sBuilder = new ();
        foreach (KeyValuePair<SalesOrder, List<SalesOrderLine>> pair in demandDict)
        {
            sBuilder.Append(pair.Key.Name);
            sBuilder.Append(" (");
            for (int i = 0; i < pair.Value.Count; i++)
            {
                sBuilder.Append(pair.Value[i].LineNumber);
                if (i != pair.Value.Count - 1)
                {
                    sBuilder.Append(", ");
                }
            }

            sBuilder.Append(") | ");
        }

        return sBuilder.ToString().TrimEnd('|', ' ');
    }

    /// <summary>
    /// The total of all Required Qties for Forecast Demands.
    /// </summary>
    public static decimal QtyToForecasts(this Scheduler.Job a_job)
    {
        decimal qty = 0;
        for (int i = 0; i < a_job.ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = a_job.ManufacturingOrders[i];
            List<Product> products = mo.GetProducts(true);
            foreach (Product product in products)
            {
                if (product.Demands != null)
                {
                    decimal moQty = product.Demands.QtyToForecasts;
                    qty += Math.Min(product.TotalOutputQty, moQty);
                }
            }
        }

        return qty;
    }

    /// <summary>
    /// The total of all Required Qties for Safety Stock Demands.
    /// </summary>
    public static decimal QtyToSafetyStock(this Scheduler.Job a_job)
    {
        decimal qty = 0;
        for (int i = 0; i < a_job.ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = a_job.ManufacturingOrders[i];
            List<Product> products = mo.GetProducts(true);
            foreach (Product product in products)
            {
                if (product.Demands != null)
                {
                    decimal moQty = product.Demands.QtyToSafetyStock;
                    qty += Math.Min(product.TotalOutputQty, moQty);
                }
            }
        }

        return qty;
    }

    /// <summary>
    /// Returns the sum of the Manufacturing Order Labor and Material costs.
    /// </summary>
    /// <returns></returns>
    public static void GetWipCosts(this Scheduler.Job a_job, ref decimal resourceCost, ref decimal materialCost)
    {
        if (a_job.ScheduledStatus != JobDefs.scheduledStatuses.Scheduled)
        {
            return;
        }

        for (int i = 0; i < a_job.ManufacturingOrders.Count; i++)
        {
            decimal rCost = 0;
            decimal mCost = 0;
            a_job.ManufacturingOrders[i].GetWipCosts(ref rCost, ref mCost);
            resourceCost += rCost;
            materialCost += mCost;
        }
    }

    /// <summary>
    /// Returns the sum of the Operation Carrying Costs multiplied by the days from the end of the MO to the MO Need Date.
    /// </summary>
    /// <returns></returns>
    public static decimal GetFinishedGoodsCarryingCosts(this Scheduler.Job a_job)
    {
        if (a_job.ScheduledStatus != JobDefs.scheduledStatuses.Scheduled)
        {
            return 0;
        }

        decimal cost = 0;
        for (int i = 0; i < a_job.ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = a_job.ManufacturingOrders[i];
            BaseOperation leadOp = mo.GetLeadOperation(); //shouldn't be null since Job is scheduled.                    
            if (mo.NeedDate.Ticks > mo.ScheduledEndDate.Ticks) //no carrying cost if late
            {
                long ticksEarly = mo.NeedDate.Ticks - mo.ScheduledEndDate.Ticks;
                cost += mo.GetOpCarryingCost() * (decimal)TimeSpan.FromTicks(ticksEarly).TotalDays;
            }
        }

        return cost;
    }

    /// <summary>
    /// Returns the sum of the Operation Carrying Costs multiplied by the days from the start of the Operation to the MO Scheduled End Date.
    /// </summary>
    /// <returns></returns>
    public static decimal GetWipCarryingCosts(this Scheduler.Job a_job)
    {
        decimal cost = 0;
        if (a_job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
        {
            for (int i = 0; i < a_job.ManufacturingOrders.Count; i++)
            {
                cost += a_job.ManufacturingOrders[i].GetWipCarryingCost();
            }
        }

        return cost;
    }

    /// <summary>
    /// The Primary Resource that the Lead Activity is scheduled on.  Null if there is no scheduled Activity.
    /// </summary>
    public static BaseResource GetLeadResource(this Scheduler.Job a_job)
    {
        ManufacturingOrder leadMO = a_job.GetLeadManufacturingOrder();
        if (leadMO != null)
        {
            InternalOperation io = leadMO.GetLeadOperation() as InternalOperation;

            if (io != null)
            {
                ResourceBlock rb = io.GetLeadActivity().PrimaryResourceRequirementBlock;

                if (rb != null)
                {
                    return rb.ScheduledResource;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns a list of Jobs that are supplied by the specified Job eaither by SuccessorMO or by mateirals, any levels deep.
    /// </summary>
    /// <param name="supplyingJob"></param>
    /// <returns></returns>
    public static List<Scheduler.Job> GetSuppliedJobs(this Scheduler.Job supplyingJob)
    {
        List<Scheduler.Job> jobList = new();
        Hashtable addedJobIdsHash = new();
        for (int moI = 0; moI < supplyingJob.ManufacturingOrders.Count; moI++)
        {
            ManufacturingOrder mo = supplyingJob.ManufacturingOrders[moI];
            List<ManufacturingOrder.ManufacturingOrderLevel> moSucLevels = mo.GetSuccessorsRecursively();
            for (int sucMOLevelI = 0; sucMOLevelI < moSucLevels.Count; sucMOLevelI++)
            {
                ManufacturingOrder.ManufacturingOrderLevel moLevel = moSucLevels[sucMOLevelI];
                if (!addedJobIdsHash.ContainsKey(moLevel.MO.Job.Id))
                {
                    jobList.Add(moLevel.MO.Job);
                    addedJobIdsHash.Add(moLevel.MO.Job.Id, null);
                }
            }

            List<ManufacturingOrder.ManufacturingOrderLevel> moMatlLevels = mo.GetMaterialSuccessorsRecursively();
            for (int sucMOLevelI = 0; sucMOLevelI < moMatlLevels.Count; sucMOLevelI++)
            {
                ManufacturingOrder.ManufacturingOrderLevel moLevel = moMatlLevels[sucMOLevelI];
                if (!addedJobIdsHash.ContainsKey(moLevel.MO.Job.Id))
                {
                    jobList.Add(moLevel.MO.Job);
                    addedJobIdsHash.Add(moLevel.MO.Job.Id, null);
                }
            }
        }

        return jobList;
    }
}