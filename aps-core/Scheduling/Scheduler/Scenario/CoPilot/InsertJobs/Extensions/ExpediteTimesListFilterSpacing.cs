namespace PT.Scheduler.CoPilot.InsertJobs;

internal class ExpediteTimesListFilterSpacing : IExpediteTimesListFilterModule
{
    private readonly long m_minInterval;

    public ExpediteTimesListFilterSpacing(TimeSpan a_minInterval)
    {
        m_minInterval = a_minInterval.Ticks;
    }

    /// <summary>
    /// Remove all times that are followed by another time within a specified interval. This will reduce the number of times to try.
    /// The times before gaps are preserved so that expedites occur at the times followed by gaps.
    /// The last time is not removed.
    /// </summary>
    /// <param name="a_expediteTimeList"></param>
    /// <param name="a_minInterval"></param>
    public void Filter(IExpediteTimesList a_expediteTimeList)
    {
        a_expediteTimeList.SortByTimeAndPathIndex();
        for (int i = a_expediteTimeList.GetCount() - 2; i >= 0; i--)
        {
            InsertJobsTimeItem next = a_expediteTimeList.GetByIndex(i + 1);
            InsertJobsTimeItem current = a_expediteTimeList.GetByIndex(i);
            if (next.Ticks - current.Ticks < m_minInterval)
            {
                //Don't remove times within the interval that are on different paths.
                if (current.PathIndex == next.PathIndex)
                {
                    a_expediteTimeList.RemoveAtIndex(i);
                }
            }
        }
    }
}