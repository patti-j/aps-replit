namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// Filters out jobs that are scheduled before need date
/// </summary>
internal class JobSequencerModuleFilterScheduledOnTime : IJobSequencerFilter
{
    public bool ShouldExludeJob(Job a_jobToTest, out string o_filterReason)
    {
        //No notification should be shown
        o_filterReason = "";

        //Check if the job is scheduled before need date.
        if (a_jobToTest.Scheduled && !a_jobToTest.Late)
        {
            return true;
        }

        return false;
    }
}