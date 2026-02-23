namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// Sorts jobs by need date.
/// </summary>
internal class JobSequencerSortingNeedDate : IJobSequencerSortingLogic
{
    /// <summary>
    /// The sorting Priority is NeedDate
    /// </summary>
    public void SortByPriority(List<Job> a_jobList)
    {
        a_jobList.Sort((x, y) => x.NeedDateTime.CompareTo(y.NeedDateTime));
    }
}