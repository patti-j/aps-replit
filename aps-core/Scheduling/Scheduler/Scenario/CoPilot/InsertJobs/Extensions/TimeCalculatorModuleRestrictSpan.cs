using PT.Scheduler.PackageDefs;

namespace PT.Scheduler.CoPilot.InsertJobs;

/// <summary>
/// Return the frozen span ticks. InsertJobs TODO: there may be a time conversion error somewhere. On the gantt this local time value is incorrect.
/// </summary>
internal class TimeCalculatorModuleFrozenSpan : IConstraintTimeCalculatorModule
{
    public long CalculateMinTime(Scenario a_workingScenario,
                                 List<JobToInsert> a_jobIdToExpedite,
                                 byte[] a_sdByteArray,
                                 byte[] a_ssByteArray,
                                 IPackageManager a_packageManager)
    {
        using (a_workingScenario.ScenarioDetailLock.EnterRead(out ScenarioDetail newSd))
        {
            return newSd.PlantManager.GetEarliestFrozenSpanEnd(newSd.ClockDate.Ticks);
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