using System.Collections;

namespace PT.Common.Collections;

public class PriorityQueue<QueueType> : IEnumerable<QueueType>
{
    private QueueType[] m_elements;
    private readonly IPTCollectionsComparer<QueueType> m_comparer;

    private int m_maxElementsIndex;
    private int m_nextElementsIndex;

    // This becomes 512 when the constructor is run. 
    // This is due to index 0 not being used.
    private const int DEFAULT_SIZE = 511;

    public PriorityQueue(IPTCollectionsComparer<QueueType> a_comparer)
        : this(a_comparer, DEFAULT_SIZE) { }

    /// <summary>
    /// You may specify the starting size of the queue's array.
    /// </summary>
    /// <param name="aSize">
    /// Index 0 isn't used, so the size you specify is incremented by 1.
    /// For instance if you specify 511 then the array will be 512 in length; big enough to contain 511 elements but not using the first element.
    /// If you don't use this constructor the default size of the queue is 511.
    /// </param>
    public PriorityQueue(IPTCollectionsComparer<QueueType> a_comparer, int aSize)
    {
        if (aSize < 1)
        {
            throw new Exception("The size of the Priority Queue must be greater than 1.");
        }

        m_comparer = a_comparer;
        ++aSize;
        m_elements = new QueueType[aSize];
        m_nextElementsIndex = 1;
        m_maxElementsIndex = aSize - 1;
    }

    public void Clear()
    {
        m_nextElementsIndex = 1;
    }

    private void Dump()
    {
        m_elements = new QueueType[m_elements.Length];
    }

    /// <summary>
    /// O(n).
    /// Call this function when the queue runs out of space.
    /// It doubles the size of the queue.
    /// </summary>
    private void Resize()
    {
        long newSize = m_elements.Length * 2;

        if (newSize > int.MaxValue)
        {
            if (m_elements.Length == int.MaxValue)
            {
                throw new Exception("The size of the priority queue can't be increased.");
            }

            newSize = int.MaxValue;
        }

        QueueType[] tempElements = new QueueType[newSize];
        for (int i = 0; i < m_elements.Length; ++i)
        {
            tempElements[i] = m_elements[i];
        }

        m_elements = tempElements;
        m_maxElementsIndex = (int)(newSize - 1);
    }

    /// <summary>
    /// O(1). The number of elements in the queue.
    /// If you are trying to determine whether the queue contains elements use the ContainsElements property, because this property has to perform a subtraction.
    /// </summary>
    public int Count => m_nextElementsIndex - 1;

    /// <summary>
    /// O(1). Whether the queue contains elements.
    /// If you are trying to determine whether the priority queue contains elements use this function over the Count function since Count performs a subtraction.
    /// </summary>
    public bool ContainsElements => m_nextElementsIndex > 1;

    /// <summary>
    /// O(n). Typically O(1), unless the queue needs to be resized.
    /// You may use this function to help build up the initial queue.
    /// After creating your queue stuff all your elements into it using
    /// this function and once complete call function InitialInsertionCompletion().
    /// This technique has significant performance advantages to using
    /// Insert() n times to initially populate your queue.
    /// </summary>
    /// <param name="aElement">The element to add to the end of the queue.</param>
    public void InitialInsert(QueueType aElement)
    {
        if (m_nextElementsIndex > m_maxElementsIndex)
        {
            Resize();
        }

        m_elements[m_nextElementsIndex] = aElement;
        ++m_nextElementsIndex;
    }

    /// <summary>
    /// O(n). Use this in combination with InitialInsert() to initially build and
    /// organize your queue. This technique has significant performance advanteages
    /// to building up your queue using n calls to Insert().
    /// </summary>
    public void InitialInsertionComplete()
    {
        // Percolate down.
        for (int i = Count >> 1; i > 0; --i)
        {
            bool percolatedDown;
            int parentIndex = i;
            int childIndex = parentIndex << 1;

            do
            {
                int minChildIndex;
                int nextChildIndex = childIndex + 1;

                if (nextChildIndex < m_nextElementsIndex)
                {
                    // Has 2 children.
                    if (m_comparer.LessThanOrEqual(m_elements[childIndex], m_elements[nextChildIndex]))
                    {
                        minChildIndex = childIndex;
                    }
                    else
                    {
                        minChildIndex = nextChildIndex;
                    }
                }
                else if (childIndex < m_nextElementsIndex)
                {
                    // Has 1 child.
                    minChildIndex = childIndex;
                }
                else
                {
                    minChildIndex = -1;
                }

                if (minChildIndex == -1)
                {
                    percolatedDown = false;
                }
                else
                {
                    percolatedDown = m_comparer.LessThan(m_elements[minChildIndex], m_elements[parentIndex]);
                }

                if (percolatedDown)
                {
                    QueueType tempMinChild = m_elements[minChildIndex];
                    m_elements[minChildIndex] = m_elements[parentIndex];
                    m_elements[parentIndex] = tempMinChild;
                    parentIndex = minChildIndex;
                    childIndex = parentIndex << 1;
                }
            } while (percolatedDown);
        }
    }

    /// <summary>
    /// log(n). Insert an element into the queue.
    /// </summary>
    /// <param name="aElement">This value is percolated to the right spot in the queue.</param>
    public void Insert(QueueType aElement)
    {
        if (m_nextElementsIndex > m_maxElementsIndex)
        {
            Resize();
        }

        int currentIndex = m_nextElementsIndex;
        int parentIndex = currentIndex >> 1;

        while (parentIndex != 0 && m_comparer.GreaterThan(m_elements[parentIndex], aElement))
        {
            m_elements[currentIndex] = m_elements[parentIndex];
            currentIndex = parentIndex;
            parentIndex >>= 1;
        }

        m_elements[currentIndex] = aElement;
        ++m_nextElementsIndex;
    }

    /// <summary>
    /// Returns the minimum element in the queue if one exists. If the queue is empty an exception is thrown.
    /// </summary>
    /// <returns></returns>
    public QueueType DeleteMin()
    {
        if (m_nextElementsIndex == 1)
        {
            throw new Exception("The priority queue is empty.");
        }

        // Remove the minimum element. This creates a hole in the heap at one of the leaves.
        QueueType retVal = m_elements[1];

        int parentIndex = 1;
        int childIndex = 2;

        while (childIndex < m_nextElementsIndex)
        {
            int minChildIndex;
            int nextChildIndex = childIndex + 1;

            if (nextChildIndex < m_nextElementsIndex)
            {
                //There are two children.
                if (m_comparer.LessThan(m_elements[childIndex], m_elements[nextChildIndex]))
                {
                    minChildIndex = childIndex;
                }
                else
                {
                    minChildIndex = nextChildIndex;
                }
            }
            else
            {
                //There's one child.
                minChildIndex = childIndex;
            }

            m_elements[parentIndex] = m_elements[minChildIndex];
            parentIndex = minChildIndex;
            childIndex = parentIndex << 1;
        }

        // Percolate the last element up at the hole that was created.
        childIndex = parentIndex;
        parentIndex = childIndex >> 1;
        QueueType percolatingValue = m_elements[m_nextElementsIndex - 1];

        while (parentIndex != 0 && m_comparer.GreaterThan(m_elements[parentIndex], percolatingValue))
        {
            m_elements[childIndex] = m_elements[parentIndex];
            childIndex = parentIndex;
            parentIndex >>= 1;
        }

        m_elements[childIndex] = percolatingValue;

        // Adjust the tracking of the next elements index.
        --m_nextElementsIndex;

        return retVal;
    }

    /// <summary>
    /// Check the value of the min element without deleting it.
    /// </summary>
    /// <returns>The minimum element.</returns>
    public QueueType PeekMin()
    {
        if (m_nextElementsIndex == 1)
        {
            throw new Exception("The priority queue is empty.");
        }

        return m_elements[1];
    }

    /// <summary>
    /// For testing purposes. Determine whether the Priority Queue parent child
    /// < relationship is valid for every node.
    ///     An exception is thrown if there is a problem.
    /// </summary>
    public void Validate()
    {
        for (int i = 1; i < m_nextElementsIndex; ++i)
        {
            int parent = i;
            int child1 = i << 1;
            int child2 = child1 + 1;

            if (child2 < m_nextElementsIndex)
            {
                if (m_comparer.GreaterThan(m_elements[parent], m_elements[child2]))
                {
                    throw new Exception("Priority Queue validation error.");
                }

                if (m_comparer.GreaterThan(m_elements[parent], m_elements[child1]))
                {
                    throw new Exception("Priority Queue validation error.");
                }
            }
            else if (child1 < m_nextElementsIndex)
            {
                if (m_comparer.GreaterThan(m_elements[parent], m_elements[child1]))
                {
                    throw new Exception("Priority Queue validation error.");
                }
            }
        }
    }

    /// <summary>
    /// Do not use. For debugging. Get a copy of the elements in the queue.
    /// The compiler must have a bug in it. The code won't build unless the function is marked as public. When marked as Internal an error says the function  definition couldn't be found.
    /// </summary>
    /// <returns></returns>
    public string[] GetElementsArray()
    {
        string[] qt = new string[m_nextElementsIndex];

        for (int i = 0; i < m_nextElementsIndex; ++i)
        {
            if (m_elements[i] != null)
            {
                qt[i] = m_elements[i].ToString();
            }
        }

        return qt;
    }

    public override string ToString()
    {
        return base.ToString() + " Contains " + Count + " elements";
    }

    /// <summary>
    /// Used to enumerator  a priotity queue.
    /// </summary>
    private class PriorityQueueEnumerator : IEnumerator<QueueType>
    {
        internal PriorityQueueEnumerator(QueueType[] a_elements, long a_maxIdx)
        {
            m_elements = a_elements;
            m_maxIdx = a_maxIdx;
        }

        private readonly QueueType[] m_elements;
        private readonly long m_maxIdx;
        private int m_curIdx;
        public QueueType Current => m_elements[m_curIdx];

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            if (m_curIdx >= m_maxIdx)
            {
                return false;
            }

            ++m_curIdx;
            return true;
        }

        public void Reset()
        {
            m_curIdx = 0;
            throw new NotImplementedException();
        }
    }

    public IEnumerator<QueueType> GetEnumerator()
    {
        return new PriorityQueueEnumerator(m_elements, m_nextElementsIndex);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}