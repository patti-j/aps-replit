using PT.Scheduler.PackageDefs;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// The default TimeCalculator module.
/// </summary>
internal class TimeCalculatorModule : IConstraintTimeCalculatorModule
{
    /// <summary>
    /// Expedites the job and returns its start date.
    /// </summary>
    public long CalculateMinTime(Scenario a_workingScenario,
                                 List<JobToInsert> a_jobIdToExpedite,
                                 byte[] a_sdByteArray,
                                 byte[] a_ssByteArray,
                                 IPackageManager a_packageManager)
    {
        long earliestExpediteTime = long.MaxValue;
        CoPilotSimulationUtilities simUtilities = new (a_packageManager);
        Scenario newScenario = simUtilities.CopySimScenario(a_sdByteArray, a_ssByteArray);
        ScenarioDetail newSd;
        using (newScenario.ScenarioDetailLock.EnterWrite(out newSd))
        {
            ScenarioDetailExpediteJobsT expediteT = simUtilities.CreateExpediteT(a_jobIdToExpedite, newSd.Clock);
            newSd.ExpediteJobs(expediteT, new ScenarioDataChanges());

            //Check for job start date
            for (int i = 0; i < a_jobIdToExpedite.Count; i++)
            {
                Job currentJob = newSd.JobManager.GetById(a_jobIdToExpedite[i].Id);
                earliestExpediteTime = Math.Min(earliestExpediteTime, currentJob.ScheduledStartDate.Ticks);
            }
        }

        newScenario.Dispose();
        return earliestExpediteTime;
    }

    /// <summary>
    /// Returns the planning horizon.
    /// </summary>
    public long CalculateMaxTime(Scenario a_workingScenario,
                                 List<JobToInsert> a_jobIdToExpedite,
                                 byte[] a_sdByteArray,
                                 byte[] a_ssByteArray)
    {
        ScenarioDetail newSd;
        using (a_workingScenario.ScenarioDetailLock.EnterRead(out newSd))
        {
            return newSd.GetPlanningHorizonEnd().Ticks;
        }
    }
}