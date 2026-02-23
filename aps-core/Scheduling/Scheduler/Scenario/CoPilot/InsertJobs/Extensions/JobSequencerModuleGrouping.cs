namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// This module modifies the standard insert process by managing all remaining jobs as a group.
/// They will be returned as a set, and removed all at once after a simulation.
/// </summary>
internal class JobSequencerModuleGrouping : IJobSequencerGroupingLogic
{
    public List<JobToInsert> GetNextExpediteSet(List<JobToInsert> a_currentJobList)
    {
        return a_currentJobList;
    }

    public void RemoveCurrentSet(List<JobToInsert> a_currentJobList)
    {
        a_currentJobList.Clear();
    }

    //Either 1 or 0. 
    public int GetRemainingSetsCount(List<JobToInsert> a_currentJobList)
    {
        return Math.Min(1, a_currentJobList.Count);
    }
}