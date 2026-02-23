using System.Collections;

using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// Each AlternatePath specifies one possible routing that can be followed for an MO.  Each MO has at least one AlternatePath.
/// </summary>
public partial class AlternatePath
{
    /// <summary>
    /// Stores a collection of nodes.
    /// </summary>
    public class NodeCollection
    {
        #region Declarations
        private readonly ArrayList pathNodes = new ();

        public class AlternatePathNodesCollectionException : PTException
        {
            public AlternatePathNodesCollectionException(string message)
                : base(message) { }
        }
        #endregion

        #region Construction
        public NodeCollection()
        {
            ArrayList pathNodes = new ();
        }

        public NodeCollection(NodeCollection nodeCollection)
        {
            pathNodes = (ArrayList)nodeCollection.pathNodes.Clone();
        }
        #endregion

        #region Properties and Methods
        public Type ElementType => typeof(Node);

        public Node Add(Node pathNode)
        {
            pathNodes.Add(pathNode);
            return pathNode;
        }

        public void Remove(int index)
        {
            pathNodes.RemoveAt(index);
        }

        public int Count => pathNodes.Count;

        internal void Clear()
        {
            pathNodes.Clear();
        }

        public Node this[int index] => (Node)pathNodes[index];
        #endregion

        public override string ToString()
        {
            return string.Format("Contains {0} nodes.", Count);
        }
    }
}