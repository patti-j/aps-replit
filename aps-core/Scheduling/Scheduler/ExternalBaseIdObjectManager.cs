using PT.APSCommon;
using PT.Common.Extensions;
using PT.Transmissions;

namespace PT.Scheduler;

public interface IFastLookupByExternalId
{
    bool InitFastLookupByExternalId();

    void DeInitFastLookupByExternalId();
}

/// <summary>
/// Summary description for BaseManager.  All Managers for BaseIdObjects are derived from this.
/// This class manages a SortedList of BaseIdObjects keying off the object's BaseIds and is also
/// able to uniquely create BaseIds.
/// </summary>
public abstract class ExternalBaseIdObjectManager<T> : BaseIdObjectManager<T>, IFastLookupByExternalId where T : ExternalBaseIdObject
{
    #region Construction
    protected ExternalBaseIdObjectManager(BaseIdGenerator a_idGenerator)
        : base(a_idGenerator) { }

    protected ExternalBaseIdObjectManager(BaseIdObjectManager<T> original, BaseIdGenerator aIdGenerator)
        : base(original, aIdGenerator) { }
    #endregion

    #region IPTSerializable Members

    public const int UNIQUE_ID = 983;

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    private Dictionary<string, T> m_externalIdDictionary;

    private readonly object m_initFastLookupLock = new ();
    private int m_fastLookupTracker;

    /// <summary>
    /// You cannot change the External Ids of objects held by this manager while using this functionality. To do so is an error
    /// whose consequences are unknown.
    /// You can only use this function if the objects that are being stored derive from BaseObject, there is at least
    /// one instance of a class that derives from this class but doesn't store BaseObjects in it.
    /// If you need to perform a lot of finds by External Id you can setup fast searches by External Id
    /// by calling this function. As adds and deletes are performed to the manager the fast find datastructure
    /// is kept up to date. When you are done with fast finding of External Ids call the DeInitFastGetByExternalId()
    /// to release the memory consumed.
    /// An example of a time when you may need to use this is when processing an ERP transmission. You will
    /// be searching for existing objects based on External Id.
    /// </summary>
    /// <returns>true if initialized (was not already initialized)</returns>
    public bool InitFastLookupByExternalId()
    {
        //Wait for one of the threads to set up the dictionary
        lock (m_initFastLookupLock)
        {
            m_fastLookupTracker++;

            if (m_externalIdDictionary != null)
            {
                return false; // already setup, additional threads will return here
            }

            m_externalIdDictionary = new Dictionary<string, T>(StringComparers.CaseSensitiveComparer);

            foreach (T o in this)
            {
                m_externalIdDictionary.Add(o.ExternalId, o);
            }
        }

        return true;
    }

    /// <summary>
    /// When you are done fast finding by external id, call this function to release the data consumed by the
    /// fast find datastructure. To make sure you call this function try putting its call in a finally block.
    /// No error will occur if this function is called without InitFastGetByExternalId() having been called.
    /// </summary>
    public void DeInitFastLookupByExternalId()
    {
        lock (m_initFastLookupLock)
        {
            if (m_fastLookupTracker == 0)
            {
                return;
            }

            m_fastLookupTracker--;
            //Don't release until all threads have finished using the dictionary
            if (m_fastLookupTracker == 0)
            {
                m_externalIdDictionary = null;
            }
        }
    }

    public new bool Contains(T a_obj)
    {
        if (m_externalIdDictionary != null)
        {
            return m_externalIdDictionary.ContainsKey(a_obj.ExternalId);
        }

        return base.Contains(a_obj);
    }

    /// <summary>
    /// O(1) if InitFastGetByExternalId has been called else O(n)
    /// </summary>
    /// <param name="a_externalId">The ExternalId of the BaseObject you looking for.</param>
    /// <returns>The BaseObject or null if it's not in the list.</returns>
    public T GetByExternalId(string a_externalId)
    {
        if (m_externalIdDictionary == null)
        {
            foreach (T o in this)
            {
                if (StringComparers.CaseSensitiveComparer.Compare(o.ExternalId, a_externalId) == 0)
                {
                    return o;
                }
            }
        }
        else
        {
            if (m_externalIdDictionary.TryGetValue(a_externalId, out T value))
            {
                return value;
            }
        }

        return null;
    }

    private const string DEFAULT_SEPERATOR = "";

    /// <summary>
    /// Return an ExternalId that is not in use.
    /// </summary>
    /// <returns></returns>
    internal string NextExternalId(string a_externalId)
    {
        return NextExternalId(a_externalId, DEFAULT_SEPERATOR);
    }

    /// <summary>
    /// O(1) if collection has been initialized for fast Lookups by ExternalId (i.e. method InitFastGetByExternald)
    /// else O(n)
    /// </summary>
    /// <param name="a_externalId">ExternalId of the object to search for</param>
    /// <returns>true if object with ExternalId == a_externalId exists in this collection else false.</returns>
    public bool ContainsExternalId(string a_externalId)
    {
        if (m_externalIdDictionary == null)
        {
            foreach (T o in this)
            {
                if (StringComparers.CaseSensitiveComparer.Compare(o.ExternalId, a_externalId) == 0)
                {
                    return true;
                }
            }

            return false; // didn't find it.
        }

        return m_externalIdDictionary.ContainsKey(a_externalId);
    }

    internal string NextExternalId(string externalId, string separator)
    {
        string prefix = GetPrefix(externalId, separator);
        prefix.TrimEnd(null);

        long i = 1;
        while (true)
        {
            string newId = CreateId(prefix, separator, i);

            if (GetByExternalId(newId) == null)
            {
                return newId;
            }

            i++;
        }
    }

    /// <summary>
    /// O(n).
    /// Throws a Validation Exception if the proposedExternalId is already being used.
    /// </summary>
    /// <param name="a_proposedExternalId"></param>
    protected void ValidateNewExternalId(string a_proposedExternalId)
    {
        if (GetByExternalId(a_proposedExternalId) != null)
        {
            throw new ValidationException("2122", new object[] { a_proposedExternalId });
        }
    }

    /// <summary>
    /// O(n).
    /// </summary>
    /// <param name="a_obj">Object to add to this Manager</param>
    /// <returns>the object</returns>
    protected override T Add(T a_obj)
    {
        bool? tryAdd = m_externalIdDictionary?.TryAdd(a_obj.ExternalId, a_obj);
        if (tryAdd.HasValue && !tryAdd.Value)
        {
            //ExternalIdDictionary is enabled and we added a duplicate ExternalId
            throw new ValidationException($"Attempted to add duplicate {a_obj.GetType()} with ExternalId {a_obj.ExternalId}", new object[] { a_obj.ExternalId });
        }

        return base.Add(a_obj);
    }

    protected override void Remove(T a_obj)
    {
        m_externalIdDictionary?.Remove(a_obj.ExternalId);

        base.Remove(a_obj);
    }

    /// <summary>
    /// O(log2(n)).
    /// </summary>
    /// <param name="a_id"></param>
    protected override void Remove(BaseId a_id)
    {
        if (m_externalIdDictionary != null && Contains(a_id))
        {
            T obj = GetById(a_id);
            m_externalIdDictionary.Remove(obj.ExternalId);
        }

        base.Remove(a_id);
    }

    protected override void RemoveAt(int index)
    {
        T toRemove = this[index];

        if (m_externalIdDictionary != null)
        {
            m_externalIdDictionary.Remove(toRemove.ExternalId);
        }

        base.Remove(toRemove);
    }

    protected override void Clear()
    {
        base.Clear();
        m_externalIdDictionary?.Clear();
    }

    protected override T AddCopy(T a_original, T a_clone, BaseId a_nextId)
    {
        if (m_externalIdDictionary != null)
        {
            m_externalIdDictionary.TryAdd(a_clone.ExternalId, a_clone);
        }

        return base.AddCopy(a_original, a_clone, a_nextId);
    }

    /// <summary>
    /// If an object in this collection has its ExternalId modified, we need to track the change in the ExternalId Cache
    /// </summary>
    /// <param name="a_oldExternalId"></param>
    /// <param name="a_newExternalId"></param>
    /// <param name="a_obj"></param>
    protected void UpdateObjectExternalId(string a_oldExternalId, string a_newExternalId, T a_obj)
    {
        if (m_externalIdDictionary != null)
        {
            m_externalIdDictionary.Remove(a_oldExternalId);
            m_externalIdDictionary.Add(a_newExternalId, a_obj);
        }
    }
}