using PT.Common.Exceptions;

namespace PT.ERPTransmissions;

public partial class JobT
{
    public class AlternateNode : IPTSerializable
    {
        #region PT Serialization
        public AlternateNode(IReader reader)
        {
            if (reader.VersionNumber >= 209)
            {
                reader.Read(out operationExternalId);

                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    PredecessorOperationAttributes suc = new (reader);
                    successors.Add(suc);
                }
            }

            #region 1
            else if (reader.VersionNumber >= 1)
            {
                reader.Read(out operationExternalId);

                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    string sucExternalId;
                    reader.Read(out sucExternalId);
                    PredecessorOperationAttributes suc = new (sucExternalId);
                    successors.Add(suc);
                }
            }
            #endregion
        }

        public virtual void Serialize(IWriter writer)
        {
            writer.Write(operationExternalId);

            writer.Write(Count);
            for (int i = 0; i < Count; i++)
            {
                this[i].Serialize(writer);
            }
        }

        public const int UNIQUE_ID = 242;

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        private readonly List<PredecessorOperationAttributes> successors = new ();

        public AlternateNode(string operationExternalId)
        {
            this.operationExternalId = operationExternalId;
        }

        // Used during construction to verify that a successor operation isn't added more than once.
        private HashSet<string> successorExternalIds;

        public void AddSuccessor(PredecessorOperationAttributes poa)
        {
            if (successorExternalIds == null)
            {
                successorExternalIds = new HashSet<string>();
            }

            if (successorExternalIds.Contains(poa.OperationExternalId))
            {
                throw new PTException("Data error! Operation '{0}' is being set to reference successor operation {1} multiple times.", new object[] { operationExternalId, poa.OperationExternalId });
            }

            successors.Add(poa);

            successorExternalIds.Add(poa.OperationExternalId);
        }

        /// <summary>
        /// The number of successors of this node.
        /// </summary>
        public int Count => successors.Count;

        private string operationExternalId;

        public string OperationExternalId => operationExternalId;

        protected void SetOperationExternalId(string operationExternalId)
        {
            this.operationExternalId = operationExternalId;
        }

        public PredecessorOperationAttributes this[int i] => successors[i];

        /// <summary>
        /// Whether circularity testing for this nodes successor nodes has already started.
        /// </summary>
        internal bool successorsCircularityTestingStarted;
    }
}