using PT.APSCommon;
using PT.Scheduler.Schedule;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Collections;
using PT.Scheduler.Simulation.Extensions;
using PT.Scheduler.Simulation.Scheduler.AlternatePaths;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// The class from which all Operations derive.
/// </summary>
public abstract partial class BaseOperation 
{
    #region Simulation
    #region Code for tracking scheduling of predecessor operations within this manufacturing order.
    /// <summary>
    /// Keeps track of the number of predecessor operations that are constraints on this operation and must be scheduled before this operation can
    /// be scheduled. This count does not include predecessor operations that have already been finished or are omitted.
    /// </summary>
    private int m_waitingOnPredecessorReadyEventCount;

    /// <summary>
    /// Whether the operation is waiting on any predecessors to be scheduled.
    /// </summary>
    private bool WaitingOnPredecessorReadyEvent => m_waitingOnPredecessorReadyEventCount > 0;

    /// <summary>
    /// Call this function whenever a predecessor release event occurs for one of the operations that is an immediate predecessor of this operation.
    /// </summary>
    private void DecrementWaitOnPredecessorReadyEventCount(long a_readyDateTicks)
    {
        if (IsNotFinishedAndNotOmitted)
        {
            --m_waitingOnPredecessorReadyEventCount;
        }

        if (a_readyDateTicks > m_latestPredecessorReadyDate)
        {
            m_latestPredecessorReadyDate = a_readyDateTicks;
        }
    }

    private long m_latestPredecessorReadyDate;

    internal long LatestPredecessorReadyDate => m_latestPredecessorReadyDate;

    /// <summary>
    /// Call this function before the start of a simulation to initialize tracking the readiness of predecessor operations.
    /// </summary>
    private void InitWaitOnPredecessorReadyEventCount()
    {
        m_waitingOnPredecessorReadyEventCount = 0;

        AlternatePath.AssociationCollection predecessorCollection = AlternatePathNode.Predecessors;

        for (int predecessorI = 0; predecessorI < predecessorCollection.Count; predecessorI++)
        {
            BaseOperation predecessorOperation = predecessorCollection[predecessorI].Predecessor.Operation;
            if (predecessorOperation.IsNotFinishedAndNotOmitted) // Finished operations are handled a different way. Their reported finish date releases the operation.
            {
                ++m_waitingOnPredecessorReadyEventCount;
            }
        }
    }
    #endregion

    #region Simulation State Variables
    private BoolVector32 m_simulationFlags;

    // Used simulation flags.
    private const int WaitForHoldUntilEventIndex = 0;
    private const int PredecessorReadyEventScheduledIndex = 1;
    private const int WaitForFinishedPredecessorMOsAvailableEventIndex = 2;
    private const int WaitForOperationFinishedEventIndex = 3;
    private const int OperationReadyProcessedIdx = 4;
    private const int WaitForLeftBatchNeighborIdx = 5;
    private const int PredBatchOpsScheduledIdx = 6;

    /// <summary>
    /// Whether the operation is currently constrained by the hold date.
    /// </summary>
    internal bool WaitForHoldUntilEvent
    {
        get => m_simulationFlags[WaitForHoldUntilEventIndex];

        set => m_simulationFlags[WaitForHoldUntilEventIndex] = value;
    }

    /// <summary>
    /// Whether the operation is constrained by some finished predecessor MO.
    /// </summary>
    internal bool WaitForFinishedPredecessorMOsAvailableEvent
    {
        get => m_simulationFlags[WaitForFinishedPredecessorMOsAvailableEventIndex];

        set => m_simulationFlags[WaitForFinishedPredecessorMOsAvailableEventIndex] = value;
    }

    /// <summary>
    /// Whether the predecessor release event has been scheduled for this operation.
    /// </summary>
    internal bool PredecessorReadyEventScheduled
    {
        get => m_simulationFlags[PredecessorReadyEventScheduledIndex];

        set => m_simulationFlags[PredecessorReadyEventScheduledIndex] = value;
    }

    /// <summary>
    /// If the operation has been finished then this event will be setup to release it if the operation is scheduled to finish after
    /// the clock time.
    /// </summary>
    internal bool WaitForOperationFinishedEvent
    {
        get => m_simulationFlags[WaitForOperationFinishedEventIndex];

        set => m_simulationFlags[WaitForOperationFinishedEventIndex] = value;
    }

    /// <summary>
    /// Whether the all the operation's constraints have been released
    /// and it has been releaed for scheduling (Ready processed).
    /// </summary>
    internal bool OperationReadyProcessed
    {
        get => m_simulationFlags[OperationReadyProcessedIdx];
        set => m_simulationFlags[OperationReadyProcessedIdx] = value;
    }

    /// <summary>
    /// If the activity is a member of a batch, and within the batch is sequenced after another MO, then wait for the activitiy of left batch to be scheduled.
    /// </summary>
    internal bool WaitForLeftBatchNeighborReleaseEvent
    {
        get => m_simulationFlags[WaitForLeftBatchNeighborIdx];
        set => m_simulationFlags[WaitForLeftBatchNeighborIdx] = value;
    }

    internal bool PredBatchOpsScheduled
    {
        get => m_simulationFlags[PredBatchOpsScheduledIdx];
        set => m_simulationFlags[PredBatchOpsScheduledIdx] = value;
    }
    #endregion

    internal void PredecessorOperationReady(long a_readyDateTicks, AlternatePath.Association a_predecessorAssociation)
    {
        //If the predecessor was finished, it won't have incremented the predecessor counter
        if (!a_predecessorAssociation.Predecessor.Operation.IsFinishedOrOmitted)
        {
            DecrementWaitOnPredecessorReadyEventCount(a_readyDateTicks);
            //If this operation has a resource connector, update the release time
            if (m_connectorFromPredecessorsConstraints.TryGetValue(a_predecessorAssociation.Predecessor.Operation.Id, out ResourceConnectorConstraintInfo constraintInfo))
            {
                //Subtract transfer span because this will occur concurrently with the resource transfer
                constraintInfo.PredecessorReleaseTicks = a_readyDateTicks - a_predecessorAssociation.TransferSpanTicks;
            }
        }
    }

    /// <summary>
    /// Performed before any SimulationInitializations on all objects within the system.
    /// </summary>
    internal virtual void ResetSimulationStateVariables(ScenarioDetail a_sd)
    {
        InitWaitOnPredecessorReadyEventCount();
        m_materialRequirementsCollection.ResetSimulationStateVariables(a_sd, this);
        m_predecessorMOCount = 0;
        m_simulationFlags.Clear();
        m_latestPredecessorReadyDate = 0;
        m_transferInfo = new TransferInfoCollection();
        m_products.ResetSimulationStateVariables();
    }

    /// <summary>
    /// Performed after RestSimulationStateVariables() on all objects in the system.
    /// </summary>
    internal virtual void SimulationInitialization(PlantManager a_plantManager, ProductRuleManager a_productRuleManager, ExtensionController a_extensionController, ICalculatedValueCacheManager a_cacheManager)
    {
        //__connectorResConstrainingSucOpResEligibility = null;
        //__connectorNonIntersectingResourcesToSucOpErr = false;

        m_connectorFromPredecessorsConstraints.Clear();

        //m_manufacturingOrderBatch_batchOperationNameData = null;
        m_manufacturingOrderBatch_batchOrderData_op_index = -1;
    }

    internal virtual void PostSimulationInitialization() { }

    internal bool Released
    {
        get
        {
            if (WaitingOnPredecessorReadyEvent)
            {
                return false;
            }

            if (!m_materialRequirementsCollection.Released)
            {
                return false;
            }

            if (WaitForFinishedPredecessorMOsAvailableEvent)
            {
                return false;
            }

            if (m_predecessorMOCount > 0)
            {
                return false;
            }

            if (WaitForHoldUntilEvent)
            {
                return false;
            }

            if (!ManufacturingOrder.Released)
            {
                return false;
            }

            if (WaitForLeftBatchNeighborReleaseEvent)
            {
                return false;
            }

            return AlternatePathNode.Released;
        }
    }

    /// <summary>
    /// Whether one or more Predecessors are Late.
    /// </summary>
    internal bool HasLatePredecessors
    {
        get
        {
            AlternatePath.AssociationCollection predecessorCollection = AlternatePathNode.Predecessors;
            for (int predecessorI = 0; predecessorI < predecessorCollection.Count; predecessorI++)
            {
                InternalOperation predecessorOperation = predecessorCollection[predecessorI].Predecessor.Operation;
                if ((!predecessorOperation.Finished && predecessorOperation.Late) || predecessorOperation.HasLatePredecessors)
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal abstract void SetupWaitForAnchorDroppedFlag();

    /// <summary>
    /// Used to help determine whether this operation has any predecessors that are being operated on.
    /// </summary>
    internal long m_hitCount;

    /// <summary>
    /// Among all the blocks that are scheduled determine whether the activity is scheduled in a cell. If 1 block's resource is in a cell then that cell is returned.
    /// If another block is scheduled in a different cell then null is returned since this is just an error on the part of the user in defining their cells.
    /// </summary>
    internal abstract Cell Cell { get; }

    //internal ManufacturingOrderBatch.BatchesOperationNameData m_manufacturingOrderBatch_batchOperationNameData;

    [Common.Attributes.DebugLogging(Common.Attributes.EDebugLoggingType.None)]
    internal int m_manufacturingOrderBatch_batchOrderData_op_index;
    #endregion Simulation

    #region Debugging
    public override string ToString()
    {
        string str = string.Format("{0};  Operation: Name '{1}';  ExternalId '{2}';", ManufacturingOrder, Name, ExternalId);
        #if DEBUG
        str = str + string.Format("  Id={0};", Id);
        #endif
        return str;
    }
    #endregion

    #region JIT
    private long m_standardOperationBufferTicks;

    // TimeSpan is subtracted from JITStartDate to allow some slack per Operation bases.
    internal long StandardOperationBufferTicks
    {
        get => m_standardOperationBufferTicks;
        private set
        {
            if (value < 0)
            {
                throw new PTValidationException("MinOperationBufferTicks value was invalid.");
            }

            m_standardOperationBufferTicks = value;
        }
    }

    public double StandardOperationBufferDays => TimeSpan.FromTicks(StandardOperationBufferTicks).TotalDays;

    /// <summary>
    /// JIT start to be on time for the buffer start.
    /// </summary>
    internal long DbrJitStartDateTicks => m_needInfo.EarliestJitBufferInfo.DbrJitStartDate;

    /// <summary>
    /// Actual JIT date to not be late
    /// </summary>
    internal long JitStartDateTicks => m_needInfo.EarliestJitBufferInfo.JitStartDate;
    
    protected bool JITStartDateNotSet => DbrJitStartDateTicks == PTDateTime.MinDateTime.Ticks;

    internal abstract void JITCalculateStartDates(long a_clockDate, long a_shippingBuffer);
    #endregion

    /// <summary>
    /// Omitted has several states. This breaks those down to either omitted or not.
    /// </summary>
    internal bool IsOmitted => Omitted != BaseOperationDefs.omitStatuses.NotOmitted;

    /// <summary>
    /// Omitted has several states. This returns true if the operation is Not omitted.
    /// If you want to perform !IsOmitted you will prefer this function because it takes
    /// one less operation to perform. Using IsOmitted would require a comparison and the ! operator.
    /// Using this function only requires a comparison operator.
    /// </summary>
    internal bool IsNotOmitted => Omitted == BaseOperationDefs.omitStatuses.NotOmitted;

    /// <summary>
    /// A common question that comes up in the scheduling logic is whether an operation is finished or omitted.
    /// </summary>
    public bool IsFinishedOrOmitted => Finished || IsOmitted;

    /// <summary>
    /// A common question that comes up in the scheduling logic is whether an operation is Not Finished and Not Omitted.
    /// </summary>
    internal bool IsNotFinishedAndNotOmitted => !Finished && IsNotOmitted;

    internal long GetLatestHoldTicks()
    {
        long holdUntilTicks;
        GetLatestHoldTicks(out holdUntilTicks);
        return holdUntilTicks;
    }

    internal HoldEnum GetLatestHoldTicks(out long o_holdUntilTicks)
    {
        o_holdUntilTicks = 0;
        HoldEnum holdType = HoldEnum.None;

        if (Job.Hold)
        {
            o_holdUntilTicks = Job.HoldUntilTicks;
            holdType = HoldEnum.Job;
        }

        if (ManufacturingOrder.Hold && ManufacturingOrder.HoldUntilTicks > o_holdUntilTicks)
        {
            o_holdUntilTicks = ManufacturingOrder.HoldUntilTicks;
            holdType = HoldEnum.MO;
        }

        if (OnHold && HoldUntilTicks > o_holdUntilTicks)
        {
            o_holdUntilTicks = HoldUntilTicks;
            holdType = HoldEnum.Operation;
        }

        return holdType;
    }

    #region Similarity
    internal virtual int SimilarityComparison(BaseOperation a_bo)
    {
        int v;

        if ((v = m_requiredFinishQty.CompareTo(a_bo.m_requiredFinishQty)) != 0)
        {
            return v;
        }

        if ((v = m_bools.CompareTo(a_bo.m_bools)) != 0)
        {
            return v;
        }

        return 0;
    }

    internal abstract void DetermineDifferences(BaseOperation a_op, int a_differenceTypes, System.Text.StringBuilder a_warnings);
    #endregion

    #region Connectors
    //internal BaseResource __connectorResConstrainingSucOpResEligibility;
    //bool __connectorNonIntersectingResourcesToSucOpErr;

    //internal void ___ConnectorResConstrainingSucOpResEligibilitySet(BaseResource br)
    //{
    //    if (!__connectorNonIntersectingResourcesToSucOpErr)
    //    {
    //        if (__connectorResConstrainingSucOpResEligibility != null)
    //        {
    //            if (__connectorResConstrainingSucOpResEligibility != br)
    //            {
    //                __connectorNonIntersectingResourcesToSucOpErr = true;
    //                __connectorResConstrainingSucOpResEligibility = null;
    //            }
    //        }
    //        else
    //        {
    //            __connectorResConstrainingSucOpResEligibility = br;
    //        }
    //    }
    //}

    //TODO: Replace with a sim class that caches the connector data
    private readonly Dictionary<BaseId, ResourceConnectorConstraintInfo> m_connectorFromPredecessorsConstraints = new ();

    /// <summary>
    /// The predecessor resource and operation with connectors that restrict the set of resources the successor operation can be scheduled on.
    /// </summary>
    internal Dictionary<BaseId, ResourceConnectorConstraintInfo> PotentialConnectorsFromPredecessors => m_connectorFromPredecessorsConstraints;

    /// <summary>
    /// Set potential resource connectors based on the predecessor scheduled resource
    /// </summary>
    /// <param name="a_predConnectorRes"></param>
    /// <param name="a_predecessorOp"></param>
    /// <param name="a_predConnectorResCnt"></param>
    /// <param name="a_connectorResReqIdx"></param>
    /// <exception cref="Exception"></exception>
    internal void SetPredOpConnectorResource(BaseResource a_predConnectorRes, InternalOperation a_predecessorOp, ResourceConnectorManager a_rcm)
    {
        if (m_connectorFromPredecessorsConstraints.Count != 0)
        {
            return;
        }

        IEnumerable<ResourceConnector> connectors = a_rcm.GetConnectorsFromResource(a_predConnectorRes.Id);
        ResourceConnectorConstraintInfo predConstraintInfo = new ResourceConnectorConstraintInfo(a_predConnectorRes, a_predecessorOp);
        foreach (ResourceConnector connector in connectors)
        {
            predConstraintInfo.AddPotentialConnector(connector);
        }

        if (predConstraintInfo.ResourceConnectors.Count > 0)
        {
            m_connectorFromPredecessorsConstraints.Add(a_predecessorOp.Id, predConstraintInfo);
        }

        //Only add an operation once. It's possible that an op has multiple activities
        //if (!m_connectorConstraints.ContainsKey(a_predecessorOp.Id))
        //{
        //    m_connectorConstraints.Add(a_predecessorOp.Id,
        //        new ResourceConnectorConstraintInfo
        //        {
        //            PredecessorResource = a_predConnectorRes,
        //            PredecessorOperation = a_predecessorOp,
        //            OperationResourceRequirementIdx = a_connectorResReqIdx,
        //            PredecessorReleaseTicks = PTDateTime.InvalidDateTime.Ticks //This value will be set when the predecessor operation releases the successor
        //        });
        //}
//#if DEBUG
//        if (!a_predConnectorRes.HasConnectors)
//        {
//            throw new Exception("The resource has no connections.");
//        }
//        #endif
    }
    #endregion

    /// <summary>
    /// Whether the operation is a root operation; the final operation of a path.
    /// </summary>
    internal bool IsRoot => Successors.Count == 0;

    /// <summary>
    /// Used to track transfer span end if the transfer end usage is not StartOfOperation
    /// Only set if there is a transfer from a predecessor to this operation.
    /// </summary>
    internal void SetOrUpdateTransferInfo(TransferInfo a_info)
    {
        //Set or combine multiple transfer constraints
        m_transferInfo.Merge(a_info);
    }

    private TransferInfoCollection m_transferInfo = new TransferInfoCollection();

    public TransferInfoCollection TransferInfo => m_transferInfo;
}