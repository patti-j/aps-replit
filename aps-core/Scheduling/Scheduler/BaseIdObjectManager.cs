using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Summary description for BaseManager.  All Managers for BaseIdObjects are derived from this.
/// This class manages a SortedList of BaseIdObjects keying off the object's BaseIds and is also
/// able to uniquely create BaseIds.
/// </summary>
public abstract partial class BaseIdObjectManager<T> : ICopyTable, IPTSerializable, AfterRestoreReferences.IAfterRestoreReferences, IEnumerable<T> where T : BaseIdObject
{
    #region Construction
    protected BaseIdObjectManager(BaseIdGenerator a_idGenerator)
    {
        m_idGen = a_idGenerator;
    }

    protected BaseIdObjectManager(BaseIdObjectManager<T> original, BaseIdGenerator aIdGenerator)
    {
        m_idGen = aIdGenerator;

        foreach (T o in original)
        {
            Add(o);
        }
    }
    #endregion

    #region IPTSerializable Members

    public virtual void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif

        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            IPTSerializable ptObject = this[i];
            ptObject.Serialize(a_writer);
        }
    }

    public const int UNIQUE_ID = 4;

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region IAfterRestoreReferences
    public void AfterRestoreReferences_1(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        if (!processedAfterRestoreReferences_1.Contains(this))
        {
            processedAfterRestoreReferences_1.Add(this);
            ReinsertObjects(true);
            OnContentsCallAfterRestoreReferences_1(serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
            AfterRestoreReferences.Helpers.CallObjMembers_AfterRestoreReferences_1(serializationVersionNbr, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        }
    }

    public void AfterRestoreReferences_2(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        if (!processedAfterRestoreReferences_2.Contains(this))
        {
            processedAfterRestoreReferences_2.Add(this);
            ReinsertObjects(false);
            OnContentsCallAfterRestoreReferences_2(serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
            AfterRestoreReferences.Helpers.CallObjMembers_AfterRestoreReferences_2(serializationVersionNbr, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        }
    }

    internal void ReinsertObjects(bool resetIds)
    {
        List<T> resetList = new ();

        for (int i = 0; i < Count; ++i)
        {
            resetList.Add(m_IdSortedList.Values[i]);
        }

        for (int i = Count - 1; i >= 0; --i)
        {
            Remove(m_IdSortedList.Keys[i]);
        }

        for (int i = 0; i < resetList.Count; ++i)
        {
            T o = resetList[i];

            if (resetIds)
            {
                o.Id = NextID();
            }

            Add(o);
        }
    }

    protected void OnContentsCallAfterRestoreReferences_1(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        foreach (KeyValuePair<BaseId, T> kv in m_IdSortedList)
        {
            kv.Value.AfterRestoreReferences_1(serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        }
    }

    protected void OnContentsCallAfterRestoreReferences_2(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        foreach (KeyValuePair<BaseId, T> kv in m_IdSortedList)
        {
            kv.Value.AfterRestoreReferences_2(serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        }
    }
    #endregion

    #region Declarations
    // Choose SortedList for the time being because I'm not sure whether enumerating a Dictionary would be stable across multiple clients.
    // Enumerating the SortedDictionary always yields the same results, but it is slower than Dictionary.
    private readonly SortedList<BaseId, T> m_IdSortedList = new ();

    public class BaseManagerException : PTException
    {
        public BaseManagerException(string message)
            : base(message) { }
    }
    #endregion

    #region Find functions
    /// <summary>
    /// O(ln(n)) - does binary search
    /// </summary>
    /// <param name="a_id">The BaseId of the BaseIdObject.</param>
    /// <returns>The object or null if it's not in the list.</returns>
    public T GetById(BaseId a_id)
    {
        m_IdSortedList.TryGetValue(a_id, out T obj);
        return obj;
    }

    public bool Contains(T a_obj)
    {
        return Contains(a_obj.Id);
    }

    public bool Contains(BaseId a_id)
    {
        return m_IdSortedList.ContainsKey(a_id);
    }

    public T GetByIndex(int a_index)
    {
        return m_IdSortedList.Values[a_index];
    }

    public T this[int a_idx] => GetByIndex(a_idx);
    #endregion

    #region Edit Functions
    private readonly BaseIdGenerator m_idGen;

    protected BaseIdGenerator IdGen => m_idGen;

    protected void InitNextId(long aId)
    {
        m_idGen.InitNextId(aId);
    }

    /// <summary>
    /// O(1).
    /// </summary>
    /// <returns></returns>
    internal BaseId NextID()
    {
        return m_idGen.NextID();
    }

    internal static string GetPrefix(string aPrefix, string aSeparator)
    {
        int sepIdx = aPrefix.IndexOf(aSeparator);

        if (sepIdx >= 1)
        {
            return aPrefix.Remove(sepIdx);
        }

        return aPrefix;
    }

    internal static string CreateId(string aPrefix, string aSeparator, long aNbr)
    {
        return aPrefix + aSeparator + aNbr.ToString().PadLeft(5, '0');
    }

    protected static string MakeDefaultName(string aPrefix, string aSeparator, long aNbr)
    {
        string prefix = GetPrefix(aPrefix, aSeparator);
        return CreateId(prefix, aSeparator, aNbr);
    }

    /// <summary>
    /// O(log2(n)).
    /// Checks to see that an object exists in the list for the specified id.  If not then an error is thrown.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    protected T ValidateExistence(BaseId id)
    {
        if (!Contains(id))
        {
            throw new ValidationException("2121", new object[] { id.ToString() });
        }

        return m_IdSortedList[id];
    }

    /// <summary>
    /// O(n).
    /// </summary>
    /// <param name="a_obj"></param>
    /// <returns></returns>
    protected virtual T Add(T a_obj)
    {
        T o = GetById(a_obj.Id);
        if (o != null)
        {
            return o;
        }

        m_IdSortedList.Add(a_obj.Id, a_obj);

        return a_obj;
    }

    /// <summary>
    /// O(log2(n)).
    /// Add a copy of a clone to the SortedList. The Name of the clone is changed to something else other than what the original BaseIdObject is named.
    /// </summary>
    /// <param name="a_original"></param>
    /// <param name="a_clone"></param>
    /// <param name="a_nextId">The clone's BaseId is changed to this value.</param>
    /// <returns>The clone is returned.</returns>
    protected virtual T AddCopy(T a_original, T a_clone, BaseId a_nextId)
    {
        a_clone.Id = a_nextId;
        return AddCopy(a_original, a_clone);
    }

    protected virtual T AddCopy(T a_original, T a_clone)
    {
        if (a_original.Id == a_clone.Id)
        {
            a_clone.Id = NextID();
        }

        return Add(a_clone);
    }

    /// <summary>
    /// O(log2(n)).
    /// </summary>
    /// <param name="a_id"></param>
    protected virtual void Remove(BaseId a_id)
    {
        m_IdSortedList.Remove(a_id);
    }

    protected virtual void Remove(T a_obj)
    {
        Remove(a_obj.Id);
    }

    protected virtual void RemoveAt(int index)
    {
        m_IdSortedList.RemoveAt(index);
    }

    protected virtual void Clear()
    {
        m_IdSortedList.Clear();
    }
    #endregion

    #region ICopyTable
    public abstract Type ElementType { get; }

    public int Count => m_IdSortedList.Count;

    public object GetRow(int a_index)
    {
        return GetByIndex(a_index);
    }
    #endregion ICopyTable

    #region ERP Transmission handling functionality
    internal enum ERPTHandlerResult { added, updated }
    #endregion

    #region IEnumerable
    public IEnumerator<T> GetEnumerator()
    {
        return m_IdSortedList.Values.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion
}