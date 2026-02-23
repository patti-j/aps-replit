using PT.Scheduler.PackageDefs;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// This module will schedule each path of a job to find the earliest date that the job can be scheduled.  Paths with preference 0 will be skipped.
/// There is some error here since all paths may not have the same earliest schedule time.
/// As a result of this, some times may be added that are not possible to schedule to.
/// </summary>
internal class TimeCalculatorModuleAlternatePaths : IConstraintTimeCalculatorModule
{
    public long CalculateMinTime(Scenario a_workingScenario,
                                 List<JobToInsert> a_jobsToExpedite,
                                 byte[] a_sdByteArray,
                                 byte[] a_ssByteArray,
                                 IPackageManager a_packageManager)
    {
        long earliestExpediteTime = long.MaxValue;
        ScenarioDetail newSd;
        CoPilotSimulationUtilities simUtilities = new (a_packageManager);
        List<int> pathIdxList = new ();

        //Fist get the pathID of all paths that should be scheduled.
        using (a_workingScenario.ScenarioDetailLock.EnterRead(out newSd))
        {
            Job currentJob = newSd.JobManager.GetById(a_jobsToExpedite[0].Id);
            for (int moI = 0; moI < currentJob.ManufacturingOrderCount; moI++)
            {
                for (int pathI = 0; pathI < currentJob.ManufacturingOrders[moI].AlternatePaths.Count; pathI++)
                {
                    AlternatePath path = currentJob.ManufacturingOrders[moI].AlternatePaths[pathI];
                    if (path.Preference > 0)
                    {
                        //This job has at least one path to expedite.
                        pathIdxList.Add(pathI);
                    }
                }
            }
        }

        //Create a new scenario for each expedite
        for (int pI = 0; pI < pathIdxList.Count; pI++)
        {
            Scenario newScenario = simUtilities.CopySimScenario(a_sdByteArray, a_ssByteArray);
            using (newScenario.ScenarioDetailLock.EnterWrite(out newSd))
            {
                for (int jobI = 0; jobI < a_jobsToExpedite.Count; jobI++)
                {
                    Job currentJob = newSd.JobManager.GetById(a_jobsToExpedite[jobI].Id);
                    //Set all paths to if current. Set the current path accordingly.
                    for (int moI = 0; moI < currentJob.ManufacturingOrderCount; moI++)
                    {
                        AlternatePathsCollection paths = currentJob.ManufacturingOrders[moI].AlternatePaths;
                        for (int pathI = 0; pathI < paths.Count; pathI++)
                        {
                            if (pathI == pathIdxList[pI])
                            {
                                currentJob.ManufacturingOrders[moI].CurrentPath_setter = (AlternatePath)paths.GetRow(pathI);
                                //currentJob.ManufacturingOrders[moI].DefaultPath = (AlternatePath)paths.GetByIndex(pathI);
                            }

                            //Each path needs to be IfCurrent so that the current path will be chosen.
                            paths[pathI].AutoUse = AlternatePathDefs.AutoUsePathEnum.IfCurrent;
                        }
                    }
                }

                //Expedite the job
                Transmissions.ScenarioDetailExpediteJobsT expediteT = simUtilities.CreateExpediteT(a_jobsToExpedite, newSd.Clock);
                try
                {
                    newSd.ExpediteJobs(expediteT, new ScenarioDataChanges());
                }
                catch
                {
                    //Keep trying the other paths incase one of them can schedule
                }

                //Check for job start date. Use the earliest date.
                long latestJobStartTime = 0;
                for (int j = 0; j < a_jobsToExpedite.Count; j++)
                {
                    Job currentJob = newSd.JobManager.GetById(a_jobsToExpedite[j].Id);
                    latestJobStartTime = Math.Max(latestJobStartTime, currentJob.ScheduledStartDate.Ticks);
                }

                earliestExpediteTime = Math.Min(earliestExpediteTime, latestJobStartTime);
            }

            newScenario.Dispose();
        }

        return earliestExpediteTime;
    }

    public long CalculateMaxTime(Scenario a_workingScenario,
                                 List<JobToInsert> a_jobsToExpedite,
                                 byte[] a_sdByteArray,
                                 byte[] a_ssByteArray)
    {
        return long.MaxValue;
    }
}