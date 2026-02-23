using System.Collections;

using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public partial class JobT
{
    public class AlternatePath : PTObjectBase, IPTSerializable
    {
        #region PT Serialization
        public AlternatePath(IReader a_reader)
            : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12505)
            {
                m_bools = new BoolVector32(a_reader);
                
                a_reader.Read(out preference);
                a_reader.Read(out int autoUseTemp);
                m_autoUse = (AlternatePathDefs.AutoUsePathEnum)autoUseTemp;
                a_reader.Read(out m_autoUseReleaseOffsetTimeSpan);
                a_reader.Read(out m_validityStartDate);
                a_reader.Read(out m_validityEndDate);
                
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    AlternateNode node = new (a_reader);
                    Add(node);
                }
            }
            else if (a_reader.VersionNumber >= 366)
            {
                a_reader.Read(out preference);
                a_reader.Read(out int autoUseTemp);
                m_autoUse = (AlternatePathDefs.AutoUsePathEnum)autoUseTemp;
                a_reader.Read(out m_autoUseReleaseOffsetTimeSpan);
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    AlternateNode node = new (a_reader);
                    Add(node);
                }
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);

            m_bools.Serialize(a_writer);

            a_writer.Write(preference);
            a_writer.Write((int)m_autoUse);
            a_writer.Write(m_autoUseReleaseOffsetTimeSpan);
            a_writer.Write(m_validityStartDate);
            a_writer.Write(m_validityEndDate);
            
            a_writer.Write(Count);
            for (int i = 0; i < Count; i++)
            {
                this[i].Serialize(a_writer);
            }
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        private readonly ArrayList nodes = new ();
        private readonly Hashtable alternateHash = new ();

        public AlternatePath(string externalId, string name, int preference)
            : base(externalId, name)
        {
            this.preference = preference;
        }

        public AlternatePath(JobDataSet.AlternatePathRow pathRow)
            : base(pathRow.ExternalId, "")
        {
            //Base values
            if (!pathRow.IsNameNull())
            {
                Name = pathRow.Name;
            }

            if (!pathRow.IsPreferenceNull())
            {
                Preference = pathRow.Preference;
            }

            if (!pathRow.IsAutoUseNull())
            {
                try
                {
                    AutoUse = (AlternatePathDefs.AutoUsePathEnum)Enum.Parse(typeof(AlternatePathDefs.AutoUsePathEnum), pathRow.AutoUse);
                }
                catch (Exception err)
                {
                    throw new APSCommon.PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            pathRow.AutoUse, "AlternatePath", "AutoUse",
                            string.Join(", ", Enum.GetNames(typeof(AlternatePathDefs.AutoUsePathEnum)))
                        });
                }
            }

            if (!pathRow.IsAutoUseReleaseOffsetDaysNull())
            {
                AutoUseReleaseOffsetTimeSpan = TimeSpan.FromDays(pathRow.AutoUseReleaseOffsetDays);
            }

            if (!pathRow.IsValidityStartDateNull())
            {
                ValidityStartDate = pathRow.ValidityStartDate.ToServerTime();
            }

            if (!pathRow.IsValidityEndDateNull())
            {
                ValidityEndDate = pathRow.ValidityEndDate.ToServerTime();
            }
        }

        private int preference = int.MaxValue;

        /// <summary>
        /// This values can be used in different way by custom algorithms and serves as a visual indicator to the planner.
        /// </summary>
        public int Preference
        {
            get => preference;
            set
            {
                preference = value;
                m_bools[c_preferenceIsSetIdx] = true;
            }
        }

        public bool PreferenceIsSet => m_bools[c_preferenceIsSetIdx];

        private AlternatePathDefs.AutoUsePathEnum m_autoUse = AlternatePathDefs.AutoUsePathEnum.IfCurrent;

        public AlternatePathDefs.AutoUsePathEnum AutoUse
        {
            get => m_autoUse;
            set
            {
                m_autoUse = value;
                m_bools[c_autoUseIsSetIdx] = true;
            }
        }

        public bool AutoUseIsSet => m_bools[c_autoUseIsSetIdx];

        private TimeSpan m_autoUseReleaseOffsetTimeSpan;

        /// <summary>
        /// A TimeSpan that defines when the path becomes eligible for automatic selection. The Alternate Path will not be used before the default path's release date + AutoPathSelectionReleaseOffset. For
        /// instance if a ManufacturingOrder has 2 alternate paths and the Default Path's release date is January 1 and the second AlternatePath is setup for AutoPathSection with AutoPathSelectionReleaseOffset=1
        /// day. Then the second path could potentially be used on or after January 2nd. The path that ends up being selected will depend on your optimization rules and resource availability. This value isn't
        /// used to determine the release date of the Default Path.
        /// </summary>
        public TimeSpan AutoUseReleaseOffsetTimeSpan
        {
            get => m_autoUseReleaseOffsetTimeSpan;
            set
            {
                m_autoUseReleaseOffsetTimeSpan = value;
                m_bools[c_autoUseReleaseOffsetTimeSpanIsSetIdx] = true;
            }
        }

        public bool AutoUseReleaseOffsetTimeSpanIsSet => m_bools[c_autoUseReleaseOffsetTimeSpanIsSetIdx];

        private DateTime m_validityStartDate = PTDateTime.InvalidDateTime;

        public DateTime ValidityStartDate
        {
            get => m_validityStartDate;
            set
            {
                m_validityStartDate = value;
                m_bools[c_validityStartDateIsSetIdx] = true;
            }
        }
        
        public bool ValidityStartDateIsSet => m_bools[c_validityStartDateIsSetIdx];
        
        private DateTime m_validityEndDate = PTDateTime.InvalidDateTime;
        public DateTime ValidityEndDate
        {
            get => m_validityEndDate;
            set
            {
                m_validityEndDate = value;
                m_bools[c_validityEndDateIsSetIdx] = true;
            }
        }
        
        public bool ValidityEndDateIsSet => m_bools[c_validityEndDateIsSetIdx];

        private BoolVector32 m_bools;

        private const short c_autoUseIsSetIdx = 0;
        private const short c_preferenceIsSetIdx = 1;
        private const short c_autoUseReleaseOffsetTimeSpanIsSetIdx = 2;
        private const short c_validityStartDateIsSetIdx = 3;
        private const short c_validityEndDateIsSetIdx = 4;
        

        /// <summary>
        /// Adds a Node to the Path.
        /// Note that Node Successors and Predecessor Operation Attributes must also be set if needed when using this Method.
        /// The other Add() method will do this automatically if creating linear paths.
        /// </summary>
        /// <param name="node"></param>
        public void Add(AlternateNode node)
        {
            if (alternateHash.Contains(node.OperationExternalId))
            {
                throw new ValidationException("2058", new object[] { ExternalId, node.OperationExternalId });
            }

            alternateHash.Add(node.OperationExternalId, node);
            nodes.Add(node);
        }

        public bool ContainsNode(string predecessorOperationExternalId)
        {
            return alternateHash.Contains(predecessorOperationExternalId);
        }

        /// <summary>
        /// If node doesn't exist an error will occur.  Use ContainsNode() to verify the node exists first.
        /// </summary>
        public AlternateNode GetNode(string predecessorOperationExternalId)
        {
            return (AlternateNode)alternateHash[predecessorOperationExternalId];
        }

        /// <summary>
        /// Adds a Node to the Path and sets it as a successor to any previous Node that was added to the Path.
        /// </summary>
        public void Add(AlternateNode nextNode, ManufacturingOrder mo)
        {
            Add(nextNode,
                mo,
                new TimeSpan(TimeSpan.MaxValue.Ticks),
                new TimeSpan(0),
                1,
                false,
                new TimeSpan(0),
                1,
                InternalOperationDefs.overlapTypes.NoOverlap,
                InternalOperationDefs.autoFinishPredecessorOptions.NoAutoFinish,
                false,
                OperationDefs.EOperationTransferPoint.NoTransfer,
                OperationDefs.EOperationTransferPoint.NoTransfer);
        }

        /// <summary>
        /// Removes any existing Nodes.
        /// </summary>
        public void ClearNodes()
        {
            alternateHash.Clear();
            nodes.Clear();
        }

        /// <summary>
        /// Adds a Node to the Path and sets it as a successor to any previous Node that was added to the Path.
        /// </summary>
        public void Add(AlternateNode nextNode,
                        ManufacturingOrder mo,
                        TimeSpan maxDelay,
                        TimeSpan transferSpan,
                        decimal usageQtyPerCycle,
                        bool overlapSetups,
                        TimeSpan overlapTransferSpan,
                        decimal overlapPercentComplete,
                        InternalOperationDefs.overlapTypes overlapType,
                        InternalOperationDefs.autoFinishPredecessorOptions aAutoFinishPred,
                        bool aAllowManualConnectorViolation,
                        OperationDefs.EOperationTransferPoint aTransferStart,
                        OperationDefs.EOperationTransferPoint aTransferEnd)
        {
            if (nodes.Count > 0) //have a previous Node so make it the Predecessor to this one.
            {
                AlternateNode predNode = (AlternateNode)nodes[nodes.Count - 1];
                Add(nextNode);

                PredecessorOperationAttributes predAtts = new (nextNode.OperationExternalId,
                    transferSpan,
                    usageQtyPerCycle,
                    maxDelay,
                    overlapType,
                    overlapTransferSpan,
                    overlapPercentComplete,
                    overlapSetups,
                    aAutoFinishPred,
                    aAllowManualConnectorViolation,
                    aTransferStart,
                    aTransferEnd);

                predNode.AddSuccessor(predAtts);
            }
            else //first node in the path so just add it.
            {
                Add(nextNode);
            }
        }

        /// <summary>
        /// Determine whether any node can be reached through recursive visitation of its successors.
        /// </summary>
        /// <returns>true if there's a circularity.</returns>
        public bool Cirularities()
        {
            for (int nodeCirularityCheckI = 0; nodeCirularityCheckI < nodes.Count; nodeCirularityCheckI++)
            {
                ResetSuccessorsCircularityTestingStartedFlag();
                AlternateNode currentNode = this[nodeCirularityCheckI];
                if (CircularitiesHelper(currentNode, currentNode))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reset the flag used to check whether testing of a nodes successors has already been started.
        /// This prevents a nodes successor from being testing multiple times when the node has multiple
        /// predecessors.
        /// This function must be called before each node is tested.
        /// </summary>
        private void ResetSuccessorsCircularityTestingStartedFlag()
        {
            // Reset all the successorsTestedForCircularities flags before testing a node.
            for (int resetNodeI = 0; resetNodeI < nodes.Count; ++resetNodeI)
            {
                this[resetNodeI].successorsCircularityTestingStarted = false;
            }
        }

        /// <summary>
        /// Determine whether a node can be reached through recursive visitation of its successor.
        /// </summary>
        /// <param name="testNode">This is the node who we are testing whether we can reach through recursive visitation of its successors.</param>
        /// <param name="digIntoSuccessorsNode">This is the node whose successors will be recursively dug into.</param>
        /// <returns></returns>
        private bool Circularities(AlternateNode testNode, AlternateNode digIntoSuccessorsNode)
        {
            if (testNode == digIntoSuccessorsNode)
            {
                return true;
            }

            if (digIntoSuccessorsNode.successorsCircularityTestingStarted)
            {
                return false;
            }

            return CircularitiesHelper(testNode, digIntoSuccessorsNode);
        }

        /// <summary>
        /// Recursively test a node.
        /// </summary>
        /// <param name="testNode">This is the node who we are testing whether we can reach through recursive visitation of its successors.</param>
        /// <param name="digIntoSuccessorsNode">This is the node whose successors will be recursively dug into.</param>
        /// <returns></returns>
        private bool CircularitiesHelper(AlternateNode testNode, AlternateNode digIntoSuccessorsNode)
        {
            digIntoSuccessorsNode.successorsCircularityTestingStarted = true;

            for (int successorI = 0; successorI < digIntoSuccessorsNode.Count; ++successorI)
            {
                string successorKey = digIntoSuccessorsNode[successorI].OperationExternalId;
                AlternateNode successorNode = (AlternateNode)alternateHash[successorKey];

                if (successorNode != null)
                {
                    if (Circularities(testNode, successorNode))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// This doesn't include nodes that have a predecessor and don't have a successor.
        /// For instance if the Path is 10->20 then there will be one node. If the path were 10 there would still be one node.
        /// If it had been 10->20->30 there would be 2 nodes.
        /// </summary>
        public int Count => nodes.Count;

        public AlternateNode this[int i] => (AlternateNode)nodes[i];
    }
}