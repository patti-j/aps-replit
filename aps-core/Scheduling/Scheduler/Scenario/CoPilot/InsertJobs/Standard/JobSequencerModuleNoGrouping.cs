namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// This module is the default. Each job will be treated individually. There is no grouping.
/// </summary>
internal class JobSequencerModuleNoGrouping : IJobSequencerGroupingLogic
{
    public List<JobToInsert> GetNextExpediteSet(List<JobToInsert> a_currentJobList)
    {
        List<JobToInsert> newList = new ();
        newList.Add(a_currentJobList[0]);
        return newList;
    }

    public void RemoveCurrentSet(List<JobToInsert> a_currentJobList)
    {
        a_currentJobList.RemoveAt(0);
    }

    public int GetRemainingSetsCount(List<JobToInsert> a_currentJobList)
    {
        return a_currentJobList.Count;
    }
}