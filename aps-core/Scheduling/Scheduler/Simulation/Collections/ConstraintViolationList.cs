using System.Collections;

using PT.Common.Exceptions;

using ListDataType = PT.Scheduler.ConstraintViolation;
using ListType = PT.Scheduler.ConstraintViolationList;

namespace PT.Scheduler;

/// <summary>
/// ListTemplate
/// </summary>
internal class ConstraintViolationList
{
    internal class Node
    {
        internal Node m_previous;
        internal Node m_next;
        private readonly ListType m_list;
        private readonly ListDataType m_data;

        internal Node(ListDataType a_data, Node a_previous, Node a_next, ListType a_list)
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

        internal ListDataType Data => m_data;

        internal Node Previous => m_previous;

        internal Node Next => m_next;

        internal ListType List => m_list;
    }

    private Node m_first;

    internal Node First => m_first;

    private Node m_last;

    internal Node Last => m_last;

    private int m_count;

    internal int Count => m_count;

    /// <summary>
    /// Add element after the specified node.
    /// </summary>
    /// <param name="data">The element to add to the list.</param>
    /// <param name="after">The element will be added after this node. The node must be a member of this list. If you specify null, the element will be added to the front of the list.</param>
    /// <returns>The node the element is stored in.</returns>
    internal Node Add(ListDataType a_data, Node a_after)
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
    /// <param name="data">The element to add to the list.</param>
    /// <returns></returns>
    internal Node Add(ListDataType a_data)
    {
        return Add(a_data, Last);
    }

    /// <summary>
    /// Add element to the front of the list.
    /// </summary>
    /// <param name="data">The element to add to the list.</param>
    /// <returns>The node the element is stored in.</returns>
    internal Node AddFront(ListDataType a_data)
    {
        return Add(a_data, null);
    }

    /// <summary>
    /// Add element to the end of the list.
    /// </summary>
    /// <param name="data">The element to add to the list.</param>
    /// <returns>The node the element is stored in.</returns>
    internal Node AddEnd(ListDataType a_data)
    {
        return Add(a_data, Last);
    }

    internal virtual void Clear()
    {
        m_first = null;
        m_last = null;
        m_count = 0;
    }

    internal void Remove(Node a_node)
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

    internal class ListException : PTException
    {
        public ListException(string a_msg)
            : base(a_msg) { }
    }

    internal class ListsDontMatchException : ListException
    {
        internal ListsDontMatchException()
            : base("The lists don't match in ConstraintViolationList.") { }
    }

    internal ArrayList CreateArrayListShallowCopy()
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
}