using PT.Common.Localization;

namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// Filters out jobs that are finished
/// </summary>
internal class JobSequencerModuleFilterFinished : IJobSequencerFilter
{
    public bool ShouldExludeJob(Job a_jobToTest, out string o_filterReason)
    {
        if (a_jobToTest.Finished)
        {
            o_filterReason = Localizer.GetString("Job is finished");
            return true;
        }

        o_filterReason = "";
        return false;
    }
}