namespace PT.Common.Collections;

/// <summary>
/// Queue of limited size that wraps around when more elements than its max size are added.
/// </summary>
/// <typeparam name="Ty"></typeparam>
public class CircularQueue<Ty>
{
    /// <summary>
    /// The maximum number of elements the queue can contain.
    /// </summary>
    private readonly int m_maxSize;

    /// <summary>
    /// The maximum index in the queue's data array.
    /// </summary>
    private readonly int m_maxIdx;

    /// <summary>
    /// The elements of the queue.
    /// </summary>
    private Ty[] m_data;

    /// <summary>
    /// The index of the last element added to the queue.
    /// </summary>
    private int m_curIdx;

    /// <summary>
    /// The number of elements stored in the queue.
    /// </summary>
    private int m_count;

    /// <summary>
    /// A fast lookup collection that is only maintained if the Contains method is called
    /// </summary>
    private HashSet<Ty> m_keyHash;

    /// <summary>
    /// Create a circular queue specifying the maximum number of elements it can contain.
    /// </summary>
    /// <param name="a_maxSize">The maximum number of elements the queue can contain. This value must be greater than 0 or an exception will be thrown.</param>
    public CircularQueue(int a_maxSize)
    {
        if (a_maxSize < 1)
        {
            throw new Exception(
                string.Format("The size of the CircularQueue must be greater than or equal to 1. The size passed to the constructor was {0}.",
                    a_maxSize));
        }

        m_maxSize = a_maxSize;
        m_maxIdx = m_maxSize - 1;

        Clear();
    }

    /// <summary>
    /// Creates a deep copy of the queue. But the queue doesn't create deep copies of the elements contained.
    /// </summary>
    /// <param name="a_queue">The queue to be copied.</param>
    public CircularQueue(CircularQueue<Ty> a_queue)
    {
        m_maxSize = a_queue.m_maxSize;
        m_maxIdx = a_queue.m_maxIdx;
        m_curIdx = a_queue.m_curIdx;
        m_count = a_queue.m_count;
        m_data = new Ty[m_maxSize];

        for (int i = 0; i < m_maxSize; ++i)
        {
            m_data[i] = a_queue.m_data[i];
        }
    }

    /// <summary>
    /// Releases objects referenced by the queue or values and resets the count to 0.
    /// </summary>
    public void Clear()
    {
        m_data = new Ty[m_maxSize];
        m_curIdx = -1;
        m_count = 0;
    }

    /// <summary>
    /// The maximum number of elemnts the queue can hold.
    /// </summary>
    public int MaxSize => m_maxSize;

    /// <summary>
    /// The number of elements in the queue.
    /// </summary>
    public int Count => m_count;

    /// <summary>
    /// Add an element to the end of the queue.
    /// </summary>
    /// <param name="a_data">The element to add to the end of the queue.</param>
    public void Enqueue(Ty a_data)
    {
        ++m_curIdx;
        if (m_curIdx > m_maxIdx)
        {
            m_curIdx = 0;
        }

        m_data[m_curIdx] = a_data;
        if (m_count != m_maxSize)
        {
            ++m_count;
        }

        m_keyHash?.Add(a_data);
    }

    /// <summary>
    /// You must make sure the Count of this collection is greater than 0 before calling this function.
    /// Removes the object at the beginning of the queue and returns it.
    /// </summary>
    /// <returns>The object at the begging of the queue.</returns>
    public Ty Dequeue()
    {
        long idx = GetIdxOfFirstInQueue();
        --m_count;
        m_keyHash?.Remove(m_data[idx]);
        return m_data[idx];
    }

    /// <summary>
    /// You must make sure the Count of this collection is greater than 0 before calling this function.
    /// Returns the object at the beginning of the queue without removing it.
    /// </summary>
    /// <returns>The object at the begging of the queue.</returns>
    public Ty Peek()
    {
        long idx = GetIdxOfFirstInQueue();
        return m_data[idx];
    }

    /// <summary>
    /// You must make sure the Count of this collection is greater than 0 before calling this function.
    /// An exception will be thrown if there are no elements.
    /// </summary>
    /// <returns>An index into the queue's array.</returns>
    private long GetIdxOfFirstInQueue()
    {
        if (m_count == 0)
        {
            throw new Exception("The CircularQueue is empty. You must check the length before attempting a dequeue.");
        }

        long idx = m_curIdx - (m_count - 1);

        if (idx < 0)
        {
            idx += m_maxSize;
        }

        return idx;
    }

    /// <summary>
    /// Return a copy of the queue.
    /// </summary>
    /// <returns>A array copy of the queue.</returns>
    public Ty[] GetQueueCopy()
    {
        Ty[] queueCopy = new Ty[m_count];
        CopyQueueIntoArray(queueCopy);
        return queueCopy;
    }

    /// <summary>
    /// Copy the queue's elements into an array. The size of the array must be at least as large as the queue. The array isn't cleared or initialized. Only the first Count indexes are set, data beyond that
    /// isn't overwritten.
    /// </summary>
    /// <param name="a_queueCopy">The array to copy the queue into, the size must be >= Count.</param>
    public long CopyQueueIntoArray(Ty[] a_queueCopy)
    {
        if (m_count > 0)
        {
            long queueCopyIdx = 0;

            long firstQueueIdx = GetIdxOfFirstInQueue();
            if (m_curIdx >= firstQueueIdx)
            {
                // Copy from the first to last element in queue.
                for (long dataIdx = firstQueueIdx; dataIdx <= m_curIdx; ++dataIdx)
                {
                    a_queueCopy[queueCopyIdx++] = m_data[dataIdx];
                }
            }
            else
            {
                // The first part of the queue is at the end of the data array. The second part is at the start of the data array.

                // Copy the first part of the queue from the last part of the data array.
                for (long dataIdx = firstQueueIdx; dataIdx <= m_maxIdx; ++dataIdx)
                {
                    a_queueCopy[queueCopyIdx++] = m_data[dataIdx];
                }

                // Copy the last part of the queue from the first part of the data array.
                for (long dataIdx = 0; dataIdx <= m_curIdx; ++dataIdx)
                {
                    a_queueCopy[queueCopyIdx++] = m_data[dataIdx];
                }
            }
        }

        return m_count;
    }

    private void InitKeyHash()
    {
        m_keyHash = new HashSet<Ty>(m_maxSize);
        foreach (Ty y in m_data)
        {
            m_keyHash.Add(y);
        }
    }

    public bool Contains(Ty a_item)
    {
        if (m_keyHash == null)
        {
            InitKeyHash();
        }

        return m_keyHash.Contains(a_item);
    }
}