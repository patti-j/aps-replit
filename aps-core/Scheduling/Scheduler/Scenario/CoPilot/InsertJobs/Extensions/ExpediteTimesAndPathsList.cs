namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// Stores time, path index, and path preference.
/// See interface for additional comments.
/// </summary>
internal class ExpediteTimesAndPathsList : ExpediteTimesListCommon, IExpediteTimesList
{
    private List<Tuple<long, int, int>> m_timeItemList;

    internal ExpediteTimesAndPathsList()
    {
        m_timeItemList = new List<Tuple<long, int, int>>();
    }

    public override void Add(InsertJobsTimeItem a_time)
    {
        m_timeItemList.Add(new Tuple<long, int, int>(a_time.Ticks, a_time.PathIndex, a_time.PathPreference));
    }

    public override void Add(Job a_jobToExpedite, long a_earliestExpediteTime)
    {
        for (int moI = 0; moI < a_jobToExpedite.ManufacturingOrderCount; moI++)
        {
            for (int pathI = 0; pathI < a_jobToExpedite.ManufacturingOrders[moI].AlternatePaths.Count; pathI++)
            {
                AlternatePath path = a_jobToExpedite.ManufacturingOrders[moI].AlternatePaths[pathI];
                if (path.Preference > 0)
                {
                    Add(new InsertJobsTimeItem(a_earliestExpediteTime, pathI, path.Preference));
                }
            }
        }
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
        for (int i = 0; i < tempList.Count; i++)
        {
            m_timeItemList.Add(new Tuple<long, int, int>(tempList[i].Ticks, tempList[i].PathIndex, tempList[i].PathPreference));
        }
    }

    public override InsertJobsTimeItem GetByIndex(int a_index)
    {
        return new InsertJobsTimeItem(m_timeItemList[a_index].Item1, m_timeItemList[a_index].Item2, m_timeItemList[a_index].Item3);
    }

    public InsertJobsTimeItem GetCurrentItem()
    {
        return new InsertJobsTimeItem(m_timeItemList[m_expediteTimeIterator].Item1, m_timeItemList[m_expediteTimeIterator].Item2, m_timeItemList[m_expediteTimeIterator].Item3);
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
        m_timeItemList.Sort((x, y) => x.Item1.CompareTo(y.Item1));
    }

    public void SortByTimeAndPathPreference()
    {
        m_timeItemList = m_timeItemList.OrderBy(x => x.Item3).ThenBy(x => x.Item1).ToList();
    }

    public void SortByTimeAndPathIndex()
    {
        m_timeItemList = m_timeItemList.OrderBy(x => x.Item2).ThenBy(x => x.Item1).ToList();
    }

    /// <summary>
    /// Sorts the time values in descending order. This is used for scheduling before the need date where the times are iterated from later to earlier times
    /// </summary>
    public void SortByTimeAndPathDescending()
    {
        m_timeItemList = m_timeItemList.OrderBy(x => x.Item3).ThenByDescending(x => x.Item1).ToList();
    }

    public IExpediteTimesList GetSubsetList(int a_currentRangeStart, int a_nextRangeLength)
    {
        ExpediteTimesAndPathsList newList = new ();
        List<Tuple<long, int, int>> tempList = m_timeItemList.GetRange(a_currentRangeStart, a_nextRangeLength);
        for (int i = 0; i < tempList.Count; i++)
        {
            newList.Add(new InsertJobsTimeItem(tempList[i].Item1, tempList[i].Item2, tempList[i].Item3));
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

    /// <summary>
    /// Adds a filter module to the list of filters.
    /// </summary>
    public new void AddModule(IExpediteTimesListFilterModule a_module)
    {
        base.AddModule(a_module);
    }

    /// <summary>
    /// Filter the list using all added filter modules.
    /// </summary>
    public void FilterList()
    {
        base.FilterList(this);
    }
}