using System.Collections;

namespace PT.Scheduler;

public partial class AlternatePath
{
    /// <summary>
    /// Stores a collection of associations.
    /// </summary>
    public partial class AssociationCollection : IEnumerable<Association>
    {
        #region IPTSerializable Members
        internal AssociationCollection(IReader reader, Node predecessorNode)
        {
            int count;
            reader.Read(out count);

            for (int i = 0; i < count; ++i)
            {
                Association association = new (reader, predecessorNode);
                AddDuringConstruction(association);
            }
        }

        internal void DeserializationFixups(Node[] opBaseIds)
        {
            for (int i = 0; i < Count; ++i)
            {
                this[i].DeserializationFixups(opBaseIds);
            }
        }

        [Common.Attributes.DebugLogging(Common.Attributes.EDebugLoggingType.None)]
        private bool deserializationFixup2sComplete;

        internal void DeserializationFixups2()
        {
            if (!deserializationFixup2sComplete)
            {
                deserializationFixup2sComplete = true;

                for (int i = 0; i < Count; ++i)
                {
                    Association association = this[i];
                    string associationExternalId = association.Successor.Operation.ExternalId;
                    m_successorAssociationExternalIds.Add(associationExternalId, null);
                }
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(m_associations.Count);

            for (int i = 0; i < m_associations.Count; ++i)
            {
                Association association = m_associations[i];
                association.Serialize(writer);
            }
        }
        #endregion

        #region Declarations
        private readonly List<Association> m_associations = new ();
        private readonly Hashtable m_successorAssociationExternalIds = new ();
        #endregion

        #region Construction
        public AssociationCollection() { }
        #endregion

        #region Properties and Methods
        public Type ElementType => typeof(Association);

        /// <summary>
        /// Add another association to this collection.
        /// </summary>
        /// <param name="a_association">The association that you want to add.</param>
        internal void Add(Association a_association)
        {
            m_associations.Add(a_association);

            string externalId = a_association.Successor.Operation.ExternalId;
            if (!m_successorAssociationExternalIds.ContainsKey(externalId))
            {
                m_successorAssociationExternalIds.Add(externalId, null);
            }
        }

        /// <summary>
        /// Use this version of add when reconstructing this object.
        /// </summary>
        /// <param name="a_association"></param>
        private void AddDuringConstruction(Association a_association)
        {
            m_associations.Add(a_association);
        }

        /// <summary>
        /// Remove an association from this collection.
        /// </summary>
        /// <param name="a_index">The index of the collection that you want to remove.</param>
        internal void Remove(int a_index)
        {
            Association association = m_associations[a_index];
            m_associations.RemoveAt(a_index);
            string successorExternalId = association.Successor.Operation.ExternalId;
            m_successorAssociationExternalIds.Remove(successorExternalId);
        }

        /// <summary>
        /// The number of associations in this collection.
        /// </summary>
        public int Count => m_associations.Count;

        /// <summary>
        /// Access a specific association in this collection by index.
        /// </summary>
        public Association this[int i] => m_associations[i];

        /// <summary>
        /// Whether this association contains an association with the specified external id.
        /// </summary>
        /// <param name="a_externalId">The external id that you are looking for.</param>
        /// <returns></returns>
        internal bool Contains(string a_externalId)
        {
            return m_successorAssociationExternalIds.ContainsKey(a_externalId);
        }

        /// <summary>
        /// Find an association by the successor operation's external id.
        /// </summary>
        /// <param name="a_externalId">The external id of the successor operation whose association you are seeking.</param>
        /// <returns>The association found or null.</returns>
        internal Association FindBySuccessorExternalId(string a_externalId)
        {
            for (int associationI = 0; associationI < Count; ++associationI)
            {
                Association association = this[associationI];
                if (association.Successor.Operation.ExternalId == a_externalId)
                {
                    return association;
                }
            }

            return null;
        }
        #endregion

        #region Debugging
        public override string ToString()
        {
            return string.Format("Contains {0} associations.", Count);
        }

        public IEnumerator<Association> GetEnumerator()
        {
            return m_associations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}