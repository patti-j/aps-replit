using System.Collections;

using ListDataType = PT.SchedulerDefinitions.ActivityKey;
using ListType = PT.Scheduler.ActivityKeyList;

namespace PT.Scheduler;

/// <summary>
/// ListTemplate
/// </summary>
public class ActivityKeyList : IPTSerializable, IEnumerable<SchedulerDefinitions.ActivityKey>
{
    #region IPTSerializable Members
    public ActivityKeyList(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            Node previousNode = null;
            Node currentNode = null;

            for (int i = 0; i < count; i++)
            {
                currentNode = new Node(reader, previousNode, this);
                previousNode = currentNode;

                if (i == 0)
                {
                    first = currentNode;
                }

                previousNode = currentNode;
            }

            last = currentNode;
            this.count = count;
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(Count);
        Node node = First;

        while (node != null)
        {
            node.Serialize(writer);
            node = node.Next;
        }
    }

    public const int UNIQUE_ID = 192;

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ActivityKeyList() { }

    /// <summary>
    /// Creates a deep copy of the activity list.
    /// </summary>
    /// <param name="a_actKeyList"></param>
    public ActivityKeyList(ActivityKeyList a_actKeyList)
    {
        IEnumerator<SchedulerDefinitions.ActivityKey> etr = a_actKeyList.GetEnumerator();
        while (etr.MoveNext())
        {
            Add(etr.Current);
        }
    }

    public class Node : IPTSerializable
    {
        #region IPTSerializable Members
        public Node(IReader reader, Node previous, ListType list)
        {
            if (reader.VersionNumber >= 1)
            {
                data = new ListDataType(reader);
                this.previous = previous;
                if (previous != null)
                {
                    previous.next = this;
                }

                next = null;
                this.list = list;
            }
        }

        public void Serialize(IWriter writer)
        {
            data.Serialize(writer);
        }

        public const int UNIQUE_ID = 197;

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        internal Node previous;
        internal Node next;
        private readonly ListType list;
        private readonly ListDataType data;

        public Node(ListDataType data, Node previous, Node next, ListType list)
        {
            this.data = data;
            this.list = list;

            this.previous = previous;
            this.next = next;

            if (previous != null)
            {
                previous.next = this;
                if (previous.list != list)
                {
                    throw new ListsDontMatchException();
                }
            }

            if (next != null)
            {
                next.previous = this;
                if (next.list != list)
                {
                    throw new ListsDontMatchException();
                }
            }
        }

        public ListDataType Data => data;

        public Node Previous => previous;

        public Node Next => next;

        internal ListType List => list;
    }

    private Node first;

    public Node First => first;

    private Node last;

    public Node Last => last;

    private int count;

    public int Count => count;

    /// <summary>
    /// Adds a deep copy of the data.
    /// </summary>
    /// <param name="data">The element to add to the list.</param>
    /// <param name="after">The element will be added after this node. The node must be a member of this list. If you specify null, the element will be added to the front of the list.</param>
    /// <returns>The node the element is stored in.</returns>
    public Node Add(ListDataType data, Node after)
    {
        Node newNode;
        ListDataType dataCopy = new (data);

        if (after == null)
        {
            newNode = new Node(dataCopy, null, first, this);
            first = newNode;
            if (last == null)
            {
                last = first;
            }
        }
        else
        {
            newNode = new Node(dataCopy, after, after.next, this);

            if (newNode.Next == null)
            {
                last = newNode;
            }
        }

        count++;
        return newNode;
    }

    /// <summary>
    /// Adds a copy of the key.
    /// </summary>
    /// <param name="data">The element to add to the list.</param>
    /// <returns></returns>
    public Node Add(ListDataType data)
    {
        return Add(data, Last);
    }

    /// <summary>
    /// Add a copy of the key to the front of the list.
    /// </summary>
    /// <param name="data">The element to add to the list.</param>
    /// <returns>The node the element is stored in.</returns>
    public Node AddFront(ListDataType data)
    {
        return Add(data, null);
    }

    /// <summary>
    /// Add a copy of the key to the end of the list.
    /// </summary>
    /// <param name="data">The element to add to the list.</param>
    /// <returns>The node the element is stored in.</returns>
    public Node AddEnd(ListDataType data)
    {
        return Add(data, Last);
    }

    public void Clear()
    {
        first = null;
        last = null;
        count = 0;
    }

    public void Remove(Node node)
    {
        if (node.List != this)
        {
            throw new ListsDontMatchException();
        }

        if (node == first && node == last)
        {
            first = null;
            last = null;
        }
        else if (node == first)
        {
            first = first.Next;
            first.previous = null;
        }
        else if (node == last)
        {
            last = last.previous;
            last.next = null;
        }
        else
        {
            node.previous.next = node.next;
            node.next.previous = node.previous;
        }

        #if DEBUG
        node.previous = null;
        node.next = null;
        #endif

        count--;
    }

    public class ListException : CommonException
    {
        public ListException(string msg)
            : base(msg) { }
    }

    public class ListsDontMatchException : ListException
    {
        internal ListsDontMatchException()
            : base("4032") { }
    }

    public ArrayList CreateArrayListShallowCopy()
    {
        ArrayList al = new ();

        Node current = First;

        while (current != null)
        {
            al.Add(current.Data);
            current = current.Next;
        }

        return al;
    }

    #region IEnumerable<ActivityKey>
    /// <summary>
    /// Enumerate an ActivityKeyList
    /// Will not work if the collection being enumerated is changed after the enumerator is created.
    /// </summary>
    private class ActivityKeyEnumerator : IEnumerator<SchedulerDefinitions.ActivityKey>
    {
        /// <summary>
        /// The first node in the ActivityKeyList.
        /// </summary>
        private readonly Node m_startNode;

        /// <summary>
        /// The current node being iterated.
        /// </summary>
        private Node m_curNode;

        /// <summary>
        /// The next node to iterate.
        /// </summary>
        private Node m_nextNode;

        public ActivityKeyEnumerator(ActivityKeyList a_actKeyList)
        {
            m_startNode = a_actKeyList.First;
            Reset();
        }

        public ListDataType Current => m_curNode.Data;

        public void Dispose() { }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            m_curNode = m_nextNode;
            if (m_curNode == null)
            {
                return false;
            }

            m_nextNode = m_curNode.next;
            return true;
        }

        public void Reset()
        {
            m_curNode = null;
            m_nextNode = m_startNode;
        }
    }

    /// <summary>
    /// The enumeration isn't tread safe.
    /// An error may occur if the enumertion is used after the collection is modified.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<ListDataType> GetEnumerator()
    {
        return new ActivityKeyEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion

    public override string ToString()
    {
        string s = string.Format("Count={0}", Count);
        if (Count == 1)
        {
            s += "; " + First.Data;
        }

        return s;
    }
}