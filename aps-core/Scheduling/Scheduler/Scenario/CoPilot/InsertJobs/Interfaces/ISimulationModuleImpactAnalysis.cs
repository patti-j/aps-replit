namespace PT.Scheduler.CoPilot.InsertJobs;

internal interface ISimulationModuleImpactAnalysis
{
    bool AnalyzeImpact(List<Job> a_expediteJobList, ScenarioDetail a_sd);
}