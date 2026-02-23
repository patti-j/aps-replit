using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.InsertJobs;

internal class SimulationModuleInsertPreProcessingPaths : ISimulationCoreInsertPreProcessing
{
    /// <summary>
    /// Sets all path AutoUse enums to IfCurrent. Sets the current path to the path index specified in the insertTime.
    /// All jobs in the set are modified this way.
    /// </summary>
    public void PerformPreProcessing(ScenarioDetail a_sd, List<JobToInsert> a_jobsIdsToInsert, IExpediteTimesList a_insertTimes)
    {
        //Since this is a specific path, the path must be set as the default.
        for (int jobI = 0; jobI < a_jobsIdsToInsert.Count; jobI++)
        {
            Job job = a_sd.JobManager.GetById(a_jobsIdsToInsert[jobI].Id);
            for (int i = 0; i < job.ManufacturingOrderCount; i++)
            {
                AlternatePathsCollection paths = job.ManufacturingOrders[i].AlternatePaths;
                for (int pathI = 0; pathI < paths.Count; pathI++)
                {
                    if (pathI == a_insertTimes.GetCurrentItem().PathIndex)
                    {
                        job.ManufacturingOrders[i].CurrentPath_setter = paths[pathI];
                    }

                    //Each path needs to be IfCurrent so that the current path will be chosen.
                    paths[pathI].AutoUse = AlternatePathDefs.AutoUsePathEnum.IfCurrent;
                }
            }
        }
    }
}