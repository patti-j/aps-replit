using PT.Common.Debugging;

namespace PT.Common.Collections;

/// <summary>
/// Queue of limited size that wraps around when more elements than its max size are added.
/// </summary>
/// <typeparam name="Ty"></typeparam>
public class CircularQueueDictionary<Tkey, Tobject> where Tobject : IEquatable<Tobject> where Tkey : IComparable
{
    private readonly Dictionary<Tkey, Tobject> m_objectDictionary;
    private readonly CircularQueue<Tobject> m_queue;

    /// <summary>
    /// Create a circular queue specifying the maximum number of elements it can contain.
    /// </summary>
    /// <param name="a_maxSize">The maximum number of elements the queue can contain. This value must be greater than 0 or an exception will be thrown.</param>
    public CircularQueueDictionary(int a_maxSize)
    {
        m_objectDictionary = new Dictionary<Tkey, Tobject>();
        m_queue = new CircularQueue<Tobject>(a_maxSize);
        m_queue.Clear();
    }

    /// <summary>
    /// Creates a deep copy of the queue. But the queue doesn't create deep copies of the elements contained.
    /// </summary>
    /// <param name="a_queue">The queue to be copied.</param>
    //public CircularQueueDictionary(CircularQueueDictionary<Tkey, Tobject> a_queue) : base(a_queue)
    //{
    //    a_objectHash = new HashSet<Tkey>();
    //}

    /// <summary>
    /// Releases objects referenced by the queue or values and resets the count to 0.
    /// </summary>
    public void Clear()
    {
        m_objectDictionary.Clear();
        m_queue.Clear();
    }

    /// <summary>
    /// Add an element to the end of the queue.
    /// </summary>
    /// <param name="a_data">The element to add to the end of the queue.</param>
    public void Enqueue(Tkey a_key, Tobject a_data)
    {
        m_objectDictionary.Add(a_key, a_data);
        m_queue.Enqueue(a_data);
    }

    /// <summary>
    /// You must make sure the Count of this collection is greater than 0 before calling this function.
    /// Removes the object at the beginning of the queue and returns it.
    /// </summary>
    /// <returns>The object at the begging of the queue.</returns>
    public Tobject Dequeue()
    {
        Tobject dataObject = m_queue.Dequeue();
        //TODO: Improve this
        Tkey dataKey = default(Tkey);
        bool found = false;
        foreach (KeyValuePair<Tkey, Tobject> keyValuePair in m_objectDictionary)
        {
            if (keyValuePair.Value.Equals(dataObject))
            {
                dataKey = keyValuePair.Key;
                found = true;
                break;
            }
        }

        if (!found)
        {
            //TODO:
            throw new Exception("Object key not found");
        }

        m_objectDictionary.Remove(dataKey);
        return dataObject;
    }

    /// <summary>
    /// You must make sure the Count of this collection is greater than 0 before calling this function.
    /// Returns the object at the beginning of the queue without removing it.
    /// </summary>
    /// <returns>The object at the begging of the queue.</returns>
    public Tobject Peek()
    {
        return m_queue.Peek();
    }

    /// <summary>
    /// Return a copy of the queue.
    /// </summary>
    /// <returns>A array copy of the queue.</returns>
    //public Ty[] GetQueueCopy()
    //{
    //    Ty[] queueCopy = new Ty[m_count];
    //    CopyQueueIntoArray(queueCopy);
    //    return queueCopy;
    //}

    public int Count => m_objectDictionary.Count;

    public bool Contains(Tkey a_item)
    {
        return m_objectDictionary.ContainsKey(a_item);
    }

    public bool Peek(Tkey a_key, out Tobject a_foundObject)
    {
        if (m_objectDictionary.TryGetValue(a_key, out Tobject dataObject))
        {
            a_foundObject = dataObject;
            return true;
        }

        DebugException.ThrowInDebug("Queued object not found");
        a_foundObject = default;
        return false;
    }

    /// <summary>
    /// Return a copy of the queue.
    /// </summary>
    /// <returns>A array copy of the queue.</returns>
    public Tobject[] GetQueueCopy()
    {
        return m_queue.GetQueueCopy();
    }
}