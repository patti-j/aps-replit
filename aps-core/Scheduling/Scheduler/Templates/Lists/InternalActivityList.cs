using System.Collections;

using PT.Common.Exceptions;

using ListDataType = PT.Scheduler.InternalActivity;
using ListType = PT.Scheduler.InternalActivityList;

// DEVIANT: Not IPTSerializable

namespace PT.Scheduler;

/// <summary>
/// ListTemplate
/// </summary>
[Serializable]
public class InternalActivityList //, PT.Common.IPTSerializable
{
    public const int UNIQUE_ID = 301;

    //		#region IPTSerializable Members
    //
    //		public InternalActivityList(PT.Common.IReader reader)
    //		{
    //			if(reader.VersionNumber>=1)
    //			{
    //				int count;
    //				reader.Read(out count);
    //				Node previousNode=null;
    //				Node currentNode=null;
    //
    //				for(int i=0; i<count; i++)
    //				{
    //					currentNode=new Node(reader, previousNode, this);
    //					previousNode=currentNode;
    //
    //					if(i==0)
    //					{
    //						first=currentNode;
    //					}
    //
    //					previousNode=currentNode;
    //				}
    //
    //				this.last=currentNode;
    //				this.count=count;
    //			}
    //		}
    //
    //		public void Serialize(PT.Common.IWriter writer)
    //		{
    //			writer.Write(this.Count);
    //			Node node=this.First;
    //
    //			while(node!=null)
    //			{
    //				node.Serialize(writer);
    //				node=node.Next;
    //			}
    //		}
    //
    //		public virtual int UniqueId
    //		{
    //			get
    //			{
    //				return UNIQUE_ID;
    //			}
    //		}
    //
    //		#endregion

    public class Node //, PT.Common.IPTSerializable
    {
        public const int UNIQUE_ID = 197;

        //			#region IPTSerializable Members
        //
        //			public Node(PT.Common.IReader reader, Node previous, ListType list)
        //			{
        //				if(reader.VersionNumber>=1)
        //				{
        //					this.data=new ListDataType(reader);
        //					this.previous=previous;
        //					if(previous!=null)
        //					{
        //						previous.next=this;
        //					}
        //					this.next=null;
        //					this.list=list;
        //				}
        //			}
        //
        //			public void Serialize(PT.Common.IWriter writer)
        //			{
        //				this.data.Serialize(writer);
        //			}
        //
        //			public virtual int UniqueId
        //			{
        //				get
        //				{
        //					return UNIQUE_ID;
        //				}
        //			}
        //
        //			#endregion

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

    /// <summary>
    /// Add element to the end of the list.
    /// </summary>
    /// <param name="data">The element to add to the list.</param>
    /// <returns></returns>
    public Node Add(ListDataType data)
    {
        return Add(data, Last);
    }

    /// <summary>
    /// Add element to the front of the list.
    /// </summary>
    /// <param name="data">The element to add to the list.</param>
    /// <returns>The node the element is stored in.</returns>
    public Node AddFront(ListDataType data)
    {
        return Add(data, null);
    }

    /// <summary>
    /// Add element to the end of the list.
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

    public class ListException : PTException
    {
        public ListException(string msg)
            : base(msg) { }
    }

    public class ListsDontMatchException : ListException
    {
        internal ListsDontMatchException()
            : base("The lists don't match in InternalActivityList.") { }
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

    public override string ToString()
    {
        return string.Format("Count={0}", Count);
    }
}