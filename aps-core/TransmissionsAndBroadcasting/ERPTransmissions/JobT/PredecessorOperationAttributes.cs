using PT.SchedulerDefinitions;

namespace PT.ERPTransmissions;

public partial class JobT
{
    /// <summary>
    /// This class is used to describe a node's relationships with its successor operations.
    /// This same class use to be used by the successor operation; this was a mistake and
    /// its usage was transferred to the predecessor node.
    /// </summary>
    public class PredecessorOperationAttributes : IPTSerializable
    {
        #region PT Serialization
        public PredecessorOperationAttributes(IReader reader)
        {
            if (reader.VersionNumber >= 12207)
            {
                m_bools = new BoolVector32(reader);
                reader.Read(out m_transferSpan);
                reader.Read(out m_maxDelay);
                reader.Read(out m_usageQtyPerCycle);
                reader.Read(out m_operationExternalId);

                // Read overlap settings.
                int overlapTransferTypeTemp;
                reader.Read(out overlapTransferTypeTemp);
                m_overlapType = (InternalOperationDefs.overlapTypes)overlapTransferTypeTemp;
                reader.Read(out m_overlapTransferTicks);
                reader.Read(out m_overlapPercentComplete);
                reader.Read(out m_overlapSetups);
                int autoFinishTemp;
                reader.Read(out autoFinishTemp);
                m_autoFinishPred = (InternalOperationDefs.autoFinishPredecessorOptions)autoFinishTemp;
                reader.Read(out short usageVal);
                m_transferStart = (OperationDefs.EOperationTransferPoint)usageVal;
                reader.Read(out usageVal);
                m_transferEnd = (OperationDefs.EOperationTransferPoint)usageVal;
            }

            #region 697
            //m_overlapPercentComplete changed from byte to decimal
            else if (reader.VersionNumber >= 697)
            {
                m_bools = new BoolVector32(reader);
                reader.Read(out m_transferSpan);
                reader.Read(out m_maxDelay);
                reader.Read(out m_usageQtyPerCycle);
                reader.Read(out m_operationExternalId);

                // Read overlap settings.
                int overlapTransferTypeTemp;
                reader.Read(out overlapTransferTypeTemp);
                m_overlapType = (InternalOperationDefs.overlapTypes)overlapTransferTypeTemp;
                reader.Read(out m_overlapTransferTicks);
                reader.Read(out m_overlapPercentComplete);
                reader.Read(out m_overlapSetups);
                int autoFinishTemp;
                reader.Read(out autoFinishTemp);
                m_autoFinishPred = (InternalOperationDefs.autoFinishPredecessorOptions)autoFinishTemp;
            }
            #endregion 697

            #region 624
            else if (reader.VersionNumber >= 624)
            {
                m_bools = new BoolVector32(reader);
                reader.Read(out m_transferSpan);
                reader.Read(out m_maxDelay);
                reader.Read(out m_usageQtyPerCycle);
                reader.Read(out m_operationExternalId);

                // Read overlap settings.
                int overlapTransferTypeTemp;
                reader.Read(out overlapTransferTypeTemp);
                m_overlapType = (InternalOperationDefs.overlapTypes)overlapTransferTypeTemp;
                reader.Read(out m_overlapTransferTicks);
                reader.Read(out byte overlapByte);
                m_overlapPercentComplete = Convert.ToDecimal(overlapByte);
                reader.Read(out m_overlapSetups);
                int autoFinishTemp;
                reader.Read(out autoFinishTemp);
                m_autoFinishPred = (InternalOperationDefs.autoFinishPredecessorOptions)autoFinishTemp;
            }
            #endregion

            #region 289
            else if (reader.VersionNumber >= 289)
            {
                reader.Read(out m_transferSpan);
                reader.Read(out m_maxDelay);
                reader.Read(out m_usageQtyPerCycle);
                reader.Read(out m_operationExternalId);

                // Read overlap settings.
                int overlapTransferTypeTemp;
                reader.Read(out overlapTransferTypeTemp);
                m_overlapType = (InternalOperationDefs.overlapTypes)overlapTransferTypeTemp;
                reader.Read(out m_overlapTransferTicks);
                reader.Read(out byte overlapByte);
                m_overlapPercentComplete = Convert.ToDecimal(overlapByte);
                reader.Read(out m_overlapSetups);
                int autoFinishTemp;
                reader.Read(out autoFinishTemp);
                m_autoFinishPred = (InternalOperationDefs.autoFinishPredecessorOptions)autoFinishTemp;
                bool allowManualConnectorViolation;
                reader.Read(out allowManualConnectorViolation);
                AllowManualConnectorViolation = allowManualConnectorViolation;
            }
            #endregion

            #region Version 284
            else if (reader.VersionNumber >= 284)
            {
                reader.Read(out m_transferSpan);
                reader.Read(out m_maxDelay);
                reader.Read(out m_usageQtyPerCycle);
                reader.Read(out m_operationExternalId);

                // Read overlap settings.
                int overlapTransferTypeTemp;
                reader.Read(out overlapTransferTypeTemp);
                m_overlapType = (InternalOperationDefs.overlapTypes)overlapTransferTypeTemp;
                reader.Read(out m_overlapTransferTicks);
                reader.Read(out byte overlapByte);
                m_overlapPercentComplete = Convert.ToDecimal(overlapByte);
                reader.Read(out m_overlapSetups);
                int autoFinishTemp;
                reader.Read(out autoFinishTemp);
                m_autoFinishPred = (InternalOperationDefs.autoFinishPredecessorOptions)autoFinishTemp;
            }
            #endregion

            #region Version 218
            else if (reader.VersionNumber >= 218)
            {
                reader.Read(out m_transferSpan);
                reader.Read(out m_maxDelay);
                reader.Read(out m_usageQtyPerCycle);
                reader.Read(out m_operationExternalId);

                // Read overlap settings.
                int overlapTransferTypeTemp;
                reader.Read(out overlapTransferTypeTemp);
                m_overlapType = (InternalOperationDefs.overlapTypes)overlapTransferTypeTemp;
                reader.Read(out m_overlapTransferTicks);
                reader.Read(out byte overlapByte);
                m_overlapPercentComplete = Convert.ToDecimal(overlapByte);
                reader.Read(out m_overlapSetups);
            }
            #endregion

            #region 140
            else if (reader.VersionNumber >= 140)
            {
                reader.Read(out m_transferSpan);
                reader.Read(out m_maxDelay);
                reader.Read(out m_usageQtyPerCycle);
                reader.Read(out m_operationExternalId);

                // Read overlap settings.
                int overlapTransferTypeTemp;
                reader.Read(out overlapTransferTypeTemp);
                m_overlapType = (InternalOperationDefs.overlapTypes)overlapTransferTypeTemp;
                reader.Read(out m_overlapTransferTicks);
                reader.Read(out m_overlapSetups);
            }
            #endregion

            #region 1
            else if (reader.VersionNumber >= 1)
            {
                reader.Read(out m_transferSpan);
                reader.Read(out m_maxDelay);
                reader.Read(out m_usageQtyPerCycle);
                reader.Read(out m_operationExternalId);
            }
            #endregion

            #if DEBUG
            //TODO: Figure out when this happened, and add a reader.VersionNumber constraint
            if (OverlapPercentComplete > 1 && OverlapPercentComplete <= 100)
            {
                //This was changed from 1 - 100 to 0 - 1 at some point. This breaks all old recordings. So for MR testing convert it.
                OverlapPercentComplete = OverlapPercentComplete / 100;
            }
            #endif
        }

        public virtual void Serialize(IWriter writer)
        {
            m_bools.Serialize(writer);
            writer.Write(m_transferSpan);
            writer.Write(m_maxDelay);
            writer.Write(m_usageQtyPerCycle);
            writer.Write(m_operationExternalId);

            // Serialize overlap information.
            writer.Write((int)m_overlapType);
            writer.Write(m_overlapTransferTicks);
            writer.Write(m_overlapPercentComplete);
            writer.Write(m_overlapSetups);

            writer.Write((int)m_autoFinishPred);
            writer.Write((short)m_transferStart);
            writer.Write((short)m_transferEnd);
        }

        public const int UNIQUE_ID = 246;

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        #region Construction
        public PredecessorOperationAttributes(string operationExternalId)
        {
            OperationExternalId = operationExternalId;
        }

        public PredecessorOperationAttributes(PredecessorOperationAttributes sourceAtts)
        {
            Update(sourceAtts);
            OperationExternalId = sourceAtts.OperationExternalId;
        }

        public PredecessorOperationAttributes(string aOperationExternalId,
                                              TimeSpan aTransferSpan,
                                              decimal aUseageQtyPerCycle,
                                              TimeSpan aMaxDelay,
                                              InternalOperationDefs.overlapTypes aOverlapType,
                                              TimeSpan aOverlapTransfer,
                                              decimal aOverlapPercentComplete,
                                              bool aOverlapSetups,
                                              InternalOperationDefs.autoFinishPredecessorOptions aAutoFinishPred,
                                              bool aAllowManualConnectorViolation,
                                              OperationDefs.EOperationTransferPoint aTransferStart,
                                              OperationDefs.EOperationTransferPoint aTransferEnd)
        {
            OperationExternalId = aOperationExternalId;

            TransferSpan = aTransferSpan;
            UsageQtyPerCycle = aUseageQtyPerCycle;
            MaxDelay = aMaxDelay;

            OverlapType = aOverlapType;
            OverlapTransferSpan = aOverlapTransfer;
            OverlapPercentComplete = aOverlapPercentComplete;
            OverlapSetups = aOverlapSetups;
            AutoFinishPredecessor = aAutoFinishPred;
            AllowManualConnectorViolation = aAllowManualConnectorViolation;
            TransferStart = aTransferStart;
            TransferEnd = aTransferEnd;
        }

        public PredecessorOperationAttributes(JobDataSet.AlternatePathNodeRow nodeRow, string aSuccessorOpExternalId)
        {
            OperationExternalId = aSuccessorOpExternalId;
            if (!nodeRow.IsTransferHrsNull())
            {
                TransferSpan = nodeRow.TransferHrs == TimeSpan.MaxValue.TotalHours ? TimeSpan.MaxValue : TimeSpan.FromHours(nodeRow.TransferHrs); //else overflows
            }

            if (!nodeRow.IsUsageQtyPerCycleNull())
            {
                m_usageQtyPerCycle = nodeRow.UsageQtyPerCycle;
            }

            if (!nodeRow.IsMaxDelayHrsNull())
            {
                m_maxDelay = PTDateTime.GetSafeTimeSpan(nodeRow.MaxDelayHrs).Ticks;
            }

            // Overlap fields.
            if (!nodeRow.IsOverlapTypeNull())
            {
                try
                {
                    m_overlapType = (InternalOperationDefs.overlapTypes)Enum.Parse(typeof(InternalOperationDefs.overlapTypes), nodeRow.OverlapType);
                }
                catch (Exception err)
                {
                    throw new APSCommon.PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            nodeRow.OverlapType, "AlternatePathNode", "OverlapType",
                            string.Join(", ", Enum.GetNames(typeof(InternalOperationDefs.overlapTypes)))
                        });
                }
            }

            if (!nodeRow.IsOverlapTransferHrsNull())
            {
                m_overlapTransferTicks = PTDateTime.GetSafeTimeSpan(nodeRow.OverlapTransferHrs).Ticks;
            }

            if (!nodeRow.IsOverlapPercentCompleteNull())
            {
                OverlapPercentComplete = nodeRow.OverlapPercentComplete / 100;
            }


            if (!nodeRow.IsAutoFinishPredecessorNull())
            {
                try
                {
                    m_autoFinishPred = (InternalOperationDefs.autoFinishPredecessorOptions)Enum.Parse(typeof(InternalOperationDefs.autoFinishPredecessorOptions), nodeRow.AutoFinishPredecessor);
                }
                catch (Exception err)
                {
                    throw new APSCommon.PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            nodeRow.AutoFinishPredecessor, "AlternatePathNode", "AutoFinishPredecessor",
                            string.Join(", ", Enum.GetNames(typeof(InternalOperationDefs.autoFinishPredecessorOptions)))
                        });
                }
            }

            if (!nodeRow.IsAllowManualConnectorViolationNull())
            {
                AllowManualConnectorViolation = nodeRow.AllowManualConnectorViolation;
            }

            if (!nodeRow.IsTransferDuringPredeccessorOnlineTimeNull())
            {
                TransferDuringPredeccessorOnlineTime = nodeRow.TransferDuringPredeccessorOnlineTime;
            }

            if (!nodeRow.IsTransferStartNull())
            {
                try
                {
                    m_transferStart = (OperationDefs.EOperationTransferPoint)Enum.Parse(typeof(OperationDefs.EOperationTransferPoint), nodeRow.TransferStart);
                    if (m_transferStart == OperationDefs.EOperationTransferPoint.EndOfStorage)
                    {
                        throw new APSCommon.PTValidationException("4458");
                    }
                }
                catch (Exception err)
                {
                    throw new APSCommon.PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            nodeRow.TransferStart, "AlternatePathNode", "TransferStart",
                            string.Join(", ", Enum.GetNames(typeof(OperationDefs.EOperationTransferPoint)))
                        });
                }
            }

            if (!nodeRow.IsTransferEndNull())
            {
                try
                {
                    m_transferEnd = (OperationDefs.EOperationTransferPoint)Enum.Parse(typeof(OperationDefs.EOperationTransferPoint), nodeRow.TransferEnd);
                    if (m_transferEnd == OperationDefs.EOperationTransferPoint.EndOfStorage)
                    {
                        throw new APSCommon.PTValidationException("4459");
                    }
                }
                catch (Exception err)
                {
                    throw new APSCommon.PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            nodeRow.TransferEnd, "AlternatePathNode", "TransferEnd",
                            string.Join(", ", Enum.GetNames(typeof(OperationDefs.EOperationTransferPoint)))
                        });
                }
            }
        }

        internal void Update(PredecessorOperationAttributes sourceAtts)
        {
            TransferSpan = sourceAtts.TransferSpan;
            m_usageQtyPerCycle = sourceAtts.UsageQtyPerCycle;
            MaxDelay = sourceAtts.MaxDelay;

            // Overlap fields.
            m_overlapType = sourceAtts.OverlapType;
            m_overlapTransferTicks = sourceAtts.OverlapTransferTicks;
            OverlapPercentComplete = sourceAtts.OverlapPercentComplete;
            m_overlapSetups = sourceAtts.OverlapSetups;
            m_autoFinishPred = sourceAtts.AutoFinishPredecessor;
            AllowManualConnectorViolation = sourceAtts.AllowManualConnectorViolation;
            TransferStart = sourceAtts.TransferStart;
            TransferEnd = sourceAtts.TransferEnd;
        }
        #endregion

        #region Shared Properties
        private BoolVector32 m_bools;

        private const short c_allowManualConnectorViolationIdx = 0;
        private const short c_transferDuringPredeccessorOnlineTimeIdx = 1;

        private string m_operationExternalId;

        /// <summary>
        /// The external id of the successor operation that these values correspond to.
        /// </summary>
        public string OperationExternalId
        {
            get => m_operationExternalId;
            set => m_operationExternalId = value;
        }

        /// <summary>
        /// The quantity of the Operation that is needed for each cycle of the Successor Operation.  This is used for calculating when a Successor Activity can start in an overlap situations.
        /// </summary>
        private decimal m_usageQtyPerCycle = 1;

        public decimal UsageQtyPerCycle
        {
            get => m_usageQtyPerCycle;
            set => m_usageQtyPerCycle = value;
        }

        private long m_transferSpan;

        /// <summary>
        /// Amount of time to move material from an Operation to its Successor.  The successor cannot start until this time passes.This can be specified from the ERP system or be calculated using a customizable
        /// script.
        /// </summary>
        public TimeSpan TransferSpan
        {
            get => new (m_transferSpan);
            set => m_transferSpan = value.Ticks;
        }

        public long TransferSpanTicks => m_transferSpan;

        /// <summary>
        /// Specifies that the Successor Operation must start setup within this amount of time from the end of the Predecessor Operation's scheduled finish.
        /// This is like a shelf-life. For example if you paint something you have to enamel it within the next 24 hours.
        /// Individual dispatch rules may or may not attempt to expedite such Operations.
        /// There is a Flag setup to warn if this maximum is violated.
        /// </summary>
        private long m_maxDelay = long.MaxValue;

        public TimeSpan MaxDelay
        {
            get => new (m_maxDelay);
            set => m_maxDelay = value.Ticks;
        }

        public long MaxDelayTicks => m_maxDelay;

        private InternalOperationDefs.overlapTypes m_overlapType = InternalOperationDefs.overlapTypes.NoOverlap;

        /// <summary>
        /// NotAllowed means the Predecessor must be finished. TransferQty means parts are transferred in groups of OverlapTransferQty to the successor operation. The successor will not be scheduled to start
        /// until enough material is available to allow it to run continuously. Overlap is currently supported for linear operations that have one activity.
        /// </summary>
        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public InternalOperationDefs.overlapTypes OverlapType
        {
            get => m_overlapType;
            set => m_overlapType = value;
        }

        private long m_overlapTransferTicks;

        /// <summary>
        /// Successor Operations can start once the TransferSpan has passed (in calendar time) after the Operation's Scheduled Run Start.
        /// </summary>
        [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
        public TimeSpan OverlapTransferSpan
        {
            get => new (m_overlapTransferTicks);

            set
            {
                if (value.Ticks < 0)
                {
                    throw new APSCommon.PTValidationException("2832", new object[] { value });
                }

                m_overlapTransferTicks = value.Ticks;
            }
        }

        private const decimal c_overlapPercentCompleteDefault = 1;
        private decimal m_overlapPercentComplete = c_overlapPercentCompleteDefault;

        /// <summary>
        /// If the overlap type is PercentComplete then the successor operation is allowed to start after this percentage of
        /// the predecessor has been completed.
        /// The valid range is 0 to 1.
        /// </summary>
        public decimal OverlapPercentComplete
        {
            get => m_overlapPercentComplete;
            set
            {
                if (value < 0 || value > 1)
                {
                    // The grid for this displays percentages as out of 100 instead of 0 to 1 
                    // so this error message parameter being multiplied by 100 is to accommodate this shift.
                    throw new APSCommon.PTValidationException("2976", new object[] { value * 100 });
                }

                m_overlapPercentComplete = value;
            }
        }

        /// <summary>
        /// Successor Operations can start once the TransferSpan has passed (in calendar time) after the Operation's Scheduled Run Start.
        /// </summary>
        internal long OverlapTransferTicks => m_overlapTransferTicks;

        private bool m_overlapSetups = true;

        /// <summary>
        /// Whether successorsOLD can start setting up before the setup is complete on this operation.
        /// </summary>
        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public bool OverlapSetups
        {
            get => m_overlapSetups;
            set => m_overlapSetups = value;
        }

        private InternalOperationDefs.autoFinishPredecessorOptions m_autoFinishPred = InternalOperationDefs.autoFinishPredecessorOptions.NoAutoFinish;

        /// <summary>
        /// Can be used to automatically finish a predecessor operation based on progress on a successor operation in order to avoid having to explicitly finish the predecessor.
        /// </summary>
        public InternalOperationDefs.autoFinishPredecessorOptions AutoFinishPredecessor
        {
            get => m_autoFinishPred;
            set => m_autoFinishPred = value;
        }

        /// <summary>
        /// If true then Operations can be moved to Resources that are invalid based on the predecessor Resources Connectors.
        /// This can be used to allow a tempoary violation which is Flagged thus giving the user more flexibility to manually reconsile as well as make exceptions.
        /// </summary>
        public bool AllowManualConnectorViolation
        {
            get => m_bools[c_allowManualConnectorViolationIdx];
            set => m_bools[c_allowManualConnectorViolationIdx] = value;
        }

        public bool TransferDuringPredeccessorOnlineTime
        {
            get => m_bools[c_transferDuringPredeccessorOnlineTimeIdx];
            set => m_bools[c_transferDuringPredeccessorOnlineTimeIdx] = value;
        }

        private OperationDefs.EOperationTransferPoint m_transferStart;

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
        #endregion
    }
}