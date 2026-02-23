using System.Text;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.ERPTransmissions;
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
        #region IPTSerializable Members
        [Common.Attributes.DebugLogging(Common.Attributes.EDebugLoggingType.None)]
        private readonly BaseId m_successorIdSerializationTemp;

        // Used to fix references during deserialization.
        internal Association(IReader reader, Node predecessorNode)
        {
            if (reader.VersionNumber >= 12205)
            {
                m_predecessor = predecessorNode;
                reader.Read(out m_usageQtyPerCycle);
                reader.Read(out m_transferSpanTicks);
                reader.Read(out m_maxDelayTicks);
                m_successorIdSerializationTemp = new BaseId(reader);

                // Read overlap settings.
                int overlapTransferTypeTemp;
                reader.Read(out overlapTransferTypeTemp);
                m_overlapType = (InternalOperationDefs.overlapTypes)overlapTransferTypeTemp;

                reader.Read(out m_overlapTransferTicks);
                reader.Read(out m_overlapPercentComplete);
                reader.Read(out m_overlapSetups);
                int val;
                reader.Read(out val);
                m_autoFinishPred = (InternalOperationDefs.autoFinishPredecessorOptions)val;
                reader.Read(out m_allowManualConnectorViolation);
                reader.Read(out m_transferDuringPredeccessorOnlineTicks);

                reader.Read(out short usageVal);
                m_transferStart = (OperationDefs.EOperationTransferPoint)usageVal;
                reader.Read(out usageVal);
                m_transferEnd = (OperationDefs.EOperationTransferPoint)usageVal;
            }
            else if (reader.VersionNumber >= 623)
            {
                m_predecessor = predecessorNode;
                reader.Read(out m_usageQtyPerCycle);
                reader.Read(out m_transferSpanTicks);
                reader.Read(out m_maxDelayTicks);
                m_successorIdSerializationTemp = new BaseId(reader);

                // Read overlap settings.
                int overlapTransferTypeTemp;
                reader.Read(out overlapTransferTypeTemp);
                m_overlapType = (InternalOperationDefs.overlapTypes)overlapTransferTypeTemp;

                reader.Read(out m_overlapTransferTicks);
                reader.Read(out m_overlapPercentComplete);
                reader.Read(out m_overlapSetups);
                int val;
                reader.Read(out val);
                m_autoFinishPred = (InternalOperationDefs.autoFinishPredecessorOptions)val;
                reader.Read(out m_allowManualConnectorViolation);
                reader.Read(out m_transferDuringPredeccessorOnlineTicks);

                m_transferStart = OperationDefs.EOperationTransferPoint.NoTransfer;
                m_transferEnd = OperationDefs.EOperationTransferPoint.NoTransfer;
            }

            if (reader.VersionNumber < 12310)
            {
                //TransferSpan of 0 is now allowed, so we need to reset the Start and End for backwards compatibility
                if (TransferSpan == TimeSpan.Zero)
                {
                    m_transferStart = OperationDefs.EOperationTransferPoint.NoTransfer;
                    m_transferEnd = OperationDefs.EOperationTransferPoint.NoTransfer;
                }
            }

            if (reader.VersionNumber < 740)
            {
                //OverlapTransferSpan used to be a max number, but it causes data issues, it should default to 0
                if (m_overlapTransferTicks > TimeSpan.FromDays(3650).Ticks)
                {
                    m_overlapTransferTicks = 0;
                }
            }
        }

        internal void DeserializationFixups(Node[] opBaseIds)
        {
            m_successor = opBaseIds[m_successorIdSerializationTemp.Value];
            m_successor.Predecessors.Add(this);
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(m_usageQtyPerCycle);
            writer.Write(m_transferSpanTicks);
            writer.Write(m_maxDelayTicks);
            m_successor.Id.Serialize(writer);

            // Serialize overlap information.
            writer.Write((int)m_overlapType);
            writer.Write(m_overlapTransferTicks);
            writer.Write(m_overlapPercentComplete);
            writer.Write(m_overlapSetups);
            writer.Write((int)m_autoFinishPred);

            writer.Write(m_allowManualConnectorViolation);
            writer.Write(m_transferDuringPredeccessorOnlineTicks);
            writer.Write((short)m_transferStart);
            writer.Write((short)m_transferEnd);
        }
        #endregion

        #region Construction
        internal Association(Node predecessor, Node successor, JobT.PredecessorOperationAttributes predecessorOperationAttributes)
        {
            m_predecessor = predecessor;
            m_successor = successor;

            m_maxDelayTicks = predecessorOperationAttributes.MaxDelay.Ticks;
            m_transferSpanTicks = predecessorOperationAttributes.TransferSpan.Ticks;
            UsageQtyPerCycle = predecessorOperationAttributes.UsageQtyPerCycle;

            // Overlap information.
            m_overlapType = predecessorOperationAttributes.OverlapType;
            OverlapTransferTicks = predecessorOperationAttributes.OverlapTransferSpan.Ticks;
            OverlapPercentComplete = predecessorOperationAttributes.OverlapPercentComplete;
            m_overlapSetups = predecessorOperationAttributes.OverlapSetups;
            m_transferDuringPredeccessorOnlineTicks = predecessorOperationAttributes.TransferDuringPredeccessorOnlineTime;

            m_autoFinishPred = predecessorOperationAttributes.AutoFinishPredecessor;
            m_allowManualConnectorViolation = predecessorOperationAttributes.AllowManualConnectorViolation;
            m_transferStart = predecessorOperationAttributes.TransferStart;
            m_transferEnd = predecessorOperationAttributes.TransferEnd;
        }

        internal Association(Node predecessor, Node successor, Association sourceAssociation)
        {
            m_predecessor = predecessor;
            m_successor = successor;

            m_maxDelayTicks = sourceAssociation.MaxDelay.Ticks;
            m_transferSpanTicks = sourceAssociation.TransferSpan.Ticks;
            UsageQtyPerCycle = sourceAssociation.UsageQtyPerCycle;

            // Overlap information.
            m_overlapType = sourceAssociation.m_overlapType;
            m_overlapTransferTicks = sourceAssociation.m_overlapTransferTicks;
            OverlapPercentComplete = sourceAssociation.OverlapPercentComplete;
            m_overlapSetups = sourceAssociation.m_overlapSetups;
            m_transferDuringPredeccessorOnlineTicks = sourceAssociation.m_transferDuringPredeccessorOnlineTicks;

            m_autoFinishPred = sourceAssociation.AutoFinishPredecessor;
            m_allowManualConnectorViolation = sourceAssociation.AllowManualConnectorViolation;

            m_transferStart = sourceAssociation.TransferStart;
            m_transferEnd = sourceAssociation.TransferEnd;
        }
        #endregion

        #region Predecessors and successors
        private readonly Node m_predecessor;

        public Node Predecessor => m_predecessor;

        private Node m_successor;

        public Node Successor => m_successor;
        #endregion

        #region Job DataSet
        internal void PopulateJobDataSet(JobDataSet.AlternatePathNodeRow a_row)
        {
            a_row.UsageQtyPerCycle = UsageQtyPerCycle;
            a_row.TransferHrs = TransferSpan.TotalHours;
            a_row.MaxDelayHrs = MaxDelay.TotalHours;
            a_row.OverlapTransferHrs = OverlapTransferSpan.TotalHours;
            a_row.OverlapType = OverlapType.ToString();
            a_row.OverlapPercentComplete = OverlapPercentComplete * 100;
            a_row.TransferDuringPredeccessorOnlineTime = TransferDuringPredeccessorOnlineTime;
            a_row.AutoFinishPredecessor = AutoFinishPredecessor.ToString();
            a_row.AllowManualConnectorViolation = AllowManualConnectorViolation;
            a_row.TransferStart = TransferStart.ToString();
            a_row.TransferEnd = TransferEnd.ToString();
        }

        internal static void PopulateJobDataSetDefaults(JobDataSet.AlternatePathNodeRow a_row)
        {
            a_row.UsageQtyPerCycle = 1;
            a_row.TransferHrs = 0;
            a_row.MaxDelayHrs = TimeSpan.MaxValue.TotalHours;
            a_row.OverlapTransferHrs = 0;
            a_row.OverlapType = InternalOperationDefs.overlapTypes.NoOverlap.ToString();
            a_row.OverlapPercentComplete = 0;
            a_row.TransferDuringPredeccessorOnlineTime = false;
            a_row.AutoFinishPredecessor = InternalOperationDefs.autoFinishPredecessorOptions.NoAutoFinish.ToString();
            a_row.TransferStart = OperationDefs.EOperationTransferPoint.NoTransfer.ToString();
            a_row.TransferEnd = OperationDefs.EOperationTransferPoint.NoTransfer.ToString();
        }
        #endregion

        #region Predecessor operation attributes
        private long m_transferSpanTicks;

        /// <summary>
        /// Amount of time to move material from an Operation to its Successor.  The successor cannot start until this time passes.This can be specified from the ERP system or be calculated using a customizable
        /// script.
        /// </summary>
        internal long TransferSpanTicks
        {
            get => m_transferSpanTicks;
            private set => m_transferSpanTicks = value;
        }

        private bool m_transferDuringPredeccessorOnlineTicks;

        /// <summary>
        /// Whether the transfer time occurs during the predecessor operation's primary resource
        /// online time.
        /// </summary>
        public bool TransferDuringPredeccessorOnlineTime
        {
            get => m_transferDuringPredeccessorOnlineTicks;
            private set => m_transferDuringPredeccessorOnlineTicks = value;
        }

        /// <summary>
        /// Amount of time to move material from an Operation to its Successor.  The successor cannot start until this time passes.This can be specified from the ERP system or be calculated using a customizable
        /// script.
        /// When used in combination with OverlapTransferSpan the affect is additive. For instance, if TransferSpan is 1 minute and OverlapTransferSpan is 0 minutes, then the successor operation won't start
        /// until 1 minute after the predecessor.
        /// </summary>
        public TimeSpan TransferSpan
        {
            get => new (m_transferSpanTicks);
            private set => m_transferSpanTicks = value.Ticks;
        }

        private OperationDefs.EOperationTransferPoint m_transferStart = OperationDefs.EOperationTransferPoint.NoTransfer;

        /// <summary>
        /// When the operation transfer time starts.
        /// </summary>
        public OperationDefs.EOperationTransferPoint TransferStart
        {
            get => m_transferStart;
            private set => m_transferStart = value;
        }

        private OperationDefs.EOperationTransferPoint m_transferEnd;

        /// <summary>
        /// When the transfer time stops on the next operation.
        /// If this is not StartOfOperation, then the successor can start before the transfer time is up.
        /// </summary>
        public OperationDefs.EOperationTransferPoint TransferEnd
        {
            get => m_transferEnd;
            private set => m_transferEnd = value;
        }

        private long m_maxDelayTicks;

        /// <summary>
        /// Specifies that the Successor Operation must start setup within this amount of time from the end of the Predecessor Operation's scheduled finish. This is like a shelf-life. For example if you paint
        /// something you have to enamel it within the next 24 hours. Individual dispatch rules may or may not attempt to expedite such Operations.  So, if the maxDelay is reached during a Simulation then the
        /// Predecessor and any of the Predecessor's other Successors are unscheduled in an attempt to find a future period where the maxDelay will be met.
        /// </summary>
        internal long MaxDelayTicks => m_maxDelayTicks;

        /// <summary>
        /// Specifies that the Successor Operation must start setup within this amount of time from the end of the Predecessor Operation's scheduled finish.
        /// This is like a shelf-life. For example if you paint something you have to enamel it within the next 24 hours. If an operation must begin
        /// immediately after it's predecessor you can set this value to 0.
        /// Individual dispatch rules may or may not attempt to expedite such Operations.
        /// There is a Flag setup to warn if this maximum is violated.
        /// A value of long.MaxValue (which is equal to 9223372036854775807) means this value is not set.
        /// </summary>
        public TimeSpan MaxDelay
        {
            get => new (m_maxDelayTicks);

            private set
            {
                if (m_maxDelayTicks != value.Ticks)
                {
                    m_maxDelayTicks = value.Ticks;
                }
            }
        }

        /// <summary>
        /// The quantity of the Operation that is needed for each cycle of the Successor Operation.  This is used for calculating when a Successor Activity can start in an overlap situations.
        /// </summary>
        private decimal m_usageQtyPerCycle;

        public decimal UsageQtyPerCycle
        {
            get => m_usageQtyPerCycle;
            private set => m_usageQtyPerCycle = value;
        }

        private InternalOperationDefs.overlapTypes m_overlapType = InternalOperationDefs.overlapTypes.NoOverlap;

        /// <summary>
        /// NotAllowed means the Predecessor must be finished. TransferQty means parts are transferred in groups of OverlapTransferQty to the successor operation. The successor will not be scheduled to start
        /// until enough material is available to allow it to run continuously. Overlap is currently supported for linear operations that have one activity.
        /// </summary>
        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public InternalOperationDefs.overlapTypes OverlapType => m_overlapType;

        private long m_overlapTransferTicks;

        /// <summary>
        /// Successor Operations can start once the TransferSpan has passed (in calendar time) after the Operation's Scheduled Run Start.
        /// </summary>
        internal long OverlapTransferTicks
        {
            get => m_overlapTransferTicks;
            private set
            {
                if (value < 0)
                {
                    throw new PTValidationException("2832", new object[] { value });
                }

                m_overlapTransferTicks = value;
            }
        }

        /// <summary>
        /// Successor Operations can start once the TransferSpan has passed (in calendar time) after the Operation's Scheduled Run Start.
        /// When used in combination with OverlapTransferSpan the affect is additive. For instance, if TransferSpan is 1 minute and OverlapTransferSpan is 0 minutes, then the successor operation won't start
        /// until 1 minute after the predecessor.
        /// </summary>
        [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
        public TimeSpan OverlapTransferSpan => new (m_overlapTransferTicks);

        private decimal m_overlapPercentComplete;

        /// <summary>
        /// If "Overlap Type" is set to "Percent Complete" then the predecessor operation will only constrain the successor operation up
        /// to this level of completion.
        /// To use this feature the predecessor operation must be set for Time Based Progress reporting and the activity must not be split.
        /// </summary>
        public decimal OverlapPercentComplete
        {
            get => m_overlapPercentComplete;
            private set
            {
                if (value < 0 || value > 1)
                {
                    // The grid for this displays percentages as out of 100 instead of 0 to 1 
                    // so this error message parameter being multiplied by 100 is to accommodate this shift. 
                    throw new PTValidationException("2976", new object[] { value * 100 });
                }

                m_overlapPercentComplete = value;
            }
        }

        internal bool GetOverlapTransferSpanReleaseTicks(out long o_overlapTransferSpanReleaseTicks)
        {
            long scheduledStartTicks;

            if (Predecessor.Operation.GetScheduledStartDateTicks(out scheduledStartTicks))
            {
                o_overlapTransferSpanReleaseTicks = GetOverlapTransferSpanReleaseTicks(scheduledStartTicks);
                return true;
            }

            o_overlapTransferSpanReleaseTicks = PTDateTime.MinDateTime.Ticks;

            return false;
        }

        internal long GetOverlapTransferSpanReleaseTicks(long opScheduledStartTicks)
        {
            #if DEBUG
            if (OverlapType != InternalOperationDefs.overlapTypes.TransferSpan)
            {
                throw new Exception("GetOverlapTransferSpanReleaseTicks() error. The association isn't the right type for this function call.");
            }
            #endif

            return opScheduledStartTicks + m_overlapTransferTicks;
        }

        /// <summary>
        /// For overlap type InternalOperationDefs.overlapTypes.TransferSpanBeforeStartOfPredecessor calculate the earliest the
        /// successor operation can start before the predecessor operation.
        /// </summary>
        /// <param name="a_predOpStartTicks">The start of the predecessor.</param>
        /// <returns>The earliest the successor can start.</returns>
        internal long CalcEarliestSucOpCanStartForOverlapByTransferSpanBeforeStartOfPredecessor(long a_predOpStartTicks)
        {
            #if DEBUG
            if (OverlapType != InternalOperationDefs.overlapTypes.TransferSpanBeforeStartOfPredecessor)
            {
                throw new Exception("GetOverlapTransferSpanReleaseTicks() error. The association isn't the right type for this function call.");
            }
            #endif

            return a_predOpStartTicks - m_overlapTransferTicks;
        }

        /// <summary>
        /// For overlap type InternalOperationDefs.overlapTypes.TransferSpanBeforeStartOfPredecessor calculate the earliest the
        /// successor operation can start before the predecessor operation.
        /// </summary>
        /// <param name="o_overlapTransferSpanReleaseTicks">The earliest the successor can start.</param>
        /// <returns>Whether the date was calculated.</returns>
        internal bool CalcEarliestSucOpCanStartForOverlapByTransferSpanBeforeStartOfPredecessor(out long o_overlapTransferSpanReleaseTicks)
        {
            #if DEBUG
            if (OverlapType != InternalOperationDefs.overlapTypes.TransferSpanBeforeStartOfPredecessor)
            {
                throw new Exception("GetOverlapTransferSpanReleaseTicksBeforeStart() error. The association isn't the right type for this function call.");
            }
            #endif

            long scheduledStartTicks;

            if (Predecessor.Operation.GetScheduledStartDateTicks(out scheduledStartTicks))
            {
                o_overlapTransferSpanReleaseTicks = scheduledStartTicks - m_overlapTransferTicks;
                return true;
            }

            o_overlapTransferSpanReleaseTicks = PTDateTime.MinDateTime.Ticks;
            return false;
        }

        internal long GetOverlapTransferSpanAfterSetupReleaseTicks()
        {
            InternalOperation io = (InternalOperation)Predecessor.Operation;
            InternalActivity leadAct = io.GetLeadActivity();

            if (leadAct != null && leadAct.ProductionStatus == InternalActivityDefs.productionStatuses.Running && leadAct.ReportedProcessingStartTicks >= 0)
            {
                return GetOverlapTransferSpanAfterSetupReleaseTicksFromStartOfProcessing(leadAct.ReportedProcessingStartTicks);
            }

            return GetOverlapTransferSpanAfterSetupReleaseTicksFromStartOfProcessing(io.GetScheduledStartOfProcessingTicks(leadAct));
        }

        /// <summary>
        /// Gets overlap release date given a start of processing date.
        /// </summary>
        /// <param name="a_processingStartTicks">Scheduled or reported processing start ticks.</param>
        /// <returns></returns>
        internal long GetOverlapTransferSpanAfterSetupReleaseTicksFromStartOfProcessing(long a_processingStartTicks)
        {
            #if DEBUG
            if (OverlapType != InternalOperationDefs.overlapTypes.TransferSpanAfterSetup)
            {
                throw new Exception("GetOverlapTransferSpanReleaseTicks() error. The association isn't the right type for this function call.");
            }
            #endif
            return a_processingStartTicks + m_overlapTransferTicks;
        }

        private bool m_overlapSetups = true;

        /// <summary>
        /// Whether successors can start setting up before the setup is complete on this operation.
        /// </summary>
        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public bool OverlapSetups
        {
            get => m_overlapSetups;
            private set => m_overlapSetups = value;
        }

        private InternalOperationDefs.autoFinishPredecessorOptions m_autoFinishPred = InternalOperationDefs.autoFinishPredecessorOptions.NoAutoFinish;

        /// <summary>
        /// Can be used to automatically finish a predecessor operation based on progress on a successor operation in order to avoid having to explicitly finish the predecessor.
        /// </summary>
        public InternalOperationDefs.autoFinishPredecessorOptions AutoFinishPredecessor
        {
            get => m_autoFinishPred;
            private set => m_autoFinishPred = value;
        }

        private bool m_allowManualConnectorViolation;

        /// <summary>
        /// If true then Operations can be moved to Resources that are invalid based on the predecessor Resources Connectors.
        /// This can be used to allow a tempoary violation which is Flagged thus giving the user more flexibility to manually reconsile as well as make exceptions.
        /// </summary>
        public bool AllowManualConnectorViolation
        {
            get => m_allowManualConnectorViolation;
            private set => m_allowManualConnectorViolation = value;
        }
        #endregion

        #region Update
        /// <summary>
        /// Update the values in this association with the values in a new association.
        /// </summary>
        /// <param name="newAssociation">The updated association whose values you want to use to update this association with.</param>
        /// <returns>Whether any updates have occurred.</returns>
        internal bool Update(Association a_newAssociation)
        {
            bool updateResult = false;

            if (m_maxDelayTicks != a_newAssociation.m_maxDelayTicks)
            {
                updateResult = true;
                m_maxDelayTicks = a_newAssociation.m_maxDelayTicks;
            }

            if (m_transferSpanTicks != a_newAssociation.m_transferSpanTicks)
            {
                updateResult = true;
                m_transferSpanTicks = a_newAssociation.m_transferSpanTicks;
            }

            if (m_usageQtyPerCycle != a_newAssociation.m_usageQtyPerCycle)
            {
                updateResult = true;
                m_usageQtyPerCycle = a_newAssociation.m_usageQtyPerCycle;
            }

            if (m_overlapSetups != a_newAssociation.m_overlapSetups)
            {
                updateResult = true;
                m_overlapSetups = a_newAssociation.m_overlapSetups;
            }

            if (m_overlapTransferTicks != a_newAssociation.m_overlapTransferTicks)
            {
                updateResult = true;
                m_overlapTransferTicks = a_newAssociation.m_overlapTransferTicks;
            }

            if (m_overlapType != a_newAssociation.m_overlapType)
            {
                updateResult = true;
                m_overlapType = a_newAssociation.m_overlapType;
            }

            if (OverlapPercentComplete != a_newAssociation.OverlapPercentComplete)
            {
                updateResult = true;
                OverlapPercentComplete = a_newAssociation.OverlapPercentComplete;
            }

            if (m_autoFinishPred != a_newAssociation.AutoFinishPredecessor)
            {
                updateResult = true;
                m_autoFinishPred = a_newAssociation.AutoFinishPredecessor;
            }

            if (m_allowManualConnectorViolation != a_newAssociation.AllowManualConnectorViolation)
            {
                updateResult = true;
                m_allowManualConnectorViolation = a_newAssociation.AllowManualConnectorViolation;
            }

            if (TransferDuringPredeccessorOnlineTime != a_newAssociation.TransferDuringPredeccessorOnlineTime)
            {
                updateResult = true;
                TransferDuringPredeccessorOnlineTime = a_newAssociation.TransferDuringPredeccessorOnlineTime;
            }

            if (TransferStart != a_newAssociation.TransferStart)
            {
                updateResult = true;
                TransferStart = a_newAssociation.TransferStart;
            }

            if (TransferEnd != a_newAssociation.TransferEnd)
            {
                updateResult = true;
                TransferEnd = a_newAssociation.TransferEnd;
            }

            return updateResult;
        }
        #endregion

        public override string ToString()
        {
            StringBuilder sb = new ();
            string predName = GetOpName(Predecessor);
            string sucName = GetOpName(Successor);
            sb.AppendFormat("Pred={0}; Suc={1}; TransferSpan={2}; OverlapType={3}; OverlapSetups={4}; OverlapTransferSpan={5}; UsageQtyPerCycle={6};  MaxDelay={7}; TransferStart={8}; TransferEnd={9}".Localize(), predName, sucName, TransferSpan, OverlapType, OverlapSetups, OverlapTransferSpan, UsageQtyPerCycle, MaxDelay, TransferStart.ToString().Localize(), TransferEnd.ToString().Localize());
            return sb.ToString();
        }

        private string GetOpName(Node node)
        {
            if (node == null)
            {
                return "\"NULL\"".Localize();
            }

            return node.Operation.Name;
        }
    }
}