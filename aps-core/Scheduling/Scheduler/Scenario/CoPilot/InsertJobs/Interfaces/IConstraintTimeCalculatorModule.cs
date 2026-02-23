using PT.Scheduler.PackageDefs;

namespace PT.Scheduler.CoPilot.InsertJobs;

internal interface IConstraintTimeCalculatorModule
{
    long CalculateMinTime(Scenario a_workingScenario,
                          List<JobToInsert> a_jobsToExpedite,
                          byte[] a_sdByteArray,
                          byte[] a_ssByteArray,
                          IPackageManager a_packageManager);

    long CalculateMaxTime(Scenario a_workingScenario,
                          List<JobToInsert> a_jobsToExpedite,
                          byte[] a_sdByteArray,
                          byte[] a_ssByteArray);
}