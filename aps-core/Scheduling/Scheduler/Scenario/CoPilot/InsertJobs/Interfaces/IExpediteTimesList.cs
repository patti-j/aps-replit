namespace PT.Scheduler.CoPilot.InsertJobs;

//The ExpediteTimeList provides all functionality for InsertJobs list of expedite times.
internal interface IExpediteTimesList
{
    /// <summary>
    /// Stores any required information from the InsertJobsTimeItem class in the list. The implementations decide what information to store.
    /// </summary>
    void Add(InsertJobsTimeItem a_time);

    /// <summary>
    /// Stores a time for each path of a job if needed. The implementation will decide which paths to add to.
    /// </summary>
    void Add(Job a_jobToExpedite, long a_timeTicks);

    /// <summary>
    /// Adds a series of times to each path if needed. A range of times are calculated based upon the job need date and the earliest allowed schedule time.
    /// </summary>
    void AddTimesBeforeNeedDate(Job a_jobToExpedite, long a_earliestScheduleTime);

    /// <summary>
    /// Adds a series of times to each path if needed. A range of times are calculated based upon the last block on eligble resources and the last block.
    /// </summary>
    void AddTimesAfterLastBlock(Job a_jobToExpedite, long a_lastBlockTime);

    void Clear();

    int GetCount();

    int GetCurrentIndex();

    /// <summary>
    /// Sorts the list in place, ordering by time.
    /// </summary>
    void SortByTime();

    /// <summary>
    /// Sorts the list in place ordering by path preference then time.
    /// </summary>
    void SortByTimeAndPathPreference();

    /// <summary>
    /// Sorts the list in place ordering by path index then time.
    /// </summary>
    void SortByTimeAndPathIndex();

    /// <summary>
    /// Sorts the list in place ordering by path preference and then time. The implementation will distinguish this function from SortByTimeAndPath()
    /// </summary>
    void SortByTimeAndPathDescending();

    /// <summary>
    /// Returns a new IExpediteTimeList
    /// </summary>
    IExpediteTimesList GetSubsetList(int a_currentRangeStart, int a_nextRangeLength);

    /// <summary>
    /// Add items from a hash set
    /// </summary>
    void AddHashSet(HashSet<InsertJobsTimeItem> a_hashSet);

    InsertJobsTimeItem GetByIndex(int a_index);

    InsertJobsTimeItem GetCurrentItem();

    bool AtEndOfList();

    //Increases the iterator.
    void MoveNext();

    void RemoveAtIndex(int a_index);

    void AddModule(IExpediteTimesListFilterModule a_module);

    /// <summary>
    /// Filter the list using all added filter modules.
    /// </summary>
    void FilterList();
}