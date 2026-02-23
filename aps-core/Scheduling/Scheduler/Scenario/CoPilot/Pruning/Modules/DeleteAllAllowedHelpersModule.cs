using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

internal class DeleteAllAllowedHelpersModule : PruneScenario.IPruneScenarioModule
{
    public bool Prune(Scenario a_scenario, IScenarioDataChanges a_dataChanges)
    {
        using (a_scenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            Transmissions.ScenarioDetailClearT clearT = new (a_scenario.Id);
            clearT.ClearAllowedHelpers = true;
            sd.PlantManager.Receive(clearT, sd, a_dataChanges);
        }

        return true;
    }
}