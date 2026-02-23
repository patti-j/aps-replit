using System.Collections;

namespace PT.Scheduler;

internal class QtyProfileTimePoint<QtyNodeT> : IEnumerable<QtyNodeT> where QtyNodeT : QtyNode
{
    private readonly IQtyProfile m_profile;
    private readonly LinkedList<QtyNodeT> m_qtyNodes = new ();

    public QtyProfileTimePoint() { }

    public QtyProfileTimePoint(QtyNodeT a_qtyNode, QtyProfile<QtyNodeT> a_newProfile)
    {
        m_profile = a_newProfile;
        a_qtyNode.SetProfile(a_newProfile);
        m_qtyNodes.AddFirst(a_qtyNode);
    }

    /// <summary>
    /// Create a new QtyTimePoint with the nodes from the existing node.
    /// This is a shallow copy to allow for merging profiles
    /// </summary>
    /// <param name="a_existingTimePoint"></param>
    public QtyProfileTimePoint(QtyProfileTimePoint<QtyNodeT> a_existingTimePoint, QtyProfile<QtyNodeT> a_newProfile)
    {
        m_profile = a_newProfile;
        foreach (QtyNodeT node in a_existingTimePoint.m_qtyNodes)
        {
            node.SetProfile(a_newProfile);
            m_qtyNodes.AddLast(node);
        }
    }

    public long Date => m_qtyNodes.First.Value.Date;
    public bool HasNodes => m_qtyNodes.Count > 0;
    public QtyNodeT LastQtyNode => m_qtyNodes.Last.Value;

    internal LinkedList<QtyNodeT> QtyNodes => m_qtyNodes;
    public bool Disconnected;

    public void AddToEnd(QtyNodeT a_node)
    {
        a_node.SetProfile(m_profile);
        m_qtyNodes.AddLast(a_node);
    }

    public void UpdateQty(decimal a_qty)
    {
        m_qtyNodes.Last.Value.UpdateQty(a_qty);
    }

    public void Clear()
    {
        m_qtyNodes.Clear();
    }

    public bool Remove(QtyNodeT a_node)
    {
        a_node.SetProfile(null);
        return m_qtyNodes.Remove(a_node);
    }

    public IEnumerator<QtyNodeT> GetEnumerator()
    {
        return m_qtyNodes.GetEnumerator();
    }

    public void Merge(QtyProfileTimePoint<QtyNodeT> a_newTimePoint)
    {
        using IEnumerator<QtyNodeT> enumerator = a_newTimePoint.GetEnumerator();
        while (enumerator.MoveNext())
        {
            QtyNodeT node = enumerator.Current;
            node.SetProfile(m_profile);
            m_qtyNodes.AddLast(node); //TODO: Possibly insert by Qty
        }
    }

    //Get the LinkedList node associated with the QtyNode. This can be used for enumeration
    internal LinkedListNode<QtyNodeT> GetQtyNodeLLNode(QtyNodeT a_targetNode)
    {
        return m_qtyNodes.Find(a_targetNode);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
