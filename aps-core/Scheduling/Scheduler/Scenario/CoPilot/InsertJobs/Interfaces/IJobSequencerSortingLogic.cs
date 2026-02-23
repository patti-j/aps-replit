namespace PT.Scheduler.CoPilot.InsertJobs;

internal interface IJobSequencerSortingLogic
{
    void SortByPriority(List<Job> a_jobList);
}