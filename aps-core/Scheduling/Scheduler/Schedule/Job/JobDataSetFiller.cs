using System.Text;

using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public class JobDataSetFiller
{
    public void FillDataSet(JobDataSet ds, List<Job> jobs, JobManager jobManager)
    {
        for (int i = 0; i < jobs.Count; i++)
        {
            FillDataSet(ds, jobs[i], jobManager);
        }
    }

    public void FillDataSet(JobDataSet ds, Job job, JobManager jobManager)
    {
        JobDataSet.JobRow jobRow = ds.Job.NewJobRow();

        jobRow.Id = job.Id.ToBaseType();
        jobRow.Name = job.Name;
        jobRow.ExternalId = job.ExternalId;
        jobRow.Name = job.Name;
        jobRow.ColorCode = ColorUtils.ConvertColorToHexString(job.ColorCode);
        jobRow.Description = job.Description;
        jobRow.Notes = job.Notes;
        jobRow.Analysis = job.Analysis;
        jobRow.CanSpanPlants = job.CanSpanPlants;
        jobRow.Cancelled = job.Cancelled;
        jobRow.Reviewed = job.Reviewed;
        jobRow.Classification = job.Classification.ToString();
        jobRow.Commitment = job.Commitment.ToString();
        jobRow.EarliestDelivery = job.EarliestDelivery.ToDisplayTime().ToDateTime();
        jobRow.EntryDate = job.EntryDate.ToDateTime().ToDisplayTime().ToDateTime();
        jobRow.MaintenanceMethod = (int)job.MaintenanceMethod;
        jobRow.Finished = job.Finished;
        jobRow.Anchored = (int)job.Anchored;
        jobRow.History = job.GetHistory();
        jobRow.Hot = job.Hot;
        jobRow.HotReason = job.HotReason;
        jobRow.Importance = job.Importance;
        jobRow.Late = job.Late;
        jobRow.LatePenaltyCost = job.LatePenaltyCost;
        jobRow.Lateness = job.Lateness;
        jobRow.Locked = (int)job.Locked;
        jobRow.MaxEarlyDeliveryDays = job.MaxEarlyDeliverySpan.TotalDays;
        jobRow.AlmostLateDays = job.AlmostLateSpan.TotalDays;
        jobRow.NeedDateTime = job.NeedDateTime.ToDisplayTime().ToDateTime();
        jobRow.OnHold = job.OnHold.ToString();
        jobRow.HoldReason = job.HoldReason;
        jobRow.Hold = job.Hold;
        jobRow.HoldUntilDate = job.HoldUntil.ToDisplayTime().ToDateTime();
        jobRow.OrderNumber = job.OrderNumber;
        jobRow.CustomerEmail = job.CustomerEmail;
        jobRow.AgentEmail = job.AgentEmail;
        jobRow.Overdue = job.Overdue;
        jobRow.OverdueSpan = job.OverdueSpan;
        jobRow.Priority = job.Priority;
        jobRow.Profit = job.Profit;
        jobRow.FailedToScheduleReason = job.FailedToScheduleReason;
        jobRow.Revenue = job.Revenue;
        jobRow.ScheduledEnd = job.ScheduledEndDate.ToDisplayTime().ToDateTime();
        jobRow.ScheduledStart = job.ScheduledStartDate.ToDisplayTime().ToDateTime();
        jobRow.ScheduledStatus = (int)job.ScheduledStatus;
        jobRow.Summary = job.Summary;
        jobRow.Type = job.Type;
        jobRow.DoNotDelete = job.DoNotDelete;
        jobRow.DoNotSchedule = job.DoNotSchedule;
        jobRow.PercentFinished = job.PercentFinished;
        jobRow.Template = job.Template;
        jobRow.PercentOfStandardHrs = job.PercentOfStandardHrs;
        jobRow.ExpectedSetupHrs = job.ExpectedSetupHours;
        jobRow.ExpectedRunHrs = job.ExpectedRunHours;
        jobRow.StandardHrs = job.StandardHours;
        jobRow.Invoiced = job.Invoiced;
        jobRow.Printed = job.Printed;
        jobRow.Shipped = (int)job.Shipped;
        jobRow.Destination = job.Destination;
        jobRow.PercentOfMaterialsFinished = job.PercentOfMaterialsAvailable;
        jobRow.Throughput = job.Throughput;
        jobRow.MaterialCost = job.MaterialCost;
        jobRow.LaborCost = job.LaborCost;
        jobRow.MachineCost = job.MachineCost;
        jobRow.SubcontractCost = job.SubcontractCost;
        jobRow.ShippingCost = job.ShippingCost;
        jobRow.ExpectedLatePenaltyCost = job.ExpectedLatePenaltyCost;
        jobRow.TotalCost = job.TotalCost;
        jobRow.PastPlanningHorizon = job.PastPlanningHorizon;
        jobRow.UserFields = job.UserFields?.GetUserFieldImportString() ?? "";

        ds.Job.AddJobRow(jobRow);

        foreach (Customer jobCustomer in job.Customers)
        {
            JobDataSet.CustomerRow newCustomerRow = ds.Customer.NewCustomerRow();
            newCustomerRow.JobExternalId = job.ExternalId;
            newCustomerRow.CustomerExternalId = jobCustomer.ExternalId;
            ds.Customer.AddCustomerRow(newCustomerRow);
        }


        //TODO: Add customer rows
        //Populate with MOs
        job.ManufacturingOrders.PopulateJobDataSet(jobManager, ref ds);

        //SubManufacturingOrder
        int bomLevel = 0;
        for (int moI = 0; moI < job.ManufacturingOrders.Count; moI++)
        {
            ManufacturingOrder mo = job.ManufacturingOrders[moI];
            AddSubMO(jobRow, job, mo, ds, bomLevel, GetOnHandMaterials(mo), GetNOTOnHandMaterials(mo));
        }
    }

    private readonly HashSet<BaseId> MOsAlreadyAdded = new ();

    private void AddSubMO(JobDataSet.JobRow jobRow, Job job, ManufacturingOrder mo, JobDataSet ds, int bomLevel, string onHandMaterials, string notOnHandMaterials)
    {
        string currentResources = "not scheduled";
        ResourceOperation resOp = mo.GetLeadOperation() as ResourceOperation;
        if (resOp != null)
        {
            currentResources = resOp.Activities.GetByIndex(0).ResourcesUsed;
        }


        if (!MOsAlreadyAdded.Contains(mo.Id))
        {
            MOsAlreadyAdded.Add(mo.Id);
            ds.SubManufacturingOrders.AddSubManufacturingOrdersRow(
                bomLevel,
                job.Id.ToBaseType(),
                mo.Job.Id.ToBaseType(),
                mo.Id.ToBaseType(),
                mo.Name,
                mo.Description,
                mo.ProductName,
                mo.ProductDescription,
                mo.RequiredQty,
                mo.NeedDate.ToDisplayTime().ToDateTime(),
                mo.MoNeedDate,
                job.Name,
                mo.PercentFinished,
                currentResources,
                mo.GetScheduledStartDate().ToDisplayTime().ToDateTime(),
                mo.GetScheduledEndDate().ToDisplayTime().ToDateTime(),
                onHandMaterials,
                notOnHandMaterials,
                job.ManufacturingOrders.Count,
                jobRow,
                mo.ExternalId,
                mo.CapacityBottlenecks,
                mo.MaterialBottlenecks
            );

            AddSupplyingMOs(job, ds, mo, jobRow, bomLevel + 1);
        }
    }

    /// <summary>
    /// Add an MO row for each MO that supplies this MO.
    /// </summary>
    private void AddSupplyingMOs(Job job, JobDataSet ds, ManufacturingOrder mo, JobDataSet.JobRow jobRow, int bomLevel)
    {
        List<ManufacturingOrder> moList = new ();
        HashSet<BaseId> moHash = new ();

        //Get a list of all MOs that supply this MO.
        for (int nodeI = 0; nodeI < mo.CurrentPath.NodeCount; nodeI++)
        {
            AlternatePath.Node node = mo.CurrentPath.GetNodeByIndex(nodeI);
            BaseOperation bOp = node.Operation;
            for (int matI = 0; matI < bOp.MaterialRequirements.Count; matI++)
            {
                MaterialRequirement mr = bOp.MaterialRequirements[matI];
                foreach (InternalActivity supplyingActivity in mr.SupplyingActivities)
                {
                    ManufacturingOrder supplyingMO = supplyingActivity.Operation.ManufacturingOrder;
                    if (moHash.Add(supplyingMO.Id))
                    {
                        moList.Add(supplyingMO);
                    }
                }
            }
        }

        //Now add the MOs to the table.
        for (int moI = 0; moI < moList.Count; moI++)
        {
            ManufacturingOrder subMo = moList[moI];
            //Don't add self to list -- can get infinite loop and not needed.
            if (!Equals(subMo, mo))
            {
                AddSubMO(jobRow, subMo.Job, subMo, ds, bomLevel, GetOnHandMaterials(subMo), GetNOTOnHandMaterials(subMo));
            }
        }
    }

    private string GetOnHandMaterials(ManufacturingOrder mo)
    {
        StringBuilder onHandMaterialsBuilder = new ();

        for (int nodeI = 0; nodeI < mo.CurrentPath.NodeCount; nodeI++)
        {
            AlternatePath.Node node = mo.CurrentPath.GetNodeByIndex(nodeI);
            BaseOperation bOp = node.Operation;
            for (int matI = 0; matI < bOp.MaterialRequirements.Count; matI++)
            {
                MaterialRequirement mr = bOp.MaterialRequirements[matI];
                if (mr.Available)
                {
                    onHandMaterialsBuilder.Append(string.Format("{0:N} {1} {2}\r\n {3}\r\n\r\n", mr.TotalRequiredQty, mr.UOM, mr.MaterialName, mr.MaterialDescription));
                }
            }
        }

        return onHandMaterialsBuilder.ToString();
    }

    private string GetNOTOnHandMaterials(ManufacturingOrder mo)
    {
        StringBuilder onHandMaterialsBuilder = new ();

        for (int nodeI = 0; nodeI < mo.CurrentPath.NodeCount; nodeI++)
        {
            AlternatePath.Node node = mo.CurrentPath.GetNodeByIndex(nodeI);
            BaseOperation bOp = node.Operation;
            for (int matI = 0; matI < bOp.MaterialRequirements.Count; matI++)
            {
                MaterialRequirement mr = bOp.MaterialRequirements[matI];
                if (!mr.Available)
                {
                    onHandMaterialsBuilder.Append(string.Format("{0:N} {1} {2}{5} {3}{5} {4}{5}{5}", mr.TotalRequiredQty, mr.UOM, mr.MaterialName, mr.MaterialDescription, mr.Source, Environment.NewLine));
                }
            }
        }

        return onHandMaterialsBuilder.ToString();
    }
}