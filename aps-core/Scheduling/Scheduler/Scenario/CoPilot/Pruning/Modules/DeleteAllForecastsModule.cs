using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

/// <summary>
/// Delete all Forecasts in the system
/// </summary>
internal class DeleteAllForecastsModule : PruneScenario.IPruneScenarioModule
{
    public bool Prune(Scenario a_scenario, IScenarioDataChanges a_dataChanges)
    {
        ScenarioDetail sd;
        using (a_scenario.ScenarioDetailLock.EnterWrite(out sd))
        {
            Transmissions.ScenarioDetailClearT clearT = new (a_scenario.Id);
            clearT.ClearForecasts = true;
            sd.Receive(clearT, a_dataChanges);
        }

        return true;
    }
}