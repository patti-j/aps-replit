using PT.Scheduler.Simulation;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Each AlternatePath specifies one possible routing that can be followed for an MO.  Each MO has at least one AlternatePath.
/// </summary>
public partial class AlternatePath
{
    /// <summary>
    /// Defines the relationship between two nodes.
    /// </summary>
    public partial class Association
    {
        #region Simulation
        #region Simulation State bools
        private BoolVector32 m_simulationState;

        #region Constant Indexes
        private const int TransferSpanReleasedIdx = 0;
        private const int WaitingOnPredecessorResourceTransferSpanIdx = 1;
        private const int PredecessorReadyEventScheduledIdx = 2;
        private const int MaxDelaySetIdx = 3;
        #endregion Constant Indexes

        #region TransferSpanReleased
        internal bool TransferSpanReleased
        {
            get => m_simulationState[TransferSpanReleasedIdx];
            set => m_simulationState[TransferSpanReleasedIdx] = value;
        }
        #endregion TransferSpanReleased

        #region WaitingOnPredecessorResourceTransferSpan
        private bool WaitingOnPredecessorResourceTransferSpan
        {
            get => m_simulationState[WaitingOnPredecessorResourceTransferSpanIdx];
            set => m_simulationState[WaitingOnPredecessorResourceTransferSpanIdx] = value;
        }
        #endregion

        #region PredecessorReadyEventScheduled
        /// <summary>
        /// Whether the predecessor operation ready event has already been scheduled for this association.
        /// </summary>
        internal bool PredecessorReadyEventScheduled
        {
            get => m_simulationState[PredecessorReadyEventScheduledIdx];
            set => m_simulationState[PredecessorReadyEventScheduledIdx] = value;
        }

        #region MaxDelaySet
        /// <summary>
        /// Whether to use MaxDelay. Currently it will only by used if it's less than c_maxDelayMinUsageSpanTicks.
        /// Consider changing this once this feature is fully developed.
        /// </summary>
        internal bool MaxDelaySet
        {
            get => m_simulationState[MaxDelaySetIdx];
            set => m_simulationState[MaxDelaySetIdx] = value;
        }

        /// <summary>
        /// The Maximim length max delay can set to. If set higher, it won't be used.
        /// </summary>
        private const long c_maxDelayMinUsageSpanTicks = TimeSpan.TicksPerDay * 90;

        /// <summary>
        /// Initialize the value that determines whether Max Delay will be used.
        /// </summary>
        private void InitMaxDelaySet()
        {
            MaxDelaySet = MaxDelayTicks <= c_maxDelayMinUsageSpanTicks;
        }
        #endregion
        #endregion
        #endregion Simulation State bools

        #region Simulation Initialization
        internal void ResetSimulationStateVariables()
        {
            m_simulationState.Clear();
            InitMaxDelaySet();
            InternalOperation op = m_predecessor.Operation as InternalOperation;
            if (op != null)
            {
                if (op.IsFinishedOrOmitted // There won't be a TransferSpan since the op isn't going to schedule.
                    ||
                    op.EligibleForTransferQtyOverlap() ||
                    op.EligibleForAtFirstTransferOverlap())
                {
                    TransferSpanReleased = true;
                }
            }

            Successor.ResetSimulationStateVariables();
        }
        #endregion Simulation Initialization

        internal bool Released
        {
            get
            {
                if (WaitingOnPredecessorResourceTransferSpan)
                {
                    return false;
                }

                if (TransferSpanReleased 
                    || TransferStart == OperationDefs.EOperationTransferPoint.NoTransfer 
                    || TransferEnd == OperationDefs.EOperationTransferPoint.NoTransfer)
                {
                    return true;
                }

                return false;
            }
        }
        #endregion Simulation

        internal long GetOverlapPercentCompleteReleaseTime(long minimumPredecessorReadyTime, Resource a_predRes, long a_startOfProcTicks = -1)
        {
            ResourceOperation predOp = Predecessor.Operation as ResourceOperation;

            if (OverlapPercentCompleteOverlapPossible)
            {
                InternalActivity predAct = predOp.Activities.GetByIndex(0);
                {
                    long reqProcTicks = 0;
                    if (predOp.Activities.Count == 1 && predAct.Scheduled)
                    {
                        reqProcTicks = predAct.GetRequiredProcessingTicks();
                    }
                    else
                    {
                        reqProcTicks = predOp.GetTotalRequiredProcessingTicks();
                    }

                    long startOfProc;
                    if (a_startOfProcTicks == -1)
                    {
                        startOfProc = predAct.GetScheduledEndOfSetupTicks();
                    }
                    else
                    {
                        startOfProc = a_startOfProcTicks;
                    }

                    long capacityRequired = (long)(reqProcTicks * OverlapPercentComplete - predAct.ReportedRun);
                    long overlapTime = minimumPredecessorReadyTime;

                    if (capacityRequired > 0)
                    {
                        if (a_predRes != null)
                        {
                            ActivityResourceBufferInfo bufferInfo = predAct.GetBufferResourceInfo(a_predRes.Id);
                            FindCapacityResult capacityResult = a_predRes.FindCapacity(startOfProc, capacityRequired, predOp.CanPause, null, MainResourceDefs.usageEnum.Run, false, false, predOp.ResourceRequirements.PrimaryResourceRequirement.CapacityCode, false, predAct.PeopleUsage, predAct.NbrOfPeople, out bool willSpanLateOnlyRci);
                            if (capacityResult.ResultStatus == SchedulableSuccessFailureEnum.Success)
                            {
                                //Check if run end penetrates the buffer end rerun the capacity function if any late only RCI were spanned 
                                if (willSpanLateOnlyRci && capacityResult.CapacityUsageProfile[^1].EndTicks > bufferInfo.BufferEndDate)
                                {
                                    capacityResult = a_predRes.FindCapacity(startOfProc, capacityRequired, predOp.CanPause, null, MainResourceDefs.usageEnum.Run, false, true, predOp.ResourceRequirements.PrimaryResourceRequirement.CapacityCode, false, predAct.PeopleUsage, predAct.NbrOfPeople, out willSpanLateOnlyRci);
                                    if (capacityResult.ResultStatus == SchedulableSuccessFailureEnum.Success)
                                    {
                                        overlapTime = Math.Max(minimumPredecessorReadyTime, capacityResult.FinishDate);
                                    }
                                }
                            }
                        }
                    }

                    return overlapTime;
                }
            }

            return -1;
        }

        internal bool OverlapPercentCompleteOverlapPossible
        {
            get
            {
                ResourceOperation predOp = Predecessor.Operation as ResourceOperation;
                return predOp != null && predOp.Activities.Count == 1; //&& predOp.TimeBasedReporting;
            }
        }

        #region Similarity
        /// <summary>
        /// Determines which scheduling fields are different.
        /// </summary>
        /// <param name="a_compare"></param>
        /// <param name="a_sb"></param>
        internal void DetermineDifferences(Association a_compare, int a_differenceTypes, System.Text.StringBuilder a_warnings)
        {
            if (m_maxDelayTicks != a_compare.m_maxDelayTicks)
            {
                a_warnings.Append("Max delay ticks;");
            }

            if (m_overlapPercentComplete != a_compare.m_overlapPercentComplete)
            {
                a_warnings.Append("Overlap percent complete;");
            }

            if (m_overlapSetups != a_compare.m_overlapSetups)
            {
                a_warnings.Append("Overlap setups;");
            }

            if (m_overlapTransferTicks != a_compare.m_overlapTransferTicks)
            {
                a_warnings.Append("overlap transfer ticks;");
            }

            if (m_overlapType != a_compare.m_overlapType)
            {
                a_warnings.Append("overlap type;");
            }

            if (m_transferSpanTicks != a_compare.m_transferSpanTicks)
            {
                a_warnings.Append("transfer span ticks;");
            }

            if (m_usageQtyPerCycle != a_compare.m_usageQtyPerCycle)
            {
                a_warnings.Append("usage quantity per cycle;");
            }

            if (m_transferStart != a_compare.m_transferStart)
            {
                a_warnings.Append("transfer start;");
            }

            if (m_transferEnd != a_compare.m_transferEnd)
            {
                a_warnings.Append("transfer end;");
            }
        }
        #endregion
    }
}