namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// This class holds common functions for any IExpediteTimesList implementations so that there is no duplicated code.
/// ExpediteTimeLists should inherit from this class and IExpediteTimesList interface.
/// All virtual functions in this class must be overridden.
/// </summary>
internal class ExpediteTimesListCommon
{
    private const long c_gapLengthTicks = 288000000000; //8 hours
    private readonly List<IExpediteTimesListFilterModule> m_moduleList = new ();
    protected int m_expediteTimeIterator = 0;

    /// <summary>
    /// Adds a series of times from the earliest schedule time to the job need date. Will add times before, withing, and after scheduled blocks.
    /// </summary>
    internal void AddTimesBeforeNeedDate(Job a_jobToExpedite, long a_earliestScheduleTime)
    {
        if (a_earliestScheduleTime == long.MaxValue)
        {
            //There is nothing to add. This job can't be scheduled before need date
            return;
        }

        //Add a series of times before the need date in periods after each block.
        SortByTime();

        long newTimeToAdd = a_earliestScheduleTime;
        newTimeToAdd += c_gapLengthTicks;
        int indexCount = 0;
        int indexMax = GetCount();
        //Add new times between blocks so that jobs can schedule in empty areas.
        while (indexCount < indexMax)
        {
            if (newTimeToAdd < GetByIndex(indexCount).Ticks)
            {
                Add(a_jobToExpedite, newTimeToAdd);
            }
            else
            {
                //The new time is greater than a time already in the list. Start from the next time.
                newTimeToAdd = GetByIndex(indexCount).Ticks;
                indexCount++;
            }

            newTimeToAdd += c_gapLengthTicks;
        }

        //No more blocks, add times from the last block until the need date.
        while (newTimeToAdd < a_jobToExpedite.NeedDateTicks)
        {
            Add(a_jobToExpedite, newTimeToAdd);
            newTimeToAdd += c_gapLengthTicks;
        }
    }

    internal void AddTimesAfterLastBlock(Job a_jobToExpedite, long a_lastBlockTime)
    {
        //Add a series of times before the need date in periods after each block.
        if (a_lastBlockTime > 0)
        {
            SortByTime();
            long lastEligibleBlockTime = GetByIndex(GetCount() - 1).Ticks;

            long newTimeToAdd = lastEligibleBlockTime;
            newTimeToAdd += c_gapLengthTicks;
            //Add new times between blocks so that jobs can schedule in empty areas.
            while (newTimeToAdd <= a_lastBlockTime)
            {
                Add(a_jobToExpedite, newTimeToAdd);
                newTimeToAdd += c_gapLengthTicks;
            }

            //No more blocks, add one final time after the last block
            Add(a_jobToExpedite, newTimeToAdd);
        }
    }

    protected void AddModule(IExpediteTimesListFilterModule a_module)
    {
        m_moduleList.Add(a_module);
    }

    /// <summary>
    /// Filters the times in the list using all of the filter moduels added.
    /// </summary>
    protected void FilterList(IExpediteTimesList a_currentList)
    {
        foreach (IExpediteTimesListFilterModule filter in m_moduleList)
        {
            filter.Filter(a_currentList);
        }
    }

    public virtual InsertJobsTimeItem GetByIndex(int a_index)
    {
        throw new NotImplementedException();
    }

    public virtual void Add(Job a_jobToExpedite, long a_newTimeToAdd)
    {
        Add(new InsertJobsTimeItem(a_newTimeToAdd));
    }

    public virtual void Add(InsertJobsTimeItem a_time)
    {
        throw new NotImplementedException();
    }

    public virtual void SortByTime()
    {
        throw new NotImplementedException();
    }

    public virtual int GetCount()
    {
        throw new NotImplementedException();
    }
}