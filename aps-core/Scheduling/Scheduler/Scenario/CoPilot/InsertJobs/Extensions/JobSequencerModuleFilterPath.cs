using PT.Common.Localization;

namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// Filters out jobs that do not have any scheduleable pahts (path preferences are all set to 0)
/// </summary>
internal class JobSequencerModuleFilterPath : IJobSequencerFilter
{
    public bool ShouldExludeJob(Job a_jobToTest, out string o_filterReason)
    {
        for (int moI = 0; moI < a_jobToTest.ManufacturingOrderCount; moI++)
        {
            for (int pathI = 0; pathI < a_jobToTest.ManufacturingOrders[moI].AlternatePaths.Count; pathI++)
            {
                AlternatePath path = a_jobToTest.ManufacturingOrders[moI].AlternatePaths[pathI];
                if (path.Preference > 0)
                {
                    //This job has at least one path to expedite.
                    o_filterReason = "";
                    return false;
                }
            }
        }

        o_filterReason = Localizer.GetString("All paths have 0 preference");
        return true;
    }
}