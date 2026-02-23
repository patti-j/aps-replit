using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// Each AlternatePath specifies one possible routing that can be followed for an MO.  Each MO has at least one AlternatePath.
/// </summary>
public partial class AlternatePath
{
    /// <summary>
    /// Used to create a network of operations that define the steps of an alternate path.
    /// </summary>
    public partial class Node
    {
        #region IPTSerializable Members
        internal Node(IReader reader, BaseOperationManager operations)
        {
            m_id = new BaseId(reader);
            BaseId operationId = new (reader);
            m_operation = (ResourceOperation)operations[operationId];
            m_successors = new AssociationCollection(reader, this);
        }

        internal void DeserializationFixups(Node[] opBaseIds)
        {
            m_successors.DeserializationFixups(opBaseIds);
        }

        internal void DeserializationFixups2()
        {
            m_successors.DeserializationFixups2();
        }

        public void Serialize(IWriter writer)
        {
            m_id.Serialize(writer);
            Operation.Id.Serialize(writer);
            m_successors.Serialize(writer);
        }
        #endregion

        #region Construction
        /// <summary>
        /// Create a node associated with the specified operation.
        /// </summary>
        /// <param name="operation">The operation this node is to be associated with.</param>
        /// <param name="id"></param>
        public Node(ResourceOperation operation, BaseId id, AlternatePath a_path)
        {
            Operation = operation;
            m_id = id;
            m_path = a_path;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The unique id of this node within a path.
        /// </summary>
        private readonly BaseId m_id;

        /// <summary>
        /// The parent Path.
        /// </summary>
        private readonly AlternatePath m_path;

        /// <summary>
        /// The unique id of this node within a path.
        /// </summary>
        internal BaseId Id => m_id;

        /// <summary>
        /// The operation this node is associated with.
        /// </summary>
        private ResourceOperation m_operation;

        /// <summary>
        /// The Predecessor Operation.
        /// </summary>
        public ResourceOperation Operation
        {
            get => m_operation;
            internal set => m_operation = value;
        }

        /// <summary>
        /// A set of associations that define the predecessor nodes of this node.
        /// </summary>
        private readonly AssociationCollection m_predecessors = new ();

        /// <summary>
        /// A set of associations that define the predecessor nodes of this node.
        /// </summary>
        public AssociationCollection Predecessors => m_predecessors;

        /// <summary>
        /// Returns True if the specified Node is a Predecessor (immediate or otherwise) of this Node.
        /// </summary>
        internal bool IsPredecessor(Node a_potentialPred)
        {
            for (int i = 0; i < Predecessors.Count; i++)
            {
                Association predAssociation = Predecessors[i];
                if (predAssociation.Predecessor.m_id == a_potentialPred.m_id)
                {
                    return true;
                }

                if (predAssociation.Predecessor.IsPredecessor(a_potentialPred))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Add a predecessor association.
        /// </summary>
        /// <param name="a_predecessor">A predecessor of this node.</param>
        public void AddPredecessor(Association a_predecessor)
        {
            m_predecessors.Add(a_predecessor);
        }

        /// <summary>
        /// A set of associations that define the successor nodes of this node.
        /// </summary>
        private readonly AssociationCollection m_successors = new ();

        public AssociationCollection Successors => m_successors;
        
        /// <summary>
        /// Set when another path in the MO schedules, and this has been removed.
        /// Use this to clear out other operations that may attempt to schedule or be dispatched.
        /// </summary>
        internal bool HasAnotherPathScheduled { get; set; }

        /// <summary>
        /// Add a successor association.
        /// </summary>
        /// <param name="a_successor"></param>
        public void AddSuccessor(Association a_successor)
        {
            m_successors.Add(a_successor);
        }
        #endregion

        #region Node structure comparison
        /// <summary>
        /// Determine whether two nodes have the same set of Alternate nodes as successors.
        /// </summary>
        /// <param name="a_node2">The node you are comparing successors to.</param>
        /// <returns>Whether the two node have the same set of succesors.</returns>
        internal bool IdenticleSuccessorSet(Node a_node2)
        {
            //Check all of this node's successors
            //Ignore removed successor constraints. This does not need to unschedule the MO.
            //for (int nodeI = 0; nodeI < this.Successors.Count; ++nodeI)
            //{
            //    Association successorAssociationI = m_successors[nodeI];
            //    Node successorI = successorAssociationI.Successor;
            //    string nodeIExternalId = successorI.Operation.ExternalId;

            //    if (!a_node2.Successors.Contains(nodeIExternalId))
            //    {
            //        return false;
            //    }
            //}

            //Verify nodes have the same number of successors
            if (Successors.Count != a_node2.Successors.Count)
            {
                return false;
            }

            //Now check all of the new node's successors.  Successors may have been added.
            //Check all of this node's successors
            for (int nodeI = 0; nodeI < a_node2.Successors.Count; ++nodeI)
            {
                Association successorAssociationI = a_node2.Successors[nodeI];
                Node successorI = successorAssociationI.Successor;
                string nodeIExternalId = successorI.Operation.ExternalId;

                if (!Successors.Contains(nodeIExternalId))
                {
                    return false;
                }
            }

            // In case an operation references the same successor more than once.
            if (Successors.Count < a_node2.Successors.Count)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Update
        /// <summary>
        /// Update the values in a node.
        /// </summary>
        /// <param name="a_updatedNode"></param>
        /// <returns>Whether there were any significant changes.</returns>
        internal bool Update(Node a_updatedNode)
        {
            bool updates = false;

            // Update the successsor associations.
            // The old and new successor association are matched up by their successor operations.
            for (int successorAssociationI = 0; successorAssociationI < Successors.Count; ++successorAssociationI)
            {
                Association oldSuccessorAssociation = m_successors[successorAssociationI];

                Association updatedSuccessorAssociation = a_updatedNode.Successors.FindBySuccessorExternalId(oldSuccessorAssociation.Successor.Operation.ExternalId);

                if (updatedSuccessorAssociation != null)
                {
                    if (oldSuccessorAssociation.Update(updatedSuccessorAssociation))
                    {
                        updates = true;
                    }
                }
            }

            return updates;
        }
        #endregion

        #region Debugging
        public override string ToString()
        {
            return string.Format("Op={0}; with {1} predecessors and {2} successors.", Operation.Name, Predecessors.Count, Successors.Count);
        }
        #endregion
    }
}