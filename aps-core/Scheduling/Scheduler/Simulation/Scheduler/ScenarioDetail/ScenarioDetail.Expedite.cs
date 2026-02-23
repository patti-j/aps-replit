using System.Text;

using PT.APSCommon;
using PT.Scheduler.AuditObject;
using PT.Scheduler.Simulation.Events;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

partial class ScenarioDetail
{
    internal void ExpediteJobs(ScenarioDetailExpediteJobsT a_expediteT, IScenarioDataChanges a_dataChanges)
    {
        try
        {
            MOKeyList expediteMOKeyList = new ();

            InternalResource resToScheduleEligibleLeadOpsOn = ValidateExpediteResourceToScheduleEligibleLeadActivitiesOn(a_expediteT.ResToScheduleEligibleLeadActivitiesOn);

            foreach (BaseId jobId in a_expediteT.Jobs)
            {
                Job job = JobManager.GetById(jobId);

                if (job == null)
                {
                    throw new ExpediteValidationException("2524", new object[] { jobId.ToString() });
                }

                if (job.Template) //create a new Job based on the template.  This is to allow drag-and-drop of a Template onto the Gantt to create a new Job.
                {
                    job = JobManager.Copy(job);
                    job.Template = false;
                    job.NeedDateTicks = a_expediteT.Date;
                    job.ComputeEligibility(m_productRuleManager);
                    JobManager.AddNewJob(job);
                }

                MOKeyList jobMOs = job.GetMOKeyList();
                MOKeyList.Node moNode = jobMOs.First;

                while (moNode != null)
                {
                    expediteMOKeyList.Add(moNode.Data, null);
                    moNode = moNode.Next;
                }
            }

            ExpediteMOs(a_expediteT, expediteMOKeyList, a_expediteT.LockToResources, a_expediteT.Anchor, resToScheduleEligibleLeadOpsOn, a_dataChanges);
        }
        catch (SimulationValidationException e)
        {
            FireSimulationValidationFailureEvent(e, a_expediteT);
            throw;
        }
    }

    internal void ExpediteMOs(ScenarioDetailExpediteMOsT a_expediteT, IScenarioDataChanges a_dataChanges)
    {
        ExpediteMOs(a_expediteT, a_expediteT.MOs, a_expediteT.LockToResources, a_expediteT.Anchor, null, a_dataChanges);
    }

    private InternalResource ValidateExpediteResourceToScheduleEligibleLeadActivitiesOn(BaseId a_resId)
    {
        InternalResource resToScheduleEligibleLeadActivitiesOn = null;

        if (a_resId != BaseId.NULL_ID)
        {
            if ((resToScheduleEligibleLeadActivitiesOn = (InternalResource)PlantManager.GetResource(a_resId)) == null)
            {
                throw new ExpediteValidationException("2525", new object[] { a_resId });
            }
        }

        return resToScheduleEligibleLeadActivitiesOn;
    }

    /// <summary>
    /// Expedite a single MO to the specified time.
    /// </summary>
    /// <param name="t">The transmission that ended up causing this to occur.</param>
    /// <param name="expediteDate">The MO expedite time.</param>
    internal void ExpediteMO(ScenarioBaseT a_t, long a_expediteDate, ManufacturingOrder a_mo, bool a_resourceLock, bool a_anchor, IScenarioDataChanges a_dataChanges)
    {
        MOKeyList expediteMOKeyList = new();
        ManufacturingOrderKey moKey = new(a_mo.Job.Id, a_mo.Id);
        expediteMOKeyList.Add(moKey, null);
        ExpediteMOs(a_t, ScenarioDetailExpediteBaseT.ExpediteStartDateType.SpecificDateTime, a_expediteDate, expediteMOKeyList, a_resourceLock, a_anchor, true, null, null, a_dataChanges);
    }

    private void ExpediteMOs(ScenarioDetailExpediteBaseT a_expediteT, MOKeyList a_expediteMOKeyList, bool a_resourceLock, bool a_anchor, InternalResource a_resToScheduleEligibleLeadOpsOn, IScenarioDataChanges a_dataChanges)
    {
        long expediteDate = GetAndValidateExpediteStart(a_expediteT);
        ExpediteMOs(a_expediteT, a_expediteT.StartDateType, expediteDate, a_expediteMOKeyList, a_resourceLock, a_anchor, a_expediteT.ManuallyTriggered, a_resToScheduleEligibleLeadOpsOn, a_expediteT.AlternatePathExternalId, a_dataChanges);
    }

    private void ExpediteMOs(ScenarioBaseT a_expediteT, ScenarioDetailExpediteBaseT.ExpediteStartDateType a_expediteStartAdjustment, long a_expediteDate, MOKeyList a_expediteMOKeyList, bool a_resourceLock, bool a_anchor, bool a_manuallyTriggered,
                             InternalResource a_resToScheduleEligibleLeadOpsOn, string a_targetPathExternalId, IScenarioDataChanges a_dataChanges)
    {
        List<ManufacturingOrder> expediteMOs = new (a_expediteMOKeyList.Count);

        Dictionary<ManufacturingOrderKey, ExpediteAudit> expediteTracker = new Dictionary<ManufacturingOrderKey, ExpediteAudit>();
        try
        {
            #if TEST
                SimDebugSetup();
            #endif

            if (a_expediteMOKeyList.Count == 0)
            {
                throw new ExpediteValidationException("2526");
            }

            MOKeyList.Node moKeyListNode = a_expediteMOKeyList.First;

            while (moKeyListNode != null)
            {
                ManufacturingOrderKey moKey = moKeyListNode.Data;

                Job job = m_jobManager.GetById(moKey.JobId);

                if (job == null)
                {
                    throw new ExpediteValidationException("2527");
                }

                ManufacturingOrder mo = job.ManufacturingOrders.GetById(moKey.MOId);
                if (mo == null)
                {
                    throw new ExpediteValidationException("2528");
                }

                expediteMOs.Add(mo);

                expediteTracker.Add(moKey, new ExpediteAudit(mo, ClockDate));

                moKeyListNode = moKeyListNode.Next;
            }
        }
        catch (SimulationValidationException e)
        {
            SimulationProgress.FireSimulationProgressEvent(this, SimulationType.Expedite, a_expediteT, SimulationProgress.Status.Terminated, m_nbrOfSimulationsSinceStartup);
            FireSimulationValidationFailureEvent(e, m_activeOptimizeSettingsT);
            throw;
        }

        ExpediteMOs(a_expediteT, a_expediteStartAdjustment, a_expediteDate, expediteMOs, a_resourceLock, a_anchor, a_manuallyTriggered, a_resToScheduleEligibleLeadOpsOn, a_targetPathExternalId, a_dataChanges);

        MOKeyList.Node moNode = a_expediteMOKeyList.First;

        while (moNode != null)
        {
            ManufacturingOrderKey moKey = moNode.Data;

            Job job = m_jobManager.GetById(moKey.JobId);

            if (job == null)
            {
                throw new ExpediteValidationException("2527");
            }

            ManufacturingOrder mo = job.ManufacturingOrders.GetById(moKey.MOId);
            if (mo == null)
            {
                throw new ExpediteValidationException("2528");
            }
            
            if (expediteTracker.TryGetValue(moKey, out ExpediteAudit expediteAudit))
            {
                ManufacturingOrder manufacturingOrder = JobManager.GetById(moKey.JobId).ManufacturingOrders.GetById(moKey.MOId);
                expediteAudit.ExpediteStatus = manufacturingOrder.ScheduledStartDate != expediteAudit.FromStartDate && manufacturingOrder.ScheduledStartDate < ClockDate + ScenarioOptions.PlanningHorizon;
                expediteAudit.ToStartDate = manufacturingOrder.ScheduledStartDate;

                ResourceOperation leadOperation = manufacturingOrder.GetLeadOperation() as ResourceOperation;
                if (leadOperation != null)
                {
                    Resource scheduledPrimaryResource = leadOperation.GetScheduledPrimaryResource();
                    expediteAudit.ToDepartmentFrozenSpan = ClockDate + scheduledPrimaryResource.Department.FrozenSpan < manufacturingOrder.ScheduledStartDate;
                    expediteAudit.ToPlantStableSpan = ClockDate + scheduledPrimaryResource.Department.Plant.StableSpan < manufacturingOrder.ScheduledStartDate;
                }

                a_dataChanges.AuditEntry(expediteAudit.GetAuditEntry());
            }

            moNode = moNode.Next;
        }
       
    }

    /// <summary>
    /// Determine the start of expedite which may vary depending on whether the department Frozen Span is being used.
    /// </summary>
    private SimulationTimePoint m_expediteStartTime;

    private void ExpediteMOs(ScenarioBaseT a_T, ScenarioDetailExpediteBaseT.ExpediteStartDateType a_expediteStartAdjustment, long a_defaultExpediteDate, List<ManufacturingOrder> a_expediteMOs, bool a_resourceLock, bool a_anchor, bool a_manuallyTriggered,
                             InternalResource a_resToLockEligibleLeadOpsTo, string a_targetPathExternalId, IScenarioDataChanges a_dataChanges)
    {
        if (PTSystem.LicenseKey.TrialVersion)
        {
            long totalMOsToBeScheduledCnt = JobManager.GetScheduledMOsCount();

            for (int moI = a_expediteMOs.Count - 1; moI >= 0; --moI)
            {
                ManufacturingOrder mo = a_expediteMOs[moI];
                Job job = mo.Job;
                if (!job.Scheduled)
                {
                    ++totalMOsToBeScheduledCnt;
                }
            }

            if (totalMOsToBeScheduledCnt > c_trialDemoMaxMOsAllowedToSchedule)
            {
                throw new PTValidationException("2836");
            }
        }

        // Validate the expedited MOs have the required target path
        if (!string.IsNullOrEmpty(a_targetPathExternalId))
        {
            for (int moI = 0; moI < a_expediteMOs.Count; ++moI)
            {
                ManufacturingOrder mo = a_expediteMOs[moI];
                if (mo.AlternatePaths.FindByExternalId(a_targetPathExternalId) == null)
                {
                    throw new PTValidationException("3036", new object[] { mo.Job.Name, mo.Name, a_targetPathExternalId });
                }
            }
        }

        /*
         *
         * Determine the start of the Expedite.
         *
         * */
        LockEligibleLeadOperationsToResourceHelper lockEligibleLeadOperationsToResourceHelper = null;
        SimulationType simType = SimulationType.Expedite;

        OptimizeSettings.ETimePoints expediteStartAdjustment;
        switch (a_expediteStartAdjustment)
        {
            case ScenarioDetailExpediteBaseT.ExpediteStartDateType.Clock:
                expediteStartAdjustment = OptimizeSettings.ETimePoints.CurrentPTClock;
                break;

            case ScenarioDetailExpediteBaseT.ExpediteStartDateType.EndOfFrozenSpan:
                expediteStartAdjustment = OptimizeSettings.ETimePoints.EndOfFrozenZone;
                break;

            case ScenarioDetailExpediteBaseT.ExpediteStartDateType.SpecificDateTime:
                expediteStartAdjustment = OptimizeSettings.ETimePoints.SpecificDateTime;
                break;
            case ScenarioDetailExpediteBaseT.ExpediteStartDateType.EndOfStableSpan:
                expediteStartAdjustment = OptimizeSettings.ETimePoints.EndOfStableZone;
                break;
            default:
                throw new CommonException("A new ExpediteStartDateType wasn't handled.");
        }

        m_expediteStartTime = new SimulationTimePoint(this, a_defaultExpediteDate, expediteStartAdjustment);

        try
        {
            #if TEST
                SimDebugSetup();
            #endif
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_T, SimulationProgress.Status.Initializing, m_nbrOfSimulationsSinceStartup);
            Common.Testing.Timing ts = CreateTiming("ExpediteMOs");

            MainResourceSet availableResources;
            CreateActiveResourceList(out availableResources);
            SimulationInitializationAll(availableResources, a_T, simType, ExpediteProcessPathReleaseHandler);

            foreach (Job job in JobManager.JobEnumerator)
            {
                //If MOs were resized, set back to original Qty.
                foreach (ManufacturingOrder mo in job.ManufacturingOrders)
                {
                    if (mo.Resized)
                    {
                        mo.AdjustToOriginalQty(null, this.ProductRuleManager);
                    }
                }
            }

            List<Job> jobsToSchedule = JobManager.GetScheduledJobs();
            SimulationInitialization1();
            SimulationInitialization2(availableResources, a_T, simType, ExpediteProcessPathReleaseHandler);
            SimulationInitialization3(availableResources, jobsToSchedule, simType);
            // Expedite doesn't preserve anchor time of unscheduled anchored activities.

            /*
             *
             * Validate MOs are satisfiable, not cancelled, and lock the lead activity to a specific resource (if necessary).
             *
             * */
            List<Job> jobsWithIneligiblePaths = new ();
            bool allOpsFinishedOrOmitted = true;

            for (int moI = 0; moI < a_expediteMOs.Count; ++moI)
            {
                ManufacturingOrder mo = a_expediteMOs[moI];
                Job job = mo.Job;

                if (job.Cancelled || job.Finished)
                {
                    throw new ExpediteValidationException("2529", new object[] { a_expediteMOs[moI].Job.ExternalId });
                }

                if (mo.IsFinishedOrOmitted)
                {
                    continue;
                }

                allOpsFinishedOrOmitted = false;

                foreach (BaseOperation operation in job.GetOperations())
                {
                    if (operation is ResourceOperation op)
                    {
                        bool scheduledOnManualScheduleOnlyResource = false;
                        Resource primaryResource = op.GetScheduledPrimaryResource();
                        if (primaryResource != null && primaryResource.ManualSchedulingOnly)
                        {
                            scheduledOnManualScheduleOnlyResource = true;
                        }
                        if (op.Split && op.AutoSplitInfo.AutoSplitType != OperationDefs.EAutoSplitType.None && op.AutoSplitInfo.CanAutoJoin(m_activeSimulationType, op, scheduledOnManualScheduleOnlyResource))
                        {
                            op.Unsplit(new ScenarioDataChanges(), true);
                        }
                    }
                }

                for (int i = 0; i < mo.OperationManager.Count; i++)
                {
                    if (mo.OperationManager.GetByIndex(i) is ResourceOperation op)
                    {
                        //Rejoin autosplit operations
                        bool scheduledOnManualScheduleOnlyResource = false;
                        Resource primaryResource = op.GetScheduledPrimaryResource();
                        if (primaryResource != null && primaryResource.ManualSchedulingOnly)
                        {
                            scheduledOnManualScheduleOnlyResource = true;
                        }
                        if (op.Split && op.AutoSplitInfo.AutoSplitType != OperationDefs.EAutoSplitType.None && op.AutoSplitInfo.CanAutoJoin(m_activeSimulationType, op, scheduledOnManualScheduleOnlyResource))
                        {
                            op.Unsplit(new ScenarioDataChanges(), true); //Don't need to track activity deletes here
                        }
                    }
                }

                if (a_resToLockEligibleLeadOpsTo != null)
                {
                    // It only configures the first MO. But the helper class could handle multiple MOs with an additional function to allow more MOs to be added to what it tracks.
                    lockEligibleLeadOperationsToResourceHelper = new LockEligibleLeadOperationsToResourceHelper(mo, a_resToLockEligibleLeadOpsTo, a_targetPathExternalId);
                }

                ManufacturingOrder.AlternatePathSatisfiability satisfiable = job.AdjustedPlantResourceEligibilitySets_IsSatisfiable(simType);
                if (!AllowJobToSchedule(ScenarioOptions.EUnsatisfiableJobPathHandlingEnum.OptimizeSatisfiablePaths, satisfiable))
                {
                    jobsWithIneligiblePaths.Add(job);
                }
            }

            if (allOpsFinishedOrOmitted)
            {
                throw new ExpediteValidationException("All Operations were finished or omitted, Expedite was not performed.".Localize());
            }

            if (jobsWithIneligiblePaths.Count > 0)
            {
                StringBuilder sb = new ();

                foreach (Job job in jobsWithIneligiblePaths)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.AppendFormat("'{0}'", job.ExternalId);
                    job.ScheduledStatus_set = JobDefs.scheduledStatuses.FailedToSchedule;
                }

                throw new ExpediteValidationException("2833", new object[] { sb.ToString() });
            }

            /*
             *
             * Configure the MOs for the Expedite and run the simulation.
             *
             * */
            for (int moI = 0; moI < a_expediteMOs.Count; moI++)
            {
                ManufacturingOrder mo = a_expediteMOs[moI];
                SetToBeScheduledFlags(mo.Job, mo);
                InternalActivity excludeAct = null;
                Dictionary<BaseId, List<BaseId>> excludedManualFilterDictionary = new();
                AlternatePath path = string.IsNullOrEmpty(a_targetPathExternalId) ? mo.CurrentPath : mo.AlternatePaths.FindByExternalId(a_targetPathExternalId);
                List<ResourceOperation> ops = path.GetOperationsByLevel(true);

                // Get the first operation and node of the target path, or current path
                excludeAct = ops[ops.Count - 1].Activities.GetByIndex(0);
                if (a_resToLockEligibleLeadOpsTo != null)
                {
                    excludedManualFilterDictionary.Add(excludeAct.Id, new List<BaseId>() { a_resToLockEligibleLeadOpsTo.Id });
                }
                else if (a_manuallyTriggered)
                {
                    //Only create the manual exclusion filter to allow the op to schedule on manual schedule only Resources if the Expedite was triggered manually
                    List<Resource> resources = excludeAct.ResReqsEligibilityNarrowedDuringSimulation.PrimaryEligibilitySet.GetResources();
                    List<BaseId> resourceIds = new List<BaseId>(resources.Count);
                    foreach (Resource res in resources)
                    {
                        resourceIds.Add(res.Id);
                    }

                    excludedManualFilterDictionary.Add(excludeAct.Id, resourceIds);
                }

                //Only create the manual exclusion filter to allow the op to schedule on manual schedule only Resources if the Expedite was triggered manually
                if (a_manuallyTriggered)
                {
                    foreach (ResourceOperation ro in ops)
                    {
                        if (ro.Id == excludeAct.Operation.Id)
                        {
                            continue;
                        }

                        InternalActivity act = ro.Activities.GetByIndex(0);
                        List<Resource> resources = act.ResReqsEligibilityNarrowedDuringSimulation.PrimaryEligibilitySet.GetResources();
                        List<BaseId> resourceIds = new List<BaseId>(resources.Count);
                        foreach (Resource res in resources)
                        {
                            resourceIds.Add(res.Id);
                        }

                        excludedManualFilterDictionary.Add(act.Id, resourceIds);
                    }
                }

                ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter excludeFromManualFilter;
                excludeFromManualFilter.ExcludedManualFiltersDictionary = excludedManualFilterDictionary;
                mo.AdjustedPlantResourceEligibilitySets_Filter(excludeFromManualFilter);

                mo.Unschedule(ManufacturingOrder.UnscheduleType.Normal, false);

                if (a_anchor)
                {
                    // The MO is unanchored, it will be reanchored after the expedite.
                    mo.Anchor(false, ScenarioOptions);
                }
                else
                {
                    if (mo.IsAnchored())
                    {
                        // The MO is anchored, it's anchored activities will be reanchored after the Expedite.
                        mo.ReanchorAfterBeingExpedited = true;
                        mo.ReanchorSetup();
                    }
                }

                mo.BeingExpedited = true;
                long releaseDateTemp;

                if (mo.EffectiveReleaseDate > a_defaultExpediteDate)
                {
                    releaseDateTemp = mo.EffectiveReleaseDate;
                }
                else
                {
                    releaseDateTemp = a_defaultExpediteDate;
                }

                SimulationTimePoint expSST;
                if (a_expediteStartAdjustment == ScenarioDetailExpediteBaseT.ExpediteStartDateType.EndOfFrozenSpan)
                {
                    expSST = new SimulationTimePoint(this, releaseDateTemp, OptimizeSettings.ETimePoints.EndOfFrozenZone);
                }
                else
                {
                    expSST = new SimulationTimePoint(releaseDateTemp);
                }

                AddManufacturingOrderReleaseEvent(Clock, mo, expSST, null);
            }

            // Unschedule everything on and after the expedite time and store those activities in
            // the non-expedite activities collection.			
            ResourceActivitySets nonExpediteActivities = new (availableResources);
            ResourceActivitySets sequentialResourceActivities = new (availableResources);
            UnscheduleInProcessActivitiesAndAddInProcessReleaseEvents(true);
            UnscheduleActivities(availableResources, new SimulationTimePoint(Clock), new SimulationTimePoint(EndOfPlanningHorizon), nonExpediteActivities, simType, Clock);

            // Perform a simulation.
            // The first step of the simulation will schedule the expedite activities, the second step will schedule the
            // non-expedite activities.
            ActivitiesCollection empty = new ();
            MoveDispatcherDefinition dispatcherDefinition = new (new BaseId(0));
            SimulationResourceInitialization(new SimulationResourceDispatcherUsageArgs(Clock, dispatcherDefinition));

            List<ResourceRequirement> clearDefaultResourceSet = new ();

            Simulate(Clock, nonExpediteActivities, simType, a_T);

            for (int moI = 0; moI < a_expediteMOs.Count; moI++)
            {
                ManufacturingOrder mo = a_expediteMOs[moI];
                Job job = mo.Job;
                if (!job.JobScheduledStatusUpdated)
                {
                    job.UpdateScheduledStatus();
                    job.JobScheduledStatusUpdated = true;
                }
            }

            // Reanchor all the MOs that were anchored prior to the expedite.
            for (int moI = 0; moI < a_expediteMOs.Count; moI++)
            {
                ManufacturingOrder mo = a_expediteMOs[moI];

                if (a_anchor)
                {
                    mo.Anchor(true, ScenarioOptions);
                }
                else
                {
                    // If the MO was anchored prior to the expedite, it will be reanchored.
                    if (mo.ReanchorAfterBeingExpedited)
                    {
                        mo.Reanchor(ScenarioOptions);
                    }
                }

                if (a_resourceLock)
                {
                    mo.Lock(true);
                }
            }

            StopTiming(ts, false);

            #if TEST
                TestSchedule("Expedite");
            #endif
            SimulationActionComplete();
            ExpediteComplete(a_T);
            CheckForRequiredAdditionalSimulation(a_dataChanges);
        }
        catch (SimulationValidationException e)
        {
            HandleExpediteValidationException(a_T, simType, e);
            throw;
        }
        catch (Exception e)
        {
            HandleExpediteException(a_T, simType);
            throw;
        }
        finally
        {
            if (lockEligibleLeadOperationsToResourceHelper != null)
            {
                lockEligibleLeadOperationsToResourceHelper.ClearOrRestoreDefaultPaths(m_productRuleManager);
            }
            #if TEST
                SimDebugCleanup();
            #endif
        }
    }

    /// <summary>
    /// This helper class is used during an Expdite to force eligible lead operations onto a resource.
    /// </summary>
    private class LockEligibleLeadOperationsToResourceHelper
    {
        private readonly InternalResource m_resToScheduleEligibleLeadOpsOn;

        internal LockEligibleLeadOperationsToResourceHelper(ManufacturingOrder a_mo, InternalResource a_resToLockEligibleLeadOpsTo, string a_specificPathExternalId)
        {
            m_resToScheduleEligibleLeadOpsOn = a_resToLockEligibleLeadOpsTo;

            // When the Resource to shedule lead activities on is specified, the Default resource is set to implement the functionality. 
            // After the simulation, the values in the Default resource are reset to what they were prior to being changed below.
            for (int pathI = 0; pathI < a_mo.AlternatePaths.Count; ++pathI)
            {
                AlternatePath path = a_mo.AlternatePaths[pathI];

                //Exclude all paths that are not the target path
                if (!string.IsNullOrEmpty(a_specificPathExternalId) && path.ExternalId != a_specificPathExternalId)
                {
                    if (path.ExternalId != a_specificPathExternalId)
                    {
                        path.ExcludedByExpediteBecauseItsLeadOpsArentEligibleOnSpecifiedRes = true;
                        continue;
                    }
                }

                // All the paths aren't always released, but it was easier to go through the all here instead of figuring out which ones would be released. 
                // If which paths will be released becomes available you can add code here to ignore the other paths.
                bool pathIsEligibleForResToScheduledEligibleLeadActivitiesOn = false;

                if (path.AdjustedPlantResourceEligibilitySets_IsSatisfiable())
                {
                    AlternatePath.NodeCollection effectiveLeaves = path.EffectiveLeaves;

                    // All leaves that have an eligible path are configured so their primary resource requirement is scheduled on the default resource.
                    for (int leafI = 0; leafI < effectiveLeaves.Count; ++leafI)
                    {
                        AlternatePath.Node node = effectiveLeaves[leafI];
                        InternalOperation op = node.Operation as InternalOperation;
                        PlantResourceEligibilitySet primaryResourcePRES = node.ResReqsEligibilityNarrowedDuringSimulation[op.ResourceRequirements.PrimaryResourceRequirementIndex];
                        if (primaryResourcePRES.Contains(m_resToScheduleEligibleLeadOpsOn))
                        {
                            pathIsEligibleForResToScheduledEligibleLeadActivitiesOn = true;
                            AddDefaultResourceSavedValues(op);
                            op.ResourceRequirements.PrimaryResourceRequirement.DefaultResource_Set(m_resToScheduleEligibleLeadOpsOn, false, 0);
                        }
                    }

                    // Paths that don't have activities eligible on the default resource are excluded from being released.
                    if (!pathIsEligibleForResToScheduledEligibleLeadActivitiesOn)
                    {
                        path.ExcludedByExpediteBecauseItsLeadOpsArentEligibleOnSpecifiedRes = true;
                    }
                }
            }
        }

        internal class DefaultResourceSavedValues : ResourceRequirement.DefaultResourceValues
        {
            internal DefaultResourceSavedValues(InternalOperation a_op)
                : base(a_op.ResourceRequirements.PrimaryResourceRequirement)
            {
                m_op = a_op;
            }

            internal readonly InternalOperation m_op;
        }

        private readonly List<DefaultResourceSavedValues> m_leadActivitiesToClearOrReset = new ();

        private int LeadActivitiesToClearOrResetCount => m_leadActivitiesToClearOrReset.Count;

        internal void AddDefaultResourceSavedValues(InternalOperation a_op)
        {
            m_leadActivitiesToClearOrReset.Add(new DefaultResourceSavedValues(a_op));
        }

        private DefaultResourceSavedValues GetDefaultResourceSavedValues(int a_idx)
        {
            return m_leadActivitiesToClearOrReset[a_idx];
        }

        internal void ClearOrRestoreDefaultPaths(ProductRuleManager a_productRuleManager)
        {
            List<Job> recomputeEligibility = new ();
            for (int leadActI = 0; leadActI < LeadActivitiesToClearOrResetCount; ++leadActI)
            {
                DefaultResourceSavedValues defaultResourceSavedValues = GetDefaultResourceSavedValues(leadActI);
                ResourceRequirement primaryRR = defaultResourceSavedValues.m_op.ResourceRequirements.PrimaryResourceRequirement;
                if (defaultResourceSavedValues.m_op.Scheduled)
                {
                    // If the operation was scheduled on a different resource than what the default resource initially was, then clear the default resource. Otherwise restore the default resource's other settings.
                    if (defaultResourceSavedValues.m_defaultResource != null && defaultResourceSavedValues.m_defaultResource != m_resToScheduleEligibleLeadOpsOn)
                    {
                        primaryRR.DefaultResource_Clear();
                        recomputeEligibility.Add(defaultResourceSavedValues.m_op.Job);
                    }
                    else
                    {
                        primaryRR.DefaultResource_Set(defaultResourceSavedValues);
                    }
                }
                else
                {
                    // Situtions in which this might occur include the path not being scheduled, the operation being Finished, etc.
                    // Restore the default resource settings.
                    primaryRR.DefaultResource_Set(defaultResourceSavedValues);
                }
            }

            foreach (Job job in recomputeEligibility)
            {
                job.ComputeEligibilityAndUnscheduleIfIneligible(a_productRuleManager);
            }
        }

        public override string ToString()
        {
            return string.Format("lockedToRes=\"{0}\"", m_resToScheduleEligibleLeadOpsOn.Name);
        }
    }

    private long GetAndValidateExpediteStart(ScenarioDetailExpediteBaseT a_t)
    {
        switch (a_t.StartDateType)
        {
            case ScenarioDetailExpediteBaseT.ExpediteStartDateType.Clock:
                return Clock;
            case ScenarioDetailExpediteBaseT.ExpediteStartDateType.EndOfFrozenSpan:
                return PlantManager.GetEarliestFrozenSpanEnd(Clock);

            case ScenarioDetailExpediteBaseT.ExpediteStartDateType.SpecificDateTime:
                ExpediteDateErrorCheck(a_t.Date);
                return a_t.Date;
            case ScenarioDetailExpediteBaseT.ExpediteStartDateType.EndOfStableSpan:
                return PlantManager.GetEarliestStableSpanEnd(Clock);
            default:
                throw new CommonException("New ExpediteStartDateType enum not handled!");
        }
    }

    private void ExpediteDateErrorCheck(long a_expediteDate)
    {
        try
        {
            DateErrorCheck(a_expediteDate);
        }
        catch (DateErrorCheckException)
        {
            throw new ExpediteValidationException("2530");
        }
    }

    private void ExpediteProcessPathReleaseHandler(long a_time, ManufacturingOrder a_mo, AlternatePath a_path)
    {
        ActivitiesCollection rescheduleActivities = new ();
        AddOutterUnscheduledActivities(a_mo, a_path, rescheduleActivities);
        SetupMaterialConstraints(rescheduleActivities, a_time);
        InitExpediteActivities(a_time, rescheduleActivities);
    }

    private void InitExpediteActivities(long a_startTime, ActivitiesCollection a_rescheduleActivities)
    {
        for (int rescheduleActivitiesI = 0; rescheduleActivitiesI < a_rescheduleActivities.Count; ++rescheduleActivitiesI)
        {
            InternalActivity activity = a_rescheduleActivities[rescheduleActivitiesI];
            InternalOperation op = activity.Operation;
            op.SetLatestConstraintToMOReleaseDate(Clock);
            OperationReadyEvent operationReadyEvent = new (a_startTime, op, InternalOperation.LatestConstraintEnum.AlternatePath);
            AddEvent(operationReadyEvent);
        }
    }
}