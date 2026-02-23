namespace PT.APSCommon.Collections;

/// <summary>
/// A dictionary collection to optimize performance while allowing for accessing values in a sorted enumerator
/// This is useful for managing data objects that need to be serialized in a predictable sequence.
/// </summary>
/// <typeparam name="ValueT"></typeparam>
/// <typeparam name="KeyT"></typeparam>
public abstract class CustomSortedCollection<ValueT> where ValueT : IComparable<ValueT>, IPTSerializable
{
    #region IPTSerializable memebers
    public virtual void Serialize(IWriter a_writer)
    {
        Sort();
        a_writer.Write(m_sortedList.Count);
        foreach (ValueT obj in m_sortedList)
        {
            Serialize(a_writer, obj);
        }
    }

    protected virtual void Serialize(IWriter a_writer, ValueT a_value)
    {
        a_value.Serialize(a_writer);
    }

    /// <summary>
    /// IF this constructor is used, you must call the DeserializeConstructor();
    /// </summary>
    protected CustomSortedCollection()
    {
        m_sortedList = new List<ValueT>();
    }

    protected CustomSortedCollection(IReader a_reader, BaseIdGenerator a_idGenerator = null)
        : this()
    {
        DeserializeConstructor(a_reader, a_idGenerator);
    }

    /// <summary>
    /// Call this if you're not able to use the normal deserialization constructor.
    /// </summary>
    /// <param name="a_reader"></param>
    /// <param name="a_comparer"></param>
    protected void DeserializeConstructor(IReader a_reader, BaseIdGenerator a_idGenerator = null)
    {
        a_reader.Read(out int count);
        for (int i = 0; i < count; i++)
        {
            ValueT element;
            if (a_idGenerator == null)
            {
                element = CreateInstance(a_reader);
            }
            else
            {
                element = CreateInstance(a_reader, a_idGenerator);
            }

            Add(element);
        }
    }

    protected abstract ValueT CreateInstance(IReader a_reader);

    protected virtual ValueT CreateInstance(IReader a_reader, BaseIdGenerator a_idGenerator)
    {
        return CreateInstance(a_reader);
    }

    public virtual int UniqueId => 1110;
    #endregion

    /// <summary>
    /// This contains the elements added to this collection. It's sorted prior to its use.
    /// </summary>
    private List<ValueT> m_sortedList;

    public IReadOnlyList<ValueT> ReadOnlyList
    {
        get
        {
            Sort();
            return m_sortedList.AsReadOnly();
        }
    }

    /// <summary>
    /// Dictionary used for faster lookups. Using Dictionary instead of Hashtable since
    /// Values won't need to be cast to ValueT.
    /// </summary>
    private readonly HashSet<ValueT> m_hashSet = new ();

    /// <summary>
    /// Number of elements contained in the list.
    /// </summary>
    public int Count => m_hashSet.Count;

    private bool m_sorted;

    protected void ClearSortedList()
    {
        m_sortedList.Clear();
        m_sorted = false;
    }

    /// <summary>
    /// Adds a_element to the list only if another element with the same key doesn't
    /// already exist in the list.
    /// </summary>
    /// <param name="a_element">element to be added</param>
    /// <exception cref="ArgumentException">If element with same key has already been added to collection</exception>
    public void Add(ValueT a_element)
    {
        m_hashSet.Add(a_element);
        ClearSortedList();
    }

    /// <summary>
    /// </summary>
    /// <param name="a_element"></param>
    /// <returns></returns>
    public bool RemoveObject(ValueT a_element)
    {
        if (m_hashSet.Remove(a_element))
        {
            ClearSortedList();
            return true;
        }

        return false;
    }

    /// <summary>
    /// The sorted list is only up to date when sorted. Otherwise it is cleared
    /// </summary>
    protected void Sort()
    {
        if (!m_sorted)
        {
            m_sortedList = m_hashSet.ToList();
            m_sortedList.Sort();
            m_sorted = true;
        }
    }

    /// <summary>
    /// Throws IndexOutOfRangeException if index is out of bounds.
    /// </summary>
    /// <param name="a_index">position of element in Sorted List</param>
    /// <returns></returns>
    public ValueT GetByIndex(int a_index)
    {
        Sort();
        return m_sortedList[a_index];
    }
    
    /// <summary>
    /// Returns Default(T) if the element does not exist.
    /// </summary>
    /// <param name="a_element"></param>
    /// <returns></returns>
    public bool ContainsValue(ValueT a_element)
    {
        return m_hashSet.Contains(a_element);
    }

    /// <summary>
    /// Gets a sorted instance of IEnumerator for the list.
    /// </summary>
    /// <returns>Sorted IEnumerator</returns>
    public IEnumerator<ValueT> GetEnumerator()
    {
        Sort();
        return m_sortedList.GetEnumerator();
    }

    /// <summary>
    /// Warning! This is only for use when restoring references when it's possible for the elements of the list to be unsortable.
    /// Not all sub classes will need to use this version during restore references, only the ones whose variables that are used for sorting haven't been
    /// set by the time restore references is called.
    /// </summary>
    public IEnumerator<ValueT> GetUnsortedEnumerator()
    {
        return m_hashSet.GetEnumerator();
    }

    /// <summary>
    /// Clears the entire list.
    /// </summary>
    public void Clear()
    {
        m_hashSet.Clear();
        ClearSortedList();
    }
}