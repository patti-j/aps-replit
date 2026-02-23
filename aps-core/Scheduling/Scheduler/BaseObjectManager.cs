using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Manager for BaseObject objects. This adds methods relating to finding things by Name and ExternalId.
/// </summary>
public abstract class BaseObjectManager<T> : ExternalBaseIdObjectManager<T> where T : BaseObject
{
    #region Construction
    protected BaseObjectManager(BaseIdGenerator a_idGenerator)
        : base(a_idGenerator) { }

    protected BaseObjectManager(BaseObjectManager<T> original, BaseIdGenerator aIdGenerator)
        : base(original, aIdGenerator) { }
    #endregion

    #region IPTSerializable Members
    public new const int UNIQUE_ID = 799;

    public new virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region Find functions
    private T GetByName(string a_name, StringComparer a_comparer)
    {
        foreach (T o in this)
        {
            if (a_comparer.Compare(o.Name, a_name) == 0)
            {
                return o;
            }
        }

        return null;
    }

    /// <summary>
    /// O(n)
    /// </summary>
    /// <param name="a_name">The BaseObject Name of the object you are looking for.</param>
    /// <param name="a_caseSensitive">whether to perform a case-sensitive comparison</param>
    /// <returns>The BaseObject or null if it's not found.</returns>
    public T GetByName(string a_name, bool a_caseSensitive = false)
    {
        return GetByName(a_name, a_caseSensitive ? StringComparers.CaseSensitiveComparer : StringComparers.CaseInsensitiveComparer);
    }

    /// <summary>
    /// O(n).
    /// </summary>
    /// <param name="a_substring">the substring that should be contained in the BaseObject Name</param>
    /// <returns>the first object with Name that contains a_substring.  If no object with such name is found, returns null.</returns>
    public T GetByNameSubstring(string a_substring, bool a_caseSensitive)
    {
        StringComparison comparison = a_caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

        foreach (T o in this)
        {
            if (o.Name.IndexOf(a_substring, comparison) != -1)
            {
                return o;
            }
        }

        return null;
    }

    /// <summary>
    /// O(n)
    /// </summary>
    /// <param name="a_name">name to search</param>
    /// <returns>true if an object exists with provided name else false</returns>
    public bool ContainsName(string a_name)
    {
        return GetByName(a_name) != null;
    }

    public new bool Contains(T a_obj)
    {
        return base.Contains(a_obj);
    }
    #endregion

    #region Edit Functions
    // this is a cache that stores the last number used when creating a default name.
    private int m_currentNamingNbr = -1;

    /// <summary>
    /// call this when an object is removed from this collection. This is necessary
    /// to ensure client and server stay in sync.
    /// </summary>
    private void ResetCurrentNamingNbr()
    {
        m_currentNamingNbr = -1;
    }

    /// <summary>
    /// if the object with m_currentNamingNbr in its name is deleted, we need to reduce
    /// m_currentNamingNbr by one since otherwise another client that has not cached
    /// m_currentNamingNbr would calculate a different value for it.
    /// </summary>
    /// <param name="a_obj"></param>
    private void RemoveHelper(T a_obj)
    {
        string nbrPartOfName = a_obj.Name?.Replace($"{a_obj.DefaultNamePrefix.Localize()} ", "");
        if (int.TryParse(nbrPartOfName, out int nbr) && m_currentNamingNbr == nbr)
        {
            m_currentNamingNbr--;
        }
    }

    private string GetNextDefaultName(T a_obj)
    {
        if (m_currentNamingNbr != -1) // last used nbr was calculated previously, add one and use it.
        {
            m_currentNamingNbr += 1;
            return MakeDefaultName(a_obj, m_currentNamingNbr);
        }

        int lastUsedNbr = 0;
        foreach (T o in this)
        {
            string localizedPrefix = o.DefaultNamePrefix.Localize();
            string nbrPartOfName = o.Name?.Replace($"{localizedPrefix} ", "");
            if (int.TryParse(nbrPartOfName, out int nbr) && nbr > lastUsedNbr)
            {
                lastUsedNbr = nbr;
            }
        }

        m_currentNamingNbr = lastUsedNbr;
        return GetNextDefaultName(a_obj);
    }

    protected static string MakeDefaultName(T a_o, long a_nbr)
    {
        return string.Format("{0} {1}".Localize(), a_o.DefaultNamePrefix.Localize(), a_nbr.ToString());
    }

    protected static string MakeCopyName(string a_nameOfOriginal)
    {
        return string.Format("Copy of {0}".Localize(), a_nameOfOriginal);
    }

    /// <summary>
    /// O(n).
    /// </summary>
    /// <param name="a_obj">Object to add to this Manager</param>
    /// <returns>the object</returns>
    protected override T Add(T a_obj)
    {
        a_obj = base.Add(a_obj);

        if (string.IsNullOrEmpty(a_obj.Name))
        {
            a_obj.Name = GetNextDefaultName(a_obj);
        }

        return a_obj;
    }

    protected override T AddCopy(T a_original, T a_clone, BaseId a_nextId)
    {
        a_clone.Name = MakeCopyName(a_original.Name);
        a_clone.ExternalId = ExternalBaseIdObject.MakeExternalId(a_nextId.Value);
        return base.AddCopy(a_original, a_clone, a_nextId);
    }

    protected override void Remove(T a_obj)
    {
        RemoveHelper(a_obj);

        base.Remove(a_obj.Id);
    }

    /// <summary>
    /// O(log2(n)).
    /// </summary>
    /// <param name="a_id"></param>
    protected override void Remove(BaseId a_id)
    {
        T obj = GetById(a_id);

        if (obj != null)
        {
            RemoveHelper(obj);
        }

        base.Remove(a_id);
    }

    protected override void RemoveAt(int index)
    {
        T toRemove = this[index];
        RemoveHelper(toRemove);

        base.RemoveAt(index);
    }

    protected override void Clear()
    {
        ResetCurrentNamingNbr();
        base.Clear();
    }

    internal bool DeleteUserFieldByExternalId(string a_userFieldExternalId)
    {
        bool removed = false;
        foreach (T baseObject in this)
        {
            if (baseObject.UserFields.Contains(a_userFieldExternalId))
            {
                removed |= baseObject.UserFields.Remove(a_userFieldExternalId);
            }
        }

        return removed;
    }
    #endregion
}