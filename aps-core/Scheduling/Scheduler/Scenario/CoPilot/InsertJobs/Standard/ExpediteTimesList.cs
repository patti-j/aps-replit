namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// Stores just the Time value to insert at.
/// </summary>
internal class ExpediteTimesList : ExpediteTimesListCommon, IExpediteTimesList
{
    private readonly List<long> m_timeItemList;

    internal ExpediteTimesList()
    {
        m_timeItemList = new List<long>();
    }

    public override void Add(InsertJobsTimeItem a_time)
    {
        m_timeItemList.Add(a_time.Ticks);
    }

    public override void Add(Job a_jobToExpedite, long a_earliestExpediteTime)
    {
        Add(new InsertJobsTimeItem(a_earliestExpediteTime));
    }

    public void Clear()
    {
        m_timeItemList.Clear();
    }

    public override int GetCount()
    {
        return m_timeItemList.Count;
    }

    public void AddHashSet(HashSet<InsertJobsTimeItem> a_hashSet)
    {
        List<InsertJobsTimeItem> tempList = a_hashSet.ToList();
        foreach (InsertJobsTimeItem item in tempList)
        {
            m_timeItemList.Add(item.Ticks);
        }
    }

    public override InsertJobsTimeItem GetByIndex(int a_index)
    {
        return new InsertJobsTimeItem(m_timeItemList[a_index]);
    }

    public InsertJobsTimeItem GetCurrentItem()
    {
        return new InsertJobsTimeItem(m_timeItemList[m_expediteTimeIterator]);
    }

    public int GetCurrentIndex()
    {
        return m_expediteTimeIterator;
    }

    public void MoveNext()
    {
        m_expediteTimeIterator++;
    }

    public bool AtEndOfList()
    {
        return m_expediteTimeIterator >= m_timeItemList.Count;
    }

    public void RemoveAtIndex(int a_index)
    {
        m_timeItemList.RemoveAt(a_index);
    }

    public override void SortByTime()
    {
        m_timeItemList.Sort((x, y) => x.CompareTo(y));
    }

    //There is nothing to sort by except time.
    public void SortByTimeAndPathPreference()
    {
        SortByTime();
    }

    //There is nothing to sort by except time.
    public void SortByTimeAndPathIndex()
    {
        SortByTime();
    }

    //Sort times in reverse order
    public void SortByTimeAndPathDescending()
    {
        m_timeItemList.Sort((x, y) => y.CompareTo(x));
    }

    public IExpediteTimesList GetSubsetList(int a_currentRangeStart, int a_nextRangeLength)
    {
        ExpediteTimesList newList = new ();
        List<long> tempList = m_timeItemList.GetRange(a_currentRangeStart, a_nextRangeLength);
        for (int i = 0; i < tempList.Count; i++)
        {
            newList.Add(new InsertJobsTimeItem(tempList[i]));
        }

        return newList;
    }

    public new void AddTimesBeforeNeedDate(Job a_jobToExpedite, long a_earliestExpediteTime)
    {
        base.AddTimesBeforeNeedDate(a_jobToExpedite, a_earliestExpediteTime);
    }

    public new void AddTimesAfterLastBlock(Job a_jobToExpedite, long a_lastBlockTime)
    {
        base.AddTimesAfterLastBlock(a_jobToExpedite, a_lastBlockTime);
    }

    public new void AddModule(IExpediteTimesListFilterModule a_module)
    {
        base.AddModule(a_module);
    }

    /// <summary>
    /// Filter the times in the list using all added filter modules.
    /// </summary>
    public void FilterList()
    {
        base.FilterList(this);
    }
}