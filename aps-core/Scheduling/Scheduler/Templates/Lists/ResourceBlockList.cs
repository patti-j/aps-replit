using PT.Common.Exceptions;
using PT.SchedulerDefinitions;

using ListDataType = PT.Scheduler.ResourceBlock;
using ListType = PT.Scheduler.ResourceBlockList;

namespace PT.Scheduler;

public class ResourceBlockList : IPTSerializable
{
    public const int UNIQUE_ID = 324;

    #region IPTSerializable Members
    public ResourceBlockList(IReader reader)
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

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceBlockList() { }

    public class Node : IPTSerializable
    {
        public const int UNIQUE_ID = 325;

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

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        internal Node previous;
        internal Node next;
        private readonly ListType list;
        private readonly ListDataType data;
        #if DEBUG
        public string test = "UNTOUCHED";
        #endif

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

        public override string ToString()
        {
            System.Text.StringBuilder sb = new ();

            sb.AppendFormat("Block={0}; HasPrev={1}; HasNext={2}", data, previous != null, next != null);

            return sb.ToString();
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
    public Node Add(ListDataType data, Node after)
    {
        Node newNode;

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
            newNode = new Node(data, after, after.next, this);

            if (newNode.Next == null)
            {
                last = newNode;
            }
        }

        count++;
        return newNode;
    }

    ///// <summary>
    ///// Add element to the end of the list.
    ///// </summary>
    ///// <param name="data">The element to add to the list.</param>
    ///// <returns></returns>
    //public Node Add(ListDataType data)
    //{
    //    return Add(data, this.Last);
    //}

    ///// <summary>
    ///// Add element to the front of the list.
    ///// </summary>
    ///// <param name="data">The element to add to the list.</param>
    ///// <returns>The node the element is stored in.</returns>
    //public Node AddFront(ListDataType data)
    //{
    //    return Add(data, null);
    //}

    ///// <summary>
    ///// Add element to the end of the list.
    ///// </summary>
    ///// <param name="data">The element to add to the list.</param>
    ///// <returns>The node the element is stored in.</returns>
    //public Node AddEnd(ListDataType data)
    //{
    //    return Add(data, this.Last);
    //}

    //public void Clear()
    //{
    //    first = null;
    //    last = null;
    //    count = 0;
    //}

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
        node.test = "DEAD";
        node.previous = null;
        node.next = null;
        #endif

        count--;
    }

    public class ListException : PTException
    {
        public ListException(string msg)
            : base(msg) { }
    }

    public class ListsDontMatchException : ListException
    {
        internal ListsDontMatchException()
            : base("The lists don't match in ResourceBlockList.") { }
    }

    //public ArrayList CreateArrayListShallowCopy()
    //{
    //    ArrayList al = new ArrayList();

    //    Node current = this.First;

    //    while (current != null)
    //    {
    //        al.Add(current.Data);
    //        current = current.Next;
    //    }

    //    return al;
    //}

    /// <summary>
    /// Find by reference. null means the value isn't in the list.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    internal Node FindByRef(ListDataType data)
    {
        Node temp = first;
        while (temp != null)
        {
            if (ReferenceEquals(temp.Data, data))
            {
                return temp;
            }

            temp = temp.Next;
        }

        return null;
    }

    /// <summary>
    /// The baseId of the block isn't unique. It matches the resource requirement it's used to satisfy.
    /// This function searches for a Block that contains a Batch that contains the specified Activity.
    /// </summary>
    internal Node FindNodeByKey(BlockKey a_bk)
    {
        // To find a block this function is checking for a match on BlockId (which equals the activity's resource requirement index)
        // and the block must also contain the activity. 
        // So it's searching for a block for an activity's resource requirement.
        // BlockKey may be changed in the future to use a single unique BaseId identifier or maybe a resource and unique block identifier on the resource.
        Node cur = first;
        while (cur != null)
        {
            ResourceBlock rb = cur.Data;
            if (rb.Id == a_bk.BlockId)
            {
                InternalActivity ia = rb.Batch.FindActivity(a_bk.ActivityId);
                if (ia != null)
                {
                    return cur;
                }
            }

            cur = cur.Next;
        }

        return null;
    }

    /// <summary>
    /// The baseId of the block isn't unique the block id is equal to resource requirement it's used to satisfy.
    /// The search is based on the unique ActivityId that's part of the block id.
    /// </summary>
    public ResourceBlock FindByKey(BlockKey a_bk)
    {
        Node n = FindNodeByKey(a_bk);
        if (n != null)
        {
            return n.Data;
        }

        return null;
    }

    /// <summary>
    /// Returns the first node in the list that interests with the date.
    /// </summary>
    public Node FindFirstBlockContainingDate(long a_date)
    {
        Node current = first;
        while (current != null)
        {
            if (Common.PTMath.Interval.Contains(current.Data.StartTicks, current.Data.EndTicks, a_date))
            {
                return current;
            }

            current = current.Next;
        }

        return null;
    }

    /// <summary>
    /// Returns all nodes that interesect with the date.
    /// </summary>
    public List<Node> FindAllBlocksContainingDate(long a_date)
    {
        List<Node> matchingNodes = new ();
        Node current = first;
        while (current != null)
        {
            if (current.Data.StartTicks > a_date)
            {
                return matchingNodes;
            }

            if (Common.PTMath.Interval.Contains(current.Data.StartTicks, current.Data.EndTicks, a_date))
            {
                matchingNodes.Add(current);
            }

            current = current.Next;
        }

        return matchingNodes;
    }

    /// <summary>
    /// Iterates the Blocks and returns the first Block starting after the specified date.
    /// Returns null if no such Block is found.
    /// </summary>
    /// <param name="aAfterDateTime"></param>
    /// <returns></returns>
    public Node FindFirstBlockStartingAfter(DateTime aAfterDateTime)
    {
        return FindFirstBlockStartingAfter(aAfterDateTime.Ticks);
    }

    public Node FindFirstBlockStartingAfter(long a_date)
    {
        Node current = first;
        while (current != null)
        {
            if (current.Data.StartTicks > a_date)
            {
                return current;
            }

            current = current.Next;
        }

        return null;
    }

    /// <summary>
    /// Iterates the Blocks and returns the Block scheduled before the given DateTime.
    /// Returns null if no such Block is found.
    /// </summary>
    /// <param name="a_beforeDateTime"></param>
    /// <returns></returns>
    public Node FindFirstBlockBefore(DateTime a_beforeDateTime)
    {
        return FindFirstBlockBefore(a_beforeDateTime.Ticks);
    }

    internal Node FindFirstBlockBefore(long a_ticks)
    {
        // Find the first block at ticks or after ticks.
        // The previous block will be the block before.
        Node cur = First;
        while (cur != null)
        {
            if (cur.Data.Contains(a_ticks))
            {
                return cur.Previous;
            }

            if (cur.Data.StartTicks > a_ticks)
            {
                return cur.Previous;
            }

            cur = cur.Next;
        }

        return null;
    }

    public override string ToString()
    {
        return string.Format("{0} blocks", Count);
    }
}