using PT.Common.Localization;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// Filters out jobs that have any activity with production status of Running
/// </summary>
internal class JobSequencerModuleFilterRunning : IJobSequencerFilter
{
    public bool ShouldExludeJob(Job a_jobToTest, out string o_filterReason)
    {
        //Check if the job is running.
        for (int moI = 0; moI < a_jobToTest.ManufacturingOrderCount; moI++)
        {
            ManufacturingOrder mo = a_jobToTest.ManufacturingOrders[moI];
            for (int opI = 0; opI < mo.OperationCount; opI++)
            {
                InternalOperation op = (InternalOperation)mo.OperationManager.GetByIndex(opI);
                foreach (InternalActivity act in op.Activities.Activities.Values)
                {
                    if (act.ProductionStatus == InternalActivityDefs.productionStatuses.Running)
                    {
                        o_filterReason = Localizer.GetString("Job is Running");
                        return true;
                    }
                }
            }
        }

        o_filterReason = "";
        return false;
    }
}