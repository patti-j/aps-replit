using System.Text;

using PT.APSCommon;
using PT.APSCommon.Exceptions;
using PT.Common.Localization;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Events;
using PT.SchedulerDefinitions;
using PT.Transmissions;
using static PT.ERPTransmissions.JobT;

namespace PT.Scheduler;

partial class ScenarioDetail
{
    /// <summary>
    /// This value is only valid when an optimization is taking place.
    /// It is set to either the scenario's optimize settings or the activeOptimizeSettings's optimize settings.
    /// </summary>
    private ScenarioDetailOptimizeT m_activeOptimizeSettingsT;

    private OptimizeSettings m_activeOptimizeSettings;

    /// <summary>
    /// whether the Optimize is being performed for MRP processing.
    /// </summary>
    public bool OptimizeSettingsRunningMRP
    {
        get
        {
            if (m_activeOptimizeSettings == null)
            {
                return false;
            }

            return m_activeOptimizeSettings.RunMrpDuringOptimizations;
        }
    }

    /// <summary>
    /// Set at the start of an Optimize Simulation.
    /// This is the default optimize start time for resources that don't have define their own optimize start time.
    /// As of Backlog 2737 it's possible for resources to have different optimize start times if departmental frozen spans are used.
    /// </summary>
    private long m_optimizeDefaultStartTicks;

    private SimulationTimePoint m_simStartTime;

    internal bool m_rightCompression = true;

    /// <summary>
    /// During Simulation(), this value is set to true if the need dates of sub-jobs are updated.
    /// An attempt to update sub-job need dates is only made when there are multiple stages
    /// and ScenarioOptions.SetSubJobNeedDates==true.
    /// </summary>
    private bool m_updatedSubJobNeedDates;

    private void Optimize(ScenarioDetailOptimizeT a_optimizeT, IScenarioDataChanges a_dataChanges)
    {
        SimulationType simType = SimulationType.Optimize;
        SimulationProgress.FireSimulationProgressEvent(this, simType, a_optimizeT, SimulationProgress.Status.Initializing, m_nbrOfSimulationsSinceStartup);
        Common.Testing.Timing ts = CreateTiming("Optimize");
        #if TEST
            PT.Common.Testing.TimingSet timingSet = new PT.Common.Testing.TimingSet(true);
        #endif

        #if TEST
            //for (int i = 0; i < 101; ++i) // Timing of the first optimize is skipped.
        #endif
        {
            #if TEST
                timingSet.Start();
            #endif
            try
            {
                #if TEST
                    SimDebugSetup();
                #endif

                try
                {
                    m_optimizeDefaultStartTicks = GetScheduleDefaultStartTime(m_activeOptimizeSettings).DateTimeTicks;
                }
                catch (DateErrorCheckException)
                {
                    throw new SimulationValidationException("2492");
                }

                //ActivitiesCollection rescheduleActivities = new ActivitiesCollection();
                MainResourceSet availableResources = new ();

                // Add all the active resources to the available resource set.
                for (int plantI = 0; plantI < m_plantManager.Count; ++plantI)
                {
                    Plant plant = m_plantManager[plantI];

                    for (int departmentI = 0; departmentI < plant.Departments.Count; ++departmentI)
                    {
                        Department department = plant.Departments[departmentI];

                        for (int resourceI = 0; resourceI < department.Resources.Count; ++resourceI)
                        {
                            Resource machine = department.Resources[resourceI];
                            if (machine.Active)
                            {
                                availableResources.Add(machine);
                            }
                        }
                    }
                }

                // If a the optimize is for a single plant, this value is set, otherwise it's null.
                Plant plantBeingOptimized = null;
                if (m_activeOptimizeSettings.ResourceScope == OptimizeSettings.resourceScopes.OnePlant)
                {
                    plantBeingOptimized = m_plantManager.GetById(m_activeOptimizeSettings.PlantToInclude);

                    if (plantBeingOptimized == null)
                    {
                        throw new SimulationValidationException("2493", new object[] { m_activeOptimizeSettings.PlantToInclude });
                    }
                }

                // This value is initialized to 1 but may be set to 2 if there are multiple stages and sub-components need dates need to be updated.
                // In the case of 2 simulations, the first creates an initial schedule and updates the need dates of the sub-components based on teh
                long nbrOfSimulationsToPerform = 1;
                long nbrOfSimulationsPerformed = 0;

                // The set of jobs that failed to schedule during the first attempt to optimize within this function call.
                // If any jobs fail to schedule, a better schedule will be created by excluding these jobs from a second optimize.
                // This is necessary because failed to schedule jobs may have been partially scheduled and left gaps behind in the
                // schedule when their blocks are unscheduled when the simulation is complete. A better schedule will use these
                // gaps.
                HashSet<Job> sim1FailedToScheduleJobs = new ();

                // Perform 1, 2, or 3 optimizes.
                // The first is the normal optimize.
                // A second and or third optimize may be performed to produce a better schedule if jobs fail to schedule or sub-component availability dates
                // change.
                do
                {
                    RecalculateJITStartTimes(a_dataChanges); //Take into account any changes made from the previous schedule

                    Dictionary<ActivityKey, bool> alteredJobActivities = new Dictionary<ActivityKey, bool>();
                    if (a_optimizeT.SelectedJobIdList?.Count > 0)
                    {
                        FreezeJobs(a_optimizeT.SelectedJobIdList, out alteredJobActivities);
                    }

                    SimulationInitialization1();
                    m_activeSimulationType = SimulationType.Optimize;

                    SimulationInitialization2(availableResources, m_activeOptimizeSettingsT, simType, OptimizeProcessPathReleaseHandler);

                    // This is used to limit the number of jobs that can be scheduled with a trial/demo key.
                    long nbrOfMOsToSchedule = 0;
                    
                    m_simStartTime = new SimulationTimePoint(this, m_optimizeDefaultStartTicks, m_activeOptimizeSettings.StartTime);
                    // Determine which manufacturing orders are not schedulable.
                    // If an MO is not schedulable neither are its predecessors.
                    // Initialize the state of all the jobs that will attempt to be scheduled to FailedToSchedule. Their state will be set to Scheduled once they have successfully scheduled.
                    for (int jobManagerI = 0; jobManagerI < m_jobManager.Count; jobManagerI++)
                    {
                        Job job = m_jobManager[jobManagerI];
                        job.FailedToScheduleReason = "";
                        JobDefs.ExcludedReasons excludedReasons = job.ExcludedReasons;
                        job.ExcluedReasons_SetNotExcluded();

                        if (!job.Cancelled && !job.Finished)
                        {
                            bool trialDemoLimitReached = PTSystem.LicenseKey.TrialVersion && nbrOfMOsToSchedule >= c_trialDemoMaxMOsAllowedToSchedule;

                            if (job.Template)
                            {
                                if (job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
                                {
                                    job.Unschedule(false);
                                }

                                job.ScheduledStatus_set = JobDefs.scheduledStatuses.Template;
                            }
                            else if (job.DoNotSchedule || ((job.ScheduledStatus == JobDefs.scheduledStatuses.New || excludedReasons.HasFlag(JobDefs.ExcludedReasons.ExcludedNew)) && m_activeOptimizeSettings.ExcludeNewJobs) || ((job.ScheduledStatus == JobDefs.scheduledStatuses.Unscheduled || excludedReasons.HasFlag(JobDefs.ExcludedReasons.ExcludedUnscheduled)) && m_activeOptimizeSettings.ExcludeUnscheduledJobs) || (job.Commitment == JobDefs.commitmentTypes.Planned && m_activeOptimizeSettings.ExcludePlannedJobs) || (job.Commitment == JobDefs.commitmentTypes.Estimate && m_activeOptimizeSettings.ExcludeEstimateJobs) || (job.OnHold == holdTypes.OnHold && m_activeOptimizeSettings.ExcludeOnHoldJobs) || trialDemoLimitReached)
                            {
                                if (job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
                                {
                                    job.Unschedule(false);
                                }

                                if (job.DoNotSchedule)
                                {
                                    job.ExcludeReasons_AddReason(JobDefs.ExcludedReasons.ExcludedDoNotSchedule);
                                }

                                if ((job.ScheduledStatus == JobDefs.scheduledStatuses.New || excludedReasons.HasFlag(JobDefs.ExcludedReasons.ExcludedNew)) && m_activeOptimizeSettings.ExcludeNewJobs)
                                {
                                    job.ExcludeReasons_AddReason(JobDefs.ExcludedReasons.ExcludedNew);
                                }

                                if ((job.ScheduledStatus == JobDefs.scheduledStatuses.Unscheduled || excludedReasons.HasFlag(JobDefs.ExcludedReasons.ExcludedUnscheduled)) && m_activeOptimizeSettings.ExcludeUnscheduledJobs)
                                {
                                    job.ExcludeReasons_AddReason(JobDefs.ExcludedReasons.ExcludedUnscheduled);
                                }

                                if (job.Commitment == JobDefs.commitmentTypes.Planned && m_activeOptimizeSettings.ExcludePlannedJobs)
                                {
                                    job.ExcludeReasons_AddReason(JobDefs.ExcludedReasons.ExcludedPlanned);
                                }

                                if (job.Commitment == JobDefs.commitmentTypes.Estimate && m_activeOptimizeSettings.ExcludeEstimateJobs)
                                {
                                    job.ExcludeReasons_AddReason(JobDefs.ExcludedReasons.ExcludedEstimate);
                                }

                                if (job.OnHold == holdTypes.OnHold && m_activeOptimizeSettings.ExcludeOnHoldJobs)
                                {
                                    job.ExcludeReasons_AddReason(JobDefs.ExcludedReasons.ExcludedOnHold);
                                }

                                if (trialDemoLimitReached)
                                {
                                    job.ExcludeReasons_AddReason(JobDefs.ExcludedReasons.ExcludedMaxTrialDemoLimit);
                                }

                                #if DEBUG
                                // Not  a critical bug.
                                if (job.ExcludedReasons == JobDefs.ExcludedReasons.NotExcluded)
                                {
                                    throw new Exception("The exclude reason wasn't set. This could happen if a new exclude reason was added but the reason wasn't added to this if-else if block of code.");
                                }
                                #endif

                                job.ScheduledStatus_set = JobDefs.scheduledStatuses.Excluded; //needs to be after the New status is checked above.
                            }
                            else
                            {
                                if (plantBeingOptimized != null)
                                {
                                    // Allow this job to schedule in the plant being optimized by filtering its eligibility down to the optimize plant.

                                    if (job.ScheduledStatus != JobDefs.scheduledStatuses.Scheduled)
                                    {
                                        // Allow this job to schedule in the plant being optimized by filtering its eligibility down to the optimize plant.
                                        if (AllowJobToSchedule(ScenarioOptions.UnsatisfiableJobPathHandling, job.AdjustedPlantResourceEligibilitySets_IsSatisfiable(simType)))
                                        {
                                            job.AdjustedPlantResourceEligibilitySets_FilterDownToSpecificPlant(plantBeingOptimized.Id);
                                            if (!AllowJobToSchedule(ScenarioOptions.UnsatisfiableJobPathHandling, job.AdjustedPlantResourceEligibilitySets_IsSatisfiable(simType)))
                                            {
                                                job.DoNotSchedule_SetPlantNotIncludedInOptimize();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // The job is already scheduled, leave it in place.
                                        nbrOfMOsToSchedule = InitJobForOptimize(job, nbrOfMOsToSchedule, simType);
                                        continue;
                                    }
                                }

                                // This filtering limits activities scheduled on manual scheduling only resources to stay put.
                                nbrOfMOsToSchedule = InitJobForOptimize(job, nbrOfMOsToSchedule, simType);
                            }
                        }
                    }

                    AddMOReleaseEventArgsForOpt fxArgs = new (m_activeOptimizeSettings);

                    //HashSet<BaseId> moIdsBeforeSimStart = null;
                    //if (m_activeOptimizeSettings.MOBatchingByBatchGroupEnabled)
                    //{
                    //    List<Resource> resList = PlantManager.GetResourceList();
                    //    SimulationTimePoint sst = new (this, m_optimizeDefaultStartTicks, m_activeOptimizeSettings.StartTime);
                    //    moIdsBeforeSimStart = CreateMOsBeforeSimStartHashSet(sst, resList);
                    //    CreateExistingMOBSByGroup(sst, fxArgs, resList);
                    //}

                    ResourceActivitySets nonExpediteActivities = new (availableResources);

                    // A list of all the MOs that are taking part in the optimization.
                    LinkedList<ManufacturingOrder> mosToOptimize = new ();
                    List<Job> jobsToOptimize = new ();

                    // Add all the unscheduled activities to the reschedule activities list.
                    for (int jobManagerI = 0; jobManagerI < m_jobManager.Count; jobManagerI++)
                    {
                        bool scheduleJob = false;
                        Job job = m_jobManager[jobManagerI];
                        ManufacturingOrderManager moCollection = job.ManufacturingOrders;
                        if (!job.Cancelled && !job.Finished && job.ScheduledStatus != JobDefs.scheduledStatuses.Excluded && !(job.ScheduledStatus == JobDefs.scheduledStatuses.Unscheduled && m_activeOptimizeSettings.ExcludeUnscheduledJobs) && job.ScheduledStatus != JobDefs.scheduledStatuses.Template && !job.DoNotSchedule_PlantNotIncludedInOptimize && job.IsSchedulable())
                        {
                            // Verify that all non-finished activities are schedulable.
                            bool foundMOThatsNotSchedulable = false;
                            for (int moI = 0; moI < moCollection.Count; moI++)
                            {
                                ManufacturingOrder mo = moCollection[moI];

                                if (!mo.Finished && !mo.Schedulable)
                                {
                                    foundMOThatsNotSchedulable = true;
                                    break;
                                }
                            }

                            // Mark non-finished schedulable MOs as ToBeScheduled and add them to  mosToOptimize.
                            if (!foundMOThatsNotSchedulable)
                            {
                                for (int moI = 0; moI < moCollection.Count; moI++)
                                {
                                    ManufacturingOrder mo = moCollection[moI];

                                    if (!mo.Finished && !mo.IsFinishedOrOmitted)
                                    {
                                        if (!sim1FailedToScheduleJobs.Contains(job) // Jobs that failed to schedule during the first simulation are excluded from the next simulation
                                            &&
                                            mo.Schedulable)
                                        {
                                            scheduleJob = true;

                                            job.ToBeScheduled = true;
                                            mo.ToBeScheduled = true;

                                            mosToOptimize.AddFirst(mo);
                                        }
                                    }
                                }
                            }

                            if (scheduleJob)
                            {
                                jobsToOptimize.Add(job);
                            }
                        }
                    }

                    //This must be done before any events are added because resource staging is initialized here.
                    SimulationInitialization3(availableResources, jobsToOptimize, simType);
                    SimulationInitialization4_configUnschedAnchoredForAnchor(simType);

                    UnscheduleInProcessActivitiesAndAddInProcessReleaseEvents(false);

                    if (plantBeingOptimized != null)
                    {
                        // Break up available resources into 2 sets optimize plant and other plants.
                        MainResourceSet optimizeResources = new ();
                        MainResourceSet otherResources = new ();
                        for (int i = 0; i < availableResources.Count; ++i)
                        {
                            Resource res = (Resource)availableResources[i];
                            if (res.Plant == plantBeingOptimized)
                            {
                                optimizeResources.Add(res);
                                ResourceBlockList.Node curBlkNode = res.Blocks.First;
                                while (curBlkNode != null)
                                {
                                    Batch batch = curBlkNode.Data.Batch;
                                    // Nodes already scheduled in the optimize plant are locked to it, otherwise they could end up being scheudled in a different plant
                                    // when the optimize is performed.
                                    // Note: there's no support for operations whose activities are split across multiple plants.
                                    IEnumerator<InternalActivity> actEtr = batch.GetEnumerator();
                                    while (actEtr.MoveNext())
                                    {
                                        InternalActivity act = actEtr.Current;
                                        act.Operation.AlternatePathNode.AdjustedPlantResourceEligibilitySets_FilterDownToSpecificPlant(plantBeingOptimized.Id, ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter.NoFilter());
                                    }

                                    curBlkNode = curBlkNode.Next;
                                }
                            }
                            else
                            {
                                otherResources.Add(res);
                            }
                        }

                        // The unscheduled operations will be configured to remain on the same resources,starting at the same times, but still
                        // might be effected by changes in the plant being optimized.
                        UnscheduleActivities(otherResources, m_simStartTime, new SimulationTimePoint(EndOfPlanningHorizon), nonExpediteActivities, SimulationType.ConstraintsChangeAdjustment, 0);
                        // The blocks scheduled in the optimize plant are unscheduled as usual per optimize.
                        UnscheduleActivities(optimizeResources, m_simStartTime, new SimulationTimePoint(EndOfPlanningHorizon), nonExpediteActivities, SimulationType.Optimize, 0);
                    }
                    else
                    {
                        UnscheduleActivities(availableResources, m_simStartTime, new SimulationTimePoint(EndOfPlanningHorizon), nonExpediteActivities, SimulationType.Optimize, 0);
                    }

                    //if (m_activeOptimizeSettings.MOBatchingByBatchGroupEnabled)
                    //{
                    //    CreateMOBSByGroup(mosToOptimize, fxArgs, moIdsBeforeSimStart);

                        //This was removed in V12.
                        //if (CustomizationInstances.ManufacturingOrderBatchingCompleteCustomization != null)
                        //{
                        //    CustomizationInstances.ManufacturingOrderBatchingCompleteCustomization.BatchingCompleteExecute(this);
                        //}
                    //}

                    LinkedListNode<ManufacturingOrder> moNode = mosToOptimize.First;
                    SimulationTimePoint addMORelSST = new (Clock);
                    while (moNode != null)
                    {
                        ManufacturingOrder mo = moNode.Value;

                        AddManufacturingOrderReleaseEvent(Clock, mo, addMORelSST, fxArgs);
                        AddManufacturingOrderHoldReleasedEvent(mo, Clock);

                        moNode = moNode.Next;
                    }

                    AddJobHoldReleasedEvents(jobsToOptimize);

                    {
                        SimulationResourceDispatcherUsageArgs simulationResourceInitializationArgs;

                        if (m_activeOptimizeSettings.DispatcherSource == OptimizeSettings.dispatcherSources.OneRule)
                        {
                            DispatcherDefinition dispatcherDefinition = DispatcherDefinitionManager.GetById(m_activeOptimizeSettings.GlobalDispatcherId);
                            simulationResourceInitializationArgs = new SimulationResourceDispatcherUsageArgs(Clock, dispatcherDefinition);
                        }
                        else
                        {
                            simulationResourceInitializationArgs = new SimulationResourceDispatcherUsageArgs(Clock, m_activeOptimizeSettings.DispatcherSource);
                        }

                        SimulationResourceInitialization(simulationResourceInitializationArgs);
                    }

                    #if DEBUG
                    // Partially scheduled (failed to schedule) jobs are often the source of this problem. 
                    // During the first optimize suppress these errors. The should be resolved by the cleanup
                    // optimize.
                    z_supressActivitiesOnDispatcherException = nbrOfSimulationsPerformed == 0;
                    #endif
                    Simulate(Clock, nonExpediteActivities, simType, m_activeOptimizeSettingsT);
                    ++nbrOfSimulationsPerformed;

                    // Update the set of failed to schedule jobs.
                    // I presume jobs only fail to schedule during the first simulation since the failed to schedule jobs are
                    // excluded from subsequent simulations.
                    // The purpose of the 2nd optimize after jobs fail to scheule is to utilize capacity that may have been
                    // consumed by failed to schedule jobs that partially scheduled; a few of the failed to schedule job's operations
                    // may have scheduled. In the 2nd optimize, these failed to schedule jobs will be excluded freeing capacity consumed by their operations.
                    if (nbrOfSimulationsPerformed == 1)
                    {
                        for (int jobI = 0; jobI < JobManager.Count; ++jobI)
                        {
                            Job job = JobManager[jobI];
                            if (job.ToBeScheduled && job.ScheduledStatus == JobDefs.scheduledStatuses.FailedToSchedule)
                            {
                                sim1FailedToScheduleJobs.Add(job);
                            }
                        }
                    }

                    //--------------------------------------------------
                    // If necessary, cause additional optimizes to occur
                    //--------------------------------------------------

                    // Reoptimize if jobs fail to schedule. They should only fail during the first optimization since the failed
                    // to schedule jobs are excluded from subsequent optimizations.
                    bool nbrOfSimsIncreasedTo2ByFailedJobs = false;
                    if (sim1FailedToScheduleJobs.Count > 0)
                    {
                        if (nbrOfSimulationsPerformed == 1) // Jobs should only fail in the initial simulation. 
                        {
                            nbrOfSimsIncreasedTo2ByFailedJobs = true;
                            // Perform a second simulation to try to use any gaps created by the jobs
                            // that failed to schedule.
                            nbrOfSimulationsToPerform = 2;
                        }
                    }

                    UnFreezeJobs(alteredJobActivities);

                    if (m_updatedSubJobNeedDates)
                    {
                        if (nbrOfSimsIncreasedTo2ByFailedJobs)
                        {
                            // The second simulation to be performed due to failed to schedule jobs will probably change the schedule, a third simulation 
                            // needs to be performed for changes to sub job need dates.
                            nbrOfSimulationsToPerform = 3;
                        }
                        else
                        {
                            // A 2nd simulation needs to be performed for sub job need dates.
                            nbrOfSimulationsToPerform = 2;
                        }
                    }
                } while (nbrOfSimulationsPerformed != nbrOfSimulationsToPerform);

                StopTiming(ts, false);

                #if TEST
                    TestSchedule("Optimize");
                #endif
                SimulationActionComplete();
                if (!a_optimizeT.SuppressEvents)
                {
                    m_simulationProgress.PostSimulationWorkComplete();
                }

                CheckForRequiredAdditionalSimulation(a_dataChanges);

                if (m_activeOptimizeSettings.AutoJoinMOs)
                {
                    if (m_extensionController.RunAutoJoinExtension)
                    {
                        m_extensionController.BeforeAutoJoinProcessing(this);
                    }

                    AutoJoinManufacturingOrders(a_optimizeT, a_dataChanges);
                }
            }
            catch (PTSimCancelledException)
            {
                throw;
            }
            catch (SimulationValidationException e)
            {
                SimulationProgress.FireSimulationProgressEvent(this, simType, a_optimizeT, SimulationProgress.Status.Terminated, m_nbrOfSimulationsSinceStartup);
                FireSimulationValidationFailureEvent(e, m_activeOptimizeSettingsT);
                throw;
            }
            catch (Exception)
            {
                SimulationProgress.FireSimulationProgressEvent(this, simType, a_optimizeT, SimulationProgress.Status.Exception, m_nbrOfSimulationsSinceStartup);
                throw;
            }

            #if TEST
                timingSet.Stop();
            #endif
        }
    }
    /// <summary>
    /// Locks and Anchors specified jobs to remain same during the optimize process.
    /// </summary>
    /// <remarks>Also, temporary flags all unscheduled jobs as Do Not Schedule so, they remain unscheduled during the optimize process</remarks>>
    private void FreezeJobs(BaseIdList a_selectedJobs, out Dictionary<ActivityKey, bool> o_alteredJobActivities)
    {
        o_alteredJobActivities = new Dictionary<ActivityKey, bool>();
        foreach (Job job in JobManager.JobEnumerator)
        {
            if (a_selectedJobs.Contains(job.Id))
            {
                continue;
            }

            //if they are unscheduled because they are already marked as Do not schedule, do flag nor add them to the collection of jobs
            //to be temporarily flagged as DoNot schedule
            if (job.DoNotSchedule)
            {
                continue;
            }

            foreach (InternalActivity internalActivity in job.GetActivities())
            {
                // flag all unscheduled jobs as do not schedule so, they aren't accidentally scheduled as part of the filtered optimize
                if (!job.Scheduled)
                {
                    o_alteredJobActivities.Add(new ActivityKey(job.Id, internalActivity.Operation.ManufacturingOrder.Id, internalActivity.Operation.Id, internalActivity.Id), internalActivity.Anchored);
                    job.DoNotSchedule = true;
                    break;
                }


                if (!internalActivity.Finished)
                {
                    if (internalActivity.Scheduled)
                    {
                        o_alteredJobActivities.Add(new ActivityKey(job.Id, internalActivity.Operation.ManufacturingOrder.Id, internalActivity.Operation.Id, internalActivity.Id), internalActivity.Anchored);
                        internalActivity.TempLock();

                        if (!internalActivity.Anchored)
                        {
                            internalActivity.Operation.TempAnchor();
                            internalActivity.SetAnchor(true);
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// Reverts temporarily locked and Anchored activities to their original states before scheduling
    /// </summary>
    /// <param name="a_alteredActivities"></param>
    private void UnFreezeJobs(Dictionary<ActivityKey, bool> a_alteredActivities)
    {
        foreach ((ActivityKey activityKey, bool anchor) in a_alteredActivities)
        {
            Job job = JobManager.GetById(activityKey.JobId);

            //If job is flag as do not schedule in this collection it means they were previously unscheduled and need to marked as such
            if (job.DoNotSchedule)
            {
                job.DoNotSchedule = false;
                job.ScheduledStatus_set = JobDefs.scheduledStatuses.Unscheduled;
            }
            else
            {
                InternalOperation internalOperation = job.FindOperation(activityKey.MOId, activityKey.OperationId);

                InternalActivity internalActivity = internalOperation.Activities[activityKey.ActivityId];

                internalActivity.TempLockClear();
                internalActivity.SetAnchor(anchor);
                internalOperation.TempAnchorClear();
            }
        }
    }
    /// <summary>
    /// Find lot unused quantities as QtyProfiles.
    /// </summary>
    private void CalcQtyAvailProfiles()
    {
        //TODO: Storage
        //for (int i = 0; i < WarehouseManager.Count; ++i)
        //{
        //    Warehouse wh = WarehouseManager[i];
        //    for (int iI = 0; iI < wh.Inventories.Count; ++iI)
        //    {
        //        Inventory inv = wh.Inventories.GetByIndex(iI);
        //        PT.Scheduler.Simulation.Overlap.QtyProfileLotAvail p = inv.CalcQtyProfileLotAvail();
        //    }
        //}
    }

    /// <summary>
    /// A helper function of Optimize.
    /// Initialize a job to be optimized.
    /// </summary>
    /// <param name="a_job">The job to initialize</param>
    /// <param name="a_nbrOfMOsToSchedule">The total number of MOs to schedule up to this job's initialization processing.</param>
    /// <param name="a_simType"></param>
    /// <returns>The total number of MOs to schedule based on all calls to this function within an optimize.</returns>
    private long InitJobForOptimize(Job a_job, long a_nbrOfMOsToSchedule, SimulationType a_simType)
    {
        //If MOs were resized, set back to original Qty.
        foreach (ManufacturingOrder mo in a_job.ManufacturingOrders)
        {
            if (mo.Resized)
            {
                mo.AdjustToOriginalQty(null, this.ProductRuleManager);
            }
        }

        if (m_scenarioOptions.RestoreMaterialConstraintsOnOptimizedActivities)
        {
            //Loop through material requirements and reset the constraint type on any 'Ignore' constraint types.
            foreach (InternalOperation op in a_job.GetOperations())
            {
                if (op.GetEarliestScheduledActivityStartDate(out InternalActivity act) == PTDateTime.MaxDateTimeTicks)
                {
                    continue;
                }

                //Run this logic for ops/acts that start after the optimize start time, calculated above.
                if (act.Batch == null || op.StartDateTime.Ticks < m_simStartTime.GetTimeForResource(act.Batch.PrimaryResource))
                {
                    continue;
                }

                foreach (MaterialRequirement mr in op.MaterialRequirements)
                {
                    if (mr.ConstraintType == MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByAvailableDate)
                    {
                        mr.ConstraintType = MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate;
                    }
                    else if (mr.ConstraintType == MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByEarlierOfLeadTimeOrAvailableDate)
                    {
                        mr.ConstraintType = MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate;
                    }
                }
            }
        }

        //Rejoin autosplit operations
        ScenarioDataChanges activityChanges = new ScenarioDataChanges();
        foreach (BaseOperation operation in a_job.GetOperations())
        {
            if (operation is ResourceOperation op)
            {
                //Don't unsplit the ops that are at least partially scheduled before the Optimize Start as they don't get Unscheduled by the Scheduler
                if (op.Scheduled)
                {
                    long scheduledStart = op.ScheduledStartDate.Ticks;
                    if (scheduledStart < m_simStartTime.GetTimeForResource(op.GetScheduledPrimaryResource()))
                    {
                        op.AutoSplitInfo.SetInFrozenSpan();
                        continue;
                    }
                }

                bool scheduledOnManualScheduleOnlyResource = false;
                Resource primaryResource = op.GetScheduledPrimaryResource();
                if (primaryResource != null && primaryResource.ManualSchedulingOnly)
                {
                    scheduledOnManualScheduleOnlyResource = true;
                }
                if (op.Split && op.AutoSplitInfo.AutoSplitType != OperationDefs.EAutoSplitType.None && op.AutoSplitInfo.CanAutoJoin(SimulationType.Optimize, op, scheduledOnManualScheduleOnlyResource))
                {
                    op.Unsplit(activityChanges, true);
                }
            }
        }

        a_job.AdjustedPlantResourceEligibilitySets_Filter(ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter.NoFilter());
        ManufacturingOrder.AlternatePathSatisfiability satisfiability = a_job.AdjustedPlantResourceEligibilitySets_IsSatisfiable(a_simType);
        if (!AllowJobToSchedule(ScenarioOptions.UnsatisfiableJobPathHandling, satisfiability))
        {
            a_job.DoNotSchedule_SetNotSatisfiable();
        }

        if (!a_job.DoNotSchedule_PlantNotIncludedInOptimize)
        {
            ManufacturingOrderManager moCollection = a_job.ManufacturingOrders;

            for (int moI = 0; moI < moCollection.Count; moI++)
            {
                ManufacturingOrder mo = moCollection[moI];
                if (mo.Schedulable && !mo.Finished)
                {
                    if (a_job.DoNotSchedule_NotSatisfiable && !mo.EligibleResources_IsSatisfiable())
                    {
                        // Sets successor MOs to be not schedulable.
                        mo.Schedulable = false;
                        // Sets the failed to schedule MO reason
                        a_job.SetFailedToScheduleToNotSatisfiable(mo);
                    }
                    else
                    {
                        ++a_nbrOfMOsToSchedule;
                    }
                }
            }

            // All jobs are initially set to FailedToSchedule. Their status is changed to Scheduled after the simulation. 
            a_job.ScheduledStatus_set = JobDefs.scheduledStatuses.FailedToSchedule;
        }

        return a_nbrOfMOsToSchedule;
    }

    /// <summary>
    /// After a simulation you can call this function to unschedule any jobs that failed to schedule and mark them as failedToSchedule.
    /// </summary>
    /// <param name="a_schedulingJobs">The jobs that should have scheduled.</param>
    /// <returns>The number of jobs that failed to schedule. This should be 0 if everything is working properly.</returns>
    private long SetScheduledStatusAndUnschedPartiallySchedJobs()
    {
        long failedToScheduleCount = 0;

        for (int jobI = 0; jobI < JobManager.Count; ++jobI)
        {
            Job j = JobManager[jobI];

            if (j.Scheduled)
            {
                j.ScheduledStatus_set = JobDefs.scheduledStatuses.Scheduled;
                j.ExcluedReasons_SetNotExcluded();
            }
            else if (j.ToBeScheduled || j.SplitDuringOptimizeAndToBeScheduled) // jobs that are split during optimize are part of the jobs to be optimized.
            {
                j.Unschedule();
                j.ScheduledStatus_set = JobDefs.scheduledStatuses.FailedToSchedule;
                j.ExcluedReasons_SetNotExcluded();
                if (m_attemptToScheduleToNonConnectedRes.Contains(j))
                {
                    j.FailedToScheduleReason = Localizer.GetString("4444");
                }

                ++failedToScheduleCount;
            }
        }

        Batches.RemoveDeadBatches();

        return failedToScheduleCount;
    }

    /// <summary>
    /// whether to try to schedule a job based on the satisfiability of its paths.
    /// </summary>
    /// <param name="a_handling">Whether to exclude jobs with unsatisfiable paths or allow jobs that have some satisfiable paths to schedule.</param>
    /// <param name="a_satisfiability">The state of the job's paths' satisfiability.</param>
    /// <returns>true if the job should be scheduled; false otherwise.</returns>
    private bool AllowJobToSchedule(ScenarioOptions.EUnsatisfiableJobPathHandlingEnum a_handling, ManufacturingOrder.AlternatePathSatisfiability a_satisfiability)
    {
        if (a_handling == ScenarioOptions.EUnsatisfiableJobPathHandlingEnum.ExcludeJob)
        {
            return a_satisfiability == ManufacturingOrder.AlternatePathSatisfiability.AllPathsSatisfiable;
        }

        return a_satisfiability != ManufacturingOrder.AlternatePathSatisfiability.NoPathsSatisfiable;
    }

    // !ALTERNATE_PATH!; $$$ 2.1 This is OptimizeProcessPathReleaseHandler
    private void OptimizeProcessPathReleaseHandler(long a_time, ManufacturingOrder a_mo, AlternatePath a_path)
    {
        ActivitiesCollection rescheduleActivities = new ();
        AddOutterUnscheduledActivities(a_mo, a_path, rescheduleActivities);

        SetupMaterialConstraints(rescheduleActivities, a_time);

        // In pre check in version of my code, these 2 lines were replaced by the 2 lines below it, but I'm not sure why.
        // I commented out the change and restored the original lines of code because the change
        // doens't match anything I'm currently working on.
        long earliestRelease = a_mo.GetEarliestDepartmentalEndSpan(m_simStartTime);
        long releaseTicks = Math.Max(a_time, earliestRelease);

        //long releaseTicks;
        //releaseTicks = m_optimizeStartTime.OptimizeDefaultStartTicks;

        AddOptimizationReleaseEventsForRescheduleActivities(releaseTicks, rescheduleActivities);
    }

    //#region Create manufacturing Order batch sets by group
    //private ManufacturingOrderBatchSetsByDefAndGroup m_mobsByGroup;
    //private ManufacturingOrderBatchSetsByDefAndGroup m_mobsByGroupBeforeOptimizeStartDate;

    //private void CreateExistingMOBSByGroup(SimulationTimePoint a_simStartTime, AddMOReleaseEventArgsForOpt a_optimizeSettings, List<Resource> a_resList)
    //{
    //    List<ManufacturingOrderBatch> batchesBeforeSimStartList = new ();

    //    for (int resI = 0; resI < a_resList.Count; ++resI)
    //    {
    //        Resource res = a_resList[resI];
    //        long simStartTicks = a_simStartTime.GetTimeForResource(res);
    //        ResourceBlockList.Node prevNode = res.Blocks.First;

    //        while (prevNode != null)
    //        {
    //            ResourceBlock prevBlock = prevNode.Data;
    //            ResourceBlockList.Node nextNode = prevNode.Next;

    //            ManufacturingOrder prevMO = prevBlock.Activity.ManufacturingOrder;
    //            ManufacturingOrderBatchDefinition mobd;

    //            if (prevBlock.StartTicks < simStartTicks && prevBlock.Activity.ManufacturingOrder.IsSetupForBatching() && (mobd = m_mobDefManager.GetMOBatchSetDefinition(prevMO.BatchDefinitionName)) != null)
    //            {
    //                ManufacturingOrderBatch mob = prevMO.m_batch;
    //                if (mob == null)
    //                {
    //                    mob = new ManufacturingOrderBatch(mobd, Clock, a_optimizeSettings);
    //                    prevMO.m_batch = mob;
    //                    prevMO.m_batchDefinition = mobd;
    //                    mob.Add(prevMO);
    //                    batchesBeforeSimStartList.Add(mob);
    //                }

    //                if (nextNode != null)
    //                {
    //                    ResourceBlock nextBlock = nextNode.Data;
    //                    ManufacturingOrder nextMO = nextBlock.Activity.ManufacturingOrder;

    //                    if (nextMO.m_batch == null)
    //                    {
    //                        if (nextBlock.StartTicks < simStartTicks)
    //                        {
    //                            if (ManufacturingOrder.AreBatchable(prevMO, nextMO) && ManufacturingOrder.AreScheduledBackToBack(prevMO, nextMO))
    //                            {
    //                                nextMO.m_batch = mob;
    //                                nextMO.m_batchDefinition = mobd;
    //                                mob.Add(nextMO);
    //                            }
    //                        }
    //                        else
    //                        {
    //                            break;
    //                        }
    //                    }
    //                }
    //            }

    //            prevNode = nextNode;
    //        }
    //    }

    //    m_mobsByGroupBeforeOptimizeStartDate = new ManufacturingOrderBatchSetsByDefAndGroup();

    //    for (int i = 0; i < batchesBeforeSimStartList.Count; ++i)
    //    {
    //        ManufacturingOrderBatch mob = batchesBeforeSimStartList[i];
    //        ManufacturingOrder mo = mob.m_mos[0].m_mo;
    //        ManufacturingOrderBatchSet mobs = m_mobsByGroupBeforeOptimizeStartDate.GetMOSetByDefAndGroup(mo.BatchDefinitionName, mo.BatchGroupName);
    //        if (mobs == null)
    //        {
    //            mobs = new ManufacturingOrderBatchSet();
    //        }

    //        mobs.Add(mob);

    //        if (m_mobsByGroupBeforeOptimizeStartDate.GetMOSetByDefAndGroup(mo.BatchDefinitionName, mo.BatchGroupName) == null)
    //        {
    //            m_mobsByGroupBeforeOptimizeStartDate.Add(mo.BatchDefinitionName, mo.BatchGroupName, mobs);
    //        }
    //    }
    //}

    ///// <summary>
    ///// Create a HashSet of BaseIds of Manufacturing Orders that are scheudled to start before the simulation start time.
    ///// </summary>
    ///// <param name="a_simulationStartTime"></param>
    ///// <param name="a_resList"></param>
    ///// <returns></returns>
    //private HashSet<BaseId> CreateMOsBeforeSimStartHashSet(SimulationTimePoint a_simulationStartTime, List<Resource> a_resList)
    //{
    //    HashSet<BaseId> o_mosBeforeSimStart;
    //    o_mosBeforeSimStart = new HashSet<BaseId>();

    //    for (int resI = 0; resI < a_resList.Count; ++resI)
    //    {
    //        Resource res = a_resList[resI];
    //        ResourceBlockList.Node curNode = res.Blocks.First;
    //        long resSimStartTime = a_simulationStartTime.GetTimeForResource(res);

    //        while (curNode != null)
    //        {
    //            ResourceBlock rb = curNode.Data;
    //            if (rb.StartTicks < resSimStartTime)
    //            {
    //                ManufacturingOrder mo = rb.Activity.ManufacturingOrder;
    //                if (!o_mosBeforeSimStart.Contains(mo.Id))
    //                {
    //                    o_mosBeforeSimStart.Add(mo.Id);
    //                }
    //            }

    //            curNode = curNode.Next;
    //        }
    //    }

    //    return o_mosBeforeSimStart;
    //}

    //private void CreateMOBSByGroup(ManufacturingOrderList a_moList,
    //                               AddMOReleaseEventArgsForOpt a_optimizeSettings,
    //                               HashSet<BaseId> a_mosBeforeSimStart)
    //{
    //    SortedDictionary<ManufacturingOrderBatchSetsByDefAndGroup.Key, ManufacturingOrderBatchSet>.Enumerator oldBatchesEnum = m_mobsByGroupBeforeOptimizeStartDate.GetEnumerator();
    //    while (oldBatchesEnum.MoveNext())
    //    {
    //        oldBatchesEnum.Current.Value.BatchingComplete();
    //    }

    //    ManufacturingOrderList.Node currentMONode = a_moList.First;
    //    List<ManufacturingOrder> moSortedList = new (a_moList.Count);

    //    while (currentMONode != null)
    //    {
    //        ManufacturingOrder mo = currentMONode.Data;

    //        if (mo.BatchGroupName.Length > 0 && !a_mosBeforeSimStart.Contains(mo.Id) // Prevent MOs scheduled before the simulation start date from being added to batches.
    //           )
    //        {
    //            ManufacturingOrderBatchDefinition mobd = m_mobDefManager.GetMOBatchSetDefinition(mo.BatchDefinitionName);
    //            if (mobd != null)
    //            {
    //                moSortedList.Add(mo);
    //                mo.m_batchDefinition = mobd;
    //            } // ***** You might add something to the simulation warning log here on MO batch groups that have no batch definitions
    //        }

    //        currentMONode = currentMONode.Next;
    //    }

    //    MOBatchComparer comparer = new (Clock, a_optimizeSettings);
    //    moSortedList.Sort(comparer);

    //    string currentBatchGroupDefinitionName = null;
    //    string currentBatchGroup = null;
    //    ManufacturingOrderBatchSet currentMOBSet = null;
    //    m_mobsByGroup = new ManufacturingOrderBatchSetsByDefAndGroup();

    //    for (int moI = 0; moI < moSortedList.Count; ++moI)
    //    {
    //        ManufacturingOrder mo = moSortedList[moI];

    //        if (mo.BatchDefinitionName != currentBatchGroupDefinitionName || mo.BatchGroupName != currentBatchGroup)
    //        {
    //            if (currentMOBSet != null)
    //            {
    //                currentMOBSet.BatchingComplete();
    //            }

    //            currentMOBSet = m_mobsByGroup.GetMOSetByDefAndGroup(mo.BatchDefinitionName, mo.BatchGroupName);

    //            if (currentMOBSet == null)
    //            {
    //                currentMOBSet = new ManufacturingOrderBatchSet();
    //                m_mobsByGroup.Add(mo.BatchDefinitionName, mo.BatchGroupName, currentMOBSet);
    //            }

    //            currentBatchGroupDefinitionName = mo.BatchDefinitionName;
    //            currentBatchGroup = mo.BatchGroupName;
    //        }

    //        ManufacturingOrderBatch mob = null;
    //        if (currentMOBSet.Count > 0)
    //        {
    //            mob = currentMOBSet[currentMOBSet.Count - 1];
    //            if (mob.IsEligible(mo))
    //            {
    //                mob.Add(mo);
    //            }
    //            else
    //            {
    //                mob = null;
    //            }
    //        }

    //        if (mob == null)
    //        {
    //            ManufacturingOrderBatchDefinition mobd = m_mobDefManager.GetMOBatchSetDefinition(mo.BatchDefinitionName);
    //            ManufacturingOrderBatch newMob = new (mobd, Clock, a_optimizeSettings);
    //            newMob.Add(mo);
    //            currentMOBSet.Add(newMob);
    //        }
    //    }

    //    if (currentBatchGroup != null)
    //    {
    //        currentMOBSet.BatchingComplete();
    //    }

    //    ValidateMOBatches();
    //}

    //#region Validation
    //internal void ValidateJobTWarningsAndWriteToLog(List<Job> a_addedJobs, List<Job> a_updatedJobs)
    //{
    //    if (m_scenarioOptions.ShowMOBatchingTabOnOptimizeDialog)
    //    {
    //        StringBuilder batchWarningText = new ();
    //        List<Job> allJobs = new ();
    //        allJobs.AddRange(a_addedJobs);
    //        allJobs.AddRange(a_updatedJobs);
    //        for (int jobI = 0; jobI < allJobs.Count; ++jobI)
    //        {
    //            Job job = allJobs[jobI];
    //            for (int moI = 0; moI < job.ManufacturingOrders.Count; ++moI)
    //            {
    //                ManufacturingOrder mo = job.ManufacturingOrders[moI];
    //                ValidateMOsBatchDefExists(batchWarningText, mo);
    //            }
    //        }

    //        if (batchWarningText.Length > 0)
    //        {
    //            m_errorReporter.LogException(new SchedulingWarningException(batchWarningText.ToString()), null);
    //        }
    //    }
    //}

    //private void ValidateMOBatches()
    //{
    //    StringBuilder warnings = new ();
    //    StringBuilder routingDifferenceWarnings = new ();
    //    SortedDictionary<ManufacturingOrderBatchSetsByDefAndGroup.Key, ManufacturingOrderBatchSet>.Enumerator e = m_mobsByGroup.GetEnumerator();

    //    while (e.MoveNext())
    //    {
    //        ManufacturingOrderBatchSet mobs = e.Current.Value;
    //        for (int mobI = 0; mobI < mobs.Count; ++mobI)
    //        {
    //            ManufacturingOrderBatch mob = mobs[mobI];
    //            ManufacturingOrder moComparison = null;
    //            for (int moI = 0; moI < mob.m_mos.Count; ++moI)
    //            {
    //                ManufacturingOrder mo = mob.m_mos[moI].m_mo;
    //                if (moComparison == null)
    //                {
    //                    moComparison = mo;
    //                }
    //                else
    //                {
    //                    moComparison.DetermineDifferences(mo, ManufacturingOrder.DifferenceTypes.any, routingDifferenceWarnings);
    //                    if (routingDifferenceWarnings.Length > 0)
    //                    {
    //                        warnings.AppendFormat("Differences were found between ManufacturingOrders in the same batch. Manufacturing Order \"{0}\" and Manufacturing Order \"{1}\" have the following differences:", moComparison, mo);
    //                        warnings.AppendLine(routingDifferenceWarnings.ToString());
    //                        routingDifferenceWarnings = new StringBuilder();
    //                    }
    //                }

    //                ValidateMOsBatchDefExists(warnings, mo);
    //            }
    //        }
    //    }

    //    if (warnings.Length > 0)
    //    {
    //        m_errorReporter.LogException(new SchedulingWarningException(warnings.ToString()), null);
    //    }
    //}

    //private void ValidateMOsBatchDefExists(StringBuilder a_batchWarningText, ManufacturingOrder a_mo)
    //{
    //    if (a_mo.BatchDefinitionName.Length > 0)
    //    {
    //        if (!m_mobDefManager.Contains(a_mo.BatchDefinitionName))
    //        {
    //            a_batchWarningText.AppendFormat("Manufacturing Order {0} specifies MO Batch Definition Name \"{1}\", but there is no Batch Definition rule with this name.{2}", a_mo, a_mo.BatchDefinitionName, Environment.NewLine);
    //        }
    //    }
    //}
    //#endregion

    //private class MOBatchComparer : IComparer<ManufacturingOrder>
    //{
    //    internal MOBatchComparer(long a_clock, AddMOReleaseEventArgsForOpt a_args)
    //    {
    //        m_clock = a_clock;
    //        m_args = a_args;
    //    }

    //    private readonly long m_clock;
    //    private readonly AddMOReleaseEventArgsForOpt m_args;

    //    int IComparer<ManufacturingOrder>.Compare(ManufacturingOrder a_x, ManufacturingOrder a_y)
    //    {
    //        int c;

    //        if ((c = string.Compare(a_x.BatchDefinitionName, a_y.BatchDefinitionName)) != 0)
    //        {
    //            return c;
    //        }

    //        if ((c = string.Compare(a_x.BatchGroupName, a_y.BatchGroupName)) != 0)
    //        {
    //            return c;
    //        }

    //        switch (a_x.m_batchDefinition.MOBatchMethod)
    //        {
    //            case ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedDate:
    //                return CompareMOsByNeedDateAndBaseId(a_x, a_y);

    //            case ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByReleaseDate:
    //                if ((c = CompareMOsByReleaseDate(a_x, a_y)) != 0)
    //                {
    //                    return c;
    //                }

    //                return CompareMOsByNeedDateAndBaseId(a_x, a_y);

    //            case ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedWhereBatchEligibilityIsDeterminedByReleaseDateBeingEarlierThanNeedDate:
    //                if ((c = Comparison.Compare(a_x.GetNeedDateTicksOfMOForScheduling(), a_y.GetNeedDateTicksOfMOForScheduling())) != 0)
    //                {
    //                    return c;
    //                }

    //                if ((c = CompareMOsByReleaseDate(a_x, a_y)) != 0)
    //                {
    //                    return c;
    //                }

    //                return BaseId.Compare(a_x.Id, a_y.Id);

    //            default:
    //                throw new ManufacturingOrderBatch.ManufacturingOrderBatchTypeException();
    //        }
    //    }

    //    private int CompareMOsByReleaseDate(ManufacturingOrder a_x, ManufacturingOrder a_y)
    //    {
    //        ManufacturingOrder.EffectiveReleaseDateType t;

    //        long xReleaseDateTicks;
    //        a_x.CalcPathsReleaseTicks(m_clock, a_x.CurrentPath, m_clock, m_args, out t, out xReleaseDateTicks);

    //        long yReleaseDateTicks;
    //        a_y.CalcPathsReleaseTicks(m_clock, a_y.CurrentPath, m_clock, m_args, out t, out yReleaseDateTicks);

    //        return Comparison.Compare(xReleaseDateTicks, yReleaseDateTicks);
    //    }

    //    private int CompareMOsByNeedDateAndBaseId(ManufacturingOrder a_x, ManufacturingOrder a_y)
    //    {
    //        int c;

    //        if ((c = Comparison.Compare(a_x.GetNeedDateTicksOfMOForScheduling(), a_y.GetNeedDateTicksOfMOForScheduling())) != 0)
    //        {
    //            return c;
    //        }

    //        return BaseId.Compare(a_x.Id, a_y.Id);
    //    }
    //}
    //#endregion

    private void AddJobHoldReleasedEvents(List<Job> a_jobs)
    {
        for (int jobI = 0; jobI < a_jobs.Count; ++jobI)
        {
            Job job = a_jobs[jobI];
            AddJobHoldReleaseEvent(job, Clock);
        }
    }

    // !ALTERNATE_PATH!; I don't know why this function was marked. It might have been a mistake. Check the old SourceControl to find out.
    /// <summary>
    /// Create OptimizationReleaseEvent events to prevent activities from being scheduled before
    /// the optimization time.
    /// </summary>
    /// <param name="optimizationTime">The start time of the optimization.</param>
    /// <param name="rescheduleActivities">The leaf activities of the activities that are being rescheduled.</param>
    private void AddOptimizationReleaseEventsForRescheduleActivities(long a_optimizationTime, ActivitiesCollection a_rescheduleActivities)
    {
        for (int activityI = 0; activityI < a_rescheduleActivities.Count; ++activityI)
        {
            InternalActivity activity = a_rescheduleActivities[activityI];

            if (!activity.Sequenced)
            {
                activity.WaitForOptimizationReleaseEvent = true;
                AddEvent(new OptimizationReleaseEvent(a_optimizationTime, activity));
            }
        }
    }
}