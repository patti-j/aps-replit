namespace PT.Common.Collections.PriorityQueue;

public class PriorityQueueSimple<QueueType> where QueueType : IComparable<QueueType>
{
    private readonly List<QueueType> m_queue;
    private readonly object m_lock = new ();

    public PriorityQueueSimple()
    {
        m_queue = new List<QueueType>();
    }

    public void Clear()
    {
        m_queue.Clear();
    }

    public int Count => m_queue.Count;
    public bool Empty => m_queue.Count == 0;

    public void Add(QueueType a_item)
    {
        lock (m_lock)
        {
            m_queue.Add(a_item);
            Sort();
        }
    }

    public QueueType PeekNext()
    {
        return m_queue[0];
    }

    public QueueType RemoveNext()
    {
        QueueType next = m_queue[0];
        m_queue.RemoveAt(0);
        return next;
    }

    private void Sort()
    {
        m_queue.Sort((x, y) => x.CompareTo(y));
    }
}