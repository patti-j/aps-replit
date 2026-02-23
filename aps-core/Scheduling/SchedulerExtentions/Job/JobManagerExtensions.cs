using System.Collections;

using PT.Common.Debugging;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.Transmissions;

using OperationAttribute = PT.Scheduler.OperationAttribute;

namespace PT.SchedulerExtensions.Job;

public static class JobManagerExtensions
{
    /// <summary>
    /// Returns a list of Jobs that supply the specified Job either by the SuccessorJob relation or through materials, any levels deep.
    /// </summary>
    public static List<Scheduler.Job> GetSupplyingJobs(this JobManager a_jobManager, Scheduler.Job suppliedJob)
    {
        MOKeyList unusedMoKeyList;
        BaseIdList suppliedJobs = new ();
        suppliedJobs.Add(suppliedJob.Id);

        List<ManufacturingOrder> predMOs = ManufacturingOrder.GetPredecessorMOsRecursively(a_jobManager, suppliedJobs, out unusedMoKeyList, false, true);
        List<ManufacturingOrder> matlSupplyMos = new SupplyingMoList(a_jobManager, suppliedJobs, false, true).GetSupplyingMOList();

        Hashtable addedJobIds = new ();
        List<Scheduler.Job> supplyingJobs = new ();
        //Copy the unique Jobs to the output list
        for (int i = 0; i < predMOs.Count; i++)
        {
            ManufacturingOrder mo = predMOs[i];
            if (!addedJobIds.ContainsKey(mo.Job.Id))
            {
                addedJobIds.Add(mo.Job.Id, null);
                supplyingJobs.Add(mo.Job);
            }
        }

        for (int i = 0; i < matlSupplyMos.Count; i++)
        {
            ManufacturingOrder mo = matlSupplyMos[i];
            if (!addedJobIds.ContainsKey(mo.Job.Id))
            {
                addedJobIds.Add(mo.Job.Id, null);
                supplyingJobs.Add(mo.Job);
            }
        }

        return supplyingJobs;
    }

    public static MOKeyList GetMoListWithSupplyMosAdded(this JobManager a_jobManager, BaseIdList a_jobKeyList)
    {
        List<ManufacturingOrder> listOfMoLists = GetPredAndSupplyingMos(a_jobManager, a_jobKeyList);
        return SupplyingMoList.ConvertToMOKeyList(listOfMoLists);
    }

    private static List<ManufacturingOrder> GetPredAndSupplyingMos(JobManager a_jobManager, BaseIdList a_jobKeyList)
    {
        List<ManufacturingOrder> expediteMos = ManufacturingOrder.GetPredecessorMOsRecursively(a_jobManager, a_jobKeyList, out MOKeyList _, true, false);
        List<ManufacturingOrder> matlSupplyMos = new SupplyingMoList(a_jobManager, a_jobKeyList, true, true).GetSupplyingMOList();
        List<ManufacturingOrder> listOfMoLists = new();
        listOfMoLists.AddRange(expediteMos);
        listOfMoLists.AddRange(matlSupplyMos);

        return listOfMoLists;
    }

    public static List<ManufacturingOrder> GetMoExpediteListConstrainedByDate(this JobManager a_jobManager, ScenarioDetail a_sd, List<Scheduler.Job> a_jobs, ScenarioDetailExpediteBaseT.ExpediteStartDateType a_expediteStartType, DateTime a_specificDate, bool a_addSupplies)
    {
        DateTime earliestExpediteDate = PTDateTime.MaxDateTime;
        List<ManufacturingOrder> mosToExpedite = new();

        if (a_expediteStartType == ScenarioDetailExpediteBaseT.ExpediteStartDateType.Clock)
        {
            earliestExpediteDate = a_sd.ClockDate;
        }
        else if (a_expediteStartType == ScenarioDetailExpediteBaseT.ExpediteStartDateType.SpecificDateTime)
        {
            earliestExpediteDate = a_specificDate;
        }

        List<ManufacturingOrder> mos = new();
        bool isEndOfFrozenSpan = a_expediteStartType == ScenarioDetailExpediteBaseT.ExpediteStartDateType.EndOfFrozenSpan;
        bool isEndOfStableSpan = a_expediteStartType == ScenarioDetailExpediteBaseT.ExpediteStartDateType.EndOfStableSpan;

        BaseIdList jobs = new();
        foreach (Scheduler.Job curJob in a_jobs)
        {
            jobs.Add(curJob.Id);
            mosToExpedite.AddRange(curJob.ManufacturingOrders);

            if (!isEndOfFrozenSpan && !isEndOfStableSpan)
            {
                continue;
            }

            foreach (InternalOperation op in curJob.GetOperations())
            {
                if (op.IsFinishedOrOmitted || op.Predecessors.Count > 0)
                {
                    continue;
                }

                if (op.Scheduled)
                {
                    Resource res = ((ResourceOperation)op).GetScheduledPrimaryResource();
                    DateTime constrainingDate = a_sd.ClockDate.Add(res.Department.FrozenSpan);
                    if (isEndOfStableSpan)
                    {
                        constrainingDate = constrainingDate.Add(res.Department.Plant.StableSpan);
                    }

                    if (constrainingDate < earliestExpediteDate)
                    {
                        earliestExpediteDate = constrainingDate;
                    }
                }
                else
                {
                    List<Resource> eligibleResources = op.ResourceRequirements.PrimaryResourceRequirement.GetEligibleResources();
                    foreach (Resource res in eligibleResources)
                    {
                        DateTime frozenSpanDate = a_sd.ClockDate.Add(res.Department.FrozenSpan);
                        if (isEndOfStableSpan)
                        {
                            frozenSpanDate = frozenSpanDate.Add(res.Department.Plant.StableSpan);
                        }

                        if (frozenSpanDate < earliestExpediteDate)
                        {
                            earliestExpediteDate = frozenSpanDate;
                        }
                    }
                }
            }
        }

        if (a_addSupplies)
        {
            //pred mos need to be constrained by mo end date
            List<ManufacturingOrder> expediteMos = ManufacturingOrder.GetPredecessorMOsRecursively(a_jobManager, jobs, out MOKeyList _, true, false);
            foreach (ManufacturingOrder expediteMo in expediteMos)
            {
                if (expediteMo.Scheduled && expediteMo.ScheduledEndDate.Ticks > earliestExpediteDate.Ticks)
                {
                    mosToExpedite.Add(expediteMo);
                }
            }

            //supplying mos need to be constrained by primary product + material pp
            List<ManufacturingOrder> matlSupplyMos = new SupplyingMoList(a_jobManager, jobs, true, true).GetSupplyingMOList();
            foreach (ManufacturingOrder matlSupplyMo in matlSupplyMos)
            {
                if (matlSupplyMo.GetPrimaryProduct() is Product supply)
                {
                    if (matlSupplyMo.ScheduledEndDate.Ticks + supply.MaterialPostProcessingTicks > earliestExpediteDate.Ticks)
                    {
                        mosToExpedite.Add(matlSupplyMo);
                    }
                }
            }
        }

        return mosToExpedite;
    }

    public static IEnumerable<PT.Scheduler.Job> GetOverdueJobs(this JobManager a_jobManager)
    {
        return a_jobManager.Where(j => j.Overdue);
    }

    public static IEnumerable<PT.Scheduler.Job> GetOnHoldJobs(this JobManager a_jobManager)
    {
        return a_jobManager.Where(j => j.OnHold == holdTypes.OnHold);
    }

    public static IEnumerable<PT.Scheduler.Job> GetJobsForSameCustomer(this JobManager a_jobManager, Scheduler.Job a_job)
    {
        return a_jobManager.Where(j => j.Customers.Intersect(a_job.Customers).Any());
    }

    /// <summary>
    /// Returns a list populated with the Names of all Attributes from all Operations.
    /// </summary>
    /// <returns></returns>
    public static List<string> GetUniqueOperationAttributeNames(this JobManager a_jobManager)
    {
        List<string> attNames = new ();
        Hashtable attNamesHash = new ();
        for (int jobI = 0; jobI < a_jobManager.Count; jobI++)
        {
            Scheduler.Job job = a_jobManager[jobI];
            for (int moI = 0; moI < job.ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = job.ManufacturingOrders[moI];
                for (int opI = 0; opI < mo.OperationManager.Count; opI++)
                {
                    BaseOperation op = mo.OperationManager.GetByIndex(opI);
                    for (int attI = 0; attI < op.Attributes.Count; attI++)
                    {
                        OperationAttribute att = op.Attributes[attI];
                        if (att.PTAttribute.Name != null && att.PTAttribute.Name.Length > 0 && !attNamesHash.Contains(att.PTAttribute.Name))
                        {
                            attNames.Add(att.PTAttribute.Name);
                            attNamesHash.Add(att.PTAttribute.Name, null);
                        }
                    }
                }
            }
        }

        return attNames;
    }

    /// <summary>
    /// Returns a list of all Jobs either supplying the specified Job or being supplied by the specified Job.
    /// Includes relations from SuccessorMOs and Materials.
    /// The list includes the specified Job as well.
    /// </summary>
    /// <param name="job"></param>
    /// <returns></returns>
    public static List<Scheduler.Job> GetRelatedJobs(this JobManager a_jobManager, Scheduler.Job job)
    {
        Hashtable addedJobIds = new ();
        List<Scheduler.Job> outJobList = new ();
        try
        {
            addedJobIds.Add(job.Id, null);
            outJobList.Add(job);

            List<Scheduler.Job> supplyingJobs = a_jobManager.GetSupplyingJobs(job);
            for (int i = 0; i < supplyingJobs.Count; i++)
            {
                Scheduler.Job nxtJob = supplyingJobs[i];
                if (!addedJobIds.ContainsKey(nxtJob.Id))
                {
                    outJobList.Add(nxtJob);
                    addedJobIds.Add(nxtJob.Id, null);
                }
            }

            List<Scheduler.Job> suppliedJobs = job.GetSuppliedJobs();
            for (int i = 0; i < suppliedJobs.Count; i++)
            {
                Scheduler.Job nxtJob = suppliedJobs[i];
                if (!addedJobIds.ContainsKey(nxtJob.Id))
                {
                    outJobList.Add(nxtJob);
                    addedJobIds.Add(nxtJob.Id, null);
                }
            }
        }
        catch (Exception e)
        {
            DebugException.ThrowInDebug("GetRelatedJobs", e);
        }

        return outJobList;
    }
}