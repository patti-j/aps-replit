using System.Diagnostics;

using PT.APSCommon;
using PT.Common.Exceptions;

using ListDataType = PT.Scheduler.Simulation.Events.ResourceAvailableEvent;
using ListType = PT.Scheduler.CalendarResourceAvailableEventList;

// DEVIATIONS in Add() and Remove().CreateArrayListShallowCopy()

namespace PT.Scheduler;

/// <summary>
/// ListTemplate
/// </summary>
internal class CalendarResourceAvailableEventList
{
    internal class Node
    {
        #region Additions
        /// <summary>
        /// This is a temporary variable only used during the simulation.
        /// This applies to batch resources only, the activity was left on the available resource event set to allow it to accept more activities.
        /// It's removed from the available resource event set after ScheduleReadyResource has completed for the simulation clock.
        /// </summary>
        internal bool m_removeFromAvailableResourceEventSet; // [BATCH]

        /// <summary>
        /// This is a temporary variable only used during simulation.
        /// When set it indicates that the batch resource should be removed from the set of available resources, and a resource unavailable event
        /// should be scheduled for it.
        /// </summary>
        internal bool BatchResScheduleResourceUnavilableEventForPrimaryRes { get; set; }
        #endregion

        internal Node _previous;
        internal Node _next;
        private ListType _list;
        private ListDataType _data;

        internal Node(ListDataType data, Node previous, Node next, ListType list)
        {
            _data = data;
            Init(previous, next, list);
        }

        internal Node(ListDataType data)
        {
            _data = data;
        }

        internal void Init(Node previous, Node next, ListType list)
        {
            _list = list;

            _previous = previous;
            _next = next;

            if (previous != null)
            {
                previous._next = this;
                if (previous._list != list)
                {
                    throw new ListsDontMatchException();
                }
            }

            if (next != null)
            {
                next._previous = this;
                if (next._list != list)
                {
                    throw new ListsDontMatchException();
                }
            }
        }

        internal ListDataType Data => _data;

        internal Node Previous => _previous;

        internal Node Next => _next;

        internal ListType List => _list;

        internal void Clear()
        {
            _data = null;
            _list = null;
            _next = null;
            _previous = null;
        }

        public override string ToString()
        {
            if (_data != null)
            {
                return _data.ToString();
            }

            return "null";
        }
    }

    private Node first;

    public Node First => first;

    private Node last;

    public Node Last => last;

    private int count;

    public int Count => count;

    /// <summary>
    /// Add element after the specified node.
    /// </summary>
    /// <param name="data">The element to add to the list.</param>
    /// <param name="after">The element will be added after this node. The node must be a member of this list. If you specify null, the element will be added to the front of the list.</param>
    /// <returns>The node the element is stored in.</returns>
    internal Node Add(ListDataType data, Node after)
    {
        Node newNode;
        TestStructure();
        if (after == null)
        {
            newNode = new Node(data, null, first, this);
            first = newNode;
            if (last == null)
            {
                last = first;
            }
        }
        else
        {
            newNode = new Node(data, after, after._next, this);

            if (newNode.Next == null)
            {
                last = newNode;
            }
        }
        TestStructure();
        count++;
        return newNode;
    }

    [Conditional("DEBUG")]
    private void TestStructure()
    {
        if (Last?.Next != null)
        {
            bool b = false;
        }
    }

    internal void Add(Node addNode, Node after)
    {
        TestStructure();

        if (after == null)
        {
            addNode.Init(null, first, this);
            first = addNode;
            if (last == null)
            {
                last = first;
            }
        }
        else
        {
            addNode.Init(after, after.Next, this);

            if (addNode.Next == null)
            {
                last = addNode;
            }
        }

        TestStructure();
        count++;
    }

    /// <summary>
    /// Add element to the end of the list.
    /// </summary>
    /// <param name="data">The element to add to the list.</param>
    /// <returns></returns>
    internal Node Add(ListDataType data)
    {
        return Add(data, Last);
    }

    internal void Add(Node addNode)
    {
        Add(addNode, Last);
    }

    /// <summary>
    /// Add element to the front of the list.
    /// </summary>
    /// <param name="data">The element to add to the list.</param>
    /// <returns>The node the element is stored in.</returns>
    internal Node AddFront(ListDataType data)
    {
        return Add(data, null);
    }

    internal void AddFront(Node addNode)
    {
        Add(addNode, null);
    }

    /// <summary>
    /// Add element to the end of the list.
    /// </summary>
    /// <param name="data">The element to add to the list.</param>
    /// <returns>The node the element is stored in.</returns>
    internal Node AddEnd(ListDataType data)
    {
        return Add(data, Last);
    }

    internal void AddEnd(Node addNode)
    {
        Add(addNode, Last);
    }

    internal virtual void Clear()
    {
        first = null;
        last = null;
        count = 0;
    }

    internal void Remove(Node node)
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
            first._previous = null;
        }
        else if (node == last)
        {
            last = last._previous;
            last._next = null;
        }
        else
        {
            node._previous._next = node._next;
            node._next._previous = node._previous;
        }

        count--;
    }

    internal class ListException : PTException
    {
        public ListException(string msg)
            : base(msg) { }
    }

    internal class ListsDontMatchException : ListException
    {
        internal ListsDontMatchException()
            : base("The lists don't match.") { }
    }

    public override string ToString()
    {
        return base.ToString() + " Contains " + Count + " elements.";
    }

    private void DuplicateCheck()
    {
        Node cur = First;
        HashSet<InternalResource> hs = new ();

        while (cur != null)
        {
            if (hs.Contains(cur.Data.Resource))
            {
                throw new Exception("Duplicate resource added to CalendarResourceAvailableEventList.");
            }

            hs.Add(cur.Data.Resource);
            cur = cur.Next;
        }
    }

    internal ListDataType[] GetDataArray()
    {
        ListDataType[] array = new ListDataType[Count];

        Node cur = First;
        int i = 0;
        while (cur != null)
        {
            array[i] = cur.Data;
            ++i;
            cur = cur.Next;
        }

        return array;
    }

    internal Node[] GetNodeArray()
    {
        Node[] array = new Node[Count];
        int i = 0;
        Node cur = First;
        while (cur != null)
        {
            array[i] = cur;
            ++i;
            cur = cur.Next;
        }

        return array;
    }

    #region Test functions
    internal bool Contains(Resource a_res)
    {
        return Contains(a_res.Id);
    }

    internal bool Contains(BaseId a_id)
    {
        return Contains(a_id.Value);
    }

    internal bool Contains(long a_resId)
    {
        Node cur = First;
        while (cur != null)
        {
            Simulation.Events.ResourceAvailableEvent rae = cur.Data;
            if (rae.Resource.Id == a_resId)
            {
                return true;
            }

            cur = cur.Next;
        }

        return false;
    }
    #endregion
}