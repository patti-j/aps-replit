namespace PT.Scheduler;

public class TimeBucketListHash
{
    private readonly SortedList<string, TimeBucketList> m_hash = new ();

    public TimeBucketList this[string key]
    {
        get
        {
            if (m_hash.ContainsKey(key))
            {
                return m_hash[key];
            }

            return null;
        }
    }

    public int Count => m_hash.Count;

    public TimeBucketList GetByIndex(int index)
    {
        return m_hash.Values[index];
    }

    public bool Contains(string key)
    {
        return m_hash.ContainsKey(key);
    }

    /// <summary>
    /// Adds a new list.  If a list with the specified key already exists then the two are consolidated.
    /// </summary>
    public void AddOrConsolidate(TimeBucketList timeBucketList)
    {
        if (m_hash.ContainsKey(timeBucketList.Key))
        {
            m_hash[timeBucketList.Key].Absorb(timeBucketList);
        }
        else
        {
            m_hash.Add(timeBucketList.Key, timeBucketList);
        }
    }
}