using System.Collections;

using PT.APSCommon;
using PT.Common.Extensions;
using PT.Scheduler.Schedule;
using PT.Scheduler.Schedule.Demand;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Extensions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of Job objects.
/// </summary>
public partial class JobManager
{
    #region Eligibility
    /// <summary>
    /// Compute eligibility for every job. Returns whether any jobs were unscheduled because they were scheduled on resources that are no long eligible.
    /// This function needs to be called:
    /// . When the system is started.
    /// . When a resource becomes active or inactive.
    /// . When a resource is added.
    /// . When a resource is deleted.
    /// . Capabilities are adjusted.
    /// There may be other times when this function need to be called too. It needs to be called whenever there is a chance that
    /// eligibility may have changed.
    /// The reason it needs to be called on system startup is that eligibilty is not stored serialized, so the datastructures for it must be recalculated.
    /// There are 5 steps to determining eligibility.
    /// The first 4 are determined before scheduling.
    /// The final step is determined at simulation time.
    /// The steps are:
    /// 1. Determine the plants and resources where a resource requirement may be satisfied.
    /// 2. Determine the plants and resources capable of satisfying all resource requirements of all operations.
    /// 3. If resource connectors are defined, resources with connectors that don't lead to an eligible successor operation are excluded.
    /// 4. Take Job.CanSpanPlants and ManufacturingOrder.CanSpanPlants into consideration.
    /// 5. During the simulation process the selection of a resource may further narrow down eligibility due to the settings of
    /// MO.CanSpanPlants and/or Job.CanSpanPlants.
    /// </summary>
    //public bool ComputeEligibility()
    //{
    //    m_jobsUnscheduled = false;

    //    if (Count > 0)
    //    {
    //        int processorCount = Environment.ProcessorCount;
    //        int processorsToUse = Math.Min(processorCount, Count);

    //        decimal jobsPerProcessorTemp = Math.Floor(Count / (decimal)processorsToUse);
    //        jobsPerProcessorTemp = Math.Max(1, jobsPerProcessorTemp); // Turn 0 into 1.

    //        int jobsPerProcessor = (int)jobsPerProcessorTemp;

    //        int startIdx = 0;
    //        int endIdx = startIdx + jobsPerProcessor;
    //        int threadI = 0;
    //        int maxIdx = Count - 1;
    //        List<Thread> threads = new List<Thread>();

    //        do
    //        {
    //            Data data = new Data(startIdx, endIdx);
    //            Thread thread = new Thread(ComputeJobEligibility);
    //            threads.Add(thread);
    //            thread.Start(data);

    //            startIdx = endIdx;
    //            endIdx += jobsPerProcessor;
    //            endIdx = Math.Min(endIdx, maxIdx);
    //            ++threadI;
    //        } while (startIdx != maxIdx);

    //        for (threadI = 0; threadI < threads.Count; ++threadI)
    //        {
    //            threads[threadI].Join();
    //        }
    //    }

    //    return m_jobsUnscheduled;
    //}

    //class Data
    //{
    //    internal Data(int a_start, int a_end)
    //    {
    //        StartIdx = a_start;
    //        EndIdx = a_end;
    //    }

    //    internal int StartIdx { get; set; }
    //    internal int EndIdx { get; set; }
    //}

    //bool m_jobsUnscheduled;

    //void ComputeJobEligibility(object a_data)
    //{
    //    Data data = (Data)a_data;

    //    for (int jobI = data.StartIdx; jobI < data.EndIdx; ++jobI)
    //    {
    //        Job job = this[jobI];
    //        job.ComputeEligibility();
    //        if (job.ScheduledStatus != JobDefs.scheduledStatuses.Finished)
    //        {
    //            if (!job.VerifyEligibilityOfScheduledResourceRequirements())
    //            {
    //                m_jobsUnscheduled = true;
    //                job.Unschedule();
    //            }
    //        }
    //    }
    //    // Need to return Exception information to the parent thread.
    //}
    public bool ComputeEligibility(bool a_recalculateJIT = true)
    {
        bool jobsUnscheduled = false;

        for (int jobI = 0; jobI < Count; jobI++)
        {
            Job job = this[jobI];
            if (job.ComputeEligibilityAndUnscheduleIfIneligible(ScenarioDetail.ProductRuleManager, a_recalculateJIT))
            {
                jobsUnscheduled = true;
            }
        }

        return jobsUnscheduled;
    }

    /// <summary>
    /// When an adjustment is made to the plant list (adds/deletes), you need to call this function so the jobs can update their
    /// MO plant eligibility.
    /// </summary>
    /// <param name="t"></param>
    internal void AdjustEligiblePlants(ScenarioBaseT a_t)
    {
        for (int jobI = 0; jobI < Count; ++jobI)
        {
            Job j = this[jobI];
            j.AdjustEligiblePlants(a_t);
        }
    }

    ///// <summary>
    ///// Returns an ArrayList of SystemMessages describing why Jobs failed to schedule.
    ///// An empty list is returned if there are no Job with a Failed to Schedule status.
    ///// </summary>
    ///// <returns></returns>
    //public ArrayList GetFailedToScheduleReasons()
    //{
    //    ArrayList systemMessages = new ArrayList();
    //    int failedCount = 0;
    //    //Put in a place holder for the summary message -- will need to fill in the text later once we know the failed count
    //    SystemMessage summaryMessage = new SystemMessage(BaseId.NULL_ID, BaseId.NULL_ID, "", SystemMessage.MessageSeverity.Warning, SystemMessage.ErrorTypes.JobScheduleFailure);
    //    systemMessages.Add(summaryMessage);

    //    Hashtable capabilitiesWithoutActiveResourcesHash = new Hashtable();
    //    for (int i = 0; i < this.Count; i++)
    //    {
    //        Job job = this[i];
    //        if (job.ScheduledStatus == JobDefs.scheduledStatuses.FailedToSchedule)
    //        {
    //            failedCount++;
    //            ArrayList jobCapabilitiesWithoutActiveResourcesArray = new ArrayList();
    //            ArrayList jobsFailedReasons = job.GetFailedToScheduleReasons(ref jobCapabilitiesWithoutActiveResourcesArray);
    //            for (int f = 0; f < jobsFailedReasons.Count; f++)
    //            {
    //                SystemMessage nextMsg = (SystemMessage)jobsFailedReasons[f];
    //                systemMessages.Add(nextMsg);
    //            }

    //            //Copy the problem capabilites from the Job into a unique list of Capabilites that have no active resources
    //            for (int capI = 0; capI < jobCapabilitiesWithoutActiveResourcesArray.Count; capI++)
    //            {
    //                Capability capability = (Capability)jobCapabilitiesWithoutActiveResourcesArray[capI];
    //                if (!capabilitiesWithoutActiveResourcesHash.Contains(capability.Id))
    //                {
    //                    string msg = String.Format("Capability {0} is not linked to any Active Resources.", capability.Name);
    //                    SystemMessage capMsg = new SystemMessage(BaseId.NULL_ID, BaseId.NULL_ID, msg, SystemMessage.MessageSeverity.Warning, SystemMessage.ErrorTypes.CapabilityProblem);
    //                    systemMessages.Add(capMsg);
    //                    capabilitiesWithoutActiveResourcesHash.Add(capability.Id, capability);
    //                }
    //            }
    //        }
    //    }
    //    //Now fill in the summary message info
    //    summaryMessage.MessageText = String.Format("There are {0} Jobs that have failed to schedule.  Details may be listed below or you can check the Job's Analysis window.", failedCount);

    //    //Get rid of the summary message if there are no failed jobs
    //    if (failedCount == 0)
    //        systemMessages.Clear();

    //    return systemMessages;
    //}
    #endregion

    #region JIT
    /// <summary>
    /// Calculate the JIT Start times of all the paths in the jobs. Cancelled and Finished jobs are not included.
    /// </summary>
    /// <param name="a_onlyScheduledJobs">Whether to limit the JIT calculation to scheduled jobs.</param>
    public void CalculateJitTime(long a_simClock, bool a_onlyScheduledJobs)
    {
        /// Since it's possible for a job to have multiple predecessor jobs and JITS of
        /// successor jobs are calculated first. To prevent JITS from being calculated twice,
        /// 
        HashSet<BaseId> alreadyCalculatedMoIds = new ();
        foreach (Job job in JobEnumerator)
        {
            if (job.ScheduledStatus != JobDefs.scheduledStatuses.Cancelled && job.ScheduledStatus != JobDefs.scheduledStatuses.Finished)
            {
                if (a_onlyScheduledJobs == false || (a_onlyScheduledJobs && job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled))
                {
                    job.CalculateJitTimes(a_simClock, false, alreadyCalculatedMoIds);
                }
            }
        }
    }
    #endregion

    #region SuccessorMO
    /// <summary>
    /// This function links predecessor and successor MOs within the SucMO class.
    /// It is important that this function be called in the following circumstances
    /// so that the predecessor and successor mos are linked and unliked correctly
    /// (esspecially in the case where jobs or MOs are deleted).
    /// Some of the circumstances in which this function include:
    /// 1. MO deleted.
    /// 2. MO updated.
    /// 3. MO created.
    /// 4. Finish of predecessor MO.
    /// 5. Finish of successor operation.
    /// 6. Deserialization.
    /// </summary>
    internal void LinkSuccessorsInAllJobs()
    {
        InitFastLookupByExternalId();
        try
        {
            for (int jobI = 0; jobI < Count; ++jobI)
            {
                Job job = this[jobI];
                job.LinkSuccessorMOs();
            }
        }
        finally
        {
            DeInitFastLookupByExternalId();
        }

        ScenarioDetail.SuccessorMOsLinked();
    }
    #endregion

    #region Simulation
    /// <summary>
    /// Setup state variables for simulation.
    /// </summary>
    internal void ResetSimulationStateVariables(ScenarioDetail a_sd)
    {
        for (int i = 0; i < Count; i++)
        {
            Job job = this[i];
            job.ResetSimulationStateVariables(a_sd);
        }
    }

    internal void ResetSimulationStateVariables2()
    {
        for (int i = 0; i < Count; i++)
        {
            Job job = this[i];
            job.ResetSimulationStateVariables2();
        }
    }

    /// <summary>
    /// More simulation initialization that occurs after simulation state variable initialization.
    /// </summary>
    internal void SimulationInitialization(PlantManager a_plantManager, ProductRuleManager a_productRuleManager, ExtensionController a_extensionController, ICalculatedValueCacheManager a_cacheManager)
    {
        for (int jobI = 0; jobI < Count; ++jobI)
        {
            Job job = this[jobI];

            if (!job.Cancelled)
            {
                this[jobI].SimulationInitialization(a_plantManager, a_productRuleManager, a_extensionController, a_cacheManager);
            }
        }
    }

    internal void PostSimulationInitialization()
    {
        for (int jobI = 0; jobI < Count; ++jobI)
        {
            Job job = this[jobI];

            if (!job.Cancelled)
            {
                this[jobI].PostSimulationInitialization();
            }
        }
    }

    /// <summary>
    /// This is called after Simulation only if a PostSimStageCust is loaded.
    /// </summary>
    /// <param name="a_simType"></param>
    /// <param name="a_t"></param>
    /// <param name="a_sd"></param>
    internal ManufacturingOrder.PostSimStageChangeTypes PostSimStageCust(long a_simClock, ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, int a_curSimStageIdx, int a_finalSimStageIdx)
    {
        ManufacturingOrder.PostSimStageChangeTypes postSimChanges = ManufacturingOrder.PostSimStageChangeTypes.None;
        for (int jobI = 0; jobI < Count; ++jobI)
        {
            Job job = this[jobI];

            if (!job.Cancelled)
            {
                ManufacturingOrder.PostSimStageChangeTypes jobPostSimChanges = ManufacturingOrder.PostSimStageChangeTypes.None;
                jobPostSimChanges = this[jobI].PostSimStageCust(a_simClock, a_simType, a_t, a_sd, a_curSimStageIdx, a_finalSimStageIdx);
                postSimChanges |= jobPostSimChanges;
            }
        }

        int needFlagged = (int)postSimChanges & (int)ManufacturingOrder.PostSimStageChangeTypes.NeedDate;
        if (needFlagged != 0)
        {
            CalculateJitTime(a_simClock, false);
        }

        return postSimChanges;
    }

    /// <summary>
    /// This is called after Simulation only if a PostSimStageCust is loaded.
    /// </summary>
    /// <param name="a_simType"></param>
    /// <param name="a_t"></param>
    /// <param name="a_sd"></param>
    internal ManufacturingOrder.PostSimStageChangeTypes EndOfSimulationCustExecute(long a_simClock, ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd)
    {
        ManufacturingOrder.PostSimStageChangeTypes postSimChanges = ManufacturingOrder.PostSimStageChangeTypes.None;
        for (int jobI = 0; jobI < Count; ++jobI)
        {
            Job job = this[jobI];

            if (!job.Cancelled)
            {
                ManufacturingOrder.PostSimStageChangeTypes jobPostSimChanges = ManufacturingOrder.PostSimStageChangeTypes.None;
                jobPostSimChanges = this[jobI].EndOfSimulationCustExecute(a_simClock, a_simType, a_t, a_sd);
                postSimChanges |= jobPostSimChanges;
            }
        }

        int needFlagged = (int)postSimChanges & (int)ManufacturingOrder.PostSimStageChangeTypes.NeedDate;
        if (needFlagged != 0)
        {
            CalculateJitTime(a_simClock, false);
        }

        int qtyFlag = (int)postSimChanges & (int)ManufacturingOrder.PostSimStageChangeTypes.RequiredQuantity;
        if (qtyFlag != 0)
        {
            a_sd.ExtensionController.DataChanges.FlagProductionChanges(BaseId.NULL_ID);
        }

        return postSimChanges;
    }
    #endregion

    #region Tempaltes
    internal class DuplicateTemplateException : Exception
    {
        internal DuplicateTemplateException(string errMsg)
            : base(errMsg) { }
    }
    #endregion Templates

    #region Quantity Adjustments
    internal void HandleScenarioDetailChangeMOQtyT(ScenarioDetailChangeMOQtyT a_t, ProductRuleManager a_productRuleManager)
    {
        Job job = GetById(a_t.ActivityKey.JobId);

        if (job == null)
        {
            throw new PTValidationException("2487");
        }

        job.HandleScenarioDetailChangeMOQtyT(a_t, a_productRuleManager);
    }
    #endregion

    #region Split Job
    internal Job Split(long a_simClock, Job a_sourceJob, decimal a_splitRatio, ProductRuleManager a_productRuleManager)
    {
        if (a_sourceJob.ManufacturingOrders.Count != 1)
        {
            throw new PTValidationException("2488");
        }

        ManufacturingOrder sourceMO = a_sourceJob.GetFirstMO();
        Simulation.Scheduler.Job.SplitHelpers.MOSplittableValidation(sourceMO);


        //We need to store the currently scheduled resources by operation to check eligibility after split
        Dictionary<string, Resource> opScheduledResource = new ();
        foreach (ResourceOperation op in a_sourceJob.GetOperationsFromCurrentPaths())
        {
            //TODO check all ResourceRequirements, not just lead act
            opScheduledResource[op.ExternalId] = op.GetScheduledPrimaryResource();
        }

        Job newJob = Copy(a_sourceJob);
        newJob.InitGeneratedJob(a_simClock, ScenarioDetail.ProductRuleManager);

        ManufacturingOrder newMO = newJob.GetFirstMO();

        bool unschedule = sourceMO.AdjustRequiredQtyByRatio(newMO, a_splitRatio, ScenarioDetail.ProductRuleManager);
        if (unschedule)
        {
            a_sourceJob.Unschedule();
        }

        //We need to check the new job's eligibility based on the previous job's scheduled operations
        foreach (ResourceOperation newOp in newJob.GetOperationsFromCurrentPaths())
        {
            //Find the scheduled resource of the equivalent op in the source job
            Resource scheduledResource = opScheduledResource[newOp.ExternalId];
            if (!newOp.ResourceRequirements.PrimaryResourceRequirement.IsEligible(scheduledResource, a_productRuleManager))
            {
                newJob.Unschedule();
                break;
            }
        }

        sourceMO.PreserveRequiredQty = true;
        newMO.PreserveRequiredQty = true;

        //Need to compute eligiblity because other resources may now be capable 
        a_sourceJob.ComputeEligibility(ScenarioDetail.ProductRuleManager);
        newJob.ComputeEligibility(ScenarioDetail.ProductRuleManager);

        newJob.MaintenanceMethod = JobDefs.EMaintenanceMethod.Manual;
        newJob.SplitOffFromOtherJob = true;
        newJob.SplitDuringOptimizeAndToBeScheduled = true;
        Add(newJob);

        newJob.ScheduledStatus_set = a_sourceJob.ScheduledStatus;

        return newJob;
    }

    internal void Join(Job a_leftJob, Job a_rightJob, ProductRuleManager a_productRuleManager, IScenarioDataChanges a_scenarioDataChanges)
    {
        Join(a_leftJob.GetFirstMO(), a_rightJob.GetFirstMO(), a_productRuleManager, a_scenarioDataChanges); //can remove once the function calling this is updated to match the parameters
    }

    internal void Join(ManufacturingOrder a_leftMO, ManufacturingOrder a_rightMO, ProductRuleManager a_productRuleManager, IScenarioDataChanges a_scenarioDataChanges)
    {
        bool needsToUnschedule = a_leftMO.Join(a_rightMO, a_productRuleManager);
        JoinLots(a_leftMO, a_rightMO);
        a_leftMO.Job.Notes += string.Format("{2}{2}** Joined Job {0} **{2}{1}", a_rightMO.Job.Name, a_rightMO.Job.Notes, Environment.NewLine);
        if (a_rightMO.Job.ManufacturingOrderCount == 1)
        {
            DeleteJob(a_rightMO.Job, a_scenarioDataChanges);
        }
        else
        {
            a_rightMO.Job.ManufacturingOrders.Delete(a_rightMO, a_scenarioDataChanges);
        }

        if (needsToUnschedule)
        {
            a_leftMO.Job.Unschedule();
        }

        //Need to compute eligiblity because other resources may now be capable 
        a_leftMO.Job.ComputeEligibility(ScenarioDetail.ProductRuleManager);
    }

    private static void JoinLots(ManufacturingOrder a_resultingMo, ManufacturingOrder a_joiningMo)
    {
        //Join Lot Codes
        try
        {
            //Note: this does not support multiple products of the same item in the MO.
            //First collect all of the Product lot codes
            Dictionary<BaseId, string> productCodesToJoin = new ();
            List<ResourceOperation> resourceOperations = a_resultingMo.CurrentPath.GetOperationsByLevel(true);
            foreach (ResourceOperation operation in resourceOperations)
            {
                foreach (Product product in operation.Products)
                {
                    if (string.IsNullOrWhiteSpace(product.LotCode))
                    {
                        continue;
                    }

                    if (productCodesToJoin.TryGetValue(product.Item.Id, out string existingCode))
                    {
                        //TODO: Multuple products of the same item 
                        //this is not compatible
                    }
                    else
                    {
                        productCodesToJoin.Add(product.Item.Id, product.LotCode);
                    }
                }
            }

            //Update current MO Products and collect replacements
            Dictionary<string, string> replacementCodes = new ();
            resourceOperations = a_joiningMo.CurrentPath.GetOperationsByLevel(true);
            foreach (ResourceOperation operation in resourceOperations)
            {
                foreach (Product product in operation.Products)
                {
                    if (productCodesToJoin.TryGetValue(product.Item.Id, out string lotCodeToReplace))
                    {
                        if (lotCodeToReplace == product.LotCode)
                        {
                            //The lot codes are the same, not need to update anything
                            continue;
                        }

                        if (!replacementCodes.ContainsKey(lotCodeToReplace))
                        {
                            replacementCodes.Add(product.LotCode, lotCodeToReplace);
                        }
                    }
                }
            }

            //Replace MR lot codes using the replaced product codes
            if (replacementCodes.Count > 0)
            {
                List<ResourceOperation> ops = a_resultingMo.CurrentPath.GetOperationsByLevel(true);
                HashSet<BaseId> processedJobs = new ();
                foreach (ResourceOperation operation in ops)
                {
                    foreach (Product product in operation.Products)
                    {
                        if (string.IsNullOrWhiteSpace(product.LotCode))
                        {
                            continue;
                        }

                        //Find adjustment representing consumption of the joined MO product qty
                        AdjustmentArray adjArray = product.Inventory.GetAdjustmentArray();
                        for (int i = 0; i < adjArray.Count; i++)
                        {
                            Adjustment adj = adjArray[i];

                            //If adj reason is an activity, update the Job MRs lot codes
                            if (adj.ChangeQty < 0 && adj is ActivityAdjustment actAdj)
                            {
                                InternalActivity act = actAdj.Activity;
                                if (!processedJobs.Contains(act.Job.Id))
                                {
                                    if (act.Job.ReplaceJobMRLotCodes(replacementCodes))
                                    {
                                        processedJobs.Add(act.Job.Id);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Now we need to take all of the lot codes that were on the joining MO and add them to the MR on the resulting MO.
            Dictionary<BaseId, List<string>> lotCodesToJoin = new ();
            resourceOperations = a_joiningMo.CurrentPath.GetOperationsByLevel(true);
            foreach (ResourceOperation operation in resourceOperations)
            {
                foreach (MaterialRequirement mr in operation.MaterialRequirements)
                {
                    if (mr.BuyDirect || !mr.MustUseEligLot)
                    {
                        continue;
                    }

                    List<string> newlotCodes = new ();

                    Dictionary<string, EligibleLot>.Enumerator enumerator = mr.EligibleLots.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        newlotCodes.Add(enumerator.Current.Value.LotId);
                    }

                    if (lotCodesToJoin.TryGetValue(mr.Item.Id, out List<string> lotCodes))
                    {
                        lotCodes.AddRange(newlotCodes);
                    }
                    else
                    {
                        lotCodesToJoin.Add(mr.Item.Id, newlotCodes);
                    }
                }
            }

            if (lotCodesToJoin.Count > 0)
            {
                //Now that we have the list, update the resulting MO
                resourceOperations = a_resultingMo.CurrentPath.GetOperationsByLevel(true);
                foreach (ResourceOperation operation in resourceOperations)
                {
                    foreach (MaterialRequirement mr in operation.MaterialRequirements)
                    {
                        if (mr.BuyDirect || !mr.MustUseEligLot)
                        {
                            continue;
                        }

                        if (lotCodesToJoin.TryGetValue(mr.Item.Id, out List<string> extraLotCodes))
                        {
                            //This MR needs to be updated
                            foreach (string extraLotCode in extraLotCodes)
                            {
                                if (!mr.EligibleLots.Contains(extraLotCode))
                                {
                                    mr.EligibleLots.Add(extraLotCode, mr);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            //TODO: Log errors.
            //Catch everything for now. 
        }
    }
    #endregion

    /// <summary>
    /// Return a list of all the jobs that are scheduled.
    /// </summary>
    /// <returns></returns>
    internal List<Job> GetScheduledJobs()
    {
        List<Job> scheduledJobs = new ();

        for (int i = 0; i < Count; ++i)
        {
            Job j = this[i];
            if (j.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
            {
                scheduledJobs.Add(j);
            }
        }

        return scheduledJobs;
    }

    //void ProductRules_AssociateRulesAndOperations(List<Job> a_jobs)
    //{
    //    for (int jobI = 0; jobI < a_jobs.Count; ++jobI)
    //    {
    //        Job job = a_jobs[jobI];
    //        ProductRules_AssociateRulesAndOperations(job);
    //    }
    //}

    //internal void ProductRules_AssociateRulesAndOperations(Job a_job)
    //{
    //    for (int moI = 0; moI < a_job.ManufacturingOrders.Count; ++moI)
    //    {
    //        ManufacturingOrder mo = a_job.ManufacturingOrders[moI];
    //        for (int opI = 0; opI < mo.OperationManager.Count; ++opI)
    //        {
    //            BaseOperation bo = mo.OperationManager.GetByIndex(opI);
    //            ProductRules_AssociateRulesAndOperations(bo);
    //        }
    //    }
    //}

    //private void ProductRules_AssociateRulesAndOperations(BaseOperation bo)
    //{
    //    Item item = bo.GetItemForProductRule();
    //    if (item != null)
    //    {
    //        ProductRule[] productRules = m_productRulesByItemExternalIdAndOpnName.GetProductRule(item.ExternalId, bo.Name);
    //        bo.m_productRuleArray = productRules;
    //    }
    //    else
    //    {
    //        bo.m_productRuleArray = null;
    //    }
    //}

    /// <summary>
    /// Lock and/or Anchor activities that start before a time.
    /// </summary>
    internal void LockAndAnchorBefore(OptimizeSettings.ETimePoints a_startSpan, ScenarioOptions a_scenarioOptions, IScenarioDataChanges a_dataChanges)
    {
        for (int i = 0; i < Count; i++)
        {
            Job job = this[i];
            job.LockAndAnchorBefore(a_startSpan, a_scenarioOptions);
            a_dataChanges.JobChanges.UpdatedObject(job.Id);
        }
    }

    /// <summary>
    /// Add all the lot codes of lots used as eligible lots to an EligibleLots<string>
    /// </summary>
    /// <param name="">The EligibleLots to add to.</param>
    internal void AddEligibleLotCodes(EligibleLots a_eligibleLotCodes)
    {
        foreach (Job job in this)
        {
            if (!job.Template)
            {
                //Add all paths. Once a path is scheduled, activities from other paths are removed from the dispatcher.
                //During that removal, their lot usages will be removed from the tracking.
                //If we don't add all paths, an alternate path move will fail
                foreach (ManufacturingOrder mo in job)
                {
                    for (int i = 0; i < mo.OperationManager.Count; i++)
                    {
                        BaseOperation baseOperation = mo.OperationManager.GetByIndex(i);
                        if (!baseOperation.IsFinishedOrOmitted)
                        {
                            foreach (MaterialRequirement mr in baseOperation.MaterialRequirements)
                            {
                                if (mr.MustUseEligLot && !mr.IssuedComplete)
                                {
                                    mr.AddEligibleLotCodes(a_eligibleLotCodes);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Some information used to help keep track of subcomponents.
    /// </summary>
    public class SubComponentSource
    {
        internal SubComponentSource(long a_level, BaseIdObject a_source, BaseIdObject a_destination, Product a_linkedProduct)
        {
            Level = a_level;
            Source = a_source;
            Destination = a_destination;
            LinkedProduct = a_linkedProduct;
        }

        public long Level { get; private set; }

        public BaseIdObject Destination { get; private set; }

        public BaseIdObject Source { get; private set; }
        public Product LinkedProduct { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}. {1}", Level, Source);
        }
    }

    /// <summary>
    /// Recursively find all the subcomponent sources of a job.
    /// </summary>
    /// <param name="a_jobManager"></param>
    /// <returns></returns>
    public List<SubComponentSource> FindAllSubComponents(Job a_job)
    {
        List<SubComponentSource> sources = new ();

        // The first entry is the top level job.
        sources.Add(new SubComponentSource(0, a_job, a_job, a_job.GetPrimaryProduct()));

        //This collection is used to help track and prevent an endless recursion in the AddSubcomponent where jobs were getting re-added
        HashSet<BaseId> addedJobs = new HashSet<BaseId>();

        AddJob(a_job, 0, sources,addedJobs);
        return sources;
    }

    private void AddJob(Job a_sourceJob, long a_level, List<SubComponentSource> a_sources, HashSet<BaseId> a_addedJobs)
    {
        a_addedJobs.Add(a_sourceJob.Id);

        long nextLevel = a_level + 1;

        foreach (ManufacturingOrder mo in a_sourceJob.ManufacturingOrders)
        {
            IDictionaryEnumerator opEtr = mo.OperationManager.GetOpEnumerator();

            while (opEtr.MoveNext())
            {
                InternalOperation op = (InternalOperation)opEtr.Value;
                foreach (MaterialRequirement mr in op.MaterialRequirements)
                {
                    AddSubcomponent(mr, op, a_sources, nextLevel, a_addedJobs);
                }
            }

            //List<ManufacturingOrder> predMOList = mo.GetPredecessorMOs(this);

            //foreach (ManufacturingOrder predMO in predMOList)
            //{
            //    a_sources.Add(new SubComponentSource(nextLevel, predMO, a_job));
            //    AddSubcomponentMO(predMO, a_job, a_sources, nextLevel);
            //}
        }
    }

    private void AddSubcomponent(MaterialRequirement a_mr, InternalOperation a_destOp, List<SubComponentSource> a_sources, long a_level, HashSet<BaseId> a_addedJobs)
    {
        if (a_mr.SupplyingActivityAdjustments.Any())
        {
            HashSet<BaseId> sourceIds = new ();
            foreach ((InternalActivity internalActivity, decimal qty, bool expired) in a_mr.SupplyingActivityAdjustments)
            {
                //To prevent an endless recursion if the job has already been added
                if (!a_addedJobs.Contains(internalActivity.Operation.Job.Id))
                {
                    AddJob(internalActivity.Operation.Job, a_level, a_sources, a_addedJobs);
                }

                if (sourceIds.AddIfNew(internalActivity.Id))
                {
                    Product linkedProduct = internalActivity.Operation.Products.GetByItemId(a_mr.Item.Id);

                    a_sources.Add(new SubComponentSource(a_level, internalActivity, a_destOp, linkedProduct));
                }
            }
        }
        
        //foreach (Simulation.MRSupplyNode supply in a_mr.MRSupply)
        //{
        //    if (supply.Supply is InternalActivity ia )
        //    {
        //        Job supplyJob = ia.Job;
        //        AddJob(supplyJob, a_level, a_sources);
        //    }
        //    else
        //    {
        //        a_sources.Add(new SubComponentSource(a_level, a_destOp, supply.Supply));
        //    }
        //}
    }

    //private void AddSubcomponentMO(ManufacturingOrder a_source, Job a_destJob, List<SubComponentSource> a_sources, long a_level)
    //{
    //    a_sources.Add(new SubComponentSource(a_level, a_source, a_destJob));
    //    List<ManufacturingOrder> predMOList = a_source.GetPredecessorMOs(this);

    //    long nextLevel = a_level + 1;
    //    foreach (ManufacturingOrder predMO in predMOList)
    //    {
    //        a_sources.Add(new SubComponentSource(nextLevel, predMO, a_destJob));
    //    }
    //}
    internal void SimulationActionComplete()
    {
        foreach (Job job in JobEnumerator)
        {
            foreach (ManufacturingOrder mo in job.ManufacturingOrders)
            {
                foreach (MaterialRequirement mr in mo.GetMaterialRequirements())
                {
                    mr.SimulationActionComplete();
                }
            }
        }
    }
}