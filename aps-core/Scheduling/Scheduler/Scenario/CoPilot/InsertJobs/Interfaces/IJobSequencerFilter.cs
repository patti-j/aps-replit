namespace PT.Scheduler.CoPilot.InsertJobs;

internal interface IJobSequencerFilter
{
    bool ShouldExludeJob(Job a_jobToTest, out string o_filterReason);
}