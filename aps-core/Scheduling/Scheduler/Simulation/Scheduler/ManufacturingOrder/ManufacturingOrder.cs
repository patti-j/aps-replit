using System.Collections;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule;
using PT.Scheduler.Simulation.Events;
using PT.Scheduler.Simulation.Extensions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Request to produce a specified qty of a specified product at a specified time.
/// </summary>
public partial class ManufacturingOrder
{
    #region Eligibility
    /// <summary>
    /// When an adjustment is made to the plant list (adds/deletes), you need to \call this function so the jobs can update their
    /// MO plant eligibility.
    /// </summary>
    /// <param name="t"></param>
    internal void AdjustEligiblePlants(ScenarioBaseT a_t)
    {
        if (a_t is PlantDeleteAllT)
        {
            EligiblePlants.Clear();
        }
        else if (a_t is PlantDeleteT)
        {
            for (int pI = EligiblePlants.Count - 1; pI >= 0; --pI)
            {
                if (!ScenarioDetail.PlantManager.Contains(EligiblePlants[pI].Plant.Id))
                {
                    EligiblePlants.Remove(pI);
                }
            }
        }
        else if (a_t is PlantT || a_t is PlantDefaultT)
        {
            if (!EligiblePlantsSpecified)
            {
                m_eligiblePlants.MakeEligible(ScenarioDetail.PlantManager);
            }
        }
    }

    /// <summary>
    /// Clear all op-path node associations then reassociate ops and path nodes.
    /// *LRH*TODO*		Take a look at eligibility. In this case you may need to recalculate eligibility for the entire job since you will need to take CanSpanPlants into consideration. There may be more you can
    /// do with this.
    /// <param name="a_automaticallyResolveErrors">Whether to alter transfer quantity as needed to avoid validation errors</param>
    /// </summary>
    private void AssociateOpsWithPaths(bool a_automaticallyResolveErrors)
    {
        OperationManager.ClearOpPathNodeAssociations();
        AlternatePaths.AssociateOpsWithPathNodes(a_automaticallyResolveErrors);
    }

    /// <summary>
    /// Whether all, some, or none of following path types can be scheduled: the current path or paths with RegularRelease and ReleaseOffsetFromDefaultPathsLatestRelease.
    /// </summary>
    /// <param name="a_simulationType"></param>
    internal AlternatePathSatisfiability AdjustedPlantResourceEligibilitySets_IsSatisfiable(ScenarioDetail.SimulationType a_simulationType)
    {
        int releasablePaths = 0;
        int satisfiablePaths = 0;

        for (int altI = 0; altI < AlternatePaths.Count; altI++)
        {
            AlternatePath ap = AlternatePaths[altI];

            if ((ap == CurrentPath ||
                 ap.AutoUse == AlternatePathDefs.AutoUsePathEnum.RegularRelease ||
                 ap.AutoUse == AlternatePathDefs.AutoUsePathEnum.ReleaseOffsetFromDefaultPathsLatestRelease)
                ||
                a_simulationType == ScenarioDetail.SimulationType.Expedite)
            {
                ++releasablePaths;
                if (ap.AdjustedPlantResourceEligibilitySets_IsSatisfiable())
                {
                    ++satisfiablePaths;
                }
            }
        }

        if (releasablePaths == satisfiablePaths)
        {
            return AlternatePathSatisfiability.AllPathsSatisfiable;
        }

        if (satisfiablePaths == 0)
        {
            return AlternatePathSatisfiability.NoPathsSatisfiable;
        }

        return AlternatePathSatisfiability.SomePathsNotSatisfiable;
    }

    /// <summary>
    /// Determine whether the manufacturing orders current alternate path is satisfiable somewhere.
    /// Based on the eligibility determined in step 4.
    /// </summary>
    /// <returns>true if there are eligible location at which to produce the manufacturing order.</returns>
    internal bool EligibleResources_IsSatisfiable()
    {
        return CurrentPath.EligibleResources_IsSatisfiable();
    }

    /// <summary>
    /// Returns a collection of all nodes that cannot be satisfied by existing Resources.
    /// </summary>
    internal AlternatePath.NodeCollection GetUnsatisfiableNodes()
    {
        return CurrentPath.GetUnsatisfiableNodes();
    }

    /// <summary>
    /// Filter AdjustedPlantResourceEligibilitySets down to a single plant. This might lead to the job being unschedulable.
    /// If needed, this should be done prior to the optimize to it can be excluded if it's no longer schedulable.
    /// </summary>
    /// <param name="a_plantId"></param>
    internal void AdjustedPlantResourceEligibilitySets_FilterDownToSpecificPlant(BaseId a_plantId)
    {
        AlternatePaths.AdjustedPlantResourceEligibilitySets_FilterDownToSpecificPlant(a_plantId);
    }

    /// <summary>
    /// Activity's whose eligibility has already been overridden are not filtered.
    /// </summary>
    internal void AdjustedPlantResourceEligibilitySets_Filter(ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter a_excludeFromManualFilter)
    {
        AlternatePaths.AdjustedPlantResourceEligibilitySets_Filter(a_excludeFromManualFilter);
    }
    #endregion

    #region JIT
    /// <summary>
    /// Calculate the JIT start date of the MO.
    /// </summary>
    /// <param name="a_limitToCurrentPath">Whether to limit the calculation to the current path.</param>
    internal void CalculateJitTimes(long a_simClock, bool a_limitToCurrentPath)
    {
        if (a_limitToCurrentPath)
        {
            CalculateJitTimes(a_simClock, CurrentPath);
        }
        else
        {
            for (int i = 0; i < AlternatePaths.Count; ++i)
            {
                CalculateJitTimes(a_simClock, AlternatePaths[i]);
            }
        }
    }

    /// <summary>
    /// Calculate the JIT start dates of a specific path.
    /// </summary>
    /// <param name="a_path">The paths whose JIT dates to calculate.</param>
    private void CalculateJitTimes(long a_simClock, AlternatePath a_path)
    {
        //m_needDateCache = null; //Clear the cache here because job need dates may have changed (a common reason for calling this function).

        List<ResourceOperation> operations = a_path.GetOperationsByLevel(true);
        TimeSpan shippingBuffer = GetProductShippingBuffer();
        for (int opI = 0; opI < operations.Count; ++opI)
        {
            ResourceOperation ro = operations[opI];
            ro.JITCalculateStartDates(a_simClock, shippingBuffer.Ticks);
            shippingBuffer = TimeSpan.Zero; // Only apply the shipping buffer to the last operation.
        }
    }
    #endregion

    #region Successor MOs
    #region Variable, linking to successor MOs, validation.
    private SuccessorMOArrayList m_successorMOs;

    [System.ComponentModel.Browsable(false)]
    public SuccessorMOArrayList SuccessorMOs => m_successorMOs;

    /// <summary>
    /// Create links between predecessor and successor MOs.
    /// References are stored at the operation and MO level
    /// of this MO.
    /// </summary>
    internal void LinkSuccessorMOs()
    {
        if (m_successorMOs != null) //can be null from CTP Jobs
        {
            for (int sucI = 0; sucI < m_successorMOs.Count; ++sucI)
            {
                SuccessorMO sucMO = m_successorMOs[sucI];
                sucMO.LinkSuccessorMOs(this);
            }
        }
    }
    #endregion

    #region Successor MO processing; initialization for simulation process; processing necessary after a simulation has completed.
    /// <summary>
    /// Sets up predecessor MO constraints against successor MOs.
    /// </summary>
    internal void SuccessorMOInit()
    {
        for (int sucI = 0; sucI < m_successorMOs.Count; ++sucI)
        {
            SuccessorMO sucMO = m_successorMOs[sucI];

            if (sucMO.SuccessorManufacturingOrder != null)
            {
                if (sucMO.AlternatePath != null)
                {
                    if (sucMO.SuccessorManufacturingOrder.CurrentPath == sucMO.AlternatePath)
                    {
                        if (sucMO.Operation != null)
                        {
                            // If the successor is using the specified AlternatePath then constrain the operation.
                            sucMO.Operation.NotifyOfPredecessorMOConstraint();
                        }
                        else
                        {
                            // If the successor is using the specified AlternatePath then constrain the entire MO.
                            sucMO.SuccessorManufacturingOrder.NotifyOfPredecessorMOConstraint();
                        }
                    }

                    continue;
                }

                if (sucMO.Operation != null)
                {
                    // Constrain the operation.
                    sucMO.Operation.NotifyOfPredecessorMOConstraint();
                    continue;
                }

                // Constrain the entire ManufacturingOrder.
                sucMO.SuccessorManufacturingOrder.NotifyOfPredecessorMOConstraint();
            }
        }
    }

    /// <summary>
    /// Call this function after this MO has been scheduled. It sends out a predecessor mo release event for every successor task.
    /// </summary>
    /// <param name="scheduledCompletionTime">The scheduled completion time of this MO.</param>
    /// <param name="events">The events queue. PredecessorMOAvailableEvents are put onto this queue.</param>
    internal void SuccessorCompletionProcessing(long a_scheduledCompletionTime, ScenarioDetail a_sd)
    {
        if (!SuccessorMOReleaseProcessed)
        {
            SuccessorMOReleaseProcessed = true;

            for (int sucMOI = 0; sucMOI < m_successorMOs.Count; ++sucMOI)
            {
                SuccessorMO sucMO = SuccessorMOs[sucMOI];
                long sucMOsCompTime = a_scheduledCompletionTime + sucMO.TransferSpan;

                PredecessorMOAvailableEvent predecessorMOAvailableEvent;

                if (sucMO.SuccessorManufacturingOrder != null && sucMO.SuccessorManufacturingOrder.ToBeScheduled)
                {
                    if (sucMO.AlternatePath != null)
                    {
                        if (sucMO.Operation != null)
                        {
                            // Notify the operation that a constraint on it has been released.
                            predecessorMOAvailableEvent = new PredecessorMOAvailableEvent(sucMOsCompTime, this, sucMO.Operation);
                        }
                        else
                        {
                            // Notify the MO that a constraint has been released.
                            predecessorMOAvailableEvent = new PredecessorMOAvailableEvent(sucMOsCompTime, this, sucMO.SuccessorManufacturingOrder);
                        }

                        a_sd.AddEvent(predecessorMOAvailableEvent);
                    }
                    else if (sucMO.Operation != null)
                    {
                        // Notify the operation that a constraint on it has been released.
                        predecessorMOAvailableEvent = new PredecessorMOAvailableEvent(sucMOsCompTime, this, sucMO.Operation);
                        a_sd.AddEvent(predecessorMOAvailableEvent);
                    }
                    else
                    {
                        // Notify the MO that a constraint on it has been released.
                        predecessorMOAvailableEvent = new PredecessorMOAvailableEvent(sucMOsCompTime, this, sucMO.SuccessorManufacturingOrder);
                        a_sd.AddEvent(predecessorMOAvailableEvent);
                    }
                }
            }
        }
    }
    #endregion

    #region FinishedPredecessorMOReleaseInfo. Information on prececessor MOs that have been completed.
    private readonly FinishedPredecessorMOReleaseInfoManager m_finishedPredecessorMOReleaseInfoManager = new ();

    public FinishedPredecessorMOReleaseInfoManager FinishedPredecessorMOReleaseInfoManager => m_finishedPredecessorMOReleaseInfoManager;

    /// <summary>
    /// This function needs to be called on the successor MO when a predecessor MO is finished.
    /// </summary>
    /// <param name="mo">The predecessor MO.</param>
    /// <param name="readyTicks">The time the predecessor releases this successor.</param>
    private void NotificationOfPredecessorMOFinish(ManufacturingOrder a_predMO, long a_readyTicks)
    {
        m_finishedPredecessorMOReleaseInfoManager.Add(a_predMO, a_readyTicks);
    }

    /// <summary>
    /// When this MO is finished this function needs to be called so the MO can notify its successor MOs
    /// of its completion and the time when this predecessor releases its successor.
    /// </summary>
    private void NotifySuccessorMOsOfFinish()
    {
        for (int i = 0; i < m_successorMOs.Count; ++i)
        {
            SuccessorMO sucMO = m_successorMOs[i];
            long finishedDate;
            bool finishedTemp = GetReportedFinishDate(out finishedDate);

            if (sucMO.Operation != null)
            {
                // This MO constrains an operation in the successor MO.
                long readyTicks = finishedDate + sucMO.TransferSpan;

                sucMO.Operation.NotificationOfPredecessorMOFinish(this, readyTicks);
            }
            else if (sucMO.SuccessorManufacturingOrder != null)
            {
                // This MO constrains an MO.
                long readyTicks = finishedDate + sucMO.TransferSpan;

                sucMO.SuccessorManufacturingOrder.NotificationOfPredecessorMOFinish(this, readyTicks);
            }
        }
    }
    #endregion
    #endregion

    #region Sim Flags
    private BoolVector32 m_simFlags;
    private const int BeingExpeditedIdx = 0;
    private const int MOReleasedDateEventOccurredIdx = 1;
    private const int ManufacturingOrderReleasedEventScheduledIdx = 2;
    private const int UnscheduledMOMarkerIdx = 3;
    private const int SuppressReleaseDateAdjustmentsIdx = 4;
    private const int SchedulableIdx = 5;
    private const int ReanchorAfterBeingExpeditedIdx = 6;
    private const int SuccessorMOReleaseProcessedIdx = 7;
    private const int ActivitiesScheduledIdx = 8;
    private const int WaitingOnHoldReleasedEventIdx = 9;
    private const int CanSpanPlantsHandledIdx = 10;
    private const int CurrentPathNeedsToBeSetWhenFirstActivityIsScheduledIdx = 11;
    private const int LockedToCurrentPathByOptimize_InPlantThatsNotBeingOptimizedIdx = 12;
    private const int LockedToCurrentPathByOptimize_ScheduledBeforeStartOfOptimizeIdx = 13;
    private const int ToBeScheduledIdx = 14;
    private const int c_compressLimitedByDateDeterminedIdx = 15;
    private const int c_scheduledEndOfStageHandlingCompleteIdx = 16;
    private const int c_jitCalculatedIdx = 17;
    private const int c_pathReleaseEventProcesedIdx = 18;

    /// <summary>
    /// Simulation state variable. Whether the MO is being expedited.
    /// </summary>
    internal bool BeingExpedited
    {
        get => m_simFlags[BeingExpeditedIdx];
        set => m_simFlags[BeingExpeditedIdx] = value;
    }

    /// <summary>
    /// Simulation state variable that indicates whether the
    /// MO has been released within the simulation.
    /// </summary>
    internal bool MOReleasedDateEventOccurred
    {
        get => m_simFlags[MOReleasedDateEventOccurredIdx];
        set => m_simFlags[MOReleasedDateEventOccurredIdx] = value;
    }

    /// <summary>
    /// Simulation state variable. Whether the corresponding event has been scheduled.
    /// </summary>
    internal bool ManufacturingOrderReleasedEventScheduled
    {
        get => m_simFlags[ManufacturingOrderReleasedEventScheduledIdx];
        set => m_simFlags[ManufacturingOrderReleasedEventScheduledIdx] = value;
    }

    /// <summary>
    /// Simulation state variable used to indicate that the MO was unscheduled prior to
    /// a simulation and needs to be rescheduled in the simulation.
    /// </summary>
    internal bool UnscheduledMOMarker
    {
        get => m_simFlags[UnscheduledMOMarkerIdx];
        set => m_simFlags[UnscheduledMOMarkerIdx] = value;
    }

    /// <summary>
    /// If this is true, the release date will not be affected by the JIT start date, Frozen Span, Stable Span, Headstart Span, etc.
    /// </summary>
    internal bool SuppressReleaseDateAdjustments
    {
        get => m_simFlags[SuppressReleaseDateAdjustmentsIdx];
        set => m_simFlags[SuppressReleaseDateAdjustmentsIdx] = value;
    }

    /// <summary>
    /// Indicates that the MO is being expedited and will be reanchored after the expedite.
    /// </summary>
    internal bool ReanchorAfterBeingExpedited
    {
        get => m_simFlags[ReanchorAfterBeingExpeditedIdx];
        set => m_simFlags[ReanchorAfterBeingExpeditedIdx] = value;
    }

    /// <summary>
    /// Whether successor MO processing has been performed.
    /// </summary>
    private bool SuccessorMOReleaseProcessed
    {
        get => m_simFlags[SuccessorMOReleaseProcessedIdx];
        set => m_simFlags[SuccessorMOReleaseProcessedIdx] = value;
    }

    internal bool ActivitiesScheduled
    {
        get => m_simFlags[ActivitiesScheduledIdx];
        set => m_simFlags[ActivitiesScheduledIdx] = value;
    }

    internal bool WaitingOnHoldReleasedEvent
    {
        get => m_simFlags[WaitingOnHoldReleasedEventIdx];
        set => m_simFlags[WaitingOnHoldReleasedEventIdx] = value;
    }

    private bool CanSpanPlantsHandled
    {
        get => m_simFlags[CanSpanPlantsHandledIdx];
        set => m_simFlags[CanSpanPlantsHandledIdx] = value;
    }

    /// <summary>
    /// Multiple paths can be released during a simulation. This flag tracks whether the CurrentPath
    /// field needs to be set when simulation chooses the path to schedule.
    /// </summary>
    internal bool CurrentPathNeedsToBeSetWhenFirstActivityIsScheduled
    {
        get => m_simFlags[CurrentPathNeedsToBeSetWhenFirstActivityIsScheduledIdx];
        set => m_simFlags[CurrentPathNeedsToBeSetWhenFirstActivityIsScheduledIdx] = value;
    }

    /// <summary>
    /// During an Optimize this flag is set if any of the MO's activities are scheduled in a plant that's not being optimized.
    /// </summary>
    internal bool LockedToCurrentPathByOptimize_InPlantThatsNotBeingOptimized
    {
        get => m_simFlags[LockedToCurrentPathByOptimize_InPlantThatsNotBeingOptimizedIdx];
        set => m_simFlags[LockedToCurrentPathByOptimize_InPlantThatsNotBeingOptimizedIdx] = value;
    }

    /// <summary>
    /// During an Optimize this flag is set if any of the MO's activities are scheduled to start before the Optimize start time.
    /// </summary>
    internal bool LockedToCurrentPathByOptimize_ScheduledBeforeStartOfOptimize
    {
        get => m_simFlags[LockedToCurrentPathByOptimize_ScheduledBeforeStartOfOptimizeIdx];
        set => m_simFlags[LockedToCurrentPathByOptimize_ScheduledBeforeStartOfOptimizeIdx] = value;
    }

    /// <summary>
    /// Prior to the start of simulation is performed, this is set to indicate whether the MO will be scheduled.
    /// This can't be merged with the corresponding Job property since it's possible that not all of the MOs within a job
    /// will be scheduled (some can be finished).
    /// </summary>
    internal bool ToBeScheduled
    {
        get => m_simFlags[ToBeScheduledIdx];
        set => m_simFlags[ToBeScheduledIdx] = value;
    }

    /// <summary>
    /// If a compress is being performed that's limited by DBR date (releaseRule=DBR) is being performed,
    /// this value is set to true after it has been determined whether the MO's compression
    /// should be limited by the DBR Release Date.
    /// </summary>
    internal bool CompressLimitedByDateDetermined
    {
        get => m_simFlags[c_compressLimitedByDateDeterminedIdx];
        set => m_simFlags[c_compressLimitedByDateDeterminedIdx] = value;
    }

    /// <summary>
    /// Used to track extra end of stage simulation processing after the stage in which an MO is scheduled in.
    /// When an MO is finished after a stage in a multi-stage simulation, it's processed and this flag is set to true.
    /// </summary>
    internal bool ScheduledEndOfStageHandlingComplete
    {
        get => m_simFlags[c_scheduledEndOfStageHandlingCompleteIdx];
        set => m_simFlags[c_scheduledEndOfStageHandlingCompleteIdx] = value;
    }

    /// <summary>
    /// Whether this MOs JIT has been calculated.
    /// </summary>
    internal bool JITCalculated
    {
        get => m_simFlags[c_jitCalculatedIdx];
        set => m_simFlags[c_jitCalculatedIdx] = value;
    }

    /// <summary>
    /// Whether the MO's path has been released.
    /// </summary>
    internal bool PathReleaseEventProcessed
    {
        get => m_simFlags[c_pathReleaseEventProcesedIdx];
        set => m_simFlags[c_pathReleaseEventProcesedIdx] = value;
    }
    #endregion

    #region simFlags2
    private BoolVector32 m_simFlags2;
    private const int SourceSubComponentIdx = 0;

    internal bool SourceSubComponent
    {
        get => m_simFlags2[SourceSubComponentIdx];
        set => m_simFlags2[SourceSubComponentIdx] = value;
    }
    #endregion

    /// <summary>
    /// When this value is set to false in a predecessor MO the successors "Schedulable" flags are also set to false.
    /// </summary>
    internal bool Schedulable
    {
        get => m_simFlags[SchedulableIdx];
        set
        {
            m_simFlags[SchedulableIdx] = value;

            if (!value)
            {
                for (int sucI = 0; sucI < m_successorMOs.Count; ++sucI)
                {
                    SuccessorMO sucMO = m_successorMOs[sucI];

                    if (sucMO.SuccessorManufacturingOrder != null && sucMO.SuccessorManufacturingOrder.Schedulable)
                    {
                        sucMO.SuccessorManufacturingOrder.Schedulable = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns the smallest stage of the leaves.
    /// </summary>
    /// <param name="a_defaultStage">
    /// If a stage can't be found a_defaultStage is returned; this may mean there aren't eligible resources able to process the operation or it's finished or omitted (in which
    /// case eligibility isn't calculated).
    /// </param>
    /// <returns>
    /// The earliest stage number. If a stage can't be found a_defaultStage is returned; this may mean there aren't eligible resources able to process the operation or it's finished or omitted (in
    /// which case eligibility isn't calculated).
    /// </returns>
    internal int GetFirstStageToScheduleIn(int a_defaultStage)
    {
        if (Finished)
        {
            return a_defaultStage;
        }

        AlternatePath path;
        AlternatePath.NodeCollection effectiveLeaves;
        AlternatePath.Node node;
        InternalOperation op;

        GetFirstStageToScheduleInHelper_GetData1(CurrentPath, out path, out effectiveLeaves, out node, out op);

        if (!CurrentPath.AdjustedPlantResourceEligibilitySets_IsSatisfiable())
        {
            // If the current path isn't satisfiable, try other paths.
            for (int pathI = 0; pathI < AlternatePaths.Count; ++pathI)
            {
                AlternatePath pathTmp = AlternatePaths[pathI];
                if (pathTmp != CurrentPath)
                {
                    // Use the first path that's satisfiable.
                    if (pathTmp.AdjustedPlantResourceEligibilitySets_IsSatisfiable())
                    {
                        GetFirstStageToScheduleInHelper_GetData1(pathTmp, out path, out effectiveLeaves, out node, out op);
                        break;
                    }
                }

            }
        }

        int stage = int.MaxValue;

        for (int i = 0; i < effectiveLeaves.Count; ++i)
        {
            GetFirstStageToScheduleInHelper_GetData2(path, out node, out op);
            stage = Math.Min(stage, op.GetStageToBeScheduledIn(a_defaultStage));
        }

        return stage;
    }

    /// <summary>
    /// A helper for GetFirstStageToScheduleIn()
    /// </summary>
    private static void GetFirstStageToScheduleInHelper_GetData1(AlternatePath a_pathToUse, out AlternatePath a_path, out AlternatePath.NodeCollection o_effectiveLeaves, out AlternatePath.Node o_node, out InternalOperation o_op)
    {
        o_effectiveLeaves = a_pathToUse.EffectiveLeaves;
        a_path = a_pathToUse;
        GetFirstStageToScheduleInHelper_GetData2(a_pathToUse, out o_node, out o_op);
    }

    /// <summary>
    /// A helper for GetFirstStageToScheduleIn()
    /// </summary>
    private static void GetFirstStageToScheduleInHelper_GetData2(AlternatePath a_pathToUse, out AlternatePath.Node o_node, out InternalOperation o_op)
    {
        o_node = a_pathToUse.EffectiveLeaves[0];
        o_op = o_node.Operation as InternalOperation;
    }

    #region Predecessor MO Requirements
    /// <summary>
    /// The number of predecessor MOs this operation is waiting on.
    /// </summary>
    private int m_predecessorMOCount;

    /// <summary>
    /// During a simulation call this function for each predecessor MO constraint.
    /// </summary>
    internal void NotifyOfPredecessorMOConstraint()
    {
        ++m_predecessorMOCount;
        m_predecessorMosReleasedTime = 0;
    }

    /// <summary>
    /// During a simulation call this function as each predecessor MO constraint is lifted.
    /// </summary>
    /// <param name="a_releaseDate"></param>
    internal void NotifyOfPredecessorMOConstraintSatisfaction(long a_releaseDate)
    {
        --m_predecessorMOCount;

        #if DEBUG
        if (m_predecessorMOCount < 0)
        {
            throw new Exception("ManufacturingOrder.predecessorMOCount has gone negative.");
        }
        #endif

        if (m_predecessorMOCount == 0)
        {
            m_predecessorMosReleasedTime = a_releaseDate;
        }
    }

    /// <summary>
    /// True if some MOs are constraining the MO.
    /// </summary>
    private bool WaitingOnPredecessors => m_predecessorMOCount > 0;

    private long m_predecessorMosReleasedTime;

    public DateTime PredecessorMosReleasedTime => m_predecessorMosReleasedTime > 0 ? new DateTime(m_predecessorMosReleasedTime) : PTDateTime.InvalidDateTime;
    #endregion

    /// <summary>
    /// This is null if tracking sub-components is not turned on in ScenarioOptions.
    /// </summary>
    private Dictionary<long, SubComponentSourceAndDest> m_subComponentSourceAndDestActivities;

    internal Dictionary<long, SubComponentSourceAndDest> SubComponentSourceAndDestActivities => m_subComponentSourceAndDestActivities;

    private int m_subComponentLevel;

    internal int SubComponentLevel => m_subComponentLevel;

    internal class SubComponentSourceAndDest
    {
        internal SubComponentSourceAndDest(InternalActivity a_source, InternalActivity a_destination, Product a_linkedProduct)
        {
            m_source = a_source;
            m_destination = a_destination;
            Product = a_linkedProduct;
        }

        internal InternalActivity m_source;
        internal readonly InternalActivity m_destination;
        internal readonly Product Product;
    }

    /// <summary>
    /// Add MO supply.
    /// </summary>
    internal void AddSubComponentSupply(InternalActivity a_source, InternalActivity a_activityOfThisMO_destination, Product a_linkedProduct)
    {
        if (m_subComponentSourceAndDestActivities != null)
        {
            a_source.ManufacturingOrder.SourceSubComponent = true;

            if (m_subComponentSourceAndDestActivities.TryGetValue(a_source.ManufacturingOrder.Id.Value, out SubComponentSourceAndDest scs))
            {
                if (a_source.Operation.DbrJitStartDateTicks < scs.m_source.Operation.DbrJitStartDateTicks)
                {
                    //Store the source with the earliest JIT start date.
                    m_subComponentSourceAndDestActivities[a_source.ManufacturingOrder.Id.Value] = new SubComponentSourceAndDest(a_source, a_activityOfThisMO_destination, a_linkedProduct);
                }
            }
            else
            {
                m_subComponentSourceAndDestActivities.Add(a_source.ManufacturingOrder.Id.Value, new SubComponentSourceAndDest(a_source, a_activityOfThisMO_destination, a_linkedProduct));
            }

            int tempSubComponentLevel = a_source.ManufacturingOrder.m_subComponentLevel + 1;
            if (tempSubComponentLevel > m_subComponentLevel)
            {
                m_subComponentLevel = tempSubComponentLevel;
            }
        }
    }

    internal bool UpdateSubJobSettings(long a_simClock, ScenarioOptions a_scenarioOptions, IScenarioDataChanges a_dataChanges)
    {
        bool updatedSubJobNeedDates = false;
        foreach (MaterialRequirement materialRequirement in GetMaterialRequirements())
        {
            InternalActivity destinationActivity = materialRequirement.Operation.GetLeadActivity() ?? materialRequirement.Operation.Activities.GetByIndex(0);
            Job.UpdateSubJobSettings(a_simClock, destinationActivity, a_scenarioOptions.SetSubJobHotFlags, a_scenarioOptions.SetSubJobNeedDatePoint, a_scenarioOptions.SetSubJobPriorities, out bool updatedSubJobNeedDatesTmp, a_scenarioOptions, a_dataChanges);

            if (updatedSubJobNeedDatesTmp)
            {
                updatedSubJobNeedDates = true;
            }
        }
        return updatedSubJobNeedDates;
    }

    internal void ResetSubComponentVariables(bool a_trackSubComponentSourceMOs)
    {
        m_subComponentLevel = 0;

        if (a_trackSubComponentSourceMOs)
        {
            m_subComponentSourceAndDestActivities = new Dictionary<long, SubComponentSourceAndDest>();
        }
        else
        {
            m_subComponentSourceAndDestActivities = null;
        }
    }

    /// <summary>
    /// This should be called before SimulationInitialization. This allows all the state variables in all objects to be reset before
    /// SimulationInitialization() takes place.
    /// </summary>
    internal void ResetSimulationStateVariables(ScenarioDetail a_sd)
    {
        m_simFlags.Clear();

        //CurrentPath.ResetSimulationStateVariables();
        //IDictionaryEnumerator alternateNodesEnumerator = CurrentPath.AlternateNodeHash.GetEnumerator();
        //while (alternateNodesEnumerator.MoveNext())
        //{
        //    DictionaryEntry de = (DictionaryEntry)alternateNodesEnumerator.Current;
        //    AlternatePath.Node node = (AlternatePath.Node)de.Value;
        //    BaseOperation baseOperation = node.Operation;
        //    baseOperation.ResetSimulationStateVariables();
        //}
        // !ALTERNATE_PATH!; ResetSimulationStateVariables() for the AlternatePaths.
        AlternatePaths.ResetSimulationStateVariables(a_sd);

        m_predecessorMOCount = 0;

        a_sd.InitializeCache(ref m_cachedScheduledEndDate);
    }

    internal void ResetSimulationStateVariables2()
    {
        m_simFlags2.Clear();
        ResetSubComponentVariables(ScenarioDetail.ScenarioOptions.TrackSubComponentSourceMOs);
    }

    /// <summary>
    /// Performed before any SimulationInitializations on all objects within the system.
    /// </summary>
    internal void SimulationInitialization(PlantManager a_plantManager, ProductRuleManager a_productRuleManager, ExtensionController a_extensionController, ICalculatedValueCacheManager a_cacheManager)
    {
        UsedAsSubComponent = false;
        m_subComponentNeedDateSet = false;
        Schedulable = true;
        SuccessorMOInit();

        //IDictionaryEnumerator alternateNodesEnumerator = CurrentPath.AlternateNodeHash.GetEnumerator();

        //while (alternateNodesEnumerator.MoveNext())
        //{
        //    DictionaryEntry de = (DictionaryEntry)alternateNodesEnumerator.Current;
        //    AlternatePath.Node node = (AlternatePath.Node)de.Value;
        //    BaseOperation baseOperation = node.Operation;
        //    baseOperation.SimulationInitialization();
        //}
        // !ALTERNATE_PATH!; SimulationInitialization() for the AlternatePaths. 
        AlternatePaths.SimulationInitialization(a_plantManager, a_productRuleManager, a_extensionController, a_cacheManager);

        m_earliestDepartmentalEndOfFrozenOrStableSpan = long.MinValue;
        m_simTimeEarliestDepartmentalEndOfFrozenOrStableSpanIsBasedOn = null;
    }

    internal void PostSimulationInitialization()
    {
        AlternatePaths.PostSimulationInitialization();
    }

    internal enum PostSimStageChangeTypes
    {
        None = 0,
        NeedDate = 1,
        RequiredQuantity = 2,
        UserFields = 4,
        IsReleased = 8,
        ReleaseDate = 16
    }

    /// <summary>
    /// This is called after Simulation only if a PostSimStageCust is loaded.
    /// It sets MoNeedDate and NeedDateTicks members if they've been set by the customization.
    /// </summary>
    /// <param name="a_simType"></param>
    /// <param name="a_t"></param>
    /// <param name="a_sd"></param>
    internal PostSimStageChangeTypes PostSimStageCustExecute(ScenarioDetail.SimulationType a_simType, long a_simClock, ScenarioBaseT a_t, ScenarioDetail a_sd, int a_curSimStageIdx, int a_finalSimStageIdx)
    {
        PostSimStageChangeTypes changeTypes = PostSimStageChangeTypes.None;
        ChangableMOValues changable = null;
        a_sd.ExtensionController.PostSimStageChangeMO(a_simType, a_t, a_sd, this, a_curSimStageIdx, a_finalSimStageIdx, out changable);

        if (changable != null)
        {
            if (changable.MoNeedDateSet)
            {
                MoNeedDate = changable.MoNeedDate;
            }

            if (changable.NeedDateTicksSet)
            {
                NeedDateTicks = changable.NeedDateTicks;
                changeTypes |= PostSimStageChangeTypes.NeedDate;
            }

            if (changable.RequiredQtySet)
            {
                SetRequiredQty(a_simClock, changable.RequiredQty, a_sd.ProductRuleManager);
                changeTypes |= PostSimStageChangeTypes.RequiredQuantity;
            }

            if (changable.UserFieldListSet)
            {
                //UpdateUserFields(changable.UserFields);
                changeTypes |= PostSimStageChangeTypes.UserFields;
            }
        }

        return changeTypes;
    }

    /// <summary>
    /// /// This is called after Simulation only if a PostSimStageCust is loaded.
    /// </summary>
    /// <param name="a_simType"></param>
    /// <param name="a_t"></param>
    /// <param name="a_sd"></param>
    internal PostSimStageChangeTypes PostSimStageCust(long a_simClock, ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, int a_currentSimStageIdx, int a_lastSimStageIdx, out bool o_computeEligibility)
    {
        bool computeEligibility = false;
        for (int i = 0; i < OperationManager.Count; i++)
        {
            InternalOperation op = OperationManager.GetByIndex(i) as InternalOperation;
            if (op == null)
            {
                continue;
            }

            computeEligibility = op.PostSimStageCust(a_simType, a_t, a_sd, a_currentSimStageIdx, a_lastSimStageIdx) || computeEligibility;
        }

        o_computeEligibility = computeEligibility;

        return PostSimStageCustExecute(a_simType, a_simClock, a_t, a_sd, a_currentSimStageIdx, a_lastSimStageIdx);
    }

    /// <summary>
    /// /// This is called after Simulation only if a EndOfSimulationCustomization is loaded.
    /// </summary>
    /// <param name="a_simType"></param>
    /// <param name="a_t"></param>
    /// <param name="a_sd"></param>
    internal PostSimStageChangeTypes EndOfSimulationCustExecute(long a_simClock, ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, out bool o_computeEligibility)
    {
        bool computeEligibility = false;
        for (int i = 0; i < OperationManager.Count; i++)
        {
            InternalOperation op = OperationManager.GetByIndex(i) as InternalOperation;
            if (op == null)
            {
                continue;
            }

            computeEligibility = op.EndOfSimulationCustExecute(a_simType, a_t, a_sd) || computeEligibility;
        }

        o_computeEligibility = computeEligibility;

        PostSimStageChangeTypes changeTypes = PostSimStageChangeTypes.None;
        a_sd.ExtensionController.EndOfSimStageChangeMO(a_simType, a_t, a_sd, this, out ChangableMOValues changable);

        if (changable != null)
        {
            if (changable.MoNeedDateSet)
            {
                MoNeedDate = changable.MoNeedDate;
            }

            if (changable.NeedDateTicksSet)
            {
                NeedDateTicks = changable.NeedDateTicks;
                changeTypes |= PostSimStageChangeTypes.NeedDate;
            }

            if (changable.RequiredQtySet)
            {
                SetRequiredQty(a_simClock, changable.RequiredQty, a_sd.ProductRuleManager);
                changeTypes |= PostSimStageChangeTypes.RequiredQuantity;
                a_sd.ExtensionController.DataChanges.FlagProductionChanges(Id);
            }

            if (changable.UserFieldListSet)
            {
                //UpdateUserFields(changable.UserFields);
                changeTypes |= PostSimStageChangeTypes.UserFields;
            }
        }

        return changeTypes;
    }

    /// <summary>
    /// Needed for one of ManufacturingOrder's Unschedule() functions.
    /// </summary>
    internal enum UnscheduleType
    {
        /// <summary>
        /// Unschedule the MO with normal processing. For instance successor MOs are also triggered to be unscheduled.
        /// </summary>
        Normal,

        /// <summary>
        /// The MO or Job is being deleted. Successor MOs aren't unscheduled.
        /// </summary>
        Deletion,

        /// <summary>
        /// The Job is being cancelled. Successor MOs aren't unscheduled.
        /// </summary>
        Cancelling,

        /// <summary>
        /// The MO is being replaced with another MO. Successor MOs aren't unscheudled.
        /// </summary>
        Replacement
    }

    /// <summary>
    /// Unschedule the MO.
    /// </summary>
    /// <param name="earliestUnscheduleTime">The scheduled start date of the earliest scheduled activity.</param>
    /// <param name="deletingOrCancelingMO">true if the MO is being deleted because it is being deleted.</param>
    /// Returns whether this MO was Scheduled and now Unscheduled
    internal bool Unschedule(UnscheduleType a_unscheduleType = UnscheduleType.Normal, bool a_clearLocks = true, UnscheduleHelper a_unscheduleHelper = null)
    {
        UsedAsSubComponent = false;

        bool scheduled = Scheduled;

        if (scheduled)
        {
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            while (alternateNodesEnumerator.MoveNext())
            {
                AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                InternalOperation baseOperation = node.Operation;
                baseOperation.Unschedule(a_clearLocks);
            }
        }

        bool unscheduleSuccessorMOs;

        if (Finished)
        {
            unscheduleSuccessorMOs = false;
        }
        else
        {
            switch (a_unscheduleType)
            {
                case UnscheduleType.Normal:
                case UnscheduleType.Cancelling:
                case UnscheduleType.Replacement:
                    unscheduleSuccessorMOs = true;
                    break;
                case UnscheduleType.Deletion:
                    if (SuccessorMOs.Count > 0)
                    {
                        //Don't unschedule successors, but signal that there was a change
                        ScenarioDetail.SignalMOChanges();
                    }

                    unscheduleSuccessorMOs = false;
                    break;
                default:
                    throw new PTException("The ManufacturingOrder.UnscheduleType isn't supported within Unschedule().");
            }
        }

        if (unscheduleSuccessorMOs)
        {
            if (a_unscheduleHelper == null)
            {
                a_unscheduleHelper = new UnscheduleHelper();
            }

            for (int i = 0; i < SuccessorMOs.Count; ++i)
            {
                Job j = SuccessorMOs[i].SuccessorManufacturingOrder?.Job;
                a_unscheduleHelper.AddJobToUnschedule(j);
            }

            while (!a_unscheduleHelper.UnscheduledJobsIsEmpty)
            {
                Job j = a_unscheduleHelper.GetNextJobToUnschedule();
                j.Unschedule(false, a_unscheduleHelper);
            }
        }

        //if this MO was not unscheduled, no need to go through the job to update statuses.
        if (scheduled)
        {
            m_job.UnscheduledMONotification(this);
        }

        return scheduled;
    }

    internal void UnscheduleNonResourceUsingOperations()
    {
        CurrentPath.UnscheduleNonResourceUsingOperations();
    }

    /// <summary>
    /// Unschedules successor MOs that are marked for unscheduling.
    /// </summary>
    internal bool UnscheduleMarkedSuccessorMOs()
    {
        return SuccessorMOs.UnscheduleMarkedSuccessorMOs();
    }

    /// <summary>
    /// Returns the operation with the earliest JitStartDate of all of the AlternatePath's Operations.
    /// </summary>
    /// <returns></returns>
    private ResourceOperation GetJitReleaseDate(AlternatePath a_path)
    {
        long jitRelease = PTDateTime.MaxDateTime.Ticks;
        ResourceOperation op = null;

        AlternatePath.NodeCollection effectiveLeaves = a_path.EffectiveLeaves;

        for (int i = 0; i < effectiveLeaves.Count; ++i)
        {
            AlternatePath.Node node = effectiveLeaves[i];
            if (node.Operation.DbrJitStartDateTicks < jitRelease)
            {
                jitRelease = node.Operation.DbrJitStartDateTicks;
                op = node.Operation;
            }
        }

        return op;
    }

    internal void SetupWaitForAnchorDroppedFlag()
    {
        IDictionaryEnumerator enumerator = OperationManager.OperationsHashInternal.GetEnumerator();

        while (enumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)enumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            op.SetupWaitForAnchorDroppedFlag();
        }
    }

    /// <summary>
    /// Only valid during simulation.
    /// Determines whether the MO has been released.
    /// No operations can be scheduled until the MO has been released. Once released individual operations may have other constraints that
    /// prevent them from being schedulable.
    /// </summary>
    public bool Released
    {
        get
        {
            if (!Job.WaitingForSimRelease)
            {
                return false;
            }

            if (!MOReleasedDateEventOccurred)
            {
                return false;
            }

            if (WaitingOnPredecessors)
            {
                return false;
            }

            if (WaitingOnHoldReleasedEvent)
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Call this function during a simulation after the MOs activities have been unscheduled.
    /// It marks the MO as having been unscheduled and unschedules any activities that are scheduled
    /// for post processing only without need for any resource time.
    /// </summary>
    internal void UnscheduleNotification()
    {
        if (!UnscheduledMOMarker)
        {
            UnscheduledMOMarker = true;
            UnscheduleNonResourceUsingOperations();
        }
    }

    #region Debugging
    public override string ToString()
    {
        string str = string.Format("{0};  ManufacturingOrder: Name '{1}';  ExternalId '{2}';", Job, Name, ExternalId);
        #if DEBUG
        str = str + string.Format("  Id={0};", Id);
        #endif
        return str;
    }
    #endregion

    #region Anchoring
    /// <summary>
    /// Anchored Activities move less (in time) during Optimizations and stay on the same resource. Manual moves are allowed but require confirmation.
    /// </summary>
    public anchoredTypes Anchored
    {
        get
        {
            int anchoredCount = 0;
            int internalOpCount = 0;
            IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
            while (operationsEnumerator.MoveNext())
            {
                DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
                BaseOperation op = (BaseOperation)de.Value;
                InternalOperation internalOp;
                if (op is InternalOperation)
                {
                    internalOp = (InternalOperation)op;
                    internalOpCount++;
                    if (internalOp.Anchored == anchoredTypes.SomeActivitiesAnchored)
                    {
                        return anchoredTypes.SomeActivitiesAnchored; //No need to go further.  If one Operation is partially anchored then the MO is partially Anchored.
                    }

                    if (internalOp.Anchored == anchoredTypes.Anchored)
                    {
                        anchoredCount++;
                    }
                }
            }

            if (anchoredCount == 0)
            {
                return anchoredTypes.Free;
            }

            if (anchoredCount == internalOpCount)
            {
                return anchoredTypes.Anchored;
            }

            return anchoredTypes.SomeActivitiesAnchored;
        }
    }

    /// <summary>
    /// Anchor/Unanchor all Internal Operations in the Manufacturing Order.
    /// </summary>
    /// <param name="anchor"></param>
    internal void Anchor(bool a_anchor, ScenarioOptions a_scenarioOptions)
    {
        for (int i = 0; i < CurrentPath.NodeCount; ++i)
        {
            AlternatePath.Node node = CurrentPath[i];
            InternalOperation op = (InternalOperation)node.Operation;
            op.SetAnchor(a_anchor, a_scenarioOptions);
        }
    }

    #region Reanchoring after a simulation
    /// <summary>
    /// Used during simulations to setup the activity for reanchoring at simulation end. This function only has an affect if the activity was anchored prior to being
    /// called. At simulation completion call Reanchor() to complete this process.
    /// </summary>
    internal void ReanchorSetup()
    {
        IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
        while (operationsEnumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            if (op is InternalOperation && op.IsNotFinishedAndNotOmitted)
            {
                ((InternalOperation)op).ReanchorSetup();
            }
        }
    }

    /// <summary>
    /// Reanchors activities after simulation. This function call only affects activities that were anchored prior to simulation start. ReanchorSetup()
    /// must have been called on this activity before the start of the simulation.
    /// </summary>
    internal void Reanchor(ScenarioOptions a_scenarioOptions)
    {
        IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
        while (operationsEnumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            if (op is InternalOperation && CurrentPath.ContainsOperation(op.ExternalId))
            {
                ((InternalOperation)op).Reanchor(a_scenarioOptions);
            }
        }
    }
    #endregion
    #endregion

    #region Quantity Adjustments
    internal bool AdjustRequiredQtyByRatio(ManufacturingOrder a_copyMO, decimal a_adjustmentRatio, ProductRuleManager a_productRuleManager)
    {
        a_copyMO.AdjustRequiredQtyByRatioHelper(a_adjustmentRatio, a_productRuleManager);

        // Reduce the amount from the original order.
        decimal newSourceSplitRatio = 1 - a_adjustmentRatio;
        bool needsToUnscheduleJob = AdjustRequiredQtyByRatioHelper(newSourceSplitRatio, a_productRuleManager);

        SplitBlocks(a_copyMO);

        return needsToUnscheduleJob;
    }

    internal bool AdjustRequiredQtyByRatioHelper(decimal a_adjustmentRatio, ProductRuleManager a_productRuleManager)
    {
        return AdjustQtyRequiredByRatio(a_adjustmentRatio, null, null, a_productRuleManager);
    }
    
    internal void AdjustToOriginalQty(Resource a_primaryRes, ProductRuleManager a_productRuleManager)
    {
        if (!Resized)
        {
            return;
        }

        decimal ratio = OriginalQty / RequiredQty;
        AdjustQtyRequiredByRatioForStorage(ratio, a_primaryRes, a_productRuleManager);

        OriginalQty = RequiredQty;
    }

    /// <summary>
    /// Update the required finish quantity and quantities of the following types of objects:
    /// operations, activities, products, and stock material requirements.
    /// </summary>
    /// <returns> boolean for whether the job needs to unschedule if it is no longer able to schedule on the resource</returns>
    internal bool AdjustQtyRequiredByRatioForStorage(decimal a_ratio, Resource a_primaryRes, ProductRuleManager a_productRuleManager)
    {
        decimal adjustedQty = ScenarioDetail.ScenarioOptions.RoundQty(RequiredQty * a_ratio);

        OriginalQty = RequiredQty;
        RequiredQty = adjustedQty;
        ExpectedFinishQty = ScenarioDetail.ScenarioOptions.RoundQty(ExpectedFinishQty * a_ratio);

        return OperationManager.AdjustRequiredQtyForStorage(a_ratio, RequiredQty, a_primaryRes, a_productRuleManager);
    }

    internal bool Join(ManufacturingOrder a_mo, ProductRuleManager a_productRuleManager)
    {
        AlternatePath.NodeCollection primaryRoots = CurrentPath.GetRoots();
        ResourceOperation primaryRoot = (ResourceOperation)primaryRoots[0].Operation;

        AlternatePath.NodeCollection joinRoots = a_mo.CurrentPath.GetRoots();
        ResourceOperation joinRoot = (ResourceOperation)joinRoots[0].Operation;

        decimal newQty = primaryRoot.RequiredFinishQty + joinRoot.RequiredFinishQty;
        decimal ratio = newQty / primaryRoot.RequiredFinishQty;

        bool needsToUnschedule = AdjustQtyRequiredByRatio(ratio, null, null, a_productRuleManager);

        //Set the priority of the surviving job to the higher of the two priorities.
        if (a_mo.Job.Priority < Job.Priority)
        {
            Job.Priority = a_mo.Job.Priority;
        }

        return needsToUnschedule;
    }

    /// <summary>
    /// Update the required finish quantity and quantities of the following types of objects:
    /// operations, activities, products, and stock material requirements.
    /// </summary>
    /// <returns> boolean for whether the job needs to unschedule if it is no longer able to schedule on the resource</returns>
    internal bool AdjustQtyRequiredByRatio(decimal a_ratio, BaseOperation a_sourceOfChangeOp, InternalActivity a_sourceOfChangeAct, ProductRuleManager a_productRuleManager)
    {
        decimal qtyToVerify = ScenarioDetail.ScenarioOptions.RoundQty(RequiredQty * a_ratio);
        if (ScenarioDetail.ScenarioOptions.IsApproximatelyZeroOrLess(qtyToVerify))
        {
            throw new SplitRoundingException("4089", new object[] { Job.ExternalId, ExternalId });
        }

        OriginalQty = RequiredQty;
        RequiredQty = qtyToVerify;
        RequestedQty = ScenarioDetail.ScenarioOptions.RoundQty(RequestedQty * a_ratio);
        ExpectedFinishQty = ScenarioDetail.ScenarioOptions.RoundQty(ExpectedFinishQty * a_ratio);

        return OperationManager.AdjustRequiredQty(a_ratio, RequiredQty, a_sourceOfChangeOp, a_sourceOfChangeAct, a_productRuleManager);
    }

    /// <summary>
    /// Call this function when a ManufacturingOrder is being joined or unsplit with another manufacturing order.
    /// One will be deleted. The other will absorb the required finish quantity and reported values of the ManufacturingOrder that's being absorbed.
    /// </summary>
    /// <param name="absorbe">The ManufacturingOrder that's being absorbed.</param>
    internal void AbsorbReportedValues(ManufacturingOrder a_absorbe)
    {
        AlternatePaths.AbsorbeReportedValues(a_absorbe.AlternatePaths);
    }

    public ResourceOperation GetCurrentRoot()
    {
        AlternatePath.NodeCollection roots = CurrentPath.GetRoots();

        if (!AlternatePaths.AllPathsHaveOneRoot())
        {
            throw new PTValidationException("2490");
        }

        if (CurrentPath.AnyFinishedActivities())
        {
            throw new PTValidationException("2491");
        }

        AlternatePath.Node node = roots[0];
        return (ResourceOperation)node.Operation;
    }

    /// <summary>
    /// Adjust the quantity by the number of cycles.
    /// </summary>
    /// <param name="cycles"></param>
    internal void ChangeQty(decimal a_newQty, BaseOperation a_sourceOfChangeOp, InternalActivity a_sourceOfChangeAct, ProductRuleManager a_productRuleManager)
    {
        //ResourceOperation rootOp = GetRootForChangeQty();

        //decimal nbrOfCycles = rootOp.RequiredFinishQty / rootOp.QtyPerCycle;
        //nbrOfCycles = Math.Ceiling(nbrOfCycles);
        //nbrOfCycles += a_nbrOfCyclesToAddOrSubtract;

        //if (nbrOfCycles < 1)
        //{
        //    throw new PTValidationException("The adjustment can't be made because it would result in no cycles.");
        //}

        //decimal newQty = nbrOfCycles * rootOp.QtyPerCycle;
        //decimal ratio = newQty / rootOp.RequiredFinishQty;

        decimal ratio = a_newQty / RequiredQty;
        AdjustQtyRequiredByRatio(ratio, a_sourceOfChangeOp, a_sourceOfChangeAct, a_productRuleManager);
    }
    #endregion

    internal static bool AreScheduledBackToBack(ManufacturingOrder a_leftMO, ManufacturingOrder a_rightMO)
    {
        List<Pair<AlternatePath.Node, int>> mo1NodeList = a_leftMO.CurrentPath.GetNodesByLevel(true);
        List<Pair<AlternatePath.Node, int>> mo2NodeList = a_rightMO.CurrentPath.GetNodesByLevel(true);

        for (int nodeI = 0; nodeI < mo1NodeList.Count; ++nodeI)
        {
            AlternatePath.Node mo1Node = mo1NodeList[nodeI].value1;
            InternalActivityManager iam1 = ((InternalOperation)mo1Node.Operation).Activities;

            AlternatePath.Node mo2Node = mo2NodeList[nodeI].value1;
            InternalActivityManager iam2 = ((InternalOperation)mo2Node.Operation).Activities;

            if (iam1.Count > 1 || iam2.Count > 1)
            {
                return false;
            }

            InternalActivity ia1 = iam1.GetByIndex(0);
            InternalActivity ia2 = iam2.GetByIndex(0);

            for (int bI = 0; bI < ia1.ResourceRequirementBlockCount; ++bI)
            {
                ResourceBlock block1 = ia1.GetResourceRequirementBlock(bI);
                ResourceBlock block2 = ia2.GetResourceRequirementBlock(bI);
                if (block1 != null && block2 != null)
                {
                    if (block1.ScheduledResource != block2.ScheduledResource)
                    {
                        return false;
                    }

                    if (block1.ScheduledResource.CapacityType != InternalResourceDefs.capacityTypes.SingleTasking)
                    {
                        return false;
                    }

                    if (block1.MachineBlockListNode.Next != block2.MachineBlockListNode)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    internal class DifferenceTypes
    {
        internal const int timing = 1;
        internal const int setupCodes = 2;
        internal const int resourceRequirements = 4;
        internal const int any = int.MaxValue;
    }

    internal void DetermineDifferences(ManufacturingOrder a_mo, int a_differenceTypes, System.Text.StringBuilder a_warnings)
    {
        AlternatePaths.DetermineDifferences(a_mo.AlternatePaths, a_differenceTypes, a_warnings);
    }

    internal void ActivityScheduled(InternalActivity a_act)
    {
        Job.ActivityScheduled(a_act);
        FilterForCanSpanPlants(a_act);
    }

    internal void FilterForCanSpanPlants(InternalActivity a_act)
    {
        if (!CanSpanPlants && !CanSpanPlantsHandled)
        {
            CanSpanPlantsHandled = true;
            // Narrow eligibility to a single plant
            Plant p = a_act.GetScheduledPlant();
            if (p != null)
            {
                FilterForCanSpanPlants(p);
            }
        }
    }

    internal void FilterForCanSpanPlants(Plant a_plant)
    {
        CurrentPath.AdjustedPlantResourceEligibilitySets_FilterDownToSpecificPlant(a_plant.Id);
    }

    internal long GetNbrOfActivitiesToSchedule()
    {
        long nbrOfActivites = 0;
        nbrOfActivites += CurrentPath.GetNbrOfActivitiesToSchedule();
        return nbrOfActivites;
    }

    internal enum AlternatePathSatisfiability { NotSet, NoPathsSatisfiable, SomePathsNotSatisfiable, AllPathsSatisfiable }

    /// <summary>
    /// Lock and/or Anchor activities that start before a time.
    /// </summary>
    internal void LockAndAnchorBefore(OptimizeSettings.ETimePoints a_startSpan, ScenarioOptions scenarioOptions)
    {
        CurrentPath?.LockAndAnchorBefore(a_startSpan, scenarioOptions);
    }

    //++++++++
    // The two variables below are used to cache the most recently calculated Departmental Frozen or Stable Span.
    //++++++++

    /// <summary>
    /// Initialized to long.MinValue in SimulationInitialization().
    /// </summary>
    private long m_earliestDepartmentalEndOfFrozenOrStableSpan;

    /// <summary>
    /// This is initialized to null in SimulationInitialization().
    /// </summary>
    private SimulationTimePoint m_simTimeEarliestDepartmentalEndOfFrozenOrStableSpanIsBasedOn;

    /// <summary>
    /// Get the earliest departmental frozen or stable span among the non-finished non-omitted leaves.
    /// </summary>
    /// <param name="a_simStartTime">Used to calculate the end of frozen or stable span. .</param>
    /// <returns>The earliest release time among eligible resource's departments. </returns>
    internal long GetEarliestDepartmentalEndSpan(SimulationTimePoint a_simStartTime)
    {
        if (m_earliestDepartmentalEndOfFrozenOrStableSpan != long.MinValue && a_simStartTime.Equals(m_simTimeEarliestDepartmentalEndOfFrozenOrStableSpanIsBasedOn))
        {
            return m_earliestDepartmentalEndOfFrozenOrStableSpan;
        }

        m_earliestDepartmentalEndOfFrozenOrStableSpan = a_simStartTime.DateTimeTicks;

        AlternatePath.NodeCollection effectiveLeaves = CurrentPath.EffectiveLeaves;
        for (int leafNodeI = 0; leafNodeI < effectiveLeaves.Count; ++leafNodeI)
        {
            if (effectiveLeaves[leafNodeI].Operation is InternalOperation io)
            {
                long opEarliestSpan = io.GetEarliestDepartmentalEndSpan(a_simStartTime);
                m_earliestDepartmentalEndOfFrozenOrStableSpan = Math.Min(m_earliestDepartmentalEndOfFrozenOrStableSpan, opEarliestSpan);
            }
        }

        return m_earliestDepartmentalEndOfFrozenOrStableSpan;
    }

    #region Calculate when an MO or one of it's paths can be released.
    //TODO: The goal is to move this code into the operation release. So we should not need this type of drum and release calculations
    /// <summary>
    /// Calculate the release date of a path factoring in values such as the MOs effective release date and DBR release date.
    /// </summary>
    /// <param name="a_mosPath">The path whose release date is to be calculated.</param>
    /// <param name="a_earliestSimStartTicks">The earliest time the path can be released, such as the Clock or SimulationClock.</param>
    /// <param name="a_optSettings">The optimization settings have an effect on how the release date is determined.</param>
    /// <returns></returns>
    //internal long CalcPathsReleaseTicks(long a_simClock, AlternatePath a_mosPath, long a_earliestSimStartTicks, ScenarioDetail.AddMOReleaseEventArgsForOpt a_optSettings)
    //{
    //    CalcPathsReleaseTicks(a_simClock, a_mosPath, a_earliestSimStartTicks, a_optSettings, out long o_moReleaseTime);
    //    return o_moReleaseTime;
    //}

    //TODO: The goal is to move this code into the operation release. So we should not need this type of drum and release calculations
    /// <summary>
    /// Determines the release date of a path factoring in values such as the MOs effective release date and DBR release date.
    /// </summary>
    /// <param name="a_mosPath">The path whose release date is to be calculated.</param>
    /// <param name="a_earliestSimStartTicks">The earliest time the path can be released.</param>
    /// <param name="a_optSettings">The optimization settings have an effect on how the release date is determined.</param>
    /// <param name="o_releaseType">What released the manufacturing order (though not necessarily what the latest release on the path).</param>
    /// <param name="o_pathReleaseTicks">The time the path can be released.</param>
    internal long CalcPathsReleaseTicks(long a_simClock, long a_earliestSimStartTicks)
    {
        return CalcMoOrPathReleaseTicksShell(a_simClock, a_earliestSimStartTicks);
    }

    //TODO: The goal is to move this code into the operation release. So we should not need this type of drum and release calculations
    /// <summary>
    /// Used to calculate the earliest DBR release date among all the MO's paths.
    /// </summary>
    /// <param name="a_earliestSimulationStartTime">The earliest the MO can be released. For instance this could be the Clock or Simulation Clock.</param>
    /// <param name="a_optimizeSettings"></param>
    /// <param name="o_releaseType"></param>
    /// <param name="o_moReleaseTime"></param>
    //internal void CalcEarliestMODbrReleaseTicks(long a_simClock, long a_earliestSimulationStartTime, ScenarioDetail.AddMOReleaseEventArgsForOpt a_optimizeSettings, out long o_moReleaseTime)
    //{
    //    o_moReleaseTime = CalcMoOrPathReleaseTicksShell(a_simClock, CalcEarliestDbrReleaseTicks, a_earliestSimulationStartTime, a_optimizeSettings);
    //}

    //TODO: The goal is to move this code into the operation release. So we should not need this type of drum and release calculations
    /// <summary>
    /// For use as the delegate to GetMoOrPathReleaseTicksShell().
    /// Used to calculate the earliest DBR release date among all the MO's paths.
    /// </summary>
    /// <param name="a_optimizeJitSlackTicks">The optimize's JIT slack ticks.</param>
    /// <param name="a_earliestReleaseTicks">The earliest the MO can be released.</param>
    /// <param name="a_mosPath">Pass in null. This value isn't used. It's only a place holder for the delegate this function is designed for.</param>
    /// <returns>The earliest release ticks among all the paths.</returns>
    //private long CalcEarliestDbrReleaseTicks(long a_simClock, long a_optimizeJitSlackTicks, bool a_useResourceCapacityForHeadstart, long a_earliestReleaseTicks, AlternatePath a_mosPath)
    //{
    //    long earliestDbrReleaseTicks = long.MaxValue;

    //    for (int pathI = 0; pathI < AlternatePaths.Count; ++pathI)
    //    {
    //        bool constrained = GetConstrainedReleaseTicks(out long moEffectiveReleaseTicks);
    //        if (constrained)
    //        {
    //            earliestDbrReleaseTicks = Math.Min(earliestDbrReleaseTicks, moEffectiveReleaseTicks);
    //        }
    //    }

    //    if (earliestDbrReleaseTicks == long.MaxValue)
    //    {
    //        earliestDbrReleaseTicks = a_earliestReleaseTicks;
    //    }

    //    earliestDbrReleaseTicks = Math.Max(earliestDbrReleaseTicks, a_earliestReleaseTicks);
    //    return earliestDbrReleaseTicks;
    //}

    /// <summary>
    /// Common shell functionality for determining the MO release date or release date of an MO's path.
    /// The internal details of determining the DBR release date must be defined by the caller through a delegate passed to this function.
    /// </summary>
    /// <param name="a_dbrhanlder">A delegate that specifies how to calculate the DBR release date.</param>
    /// <param name="a_earliestSimulationStartTime">The earliest the release date can be.</param>
    /// <param name="a_optimizeSettings"></param>
    /// <param name="o_releaseType">The type of release MO release.</param>
    /// <param name="a_mosPath">An optional parameter whose default value is null.</param>
    /// <returns></returns>
    private long CalcMoOrPathReleaseTicksShell(long a_simClock, long a_earliestSimulationStartTime)
    {
        bool constrained = GetConstrainedReleaseTicks(out long moEffectiveReleaseTicks);
        long moReleaseTicks = Math.Max(a_earliestSimulationStartTime, moEffectiveReleaseTicks);
        
        return Math.Max(a_simClock, moReleaseTicks);
    }

    /// <summary>
    /// Used to define the internal operation of GetManufacturingOrderReleaseDateTicksShell();
    /// </summary>
    /// <param name="a_mo">The MO whose release date is to be determined.</param>
    /// <param name="a_optimizeJitSlackTicks">The Optimize simulations JIT slack ticks.</param>
    /// <param name="a_moReleaseTime">The time the MO can be released.</param>
    /// <param name="a_mosPath">The path whose release date is to be determined.</param>
    /// <returns></returns>
    private delegate long MoDbrDelegate(long a_simClock, long a_optimizeJitSlackTicks, bool a_useResourceCapacityForHeadstart, long a_moReleaseTime, AlternatePath a_mosPath);

    //TODO: The goal is to move this code into the operation release. So we should not need this type of drum and release calculations
    /// <summary>
    /// Get the Path's DBR release ticks.
    /// </summary>
    /// <param name="a_simClock"></param>
    /// <param name="a_mosPath">The path whose release ticks are to be determined.</param>
    /// <param name="a_optimizeJitSlackTicks">The optimizes JIT slack ticks.</param>
    /// <param name="a_useResourceCapacityForHeadstart"></param>
    /// <param name="a_moReleaseTicks">The MO release ticks.</param>
    /// <returns></returns>
    //internal long CalcPathDbrReleaseTicks(long a_simClock, AlternatePath a_mosPath, long a_optimizeJitSlackTicks, bool a_useResourceCapacityForHeadstart, long a_moReleaseTicks)
    //{
    //    long releaseTicks = CalcDbrReleaseTicks(a_simClock, a_mosPath, a_optimizeJitSlackTicks, a_useResourceCapacityForHeadstart);
    //    if (!(releaseTicks >= PTDateTime.MaxDateTime.Ticks))
    //    {
    //        releaseTicks = Math.Max(a_moReleaseTicks, releaseTicks);
    //        return releaseTicks;
    //    }

    //    return a_moReleaseTicks;
    //}
    #endregion

    #region DBR
    //TODO: The goal is to move this code into the operation release. So we should not need this type of drum and release calculations
    /// <summary>
    /// The datetime when the MO should be released.
    /// If this Job Classification is Buffer Stock Replenishment then this is the Manufacturing Order's regular Release Date.
    /// Otherwise, if the MO uses a Drum then this is the Drum Due Date minus the Drum Buffer.
    /// Otherwise this is the JIT Start Date minus the Shipping Buffer.
    /// </summary>
    /// <returns>The DBR release date in ticks.</returns>
    //private long CalcDbrReleaseTicks(long a_simClock, AlternatePath a_path, long a_optimizeJitSlackTicks, bool a_useResourceCapacityForHeadstart)
    //{
    //    long releaseTicks = PTDateTime.MinDateTime.Ticks;

    //    if (!Finished)
    //    {
    //        // Either the DBR or earliest JIT operation.
    //        ResourceOperation bufferOp = null;
    //        if (Job.Classification == JobDefs.classifications.BufferStockReplenishment) //No NeedDate dependent at all.
    //        {
    //            releaseTicks = ReleaseTicks;
    //        }
    //        else
    //        {
    //            DrumInfo drumInfo = GetDrumInfo(a_path, this);

    //            if (drumInfo.HasDrum)
    //            {
    //                releaseTicks = drumInfo.DbrReleaseDate.Ticks;
    //                bufferOp = drumInfo.DrumOperation;
    //            }
    //            else
    //            {
    //                bufferOp = GetJitReleaseDate(a_path);
    //                if (bufferOp == null)
    //                {
    //                    CalculateJitTimes(a_simClock, a_path);
    //                    bufferOp = GetJitReleaseDate(a_path);
    //                }

    //                releaseTicks = bufferOp.JITStartDateTicks;
    //            }
    //        }

    //        if (bufferOp != null)
    //        {
    //            releaseTicks = bufferOp.DBRJITStartTicks;

    //            if (a_useResourceCapacityForHeadstart)
    //            {
    //                //Now take into account capacity based headstart
    //                InternalResource capacityToUse = bufferOp.Scheduled ? bufferOp.GetScheduledPrimaryResource() : (bufferOp as InternalOperation).ResourceRequirements.GetFirstEligiblePrimaryResource();

    //                if (capacityToUse != null)
    //                {
    //                    DateTime capacityAdjustedRelease = capacityToUse.GetAdjustedDateTime(a_simClock, releaseTicks, a_optimizeJitSlackTicks);
    //                    releaseTicks = capacityAdjustedRelease.Ticks;
    //                }
    //            }
    //        }
    //        //This is accoutned for in the JIT calculations
    //        //long productDbrShippingBuffer = GetProductDbrShippingBufferTicks();
    //        //releaseTicks -= productDbrShippingBuffer;

    //        //If we are not using the 'account for offline intervals' option we will need to subtract the JITSlackTicks from releaseTicks.
    //        if (!a_useResourceCapacityForHeadstart)
    //        {
    //            releaseTicks -= a_optimizeJitSlackTicks;
    //        }
    //    }

    //    return releaseTicks;
    //}

    /// <summary>
    /// The Shipping Buffer of the Product. Zero if no Product.
    /// </summary>
    /// <returns></returns>
    private long GetProductShippingBufferTicks()
    {
        long shippingBuffer = 0;
        if (ShippingBufferOverrideTicks.HasValue)
        {
            shippingBuffer = ShippingBufferOverrideTicks.Value;
        }
        else
        {
            Product product = GetFirstProductInCurrentPath();
            if (product != null)
            {
                shippingBuffer = Math.Max(shippingBuffer, product.Inventory.ShippingBufferTicks);
            }
        }

        return shippingBuffer;
    }

    #region EffectiveRelease
    /// <summary>
    /// Used to describe the result return by GetEffectiveReleaseDate().
    /// </summary>
    internal enum EffectiveReleaseDateType
    {
        /// <summary>
        /// The MO may begin ASAP.
        /// </summary>
        Unconstrained,

        /// <summary>
        /// A finished predecessor MO constrains when the MO can start.
        /// </summary>
        PredecessorMO,

        /// <summary>
        /// The values hasn't been set yet.
        /// </summary>
        NotSet
    }

    /// <summary>
    /// This takes the IsReleased field into consideration.
    /// If IsReleased is true then this property will return PTDateTime.MinValue.Ticks.
    /// In addition release dates of any finished predecessor MOs are taken into consideration.
    /// </summary>
    internal long EffectiveReleaseDate
    {
        get
        {
            if (GetConstrainedReleaseTicks(out long constrainedReleaseTicks))
            {
                return constrainedReleaseTicks;
            }

            return PTDateTime.MinValue.Ticks;
        }
    }

    /// <summary>
    /// This takes the IsReleased field into consideration.
    /// If IsReleased is true then this property will return DateTime.MinValue.Ticks.
    /// In addition release dates of any finished predecessor MOs are taken into consideration.
    /// </summary>
    /// <param name="releaseType">The type of the most constraining constraint.</param>
    /// <returns></returns>
    public bool GetConstrainedReleaseTicks(out long o_constrainedReleaseTicks)
    {
        if (m_finishedPredecessorMOReleaseInfoManager.MaximumReleaseTicks > 0)
        {
            o_constrainedReleaseTicks = m_finishedPredecessorMOReleaseInfoManager.MaximumReleaseTicks;
            return true;
        }

        o_constrainedReleaseTicks = -1;
        return false;
    }
    #endregion
    #endregion

    public void Omit()
    {
        for (var i = 0; i < OperationManager.Count; i++)
        {
            BaseOperation baseOperation = OperationManager.GetByIndex(i);
            baseOperation.Omitted = BaseOperationDefs.omitStatuses.OmittedAutomatically;
        }
    }
}