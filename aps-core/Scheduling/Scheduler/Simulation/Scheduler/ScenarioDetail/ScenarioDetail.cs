// !ALTERNATE_PATH!; The initial version of this enhancement was checked in on 10/5/2011. You can see the initial changes in SourceOffSite.

// Enable the defintion to disable the changes made for this task. This can be deleted after testing is complete.
// The changes delay the release of operations whose material requirements eligible lots aren't available yet.
//#define tfstask10688Disable

#if TEST
//#define EVENT_LISTS

//// Used to count activity scheduling. 
//// This definition didn't work in release mode the last time I tried it. I had to add it to the release build's definitions. 
//// Look further into this if it's needed again. I may have been mixing up different definitions. Their code has now been separated,
//// so maybe the problem won't happen next time.
//#define SCHEDULED_ACTIVITY_COUNT

//// Used to count the number different types of events are added.
//#define EVENTS_COUNT

#endif

using System.Collections;
using System.Text;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Collections;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.File;
using PT.Common.Localization;
using PT.Common.PTMath;
using PT.Common.Range;
using PT.ERPTransmissions;
using PT.Scheduler.Demand;
using PT.Scheduler.ErrorReporting;
using PT.Scheduler.Schedule;
using PT.Scheduler.Schedule.Demand;
using PT.Scheduler.Schedule.Operation;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Collections;
using PT.Scheduler.Simulation.Customizations;
using PT.Scheduler.Simulation.Events;
using PT.Scheduler.Simulation.Scheduler.AlternatePaths;
using PT.Scheduler.Simulation.UndoReceive.Move;
using PT.SchedulerDefinitions;
using PT.Transmissions;

using static PT.Scheduler.ManufacturingOrder;

namespace PT.Scheduler;

/// <summary>
/// Contains all data related to one copy
/// </summary>
public partial class ScenarioDetail : ICalculatedValueCacheManager
{
    private const long c_trialDemoMaxMOsAllowedToSchedule = 100;

    /// <summary>
    /// Used to initialize some simulation state variables so the client and server have all the same values when the client starts.
    /// </summary>
    private void InitNonSerializedSimulationMembers()
    {
        m_withinPlanningHorizon = true;
        m_currentScheduledActivityNbr = -1;
        m_dataLimitReached = false;
    }

    #region Signals
    #region TimeAdjustment Signals
    /// <summary>
    /// Used to keep track of events that have occured during transmission processing.
    /// </summary>
    private BoolVector32 m_signals;

    private const int SignalJobsFinishedIdx = 0;
    private const int SignalManufacturingOrdersFinishedIdx = 1;
    private const int SignalActivitiesFinishedIdx = 2;
    private const int SignalJobsUnscheduledIdx = 3;
    private const int SignalOperationFinishedIdx = 4;
    private const int SignalOperationsUpdatedIdx = 5;
    private const int SignalProgress = 6;
    private const int SignalActivityRequiredFinishQuantityChangeIdx = 7;
    private const int SignalActivitiesDeletedIdx = 8;
    private const int SignalCriticalUperationUpdateIdx = 9;
    private const int SignalCriticalResourceUpdateIdx = 10;
    private const int SignalCriticalSuccessorMOUpdateIdx = 11;
    private const int PurchaseToStockAvailableDateChnagedIdx = 12;
    private const int SetupTableAdjustmentIdx = 13;
    private const int ScenarioOptionsTrackSubComponentSourceMOsIdx = 14;

    /// <summary>
    /// Call when a job is finished. Scenario detail will use this notification to adjust the schedule at some point.
    /// </summary>
    internal void JobsFinished()
    {
        m_signals[SignalJobsFinishedIdx] = true;
    }

    /// <summary>
    /// Call when an MO is finished. Scenario detail will use this notification to adjust the schedule at some point.
    /// </summary>
    internal void ManufacturingOrdersFinished()
    {
        m_signals[SignalManufacturingOrdersFinishedIdx] = true;
    }

    /// <summary>
    /// Call when an operation is finished. Scenario detail will use this notification to adjust the schedule at some point.
    /// </summary>
    internal void OperationsFinished()
    {
        m_signals[SignalOperationFinishedIdx] = true;
    }

    /// <summary>
    /// Call when an activity is finished. Scenario detail will use this notification to adjust the schedule at some point.
    /// </summary>
    internal void ActivitiesFinished()
    {
        m_signals[SignalActivitiesFinishedIdx] = true;
    }

    /// <summary>
    /// Call this notification during an update if a Job needs to be unscheduled.
    /// Scenario detail will use this notification to adjust the schedule at some point.
    /// </summary>
    internal void JobsUnscheduled()
    {
        m_signals[SignalJobsUnscheduledIdx] = true;
    }

    /// <summary>
    /// Call when fields that may affect the amount of processing time of an operation have been updated.
    /// Scenario detail will use this notification to adjust the schedule at some point.
    /// </summary>
    internal void OperationsUpdated()
    {
        m_signals[SignalOperationsUpdatedIdx] = true;
    }

    /// <summary>
    /// Call this function when progress of an operation has been updated.
    /// Scenario detail will use this notification to adjust the schedule at some point.
    /// </summary>
    internal void ProgressUpdated()
    {
        m_signals[SignalProgress] = true;
    }

    /// <summary>
    /// Call this function when the RequiredFinishQuantity of a scheduled activity has been changed.
    /// Scenario detail will use this notification to adjust the schedule at some point.
    /// </summary>
    internal void ActivityRequiredFinishQuantityChange()
    {
        m_signals[SignalActivityRequiredFinishQuantityChangeIdx] = true;
    }

    /// <summary>
    /// Call when an activity has been deleted.
    /// Scenario detail will use this notification to adjust the schedule at some point.
    /// </summary>
    internal void ActivitiesDeleted()
    {
        m_signals[SignalActivitiesDeletedIdx] = true;
    }

    /// <summary>
    /// Call this function when a field relevant to scheduling is changed in an operation; unless there is a more relevant signal.
    /// Scenario detail will use this notification to adjust the schedule at some point.
    /// </summary>
    internal void CriticalOperationUpdate()
    {
        m_signals[SignalCriticalUperationUpdateIdx] = true;
    }

    /// <summary>
    /// Call this function when a field relevant to scheduling is changed in an operation; unless there is a more relevant signal.
    /// Scenario detail will use this notification to adjust the schedule at some point.
    /// </summary>
    internal void CriticalResourceUpdate()
    {
        m_signals[SignalCriticalResourceUpdateIdx] = true;
    }

    /// <summary>
    /// Signal this event when any of the following occurs:
    /// 1. A new SuccessorMO is created.
    /// 2. A SuccessorMO updated. Some examples: the constrained operation is changed, the TransferTime is adjusted.
    /// </summary>
    internal void SignalCriticalSuccessorMOUpdate()
    {
        m_signals[SignalCriticalSuccessorMOUpdateIdx] = true;
    }

    internal void PurchaseToStockAvailableDateChanged()
    {
        m_signals[PurchaseToStockAvailableDateChnagedIdx] = true;
    }

    /// <summary>
    /// Call this function when a setup table has been changed. The schedule will be updated if necessary.
    /// </summary>
    internal void SetupTableAdjustmentChanged()
    {
        m_signals[SetupTableAdjustmentIdx] = true;
    }

    internal void ScenarioOptionsTrackSubComponentSourceMOsChanged()
    {
        m_signals[ScenarioOptionsTrackSubComponentSourceMOsIdx] = true;
    }
    #endregion

    #region resource eligibility signals
    private BoolVector32 m_eligibilitySignals;

    private const int SetupRangeUpdateIdx = 0;
    private const int LookupAttributeNumberRangeIdx = 1;

    private void _eligibilitySignal_SetupRangeUpdated()
    {
        m_eligibilitySignals[SetupRangeUpdateIdx] = true;
    }

    private void _eligibilitySignal_LookupAttributeNumberRangeUpdated()
    {
        m_eligibilitySignals[LookupAttributeNumberRangeIdx] = true;
    }
    #endregion

    #region SuccessorMO Signals
    private BoolVector32 m_successorMOSignals;

    //		const int SuccessorMOSignalJobDeletedIdx   = 0;
    private const int SuccessorMOSignalMOChangesIdx = 1;

    //		const int SuccessorMOSignalPathUpdatedIdx  = 2;
    private const int SuccessorMOSignalJobBaseTIdx = 3;

    //		/// <summary>
    //		/// Call when a job is deleted. Scenario detail will use this notification to adjust the schedule at some point.
    //		/// </summary>
    //		internal void SignalJobDeleted()
    //		{
    //			successorMOSignals[SuccessorMOSignalJobDeletedIdx]=true;
    //		}

    /// <summary>
    /// Call when a job is deleted or added other changes may also require and call to this function.
    /// </summary>
    internal void SignalMOChanges()
    {
        m_successorMOSignals[SuccessorMOSignalMOChangesIdx] = true;
    }

    //		/// <summary>
    //		/// Call when a job is deleted. Scenario detail will use this notification to adjust the schedule at some point.
    //		/// This is important since it may cause the predecessors referenced path or path/operation combination to be referencable or
    //		/// unrefrencable.
    //		/// </summary>
    //		internal void SignalPathUpdated()
    //		{
    //			successorMOSignals[SuccessorMOSignalPathUpdatedIdx]=true;
    //		}

    /// <summary>
    /// Call this when a JobT has been received. JobTs may end up:
    /// creating new Jobs or MOs
    /// updating alternate paths
    /// deleting Jobs or MOs
    /// updating the status of Jobs and MOs; possibly finish operation, Jobs, and MOs
    /// </summary>
    internal void SignalJobBaseT()
    {
        m_successorMOSignals[SuccessorMOSignalJobBaseTIdx] = true;
    }

    /// <summary>
    /// Make sure you call this function when predecessor and successor MOs are linked to the predecessors.
    /// It clears a flag that determines whether a linking needs to be done after transmissions handling.
    /// </summary>
    internal void SuccessorMOsLinked()
    {
        m_successorMOSignals.Clear();
    }

    /// <summary>
    /// Links the predecessor and successor MOs and clears the SuccessorMOsSignals. Linking of successor MOs
    /// on a scenario wide basis should always be done through this function unless you call SuccessorMOsLinked() manually.
    /// </summary>
    private void LinkSuccessorMOs()
    {
        m_jobManager.LinkSuccessorsInAllJobs();
        SuccessorMOsLinked();
    }
    #endregion
    #endregion

    #region Types
    public enum SimulationType
    {
        None,
        Optimize,
        Move,
        MoveAndExpedite,
        ClockAdvance,
        Compress,
        Expedite,
        TimeAdjustment,
        ConstraintsChangeAdjustment,
        UnscheduleJobs,
        ScheduleJobs, // Not implemented. Would be in the same fx where UnscheduleJobs is.
        JitCompress,
        Undo,
        Redo
    }

    public class SimulationValidationException : PTValidationException
    {
        internal SimulationValidationException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = true)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }
    }

    /// <summary>
    /// This is the exception thrown if a validation problem prevents an Expedite from being performed.
    /// </summary>
    internal class ExpediteValidationException : SimulationValidationException
    {
        internal ExpediteValidationException(string a_errorMessage, object[] a_stringParameters = null, bool a_appendHelpUrl = true)
            : base(a_errorMessage, a_stringParameters, a_appendHelpUrl) { }
    }

    internal class SimulationFailureException : CommonException
    {
        public SimulationFailureException(string a_message)
            : base(a_message) { }
    }

    public class SchedulingWarningException : PTHandleableException //Made public so the UI can check exception type to decide whether to show these messages.
    {
        internal SchedulingWarningException(string a_msg, object[] a_stringParameters = null, bool a_appendHelpUrl = true) :
            base(a_msg, a_stringParameters, a_appendHelpUrl) { }
    }
    #endregion Types

    #region Actions : This region defines the different types of simulation actions that may be performed through a transmission.
    #region Adjust for changed constraints
    private void RemoveExpediteViolators(ConstraintViolationList a_violations, InternalActivityList a_expediteActivities)
    {
        Hashtable expediteActivitiesHash = new ();

        InternalActivityList.Node current = a_expediteActivities.First;

        while (current != null)
        {
            InternalActivity ia = current.Data;
            expediteActivitiesHash.Add(ia, ia);
            current = current.Next;
        }

        ConstraintViolationList.Node currentCVN = a_violations.Last;
        while (currentCVN != null)
        {
            ConstraintViolation data = currentCVN.Data;
            ConstraintViolationList.Node currentCVNTemp = currentCVN;
            currentCVN = currentCVN.Previous;

            if (expediteActivitiesHash.Contains(data.m_activity))
            {
                a_violations.Remove(currentCVNTemp);
            }
        }
    }

    /// <summary>
    /// Call this function to cleanup any constraint violations that may have been created.
    /// Activities are laid back down in the schedule as they were before, except activities with constaint violations
    /// are allowed to end up somewhere else in the schedule.
    /// </summary>
    /// <param name="transmission"></param>
    /// These are kept on the same resource they are currently scheduled on.
    /// </param>
    /// <returns>Whether a constraint change adjustment was performed. An adjustment is only necessary in the event that there are activities that violate some Job, MO, Operation, or other constraint.</returns>
    internal void ConstraintsChangeAdjustment(ScenarioBaseT a_transmission, IScenarioDataChanges a_dataChanges)
    {
        SimulationType simType = SimulationType.ConstraintsChangeAdjustment;

        try
        {
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_transmission, SimulationProgress.Status.Initializing, m_nbrOfSimulationsSinceStartup);
            PT.Common.Testing.Timing ts = CreateTiming("ConstraintsChangeAdjustment");

            #if TEST
                SimDebugSetup();
            #endif

            MainResourceSet availableResources;
            CreateActiveResourceList(out availableResources);
            SimulationInitializationAll(availableResources, a_transmission, simType, null);

            UnscheduleInProcessActivitiesAndAddInProcessReleaseEvents(true);
            ConstraintViolationList violations = new ();
            m_jobManager.GetConstraintViolations(violations);
            RemoveExpediteViolators(violations, m_inProcessActivities);

            ConstraintViolationList.Node cvNode = violations.First;

            while (cvNode != null)
            {
                ConstraintViolation cv = cvNode.Data;
                InternalActivity activity = cv.m_activity;

                if (activity.Scheduled)
                {
                    InternalOperation operation = activity.Operation;
                    UnscheduleSuccessors(operation, UnscheduleType.Regular, null);
                    activity.Unschedule(false, true);
                    activity.Operation.AdjustedPlantResourceEligibilitySets_FilterNodeAndSuccessors(ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter.NoFilter());

                    if (activity.SetupWaitForAnchorSetFlag())
                    {
                        AddAnchorReleaseEvent(activity, activity.AnchorDateTicks);
                    }

                    operation.ManufacturingOrder.UnscheduledMOMarker = true;
                    SetupMaterialConstraints(activity, 0);
                }

                cvNode = cvNode.Next;
            }

            // Unschedule all other activities in the system and prepare for a simulation.

            ResourceActivitySets nonExpediteActivities = new (availableResources);

            UnscheduleActivities(availableResources, new SimulationTimePoint(Clock), new SimulationTimePoint(EndOfPlanningHorizon), nonExpediteActivities, simType, Clock);
            SimulationResourceInitialization(new SimulationResourceDispatcherUsageArgs(Clock, OptimizeSettings.dispatcherSources.NormalRules));
            Simulate(Clock, nonExpediteActivities, simType, a_transmission);

            StopTiming(ts, false);

            m_signals.Clear();
            #if TEST
                TestSchedule(simType.ToString());
            #endif
            SimulationActionComplete();

            m_simulationProgress.PostSimulationWorkComplete();
            CheckForRequiredAdditionalSimulation(a_dataChanges);
        }
        catch (SimulationValidationException e)
        {
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_transmission, SimulationProgress.Status.Terminated, m_nbrOfSimulationsSinceStartup);
            FireSimulationValidationFailureEvent(e, m_activeOptimizeSettingsT);
            throw;
        }
        catch (Exception)
        {
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_transmission, SimulationProgress.Status.Exception, m_nbrOfSimulationsSinceStartup);
            throw;
        }
    }
    #endregion

    #region Setup Helpers
    private void AddJobHoldReleaseEvent(Job a_job, long a_minimumReleaseDate)
    {
        if (a_job.Hold && a_job.HoldUntilTicks > a_minimumReleaseDate)
        {
            a_job.WaitingForJobReleaseEvent = true;
            JobHoldReleasedEvent jobEvent = new (a_job.HoldUntilTicks, a_job);
            AddEvent(jobEvent);
        }
    }

    private void AddManufacturingOrderHoldReleasedEvent(ManufacturingOrder a_mo, long a_minimumReleaseDate)
    {
        if (a_mo.Hold && a_mo.HoldUntilTicks > a_minimumReleaseDate)
        {
            a_mo.WaitingOnHoldReleasedEvent = true;
            ManufacturingOrderHoldReleasedEvent holdReleasedEvent = new (a_mo.HoldUntilTicks, a_mo);
            AddEvent(holdReleasedEvent);
        }
    }
    #endregion

    #region In-Process activities handling
    private InternalActivityList m_inProcessActivities;

    /// <summary>
    /// Unschedule the in-process activities and setup events to release them right away or at their hold dates.
    /// ManufacturingOrder.ToBeScheduled must have been set before calling this function.
    /// Only activities whose ManufacturingOrder.ToBeScheduled == true will be configured to be released.
    /// </summary>
    /// <param name="scheduledOnly"></param>
    private void UnscheduleInProcessActivitiesAndAddInProcessReleaseEvents(bool a_scheduledOnly)
    {
        m_inProcessActivities = m_jobManager.GetInprocessActivities(false, a_scheduledOnly, true);
        InternalActivityList.Node currentIAN = m_inProcessActivities.First;

        while (currentIAN != null)
        {
            if (currentIAN.Data.ManufacturingOrder.ToBeScheduled) // Must be set prior to using this function (currently either in SimulationInitialization1() or Optimize()
            {
                AlternatePath path = currentIAN.Data.ManufacturingOrder.AlternatePaths.GetOpsPath(currentIAN.Data.Operation);
                if (path != null && path.AdjustedPlantResourceEligibilitySets_IsSatisfiable())
                {
                    UnscheduleInProcessActivityAndAddInProcessReleaseEvent(currentIAN.Data);
                }
            }

            currentIAN = currentIAN.Next;
        }
    }

    /// <summary>
    /// Unschedule an in-process activity and setup an event to release it either right away or at the hold date.
    /// </summary>
    /// <param name="ia"></param>
    private void UnscheduleInProcessActivityAndAddInProcessReleaseEvent(InternalActivity a_ia)
    {
        a_ia.TempLock();
        a_ia.Unschedule(false, true);
        //#14139 Added validation so in-process activities cannot be put on hold. Removed this for simplicity.
        //long releaseTime = a_ia.Operation.GetLatestHoldTicks();
        InProcessReleaseEvent ipre = new (SimClock, a_ia);
        AddEvent(ipre);
        a_ia.InProcessReleaseEventAdded = true;
    }

    private void CleanupInProcessActivities()
    {
        InternalActivityList.Node currentIAN = m_inProcessActivities.First;

        while (currentIAN != null)
        {
            InternalActivity ia = currentIAN.Data;
            if (ia.InProcessReleaseEventAdded)
            {
                ia.TempLockClear();
            }

            currentIAN = currentIAN.Next;
        }

        m_inProcessActivities.Clear();
    }
    #endregion

    #region Schedule/Unschedule Jobs
    private void ScheduleJobs(ScenarioDetailScheduleJobsT a_t, IScenarioDataChanges a_scenarioDataChanges)
    {
        try
        {
            List<Job> jobs = JobManager.FindJobs(a_t.Jobs);
            bool jobUnscheduled = false;
            for (int jobI = 0; jobI < jobs.Count; ++jobI)
            {
                Job job = jobs[jobI];
                if (job.Finished)
                {
                    //skip
                }
                else //unschedule
                {
                    UnscheduleAudit jobUnscheduleAudit = new UnscheduleAudit(job);
                    job.Unschedule(false);
                    jobUnscheduled = true;

                    a_scenarioDataChanges.AuditEntry(jobUnscheduleAudit.GetAuditEntry());
                }
            }

            if (jobUnscheduled)
            {
                TimeAdjustment(a_t);
            }
        }
        catch (SimulationValidationException e)
        {
            FireSimulationValidationFailureEvent(e, a_t);
            throw;
        }
    }

    
    #endregion

    #region Time Adjustment
    internal void TimeAdjustment(DateTime a_startDate, ScenarioBaseT a_scenarioT)
    {
        try
        {
            TimeAdjustment(a_scenarioT);
        }
        catch (SimulationValidationException e)
        {
            FireSimulationValidationFailureEvent(e, a_scenarioT);
            throw;
        }
    }
    #endregion Time Adjustment

    #region Split/Unsplit
    #region MO Splitting
    private void SplitMO(ScenarioDetailSplitMOT a_t)
    {
        //--------------------------------------------------------------------------------------------
        // Not in use. You'll probably need to rewrite some of this code if you use this transmission.
        //--------------------------------------------------------------------------------------------

        //try
        //{
        //    BaseId jobId = t.JobId;
        //    BaseId moId = t.MOId;

        //    Job job = (Job)JobManager.GetById(jobId);

        //    if (job == null)
        //    {
        //        throw new SimulationValidationException("The job to be split couldn't be found.");
        //    }

        //    ManufacturingOrder mo = (ManufacturingOrder)job.ManufacturingOrders.GetById(moId);
        //    IsSplitableValidation(mo, t.SplitQty);
        //    job.Split(moId, t.SplitQty);
        //    TimeAdjustment(t);
        //}
        //catch (SimulationValidationException e)
        //{
        //    FireSimulationValidationFailureEvent(e, t);
        //    throw;
        //}
    }

    private string GetJobOrMOSplitErrorMessage()
    {
        string s;
        s = "Splitting Jobs enablement=" + ScenarioOptions.SplitJobEnabled;
        s += Environment.NewLine;
        s += "Spitting Manufacturing Orders enablement=" + ScenarioOptions.SplitMOEnabled;
        if (ScenarioOptions.SplitJobEnabled || ScenarioOptions.SplitMOEnabled)
        {
            s += Environment.NewLine;
            s += "Verify you're trying to perform the right type of split.";
        }

        return s;
    }

    internal class SplitParams
    {
        internal SplitParams(JobDefs.SplitScopeEnum a_splitScope,
                             Resource a_resource,
                             ResourceBlockList.Node a_resBlockListNode,
                             ScenarioDetailSplitJobOrMOT.SplitTypeEnum a_splitType,
                             long a_splitPoint)
        {
            SplitScope = a_splitScope;
            Resource = a_resource;
            ResourceBlockListNode = a_resBlockListNode;
            SplitType = a_splitType;
            SplitPoint = a_splitPoint;
        }

        internal JobDefs.SplitScopeEnum SplitScope { get; private set; }

        internal Resource Resource { get; private set; }

        internal ResourceBlockList.Node ResourceBlockListNode { get; private set; }

        internal ScenarioDetailSplitJobOrMOT.SplitTypeEnum SplitType { get; private set; }

        internal long SplitPoint { get; private set; }
    }

    private void SplitJobOrMOByCycleAtTimeT(long a_simClock, ScenarioDetailSplitJobOrMOT a_t, IScenarioDataChanges a_scenarioDataChanges)
    {
        try
        {
            ValidationJobOrMoSplitEnabled(a_t);

            Resource res = FindResource(a_t.ResourceKey);

            if (res == null)
            {
                throw new SimulationValidationException("2531");
            }

            ResourceBlockList.Node resListNode = FindBlock(res, a_t.BlockKey);

            if (resListNode == null)
            {
                throw new SimulationValidationException("2532");
            }

            SplitParams splitParams = new (a_t.SplitScope, res, resListNode, a_t.SplitType, a_t.SplitPoint);

            SplitJobOrMOByCycleAtTimeT_X(a_simClock, splitParams);
            ConstraintsChangeAdjustment(a_t, a_scenarioDataChanges);
        }
        catch (SimulationValidationException e)
        {
            FireSimulationValidationFailureEvent(e, a_t);
            throw;
        }
    }

    private void ValidationJobOrMoSplitEnabled(ScenarioDetailSplitJobOrMOT a_splitT)
    {
        if (a_splitT.SplitScope == JobDefs.SplitScopeEnum.Job)
        {
            if (!ScenarioOptions.SplitJobEnabled)
            {
                throw new SimulationValidationException(GetJobOrMOSplitErrorMessage());
            }
        }
        else if (a_splitT.SplitScope == JobDefs.SplitScopeEnum.MO)
        {
            if (!ScenarioOptions.SplitMOEnabled)
            {
                throw new SimulationValidationException(GetJobOrMOSplitErrorMessage());
            }
        }
    }

    private void SplitJobOrMOByCycleAtTimeT_X(long a_simClock, SplitParams a_splitParams)
    {
        if (a_splitParams.SplitScope == JobDefs.SplitScopeEnum.Job)
        {
            if (!ScenarioOptions.SplitJobEnabled)
            {
                throw new SimulationValidationException(GetJobOrMOSplitErrorMessage());
            }
        }
        else
        {
            if (!ScenarioOptions.SplitMOEnabled)
            {
                throw new SimulationValidationException(GetJobOrMOSplitErrorMessage());
            }
        }

        Resource res = a_splitParams.Resource;

        if (res == null)
        {
            throw new SimulationValidationException("2531");
        }

        ResourceBlockList.Node resBlockListNode = a_splitParams.ResourceBlockListNode;
        ResourceBlock block = resBlockListNode.Data;
        InternalActivity ia = block.Activity;

        RequiredSpanPlusSetup requiredSpan;
        if (ia.Batch != null)
        {
            requiredSpan = ia.Batch.SetupCapacitySpan;
        }
        else
        {
            requiredSpan = RequiredSpanPlusSetup.s_notInit;
        }

        SchedulableResult sr = res.RecalcSchedulableResult(this, SimClock, ia, true, block, resBlockListNode.Previous, requiredSpan);

        if (sr.m_result != SchedulableSuccessFailureEnum.Success)
        {
            throw new SimulationValidationException("2533");
        }

        CycleAdjustmentProfile ccp;
        int cycleI;

        res.CalculateCycleCompletionTimes(ia, sr.m_si, out ccp);

        if (ccp.Count == 0)
        {
            throw new SimulationValidationException("2534");
        }

        if (ccp.Count == 1)
        {
            throw new SimulationValidationException("2535");
        }

        switch (a_splitParams.SplitType)
        {
            case ScenarioDetailSplitJobOrMOT.SplitTypeEnum.AtClickTime:
            {
                CycleAdjustment clickedCycle = null;
                long cycleStartTicks = ia.GetScheduledEndOfSetupTicks();
                long cycleEndDate;

                for (cycleI = 0; cycleI < ccp.Count; ++cycleI)
                {
                    CycleAdjustment cc = ccp[cycleI];
                    cycleEndDate = cc.Date;

                    if (a_splitParams.SplitPoint >= cycleStartTicks && a_splitParams.SplitPoint < cycleEndDate)
                    {
                        clickedCycle = cc;
                        break;
                    }

                    cycleStartTicks = cycleEndDate;
                }

                if (clickedCycle == null)
                {
                    throw new SimulationValidationException("2536");
                }

                if (cycleI == 0)
                {
                    cycleI = 1;
                }
            }

                break;

            case ScenarioDetailSplitJobOrMOT.SplitTypeEnum.NbrOfCycles:
            {
                if (a_splitParams.SplitPoint >= ccp.Count)
                {
                    throw new SimulationValidationException("2537");
                }

                if (a_splitParams.SplitPoint > int.MaxValue || a_splitParams.SplitPoint <= 0)
                {
                    throw new SimulationValidationException("2538");
                }

                cycleI = ccp.Count - (int)a_splitParams.SplitPoint;
            }

                break;

            default:
                throw new SimulationValidationException("Unknown SplitTypeEnum");
        }

        decimal qty = 0;

        for (; cycleI < ccp.Count; ++cycleI)
        {
            CycleAdjustment cc = ccp[cycleI];
            qty += cc.Qty;
        }

        InternalOperation io = ia.Operation;
        ManufacturingOrder mo = io.ManufacturingOrder;

        decimal ratio = qty / io.RequiredFinishQty;

        IsSplitableValidation(mo, qty);

        if (a_splitParams.SplitScope == JobDefs.SplitScopeEnum.Job)
        {
            JobManager.Split(a_simClock, mo.Job, ratio, m_productRuleManager);
        }
        else
        {
            ManufacturingOrder newMo = mo.Job.SplitOffFractionIntoAnotherMOWithinTheSameJob(a_simClock, ia.ManufacturingOrder.Id, ratio, m_productRuleManager);
            mo.PreserveRequiredQty = true;
            newMo.PreserveRequiredQty = true;
        }
    }

    private void IsSplitableValidation(ManufacturingOrder a_mo, decimal a_splitQty)
    {
        string msg;

        if (a_mo == null)
        {
            throw new SimulationValidationException("2541");
        }

        if (!IsMOSplitable(a_mo, a_splitQty, out msg))
        {
            throw new SimulationValidationException("2542", new object[] { a_mo.Job.Name, a_mo.Name, msg });
        }
    }

    private bool IsMOSplitable(ManufacturingOrder a_mo, decimal a_splitQty, out string o_errMsg)
    {
        if (a_splitQty <= 0)
        {
            o_errMsg = "The split quantity must be greater than 0.";
            return false;
        }

        AlternatePath.NodeCollection nodes = a_mo.CurrentPath.GetRoots();

        if (nodes.Count != 1)
        {
            o_errMsg = string.Format("ManufacturingOrders with multiple final operations can't be split. The current path has {0} roots.", nodes.Count);
            return false;
        }

        if (a_mo.CurrentPath.HasTimeBasedReportingOperations())
        {
            o_errMsg = "This feature isn't compatible with Operations that use time based reporting.";
            return false;
        }

        o_errMsg = "";

        return true;
    }

    private void HandleScenarioDetailChangeMOQtyT(ScenarioDetailChangeMOQtyT a_t)
    {
        if (!ScenarioOptions.ChangeMOQty)
        {
            throw new SimulationValidationException("2543");
        }

        JobManager.HandleScenarioDetailChangeMOQtyT(a_t, m_productRuleManager);
        TimeAdjustment(a_t);
    }
    #endregion MO Splitting

    #region MO Unsplitting
    private void JoinJobOrMOT(ScenarioDetailJoinJobOrMOT a_t, IScenarioDataChanges a_scenarioDataChanges)
    {
        try
        {
            if (!ScenarioOptions.JoinEnabled)
            {
                throw new SimulationValidationException("2544");
            }

            Resource res = FindResource(a_t.ResourceKey);
            ResourceBlockList.Node resListNode = FindBlock(res, a_t.BlockKey);

            Job leftJob;
            ManufacturingOrder leftMO;

            Job rightJob;
            ManufacturingOrder rightMO;

            bool jobJoin;

            ValidateUnsplit(resListNode, out leftJob, out leftMO, out rightJob, out rightMO, out jobJoin);

            if (jobJoin)
            {
                m_jobManager.Join(leftJob, rightJob, m_productRuleManager, a_scenarioDataChanges);
                leftJob.ComputeEligibility(m_productRuleManager);
            }
            else
            {
                leftMO.Job.ManufacturingOrders.Unsplit(leftMO, rightMO, m_productRuleManager, a_scenarioDataChanges);
                leftMO.Job.UpdateScheduledStatus();
            }

            ConstraintsChangeAdjustment(a_t, a_scenarioDataChanges);
        }
        catch (SimulationValidationException e)
        {
            FireSimulationValidationFailureEvent(e, a_t);
            throw;
        }
    }

    private const string CANT_UNSPLIT = "Can't join ManufacturingOrders. ";

    /// <summary>
    /// Throws a validation exception if this block isn't to the right of another block of the same job that it can be merged with.
    /// </summary>
    /// <param name="blockNode"></param>
    private void ValidateUnsplit(ResourceBlockList.Node a_blockNode, out Job o_jobLeft, out ManufacturingOrder o_moLeft, out Job o_jobRight, out ManufacturingOrder o_moRight, out bool o_jobSplit)
    {
        o_jobSplit = false;

        ResourceBlockList.Node leftNeighborNode = a_blockNode.Previous;
        if (leftNeighborNode == null)
        {
            throw new ValidationException("2545", new object[] { CANT_UNSPLIT });
        }

        ResourceBlock rightBlock = a_blockNode.Data;
        InternalActivity iaRight = rightBlock.Activity;
        ResourceBlock leftBlock = leftNeighborNode.Data;
        InternalActivity iaLeft = leftBlock.Activity;
        Resource res = leftBlock.ScheduledResource;
        o_moLeft = iaLeft.ManufacturingOrder;
        o_jobLeft = o_moLeft.Job;
        o_moRight = iaRight.ManufacturingOrder;
        o_jobRight = o_moRight.Job;

        o_jobSplit = iaLeft.Job != iaRight.Job;

        if (o_jobSplit)
        {
            if (o_jobLeft.Product != o_jobRight.Product)
            {
                throw new SimulationValidationException("2546", new object[] { o_jobLeft.Product, o_jobRight.Product, CANT_UNSPLIT });
            }

            if (o_jobLeft.ManufacturingOrders.Count != 1 || o_jobRight.ManufacturingOrders.Count != 1)
            {
                throw new SimulationValidationException("2547", new object[] { CANT_UNSPLIT });
            }
        }
        else
        {
            if (!iaLeft.ManufacturingOrder.Split && !iaRight.ManufacturingOrder.Split)
            {
                throw new ValidationException("2548", new object[] { CANT_UNSPLIT });
            }

            string moLeftExtId = UnSplitMOExternalIdValidation(o_moLeft);
            string moRightExtId = UnSplitMOExternalIdValidation(o_moRight);

            if (!moLeftExtId.Equals(moRightExtId))
            {
                throw new ValidationException("2549", new object[] { CANT_UNSPLIT });
            }
        }

        if (!res.ArePointsWithinAContinuousOnlineInterval(leftBlock.EndTicks, rightBlock.StartTicks))
        {
            throw new ValidationException("2550", new object[] { CANT_UNSPLIT });
        }

        if (ReferenceEquals(o_moLeft, o_moRight))
        {
            throw new ValidationException("2551", new object[] { CANT_UNSPLIT });
        }

        if (o_moLeft.CurrentPath.Name != o_moRight.CurrentPath.Name)
        {
            throw new ValidationException("2552", new object[] { CANT_UNSPLIT });
        }
    }

    private string UnSplitMOExternalIdValidation(ManufacturingOrder a_mo)
    {
        if (a_mo.Split)
        {
            string externalId;
            int splitNbr;
            if (Simulation.Scheduler.Job.SplitHelpers.GetSourceMOExternalId(a_mo, out externalId, out splitNbr))
            {
                return externalId;
            }

            throw new ValidationException("2553", new object[] { CANT_UNSPLIT });
        }

        return a_mo.ExternalId;
    }
    #endregion

    /// <summary>
    /// Throws a validation exception if the resource isn't found.
    /// </summary>
    /// <param name="resKey"></param>
    /// <returns></returns>
    private Resource FindResourceWithValidationException(ResourceKey a_resKey)
    {
        try
        {
            return FindResource(a_resKey);
        }
        catch (Exception e)
        {
            throw new ValidationException("2554", new object[] { e.Message });
        }
    }

    /// <summary>
    /// Returns ResourceBlockList.Node
    /// Throws a validation exception if the block isn't found.
    /// </summary>
    /// <param name="blkKey"></param>
    /// <returns></returns>
    private ResourceBlockList.Node FindBlock(Resource a_res, BlockKey a_blkKey)
    {
        ResourceBlockList.Node resListNode = a_res.Blocks.FindNodeByKey(a_blkKey);
        if (resListNode == null)
        {
            throw new ValidationException("2555");
        }

        return resListNode;
    }
    #endregion

    #region Common Helpers :This region defines functions common to some of the different actions.
    /// <summary>
    /// Get the start time to use from an OptimizeSettings object.
    /// </summary>
    /// <param name="optimizeSettings"></param>
    /// <returns></returns>
    private SimulationTimePoint GetScheduleDefaultStartTime(OptimizeSettings a_optimizeSettings)
    {
        SimulationTimePoint startTime;

        switch (a_optimizeSettings.StartTime)
        {
            case OptimizeSettings.ETimePoints.EndOfFrozenZone:
                startTime = new SimulationTimePoint(this, m_plantManager.GetEarliestFrozenSpanEnd(Clock), OptimizeSettings.ETimePoints.EndOfFrozenZone);
                break;
            case OptimizeSettings.ETimePoints.EndOfStableZone:
                startTime = new SimulationTimePoint(this, m_plantManager.GetEarliestStableSpanEnd(Clock), OptimizeSettings.ETimePoints.EndOfStableZone);
                break;
            case OptimizeSettings.ETimePoints.EndOfShortTerm:
                startTime = new SimulationTimePoint(GetEndOfShortTerm().Ticks);
                break;
            case OptimizeSettings.ETimePoints.CurrentPTClock:
                startTime = new SimulationTimePoint(Clock);
                break;
            case OptimizeSettings.ETimePoints.EndOfPlanningHorizon:
                startTime = new SimulationTimePoint(EndOfPlanningHorizon);
                break;
            case OptimizeSettings.ETimePoints.EntireSchedule:
                throw new SimulationValidationException("2558"); //End of the horizon doesn't make sense as a start time
            case OptimizeSettings.ETimePoints.SpecificDateTime:
                startTime = new SimulationTimePoint(a_optimizeSettings.SpecificStartTime.Ticks);
                break;
            default:
                throw new SimulationValidationException("2558");
        }

        DateErrorCheck(startTime.DateTimeTicks);

        return startTime;
    }

    /// <summary>
    /// Get the stop time to use from an OptimizeSettings object.
    /// </summary>
    /// <param name="optimizeSettings"></param>
    /// <returns></returns>
    private SimulationTimePoint GetScheduleDefaultEndTime(OptimizeSettings a_optimizeSettings)
    {
        SimulationTimePoint stopTime;

        switch (a_optimizeSettings.EndTime)
        {
            case OptimizeSettings.ETimePoints.EndOfFrozenZone:
                stopTime = new SimulationTimePoint(this, m_plantManager.GetEarliestFrozenSpanEnd(Clock), OptimizeSettings.ETimePoints.EndOfFrozenZone); //TODO: Could be latest?
                break;
            case OptimizeSettings.ETimePoints.EndOfStableZone:
                stopTime = new SimulationTimePoint(this, m_plantManager.GetEarliestStableSpanEnd(Clock), OptimizeSettings.ETimePoints.EndOfStableZone); //TODO: Could be latest?
                break;
            case OptimizeSettings.ETimePoints.EndOfShortTerm:
                stopTime = new SimulationTimePoint(GetEndOfShortTerm().Ticks);
                break;
            case OptimizeSettings.ETimePoints.CurrentPTClock:
                throw new SimulationValidationException("2558"); //Clock doesn't make sense as a stopping point
            case OptimizeSettings.ETimePoints.EndOfPlanningHorizon:
                stopTime = new SimulationTimePoint(EndOfPlanningHorizon);
                break;
            case OptimizeSettings.ETimePoints.EntireSchedule:
                long maxBlockEnd = PlantManager.GetResourceList().Where(r => r.Blocks.Count > 0).Max(r => r.Blocks.Last.Data.EndTicks);
                stopTime = new SimulationTimePoint(maxBlockEnd);
                break;
            case OptimizeSettings.ETimePoints.SpecificDateTime:
                stopTime = new SimulationTimePoint(a_optimizeSettings.SpecificEndTime.Ticks);
                break;
            default:
                throw new SimulationValidationException("2558");
        }

        DateErrorCheck(stopTime.DateTimeTicks);

        return stopTime;
    }

    public class DateErrorCheckException : SimulationValidationException
    {
        public DateErrorCheckException()
            : base("2559") { }
    }

    /// <summary>
    /// Verify that the date the simulation is being performed on is not less than the current clock time. A problem is reported through the throw of an exception.
    /// </summary>
    /// <param name="expediteDate">The date of the simulation action.</param>
    private void DateErrorCheck(long a_date)
    {
        if (a_date != 0 && a_date < Clock)
        {
            throw new DateErrorCheckException();
        }
    }

    // Since this class is only initialized with the values in m_activeOptimizeSettings, you can get rid of it and use m_activeOptimizeSettings instead. But first 
    // check that it's not possible for m_activeOptimizeSettings to change within the same simulation.
    internal class AddMOReleaseEventArgsForOpt
    {
        public AddMOReleaseEventArgsForOpt(OptimizeSettings a_optimizeSettings)
        {
            m_JITSlackDays = a_optimizeSettings.JITSlackTicks;
            m_useResourceCapacityForHeadstart = a_optimizeSettings.UseResourceCapacityForHeadstart;
        }

        internal long m_JITSlackDays;
        internal bool m_useResourceCapacityForHeadstart;
    }

    /// <summary>
    /// Add a release event for the MO.
    /// </summary>
    /// <param name="a_mo">A release event will be added for this MO.</param>
    /// <param name="a_earliestSimulationStartTime">Start time of the simulation. The order cannot be released before this time, but can be released after this time depending on the MO's release date.</param>
    /// <param name="a_optimizeSettings">This value can be null; for instance when adding this event for an MO being expedited.</param>
    private void AddManufacturingOrderReleaseEvent(long a_simClock, ManufacturingOrder a_mo, SimulationTimePoint a_earliestSimulationStartTime, AddMOReleaseEventArgsForOpt a_optimizeSettings)
    {
        if (!a_mo.ManufacturingOrderReleasedEventScheduled)
        {
            a_mo.ManufacturingOrderReleasedEventScheduled = true;

            long moReleaseDateTicks;

            EffectiveReleaseDateType releaseType = EffectiveReleaseDateType.NotSet;

            if (a_mo.InProcess) //This is not Started meaning we are excluding Finished operations
            {
                moReleaseDateTicks = Clock;
                releaseType = EffectiveReleaseDateType.Unconstrained;
            }
            else
            {
                //if (a_mo.m_batch != null && m_activeOptimizeSettings.MOBatchingByBatchGroupEnabled)
                //{
                //    moReleaseDateTicks = a_mo.m_batch.m_releaseDateTicks;
                //    releaseType = a_mo.m_batch.m_releaseType;
                //}
                //else
                {
                    moReleaseDateTicks = a_mo.GetEarliestDepartmentalEndSpan(a_earliestSimulationStartTime);
                    bool constrained = a_mo.GetConstrainedReleaseTicks(out long moEffectiveReleaseTicks);
                    if (constrained)
                    {
                        moReleaseDateTicks = Math.Max(moReleaseDateTicks, moEffectiveReleaseTicks);
                        releaseType = EffectiveReleaseDateType.PredecessorMO;
                    }
                }
            }

            ManufacturingOrderReleasedEvent manufacturingOrderReleasedEvent = new (moReleaseDateTicks, a_mo, releaseType);
            AddEvent(manufacturingOrderReleasedEvent);
        }
    }

    /// <summary>
    /// Add any necessary hold and FinishedPredecessorMOsAvailableEvents for the MO.
    /// Note: finished predecessor MOs that constrain successor MOs at the MO level aren't covered by these events.
    /// That is currently handled within the release time of the ManufacturingOrderReleaseEvent.
    /// </summary>
    /// <param name="a_alternatePath"></param>
    private void AddConstrainingEvents(AlternatePath a_alternatePath)
    {
        IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = a_alternatePath.AlternateNodeSortedList.GetEnumerator();
        while (alternateNodesEnumerator.MoveNext())
        {
            AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
            BaseOperation operation = node.Operation;

            if (!operation.IsFinishedOrOmitted)
            {
                AddOnholdEvent(operation);
                SetupMaterialConstraints(operation, SimClock);
                AddFinishedPredecessorMOsAvailableEvent(operation);

                //Check for possibly finished predecessor operations to create some events to make sure that transfer info is enforced.
                if (operation.Predecessors.Count > 0)
                {
                    long latestReleaseTicks = long.MinValue;
                    foreach (AlternatePath.Association association in operation.Predecessors)
                    {
                        InternalOperation predOp = association.Predecessor.Operation;
                        if (predOp.Finished && predOp.GetReportedStartDate(out long reportedStart) && predOp.GetReportedFinishDate(out long reportedFinishDate))
                        {
                            if (association.TransferSpanTicks > 0)
                            {
                                long releaseTime = CalcReleaseWithTransferTime(reportedStart, reportedFinishDate, association, out TransferInfo ti);
                                operation.SetOrUpdateTransferInfo(ti);

                                if (releaseTime > latestReleaseTicks)
                                {
                                    latestReleaseTicks = releaseTime;
                                }
                            }
                        }
                    }

                    if (latestReleaseTicks > long.MinValue)
                    {
                        latestReleaseTicks = Math.Max(latestReleaseTicks, SimClock);
                        operation.WaitForOperationFinishedEvent = true;
                        OperationFinishedEvent ofe = new(latestReleaseTicks, latestReleaseTicks, operation);
                        AddEvent(ofe);
                    }
                }
            }
        }
    }

    /// <summary>
    /// For the specified operation, if necessary, add event HoldUntilEvent.
    /// </summary>
    /// <param name="operation"></param>
    private void AddOnholdEvent(BaseOperation a_operation)
    {
        long holdUntilTicks;
        HoldEnum holdType = a_operation.GetLatestHoldTicks(out holdUntilTicks);

        if (holdUntilTicks > Clock && holdUntilTicks >= SimClock)
        {
            #if TEST
                if (a_operation.WaitForHoldUntilEvent)
                {
                    System.Diagnostics.Trace.WriteLine("DEBUG Hold error. Handled though, but you should investigate this.");
                }
            #endif

            HoldUntilEvent holdUntilEvent = new (holdUntilTicks, a_operation, holdType);
            AddEvent(holdUntilEvent);
            a_operation.WaitForHoldUntilEvent = true;
        }
    }

    /// <summary>
    /// For the specified operation, if necessary, add event FinishedPredecessorMOsAvailableEvent.
    /// </summary>
    /// <param name="sucOp">The operation to add an event for.</param>
    private void AddFinishedPredecessorMOsAvailableEvent(BaseOperation a_sucOp)
    {
        if (!a_sucOp.WaitForFinishedPredecessorMOsAvailableEvent)
        {
            long maxReleaseTicks = a_sucOp.FinishedPredecessorMOReleaseInfoManager.MaximumReleaseTicks;

            if (maxReleaseTicks > Clock)
            {
                FinishedPredecessorMOsAvailableEvent availEvent = new (maxReleaseTicks, a_sucOp);
                AddEvent(availEvent);
                a_sucOp.WaitForFinishedPredecessorMOsAvailableEvent = true;
            }
        }
    }

    /// <summary>
    /// Add manufacturing order release events for each Manufacturing Order marked by UnscheduledMOMarker.
    /// Don't call this function for optimize type of simulations.
    /// </summary>
    /// <param name="minimumReleaseDate">The clock or the new clock time in the case of clock advance.</param>
    private void AddMOReleaseEventsHoldsAndJobHoldsWhereUnscheduledMOMarkerIsTrue(long a_simClock, long a_minimumReleaseDate)
    {
        SimulationTimePoint sst = new (a_minimumReleaseDate);

        for (int jobManagerI = 0; jobManagerI < m_jobManager.Count; jobManagerI++)
        {
            Job j = m_jobManager[jobManagerI];
            ManufacturingOrderManager moCollection = j.ManufacturingOrders;
            bool jobUnscheduled = false;

            for (int moI = 0; moI < moCollection.Count; moI++)
            {
                ManufacturingOrder mo = moCollection[moI];
                if (mo.UnscheduledMOMarker)
                {
                    AddManufacturingOrderReleaseEvent(a_simClock, mo, sst, null);
                    AddManufacturingOrderHoldReleasedEvent(mo, a_minimumReleaseDate);
                    jobUnscheduled = true;
                }
            }

            if (jobUnscheduled)
            {
                AddJobHoldReleaseEvent(m_jobManager[jobManagerI], a_minimumReleaseDate);
            }
        }
    }

    #region AddOutterUnscheduledActivities(): Add unscheduled leafish activities to the set of activities that are to be rescheduled.
    /// <summary>
    /// Add unscheduled leafish activities to the set of activities that are to be rescheduled.
    /// </summary>
    /// <param name="a_mo">The MO whose activities to get.</param>
    /// <param name="a_ap">The MOs path activities to get.</param>
    /// <param name="a_rescheduleActivities">This should already be clear. The outter unscheduled activites of the path are stored in this set.</param>
    private void AddOutterUnscheduledActivities(ManufacturingOrder a_mo, AlternatePath a_ap, ActivitiesCollection a_rescheduleActivities)
    {
        AlternatePath.NodeCollection leaves = a_ap.Leaves;

        for (int pathNodeCollectionI = 0; pathNodeCollectionI < leaves.Count; ++pathNodeCollectionI)
        {
            InternalOperation io = leaves[pathNodeCollectionI].Operation as InternalOperation;

            if (io != null)
            {
                if (io.IsFinishedOrOmitted)
                {
                    AddOutterUnscheduledActivitiesSuccessorHelper(io, a_rescheduleActivities);
                }
                else
                {
                    AddNonFinishedActivitiesToActivitiesCollection(io, a_rescheduleActivities);
                    for (int sucI = 0; sucI < io.Successors.Count; ++sucI)
                    {
                        InternalOperation sucIO = (InternalOperation)io.Successors[sucI].Successor.Operation;
                        if (sucIO.IsFinishedOrOmitted)
                        {
                            AddOutterUnscheduledActivitiesSuccessorHelper(sucIO, a_rescheduleActivities);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Help dig into finished operations so their successors can be added to the list of unfinished operations.
    /// This was written to assist AddOutterUnscheduledActivities().
    /// </summary>
    /// <param name="operation">A finished operation.</param>
    private void AddOutterUnscheduledActivitiesSuccessorHelper(InternalOperation a_operation, ActivitiesCollection a_rescheduleActivities)
    {
        AlternatePath.Node apNode = a_operation.AlternatePathNode;
        if (a_operation.Finished && a_operation.IsNotOmitted)
        {
            long finishTicks = a_operation.CalcEndOfResourceTransferTimeTicks();
            long maxClock = Math.Max(Clock, SimClock);
            finishTicks = Math.Max(maxClock, finishTicks);
            for (int sucI = 0; sucI < apNode.Successors.Count; ++sucI)
            {
                AlternatePath.Association asn = apNode.Successors[sucI];

                //This association has been processed. The finished event from this predecessor will not trigger a release event
                asn.PredecessorReadyEventScheduled = true;

                if (asn.Successor.Predecessors.Count > 1)
                {
                    //This successor has multiple predecessors. It's possible that they are not all finished.
                    //If we release the successor here when one of the predecessors is not finished, the successor will schedule too early.
                    //Check if all are finished.
                    //  If not, don't release, the other predecessor will release it.
                    //  If so, then we will release the successor here.
                    bool allPredsFinished = true;
                    foreach (AlternatePath.Association successorPredecessor in asn.Successor.Predecessors)
                    {
                        if (!successorPredecessor.Predecessor.Operation.Finished)
                        {
                            allPredsFinished = false;
                            break;
                        }
                    }

                    if (!allPredsFinished)
                    {
                        //There is a predecessor that is not finished, skip releasing the successor here.
                        return;
                    }
                }

                PredecessorOperationAvailableEvent evt = new (finishTicks, asn, finishTicks);
                AddEvent(evt);
            }
        }

        for (int successorI = 0; successorI < a_operation.Successors.Count; ++successorI)
        {
            InternalOperation successorOperation = a_operation.Successors[successorI].Successor.Operation as InternalOperation;

            if (successorOperation != null)
            {
                if (successorOperation.IsFinishedOrOmitted)
                {
                    AddOutterUnscheduledActivitiesSuccessorHelper(successorOperation, a_rescheduleActivities);
                }
                else
                {
                    AddNonFinishedActivitiesToActivitiesCollection(successorOperation, a_rescheduleActivities);
                }
            }
        }
    }

    /// <summary>
    /// Add the non finished activities in the operation to the activities collection.
    /// This was written to assist AddOutterUnscheduledActivities().
    /// </summary>
    /// <param name="operation">An operation that has not been finished.</param>
    /// <param name="rescheduleActivities">A set to which non finished activities are made members of.</param>
    private void AddNonFinishedActivitiesToActivitiesCollection(InternalOperation a_operation, ActivitiesCollection a_rescheduleActivities)
    {
        for (int activitiesI = 0; activitiesI < a_operation.Activities.Count; ++activitiesI)
        {
            InternalActivity activity = a_operation.Activities.GetByIndex(activitiesI);
            if (activity.ProductionStatus != InternalActivityDefs.productionStatuses.Finished)
            {
                if (!activity.ActivityAddedToUnscheduledActivitiesSet)
                {
                    if (activity.SetupWaitForAnchorSetFlag())
                    {
                        long anchorDateTicksAdjustedForSimClock = Math.Max(activity.AnchorDateTicks, SimClock);
                        AddAnchorReleaseEvent(activity, anchorDateTicksAdjustedForSimClock);
                    }

                    a_rescheduleActivities.Add(activity);
                    activity.ActivityAddedToUnscheduledActivitiesSet = true;
                }
            }
        }
    }
    #endregion AddOutterUnscheduledActivities(): Add unscheduled leafish activities to the set of activities that are to be rescheduled.
    #endregion Common Helpers
    #endregion Actions

    #region Other Actions: Other types of actions may require that the schedule be adjusted. For instance if an MO is deleted setup times may require an adjustments.
    internal void Touch(ScenarioBaseT a_t)
    {
        if (a_t is ScenarioTouchT)
        {
            ScenarioTouchT touchT = a_t as ScenarioTouchT;
            if (touchT.SdNbrOfSimulations != 0 && touchT.SdNbrOfSimulations < NbrOfSimulations)
            {
                //A simulation has been performed since this transmission was created. No need to process.
                return;
            }
        }

        TimeAdjustment(a_t);
    }

    internal void TimeAdjustment(ScenarioBaseT a_scenarioT)
    {
        if (!ScenarioOnlineMode.Online)
        {
            return;
        }

        TestSleep();

        SimulationType simType = SimulationType.TimeAdjustment;

        try
        {
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_scenarioT, SimulationProgress.Status.Initializing, m_nbrOfSimulationsSinceStartup);
            PT.Common.Testing.Timing ts = CreateTiming("TimeAdjustment");

            #if TEST
                SimDebugSetup();
            #endif
            ScenarioDataChanges dataChanges = new ();

            CreateActiveResourceList(out MainResourceSet availableResources);
            SimulationInitializationAll(availableResources, a_scenarioT, simType, null);

            ResourceActivitySets sequentialResourceActivities = new (availableResources);
            UnscheduleInProcessActivitiesAndAddInProcessReleaseEvents(true);
            SimulationTimePoint sst = new (Clock);
            UnscheduleActivities(availableResources, sst, new SimulationTimePoint(EndOfPlanningHorizon), sequentialResourceActivities, simType, 0);
            SimulationResourceInitialization(new SimulationResourceDispatcherUsageArgs(Clock, OptimizeSettings.dispatcherSources.NormalRules));
            //*LRH*STATUS
            Simulate(Clock, sequentialResourceActivities, simType, a_scenarioT);

            StopTiming(ts, false);


            m_signals.Clear();

            #if TEST
                TestSchedule("Time Adjustment");
            #endif
            SimulationActionComplete();
            m_simulationProgress.PostSimulationWorkComplete();
            CheckForRequiredAdditionalSimulation(new ScenarioDataChanges()); //Time adjustments don't handle data changes. There shouldn't be any
        }
        catch (SimulationValidationException e)
        {
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_scenarioT, SimulationProgress.Status.Terminated, m_nbrOfSimulationsSinceStartup);
            FireSimulationValidationFailureEvent(e, m_activeOptimizeSettingsT);
            throw;
        }
        catch (Exception e)
        {
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_scenarioT, SimulationProgress.Status.Exception, m_nbrOfSimulationsSinceStartup);
            throw;
        }

        TestSleep();
    }

    private long DetermineTimeAdjustmentStartDate(MainResourceSet a_availableResources, long a_earliestTimeOfAdjustment)
    {
        long adjustedStartDate = a_earliestTimeOfAdjustment;

        for (int availableResourcesI = 0; availableResourcesI < a_availableResources.Count; ++availableResourcesI)
        {
            if (a_availableResources[availableResourcesI] is Resource)
            {
                Resource resource = (Resource)a_availableResources[availableResourcesI];
                ResourceBlockList blocks = resource.Blocks;
                ResourceBlockList.Node node = blocks.First;

                while (node != null)
                {
                    ResourceBlock block = node.Data;
                    if (block.Contains(a_earliestTimeOfAdjustment))
                    {
                        if (block.StartTicks < adjustedStartDate)
                        {
                            adjustedStartDate = block.StartTicks;
                        }
                    }

                    node = node.Next;
                }
            }
        }

        return adjustedStartDate;
    }
    #endregion Other Actions

    #region Simulation Algorithm
    private long m_endOfPlanningHorizon;

    /// <summary>
    /// This equals the DateTime.Ticks of the end of planning horizon.
    /// </summary>
    internal long EndOfPlanningHorizon => m_endOfPlanningHorizon;

    [Common.Attributes.DebugLogging(Common.Attributes.EDebugLoggingType.None)]
    private CalendarResourceAvailableEventSetBase m_availableResourceEventsSet;

    /// Backlog 528. Need to change this.
    internal CalendarResourceAvailableEventList GetAvailableResourceEventsSet()
    {
        return m_availableResourceEventsSet.m_allAvailableResourceEvents;
    }

    private abstract class CalendarResourceAvailableEventSetBase
    {
        internal readonly CalendarResourceAvailableEventList m_allAvailableResourceEvents = new ();
        internal readonly CalendarResourceAvailableEventList m_availablePrimaryResourceEvents = new ();

        internal void Clear()
        {
            m_allAvailableResourceEvents.Clear();
            m_availablePrimaryResourceEvents.Clear();
            m_nextNode = null;
        }

        internal virtual void Add(ResourceAvailableEvent a_rae)
        {
            CalendarResourceAvailableEventList.Node availableNode = new (a_rae);
            m_allAvailableResourceEvents.Add(availableNode);

            CalendarResourceAvailableEventList.Node primaryNode = null;
            if (a_rae.Resource.UsedAsPrimaryResource)
            {
                primaryNode = new CalendarResourceAvailableEventList.Node(a_rae);
                m_availablePrimaryResourceEvents.Add(primaryNode);
            }

            a_rae.Resource.SetAvailabilityVars(availableNode, primaryNode);
        }

        internal void Remove(InternalResource a_res)
        {
            //
            // Handle the full set of available resources.
            //
            if (a_res.AvailableNode != null) // This null check is necessary because it's possible the same resource has been used to fulfill multiple resource requirements and was removed from m_allAvailableResourceEvents when one of the other requirements was scheduled.
            {
                m_allAvailableResourceEvents.Remove(a_res.AvailableNode);
            }

            //
            //Handle the primary available resources.
            //
            if (a_res.AvailablePrimaryNode != null)
            {
                // If you're removing the NextNode, advance it first.
                if (a_res.AvailablePrimaryNode == m_nextNode)
                {
                    m_nextNode = m_nextNode.Next;
                }

                m_availablePrimaryResourceEvents.Remove(a_res.AvailablePrimaryNode);
            }

            a_res.ClearAvailabilityVars();
        }

        internal abstract void InitForScheduling();

        private CalendarResourceAvailableEventList.Node m_nextNode;

        /// <summary>
        /// This value can be null. Used in combination with InitNextNodeAsSecondFromFirst
        /// Used to track the next node when traversing the list.
        /// Removals from the list are automatically accounted for.
        /// For instance if the list contains elements 1,2,3 and we start with element 1,
        /// then element 2 is initially the next node. But if element 2 is removed during the
        /// traversal element 3 is made the next node.
        /// </summary>
        internal CalendarResourceAvailableEventList.Node NextNode => m_nextNode;

        /// <summary>
        /// Set the NextNode property to the passed in node's next node.
        /// </summary>
        internal void InitNextNodeAsNodeAfter(CalendarResourceAvailableEventList.Node a_node)
        {
            if (a_node != null && a_node.Next != null)
            {
                m_nextNode = a_node.Next;
            }
            else
            {
                m_nextNode = null;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} primary resource evetns; {1} all available resource events", m_availablePrimaryResourceEvents.Count, m_allAvailableResourceEvents.Count);
        }

        #region Testing; Various functions for testing.
        /// <summary>
        /// Whether the resource is available at the current simulation time.
        /// </summary>
        /// <param name="a_res"></param>
        /// <returns></returns>
        internal bool Contains(Resource a_res)
        {
            return Contains(a_res.Id);
        }

        /// <summary>
        /// Whether the resource is available at the current simulation time.
        /// </summary>
        /// <param name="a_resId"></param>
        /// <returns></returns>
        internal bool Contains(BaseId a_resId)
        {
            return Contains(a_resId.Value);
        }

        /// <summary>
        /// Whether the resource is available at the current simulation time.
        /// </summary>
        /// <param name="a_id"></param>
        /// <returns></returns>
        internal bool Contains(long a_id)
        {
            return m_allAvailableResourceEvents.Contains(a_id) || m_availablePrimaryResourceEvents.Contains(a_id);
        }
        #endregion
    }

    /// <summary>
    /// Used when there are multiple dispatchers.
    /// </summary>
    private sealed class CalendarResourceAvailableEventSetForNonComparableDispatching : CalendarResourceAvailableEventSetBase
    {
        internal override void Add(ResourceAvailableEvent a_rae)
        {
            base.Add(a_rae);
        }

        private int CompareBySimSort(CalendarResourceAvailableEventList.Node a_n1, CalendarResourceAvailableEventList.Node a_n2)
        {
            long n1Val = a_n1.Data.Resource.m_v_simSort;
            long n2Val = a_n2.Data.Resource.m_v_simSort;

            if (n1Val < n2Val)
            {
                return -1;
            }

            if (n1Val > n2Val)
            {
                return 1;
            }

            return 0;
        }

        internal override void InitForScheduling()
        {
            //********************************************************************************************************
            // If no resources were added or removed from this set then you don't need to perform this step each time.
            //********************************************************************************************************

            List<CalendarResourceAvailableEventList.Node> sortedList = new ();

            CalendarResourceAvailableEventList.Node curNode = m_allAvailableResourceEvents.First;
            while (curNode != null)
            {
                if (!curNode.Data.Cancelled)
                {
                    sortedList.Add(curNode);
                }

                curNode = curNode.Next;
            }

            Clear();

            sortedList.Sort(CompareBySimSort);

            // Re-add the nodes to the lists in the order they're sorted in.
            // The list nodes are reused so the Nodes don't need to be reallocated from the heap.
            for (int i = 0; i < sortedList.Count; ++i)
            {
                CalendarResourceAvailableEventList.Node node = sortedList[i];

                m_allAvailableResourceEvents.Add(node); // node was from this list, so you can add it.

                if (node.Data.Resource.UsedAsPrimaryResource)
                {
                    m_availablePrimaryResourceEvents.Add(node.Data.Resource.AvailablePrimaryNode); // Re-add the node from this list.
                }
            }
        }
    }

    /// <summary>
    /// Used when there is a single dispatcher.
    /// </summary>
    private sealed class CalendarResourceAvailableEventSetForAllActivitiesComparableDispatching : CalendarResourceAvailableEventSetBase
    {
        internal override void InitForScheduling() { }
    }

    private bool m_withinPlanningHorizon;

    private bool WithinPlanningHorizon => m_withinPlanningHorizon;

    private class SimilarityComparer : IComparer<InternalActivity>
    {
        #region IComparer<BaseActivity> Members
        public int Compare(InternalActivity a_x, InternalActivity a_y)
        {
            return a_x.SimilarityComparison(a_y);
        }
        #endregion
    }

    private bool m_similarityValuesSet;

    private void InitSimilarityIds()
    {
        //Note: In v12 ScheudlingOptions class was removed because it was not used by any extensions.
        m_similarityValuesSet = false;
        return;

        PT.Common.Testing.Timing ts = CreateTiming(".InitSimilarityIds");

        #if DEBUG
        decimal ttlActCnt = 0;
        #endif
        m_similarityValuesSet = true;
        SortedDictionary<InternalActivity, long> actSimilarityValues = new (new SimilarityComparer());
        long curVal = 0;

        for (int jI = 0; jI < JobManager.Count; ++jI)
        {
            Job j = JobManager[jI];

            for (int mI = 0; mI < j.ManufacturingOrders.Count; ++mI)
            {
                ManufacturingOrder mo = j.ManufacturingOrders[mI];

                for (int oI = 0; oI < mo.OperationManager.Count; ++oI)
                {
                    InternalOperation bo = (InternalOperation)mo.OperationManager.GetByIndex(oI);

                    for (int aI = 0; aI < bo.Activities.Count; ++aI)
                    {
                        #if DEBUG
                        ++ttlActCnt;
                        #endif
                        InternalActivity ia = bo.Activities.GetByIndex(aI);

                        if (actSimilarityValues.ContainsKey(ia))
                        {
                            ia.m_similarityValue = actSimilarityValues[ia];
                        }
                        else
                        {
                            actSimilarityValues.Add(ia, ++curVal);
                            ia.m_similarityValue = curVal;
                        }
                    }
                }
            }
        }

        #if DEBUG
        decimal p = actSimilarityValues.Count / ttlActCnt;
        #endif

        StopTiming(ts, false);
    }

    /// <summary>
    /// Each time an activity is scheduled, this value is incremented.
    /// The first activity scheduled will be assigned 0, the second will be assigned 1, and so on.
    /// </summary>
    private long m_currentScheduledActivityNbr;

    private bool m_dataLimitReached = false;

    #if TEST
        void SimulationInitialization2()
        {
            SimulationInitialization(mt_availableResources, mt_transmission, mt_simulationType, mt_processPathReleaseDelegate);
        }

        MainResourceSet mt_availableResources;
        ScenarioBaseT mt_transmission;
        SimulationType mt_simulationType;
        ProcessPathReleaseDelegate mt_processPathReleaseDelegate;
    #endif

    /// <summary>
    /// Calls all Simulation Initialization methods. Use this version if you don't need to break up the initialization.
    /// In this version of SimulationInitialiation all scheduled jobs will be rescheduled.
    /// </summary>
    private void SimulationInitializationAll(MainResourceSet a_availableResources, ScenarioBaseT a_transmission, SimulationType a_simulationType, ProcessPathReleaseDelegate a_processPathReleaseDelegate, SimDetailsGroupings a_simDetails = null)
    {
        List<Job> jobsToSchedule = JobManager.GetScheduledJobs();
        SimulationInitialization1();
        SimulationInitialization2(a_availableResources, a_transmission, a_simulationType, a_processPathReleaseDelegate, a_simDetails);
        SimulationInitialization3(a_availableResources, jobsToSchedule, a_simulationType);
        SimulationInitialization4_configUnschedAnchoredForAnchor(a_simulationType);
    }

    /// <summary>
    /// Perform the most basic initializations prior to each simulation.
    /// In this step of initialization, only the simulation clock is reset.
    /// </summary>
    private void SimulationInitialization1()
    {
        SimClock = 0;
    }

    // [USAGE_CODE] m_usageFeatureActive: whether there are any jobs whose UsageStart!=Setup.
    /// <summary>
    /// Whether the usage feature is active.
    /// </summary>
    private bool m_usageFeatureActive;

    // [USAGE_CODE] InitUsagesWithoutSetup: Initializes m_usagesWithoutSetup when when called and returns whether the feature is active.
    /// <summary>
    /// Call during SimulationInitialization to activate the usage feature if necessary.
    /// The feature is automatically turned if any RR UsageStart!=Setup
    /// </summary>
    internal bool UsageFeatureActive()
    {
        for (int j = 0; j < JobManager.Count; ++j)
        {
            Job job = JobManager[j];
            for (int m = 0; m < job.ManufacturingOrders.Count; ++m)
            {
                ManufacturingOrder mo = job.ManufacturingOrders[m];
                for (int o = 0; o < mo.OperationManager.Count; ++o)
                {
                    InternalOperation op = (InternalOperation)mo.OperationManager.GetByIndex(o);
                    for (int rrI = 0; rrI < op.ResourceRequirements.Count; ++rrI)
                    {
                        ResourceRequirement rr = op.ResourceRequirements.GetByIndex(rrI);
                        if (rr.UsageStart != MainResourceDefs.usageEnum.Setup)
                        {
                            m_usageFeatureActive = true;
                            return m_usageFeatureActive;
                        }
                    }
                }
            }
        }

        m_usageFeatureActive = false;
        return m_usageFeatureActive;
    }

    /// <summary>
    /// This function must be called prior to the start of a simulation. This version performs the bulk of the initialization.
    /// </summary>
    private void SimulationInitialization2(MainResourceSet a_availableResources, ScenarioBaseT a_transmission, SimulationType a_simulationType, ProcessPathReleaseDelegate a_processPathReleaseDelegate, SimDetailsGroupings a_simDetails = null)
    {
        InitNonSerializedSimulationMembers();

        //This is only used to store the caches to they can be enabled after a simulation
        //Clear this first before other object's caches get added.
        m_calculatedValueCaches.Clear();

        #if TEST
            mt_availableResources = a_availableResources;
            mt_transmission = a_transmission;
            mt_simulationType = a_simulationType;
            mt_processPathReleaseDelegate = a_processPathReleaseDelegate;
        #endif

        // !ALTERNATE_PATH!; m_processPathReleaseDelegate is set here.
        m_processPathReleaseDelegate = a_processPathReleaseDelegate;

        #if TEST
            if (!System.IO.Directory.Exists(z_c_tmpDir))
            {
                System.IO.Directory.CreateDirectory(z_c_tmpDir);
            }

            z_scheduledActivityFileName = CreateLogName(z_c_tmpDir, "Scheduled");
        #endif

        #if TEST
            if (!System.IO.Directory.Exists(z_c_tmpDir))
            {
                System.IO.Directory.CreateDirectory(z_c_tmpDir);
            }

            if (!System.IO.Directory.Exists(z_c_tmpEvents))
            {
                System.IO.Directory.CreateDirectory(z_c_tmpEvents);
            }

            z_c_tempEventsFile = CreateLogName(z_c_tmpEvents, z_c_eventsFilePrefix);
        #endif

        InitSimilarityIds();

        m_endOfPlanningHorizon = GetPlanningHorizonEndTicks();
        ItemManager.ResetSimulationStateVariables();

        m_dispatcherDefinitionManager.SimulationInitialization(this);
        m_jobManager.ResetSimulationStateVariables(this);
        PlantManager.ResetSimulationStateVariables(Clock, ScenarioOptions);
        WarehouseManager.ResetSimulationStateVariables(Clock, this);
        SalesOrderManager.ResetSimulationStateVariables();
        TransferOrderManager.ResetSimulationStateVariables();
        m_compatibilityCodeTableManager.ResetSimulationStateVariables();
        m_resourceConnectorManager.ResetSimulationStateVariables();

        m_events.Clear();
        m_events.InitialInsertionModeStart();

        List<QtyToStockEvent> futureOnHandLotEvents = new ();
        WarehouseManager.SimulationInitialization(Clock, ref futureOnHandLotEvents);

        m_events.AddEvents(futureOnHandLotEvents);

        // Initialization the PurchaseToStocks
        AddPurchaseToStockEvents();

        AddSalesOrderLineDistributionEvents();

        AddInventoryLeadTimeEvents();

        AddShelfLifeExpirationEvents();

        //MRP Events
        AddInventorySafetyStockAdjustments();

        m_extensionController.SimulationInitialization(this, a_simulationType, a_transmission);

        // Initialize the jobs.
        m_jobManager.SimulationInitialization(m_plantManager, m_productRuleManager, m_extensionController, this);

        // This is set here in case its value is accessed prior to the start of the simulation.
        // It is also set at the start of each stage simulate.

        //m_mobsByGroup = null;
        //m_mobsByGroupBeforeOptimizeStartDate = null;


        m_unscheduledActivitiesBeingMovedCount = 0;
        m_moveActivitySequence = null;
        m_nextSequencedMoveActivityToSchedule = 0;

        // [BATCH_CODE]
        m_resourcesReserved = false;

        m_availableResources = a_availableResources;

        #if TEST
            _deScheduledActivityCount = 0;
        #endif
        TestResSimSeqNbrs(a_availableResources); // Make this debug only after testing at customer sites.

        #if TEST
            _deScheduledActivityCount = 0;
            _deAttemptToScheduleActivity_Nbr = 0;
            _deAlreadyScheduled = 0;
            _deSchedulabilityCustomization = 0;
            _deSchedulableResult = 0;
            _deFindMaterial = 0;
            _deNonPrimaryRR = 0;
            _deAllResources = PlantManager.GetResourceList();

            _nbrOfJobs = 0;
            _nbrOfMos = 0;
            _nbrOfOps = 0;
            _nbrOfActs = 0;

            // Add the number of activities to schedule.
            for (int j = 0; j < JobManager.Count; ++j)
            {
                Job job = JobManager[j];
                if (!job.Template)
                {
                    ++_nbrOfJobs;
                    for (int m = 0; m < job.ManufacturingOrders.Count; ++m)
                    {
                        ManufacturingOrder mo = job.ManufacturingOrders[m];
                        ++_nbrOfMos;
                        for (int o = 0; o < mo.OperationManager.Count; ++o)
                        {
                            ResourceOperation op = (ResourceOperation)mo.OperationManager.GetByIndex(o);
                            ++_nbrOfOps;
                            for (int a = 0; a < op.Activities.Count; ++a)
                            {
                                InternalActivity act = op.Activities.GetByIndex(a);
                                if (!act.Finished && op.Omitted == BaseOperationDefs.omitStatuses.NotOmitted)
                                {
                                    ++_nbrOfActs;
                                }
                            }
                        }
                    }
                }
            }
        #endif

        // This part of stage initialization must be done prior to the rest of stage initialization because during an optimize 
        // AddEvent() is called and it depends on this variable's value as having been reset.
        StageNbr = 0;

        // Except in the case of an optimize simulation, it's presumed that all the scheduled MOs will be rescheduled.
        // All scheduled MOs are marked that they will be scheduled during the simulation.
        if (a_simulationType != SimulationType.Optimize)
        {
            for (int jobI = 0; jobI < JobManager.Count; ++jobI)
            {
                Job job = JobManager[jobI];

                for (int moI = 0; moI < job.ManufacturingOrders.Count; ++moI)
                {
                    ManufacturingOrder mo = job.ManufacturingOrders[moI];
                    if (mo.Scheduled)
                    {
                        SetToBeScheduledFlags(job, mo);
                    }
                }
            }
        }

        m_simDetailsGroupings = a_simDetails;

        // [BATCH_CODE]
        // Save how the batches were configured in the prior simulation so they can be restored during the 
        // current simulation if necessary.
        m_batchManager.SetLastSimBatchesOfActivities();
        m_lastSimBatches = m_batchManager;
        m_attemptToScheduleToNonConnectedRes.Clear();
        m_moveTicksState = MoveReleaseState.NotMove;

        InitResourceUsedAsPrimary();

        m_blockReservations.Clear();

        m_updatedSubJobNeedDates = false;

        AddTransferOrderDistributionEvents();

        m_moveIntersectors.Clear();

        #if DEBUG
        z_supressActivitiesOnDispatcherException = false;
        #endif
        // Initialization for eligible lots.
        m_usedAsEligibleLotLotCodes.Clear();
        SalesOrderManager.AddEligibleLotCodes(m_usedAsEligibleLotLotCodes);
        TransferOrderManager.AddEligibleLotCodes(m_usedAsEligibleLotLotCodes);
        JobManager.AddEligibleLotCodes(m_usedAsEligibleLotLotCodes);
        ReleaseOnHandEligibleInventoryLots();

        m_materialConstrainedActivities.Clear();
        m_resRvns.SimulationInitialization();

        InitializeActualsHistory();
    }

    internal void InitializeActualsHistory()
    {
        //TODO: Clear

        if (ScenarioOptions.TrackActualsAgeLimit.Ticks > 0) //if not then disable this feature effectively.
        {
            DateTime startOfHistory = ClockDate.Subtract(ScenarioOptions.TrackActualsAgeLimit);

            foreach (Job job in JobManager.JobEnumerator)
            {
                foreach (InternalActivity activity in job.GetActivities())
                {
                    if (activity.ActualResourcesUsed != null &&
                        activity.ProductionStatus == InternalActivityDefs.productionStatuses.Finished)
                    {
                        DateTime reportedStartDate = DateTime.MinValue;
                        DateTime reportedEndDate = DateTime.MinValue;
                        if (activity.ReportedStartDateSet)
                        {
                            reportedStartDate = activity.ReportedStartDate;
                            if (!activity.ReportedFinishDateSet && activity.ReportedSetupProcessingAndPostProcessing > TimeSpan.Zero)
                            {
                                reportedEndDate = activity.ReportedStartDate.Add(activity.ReportedSetupProcessingAndPostProcessing);
                            }
                        }

                        if (activity.ReportedFinishDateSet)
                        {
                            reportedEndDate = activity.ReportedFinishDate;
                            if (!activity.ReportedStartDateSet && activity.ReportedSetupProcessingAndPostProcessing > TimeSpan.Zero)
                            {
                                reportedStartDate = activity.ReportedFinishDate.Subtract(activity.ReportedSetupProcessingAndPostProcessing);
                            }
                        }

                        if (reportedStartDate != DateTime.MinValue &&
                            reportedEndDate != DateTime.MinValue &&
                            reportedStartDate < reportedEndDate &&
                            reportedEndDate >= startOfHistory)
                        {
                            ResourceKeyList.Node resNode = activity.ActualResourcesUsed.First;
                            while (resNode != null)
                            {
                                if (PlantManager.GetResource(resNode.Data) is Resource resource)
                                {
                                    resource.AddCleanoutHistory(
                                        reportedStartDate,
                                        reportedEndDate,
                                        activity);
                                }

                                resNode = resNode.Next;
                            }
                        }
                    }
                }
            }
        }
    }

    private static void SetToBeScheduledFlags(Job job, ManufacturingOrder mo)
    {
        job.ToBeScheduled = true;
        mo.ToBeScheduled = true;
    }

    /// <summary>
    /// Add ship events for all open transfer orders.
    /// Receive events aren't added until the ship quantity is known when the ship events are processed.
    /// </summary>
    private void AddTransferOrderDistributionEvents()
    {
        for (int toI = 0; toI < TransferOrderManager.Count; ++toI)
        {
            TransferOrder to = TransferOrderManager.GetByIndex(toI);

            if (!to.Closed)
            {
                for (int dI = 0; dI < to.Distributions.Count; ++dI)
                {
                    TransferOrderDistribution dist = to.Distributions.GetByIndex(dI);
                    decimal remainingQty = dist.QtyOpenToShip;

                    if (!dist.Closed && remainingQty > 0)
                    {
                        TransferOrderShipEvent evt = new (dist.ScheduledShipDateTicks, dist);
                        AddEvent(evt);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Set UsedAsPrimary of each resource to true if it's used by any job as a primary resource.
    /// It's possible to decrease the number of primaries further by only examining the job
    /// that will be scheduled by the simulation.
    /// </summary>
    private void InitResourceUsedAsPrimary()
    {
        long tstPrimaryCount = 0;
        for (int jobI = 0; jobI < JobManager.Count; ++jobI)
        {
            Job job = JobManager[jobI];
            for (int moI = 0; moI < job.ManufacturingOrders.Count; ++moI)
            {
                ManufacturingOrder mo = job.ManufacturingOrders[moI];
                for (int opI = 0; opI < mo.OperationManager.Count; ++opI)
                {
                    InternalOperation op = (InternalOperation)mo.OperationManager.GetByIndex(opI);
                    if (op.ResourceRequirements.PrimaryResourceRequirementIndex >= 0 && op.AlternatePathNode != null)
                    {
                        if (op.ResourceRequirements.PrimaryResourceRequirementIndex <= op.AlternatePathNode.ResReqsMasterEligibilitySet.Count - 1)
                        {
                            PlantResourceEligibilitySet pres = null;
                            pres = op.AlternatePathNode.ResReqsMasterEligibilitySet[op.ResourceRequirements.PrimaryResourceRequirementIndex];

                            SortedDictionary<BaseId, EligibleResourceSet>.Enumerator etr = pres.GetEnumerator();
                            while (etr.MoveNext())
                            {
                                EligibleResourceSet ers = etr.Current.Value;
                                for (int resI = 0; resI < ers.Count; ++resI)
                                {
                                    InternalResource res = ers[resI];
                                    res.UsedAsPrimaryResource = true;
                                    ++tstPrimaryCount;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Performs additional initialization that needs to be delayed. For instance, it's not possible to always know all the jobs that should be scheduled
    /// until after the SimulationInitialziation1() has been executed. So initialization that depends on knowing the jobs to schedule has been moved to this
    /// function which will be called some time after SimulationInitialziation1().
    /// </summary>
    /// <param name="a_availableResources">The resources that will take part in the simulation.</param>
    /// <param name="a_jobsToSchedule">ATh ejobs that will be scheduled.</param>
    private void SimulationInitialization3(MainResourceSet a_availableResources, List<Job> a_jobsToSchedule, SimulationType a_simType)
    {
        SimulationInitializationOfStages(a_availableResources, a_jobsToSchedule);
    }

    private void SimulationInitialization4_configUnschedAnchoredForAnchor(SimulationType a_simType)
    {
        ConfigureUnscheduledAnchoredActivitiesForAnchor(a_simType);
    }

    /// <summary>
    /// Initialize unscheduled achored activities for anchoring. Because they're unscheduled, they end up bypassing the nomal anchor setup code for scheduled activities.
    /// Related task: 2089.
    /// Unscheduled activities that have their anchor values set will use the anchor when they're scheduled.
    /// </summary>
    /// <param name="a_simType">Current Simulation Type</param>
    private void ConfigureUnscheduledAnchoredActivitiesForAnchor(SimulationType a_simType)
    {
        for (int JobI = 0; JobI < m_jobManager.Count; JobI++)
        {
            Job job = m_jobManager[JobI];
            //Restrict Simulation
            if (a_simType != SimulationType.Optimize || (!job.DoNotSchedule_PlantNotIncludedInOptimize && !(job.ScheduledStatus == JobDefs.scheduledStatuses.Unscheduled && m_activeOptimizeSettings.ExcludeUnscheduledJobs)))
            {
                //Restrict Job
                if (!job.Cancelled &&
                    !job.Finished &&
                    job.ScheduledStatus != JobDefs.scheduledStatuses.Template &&
                    job.Anchored != anchoredTypes.Free //This method does nothing if the activities are not anchored
                    &&
                    job.ScheduledStatus != JobDefs.scheduledStatuses.Excluded &&
                    job.IsSchedulable())
                {
                    ManufacturingOrderManager mosToOptimize = job.ManufacturingOrders;

                    for (int moI = 0; moI < mosToOptimize.Count; moI++)
                    {
                        ManufacturingOrder mo = mosToOptimize[moI];
                        //Restrict MO
                        if (!mo.Finished && mo.Schedulable)
                        {
                            for (int pathI = 0; pathI < mo.AlternatePaths.Count; ++pathI)
                            {
                                AlternatePath path = mo.AlternatePaths[pathI];

                                if (path.AdjustedPlantResourceEligibilitySets_IsSatisfiable())
                                {
                                    for (int nodeI = 0; nodeI < path.AlternateNodeSortedList.Count; ++nodeI)
                                    {
                                        AlternatePath.Node node = path.AlternateNodeSortedList.Values[nodeI];
                                        InternalOperation iOp = node.Operation as InternalOperation;
                                        if (iOp != null)
                                        {
                                            //Restrict Operation
                                            if (!iOp.IsFinishedOrOmitted)
                                            {
                                                for (int actI = 0; actI < iOp.Activities.Count; actI++)
                                                {
                                                    InternalActivity act = iOp.Activities.GetByIndex(actI);
                                                    //Restrict Activity
                                                    if (!act.Scheduled && !act.Finished && act.SetupWaitForAnchorSetFlag())
                                                    {
                                                        AddAnchorReleaseEvent(act, act.AnchorDateTicks);

                                                        act.SuppressReleaseDateAdjustments = true;
                                                        act.Operation.ManufacturingOrder.SuppressReleaseDateAdjustments = true;

                                                        iOp.UnscheduledAnchoredActivitiesHaveBeenSetupForAnchoring = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    #if DEBUG
    private void zCountBlocksAndBatches(out int blockCount, out int batchCount)
    {
        blockCount = 0;
        batchCount = 0;

        HashSet<BaseId> batchTest = new ();
        if (true)
        {
            List<Resource> ral = PlantManager.GetResourceArrayList();
            for (int i = 0; i < ral.Count; ++i)
            {
                Resource res = ral[i];
                blockCount += res.Blocks.Count;
                ResourceBlockList.Node current = res.Blocks.First;
                while (current != null)
                {
                    BaseId id = current.Data.Batch.Id;
                    if (!batchTest.Contains(id))
                    {
                        batchTest.Add(id);
                    }

                    current = current.Next;
                }
            }

            batchCount = batchTest.Count;
        }
    }
    #endif

    private void AddPurchaseToStockEvents()
    {
        for (int ptsI = 0; ptsI < m_purchaseToStockManager.Count; ++ptsI)
        {
            PurchaseToStock pts = m_purchaseToStockManager.GetByIndex(ptsI);
            PurchaseToStockEvent ptsEvt = new (pts.AvailableDateTicks, pts);
            AddEvent(ptsEvt);
        }
    }

    /// <summary>
    /// Add for each sales order line distribution, add a SalesOrderLineDistributionEvent; an attempt will be made at
    /// each's RequiredAvailableDateTicks to consume the material for the SOD.
    /// </summary>
    private void AddSalesOrderLineDistributionEvents()
    {
        List<SalesOrderLineDistribution> sods = SalesOrderManager.GetDistributionsList();

        foreach (SalesOrderLineDistribution sod in sods)
        {
            if (!sod.Closed && sod.QtyOpenToShip > 0)
            {
                SalesOrderLineDistributionEvent sode = new (sod.RequiredAvailableDateTicks, sod);
                AddEvent(sode);
            }
        }
    }

    /// <summary>
    /// Add an event for each inventories lead time. This will notify constraint MRs that the inventory is now available.
    /// </summary>
    private void AddInventoryLeadTimeEvents()
    {
        foreach (Warehouse warehouse in WarehouseManager)
        {
            foreach (Inventory inventory in warehouse.Inventories)
            {
                LeadTimeEvent lte = new (Clock + inventory.LeadTimeTicks, inventory.Item, warehouse);
                AddEvent(lte);
            }
        }
    }    
    
    /// <summary>
    /// Add an event for each inventory safety stock. This can be used by MRP or inventory visualizations
    /// </summary>
    private void AddInventorySafetyStockAdjustments()
    {
        foreach (Warehouse warehouse in WarehouseManager)
        {
            foreach (Inventory inventory in warehouse.Inventories)
            {
                if (inventory.SafetyStock > 0)
                {
                    inventory.AddSimulationAdjustment(new SafetyStockMrpAdjustment(inventory, Clock, -inventory.SafetyStock));
                }
            }
        }
    }    
    
    /// <summary>
    /// Add an event for each on-hand lot that will expire
    /// </summary>
    private void AddShelfLifeExpirationEvents()
    {
        foreach (Warehouse warehouse in WarehouseManager)
        {
            foreach (Inventory inventory in warehouse.Inventories)
            {
                foreach (Lot lot in inventory.Lots) //This should be only the on-hand lots that were not cleared
                {
                    if (lot.ShelfLifeData.Expirable)
                    {
                        long expirationTicks = lot.ShelfLifeData.ExpirationTicks;
                        if (expirationTicks > m_clock)
                        {
                            AddEvent(new ShelfLifeEvent(expirationTicks, lot));
                        }
                    }
                }
            }
        }
    }

    // [TANK_CODE]
    [Obsolete("Just use the code for BlockStartAndEndCalculated. The other enums are no longer possible")]
    private enum PredecessorReadyType
    {
        /// <summary>
        /// The start and end of the block were available when the block was created.
        /// </summary>
        BlockStartAndEndCalculated,

        /// <summary>
        /// The block start was calculated, but the block end is still unknown. This is the case with Tank resources.
        /// </summary>
        BlockStartCalculated,

        /// <summary>
        /// The block end was calculated. This is the case with Tank Resources.
        /// </summary>
        BlockEndCalculated
    }

    /// <summary>
    /// Add a predecessor available event to the event queue at the time this operation is ready.
    /// </summary>
    /// <param name="a_op">The operation that is ready.</param>
    /// <param name="a_minimumPredecessorReadyTime">The earliest time you are willing to release the predecessor operation.</param>
    /// <param name="a_blockEndCalculated"></param>
    private void AddPredecessorAvailableEvent(InternalOperation a_op, long a_minimumPredecessorReadyTime, PredecessorReadyType a_predOpReadyType)
    {
        for (int i = 0; i < a_op.AlternatePathNode.Successors.Count; ++i)
        {
            AlternatePath.Association apAssociation = a_op.AlternatePathNode.Successors[i];

            switch (a_predOpReadyType)
            {
                case PredecessorReadyType.BlockStartAndEndCalculated:

                    switch (apAssociation.OverlapType)
                    {
                        case InternalOperationDefs.overlapTypes.NoOverlap:
                            AddPredecessorAvailableEventAtOperationReadyTime(apAssociation, a_minimumPredecessorReadyTime);
                            break;

                        case InternalOperationDefs.overlapTypes.TransferQty:
                            AddPredecessorAvailableOverlappingFromEarliestStartDate(apAssociation, a_minimumPredecessorReadyTime);
                            break;

                        case InternalOperationDefs.overlapTypes.AtFirstTransfer:
                            AddPredecessorAvailableOverlappingAtFirstTransfer(apAssociation, a_minimumPredecessorReadyTime);
                            break;

                        case InternalOperationDefs.overlapTypes.TransferSpan:
                            AddPredecessorAvailableOverlappingFromTransferSpan(apAssociation);
                            break;

                        case InternalOperationDefs.overlapTypes.TransferSpanAfterSetup:
                            AddPredecessorAvailableOverlappingFromTransferSpanAfterSetup(apAssociation, a_minimumPredecessorReadyTime);
                            break;

                        case InternalOperationDefs.overlapTypes.PercentComplete:
                            AddPredecessorAvailableOverlappingAtPercentComplete(apAssociation, a_minimumPredecessorReadyTime);
                            break;

                        case InternalOperationDefs.overlapTypes.TransferSpanBeforeStartOfPredecessor:
                            AddPredecessorAvailableOverlappingFromTransferSpanBeforeStart(apAssociation);
                            break;
                    }

                    break;

                case PredecessorReadyType.BlockStartCalculated:
                    switch (apAssociation.OverlapType)
                    {
                        case InternalOperationDefs.overlapTypes.TransferSpan:
                            AddPredecessorAvailableOverlappingFromTransferSpan(apAssociation);
                            break;

                        case InternalOperationDefs.overlapTypes.TransferSpanAfterSetup:
                            AddPredecessorAvailableOverlappingFromTransferSpanAfterSetup(apAssociation, a_minimumPredecessorReadyTime);
                            break;

                        case InternalOperationDefs.overlapTypes.TransferSpanBeforeStartOfPredecessor:
                            AddPredecessorAvailableOverlappingFromTransferSpanBeforeStart(apAssociation);
                            break;
                    }

                    break;

                case PredecessorReadyType.BlockEndCalculated:
                    switch (apAssociation.OverlapType)
                    {
                        case InternalOperationDefs.overlapTypes.NoOverlap:
                            AddPredecessorAvailableEventAtOperationReadyTime(apAssociation, a_minimumPredecessorReadyTime);
                            break;

                        case InternalOperationDefs.overlapTypes.TransferQty:
                            AddPredecessorAvailableOverlappingFromEarliestStartDate(apAssociation, a_minimumPredecessorReadyTime);
                            break;

                        case InternalOperationDefs.overlapTypes.AtFirstTransfer:
                            AddPredecessorAvailableOverlappingAtFirstTransfer(apAssociation, a_minimumPredecessorReadyTime);
                            break;

                        case InternalOperationDefs.overlapTypes.TransferSpan:
                            AddPredecessorAvailableOverlappingFromTransferSpan(apAssociation);
                            break;

                        case InternalOperationDefs.overlapTypes.TransferSpanAfterSetup:
                            AddPredecessorAvailableOverlappingFromTransferSpanAfterSetup(apAssociation, a_minimumPredecessorReadyTime);
                            break;

                        case InternalOperationDefs.overlapTypes.PercentComplete:
                            AddPredecessorAvailableOverlappingAtPercentComplete(apAssociation, a_minimumPredecessorReadyTime);
                            break;

                        case InternalOperationDefs.overlapTypes.TransferSpanBeforeStartOfPredecessor:
                            AddPredecessorAvailableOverlappingFromTransferSpanBeforeStart(apAssociation);
                            break;
                    }

                    break;
            }
        }
    }

    private void AddPredecessorAvailableEventAtOperationReadyTime(AlternatePath.Association a_association, long a_minimumPredecessorReadyTime)
    {
        if (a_association.Predecessor.Operation is InternalOperation predecessorOperation)
        {
            if (!a_association.PredecessorReadyEventScheduled)
            {
                long opReadyTime;

                if (predecessorOperation.AllSucOpnsFinishedOrOmitted())
                {
                    // If the successor operatoins are all finished. The Transfer time isn't necessary.
                    predecessorOperation.GetScheduledFinishDate(out opReadyTime, false);
                }
                else
                {
                    // Include the transfer time after the pred finish.
                    opReadyTime = predecessorOperation.CalcEndOfResourceTransferTimeTicks();
                }

                long eventTime = Math.Max(a_minimumPredecessorReadyTime, opReadyTime);
                PredecessorOperationAvailableEvent nonFinishedPredReleaseEvt = new (eventTime, a_association, eventTime);
                AddEvent(nonFinishedPredReleaseEvt);
                a_association.PredecessorReadyEventScheduled = true;


                InternalOperation successorOperation = a_association.Successor.Operation;
                SetSuccessorOverlapResourceReleaseTimes(successorOperation, eventTime);
            }
        }
    }

    private void AddPredecessorAvailableOverlappingFromTransferSpan(AlternatePath.Association a_association)
    {
        if (!a_association.PredecessorReadyEventScheduled)
        {
            if (a_association.Predecessor.Operation is InternalOperation)
            {
                if (!a_association.GetOverlapTransferSpanReleaseTicks(out long releaseTicks))
                {
                    releaseTicks = SimClock;
                }

                releaseTicks = Math.Max(m_clock, releaseTicks);
                PredecessorOperationAvailableEvent e = new (releaseTicks, a_association, releaseTicks);
                AddEvent(e);
                a_association.PredecessorReadyEventScheduled = true;

                InternalOperation successorOperation = a_association.Successor.Operation;
                SetSuccessorOverlapResourceReleaseTimes(successorOperation, releaseTicks);
            }
        }
    }

    /// <summary>
    /// Add event that may release the successor operation before the start of the predecessor.
    /// </summary>
    /// <param name="a_association">Association between the available predecessor and successor.</param>
    private void AddPredecessorAvailableOverlappingFromTransferSpanBeforeStart(AlternatePath.Association a_association)
    {
        if (!a_association.PredecessorReadyEventScheduled)
        {
            if (a_association.Predecessor.Operation is InternalOperation predecessorOperation)
            {
                if (!a_association.CalcEarliestSucOpCanStartForOverlapByTransferSpanBeforeStartOfPredecessor(out long releaseTicks))
                {
                    releaseTicks = SimClock;
                }

                releaseTicks = Math.Max(m_clock, releaseTicks);
                PredecessorOperationAvailableEvent e = new (releaseTicks, a_association, releaseTicks);
                AddEvent(e);
                a_association.PredecessorReadyEventScheduled = true;

                InternalOperation successorOperation = a_association.Successor.Operation;
                SetSuccessorOverlapResourceReleaseTimes(successorOperation, releaseTicks);
            }
        }
    }

    private void AddPredecessorAvailableOverlappingFromTransferSpanAfterSetup(AlternatePath.Association a_association, long a_minimumPredecessorReadyTime)
    {
        if (!a_association.PredecessorReadyEventScheduled)
        {
            if (a_association.Predecessor.Operation is InternalOperation predecessorOperation)
            {
                long overlapTime = a_association.GetOverlapTransferSpanAfterSetupReleaseTicks();
                overlapTime = Math.Max(a_minimumPredecessorReadyTime, overlapTime);
                PredecessorOperationAvailableEvent e = new (overlapTime, a_association, overlapTime);
                AddEvent(e);
                a_association.PredecessorReadyEventScheduled = true;

                InternalOperation successorOperation = a_association.Successor.Operation;
                SetSuccessorOverlapResourceReleaseTimes(successorOperation, overlapTime);
            }
        }
    }

    private void AddPredecessorAvailableOverlappingAtPercentComplete(AlternatePath.Association a_association, long a_minimumPredecessorReadyTime)
    {
        if (!a_association.PredecessorReadyEventScheduled)
        {
            long overlapTime = -1;
            if (a_association.Predecessor.Operation.Scheduled)
            {
                ResourceOperation predOp = (ResourceOperation)a_association.Predecessor.Operation;
                InternalActivity predAct = predOp.Activities.GetByIndex(0);
                Resource predActSchedRes = predAct.PrimaryResourceRequirementBlock.ScheduledResource;
                overlapTime = a_association.GetOverlapPercentCompleteReleaseTime(a_minimumPredecessorReadyTime, predActSchedRes);
            }

            if (overlapTime != -1)
            {
                PredecessorOperationAvailableEvent e = new (overlapTime, a_association, overlapTime);
                AddEvent(e);
                a_association.PredecessorReadyEventScheduled = true;

                InternalOperation successorOperation = a_association.Successor.Operation;
                SetSuccessorOverlapResourceReleaseTimes(successorOperation, overlapTime);
            }
            else
            {
                AddPredecessorAvailableEventAtOperationReadyTime(a_association, a_minimumPredecessorReadyTime);
            }
        }
    }

    private void AddPredecessorAvailableOverlappingAtFirstTransfer(AlternatePath.Association a_association, long a_minimumPredecessorReadyTime)
    {
        if (!a_association.PredecessorReadyEventScheduled)
        {
            ResourceOperation resourceOperation = a_association.Predecessor.Operation as ResourceOperation;

            if (resourceOperation != null)
            {
                if (resourceOperation.IsFinishedOrOmitted || !resourceOperation.Scheduled)
                {
                    AddPredecessorAvailableEventAtOperationReadyTime(a_association, a_minimumPredecessorReadyTime);
                }
                else
                {
                    if (resourceOperation.TransferQtyProfile != null && resourceOperation.TransferQtyProfile.Count > 0)
                    {
                        long overlapReleaseTicks = resourceOperation.TransferQtyProfile[0].m_completionDate;
                        overlapReleaseTicks = Math.Max(a_minimumPredecessorReadyTime, overlapReleaseTicks);
                        PredecessorOperationAvailableEvent e = new (overlapReleaseTicks, a_association, overlapReleaseTicks);
                        AddEvent(e);
                        a_association.PredecessorReadyEventScheduled = true;

                        InternalOperation successorOperation = a_association.Successor.Operation;
                        SetSuccessorOverlapResourceReleaseTimes(successorOperation, overlapReleaseTicks);
                    }
                    else
                    {
                        AddPredecessorAvailableEventAtOperationReadyTime(a_association, a_minimumPredecessorReadyTime);
                    }
                }
            }
            else
            {
                AddPredecessorAvailableEventAtOperationReadyTime(a_association, a_minimumPredecessorReadyTime);
            }
        }
    }

    private static void SetSuccessorOverlapResourceReleaseTimes(InternalOperation a_successorOperation, long a_overlapReleaseTicks)
    {
        Dictionary<Resource, long> overlapResTimes = new();
        ResReqsPlantResourceEligibilitySets narrowedSet = a_successorOperation.AlternatePathNode.ResReqsEligibilityNarrowedDuringSimulation;

        if (narrowedSet.Count == 0)
        {
            return;
        }

        foreach (Resource resource in narrowedSet.PrimaryEligibilitySet.GetResources())
        {
            overlapResTimes.Add(resource, a_overlapReleaseTicks);
        }

        a_successorOperation.OverlapResourceReleaseTimes = overlapResTimes;
    }

    private void AddPredecessorAvailableOverlappingFromEarliestStartDate(AlternatePath.Association a_association, long a_minimumPredecessorReadyTime)
    {
        if (!a_association.PredecessorReadyEventScheduled)
        {
            ResourceOperation resourceOperation = a_association.Predecessor.Operation as ResourceOperation;

            if (resourceOperation != null)
            {
                if (resourceOperation.IsFinishedOrOmitted || !resourceOperation.Scheduled)
                {
                    AddPredecessorAvailableEventAtOperationReadyTime(a_association, a_minimumPredecessorReadyTime);
                }
                else
                {
                    if (resourceOperation.TransferQtyProfile != null && resourceOperation.TransferQtyProfile.Count > 0)
                    {
                        // In the case where overlap using transfer quantity is potentially taking place we assume there is only 1 successor operation.
                        InternalOperation successor = (InternalOperation)resourceOperation.Successors[0].Successor.Operation;

                        PlantResourceEligibilitySet pres = null;

                        if (successor.AlternatePathNode.ResReqsEligibilityNarrowedDuringSimulation.Count > successor.ResourceRequirements.PrimaryResourceRequirementIndex)
                        {
                            pres = successor.AlternatePathNode.ResReqsEligibilityNarrowedDuringSimulation[successor.ResourceRequirements.PrimaryResourceRequirementIndex];
                        }

                        try
                        {
                            Dictionary<Resource, long> resourceReleaseTimes = new ();
                            long minReleaseDate = long.MaxValue;

                            if (successor.IsNotFinishedAndNotOmitted) // 2016.01.12: This check use to be done by checking for a null pres: if (pres != null)
                            {
                                SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = pres.GetEnumerator();
                                while (ersEtr.MoveNext())
                                {
                                    EligibleResourceSet eligibleResourceSet = ersEtr.Current.Value;

                                    for (int i = 0; i < eligibleResourceSet.Count; ++i)
                                    {
                                        InternalResource eligibleResource = eligibleResourceSet[i];
                                        Resource resource = eligibleResource as Resource;

                                        if (resource != null)
                                        {
                                            long earliestStartDate = resource.DetermineEarliestOverlapTime(Clock, successor, resourceOperation);
                                            resourceReleaseTimes.Add(resource, earliestStartDate);

                                            if (earliestStartDate < minReleaseDate)
                                            {
                                                minReleaseDate = earliestStartDate;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Finished or omitted
                                minReleaseDate = a_minimumPredecessorReadyTime;
                            }

                            if (minReleaseDate != long.MaxValue)
                            {
                                successor.OverlapResourceReleaseTimes = resourceReleaseTimes;
                                PredecessorOperationAvailableEvent e = new (minReleaseDate, a_association, minReleaseDate);
                                AddEvent(e);
                                a_association.PredecessorReadyEventScheduled = true;

                                InternalOperation successorOperation = a_association.Successor.Operation;
                                SetSuccessorOverlapResourceReleaseTimes(successorOperation, minReleaseDate);
                            }
                        }
                        catch (Exception e)
                        {
                            Overlap.Throw_OverlapDebugError(e);
                            successor.OverlapResourceReleaseTimes = null;
                            AddPredecessorAvailableEventAtOperationReadyTime(a_association, a_minimumPredecessorReadyTime);
                        }
                    }
                    else
                    {
                        AddPredecessorAvailableEventAtOperationReadyTime(a_association, a_minimumPredecessorReadyTime);
                    }
                }
            }
            else
            {
                AddPredecessorAvailableEventAtOperationReadyTime(a_association, a_minimumPredecessorReadyTime);
            }
        }
    }

    #region Simulation helpers
    private void CreateActiveResourceList(out MainResourceSet o_availableResources)
    {
        o_availableResources = new MainResourceSet();

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
                        o_availableResources.Add(machine);
                    }
                }
            }
        }
    }

    private void UnscheduleActivities(MainResourceSet a_availResources,
                                      SimulationTimePoint a_startTime,
                                      SimulationTimePoint a_endTime,
                                      ResourceActivitySets a_sequentialResourceActivities,
                                      SimulationType a_simType,
                                      long a_newClock)
    {
        AVLTree<OperationKey, InternalOperation> unscheduledOperations = new (new OperationKeyComparer());
        List<InternalActivity> unscheduleActivities = new ();

        // Create a set of all the scheduled batches.
        bool invalidBatchesFound = false;
        HashSet<Batch> scheduledBatches = new ();
        for (int resI = 0; resI < a_availResources.Count; ++resI)
        {
            if (a_availResources[resI] is Resource)
            {
                Resource res = (Resource)a_availResources[resI];
                ResourceBlockList.Node curBlkNode = res.Blocks.First;
                while (curBlkNode != null)
                {
                    ResourceBlock block = curBlkNode.Data;

                    //TEST CODE TO CHECK FOR INVALID BLOCK REMOVAL.
                    //We found a case where a block node exists and is never removed but we don't know why.
                    //Avoid the error by removing the invalid node and 
                    if (!block.Batch.BlockAtIndex(0).Batched)
                    {
                        invalidBatchesFound = true;
                        ResourceBlockList.Node next = curBlkNode.next;
                        res.Blocks.Remove(curBlkNode);
                        curBlkNode = next;
                        continue;
                    }

                    scheduledBatches.Add(block.Batch);

                    curBlkNode = curBlkNode.Next;
                }
            }
        }

        if (invalidBatchesFound)
        {
            ScenarioExceptionInfo sei = new ScenarioExceptionInfo().Create(this);
            m_errorReporter.LogException(new PTHandleableException("Invalid batch data removed. The underlying issue needs to be resolved. Contact Support"), sei);
        }

        // Create Anchor release events,
        // mark sequenced activities and create ClockReleaseEvents for them.
        HashSet<Batch>.Enumerator batchEtr = scheduledBatches.GetEnumerator();
        while (batchEtr.MoveNext())
        {
            bool createBatchActList = false;
            Batch batch = batchEtr.Current;
            IEnumerator<InternalActivity> actEtr = batch.GetEnumerator();
            while (actEtr.MoveNext())
            {
                InternalActivity act = actEtr.Current;

                BaseId simPlantId = BaseId.NULL_ID;

                // If optimizing all plants, don't use this setting.
                if (m_activeCompressSettings != null)
                {
                    simPlantId = m_activeCompressSettings.PlantToInclude;
                }
                else if (m_activeOptimizeSettings != null && m_activeOptimizeSettings.ResourceScope == OptimizeSettings.resourceScopes.OnePlant)
                {
                    simPlantId = m_activeOptimizeSettings.PlantToInclude;
                }

                if (simPlantId != BaseId.NULL_ID && batch.PrimaryResource.PlantId != simPlantId)
                {
                    act.PlantNotIncludedInSimulate = true;
                    act.ManufacturingOrder.LockedToCurrentPathByOptimize_InPlantThatsNotBeingOptimized = true;
                }

                if (act.SetupWaitForAnchorSetFlag())
                {
                    AddAnchorReleaseEvent(act, act.AnchorDateTicks);
                }

                long simStartTicks = a_startTime.GetTimeForResource(batch.PrimaryResource);
                long simStopTicks = a_endTime.GetTimeForResource(batch.PrimaryResource);

                if (!act.Sequenced)
                {
                    if (a_simType == SimulationType.ClockAdvance ||
                        a_simType == SimulationType.Move ||
                        a_simType == SimulationType.MoveAndExpedite ||
                        a_simType == SimulationType.TimeAdjustment ||
                        a_simType == SimulationType.Expedite ||
                        a_simType == SimulationType.ConstraintsChangeAdjustment)
                    {
                        AddClockReleaseEventHelper(act, a_newClock, batch, simStartTicks);
                    }
                    else if (a_simType == SimulationType.JitCompress)
                    {
                        act.Sequenced = true;
                    }
                    else if (a_simType == SimulationType.Compress)
                    {
                        if (m_activeCompressSettings.ResourceScope != OptimizeSettings.resourceScopes.OnePlant)
                        {
                            if (batch.StartTicks <= simStartTicks ||
                                batch.StartTicks >= simStopTicks ||
                                !m_activeCompressSettings.ResourceInScope(batch.PrimaryResource.Id))
                            {
                                AddClockReleaseEventHelper(act, a_newClock, batch, simStartTicks);
                            }
                            else
                            {
                                act.Sequenced = true;
                            }
                        }
                        else
                        {
                            if (act.PlantNotIncludedInSimulate ||
                                batch.StartTicks <= simStartTicks ||
                                batch.StartTicks >= simStopTicks ||
                                !m_activeCompressSettings.ResourceInScope(batch.PrimaryResource.Id))
                            {
                                AddClockReleaseEventHelper(act, a_newClock, batch, simStartTicks);
                            }
                            else
                            {
                                act.Sequenced = true;
                            }
                        }
                    }
                    else if (a_simType == SimulationType.Optimize)
                    {
                        if (m_activeOptimizeSettings.ResourceScope == OptimizeSettings.resourceScopes.OnePlant)
                        {
                            if (act.PlantNotIncludedInSimulate)
                            {
                                act.ManufacturingOrder.LockedToCurrentPathByOptimize_InPlantThatsNotBeingOptimized = true;
                                AddClockReleaseEventHelper(act, a_newClock, batch, simStartTicks);
                                act.PlantNotIncludedInSimulate = true;
                            }
                            else if (batch.StartTicks < a_startTime.GetTimeForResource(batch.PrimaryResource))
                            {
                                LockActivityToCurrentPathBecauseItsScheduledBeforeTheOptimizeStartHelper(a_newClock, batch, act, simStartTicks);
                            }
                        }

                        if (batch.StartTicks < simStartTicks)
                        {
                            LockActivityToCurrentPathBecauseItsScheduledBeforeTheOptimizeStartHelper(a_newClock, batch, act, simStartTicks);
                        }
                    }
                }

                //*************************************************************************
                // If the activity is sequenced, set it up to schedule after its left neighbor.
                //*************************************************************************
                // Now you need to take into consideration that multiple activities
                // can make up the left predecessor activity.
                // Change the data structures of things like SequentialState so they can 
                // handle multiple activity.
                // Also change a_sequentialResourceActivities to handle batch
                //*************************************************************************
                for (int blockI = 0; blockI < batch.BlockCount; ++blockI)
                {
                    ResourceBlockList.Node blockNode = batch.GetBlockNodeAtIndex(blockI);

                    if (blockNode != null)
                    {
                        ResourceBlock block = blockNode.Data;
                        ResourceBlock blockNext = null;

                        if (block.MachineBlockListNode.Next != null)
                        {
                            blockNext = block.MachineBlockListNode.Next.Data;
                        }

                        bool hasLeftSeqResAct;
                        if (blockNode.Previous != null)
                        {
                            ResourceBlock prevBlock = blockNode.Previous.Data;

                            if (prevBlock.Batch.Contains(act))
                            {
                                // The UsageStart and UsageEnd of different requirements of the activity don't overlap and
                                // are scheduled on the same resource.
                                // In this case the activity's block that's scheduled first already setup the 
                                // hasLeftSeqResAct.
                                hasLeftSeqResAct = false;
                            }
                            else
                            {
                                hasLeftSeqResAct = true;
                            }
                        }
                        else
                        {
                            hasLeftSeqResAct = false;
                        }

                        Resource blockResource = block.ScheduledResource;
                        if (batch.PrimaryResource.CapacityType == InternalResourceDefs.capacityTypes.SingleTasking || blockResource.CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
                        {
                            if (act.Sequenced)
                            {
                                if (act.PlantNotIncludedInSimulate)
                                {
                                    // The plant the activity is scheduled in is not to be included in an optimize that is occurring and the time the activity is scheduled
                                    // is beyond the optimize start date. This will result in the plant's schedule not changing except for things like predecessor constraint
                                    // and material constraint changes. 
                                    // The schedule for the plant actually can be changed. For instance if an optimize consumed some material that this activity required then
                                    // this activity would be pushed out in time. But since the sequencing is Without Left Neighbor, the activity can be scheduled later with
                                    // minimal impact to operations that were scheduled to the right of it.
                                    SetupSequentialStateWithoutLeftNeighbor(act, block);
                                }
                                else if (blockResource.CapacityType == InternalResourceDefs.capacityTypes.Infinite ||
                                         blockResource.CapacityType == InternalResourceDefs.capacityTypes.MultiTasking)
                                {
                                    SetupSequentialStateWithoutLeftNeighbor(act, block);
                                }
                                else
                                {
                                    if (act.Operation.EnforceMaxDelay())
                                    {
                                        SetupSequentialStateWithoutLeftNeighbor(act, block);
                                        SetupSequntialStateForRightNeighbor(a_startTime.GetTimeForResource(blockResource), a_simType, blockResource, block, blockNext, hasLeftSeqResAct, act);
                                    }
                                    else
                                    {
                                        act.InitSequentialSim();
                                        SetupSequntialStateForRightNeighbor(a_startTime.GetTimeForResource(blockResource), a_simType, blockResource, block, blockNext, hasLeftSeqResAct, act);
                                    }
                                }
                            }
                        }
                        else if (batch.PrimaryResource.CapacityType == InternalResourceDefs.capacityTypes.Infinite ||
                                 batch.PrimaryResource.CapacityType == InternalResourceDefs.capacityTypes.MultiTasking)
                        {
                            if (act.Sequenced)
                            {
                                SetupSequentialStateWithoutLeftNeighbor(act, block);
                            }
                        }


                        // This is to make sure only 1 attempt is made
                        // to unschedule an activity. Note: Acitivites may
                        // appear in this loop multiple times because each
                        // activity may use multiple resources.
                        if (!act.UnscheduleActivityAddedToList)
                        {
                            unscheduleActivities.Add(act);
                            act.UnscheduleActivityAddedToList = true;
                        }

                        OperationKey key = new OperationKey2(act);

                        if (unscheduledOperations.Find(key) == null)
                        {
                            unscheduledOperations.Add(key, act.Operation);
                        }

                        if (act.AddToSequentialStateList)
                        {
                            createBatchActList = true;
                        }

                        if (createBatchActList)
                        {
                            List<InternalActivity> batchActList = new ();
                            IEnumerator<InternalActivity> batchActEtr = batch.GetEnumerator();

                            while (batchActEtr.MoveNext())
                            {
                                InternalActivity batchsAct = batchActEtr.Current;
                                batchActList.Add(batchsAct);
                            }

                            a_sequentialResourceActivities[blockResource.m_sequentialResourceIdx].Add(batchActList);
                        }
                    }
                } // End Block iteration through batch
            } // End activity iteration
        } // End batch iteration

        // Unschedule the activities. They are unscheduled down here because the above code
        // requires the unaltered schedule to setup for sequential activity scheduling.
        for (int activityI = 0; activityI < unscheduleActivities.Count; ++activityI)
        {
            InternalActivity ia = unscheduleActivities[activityI];

            if (a_simType == SimulationType.Optimize)
            {
                bool suppressJITStartDate = false;
                ResourceBlock block = ia.PrimaryResourceRequirementBlock;
                InternalResource scheduledRes = block.ScheduledResource;

                if (m_activeOptimizeSettings.ResourceScope == OptimizeSettings.resourceScopes.OnePlant)
                {
                    if (block != null)
                    {
                        if (block.ScheduledResource.Department.Plant.Id != m_activeOptimizeSettings.PlantToInclude)
                        {
                            suppressJITStartDate = true;
                        }
                    }
                }

                if (ia.GetScheduledStartTicks() < a_startTime.GetTimeForResource(scheduledRes) || ia.Anchored)
                {
                    suppressJITStartDate = true;
                }

                if (suppressJITStartDate)
                {
                    ia.SuppressReleaseDateAdjustments = true;
                    ia.Operation.ManufacturingOrder.SuppressReleaseDateAdjustments = true;
                }
            }
        }

        AVLTree<OperationKey, InternalOperation>.Enumerator opsEtr = unscheduledOperations.GetEnumerator();
        while (opsEtr.MoveNext())
        {
            opsEtr.Current.Value.Unschedule(false, false);
        }

        // Mark manufacturing orders that have been unscheduled as unscheduled.
        AVLTree<OperationKey, InternalOperation>.Enumerator unscheduledOpsEtr = unscheduledOperations.GetEnumerator();
        while (unscheduledOpsEtr.MoveNext())
        {
            // This function will mark the MO as having been unscheduled, unschedule activities that have been scheduled for
            // post processing only and perform any other MO unscheduling operations.
            unscheduledOpsEtr.Current.Value.ManufacturingOrder.UnscheduleNotification();
        }

        // [BATCH_CODE]
        m_batchManager = new BatchManager();
    }

    private void SetupSequntialStateForRightNeighbor(long a_startTime, SimulationType a_simulationType, Resource a_resource, ResourceBlock a_block, ResourceBlock a_rightNeighborBlock, bool a_hasLeftSeqResAct, InternalActivity a_activity)
    {
        InternalActivity.SequentialState ss = a_activity.Sequential[a_block.ResourceRequirementIndex];
        ss.LeftSequencedNeighborScheduled = false;
        ss.SequencedSimulationResource = a_resource;

        if (a_rightNeighborBlock == null)
        {
            ss.ClearRightSequencedNeighbor();
        }
        else
        {
            // In the case of an optimization you need to make sure the neighboring activity is before the start of the optimization.
            InternalActivity rightNeighbor = a_rightNeighborBlock.Activity;
            if (a_simulationType != SimulationType.Optimize || (a_simulationType == SimulationType.Optimize && rightNeighbor.GetScheduledStartTicks() < a_startTime))
            {
                // [BATCH_CODE]
                ss.SetRightSequencedNeighbor(rightNeighbor.Batch, a_rightNeighborBlock.ResourceRequirementIndex);
            }
        }

        // [USAGE_CODE] SetupSequntialStateForRightNeighbor: To avoid deadlocks where one activity is waiting on the other to schedule, ignore the left sequenced neighbor constraint when the requirement is scheduled after other requirements.
        bool usedAfterBatchStarts = a_block.StartTicks > a_block.Batch.StartTicks;
        if (!usedAfterBatchStarts)
        {
            ss.HasSequencedLeftNeighbor = a_hasLeftSeqResAct;
        }

        // [BATCH_CODE]
        a_activity.AddToSequentialStateList = true;
    }

    /// <summary>
    /// Lock the manufacturing order to the path it's scheduled on prior to the start of the simulation and add a ClockReleaseEvent for the activity.
    /// </summary>
    /// <param name="a_newClock">The schedule's current clock time. </param>
    /// <param name="a_batch">The batch the activity is scheduled on prior to the start of the simulation.</param>
    /// <param name="activity">A ClockReleaseEvent will be added for this activity.</param>
    /// <param name="a_simStartTicks">The time the simulation starts for this activity on the resource it was scheduled on prior to the start of the simulation.</param>
    private void LockActivityToCurrentPathBecauseItsScheduledBeforeTheOptimizeStartHelper(long a_newClock, Batch a_batch, InternalActivity activity, long a_simStartTicks)
    {
        activity.ManufacturingOrder.LockedToCurrentPathByOptimize_ScheduledBeforeStartOfOptimize = true;
        AddClockReleaseEventHelper(activity, a_newClock, a_batch, a_simStartTicks);
    }

    /// <summary>
    /// Add a ClockReleaseEvent for an activity.
    /// </summary>
    /// <param name="a_act">>A ClockReleaseEvent will be added for this activity.</param>
    /// <param name="a_newClock">The schedule's clock time. </param>
    /// <param name="a_batch">The batch the activity is scheduled on prior to the start of the simulation.</param>
    /// <param name="a_simStartTicks">The time the simulation starts for this activity on the resource it was scheduled on prior to the start of the simulation. </param>
    private void AddClockReleaseEventHelper(InternalActivity a_act, long a_newClock, Batch a_batch, long a_simStartTicks)
    {
        a_act.Sequenced = true;
        bool inFrozenZone = false;
        if (a_act.Scheduled)
        {
            long scheduledStart = a_act.ScheduledStartTicks();
            inFrozenZone = scheduledStart < a_simStartTicks;
        }

        if (!a_act.Operation.EnforceMaxDelay() || inFrozenZone)
        {
            // Calculate the potential new setup backwards from the start of processing and
            // back the release of this activity off by this amount of time so the setup can 
            // complete by the time processing currently starts and the end date of the batch
            // will be unaffected. Basically, attempt to schedule any additional setup time prior to the 
            // start of processing so the end of batch remains the same.
            a_act.WaitForClockAdjustmentRelease = true;
            long clockAdjustmentReleaseDate = Math.Max(a_newClock, a_batch.StartTicks);

            ResourceBlock b = a_batch.BlockAtIndex(a_act.Operation.ResourceRequirements.PrimaryResourceRequirementIndex);
            RequiredSpanPlusSetup setupTime;
            if (b.MachineBlockListNode.Previous != null)
            {
                ResourceBlock predBlock = b.MachineBlockListNode.Previous.Data;
                InternalActivity firstPredAct = predBlock.Batch.FirstActivity;

                if (a_act.PrependSetupToMoveBlocksRightNeighborsNewLeftNeighbor != null)
                {
                    /// [PrependSetupToMoveBlocksRightNeighbors] 3-1: Calculate the setup between the move blocks right neighbor and what will be its new left neighbor. Then release move block' s right neighbor at its processing start minus this setup time.
                    long originalSetupTicks = a_batch.SetupCapacitySpan.TimeSpanTicks;


                    setupTime = a_batch.PrimaryResource.CalculateSetupTime(SimClock, a_act, new (a_act.PrependSetupToMoveBlocksRightNeighborsNewLeftNeighbor), true, RequiredSpanPlusSetup.s_notInit, a_batch.StartTicks, ExtensionController, true);

                    long additionalSetupTicks = 0;

                    if (setupTime.TimeSpanTicks > originalSetupTicks)
                    {
                        additionalSetupTicks = setupTime.TimeSpanTicks - originalSetupTicks;
                    }


                    clockAdjustmentReleaseDate -= additionalSetupTicks;
                }
            }

            ClockReleaseEvent clockAdjustmentReleaseEvent = new (clockAdjustmentReleaseDate, a_act);
            AddEvent(clockAdjustmentReleaseEvent);
        }
    }

    private void SetupSequentialStateWithoutLeftNeighbor(InternalActivity a_activity, ResourceBlock a_block)
    {
        a_activity.InitSequentialSim();
        InternalActivity.SequentialState sequentialState = a_activity.Sequential[a_block.ResourceRequirementIndex];
        sequentialState.LeftSequencedNeighborScheduled = false;
        sequentialState.HasSequencedLeftNeighbor = false;
        sequentialState.SequencedSimulationResource = a_block.ScheduledResource;
        // [BATCH_CODE]
        a_activity.AddToSequentialStateList = true;
    }

    /// <summary>
    /// Update the Lock and Anchor status in the Frozen Zone.
    /// </summary>
    private void UpdateLockAndAnchorInFrozenZone(ScenarioOptions a_scenarioOptions, IScenarioDataChanges a_dataChanges)
    {
        JobManager.LockAndAnchorBefore(OptimizeSettings.ETimePoints.EndOfFrozenZone, a_scenarioOptions, a_dataChanges);

        //Update the list of Jobs since many will have changed
        //Update the blocks in the Gantt
        using (_scenario.AutoEnterScenarioEvents(out ScenarioEvents se))
        {
            //se.FireJobChangesEvent(a_dataChanges, this.JobManager);
            se.FireBlocksChangedEvent();
        }
    }

    #region MaterialAvailableEvent creation for activities, operation, resource activity sets, etc...
    /// <summary>
    /// Setup MaterialAvailableEvents for the activities in the collection.
    /// </summary>
    /// <param name="activitiesCollection"></param>
    /// <param name="simClock">If calling this function in response to an event, provide the simulation clock time. This is used to determine whether the event is necessary.</param>
    private void SetupMaterialConstraints(ActivitiesCollection a_activitiesCollection, long a_simClock)
    {
        for (int activityI = 0; activityI < a_activitiesCollection.Count; ++activityI)
        {
            InternalActivity activity = a_activitiesCollection[activityI];
            SetupMaterialConstraints(activity, a_simClock);
        }
    }

    /// <summary>
    /// Setup MaterialAvailableEvents for the activities in the resource activity sets.
    /// </summary>
    /// <param name="resourceActivitySets"></param>
    /// <param name="simClock">If calling this function in response to an event, provide the simulation clock time. This is used to determine whether the event is necessary.</param>
    private void SetupMaterialConstraints(ResourceActivitySets a_resourceActivitySets, long a_simClock)
    {
        for (int rasI = 0; rasI < a_resourceActivitySets.Count; ++rasI)
        {
            ResourceActivitySet ras = a_resourceActivitySets[rasI];
            for (int actListI = 0; actListI < ras.Count; ++actListI)
            {
                // [BATCH_CODE]
                List<InternalActivity> actList = ras[actListI];
                for (int actI = 0; actI < actList.Count; ++actI)
                {
                    InternalActivity act = actList[actI];
                    SetupMaterialConstraints(act, a_simClock);
                }
            }
        }
    }

    /// <summary>
    /// Setup MaterialAvailableEvent for the activity if necessary.
    /// </summary>
    /// <param name="activity"></param>
    /// <param name="simClock">If calling this function in response to an event, provide the simulation clock time. This is used to determine whether the event is necessary.</param>
    private void SetupMaterialConstraints(InternalActivity a_activity, long a_simClock)
    {
        SetupMaterialConstraints(a_activity.Operation, a_simClock);
    }

    /// <summary>
    /// Create a material available date for the activity if necessary. Only one material available event
    /// is created for all an operation's materials. The release date is the latest release date among all the
    /// material required by the operation. If the latest release date in before the Clock or the current
    /// simulation time then the event is not necessary and is not created.
    /// A good time to call this function is once its predecessor operation constraints have been satisfied.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="simClock">If calling this function in response to an event, provide the simulation clock time. This is used to determine whether the event is necessary.</param>
    private void SetupMaterialConstraints(BaseOperation a_operation, long a_simClock)
    {
        if (MustSchedule(a_operation))
        {
            MaterialRequirementsCollection materials = a_operation.MaterialRequirements;
            if (!materials.MaterialConstraintsEventScheduled)
            {
                // Handle Buy Direct Material Requirements by scheduling a release event for the latest buy direct material requirement.
                MaterialRequirement mr = materials.GetLastestConstrainingBuyDirectMaterialRequirement(Clock, out long latestMaterialConstraint);

                if (latestMaterialConstraint > Clock && latestMaterialConstraint > a_simClock)
                {
                    MaterialAvailableEvent matlEvent = new (latestMaterialConstraint, mr, a_operation);
                    AddEvent(matlEvent);
                    materials.WaitingOnMaterial = true;
                }

                materials.MaterialConstraintsEventScheduled = true;

                // Schedule lead-time events for every Inventory Material Requirement.
                if (a_operation is InternalOperation internalOperation)
                {
                    AddLeadTimeEventsForInventory(internalOperation, a_simClock);
                }
            }
        }
    }

    /// <summary>
    /// Calculate the inventory's lead-time. Typlically clock+inventory.Lead-time unless it's been customized.
    /// </summary>
    /// <param name="a_op">The operation the lead-time is intended for. This is used if the lead-time is customized.</param>
    /// <param name="a_mr">The material requirement the lead-time is for. This is used if the lead-time is customized.</param>
    /// <param name="a_inv">The inventory the lead-time is for.</param>
    /// <returns></returns>
    internal long CalcLeadTimeTicks(InternalOperation a_op, MaterialRequirement a_mr, Inventory a_inv)
    {
        long leadSpanTicks = 0;
        if (a_inv != null)
        {
            leadSpanTicks = a_inv.LeadTimeTicks;
        }

        long leadTime = Clock + leadSpanTicks;
        //The customization either returns the normal lead-time or a customized lead-time if the lead-time needs to be customized.

        long? leadTimeOverride = ExtensionController.CustomizeLeadTime(a_op, a_mr, Clock, leadTime);
        if (leadTimeOverride.HasValue)
        {
            leadTime = leadTimeOverride.Value;
        }

        return leadTime;
    }

    /// <summary>
    /// For Inventory MaterialRequirements lead-time events are created.
    /// There is no specific action necessary when the event occurs.
    /// It exists simply to make sure the activity has the opportunity to schedule at the lead-time,
    /// but it is possible that the activity has already scheduled due to Inventory becoming
    /// available earlier.  The Inventory may have been made available either through existing stock,
    /// Purchase To Stocks, or Activities with material destined for stock.
    /// Since lead-times for warehouses may be different a separate event is created for each
    /// eligible warehouse's lead-time.
    /// </summary>
    internal void AddLeadTimeEventsForInventory(InternalOperation a_operation, long a_simClock)
    {
        foreach (MaterialRequirement mr in a_operation.MaterialRequirements.StockMaterials)
        {
            if (!mr.IssuedComplete)
            {
                AlternatePath.Node node = a_operation.AlternatePathNode;
                PlantResourceEligibilitySet pres = node.ResReqsEligibilityNarrowedDuringSimulation[0];

                SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = pres.GetEnumerator();
                while (ersEtr.MoveNext())
                {
                    BaseId plantId = ersEtr.Current.Key;
                    Plant plant = m_plantManager.GetById(plantId);
                    for (int warehouseI = 0; warehouseI < plant.WarehouseCount; ++warehouseI)
                    {
                        Warehouse warehouse = plant.GetWarehouseAtIndex(warehouseI);
                        Inventory inventory = warehouse.Inventories[mr.Item.Id];

                        long leadTimeDate = CalcLeadTimeTicks(node.Operation, mr, inventory);
                        if (leadTimeDate >= SimClock)
                        {
                            bool customizedLeadTime = inventory != null && Clock + inventory.LeadTimeTicks != leadTimeDate;
                            LeadTimeEvent lte = new (leadTimeDate, mr, warehouse, customizedLeadTime);
                            AddEvent(lte);
                        }
                    }
                }
            }
        }
    }
    #endregion
    #endregion

    internal class DispatchActivityAtResourceStage
    {
        internal DispatchActivityAtResourceStage(long a_time, InternalResource a_iR, InternalActivity a_iA)
        {
            m_time = a_time;
            m_ia = a_iA;
            m_res = a_iR;
        }

        internal InternalResource m_res;
        internal long m_time;
        internal InternalActivity m_ia;
    }

    private long m_lastSimulationDateTicks;

    public DateTime LastSimulationDate => new (m_lastSimulationDateTicks);

    /// <summary>
    /// The number of simulations that have occurred since the most recent startup or this scenario.
    /// </summary>
    private long m_nbrOfSimulationsSinceStartup;

    private long NbrOfSimulationsSinceStartup
    {
        get => m_nbrOfSimulationsSinceStartup;
        set => m_nbrOfSimulationsSinceStartup = value;
    }

    private long m_nbrOfSimulations;

    /// <summary>
    /// The total number of simulations that have been performed on this scenario.
    /// </summary>
    internal long NbrOfSimulations => m_nbrOfSimulations;

    private ScenarioBaseT m_simulationTransmission;

    public ScenarioBaseT SimulationTransmission => m_simulationTransmission;

    private void InitAllActivitiesComparableDispatching()
    {
        if (m_singleDispatcherDefinitionInUse != null)
        {
            m_availableResourceEventsSet = new CalendarResourceAvailableEventSetForAllActivitiesComparableDispatching();
        }
        else
        {
            m_availableResourceEventsSet = new CalendarResourceAvailableEventSetForNonComparableDispatching();
        }
    }

    [Common.Attributes.DebugLogging(Common.Attributes.EDebugLoggingType.None)]
    private SimulationProgress m_simulationProgress;

    public SimulationProgress SimulationProgress => m_simulationProgress;

    /// <summary>
    /// The resources available within the simulation.
    /// </summary>
    private MainResourceSet m_availableResources;

    /// <summary>
    /// Used to track the stage an MO was scheduled in. Also defines a method that can be used to sort objects of this type by stage.
    /// </summary>
    private class ScheduledStageLevel
    {
        internal ScheduledStageLevel(ManufacturingOrder a_mo, int a_stage)
        {
            MO = a_mo;
            Stage = a_stage;
        }

        /// <summary>
        /// An MO that was scheduled.
        /// </summary>
        internal readonly ManufacturingOrder MO;

        /// <summary>
        /// The stage the MO was scheduled in.
        /// </summary>
        internal readonly int Stage;

        /// <summary>
        /// compare objects by stage.
        /// </summary>
        /// <param name="a_x"></param>
        /// <param name="a_y"></param>
        /// <returns></returns>
        internal static int CompareStages(ScheduledStageLevel a_x, ScheduledStageLevel a_y)
        {
            return Comparer<int>.Default.Compare(a_x.Stage, a_y.Stage);
        }
    }

    // [USAGE_CODE] CancelSimulationEnum: An enumeration used to specify whether the simulation should be canceled.
    private enum CancelSimulationEnum
    {
        Continue,

        /// <summary>
        /// Cancel the simulation. The modifications to this object must be undone as it's in an unknown state.
        /// </summary>
        Cancel,

        /// <summary>
        /// This activity cannot schedule due to the before-clean segment not having capacity.
        /// </summary>
        CacheDispatchInfo,
        
        /// <summary>
        /// The activity is eligible to schedule except for a cleanout duration that needs to be appended to the previous block.
        /// </summary>
        ContinueOnlyDueToCleanout,
    }

    /// <summary>
    /// Once setup all simulations pass through this this function. It starts up the processing of events.
    /// Prerequisites
    /// Calls to:
    /// SetupInProcessActivities();
    /// </summary>
    /// <param name="startTime">Though the simulation always rebuilds the entire schedule, things up to this point are simulated sequentially.</param>
    /// <param name="lockedActivities">Activities locked to resources.</param>
    /// <param name="sequentialResourceActivities"></param>
    /// <param name="availableResources">Resources that are available for simulation.</param>
    /// <param name="simulationType"></param>
    private CancelSimulationEnum Simulate(
        long a_startTime,
        ResourceActivitySets a_sequentialResourceActivities,
        SimulationType a_simType,
        ScenarioBaseT a_t)
    {
        PT.Common.Testing.Timing ts = CreateTiming(".Simulate");

        if (a_t != null)
        {
            m_lastSimulationDateTicks = a_t.TimeStamp.Ticks;
            m_simulationTransmission = a_t;
        }

        #if EVENTS_COUNT
            ClearEventTypeCount(a_simType.ToString());
        #endif

        ++m_nbrOfSimulations;
        NbrOfSimulationsSinceStartup = NbrOfSimulationsSinceStartup + 1;

        long totalActivitiesToSchedule = GetNbrOfActivitiesToSchedule();

        m_simulationProgress = new SimulationProgress(this, a_simType, a_t, totalActivitiesToSchedule, m_nbrOfSimulationsSinceStartup, ScenarioOptions.SimulationProgressReportFrequency, a_t.SuppressEvents);

        try
        {
            m_activeSimulationType = a_simType;
            InitAllActivitiesComparableDispatching();

            JobManager.ResetSimulationStateVariables2();

            bool multipleStages = m_stages.Length > 1;

            /// If there are multiple stages, a list of scheduled MOs is created along with the stage the MO was scheduled in.
            /// This is used to update the need dates of sub-component jobs.
            /// The intended usage is as follows:
            /// The lowest level sub-component jobs are scheduled first followed by the next higher level jobs up to the 
            /// finished goods jobs. Then from the finished goods jobs, the need dates of sub-component jobs are set to
            /// when they're actually needed.
            List<ScheduledStageLevel> scheduledMOAndStageLevel = new ();

            // [STAGE_CODE]: Simulate each stage.
            for (int stage = 0; stage < m_stages.Length; ++stage)
            {
                // This part of the initialization needs to be done right away to reset the sim clock before adding events.
                SimulationInitialization1();

                StageNbr = stage;
                StageSet mrs = m_stages[StageNbr];
                StageNbr = stage;

                if (stage != 0)
                {
                    m_events.Clear();
                    m_events.InitialInsertionModeStart();
                    //WarehouseManager.SimulationStageInitialization();

                    // [STAGE_CODE]: Add events of the next stage to the event queue.
                    while (mrs.EventCount > 0)
                    {
                        EventBase evt = mrs.GetNextEvent();
                        AddEvent(evt);
                    }
                }

                bool isImport = a_t is ImportT;

                ResourceActivitySets filteredSequentialResourceActivities = FilterResourceActivitySetsDownToStage(a_sequentialResourceActivities, mrs);
                if (SimulateStage(SimClock, a_startTime, filteredSequentialResourceActivities, mrs, m_activeSimulationType, isImport) == CancelSimulationEnum.Cancel)
                {
                    return CancelSimulationEnum.Cancel;
                }

                if (multipleStages)
                {
                    // Add scheduled MOs to the set MOs that have been scheduled.
                    foreach (Job j in JobManager)
                    {
                        foreach (ManufacturingOrder m in j.ManufacturingOrders)
                        {
                            if (!m.ScheduledEndOfStageHandlingComplete && m.Scheduled) // ****************************************** MO.Scheduled should have been a function and is heavier than a property. Consider flagging scheduled mos during scheduling.
                            {
                                scheduledMOAndStageLevel.Add(new ScheduledStageLevel(m, StageNbr));
                                m.ScheduledEndOfStageHandlingComplete = true;
                            }
                        }
                    }
                }

                ManufacturingOrder.PostSimStageChangeTypes jobChangeTypes = ManufacturingOrder.PostSimStageChangeTypes.None;
                int finalStageIdx = m_stages.Length - 1;
                jobChangeTypes = PostSimStageCustExecute(a_simType, a_t, StageNbr, finalStageIdx);
                if (jobChangeTypes != ManufacturingOrder.PostSimStageChangeTypes.None && m_stages.Length > 1)
                {
                    List<Resource> resources = PlantManager.GetResourceArrayList();
                    foreach (Resource res in resources)
                    {
                        if (jobChangeTypes != ManufacturingOrder.PostSimStageChangeTypes.None && multipleStages)
                        {
                            // It's possible the changes could affect the values of constant dispatch values, which are only calculated once at the start of a simulate. In this case, 
                            // a customization may have changed the values, so the constant dispatchers need to recalculate the values and reorgainize themselves so they continue to 
                            // provide the next best scheduling choice.
                            BalancedCompositeDispatcherDefinition dispatcherDefinition = res.ActiveDispatcher.DispatcherDefinition as BalancedCompositeDispatcherDefinition;
                            //The constant dispatcher definition type is not being used. It may be re-instated if there is a performance benefit.
                            //if (dispatcherDefinition != null)
                            //{
                            //    if (dispatcherDefinition.WeightCompositeType == BalancedCompositeDispatcherDefinition.WeightCompositeTypeEnum.Constant)
                            //    {
                            //        PT.Scheduler.Simulation.Dispatcher.ConstantCompositeDispatcher dispatcher = (PT.Scheduler.Simulation.Dispatcher.ConstantCompositeDispatcher)res.ActiveDispatcher;
                            //        dispatcher.Updatekeys(SimClock);
                            //    }
                            //}
                        }
                    }
                }
            }

            scheduledMOAndStageLevel.Sort(ScheduledStageLevel.CompareStages);

            // Update the need dates of sub-component jobs.
            for (int moI = scheduledMOAndStageLevel.Count - 1; moI >= 0; --moI)
            {
                ScheduledStageLevel ssl = scheduledMOAndStageLevel[moI];
                if (ssl.MO.UpdateSubJobSettings(SimClock, ScenarioOptions, new ScenarioDataChanges())) //Datachanges are not required because a simulation complete will be fired
                {
                    m_updatedSubJobNeedDates = true;
                }
            }

            // Write a copy of the scenario right after a simulation occurs.
            UnitTestHandling();
        }
        finally
        {
            SetScheduledStatusAndUnschedPartiallySchedJobs();

            m_activeSimulationType = SimulationType.None;
            m_simulationTransmission = null;
        }

        m_warehouseManager.SimulationComplete();

        if (!m_ignorePostSimulationNotification)
        {
            PostSimulationNotification();
        }

        CleanupInProcessActivities();

        // Reset values that must be reset after a simulation completes.
        JobManager.PostSimulationInitialization();

        StopTiming(ts, false);

        m_simulationProgress.SchedulingComplete();

        LogMaxDelayViolators();

        EndOfSimCustExecute(a_simType, a_t);

        #if TEST
			events.WriteAdds();
        #endif

        #if TEST
            ++_appliedSimulates;
        #endif

        #if DEBUG
        m_batchManager.TestBatches();
        #endif

        #if TEST
            CalcEventSummary();
        #endif
        return CancelSimulationEnum.Continue;
    }

    #if TEST
        void CalcEventSummary()
        {
            List<EventSummary> evtSummary = new List<EventSummary>();
            foreach (System.Collections.Generic.KeyValuePair<long, EventBase> pair in m_allEvents)
            {
                EventBase evt = pair.Value;

                int lastIdx = evtSummary.Count - 1;
                if (lastIdx >= 0)
                {
                    EventSummary lastEvtSummary = evtSummary[lastIdx];
                    if (lastEvtSummary.Equal(evt))
                    {
                        lastEvtSummary.Increment(evt);
                    }
                    else
                    {
                        EventSummary ne = new EventSummary(evt);
                        evtSummary.Add(ne);
                    }
                }
                else
                {
                    evtSummary.Add(new EventSummary(evt));
                }
            }
        }

        class EventSummary
        {
            internal EventSummary(EventBase a_evt)
            {
                Time = a_evt.Time;
                className = a_evt.GetType().Name;
                m_events.Add(a_evt);
            }

            internal long Time
            {
                get;
                set;
            }

            internal string className
            {
                get;
                set;
            }

            long m_count;
            internal long Count
            {
                get { return m_events.Count; }
            }

            internal void Increment(EventBase a_e)
            {
                m_events.Add(a_e);
            }

            List<EventBase> m_events = new List<EventBase>();

            internal bool Equal(EventBase a_b)
            {
                if (a_b.Time == Time)
                {
                    if (a_b.GetType().Name == className)
                    {
                        return true;
                    }
                }
                return false;
            }

            public override string ToString()
            {
                string s = PT.Common.DateTimeHelper.ToLongLocalTimeFromUTCTicks(Time);
                return s;
            }
        }
    #endif

    /// <summary>
    /// Determine the total number of activities to schedule.
    /// </summary>
    /// <returns></returns>
    private long GetNbrOfActivitiesToSchedule()
    {
        long total = 0;
        for (int jobManagerI = 0; jobManagerI < m_jobManager.Count; jobManagerI++)
        {
            ManufacturingOrderManager moCollection = m_jobManager[jobManagerI].ManufacturingOrders;

            for (int moI = 0; moI < moCollection.Count; moI++)
            {
                ManufacturingOrder mo = moCollection[moI];

                if (mo.UnscheduledMOMarker // The MO was unscheduled and will be rescheduled. For instance in a TimeAdjustment.
                    ||
                    mo.ManufacturingOrderReleasedEventScheduled) // In the case of an Optimize, new unscheduled MOs might also be scheduled, all  the MOReleaseEvents are added in the Optimize function.
                {
                    total += mo.GetNbrOfActivitiesToSchedule();
                }
            }
        }

        return total;
    }

    /// <summary>
    /// This function should be called after any simulation that may result in JIT start date changes. For instance, different resources being used to schedule an activity, changes to capacity, etc.
    /// </summary>
    private void RecalculateJITStartTimes(IScenarioDataChanges a_dataChanges)
    {
        if (ScenarioOptions.RecalculateJITOnOptimizeEnabled)
        {
            PT.Common.Testing.Timing tsCalculateJitTime = CreateTiming(".CalculateJitTime");

            if (ScenarioOptions.TrackSubComponentSourceMOs)
            {
                UpdateSubJobRequiredDatePriorityAndHotFlag(Clock, a_dataChanges);
            }

            //Recalculation Jit times since they depend on the scheduled resources
            m_jobManager.CalculateJitTime(Clock, true);

            StopTiming(tsCalculateJitTime, false);
        }
    }

    #region Update sub-job required date, priority, and hot flag.
    private void UpdateSubJobRequiredDatePriorityAndHotFlag(long a_simClock, IScenarioDataChanges a_dataChanges)
    {
        List<ManufacturingOrder> usesSubComponentsOrSubComponentMOs = new ();
        // Add all the unscheduled activities to the reschedule activities list.

        //1) Sort by finished date if scheduled, need date
        SortedList<long, List<ManufacturingOrder>> sortedMoCollection = new SortedList<long, List<ManufacturingOrder>>();
        foreach (Job job in m_jobManager.JobEnumerator)
        {
            ManufacturingOrderManager moCollection = job.ManufacturingOrders;

            for (int moI = 0; moI < moCollection.Count; moI++)
            {
                ManufacturingOrder mo = moCollection[moI];
                if (mo.GetMaterialRequirements().Count == 0)
                {
                    continue;
                }

                if (mo.Scheduled)
                {
                    if (sortedMoCollection.TryGetValue(mo.ScheduledEnd, out List<ManufacturingOrder> manufacturingOrders))
                    {
                        manufacturingOrders.Add(mo);
                    }
                    else
                    {
                        sortedMoCollection.Add(mo.ScheduledEnd, new List<ManufacturingOrder> { mo });
                    }
                }
                else
                {
                    if (sortedMoCollection.TryGetValue(mo.NeedDateTicks, out List<ManufacturingOrder> manufacturingOrders))
                    {
                        manufacturingOrders.Add(mo);
                    }
                    else
                    {
                        sortedMoCollection.Add(mo.NeedDateTicks, new List<ManufacturingOrder> { mo });
                    }
                }
                //if (mo.SourceSubComponent || mo.GetMaterialRequirements().Count > 0)
                //{
                //    usesSubComponentsOrSubComponentMOs.Add(mo);
                //}
            }
        }

        //if (usesSubComponentsOrSubComponentMOs.Count > 0)
        {
            //usesSubComponentsOrSubComponentMOs.Sort(ReverseSortCompareMOsByLevel);

            foreach (KeyValuePair<long, List<ManufacturingOrder>> moCollections in sortedMoCollection)
            {
                UpdateJITAndSubJobSettings(a_simClock, moCollections.Value, a_dataChanges);
            }

            //int curLevel = usesSubComponentsOrSubComponentMOs[0].SubComponentLevel;
            //List<ManufacturingOrder> mosAtCurLevel = new ();

            //for (int moI = 0; moI < usesSubComponentsOrSubComponentMOs.Count; ++moI)
            //{
            //    ManufacturingOrder mo = usesSubComponentsOrSubComponentMOs[moI];

            //    if (mo.SubComponentLevel == curLevel)
            //    {
            //        mosAtCurLevel.Add(mo);
            //    }
            //    else
            //    {
            //        UpdateJITAndSubJobSettings(a_simClock, mosAtCurLevel, a_dataChanges);

            //        mosAtCurLevel.Clear();
            //        mosAtCurLevel.Add(mo);
            //        curLevel = mo.SubComponentLevel;
            //    }
            //}

            //UpdateJITAndSubJobSettings(a_simClock, mosAtCurLevel, a_dataChanges);
        }
    }

    private void UpdateJITAndSubJobSettings(long a_simClock, List<ManufacturingOrder> a_mosAtCurLevel, IScenarioDataChanges a_dataChanges)
    {
        for (int jitMOI = 0; jitMOI < a_mosAtCurLevel.Count; ++jitMOI)
        {
            ManufacturingOrder jitMO = a_mosAtCurLevel[jitMOI];

            if (jitMO.Job.SubComponentNeedDateSet)
            {
                jitMO.CalculateJitTimes(a_simClock, false);
            }
        }

        for (int updateMOI = 0; updateMOI < a_mosAtCurLevel.Count; ++updateMOI)
        {
            ManufacturingOrder updateMO = a_mosAtCurLevel[updateMOI];
            updateMO.UpdateSubJobSettings(a_simClock, ScenarioOptions, a_dataChanges);
        }
    }

    private static int ReverseSortCompareMOsByLevel(ManufacturingOrder a_m1, ManufacturingOrder a_m2)
    {
        if (a_m1.SubComponentLevel < a_m2.SubComponentLevel)
        {
            return 1;
        }

        if (a_m1.SubComponentLevel > a_m2.SubComponentLevel)
        {
            return -1;
        }

        return 0;
    }
    #endregion

    // [STAGE_CODE]: member variables.

    /// <summary>
    /// [STAGE_CODE]: Used to store the events to be run in each stage. Starting at 0, each index corresponds to a simulation assigned SimStage number which is a compressed version of Resource.StageNbr. This
    /// is done because Resource.StageNbr doesn't have to be continuous.
    /// </summary>
    private StageSet[] m_stages;

    /// <summary>
    /// [STAGE_CODE]: The stage number being scheduled by the simulation.
    /// </summary>
    private int m_stageNbr;

    private int StageNbr
    {
        get => m_stageNbr;
        set => m_stageNbr = value;
    }

    /// <summary>
    /// [STAGE_CODE]: If there is more than 1 stage, the first index corresponds to Item.StageArrayIndex. The second index corresponds to a stage and the value stored indicates whether the item is required
    /// in the stage.
    /// m_stageNbr is initialized in SimulationInitialization1 because during an optimize, its value is used by AddEvent() prior to the call to this method.
    /// </summary>
    private BoolMaxtrix m_stagesItemRequiredIn;

    /// <summary>
    /// [STAGE_CODE]: Initialize the simulation for stages and throw SimulationValidationExceptions if there are problems that will prevent the simulation from working.
    /// m_stageNbr is initialized in SimulationInitialization1 because during an optimize, its value is used by AddEvent() prior to the call to this method.
    /// </summary>
    /// <param name="a_availableResources"></param>
    /// <param name="a_jobsToSchedule"></param>
    private void SimulationInitializationOfStages(MainResourceSet a_availableResources, List<Job> a_jobsToSchedule)
    {
        StageSimulationValidation(a_jobsToSchedule);
        SimulationInitializationOfStagedResourceArray(a_availableResources);
    }

    /// <summary>
    /// [STAGE_CODE]: A helper function used to validate stages are correctly configured.
    /// </summary>
    /// <param name="a_jobsToSchedule"></param>
    private void StageSimulationValidation(List<Job> a_jobsToSchedule)
    {
        if (PlantManager.ContainsMultipleStages())
        {
            ItemManager.SimulationStageStageArrayInitialization();

            // For each MO one or more of the paths that can be released will be tested depending on whether the multiple Alternate Path release functionality is in use.
            List<AlternatePath> pathsToTest = new ();

            foreach (Job job in a_jobsToSchedule)
            {
                if (job.ManufacturingOrders.Count > 1)
                {
                    // Otherwise Job events would need to be played in multiple stages.
                    // I'm not ready to play the same event in multiple stages. I'd need to go back and clear release flags.
                    throw new SimulationValidationException("2560", new object[] { job.Name });
                }

                #if TEST
                    // LARRY_STAGING_TODO delete?
                #endif

                for (int moI = 0; moI < job.ManufacturingOrders.Count; ++moI)
                {
                    ManufacturingOrder mo = job.ManufacturingOrders[moI];
                    if (ReleaseAlternatePaths(mo))
                    {
                        AlternatePathDefs.AutoUsePathEnum[] usePathEnums = new[] { AlternatePathDefs.AutoUsePathEnum.ReleaseOffsetFromDefaultPathsLatestRelease, AlternatePathDefs.AutoUsePathEnum.RegularRelease };
                        mo.AlternatePaths.FindAutoUsePaths(pathsToTest, usePathEnums);
                        bool currentInList = false;
                        foreach (AlternatePath path in pathsToTest)
                        {
                            if (path == mo.CurrentPath)
                            {
                                currentInList = true;
                                break;
                            }
                        }

                        if (!currentInList)
                        {
                            pathsToTest.Add(mo.CurrentPath);
                        }
                    }
                    else
                    {
                        pathsToTest.Add(mo.CurrentPath);
                    }

                    foreach (AlternatePath path in pathsToTest)
                    {
                        StageSimulationValidationHelper_ValidatePath(job, mo, path);
                    }
                }
            }
        }
    }

    /// <summary>
    /// [STAGE_CODE]: Verify that each operation withing a path's resource requirements are limited to a single stage.
    /// </summary>
    /// <param name="job"></param>
    /// <param name="mo"></param>
    /// <param name="ap"></param>
    private void StageSimulationValidationHelper_ValidatePath(Job job, ManufacturingOrder mo, AlternatePath ap)
    {
        for (int nodeI = 0; nodeI < ap.NodeCount; ++nodeI)
        {
            AlternatePath.Node node = ap.GetNodeByIndex(nodeI);
            InternalOperation op = (InternalOperation)node.Operation;
            int stage = int.MinValue;

            for (int actI = 0; actI < op.Activities.Count; ++actI)
            {
                InternalActivity act = op.Activities.GetByIndex(actI);
                ResReqsPlantResourceEligibilitySets rrs = act.ResReqsEligibilityNarrowedDuringSimulation;

                for (int rrI = 0; rrI < rrs.Count; ++rrI)
                {
                    PlantResourceEligibilitySet pres = rrs[rrI];
                    for (int plantI = 0; plantI < pres.Count; ++plantI)
                    {
                        SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = pres.GetEnumerator();
                        while (ersEtr.MoveNext())
                        {
                            EligibleResourceSet ers = ersEtr.Current.Value;
                            for (int resI = 0; resI < ers.Count; ++resI)
                            {
                                InternalResource res = ers[resI];
                                if (stage == int.MinValue)
                                {
                                    stage = res.Stage;
                                }
                                else if (stage != res.Stage)
                                {
                                    // If this condition starts being encountered, you can add a list of each resource requirements eligible resources and assigned stage to the error message.
                                    throw new SimulationValidationException("2561", new object[] { job.Name, mo.Name, op.Name });
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Each element of the ArrayList represents a stage and contains a MainResourceSet. Stages are from left to right in the returned ArrayList.
    /// </summary>
    /// <param name="availableResources"></param>
    private void SimulationInitializationOfStagedResourceArray(MainResourceSet a_availableResources)
    {
        ArrayList sortedList = new ();

        for (int i = 0; i < a_availableResources.Count; ++i)
        {
            InternalResource ir = a_availableResources[i];
            sortedList.Add(ir);
        }

        sortedList.Sort(new StageComparer());

        int prevStage = int.MinValue;
        int simStage = -1;
        ArrayList stageList = new ();
        StageSet ss = null;

        // Create the stage list (it has an entry for each stage) 
        // and assign a continuous set of stage numbers to each resource. For instance if there 
        // are 2 stages, stage 0 and stage 100. All the stage 0 resources will be assigned to 
        // stage 0 and all stage 100 resources will be assigned to stage 1.
        for (int i = 0; i < sortedList.Count; ++i)
        {
            InternalResource ir = (InternalResource)sortedList[i];

            if (ir.Stage != prevStage)
            {
                ++simStage;
                ss = new StageSet();
                stageList.Add(ss);
                prevStage = ir.Stage;
            }

            ir.SimStage = simStage;
            ss.Add(ir);
        }

        m_stages = new StageSet[stageList.Count];
        for (int stageI = 0; stageI < stageList.Count; ++stageI)
        {
            m_stages[stageI] = (StageSet)stageList[stageI];
        }

        if (stageList.Count > 1)
        {
            if (ItemManager.Count == 0)
            {
                m_errorReporter.LogException(new SchedulingWarningException("Resource staging cannot be initialized because there aren't any items".Localize()), null);
            }
            else
            {
                m_stagesItemRequiredIn = new BoolMaxtrix(ItemManager.Count, stageList.Count);
                SimulationInitializationOfItemStages();
            }
        }
    }

    private void SimulationInitializationOfItemStages()
    {
        for (int jobI = 0; jobI < JobManager.Count; ++jobI)
        {
            Job job = JobManager[jobI];
            for (int moI = 0; moI < job.ManufacturingOrders.Count; ++moI)
            {
                ManufacturingOrder mo = job.ManufacturingOrders[moI];
                for (int pathI = 0; pathI < mo.AlternatePaths.Count; ++pathI)
                {
                    AlternatePath ap = mo.AlternatePaths[pathI];
                    if (ap.EligibleResources_IsSatisfiable())
                    {
                        for (int nodeI = 0; nodeI < ap.NodeCount; ++nodeI)
                        {
                            AlternatePath.Node node = ap[nodeI];
                            InternalOperation op = (InternalOperation)node.Operation;

                            // LARRY_STAGING_TODO This is the line the error occurs on when this fx is called with some recordings. The fx call made in the line returns null.
                            // It shouldn't ever do this for jobs that are already scheduled.
                            InternalResource firstEligiblePrimaryRes = op.ResourceRequirements.GetFirstEligiblePrimaryResource();
                            int itemStage = op.ResourceRequirements.GetFirstEligiblePrimaryResource().SimStage;

                            for (int matlI = 0; matlI < op.MaterialRequirements.Count; ++matlI)
                            {
                                Item item = op.MaterialRequirements[matlI].Item;
                                m_stagesItemRequiredIn.Set(item.StageArrayIndex, itemStage, true);

                                // Verify an Item isn't required in multiple stages.
                                ValidateItemIsntRequiredInMultipleStages(item);

                                #if TEST
                                    // LARRY_STAGING_TODO ? Add code to valiate that an operation's material requirements are limited to a single stage.
                                #endif
                            }
                        }
                    }
                }
            }
        }
    }

    private void ValidateItemIsntRequiredInMultipleStages(Item item)
    {
        int numberOfStages = 0;
        for (int stageI = 0; stageI < m_stagesItemRequiredIn.Columns; ++stageI)
        {
            if (m_stagesItemRequiredIn.Get(item.StageArrayIndex, stageI))
            {
                ++numberOfStages;
                if (numberOfStages > 1)
                {
                    // The item is required in more than 1 stage. List all the activities the item is required in.
                    throw new SimulationValidationException("2562", new object[] { item.Name });
                    #if TEST
                        // LARRY_STAGING_TODO . Test this and add additional information to this error messages, such as the stages it's in and the resources and their stages.
                    #endif
                }
            }
        }
    }

    /// <summary>
    /// Returns a new ResourceActivitySet filtered down to the resources in a MainResourceSet.
    /// </summary>
    /// <param name="ras"></param>
    /// <param name="mrs"></param>
    /// <returns></returns>
    private ResourceActivitySets FilterResourceActivitySetsDownToStage(ResourceActivitySets a_rass, MainResourceSet a_mrs)
    {
        Hashtable mrsHT = a_mrs.CreateHash();
        ResourceActivitySets rassFiltered = new ();

        for (int i = 0; i < a_rass.Count; ++i)
        {
            ResourceActivitySet ras = a_rass[i];

            if (mrsHT.Contains(ras.Resource))
            {
                rassFiltered.Add(ras);
            }
        }

        return rassFiltered;
    }

    private class StageComparer : IComparer
    {
        #region IComparer Members
        public int Compare(object a_x, object a_y)
        {
            InternalResource ir1 = (InternalResource)a_x;
            InternalResource ir2 = (InternalResource)a_y;

            if (ir1.Stage < ir2.Stage)
            {
                return -1;
            }

            if (ir1.Stage == ir2.Stage)
            {
                return 0;
            }

            return 1;
        }
        #endregion
    }

    /// <summary>
    /// Executes PostSimStageCustomizations. It should be called after a Simulation Stage has completed.
    /// </summary>
    /// <param name="a_simType"></param>
    /// <param name="a_simT"></param>
    private ManufacturingOrder.PostSimStageChangeTypes PostSimStageCustExecute(SimulationType a_simType, ScenarioBaseT a_simT, int a_curSimStageIdx, int a_finalSimStageIdx)
    {
        ManufacturingOrder.PostSimStageChangeTypes jobChangeTypes = ManufacturingOrder.PostSimStageChangeTypes.None;
        if (m_extensionController.RunPostSimStageExtensions)
        {
            m_extensionController.PostSimStageSetup(a_simType, a_simT, this, a_curSimStageIdx, a_finalSimStageIdx);

            m_jobManager.PostSimStageCust(Clock, a_simType, a_simT, this, a_curSimStageIdx, a_finalSimStageIdx);
            m_plantManager.PostSimStageCust(a_simType, a_simT, this, a_curSimStageIdx, a_finalSimStageIdx);

            m_extensionController.PostSimStageCleanup(a_simType, a_simT, this, a_curSimStageIdx, a_finalSimStageIdx);
        }

        return jobChangeTypes;
    }

    /// <summary>
    /// Executes EndOfSimulationCustomization. It should be called after a Simulation has completed.
    /// </summary>
    /// <param name="a_simType"></param>
    /// <param name="a_simT"></param>
    private void EndOfSimCustExecute(SimulationType a_simType, ScenarioBaseT a_simT)
    {
        if (ExtensionController.RunEndOfSimulationExtension)
        {
            m_extensionController.EndOfSimulationSetup(a_simType, a_simT, this);

            m_jobManager.EndOfSimulationCustExecute(SimClock, a_simType, a_simT, this);
            m_plantManager.EndOfSimulationCustExecute(a_simType, a_simT, this);

            m_extensionController.EndOfSimulationCleanup(a_simType, a_simT, this);
        }
    }

    private CancelSimulationEnum SimulateStage(
        long a_simClock,
        long a_startTime,
        ResourceActivitySets a_sequentialResourceActivities,
        MainResourceSet a_availableResources,
        SimulationType a_simulationType,
        bool a_isImport
        )
    {
        // Add an event for the Planning Horizon
        m_withinPlanningHorizon = true;
        AddEvent(new PlanningHorizonEvent(EndOfPlanningHorizon));

        AddInitialResourceAvailableEvents(a_startTime, a_availableResources);
        AddInitialResourceCleanoutEvents(a_startTime, a_availableResources);
        m_resourceDispatchers = GetReadyDispatchers(a_availableResources);

        SimulationInitialization1();

        CancelSimulationEnum cancel = CancelSimulationEnum.Continue;
        if (a_simulationType == SimulationType.Optimize)
        {
            cancel = ProcessEvents(a_isImport);
        }
        else if (a_simulationType == SimulationType.Move ||
                 a_simulationType == SimulationType.MoveAndExpedite ||
                 a_simulationType == SimulationType.TimeAdjustment ||
                 a_simulationType == SimulationType.ClockAdvance ||
                 a_simulationType == SimulationType.Expedite ||
                 a_simulationType == SimulationType.ConstraintsChangeAdjustment ||
                 a_simulationType == SimulationType.JitCompress)
        {
            AddMOReleaseEventsHoldsAndJobHoldsWhereUnscheduledMOMarkerIsTrue(a_simClock, a_startTime);
            SequentialSimulationInitialization(a_startTime, a_sequentialResourceActivities);
            SetupMaterialConstraints(a_sequentialResourceActivities, 0);
            cancel = ProcessEvents(a_isImport);
        }

        return cancel;
    }

    /// <summary>
    /// Every resource's active dispatcher.
    /// </summary>
    [Common.Attributes.DebugLogging(Common.Attributes.EDebugLoggingType.None)]
    private ReadyActivitiesDispatcher[] m_resourceDispatchers;

    /// <summary>
    /// Return an array containing every resource's active dispatcher.
    /// </summary>
    /// <returns></returns>
    private ReadyActivitiesDispatcher[] GetReadyDispatchers(MainResourceSet a_availableResources)
    {
        //			ArrayList radAL=new ArrayList();
        //			for(int pI=0; pI<this.PlantManager.Count; ++pI)
        //			{
        //				Plant p=PlantManager[pI];
        //				for(int dI=0; dI<p.Departments.Count; ++dI)
        //				{
        //					Department d=p.Departments[dI];
        //					for(int rI=0; rI<d.Resources.Count; ++rI)
        //					{
        //						Resource r=d.Resources[rI];
        //						if(r.Active)
        //						{
        //							radAL.Add(r.ActiveDispatcher);
        //						}
        //					}
        //				}
        //			}
        ReadyActivitiesDispatcher[] rad = new ReadyActivitiesDispatcher[a_availableResources.Count];
        for (int i = 0; i < a_availableResources.Count; ++i)
        {
            rad[i] = a_availableResources[i].ActiveDispatcher;
        }

        return rad;
    }

    /// <summary>
    /// A short-circuit form of TotalActivitiesOnDispatcher
    /// </summary>
    private bool ActivitiesOnDispatcher
    {
        get
        {
            for (int i = 0; i < m_resourceDispatchers.Length; ++i)
            {
                int countTmp = m_resourceDispatchers[i].Count;
                if (countTmp > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
    
    /// <summary>
    /// Adds up the number of activities on all the resources' active dispatchers.
    /// </summary>
    private int TotalActivitiesOnDispatchers
    {
        get
        {
            int count = 0;
            for (int i = 0; i < m_resourceDispatchers.Length; ++i)
            {
                int countTmp = m_resourceDispatchers[i].Count;
                count += countTmp;
            }

            return count;
        }
    }

    private void AddInitialResourceAvailableEvents(long a_startTime, MainResourceSet a_availableResources)
    {
        // Add ready events for each resource at the next time the resource will be available.
        for (int availableResourcesI = 0; availableResourcesI < a_availableResources.Count; ++availableResourcesI)
        {
            Resource mainResource = a_availableResources[availableResourcesI] as Resource;
            if (mainResource != null)
            {
                AddResourceAvailableEventAtNextOnlineTime(mainResource, a_startTime, null);
            }
        }
    }

    private void AddInitialResourceCleanoutEvents(long a_startTime, MainResourceSet a_availableResources)
    {
        // Add ready events for each resource at the next time the resource will be available.
        for (int availableResourcesI = 0; availableResourcesI < a_availableResources.Count; ++availableResourcesI)
        {
            Resource res = a_availableResources[availableResourcesI] as Resource;
            if (res != null)
            {
                if (res.HasCleanoutIntervals())
                {
                    LinkedListNode<ResourceCapacityInterval> maintInterval = res.FindNextCleanoutInterval(a_startTime);
                    if (maintInterval != null)
                    {
                        AddEvent(new ResourceCleanoutEvent(maintInterval.Value.EndDate, res, maintInterval));
                    }
                }
            }
        }
    }

    /// <summary>
    /// During a simulation, this is set to null if more than 1 DispatcherDefinition is in use.
    /// If 1 DispatcherDefinition is in use, this field is set to reference it.
    /// </summary>
    private DispatcherDefinition m_singleDispatcherDefinitionInUse;

    /// <summary>
    /// For use with SimulationResourceInitialization(). Use this to specify which dispatcher resources will use. This is dependant on the type of simulation that's to be performed.
    /// </summary>
    private class SimulationResourceDispatcherUsageArgs
    {
        private SimulationResourceDispatcherUsageArgs(long a_clock)
        {
            Clock = a_clock;
        }

        /// <summary>
        /// Specify a single DispatcherDefinition to use for all resources.
        /// </summary>
        /// <param name="a_clock"></param>
        /// <param name="a_singleDispatcherDefinition"></param>
        internal SimulationResourceDispatcherUsageArgs(long a_clock, DispatcherDefinition a_singleDispatcherDefinition)
            : this(a_clock)
        {
            DispatcherSource = OptimizeSettings.dispatcherSources.OneRule;
            SingleDispatcherDefinition = a_singleDispatcherDefinition;
        }

        /// <summary>
        /// Used when calling SimulationResourceInitialization to specify which dispather resouces should use.
        /// </summary>
        /// <param name="a_clock">The current clock time.</param>
        /// <param name="a_dispatcherSource">You may not specify OneRule. If you want one rule, you have to use the constructor where you provide the rule.</param>
        internal SimulationResourceDispatcherUsageArgs(long a_clock, OptimizeSettings.dispatcherSources a_dispatcherSource)
            : this(a_clock)
        {
            #if DEBUG
            if (a_dispatcherSource == OptimizeSettings.dispatcherSources.OneRule)
            {
                throw new Exception("When using this constructor, OneRule isn't valid. It's automatically set to this when the appropriate constructor is used.");
            }
            #endif
            DispatcherSource = a_dispatcherSource;
        }

        /// <summary>
        /// The current APS clock time.
        /// </summary>
        internal long Clock { get; private set; }

        /// <summary>
        /// Defines the source of the dispather for resources.
        /// </summary>
        internal OptimizeSettings.dispatcherSources DispatcherSource { get; private set; }

        /// <summary>
        /// This is only set when DispatcherSource equals Scheduler.OptimizeSettings.dispatcherSources.ExperimentalRules.OneRule
        /// </summary>
        internal DispatcherDefinition SingleDispatcherDefinition { get; private set; }
    }

    /// <summary>
    /// Initialize the resources for a simulation.
    /// </summary>
    /// <param name="a_dispatcherUsage">Used to indicate which dispather the resource will use.</param>
    private void SimulationResourceInitialization(SimulationResourceDispatcherUsageArgs a_dispatcherUsage)
    {
        m_singleDispatcherDefinitionInUse = a_dispatcherUsage.SingleDispatcherDefinition;
        bool singleDispatcherSet = false;

        // Initialize the resources.
        for (int plantI = 0; plantI < m_plantManager.Count; ++plantI)
        {
            Plant plant = m_plantManager[plantI];

            for (int departmentI = 0; departmentI < plant.Departments.Count; ++departmentI)
            {
                Department department = plant.Departments[departmentI];

                for (int resourceI = 0; resourceI < department.Resources.Count; ++resourceI)
                {
                    Resource machine = department.Resources[resourceI];
                    machine.SimulationInitialization(a_dispatcherUsage.Clock, this, GetPlanningHorizonEndTicks(), ScenarioOptions);

                    if (a_dispatcherUsage.SingleDispatcherDefinition != null)
                    {
                        machine.ActiveDispatcher = a_dispatcherUsage.SingleDispatcherDefinition.CreateDispatcher();
                    }
                    else
                    {
                        switch (a_dispatcherUsage.DispatcherSource)
                        {
                            case OptimizeSettings.dispatcherSources.ExperimentalRuleOne:
                                machine.ActiveDispatcher = machine.ExperimentalDispatcherOne;
                                break;
                            case OptimizeSettings.dispatcherSources.ExperimentalRuleTwo:
                                machine.ActiveDispatcher = machine.ExperimentalDispatcherTwo;
                                break;
                            case OptimizeSettings.dispatcherSources.ExperimentalRuleThree:
                                machine.ActiveDispatcher = machine.ExperimentalDispatcherThree;
                                break;
                            case OptimizeSettings.dispatcherSources.ExperimentalRuleFour:
                                machine.ActiveDispatcher = machine.ExperimentalDispatcherFour;
                                break;
                            default:
                                machine.ActiveDispatcher = machine.Dispatcher;
                                break;
                        }

                        if (singleDispatcherSet)
                        {
                            if (m_singleDispatcherDefinitionInUse != machine.ActiveDispatcher.DispatcherDefinition)
                            {
                                // There's more than 1 Dispather Definition in use.
                                m_singleDispatcherDefinitionInUse = null;
                            }
                        }
                        else
                        {
                            m_singleDispatcherDefinitionInUse = machine.ActiveDispatcher.DispatcherDefinition;
                            singleDispatcherSet = true;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Adds ActivityReleaseEvents for ready activities.
    /// </summary>
    /// <param name="sequentialResourceActivities"></param>
    private void SequentialSimulationInitialization(long a_simStartTicks, ResourceActivitySets a_sequentialResourceActivities)
    {
        for (int sequentialResourceActivitiesI = 0; sequentialResourceActivitiesI < a_sequentialResourceActivities.Count; ++sequentialResourceActivitiesI)
        {
            ResourceActivitySet ras = a_sequentialResourceActivities[sequentialResourceActivitiesI];

            for (int resourceActivitySetI = 0; resourceActivitySetI < ras.Count; ++resourceActivitySetI)
            {
                // [BATCH_CODE]
                List<InternalActivity> actList = ras[resourceActivitySetI];

                for (int actI = 0; actI < actList.Count; ++actI)
                {
                    InternalActivity act = actList[actI];
                    if (act.Released)
                    {
                        long releaseTicks = Math.Max(a_simStartTicks, act.Operation.AlternatePathNode.MaxPredecessorReleaseTicks);
                        AddActivityReleasedEvent(act, releaseTicks);
                    }
                }
            }
        }
    }

    #region Event Type Counting
    #if EVENTS_COUNT
		long z_attemptsToSchedule;
        /// <summary>
        /// The number of events added to the event queue.
        /// The key is the name of the event.
        /// The value is the number of times the event has been added to the event queue.
        /// </summary>
        Dictionary<string, int> z_eventTypeCount = new Dictionary<string, int>();
        string z_typeCountSimType = "";

        //[Conditional("DEBUG")]
        void ClearEventTypeCount(string a_simType)
        {
            if (!PTSystem.Server) return;

            z_eventTypeCount = new Dictionary<string, int>();
            z_typeCountSimType = a_simType;
            z_qtsCounts.Clear();
			z_attemptsToSchedule = 0;
        }

        //[Conditional("DEBUG")]
        void LogEventTypeCount()
        {
            if (!PTSystem.Server) return;

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(z_c_tempEventsFile, true))
            {
                sw.WriteLine(string.Format("--- {0} ---", DateTime.Now.ToString()));
                sw.WriteLine();
                sw.WriteLine(string.Format("SimType: {0}", z_typeCountSimType));
                sw.WriteLine();
                int total = 0;
                foreach (System.Collections.Generic.KeyValuePair<string, int> kv in z_eventTypeCount)
                {
                    sw.WriteLine(string.Format("- {0}: {1}", kv.Key, kv.Value.ToString("#,#")));
                    total += kv.Value;
                }
				sw.WriteLine(string.Format("- AttemptsToSchedule: {0}", z_attemptsToSchedule));
				sw.WriteLine(string.Format("- Progress: {0}", m_simulationProgress));
                sw.WriteLine();
                sw.WriteLine(string.Format("Total: {0}", total.ToString("#,#")));
                sw.WriteLine();
                Dictionary<long, QTSCounts>.Enumerator etr = z_qtsCounts.GetEnumerator();
                long hoizon = GetPlanningHorizonEndTicks();
                while (etr.MoveNext())
                {
                    if (etr.Current.Value.m_ticks >= hoizon)
                    {
                        sw.WriteLine("QtyToStock>Horizon count=" + etr.Current.Value.m_count);
                    }
                }
                sw.WriteLine("----------- END -----------");
                sw.WriteLine();
            }
        }

        const string z_c_tmpDir = "C:\\_tmpAPS";
        const string z_c_tmpEvents = z_c_tmpDir + "\\EventCounts";
        const string z_c_eventsFilePrefix = "events";
        string z_c_tempEventsFile;

        string CreateLogName(string a_dir, string a_prefix)
        {
            const string z_c_logSuffix = "log";
            string fileName;
            int logNbr = 0;
            fileName = CreateEventsLogName(a_dir, a_prefix, logNbr, z_c_logSuffix);
            while (System.IO.File.Exists(fileName))
            {
                logNbr++;
                fileName = CreateEventsLogName(a_dir, a_prefix, logNbr, z_c_logSuffix);
            }
            return fileName;
        }

        string CreateEventsLogName(string a_dir, string a_prefix, int a_logNbr, string a_suffix)
        {
            return a_dir + "\\" + a_prefix + a_logNbr + "." + a_suffix;
        }

        class QTSCounts
        {
            internal QTSCounts(long a_time, long a_count)
            {
                m_ticks = a_time;
                m_count = a_count;
            }
            internal long m_ticks;
            internal long m_count;
        }
        Dictionary<long, QTSCounts> z_qtsCounts = new Dictionary<long, QTSCounts>();
        //[Conditional("DEBUG")]
        void CountEventType(EventBase a_e)
        {
            if (!PTSystem.Server) return;

            string typeName = a_e.GetType().Name;
            if (z_eventTypeCount.ContainsKey(typeName))
            {
                z_eventTypeCount[typeName] = z_eventTypeCount[typeName] + 1;
            }
            else
            {
                z_eventTypeCount.Add(typeName, 1);
            }

            if (a_e is QtyToStockEvent)
            {
                if (z_qtsCounts.ContainsKey(a_e.Time))
                {
                    z_qtsCounts[a_e.Time].m_count = z_qtsCounts[a_e.Time].m_count + 1;
                }
                else
                {
                    z_qtsCounts.Add(a_e.Time, new QTSCounts(a_e.Time, 1));
                }
            }
        }
    #endif
    #endregion

    private readonly EventSet m_events = new ();

    // All events are added to the event set through this function.
    internal void AddEvent(EventBase a_e)
    {
        #if DEBUG
        if (a_e.Time < SimClock)
        {
            throw new Exception("Event before simulation clock exception.");
        }
        #endif
        int stageToBeScheduledIn = StageNbr;

        Job jobErr = null;
        ManufacturingOrder moErr = null;
        InternalOperation ioErr = null;

        if (a_e is ReleaseEvent)
        {
            ReleaseEvent re = (ReleaseEvent)a_e;

            stageToBeScheduledIn = re.Activity.Operation.GetStageToBeScheduledIn(StageNbr);
            ioErr = re.Activity.Operation;
        }
        else if (a_e is OperationEvent)
        {
            OperationEvent oe = (OperationEvent)a_e;
            if (oe.Operation is InternalOperation)
            {
                InternalOperation io = (InternalOperation)oe.Operation;

                stageToBeScheduledIn = io.GetStageToBeScheduledIn(StageNbr);
                ioErr = io;
            }
        }
        else if (a_e is ManufacturingOrderEvent)
        {
            ManufacturingOrderEvent moe = (ManufacturingOrderEvent)a_e;

            stageToBeScheduledIn = moe.ManufacturingOrder.GetFirstStageToScheduleIn(StageNbr);
            moErr = moe.ManufacturingOrder;
        }
        else if (a_e is JobEvent)
        {
            JobEvent je = (JobEvent)a_e;

            stageToBeScheduledIn = je.Job.GetFirstStageToScheduleIn(StageNbr);
            jobErr = je.Job;
        }
        else if (a_e is PredecessorOperationAvailableEvent)
        {
            PredecessorOperationAvailableEvent poa = (PredecessorOperationAvailableEvent)a_e;
            InternalOperation io = poa.Association.Successor.Operation as InternalOperation;
            if (io != null)
            {
                stageToBeScheduledIn = io.GetStageToBeScheduledIn(StageNbr);
                ioErr = io;
            }
        }
        else if (a_e is PredecessorMOAvailableEvent)
        {
            PredecessorMOAvailableEvent pmoa = (PredecessorMOAvailableEvent)a_e;
            if (pmoa.SuccessorMO != null)
            {
                stageToBeScheduledIn = pmoa.SuccessorMO.GetFirstStageToScheduleIn(StageNbr);
                moErr = pmoa.SuccessorMO;
            }
            else
            {
                InternalOperation io = pmoa.SuccessorOperation as InternalOperation;
                if (io != null)
                {
                    stageToBeScheduledIn = io.GetStageToBeScheduledIn(StageNbr);
                    ioErr = io;
                }
            }
        }

        if (stageToBeScheduledIn != StageNbr)
        {
            if (stageToBeScheduledIn < StageNbr)
            {
                StringBuilder sb = new ();

                if (jobErr != null)
                {
                    sb.AppendFormat(Localizer.GetString(string.Format("The Job Name is: {0}.", jobErr.Name)));
                }
                else if (moErr != null)
                {
                    sb.AppendFormat(Localizer.GetString(string.Format("The Job Name is: {0}. The MO Name is: {1}.", moErr.Job.Name, moErr.Name)));
                }
                else if (ioErr != null)
                {
                    sb.AppendFormat(Localizer.GetString(string.Format("The Job Name is: {0}. The MO Name is: {1}. The operation Name is: {2}.", ioErr.ManufacturingOrder.Job.Name, ioErr.ManufacturingOrder.Name, ioErr.Name)));
                }

                throw new SimulationValidationException("2563", new object[] { sb.ToString() });
            }

            m_stages[stageToBeScheduledIn].AddEvent(a_e);
            return;
        }

        #if TEST
            _adeAddedEvents.Add(a_e);
        #endif

        #if TEST
            if (!m_events.InitialInsertionMode && a_e.Time < SimClock)
            {
                throw new PTException("An event was added before the sim clock.");
            }
        #endif

        AddEventHelper(a_e);

        #if TEST
            CountEventType(a_e);
        #endif
    }

    #if TEST
        /// <summary>
        /// Used to sort the m_allEvents test list.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        public class DuplicateKeyComparer<TKey>
                :
             IComparer<TKey> where TKey : IComparable
        {

            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return -1;   // Handle equality as being greater
                else
                    return result;
            }

        }
        System.Collections.Generic.SortedList<long, EventBase> m_allEvents = new SortedList<long, EventBase>(new DuplicateKeyComparer<long>());
    #endif

    private void AddEventHelper(EventBase a_e)
    {
        #if TEST
            m_allEvents.Add(a_e.Time, a_e);
        #endif
        m_events.AddEvent(a_e);
    }

    private SimulationType m_activeSimulationType = SimulationType.None;

    internal SimulationType ActiveSimulationType => m_activeSimulationType;

    private long m_simClock;

    internal long SimClock
    {
        get => m_simClock;
        private set => m_simClock = value;
    }

    #if TEST
        string deSimClock
        {
            get { return PT.Common.DateTimeHelper.UTCDate(SimClock); }
        }
    #endif

    #if TEST
        //***************************************
        // The classes in this section are used for analyzing events for performance purposes.
        //***************************************

        /// <summary>
        /// Represents the set of events that were processed at the same time.
        /// </summary>
        class ProcessedEventSet : IEnumerable<EventBase>
        {
            internal ProcessedEventSet(EventBase a_e)
            {
                Add(a_e);
            }

            /// <summary>
            /// The time the events were processed.
            /// </summary>
            internal long Ticks
            {
                get
                {
                    return m_eventList[0].Time;
                }
            }

            List<EventBase> m_eventList = new List<EventBase>();

            internal void Add(EventBase a_e)
            {
                m_eventList.Add(a_e);
            }

            internal void Clear()
            {
                m_eventList.Clear();
            }

            /// <summary>
            /// Group the events in this class by name.
            /// </summary>
            /// <returns>Each index in the list is the set of events that have the same name.</returns>
            internal List<EventGroup> CalcGroups()
            {
                return CalcGroups(m_eventList);
            }

            /// <summary>
            /// Group the events in a list of events by name.
            /// </summary>
            /// <param name="a_events"></param>
            /// <returns>Each index in the list is the set of events that have the same name.</returns>
            static internal List<EventGroup> CalcGroups(List<EventBase> a_events)
            {
                Dictionary<string, List<EventBase>> groups = new Dictionary<string, List<EventBase>>();

                foreach (EventBase e in a_events)
                {
                    string name = e.GetType().Name;
                    List<EventBase> list;

                    if (groups.ContainsKey(name))
                    {
                        list = groups[name];
                    }
                    else
                    {
                        list = new List<EventBase>();
                        groups.Add(name, list);
                    }

                    list.Add(e);
                }

                List<EventGroup> result = new List<EventGroup>();
                Dictionary<string, List<EventBase>>.Enumerator etr = groups.GetEnumerator();
                while (etr.MoveNext())
                {
                    result.Add(new EventGroup(etr.Current.Value));
                }

                return result;
            }

            public override string ToString()
            {
                return PT.Common.DateTimeHelper.ToLocalTimeFromUTCTicks(m_eventList[0].Time).ToString() + ": " + Scheduled.ToString() + m_eventList.Count;
            }

            public IEnumerator<EventBase> GetEnumerator()
            {
                return m_eventList.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            internal ScheduledActs Scheduled
            {
                get;
                set;
            }
        }

        internal enum ScheduledActs
        {
            Unset,
            True,
            False
        }

        /// <summary>
        /// A group of events that have the same time.
        /// </summary>
        class EventGroup : IEnumerable<EventBase>
        {
            internal EventGroup(List<EventBase> a_events)
            {
                m_events = a_events;
            }

            List<EventBase> m_events;

            public override string ToString()
            {
                return m_events.Count + " " + m_events[0].GetType().Name + " " + PT.Common.DateTimeHelper.ToLocalTimeFromUTCTicks(m_events[0].Time).ToString() + ": " + m_events.Count;
            }

            internal long Ticks
            {
                get
                {
                    return m_events[0].Time;
                }
            }

            internal ScheduledActs Scheduled
            {
                get;
                set;
            }

            public IEnumerator<EventBase> GetEnumerator()
            {
                return m_events.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        /// <summary>
        /// Sets of all events at all event times during the simulation.
        /// Each element in this set contains the set of events that occurred at the same time.
        /// </summary>
        class ProcessedEvents
        {
            List<ProcessedEventSet> m_processedEvents = new List<ProcessedEventSet>();

            internal void Add(EventBase a_e)
            {
                if (m_processedEvents.Count > 0)
                {
                    ProcessedEventSet lastSet = m_processedEvents[m_processedEvents.Count - 1];
                    if (lastSet.Ticks != a_e.Time)
                    {
                        lastSet = new ProcessedEventSet(a_e);
                        m_processedEvents.Add(lastSet);
                    }

                    lastSet.Add(a_e);
                }
                else
                {
                    ProcessedEventSet lastSet = new ProcessedEventSet(a_e);
                    m_processedEvents.Add(lastSet);
                }
            }

            internal void Clear()
            {
                m_processedEvents.Clear();
            }

            internal List<ProcessedEventSet>.Enumerator GetEnumerator()
            {
                return m_processedEvents.GetEnumerator();
            }

            internal List<EventGroup> CalcGroups()
            {
                List<EventBase> list = new List<EventBase>();
                foreach (ProcessedEventSet p in m_processedEvents)
                {
                    foreach (EventBase e in p)
                    {
                        list.Add(e);
                    }
                }

                List<EventGroup> groups = ProcessedEventSet.CalcGroups(list);
                return groups;
            }
        }

        /// <summary>
        /// Contains all of the events that have been processed.
        /// </summary>
        ProcessedEvents m_processedEvents = new ProcessedEvents();

        /// <summary>
        /// All of the times activities have been scheduled.
        /// </summary>
        HashSet<long> m_scheduledTimes = new HashSet<long>();

        /// <summary>
        /// All of the activities that have been scheduled.
        /// </summary>
        HashSet<InternalActivity> m_activitiesScheduled = new HashSet<InternalActivity>();

        /// <summary>
        /// All of the MOs that have been scheduled.
        /// </summary>
        HashSet<ManufacturingOrder> m_mosScheduled = new HashSet<ManufacturingOrder>();

        /// <summary>
        /// The number of times attempts have been made to scheduled activiites.
        /// </summary>
        long m_totalAttemptsToScheduleActivity;

    #endif
    private readonly List<BlockReservationEvent> m_blockReservations = new ();

    #if DEBUG
    private bool z_supressActivitiesOnDispatcherException;
    #endif

    private CancelSimulationEnum ProcessEvents(bool a_isImport)
    {
        #if TEST
            m_processedEvents.Clear();
            m_scheduledTimes.Clear();
            m_activitiesScheduled.Clear();
            m_mosScheduled.Clear();
            m_totalAttemptsToScheduleActivity = 0;
        #endif

        m_events.InitialInsertionModeComplete();
        EventBase lastProcessedEvent = null;

        bool eventsToProcess = ProcessEventsOnQueue();
        while (eventsToProcess)
        {
            EventBase peekEvent = m_events.PeekMin();

            //CalcEventSummary();

            #if TEST
                if (peekEvent.Time < SimClock)
                {
                    throw new PTException("An event has occurred before the simulation clock.");
                }
            #endif
            if (lastProcessedEvent != null && lastProcessedEvent.Time != peekEvent.Time)
            {
                if (ScheduleReadyResources(lastProcessedEvent.Time, a_isImport) == CancelSimulationEnum.Cancel)
                {
                    return CancelSimulationEnum.Cancel;
                }

                peekEvent = m_events.PeekMin();
            }

            SimClock = peekEvent.Time;

            EventBase nextEvent = m_events.DeleteMin();
            ProcessEvent(nextEvent);
            #if TEST
                m_processedEvents.Add(nextEvent);
            #endif

            lastProcessedEvent = nextEvent;

            #if TEST
                    _adePlayedEvents.Add(peekEvent);
            #endif

            eventsToProcess = ProcessEventsOnQueue();
            if (!eventsToProcess)
            {
                if (ScheduleReadyResources(lastProcessedEvent.Time, a_isImport) == CancelSimulationEnum.Cancel)
                {
                    return CancelSimulationEnum.Cancel;
                }

                // Scheduling resources may have created additional events to process.
                eventsToProcess = ProcessEventsOnQueue();
            }
        }

        // After processing all the events attempt to fulfill any remaining quantity requirements.
        //ConsumeQtyReqs(SimClock);

        #if DEBUG
        if (!z_supressActivitiesOnDispatcherException && TotalActivitiesOnDispatchers != 0)
        {
            throw new Exception("ProcessEvents() exited while there were still activities available to schedule.");
        }

        if (m_materialConstrainedActivities.Count != 0)
        {
            throw new DebugException("ProcessEvents() exited while there were still activities waiting to be put back on dispatchers");
        }
        #endif

        #if TEST
            // Examine the events that occurred.
            List<EventGroup> allGroups = m_processedEvents.CalcGroups();

            List<ProcessedEventSet>.Enumerator peEtr = m_processedEvents.GetEnumerator();

            foreach (EventGroup eg in allGroups)
            {
                foreach (EventBase eb in eg)
                {
                    if (m_scheduledTimes.Contains(eb.Time))
                    {
                        eg.Scheduled = ScheduledActs.True;
                        break;
                    }
                    else
                    {
                        eg.Scheduled = ScheduledActs.False;
                    }
                }
            }

            List<ProcessedEventSet>.Enumerator etr = m_processedEvents.GetEnumerator();

            List<ProcessedEventSet> groupedProcessedEvents = new List<ProcessedEventSet>();
            while (etr.MoveNext())
            {
                ProcessedEventSet processedEvents = etr.Current;

                IEnumerator<EventBase> processedEtr = processedEvents.GetEnumerator();
                while (processedEtr.MoveNext())
                {
                    if (m_scheduledTimes.Contains(processedEtr.Current.Time))
                    {
                        etr.Current.Scheduled = ScheduledActs.True;
                        break;
                    }
                    else
                    {
                        etr.Current.Scheduled = ScheduledActs.False;
                    }
                }
            }


            List<ProcessedEventSet>.Enumerator peEtr2 = m_processedEvents.GetEnumerator();
            long allEventsAtTimeAreResourceUnavailableEventsCount = 0;
            long nbrOfProcessedEventSetsContainingResourceUnavailableEvents = 0;
            long totalResourceUnavailableEvents = 0;
            long ttlSchedEvtGroups = 0;
            long ttlNotSchedEvtGroups = 0;

            List<ProcessedEventSet> evtsGroupsThatDidntCauseSched = new List<ProcessedEventSet>();

            while (peEtr2.MoveNext())
            {
                IEnumerator<EventBase> eEtr = peEtr2.Current.GetEnumerator();
                bool allEventsAtTimeAreResourceUnavailableEvents = false;
                bool hasResourceUnavailableEvent = false;
                EventBase evt = null;
                while (eEtr.MoveNext())
                {
                    evt = eEtr.Current;
                    if (evt.GetType().Name == "ResourceUnavailableEvent")
                    {
                        allEventsAtTimeAreResourceUnavailableEvents = true;
                        hasResourceUnavailableEvent = true;
                        ++totalResourceUnavailableEvents;
                    }
                    else
                    {
                        allEventsAtTimeAreResourceUnavailableEvents = false;
                    }
                }
                if (allEventsAtTimeAreResourceUnavailableEvents)
                {
                    ++allEventsAtTimeAreResourceUnavailableEventsCount;
                }
                if (hasResourceUnavailableEvent)
                {
                    ++nbrOfProcessedEventSetsContainingResourceUnavailableEvents;
                }
                if (m_scheduledTimes.Contains(evt.Time))
                {
                    ++ttlSchedEvtGroups;
                    peEtr2.Current.Scheduled = ScheduledActs.True;
                }
                else
                {
                    ++ttlNotSchedEvtGroups;
                    evtsGroupsThatDidntCauseSched.Add(peEtr2.Current);
                    peEtr2.Current.Scheduled = ScheduledActs.False;
                }
            }
            long ttlAll = ttlNotSchedEvtGroups + ttlSchedEvtGroups;
        #endif

        return CancelSimulationEnum.Continue;
    }

    /// <summary>
    /// A helper of ProcessEvents() that indicates whether there are any events on the event queue that should be processed.
    /// </summary>
    /// <returns></returns>
    private bool ProcessEventsOnQueue()
    {
        // Process events 
        // if there are any job events on the queue (these could lead to activities being released to the dispathers)
        // or if there are any events and there are activities available to dispatch (resource avail

        return m_withinPlanningHorizon || m_events.JobEventsCount > 0 || (m_events.Count > 0 && (ActivitiesOnDispatcher || m_blockReservations.Count > 0)); // The end of the planning horizon hasn't been reached.
    }

    private CancelSimulationEnum ScheduleReadyResources(long a_simClock, bool a_isImport)
    {
        foreach (BlockReservationEvent bre in m_blockReservations)
        {
            // Create a block.
            // Only reservations at the simulation clock are in m_blockReservations. They're added to this set
            // when their events are received.
            ScheduleRR(a_simClock, bre.PrimaryResource, bre.Resource, bre.Tank, bre.HasProductsToStoreInTank, bre.Activity, bre.Activity.Operation, bre.RRIdx, bre.Requirement, bre.Reservation.SchedulableInfo, bre.Reservation.EndTicks, bre.BatchToJoin, bre.NewBatch, false);
            bre.Resource.RemoveBlockReservation(bre.Reservation); // This line isn't necessary since the values continue to be used by the resource until the intervals passes beyond the simulation clock.
        }

        m_blockReservations.Clear();

        if (ScheduleReadyResourceInitAndScheduleHelper(a_simClock, a_isImport) == CancelSimulationEnum.Cancel)
        {
            return CancelSimulationEnum.Cancel;
        }

        if (m_moveTicksState == MoveReleaseState.CanBeReleased)
        {
            InternalActivity moveAct = m_moveActivitySequence[0];
            ClearMovesResourceReservations(moveAct); // also sets m_moveTicksState to Released.
            if (ScheduleReadyResourceInitAndScheduleHelper(a_simClock, a_isImport) == CancelSimulationEnum.Cancel)
            {
                return CancelSimulationEnum.Cancel;
            }
        }

        CalendarResourceAvailableEventList.Node current = m_availableResourceEventsSet.m_availablePrimaryResourceEvents.First;

        while (current != null)
        {
            // The next node might change when the current node is removed from the set,
            // so, the next node is copied here.
            CalendarResourceAvailableEventList.Node currentNext = current.Next;

            ResourceAvailableEvent resourceAvailableEvent = current.Data;

            if (resourceAvailableEvent.Resource is Resource)
            {
                Resource primaryResource = resourceAvailableEvent.Resource;
                primaryResource.ActiveDispatcher.EndDispatch();
                if (primaryResource.AvailableInSimulation != null)
                {
                    // [BATCH_CODE]
                    // Removal of batch resources from the resource available event set is delayed from the time a batch has been completely consumed to
                    // this point in the code. When the primary batch resource is completely consumed, the variable below is set.
                    if (primaryResource.AvailablePrimaryNode.BatchResScheduleResourceUnavilableEventForPrimaryRes)
                    {
                        LinkedListNode<ResourceCapacityInterval> containingCapacityIntervalNode = primaryResource.FindContainingCapacityIntervalNode(primaryResource.LastScheduledBlockListNode.Data.StartTicks, primaryResource.LastResultantCapacityNode);
                        m_availableResourceEventsSet.Remove(primaryResource);
                        AddResourceAvailableEvent(AddResourceavailableEventType.AtSpecifiedTime, primaryResource.LastScheduledBlockListNode.Data.EndTicks, primaryResource, null);
                    }
                }
            }

            current = currentNext;
        }

        return CancelSimulationEnum.Continue;
    }

    /// <summary>
    /// Initialize the dispatchers of the ready resources and attempt to schedule activities on them.
    /// </summary>
    /// <param name="a_simTicks">The current time in the simulation.</param>
    private CancelSimulationEnum ScheduleReadyResourceInitAndScheduleHelper(long a_simTicks, bool a_isImport)
    {
        m_availableResourceEventsSet.InitForScheduling();
        CalendarResourceAvailableEventList.Node current = m_availableResourceEventsSet.m_availablePrimaryResourceEvents.First;

        while (current != null)
        {
            ResourceAvailableEvent resourceAvailableEvent = current.Data;

            if (resourceAvailableEvent.Resource is Resource)
            {
                Resource primaryResource = resourceAvailableEvent.Resource;

                // It's possible for AvailableInSimulation to be null if the resource was ready 
                // but was used to satisfy a non-primary resource requirement before it was reached within this loop.
                if (primaryResource.AvailableInSimulation != null)
                {
                    long simStartTime = GetSimStartTime(primaryResource);
                    primaryResource.ActiveDispatcher.BeginDispatch(a_simTicks, m_activeSimulationType == SimulationType.Optimize,
                        (m_activeSimulationType == SimulationType.Optimize ||
                        m_activeSimulationType == SimulationType.Expedite ||
                        (m_activeSimulationType == SimulationType.MoveAndExpedite && m_unscheduledActivitiesBeingMovedCount == 0)) && a_simTicks >= simStartTime);
                }
            }

            current = current.Next;
        }

        CancelSimulationEnum cancel = ScheduleReadyResourcesHelper(a_simTicks, a_isImport);

        return cancel;
    }

    private CancelSimulationEnum ScheduleReadyResourcesHelper(long a_simClock, bool a_isImport)
    {
        CancelSimulationEnum cancel;

        if (m_singleDispatcherDefinitionInUse != null)
        {
            cancel = ScheduleReadyResourcesHelperFromEvents(a_simClock, a_isImport);
            //                cancel = ScheduleReadyResourcesHelperWithActivityPQ(a_simClock, a_inProcessOnly);
        }
        else
        {
            cancel = ScheduleReadyResourcesHelperFromEvents(a_simClock, a_isImport);
        }

        return cancel;
    }

    private KeyAndActivity GetNextKeyAndActivity(Resource a_primaryResource, bool a_inProcessOnly)
    {
        if (a_primaryResource.AvailableInSimulation == null)
        {
            // This is possible when the resource was ready but was used to satisfy a non-primary resource requirement before it was reached within this for loop.
            return null;
        }

        KeyAndActivity ka;

        if (a_inProcessOnly)
        {
            // Check whether the next activity it in-processs before removing it from the dispatcher.
            ka = a_primaryResource.ActiveDispatcher.PeekNextKeyAndActivity();

            if (ka == null)
            {
                // There are no activities in the resource's dispatcher.
                return null;
            }

            InternalActivity activity = ka.Activity;

            if (activity.InProduction())
            {
                // The next activity is in process, remove it from the dispatcher.
                // Try to schedule this activity.
                ka = a_primaryResource.ActiveDispatcher.GetNextKeyAndActivity();
            }
            else
            {
                // Done with this resource since we are out of in-process activities on it.
                return null;
            }
        }
        else
        {
            ka = a_primaryResource.ActiveDispatcher.GetNextKeyAndActivity();
        }

        return ka;
    }

    /// <summary>
    /// Attempt to schedule work on resources that are ready.
    /// </summary>
    /// <param name="a_simClock">The current simulation time.</param>
    private CancelSimulationEnum ScheduleReadyResourcesHelperFromEvents(long a_simClock, bool a_isImport)
    {

        CalendarResourceAvailableEventList.Node current = m_availableResourceEventsSet.m_availablePrimaryResourceEvents.First;
        if (current != null)
        {
            List<ResourceAndEventPair> availableResources = new ();
            do
            {
                m_availableResourceEventsSet.InitNextNodeAsNodeAfter(current);
                ResourceAvailableEvent resourceAvailableEvent = current.Data;

                if (resourceAvailableEvent.Resource is Resource primaryResource)
                {
                    // This "if" can be null when the resource was ready but was used to satisfy a non-primary resource requirement before it was reached within this for loop.
                    // For multitasking resources it's possible that the available attention percent has reached 0.
                    if (primaryResource.AvailableInSimulation != null && primaryResource.ActiveDispatcher.HasReadyActivity)
                    {
                        availableResources.Add(new ResourceAndEventPair(primaryResource, resourceAvailableEvent));
                    }
                }
            } while ((current = m_availableResourceEventsSet.NextNode) != null);


            return ScheduleReadyResourcesHelperFromAvailable(availableResources, a_simClock, a_isImport);
        }

        return CancelSimulationEnum.Continue;
    }

    private CancelSimulationEnum ScheduleReadyResourcesHelperFromAvailable(List<ResourceAndEventPair> a_availableResources, long a_simClock, bool a_isImport)
    {
        Dictionary<BaseId, List<ResourceDispatchData>> cachedDispatchInfo = new();

        //Now schedule resources by their dispatch score
        while (a_availableResources.Count > 0)
        {
            List<ResourceDispatchData> dispatchDatas = new (a_availableResources.Count);
            for (int i = a_availableResources.Count - 1; i >= 0; i--)
            {
                ResourceAndEventPair resourceAndEventPair = a_availableResources[i];
                KeyAndActivity keyAndActivity = resourceAndEventPair.Resource.ActiveDispatcher.GetNextKeyAndActivity();

                if (keyAndActivity == null)
                {
                    if (cachedDispatchInfo.TryGetValue(resourceAndEventPair.Resource.Id, out List<ResourceDispatchData> rdd))
                    {
                        dispatchDatas.AddRange(rdd);
                    }
                    else
                    {
                        a_availableResources.RemoveAt(i);
                    }

                    continue;
                }

                InternalActivity activity = keyAndActivity.Activity;

                ResourceDispatchData dispatchData = new()
                {
                    ActivityToSchedule = activity,
                    //InProcess = activity.InProduction(), This should not be required as an activity in production will have a near max score
                    Score = keyAndActivity.Key.Score,
                    ResourcePair = resourceAndEventPair
                };

                dispatchDatas.Add(dispatchData);
            }

            //Order dispatchList by highest score. Equal scores will prioritize resource ID. This is just for a consistent tiebreaker.
            //If moving, make sure the moved operations schedule first to avoid being delayed by other conflicts such as helpers or compatibility codes
            IEnumerable<ResourceDispatchData> orderedList;
            if (m_activeSimulationType == SimulationType.Move || m_activeSimulationType == SimulationType.MoveAndExpedite)
            {
                orderedList = dispatchDatas.OrderByDescending(p => p.ActivityToSchedule.BeingMoved)
                                           .ThenBy(p => p.ActivityToSchedule.OriginalScheduledStartTicks)
                                           .ThenBy(p => p.ResourcePair.Resource.Id);
            }
            else
            {
                orderedList = dispatchDatas.OrderByDescending(p => p.Score)
                                           .ThenBy(p => p.ResourcePair.Resource.Id);
            }

            foreach (ResourceDispatchData orderedResourceData in orderedList)
            {
                Resource nextResource = orderedResourceData.ResourcePair.Resource;
                if (m_dataLimitReached)
                {
                    return CancelSimulationEnum.Cancel;
                }

                if (orderedResourceData.ResourcePair.Resource.AvailableInSimulation == null)
                {
                    //It's possible a previous activity scheduled a helper on this resource, it is no longer available
                    a_availableResources.Remove(orderedResourceData.ResourcePair);
                    continue;
                }

                if (orderedResourceData.ActivityToSchedule.SimScheduled || orderedResourceData.ActivityToSchedule.Operation.AlternatePathNode.HasAnotherPathScheduled)
                {
                    //This activity or path has already been scheduled. Remove it from the cached list and continue.
                    //We don't remove from the availableResources list as there may be another activity on the dispatcher to schedule 
                    if (cachedDispatchInfo.TryGetValue(nextResource.Id, out List<ResourceDispatchData> cachedList))
                    {
                        if (cachedList != null)
                        {
                            cachedList.Remove(orderedResourceData);

                            if (cachedList.Count == 0)
                            {
                                cachedDispatchInfo.Remove(nextResource.Id);
                            }
                        }
                    }

                    continue;
                }

                //Check if this activity was previously cached after having failed to schedule due to a before Cleans interference 
                //we need to ignore the RCI profile this time around and allow it to schedule
                bool ignoreRciProfile = false;
                if (cachedDispatchInfo.TryGetValue(nextResource.Id, out List<ResourceDispatchData> cachedDispatchDataList))
                {
                    if (cachedDispatchDataList.Any(p => p.ResourcePair.Resource.Id == nextResource.Id && p.ActivityToSchedule.Id == orderedResourceData.ActivityToSchedule.Id))
                    {
                        ignoreRciProfile = true;
                    }
                }

                CancelSimulationEnum result = AttemptToScheduleActivity(orderedResourceData.ResourcePair.ResourceAvailableEvent, nextResource, orderedResourceData.ActivityToSchedule, a_simClock, ignoreRciProfile, a_isImport, out bool resourceConsumed);
                if (result == CancelSimulationEnum.Cancel)
                {
                    return CancelSimulationEnum.Cancel;
                }
                else if (result == CancelSimulationEnum.CacheDispatchInfo)
                {
                    //A before Clean conflict prevented the act from scheduling. Cache the activity so we can attempt again later.
                    if (cachedDispatchDataList != null)
                    {
                        cachedDispatchDataList.Add(orderedResourceData);
                    }
                    else
                    {
                        cachedDispatchInfo.Add(nextResource.Id, new List<ResourceDispatchData> { orderedResourceData });
                    }
                }
                else if (result == CancelSimulationEnum.ContinueOnlyDueToCleanout)
                {
                    //The top scoring activity COULD schedule if the cleanout was not in the way. Try this activity again next time
                    a_availableResources.Remove(orderedResourceData.ResourcePair);
                }
                else
                {
                    //Scheduled
                    if (resourceConsumed)
                    {
                        a_availableResources.Remove(orderedResourceData.ResourcePair);
                    }

                    if (cachedDispatchDataList != null)
                    {
                        cachedDispatchDataList.Remove(orderedResourceData);

                        if (cachedDispatchDataList.Count == 0)
                        {
                            cachedDispatchInfo.Remove(nextResource.Id);
                        }
                    }
                }
            }
        }

        return CancelSimulationEnum.Continue;
    }

    #if TEST
        long _deScheduledActivityCount;
        long _deAttemptToScheduleActivity_Nbr;
        long _deAlreadyScheduled;
        long _deSchedulabilityCustomization;
        long _deSchedulableResult;
        long _deFindMaterial;
        long _deNonPrimaryRR;
        List<Resource> _deAllResources;
        long _nbrOfJobs;
        long _nbrOfMos;
        long _nbrOfOps;
        long _nbrOfActs;

        string z_scheduledActivityFileName;

        string ZZScheduledActivityFileName()
        {
            if (z_scheduledActivityFileName == null)
            {
                z_scheduledActivityFileName = System.IO.Path.Combine(z_c_tmpDir, "Progress.txt");
            }

            return z_scheduledActivityFileName;
        }

    #endif

    /// <summary>
    /// Used to group some stack variables necessary when scheduling an activity.
    /// </summary>
    // [BATCH_CODE]
    internal struct BatchToJoinTempStruct
    {
        private ResourceBlockList.Node m_resBlockListNodeToJoin;

        /// <summary>
        /// Stores the required finish quantity of the activity so it doesn't have to be calculated more than once.
        /// </summary>
        internal decimal BatchAmountRemaining;

        private Batch m_batch;
        private InternalActivity m_firstActivityInBatch;

        internal ResourceBlockList.Node ResBlockListNodeToJoin => m_resBlockListNodeToJoin;

        internal void SetBatchResourceBlockListNode(ResourceBlockList.Node a_node)
        {
            m_resBlockListNodeToJoin = a_node;
            m_batch = m_resBlockListNodeToJoin.Data.Batch;
            m_firstActivityInBatch = m_batch.FirstActivity;
        }

        internal bool NoJoin()
        {
            return m_resBlockListNodeToJoin == null;
        }

        internal SchedulableInfo GetSchedulableInfo()
        {
            return m_batch.m_si;
        }

        /// <summary>
        /// Create a copy of the RR satiators of the batch.
        /// </summary>
        /// <returns>a copy of the batch's RR satiators.</returns>
        internal ResourceSatiator[] GetRRSatiatorsCopy()
        {
            return m_batch.GetRRSatiatorsCopy();
        }

        internal ResourceBlock GetRRBlock(int a_rrIdx)
        {
            return m_firstActivityInBatch.GetResourceBlock(a_rrIdx);
        }

        internal Batch Batch => m_batch;
    }
    
    /// <summary>
    /// The way the batches were configured by the last simulation.
    /// This can be used to maintain the composition of batches from one simulation to the next. For instance
    /// if a move were performed, the only batches whose composition might change are batches that are being merged.
    /// Without tracking batches, the simulation wouldn't know how batches had been composed.
    /// </summary>
    private BatchManager m_lastSimBatches;

    /// <summary>
    /// Used to specify related information about a simulation. Any values specific to a simulation should be set while the simulation is
    /// being configured.
    /// </summary>
    private SimDetailsGroupings m_simDetailsGroupings;

    #if TEST
        internal class DateOccurance
        {
            internal DateOccurance(string a_date)
            {
                Date = a_date;
                Occurances = 1;

            }
            internal string Date
            {
                get;
                private set;
            }

            internal long Occurances
            {
                get;
                set;
            }

            public override string ToString()
            {
                return Date + " X" + Occurances.ToString();
            }
        }
        List<DateOccurance> z_attemptsToSchedule = null;
    #endif

    /// <summary>
    /// </summary>
    /// <param name="resourceAvailableEvent"></param>
    /// <param name="primaryResource"></param>
    /// <param name="activity"></param>
    /// <param name="simClock"></param>
    /// <returns>Whether to cancel the simulation.</returns>
    private CancelSimulationEnum AttemptToScheduleActivity(ResourceAvailableEvent a_resourceAvailableEvent, Resource a_primaryResource, InternalActivity a_act, long a_simClock, bool a_ignoreRciProfile, bool a_isImport, out bool o_resourceConsumed)
    {
        #if TEST
			++z_attemptsToSchedule;
        #endif
        //#if DEBUG
        //            // Performance enhancement. If all the blocks in the batch have already been scheduled, make the resource unavailable. This might be difficult with tanks.
        //            // Simplify the code below.
        //            // Test child task of UI batch task and reassign to Cavan if there are problems.
        //#else
        //            DELETE_THIS; this is test code or a reminder
        //#endif

        o_resourceConsumed = false;
        ResourceOperation schedulingOperation = a_act.Operation as ResourceOperation;

        // Prevent activities from automatically batching during non-optimizes.
        if (a_primaryResource.BatchResource() // Merging only applies to batch resources.
            &&
            m_activeSimulationType != SimulationType.Optimize // Merging occurs during optimizes.
            &&
            !(m_activeSimulationType == SimulationType.Expedite && a_act.ManufacturingOrder.BeingExpedited) // Expedited MO's activities can be merged into other batches.
            &&
            a_primaryResource.Blocks.Count > 0 // A batch has been scheduled.
            &&
            m_lastSimBatches != null)
        {
            ResourceBlock lastScheduledBlock = a_primaryResource.Blocks.Last.Data;

            if (lastScheduledBlock.StartTicks == a_simClock) // The activity is attempting to be scheduled at the same time as the last block scheduled on this resource (they might be batched together).
            {
                // Only allow the activity to be merged with the last scheduled batch if it was part of the batch in the last simulation.
                // Below it is presumed that batches from the last simulation are being preserved, so the first activity in the batch is representative of the batch.
                InternalActivity firstScheduledActInBatch = lastScheduledBlock.Batch.GetFirstActivity();

                if (m_activeSimulationType == SimulationType.Move)
                {
                    // activities that were part of the same batch can be joined.
                    // activities that were being moved can be joined if there's a move to batch.
                    // activites that weren't being moved 
                    // out of

                    // Tested:
                    // 1 and more into; Moved 1 block in; Moved 2 blocks in
                    // 1 and more out of; Moved 1 block out; moved 2 blocks out, the moved blocks formed a new batch which is okay.
                    // 1 or more onto non-exact; tested 1 and 2 block moves.
                    // 1 or more into exact; tested 1 and 2 blocks with success.

                    // To Test:
                    // tests to perform intersecting and not intersecting with the other blocks below:

                    // Move merges from 1 batch to another.

                    // 1 out of exact and intersecting with the batch it was scheduled in.
                    // given: 
                    //   Whether it was being moved.
                    //   The last batch the activity was part of
                    //   Whether the batch was a MergeIntoBatch
                    // ?gingWithAnotherBatchIsTrue.

                    // Last 2 error recordings worked.

                    Batch actsLastSimBatch = m_lastSimBatches.GetLastSimBatch(a_act);
                    Batch firstScheduledActInBatchs_LastSimBatch = m_lastSimBatches.GetLastSimBatch(firstScheduledActInBatch);

                    if (firstScheduledActInBatchs_LastSimBatch == actsLastSimBatch && firstScheduledActInBatch.BeingMoved && !a_act.BeingMoved)
                    {
                        // Something was moved out of a batch and dropped within the time the original batch was scheduled.
                        return CancelSimulationEnum.Continue;
                    }

                    if (firstScheduledActInBatchs_LastSimBatch != actsLastSimBatch) // Under what conditions to equal and unequal batches schedule and not schedule together.
                    {
                        Batch lastMergeIntoBatch = null; // The batch activities were to be merged into. When merging occurs a new batch will be created to take this ones place.
                        if (m_simDetailsGroupings != null && m_simDetailsGroupings.BatchSimDetails != null && m_simDetailsGroupings.BatchSimDetails.MoveMerge != null)
                        {
                            lastMergeIntoBatch = m_simDetailsGroupings.BatchSimDetails.MoveMerge.MergeIntoBatch;
                        }

                        if (actsLastSimBatch != lastMergeIntoBatch) // Activities that were part of the same batch can be merged and escape the block below.
                        {
                            if (!a_act.BeingMoved && !a_act.MoveIntoBatch)
                            {
                                // An activity that's not in the MoveIntoBatch or being moved into the MoveIntoBatch can't join the batch.
                                return CancelSimulationEnum.Continue;
                            }
                        }
                    }
                }
            }
        }

        #if TEST
            ++m_totalAttemptsToScheduleActivity;
        #endif

        ResourceRequirementManager rrm = a_act.Operation.ResourceRequirements;
        ManufacturingOrder mo = a_act.Operation.ManufacturingOrder;
        ResourceOperation op = (ResourceOperation)a_act.Operation;
        AlternatePath path = a_act.Operation.ManufacturingOrder.AlternatePaths.GetOpsPath(a_act.Operation);

#if DEBUG
        if (PathIsNoLongerEligibleBecauseAnotherPathIsAlreadyBeingScheduled(mo, path))
        {
            throw new DebugException("This non-current path should not be scheduling here.");

        }
#endif

#if TEST
            ++_deAttemptToScheduleActivity_Nbr;
#endif

        if (a_act.BeingMoved)
        {
            if (m_nextSequencedMoveActivityToSchedule == 0)
            {
                ClearMovesResourceReservations(a_act);
            }
            else
            {
                m_moveTicksState = MoveReleaseState.Released;
            }
        }

        if (a_act.SimScheduled)
        {
            // The activity has already been scheduled within this simulation.
            o_resourceConsumed = false;

            #if TEST
                ++_deAlreadyScheduled;
            #endif
            return CancelSimulationEnum.Continue;
        }

        if (!(o_resourceConsumed = SchedulabilityCustomization_TestIsSchedulableCustomizationForPrimaryResource(a_primaryResource, a_act, null, a_simClock)))
        {
            return CancelSimulationEnum.Continue;
        }

        InternalOperation operation = a_act.Operation;
        // [BATCH_CODE]
        BatchToJoinTempStruct batchToJoin = new ();


        if (a_primaryResource.BatchResource())
        {
            if (a_primaryResource.LastScheduledBlockListNode != null && a_primaryResource.LastScheduledBlockListNode.Data.StartTicks == a_simClock)
            {
                if (a_primaryResource.LastScheduledBlockListNode.Data.Activity.Operation.BatchCode != operation.BatchCode || a_act.Operation.ResourceRequirements.SimilarityComparison(a_primaryResource.LastScheduledBlockListNode.Data.Activity.Operation.ResourceRequirements) != 0)
                {
                    o_resourceConsumed = false;
                    return CancelSimulationEnum.Continue;
                }

                batchToJoin.SetBatchResourceBlockListNode(a_primaryResource.LastScheduledBlockListNode);
                batchToJoin.BatchAmountRemaining = a_act.BatchAmount;

                switch (a_primaryResource.BatchType)
                {
                    case MainResourceDefs.batchType.Percent:
                        decimal percent = batchToJoin.BatchAmountRemaining / a_act.GetResourceProductionInfo(a_primaryResource).QtyPerCycle;
                        if (batchToJoin.Batch.BatchPercentRemaining < percent)
                        {
                            o_resourceConsumed = false;
                            return CancelSimulationEnum.Continue;
                        }

                        break;

                    case MainResourceDefs.batchType.Volume:
                        if (batchToJoin.Batch.BatchVolumeRemaining < batchToJoin.BatchAmountRemaining)
                        {
                            o_resourceConsumed = false;
                            return CancelSimulationEnum.Continue;
                        }

                        break;
                }
            }
            //        if (a_primaryResource.BatchVolume < a_act.RemainingQty)
            //        {
            //    }
            //        // A new batch might be started.
            //        // Verify the activity will fit into the new batch.
            //        if (IsBatchResourceCapableOfSchedulingActivity(a_primaryResource, a_act) != IsBatchResourceCapableOfSchedulingActivityEnum.Yes)
            //        {
            //            // The batch resource isn't able to schedule this activity.
            //            o_resourceConsumed = false;
            //            return CancelSimulationEnum.Continue;
            //}
        }

        //TODO: Outsource to Hexaly optimizer
        if (schedulingOperation.PotentialConnectorsFromPredecessors.Count > 0)
        {
            //Dictionary<BaseId, BaseId> predConnectorMappings = new ();

            //Dictionary<BaseId, int> connectorPredUseCount = new ();
            ////Verify we have an available connector
            //foreach (ResourceConnectorConstraintInfo predConnectors in schedulingOperation.PotentialConnectorsFromPredecessors.Values)
            //{
            //    foreach (ResourceConnector connector in predConnectors.ResourceConnectors)
            //    {
            //        predConnectorMappings.Add(predConnectors.PredecessorOperation.Id, connector.Id);

            //        if (connectorPredUseCount.TryGetValue(connector.Id, out int useCount))
            //        {
            //            connectorPredUseCount[connector.Id] = useCount + 1;
            //        }
            //        else
            //        {
            //            connectorPredUseCount[connector.Id] = 1;
            //        }
            //    }
            //}

            //List<KeyValuePair<BaseId, int>> orderedConnectors = connectorPredUseCount.ToList().OrderBy(c => c.Value).ToList();

            //It's possible the activity was released to this resource even though there are no connectors. For example, it was moved.
            bool connectorFound = false;

            //For simplicity just pick the first predecessor using this connector
            List<ResourceConnector> allocatedConnectors = new (schedulingOperation.PotentialConnectorsFromPredecessors.Count);
            
            long earliestTransitTime = long.MaxValue;
            foreach (ResourceConnectorConstraintInfo predConnectors in schedulingOperation.PotentialConnectorsFromPredecessors.Values)
            {
                //Find an available connector. Use the one with the longest transfer time to save shorter transfers for other ops.
                foreach (ResourceConnector connector in predConnectors.ResourceConnectors.OrderByDescending(c => c.TransitTicks))
                {
                    if (connector.ToResources.ContainsKey(a_primaryResource.Id))
                    {
                        connectorFound = true;
                        //Connectors might be in use. If so, track when the earliest one will be available to retry scheduling
                        long availableDate = Math.Max(predConnectors.PredecessorReleaseTicks, connector.ReleaseTicks) + connector.TransitTicks;
                        if (!connector.InUse && a_simClock >= availableDate && connector.AttemptToConnect(a_primaryResource.Id))
                        {
                            allocatedConnectors.Add(connector);
                            break;
                        }

                        earliestTransitTime = Math.Min(earliestTransitTime, connector.ReleaseTicks + connector.TransitTicks);
                    }
                }
            }

            //Only restrict if there were connectors.
            if(connectorFound && allocatedConnectors.Count != schedulingOperation.PotentialConnectorsFromPredecessors.Count)
            {
                foreach (ResourceConnector allocatedConnector in allocatedConnectors)
                {
                    allocatedConnector.ResetSimulationStateVariables();
                }

                schedulingOperation.SetLatestConstraintToResourceConnector(Clock, a_simClock);

                //Retry scheduling once the transit time has elapsed
                if (earliestTransitTime > a_simClock)
                {
                    AddEvent(new RetryForConnectorEvent(earliestTransitTime));
                }

                return CancelSimulationEnum.Continue;
            }
        }

        //AutoSplit
        AutoSplitInfo autoSplitInfo = null;
        switch (schedulingOperation.AutoSplitInfo.AutoSplitType)
        {
            case OperationDefs.EAutoSplitType.None:
            case OperationDefs.EAutoSplitType.Manual:
                break;
            case OperationDefs.EAutoSplitType.ResourceVolumeCapacity:
                autoSplitInfo = AutoSplitOperationByResourceVolumeCapacity(a_primaryResource, a_act, schedulingOperation, m_productRuleManager);
                if (autoSplitInfo == null)
                {
                    o_resourceConsumed = false;
                    return CancelSimulationEnum.Continue;
                }
                break;
            case OperationDefs.EAutoSplitType.ResourceQtyCapacity:
                autoSplitInfo = AutoSplitOperationByResourceQtyCapacity(a_primaryResource, a_act, schedulingOperation, m_productRuleManager);
                if (autoSplitInfo == null)
                {
                    o_resourceConsumed = false;
                    return CancelSimulationEnum.Continue;
                }
                break;
            case OperationDefs.EAutoSplitType.PredecessorQuantityRatio:
                autoSplitInfo = AutoSplitOperationByPredecessorQuantity(a_primaryResource, a_act, m_productRuleManager);
                break;
            case OperationDefs.EAutoSplitType.PredecessorMaterials:
                autoSplitInfo = AutoSplitOperationByPredecessorMaterials(a_primaryResource, a_act, m_productRuleManager);
                break;
            case OperationDefs.EAutoSplitType.PrimaryCapacityAvailability:
                //This will be handled later after capacity is calculated
                break;
            case OperationDefs.EAutoSplitType.CIP:
                //calculated after capacity is calculated
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        //If the IgnoreMaterialConstraintsInFrozenSpan setting is enabled, set material constraints if the activity starts during the frozen span.
        DateTime frozenSpanEnd = ClockDate.Add(a_primaryResource.Department.FrozenSpan);
        if (ScenarioOptions.IgnoreMaterialConstraintsInFrozenSpan)
        {
            foreach (MaterialRequirement mr in a_act.Operation.MaterialRequirements)
            {
                if (a_simClock < frozenSpanEnd.Ticks)
                {
                    if (mr.ConstraintType == MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate)
                    {
                        mr.ConstraintType = MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByAvailableDate;
                    }
                    else if (mr.ConstraintType == MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate)
                    {
                        mr.ConstraintType = MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByEarlierOfLeadTimeOrAvailableDate;
                    }
                }
                else
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

        SchedulableInfo si;
        ResourceSatiator[] resReqSatiaters;
        // m_inventoryAllocations will have any material allocations added to it.
        Dictionary<BaseId, SupplyProfile> productProfiles = new();

        SchedulableSuccessFailureEnum result = AttemptToScheduleActivity_CustomizeMOHelper(this, a_resourceAvailableEvent, a_primaryResource, a_act, a_simClock, rrm
            , ref o_resourceConsumed, ref batchToJoin, out si, out resReqSatiaters, ref autoSplitInfo, ref productProfiles
            , out List<MaterialDemandProfile> materialAllocations, a_ignoreRciProfile, a_isImport);
        if (result != SchedulableSuccessFailureEnum.Success)
        {
            //if (a_act.BeingMoved)
            //{
            //    ResourceBlockList.Node firstBlock = a_primaryResource.Blocks.FindFirstBlockBefore(m_moveData.StartTicksAdjusted);
            //    RequiredCapacityPlus rcp = a_primaryResource.CalculateTotalRequiredCapacity(a_act, firstBlock, true, false, RequiredSpan.Neg1RequiredSpan, m_moveData.StartTicksAdjusted);
            //}
            o_resourceConsumed = false;

            //Resize MO to original Qty if it was resized
            a_act.ManufacturingOrder.AdjustToOriginalQty(null, this.ProductRuleManager);

            //Failed to schedule this act, rejoin any split.
            RejoinFailedToScheduleActivity(a_act, autoSplitInfo);

            if (result == SchedulableSuccessFailureEnum.FailedOnlyDueToCleanBeforeSpan)
            {
                return CancelSimulationEnum.ContinueOnlyDueToCleanout;
            }

            if (result == SchedulableSuccessFailureEnum.FailedDueToCleanBeforeSpan)
            {
                return CancelSimulationEnum.CacheDispatchInfo;
            }

            return CancelSimulationEnum.Continue;
        }

        //Check if the activity can schedule if/when using Resource.MaxSameSetupSpan.
        //string lastScheduledSetupCode = a_primaryResource.LastScheduledBlockListNode?.Data.Activity.Operation.SetupCode;
        //if (a_primaryResource.CapacityType == InternalResourceDefs.capacityTypes.SingleTasking && a_simClock < EndOfPlanningHorizon && !a_primaryResource.SameOperationSetupAllowed(si, a_act, lastScheduledSetupCode))
        //{
        //    o_resourceConsumed = false;

        //    //Failed to schedule this act, rejoin any split.
        //    RejoinFailedToScheduleActivity(a_act, autoSplitInfo);

        //    //TODO: Set LatestConstraint code here
        //    return CancelSimulationEnum.Continue;
        //}

        ///***************************************************************************************************************
        /// The Activity is now being scheduled, all checks have passed
        ///***************************************************************************************************************

        if (autoSplitInfo?.NewlySplitActivity != null)
        {
            AddActivityReleasedEvent(autoSplitInfo.NewlySplitActivity, a_simClock);
        }

        //Firm resource reservations and add events
        if (ScenarioOptions.EnforceMaxDelay)
        {
            IEnumerable<ResourceReservationEvent> resourceReservationEvents = m_resRvns.FirmAllPlanned();
            foreach (ResourceReservationEvent reservedEvent in resourceReservationEvents)
            {
                AddEvent(reservedEvent);
            }
        }

        // !ALTERNATE_PATH!; The path has already been selected. Remove unrelated paths from dispatchers.

        // Copy the required number of ticks of the various processing stages into the activity.
        // These are all based off of the primary resource requirement.
        a_act.m_scheduledSequence = ++m_currentScheduledActivityNbr;

        if (a_act.Operation.ManufacturingOrder.CurrentPathNeedsToBeSetWhenFirstActivityIsScheduled)
        {
            mo.CurrentPathNeedsToBeSetWhenFirstActivityIsScheduled = false;
            long prevPathActivitiesSchedule = mo.GetNbrOfActivitiesToSchedule();
            mo.CurrentPath_setter = mo.AlternatePaths.GetOpsPath(a_act.Operation);
            long newPathActivitiesToSchedule = mo.GetNbrOfActivitiesToSchedule();
            if (prevPathActivitiesSchedule != newPathActivitiesToSchedule)
            {
                long diff = newPathActivitiesToSchedule - prevPathActivitiesSchedule;
                m_simulationProgress.AdjustNbrOfActivitiesToSchedule(diff);
            }
        }

        //Remove here before materials are tracked that would empty tanks. We need to know the accurate lot codes remaining
        //When the first activity of the job schedules, remove other paths.
         if (!a_act.Operation.ManufacturingOrder.ActivitiesScheduled)
        {
            foreach (AlternatePath alternatePath in mo.AlternatePaths)
            {
                // Current Path will be initialized to null before the start of an optimization.
                // When the first activity is scheduled, the path will be set and any activities from different paths
                // that are on resource queues aren't automatically being removed.
                // If this block is entered, it means the path was selected and it's different than the path of the activity 
                // that is being scheduled. The activity can't be scheduled and so is removed from all resource dispatchers.
                if (alternatePath != mo.CurrentPath)
                {
                    RemovePathFromScheduling(alternatePath);
                }
            }
        }

        a_act.Operation.ManufacturingOrder.ActivitiesScheduled = true;

        // Consume the material for the Inventory Material Requirements.
        if (materialAllocations != null)
        {
            long latestMatl = 0;
            MaterialRequirement latestConstraintMaterial = null;
            HashSet<StorageArea> storageAreaEmptyCheck = new ();
            foreach (MaterialDemandProfile mrDemand in materialAllocations)
            {
                MaterialRequirement mr = mrDemand.MR;
                // Update latest material constraint time. 
                if (op.LatestConstraintInternal == InternalOperation.LatestConstraintEnum.MaterialRequirement && mr.NonConstraint && !mr.IssuedComplete)
                {
                    // If the latestConstrtaint is already material Requirment, it was set to this value
                    // the first time it was unsuccessful at allocating the material. The the time needs to be 
                    // updated based on the actual material allocate in Plant.FindAvailableMaterial().

                    long latestMaterialUseDate = mrDemand.GetLatestMaterialUseDate();
                    if (latestMaterialUseDate > latestMatl)
                    {
                        latestConstraintMaterial = mrDemand.MR;
                        latestMatl = latestMaterialUseDate;
                    }
                }

                //Update lot tracking lists to see if scheduling this activity removes lot code constraints
                //HashSet<string> lotCodesNoLongerConstrained = new ();
                if (a_act.Operation.Activities.UnfinishedActivities().Count() - a_act.Operation.Activities.ScheduledActivities.Count() == 1)
                {
                    //This is the last remaining activity to schedule in this operation. Remove the MR from any tracked lot code usage
                    Dictionary<string, EligibleLot>.Enumerator elEtr = mr.GetEligibleLotsEnumerator();
                    while (elEtr.MoveNext())
                    {
                        //If this was the last MR using the lot code, retry scheduling previous MR operations that failed due to this lot code constraint
                        bool lotCodeRequirementRemoved = UsedAsEligibleLotsLotCodes.Remove(elEtr.Current.Key, mr);
                        if (lotCodeRequirementRemoved)
                        {
                            RetryItemMatlReqs(SimClock, mr.Item);
                            //lotCodesNoLongerConstrained.Add(elEtr.Current.Key);
                        }
                    }
                }

                if (!mrDemand.HasNodes)
                {
                    //No inventory updates or latest constraints need to be updated. There were no sources
                    continue;
                }

                //All the allocated demand nodes will now consume their supply.
                List<EventBase> futureConsumptionEvents = new ();
                mrDemand.ConsumeAllocations(a_simClock, futureConsumptionEvents);
                foreach (EventBase futureConsumptionEvent in futureConsumptionEvents)
                {
                    AddEvent(futureConsumptionEvent);
                }

                //RetryItemStorageReqs(SimClock, mr.Item);

                //foreach (ItemStorage itemStorage in mrDemand.ItemStorageCache)
                //{
                //    storageAreaEmptyCheck.AddIfNew(itemStorage.StorageArea);
                //    RetryItemStorageEvents(SimClock, itemStorage);
                //}
            }

            if (latestMatl > 0)
            {
                //This function will only update the latest constraint based on the latest available material source
                a_act.Operation.SetLatestConstraintToMaterialRequirement(latestMatl, a_simClock, latestConstraintMaterial);
            }

            // Remove any tracked storage for empty ItemStorages
            //foreach (StorageArea sa in storageAreaEmptyCheck)
            //{
            //    int currentStoredItemCount = sa.UpdateStoredItemsCache();
            //    if (currentStoredItemCount == 0)
            //    {
            //        // Retry operations that depend on empty storage areas
            //        RetryStorageAreaEmptyEvents(a_simClock, sa);
            //    }
            //}
        }

        ///***************************************************************************************************************
        /// The resource connectors aren't being determined with the conditional you have below.
        /// commencted it out.
        /// change the scheduling code below to allow the activities to be associated with the resource block
        /// and the resource connectors to connect.
        /// 
        /// Every block has the potential for being part of a batch.
        ///***************************************************************************************************************

        // [BATCH_CODE]
        // A new batch if one needs to be created. 
        Batch newBatch = null;

        // The batch the activity was added to. Either a new batch or an existing batch.
        Batch batchJoined;

        Resource tank;
        if (resReqSatiaters[0].Resource.IsTank)
        {
            // The products are stored in the first tank resource.
            tank = resReqSatiaters[0].Resource;
        }
        else
        {
            tank = null;
        }

        // [TANK_CODE]: Whether the operation has any products that should be stored in a tank (if it's scheduled on a tank).
        //bool hasProductsToStoreInTank = operation.Products.HasProductsToStoreInTank();
        //bool useTank = tank != null && hasProductsToStoreInTank;

        if (batchToJoin.NoJoin())
        {
            // Create a new batch.
            newBatch = Batch_CreateBatch(a_primaryResource, a_act, si, resReqSatiaters);
            batchJoined = newBatch;
        }
        else
        {
            // Join an existing batch.
            Batch batch = batchToJoin.Batch;
            batch.Add(a_act, batchToJoin.BatchAmountRemaining);
            batchJoined = batch;
        }

        // [TANK_CODE]: The first resource requirement satiator that's a tank. It's used to store the products if the resource requirement and product are configured correctly.
        //TODO: Consider changing this to search for the first.

        // Schedule each of the resource requirements.
        for (int reqI = 0; reqI < resReqSatiaters.Length; reqI++)
        {
            ResourceRequirement rr = rrm.GetByIndex(reqI);

            ResourceSatiator resourceSatiator = resReqSatiaters[reqI];
            if (resourceSatiator == null)
            {
                a_act.Schedule(null);

                // In the case where a resource is no longer required.
                // Make sure any right neighbors are released.
                // For instance and UsageStart and UsageEnd are both setup and a block
                // is moved in front of another block that both have the same setup code,
                // the block will no longer require any setup and hence the resource would no
                // longer be necessary, but the sequential state 
                long finishDate = si.GetUsageEnd(rr);
                SequentialStateHandling(a_act, reqI, finishDate);
            }
            else
            {
                Resource res = resReqSatiaters[reqI].Resource;

                if (resourceSatiator.BlockReservation)
                {
                    // [USAGE_CODE] AttemptToScheduleActivity(): Create a block reservation 
                    // Instead of scheduling the resource now, create a block reservation and schedule the block later when the resource is needed.
                    BlockReservationEvent reservationEvent = new (resourceSatiator.Reservation.StartTicks, res, tank, false, a_primaryResource, resourceSatiator.Reservation, reqI, batchToJoin, newBatch);
                    AddEvent(reservationEvent);
                    resourceSatiator.Resource.CommitAttention(a_simClock, a_act, rr, resourceSatiator.Reservation.StartTicks, resourceSatiator.Reservation.EndTicks);
                    SequentialStateHandling(a_act, reqI, resourceSatiator.Reservation.EndTicks);
                }
                else
                {
                    long finishDate = si.GetUsageEnd(rr);
                    ResourceBlockList.Node scheduledNode = ScheduleRR(a_simClock, a_primaryResource, res, tank, false, a_act, operation, reqI, rr, si, finishDate, batchToJoin, newBatch, a_ignoreRciProfile);
                    SequentialStateHandling(a_act, reqI, finishDate);

                    resourceSatiator.Resource.CommitAttention(a_simClock, a_act, rr, scheduledNode.Data.StartTicks, scheduledNode.Data.EndTicks);
                }
            }
        }

        bool scheduledAtReservedDate = false;
        if (a_act.BeingMoved)
        {
            scheduledAtReservedDate = m_nextSequencedMoveActivityToSchedule == 0 || a_primaryResource.ReservedMoveStartTicks == a_simClock;

            --m_unscheduledActivitiesBeingMovedCount;

            ++m_nextSequencedMoveActivityToSchedule;

            if (m_nextSequencedMoveActivityToSchedule <= m_moveActivitySequence.Count - 1)
            {
                InternalActivity nextMoveActivity = GetNextSequencedMoveActivity();
                nextMoveActivity.WaitForLeftMoveBlockToSchedule = false;
                // Reserve the move resources again for the next block in the multi-block move.
                // The reserve date of the next move block starts at the finish time of the finish time
                // of the block that was just scheduled.
                long finishDate = si.GetUsageEnd(a_act.PrimaryResourceRequirement);
                ReserveNextMoveActivity(a_simClock, finishDate);
            }
        }

        // If the simulation type is sequential then the activity was only
        // associated with the single resource that it was scheduled on prior to
        // the start of the sequential simulation. Otherwise it is associated
        // with all the resources on the 
        if (a_act.Sequenced)
        {
            Resource resourceSatiater = resReqSatiaters[rrm.PrimaryResourceRequirementIndex].Resource;
            resourceSatiater.ActiveDispatcher.Remove(a_act);
        }
        else
        {
            if (a_act.GetMoveResource(a_act.Operation.ResourceRequirements.PrimaryResourceRequirementIndex, out Resource primaryMoveResource))
            {
                if (m_activeSimulationType == SimulationType.MoveAndExpedite ||
                    m_activeSimulationType == SimulationType.Move)
                {
                    primaryMoveResource.ActiveDispatcher.Remove(a_act);
                }
            }
            else
            {
                RemoveActivityFromAllResourceDispatchers(a_act);
            }
        }

        a_act.SimScheduledNotification();
        m_simulationProgress.ActivityScheduled();

        if (!Scenario.LicenseManager.VerifyDataLimit(m_currentScheduledActivityNbr, "ActivityScheduleLimit"))
        {
            m_dataLimitReached = true;
            RemoveRemainingActivitiesFromDispatchers();
            a_act.Job.Unschedule();
            a_act.Job.ScheduledStatus_set = JobDefs.scheduledStatuses.FailedToSchedule;
            a_act.Job.SetFailedToScheduleToScheduledActivityLimitReached();

            foreach (Job job in JobManager.JobEnumerator)
            {
                if (job.ToBeScheduled && job.ScheduledStatus == JobDefs.scheduledStatuses.FailedToSchedule)
                {
                    job.ToBeScheduled = false;
                }
            }
        }

#if TEST
            ++_deScheduledActivityCount;
#endif

        SetSuccessorsPredOpConnection(a_act.Operation, a_primaryResource);

        foreach (ResourceConnector connector in m_resourceConnectorManager)
        {
            //Find all allocated connectors and mark them in use (unless they allow Concurrent Use). Add event to release them
            if (!connector.AllowConcurrentUse && (connector.AllocatedResource != BaseId.NULL_ID || connector.ConnectedResource != BaseId.NULL_ID))
            {
                connector.UseConnector(si.m_postProcessingFinishDate);
                //TODO: Account for Tanks
                //TODO: Account for release timing
                AddConnectorReleaseEvent(connector, si.m_postProcessingFinishDate);
            }
        }

        /*
         * Mix:  Op10-1      Op10-2
         * Pack:     ->Op20-1->  ->Op20-2
         * The arrows represent predecessor/successor relationships.
         *
         * In Move() indirect predecessor operation finish times are used to prevent the drop from occurring before the end of the predecessors.
         *
         * But During an optimize the same thing should happen. The predecessor finish time should limit how early the operation should be allowed to
         * start. What's going on?
         * It seems like this is what should happen. There's nothing from preventing Op20-2 from being scheduled before Op10-1. Op10-1 is the indirect predecessor of
         * Op10-2
         * [TODO]To change it to be the case. Release the successors only after the tank has been emptied.
         * F&N's data won't work in this case. They'll need to adjust the routings so Tank operations are predecessor and successors.
         * */

        // [TANK_CODE]: if the product is stored in a tank, don't release successor operations, the release will be delayed until after the 

        PredOpReady_ProcessSucActivitiesAndSucMOs(a_act, a_primaryResource, PredecessorReadyType.BlockStartAndEndCalculated);

#if TEST
            if (PTSystem.LarryPCAndServer)
            {
                if (_deScheduledActivityCount % 1 == 0)
                {
                    if (System.IO.File.Exists(System.IO.Path.Combine(z_c_tmpDir, "endsim.txt")))
                    {
                        System.IO.StreamWriter sw = System.IO.File.AppendText(System.IO.Path.Combine(z_c_tmpDir, "EndSimDetected.txt"));

                        try
                        {
                            sw.WriteLine("End received at " + DateTime.Now);
                        }
                        finally
                        {
                            sw.Flush();
                            sw.Close();
                        }

                        System.IO.StreamWriter sw2 = System.IO.File.AppendText(ZZScheduledActivityFileName());

                        try
                        {
                            sw2.WriteLine("Ending Sim __scheduledActivityCount=" + _deScheduledActivityCount + " at " + DateTime.Now + " __AttemptToScheduleActivity_Nbr " + _deAttemptToScheduleActivity_Nbr);
                        }
                        finally
                        {
                            sw2.Flush();
                            sw2.Close();
                        }

                        throw new SimulationValidationException("_endSimDetected");
                    }

                    System.IO.StreamWriter sw3 = System.IO.File.AppendText(ZZScheduledActivityFileName());
                    try
                    {
                        long activityDispatcherCount = 0;
                        foreach (Resource r in _deAllResources)
                        {
                            activityDispatcherCount += r.ActiveDispatcher.Count;
                        }

                        DateTime dt = DateTime.Now;

                        DateTime horizon = (new DateTime(Clock)) + ScenarioOptions.PlanningHorizon;


                        sw3.WriteLine("Time " + dt.ToShortDateString() + " " + dt.ToShortTimeString()
                            + " ; SimClock=" + PT.Common.DateTimeHelper.ToLocalTimeFromUTCTicks(a_simClock)
                            + "; __scheduledActivityCount=" + _deScheduledActivityCount + " at " + DateTime.Now
                            + "; activityDispatcherCount = " + activityDispatcherCount
                            + "; __AttemptToScheduleActivity_Nbr " + _deAttemptToScheduleActivity_Nbr
                            + "; _deAlreadyScheduled " + _deAlreadyScheduled
                            + "; _deSchedulabilityCustomization " + _deSchedulabilityCustomization
                            + "; _deSchedulableResult " + _deSchedulableResult
                            + "; _deFindMaterial " + _deFindMaterial
                            + "; _deNonPrimaryRR " + _deNonPrimaryRR
                            + ";_nbrOfJobs " + _nbrOfJobs
                            + ";_nbrOfMos " + _nbrOfMos
                            + ";_nbrOfOps " + _nbrOfOps
                            + ";_nbrOfActs " + _nbrOfActs
                            + "; Clock " + PT.Common.DateTimeHelper.ToLocalTimeFromUTCTicks(Clock)
                            + "; Horizon " + PT.Common.DateTimeHelper.ToLocalTimeFromUTCTicks(horizon.Ticks)
                            );
                    }
                    finally
                    {
                        sw3.Flush();
                        sw3.Close();
                    }
                }
            }
#endif

#if TEST
            if (PTSystem.LarryPCAndServer)
            {
                LogEventTypeCount();
            }
#endif

        // Whether a product is being stored in a tank.
        bool productStoredInTank = false;

        InternalActivityDefs.productionStatuses actProductionStatus = a_act.ProductionStatus;
        
        //TODO: Storage area. update this to account for new storage and material processing logic
        bool skipProduceToInventory = false;

        ProductsCollection operationProducts = a_act.Operation.Products;
        if (operationProducts.Count != 0 && !skipProduceToInventory)
        {
            // Setup release of the products manufactured by the scheduled activity.
            for (int productI = 0; productI < operationProducts.Count; ++productI)
            {
                Product product = operationProducts[productI];
                long productReleaseToStockTicks = a_simClock;
                bool storeCurrentProductInTank = tank != null;

                if (productProfiles.Count == 0 || !productProfiles.TryGetValue(product.Id, out SupplyProfile supplyProfile))
                {
                    continue;
                }

                //we don't need to create adjustments if the available date is calculated to be before the sim clock,
                //we would expect that to be handled in the auto reporting or from imported status updates
                if (supplyProfile.HasNodes && supplyProfile.Last?.Date < a_simClock)
                {
                    continue;
                }

                if (!supplyProfile.HasStorageAllocation)
                {
                    //Other non lot allocations must exist or it wouldn't have succeeded. For example discard adjustments
                    supplyProfile.ConsumeNonStorageAllocations(a_act);
                    continue;
                }

                //TODO: Storage Area Tanks
                //if (storeCurrentProductInTank)
                //{
                //    // If the material is in the tank for too long, such as longer than the product's shelf life, the material will be purged and 
                //    // the resource released. If the material has no shelf-life, it will be purged at the end of the planning horizon.
                //    // Note: If the material is released to stock after the planning horizon, the event isn't created and the material is purged at the end of the
                //    // simulation, which might go past the planning horizon.
                //    CreateExpirationEventForMatlInTank(a_simClock, inventory, tank, a_act, a_act.Operation, product, productReleaseToStockTicks, si);
                //}

                // Add Events for materials of minimum ages.
                // One is created for each of the product's Item's MinAges, giving each
                // MR the opportunity the retry to schedule if it previously failed.
                #region MinAge
                //if (supplyProfile.HasMinAges)
                //{
                //    IEnumerator<QtyNode> etr = productQtyProfile.GetEnumerator();
                //    while (etr.MoveNext())
                //    {
                //        QtyNode node = etr.Current;

                //        long productionDate = node.Date;
                //        HashSet<long>.Enumerator ageEtr = profilesItem.GetMinAgesEnumerator();
                //        while (ageEtr.MoveNext())
                //        {
                //            long MinAgeTicks = ageEtr.Current;
                //            long agedDate = productionDate + MinAgeTicks;
                //            MinAgeEvent mae = new(agedDate, profilesItem);
                //            AddEvent(mae);
                //        }
                //    }
                //}
                #endregion

                List<EventBase> newEvents = new ();
                Lot storedLot = product.Warehouse.StoreOperationProducts(SimClock, a_act, product, a_primaryResource, supplyProfile, newEvents);
                foreach (EventBase eventBase in newEvents)
                {
                    AddEvent(eventBase);
                }

                if (storedLot != null && storedLot.ShelfLifeData.Expirable)
                {
                    AddEvent(new ShelfLifeEvent(storedLot.ShelfLifeData.ExpirationTicks, storedLot));
                }

                //TODO: Storage Area. Also track ItemStorage/StorageArea for this event
                foreach (QtySupplyNode qtyNode in supplyProfile)
                {
                    //Use available date since this affects when the MR can retry
                    QtyToStockEvent qtse = new(qtyNode.AvailableDate, product.Warehouse, product, product.Item);
                    AddEvent(qtse);
                }

                // Retry immediately as there could be MRs that need to calculate a new available date based on this scheduling, potentially before the first QtyToStockEvent is processed
                RetryItemMatlReqs(a_simClock, product.Item);

                //TODO: I dont think we need this any more. The node or profile will have an expiration date
                //AddShelfLifeEvents(supplyProfile, storedLot, product.Item);
            }
        }

        ClearReservations(resReqSatiaters, false);
        ClearFutureReservations(a_act);

        if (a_primaryResource.CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
        {
            // [BATCH_CODE]
            o_resourceConsumed = a_primaryResource.AvailableInSimulation == null;
        }
        else if (a_primaryResource.CapacityType == InternalResourceDefs.capacityTypes.MultiTasking)
        {
            o_resourceConsumed = a_primaryResource.AvailableInSimulation == null;
        }
        else // Infinite
        {
            o_resourceConsumed = false;
        }

        // Calculate the earliest start time against the primary resource for each scheduled resource.
        // Use the maximum of these as the best time to try 
        if (a_act.BeingMoved) //m_usageFeatureActive && a_act.BeingMoved && op.ResourceRequirements.HasMixOfUsageStarts())
        {
            if (!scheduledAtReservedDate // It didn't schedule at the desired time.
                &&
                m_undoReapplyMoveT.ReapplyTypePrevious != UndoReceiveMove.ReapplyTypeEnum.NonLockingMoveRelease) // and it's not allowed to schedule at another time.
            {
                // Re-applying the move allows other activities to overwrite the original 
                // move ticks.
                //m_undoReapplyMoveT.UndoReceivedTransmission = true;
                m_undoReapplyMoveT.ReReceiveTransmission = true;

                //ResourceRequirement moveRR = op.ResourceRequirements.GetByIndex(m_moveData.MoveRRIdx);
                //long startTicks = moveRR.GetRequirementStartTicks(si);
                //m_undoReapplyMoveT.ReReceiveMoveTicks = startTicks;
                // No need to change the move ticks, the move activity will be released at the move time but will be allowed
                // to scheduler whereever it can.

                m_undoReapplyMoveT.SI = si;
                m_undoReapplyMoveT.ReapplyType = UndoReceiveMove.ReapplyTypeEnum.NonLockingMoveRelease;

                return CancelSimulationEnum.Cancel;
            }

            // [USAGE_CODE2] 
            m_undoReapplyMoveT.AddKnownMoveTicks(a_simClock, a_act);

            IEnumerator<IntersectingRRs> intersectingRRetr = GetEnumeratorOfMoveIntersectors();
            bool redo = false;
            List<InternalActivity> moveIntersectors = new ();

            while (intersectingRRetr.MoveNext())
            {
                // Calculate its SI for each intersecting RR && use the one with the latest start date as the constraint.

                // Store the interesting Usage==RUN (where the operation has Mixed usages) in the object.
                // When the BeingMoved activity is scheduled,
                // 

                // If RR.Usage==Start, use SimClock as the constraintDate
                // else if Usage==RUN && Usages are mixed, presume run starts at SimClock, back off the setup time and 
                // calculate where setup would start such as how it's done by SD.PrimarySchedulable().

                // Back calculate the start time of Usage==Run with Mixed usages and remaining setup time
                //

                /*
                 * For each RR
                 *  if(RR==RUN and there's still some setup to perform)
                 *      Presume the RR will start after the intersecting block
                 *      Back calculate the setup release time and add it as a constraint to the redo simulation.
                 *  else
                 *      Presume all the RRs will start after the intersecting block.
                 *      Add an activity constraint at the end of the block to the redo simulation.
                 * */
                InternalActivity act = intersectingRRetr.Current.Activity;
                InternalActivity.SequentialState ss = act.Sequential[act.Operation.ResourceRequirements.PrimaryResourceRequirementIndex];
                Resource primaryRes = (Resource)ss.SequencedSimulationResource;

                RequiredCapacityPlus rc;
                {
                    long start = a_simClock;
                    if (primaryRes.LastScheduledBlockListNode != null)
                    {
                        start = primaryRes.LastScheduledBlockListNode.Data.EndTicks;
                    }

                    rc = primaryRes.CalculateTotalRequiredCapacity(a_simClock, act, LeftNeighborSequenceValues.NullValues, true, start, ExtensionController);
                }

                IntersectingRRs intRR = intersectingRRetr.Current;
                IEnumerator<IntersectingRR> rrEtr = intRR.GetEnumerator();
                while (rrEtr.MoveNext())
                {
                    IntersectingRR intersectingRR = rrEtr.Current;
                    ResourceRequirement rr = intersectingRR.RR;
                    InternalActivity intersectingAct = rrEtr.Current.Activity;
                    // for each intersecting activities RRs
                    //    If there's a matching move satiator
                    //       Use the move satiators end time as the start time for the intersecting act
                    //          If the intersecting acts RR UsageStart==Run, back calculate the start on the primary Res.
                    //          Else save the move satiators time as the time to release the intersector
                    //       Save the latest date found.
                    //     If the latest date is in the found is in the past
                    //        RemoveMoveIntersection
                    //        redo=true;
                    if ((act.Operation.ResourceRequirements.HasMixOfUsageStarts() && rr.UsageStart == MainResourceDefs.usageEnum.Run && rc.SetupSpan.TimeSpanTicks > 0) || rr.UsageStart > MainResourceDefs.usageEnum.Run) // The block this is scheduled in won't start at the current simulation clock.
                    {
                        // Find a matching move satiator.
                        for (int reqI = 0; reqI < resReqSatiaters.Length; ++reqI)
                        {
                            if (resReqSatiaters[reqI].Resource == intersectingRR.Res)
                            {
                                ResourceRequirement movedRR = a_act.Operation.ResourceRequirements.GetByIndex(reqI);
                                //The date the moved activity ends. Use this as the start date of the rr in the way
                                long reqFinishTicks = si.GetUsageEnd(movedRR);
                                if (rrEtr.Current.RR.UsageStart >= MainResourceDefs.usageEnum.Run)
                                {
                                    //TODO: Tanks
                                    RequiredCapacity capacitySpans = null;
                                    if (rrEtr.Current.RR.UsageStart > MainResourceDefs.usageEnum.Clean)
                                    {
                                        capacitySpans = new RequiredCapacity(rc.CleanBeforeSpan, rc.SetupSpan, rc.ProcessingSpan, rc.PostProcessingSpan, rc.StorageSpan, rc.CleanAfterSpan);
                                    }
                                    else if (rrEtr.Current.RR.UsageStart > MainResourceDefs.usageEnum.Storage)
                                    {
                                        capacitySpans = new RequiredCapacity(rc.CleanBeforeSpan, rc.SetupSpan, rc.ProcessingSpan, rc.PostProcessingSpan, rc.StorageSpan, RequiredSpanPlusClean.s_notInit);
                                    }
                                    else if (rrEtr.Current.RR.UsageStart > MainResourceDefs.usageEnum.PostProcessing)
                                    {
                                        capacitySpans = new RequiredCapacity(rc.CleanBeforeSpan, rc.SetupSpan, rc.ProcessingSpan, rc.PostProcessingSpan, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                                    }
                                    else if (rrEtr.Current.RR.UsageStart == MainResourceDefs.usageEnum.PostProcessing)
                                    {
                                        capacitySpans = new RequiredCapacity(rc.CleanBeforeSpan, rc.SetupSpan, rc.ProcessingSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                                    }
                                    else //Run
                                    {
                                        capacitySpans = new RequiredCapacity(rc.CleanBeforeSpan, rc.SetupSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                                    }

                                    Resource.FindStartFromEndResult backCalculatedSU = primaryRes.FindCapacityReverse(Clock, reqFinishTicks, capacitySpans, null, act);
                                    intersectingAct.MoveIntersectingReleaseTicks = Math.Max(intersectingAct.MoveIntersectingReleaseTicks, backCalculatedSU.StartTicks);
                                }
                                else
                                {
                                    intersectingAct.MoveIntersectingReleaseTicks = Math.Max(intersectingAct.MoveIntersectingReleaseTicks, reqFinishTicks);
                                }

                                redo = true;
                                if (!intersectingAct.AddedToMoveIntersectionSet)
                                {
                                    intersectingAct.AddedToMoveIntersectionSet = true;
                                    moveIntersectors.Add(intersectingAct);
                                }

                                break;
                            }
                        }
                    }
                }
            }

            if (redo)
            {
                foreach (InternalActivity act in moveIntersectors)
                {
                    m_undoReapplyMoveT.AddIntersectorRelease(act.MoveIntersectingReleaseTicks, act);
                }

                //m_undoReapplyMoveT.UndoReceivedTransmission = true;
                m_undoReapplyMoveT.ReReceiveTransmission = true;
                return CancelSimulationEnum.Cancel;
            }
        }

        if (ScenarioOptions.CalculateBalancedCompositeScore)
        {
            a_act.CalculateOptimizeFactorTotalsAndStore();
        }

        ExtensionController.ActivityScheduled(a_act, this, a_simClock);
        #if TEST
            if (!m_scheduledTimes.Contains(a_simClock))
            {
                m_scheduledTimes.Add(a_simClock);
            }

            if (!m_activitiesScheduled.Contains(a_act))
            {
                m_activitiesScheduled.Add(a_act);
            }

            if (!m_mosScheduled.Contains(a_act.ManufacturingOrder))
            {
                m_mosScheduled.Add(a_act.ManufacturingOrder);
            }
        #endif
        return CancelSimulationEnum.Continue;
    }

    private AutoSplitInfo AutoSplitOperationByResourceVolumeCapacity(Resource a_primaryResource, InternalActivity a_act, ResourceOperation a_schedulingOperation, ProductRuleManager a_productRuleManager)
    {
        AutoSplitInfo autoSplitInfo = a_act.Operation.AutoSplitInfo;
        autoSplitInfo.NewlySplitActivity = null;
        if (autoSplitInfo.CanAutoSplit(m_activeSimulationType, a_schedulingOperation, a_primaryResource.ManualSchedulingOnly))
        {
            //
            // Volume Auto Split
            //
            if (autoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.ResourceVolumeCapacity && a_act.Operation.Products.PrimaryProduct is Product volumeProduct)
            {
                decimal maxVolume = a_primaryResource.MaxVolume;
                decimal minVolume = a_primaryResource.MinVolume;
                bool useMaxVolume = a_primaryResource.MaxVolumeConstrained; //Only split if there is a max value being used. 0 is not set
                bool useMinVolume = a_primaryResource.MinVolumeConstrained;

                if (a_productRuleManager.GetProductRule(a_primaryResource.Id, volumeProduct.Item.Id, a_schedulingOperation.ProductCode) is ProductRule pr)
                {
                    if (pr.UseMaxVolume)
                    {
                        maxVolume = pr.MaxVolume;
                        useMaxVolume = pr.MaxVolume != 0m && pr.MaxVolume != decimal.MaxValue;
                    }

                    if (pr.UseMinVolume)
                    {
                        minVolume = pr.MinVolume;
                        useMinVolume = pr.MinVolume != 0m && pr.MinVolume != decimal.MinValue;
                    }
                }

                if (useMaxVolume && a_act.TotalRequiredVolume > maxVolume)
                {
                    if (autoSplitInfo.MinAutoSplitAmount > maxVolume || (useMinVolume && autoSplitInfo.MaxAutoSplitAmount < minVolume))
                    {
                        //Can't auto split here
                        return null;
                    }

                    //Check how much qty we should split. We can't always take the max from the resource as it may result in a remainder we can't schedule
                    decimal constrainedSplitQty = CalculateAutoSplitVolume(a_primaryResource, a_act, autoSplitInfo, a_productRuleManager, minVolume, maxVolume);
                    if (constrainedSplitQty == 0)
                    {
                        //No need to split off an activity. Can fit the exact amount
                        return null;
                    }

                    if (constrainedSplitQty < 0)
                    {
                        //Can't split on this resource
                        return null;
                    }

                    //For this split type, we can split here based on the resource, no need to calculate capacity yet
                    //TODO: Verify we can actually make this split
                    InternalActivity newActivity = a_schedulingOperation.AutoSplitByVolume(a_act, a_primaryResource, maxVolume, autoSplitInfo);
                    m_simulationProgress.AutoSplitActivity();

                    //Update the original activity so we can proceed with scheduling
                    a_act.InitializeProductionInfoForResources(PlantManager, a_productRuleManager, ExtensionController);

                    SimulationInitializationAutoSplitActivity(a_productRuleManager, newActivity, autoSplitInfo);
                }
            }
        }

        return autoSplitInfo;
    }

    private AutoSplitInfo AutoSplitOperationByResourceQtyCapacity(Resource a_primaryResource, InternalActivity a_act, ResourceOperation a_schedulingOperation, ProductRuleManager a_productRuleManager)
    {
        AutoSplitInfo autoSplitInfo = a_act.Operation.AutoSplitInfo;
        autoSplitInfo.NewlySplitActivity = null;
        if (autoSplitInfo.CanAutoSplit(m_activeSimulationType, a_schedulingOperation, a_primaryResource.ManualSchedulingOnly))
        {
            //
            // Volume Auto Split
            //
            if (autoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.ResourceQtyCapacity && a_act.Operation.RemainingFinishQty > 0)
            {
                decimal maxQty = a_primaryResource.MaxQty;
                decimal minQty = a_primaryResource.MinQty;
                bool useMaxQuantity = a_primaryResource.MaxQuantityConstrained; //Only split if there is a max value being used. 0 is not set
                bool useMinQty = a_primaryResource.MinQuantityConstrained; //Only split if there is a min value being used. 0 is not set

                if (a_act.Operation.Products.PrimaryProduct is Product volumeProduct)
                {
                    if (a_productRuleManager.GetProductRule(a_primaryResource.Id, volumeProduct.Item.Id, a_schedulingOperation.ProductCode) is ProductRule pr)
                    {
                        if (pr.UseMaxQty)
                        {
                            maxQty = pr.MaxQty;
                            useMaxQuantity = pr.MaxQty != 0m && pr.MaxQty != decimal.MaxValue;
                        }

                        if (pr.UseMinQty)
                        {
                            minQty = pr.MinQty;
                            useMinQty = pr.MinQty != 0m && pr.MinQty != decimal.MinValue;
                        }
                    }
                }

                if (useMaxQuantity && a_act.RequiredFinishQty > maxQty)
                {
                    if ((autoSplitInfo.MinAutoSplitAmount > maxQty) || (useMinQty && autoSplitInfo.MaxAutoSplitAmount < minQty))
                    {
                        //Can't auto split here
                        return null;
                    }

                    //Check how much qty we should split. We can't always take the max from the resource as it may result in a remainder we can't schedule
                    decimal constrainedSplitQty = CalculateAutoSplitQty(a_primaryResource, a_act, autoSplitInfo, a_productRuleManager, minQty, maxQty);
                    if (constrainedSplitQty == 0)
                    {
                        //No need to split off an activity. Can fit the exact amount
                        return autoSplitInfo;
                    }

                    if (constrainedSplitQty < 0)
                    {
                        //Can't split on this resource
                        return null;
                    }

                    //For this split type, we can split here based on the resource, no need to calculate capacity yet
                    //TODO: Verify we can actually make this split
                    InternalActivity newActivity = a_schedulingOperation.AutoSplitByQty(a_act, a_primaryResource, constrainedSplitQty, autoSplitInfo);
                    m_simulationProgress.AutoSplitActivity();

                    //Update the original activity so we can proceed with scheduling
                    a_act.InitializeProductionInfoForResources(PlantManager, a_productRuleManager, ExtensionController);

                    SimulationInitializationAutoSplitActivity(a_productRuleManager, newActivity, autoSplitInfo);
                }
            }
        }

        return autoSplitInfo;
    }

    private AutoSplitInfo AutoSplitOperationForCleanout(Resource a_primaryResource, InternalActivity a_act, long a_simClock, SchedulableInfo a_si, InternalOperation a_schedulingOperation, ProductRuleManager a_productRuleManager, out RequiredSpanPlusClean o_cleanAfterSpan)
    {
        o_cleanAfterSpan = RequiredSpanPlusClean.s_notInit;
        AutoSplitInfo autoSplitInfo = a_act.Operation.AutoSplitInfo;
        RequiredSpanPlusClean timeBasedCleanoutRequired = a_primaryResource.TimeBasedCleanoutRequired(a_si, a_simClock, out long runToSplitOff);
        if (runToSplitOff > 0)
        {
            if (autoSplitInfo.CanAutoSplit(m_activeSimulationType, a_schedulingOperation, a_primaryResource.ManualSchedulingOnly))
            {
                //Check how much qty we should split. We can't always take the max from the resource as it may result in a remainder we can't schedule
                AutoSplitInfo splitInfo = AutoSplitForCleanout(a_act, a_primaryResource, runToSplitOff, a_productRuleManager);
                if (splitInfo != null)
                {
                    o_cleanAfterSpan = timeBasedCleanoutRequired;
                }

                return splitInfo;
            }
        }

        return null;
    }

    /// <summary>
    /// Setup the operation now that it has been split.
    /// This takes into account simulation initialization functions that would have normally been called on an operation at the beginning of the simulation
    /// </summary>
    private void SimulationInitializationAutoSplitActivity(ProductRuleManager a_productRuleManager, InternalActivity newActivity, AutoSplitInfo autoSplitInfo)
    {
        //Prep the new activity for future scheduling
        newActivity.ResetSimulationStateVariables(this);
        newActivity.SimulationInitialization(PlantManager, a_productRuleManager, ExtensionController, this);
        //Filter out manual schedule only resources
        newActivity.Operation.AdjustedPlantResourceEligibilitySets_Filter(ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter.NoFilter());
        autoSplitInfo.NewlySplitActivity = newActivity;
        autoSplitInfo.Split();
    }

    private static decimal CalculateAutoSplitQty(Resource a_schedulingResource, InternalActivity a_schedulingActivity, AutoSplitInfo a_autoSplitInfo, ProductRuleManager a_productRuleManager, decimal a_schedulingResourceMin, decimal a_schedulingResourceMax)
    {
        //We need to verify that any qty we split off is above the minimum of all eligible resources, so we don't create a split that can't schedule
        decimal minAutoSplitOnResources = decimal.MaxValue;
        List<(decimal min, decimal max)> resourceConstraints = new ();
        IEnumerable<Resource> resources = a_schedulingActivity.ResReqsEligibilityNarrowedDuringSimulation.PrimaryEligibilitySet.GetResources();
        foreach (Resource resource in resources)
        {
            if (resource.ManualSchedulingOnly)
            {
                continue;
            }

            decimal resMin = resource.MinQty;
            decimal resMax = resource.MaxQty;
            bool useMinQty = resource.MinQuantityConstrained;
            bool useMaxQty = resource.MaxQuantityConstrained;
            
            if (a_schedulingActivity.Operation.Products.PrimaryProduct is Product volumeProduct)
            {
                if (a_productRuleManager.GetProductRule(resource.Id, volumeProduct.Item.Id, a_schedulingActivity.Operation.ProductCode) is ProductRule pr)
                {
                    if (pr.UseMaxQty)
                    {
                        resMax = pr.MaxQty;
                        useMaxQty = pr.MaxQty != 0m && pr.MaxQty != decimal.MaxValue;
                    }

                    if (pr.UseMinQty)
                    {
                        resMin = pr.MinQty;
                        useMinQty = pr.MinQty != 0m && pr.MinQty != decimal.MinValue;
                    }
                }
            }

            if (!useMinQty || resMin == 1m)
            {
                //There is a resource with no minimum so we can split repeatedly if needed.
                return a_schedulingResourceMax;
            }

            if (!useMaxQty || a_autoSplitInfo.MinAutoSplitAmount > resMax || a_autoSplitInfo.MaxAutoSplitAmount < resMin)
            {
                //TODO:
                continue;
            }

            resMin = Math.Max(resMin, a_autoSplitInfo.MinAutoSplitAmount);
            resMax = Math.Min(resMax, a_autoSplitInfo.MaxAutoSplitAmount);

            resourceConstraints.Add((resMin, resMax));

            minAutoSplitOnResources = Math.Min(resMin, minAutoSplitOnResources);
        }

        if (resourceConstraints.Count == 0)
        {
            return -1;
        }

        //Try max, as is the default
        //decimal remainingQtyAfterSplit = a_schedulingActivity.RequiredFinishQty - a_schedulingResource.MaxQty;
        //if (remainingQtyAfterSplit >= a_autoSplitInfo.MinAutoSplitAmount && remainingQtyAfterSplit >= minAutoSplitOnResources)
        //{
        //    return remainingQtyAfterSplit;
        //}

        decimal qtyToTry = a_schedulingResourceMax;
        int accuracy = Convert.ToInt32(Math.Ceiling((a_schedulingResourceMax - a_schedulingResourceMin) / 100));
        accuracy = Math.Max(accuracy, 1); //Ensure at least 1 increment to avoid infinite loop
        while (qtyToTry >= a_schedulingResourceMin)
        {
            decimal remainder = a_schedulingActivity.RemainingQty - qtyToTry;
            if(remainder <= 0)
            {
                return remainder;
            }

            if (CanDistributeRemainingMaterial(resourceConstraints, remainder, accuracy))
            {
                return qtyToTry;
            }

            qtyToTry -= accuracy;
        }
        
        return -1;
    }

    private static decimal CalculateAutoSplitVolume(Resource a_schedulingResource, InternalActivity a_schedulingActivity, AutoSplitInfo a_autoSplitInfo, ProductRuleManager a_productRuleManager, decimal a_schedulingResourceMin, decimal a_schedulingResourceMax)
    {
        //We need to verify that any qty we split off is above the minimum of all eligible resources, so we don't create a split that can't schedule
        decimal minAutoSplitOnResources = decimal.MaxValue;
        List<(decimal min, decimal max)> resourceConstraints = new();
        IEnumerable<Resource> resources = a_schedulingActivity.ResReqsEligibilityNarrowedDuringSimulation.PrimaryEligibilitySet.GetResources();
        foreach (Resource resource in resources)
        {
            if (resource.ManualSchedulingOnly)
            {
                continue;
            }

            decimal resMin = resource.MinVolume;
            decimal resMax = resource.MaxVolume;
            bool useMinVolume = resource.MinVolumeConstrained;
            bool useMaxVolume = resource.MaxVolumeConstrained;

            if (a_schedulingActivity.Operation.Products.PrimaryProduct is Product volumeProduct)
            {
                if (a_productRuleManager.GetProductRule(resource.Id, volumeProduct.Item.Id, a_schedulingActivity.Operation.ProductCode) is ProductRule pr)
                {
                    if (pr.UseMaxVolume)
                    {
                        resMax = pr.MaxVolume;
                        useMaxVolume = pr.MaxVolume != 0m && pr.MaxVolume != decimal.MaxValue;
                    }

                    if (pr.UseMinVolume)
                    {
                        resMin = pr.MinVolume;
                        useMinVolume = pr.MinVolume != 0m && pr.MinVolume != decimal.MinValue;
                    }
                }
            }

            if (!useMinVolume || resMin == 1m)
            {
                //There is a resource with no minimum so we can split repeatedly if needed.
                return a_schedulingResourceMax;
            }

            if (!useMaxVolume || a_autoSplitInfo.MinAutoSplitAmount > resMax || a_autoSplitInfo.MaxAutoSplitAmount < resMin)
            {
                //TODO:
                continue;
            }

            resMin = Math.Max(resMin, a_autoSplitInfo.MinAutoSplitAmount);
            resMax = Math.Min(resMax, a_autoSplitInfo.MaxAutoSplitAmount);

            resourceConstraints.Add((resMin, resMax));

            minAutoSplitOnResources = Math.Min(resMin, minAutoSplitOnResources);
        }

        if (resourceConstraints.Count == 0)
        {
            return -1;
        }

        //Try max, as is the default
        //decimal remainingQtyAfterSplit = a_schedulingActivity.RequiredFinishQty - a_schedulingResource.MaxQty;
        //if (remainingQtyAfterSplit >= a_autoSplitInfo.MinAutoSplitAmount && remainingQtyAfterSplit >= minAutoSplitOnResources)
        //{
        //    return remainingQtyAfterSplit;
        //}

        decimal qtyToTry = a_schedulingResourceMax;
        int accuracy = Convert.ToInt32(Math.Ceiling((a_schedulingResourceMax - a_schedulingResourceMin) / 100));
        accuracy = Math.Max(accuracy, 1); //Ensure at least 1 increment to avoid infinite loop
        while (qtyToTry >= a_schedulingResourceMin)
        {
            decimal remainder = a_schedulingActivity.RemainingRequiredVolume - qtyToTry;
            if (remainder <= 0)
            {
                return remainder;
            }

            if (CanDistributeRemainingMaterial(resourceConstraints, remainder, accuracy))
            {
                return qtyToTry;
            }

            qtyToTry -= accuracy;
        }

        return -1;
    }

    private static bool CanDistributeRemainingMaterial(List<(decimal min, decimal max)> a_bins, decimal a_remainingQty, int a_accuracy)
    {
        if (a_remainingQty == 0)
        {
            return true;
        }

        //Check multiples of remainder
        foreach ((decimal min, decimal max) bin in a_bins)
        {
            //If any min can evenly divide the remainder, it's still possible
            if (a_remainingQty % bin.min == 0)
            {
                return true;
            }
        }

        foreach (var bin in a_bins)
        {
            if (a_remainingQty >= bin.min && a_remainingQty <= bin.max)
            {
                //this bin can fit the qty
                return true;
            }

            for (int qty = 0; qty + bin.min <= bin.max; qty += a_accuracy)
            {
                decimal splitQty = a_remainingQty - (qty + bin.min);
                if (splitQty <= 0)
                {
                    break;
                }

                if (qty <= a_remainingQty && CanDistributeRemainingMaterial(a_bins, splitQty, a_accuracy))
                {
                    return true;
                }
            }
        }

        return false; // Cannot distribute material correctly
    }

    private AutoSplitInfo AutoSplitOperationByPredecessorMaterials(Resource a_primaryResource, InternalActivity a_schedulingActivity, ProductRuleManager a_productRuleManager)
    {
        ResourceOperation schedulingOperation = (ResourceOperation)a_schedulingActivity.Operation;
        AutoSplitInfo autoSplitInfo = schedulingOperation.AutoSplitInfo;
        autoSplitInfo.NewlySplitActivity = null;
        if (autoSplitInfo.CanAutoSplit(m_activeSimulationType, schedulingOperation, a_primaryResource.ManualSchedulingOnly))
        {
            //
            // Predecessor materials Auto Split
            //
            if (autoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.PredecessorMaterials 
                && schedulingOperation.Predecessors.Count > 0 
                && schedulingOperation.MaterialRequirements.StockMaterials.Any())
            {
                Dictionary<BaseId, MaterialRequirement> mrCache = new ();
                foreach (MaterialRequirement mr in schedulingOperation.MaterialRequirements.StockMaterials)
                {
                    mrCache.AddIfNew(mr.Item.Id, mr);
                }

                Product matchingPredProduct = null;
                MaterialRequirement matchingMr = null;
                InternalOperation predOperation = null;
                //Find the predecessor operation with a product that matches this operation's materials
                foreach (AlternatePath.Association predAssociations in schedulingOperation.Predecessors)
                {
                    InternalOperation predOp = predAssociations.Predecessor.Operation;
                    if (predOp.Products.Count > 0)
                    {
                        Product predProduct = predOp.Products.PrimaryProduct;
                        if (mrCache.TryGetValue(predProduct.Item.Id, out MaterialRequirement mr))
                        {
                            //This could be the one we are looking for, compare warehouses
                            if (predProduct.Warehouse == mr.Warehouse || mr.Warehouse == null)
                            {
                                matchingPredProduct = predProduct;
                                matchingMr = mr;
                                predOperation = predOp;
                            }
                        }
                    }
                }

                if (matchingPredProduct == null)
                {
                    //Can't auto split here
                    return autoSplitInfo;
                }

                //Allocate by activity. Start with all currently scheduled predecessor activities
                Dictionary<BaseId, InternalActivity> scheduledPredActs = new ();

                foreach (InternalActivity predAct in predOperation.Activities)
                {
                    if (predAct.Scheduled)
                    {
                        scheduledPredActs.Add(predAct.Id, predAct);
                    }
                }

                //For every previously split and scheduled activity of this operation, we can remove the predecessor it was already split for
                foreach (InternalActivity scuAct in schedulingOperation.Activities)
                {
                    if (scuAct.Scheduled && schedulingOperation.AutoSplitInfo.PredecessorSplitMappings != null)
                    {
                        if (schedulingOperation.AutoSplitInfo.PredecessorSplitMappings.TryGetValue(scuAct.Id, out BaseId predActId))
                        {
                            scheduledPredActs.Remove(predActId);
                        }
                    }
                }

                List<InternalActivity> predActivities = scheduledPredActs.Values.ToList();
                if (predActivities.Count == 0)
                {
                    if (predOperation.Scheduled)
                    {
                        //There are no more predecessor splits, nothing to schedule
                        return autoSplitInfo;
                    }

                    //We are still waiting on another predecessor activity, we can't schedule this yet
                    return autoSplitInfo;
                }

                predActivities.Sort((a1, a2) => a1.ScheduledStartDate.CompareTo(a2.ScheduledStartDate));
                InternalActivity predecessorActivityToSource = predActivities[0];
                decimal splitProductQty = predecessorActivityToSource.PrimaryProductQty;

                //Store this mapping for future splits
                if (autoSplitInfo.PredecessorSplitMappings.TryGetValue(a_schedulingActivity.Id, out BaseId predId))
                {
                    autoSplitInfo.PredecessorSplitMappings[a_schedulingActivity.Id] = predecessorActivityToSource.Id;
                }
                else
                {
                    autoSplitInfo.PredecessorSplitMappings.Add(a_schedulingActivity.Id, predecessorActivityToSource.Id);
                }

                InternalActivity newActivity = schedulingOperation.AutoSplitByMaterial(a_schedulingActivity, a_primaryResource, matchingMr, splitProductQty, ScenarioOptions);
                if (newActivity != null) //It's possible there was nothing to split
                {
                    m_simulationProgress.AutoSplitActivity();

                    //Update the original activity so we can proceed with scheduling
                    a_schedulingActivity.InitializeProductionInfoForResources(PlantManager, a_productRuleManager, ExtensionController);

                    SimulationInitializationAutoSplitActivity(a_productRuleManager, newActivity, autoSplitInfo);
                }
            }
        }

        return autoSplitInfo;
    }

    private AutoSplitInfo AutoSplitOperationByPredecessorQuantity(Resource a_primaryResource, InternalActivity a_schedulingActivity, ProductRuleManager a_productRuleManager)
    {
        ResourceOperation schedulingOperation = (ResourceOperation)a_schedulingActivity.Operation;
        AutoSplitInfo autoSplitInfo = schedulingOperation.AutoSplitInfo;
        autoSplitInfo.NewlySplitActivity = null;
        if (autoSplitInfo.CanAutoSplit(m_activeSimulationType, schedulingOperation, a_primaryResource.ManualSchedulingOnly))
        {
            //
            // Predecessor materials Auto Split
            //
            if (autoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.PredecessorQuantityRatio
                && schedulingOperation.Predecessors.Count > 0)
            {
                InternalOperation predOperation = null;
                //Find the predecessor operation with a product that matches this operation's materials
                foreach (AlternatePath.Association predAssociations in schedulingOperation.Predecessors)
                {
                    InternalOperation predOp = predAssociations.Predecessor.Operation;
                    if (predOp.Split)
                    {
                        //This could be the one we are looking for, compare warehouses
                        predOperation = predOp;
                    }
                }

                if (predOperation == null)
                {
                    //Can't auto split here
                    return autoSplitInfo;
                }

                //Allocate by activity. Start with all currently scheduled predecessor activities
                Dictionary<BaseId, InternalActivity> scheduledPredActs = new ();

                foreach (InternalActivity predAct in predOperation.Activities)
                {
                    if (predAct.Scheduled)
                    {
                        scheduledPredActs.Add(predAct.Id, predAct);
                    }
                }

                //For every previously split and scheduled activity of this operation, we can remove the predecessor it was already split for
                foreach (InternalActivity scuAct in schedulingOperation.Activities)
                {
                    if (scuAct.Scheduled && schedulingOperation.AutoSplitInfo.PredecessorSplitMappings != null)
                    {
                        if (schedulingOperation.AutoSplitInfo.PredecessorSplitMappings.TryGetValue(scuAct.Id, out BaseId predActId))
                        {
                            scheduledPredActs.Remove(predActId);
                        }
                    }
                }

                List<InternalActivity> predActivities = scheduledPredActs.Values.ToList();
                if (predActivities.Count == 0)
                {
                    if (predOperation.Scheduled)
                    {
                        //There are no more predecessor splits, nothing to schedule
                        return autoSplitInfo;
                    }

                    //We are still waiting on another predecessor activity, we can't schedule this yet
                    return autoSplitInfo;
                }

                predActivities.Sort((a1, a2) => a1.ScheduledStartDate.CompareTo(a2.ScheduledStartDate));
                InternalActivity predecessorActivityToSource = predActivities[0];
                decimal splitPredQty = predecessorActivityToSource.RequiredFinishQty;

                //Store this mapping for future splits
                if (autoSplitInfo.PredecessorSplitMappings.TryGetValue(a_schedulingActivity.Id, out BaseId predId))
                {
                    autoSplitInfo.PredecessorSplitMappings[a_schedulingActivity.Id] = predecessorActivityToSource.Id;
                }
                else
                {
                    autoSplitInfo.PredecessorSplitMappings.Add(a_schedulingActivity.Id, predecessorActivityToSource.Id);
                }

                InternalActivity newActivity = schedulingOperation.AutoSplitByQty(a_schedulingActivity, a_primaryResource, splitPredQty, ScenarioOptions);
                if (newActivity != null) //It's possible there was nothing to split
                {
                    m_simulationProgress.AutoSplitActivity();

                    //Update the original activity so we can proceed with scheduling
                    a_schedulingActivity.InitializeProductionInfoForResources(PlantManager, a_productRuleManager, ExtensionController);

                    SimulationInitializationAutoSplitActivity(a_productRuleManager, newActivity, autoSplitInfo);
                }
            }
        }

        return autoSplitInfo;
    }

    /// <summary>
    /// If some capacity was found, but not enough to run the full operations, consider splitting the Production to schedule what we can
    /// </summary>
    /// <param name="a_schedulingActivity"></param>
    /// <param name="a_schedulingResource"></param>
    /// <param name="a_ocp">The capacity by production stage</param>
    /// <param name="a_rc">The total required capacity</param>
    /// <returns></returns>
    internal AutoSplitInfo AutoSplitByAvailableCapacity(InternalActivity a_schedulingActivity, Resource a_schedulingResource, OperationCapacityProfile a_ocp, RequiredCapacityPlus a_rc, ProductRuleManager a_productRuleManager)
    {
        ResourceOperation schedulingOperation = (ResourceOperation)a_schedulingActivity.Operation;
        AutoSplitInfo autoSplitInfo = schedulingOperation.AutoSplitInfo;

        if (a_schedulingActivity.Operation.QtyPerCycle >= a_schedulingActivity.Operation.RequiredFinishQty)
        {
            //Single cycle
            //We can't split a single cycle, it always runs the same time
            return null;
        }

        //There is some capacity available, we may be able to split the operation to schedule a partial
        if (a_ocp.SetupProfile.CapacityFound < a_rc.SetupSpan.TimeSpanTicks)
        {
            //We didn't find all the setup, nothing to do here, we can't split setup.
            //NOTE: Technically we might be able to split the run and reduce setup to schedule some setup and run. Not efficient and not worth it now.
        }
        else if (a_ocp.ProductionProfile.CapacityFound < a_rc.ProcessingSpan.TimeSpanTicks)
        {
            //Split by cycles that will fit
            ProductionInfo productionInfo = a_schedulingActivity.GetResourceProductionInfo(a_schedulingResource);
            decimal cyclesToRun = Math.Floor((decimal)a_ocp.ProductionProfile.CapacityFound / productionInfo.CycleSpanTicks);
            decimal maxCycles = Math.Floor(autoSplitInfo.MaxAutoSplitAmount / productionInfo.QtyPerCycle);
           
            //Constrain by the maximum cycles to produce up to the max auto-split amount
            cyclesToRun = Math.Min(cyclesToRun, maxCycles);
            decimal qtyToSplit = cyclesToRun * productionInfo.QtyPerCycle;

            if (qtyToSplit < autoSplitInfo.MinAutoSplitAmount)
            {
                return null;
            }

            InternalActivity newActivity = schedulingOperation.AutoSplitByQty(a_schedulingActivity, a_schedulingResource, qtyToSplit, a_schedulingActivity.ScenarioDetail.ScenarioOptions);

            if (newActivity != null) //It's possible there was nothing to split
            {
                m_simulationProgress.AutoSplitActivity();

                //Store this mapping for future splits
                if (autoSplitInfo.PredecessorSplitMappings == null)
                {
                    autoSplitInfo.PredecessorSplitMappings = new Dictionary<BaseId, BaseId>();
                }

                //Update the original activity so we can proceed with scheduling
                a_schedulingActivity.InitializeProductionInfoForResources(PlantManager, a_productRuleManager, ExtensionController);

                SimulationInitializationAutoSplitActivity(a_productRuleManager, newActivity, autoSplitInfo);

                return autoSplitInfo;
            }
        }
        else if (a_ocp.PostprocessingProfile.CapacityFound < a_rc.PostProcessingSpan.TimeSpanTicks)
        {
            //TODO: We could potentially split run to make room
        }

        return null; //No Split
    }
    
    internal AutoSplitInfo AutoSplitForCleanout(InternalActivity a_schedulingActivity, Resource a_schedulingResource, long a_runTicksToSplitOff, ProductRuleManager a_productRuleManager)
    {
        ResourceOperation schedulingOperation = (ResourceOperation)a_schedulingActivity.Operation;
        AutoSplitInfo autoSplitInfo = schedulingOperation.AutoSplitInfo;

        if (a_schedulingActivity.Operation.QtyPerCycle >= a_schedulingActivity.Operation.RequiredFinishQty)
        {
            //Single cycle
            //We can't split a single cycle, it always runs the same time
            return null;
        }

        //Split by cycles that will fit
        ProductionInfo productionInfo = a_schedulingActivity.GetResourceProductionInfo(a_schedulingResource);
        decimal cyclesToRun = Math.Floor((decimal)a_runTicksToSplitOff / productionInfo.CycleSpanTicks);
        if (autoSplitInfo.UseMaxAutoSplitAmount)
        {
            decimal maxCycles = Math.Floor(autoSplitInfo.MaxAutoSplitAmount / productionInfo.QtyPerCycle);

            //Constrain by the maximum cycles to produce up to the max auto-split amount
            cyclesToRun = Math.Min(cyclesToRun, maxCycles);
        }
        
        decimal qtyToSplit = cyclesToRun * productionInfo.QtyPerCycle;

        if (autoSplitInfo.UseMinAutoSplitAmount && qtyToSplit < autoSplitInfo.MinAutoSplitAmount)
        {
            return null;
        }

        InternalActivity newActivity = schedulingOperation.AutoSplitByQty(a_schedulingActivity, a_schedulingResource, qtyToSplit, a_schedulingActivity.ScenarioDetail.ScenarioOptions);

        if (newActivity != null) //It's possible there was nothing to split
        {
            m_simulationProgress.AutoSplitActivity();

            //Store this mapping for future splits
            if (autoSplitInfo.PredecessorSplitMappings == null)
            {
                autoSplitInfo.PredecessorSplitMappings = new Dictionary<BaseId, BaseId>();
            }

            //Update the original activity so we can proceed with scheduling
            a_schedulingActivity.InitializeProductionInfoForResources(PlantManager, a_productRuleManager, ExtensionController);

            SimulationInitializationAutoSplitActivity(a_productRuleManager, newActivity, autoSplitInfo);

            return autoSplitInfo;
        }

        return null; //No Split
    }

    private void RejoinFailedToScheduleActivity(InternalActivity a_act, AutoSplitInfo a_autoSplitInfo)
    {
        if (a_autoSplitInfo?.NewlySplitActivity != null)
        {
            (a_act.Operation as ResourceOperation).AutoJoin(a_act, a_autoSplitInfo.NewlySplitActivity, a_autoSplitInfo.OriginalSplitSetupTime);
            m_simulationProgress.AutoJoinedActivity();
            if (!a_act.Operation.Split)
            {
                //It's possible that another partial split is being rejoined, but the operation has been previously autosplit
                a_autoSplitInfo.Unsplit();
            }

            a_autoSplitInfo.NewlySplitActivity = null;
            a_autoSplitInfo.PredecessorSplitMappings?.Remove(a_act.Id);

            a_act.InitializeProductionInfoForResources(PlantManager, ProductRuleManager, ExtensionController);
        }
    }

    /// <summary>
    /// Retry fullfillment of material requirements that couldn't be fullfilled at their need date.
    /// For instance when sales orders and transfer orders aren't able to be fullfilled, their events
    /// are added to the Item.RetryReqMatl and fullfillment is retried in the future when material might be available.
    /// </summary>
    /// <param name="a_retryEvtTicks"></param>
    /// <param name="a_retryItem"></param>
    private void RetryItemMatlReqs(long a_retryEvtTicks, Item a_retryItem)
    {
        foreach (EventBase eb in a_retryItem.GetRetryReqMatlEvtSetEnumerator())
        {
            eb.Time = a_retryEvtTicks;
            AddEvent(eb);
        }

        a_retryItem.ClearRetryMatlEvtSet();
    }

    /// <summary>
    /// Retry activities that previously failed to schedule due to storage constriants
    /// </summary>
    /// <param name="a_retryEvtTicks"></param>
    /// <param name="a_retryItem"></param>
    private void RetryItemStorageReqs(long a_retryEvtTicks, Item a_retryItem)
    {
        foreach (EventBase eb in a_retryItem.GetRetryStorageEventSet())
        {
            eb.Time = a_retryEvtTicks;
            AddEvent(eb);
        }

        a_retryItem.ClearRetryStorageEventSet();
    }

    //private void AddShelfLifeEvents(QtyProfile a_profile, Lot a_expirableLot, Item a_item)
    //{
    //    QtyProfileTimePoint firstNode = a_profile.First;
    //    long productionDate = firstNode.Date;
    //    long expirationDate = a_item.CalcExpiration(productionDate);

    //    if (a_item.LotUsability == ItemDefs.LotUsability.ShelfLifeNonConstraint)
    //    {
    //        if (SimClock >= m_endOfPlanningHorizon)
    //        {
    //            //The simulation is already past the planning horizon. no need to worry about expirations.
    //            return;
    //        }
    //        //Only expire material before the end of the horizon if the constraint is enabled
    //        expirationDate = m_endOfPlanningHorizon;
    //    }
    //    else if (ExtensionController.RunAdjustShelfLifeExtension)
    //    {
    //        long? customExpirationTicks = ExtensionController.AdjustShelfLife(this, firstNode.Date, expirationDate, a_profile.Qty, a_expirableLot, a_item);
    //        if (customExpirationTicks.HasValue)
    //        {
    //            expirationDate = customExpirationTicks.Value;
    //        }
    //    }

    //    ShelfLifeEvent sl = new(expirationDate, a_expirableLot, a_item);
    //    AddEvent(sl);

    //}

    /// <summary>
    /// Schedule a resource requirement.
    /// </summary>
    /// <param name="a_simClock"></param>
    /// <param name="a_primaryResource"></param>
    /// <param name="a_res"></param>
    /// <param name="tank"></param>
    /// <param name="hasProductsToStoreInTank"></param>
    /// <param name="a_act"></param>
    /// <param name="operation"></param>
    /// <param name="reqI"></param>
    /// <param name="rr"></param>
    /// <param name="si"></param>
    /// <param name="finishDate"></param>
    /// <param name="batchToJoin"></param>
    /// <param name="newBatch"></param>
    /// <param name="a_ignoreRciProfile"></param>
    /// <returns></returns>
    private ResourceBlockList.Node ScheduleRR(long a_simClock,
                                              Resource a_primaryResource,
                                              Resource a_res,
                                              Resource tank,
                                              bool hasProductsToStoreInTank,
                                              InternalActivity a_act,
                                              InternalOperation operation,
                                              int reqI,
                                              ResourceRequirement rr,
                                              SchedulableInfo si,
                                              long finishDate,
                                              BatchToJoinTempStruct batchToJoin,
                                              Batch newBatch,
                                              bool a_ignoreRciProfile)
    {
        ResourceBlockList.Node scheduledNode;

        // [BATCH_CODE]Pass the batch block to resource here so it can create a block.
        if (batchToJoin.NoJoin())
        {
            scheduledNode = a_res.Schedule(this, new BaseId(reqI), newBatch, a_act, rr, a_simClock, finishDate, si, a_ignoreRciProfile);
            newBatch.m_rrBlockNodes[reqI] = scheduledNode;
            a_act.Schedule(scheduledNode.Data);

            // [TANK_CODE]: When a move is performed sometimes operations on the same tank must be moved.
            if (a_act.MoveRelatedConstrainedActivities != null)
            {
                Dictionary<BaseId, InternalActivity>.Enumerator etr = a_act.MoveRelatedConstrainedActivities.GetEnumerator();
                while (etr.MoveNext())
                {
                    if (etr.Current.Value.Released)
                    {
                        AddActivityReleasedEvent(etr.Current.Value, finishDate);
                    }
                }
            }

            if (a_res.CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
            {
                bool addResAvailableEvent = false;
                // [BATCH_CODE]
                if (a_res.BatchResource() && a_res == a_primaryResource)
                {
                    if (a_primaryResource.BatchType == MainResourceDefs.batchType.Percent)
                    {
                        #if DEBUG
                        if (a_act.Batch.BatchPercentRemaining < 0)
                        {
                            throw new Exception("Batch Error: value negative a_act.Batch.BatchPercentRemaining < 0");
                        }
                        #endif
                        a_primaryResource.AvailablePrimaryNode.m_removeFromAvailableResourceEventSet = a_act.Batch.BatchPercentRemaining <= 0;
                    }
                    else // Batch by volume.
                    {
                        #if DEBUG
                        if (a_act.Batch.BatchVolumeRemaining < 0)
                        {
                            throw new Exception("Batch Error: value negative a_act.Batch.BatchVolumeRemaining < 0");
                        }
                        #endif
                        a_primaryResource.AvailablePrimaryNode.m_removeFromAvailableResourceEventSet = a_act.Batch.BatchVolumeRemaining <= 0;
                    }

                    if (a_primaryResource.AvailablePrimaryNode.m_removeFromAvailableResourceEventSet)
                    {
                        // Remove the resource from the available set of resources since it's batch capacity has been completely consumed.
                        m_availableResourceEventsSet.Remove(a_primaryResource);
                        addResAvailableEvent = true;
                    }
                    else
                    {
                        // Mark the batch resource for removal from the available set of resources after the point where the batch begins.
                        a_primaryResource.AvailablePrimaryNode.BatchResScheduleResourceUnavilableEventForPrimaryRes = true;
                    }
                }
                else
                {
                    m_availableResourceEventsSet.Remove(a_res);
                    addResAvailableEvent = true;
                }

                // [TANK_CODE]: The tank isn't released until the material has been drained.
                //if (tank != null && hasProductsToStoreInTank)
                //{
                //    if (rr.UsageEnd != MainResourceDefs.usageEnum.Storage
                //        && rr.UsageEnd != MainResourceDefs.usageEnum.StoragePostProcessing
                //        && rr.UsageEnd != MainResourceDefs.usageEnum.StorageClean)
                //    {
                //        // The resoure isn't used after Post-Processing. Release it at the end of post-processing.
                //        AddResourceAvailableEvent(AddResourceavailableEventType.AtSpecifiedTime, scheduledNode.Data.EndTicks, a_res, null);
                //    }
                //    else if (a_act.ProductionStatus == InternalActivityDefs.productionStatuses.Cleaning)
                //    {
                //        AddResourceAvailableEvent(AddResourceavailableEventType.AtSpecifiedTime, newBatch.EndTicks, a_res, null);
                //    }
                //    else
                //    {
                //        // This function also cancels the last added resource unavailable Event.
                //        // The tank resource is unavailable until it has been emptied.
                //        CancelLastScheduledAddedResourceUnavailableEvent(a_res);

                //        // If the material is in the tank for too long, such as longer than the product's shelf life, the material will be purged and 
                //        // the resource released.
                //    }
                //}
                //else if (addResAvailableEvent)
                {
                    LinkedListNode<ResourceCapacityInterval> availNode;
                    long nextAvailTicks;
                    a_res.FindNextOnlineTime(scheduledNode.Data.EndTicks, null, out nextAvailTicks, out availNode);
                    AddResourceAvailableEvent(AddResourceavailableEventType.AtSpecifiedTime, nextAvailTicks, a_res, null);
                }
            }
            else if (a_res.CapacityType == InternalResourceDefs.capacityTypes.MultiTasking)
            {
                ResourceBlock block = scheduledNode.Data;

                BlockFinishedEvent blockFinishedEvent = new (block.EndTicks, block);
                AddEvent(blockFinishedEvent);

                LinkedListNode<ResourceCapacityInterval> rciNode = a_res.AvailableNode.Data.Node;
                ResourceCapacityInterval rci = rciNode.Value;

                // [USAGE_CODE]
                if (rci.CalcAvailableAttentionPointInTime(block.StartTicks) == 0)
                {
                    // Remove the resource from the set of available resources.
                    m_availableResourceEventsSet.Remove(a_res);

                    ScheduleNextMultiTaskingResourceAvailableEvent(a_res, rciNode);

                    // We don't need this event anymore since the next available resource event was added.
                    CancelLastScheduledAddedResourceUnavailableEvent(a_res);
                }
            }
        }
        else
        {
            // [BATCH_CODE]
            a_act.Schedule(batchToJoin.GetRRBlock(reqI));
            scheduledNode = batchToJoin.Batch.m_rrBlockNodes[reqI];
        }

        //SequentialStateHandling(a_act, reqI, finishDate);

        if (m_activeSimulationType == SimulationType.Move ||
            m_activeSimulationType == SimulationType.MoveAndExpedite)
        {
            if (a_act.RightNeighbor != null)
            {
                AddEvent(new RightMovingNeighborReleaseEvent(scheduledNode.Data.EndTicks, a_act.RightNeighbor));
            }
        }

        return scheduledNode;
    }

    private void CreateExpirationEventForMatlInTank(long a_simClock, Inventory a_inv, Resource a_tank, InternalActivity a_act, InternalOperation operation, Product a_product, long a_productReleaseToStockTicks, SchedulableInfo si)
    {
        // The default shelf life is a large number such as long.MaxValue and causes an overflow.
        // So the length of the planning horizon is used if there is no shelf life or it's set to the default.
        long tankEndTicks;
        if (a_productReleaseToStockTicks > EndOfPlanningHorizon)
        {
            //It's possible that the material will be released past the planning horizon. In this case, end immediately after the material is available.
            tankEndTicks = a_productReleaseToStockTicks;
        }
        else
        {
            long shelfLife = a_product.Item.ShelfLifeTicks;

            if (EndOfPlanningHorizon < a_productReleaseToStockTicks + shelfLife)
            {
                //The Planning horizon ends before the shelflife, empty at the planning horizon.
                tankEndTicks = EndOfPlanningHorizon;
            }
            else
            {
                //Empty at the shelflife expiration
                tankEndTicks = a_productReleaseToStockTicks + shelfLife;
            }
        }

        // This event will check whether the tank has been released by this time.
        MaterialInTankShelfLifeExpiredEvent expiredEvent = new (a_inv, a_act.Batch, a_tank, tankEndTicks);
        AddEvent(expiredEvent);
    }

    /// <summary>
    /// Clear a move's resource reservations.
    /// </summary>
    /// <param name="a_act">One of the activities the resource reservations were made for.</param>
    private void ClearMovesResourceReservations(InternalActivity a_act)
    {
        a_act.ClearMoveResourceReservations();
        m_moveTicksState = MoveReleaseState.Released;
    }

    /// <summary>
    /// Release the resources in use by a tank. This sets the batches EndOfStorageTicks and EndTicks (which can include end of storage post processing).
    /// </summary>
    /// <param name="a_tankBatch">A batchs that has products that are being stored in a tank.</param>
    /// <param name="a_tankEndTicks">The time to release the resources being used with the tank.</param>
    //private void ReleaseTankResources(long a_simClock, ResourceWarehouse a_rwh, TankBatch a_tankBatch, long a_tankEndTicks)
    //{
    //    // Get a reference to the first activity in the batch and to the production information.
    //    InternalActivity firstTankActivity = a_tankBatch.GetFirstActivity();
    //    TankProductionInfo tankProdInfo = firstTankActivity.GetResourceProductionInfo(a_tankBatch.PrimaryResource) as TankProductionInfo;
    //    ResourceRequirementManager tankRrm = firstTankActivity.Operation.ResourceRequirements;

    //    #if DEBUG
    //    if (tankRrm.Count != a_tankBatch.BlockCount)
    //    {
    //        // The number of block scheduled for the batch must always equal the number resource requirements of each activity that's part of the batch.
    //        throw new Exception("The batch's block count doesn't match the first activity's resource requirement count.");
    //    }
    //    #endif

    //    if (tankProdInfo != null)
    //    {
    //        long tankEndOfPostProcessing = a_tankEndTicks;
    //        long tankEndOfClean = a_tankEndTicks;

    //        // [TANK_CODE]: Schedule the release of the tanks and helper resources that are used during storage and/or storage post processing.
    //        for (int blockI = 0; blockI < a_tankBatch.BlockCount; ++blockI)
    //        {
    //            ResourceBlock tanksBlock = a_tankBatch.BlockAtIndex(blockI);
    //            ResourceRequirement tankOpRR = tankRrm.GetByIndex(blockI);
    //            MainResourceDefs.usageEnum tankUsageEnd = tankOpRR.UsageEnd;

    //            if (tankUsageEnd == MainResourceDefs.usageEnum.Storage
    //                || tankUsageEnd == MainResourceDefs.usageEnum.StoragePostProcessing
    //                || tankUsageEnd == MainResourceDefs.usageEnum.StorageClean)
    //            {
    //                // [TANK_STORAGE] Determine when the tank will become available.

    //                long blockPostProcessingEnd = a_tankEndTicks;
    //                //long blockCleanEnd = a_tankEndOfStorage;

    //                // Add EndOfStoragePostProcessing if necessary
    //                if (tankOpRR.UsageEnd >= MainResourceDefs.usageEnum.StoragePostProcessing)
    //                {
    //                    FindCapacityResult postProcessingResult = a_tankBatch.PrimaryResource.FindCapacityForStoragePostProcessing(a_tankEndTicks, tankProdInfo.StoragePostProcessingTicks, a_tankBatch, tankOpRR.CapacityCode);
    //                    blockPostProcessingEnd = postProcessingResult.FinishDate;
    //                    if (postProcessingResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
    //                    {
    //                        //The storage post processing can't be scheduled. For example we hit a cleanout
    //                        //TODO:
    //                        blockPostProcessingEnd = a_tankEndTicks + tankProdInfo.StoragePostProcessingTicks;
    //                    }

    //                    tanksBlock.CapacityProfile.Add(postProcessingResult.CapacityUsageProfile, MainResourceDefs.usageEnum.StoragePostProcessing);
    //                    //blockCleanEnd = blockPostProcessingEnd;
    //                }

    //                //if (tankOpRR.UsageEnd >= MainResourceDefs.usageEnum.StorageClean)
    //                //{
    //                //    SchedulableSuccessFailureEnum result = a_tankBatch.PrimaryResource.FindCapacityForStoragePostProcessing(blockPostProcessingEnd, tankProdInfo.CleanSpanTicks, a_tankBatch, out blockCleanEnd);
    //                //    if (result != SchedulableSuccessFailureEnum.Success)
    //                //    {
    //                //        //The storage clean can't be scheduled. For example we hit a cleanout
    //                //        //TODO:
    //                //        blockCleanEnd = blockPostProcessingEnd + tankProdInfo.StoragePostProcessingTicks;
    //                //    }
    //                //}

    //                // Finalize the tank's end of storage and end.

    //                tankEndOfPostProcessing = Math.Max(tankEndOfPostProcessing, blockPostProcessingEnd);
    //                //tankEndOfClean = Math.Max(tankEndOfClean, blockCleanEnd);
    //                tanksBlock.EndTicks = Math.Max(tankEndOfPostProcessing, tankEndOfClean);
    //                AddResourceAvailableEventAtNextOnlineTime(tanksBlock.ScheduledResource, tanksBlock.EndTicks, null);

                    //InternalActivity batchFirstActivity = tanksBlock.Batch.FirstActivity;
                    //if (tanksBlock.ScheduledResource.Reservations.Intersections(batchFirstActivity, a_simClock, tankEndOfPostProcessing, out IEnumerable<ResourceReservation> reservationConflicts))
                    //{
                    //    foreach (ResourceReservation reservationConflict in reservationConflicts)
                    //    {
                    //        //We have an unfortunate conflict we will try to resolve by moving this reservation to another available resource
                    //        List<Resource> eligibleResources = batchFirstActivity.Operation.ResourceRequirements.GetByIndex(tanksBlock.ResourceRequirementIndex).GetEligibleResources();
                    //        foreach (Resource potentialAvailableRes in eligibleResources)
                    //        {
                    //            if (potentialAvailableRes == tanksBlock.ScheduledResource)
                    //            {
                    //                continue;
                    //            }

                    //            if (!potentialAvailableRes.Reservations.Intersects(reservationConflict.m_ia.Id, reservationConflict.StartTicks, reservationConflict.EndTicks, out ResourceReservation _))
                    //            {
                    //                //We found an available resource to move this reservation to.
                    //                ResourceReservation newReservation = new(reservationConflict, potentialAvailableRes);
                    //                tanksBlock.ScheduledResource.Reservations.m_reservations.Remove(reservationConflict);
                    //                if (reservationConflict.PrimaryReservation)
                    //                {
                    //                    //Only add new scheduling events for the primary reservation
                    //                    newReservation.MakeFirm();
                    //                    AddEvent(new ResourceReservationEvent(newReservation.StartTicks, newReservation.ReservedResource, newReservation));
                    //                }
                    //                break;
                    //            }
                    //        }
                    //    }
                    //    //TODO: Do we want to handle or report ones that couldn't be moved?
                    //}
    //            }
    //        }

    //        //TODO: Set storage OCP here based on how we calculated the end date
    //        a_tankBatch.ScheduleEndOfStorage(a_tankEndTicks, tankEndOfPostProcessing);

    //        if (a_tankBatch.BlockCount > 0)
    //        {
    //            Resource primaryTankRRSatiator = a_tankBatch.GetRRSatiator(firstTankActivity.Operation.ResourceRequirements.PrimaryResourceRequirementIndex).Resource;
    //            PredOpReady_ProcessSucActivitiesAndSucMOs(firstTankActivity, primaryTankRRSatiator, PredecessorReadyType.BlockEndCalculated);

    //            if (a_tankBatch.MoveIntersectorDelayedByTank)
    //            {
    //                // Use a_tankBatch as the proc start date or start date of the next move activity.
    //                InternalActivity moveAct = GetNextSequencedMoveActivity();
    //                RequiredCapacityPlus moveActRC = CalcReqCapacityOfPrimaryMoveRes(a_simClock, a_tankBatch.EndTicks, moveAct, ExtensionController);

    //                for (int reqI = 0; reqI < moveAct.Operation.ResourceRequirements.Count; ++reqI)
    //                {
    //                    Resource res = moveAct.GetMoveResource(reqI);
    //                    if (res == primaryTankRRSatiator)
    //                    {
    //                        ResourceRequirement rr = moveAct.Operation.ResourceRequirements.GetByIndex(reqI);
    //                        if (rr.UsageStart == MainResourceDefs.usageEnum.Setup)
    //                        {
    //                            ReserveNextMoveActivity(SimClock, a_tankBatch.EndTicks);
    //                        }
    //                        // a_tankBatch.EndTicks is the run start date.
    //                        // Back calculate the move start, check for intersection with other things that may have scheduled.
    //                        // If there's no intersection, reserve the move time, otherwise cancel the simulation and continue.
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    a_rwh.TankEmptied();
    //}

    /// <summary>
    /// This function was added to help create the SplitCustomization. It handles the splitting and recalcuates the output variables depending on the results.
    /// Populates m_inventoryAllocations if material has been allocated.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_resourceAvailableEvent"></param>
    /// <param name="a_primaryResource"></param>
    /// <param name="a_act"></param>
    /// <param name="a_simClock"></param>
    /// <param name="a_rrm"></param>
    /// <param name="o_resourceConsumed"></param>
    /// <param name="r_batchToJoin"></param>
    /// <param name="o_si"></param>
    /// <param name="o_resReqSatiaters"></param>
    /// <param name="r_autoSplitInfo"></param>
    /// <param name="r_productProfiles"></param>
    /// <param name="o_materialAllocations"></param>
    /// <param name="a_dictionary"></param>
    /// <param name="a_ignoreRciProfile"></param>
    /// <returns></returns>
    private SchedulableSuccessFailureEnum AttemptToScheduleActivity_CustomizeMOHelper(ScenarioDetail a_sd,
                                                                                      ResourceAvailableEvent a_resourceAvailableEvent,
                                                                                      Resource a_primaryResource,
                                                                                      InternalActivity a_act,
                                                                                      long a_simClock,
                                                                                      ResourceRequirementManager a_rrm,
                                                                                      ref bool o_resourceConsumed,
                                                                                      ref BatchToJoinTempStruct r_batchToJoin,
                                                                                      out SchedulableInfo o_si,
                                                                                      out ResourceSatiator[] o_resReqSatiaters,
                                                                                      ref AutoSplitInfo r_autoSplitInfo,
                                                                                      ref Dictionary<BaseId, SupplyProfile> r_storageAllocations,
                                                                                      out List<MaterialDemandProfile> o_materialAllocations,
                                                                                      bool a_ignoreRciProfile,
                                                                                      bool a_isImport)
    {
        o_si = null;
        o_resReqSatiaters = null;
        o_materialAllocations = null;

        bool needInitializationPass = true;
        bool moQtyChanged = false; // This may occur when a custom split at the job or MO level occurs.
        bool autoSplitForCleanout = false;
        ResourceOperation operation = (ResourceOperation)a_act.Operation;
        long simStartTime = GetSimStartTime(a_primaryResource);
        RequiredCapacityPlus tempRcp = null;

        while (needInitializationPass || moQtyChanged)
        {
            needInitializationPass = false;
            moQtyChanged = false;
            bool failedDueToCleanout = false;
            long cleanoutRetryTime = 0;

            if (r_batchToJoin.NoJoin())
            {
                LeftNeighborSequenceValues leftNeighborSequenceValues = a_primaryResource.CreateDefaultLeftNeighborSequenceValues(a_simClock);

                RequiredCapacityPlus rc = a_primaryResource.CalculateTotalRequiredCapacity(a_simClock, a_act, leftNeighborSequenceValues, true, a_simClock, ExtensionController, tempRcp);
                SchedulableResult sr = a_primaryResource.PrimarySchedulable(this, a_simClock, a_act, false, a_rrm.PrimaryResourceRequirement, a_simClock, leftNeighborSequenceValues, rc, a_resourceAvailableEvent.Node, a_ignoreRciProfile);
                if (sr.m_result == SchedulableSuccessFailureEnum.IntersectsReservedMoveDate)
                {
                    if (operation.ResourceRequirements.HasMixOfUsageStarts())
                    {
                        // [USAGE_CODE] AttemptToScheduleActivity_CustomizeMOHelper: An activity that intersects the move time needs to be handled.
                        AddMoveIntersector(a_simClock, a_primaryResource.ReservedMoveStartTicks, a_act, a_rrm.PrimaryResourceRequirement, a_rrm.PrimaryResourceRequirementIndex, a_primaryResource);
                    }
                }
                else if (sr.m_result == SchedulableSuccessFailureEnum.FailedDueToCleanBeforeSpan)
                {
                    //We failed due to cleanout capacity, but we still want to verify if the activity COULD schedule now.
                    failedDueToCleanout = true;
                    cleanoutRetryTime = sr.RetryTime;
                }

                if (sr.m_result == SchedulableSuccessFailureEnum.Success || sr.m_result == SchedulableSuccessFailureEnum.FailedDueToCleanBeforeSpan)
                {
                    if (!autoSplitForCleanout && a_act.Operation.AutoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.CIP)
                    {
                        r_autoSplitInfo = AutoSplitOperationForCleanout(a_primaryResource, a_act, a_simClock, sr.m_si, a_act.Operation, m_productRuleManager, out RequiredSpanPlusClean cleanAfterSpan);
                        if (r_autoSplitInfo != null)
                        {
                            tempRcp = a_primaryResource.CalculateTotalRequiredCapacity(a_simClock, a_act, leftNeighborSequenceValues, true, a_simClock, ExtensionController);
                            tempRcp.CleanAfterSpan = cleanAfterSpan;
                            autoSplitForCleanout = true;
                            //Recalculate capacity and try again with the split activity.
                            moQtyChanged = true;
                            continue;
                        }
                    }

                    //Now that we have the SI, we can recalaculate accurate time based cleanouts
                    //This is for resource table time based cleanouts.
                    //We need to verify now that we have accurate capacity, that this newly scheduled block won't exceed the time based cleanout trigger
                    //TODO: Shouldn't the JIT date be the start of the production?
                    bool late = !a_act.JITStartDateNotSet && sr.m_si.m_scheduledStartDate > a_act.DbrJitStartTicks;
                    string capacityGroupCode = a_act.Operation.ResourceRequirements.PrimaryResourceRequirement.CapacityCode;
                    RequiredSpanPlusClean cleanBeforeSpan = a_primaryResource.CalculateTotalRequiredCleanout(Math.Max(cleanoutRetryTime, m_simClock), a_act, leftNeighborSequenceValues, true, ExtensionController, sr.m_si.m_postProcessingFinishDate);
                    SchedulableResult validateCleanResult = sr;
                    if (!failedDueToCleanout)
                    {
                        //Only re-check cleanout if we didn't already fail due to cleanout span
                        validateCleanResult = a_primaryResource.ValidateCleanBeforeSpan(this, a_simClock, a_act, false, a_rrm.PrimaryResourceRequirement, sr.m_si.m_scheduledStartDate, leftNeighborSequenceValues, cleanBeforeSpan, a_resourceAvailableEvent.Node, a_ignoreRciProfile, late, capacityGroupCode, sr.m_si.m_ocp);
                        failedDueToCleanout = validateCleanResult.m_result == SchedulableSuccessFailureEnum.FailedDueToCleanBeforeSpan;
                        cleanoutRetryTime = validateCleanResult.RetryTime;
                    }
                    else
                    {
                        //With full schedulable info we can calculate a new clean span that is shorter, but higher grade so we need to make sure we allow it to schedule earlier than 
                        //the original calculated delay.
                        if (cleanBeforeSpan.CleanoutGrade > rc.CleanBeforeSpan.CleanoutGrade && cleanBeforeSpan.TimeSpanTicks < rc.CleanBeforeSpan.TimeSpanTicks)
                        {
                            validateCleanResult = a_primaryResource.ValidateCleanBeforeSpan(this, a_simClock, a_act, false, a_rrm.PrimaryResourceRequirement, sr.m_si.m_scheduledStartDate, leftNeighborSequenceValues, cleanBeforeSpan, a_resourceAvailableEvent.Node, a_ignoreRciProfile, late, capacityGroupCode, sr.m_si.m_ocp);
                            failedDueToCleanout = validateCleanResult.m_result == SchedulableSuccessFailureEnum.FailedDueToCleanBeforeSpan;
                            cleanoutRetryTime = validateCleanResult.RetryTime;
                            rc.CleanBeforeSpan = cleanBeforeSpan;
                            sr = a_primaryResource.PrimarySchedulable(this, a_simClock, a_act, false, a_rrm.PrimaryResourceRequirement, a_simClock, leftNeighborSequenceValues, rc, a_resourceAvailableEvent.Node, a_ignoreRciProfile);
                        }
                    }

                    if (a_sd.ExtensionController.RunOverrideCleanExtension)
                    {
                        if (cleanBeforeSpan.TimeSpanTicks > 0
                            && a_primaryResource.IsThereGapOfOnlineTimeBetweenEndDateAndStartDateOfSuccessiveActivities(validateCleanResult.FindCapacityResult.FinishDate, sr.m_si.m_scheduledStartDate))
                        {
                            RequiredSpanPlusClean overrideCleanSpan = a_sd.ExtensionController.OverrideCleanSpan(a_sd, leftNeighborSequenceValues.Activity, a_act);
                            validateCleanResult = a_primaryResource.ValidateCleanBeforeSpan(this, a_simClock, a_act, false, a_rrm.PrimaryResourceRequirement, sr.m_si.m_scheduledStartDate, leftNeighborSequenceValues, overrideCleanSpan, a_resourceAvailableEvent.Node, a_ignoreRciProfile, late, capacityGroupCode, sr.m_si.m_ocp);
                            if (validateCleanResult.m_result == SchedulableSuccessFailureEnum.Success)
                            {
                                cleanBeforeSpan = overrideCleanSpan;
                            }
                        }
                    }

                    if (validateCleanResult.m_result == SchedulableSuccessFailureEnum.Success || validateCleanResult.m_result == SchedulableSuccessFailureEnum.FailedDueToCleanBeforeSpan)
                    {
                        //Add cost for Before clean span if any 
                        if (validateCleanResult.FindCapacityResult?.CapacityUsageProfile.Count > 0)
                        {
                            sr.m_si.m_requiredAdditionalCleanBeforeSpan.SetCapacityCost(validateCleanResult.FindCapacityResult.CapacityUsageProfile.Cost);
                        }

                        sr.m_si.m_requiredSetupSpan.SetCapacityCost(sr.CapacityProfileResult.SetupProfile.Cost);
                        sr.m_si.m_requiredProcessingSpan.SetCapacityCost(sr.CapacityProfileResult.ProductionProfile.Cost);
                        sr.m_si.m_requiredPostProcessingSpan.SetCapacityCost(sr.CapacityProfileResult.PostprocessingProfile.Cost);
                        sr.m_si.m_requiredStorageSpan.SetCapacityCost(sr.CapacityProfileResult.StorageProfile.Cost);
                        sr.m_si.m_requiredCleanAfterSpan.SetCapacityCost(sr.CapacityProfileResult.CleanProfile.Cost);

                        //Recreate the schedulable info with the udpated Clean before Span
                        sr.m_si = new (sr.m_si.m_zeroLength,
                            sr.m_si.m_scheduledStartDate,
                            sr.m_si.m_setupFinishDate,
                            sr.m_si.m_productionFinishDate,
                            sr.m_si.m_postProcessingFinishDate,
                            sr.m_si.m_storageFinishDate,
                            sr.m_si.m_cleanFinishDate,
                            sr.m_si.m_finishDate,
                            a_primaryResource,
                            cleanBeforeSpan,
                            sr.m_si.m_requiredSetupSpan,
                            sr.m_si.m_requiredProcessingSpan,
                            sr.m_si.m_requiredPostProcessingSpan,
                            sr.m_si.m_requiredStorageSpan,
                            sr.m_si.m_requiredCleanAfterSpan,
                            sr.m_si.m_requiredNumberOfCycles,
                            sr.m_si.m_requiredQty,
                            sr.m_si.m_ocp);
                    }
                    else
                    {
                        sr = validateCleanResult;
                    }
                }

                if (failedDueToCleanout)
                {
                    //Retry again at the calculated retry time.
                    if (cleanoutRetryTime > a_simClock)
                    {
                        AddEvent(new RetryForCleanoutEvent(cleanoutRetryTime));
                    }
                    #if DEBUG
                    else if (cleanoutRetryTime > 0)
                    {
                        //throw new DebugException("Capacity Calculation returned SimClock");
                    }
                    #endif
                }
                else if (sr.m_result != SchedulableSuccessFailureEnum.Success)
                {
                    //Time for AUTOSPLIT
                    if (!sr.CapacityProfileResult.Empty
                        && a_act.Operation.AutoSplitInfo.CanAutoSplit(m_activeSimulationType, a_act.Operation, a_primaryResource.ManualSchedulingOnly)
                        && a_act.Operation.AutoSplitInfo.AutoSplitType == OperationDefs.EAutoSplitType.PrimaryCapacityAvailability)
                    {
                        r_autoSplitInfo = AutoSplitByAvailableCapacity(a_act, a_primaryResource, sr.CapacityProfileResult, rc, m_productRuleManager);
                        if (r_autoSplitInfo != null)
                        {
                            //Recalculate capacity and try again with the split activity.
                            moQtyChanged = true;
                            continue;
                        }
                    }

                    if (sr.m_result == SchedulableSuccessFailureEnum.LackCapacityWithRetry)
                    {
                        //Retry again at the calculated retry time.
                        if (sr.RetryTime > a_simClock)
                        {
                            AddEvent(new RetryForCleanoutEvent(sr.RetryTime));
                        }
                        #if DEBUG
                        else
                        {
                            //throw new DebugException("Capacity Calculation returned SimClock");
                        }
                        #endif
                    }

                    o_resourceConsumed = false;
#if TEST
                    ++_deSchedulableResult;
#endif
                    return sr.m_result;
                }

                o_si = sr.m_si;
            }
            else
            {
                // [BATCH_CODE]
                o_si = r_batchToJoin.GetSchedulableInfo();
            }

            TransferInfoCollection transferInfoCollection = a_act.Operation.TransferInfo;

            //Check for transfer span end usage. The transfer end date must be at or before the EndPoint based on si
            if (a_simClock < m_endOfPlanningHorizon && transferInfoCollection.Set)
            {
                //Get the transfer with the 
                List<(TransferInfo, RequiredCapacity, bool)> transferCapacities = transferInfoCollection.CalculateTransferCapacity(o_si, a_act);
                List<TransferSpanEvent> transferEventsList = new();
                foreach ((TransferInfo transferInfo, RequiredCapacity transferRc, bool constraint) in transferCapacities)
                {
                    if (constraint) //transfer time constrains this start date
                    {
                        long adjustedTransferEndingOnInterval = transferInfo.GetTransferEndForActivity(a_act.Id);
                        //We only need to validate if the transfer time will be within the planning horizon and is within the resource's capacity range
                        if (adjustedTransferEndingOnInterval < EndOfPlanningHorizon && adjustedTransferEndingOnInterval < a_primaryResource.ResourceCapacityIntervals[^1].EndDate)
                        {
                            //TODO: Performance can be improved here, these lookups are slow and enumerate all intervals
                            if (a_primaryResource.ResourceCapacityIntervals.FindIdx(adjustedTransferEndingOnInterval) == -1)
                            {
                                //The transfer time ends in an offline period. We need to find the next online interval after the date.
                                adjustedTransferEndingOnInterval = a_primaryResource.ResourceCapacityIntervals.FindFirstOnlineAtOrBefore(new DateTime(adjustedTransferEndingOnInterval)).EndDate;
                            }

                            Resource.FindStartFromEndResult backCalculatedSU = a_primaryResource.FindCapacityReverse(Clock, adjustedTransferEndingOnInterval, transferRc, null, a_act);

                            if (backCalculatedSU.Success && backCalculatedSU.StartTicks > a_simClock)
                            {
                                transferEventsList.Add(new TransferSpanEvent(backCalculatedSU.StartTicks, a_act.Operation.Predecessors[0], transferInfo));
                            }
                        }
                        else
                        {
                            transferEventsList.Add(new TransferSpanEvent(adjustedTransferEndingOnInterval, a_act.Operation.Predecessors[0], transferInfo));
                        }
                    }
                }

                //If there are any transfers that constrain, find the latest and retry again then.
                if (transferEventsList.Count > 0)
                {
                    //TODO: Consider creating a new event instead of reusing TransferSpanEvent
                    TransferSpanEvent latestConstrainingEvent = transferEventsList.OrderByDescending(e => e.Time).First();
                    AddEvent(latestConstrainingEvent);

                    //Update the collection to track latest constraint data
                    //TODO: Store this on the operation as a constraint so the data doesn't disappear once another action occurs.
                    transferInfoCollection.TrackLatestConstraint(latestConstrainingEvent.TransferInfo);

                    return SchedulableSuccessFailureEnum.LackCapacity;
                }
            }

            if (!(o_resourceConsumed = SchedulabilityCustomization_TestIsSchedulableCustomizationForPrimaryResource(a_primaryResource, a_act, o_si, a_simClock)))
            {
                return SchedulableSuccessFailureEnum.Customization;
            }

            
            // See if the material requirements can be satisfied.
            Plant plant = PlantManager.GetById(a_primaryResource.PlantId);

            //Reset any allocations from previous attempts
            for (int i = 0; i < plant.WarehouseCount; i++)
            {
                Warehouse warehouse = plant.GetWarehouseAtIndex(i);
                warehouse.ResetAllocation();
            }

            // Each material request should contain an entry in the materialAllocations list.
            Plant.FindMaterialResult matlAvailResult = plant.FindAvailableMaterial(a_act, a_primaryResource, o_si, a_simClock, Clock, this, a_isImport);

            #if DEBUG
            /// Here's the problem with the 10618 recording.
            /// Virtually all the calls to FindAvailableMaterial fail,
            /// resulting in a vast number of activities failing to schedule.
            /// Why are materials failing so much. Is it a customization or are the materials simply not available?
            #else
            #endif
            if (!matlAvailResult.MaterialSourcesFound)
            {
                o_resourceConsumed = false;

                #if TEST
                    ++_deFindMaterial;
                #endif

                //Check to see if any connectors could be the cause.
                bool possibleConnectorConstraint = false;
                foreach (ResourceConnector connector in ResourceConnectorManager.GetConnectorsToResource(a_primaryResource.Id))
                {
                    if (connector.FlagConnectorUnavailable)
                    {
                        possibleConnectorConstraint = true;
                        break;
                    }
                }

                //Remove the activity from the dispather until additional material is available.
                //Skip this if the failure may be due to a connector constraint
                if (!possibleConnectorConstraint)
                {
                    //Remove this activity from the dispatchers. Until the limiting material is available, there is no need to try to schedule this activity again.
                    //When the limiting item/inventory is updated, the activity will be added back to the dispatchers
                    IEnumerable<InternalResource> previousDispatchers = RemoveActivityFromAllResourceDispatchers(a_act);
                    matlAvailResult.SetPreviousDispatchers(previousDispatchers);

                    if (matlAvailResult.RetryDate > a_simClock)
                    {
                        //Potential constraint other than material, retry at this time
                        AddEvent(new FutureMaterialAvailableEvent(matlAvailResult.RetryDate, a_act, matlAvailResult));
                    }
                    else
                    {
                        FutureMaterialAvailableEvent retryEvent = new (SimClock, a_act, matlAvailResult);
                        matlAvailResult.FirstFailedSourceItem.AddRetryMatlEvt(retryEvent);
                    }
                }

                //Track First Shortage Date on Inventory
                if (matlAvailResult.RequiredWarehouse != null)
                {
                    Inventory inv = matlAvailResult.RequiredWarehouse.Inventories[matlAvailResult.FirstFailedSourceItem.Id];
                    inv?.TrackFirstShortageDate(a_simClock);
                }
                else
                {
                    for (int i = 0; i < WarehouseManager.Count; i++)
                    {
                        Warehouse w = WarehouseManager[i];
                        Inventory inv = w.Inventories[matlAvailResult.FirstFailedSourceItem.Id];
                        //check null in case inventory is not in this warehouse
                        inv?.TrackFirstShortageDate(a_simClock);
                    }
                }



                return SchedulableSuccessFailureEnum.MaterialUnavailable;
            }

            //Material was allocated succesfully
            o_materialAllocations = matlAvailResult.MaterialDemandProfiles;

            // These are the resources we are considering as satisfiers of the resource requirements.
            // Each index corresponds to an element in the operation's resource requirements.
            o_resReqSatiaters = new ResourceSatiator[a_rrm.Count];
            bool requirementsSatisfied;

            // [BATCH_CODE]
            if (r_batchToJoin.NoJoin())
            {
                o_resReqSatiaters[a_rrm.PrimaryResourceRequirementIndex] = new ResourceSatiator(a_primaryResource);
                SetReservation(a_primaryResource);

                // The value of requirementsSatisfied can change


                requirementsSatisfied = VerifyCompatibilityGroupConstraintsAndDetermineNonPrimaryRRs(a_simClock, a_act, o_si, a_primaryResource, o_resReqSatiaters);
            }
            else
            {
                o_resReqSatiaters = r_batchToJoin.GetRRSatiatorsCopy();
                requirementsSatisfied = true;
            }

            CycleAdjustmentProfile ccp = null;

            if (requirementsSatisfied)
            {
                if (operation.EligibleForTransferQtyOverlap())
                {
                    try
                    {
                        a_primaryResource.CalculateCycleCompletionTimes(a_act, o_si, out ccp);
                        operation.CalculateContainerCompletions(a_primaryResource, o_si.m_requiredQty, ccp);
                    }
                    catch (Exception e)
                    {
                        Overlap.Throw_OverlapDebugError(e);
                    }
                }
                else if (operation.EligibleForAtFirstTransferOverlap())
                {
                    // ***LRH*** change this to just calculat the first release.
                    try
                    {
                        a_primaryResource.CalculateCycleCompletionTimes(a_act, o_si, out ccp);
                        operation.CalculateContainerCompletions(a_primaryResource, o_si.m_requiredQty, ccp);
                    }
                    catch (Exception e)
                    {
                        Overlap.Throw_OverlapDebugError(e);
                    }
                }

                if (ContinuouslyInsertSuccessor(operation)) // The code to calculate overlap is above because it could be used by this function call.
                {
                    // ***LRH***flow*** the the following into account multiple successors, multiple predecessors, multiple activities.
                    // In this iniital test case I am assuming the first operation is scheduled by simulation and other operations are scheduled
                    // based on forwards insertion. In effect there is really only one operation. This is something like scheduling the flow
                    // of material. This will make it easy to break the operation up into activities and schedule them separately. 
                    AlternatePath.Association asn = operation.AlternatePathNode.Successors[0];

                    requirementsSatisfied = InsertContinuousSuccessor(asn, o_si);
                    if (!requirementsSatisfied)
                    {
                        //Failed to schedule the primary, clear all future reservations attempted
                        m_resRvns.ClearPlanned();
                    }
                }
            }

            if (requirementsSatisfied == false)
            {
                // [USAGE_CODE] The requirements weren't satisfied. Clear the ResourceSatiator[].
                //ClearReservations(o_resReqSatiaters, true);
                ClearResourceReservationsOnly(o_resReqSatiaters); //As a part of PR 1114 on 7.13.2022, block reservations are maintained. Not 100% if that is necessary.
                ClearUnscheduledHelperReservations(o_resReqSatiaters);
                o_resourceConsumed = false;
                #if TEST
                    ++_deNonPrimaryRR;
                #endif
                return SchedulableSuccessFailureEnum.SuccessResNotRequired;
            }

            //TODO: Add new is schedulabled endpoint here, pass in 
            //ro.TransferQtyProfile;
            long? delayScheduling = m_extensionController.IsSchedulablePostMaterialAllocation(this, m_activeSimulationType, Clock, a_simClock, simStartTime, a_primaryResource, a_act, o_si, operation.TransferQtyProfile);
            if (delayScheduling.HasValue)
            {
                if (delayScheduling.Value > a_simClock)
                {
                    SchedulabilityCustomization_AddSchedulabilityCustomizationEvent(delayScheduling.Value);
                }
                else if (delayScheduling.Value == a_simClock)
                {
                    //This will likely cause an infinite loop in the simulation and should be reported as a debug error.
                    DebugException.ThrowInDebug("Customization has set PostEventTime to SimulationClock");
                }

                #if TEST
                    ++_deSchedulabilityCustomization;
                #endif
                // The customization has indicated that the activity can't be scheduled at this time.
                return SchedulableSuccessFailureEnum.Customization;
            }

            if (m_extensionController.RunSplitExtension)
            {
                SplitResult splitCustomizationResult = m_extensionController.Split(m_activeSimulationType, Clock, a_simClock, a_primaryResource, a_act, o_si, ccp, this);

                if (splitCustomizationResult.SplitResultEnum == SplitResult.ESplitResultEnum.SplitMO)
                {
                    ManufacturingOrder splitOffMO = operation.ManufacturingOrder.Job.SplitOffFractionIntoAnotherMOWithinTheSameJobForExtension(a_simClock, operation.ManufacturingOrder, splitCustomizationResult.SplitOffRatio, m_productRuleManager);

                    InitializeMOSplitOffDuringSimulationByCustomization(a_sd, a_simClock, splitCustomizationResult, splitOffMO, ref moQtyChanged, o_resReqSatiaters);
                }
                else if (splitCustomizationResult.SplitResultEnum == SplitResult.ESplitResultEnum.SplitJob)
                {
                    if (operation.Job.ManufacturingOrderCount != 1)
                    {
                        throw new Exception("2565");
                    }

                    Job splitoffJob = JobManager.Split(a_simClock, operation.Job, splitCustomizationResult.SplitOffRatio, m_productRuleManager);

                    // **************************************************************************************************
                    //
                    // Also perform any Job initialization. You might llok at simulationInitialization or the Job's flags to determinw what needs to be done to get it into the simulation. Perhaps a ManufacturingOrderReleaseEvent.
                    // The current Scenario cuts up and schedules the first job correctly except the split off jobs are all maked as FailedToSchedule. The second Job scheduled similarly to the first.
                    // At first, the second job wouldn't split at all. It turns out when you copy a job, the user field isn't copied too. Enter this as a new bug. Possible medium because of the potential issues it could cause for Templates.
                    //
                    // **************************************************************************************************
                    InitializeMOSplitOffDuringSimulationByCustomization(a_sd, a_simClock, splitCustomizationResult, splitoffJob.ManufacturingOrders[0], ref moQtyChanged, o_resReqSatiaters);
                }
                else if (splitCustomizationResult.SplitResultEnum == SplitResult.ESplitResultEnum.NoSplitAndDontSchedule)
                {
                    //This was removed as part of PR 1114 on 7.13.2022. It's possible that we should still reset ReservedForResourceRequirement. Not 100% sure if we need to maintain block reservations
                    //ClearReservations(o_resReqSatiaters, true);
                    return SchedulableSuccessFailureEnum.Customization;
                }
            }

            r_storageAllocations.Clear();
            ///***************************************************************************************************************
            /// Verify Product Storage
            ///***************************************************************************************************************
            ProductsCollection operationProducts = a_act.Operation.Products;
            for (int productI = 0; productI < operationProducts.Count; ++productI) //We need productI for act qty indexer
            {
                Product product = operationProducts[productI];
                ProductSupplyProfile productQtyProfile = null;
                Item profilesItem = product.Item;

                long productPostProcessingTime = product.MaterialPostProcessingTicks;
                if (product.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.ByProductionCycle)
                {
                    try
                    {
                        if (ccp == null)
                        {
                            //Calculate cycles since it wasn't done during transfer code above
                            a_primaryResource.CalculateCycleCompletionTimes(a_act, o_si, out ccp);
                        }
                        productQtyProfile = operation.CalculateContainerCompletionIntoQtyProfile(this, a_primaryResource, product, productI, a_act, ccp, o_si);
                        //Note: the ccp should already take into account reported qty for the initial time point
                    }
                    catch (Exception e)
                    {
                        Overlap.Throw_OverlapDebugError(e);
                    }
                }
                else if (product.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.DuringPostProcessing)
                {
                    try
                    {
                        productQtyProfile = operation.CalculateMaterialProductionRangeIntoQtyProfile(a_primaryResource, product, a_act, o_si.m_ocp.PostprocessingProfile, o_si.m_postProcessingFinishDate, o_si);
                        if (a_act.ProductionStatus == InternalActivityDefs.productionStatuses.PostProcessing)
                        {
                            //There may be some reported qty to take into account
                            long postProcessingStart = a_act.ReportedEndOfProcessingSet ? a_act.ReportedEndOfRunTicks : a_simClock;
                            long materialPostProcessingSpanTicks = a_act.GetResourceProductionInfo(a_primaryResource).MaterialPostProcessingSpanTicks;
                            productQtyProfile.AddToFront(new QtySupplyNode(a_simClock, postProcessingStart + materialPostProcessingSpanTicks, a_act, product, product.CompletedQty));
                        }
                    }
                    catch (Exception e)
                    {
                        Overlap.Throw_OverlapDebugError(e);
                    }
                }
                else if (product.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.DuringStorage)
                {
                    try
                    {
                        productQtyProfile = operation.CalculateMaterialProductionRangeIntoQtyProfile(a_primaryResource, product, a_act, o_si.m_ocp.StorageProfile, o_si.m_storageFinishDate, o_si);
                        if (a_act.ProductionStatus == InternalActivityDefs.productionStatuses.Storing)
                        {
                            //There may be some reported qty to take into account
                            long storageStart = a_act.ReportedEndOfPostProcessingSet ? a_act.ReportedEndOfPostProcessingTicks : a_simClock;
                            long materialPostProcessingSpanTicks = a_act.GetResourceProductionInfo(a_primaryResource).MaterialPostProcessingSpanTicks;
                            productQtyProfile.AddToFront(new QtySupplyNode(a_simClock, storageStart + materialPostProcessingSpanTicks, a_act, product, product.CompletedQty));
                        }
                    }
                    catch (Exception e)
                    {
                        Overlap.Throw_OverlapDebugError(e);
                    }
                }
                else
                {
                    long productReleaseToStockTicks;
                    if (product.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.AtOperationResourcePostProcessingEnd)
                    {
                        productReleaseToStockTicks = o_si.m_postProcessingFinishDate;
                    }
                    else if (product.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.AtOperationRunStart)
                    {
                        productReleaseToStockTicks = o_si.m_setupFinishDate;
                    }
                    else if (product.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.AtOperationRunEnd)
                    {
                        productReleaseToStockTicks = o_si.m_productionFinishDate;
                    }
                    else if (product.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.AtStorageEnd)
                    {
                        productReleaseToStockTicks = o_si.m_storageFinishDate;
                    }
                    else
                    {
                        throw new Exception("Unknown product release timing.");
                    }

                    if (productReleaseToStockTicks < a_simClock)
                    {
                        continue;
                    }

                    if (operationProducts.PrimaryProduct == product)
                    {
                        ProductionInfo resourceProductionInfo = a_act.GetResourceProductionInfo(a_primaryResource);
                        productPostProcessingTime = resourceProductionInfo.MaterialPostProcessingSpanTicks;
                    }

                    productQtyProfile = new ProductSupplyProfile(product, a_act, product.Inventory, o_si);
                    productQtyProfile.AddToEnd(new QtySupplyNode(productReleaseToStockTicks, productReleaseToStockTicks + productPostProcessingTime, a_act, product, a_act.m_productQtys[productI]));
                }

                if (productQtyProfile.HasNodes) //The activity could be in a stage after the material would be produced.
                {
                    Warehouse.StorageAllocationResult storageResult = product.Warehouse.AllocateStorage(productQtyProfile, a_primaryResource, this, a_act, o_si, ccp, ref moQtyChanged);
                    if (!storageResult.Success)
                    {
                        if (WithinPlanningHorizon)
                        {
                            IEnumerable<InternalResource> previousDispatchers = [];
                            if (storageResult.Retry)
                            {
                                //This failed storage due to a resource connector and not a capacity issue
                                //In this case, don't remove from dispatchers and retry again at the earliest connector date
                                //TODO: Potentially improve performance by removing form dispatcher and then re-adding at the retry for connector event receive
                                a_act.Operation.SetLatestConstraintToStorageConnector(storageResult.RetryDate, product.Item.Name);

                                //We need to backcalculate startdate so the initial non production stages can schedule earlier.
                                RequiredCapacity preStorageCapacity = productQtyProfile.GetBackCalculateCapacity(o_si);
                                Resource.FindStartFromEndResult newStart = a_primaryResource.FindCapacityReverse(Clock, storageResult.RetryDate, preStorageCapacity, null, a_act);
                                if (newStart.Success && newStart.StartTicks > SimClock)
                                {
                                    AddEvent(new RetryForConnectorEvent(newStart.StartTicks));
                                }
                            }
                            else if (storageResult.RetryAtNextOnline)
                            {
                                //We don't know the next retry time, so just let the system retry at the next online time
                            }
                            else
                            {
                                //Remove from dispatcher until a SA that can store that item has its qty lowered. Account for drainage
                                //We can only remove from this resource's dispatcher, as other resources may have other SAs that can store the material
                                if (RemoveFromResourceDispatcher(a_act, a_primaryResource))
                                {
                                    previousDispatchers = [a_primaryResource];
                                }
                            }

                            if (storageResult.RetryStorageAreas.Count > 0)
                            {
                                foreach (ItemStorage itemStorage in storageResult.RetryStorageAreas)
                                {
                                    RetryForEmptyStorageAreaEvent retryEvent = new (SimClock, a_act, previousDispatchers);
                                    itemStorage.StorageArea.AddRetryStorageEmptyEvent(retryEvent);

                                    //Remove from the full collection to avoid duplicate events;
                                    storageResult.UsableItemStorage.Remove(itemStorage);
                                }
                            }

                            foreach (ItemStorage itemStorage in storageResult.UsableItemStorage)
                            {
                                FutureStorageAvailableEvent retryStorageEvent = new (SimClock, a_act, previousDispatchers);
                                itemStorage.AddRetryStorageEvent(retryStorageEvent);
                            }
                            
                            a_act.Operation.SetLatestConstraintToStorage(a_simClock, product.Item.Name);

                            if (m_extensionController.RunFlagDelayExtension)
                            {
                                m_extensionController.FlagSchedulingDelayDueToStorageUnavailable(this, a_act.Operation);
                            }


                            return SchedulableSuccessFailureEnum.StorageUnavailable;
                        }

                        //We are past the planning horizon so we will let the operation schedule and allocate all to disposal
                        productQtyProfile.AllocateAllToDiscard();
                    }

                    if (moQtyChanged)
                    {
                        break;
                    }

                    //Store for later when the act schedules and we need the profile to create a lot
                    r_storageAllocations.Add(product.Id, productQtyProfile);
                }
                
                //Validation succeeded, we can continue to scheduling
            }

            if (failedDueToCleanout)
            {
                //TODO: There may be more side effects to clean up
                //TODO: Are there any events we don't need to add?
                ClearResourceReservationsOnly(o_resReqSatiaters);
                
                return SchedulableSuccessFailureEnum.FailedOnlyDueToCleanBeforeSpan;
            }
        }

        return SchedulableSuccessFailureEnum.Success;
    }

    internal long GetSimStartTime(InternalResource a_res)
    {
        switch (ActiveSimulationType)
        {
            case SimulationType.None:
            case SimulationType.Move:
            case SimulationType.MoveAndExpedite:
            case SimulationType.ClockAdvance:
            case SimulationType.UnscheduleJobs:
            case SimulationType.ScheduleJobs:
            case SimulationType.TimeAdjustment:
            case SimulationType.ConstraintsChangeAdjustment:
            case SimulationType.Undo:
            case SimulationType.Redo:
                return Clock;
            case SimulationType.Optimize:
            case SimulationType.Compress:
            case SimulationType.JitCompress:
                return m_simStartTime.GetTimeForResource(a_res);
            case SimulationType.Expedite:
                //TODO: expedite should also use the m_simStartTime variable
                return m_expediteStartTime.GetTimeForResource(a_res);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void InitializeMOSplitOffDuringSimulationByCustomization(ScenarioDetail a_sd, long a_simClock, SplitResult a_splitCustomizationResult, ManufacturingOrder a_splitOffMO, ref bool r_moQtyChanged, ResourceSatiator[] a_resReqSatiaters)
    {
        if (a_splitCustomizationResult.UseSplitOffMOQty)
        {
            a_splitOffMO.Job.AdjustRequiredQty(a_simClock, a_splitOffMO, a_splitCustomizationResult.SplitOffMOQty, m_productRuleManager);
        }

        r_moQtyChanged = true;

        // [USAGE_CODE] InitializeMOSplitOffDuringSimulationByCustomization(): ClearReservations of the ResourceSatiator[].
        ClearReservations(a_resReqSatiaters, false);
        //TODO: ClearFutureReservations Should we call this for all activities in the mo?

        //TODO: the paramter above is unclear, should it be true?

        a_splitOffMO.SimulationInitialization(m_plantManager, m_productRuleManager, m_extensionController, this);
        a_splitOffMO.ResetSimulationStateVariables(a_sd);
        a_splitOffMO.ResetSimulationStateVariables2();
        AddMOReleaseEventArgsForOpt fxArgs = new (m_activeOptimizeSettings);
        SimulationTimePoint sst = new (this, a_simClock, m_activeOptimizeSettings.StartTime);
        AddManufacturingOrderReleaseEvent(a_simClock, a_splitOffMO, sst, fxArgs);
        AddManufacturingOrderHoldReleasedEvent(a_splitOffMO, a_simClock);

        long nbrOfActivitiesToSchedule = a_splitOffMO.GetNbrOfActivitiesToSchedule();
        m_simulationProgress.AdjustNbrOfActivitiesToSchedule(nbrOfActivitiesToSchedule);
    }

    private bool PathIsNoLongerEligibleBecauseAnotherPathIsAlreadyBeingScheduled(ManufacturingOrder mo, AlternatePath path)
    {
        return !mo.CurrentPathNeedsToBeSetWhenFirstActivityIsScheduled
               && path != mo.CurrentPath;
    }

    /// <summary>
    /// Remove activities from resource dispatchers. Return a collection of the resources whose dispatcher contained the activity.
    /// </summary>
    /// <param name="a_act"></param>
    /// <returns>A collection that can be used to restore the activity to the dispatchers it was original contained in</returns>
    private static IEnumerable<InternalResource> RemoveActivityFromAllResourceDispatchers(InternalActivity a_act)
    {
        if (a_act.Operation is ResourceOperation)
        {
            Resource primaryResourceRequirementLock = a_act.PrimaryResourceRequirementLock();
            if (primaryResourceRequirementLock != null)
            {
                if (primaryResourceRequirementLock.ActiveDispatcher.Remove(a_act))
                {
                    return [primaryResourceRequirementLock];
                }

                if (!a_act.Scheduled) //It probably scheduled on this resource
                {
                    DebugException.ThrowInDebug("Activity was locked to resource but was not in its dispatcher.");
                }
            }

            List<InternalResource> previousDispatchers = new ();
            PlantResourceEligibilitySet pres = a_act.ResReqsEligibilityNarrowedDuringSimulation[0];

            using SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = pres.GetEnumerator();
            while (ersEtr.MoveNext())
            {
                EligibleResourceSet eligibleResourceSet = ersEtr.Current.Value;

                for (int i = 0; i < eligibleResourceSet.Count; ++i)
                {
                    InternalResource eligibleResource = eligibleResourceSet[i];
                    if (RemoveFromResourceDispatcher(a_act, eligibleResource))
                    {
                        previousDispatchers.Add(eligibleResource);
                    }
                }
            }

            return previousDispatchers;
        }

        return null;
    }

    private static bool RemoveFromResourceDispatcher(InternalActivity a_act, InternalResource a_resource)
    {
        //TODO: Is there a better way to track this by resource for performance?
        // Flag activity so we know it has been removed from dispatcher. Check when re-adding that the bool is true. If not, ignore
        //a_act.RemovedFromDispatcherPendingRetry.Add(a_resource.Id);
        
        return a_resource.ActiveDispatcher.Remove(a_act);
    }

    private void AddResourceAvailableEventAtNextOnlineTime(Resource a_resourceSatiater, long a_searchStartPoint, LinkedListNode<ResourceCapacityInterval> a_lastUsedCapacityIntervalNode)
    {
        LinkedListNode<ResourceCapacityInterval> nextAvailableInterval;
        a_resourceSatiater.FindNextOnlineTime(a_searchStartPoint, a_lastUsedCapacityIntervalNode, out long nextAvailableStartTime, out nextAvailableInterval);
        AddResourceAvailableEvent(a_resourceSatiater, nextAvailableStartTime, nextAvailableInterval);
    }

    private void AddResourceAvailableEvent(Resource a_resource, long a_nextAvailableStartTime, LinkedListNode<ResourceCapacityInterval> a_nextAvailableInterval)
    {
        #if DEBUG
        if (a_nextAvailableStartTime < SimClock || a_nextAvailableInterval.Value.EndDate < SimClock || a_nextAvailableInterval.Value.EndDate < a_nextAvailableStartTime)
        {
            throw new DebugException("The resource offline event is not valid");
        }
        #endif

        CreateAndSetLastScheduledResourceAvailableEvent(a_resource, a_nextAvailableStartTime, a_nextAvailableInterval);
        AddEvent(a_resource.LastScheduledResourceAvailableEvent);
    }

    /// <summary>
    /// Create ResourceAvailableEvent and set Resource.LastScheduledResourceAvailableEvent
    /// </summary>
    /// <param name="a_resource"></param>
    /// <param name="a_nextAvailableStartTime"></param>
    /// <param name="a_nextAvailableInterval"></param>
    private void CreateAndSetLastScheduledResourceAvailableEvent(Resource a_resource, long a_nextAvailableStartTime, LinkedListNode<ResourceCapacityInterval> a_nextAvailableInterval)
    {
        a_resource.LastScheduledResourceAvailableEvent = new ResourceAvailableEvent(a_nextAvailableStartTime, a_resource, a_nextAvailableInterval);
    }

    private void CancelLastScheduledResourceAvailableEvent(InternalResource a_resource)
    {
        if (a_resource.LastScheduledResourceAvailableEvent != null)
        {
            a_resource.LastScheduledResourceAvailableEvent.Cancelled = true;
            a_resource.LastScheduledResourceAvailableEvent = null;
        }
    }

    /// <summary>
    /// Create a batch that's similar to an existing batch; such as when splitting a block.
    /// </summary>
    /// <param name="a_primaryResource"></param>
    /// <param name="a_act"></param>
    /// <param name="si"></param>
    /// <param name="a_sourceBatch"></param>
    /// <returns></returns>
    internal Batch Batch_CreateBatch(Resource a_primaryResource, InternalActivity a_act, SchedulableInfo si, Batch a_sourceBatch)
    {
        Batch newBatch = m_batchManager.CreateBatch(si, a_sourceBatch, a_primaryResource, a_act.ProductionStatus);
        newBatch.Add(a_act, si.m_requiredQty);
        return newBatch;
    }

    internal Batch Batch_CreateBatch(Resource a_primaryResource, InternalActivity a_act, SchedulableInfo si, ResourceSatiator[] resReqSatiaters)
    {
        Batch newBatch = m_batchManager.CreateBatch(si, resReqSatiaters, a_primaryResource, a_act.ProductionStatus);
        newBatch.Add(a_act, si.m_requiredQty);
        return newBatch;
    }

    private bool SchedulabilityCustomization_TestIsSchedulableCustomizationForPrimaryResource(Resource a_primaryResource, InternalActivity a_activity, SchedulableInfo a_schedulableInfo, long a_simClock)
    {
        if (SchedulabilityCustomization_RunCustomization(a_activity))
        {
            long simStartTime = GetSimStartTime(a_primaryResource);

            long? delayScheduling;
            if (a_schedulableInfo != null)
            {
                delayScheduling = m_extensionController.IsSchedulable(this, m_activeSimulationType, Clock, a_simClock, simStartTime, a_primaryResource, a_activity, a_schedulableInfo);
            }
            else
            {
                delayScheduling = m_extensionController.IsSchedulable(this, m_activeSimulationType, Clock, a_simClock, simStartTime, a_primaryResource, a_activity);
            }

            if (delayScheduling.HasValue)
            {
                if (delayScheduling.Value > a_simClock)
                {
                    SchedulabilityCustomization_AddSchedulabilityCustomizationEvent(delayScheduling.Value);
                }
                else if (delayScheduling.Value == a_simClock)
                {
                    //This will likely cause an infinite loop in the simulation and should be reported as a debug error.
                    DebugException.ThrowInDebug("Customization has set PostEventTime to SimulationClock");
                }

                #if TEST
                    ++_deSchedulabilityCustomization;
                #endif
                // The customization has indicated that the activity can't be scheduled at this time.
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// This event is added when one of the IsSchedulable functions of the Schedulability customization returns a time to try again.
    /// </summary>
    /// <param name="postEventTime">The next time when scheduling should be attempted.</param>
    private void SchedulabilityCustomization_AddSchedulabilityCustomizationEvent(long a_postEventTime)
    {
        SchedulabilityCustomizationEvent schedulabilityCustomizationEvent = new (a_postEventTime);
        AddEvent(schedulabilityCustomizationEvent);
    }

    private bool SchedulabilityCustomization_RunCustomization(InternalActivity a_activity)
    {
        if (m_extensionController.RunSchedulingExtension)
        {
            // An optimization can be restricted to all operations after a specified start date, to a specific plant,
            // to a start date within a specific plant, etc. Here I check to see whether the activity can be moved from
            // whereever it was scheduled checking whether an optimization is in process and whether the activity is seqeunced.
            // In the case of an optimization Sequenced means that the activiy's start date and predecessor should be changable.
            // Activitivities whose state should be changed during an optimization should be subject to customization rules.
            // ********************************************************************************
            // You should check whether this assumption is still valid and whether removing
            // the nonOptimizedActivity conditional check will affect Maximus coffee. 
            // It should probably be removed because with continuous flow it might be possible for operations
            // scheduled beyond the simulation start date to affect operations scheduled prior to this.
            // though actually this shouldn't be the case if you make sure your operations are scheduled continuously
            // from the fixed zone. 
            // ********************************************************************************
            bool nonOptimizedActivity = m_activeSimulationType == SimulationType.Optimize && a_activity.Sequenced;

            if (!nonOptimizedActivity)
            {
                return true;
            }
        }

        return false;
    }

    private bool VerifyCompatibilityGroupConstraintsAndDetermineNonPrimaryRRs(long a_simClock, InternalActivity a_act, SchedulableInfo a_si, Resource a_primaryResource, ResourceSatiator[] a_resourceRequirementSatiaters)
    {
        ResourceRequirementManager rrm = a_act.Operation.ResourceRequirements;
        List<long> schedulabilityPostEventTimes = null;

        // Try to schedule all the other resource requirements.
        bool success = true;
        Resource.NonPrimarySchedulableResult schedResult = new (SchedulableSuccessFailureEnum.NotSet);
        List<Resource.NonPrimarySchedulableResult> attnConflicts = new ();

        for (int resourceRequirementI = 0; resourceRequirementI < rrm.Count; resourceRequirementI++)
        {
            ResourceRequirement rr = rrm.GetByIndex(resourceRequirementI);
            if (a_si.ZeroCapacityRequired(rr))
            {
                continue;
            }

            if (resourceRequirementI == rrm.PrimaryResourceRequirementIndex)
            {
                //long finishDate = a_si.GetUsageEnd(rr);
                if (!VerifyCompatibilityGroupConstraint(a_simClock, a_act, a_resourceRequirementSatiaters, resourceRequirementI))
                {
                    success = false;
                }
            }
            else
            {
                Resource.NonPrimarySchedulableResult schedResultTemp = VerifyCompatibilityGroupConstraintsAndDetermineNonPrimaryRR(a_simClock, a_act, a_si, a_primaryResource, resourceRequirementI, a_resourceRequirementSatiaters, ref schedulabilityPostEventTimes);
                if (schedResultTemp.Availability != SchedulableSuccessFailureEnum.Success && schedResultTemp.Availability != SchedulableSuccessFailureEnum.SuccessResNotRequired)
                {
                    if (schedulabilityPostEventTimes != null)
                    {
                        for (int postEventI = 0; postEventI < schedulabilityPostEventTimes.Count; ++postEventI)
                        {
                            SchedulabilityCustomization_AddSchedulabilityCustomizationEvent(schedulabilityPostEventTimes[postEventI]);
                        }
                    }

                    success = false;
                    if (schedResult.Availability == SchedulableSuccessFailureEnum.NotSet)
                    {
                        schedResult = schedResultTemp;
                    }
                    else
                    {
                        int comp = Resource.NonPrimarySchedulableResult.Compare(schedResult, schedResultTemp);
                        if (comp < 0)
                        {
                            // The resource isn't available. 
                            // Use the latest time among resource requirements.
                            schedResult = schedResultTemp;
                        }
                    }
                }
            }
        }

        if (!success)
        {
            // [USAGE_CODE] VerifyCompatibilityGroupConstraintsAndDetermineNonPrimaryRRs():  If a failed scheduling attempt has try again ticks with a LackCapacity reason back offset the TryAgainTicks by the setup span.
            if (schedResult.Availability != SchedulableSuccessFailureEnum.NotSet && schedResult.HasTryAgainTicks)
            {
                long releaseTicks = 0;
                switch (schedResult.Availability)
                {
                    case SchedulableSuccessFailureEnum.IntersectsReservedMoveDate:
                        if (schedResult.RR.UsageStart == MainResourceDefs.usageEnum.Run && a_act.Operation.ResourceRequirements.PrimaryResourceRequirement.UsageStart == MainResourceDefs.usageEnum.Setup)
                        {
                            // The activity can't schedule until the try again ticks has been reached.
                            a_act.WaitForUsageEvent = true;
                        }

                        releaseTicks = schedResult.TryAgainTicks;
                        break;

                    case SchedulableSuccessFailureEnum.AttentionNotAvailable:
                        // If attention isn't and setup isn't included, this reason is passed in and the time
                        // is the time the resouce might be available again. The release time needs to be adjusted
                        // back by the amount of setup, but it's possible the real release time could be earlier
                        // or later depending on the capacity of the efficiency of the primary resource. If
                        // the resource is less efficient, then the release time should be adjusted futher back
                        // than what's being done below. This could be efficiently done, but wasn't at the time of
                        // writing the code. If the resource is more efficient, then the setup could occur
                        // later than the release below.
                        // *****************************
                        // You also need to handle the case where multiple setup is required. 
                        // In that case, this adjustment to TryAgainTicks shouldn't be made.
                        //******************************
                        releaseTicks = schedResult.TryAgainTicks; // - a_si.m_requiredSetupSpan.TimeSpanTicks;
                        break;
                    case SchedulableSuccessFailureEnum.AttentionConflictBetweenMultipleRRs:
                        releaseTicks = schedResult.TryAgainTicks;
                        //releaseTicks = EstimateTryAgainTicksForAttnConflict(a_act, a_si);
                        break;

                    case SchedulableSuccessFailureEnum.IntersectsBlockReservation:
                    case SchedulableSuccessFailureEnum.Customization:
                    case SchedulableSuccessFailureEnum.Occupied: //A primary was scheduled where the helper attempted to schedule
                    case SchedulableSuccessFailureEnum.LackCapacityWithRetry: //No need to adjust the retry date
                        releaseTicks = schedResult.TryAgainTicks;
                        break;

                    case SchedulableSuccessFailureEnum.LackCapacity:
                    case SchedulableSuccessFailureEnum.HitCleanoutInterval: //A cleanout interval is within the blocks planned schedule duration
                        releaseTicks = schedResult.TryAgainTicks;
                        if (schedResult.RR.UsageStart == MainResourceDefs.usageEnum.Run)
                        {
                            // The setup can start before the run.
                            releaseTicks -= a_si.m_requiredSetupSpan.TimeSpanTicks;
                        }

                        break;

                    case SchedulableSuccessFailureEnum.CanPause:
                        releaseTicks = 0; //No retry date can be determined.
                        break;
                    
                    default: //The rest of the enums are not expected for helper capacity check
                        DebugException.ThrowInDebug("The type of SchedulableSuccessFailureEnum was unexpected.");
                        break;
                }

                if (releaseTicks > a_simClock)
                {
                    //It's possible to backcalculate exactly to the sim clock with correct values.
                    UsageEvent evt = new(releaseTicks, a_act);
                    AddEvent(evt);
                }
            }
        }

        return success;
    }

    private long EstimateTryAgainTicksForAttnConflict(InternalActivity a_act, SchedulableInfo a_si)
    {
        long tryAgainTicks = a_si.m_finishDate;
        ResourceRequirementManager rrm = a_act.Operation.ResourceRequirements;
        for (int rrI = 0; rrI < rrm.Count; ++rrI)
        {
            ResourceRequirement rr = rrm.GetByIndex(rrI);
            SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = rr.EligibleResources.GetEnumerator();
            while (ersEtr.MoveNext())
            {
                foreach (EligibleResourceSet.ResourceData rd in ersEtr.Current.Value)
                {
                    long end = a_si.GetUsageEnd(rr);
                    LinkedListNode<ResourceCapacityInterval> curOnline = rd.m_res.FindRCIForward(end);
                    tryAgainTicks = curOnline.Next.Value.StartDate;
                }
            }
        }

        return tryAgainTicks;
    }

    //long EstimateTryAgainTicksForAttnConflicts(InternalActivity a_act, List<Resource.NonPrimarySchedulableResult> a_attnConflicts, ResourceSatiator[] a_resourceRequirementSatiaters)
    //{
    //    HashSet<Resource> intersectingResources = new HashSet<Resource>();

    //    ResourceRequirementManager rrm = a_act.Operation.ResourceRequirements;

    //    // Add all the resources eligible to fulfill the first resource requirement.
    //    AddIntersectingResources(intersectingResources, rrm, 0);

    //    // For each ResourceRequirement whose resource is known, deduct any non matching resources from the full set of resources.
    //    for (int i = 1; i < a_attnConflicts.Count; ++i)
    //    {
    //        AddIntersectingResources(intersectingResources, rrm, i);
    //    }
    //}

    private static void AddIntersectingResources(HashSet<Resource> intersectingResources, ResourceRequirementManager rrm, int i)
    {
        SortedDictionary<BaseId, EligibleResourceSet>.Enumerator eligRessetr = rrm.GetByIndex(i).EligibleResources.GetEnumerator();
        while (eligRessetr.MoveNext())
        {
            IEnumerator<EligibleResourceSet.ResourceData> eligResEtr = eligRessetr.Current.Value.GetEnumerator();
            while (eligResEtr.MoveNext())
            {
                intersectingResources.Add((Resource)eligResEtr.Current.m_res);
            }
        }
    }

    private Resource.NonPrimarySchedulableResult VerifyCompatibilityGroupConstraintsAndDetermineNonPrimaryRR(long a_simClock, InternalActivity a_act, SchedulableInfo a_si, Resource a_primaryResource, int a_resourceRequirementI, ResourceSatiator[] a_resourceRequirementSatiators, ref List<long> r_schedulabilityPostEventTimes)
    {
        ResourceRequirementManager rrm = a_act.Operation.ResourceRequirements;
        ResourceRequirement rr = rrm.GetByIndex(a_resourceRequirementI);
        long finishDate = a_si.GetUsageEnd(rr);
        Resource.NonPrimarySchedulableResult schedulableResult;

        // Determine the finish date to use for the resource requirement.
        if (finishDate == a_simClock)
        {
            a_resourceRequirementSatiators[a_resourceRequirementI] = null;
            schedulableResult = new Resource.NonPrimarySchedulableResult(SchedulableSuccessFailureEnum.Success);
            return schedulableResult;
        }

        // Find a resource to satisfy the resource requirement
        if ((m_activeSimulationType == SimulationType.Optimize ||
             m_activeSimulationType == SimulationType.TimeAdjustment ||
             m_activeSimulationType == SimulationType.Expedite ||
             m_activeSimulationType == SimulationType.ClockAdvance ||
             m_activeSimulationType == SimulationType.Move ||
             m_activeSimulationType == SimulationType.MoveAndExpedite ||
             m_activeSimulationType == SimulationType.ConstraintsChangeAdjustment) &&
            a_act.Sequential != null)
        {
            if (!a_act.GetMoveResource(a_resourceRequirementI, out Resource iRes))
            {
                // The activity is not one of the successor operations that 
                // was moved or this is not a MoveSequentialStep simulation type.
                InternalActivity.SequentialState ss = a_act.Sequential[a_resourceRequirementI];
                iRes = ss.SequencedSimulationResource;
            }

            if (iRes == null && m_activeSimulationType == SimulationType.Optimize)
            {
                return VerifyCompatibilityGroupConstraintsAndDetermineNonPrimaryRRHelper(a_simClock, a_act, a_si, a_primaryResource, a_resourceRequirementI, a_resourceRequirementSatiators, ref r_schedulabilityPostEventTimes, rr, finishDate);
            }

            return RunSchedulabilityCustomizationAndTestIsSchedulableForNonPrimaryResourceRequirementAndSetResReservation(a_simClock, a_si, a_primaryResource, iRes, a_act, rr, a_resourceRequirementI, a_resourceRequirementSatiators, finishDate, ref r_schedulabilityPostEventTimes);
        }
        else if (m_activeSimulationType == SimulationType.Move && a_act.BeingMoved && a_act.GetMoveResource(a_resourceRequirementI, out Resource iRes))
        {
            //The helper resource has been preselected during move. //TODO: Not sure how/if this happens
            if (iRes != null)
            {
                return RunSchedulabilityCustomizationAndTestIsSchedulableForNonPrimaryResourceRequirementAndSetResReservation(a_simClock, a_si, a_primaryResource, iRes, a_act, rr, a_resourceRequirementI, a_resourceRequirementSatiators, finishDate, ref r_schedulabilityPostEventTimes);
            }

            // It's possible the resource isn't required because it's only needed during setup or only some other phase or processing. 
            Resource.NonPrimarySchedulableResult noResSuccess = new (rr, SchedulableSuccessFailureEnum.SuccessResNotRequired);
            return noResSuccess;
        }
        else
        {
            //Select an eligible helper resource
            return VerifyCompatibilityGroupConstraintsAndDetermineNonPrimaryRRHelper(a_simClock, a_act, a_si, a_primaryResource, a_resourceRequirementI, a_resourceRequirementSatiators, ref r_schedulabilityPostEventTimes, rr, finishDate);
        }
    }

    /// <summary>
    /// If the resource requirement passes IsSchedulable and the IsSchedulable customization tests, return true and reserve the resource.
    /// </summary>
    /// <param name="a_simClock"></param>
    /// <param name="a_si"></param>
    /// <param name="a_primaryRes">The primary resource that will be used must already be known.</param>
    /// <param name="a_ResToTest">The non-primary resource whose schedulability for the resource requirement will be tested.</param>
    /// <param name="a_act"></param>
    /// <param name="a_rr">The resource requirement that the resource is being tested for.</param>
    /// <param name="a_rrIdx">The index of the resource requirement being tested.</param>
    /// <param name="a_rrSatiators">The set of resources to use for the different resource requirements. The value at a_resourceRequirementI is always changed by this function.</param>
    /// <param name="a_finishTicks"></param>
    /// <param name="r_schedulabilityPostEventTimes">
    /// If the resource didn't pass the a customization's IsSchedulable test and it returns a time to try scheduling again, the value will be added to this list.
    /// Also the list will be created if necessary.
    /// </param>
    /// <returns>true if the resource passes the IsSchedulable tests.</returns>
    private Resource.NonPrimarySchedulableResult RunSchedulabilityCustomizationAndTestIsSchedulableForNonPrimaryResourceRequirementAndSetResReservation(long a_simClock, SchedulableInfo a_si, Resource a_primaryRes, InternalResource a_ResToTest, InternalActivity a_act, ResourceRequirement a_rr, int a_rrIdx, ResourceSatiator[] a_rrSatiators, long a_finishTicks, ref List<long> r_schedulabilityPostEventTimes)
    {
        Resource res = a_ResToTest as Resource;

        // [USAGE_CODE] RunSchedulabilityCustomizationAndTestIsSchedulableForNonPrimaryResourceRequirementAndSetResReservation(): If the resource being tested isn't available in time but the tested RR doesn't use setup the IsSchedulable test can still be performed to check if the resource will be available at its UsageStart.
        bool usageSkipsSetup = !a_rr.ContainsUsage(MainResourceDefs.usageEnum.Setup) && a_si.m_requiredSetupSpan.TimeSpanTicks > 0; //This usage starts after primary setup
        bool usageSkipsRun = a_rr.UsageStart > MainResourceDefs.usageEnum.Run; //Primary must have run, so this starts in the future.
        Resource.NonPrimarySchedulableResult schedResult = new ();

        if (res == null)
        {
            // There are no checks.
            // null can be passed in when a resource isn't required for the resource requirement. 
            // For instance, if there's a resource requirement for setup, but setup has already been reported finished, the setup resource is no longer required and so the resource would be null.
            // Presumes a_resourceRequirementSatiators[a_resourceRequirementI] is already set to null.
            schedResult = new Resource.NonPrimarySchedulableResult(SchedulableSuccessFailureEnum.Success);
        }
        else if (
            (res.AvailableInSimulation != null || // The resource is available
             usageSkipsSetup ||
             usageSkipsRun) // or the resource isn't available but the setup time isn't required, so a check availability when processing starts needs to be performed.
            &&
            !res.ReservedForResourceRequirement)
        {
            a_rrSatiators[a_rrIdx] = new ResourceSatiator(res);

            if (SchedulabilityCustomization_RunCustomization(a_act))
            {
                long? delayedScheduleTime = m_extensionController.IsSchedulableNonPrimaryResourceRequirement(this, m_activeSimulationType, Clock, a_simClock, a_rrSatiators, a_rrIdx, a_act, a_si);
                if (delayedScheduleTime.HasValue)
                {
                    #if TEST
                        ++_deSchedulabilityCustomization;
                    #endif
                    if (delayedScheduleTime.Value > a_simClock)
                    {
                        if (r_schedulabilityPostEventTimes == null)
                        {
                            r_schedulabilityPostEventTimes = new List<long>();
                        }

                        r_schedulabilityPostEventTimes.Add(delayedScheduleTime.Value);
                    }

                    schedResult = new Resource.NonPrimarySchedulableResult(a_rr, SchedulableSuccessFailureEnum.Customization, delayedScheduleTime.Value);
                }
            }

            // [USAGE_CODE] RunSchedulabilityCustomizationAndTestIsSchedulableForNonPrimaryResourceRequirementAndSetResReservation(): Create a block reservation if setup time is excluded from a resource requirement.
            if (!schedResult.Constructed)
            {
                if (usageSkipsSetup || usageSkipsRun)
                {
                    // The end date varies depending on the UsageEnd.
                    LinkedListNode<ResourceCapacityInterval> availableNode;

                    //Get the SI for the helper based on it's usages
                    SchedulableInfo nonPrimarySchedulableInfo = a_si.GetNonPrimarySchedulableInfo(a_rr, res);

                    if (nonPrimarySchedulableInfo.m_zeroLength)
                    {
                        //Nothing to schedule
                        a_rrSatiators[a_rrIdx] = null;
                        return new Resource.NonPrimarySchedulableResult(SchedulableSuccessFailureEnum.SuccessResNotRequired);
                    }

                    //SchedulableInfo requirementsSchedInfo = new SchedulableInfo(a_si.m_zeroLength, requiredStart, requiredStart, processingFinish, finish, res, a_si.m_requiredSetupSpan, a_si.m_requiredProcessingSpan, a_si.m_requiredPostProcessingSpan, a_si.m_requiredNumberOfCycles, a_si.m_requiredQty, endOfStoragePostProcessingTicks, a_si.m_ocp);
                    // The desired start time might not be online.
                    // start ticks is set to the next time the resource is online.
                    long startTicks = res.NextAvailableDate(a_act, nonPrimarySchedulableInfo.m_scheduledStartDate, nonPrimarySchedulableInfo.m_scheduledStartDate, LeftNeighborSequenceValues.NullValues, out availableNode, ExtensionController);

                    bool availableAtStartDate = startTicks == nonPrimarySchedulableInfo.m_scheduledStartDate;

                    //We are validating whether a process stage spans intervals. It's possible that setup ends exactly at the end of an interval and Run starts on the next interval
                    // In this case, the date times are not equal, but the calculations are correct.
                    //TODO: Calculate process start AND END times in o_si. If that is done, this can be removed and updated to use usage start dates
                    if (!availableAtStartDate && availableNode.Value.StartDate == startTicks)
                    {
                        LinkedListNode<ResourceCapacityInterval> node = a_primaryRes.AvailableNode.Data.Node;
                        while (node.Value.StartDate < nonPrimarySchedulableInfo.m_scheduledStartDate)
                        {
                            if (node.Value.EndDate == nonPrimarySchedulableInfo.m_scheduledStartDate)
                            {
                                LinkedListNode<ResourceCapacityInterval> nextOnlinePrimaryNode = ResourceCapacityIntervalList.FindNextOnline(node);
                                if (nextOnlinePrimaryNode.Value.StartDate == availableNode.Value.StartDate)
                                {
                                    availableAtStartDate = true;
                                    break;
                                }
                            }

                            node = node.Next;
                        }
                    }

                    if (availableAtStartDate)
                    {
                        schedResult = res.NonPrimarySchedulable(this, a_act, a_rr, a_rrSatiators, a_si, availableNode);
                        if (schedResult.Availability == SchedulableSuccessFailureEnum.Success)
                        {
                            // [USAGE_CODE] RunSchedulabilityCustomizationAndTestIsSchedulableForNonPrimaryResourceRequirementAndSetResReservation(): Add a BlockReservation to a resource and set the BlockReservation as the satiator or a ResourceRequirement.
                            // Create a BlockReservation.
                            BlockReservation reservation = res.AddBlockReservation(a_act, a_rr, nonPrimarySchedulableInfo.m_scheduledStartDate, nonPrimarySchedulableInfo.m_finishDate, nonPrimarySchedulableInfo);
                            a_rrSatiators[a_rrIdx] = new ResourceSatiator(res, reservation);
                        }
                    }

                    //TODO: Refactor this if statement. It is split like this to share the primary capacity calculation
                    if (!availableAtStartDate || schedResult.Availability == SchedulableSuccessFailureEnum.Occupied)
                    {
                        //TODO: Move to function
                        //Determine the primary's capacity so we can back calculate based on the primary resource capacity needs.
                        LeftNeighborSequenceValues lastOpSequenceVals = a_primaryRes.CreateDefaultLeftNeighborSequenceValues(a_simClock);
                        RequiredCapacityPlus rc = a_primaryRes.CalculateTotalRequiredCapacity(a_simClock, a_act, lastOpSequenceVals, true, a_simClock, ExtensionController);
                        RequiredCapacity primaryCapacity = null;
                        //Include capacity only for the potions before this usage
                        if (a_rr.UsageStart > MainResourceDefs.usageEnum.Clean)
                        {
                            primaryCapacity = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, rc.SetupSpan, rc.ProcessingSpan, rc.PostProcessingSpan, rc.StorageSpan, rc.CleanAfterSpan);
                        }
                        else if (a_rr.UsageStart > MainResourceDefs.usageEnum.Storage)
                        {
                            primaryCapacity = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, rc.SetupSpan, rc.ProcessingSpan, rc.PostProcessingSpan, rc.StorageSpan, RequiredSpanPlusClean.s_notInit);
                        }                        
                        else if (a_rr.UsageStart > MainResourceDefs.usageEnum.PostProcessing)
                        {
                            primaryCapacity = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, rc.SetupSpan, rc.ProcessingSpan, rc.PostProcessingSpan, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                        }
                        else if (a_rr.UsageStart == MainResourceDefs.usageEnum.PostProcessing)
                        {
                            primaryCapacity = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, rc.SetupSpan, rc.ProcessingSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                        }
                        else //Run
                        {
                            primaryCapacity = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, rc.SetupSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                        }

                        if (schedResult.Availability == SchedulableSuccessFailureEnum.Occupied)
                        {
                            //Occupied, backcalculate from where the occupying block ends
                            Resource.FindStartFromEndResult backCalculatedSU = a_primaryRes.FindCapacityReverse(Clock, schedResult.TryAgainTicks, primaryCapacity, null, a_act);

                            schedResult = new Resource.NonPrimarySchedulableResult(a_rr, SchedulableSuccessFailureEnum.Occupied, backCalculatedSU.StartTicks);
                        }
                        else
                        {
                            //No capacity
                            // Report back when the resource is available.

                            //The startTicks is the earliest start of the Helper requirement. The helper may have a future usage. If so, backcalculate start for the primary.

                            //DO we need this? it does this for move backcalculate
                            //long start = a_simClock;
                            //if (primaryRes.LastScheduledBlockListNode != null)
                            //{
                            //    start = primaryRes.LastScheduledBlockListNode.Data.EndTicks;
                            //}

                            Resource.FindStartFromEndResult backCalculatedSU = a_primaryRes.FindCapacityReverse(Clock, startTicks, primaryCapacity, null, a_act);

                            long retryDate = backCalculatedSU.StartTicks;
                            if (backCalculatedSU.StartTicks == m_simClock)
                            {
                                return new Resource.NonPrimarySchedulableResult(a_rr, SchedulableSuccessFailureEnum.LackCapacity);
                            }
                            
                            if (backCalculatedSU.StartTicks == nonPrimarySchedulableInfo.m_scheduledStartDate)
                            {
                                //TODO: We have a mis calculation. The helper capacity just failed to schedule at this time, we can't try again.
                                //TODO: A better fix would be to reclaculate the processing start to be on the next interval, and not the last tick of current one.
                                //NOTE: I have seen this occur when the helper run starts on the last tick of an interval.
                                //      This is possible if setup is exactly the length of an interval and the run continues accross an offline to finish later.
                                //      If the helper usage starts at run, it tries to use the last tick of a capacity interval for run, but the rci.Contains 
                                //      function does not include the last tick (I believe so rcis can start and end at the same time without double counting the tick)
                                //As a quick fix, just try 1 tick later instead of getting stuck in an infinite loop.
                                retryDate += 1;
                            }

                            schedResult = new Resource.NonPrimarySchedulableResult(a_rr, SchedulableSuccessFailureEnum.LackCapacityWithRetry, retryDate);
                        }
                    }
                }
                else
                {
                    schedResult = res.NonPrimarySchedulable(this, a_act, a_rr, a_rrSatiators, a_si, null);
                    if (schedResult.Availability == SchedulableSuccessFailureEnum.Success)
                    {
                        SetReservation(res);
                    }
                }
            }
            else
            {
                schedResult = new Resource.NonPrimarySchedulableResult(SchedulableSuccessFailureEnum.LackCapacity);
            }
        }

        if (schedResult.Availability == SchedulableSuccessFailureEnum.IntersectsReservedMoveDate)
        {
            // [USAGE_CODE] RunSchedulabilityCustomizationAndTestIsSchedulableForNonPrimaryResourceRequirementAndSetResReservation: An activity intersecting the move time needs to be handled.
            long intersectionTicks = m_moveData.StartTicksAdjusted;
            ResourceOperation op = (ResourceOperation)a_act.Operation;
            if (op.ResourceRequirements.HasMixOfUsageStarts())
            {
                AddMoveIntersector(m_moveData.StartTicksAdjusted, res.ReservedMoveStartTicks, a_act, a_rr, a_rrIdx, res);
            }
            else
            {
                //TODO: This might not be perfect. We can't use m_moveData.StartTicksAdjusted because it could be the clock date and get into an infinite loop
                schedResult.TryAgainTicks = res.ReservedMoveEndTicks;
            }
        }

        return schedResult;
    }

    private Resource.NonPrimarySchedulableResult VerifyCompatibilityGroupConstraintsAndDetermineNonPrimaryRRHelper(long a_simClock, InternalActivity a_activity, SchedulableInfo a_si, Resource a_primaryResource, int a_resourceRequirementI, ResourceSatiator[] a_resourceRequirementSatiators, ref List<long> r_schedulabilityPostEventTimes, ResourceRequirement rr, long a_finishDate)
    {
        if ((m_activeSimulationType == SimulationType.Move ||
             m_activeSimulationType == SimulationType.MoveAndExpedite) &&
            a_activity.GetMoveResource(a_resourceRequirementI, out Resource resI))
        {
            return RunSchedulabilityCustomizationAndTestIsSchedulableForNonPrimaryResourceRequirementAndSetResReservation(a_simClock, a_si, a_primaryResource, resI, a_activity, rr, a_resourceRequirementI, a_resourceRequirementSatiators, a_finishDate, ref r_schedulabilityPostEventTimes);
        }

        Resource lockedResource = a_activity.ResourceRequirementLock(a_resourceRequirementI);

        if (lockedResource != null)
        {
            if (lockedResource.AvailableInSimulation != null && lockedResource.ReservedForResourceRequirement == false)
            {
                return RunSchedulabilityCustomizationAndTestIsSchedulableForNonPrimaryResourceRequirementAndSetResReservation(a_simClock, a_si, a_primaryResource, lockedResource, a_activity, rr, a_resourceRequirementI, a_resourceRequirementSatiators, a_finishDate, ref r_schedulabilityPostEventTimes);
            }

            if (lockedResource.ReservedForResourceRequirement == false)
            {
                return new Resource.NonPrimarySchedulableResult(SchedulableSuccessFailureEnum.ReservedForResReq);
            }

            return new Resource.NonPrimarySchedulableResult(SchedulableSuccessFailureEnum.LockedToResUnavail);
        }

        Resource.NonPrimarySchedulableResult schedRes;
        if (rr.DefaultResource != null)
        {
            Resource defaultRes = rr.DefaultResource as Resource;

            schedRes = RunSchedulabilityCustomizationAndTestIsSchedulableForNonPrimaryResourceRequirementAndSetResReservation(a_simClock, a_si, a_primaryResource, defaultRes, a_activity, rr, a_resourceRequirementI, a_resourceRequirementSatiators, a_finishDate, ref r_schedulabilityPostEventTimes);
            if (schedRes.Availability == SchedulableSuccessFailureEnum.Success)
            {
                return schedRes;
            }

            if (!rr.DefaultResource_UseJITLimitTicks)
            {
                return schedRes;
            }

            long defResJitRelease = rr.DefaultResource_CalcJimLimitRelease(a_activity);
            if (rr.DefaultResource_UseJITLimitTicks && defResJitRelease < a_simClock)
            {
                return schedRes;
            }
        }

        //Check reservations
        if (ScenarioOptions.EnforceMaxDelay)
        {
            ResourceReservation reservation = a_activity.GetReservationForRR(a_resourceRequirementI);
            if (reservation != null)
            {
                Resource defaultRes = reservation.ReservedResource;

                schedRes = RunSchedulabilityCustomizationAndTestIsSchedulableForNonPrimaryResourceRequirementAndSetResReservation(a_simClock, a_si, a_primaryResource, defaultRes, a_activity, rr, a_resourceRequirementI, a_resourceRequirementSatiators, a_finishDate, ref r_schedulabilityPostEventTimes);
                return schedRes;
            }
        }

        EligibleResourceSet eligibleResourceSet = a_activity.ResReqsEligibilityNarrowedDuringSimulation[a_resourceRequirementI][a_primaryResource.PlantId];
        bool constrainToPrimaryResourcesCell = false;

        if (a_primaryResource.Cell != null)
        {
            for (int eligibleResourceI = 0; eligibleResourceI < eligibleResourceSet.Count; eligibleResourceI++)
            {
                resI = (Resource)eligibleResourceSet[eligibleResourceI];
                Resource res = resI;
                if (res.Cell != null)
                {
                    if (a_primaryResource.Cell == res.Cell && res.Active)
                    {
                        constrainToPrimaryResourcesCell = true;
                    }
                }
            }
        }

        schedRes = new Resource.NonPrimarySchedulableResult(SchedulableSuccessFailureEnum.NotSet);
        for (int eligibleResourceI = 0; eligibleResourceI < eligibleResourceSet.Count; eligibleResourceI++)
        {
            resI = (Resource)eligibleResourceSet[eligibleResourceI];
            Resource res = resI as Resource;

            if (res != null)
            {
                if (a_primaryResource.IsAllowedHelperResource(res) == false)
                {
                    continue;
                }

                if (constrainToPrimaryResourcesCell)
                {
                    if (a_primaryResource.Cell != res.Cell)
                    {
                        // The resource isn't eligible because it is not in the same cell as the primary resource.
                        continue;
                    }
                }

                Resource.NonPrimarySchedulableResult schedResTemp = RunSchedulabilityCustomizationAndTestIsSchedulableForNonPrimaryResourceRequirementAndSetResReservation(a_simClock, a_si, a_primaryResource, resI, a_activity, rr, a_resourceRequirementI, a_resourceRequirementSatiators, a_finishDate, ref r_schedulabilityPostEventTimes);
                if (schedResTemp.Availability == SchedulableSuccessFailureEnum.Success)
                {
                    return schedResTemp;
                }

                if (schedRes.Availability == SchedulableSuccessFailureEnum.NotSet)
                {
                    schedRes = schedResTemp;
                }
                else
                {
                    int comp = Resource.NonPrimarySchedulableResult.Compare(schedRes, schedResTemp);
                    if (comp > 0)
                    {
                        // Use the soonest available resource as the time to try again.
                        schedRes = schedResTemp;
                    }
                }
            }
        }

        return schedRes;
    }

    private bool VerifyCompatibilityGroupConstraint(long a_simClock, InternalActivity a_activity, ResourceSatiator[] a_resourceRequirementSatiators, int a_resourceRequirementI)
    {
        if (!a_activity.Operation.UseCompatibilityCode)
        {
            return true;
        }

        //TODO: use end date and update tables to validate for future reservation in case of MaxDelay

        // Verify the compatibility group constraints.
        ResourceSatiator rs = a_resourceRequirementSatiators[a_resourceRequirementI];

        if (rs != null)
        {
            Resource currentRR = rs.Resource;
            foreach (CompatibilityCodeTable compatibilityTable in currentRR.CompatibilityTables)
            {
                if (!compatibilityTable.IsCodeValid(a_simClock, a_activity.Operation.CompatibilityCode))
                {
                    a_activity.Operation.SetLatestConstraintToCompatibilityCode(a_simClock, compatibilityTable.Name);
                    return false;
                }
            }
        }

        return true;
    }

    private void ScheduleNextMultiTaskingResourceAvailableEvent(Resource a_res, LinkedListNode<ResourceCapacityInterval> a_rciNode)
    {
        do
        {
            ResourceCapacityInterval rci = a_rciNode.Value;

            if (rci.Active)
            {
                long startTicks = Math.Max(SimClock, rci.StartDate);
                if (rci.CalcAvailableAttentionPointInTime(startTicks) > 0)
                {
                    AddResourceAvailableEvent(a_res, startTicks, a_rciNode);
                    break;
                }

                long nextRCIAvailableTime = rci.GetNextReleaseDate(SimClock);
                if (nextRCIAvailableTime != long.MaxValue)
                {
                    // Add an event at the next time the resource will be available.
                    //We need to verify that this node is the next available. It's possible the attention is used beyond this interval
                    if (rci.EndDate >= nextRCIAvailableTime)
                    {
                        AddResourceAvailableEvent(a_res, nextRCIAvailableTime, a_rciNode);
                        break;
                    }
                }
            }

            a_rciNode = a_rciNode.Next;
        } while (a_rciNode != null);
    }

    private void SetSuccessorsPredOpConnection(InternalOperation a_scheduledOperation, Resource a_connectorRes)
    {
        AlternatePath.AssociationCollection ac = a_scheduledOperation.AlternatePathNode.Successors;

        for (int sucI = 0; sucI < a_scheduledOperation.AlternatePathNode.Successors.Count; ++sucI)
        {
            InternalOperation sucOp = ac[sucI].Successor.Operation;
            bool setPredOpConnectorConstraint = false;
            
            if (sucOp.BeingMoved)
            {
                //successor op might be getting moved to a connected resource so we still need to set the constraint
                InternalActivity leadAct = sucOp.Activities.GetByIndex(0);
                if (leadAct.GetMoveResource(0, out Resource moveRes))
                {
                    Dictionary<BaseId, Resource> downstreamConnectors = ResourceConnectorManager.GetDownstreamSuccessorConnectorsFromResource(a_connectorRes.Id);
                    if (downstreamConnectors.Count > 0  && downstreamConnectors.ContainsKey(moveRes.Id))
                    {
                        setPredOpConnectorConstraint = true;
                    }
                }
            }
            else
            {
                //connector constraint is overriden if the successor op is locked to a resource
                setPredOpConnectorConstraint = sucOp.GetPrimaryRRLockedStatus() == lockTypes.Unlocked;
            }

            if (setPredOpConnectorConstraint)
            {
                sucOp.SetPredOpConnectorResource(a_connectorRes, a_scheduledOperation, ResourceConnectorManager);
            }

            if (a_scheduledOperation.BeingMoved)
            {
                //We need to verify the resource connectors are still valid on the new resource
                sucOp.VerifyResourceConnectors(a_connectorRes, a_scheduledOperation, ResourceConnectorManager);
            }
        }
    }
    //#if DEBUG
    //    //void SetSuccessorsPredOpConnection(InternalOperation a_io, Resource a_connectorRes, int a_connectorResCnt, int a_connectionResReqIdx)
    //    //{
    //    //    AlternatePath.AssociationCollection ac = a_io.AlternatePathNode.Successors;

    //    //    for (int sucI = 0; sucI < a_io.AlternatePathNode.Successors.Count; ++sucI)
    //    //    {
    //    //        InternalOperation sucOp = (InternalOperation)ac[sucI].Successor.Operation;
    //    //        lockTypes lockedStatus = sucOp.GetPrimaryRRLockedStatus();
    //    //        if (lockedStatus == lockTypes.Unlocked)// Resource connectors don't apply to locked resources.
    //    //        {
    //    //            // Resource connectors don't operations being moved

    //    //            if (!sucOp.BeingMoved || (sucOp.BeingMoved && sucOp.AllowMoveToOverrideConnectorTransferTicks))
    //    //            {
    //    //                sucOp.___SetPredActConnectorResource(a_connectorRes, a_connectorResCnt, a_connectionResReqIdx);
    //    //            }
    //    //        }
    //    //    }
    //    //}
    //#else
    //        delete
    //#endif

    /// <summary>
    /// After an activity has been scheduled call this function. It helps take care of notifying successor operations and MO
    /// in the event that a predecessor has completed.
    /// </summary>
    /// <param name="activity"></param>
    private void PredOpReady_ProcessSucActivitiesAndSucMOs(InternalActivity a_activity, InternalResource a_res, PredecessorReadyType a_predOpReadyType)
    {
        InternalOperation op = a_activity.Operation;

        if (op.ManufacturingOrder.Scheduled && a_predOpReadyType != PredecessorReadyType.BlockStartCalculated)
        {
            //ReleaseAnyRightBatchNeighborsAfterOpHasBeenScheduled(a_activity.Operation, SimClock, a_res);
            if (op.GetScheduledFinishDate(out long opScheduledFinishDate, false))
            {
                op.ManufacturingOrder.SuccessorCompletionProcessing(op.ManufacturingOrder.ScheduledEnd, this);
                ExecuteOperationScheduledCustomizationHelper(op, this, SimClock);
            }
        }
        else if (op.Scheduled)
        {
            //ReleaseAnyRightBatchNeighborsAfterOpHasBeenScheduled(a_activity.Operation, SimClock, a_res);

            if (op.AlternatePathNode.Successors.Count > 0)
            {
                //AddBatchOperationReadyEventForSucOpsNoLongerWaitingForPredBatchOps(op);
                AddPredecessorAvailableEvent(op, SimClock, a_predOpReadyType);
            }

            ExecuteOperationScheduledCustomizationHelper(op, this, SimClock);
        }
        else
        {
            // Notify successor operations that a predecessor operation has been scheduled.
            // This is the case where an operation has multiple activities and it's possible for the successor to be released
            // after the first activity has been scheduled.
            for (int sucI = 0; sucI < op.AlternatePathNode.Successors.Count; ++sucI)
            {
                AlternatePath.Association association = op.AlternatePathNode.Successors[sucI];

                switch (association.OverlapType)
                {
                    case InternalOperationDefs.overlapTypes.TransferSpan:
                    case InternalOperationDefs.overlapTypes.TransferSpanAfterSetup:
                    case InternalOperationDefs.overlapTypes.TransferSpanBeforeStartOfPredecessor:
                        AddPredecessorAvailableEvent(op, SimClock, a_predOpReadyType);
                        goto BreakOutOfForLoop;
                }
            }

            BreakOutOfForLoop: ;
        }
    }

    private void ExecuteOperationScheduledCustomizationHelper(InternalOperation op, ScenarioDetail a_sd, long a_simClock)
    {
        if (ExtensionController.RunOperationScheduledExtension)
        {
            ChangeableOperationValues changableOpValues = a_sd.ExtensionController.OperationScheduled(op, a_sd, a_simClock);
            if (changableOpValues != null)
            {
                if (changableOpValues.ResourceRequirementChanges != null)
                {
                    for (int rrI = 0; rrI < changableOpValues.ResourceRequirementChanges.Count; ++rrI)
                    {
                        ChangeableOperationValues.ChangableResourceRequirementValue changeRRValue = changableOpValues.ResourceRequirementChanges[rrI];

                        if (changeRRValue.DefaultResource == null)
                        {
                            changeRRValue.ResourceRequirement.DefaultResource_Clear();
                        }
                        else
                        {
                            changeRRValue.ResourceRequirement.DefaultResource_Set(changeRRValue.DefaultResource, changeRRValue.ResourceRequirement.DefaultResource_UseJITLimitTicks, changeRRValue.ResourceRequirement.DefaultResource_JITLimitTicks);
                        }
                    }
                }
            }
        }
    }

    private enum IsBatchResourceCapableOfSchedulingActivityEnum { Yes, NoReqFinQtyIsGreaterThanQtyPerCycle, NoReqFinQtyIsGreaterThanResBatchVolume }

    /// <summary>
    /// Whether a batch resource is able to schedule an activity. This can depend on the resource's BatchType.
    /// Call this function if you want to determine whether a batch resource can be used as the primary resource
    /// for an activity.
    /// </summary>
    /// <param name="a_primaryResource">If you pass in a non batch resource, this function will return true.</param>
    /// <param name="a_act"></param>
    /// <returns>bool indicating whether the activity can be scheduled on this resource.</returns>
    private IsBatchResourceCapableOfSchedulingActivityEnum IsBatchResourceCapableOfSchedulingActivity(Resource a_primaryResource, InternalActivity a_act)
    {
        decimal batchAmount = a_act.BatchAmount;

        switch (a_primaryResource.BatchType)
        {
            case MainResourceDefs.batchType.None:
                break;

            case MainResourceDefs.batchType.Percent:
                decimal percent = batchAmount / a_act.GetResourceProductionInfo(a_primaryResource).QtyPerCycle;
                if (percent > 1)
                {
                    // This isn't eligible for scheuling on BatchType.Percent resources because the quantity exceeds a single cycle.
                    return IsBatchResourceCapableOfSchedulingActivityEnum.NoReqFinQtyIsGreaterThanQtyPerCycle;
                }

                break;

            case MainResourceDefs.batchType.Volume:
                if (a_primaryResource.BatchVolume < batchAmount)
                {
                    // This isn't eligible for scheduling on this resource because the volume required exceeds the capabilities of the resource.
                    return IsBatchResourceCapableOfSchedulingActivityEnum.NoReqFinQtyIsGreaterThanResBatchVolume;
                }

                break;

            default:
                throw new Exception("An unknown batch type was encountered.");
        }

        return IsBatchResourceCapableOfSchedulingActivityEnum.Yes;
    }

    //private void AddBatchOperationReadyEventForSucOpsNoLongerWaitingForPredBatchOps(InternalOperation a_predOp, InternalOperation a_sucOp, long a_time)
    //{
    //    if (a_predOp.m_manufacturingOrderBatch_batchOperationNameData != null && a_sucOp.m_manufacturingOrderBatch_batchOperationNameData != null)
    //    {
    //        ManufacturingOrderBatch.BatchesOperationNameData sucBond = a_sucOp.m_manufacturingOrderBatch_batchOperationNameData;
    //        --sucBond.m_totalUnscheduledPredOps;

    //        if (sucBond.m_totalUnscheduledPredOps == 0)
    //        {
    //            List<InternalOperation> sucOpsNoLongerWaitingForPredBatchOps = sucBond.GetOpsNoLongerWaitingForPredBatchOps();
    //            for (int i = 0; i < sucOpsNoLongerWaitingForPredBatchOps.Count; ++i)
    //            {
    //                InternalOperation sucOp = sucOpsNoLongerWaitingForPredBatchOps[i];
    //                BatchOperationReadyEvent bore = new (a_time, sucOp);
    //                AddEvent(bore);
    //            }
    //        }
    //    }
    //}

    //private void ReleaseAnyRightBatchNeighborsAfterOpHasBeenScheduled(InternalOperation a_op, long a_minimumPredecessorReadyTime, InternalResource a_res)
    //{
    //    if (a_op.m_manufacturingOrderBatch_batchOperationNameData != null)
    //    {
    //        ManufacturingOrderBatch.BatchesOperationNameData bond = a_op.m_manufacturingOrderBatch_batchOperationNameData;
    //        List<BaseOperation> batchOpList = bond.m_opsList;
    //        if (batchOpList.Count - 1 > a_op.m_manufacturingOrderBatch_batchOrderData_op_index)
    //        {
    //            BaseOperation batchRightNeighborOp = batchOpList[a_op.m_manufacturingOrderBatch_batchOrderData_op_index + 1];
    //            InternalOperation io = batchRightNeighborOp as InternalOperation;
    //            if (io != null)
    //            {
    //                for (int i = 0; i < io.Activities.Count; ++i)
    //                {
    //                    io.Activities.GetByIndex(i).m_moBatchLockedResource = a_res;
    //                }
    //            }

    //            batchRightNeighborOp.WaitForLeftBatchNeighborReleaseEvent = false;

    //            OperationReleaseCheckAndHandling(a_minimumPredecessorReadyTime, batchRightNeighborOp, InternalOperation.LatestConstraintEnum.ManufacturingOrderBatch);
    //        }
    //    }
    //}

    /// <summary>
    /// After an activitiy in the PostProcessing state is scheduled this function needs to be called to handle any
    /// sequential state releases.
    /// </summary>
    /// <param name="activity"></param>
    private void SequentialStateHandlingForAPostProcessingActivity(InternalActivity a_activity)
    {
        ResourceRequirementManager rrm = a_activity.Operation.ResourceRequirements;

        for (int rrI = 0; rrI < rrm.Count; ++rrI)
        {
            SequentialStateHandling(a_activity, rrI, 0);
        }
    }

    /// <summary>
    /// After a ResourceRequirment is scheduled this function needs to be called to handle the sequential state releases.
    /// </summary>
    /// <param name="activity">The activity that was scheduled.</param>
    /// <param name="requirementI">The index of the resource requirement you want handled.</param>
    /// <param name="rrFinishDate">The finish date of resource requirement requirementI.</param>
    /// <param name="scheduledNode"></param>
    private void SequentialStateHandling(InternalActivity a_act, int a_requirementI, long a_rrFinishDate)
    {
        // If you don't remove the activities from the batch, a clock event will be created for the move activities and they won't get scheduled.
        // If you do remove the activities from the batch, they won't appear in the enumeration below and their flags won't be set to true.
        // 
        // Possible Solutions
        // 1. Change how Move or UnscheduleActivities works so either the activity is removed from the batch or unschedule activities ignores removed activities.
        // 2. Save the right neighbors in a list.
        // 3. Save the removed activities in the batch and access them through another iterator here. GetPreviousActivitiesBatch.
        if (a_act.Sequential != null)
        {
            InternalActivity.SequentialState ss = a_act.Sequential[a_requirementI];
            Batch rightSequencedNeighbor = ss.RightSequencedBatchNeighbor;

            if (rightSequencedNeighbor != null)
            {
                IEnumerator<InternalActivity> batchEtr = rightSequencedNeighbor.GetEnumerator();
                while (batchEtr.MoveNext())
                {
                    InternalActivity batchAct = batchEtr.Current;
                    InternalActivity.SequentialState rightNeighborsSS = batchAct.Sequential[ss.RightSequencedNeighborResourceRequirementIndex];
                    if (a_act.Operation.ManufacturingOrder != batchAct.Operation.ManufacturingOrder)
                    {
                        rightNeighborsSS.LeftSequencedNeighborScheduled = true;
                        rightNeighborsSS.ReleaseTicks = a_rrFinishDate;

                        if (batchAct.Released && !batchAct.ActivityReleaseEventPosted)
                        {
                            long activityReleaseTime = Math.Max(a_rrFinishDate, batchAct.Operation.AlternatePathNode.MaxPredecessorReleaseTicks);
                            activityReleaseTime = Math.Max(activityReleaseTime, batchAct.Operation.LatestPredecessorReadyDate);
                            AddActivityReleasedEvent(batchAct, activityReleaseTime);
                        }
                    }
                    else if (a_act.Operation.ManufacturingOrder == batchAct.Operation.ManufacturingOrder)
                    {
                        rightNeighborsSS.LeftSequencedNeighborScheduled = true;
                        rightNeighborsSS.ReleaseTicks = a_rrFinishDate;

                        AddActivityReleaseEventIfReleased(batchAct, a_rrFinishDate);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Adds ready leaf activities of a manufacturing order to eligible dispatchers.
    /// </summary>
    /// <param name="moReleasedEvent"></param>
    private void ManufacturingOrderReleased(ManufacturingOrder a_mo, long a_time, InternalOperation.LatestConstraintEnum a_latestConstraint)
    {
        // !ALTERNATE_PATH!; $$$ 1.0 Where AlternatePathReleaseEvents are Created. The order can't be scheduled until at least one path has been released.

        //
        // Release the individual paths.
        //
        if (ReleaseAlternatePaths(a_mo))
        {
            // Release the automatic paths
            bool currentPathReleased = false;
            List<AlternatePath> pathsToRelease = new ();

            a_mo.AlternatePaths.FindAutoUsePaths(pathsToRelease, new AlternatePathDefs.AutoUsePathEnum[] { AlternatePathDefs.AutoUsePathEnum.RegularRelease });

            AlternatePath inProcessPath = null;
            foreach (AlternatePath path in pathsToRelease)
            {
                InternalActivityDefs.productionStatuses prodStatus = path.GetMaxProductionStatus();
                if ((int)prodStatus >= (int)InternalActivityDefs.productionStatuses.Started)
                {
                    inProcessPath = path;
                    break;
                }
            }

            if (inProcessPath != null)
            {
                AddConstrainingEvents(inProcessPath);
                AddAlternatePathReleaseEventIfSatisfiable(m_simClock, a_time, a_mo, inProcessPath, a_latestConstraint);
            }
            else
            {
                foreach (AlternatePath path in pathsToRelease)
                {
                    if (path == a_mo.CurrentPath)
                    {
                        currentPathReleased = true;
                    }

                    AddConstrainingEvents(path);
                    AddAlternatePathReleaseEventIfSatisfiable(m_simClock, a_time, a_mo, path, a_latestConstraint);
                }

                //
                // Release paths that are released based on an offset from the DefaultPath.
                //
                pathsToRelease.Clear();

                long defaultReleaseTicks;

                if (m_activeSimulationType == SimulationType.Expedite)
                {
                    defaultReleaseTicks = a_time;
                }
                else if (m_activeSimulationType == SimulationType.Optimize)
                {
                    defaultReleaseTicks = DetermineLatestReleaseOfASatisfiablePath(a_mo);
                }
                else
                {
                    throw new Exception("2566");
                }

                AlternatePathDefs.AutoUsePathEnum[] usePathEnums;
                if (m_activeSimulationType == SimulationType.Expedite)
                {
                    usePathEnums = new[] { AlternatePathDefs.AutoUsePathEnum.ReleaseOffsetFromDefaultPathsLatestRelease, AlternatePathDefs.AutoUsePathEnum.IfCurrent };
                }
                else
                {
                    usePathEnums = new[] { AlternatePathDefs.AutoUsePathEnum.ReleaseOffsetFromDefaultPathsLatestRelease };
                }

                a_mo.AlternatePaths.FindAutoUsePaths(pathsToRelease, usePathEnums);

                foreach (AlternatePath path in pathsToRelease)
                {
                    long pathReleaseTicks;

                    if (path == a_mo.DefaultPath)
                    {
                        pathReleaseTicks = a_time;
                    }
                    else
                    {
                        pathReleaseTicks = Math.Max(defaultReleaseTicks + path.AutoUseReleaseOffsetTimeSpanTicks, a_time);
                    }

                    if (path == a_mo.CurrentPath)
                    {
                        currentPathReleased = true;
                    }

                    AddAlternatePathReleaseEventIfSatisfiable(m_simClock, pathReleaseTicks, a_mo, path, a_latestConstraint);
                }

                // Make sure the current path has been released.
                if (!currentPathReleased)
                {
                    AddAlternatePathReleaseEventIfSatisfiable(m_simClock, a_time, a_mo, a_mo.CurrentPath, a_latestConstraint);
                }
            }

            a_mo.CurrentPathNeedsToBeSetWhenFirstActivityIsScheduled = true;
        }
        else
        {
            //Only releasing the current path
            AddAlternatePathReleaseEventIfSatisfiable(m_simClock, a_time, a_mo, a_mo.CurrentPath, a_latestConstraint);
        }
    }

    /// <summary>
    /// Calculate an AlternatePath's release date and add an AlternatePathReleaseEvent to the event queue if the path is satisfiable.
    /// All alternate path release events are created from this function.
    /// </summary>
    /// <param name="a_simTicks">The simulation time.</param>
    /// <param name="a_originalReleaseTime">The clock date, or if the path has a delay from the primary path, a future date</param>
    /// <param name="a_mo">The ManufacturingOrder to create the event for.</param>
    /// <param name="a_path">The AlternatePath to add the event for.</param>
    /// <param name="a_latestConstraint">The last constraint that released the path.</param>
    private void AddAlternatePathReleaseEventIfSatisfiable(long a_simClock, long a_originalReleaseTime, ManufacturingOrder a_mo, AlternatePath a_path, InternalOperation.LatestConstraintEnum a_latestConstraint)
    {
        if (a_path.AdjustedPlantResourceEligibilitySets_IsSatisfiable()
            && a_simClock <= a_path.ValidityEndDateSimConstrained) //If we have passed the validity end date, there 
        {
            InternalOperation.LatestConstraintEnum constraint = a_latestConstraint;
            long releaseTicks = 0;
            if (a_mo.GetConstrainedReleaseTicks(out long constrainedReleaseTicks))
            {
                releaseTicks = constrainedReleaseTicks;
            }

            if (releaseTicks < a_originalReleaseTime)
            {
                releaseTicks = a_originalReleaseTime;
            }

            //Constrain by path available date start
            if (releaseTicks < a_path.ValidityStartDateSimConstrained)
            {
                releaseTicks = a_path.ValidityStartDateSimConstrained;
                constraint = InternalOperation.LatestConstraintEnum.AlternatePath;
            }

            if (releaseTicks > a_path.ValidityEndDateSimConstrained)
            {
                //Adjusted date is beyond the validity date, release at planning horizon
                releaseTicks = EndOfPlanningHorizon;
                constraint = InternalOperation.LatestConstraintEnum.AlternatePath;
            }

            //The only place a path should be released.
            AddEvent(new AlternatePathReleaseEvent(releaseTicks, a_mo, a_path, constraint));

            if (a_path.ValidityEndDateSimConstrained < GetPlanningHorizonEndTicks())
            {
                //No need to enforce validity after the planning horizon
                AddEvent(new AlternatePathValidityEndEvent(a_path.ValidityEndDateSimConstrained, a_mo, a_path));
            }
        }
    }

    private bool ReleaseAlternatePaths(ManufacturingOrder a_mo)
    {
        return ((m_activeSimulationType == SimulationType.Optimize && !a_mo.LockedToCurrentPathByOptimize_InPlantThatsNotBeingOptimized && !a_mo.LockedToCurrentPathByOptimize_ScheduledBeforeStartOfOptimize) 
                || (m_activeSimulationType == SimulationType.Expedite && a_mo.BeingExpedited)) && !a_mo.IsLockedToCurrentPath();
    }

    // !ALTERNATE_PATH!; Determine when the path can be released.
    /// <summary>
    /// First tries the Default or Current path. If neither of those are satisfiable, the first satisfiable path will be used.
    /// </summary>
    /// <param name="a_mo"></param>
    /// <returns></returns>
    private long DetermineLatestReleaseOfASatisfiablePath(ManufacturingOrder a_mo)
    {
        //Start with MO hold date
        long latestLeafReleaseDateTicks = a_mo.CalcPathsReleaseTicks(SimClock, SimClock);

        // Among the leaves of the default path, check for the latest leaf release date.
        AlternatePath pathToUseAsDefault = null;
        if (a_mo.DefaultPath.AdjustedPlantResourceEligibilitySets_IsSatisfiable())
        {
            pathToUseAsDefault = a_mo.DefaultPath;
        }
        else if (a_mo.CurrentPath != a_mo.DefaultPath && a_mo.CurrentPath.AdjustedPlantResourceEligibilitySets_IsSatisfiable())
        {
            pathToUseAsDefault = a_mo.CurrentPath;
        }
        else
        {
            for (int pathI = 0; pathI < a_mo.AlternatePaths.Count; ++pathI)
            {
                AlternatePath path = a_mo.AlternatePaths[pathI];
                if (path != a_mo.DefaultPath && path != a_mo.CurrentPath)
                {
                    if (path.AdjustedPlantResourceEligibilitySets_IsSatisfiable())
                    {
                        pathToUseAsDefault = path;
                        break;
                    }
                }
            }
        }

        if (pathToUseAsDefault == null)
        {
            // No need to localize this exception. I don't expect it to occur unless there is a programming bug.
            // Once throughly tested, this can be deleted.
            throw new CommonException("No usable schedulable alternate path was found in job " + BaseObject.EXTERNAL_ID);
        }

        AlternatePath.NodeCollection defaultPathsSchedulableLeaves = pathToUseAsDefault.EffectiveLeaves;
        for (int leafI = 0; leafI < defaultPathsSchedulableLeaves.Count; ++leafI)
        {
            AlternatePath.Node node = defaultPathsSchedulableLeaves[leafI];
            InternalOperation op = node.Operation;
            for (int actI = 0; actI < op.Activities.Count; ++actI)
            {
                if (op.ResourceRequirements.PrimaryResourceRequirement.DefaultResource != null && !op.ResourceRequirements.PrimaryResourceRequirement.DefaultResource_UseJITLimitTicks)
                {
                    // Only the default resource is releasable.
                    // Get the release date of the resource/activity combination and set defaultReleaseTicks.
                    Resource res = (Resource)op.ResourceRequirements.PrimaryResourceRequirement.DefaultResource;
                    InternalActivity act = op.Activities.GetByIndex(actI);
                    long actReleaseDate = ReleaseRuleCaculator.CalculateActivityReleaseDateForReleaseRuleJobRelease(res, act, m_activeOptimizeSettings, ClockDate, m_productRuleManager);
                    latestLeafReleaseDateTicks = Math.Max(latestLeafReleaseDateTicks, actReleaseDate);
                }
                else
                {
                    // In this case, the latest release is the latest among all eligible resources, so they're all tested.

                    // The code in this block work with and without a default resource.
                    // bool defaultResWithUseJITLimitTicks below is used to handle the case where DefaultResource is being used.
                    bool defaultResWithUseJITLimitTicks = op.ResourceRequirements.PrimaryResourceRequirement.DefaultResource != null && op.ResourceRequirements.PrimaryResourceRequirement.DefaultResource_UseJITLimitTicks;
                    // Determine the release date of each activity on each eligible primary resource.
                    PlantResourceEligibilitySet pres = op.AlternatePathNode.ResReqsMasterEligibilitySet[op.ResourceRequirements.PrimaryResourceRequirementIndex];
                    SortedDictionary<BaseId, EligibleResourceSet>.Enumerator plantEtr = pres.GetEnumerator();
                    while (plantEtr.MoveNext())
                    {
                        EligibleResourceSet ers = plantEtr.Current.Value;
                        for (int resI = 0; resI < ers.Count; ++resI)
                        {
                            InternalResource res = ers[resI];
                            InternalActivity act = op.Activities.GetByIndex(actI);
                            long actReleaseDate = ReleaseRuleCaculator.CalculateActivityReleaseDateForReleaseRuleJobRelease(res, act, m_activeOptimizeSettings, ClockDate, m_productRuleManager);
                            if (defaultResWithUseJITLimitTicks && res != op.ResourceRequirements.PrimaryResourceRequirement.DefaultResource)
                            {
                                // Default resource is set and other resource are also eligible. 
                                // Non-default resources are offset by this additional amount.
                                actReleaseDate += op.ResourceRequirements.PrimaryResourceRequirement.DefaultResource_JITLimitTicks;
                            }

                            latestLeafReleaseDateTicks = Math.Max(latestLeafReleaseDateTicks, actReleaseDate);
                        }
                    }
                }
            }
        }

        return latestLeafReleaseDateTicks;
    }

    /// <summary>
    /// Set to true to indicate that the SingleTasking resource requirement has been reserved for use by a resource requirment
    /// while attempting to schedule an activity.
    /// </summary>
    /// <param name="res"></param>
    private static void SetReservation(Resource a_res)
    {
        if (a_res.CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
        {
            a_res.ReservedForResourceRequirement = true;
        }
    }

    /// <summary>
    /// Clear the ReservedForResourceRequirement flag of all the affected resources
    /// </summary>
    /// <param name="a_reservedResources"></param>
    private static void ClearResourceReservationsOnly(ResourceSatiator[] a_reservedResources)
    {
        for (int i = 0; i < a_reservedResources.Length; ++i)
        {
            ResourceSatiator rs = a_reservedResources[i];
            if (rs != null)
            {
                Resource res = a_reservedResources[i].Resource;
                res.ReservedForResourceRequirement = false;
            }
        }
    }

    /// <summary>
    /// Clear the BlockReservations of all the affected resources
    /// </summary>
    /// <param name="a_reservedResources"></param>
    private static void ClearUnscheduledHelperReservations(ResourceSatiator[] a_reservedResources)
    {
        for (int i = 0; i < a_reservedResources.Length; ++i)
        {
            ResourceSatiator rs = a_reservedResources[i];
            if (rs?.Reservation != null)
            {
                Resource res = a_reservedResources[i].Resource;
                //A helper was reserved, but we are not scheduling it. Remove all helper reservations
                res.RemoveBlockReservation(rs.Reservation);
            }
        }
    }

    /// <summary>
    /// Remove ReservedForResourceRequirement flag and block reservations.
    /// </summary>
    /// <param name="reservedResources"></param>
    private static void ClearReservations(ResourceSatiator[] a_reservedResources, bool a_failedToSched)
    {
        ClearResourceReservationsOnly(a_reservedResources);
        if (a_failedToSched)
        {
            for (int i = 0; i < a_reservedResources.Length; ++i)
            {
                ResourceSatiator rs = a_reservedResources[i];
                if (rs != null)
                {
                    Resource res = a_reservedResources[i].Resource;

                    // [USAGE_CODE] ClearReservations(): Remove a ResourceSatiator's BlockReservation. An array of ResourceSatiator is being cleared here.
                    if (rs.Reservation != null)
                    {
                        res.RemoveBlockReservation(rs.Reservation);
                    }
                }
            }
        }
    }

    private void ClearFutureReservations(InternalActivity a_act)
    {
        if (!m_resRvns.Empty)
        {
            for (int blockIdx = 0; blockIdx < a_act.Batch.BlockCount; blockIdx++)
            {
                if (a_act.Batch.BlockAtIndex(blockIdx) is ResourceBlock block)
                {
                    m_resRvns.ActivityScheduled(block.ScheduledResource, a_act);
                }
                //Possibly no capacity to schedule
            }
        }
    }

    #region Simulation event handlers

    /// <summary>
    /// </summary>
    /// <param name="a_releaseTicks">The time to release the operation. This should be the simulation clock.</param>
    /// <param name="a_readyOperation">The operation that's ready.</param>
    /// <param name="a_latestConstraintReleaseTicks">
    /// This can be prior to the simulation clock and indicates the earliest the operation could have been released by its constraints. For instance, although
    /// constraints might have been released at the Clock, the start of optimize can force an operation's release to be much later.
    /// </param>
    /// <param name="a_latestConstraint">The reason for the operation was constrained to this time.</param>
    /// <param name="a_constraint">A reference to the latest constraint. Used to extract additional information for examination by the user. </param>
    private bool OperationReleaseCheckAndHandling(long a_releaseTicks, BaseOperation a_readyOperation, long a_latestConstraintReleaseTicks, InternalOperation.LatestConstraintEnum a_latestConstraint, object a_constraint)
    {
        ResourceOperation ro = (ResourceOperation)a_readyOperation;
        if (a_latestConstraintReleaseTicks > ro.LatestConstraintTicks)
        {
            SetLatestConstraint(a_latestConstraintReleaseTicks, a_latestConstraint, a_readyOperation, a_constraint);
        }

        bool released = ro.Released;
        if (released)
        {
            if (!a_readyOperation.OperationReadyProcessed)
            {
                a_readyOperation.OperationReadyProcessed = true;

                if (ro.IsNotFinishedAndNotOmitted &&
                    !ro.Scheduled)
                {
                    for (int activityI = 0; activityI < ro.Activities.Count; ++activityI)
                    {
                        InternalActivity activity = ro.Activities.GetByIndex(activityI);

                        if (!activity.Finished)
                        {
                            if (m_activeSimulationType == SimulationType.TimeAdjustment ||
                                m_activeSimulationType == SimulationType.ClockAdvance ||
                                m_activeSimulationType == SimulationType.Move ||
                                m_activeSimulationType == SimulationType.MoveAndExpedite ||
                                m_activeSimulationType == SimulationType.Expedite ||
                                m_activeSimulationType == SimulationType.ConstraintsChangeAdjustment)
                            {
                                if (activity.Released)
                                {
                                    AddActivityReleasedEvent(activity, a_releaseTicks);
                                }
                            }
                            else
                            {
                                bool actReleased = activity.Released; // This can change if the event below is added.

                                if (m_activeSimulationType == SimulationType.Optimize) // Could be compress or related to scheduling a single job.
                                {
                                    if (!activity.Sequenced) // Whether the activity was originally scheduled before the optimize start time.
                                    {
                                        if (a_releaseTicks < m_optimizeDefaultStartTicks)
                                        {
                                            // The activity needs to be constrained to the start of the optimize time.
                                            if (!activity.WaitForOptimizationReleaseEvent)
                                            {
                                                actReleased = false;
                                                activity.WaitForOptimizationReleaseEvent = true;
                                                long earliestOpReleaseTicks = ro.GetEarliestDepartmentalEndSpan(m_simStartTime);
                                                long earliestReleaseTicks = Math.Max(a_releaseTicks, earliestOpReleaseTicks);
                                                OptimizationReleaseEvent ore = new (earliestReleaseTicks, activity);
                                                AddEvent(ore);
                                            }
                                        }
                                    }
                                }

                                if (actReleased)
                                {
                                    ActivityReleased(activity, a_releaseTicks);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (a_readyOperation.AlternatePathNode.Successors.Count > 0)
                    {
                        AddPredecessorAvailableEvent(ro, a_releaseTicks, PredecessorReadyType.BlockStartAndEndCalculated); //*LRH*P1 Use the maximum of the time passed in and the time that the operation is scheduled to complete plus any transfer time. The operation may have become ready at the beggining of the simulation thanks to the operation being finished.
                    }
                }
            }
        }

        return released;
    }

    private void SetLatestConstraint(long a_releaseTicks, InternalOperation.LatestConstraintEnum a_latestConstraint, BaseOperation baseOperation, object a_constraint)
    {
        if (baseOperation is InternalOperation)
        {
            ResourceOperation op = (ResourceOperation)baseOperation;

            switch (a_latestConstraint)
            {
                case InternalOperation.LatestConstraintEnum.Unknown:
                    op.SetLatestConstraintToUnknownConstraint(Clock, a_releaseTicks);
                    break;

                case InternalOperation.LatestConstraintEnum.Clock:
                    op.SetLatestConstraintToClock(Clock);
                    break;

                case InternalOperation.LatestConstraintEnum.ManufacturingOrderRelease:
                    op.SetLatestConstraintToMOReleaseDate(Clock);
                    break;

                case InternalOperation.LatestConstraintEnum.PredecessorOperation:
                    if (a_constraint != null)
                    {
                        BaseOperation cop = (BaseOperation)a_constraint;
                        op.SetLatestConstraintPredecessorOp(Clock, a_releaseTicks, cop);
                    }
                    else
                    {
                        op.SetLatestConstraint(a_releaseTicks, InternalOperation.LatestConstraintEnum.Unknown);
                    }

                    break;

                case InternalOperation.LatestConstraintEnum.MaterialRequirement:
                    MaterialRequirement mr = (MaterialRequirement)a_constraint;
                    op.SetLatestConstraintToMaterialRequirement(Clock, a_releaseTicks, mr);
                    break;

                case InternalOperation.LatestConstraintEnum.PredecessorManufacturingOrder:
                    op.SetLatestConstraintToPredecessorMO(Clock, a_releaseTicks);
                    break;

                case InternalOperation.LatestConstraintEnum.JobHoldDate:
                    op.SetLatestConstraintToJobHoldReleaseDate(a_releaseTicks);
                    break;

                case InternalOperation.LatestConstraintEnum.ManufacturingOrderHoldDate:
                    op.SetLatestConstraintToMOHoldReleaseDate(a_releaseTicks);
                    break;

                case InternalOperation.LatestConstraintEnum.AlternatePath:
                    op.SetLatestConstraint(a_releaseTicks, InternalOperation.LatestConstraintEnum.AlternatePath);
                    break;

                case InternalOperation.LatestConstraintEnum.AddIn:
                    string constraintName = (string)a_constraint;
                    op.SetLatestConstraintToAddIn(a_releaseTicks, constraintName);
                    break;

                case InternalOperation.LatestConstraintEnum.ManufacturingOrderBatch:
                    op.SetLatestConstraint(a_releaseTicks, InternalOperation.LatestConstraintEnum.ManufacturingOrderBatch);
                    break;

                case InternalOperation.LatestConstraintEnum.TransferSpan:
                    op.SetLatestConstraint(a_releaseTicks, InternalOperation.LatestConstraintEnum.TransferSpan);
                    break;

                case InternalOperation.LatestConstraintEnum.FinishTime:
                    op.SetLatestConstraint(a_releaseTicks, InternalOperation.LatestConstraintEnum.FinishTime);
                    break;

                case InternalOperation.LatestConstraintEnum.OperationHoldDate:
                    op.SetLatestConstraint(a_releaseTicks, InternalOperation.LatestConstraintEnum.OperationHoldDate);
                    break;
                
                case InternalOperation.LatestConstraintEnum.StorageCapacity:
                    op.SetLatestConstraint(a_releaseTicks, InternalOperation.LatestConstraintEnum.StorageCapacity);
                    break;
                
                case InternalOperation.LatestConstraintEnum.StorageConnector:
                    op.SetLatestConstraint(a_releaseTicks, InternalOperation.LatestConstraintEnum.StorageConnector);
                    break;
                
                case InternalOperation.LatestConstraintEnum.LotCode:
                    op.SetLatestConstraintToLotCode(a_releaseTicks);
                    break;

                default:
                    throw new CommonException("Bad InternalOperation.LatestConstraintEnum value.");
            }
        }
    }



    #region Activity Release to Dispatcher Stuff.

    /// <summary>
    /// All activity releases initially pass through this function, though under some circumstances a re-release may short circuit this call.
    /// The release of an activity causes it to be added to the dispatchers of the primary
    /// resources that are able to manufacture it, but may be delayed by factors such as the Optimize JIT days and  Resource HeadStartSpan.
    /// *** The sequence of when corresponding features are taken into consideration is mirrored by InsertContinuousSuccessorRR ***
    /// </summary>
    /// <param name="activity">The activity that is being released. Its activity. Release flag must be true at this point to ensure that it is obeying all its constraints.</param>
    private void ActivityReleased(BaseActivity a_baseActivity, long a_time)
    {
        #if TEST
            if (!a_baseActivity.Released)
            {
                bool error = true;

                InternalActivity ia = a_baseActivity as InternalActivity;

                if (ia != null)
                {
                    if (ia.InProduction())
                    {
                        error = false;
                    }
                }

                if (error)
                {
                    throw new SimulationFailureException("This activity has not been released. ActivityReleased() called against non-released activity.");
                }
            }
        #endif

        if (a_baseActivity is InternalActivity)
        {
            InternalActivity activity = (InternalActivity)a_baseActivity;

            if (activity.ActivityReleaseExecuted 
                || activity.Operation.AlternatePathNode.HasAnotherPathScheduled) //This is possible if the release event was created, then the path was removed and dispatcher cleared. This event may still be in the event queue.
            {
                return;
            }

            activity.ActivityReleaseExecuted = true;

            if (activity.PostProcessingStateWithNoResourceUsage)
            {
                long remainingPostProcessingTime = activity.CalculatePostProcessingSpan(null).TimeSpanTicks;
                // This activity can be considered to start now and complete after post processing.
                // It may be possible to release successor operations and MOs after the post processing time.

                //TODO SIM add code here to create a placeholder batch for nothing but post processing time.

                activity.ScheduledOnlyForPostProcessingTime = true;
                activity.ScheduledStartTimePostProcessingNoResources = a_time;
                activity.ScheduledFinishTimePostProcessingNoResources = a_time + remainingPostProcessingTime;
                // Set scheduled start date in a special variable in InternalActivity especially for this case.
                // Set the scheduled finish date in a special variable in InternalActivity especially for this case.
                SequentialStateHandlingForAPostProcessingActivity(activity);
                PredOpReady_ProcessSucActivitiesAndSucMOs(activity, null, PredecessorReadyType.BlockStartAndEndCalculated);
            }
            else if (activity.CleanStateWithNoResourceUsage)
            {
                long remainingCleanTime = activity.CalculateCleanSpan(null).TimeSpanTicks;
                // This activity can be considered to start now and complete after post processing.
                // It may be possible to release successor operations and MOs after the post processing time.

                //TODO SIM add code here to create a placeholder batch for nothing but clean time.

                activity.ScheduledOnlyForCleanTime = true;
                activity.ScheduledStartTimeCleanNoResources = a_time;
                activity.ScheduledFinishTimeCleanNoResources = a_time + remainingCleanTime;
                // Set scheduled start date in a special variable in InternalActivity especially for this case.
                // Set the scheduled finish date in a special variable in InternalActivity especially for this case.
                SequentialStateHandlingForAPostProcessingActivity(activity);
                PredOpReady_ProcessSucActivitiesAndSucMOs(activity, null, PredecessorReadyType.BlockStartAndEndCalculated);
            }
            else
            {
                if (activity.OpOrPredOpBeingMoved)
                {
                    Resource primaryResource;
                    if (m_activeSimulationType == SimulationType.Move && activity.GetMoveResource(activity.Operation.ResourceRequirements.PrimaryResourceRequirementIndex, out primaryResource))
                    {
                        AddToDispatcherIfPossible(a_time, primaryResource, activity, 0);
                    }
                    else if (activity.ResourceReservationMade)
                    {
                        // Max Delay insertion has determined where the primary should be scheduled.
                        AddToDispatcherIfPossible(a_time, activity.GetReservationForRR(activity.Operation.ResourceRequirements.PrimaryResourceRequirementIndex).ReservedResource, activity, 0);
                    }
                    else
                    {
                        // A move and expedite is being performed so add this successor of the moved operation to all the
                        // dispatchers it is eligible to be scheduled. It get picked up by the first resource that is able
                        // to process it.
                        AddToAllEligibleDispatchers(a_time, activity);
                    }
                }
                else if (activity.BeingMoved)
                {
                    if (activity.GetMoveResource(activity.Operation.ResourceRequirements.PrimaryResourceRequirementIndex, out Resource primaryResource))
                    {
                        AddToDispatcherIfPossible(a_time, primaryResource, activity, 0);
                    }
                }
                else if (activity.ResourceReservationMade)
                {
                    // Max Delay insertion has determined where this should be scheduled.
                    AddToDispatcherIfPossible(a_time, activity.GetReservationForRR(activity.Operation.ResourceRequirements.PrimaryResourceRequirementIndex).ReservedResource, activity, 0);
                }
                else if (activity.Sequential != null)
                {
                    InternalActivity.SequentialState ss = activity.Sequential[activity.Operation.ResourceRequirements.PrimaryResourceRequirementIndex];
                    BaseResource baseResource = ss.SequencedSimulationResource;
                    if (baseResource is Resource)
                    {
                        Resource resource = (Resource)baseResource;
                        AddToDispatcherIfPossible(a_time, resource, activity, 0);
                    }
                }
                else
                {
                    // It shouldn't be released to all dispatchers. Only the resource it was originally scheduled on. It should be an InternalActivity.SequentialState
                    AddToAllEligibleDispatchers(a_time, activity);
                }
            }
        }
    }

    /// <summary>
    /// Add the activity to all the dispatchers of the resources that are eligible.
    /// </summary>
    /// <param name="time">Current simulation time.</param>
    /// <param name="activity"></param>
    private void AddToAllEligibleDispatchers(long a_simClock, InternalActivity a_act)
    {
        Resource primaryResourceRequirementLock;

        if (a_act.m_moBatchLockedResource != null)
        {
            AddToDispatcherIfPossible(a_simClock, a_act.m_moBatchLockedResource, a_act, 0);
        }
        else if ((primaryResourceRequirementLock = a_act.PrimaryResourceRequirementLock()) != null)
        {
            //***LRH what if the resource is inactive
            AddToDispatcherIfPossible(a_simClock, primaryResourceRequirementLock, a_act, 0);
        }
        else if (a_act.PrimaryDefaultResource != null)
        {
            Resource defaultRes = a_act.PrimaryDefaultResource as Resource;
            AddToDispatcherIfPossible(a_simClock, defaultRes, a_act, 0);
            if (a_act.PrimaryResourceRequirement.DefaultResource_UseJITLimitTicks)
            {
                long releaseTicks = a_act.PrimaryResourceRequirement.DefaultResource_CalcJimLimitRelease(a_act);
                //If the adjusted release is before the clock, then there is no delay. 
                //Otherwise use the default resource JIT adjustment
                long releaseDelayFromDefaultJit = a_simClock < releaseTicks ? a_act.PrimaryResourceRequirement.DefaultResource_JITLimitTicks : 0;

                AddPRESToDispatchers(a_simClock, a_act, a_act.PrimaryDefaultResource, releaseDelayFromDefaultJit);
            }
        }
        else
        {
            AddPRESToDispatchers(a_simClock, a_act, null, 0);
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="a_simClock"></param>
    /// <param name="a_op"></param>
    /// <param name="a_excludeRes">If you've already added the default resource to the dispatchers, pass it in here to prevent it from being added again.</param>
    /// <param name="a_delayReleaseTicks"></param>
    private void AddPRESToDispatchers(long a_simClock, InternalActivity a_act, BaseResource a_excludeRes, long a_delayReleaseTicks)
    {
        PlantResourceEligibilitySet pres = a_act.ResReqsEligibilityNarrowedDuringSimulation[a_act.Operation.ResourceRequirements.PrimaryResourceRequirementIndex];
        InternalOperation io = a_act.Operation;
        Cell cell = io.GetPredecessorCell();

        SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = pres.GetEnumerator();
        while (ersEtr.MoveNext())
        {
            EligibleResourceSet eligibleResourceSet = ersEtr.Current.Value;

            bool constrainToCell = false;
            if (cell != null)
            {
                for (int eligResI = 0; eligResI < eligibleResourceSet.Count; ++eligResI)
                {
                    InternalResource eligRes = eligibleResourceSet[eligResI];
                    if (eligRes.Cell == cell)
                    {
                        constrainToCell = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < eligibleResourceSet.Count; ++i)
            {
                InternalResource eligibleResource = eligibleResourceSet[i];
                if (eligibleResource != a_excludeRes)
                {
                    if (constrainToCell)
                    {
                        if (eligibleResource.Cell == cell)
                        {
                            AddToDispatcherIfPossible(a_simClock, (Resource)eligibleResource, a_act, a_delayReleaseTicks);
                        }
                    }
                    else
                    {
                        AddToDispatcherIfPossible(a_simClock, (Resource)eligibleResource, a_act, a_delayReleaseTicks);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Jobs that violated the connected resource feature.
    /// If an activity in an unscheduled job is locked to a resource and the resource connector feature is in use, the job may fail to schedule. Specifically if the predecessor operation attempts to schedule
    /// on a resource with connectors that isn't connected to the resource the successor is locked to the job will fail to schedule.
    /// Resolution:
    /// When a job is being scheduled, the scheduler will detect this condition and clear the activity's resource lock. But the job may still to schedule if the job had other locks with this problem. Each
    /// time an attempt to schedule the job is made another lock will be cleared until all of the problem locks have been cleared and the job schedules. It's not possible to clear all the locks at the start
    /// of scheduling since it's not known in advance which resource the predecessor operation will be scheduled on; the predecessor resource determines which connected resources the successor can schedule
    /// on.
    /// When a job fails to schedule for this reason the message below will appear on the Scheduling tab of the job window.
    /// The job failed to schedule because an activity was locked to a resource that it can’t schedule on anymore. An attempt to schedule it was made on a resource that wasn’t connected to the resource the
    /// activity was locked to. The lock on the activity has been cleared. The job might be schedulable now. If additional activities have this problem, they will be cleared one by one as attempts to
    /// schedule the job are made. Multiple attempts to schedule the job may be necessary to clear all the locks.
    /// </summary>
    private readonly HashSet<Job> m_attemptToScheduleToNonConnectedRes = new ();

    /// <summary>
    /// Add an activity to a resource's dispatcher. Resource HeadStart spans and the JIT start date of the activity are taken into consideration
    /// as to when the activity actually enters the dispatcher.
    /// If there are resource connectors between the predecessor and successor operation, the activity will only be added if there's a connection
    /// from the predecessor resource to this resource.
    /// </summary>
    /// <param name="time">Current simulation time.</param>
    /// <param name="earliestReleaseTicks">The earliest the activity can be released on this resource.</param>
    /// <param name="resource">The resource whose dispatcher you want to add this activity to.</param>
    /// <param name="activity"></param>
    /// <param name="a_delay">Delay the release of the activity by this length of time beyond which it would normally be released to the resources activity queue.</param>
    private void AddToDispatcherIfPossible(long a_simTime, Resource a_res, InternalActivity a_act, long a_delayReleaseTicks)
    {
        long adjustedReleaseTicks = 0;
        long earliestReleaseTicks = 0;

        //This operation has been released which means that all predecessors have been scheduled
        if (a_act.Operation.PotentialConnectorsFromPredecessors.Count > 0)
        {
            bool connectorFound = false;
            long latestConnectorAvailableFromPredecessors = 0;
            foreach (ResourceConnectorConstraintInfo connectorConstraint in a_act.Operation.PotentialConnectorsFromPredecessors.Values)
            {
                long earliestTransferTime = long.MaxValue;
                foreach (ResourceConnector connector in connectorConstraint.ResourceConnectors)
                {
                    if (connector.ToResources.ContainsKey(a_res.Id))
                    {
                        if (!connector.AllowConcurrentUse)
                        {
                            if (connectorConstraint.PredecessorReleaseTicks != PTDateTime.InvalidDateTime.Ticks)
                            {
                                earliestTransferTime = Math.Min(earliestTransferTime, connectorConstraint.PredecessorReleaseTicks + connector.TransitTicks);

                                if (connector.InUse)
                                {
                                    //This connector can't be used right now. We need to determine when it would be available
                                    long availableDate = Math.Max(connectorConstraint.PredecessorReleaseTicks + connector.TransitTicks, connector.ReleaseTicks);
                                    earliestTransferTime = Math.Min(earliestTransferTime, availableDate);
                                }
                            }
                            else
                            {
                                //This should not happen
                                DebugException.ThrowInDebug("An operation has resource connectors that were not initialized with a release date when the operation was released");
                            }
                        }
                        else
                        {
                            earliestTransferTime = Math.Min(earliestTransferTime, connectorConstraint.PredecessorReleaseTicks + connector.TransitTicks);
                        }

                        connectorFound = true;
                    }
                }

                latestConnectorAvailableFromPredecessors = Math.Max(latestConnectorAvailableFromPredecessors, earliestTransferTime);
            }

            if (connectorFound)
            {
                earliestReleaseTicks = latestConnectorAvailableFromPredecessors;
                if (a_simTime > earliestReleaseTicks)
                {
                    // Adding the transit time is not necessary since the transit time has already elapsed since the connector operation has completed. 
                    // A different predecessor operation has likely caused this operation to be relased past the transit time.
                    earliestReleaseTicks = a_simTime;
                }
            }
            else
            {
                //Connectors are set up, but there is no connector for this resource
                if (m_activeSimulationType is SimulationType.Optimize or SimulationType.Expedite)
                {
                    // The job isn't scheduled
                    a_act.Lock(false);
                    m_attemptToScheduleToNonConnectedRes.Add(a_act.Job);
                    return;
                }
            }
        }

        if (m_activeSimulationType == SimulationType.Optimize)
        {
            if (a_act.PlantNotIncludedInSimulate)
            {
                // An optimize is being performed, but this activity is being excluded from the optimize.
                // For instnace, a single plant might be optimized and this activity is in a
                // different plant. So it can be released now.
                adjustedReleaseTicks = a_simTime;
            }
            else if (!a_act.InProduction())
            {
                long bufferReleaseTime = 0;
                long earliestReleaseTime = 0;
                long resSimStartTime = m_simStartTime.GetTimeForResource(a_res);

                //Release with Buffers
                if (!a_act.SuppressReleaseDateAdjustments)
                {
                    bufferReleaseTime = ReleaseRuleCaculator.CalculateActivityReleaseDateForReleaseRuleJobRelease(a_res, a_act, m_activeOptimizeSettings, ClockDate, m_productRuleManager);
                    earliestReleaseTime = resSimStartTime;
                }
                else if (a_act.Anchored && a_act.ScheduledStartTicksBeforeSimulate > resSimStartTime && a_simTime < resSimStartTime)
                {
                    // The activity is anchored but is outside of the resource's simulation start time.
                    // It might be originally been anchored within the simulation start time but drifted out.
                    // Once it has drifted out of the simulation start, its earliest start during an optimize
                    // is limited by the resource's simulation start time (either the frozen, stable, or department frozen span).
                    earliestReleaseTime = resSimStartTime;
                }

                adjustedReleaseTicks = Math.Max(bufferReleaseTime, earliestReleaseTime);
                adjustedReleaseTicks += a_delayReleaseTicks;
            }
        }
        else if (ActiveSimulationType == SimulationType.Expedite) // || ActiveSimulationType == SimulationType.MoveAndExpedite)
        {
            if (a_act.ManufacturingOrder.BeingExpedited)
            {
                long resSimTime = m_expediteStartTime.GetTimeForResource(a_res);
                if (resSimTime > adjustedReleaseTicks)
                {
                    adjustedReleaseTicks = resSimTime;
                }
            }
        }
        else if (IsCompressTimeAdjustment)
        {
            if (a_act.CompressLimitedByTicks > 0)
            {
                // A compression is actually being performed. For whatever reason when Compress was written TimeAdjustment was set as the simulation type.
                // CompressLimitedTicks is only set during a compression.
                adjustedReleaseTicks = a_act.CompressLimitedByTicks;
            }
        }

        adjustedReleaseTicks = Math.Max(adjustedReleaseTicks, earliestReleaseTicks);

        long? adjustedReleaseByCustomization = m_extensionController.AdjustActivityRelease(this, a_act, adjustedReleaseTicks, ActiveSimulationType, SimulationTransmission);
        if (adjustedReleaseByCustomization.HasValue)
        {
            adjustedReleaseTicks = adjustedReleaseByCustomization.Value;
        }

        if (adjustedReleaseTicks <= a_simTime)
        {
            AddToDispatcherHelper(a_simTime, a_res, a_act);
        }
        else
        {
            // When processing the release of an activity to a resource the specifics of the resource might require the release be delayed.
            // For instance the following could cause the need to delay the release: JIT release, regular releases head start span, and usage of the department EndOfFronzenSpan which also affects the EndOfStableSpan.
            DelayedReleaseToResourceEvent jitRelease = new (adjustedReleaseTicks, a_act, a_res);
            AddEvent(jitRelease);
        }
    }
    
    internal void RemoveRemainingActivitiesFromDispatchers()
    {
        foreach (Resource res in PlantManager.GetResourceList())
        {
            res.ActiveDispatcher.Clear();
        }
    }

    // !ALTERNATE_PATH!; ---TODO--- Put this class in it's own file. You might also need to move some of the overlap code into here. Can't do this until I can perform a get latest.
    internal class ReleaseRuleCaculator
    {
        /// <summary>
        /// If the  release rule is releaseRules.JobRelease use this function to determine an activity's release date on a resource.
        /// </summary>
        internal static long CalculateActivityReleaseDateForReleaseRuleJobRelease(InternalResource a_res, InternalActivity a_act, OptimizeSettings a_activeOptimizeSettings, DateTime a_clockDate, ProductRuleManager a_prs)
        {
            long headStartSpanTicks = a_res.HeadStartSpan.Ticks;

            if (a_act.Operation.Products.PrimaryProduct is Product primaryProduct)
            {
                ProductRule pr = a_prs.GetProductRule(a_res.Id, primaryProduct.Item.Id, a_act.Operation.ProductCode);
                if (pr != null)
                {
                    if (pr.UseHeadStartSpan)
                    {
                        headStartSpanTicks = pr.HeadStartSpanTicks;
                    }
                }
            }

            headStartSpanTicks += a_activeOptimizeSettings.JITSlackTicks;

            ActivityResourceBufferInfo resourceBufferInfo = a_act.BufferInfo.GetResourceInfo(a_res.Id);

            if (a_activeOptimizeSettings.CombineHeadStartBuffers)
            {
                headStartSpanTicks += a_act.Operation.SequenceHeadStartTicks;
                resourceBufferInfo.SequenceHeadStartWindowEndDate = resourceBufferInfo.DbrJitStartDate;

                if (a_activeOptimizeSettings.UseResourceCapacityForHeadstart)
                {
                    //TODO: Get the adjusted headstart date from the resource
                    Resource.FindStartFromEndResult adjustedHeadstart = a_res.GetStartOfBufferFromEndDate(a_clockDate.Ticks, resourceBufferInfo.DbrJitStartDate, headStartSpanTicks);
                    resourceBufferInfo.ReleaseDate = adjustedHeadstart.StartTicks;
                }
                else
                {
                    resourceBufferInfo.ReleaseDate = resourceBufferInfo.DbrJitStartDate - headStartSpanTicks;
                }
            }
            else
            {
                //We calculate the two separately. Start with the headstart end
                if (a_activeOptimizeSettings.UseResourceCapacityForHeadstart)
                {
                    //TODO: Get the adjusted headstart date from the resource
                    Resource.FindStartFromEndResult adjustedHeadstart = a_res.GetStartOfBufferFromEndDate(a_clockDate.Ticks, resourceBufferInfo.DbrJitStartDate, headStartSpanTicks);
                    resourceBufferInfo.SequenceHeadStartWindowEndDate = adjustedHeadstart.StartTicks;
                }
                else
                {
                    resourceBufferInfo.SequenceHeadStartWindowEndDate = resourceBufferInfo.DbrJitStartDate - headStartSpanTicks;
                }

                //Now calaculate again for the head start window
                headStartSpanTicks = a_act.Operation.SequenceHeadStartTicks;

                if (a_activeOptimizeSettings.UseResourceCapacityForHeadstart)
                {
                    //TODO: Get the adjusted headstart date from the resource
                    Resource.FindStartFromEndResult adjustedHeadstart = a_res.GetStartOfBufferFromEndDate(a_clockDate.Ticks, resourceBufferInfo.SequenceHeadStartWindowEndDate, headStartSpanTicks);
                    resourceBufferInfo.ReleaseDate = adjustedHeadstart.StartTicks;
                }
                else
                {
                    resourceBufferInfo.ReleaseDate = resourceBufferInfo.SequenceHeadStartWindowEndDate - headStartSpanTicks;
                }
            }

            //Apply the changes to the res Info for use in the dispatcher release
            a_act.BufferInfo.UpdateResourceInfo(a_res.Id, resourceBufferInfo);

            return resourceBufferInfo.ReleaseDate;
        }
    }

    /// <summary>
    /// During optimization simulations activities are constrained by the JIT start time. Many activities may end up being added to a resource's
    /// dispatcher through this event.
    /// </summary>
    /// <param name="jitReleaseEvent"></param>
    private void BufferReleaseEventReceived(EventBase a_nextEvent)
    {
        DelayedReleaseToResourceEvent bufferReleaseEvent = (DelayedReleaseToResourceEvent)a_nextEvent;

        if (!bufferReleaseEvent.Activity.SimScheduled)
        {
            AddToDispatcherHelper(bufferReleaseEvent.Time, (Resource)bufferReleaseEvent.Resource, bufferReleaseEvent.Activity);
        }
    }

    private void ReleaseToResourceEventReceived(EventBase a_nextEvent)
    {
        ReleaseToResourceEvent rre = (ReleaseToResourceEvent)a_nextEvent;
        AddToDispatcherHelper2((Resource)rre.Resource, rre.Activity); //We aren't using a sequence buffer here because it's a reservation
    }

    /// <summary>
    /// All additions to dispatchers must pass through this function. It maintains state information about the simulation.
    /// </summary>
    /// <param name="a_time"></param>
    /// <param name="a_res"></param>
    /// <param name="a_activity"></param>
    private void AddToDispatcherHelper(long a_time, Resource a_res, InternalActivity a_activity)
    {
        if (a_activity.Operation.OverlapResourceReleaseTimes != null)
        {
            //Check if activity is reserved and create and event to release it at the reservation date 
            //if it's in the future.
            foreach (ResourceReservation resourceReservation in a_res.Reservations)
            {
                //TODO: add lookup for better performance
                if (resourceReservation.m_ia == a_activity && resourceReservation.StartTicks > a_time)
                {
                    ReleaseToResourceEvent rtr = new(resourceReservation.StartTicks, a_activity, a_res);
                    AddEvent(rtr);
                    return;
                }
            }

            if (a_activity.Operation.OverlapResourceReleaseTimes.TryGetValue(a_res, out long overlapReleaseTime))
            {
                if (overlapReleaseTime > a_time)
                {
                    ReleaseToResourceEvent rtr = new(overlapReleaseTime, a_activity, a_res);
                    AddEvent(rtr);
                }
                else
                {
                    AddToDispatcherHelper2(a_res, a_activity);
                }
            }
            else
            {
                AddToDispatcherHelper2(a_res, a_activity);
            }
        }
        else
        {
            AddToDispatcherHelper2(a_res, a_activity);
        }
    }

    /// <summary>
    /// All additions to dispatchers must pass through this function. It maintains state information about the simulation.
    /// </summary>
    /// <param name="a_resource"></param>
    /// <param name="a_act"></param>
    private void AddToDispatcherHelper2(Resource a_resource, InternalActivity a_act)
    {
        if (!a_act.SimScheduled && !m_dataLimitReached)
        {
            if (a_resource.ActiveDispatcher.Contains(a_act))
            {
                //TODO: This is not ideal. We don't want to check here, but some storage events add the activity multiple times. 
                return;
            }

            // It's possible that the activity has already been scheduled. For example the activity
            // may be released later on some resources. If it ends up scheduled early on a resource it was released on early
            // then SimScheduled will already be true by the time it reaches this point for release to the later resources.
            // Examples of features that may cause later releases include HeadStartSpan and Operation Overlap.

            if (a_act.Operation.Split)
            {
                //We need to verify eligibility for this activity. It's possible that one of the activities is not eligible here
                if (!a_resource.IsCapableBasedOnMinMaxVolume(a_act, a_act.Operation.AutoSplitInfo, m_productRuleManager) || !a_resource.IsCapableBasedOnMinMaxQtys(a_act, a_act.Operation.AutoSplitInfo, m_productRuleManager))
                {
                    //A different activity is eligible on this resource
                    return;
                }
            }

            bool? canScheduleOnThisResource = m_extensionController.CanScheduleOnResource(this, a_resource, a_act, m_activeSimulationType, SimulationTransmission);
            if (canScheduleOnThisResource.HasValue && !canScheduleOnThisResource.Value)
            {
                //Customization has determined this activity should not be released on this resource
                return;
            }

            long sequenceHeadStartWindowEndDate = a_act.BufferInfo.GetResourceInfo(a_resource.Id).SequenceHeadStartWindowEndDate;

            if (sequenceHeadStartWindowEndDate > m_simClock)
            {
                //This event is in the future so we need to add an event to retry at the end of the window in case there is nothing else to schedule with
                //TODO: We could track this event on the activity and remove it if the activity schedules within the window
                AddEvent(new HeadStartWindowRetryEvent(sequenceHeadStartWindowEndDate, a_act));
            }

            a_resource.ActiveDispatcher.Add(a_act, sequenceHeadStartWindowEndDate);
        }
    }
    #endregion



    /// <summary>
    /// Used by MoveActivityTimeEventReceived to determine whether resources have been reserved.
    /// The only time this function might receive multiple events during a move is when multiple activities of a batch are being moved.
    /// Prevents activity.ReserveMoveResources from being called multiple times.
    /// </summary>
    private bool m_resourcesReserved; // [BATCH]

    /// <summary>
    /// A helper function of MoveActivityTimeEventReceived.
    /// </summary>
    private void ReserveMoveResources(InternalActivity a_act, long a_simClock, long a_reserveTicks)
    {
        if (!m_resourcesReserved)
        {
            m_resourcesReserved = true;
            SchedulableInfo undoReceive = m_undoReapplyMoveT != null ? m_undoReapplyMoveT.SI : null;
            ReserveMoveResources(a_act, a_simClock, a_reserveTicks);
        }
    }

    private void ReleaseLotCode(string a_lotCode, long a_releaseTime, Item a_sourceItem)
    {
        EligibleLot el = UsedAsEligibleLotsLotCodes.Get(a_lotCode);

        if (el != null)
        {
            foreach (BaseIdObject requirer in el)
            {
                if (requirer is MaterialRequirement mr)
                {
                    mr.WaitingOnEligibleLots = false;

                    if (mr.Operation.ManufacturingOrder.PathReleaseEventProcessed)
                    {
                        if (mr.Operation.Released)
                        {
                            OperationReleaseCheckAndHandling(a_releaseTime, mr.Operation, a_releaseTime, InternalOperation.LatestConstraintEnum.LotCode, mr);
                        }
                    }
                }
            }
        }
    }

    private void ReleaseAllEligibleLots()
    {
        Dictionary<string, EligibleLot>.Enumerator etr = UsedAsEligibleLotsLotCodes.GetEligibleLotsEnumerator();
        while (etr.MoveNext())
        {
            EligibleLot el = etr.Current.Value;

            foreach (BaseIdObject requirer in el)
            {
                if (requirer is MaterialRequirement mr)
                {
                    long materialConstraintTicks = 0;
                    if (mr.WaitingOnEligibleLots)
                    {
                        //Only set the latest constraint if this was a reason the op never scheduled
                        materialConstraintTicks = EndOfPlanningHorizon;
                        mr.WaitingOnEligibleLots = false;
                    }

                    if (mr.Operation.ManufacturingOrder.PathReleaseEventProcessed)
                    {
                        if (mr.Operation.Released)
                        {
                            OperationReleaseCheckAndHandling(EndOfPlanningHorizon, mr.Operation, materialConstraintTicks, InternalOperation.LatestConstraintEnum.MaterialRequirement, mr);
                        }
                    }
                }

            }
        }
    }

    private void ReleaseEligibleLots()
    {
        Dictionary<string, EligibleLot>.Enumerator etr = UsedAsEligibleLotsLotCodes.GetEligibleLotsEnumerator();
        while (etr.MoveNext())
        {
            EligibleLot el = etr.Current.Value;

            foreach (BaseIdObject requirer in el)
            {
                if (requirer is MaterialRequirement mr)
                {
                    mr.WaitingOnEligibleLots = false;
                }
            }
        }
    }

    /// <summary>
    /// Release MaterialRequirements whose required eligible lots are already available.
    /// This should be called before the simulation begins.
    /// </summary>
    private void ReleaseOnHandEligibleInventoryLots()
    {
        foreach (Warehouse wh in WarehouseManager)
        {
            foreach (Inventory inv in wh.Inventories)
            {
                foreach (Lot lot in inv.Lots)
                {
                    if (lot.Code != null)
                    {
                        if (UsedAsEligibleLotsLotCodes.Contains(lot.Code))
                        {
                            EligibleLot el = UsedAsEligibleLotsLotCodes.Get(lot.Code);
                            foreach (BaseIdObject requirer in el)
                            {
                                if (requirer is MaterialRequirement mr)
                                {
                                    mr.WaitingOnEligibleLots = false;
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    #endregion

    private void MoveReleaseProcessing(long a_time, InternalActivity a_activity)
    {
        if (a_activity.Released)
        {
            AddActivityReleasedEvent(a_activity, a_time);
        }
    }

    private bool MustSchedule(BaseOperation a_operation)
    {
        if (a_operation.IsFinishedOrOmitted ||
            a_operation.Scheduled)
        {
            return false;
        }

        return true;
    }

    private class MOKey
    {
        internal readonly string m_jobId;
        internal readonly string m_moId;

        internal MOKey(string a_jobId, string a_moId)
        {
            m_jobId = a_jobId;
            m_moId = a_moId;
        }
    }

    private class MOKeyComparer : IComparer, IHashCodeProvider
    {
        #region IComparer Members
        public int Compare(object a_x, object a_y)
        {
            MOKey k1 = (MOKey)a_x;
            MOKey k2 = (MOKey)a_y;

            int comparison = string.Compare(k1.m_jobId, k2.m_jobId);

            if (comparison != 0)
            {
                return comparison;
            }

            return string.Compare(k1.m_moId, k2.m_moId);
        }
        #endregion

        #region IHashCodeProvider Members
        public int GetHashCode(object a_obj)
        {
            MOKey key = (MOKey)a_obj;
            string combo = string.Format("JOBID={0};MOID={1}", key.m_jobId, key.m_moId);
            return combo.GetHashCode();
        }
        #endregion
    }
    #endregion Simulation Algorithm

    #region Post simulation & test procedures
    #region Post simulation activation area. Anchor and Lock activities in the frozen zone.
    //TODO: This should be moved to a higher level outside of simulate
    private void PostSimulationNotification()
    {
        if (m_scenarioOptions.LockInFrozenZone || m_scenarioOptions.AnchorInFrozenZone)
        {
            JobManager.LockAndAnchorBefore(OptimizeSettings.ETimePoints.EndOfFrozenZone, m_scenarioOptions, new ScenarioDataChanges());
        }
    }
    #endregion

    #region Debug
    #if TEST
        void SimDebugSetup()
        {
#if TEST

            _adeAddedEvents = new List<EventBase>();
            _adePlayedEvents = new List<EventBase>();

#endif
        }

        void SimDebugCleanup()
        {
#if TEST
            _adeAddedEvents = null;
            _adePlayedEvents = null;
#endif
        }

#if TEST
        List<EventBase> _adeAddedEvents;
        List<EventBase> _adePlayedEvents;
#endif

    #endif
    #endregion

    #region Server CheckPoints for scenario
    #if TEST
        CheckSumSDSet _csSDs;
        int _appliedSimulates;
        bool _csPrimarySD = true;

        class CheckSumSDSet
        {
            internal CheckSumSDSet(ScenarioDetail aOriginalSD)
            {
                _originalSD = aOriginalSD;
                _csSDs = new List<ChecksumSD>();
                _receivedTransmissions = new List<string>();
                _exceptionNames = new List<string>();
                _exceptionText = new List<string>();
            }

            List<ChecksumSD> _csSDs;
            ScenarioDetail _originalSD;

            List<string> _receivedTransmissions;
            List<string> _exceptionNames;
            List<string> _exceptionText;

            internal void Receive(ScenarioBaseT t)
            {
                try
                {
                    for (int i = _csSDs.Count - 1; i >= 0; --i)
                    {
                        bool b;
                        try
                        {
                            b = _csSDs[i].Receive(t);
                            ValidateChecksums(i, b, t);
                        }
                        catch (Exception e)
                        {
                            _exceptionNames.Add(e.GetType().Name);
                            _exceptionText.Add(e.ToString());
                        }
                    }
                }
            }

            internal void Receive(CtpT ctpT, Scenario scenarioToSendResultTo)
            {
                try
                {
                    for (int i = _csSDs.Count - 1; i >= 0; --i)
                    {
                        bool b = false;
                        try
                        {
                            b = _csSDs[i].Receive(ctpT, scenarioToSendResultTo);
                        }
                        ValidateChecksums(i, b, ctpT);
                    }
                }
            }

            internal void Receive(ScenarioClockAdvanceT scenarioClockAdvanceT)
            {
                try
                {
                    for (int i = _csSDs.Count - 1; i >= 0; --i)
                    {
                        bool b = _csSDs[i].Receive(scenarioClockAdvanceT);
                        ValidateChecksums(i, b, scenarioClockAdvanceT);
                    }
                }
            }

            internal void Receive(ScenarioTouchT touchT)
            {
                try
                {
                    for (int i = _csSDs.Count - 1; i >= 0; --i)
                    {
                        bool b = _csSDs[i].Receive(touchT);
                        ValidateChecksums(i, b, touchT);
                    }
                }
            }

            internal void ValidateChecksums(int sdIdx, bool recreate, Broadcasting.Transmission t)
            {
                if (t != null)
                {
                    _receivedTransmissions.Add(t.GetType().Name);
                }

                // Validate
                ChecksumValues originalCSV = _originalSD.CalculateChecksums();
                ChecksumValues testCSV = _csSDs[sdIdx]._sd.CalculateChecksums();

                if (!originalCSV.Equals(testCSV))
                {
                    _originalSD.WriteUnitTestFile("C:\\_Tmp_Orig", SimulationType.None);
                    _csSDs[sdIdx]._sd.WriteUnitTestFile("C:\\_Tmp_Test", SimulationType.None);
                    throw new Exception("The simulated client checksum validation code has found a problem.");
                }
                else
                {
                }

                if (recreate)
                {
                    int maxSimulates = _csSDs[sdIdx]._maxSimulates;
                    _csSDs.RemoveAt(sdIdx);
                    Add(maxSimulates);
                }
            }

            internal void Add(int aMaxSimulates)
            {
                _csSDs.Add(new ChecksumSD(_originalSD, aMaxSimulates));
                ValidateChecksums(_csSDs.Count - 1, false, null); // Perform touch?
            }
        }

        class ChecksumSD
        {
            internal ChecksumSD(ScenarioDetail aSD, int aMaxSimulates)
            {
                _maxSimulates = aMaxSimulates;

                _sd = Scenario.Copy(aSD);
                _sd._csPrimarySD = false;
                ((IScenarioRef)_sd).SetReferences(aSD.Scenario, null);
                _sd.RestoreReferences(PT.ConstantDefinitions.SerializationVersionNumber.NoVersion, aSD.Scenario, aSD._schedulerCustomizations, aSD._operationScheduledCustomizations, aSD._schedulabilityCustomization, false);
            }

            internal ScenarioDetail _sd;
            internal int _maxSimulates;
            internal int _simulates;

            int _before;
            void Before()
            {
                _before = _sd._appliedSimulates;
            }

            bool After()
            {
                return (_simulates += _sd._appliedSimulates - _before) >= _maxSimulates;
            }

            internal bool Receive(ScenarioBaseT t)
            {
                Before();
                _sd.Receive(t);
                return After();
            }

            internal bool Receive(CtpT ctpT, Scenario scenarioToSendResultTo)
            {
                Before();
                _sd.Receive(ctpT, null);
                return After();
            }

            internal bool Receive(ScenarioClockAdvanceT scenarioClockAdvanceT)
            {
                Before();
                _sd.Receive(scenarioClockAdvanceT);
                return After();
            }

            internal bool Receive(ScenarioTouchT touchT)
            {
                Before();
                _sd.Receive(touchT);
                return After();
            }
        }

        void InitTestSequence()
        {
            if (_csPrimarySD)
            {
                _csSDs = new CheckSumSDSet(this);
                _csSDs.Add(1);
                //_csSDs.Add(4);
                //_csSDs.Add(32);
                //_csSDs.Add(200);
                //_csSDs.Add(400);
                _csSDs.Add(1000000);
            }
        }

    #endif
    #endregion
    #endregion Post simulation procedures

    public class SimulationTimingMeasurement
    {
        internal SimulationTimingMeasurement(PT.Common.Testing.Timing a_timing, bool a_endOfTimingBlock)
        {
            Timing = a_timing;
            EndOfTimingBlock = a_endOfTimingBlock;
        }

        public PT.Common.Testing.Timing Timing { get; private set; }

        public bool EndOfTimingBlock { get; private set; }
    }

    private const int c_maxNbrOfTimingEntries = 40; // This should cover the last 20 schedule adjustments. Currently there are 2 timings performed per schedule adjustments; the adjustment type function and the simulation function (contained within the type function).

    [Common.Attributes.DebugLogging(Common.Attributes.EDebugLoggingType.None)]
    private readonly CircularQueue<SimulationTimingMeasurement> m_optimizeTimingQueue = new (c_maxNbrOfTimingEntries);

    public CircularQueue<SimulationTimingMeasurement> GetCopyOfSimulationTimingQueue()
    {
        return new CircularQueue<SimulationTimingMeasurement>(m_optimizeTimingQueue);
    }

    private PT.Common.Testing.Timing CreateTiming(string a_name)
    {
        return new PT.Common.Testing.Timing(true, "Sim# " + (NbrOfSimulations + 1) + "; " + a_name);
    }

    private void StopTiming(PT.Common.Testing.Timing a_timing, bool a_printSeparator)
    {
        a_timing.Stop();
        m_optimizeTimingQueue.Enqueue(new SimulationTimingMeasurement(a_timing, a_printSeparator));
    }

    private static int CompareResById(InternalResource a_b1, InternalResource a_b2)
    {
        if (a_b1.CapacityType == InternalResourceDefs.capacityTypes.Infinite && a_b2.CapacityType != InternalResourceDefs.capacityTypes.Infinite)
        {
            return -1;
        }

        if (a_b1.CapacityType != InternalResourceDefs.capacityTypes.Infinite && a_b2.CapacityType == InternalResourceDefs.capacityTypes.Infinite)
        {
            return 1;
        }

        if (a_b1.Id.Value < a_b2.Id.Value)
        {
            return -1;
        }

        if (a_b1.Id.Value > a_b2.Id.Value)
        {
            return 1;
        }

        return 0;
    }

    #region outside simulation setup
    private const int INFINITE_RES_START_SIM_NBR = 1000000000;
    private const int OTHER_RES_START_SIM_NBR = 2000000000;

    private void ResetResSimSeqNbrs()
    {
        List<InternalResource> resList = new ();

        for (int pI = 0; pI < PlantManager.Count; ++pI)
        {
            PlantManager[pI].AddResources(resList);
        }

        resList.Sort(CompareResById);

        int infiniteNbr = INFINITE_RES_START_SIM_NBR;
        int otherNbr = OTHER_RES_START_SIM_NBR;
        for (int rI = 0; rI < resList.Count; ++rI)
        {
            InternalResource ir = resList[rI];

            if (ir.CapacityType == InternalResourceDefs.capacityTypes.Infinite)
            {
                ir.m_v_simSort = infiniteNbr;
                ++infiniteNbr;
            }
            else
            {
                ir.m_v_simSort = otherNbr;
                ++otherNbr;
            }
        }
    }

    private void TestResSimSeqNbrs(MainResourceSet a_resources)
    {
        bool uninitializedFound = false;

        for (int rI = 0; rI < a_resources.Count; ++rI)
        {
            InternalResource ir = a_resources[rI];

            if (ir.CapacityType == InternalResourceDefs.capacityTypes.Infinite)
            {
                if (ir.m_v_simSort < INFINITE_RES_START_SIM_NBR)
                {
                    uninitializedFound = true;
                    break;
                }
            }
            else
            {
                if (ir.m_v_simSort < OTHER_RES_START_SIM_NBR)
                {
                    uninitializedFound = true;
                    break;
                }
            }
        }

        if (uninitializedFound)
        {
            ResetResSimSeqNbrs();
        }
    }

    //void ___ImportCompleteTest()
    //{
    //    ___GonnellaImportCompleteTest();
    //}

    //void ___GonnellaImportCompleteTest()
    //{
    //    for (int jobI = 0; jobI < JobManager.Count; ++jobI)
    //    {
    //        Job j = JobManager[jobI];
    //        ManufacturingOrder mo = j.ManufacturingOrders[0];
    //        AlternatePath ap=mo.CurrentPath;
    //        AlternatePath.NodeCollection leaves=ap.GetEffectiveLeaves();

    //        AlternatePath.Node production = leaves[0];
    //        AlternatePath.Node freezer = production.Successors[0].Successor;
    //        AlternatePath.Node packing = freezer.Successors[0].Successor;

    //        CompareNodes(mo, production, freezer);
    //        CompareNodes(mo, production, packing);
    //    }
    //}

    //void CompareNodes(ManufacturingOrder mo, AlternatePath.Node first, AlternatePath.Node second)
    //{
    //    ResourceOperation firstOp = (ResourceOperation)first.Operation;
    //    ResourceOperation secondOp = (ResourceOperation)second.Operation;

    //}
    #endregion

    #if DEBUG
    /// <summary>
    /// Created to help find an activity during debugging.
    /// </summary>
    /// <param name="a_act"></param>
    private void xScanDispatchersForAct(InternalActivity a_act)
    {
        for (int i = 0; i < m_resourceDispatchers.Length; ++i)
        {
            ReadyActivitiesDispatcher.Enumerator etr = m_resourceDispatchers[i].GetEnumeratorOfActivitiesOnDispatcher();
            int yyy = m_resourceDispatchers[i].Count;
            while (etr.MoveNext())
            {
                if (etr.Current == a_act) { }
            }
        }
    }

    private List<InternalActivity> xCalcActsOnDispatchers()
    {
        List<InternalActivity> acts = new ();
        for (int i = 0; i < m_resourceDispatchers.Length; ++i)
        {
            acts.AddRange(m_resourceDispatchers[i]);
        }

        return acts;
    }

    private List<Job> xCalcJobsWithUnfinishedPreds()
    {
        List<Job> uPredJobs = new ();
        for (int i = 0; i < JobManager.Count; ++i)
        {
            Job job = JobManager[i];
            for (int j = 0; j < job.ManufacturingOrders.Count; ++j)
            {
                ManufacturingOrder mo = job.ManufacturingOrders[j];
                List<ResourceOperation> ops = mo.CurrentPath.GetOperationsByLevel(false);
                for (int k = 0; k < ops.Count; ++k)
                {
                    ResourceOperation op = ops[k];
                    if (op.Finished)
                    {
                        AlternatePath.AssociationCollection preds = op.AlternatePathNode.Predecessors;
                        for (int l = 0; l < preds.Count; ++l)
                        {
                            ResourceOperation predOp = (ResourceOperation)preds[l].Predecessor.Operation;
                            if (!predOp.Finished)
                            {
                                uPredJobs.Add(job);
                            }
                        }
                    }
                }
            }
        }

        return uPredJobs;
    }

    internal enum xBlockReservationType
    {
        Scheduling,
        BlockReservationCreation,
        BlockReservationEventCreation,
        ProcessingBlockReservationEvent,
        AttentionAvailable,
        PerformAndReapplyReleases
    }

    internal class xBlockReservationData
    {
        internal xBlockReservationData(xBlockReservationType a_type, long a_simClock, long a_startTicks, long a_endTicks, object a_data1, object a_data2 = null, object a_data3 = null, object a_data4 = null)
        {
            Type = a_type;
            SimClock = a_simClock;
            StartTicks = a_startTicks;
            EndTicks = a_endTicks;
            Data1 = a_data1;
            Data2 = a_data2;
            Data3 = a_data3;
            Data4 = a_data4;
        }

        internal readonly xBlockReservationType Type;
        internal readonly long SimClock;
        internal readonly long StartTicks;
        internal readonly long EndTicks;
        internal readonly object Data1;
        internal readonly object Data2;
        internal readonly object Data3;
        internal readonly object Data4;

        public override string ToString()
        {
            StringBuilder sb = new ();
            sb.AppendFormat("{0}; SimClk={1}; Start={2}; End={3}; {4}", Type, DateTimeHelper.ToLocalTimeFromUTCTicks(SimClock), DateTimeHelper.ToLocalTimeFromUTCTicks(StartTicks), DateTimeHelper.ToLocalTimeFromUTCTicks(EndTicks), Data1);
            if (Data2 != null)
            {
                sb.AppendFormat("; {0}", Data2);
            }

            if (Data3 != null)
            {
                sb.AppendFormat("; {0}", Data3);
            }

            if (Data4 != null)
            {
                sb.AppendFormat("; {0}", Data4);
            }

            if (Data1 is ResourceCapacityInterval)
            {
                sb.AppendFormat("; PctAvail={0}", Data2);
            }

            return sb.ToString();
        }
    }

    internal static List<xBlockReservationData> x_testData = new ();

    internal static void xAddData(xBlockReservationData a)
    {
        x_testData.Add(a);
    }

    internal static List<ResourceEvent> x_resEvents = new ();
    internal static void xAddResEvent(ResourceEvent e)
    {
        x_resEvents.Add(e);
    }
    #endif

    private readonly Dictionary<InternalActivity, Plant.FindMaterialResult> m_materialConstrainedActivities = new ();

    private readonly LinkedList<ICalculatedValueCache> m_calculatedValueCaches = new ();

    /// <summary>
    /// Initialize an existing cache, or create a new typed cache for an object.
    /// The caller must manage the lifespan of the cache.
    /// </summary>
    public void InitializeCache<T>(ref ICalculatedValueCache<T> a_cache)
    {
        if (a_cache == null)
        {
            a_cache = new CalculatedValueCache<T>();
        }

        a_cache.Initialize();
        m_calculatedValueCaches.AddLast(a_cache);
    }

    /// <summary>
    /// This event should be called after Simulate is complete and the post processing actions have been finished.
    /// This contains shared work that is done after every simulation
    /// </summary>
    private void SimulationActionComplete()
    {
        foreach (ICalculatedValueCache cache in m_calculatedValueCaches)
        {
            #if DEBUG
            if (cache.Enabled)
            {
                throw new DebugException("CalculatedValueCache was enabled incorrectly");
            }
            #endif
            cache.Enabled = true;
        }

        //Notify demand collections that the simulation is complete
        //m_salesOrderManager.SimulationActionComplete();
        //m_transferOrderManager.SimulationActionComplete();
        m_jobManager.SimulationActionComplete();
    }

    /// <summary>
    /// Joing MOs on the same Resource and in the same Capacity Interval into one larger MO.
    /// </summary>
    private void AutoJoinManufacturingOrders(ScenarioBaseT a_t, IScenarioDataChanges a_dataChanges)
    {
        bool autoJoinedAtLeastOnce = false;

        foreach (Resource res in PlantManager.GetResourceList())
        {
            if (AutoJoin(res, a_dataChanges))
            {
                autoJoinedAtLeastOnce = true;
            }
        }

        if (autoJoinedAtLeastOnce)
        {
            TimeAdjustment(a_t);
        }
    }

    private bool AutoJoin(Resource a_resource, IScenarioDataChanges a_dataChanges)
    {
        bool autoJoinedAtLeastOnce = false;
        if (m_extensionController.RunAutoJoinExtension)
        {
            return autoJoinedAtLeastOnce;
        }

        if (a_resource.AutoJoinSpan.Ticks > 0 && a_resource.BatchType == MainResourceDefs.batchType.None) //AutoJoin enabled; No AutoJoin for Batches yet. Might overload the batch size by joining.
        {
            long maxAutoJoinDate = Clock + a_resource.AutoJoinSpan.Ticks;

            ResourceBlockList.Node blockNode = a_resource.Blocks.First;
            ResourceBlock previousBlock = null;

            //Start at the first nodes that can be joined. 
            previousBlock = blockNode.Data;
            blockNode = blockNode.Next;

            while (blockNode != null)
            {
                if (blockNode.Data.StartTicks > maxAutoJoinDate)
                {
                    break; //reached the end of the AutoJoinSpan for this Resource
                }

                if (m_extensionController.CanAutoJoin(this, a_resource, previousBlock, blockNode))
                {
                    //Get the next block on the resource that is not from the same MO (in case the MO being joined away has multiple operations on this resource)                                
                    ResourceBlockList.Node tmpNxtNode = blockNode.Next; //Set now before blockNode gets autjoined away
                    while (tmpNxtNode != null && tmpNxtNode.Data.Batch.FirstActivity.Operation.ManufacturingOrder.Id == blockNode.Data.Batch.FirstActivity.Operation.ManufacturingOrder.Id)
                    {
                        tmpNxtNode = tmpNxtNode.Next;
                    }

                    InternalOperation joiningOp = blockNode.Data.Batch.FirstActivity.Operation;

                    m_jobManager.Join(previousBlock.Batch.FirstActivity.Operation.ManufacturingOrder, blockNode.Data.Batch.FirstActivity.Operation.ManufacturingOrder, m_productRuleManager, a_dataChanges);

                    autoJoinedAtLeastOnce = true;

                    m_extensionController.AutoJoined(a_resource, previousBlock.Batch.FirstActivity.Operation, joiningOp);

                    blockNode = tmpNxtNode;
                    //Don't change 'previousBlock' since it wil remain the block to compare against since blockNode is getting joined away
                }
                else
                {
                    previousBlock = blockNode.Data;
                    blockNode = blockNode.Next;
                }
            }
        }

        return autoJoinedAtLeastOnce;
    }

    /// <summary>
    /// Remove CapacityIntervals and RecurringCapacityIntervals that end before the Clock.
    /// </summary>
    private void PurgeCapacityIntervals(long a_newClockTime, IScenarioDataChanges a_dataChanges)
    {
        m_clock = a_newClockTime; //TEMPORARY -- do in calling function once available. This needs to be set before generating profiles so end CI for planning horizon is right.

        //Purge old capacity intervals from the past, preserving based on Actual Tracking
        long purgeStartTime = a_newClockTime - ScenarioOptions.TrackActualsAgeLimit.Ticks;
        RecurringCapacityIntervalManager.AdjustForChangedHorizon(a_newClockTime, this, a_dataChanges);
        CapacityIntervalManager.PurgeOldCapacityIntervals(purgeStartTime);
        FireCapacityIntervalsPurgedEvent(new DateTime(purgeStartTime));

        //Update the capacity profiles for any Resources that have no CIs for RCIs since their default intervals need to be updated to the new Clock and Planning Horizon.
        for (int plantI = 0; plantI < PlantManager.Count; plantI++)
        {
            Plant plant = PlantManager[plantI];
            for (int deptI = 0; deptI < plant.DepartmentCount; deptI++)
            {
                Department dept = plant.Departments[deptI];
                for (int resI = 0; resI < dept.ResourceCount; resI++)
                {
                    Resource resource = dept.Resources[resI];
                    if (resource.CapacityIntervals.Count == 0 && resource.RecurringCapacityIntervals.Count == 0)
                    {
                        resource.RegenerateCapacityProfile(this.GetPlanningHorizonEndTicks(), true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Deletes the following objects or sub collections that occur after the short term span: Jobs, Purchase orders, Sales orders and lines, Transfer orders
    /// </summary>
    /// <param name="a_t">Used for timestamp</param>
    internal void ClearJobsAndAdjustmentsAfterShortTerm(PTTransmission a_t, IScenarioDataChanges a_dataChanges)
    {
        //TODO: Add datachanges for whole function

        DateTime shortTermEnd = GetEndOfShortTerm();
        if (m_clock == shortTermEnd.Ticks)
        {
            throw new PTValidationException("Unable to clear data past short term. A short term span must first be defined. This can be done in System Options");
        }

        //Delete all jobs starting after the short term and ending after the planning horizon end. 
        for (int jobI = JobManager.Count - 1; jobI >= 0; jobI--)
        {
            Job currentJob = JobManager[jobI];
            if (currentJob.Scheduled && currentJob.ScheduledStartDate > shortTermEnd)
            {
                JobManager.DeleteJob(currentJob, a_dataChanges);
            }
        }

        for (int poI = PurchaseToStockManager.Count - 1; poI >= 0; poI--)
        {
            PurchaseToStock purchaseToStock = PurchaseToStockManager[poI];
            if (purchaseToStock.ReceiptDate > shortTermEnd)
            {
                a_dataChanges.PurchaseToStockChanges.DeletedObject(purchaseToStock.Id);
                PurchaseToStockManager.Remove(purchaseToStock);
            }
        }

        //Delete SalesOrders
        //TODO: Add datachanges
        SalesOrderManager.DeleteSalesOrdersAfterDateTime(this, shortTermEnd, a_t);

        //Delete Forecasts
        foreach (Warehouse warehouse in WarehouseManager)
        {
            foreach (Inventory itemInventory in warehouse.Inventories)
            {
                if (itemInventory.ForecastVersions != null)
                {
                    for (int fvI = itemInventory.ForecastVersions.Versions.Count - 1; fvI >= 0; fvI--)
                    {
                        ForecastVersion forecastVersion = itemInventory.ForecastVersions.Versions[fvI];
                        //For every forecast in the version
                        for (int fI = forecastVersion.Forecasts.Count - 1; fI >= 0; fI--)
                        {
                            Forecast forecast = forecastVersion.Forecasts[fI];
                            //Check each shipment and delete it if it is after the short term
                            for (int shipI = forecast.Shipments.Count - 1; shipI >= 0; shipI--)
                            {
                                ForecastShipment forecastShipment = forecast.Shipments[shipI];
                                if (forecastShipment.RequiredDate > shortTermEnd)
                                {
                                    //Delete shipment
                                    forecast.RemoveShipment(forecastShipment, this, a_t);
                                }
                            }

                            //Remove forecast if there are no shipments left
                            if (forecast.Shipments.Count == 0)
                            {
                                forecastVersion.DeleteForecast(this, forecast, a_t);
                            }
                        }
                        //TODO: maybe delete empty forecast versions
                    }
                }
            }
        }

        //Delete TransferOrders
        TransferOrderManager.DeleteTransferOrdersAfterDateTime(this, shortTermEnd, a_t, a_dataChanges);
    }
}