using PT.Scheduler.PackageDefs;

namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// The default TimeCalculator module.
/// </summary>
internal class TimeCalculatorModulePhase1 : IConstraintTimeCalculatorModule
{
    /// <summary>
    /// Returns max value if any of the jobs cannot be scheduled on time. This should only be used for phase 1.
    /// </summary>
    public long CalculateMinTime(Scenario a_workingScenario,
                                 List<JobToInsert> a_jobIdToExpedite,
                                 byte[] a_sdByteArray,
                                 byte[] a_ssByteArray,
                                 IPackageManager a_packageManager)
    {
        ScenarioDetail newSd;
        using (a_workingScenario.ScenarioDetailLock.EnterRead(out newSd))
        {
            for (int i = 0; i < a_jobIdToExpedite.Count; i++)
            {
                Job currentJob = newSd.JobManager.GetById(a_jobIdToExpedite[i].Id);
                if (currentJob.NeedDateTicks < newSd.Clock)
                {
                    return long.MaxValue;
                }
            }
        }

        return 0;
    }

    /// <summary>
    /// Returns the default max value
    /// </summary>
    public long CalculateMaxTime(Scenario a_workingScenario,
                                 List<JobToInsert> a_jobIdToExpedite,
                                 byte[] a_sdByteArray,
                                 byte[] a_ssByteArray)
    {
        return long.MaxValue;
    }
}