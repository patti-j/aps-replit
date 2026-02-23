namespace PT.Scheduler.CoPilot.InsertJobs;

internal interface IJobSequencerGroupingLogic
{
    List<JobToInsert> GetNextExpediteSet(List<JobToInsert> a_currentJobList);
    void RemoveCurrentSet(List<JobToInsert> a_currentJobList);
    int GetRemainingSetsCount(List<JobToInsert> a_currentJobList);
}