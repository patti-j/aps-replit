using PT.Scheduler.PackageDefs;

namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// For each job in the set, find its release date. Return the latest release date time.
/// </summary>
internal class TimeCalculatorModuleReleaseDates : IConstraintTimeCalculatorModule
{
    public long CalculateMinTime(Scenario a_workingScenario,
                                 List<JobToInsert> a_jobsToExpedite,
                                 byte[] a_sdByteArray,
                                 byte[] a_ssByteArray,
                                 IPackageManager a_packageManager)
    {
        ScenarioDetail newSd;
        using (a_workingScenario.ScenarioDetailLock.EnterRead(out newSd))
        {
            long latestReleaseTicks = 0;
            for (int i = 0; i < a_jobsToExpedite.Count; i++)
            {
                long currentReleaseTicks = 0;
                Job jobToExpedite = newSd.JobManager.GetById(a_jobsToExpedite[i].Id);

                currentReleaseTicks = jobToExpedite.GetMaxMoReleaseDateTicks();

                latestReleaseTicks = Math.Max(latestReleaseTicks, currentReleaseTicks);
            }

            return latestReleaseTicks;
        }
    }

    public long CalculateMaxTime(Scenario a_workingScenario,
                                 List<JobToInsert> a_jobIdToExpedite,
                                 byte[] a_sdByteArray,
                                 byte[] a_ssByteArray)
    {
        return long.MaxValue;
    }
}