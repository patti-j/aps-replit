namespace PT.Scheduler.CoPilot.InsertJobs;

internal interface ISimulationCoreInsertPreProcessing
{
    void PerformPreProcessing(ScenarioDetail a_sd, List<JobToInsert> a_jobsIdsToInsert, IExpediteTimesList a_insertTimes);
}