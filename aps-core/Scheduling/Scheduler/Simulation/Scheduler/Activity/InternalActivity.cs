using PT.APSCommon;
using PT.Common.Extensions;
using PT.Common.Localization;
using PT.Scheduler.Schedule;
using PT.Scheduler.Schedule.Operation;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Customizations;
using PT.Scheduler.Simulation.Extensions;
using PT.SchedulerDefinitions;
using System.ComponentModel;

using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// A portion of the Operation to be completed.  There is one InternalActivity per Operation unless it is Split into multiple Activities.
/// </summary>
public partial class InternalActivity : ILotGeneratorObject
{
    #region Eligibility
    /// <summary>
    /// </summary>
    private ResReqsPlantResourceEligibilitySets m_resReqsEligibilityNarrowedDuringSimulation;

    /// <summary>
    /// This is initialized to referenece the variable of the same name in AlternatePaths.Node, later if it diverges for the activity, a copy of what's in AlternatePaths.Node
    /// is made.
    /// </summary>
    internal ResReqsPlantResourceEligibilitySets ResReqsEligibilityNarrowedDuringSimulation
    {
        get
        {
            if (m_resReqsEligibilityNarrowedDuringSimulation != null)
            {
                return m_resReqsEligibilityNarrowedDuringSimulation;
            }

            return Operation.AlternatePathNode.ResReqsEligibilityNarrowedDuringSimulation;
        }
    }

    /// <summary>
    /// Call this function to alter resource eligibility of the activity. Eligibility of the activity is initialized to that of the operation. This function can only be called
    /// once per activity during a simulation.
    /// </summary>
    internal void BreakAwayResReqsEligibility()
    {
        #if DEBUG
        if (m_resReqsEligibilityNarrowedDuringSimulation != null)
        {
            throw new Exception("The break away has already been done.");
        }
        #endif
        m_resReqsEligibilityNarrowedDuringSimulation = new ResReqsPlantResourceEligibilitySets(Operation.AlternatePathNode.ResReqsEligibilityNarrowedDuringSimulation);
    }

    internal bool HasResReqEligibilityBeenOverridden()
    {
        return m_resReqsEligibilityNarrowedDuringSimulation != null;
    }

    /// <summary>
    /// No filtering occurs if the activity's eligibility has already been overridden.
    /// </summary>
    internal void AdjustedPlantResourceEligibilitySets_Filter(List<BaseId> a_doNotFilterResourceIds)
    {
        if (!HasResReqEligibilityBeenOverridden())
        {
            if (ResReqsEligibilityNarrowedDuringSimulation.IsAnyEligibleResManualSchedulingOnly())
            {
                BreakAwayResReqsEligibility();
                ResReqsEligibilityNarrowedDuringSimulation.FilterManualSchedulingOnlyResources(this, a_doNotFilterResourceIds);
            }
        }
    }
    #endregion

    #region Simulation, unscheduling, simulation state variables.
    internal class SequentialState
    {
        private bool m_hasSequencedLeftNeighbor;

        internal bool HasSequencedLeftNeighbor
        {
            get => m_hasSequencedLeftNeighbor;
            set => m_hasSequencedLeftNeighbor = value;
        }

        private bool m_leftSequencedNeighborScheduled;

        internal bool LeftSequencedNeighborScheduled
        {
            get => m_leftSequencedNeighborScheduled;
            set => m_leftSequencedNeighborScheduled = value;
        }

        // [BATCH_CODE]
        internal void SetRightSequencedNeighbor(Batch a_batch, int a_rrIdx)
        {
            m_rightSequencedBatchNeighbor = a_batch;
            m_rightSequencedNeighborResourceRequirementIndex = a_rrIdx;
        }

        // [BATCH_CODE]
        internal void ClearRightSequencedNeighbor()
        {
            m_rightSequencedBatchNeighbor = null;
            m_rightSequencedNeighborResourceRequirementIndex = -1;
        }

        private Batch m_rightSequencedBatchNeighbor; // [BATCH]

        internal Batch RightSequencedBatchNeighbor => m_rightSequencedBatchNeighbor;

        private int m_rightSequencedNeighborResourceRequirementIndex;

        internal int RightSequencedNeighborResourceRequirementIndex => m_rightSequencedNeighborResourceRequirementIndex;

        private Resource m_sequencedSimulationResource;

        internal Resource SequencedSimulationResource
        {
            get => m_sequencedSimulationResource;
            set => m_sequencedSimulationResource = value;
        }

        /// <summary>
        /// When the RR is released.
        /// </summary>
        internal long ReleaseTicks { get; set; }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new ();

            string res = m_sequencedSimulationResource == null ? "null" : m_sequencedSimulationResource.Name + " " + m_sequencedSimulationResource.Description;

            sb.Append("Res: " + res);

            if (HasSequencedLeftNeighbor)
            {
                sb.Append("; HasSeqLeftNghbr=true");
                sb.Append("; LftSeqNghBrSchd=" + LeftSequencedNeighborScheduled);
            }

            if (RightSequencedBatchNeighbor != null)
            {
                sb.Append("; HasRghtNghbr=true");
                sb.Append("; RghtBth=" + RightSequencedBatchNeighbor);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Each entry in this array corresponds to a resource requirement.
    /// </summary>
    private SequentialState[] m_sequential;

    private ActSimData m_simData;

    /// <summary>
    /// Extra data necessary for the simulation. It initially uses a fixed amount that can be used to dynamically
    /// use an unlimited amount of extra data on an as needed basis as opposed to declaring data members for
    /// fields that aren't necessary for all customers. That is memory is only consumed on an as-needed basis.
    /// </summary>
    internal ActSimData SimData => m_simData;

    /// <summary>
    /// Each index corresponds to a resource requirement.
    /// </summary>
    internal SequentialState[] Sequential => m_sequential;

    internal void InitSequentialSim()
    {
        if (m_sequential == null)
        {
            m_sequential = new SequentialState[Operation.ResourceRequirements.Count];

            for (int i = 0; i < Operation.ResourceRequirements.Count; ++i)
            {
                m_sequential[i] = new SequentialState();
            }
        }
    }

    internal override bool Released
    {
        get
        {
            if (!base.Released)
            {
                return false;
            }

            if (OpOrPredOpBeingMoved)
            {
                if (WaitForRightMovingNeighborReleaseEvent ||
                    WaitForScheduledDateBeforeMoveEvent ||
                    WaitForPredecessorOperationReleaseEvent)
                {
                    return false;
                }
            }

            if (WaitForActivityMovedEvent)
            {
                return false;
            }

            if (WaitForClockAdjustmentRelease)
            {
                return false;
            }

            // The activity is not released if it is to be scheduled sequentially and
            // any of its left neighbors haven't been scheduled.
            if (m_sequential != null)
            {
                for (int sequentialI = 0; sequentialI < m_sequential.Length; ++sequentialI)
                {
                    SequentialState ss = m_sequential[sequentialI];
                    if (ss.HasSequencedLeftNeighbor && ss.LeftSequencedNeighborScheduled == false)
                    {
                        return false;
                    }
                }
            }

            if (m_moveRelatedConstrainersCount > 0)
            {
                return false;
            }

            // Only check the operation Released if the activityity isn't in production.
            // If the activity is in production, the operation release doesn't matter since
            // it's not acting as a constraint on the activity's start. 
            if (!InProduction())
            {
                if (!Operation.Released)
                {
                    return false;
                }
            }

            if (WaitForLeftMoveBlockToSchedule)
            {
                return false;
            }

            // [USAGE_CODE] Released: Don't release the activity if the IntersectsMoveConstraint is in effect.
            if (PreventMoveIntersectionEvent)
            {
                return false;
            }

            return true;
        }
    }

    #region Move
    #region Move related constrained activities
    // [TANK_CODE]: Related constraining activities.
    private Dictionary<BaseId, InternalActivity> m_moveRelatedConstrainedActivities;

    /// <summary>
    /// Under some conditions (such as with some routing tank situations where activities are moved to the same tank), it's possible for a tank deadlock
    /// to occur. The conditions are detected during the move and the related activities that could cause a deadlock and end up with a job partially scheduled
    /// are stored in this set. These activities are constrained by this activity. They are released when the activity is scheduled.
    /// </summary>
    internal Dictionary<BaseId, InternalActivity> MoveRelatedConstrainedActivities => m_moveRelatedConstrainedActivities;

    /// <summary>
    /// Add an activity that this activity will constrain during a move and configure the constrained activity to be constrained.
    /// </summary>
    /// <param name="a_act"></param>
    internal void AddMoveRelatedConstrainedActivity(InternalActivity a_act)
    {
        if (m_moveRelatedConstrainedActivities == null)
        {
            m_moveRelatedConstrainedActivities = new Dictionary<BaseId, InternalActivity>();
        }

        if (!m_moveRelatedConstrainedActivities.ContainsKey(a_act.Id))
        {
            m_moveRelatedConstrainedActivities.Add(a_act.Id, a_act);
            ++a_act.m_moveRelatedConstrainersCount;
        }
    }

    internal void ReleaseMoveRelatedConstrainedActivities()
    {
        if (m_moveRelatedConstrainedActivities != null)
        {
            Dictionary<BaseId, InternalActivity>.Enumerator etr = m_moveRelatedConstrainedActivities.GetEnumerator();
            while (etr.MoveNext())
            {
                --etr.Current.Value.m_moveRelatedConstrainersCount;
            }
        }
    }

    /// <summary>
    /// The number of related activities that are constraining this activity.
    /// </summary>
    private long m_moveRelatedConstrainersCount;
    #endregion

    private Resource[] m_moveResources;

    /// <summary>
    /// Used to get the resource an activity's resource requirement must be scheduled on.
    /// This function returns false if the Move logic doesn't fix it to a specific resource.
    /// </summary>
    internal bool GetMoveResource(int a_resourceRequirementIndex, out Resource o_moveResource)
    {
        if (m_moveResources != null)
        {
            o_moveResource = m_moveResources[a_resourceRequirementIndex];
            return o_moveResource != null;
        }

        o_moveResource = null;
        return false;
    }

    internal Resource GetMoveResource(int a_rrIdx)
    {
        Resource res = null;
        if (GetMoveResource(a_rrIdx, out Resource tmp))
        {
            res = tmp;
        }

        return res;
    }

    /// <summary>
    /// Initialize which resource will satisfy resource requirements with the current set of resources the requirements are scheduled on.
    /// </summary>
    internal void InitMoveRRSet()
    {
        m_moveResources = CreateArrayOfRRSatiators();
    }

    /// <summary>
    /// Specify how a resource requirements should be satisfied. InitMoveRRSet must have been called before this function.
    /// </summary>
    internal void SetMoveResource(Resource a_resource, int a_resourceRequirementIndex)
    {
        m_moveResources[a_resourceRequirementIndex] = a_resource;
    }

    /// <summary>
    /// For each resource requirement, specify which resource will be used to satisfy it.
    /// </summary>
    /// <param name="a_moveResources">
    /// Don't edit this array. It will be directly referenced by this object from this point on. For each resource requirement, reference the resource that should be used to
    /// satisfy it.
    /// </param>
    internal void InitMoveRRSet(Resource[] a_moveResources)
    {
        m_moveResources = a_moveResources;
    }

    /// <summary>
    /// Reserve times when blocks will schedule on the resources the activity will schedule on.
    /// </summary>
    /// <param name="a_reserveStartTicks"></param>
    internal void ReserveMoveResources(long a_simClock, long a_reserveStartTicks, SchedulableInfo a_moveSI)
    {
        SchedulableInfo si = a_moveSI;

        // Set the resource reserve dates.
        for (int i = 0; i < m_moveResources.Length; ++i)
        {
            InternalResource ir = m_moveResources[i];

            if (ir is Resource resource)
            {
                long endTicks;

                ResourceRequirement rr = Operation.ResourceRequirements.GetByIndex(i);
                long startTicks = si.GetUsageStart(rr);

                //Can't reserve clean because it can change with changing sequence of Activities
                MainResourceDefs.usageEnum usageEnd = rr.UsageEnd;
                if (usageEnd == MainResourceDefs.usageEnum.Clean)
                {
                    endTicks = si.m_postProcessingFinishDate;
                }
                else
                {
                    endTicks = si.GetUsageEnd(rr);
                }

                //If it's the primary resource requirement we want to constrain the reservation start ticks by the 
                //adjusted move start ticks
                if (i == 0)
                {
                    startTicks = a_reserveStartTicks;
                }

                resource.SetReservedMoveDate(new ActivityKey(Job.Id, ManufacturingOrder.Id, Operation.Id, Id), startTicks, endTicks);
            }
        }
    }

    internal void ClearMoveResourceReservations()
    {
        for (int i = 0; i < m_moveResources.Length; i++)
        {
            InternalResource ir = m_moveResources[i];
            if (ir != null)
            {
                if (ir is Resource)
                {
                    Resource r = (Resource)ir;
                    r.ClearReservedMoveTime();
                }
            }
        }
    }

    /// <summary>
    /// Eligibility of the successor operation changed when the predecessor operation scheduled.
    /// For example resource connectors
    /// </summary>
    public void ClearMoveResources()
    {
        m_moveResources = null;
    }

    /// <summary>
    /// Returns a new array that specifies how resource requirements are currently satisfied.
    /// The resulting array will contain null values for requirements that aren't scheduled.
    /// </summary>
    /// <returns>Each index corresponds the resource requirement of the same index. The value at an index will be null if the requirement isn't scheduled.</returns>
    internal Resource[] CreateArrayOfRRSatiators()
    {
        Resource[] rrSatiators = new Resource[Operation.ResourceRequirements.Count];

        if (Batch != null) // It's possible the activity isn't scheduled. For instance changing paths, it's not scheduled until after the move.
        {
            for (int i = 0; i < Batch.BlockCount; ++i)
            {
                ResourceBlock rb = Batch.BlockAtIndex(i);
                if (rb != null)
                {
                    rrSatiators[i] = rb.ScheduledResource;
                }
                else
                {
                    rrSatiators[i] = null;
                }
            }
        }

        return rrSatiators;
    }
    #endregion

    #region Variables for regular activity moves.
    //		[NonSerialized]
    private InternalActivity m_rightMovingNeighbor;

    /// <summary>
    /// Non-serialized simulation state variable.
    /// If set then this indicates the right neighbor of this activity
    /// before one of the right neighbors predecessor activities was moved.
    /// </summary>
    internal InternalActivity RightNeighbor
    {
        get => m_rightMovingNeighbor;

        set => m_rightMovingNeighbor = value;
    }

    /// <summary>
    /// This is a simulation only variable.
    /// If the feature to prepend setup time of move blocks right neighbors
    /// is enabled,
    /// the move blocks' right neighbor is set to the predecessor of the block being moved so
    /// setup time can be calculated and the right neighbor can be released this much earlier
    /// so hopefully its end date won't be affectect by the addition of setup time.
    /// This value is initialized to what will become the new left neighbor of the the
    /// move block's right neighbor.
    /// [PrependSetupToMoveBlocksRightNeighbors] 2-2: Identifies what will be the new left neighbor of the move blocks right neighbors.
    /// </summary>
    internal ResourceBlockList.Node PrependSetupToMoveBlocksRightNeighborsNewLeftNeighbor { get; set; }
    #endregion

    /// <summary>
    /// Performed before any SimulationInitializations on all objects within the system.
    /// </summary>
    internal override void ResetSimulationStateVariables(ScenarioDetail a_sd)
    {
        base.ResetSimulationStateVariables(a_sd);

        m_sequential = null;
        m_moveResources = null;
        m_simulationFlags.Clear();

        // Initialization of simulation state variables.
        RightNeighbor = null;

        m_moBatchLockedResource = null;

        PrependSetupToMoveBlocksRightNeighborsNewLeftNeighbor = null;

        a_sd.InitializeCache(ref m_setupGroupQtyCache);
        a_sd.InitializeCache(ref m_setupGroupHoursCache);
        a_sd.InitializeCache(ref m_productGroupQtyCache);
        a_sd.InitializeCache(ref m_productGroupHoursCache);
        a_sd.InitializeCache(ref m_cachedOnTimeStatus);

        if (Batch != null)
        {
            OriginalScheduledStartTicks = Batch.StartTicks;
        }
        else
        {
            OriginalScheduledStartTicks = 0;
        }

        if (a_sd.ActiveSimulationType != ScenarioDetail.SimulationType.Optimize)
        {
            //When resequencing, we should not use this Idx as it will prioritize operations based on previous start date
            OriginalSimultaneousSequenceIdxScore = OriginalScheduledStartTicks + SimultaneousSequenceIdx;
        }
        else
        {
            OriginalSimultaneousSequenceIdxScore = 0;
        }

        //m_flaggedDispatchers.Clear();
    }

    /// <summary>
    /// Returns the portion of the primary product this activity represents
    /// </summary>
    internal decimal PrimaryProductQty
    {
        get
        {
            if (Operation.Products.PrimaryProduct is Product primary)
            {
                IEnumerator<Product> products = Operation.Products.GetEnumerator();
                int prodIdx = 0;
                while (products.MoveNext())
                {
                    if (products.Current == primary)
                    {
                        return m_productQtys[prodIdx];
                    }

                    prodIdx++;
                }
            }

            return 0m;
        }
    }

    /// <summary>
    /// There is one entry in this array for each product being produced by the operation.
    /// Each entry indicated how much of the corresponding product is produced by this activity.
    /// These values should be set in the simulation initialization code of the operation.
    /// </summary>
    internal decimal[] m_productQtys;
    internal Dictionary<BaseId, decimal> m_mrQtys = new ();

    /// <summary>
    /// Performed after RestSimulationStateVariables() on all objects in the system.
    /// </summary>
    internal void SimulationInitialization(PlantManager a_plantManager, ProductRuleManager a_productRuleManager, ExtensionController a_extensionController, ICalculatedValueCacheManager a_cacheManager)
    {
        if (m_productionStatus > InternalActivityDefs.productionStatuses.Ready)
        {
            Operation.ManufacturingOrder.ActivitiesScheduled = true;
        }

        m_resReqsEligibilityNarrowedDuringSimulation = null;

        m_resReqSatiators = new ResourceSatiator[Operation.ResourceRequirements.Count];
        if (Batch != null)
        {
            // I belive this is for when an activity is being split in place during a simulation.
            for (int rrI = 0; rrI < m_resReqSatiators.Length; ++rrI)
            {
                ResourceSatiator rs = Batch.GetRRSatiator(rrI);
                if (rs != null)
                {
                    m_resReqSatiators[rrI] = new ResourceSatiator(rs.Resource);
                }
            }
        }

        m_resourceReservations = null;

        m_staticComposite = 0;

        InitializeProductionInfoForResources(a_plantManager, a_productRuleManager, a_extensionController);

        InitScheduledStartTicksBeforeSimulate();

        m_simData = new ActSimData();

        MoveIntersectingReleaseTicks = 0;
    }

    internal void PostSimulationInitialization()
    {
        // [TANK_CODE]: TODO move these to something like simulationInitialization
        ///**************************************************************
        // This was temporarily put here. It should be in something like 
        // Simulation initialization except some timing issues prevent that
        // currently.
        //***************************************************************
        m_moveRelatedConstrainedActivities = null;
        m_moveRelatedConstrainersCount = 0;
    }

    /// <summary>
    /// Simulation only. Initialized during SimulationInitialization to the current scheduled resources.
    /// </summary>
    internal ResourceSatiator[] m_resReqSatiators;

    ///// <summary>
    ///// Only valid during a simulation after the call the SimulationInitialization. Checks m_resReqSatiators for ManualSchedulingOnly resources.
    ///// </summary>
    ///// <returns></returns>
    //internal bool IsAnyEligibleResManualSchedulingOnly
    //{
    //    for (int i = 0; i < m_resReqSatiators.Length; ++i)
    //    {
    //        if (m_resReqSatiators[i] != null)
    //        {
    //            if (m_resReqSatiators[i].ManualSchedulingOnly)
    //            {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

    internal decimal m_staticComposite;

    private long CalcReleaseDate()
    {
        long releaseDate = Operation.ManufacturingOrder.EffectiveReleaseDate;
        return releaseDate;
    }

    private BoolVector32 m_simulationFlags;

    private const int UnscheduleActivityAddedToListIdx = 0;
    private const int WaitForUsageEventIdx = 1;
    private const int WaitForClockAdjustmentReleaseIdx = 2;
    private const int ActivityReleaseEventPostedIdx = 3;
    private const int PreventMoveIntersectionEventIdx = 4;
    private const int WaitForScheduledDateBeforeMoveEventIdx = 5;
    private const int WaitForRightMovingNeighborReleaseEventIdx = 6;
    private const int WaitForPredecessorOperationReleaseEventIdx = 7;
    private const int OpOrPredOpBeingMovedIdx = 8;
    private const int HasLeftConstrainingNeighborIdx = 9;
    private const int SequencedIdx = 10;
    private const int BeingMovedIdx = 11;
    private const int WaitForActivityMovedEventIdx = 12;
    private const int ActivityAddedToUnscheduledActivitiesSetIdx = 13;
    private const int ActivityReleaseExecutedIdx = 14;
    private const int InProcessReleaseEventAddedIdx = 15;
    private const int AddedToDispatcherForRetryIdx = 16;
    private const int StaticCompositeSetIdx = 17;
    private const int AddToSequentialStateListIdx = 18;
    private const int WaitForleftMoveBlockToScheduleIdx = 19;
    private const int c_moveIntoBatch = 20;
    private const int ________________________UNUSED1 = 21;
    private const int AddedToMoveIntersectionSetIdx = 22;
    private const int c_minimumScoreNotMetIdx = 23;

    internal bool UnscheduleActivityAddedToList
    {
        get => m_simulationFlags[UnscheduleActivityAddedToListIdx];
        set => m_simulationFlags[UnscheduleActivityAddedToListIdx] = value;
    }

    // [USAGE_CODE] WaitForUsageEvent: A release flag on the activity.
    /// <summary>
    /// Wait for a usage event to clear this flag.
    /// </summary>
    internal bool WaitForUsageEvent
    {
        get => m_simulationFlags[WaitForUsageEventIdx];
        set => m_simulationFlags[WaitForUsageEventIdx] = value;
    }

    internal bool WaitForClockAdjustmentRelease
    {
        get => m_simulationFlags[WaitForClockAdjustmentReleaseIdx];
        set => m_simulationFlags[WaitForClockAdjustmentReleaseIdx] = value;
    }

    internal bool ActivityReleaseEventPosted
    {
        get => m_simulationFlags[ActivityReleaseEventPostedIdx];
        set => m_simulationFlags[ActivityReleaseEventPostedIdx] = value;
    }

    // [USAGE_CODE] PreventMoveIntersectionEvent: Flag used to delay the release of an activity so it doesn't intersect the move date.
    /// <summary>
    /// A move has placed a constraint on when an activity can be scheduled.
    /// This was added for RR usage start and end. In cases where a RR's start == run it's possible to
    /// perfectly place a block that intersects the move date by back calculating setup of the intersecting block so
    /// run starts exactly where the moved block ends.
    /// </summary>
    internal bool PreventMoveIntersectionEvent
    {
        get => m_simulationFlags[PreventMoveIntersectionEventIdx];
        set => m_simulationFlags[PreventMoveIntersectionEventIdx] = value;
    }

    internal bool ActivityAddedToUnscheduledActivitiesSet
    {
        get => m_simulationFlags[ActivityAddedToUnscheduledActivitiesSetIdx];
        set => m_simulationFlags[ActivityAddedToUnscheduledActivitiesSetIdx] = value;
    }

    internal bool ActivityReleaseExecuted
    {
        get => m_simulationFlags[ActivityReleaseExecutedIdx];
        set => m_simulationFlags[ActivityReleaseExecutedIdx] = value;
    }

    /// <summary>
    /// During a simulation this is set to true if the activity is in-process and
    /// an InProcessReleaseEvent has been added for it (it's been released for scheduling).
    /// </summary>
    internal bool InProcessReleaseEventAdded
    {
        get => m_simulationFlags[InProcessReleaseEventAddedIdx];
        set => m_simulationFlags[InProcessReleaseEventAddedIdx] = value;
    }

    internal bool StaticCompositeSet
    {
        get => m_simulationFlags[StaticCompositeSetIdx];
        set => m_simulationFlags[StaticCompositeSetIdx] = value;
    }

    internal bool MinimumScoreNotMet
    {
        get => m_simulationFlags[c_minimumScoreNotMetIdx];
        set => m_simulationFlags[c_minimumScoreNotMetIdx] = value;
    }

    #region These are the move state variables
    internal bool WaitForScheduledDateBeforeMoveEvent
    {
        get => m_simulationFlags[WaitForScheduledDateBeforeMoveEventIdx];
        set => m_simulationFlags[WaitForScheduledDateBeforeMoveEventIdx] = value;
    }

    internal bool WaitForRightMovingNeighborReleaseEvent
    {
        get => m_simulationFlags[WaitForRightMovingNeighborReleaseEventIdx];
        set => m_simulationFlags[WaitForRightMovingNeighborReleaseEventIdx] = value;
    }

    internal bool WaitForPredecessorOperationReleaseEvent
    {
        get => m_simulationFlags[WaitForPredecessorOperationReleaseEventIdx];
        set => m_simulationFlags[WaitForPredecessorOperationReleaseEventIdx] = value;
    }

    /// <summary>
    /// Non-serialized simulation state variable.
    /// Whether one of this activity's predecessors is being moved or a split of this activity's operation is being moved.
    /// </summary>
    internal bool OpOrPredOpBeingMoved
    {
        get => m_simulationFlags[OpOrPredOpBeingMovedIdx];
        set => m_simulationFlags[OpOrPredOpBeingMovedIdx] = value;
    }

    /// <summary>
    /// Non-serialized simulation state variable.
    /// Whether this activity had a left neighbor before the move.
    /// </summary>
    internal bool HasLeftConstrainingNeighbor
    {
        get => m_simulationFlags[HasLeftConstrainingNeighborIdx];
        set => m_simulationFlags[HasLeftConstrainingNeighborIdx] = value;
    }

    internal bool Sequenced
    {
        get => m_simulationFlags[SequencedIdx];
        set => m_simulationFlags[SequencedIdx] = value;
    }

    /// <summary>
    /// Set to true if the activity is being moved; this is the activity that was dragged and dropped.
    /// </summary>
    internal bool BeingMoved
    {
        get => m_simulationFlags[BeingMovedIdx];
        set => m_simulationFlags[BeingMovedIdx] = value;
    }

    /// <summary>
    /// Simulation state variable. This is used to indicate whether this activity it
    /// waiting for the occurance of ActivityMovedEvent. This flag must be set when
    /// an activity it moved. It is set to false whent he activities ActivityMoveEvent
    /// has occurred. It serves as another release on the activity.
    /// </summary>
    internal bool WaitForActivityMovedEvent
    {
        get => m_simulationFlags[WaitForActivityMovedEventIdx];
        set => m_simulationFlags[WaitForActivityMovedEventIdx] = value;
    }

    /// <summary>
    /// During unscheduling, this is used to flag whether the activity should be added to the list
    /// of sequential activities. This occurs as part of the simulation setup.
    /// </summary>
    internal bool AddToSequentialStateList
    {
        get => m_simulationFlags[AddToSequentialStateListIdx];
        set => m_simulationFlags[AddToSequentialStateListIdx] = value;
    }

    /// <summary>
    /// Don't allow a block being moved to be scheduled until another block being moved has been scheduled.
    /// This is set to true if:
    /// If a move is being performed and more than 1 block is being moved,
    /// If another move block is to be scheduled before this block.
    /// </summary>
    internal bool WaitForLeftMoveBlockToSchedule
    {
        get => m_simulationFlags[WaitForleftMoveBlockToScheduleIdx];
        set => m_simulationFlags[WaitForleftMoveBlockToScheduleIdx] = value;
    }

    internal bool MoveIntoBatch
    {
        get => m_simulationFlags[c_moveIntoBatch];
        set => m_simulationFlags[c_moveIntoBatch] = value;
    }

    /// <summary>
    /// [USAGE_CODE]: Whether the activity has been added to the set of activities that intersect the move date.
    /// </summary>
    internal bool AddedToMoveIntersectionSet
    {
        get => m_simulationFlags[AddedToMoveIntersectionSetIdx];
        set => m_simulationFlags[AddedToMoveIntersectionSetIdx] = value;
    }

    //private readonly HashSet<BaseId> m_flaggedDispatchers = new ();
    /// <summary>
    /// [USAGE_CODE]: Whether the activity has been added to the set of activities that intersect the move date.
    /// </summary>
    //internal HashSet<BaseId> RemovedFromDispatcherPendingRetry => m_flaggedDispatchers;
    #endregion

    public override string ToString()
    {
        string str = string.Format("{0};  Activity: ExternalId '{1}';",
            Operation,
            ExternalId);
        #if DEBUG
        str = str + string.Format("  Id={0};", Id);
        #endif
        return str;
    }

    #region Unscheduling
    internal void Unschedule(bool a_clearLocks, bool a_removeFromBatch)
    {
        Scheduled = false;

        ScheduledOnlyForPostProcessingTime = false;
        ScheduledStartTimePostProcessingNoResources = 0;
        ScheduledStartTimeCleanNoResources = 0;
        ScheduledFinishTimePostProcessingNoResources = 0;
        ScheduledFinishTimeCleanNoResources = 0;

        ResourceBlock[] resBlocks;
        // [BATCH_CODE]
        if ((resBlocks = Batch_Unschedule(a_removeFromBatch)) != null)
        {
            // If unscheduling the activity from the batch resulted in the batch having no activities associated with it,
            // unschedule all the resource blocks.
            for (int i = 0; i < resBlocks.Length; ++i)
            {
                ResourceBlock rb = resBlocks[i];
                if (rb != null)
                {
                    rb.Unschedule();
                }
            }
        }

        if (a_clearLocks)
        {
            ClearLocks();
        }
    }
    #endregion

    /// <summary>
    /// This can be used to take NbrOfPeople into consideration by adjusting the AttentionPercent.
    /// </summary>
    /// <param name="rrm">The ResourceRequirementManager of the activity being scheduled.</param>
    /// <param name="primaryRR">The resource requirement being scheduled.</param>
    /// <param name="rci">The interval the activity is being scheduled on.</param>
    /// <returns></returns>
    internal decimal GetAdjustedAttentionPercent(ResourceRequirement a_rr, ResourceCapacityInterval a_rci)
    {
        ResourceRequirementManager rrm = Operation.ResourceRequirements;
        decimal adjustedAttentionPercent = 1m;

        if (a_rci.IsPastPlanningHorizon)
        {
            //No attention percent required past planning horizon. Otherwise the op will never schedule.
            return 0;
        }

        if (PeopleUsage == InternalActivityDefs.peopleUsages.UseSpecifiedNbr)
        {
            decimal adjustedNbrOfPeople = NbrOfPeople > a_rci.NbrOfPeople ? a_rci.NbrOfPeople : NbrOfPeople;
            adjustedAttentionPercent = CalculateAdjustedAttentionPercentBasedOnNbrOfPeople(a_rci, adjustedNbrOfPeople);
        }
        else if (PeopleUsage == InternalActivityDefs.peopleUsages.UseMultipleOfSpecifiedNbr)
        {
            decimal nbrOfPeopleMultiple = a_rci.CalculateCapacityMultiple(NbrOfPeople);
            adjustedAttentionPercent = CalculateAdjustedAttentionPercentBasedOnNbrOfPeople(a_rci, nbrOfPeopleMultiple);
        }

        adjustedAttentionPercent *= a_rr.AttentionPercent;

        return adjustedAttentionPercent;
    }

    /// <summary>
    /// </summary>
    /// <param name="a_rci"></param>
    /// <param name="a_nbrOfPeople"></param>
    /// <returns></returns>
    private decimal CalculateAdjustedAttentionPercentBasedOnNbrOfPeople(ResourceCapacityInterval a_rci, decimal a_nbrOfPeople)
    {
        // Round down to 14 decimal places of precision. Since everything is in whole percents, it should take a giant number of errors
        // to add up to the point where too much has been rounded off and extra percents allow too many activities to schedule on the resource.
        //
        // These 2 commented lines show how this value was obtained.
        // const int c_decimalPrecision=14;
        // decimal c_precisionMultiplier = Math.Pow(10, c_decimalPrecision);
        decimal c_precisionFactor = 100000000000000;

        // Remove the 15th decimal point of precision. The minus 1 decreases the value at the 14 decimal place of precision.
        decimal pctFraction = a_nbrOfPeople / a_rci.NbrOfPeople;

        if (MathStatics.Fraction(pctFraction))
        {
            pctFraction = (Math.Floor(pctFraction * c_precisionFactor) - 1) / c_precisionFactor;
        }

        return pctFraction;
    }

    /// <summary>
    /// returns Activity.RequiredStartQty/Operation.RequiredStartQty
    /// </summary>
    internal decimal FractionOfStartQty => RequiredStartQty / Operation.RequiredStartQty;

    /// <summary>
    /// Among all the blocks that are scheduled determine whether the activity is scheduled in a cell. If 1 block's resource is in a cell then that cell is returned.
    /// If another block is scheduled in a different cell then null is returned since this is just an error on the part of the user in defining their cells.
    /// </summary>
    internal Cell Cell
    {
        get
        {
            Cell cell = null;
            if (Batch != null)
            {
                for (int reqI = 0; reqI < Batch.BlockCount; ++reqI)
                {
                    ResourceBlock block = Batch.BlockAtIndex(reqI);
                    if (block != null)
                    {
                        if (block.ScheduledResource.Cell != null)
                        {
                            if (cell != null && block.ScheduledResource.Cell != cell)
                            {
                                return null;
                            }

                            cell = block.ScheduledResource.Cell;
                        }
                    }
                }
            }

            return cell;
        }
    }
    #endregion

    private long m_intersectingReleaseTicks;

    /// <summary>
    /// Used to store the best release time if this activity is found to intersect the release date
    /// and UsageStart==Run. This date is used to re-apply the move and best squeeze it in around the moved blocks.
    /// </summary>
    internal long MoveIntersectingReleaseTicks
    {
        get => m_intersectingReleaseTicks;
        set => m_intersectingReleaseTicks = value;
    }

    #region Debug
    [Browsable(false)]
    public string dn
    {
        get
        {
            string s = string.Format("Job={0}; MO={1}; Op={2}", Operation.ManufacturingOrder.Job.Name, Operation.ManufacturingOrder.Name, Operation.Name);
            return s;
        }
    }

    [Browsable(false)]
    public string di
    {
        get
        {
            string s = string.Format("Job={0}; MO={1}; Op={2}; Activity={3}", Operation.ManufacturingOrder.Job.Id, Operation.ManufacturingOrder.Id, Operation.Id, Id);
            return s;
        }
    }

    /// <summary>
    /// The Scheduled End Date minus Scheduled Start Date.
    /// </summary>
    public TimeSpan CalendarDuration => ScheduledEndDate.Subtract(ScheduledStartDate);
    #endregion

    /// <summary>
    /// Returns the scheduled start date of the primary resource requirement or when post processing starts in the case
    /// where the activities state is PostProcessing and the primary resource requirement doesn't require a resource
    /// during post processing.
    /// This assumes the requirement has been scheduled. Otherwise an error will occur.
    /// </summary>
    internal override long GetScheduledStartTicks()
    {
        if (PostProcessingStateWithNoResourceUsage)
        {
            return ScheduledStartTimePostProcessingNoResources;
        }

        return PrimaryResourceRequirementBlock.StartTicks;
    }

    internal long GetScheduledStartOfProcessingTicks()
    {
        if (PostProcessingStateWithNoResourceUsage)
        {
            return ScheduledStartTimePostProcessingNoResources;
        }

        return GetScheduledEndOfSetupTicks();
    }

    /// <summary>
    /// ex. If the ratio is 3/2 and the quantity is 10. This function will change the required finish quantity to (3/2)*10=15.
    /// </summary>
    /// <param name="ratio"></param>
    internal void AdjustRequiredQty(decimal a_ratio, decimal a_newRequiredMOQty)
    {
        RequiredFinishQty = ScenarioDetail.ScenarioOptions.RoundQtyWithZeroCheck(a_ratio * RequiredFinishQty, a_newRequiredMOQty);
    }

    internal void AdjustRequiredQty(decimal a_requiredQty)
    {
        RequiredFinishQty = ScenarioDetail.ScenarioOptions.RoundQty(a_requiredQty);
    }

    internal void AbsorbReportedValues(InternalActivity a_absorb)
    {
        ReportedGoodQty += a_absorb.ReportedGoodQty;
        ReportedPostProcessing += a_absorb.ReportedPostProcessing;
        ReportedRun += a_absorb.ReportedRun;
        ReportedScrapQty += a_absorb.ReportedScrapQty;
        ReportedSetup += a_absorb.ReportedSetup;
    }

    internal decimal CalculateReqFinishQty()
    {
        RequiredSpan procSpan;
        long reqNbrCycles;
        decimal finishQty;
        // If this function ends up being used a lot, it might be possible to improve its
        // performance by having it calculate the quantity directly and have 
        // CalculateProcessingTimeSpan() call this function to get the quantity.
        //
        // Or you might precalculate these values before the simulation and cache them here.
        CalculateProcessingTimeSpan(null, out procSpan, out reqNbrCycles, out finishQty);
        return finishQty;
    }

    internal void CalculateProcessingTimeSpanOnResource(Resource a_res, out RequiredSpan o_actProcessingSpan, out long o_actRequiredNumberOfCycles, out decimal o_actFinishQty)
    {
        CalculateProcessingTimeSpan(a_res, out o_actProcessingSpan, out o_actRequiredNumberOfCycles, out o_actFinishQty);
    }

    /// <summary>
    /// Find the processing time, number of cycles, and finish quantity of an activity.
    /// Doesn't require any sim state variables.
    /// </summary>
    /// <param name="aActivity"></param>
    /// <param name="aDeductReported">If set to true reported times and quantites are taken into consideration.</param>
    /// <param name="aActProcessingTimeSpan"></param>
    /// <param name="aActRequiredNumberOfCycles"></param>
    /// <param name="aActFinishQty"></param>
    internal void CalculateProcessingTimeSpan(BaseResource a_res, out RequiredSpan o_actProcessingSpan, out long o_actRequiredNumberOfCycles, out decimal o_actFinishQty)
    {
        InternalActivityDefs.productionStatuses currentProductionStatus = ProductionStatus;
        if (currentProductionStatus >= InternalActivityDefs.productionStatuses.PostProcessing)
        {
            o_actRequiredNumberOfCycles = 0;
            o_actProcessingSpan = new RequiredSpan(0, false);
            o_actFinishQty = 0;
        }
        else
        {
            ResourceOperation operation = (ResourceOperation)Operation;

            o_actFinishQty = FinishQtyGet();
            long actProcessingTimeSpan;

            ProductionInfo resourceProductionInfo = GetResourceProductionInfo(a_res);

            if (operation.TimeBasedReporting)
            {
                o_actFinishQty = InternalOperation.PlanningScrapPercentAdjustedQty(resourceProductionInfo.PlanningScrapPercent, o_actFinishQty);
                o_actFinishQty = Math.Max(0, o_actFinishQty);
                actProcessingTimeSpan = CalcProcessingSpanToCompleteQty(a_res, o_actFinishQty, out o_actRequiredNumberOfCycles);
            }
            else
            {
                decimal qtyToBeManufactured = o_actFinishQty;

                qtyToBeManufactured -= ReportedGoodQty;

                if (operation.DeductScrapFromRequired)
                {
                    qtyToBeManufactured -= ReportedScrapQty;
                    qtyToBeManufactured = Math.Max(0, qtyToBeManufactured);
                    o_actFinishQty = qtyToBeManufactured;
                }
                else
                {
                    qtyToBeManufactured = Math.Max(0, qtyToBeManufactured);
                    o_actFinishQty = qtyToBeManufactured;
                    qtyToBeManufactured = InternalOperation.PlanningScrapPercentAdjustedQty(resourceProductionInfo.PlanningScrapPercent, qtyToBeManufactured);
                }

                o_actRequiredNumberOfCycles = (long)Math.Ceiling(qtyToBeManufactured / resourceProductionInfo.QtyPerCycle);
                actProcessingTimeSpan = o_actRequiredNumberOfCycles * resourceProductionInfo.CycleSpanTicks;
            }

            // Zero length handling for processing span. 
            InternalActivityDefs.productionStatuses status = ProductionStatus;
            bool overrun = actProcessingTimeSpan <= 0 && (status == InternalActivityDefs.productionStatuses.Running || status == InternalActivityDefs.productionStatuses.Started);
            if (overrun)
            {
                o_actRequiredNumberOfCycles = 1;
            }

            actProcessingTimeSpan = Math.Max(actProcessingTimeSpan, 0);
            o_actProcessingSpan = new RequiredSpan(actProcessingTimeSpan, overrun);
        }
    }

    internal long CalcProcessingSpanOfRequiredFinishQty(BaseResource a_res)
    {
        long tmp;
        decimal finishQty = FinishQtyGet();
        return CalcProcessingSpanToCompleteQty(a_res, finishQty, out tmp);
    }

    private long CalcProcessingSpanToCompleteQty(BaseResource a_res, decimal a_qty, out long o_nbrOfCyclesToRun)
    {
        ProductionInfo resourceProductionInfo = GetResourceProductionInfo(a_res);
        decimal qtyPerCycle = resourceProductionInfo.QtyPerCycle;

        if (qtyPerCycle == 0m)
        {
            throw new PTHandleableException("2114");
        }

        o_nbrOfCyclesToRun = (long)Math.Ceiling(a_qty / qtyPerCycle);
        long processingTicks;

        long completedCycles = ReportedRun / resourceProductionInfo.CycleSpanTicks;
        o_nbrOfCyclesToRun -= completedCycles;

        if (ReportedRun > 0 && completedCycles == 0)
        {
            processingTicks = (Math.Max(0, o_nbrOfCyclesToRun) * resourceProductionInfo.CycleSpanTicks) - ReportedRun;
        }
        else
        {
            processingTicks = Math.Max(0, o_nbrOfCyclesToRun) * resourceProductionInfo.CycleSpanTicks;
        }

        return processingTicks;
    }

    internal RequiredSpan CalculatePostProcessingSpan(BaseResource a_resource)
    {
        ProductionInfo resourceProductionInfo = GetResourceProductionInfo(a_resource);
        RequiredSpan requiredSpan = new ();
        if (ProductionStatus > InternalActivityDefs.productionStatuses.PostProcessing)
        {
            return requiredSpan;
        }

        long remainingPostProcessing = resourceProductionInfo.PostProcessingSpanTicks - m_reportedPostProcessing;

        bool overrun = false;
        if (ProductionStatus == InternalActivityDefs.productionStatuses.PostProcessing)
        {
            overrun = remainingPostProcessing <= 0;
        }

        remainingPostProcessing = Math.Max(0, remainingPostProcessing);
        requiredSpan = new RequiredSpan(remainingPostProcessing, overrun);

        return requiredSpan;
    }

    internal RequiredSpan CalculateStorageSpan(BaseResource a_resource)
    {
        ProductionInfo resourceProductionInfo = GetResourceProductionInfo(a_resource);
        RequiredSpan requiredSpan = new ();
        if (ProductionStatus > InternalActivityDefs.productionStatuses.Storing)
        {
            return requiredSpan;
        }

        long remainingStorage = resourceProductionInfo.StorageSpanTicks - m_reportedStorage;

        bool overrun = false;
        if (ProductionStatus == InternalActivityDefs.productionStatuses.Storing)
        {
            overrun = remainingStorage <= 0;
        }

        remainingStorage = Math.Max(0, remainingStorage);
        requiredSpan = new RequiredSpan(remainingStorage, overrun);

        return requiredSpan;
    }

    internal RequiredSpanPlusClean CalculateCleanSpan(BaseResource a_resource)
    {
        if (ProductionStatus == InternalActivityDefs.productionStatuses.Finished)
        {
            return RequiredSpanPlusClean.s_notInit;
        }

        ProductionInfo resourceProductionInfo = GetResourceProductionInfo(a_resource);
        long remainingCleanTime = resourceProductionInfo.CleanSpanTicks - m_reportedClean;

        bool overrun = false;
        if (ProductionStatus == InternalActivityDefs.productionStatuses.Cleaning)
        {
            overrun = remainingCleanTime <= 0;
        }

        remainingCleanTime = Math.Max(0, remainingCleanTime);
        decimal productionCleanoutCost = 0;
        decimal resourceCleanoutCost = 0;
        if (remainingCleanTime > 0)
        {
            productionCleanoutCost = resourceProductionInfo.CleanoutCost;
            if (a_resource is InternalResource res)
            {
                resourceCleanoutCost = res.ResourceCleanoutCost;
            }
        }

        RequiredSpanPlusClean requiredSpanPlusClean = new RequiredSpanPlusClean(remainingCleanTime, overrun, resourceProductionInfo.CleanoutGrade);
        requiredSpanPlusClean.SetStaticCosts(0, productionCleanoutCost, resourceCleanoutCost);
        return requiredSpanPlusClean;
    }

    #region JIT
    private readonly ActivityBufferInfo m_bufferInfo = new ();
    internal ActivityBufferInfo BufferInfo => m_bufferInfo;

    /// <summary>
    /// This represents the earliest JIT start out of all the Resources capable of scheduling this Activity, Get JIT start from the Resource Buffer Info if value is needed for
    /// scheduling.
    /// </summary>
    internal long DbrJitStartTicks => m_bufferInfo.EarliestJitBufferInfo.DbrJitStartDate;
    internal long JitStartTicks => m_bufferInfo.EarliestJitBufferInfo.JitStartDate;

    /// <summary>
    /// This represents the earliest JIT start out of all the Resources capable of scheduling this Activity, Get JIT start from the Resource Buffer Info if value is needed for
    /// scheduling.
    /// </summary>
    internal long NeedTicks => m_bufferInfo.EarliestJitBufferInfo.BufferEndDate;

    public ActivityResourceBufferInfo GetBufferResourceInfo(BaseId a_resId)
    {
        return m_bufferInfo.GetResourceInfo(a_resId);
    }

    public DateTime NeedDate => new (NeedTicks);

    internal bool JITStartDateNotSet => !m_bufferInfo.AnyBufferSet();

    /// <summary>
    /// Returns either the expected finish quantity or the required finish quantity.
    /// </summary>
    /// <returns></returns>
    private decimal FinishQtyGet()
    {
        if (Operation.UseExpectedFinishQty)
        {
            return ExpectedFinishQty;
        }

        return RequiredFinishQty;
    }

    private RequiredSpan RemainingProcessingSpan(BaseResource a_res)
    {
        CalculateProcessingTimeSpan(a_res, out RequiredSpan remainingActProcSpan, out long requiredNumberOfCycles, out decimal finishQty);
        return remainingActProcSpan;
    }

    /// <summary>
    /// For use with JIT. Sequencing isn't taken into consideration if the activity hasn't already been scheduled.
    /// </summary>
    internal RequiredCapacity ScheduledOrRemainingRequiredCapacity(long a_simClock, Resource a_res, ExtensionController a_timeCustomizer)
    {
        RequiredCapacity rc;


        if (Scheduled)
        {
            // This can happen if the primary resource requirement's usedDuring is less than the helper resource's used during.
            rc = new RequiredCapacity(
                RequiredSpanPlusClean.s_notInit,
                new RequiredSpanPlusSetup(Batch.SetupCapacitySpan),
                new RequiredSpan(Batch.ProcessingCapacitySpan),
                new RequiredSpan(Batch.PostProcessingSpan),
                new RequiredSpan(Batch.StorageSpan),
                new RequiredSpanPlusClean(Batch.CleanSpan));
        }
        else if (a_res != null)
        {
            rc = a_res.CalculateTotalRequiredCapacity(a_simClock, this, LeftNeighborSequenceValues.NullValues, true, -1, a_timeCustomizer);
        }
        else
        {
            rc = new RequiredCapacity(
                RequiredSpanPlusClean.s_notInit,
                RequiredSpanPlusSetup.s_overrun,
                RemainingProcessingSpan(a_res),
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpanPlusClean.s_overrun);
        }

        return rc;
    }
    #endregion

    internal long GetLongestEligibleBufferTime()
    {
        Resource primaryResourceRequirementLock = PrimaryResourceRequirementLock();

        long resBuffer = 0;
        if (primaryResourceRequirementLock != null)
        {
            resBuffer = primaryResourceRequirementLock.BufferSpanTicks;
        }

        if (PrimaryDefaultResource is InternalResource defaultResource)
        {
            resBuffer = defaultResource.BufferSpanTicks;
        }

        foreach (Resource eligibleResource in Operation.ResourceRequirements.PrimaryResourceRequirement.GetEligibleResources())
        {
            resBuffer = Math.Max(resBuffer, eligibleResource.BufferSpanTicks);
        }

        return resBuffer + Operation.StandardOperationBufferTicks;
    }

    internal long m_similarityValue;

    internal override int SimilarityComparison(BaseActivity a_ba)
    {
        int v;
        InternalActivity ia = (InternalActivity)a_ba;

        if (!ReferenceEquals(m_operation, ia.Operation))
        {
            if ((v = m_operation.SimilarityComparison(ia.Operation)) != 0)
            {
                return v;
            }
        }

        if ((v = base.SimilarityComparison(a_ba)) != 0)
        {
            return v;
        }

        if ((v = ActivityProductionInfo.SimilarityComparison(ia.ActivityProductionInfo)) != 0)
        {
            return v;
        }

        if ((v = m_productionStatus.CompareTo(ia.m_productionStatus)) != 0)
        {
            return v;
        }

        if ((v = m_reportedGoodQty.CompareTo(ia.m_reportedGoodQty)) != 0)
        {
            return v;
        }

        if ((v = m_reportedScrapQty.CompareTo(ia.m_reportedScrapQty)) != 0)
        {
            return v;
        }

        if ((v = m_peopleUsage.CompareTo(ia.m_peopleUsage)) != 0)
        {
            return v;
        }

        if ((v = m_nbrOfPeople.CompareTo(ia.m_nbrOfPeople)) != 0)
        {
            return v;
        }

        if ((v = m_reportedRun.CompareTo(ia.m_reportedRun)) != 0)
        {
            return v;
        }

        if ((v = m_reportedSetup.CompareTo(ia.m_reportedSetup)) != 0)
        {
            return v;
        }

        if ((v = m_reportedPostProcessing.CompareTo(ia.m_reportedPostProcessing)) != 0)
        {
            return v;
        }

        if ((v = m_persistentFlags.CompareTo(ia.m_persistentFlags)) != 0)
        {
            return v;
        }

        return 0;
    }

    internal void SimScheduledNotification()
    {
        SimScheduled = true;
        Operation.ActivityScheduled(this);
    }

    internal Resource m_moBatchLockedResource;

    /// <summary>
    /// This is the operation's PrimaryResourceRequirement's Default Resource.
    /// </summary>
    internal BaseResource PrimaryDefaultResource => m_operation.ResourceRequirements.PrimaryResourceRequirement.DefaultResource;

    /// <summary>
    /// The resource that satisfies this requirement is used to determine the length of the activity. It's UsedDuring
    /// is presumed to be at the maximum among all resource requirements. If the activity has any blocks scheduled for it,
    /// it's presumed on the blocks corresponds to the primary resource requirement.
    /// </summary>
    internal ResourceRequirement PrimaryResourceRequirement => m_operation.ResourceRequirements.PrimaryResourceRequirement;

    internal void Schedule(ResourceBlock a_block)
    {
        Scheduled = true;
        ReleaseMoveRelatedConstrainedActivities();
    }

    /// <summary>
    /// Return the number of ticks required to make a quantity of part.
    /// </summary>
    internal long CalcSpanToMakeQty(BaseResource a_res, decimal a_qtyToMake)
    {
        ProductionInfo resourceProductionInfo = GetResourceProductionInfo(a_res);
        long cycles = (long)Math.Ceiling(a_qtyToMake / resourceProductionInfo.QtyPerCycle);
        long ticks = cycles * resourceProductionInfo.CycleSpanTicks;
        return ticks;
    }

    /// <summary>
    /// Resource Reservations are made for operations where there are shelf life constraints or continuous operation constraints like conveyors involved.
    /// If this flag has been set it means the activity has already been assigned to a resource.
    /// </summary>
    internal bool ResourceReservationMade => m_resourceReservations != null;

    /// <summary>
    /// Return the reservation for the specified resource requirement
    /// Can be null if there is nothing reserved. Check ResourceReservationMade first.
    /// </summary>
    /// <param name="a_rrIdx"></param>
    /// <returns></returns>
    internal ResourceReservation GetReservationForRR(int a_rrIdx)
    {
        return m_resourceReservations?[a_rrIdx];
    }

    /// <summary>
    /// Apply the reservation
    /// </summary>
    /// <param name="a_reservation"></param>
    internal void SetResourceReservationForRR(ResourceReservation a_reservation)
    {
        m_resourceReservations ??= new ResourceReservation[Operation.ResourceRequirements.Count];

        m_resourceReservations[a_reservation.m_rrIndex] = a_reservation;
    }

    /// <summary>
    /// A resource reservation array where the index represents the resource requirement index
    /// </summary>
    private ResourceReservation[] m_resourceReservations;

    /// <summary>
    /// Cache all resource specific production infos.
    /// </summary>
    /// <param name="a_resList">Eligible resources that a ProductionInfo should be calculated for</param>
    internal void InitializeProductionInfoForResources(PlantManager a_plantManager, ProductRuleManager a_prs, ExtensionController a_timeCustomizer)
    {
        IEnumerable<Resource> activeResources = a_plantManager.GetResourceList().Where(r => r.Active);

        m_resourceProductionInfos.Clear();
        if (Operation.Job.Template)
        {
            return;
        }

        foreach (Resource resource in activeResources)
        {
            ProductionInfo resProductionInfo = Operation.ProductionInfo.Clone();

            if (a_timeCustomizer != null)
            {
                try
                {
                    ProductionInfo overrideProductionInfo = a_timeCustomizer.OverrideProductionInfo(this, resource);
                    if (overrideProductionInfo != null)
                    {
                        m_resourceProductionInfos.Add(resource.Id, overrideProductionInfo);
                        continue;
                    }
                }
                catch (Exception e)
                {
                    CustomizationExceptionHelpers.THROW_RequiredCapacityCutomization_EXCEPTION(e, Operation);
                }
            }

            //Account for multipliers, these will be overwritten if used in a product rule
            resProductionInfo.CycleSpanTicks = Convert.ToInt64(resProductionInfo.CycleSpanTicks / resource.CycleEfficiencyMultiplier);
            resProductionInfo.SetupSpanTicks = Convert.ToInt64(resProductionInfo.SetupSpanTicks / resource.ActivitySetupEfficiencyMultiplier);
            
            Product primaryProduct = Operation.Products.PrimaryProduct;
            Item productItem;

            //If the current operation does not produce an item, try to look up an item produced by an operation on the same MO
            if (primaryProduct == null)
            {
                productItem = Operation.ManufacturingOrder.Product;
            }
            else
            {
                productItem = primaryProduct.Item;
            }

            if (productItem != null && a_prs.GetProductRule(resource.Id, productItem.Id, Operation.ProductCode) is ProductRule pr)
            {

                if (pr.UsePlanningScrapPercent)
                {
                    resProductionInfo.PlanningScrapPercent = pr.PlanningScrapPercent;
                }

                if (pr.UseSetupSpan)
                {
                    resProductionInfo.SetupSpanTicks = pr.m_SetupSpanTicks;
                }

                if (pr.UseProductionSetupCost)
                {
                    resProductionInfo.ProductionSetupCost = pr.ProductionSetupCost;
                }

                if (pr.UseCycleSpan)
                {
                     resProductionInfo.CycleSpanTicks = pr.CycleSpanTicks;
                }

                if (pr.UsePostProcessingSpan)
                {
                     resProductionInfo.PostProcessingSpanTicks = pr.PostProcessingSpanTicks;
                }

                if (pr.UseCleanSpan)
                {
                     resProductionInfo.CleanSpanTicks = pr.CleanSpanTicks;
                }

                if (pr.UseCleanoutCost)
                {
                    resProductionInfo.CleanoutCost = pr.CleanoutCost;
                }

                if (pr.UseQtyPerCycle)
                {
                     resProductionInfo.QtyPerCycle = pr.QtyPerCycle;
                }

                if (pr.UseMaterialPostProcessingSpan)
                {
                     resProductionInfo.MaterialPostProcessingSpanTicks = pr.MaterialPostProcessingSpanTicks;
                }
                else
                {
                    //If PR doesn't define material post processing, use the primary products value
                    resProductionInfo.MaterialPostProcessingSpanTicks = Operation.Products.PrimaryProduct?.MaterialPostProcessingTicks ?? 0;
                }
                
                if (pr.UseStorageSpan)
                {
                    resProductionInfo.StorageSpanTicks = pr.StorageSpanTicks;
                }
               
                if (pr.UseTransferQty)
                {
                    resProductionInfo.TransferQty = pr.TransferQty;
                }
                else
                {
                    resProductionInfo.TransferQty = Operation.Products.PrimaryProduct?.Item.TransferQty ?? 0;
                }

                //TODO: implement on production info?
                //if (pr.UseMinVolume)
                //{
                //    if (!resProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle)
                //    {
                //        resProductionInfo.MinVolume = pr.MinVolume;
                //    }
                //}

                //if (pr.UseMaxVolume)
                //{
                //    if (!resProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle)
                //    {
                //        resProductionInfo.MaxVolume = pr.MaxVolume;
                //    }
                //}
            }
            else
            {
                //No PR. We still need to define material post processing, use the primary products value
                resProductionInfo.MaterialPostProcessingSpanTicks = Operation.Products.PrimaryProduct?.MaterialPostProcessingTicks ?? 0;
                resProductionInfo.TransferQty = Operation.Products.PrimaryProduct?.Item.TransferQty ?? 0;
            }

            // [BATCH_CODE]
            if (resource.BatchType == MainResourceDefs.batchType.Volume)
            {
                // The quantity per cycle is determined by the resources. ProductRules and Operation quantity per cycles don't matter.
                resProductionInfo.QtyPerCycle = resource.BatchVolume;

                // Activities scheduled on batch resources don't support manual overrides.
                resProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle = false;
            }

            m_resourceProductionInfos.Add(resource.Id, resProductionInfo.OverrideProductionInfo(m_productionInfoOverride));
        }
    }

    /// <summary>
    /// Calculates the Critical Ratio for Activity given the clock. The number required processing spans between the need date and the SimClock.
    /// Negative values are returned if the SimClock is after the date.
    /// Positive values are returned if the SimClock is before the date.
    /// or 0 if the activity doesn't require any time.
    /// </summary>
    /// <param name="a_simClock">current clock</param>
    /// <returns>CriticalRatio</returns>
    public decimal GetCriticalRatio(BaseResource a_res, long a_simClock)
    {
        RequiredSpan requiredSpan;
        CalculateProcessingTimeSpan(a_res, out requiredSpan, out long _, out decimal _);

        decimal ret;

        if (requiredSpan.TimeSpanTicks <= 0)
        {
            ret = 0;
        }
        else
        {
            ret = (NeedTicks - a_simClock) / (decimal)requiredSpan.TimeSpanTicks;
        }

        return ret;
    }

    #region Resource locking
    /// <summary>
    /// This function needs to be called when:
    /// the internal activity has been constructed
    /// the routing has been updated.
    /// It creates an array of resources whose size is equal to the number of resource requirements.
    /// If an original activity is passed, the resources array is initialized to that of the orignal one.
    /// </summary>
    /// <param name="a_originalActivity">Optional parameter. Will initialize the </param>
    private void InitializeResourceLocks(InternalActivity a_originalActivity = null)
    {
        if (a_originalActivity != null)
        {
            m_lockedResources = a_originalActivity.m_lockedResources.Clone();
        }
        else
        {
            m_lockedResources = new ();
        }
    }

    /// <summary>
    /// Removes all the resource locks this activity has.
    /// </summary>
    private void ClearLocks()
    {
        m_lockedResources.Clear();
    }

    /// <summary>
    /// An indication of the locked status of this activity. It can be Locked, Unlocked, or, Some blocks locked.
    /// </summary>
    /// <summary>
    /// Locked Blocks cannot be moved to different Resources during Optimizations, Moves, or Expedites.
    /// </summary>
    public lockTypes Locked
    {
        get
        {
            int lockedCount = m_lockedResources.Count;

            if (lockedCount == 0)
            {
                return lockTypes.Unlocked;
            }

            if (lockedCount == m_operation.ResourceRequirements.Count)
            {
                return lockTypes.Locked;
            }

            return lockTypes.SomeBlocksLocked;
        }
    }

    /// <summary>
    /// Determine whether the resource for a specific resource requirement has been locked.
    /// </summary>
    /// <param name="a_resourceRequirementIndex">An index of a resource requirement.</param>
    /// <returns>Whether the resource requirement have been locked.</returns>
    internal bool ResourceRequirementLocked(int a_resourceRequirementIndex)
    {
        return m_lockedResources.ContainsKey(a_resourceRequirementIndex);
    }

    /// <summary>
    /// Lock a specific resource requirement to the resource that it is currently scheduled on.
    /// This function should only be called when the activity has been scheduled otherwise
    /// an exception will result.
    /// </summary>
    /// <param name="resourceRequirementIndex">The index of a resource requirement.</param>
    internal void LockResourceRequirement(int a_resourceRequirementIndex)
    {
        ResourceBlock rb = Batch.BlockAtIndex(a_resourceRequirementIndex);
        if (rb != null)
        {
            m_lockedResources[a_resourceRequirementIndex] = rb.ScheduledResource;
        }
    }

    /// <summary>
    /// Unlock a specific resource requirement.
    /// </summary>
    /// <param name="a_resourceRequirementIndex"></param>
    internal void UnlockResourceRequirement(int a_resourceRequirementIndex)
    {
        m_lockedResources.Remove(a_resourceRequirementIndex);
    }

    /// <summary>
    /// Lock or unlock all the blocks of this activity.
    /// </summary>
    /// <param name="a_lockit">Whether to lock or unlock.</param>
    internal void Lock(bool a_lockit)
    {
        if (a_lockit)
        {
            if (Batch != null)
            {
                for (int i = 0; i < Batch.BlockCount; ++i)
                {
                    ResourceBlock rb = Batch.BlockAtIndex(i);
                    if (rb != null)
                    {
                        m_lockedResources[i] = rb.ScheduledResource;
                    }
                    else
                    {
                        m_lockedResources.Remove(i);
                    }
                }
            }
        }
        else
        {
            ClearLocks();
        }
    }

    private Dictionary<int, Resource> m_tempLockedResources = new();

    /// <summary>
    /// Temporarily locks every block to the resource it's scheduled on.
    /// </summary>
    internal void TempLock()
    {
        m_tempLockedResources = m_lockedResources.Clone();

        Lock(true);
    }

    /// <summary>
    /// Use this function to clear the locks created with TempLock(). You must call this function if the TempLock() function was called against this activity.
    /// </summary>
    internal void TempLockClear()
    {
        m_lockedResources = m_tempLockedResources.Clone();
        m_tempLockedResources.Clear();
    }
    #endregion Resource Locking

    #region ScheduledStartTicksBeforeSimulate
    /// <summary>
    /// In SimulationInitialization this is set to the start of the activity. If it's not scheduled it's set to 0.
    /// </summary>
    internal long ScheduledStartTicksBeforeSimulate { get; private set; }

    /// <summary>
    /// Must be called prior to a simulation, before the activity has been unscheduled.
    /// If the activity is scheduled, it will record its start time. If not it will set the value to 0.
    /// </summary>
    private void InitScheduledStartTicksBeforeSimulate()
    {
        long dt = ScheduledStartTicks();
        if (dt == PTDateTime.MinDateTimeTicks)
        {
            ScheduledStartTicksBeforeSimulate = 0;
        }

        ScheduledStartTicksBeforeSimulate = dt;
    }
    #endregion

    /// <summary>
    /// Get the primary resource this activity must schedule on due to
    /// a primary resource requuirement lock or a default resource
    /// </summary>
    /// <returns></returns>
    internal InternalResource GetMustScheduleResource()
    {
        InternalResource mustSceduleRes = null;
        if ((mustSceduleRes = PrimaryResourceRequirementLock()) != null)
        {
            mustSceduleRes = (InternalResource)PrimaryDefaultResource;
        }
        else if (PrimaryDefaultResource != null)
        {
            mustSceduleRes = (InternalResource)PrimaryDefaultResource;
        }

        return mustSceduleRes;
    }

    /// <summary>
    /// A simulation only variable.
    /// If material overlap exists betwen this activity's operation and the predecessor, the start times of each cycle are temporairily stored here.
    /// </summary>
    internal long[] m_overlapCycleTimes;

    /// <summary>
    /// Dictionary of previously created LotIds. Each LotId is associated with an activity.
    /// To allow different simulations to produce the same BaseId for each Lot source,
    /// this dictionary isn't cleared between simulations. This collection is also serialized
    /// so clients also generate the same ids.
    /// </summary>
    private readonly Dictionary<BaseId, BaseId> m_generatedLotIds = new ();

    /// <summary>
    /// Returns a BaseId to use to create a Lot. If an activity has created a lot during another simulation,
    /// the same Id will be returned, otherwise a new BaseId will be generated.
    /// </summary>
    /// <param name="a_sourceId">The BaseId of the inventory the lot is being created for. A unique Lot Id will be created for each source Id.  </param>
    /// <param name="a_idGen"></param>
    /// <returns></returns>
    public BaseId CreateLotId(BaseId a_sourceId, IIdGenerator a_idGen)
    {
        if (m_generatedLotIds.TryGetValue(a_sourceId, out BaseId lotId))
        {
            return lotId;
        }

        lotId = a_idGen.NextID();
        m_generatedLotIds.Add(a_sourceId, lotId);
        return lotId;
    }

    /// <summary>
    /// If greater than 0, this activity has scheduled to start at the same time as another block on the same resource.
    /// This index will be used to correctly schedule the simultaneous blocks in the correct order to their materials other constraints will be
    /// validated consistently on non-optimize simulations.
    /// This value is serialized and not cleared on initialization. It will only be reset when the activity schedules.
    /// </summary>
    internal int SimultaneousSequenceIdx
    {
        get => m_simultaneousSequenceIdx;
        set => m_simultaneousSequenceIdx = value;
    }

    private int m_simultaneousSequenceIdx;

    /// <summary>
    /// The value of SimultaneousSequenceIdx when the activity was added to the dispatcher
    /// We need this so we can remove it from the dispatcher using the same key values.
    /// </summary>
    internal long OriginalSimultaneousSequenceIdxScore;

    /// <summary>
    /// This value is the original Start Ticks of the Activity when it schedules on a simulation
    /// </summary>
    internal long OriginalScheduledStartTicks;

    /// <summary>
    /// Update this activity's overriden setup span. 
    /// </summary>
    /// <param name="a_newSetup"></param>
    /// <param name="a_autoSplitting">Flag to indicate if this method was called by an auto spilt execution</param>
    /// <param name="a_isUnsplitting">Flag to indicate if this method was called from a logic to unsplit setup</param>
    public void SplitSetup(long a_newSetup, bool a_autoSplitting = false, bool a_isUnsplitting = false)
    {
        //Do not auto spilt if the SetupSpanOverride is set and was not set by the auto split
        if (m_productionInfoOverride.SetupSpanOverride && !Operation.AutoSplitInfo.OperationIsAutoSplit && a_autoSplitting)
        {
            return;
        }

        if (a_newSetup != -1 && m_productionInfoOverride.SetupSpanTicks != a_newSetup)
        {
            m_productionInfoOverride.SetupSpanTicks = a_newSetup;
            m_productionInfoOverride.OnlyAllowManualUpdatesToSetupSpan = true;
        }

        //If un-splitting and Operation is not AutoSplit do not set or unset override
        if (a_isUnsplitting && !Operation.AutoSplitInfo.OperationIsAutoSplit)
        {
            return;
        }

        if (Operation.AutoSplitInfo.OperationIsAutoSplit)
        {
            //if we are un-splitting and the SetupSpanOverride flag was set by AutoSplit
            //unset the flag and un-track activity
            if (a_isUnsplitting && Operation.AutoSplitInfo.IsOverrideSet(Id))
            {
                m_productionInfoOverride.SetupSpanOverride = false;
                Operation.AutoSplitInfo.UnTrackActivity(Id);
            }
            //if we are splitting and activity is not tracked amongst the list of activities with 
            //setupOverride flag set by auto-split, track it and set it
            else if (!a_isUnsplitting && !Operation.AutoSplitInfo.IsOverrideSet(Id))
            {
                Operation.AutoSplitInfo.TrackActivity(Id);
                m_productionInfoOverride.SetupSpanOverride = true;
            }
        }
        else
        {
            //Only track activity if this was called by an auto split logic otherwise just set the SetupOverride flag
            if (a_autoSplitting)
            {
                Operation.AutoSplitInfo.TrackActivity(Id);
            }

            m_productionInfoOverride.SetupSpanOverride = true;
        }
    }

    public bool CanResizeForStorage(ScenarioDetail a_sd, decimal a_resizeQty, Resource a_primaryResource, SchedulableInfo a_si, CycleAdjustmentProfile a_ccp)
    {
        //Only resize once
        if (ManufacturingOrder.Resized)
        {
            return false;
        }

        //Can't resize started Ops
        if (ProductionStatus >= InternalActivityDefs.productionStatuses.Started)
        {
            return false;
        }

        //Can't resize if there are multiple ops with no predecessors
        if (Operation.ManufacturingOrder.CurrentPath.Leaves.Count > 1)
        {
            return false;
        }

        //Only the first op can trigger a resize
        if (Operation.Predecessors.Count > 0)
        {
            return false;
        }

        //The first op can only resize if it's the op producing the Product
        if (Operation.Products.Count == 0)
        {
            return false;
        }

        //Resizing feature works on Optimize, Move, Move and Expedite, Expedite, Compress, JIT Compress.
        if (a_sd.ActiveSimulationType is not ScenarioDetail.SimulationType.Optimize 
            and not ScenarioDetail.SimulationType.Move 
            and not ScenarioDetail.SimulationType.MoveAndExpedite 
            and not ScenarioDetail.SimulationType.Compress
            and not ScenarioDetail.SimulationType.Expedite 
            and not ScenarioDetail.SimulationType.JitCompress)
        {
            return false;
        }

        //Check extension for custom logic for whether the MO should resize or not.
        if (a_sd.ExtensionController.RunAdjustMOExtension)
        {
            if (!a_sd.ExtensionController.AdjustMOForStorage(a_sd.ActiveSimulationType, a_sd.Clock, a_sd.SimClock, a_resizeQty, a_primaryResource, this, a_si, a_ccp, a_sd))
            {
                return false;
            }
        }

        return true;
    }
}