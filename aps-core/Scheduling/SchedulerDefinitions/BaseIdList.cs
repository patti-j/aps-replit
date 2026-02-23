using System.Collections;

using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// This is a linked list of BaseId. The list is serializable. If you don't need a serializable list, use built in LinkedList
/// There is a Node class that is used for backwards compatibility in deserializing old lists.
/// </summary>
public class BaseIdList : IPTSerializable, IEquatable<BaseIdList>, IEnumerable<BaseId>
{
    public const int UNIQUE_ID = 301;

    #region IPTSerializable Members
    private readonly LinkedList<BaseId> m_idList = new ();

    public BaseIdList(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12204)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                BaseId nextId = new (a_reader);
                m_idList.AddLast(nextId);
            }
        }
        else if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int count);
            Node previousNode = null;

            for (int i = 0; i < count; i++)
            {
                Node currentNode = new (a_reader, previousNode, this);
                m_idList.AddLast(currentNode.Data);

                previousNode = currentNode;
            }
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(Count);
        foreach (BaseId baseId in m_idList)
        {
            baseId.Serialize(writer);
        }
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public BaseIdList() { }

    public BaseIdList(IEnumerable<BaseId> a_collection)
    {
        foreach (BaseId baseId in a_collection)
        {
            Add(baseId);
        }
    }

    [Obsolete("No longer used")]
    public class Node : IPTSerializable
    {
        public const int UNIQUE_ID = 197;

        #region IPTSerializable Members
        public Node(IReader reader, Node previous, BaseIdList list)
        {
            if (reader.VersionNumber >= 1)
            {
                data = new BaseId(reader);
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
        private readonly BaseIdList list;
        private readonly BaseId data;

        public Node(BaseId data, Node previous, Node next, BaseIdList list)
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
                    //throw new ListsDontMatchException();
                }
            }

            if (next != null)
            {
                next.previous = this;
                if (next.list != list)
                {
                    //throw new ListsDontMatchException();
                }
            }
        }

        public BaseId Data => data;

        public Node Previous => previous;

        public Node Next => next;
    }

    public int Count => m_idList.Count;

    private HashSet<BaseId> m_cachedLookupSet;

    public bool Contains(BaseId a_id)
    {
        if (m_cachedLookupSet == null)
        {
            m_cachedLookupSet = new HashSet<BaseId>(m_idList);
        }

        return m_cachedLookupSet.Contains(a_id);
    }

    /// <summary>
    /// Add element after the specified node.
    /// </summary>
    /// <param name="a_newId">The element to add to the list.</param>
    /// <param name="a_existingId">The element will be added after this node. The node must be a member of this list. If you specify null, the element will be added to the front of the list.</param>
    /// <returns>The node the element is stored in.</returns>
    public bool Add(BaseId a_newId, BaseId a_existingId)
    {
        m_cachedLookupSet = null;

        LinkedListNode<BaseId> existingNode = m_idList.Find(a_existingId);
        if (existingNode != null)
        {
            m_idList.AddAfter(existingNode, a_newId);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Add element to the end of the list.
    /// </summary>
    /// <param name="a_id">The element to add to the list.</param>
    /// <returns></returns>
    public void Add(BaseId a_id)
    {
        m_cachedLookupSet = null;
        m_idList.AddLast(a_id);
    }

    public void AddRange(IEnumerable<BaseId> a_intersect)
    {
        m_cachedLookupSet = null;
        foreach (BaseId baseId in a_intersect)
        {
            m_idList.AddLast(baseId);
        }
    }

    /// <summary>
    /// Add element to the front of the list.
    /// </summary>
    /// <param name="a_id">The element to add to the list.</param>
    /// <returns>The node the element is stored in.</returns>
    public void AddFront(BaseId a_id)
    {
        m_cachedLookupSet = null;
        m_idList.AddFirst(a_id);
    }

    public void Clear()
    {
        m_cachedLookupSet = null;
        m_idList.Clear();
    }

    public void Remove(BaseId a_id)
    {
        m_cachedLookupSet = null;
        m_idList.Remove(a_id);
    }

    public override string ToString()
    {
        return string.Format("Contains {0} BaseIds.", Count);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Equals(BaseIdList a_other)
    {
        if (ReferenceEquals(null, a_other))
        {
            return false;
        }

        if (ReferenceEquals(this, a_other))
        {
            return true;
        }

        if (Count != a_other.Count)
        {
            return false;
        }

        if (Count == 0)
        {
            //Empty list
            return true;
        }

        BaseId[] thisIds = m_idList.ToArray();
        BaseId[] otherIds = a_other.m_idList.ToArray();

        for (int i = 0; i < thisIds.Length; i++)
        {
            if (thisIds[i] != otherIds[i])
            {
                return false;
            }
        }

        return true;
    }

    public IEnumerator<BaseId> GetEnumerator()
    {
        return m_idList.GetEnumerator();
    }

    public override bool Equals(object a_obj)
    {
        if (ReferenceEquals(null, a_obj))
        {
            return false;
        }

        if (ReferenceEquals(this, a_obj))
        {
            return true;
        }

        if (a_obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((BaseIdList)a_obj);
    }

    public override int GetHashCode()
    {
        return m_idList.GetHashCode();
    }

    public BaseId GetLast()
    {
        return m_idList.Last.Value;
    }

    public BaseId GetFirst()
    {
        return m_idList.First.Value;
    }

    public void Intersection(BaseIdList a_intersectionList)
    {
        BaseId[] baseIds = m_idList.Intersect(a_intersectionList).ToArray();
        Clear();
        AddRange(baseIds);
    }
}