using ListDataType = PT.Common.File.ExceptionDescriptionInfo;
using ListType = PT.Common.ExceptionList;

// Warning. This doesn't follow the standard model for list templates.
//
// Differences:
// 1. Exception doesn't have a constructor that accepts an IReader.

namespace PT.Common;

public class ExceptionList : IPTSerializable
{
    public const int UNIQUE_ID = 192;

    #region IPTSerializable Members
    public ExceptionList(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            int count;
            a_reader.Read(out count);
            Node previousNode = null;
            Node currentNode = null;

            for (int i = 0; i < count; i++)
            {
                currentNode = new Node(a_reader, previousNode, this);

                if (i == 0)
                {
                    m_first = currentNode;
                }

                previousNode = currentNode;
            }

            m_last = currentNode;
            m_count = count;
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(Count);
        Node node = First;

        while (node != null)
        {
            node.Serialize(a_writer);
            node = node.Next;
        }
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ExceptionList() { }


    public class Node : IPTSerializable
    {
        public const int UNIQUE_ID = 193;

        #region IPTSerializable Members
        public Node(IReader reader, Node a_previous, ListType list)
        {
            if (reader.VersionNumber >= 1)
            {
                m_data = new ListDataType(reader);

                m_previous = a_previous;
                if (a_previous != null)
                {
                    a_previous.m_next = this;
                }

                m_next = null;
                m_list = list;
            }
        }

        public void Serialize(IWriter writer)
        {
            m_data.Serialize(writer);
        }

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        internal Node m_previous;
        internal Node m_next;
        private readonly ListType m_list;
        private readonly ListDataType m_data;

        public Node(ListDataType a_data, Node a_previous, Node a_next, ListType a_list)
        {
            m_data = a_data;
            m_list = a_list;

            m_previous = a_previous;
            m_next = a_next;

            if (a_previous != null)
            {
                a_previous.m_next = this;
                if (a_previous.m_list != a_list)
                {
                    throw new ListsDontMatchException();
                }
            }

            if (a_next != null)
            {
                a_next.m_previous = this;
                if (a_next.m_list != a_list)
                {
                    throw new ListsDontMatchException();
                }
            }
        }

        public ListDataType Data => m_data;

        public Node Previous => m_previous;

        public Node Next => m_next;

        internal ListType List => m_list;
    }

    private Node m_first;

    public Node First => m_first;

    private Node m_last;

    public Node Last => m_last;

    private int m_count;

    public int Count => m_count;

    public void Add(Exception a_e, Node a_after)
    {
        Add(new ListDataType(a_e), a_after);
    }

    public void Add(Exception a_e)
    {
        Add(new ListDataType(a_e), null);
    }

    /// <summary>
    /// Add element after the specified node.
    /// </summary>
    /// <param name="a_data">The element to add to the list.</param>
    /// <param name="a_after">The element will be added after this node. The node must be a member of this list. If you specify null, the element will be added to the front of the list.</param>
    /// <returns>The node the element is stored in.</returns>
    public Node Add(ListDataType a_data, Node a_after)
    {
        Node newNode;

        if (a_after == null)
        {
            newNode = new Node(a_data, null, m_first, this);
            m_first = newNode;
            if (m_last == null)
            {
                m_last = m_first;
            }
        }
        else
        {
            newNode = new Node(a_data, a_after, a_after.m_next, this);

            if (newNode.Next == null)
            {
                m_last = newNode;
            }
        }

        m_count++;
        return newNode;
    }

    /// <summary>
    /// Add element to the end of the list.
    /// </summary>
    /// <param name="a_data">The element to add to the list.</param>
    /// <returns></returns>
    public Node Add(ListDataType a_data)
    {
        return Add(a_data, Last);
    }

    /// <summary>
    /// Add element to the front of the list.
    /// </summary>
    /// <param name="a_data">The element to add to the list.</param>
    /// <returns>The node the element is stored in.</returns>
    public Node AddFront(ListDataType a_data)
    {
        return Add(a_data, null);
    }

    /// <summary>
    /// Add element to the end of the list.
    /// </summary>
    /// <param name="a_data">The element to add to the list.</param>
    /// <returns>The node the element is stored in.</returns>
    public Node AddEnd(ListDataType a_data)
    {
        return Add(a_data, Last);
    }

    public void Clear()
    {
        m_first = null;
        m_last = null;
        m_count = 0;
    }

    public void Remove(Node a_node)
    {
        if (a_node.List != this)
        {
            throw new ListsDontMatchException();
        }

        if (a_node == m_first && a_node == m_last)
        {
            m_first = null;
            m_last = null;
        }
        else if (a_node == m_first)
        {
            m_first = m_first.Next;
            m_first.m_previous = null;
        }
        else if (a_node == m_last)
        {
            m_last = m_last.m_previous;
            m_last.m_next = null;
        }
        else
        {
            a_node.m_previous.m_next = a_node.m_next;
            a_node.m_next.m_previous = a_node.m_previous;
        }

        #if DEBUG
        a_node.m_previous = null;
        a_node.m_next = null;
        #endif

        m_count--;
    }

    public class ListException : Exception
    {
        public ListException(string a_msg)
            : base(a_msg) { }
    }

    public class ListsDontMatchException : ListException
    {
        internal ListsDontMatchException()
            : base("The lists don't match in ExceptionList.") { }
    }
}