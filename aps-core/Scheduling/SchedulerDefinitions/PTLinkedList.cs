using System.Collections;
using System.Reflection;

namespace PT.Scheduler;

/// <summary>
/// ListTemplate
/// </summary>
public class PTLinkedList<T> : IPTSerializable, IEquatable<PTLinkedList<T>>, IEnumerable<T> where T : IPTDeserializable, IEquatable<T>
{
    #region IPTSerializable Members
    private readonly LinkedList<T> m_ptObjectList = new ();

    public PTLinkedList(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12204)
        {
            a_reader.Read(out int count);

            //This should not get an error, as indicated by T implementing IPTDeserializable
            ConstructorInfo constructorInfo = typeof(T).GetConstructor(new[] { typeof(IReader) });

            for (int i = 0; i < count; i++)
            {
                T newT = (T)constructorInfo.Invoke(new object[] { a_reader });
                m_ptObjectList.AddLast(newT);
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(Count);
        foreach (T ptObject in m_ptObjectList)
        {
            ptObject.Serialize(a_writer);
        }
    }

    public int UniqueId => 1054;
    #endregion

    public PTLinkedList() { }

    public PTLinkedList(IEnumerable<T> a_collection)
    {
        foreach (T baseId in a_collection)
        {
            Add(baseId);
        }
    }

    public int Count => m_ptObjectList.Count;

    private HashSet<T> m_cachedLookupSet;

    public bool Contains(T a_object)
    {
        if (m_cachedLookupSet == null)
        {
            m_cachedLookupSet = new HashSet<T>(m_ptObjectList);
        }

        return m_cachedLookupSet.Contains(a_object);
    }

    /// <summary>
    /// Add element after the specified node.
    /// </summary>
    /// <param name="a_newId">The element to add to the list.</param>
    /// <param name="a_existingId">The element will be added after this node. The node must be a member of this list. If you specify null, the element will be added to the front of the list.</param>
    /// <returns>The node the element is stored in.</returns>
    public bool Add(T a_newObject, T a_existingObject)
    {
        m_cachedLookupSet = null;

        LinkedListNode<T> existingNode = m_ptObjectList.Find(a_existingObject);
        if (existingNode != null)
        {
            m_ptObjectList.AddAfter(existingNode, a_newObject);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Add element to the end of the list.
    /// </summary>
    /// <param name="a_object">The element to add to the list.</param>
    /// <returns></returns>
    public void Add(T a_object)
    {
        m_cachedLookupSet = null;
        m_ptObjectList.AddLast(a_object);
    }

    public void AddRange(IEnumerable<T> a_intersect)
    {
        m_cachedLookupSet = null;
        foreach (T baseId in a_intersect)
        {
            m_ptObjectList.AddLast(baseId);
        }
    }

    /// <summary>
    /// Add element to the front of the list.
    /// </summary>
    /// <param name="a_id">The element to add to the list.</param>
    /// <returns>The node the element is stored in.</returns>
    public void AddFront(T a_object)
    {
        m_cachedLookupSet = null;
        m_ptObjectList.AddFirst(a_object);
    }

    public void Clear()
    {
        m_cachedLookupSet = null;
        m_ptObjectList.Clear();
    }

    public void Remove(T a_object)
    {
        m_cachedLookupSet = null;
        m_ptObjectList.Remove(a_object);
    }

    public override string ToString()
    {
        return string.Format("Contains {0} BaseIds.", Count);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Equals(PTLinkedList<T> a_other)
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

        T[] thisObjects = m_ptObjectList.ToArray();
        T[] otherObjects = a_other.m_ptObjectList.ToArray();

        for (int i = 0; i < thisObjects.Length; i++)
        {
            if (thisObjects[i].Equals(otherObjects[i]))
            {
                return false;
            }
        }

        return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return m_ptObjectList.GetEnumerator();
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
        return m_ptObjectList.GetHashCode();
    }

    public T GetLast()
    {
        return m_ptObjectList.Last.Value;
    }

    public T GetFirst()
    {
        return m_ptObjectList.First.Value;
    }

    public void Intersection(PTLinkedList<T> a_intersectionList)
    {
        m_ptObjectList.Clear();
        AddRange(m_ptObjectList.Intersect(a_intersectionList));
    }
}