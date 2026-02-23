namespace PT.APSCommon.Collections;

/// <summary>
/// This was written for Product backlog 1633.
/// This was written as a replacement for persistent hash tables and dictionaries.
/// It was found that use of hash tables and dictionaries could cause a desynchronization problem if the collections are built independantly, such as on
/// the server and the client. For example the server will run for a long time and its collections might udergo lots of insertions and deletions, but when
/// the data is transferred to the client its array won't be built identically to the copy on the server. The end result will cause scheduling differences
/// when scheduling depends on enumerating the arrays.
/// This data structure keeps everything in a list and hashtable which is sorted just prior its elements being accessed, so the collection enumerates through its elements
/// identically on the client and server.
/// It also has a hash table built into it for a performance boost. In the future the hash table will be optional, not using it could save a significant
/// amount of memory. In one test of a customer's data, not using it saved about 200 MB.
/// </summary>
/// <typeparam name="ValueT"></typeparam>
public abstract class BaseIdCustomSortedList<ValueT> where ValueT : IPTSerializable
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
    protected BaseIdCustomSortedList(IBaseIdKeyObjectComparer<ValueT> a_comparer)
    {
        m_sortedList = new List<ValueT>();
        m_comparer = a_comparer;
    }

    protected BaseIdCustomSortedList(IReader a_reader, IBaseIdKeyObjectComparer<ValueT> a_comparer, BaseIdGenerator a_idGenerator = null)
        : this(a_comparer)
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

    public virtual int UniqueId => 1040;
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
    private readonly Dictionary<BaseId, ValueT> m_table = new ();

    /// <summary>
    /// Number of elements contained in the list.
    /// </summary>
    public int Count => m_table.Count;

    protected readonly IBaseIdKeyObjectComparer<ValueT> m_comparer;

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
        BaseId key = m_comparer.GetKey(a_element);
        m_table.Add(key, a_element);
        ClearSortedList();
    }

    /// <summary>
    /// Determines equality using the element with ValueT key
    /// </summary>
    /// <param name="a_element"></param>
    /// <returns></returns>
    public bool RemoveObject(ValueT a_element)
    {
        BaseId key = m_comparer.GetKey(a_element);
        if (m_table.Remove(key))
        {
            ClearSortedList();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes the element with specified key from the list.
    /// </summary>
    /// <param name="a_key">key for the element to remove</param>
    /// <returns>true if element was found and removed, otherwise false</returns>
    public bool RemoveByKey(BaseId a_key)
    {
        if (m_table.Remove(a_key))
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
            m_sortedList = m_table.Values.ToList();
            m_sortedList.Sort(m_comparer);
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

    protected abstract ValueT CreateKeyValue(object a_key);

    /// <summary>
    /// Get the object with specified key.
    /// </summary>
    /// <param name="a_key">key of the object to return</param>
    /// <returns>object if found, otherwise null</returns>
    public ValueT GetValue(BaseId a_key)
    {
        TryGetValue(a_key, out ValueT v);

        return v;
    }

    /// <summary>
    /// TEMPORARY until ScenarioDetail is updated.
    /// </summary>
    /// <param name="a_key"></param>
    /// <returns></returns>
    public ValueT Find(BaseId a_key)
    {
        return GetValue(a_key);
    }

    /// <summary>
    /// Gets value associated with a_key.
    /// </summary>
    /// <returns>
    /// true if the collection contains an element with the specified key; otherwise, false.
    /// </returns>
    /// <param name="a_key"></param>
    /// <param name="o_value">
    /// When this method returns, contains the value associated with the specified
    /// key, if the key is found; otherwise, the default value for the type of the
    /// value parameter. This parameter is passed uninitialized.
    /// </param>
    public bool TryGetValue(BaseId a_key, out ValueT o_value)
    {
        return m_table.TryGetValue(a_key, out o_value);
    }

    /// <summary>
    /// Returns Default(T) if the element does not exist.
    /// </summary>
    /// <param name="a_element"></param>
    /// <returns></returns>
    public bool ContainsValue(ValueT a_element)
    {
        return ContainsKey(m_comparer.GetKey(a_element));
    }

    /// <summary>
    /// Returns true if the list contains an element with key = a_key.
    /// </summary>
    /// <param name="a_key">key to search for</param>
    /// <returns></returns>
    public bool ContainsKey(BaseId a_key)
    {
        return m_table.ContainsKey(a_key);
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
        return m_table.Values.GetEnumerator();
    }

    /// <summary>
    /// Clears the entire list.
    /// </summary>
    public void Clear()
    {
        m_table.Clear();
        ClearSortedList();
    }

    /// <summary>
    /// Set the objects in this collection to the list passed in.
    /// </summary>
    /// <param name="a_newList">The collection will take this object and use it as its new list of objects.</param>
    public void SetCollectionListTo(List<ValueT> a_newList)
    {
        Clear();

        foreach (ValueT el in a_newList)
        {
            Add(el);
        }
    }
}

/// <summary>
/// Used to define how objects in an instance of CustomSortedList should be compared.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IBaseIdKeyObjectComparer<T> : IComparer<T>
{
    BaseId GetKey(T a_element);
}