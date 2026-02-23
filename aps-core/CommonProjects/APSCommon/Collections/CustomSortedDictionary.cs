using System.Collections;

namespace PT.APSCommon.Collections;

/// <summary>
/// A dictionary collection to optimize performance while allowing for accessing values in a sorted enumerator
/// This is useful for managing data objects that need to be serialized in a predictable sequence.
/// </summary>
/// <typeparam name="ValueT"></typeparam>
/// <typeparam name="KeyT"></typeparam>
public abstract class CustomSortedDictionary<KeyT, ValueT> : IEnumerable<ValueT> where KeyT : IEquatable<KeyT> where ValueT : IPTSerializable, IComparable<ValueT>, IKey<KeyT>
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
    protected CustomSortedDictionary()
    {
        m_sortedList = new List<ValueT>();
    }

    protected CustomSortedDictionary(IReader a_reader, BaseIdGenerator a_idGenerator = null)
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

    public virtual int UniqueId => 1041;
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
    private readonly Dictionary<KeyT, ValueT> m_dictionary = new ();

    /// <summary>
    /// Number of elements contained in the list.
    /// </summary>
    public int Count => m_dictionary.Count;

    private bool m_sorted;

    protected bool Sorted => m_sorted;

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
    public virtual void Add(ValueT a_element)
    {
        KeyT key = a_element.GetKey();
        m_dictionary.Add(key, a_element);
        ClearSortedList();
    }

    /// <summary>
    /// </summary>
    /// <param name="a_element"></param>
    /// <returns></returns>
    public bool RemoveObject(ValueT a_element)
    {
        KeyT key = a_element.GetKey();
        if (m_dictionary.Remove(key))
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
    public bool RemoveByKey(KeyT a_key)
    {
        if (m_dictionary.Remove(a_key))
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
            m_sortedList = m_dictionary.Values.ToList();
            m_sortedList.Sort();
            m_sorted = true;
        }
    }

    public ValueT this[int a_index] => GetByIndex(a_index);

    public ValueT this[KeyT a_key] => GetValue(a_key);

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
    /// Get the object with specified key.
    /// </summary>
    /// <param name="a_key">key of the object to return</param>
    /// <returns>object if found, otherwise null</returns>
    public ValueT GetValue(KeyT a_key)
    {
        TryGetValue(a_key, out ValueT v);

        return v;
    }

    /// <summary>
    /// Gets value associated with a_key.
    /// </summary>
    /// <returns>
    /// true if the collection contains an
    /// element with the specified key; otherwise, false.
    /// </returns>
    /// <param name="a_key"></param>
    /// <param name="o_value">
    /// When this method returns, contains the value associated with the specified
    /// key, if the key is found; otherwise, the default value for the type of the
    /// value parameter. This parameter is passed uninitialized.
    /// </param>
    public bool TryGetValue(KeyT a_key, out ValueT o_value)
    {
        return m_dictionary.TryGetValue(a_key, out o_value);
    }

    /// <summary>
    /// Returns Default(T) if the element does not exist.
    /// </summary>
    /// <param name="a_element"></param>
    /// <returns></returns>
    public bool ContainsValue(ValueT a_element)
    {
        return ContainsKey(a_element.GetKey());
    }

    /// <summary>
    /// Returns true if the list contains an element with key = a_key.
    /// </summary>
    /// <param name="a_key">key to search for</param>
    /// <returns></returns>
    public bool ContainsKey(KeyT a_key)
    {
        return m_dictionary.ContainsKey(a_key);
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
        return m_dictionary.Values.GetEnumerator();
    }

    /// <summary>
    /// Get the keys in a shallow copy so the underlying collection can be modified while enumerating these keys
    /// </summary>
    /// <returns></returns>
    public IEnumerable<KeyT> GetUnsortedKeys()
    {
        //TODO: Could this array be too big? Should we use LL?
        return m_dictionary.Keys.ToArray();
    }

    /// <summary>
    /// Clears the entire list.
    /// </summary>
    public void Clear()
    {
        m_dictionary.Clear();
        ClearSortedList();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}