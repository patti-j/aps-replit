using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

internal class DeleteAllCellsModule : PruneScenario.IPruneScenarioModule
{
    public bool Prune(Scenario a_scenario, IScenarioDataChanges a_dataChanges)
    {
        ScenarioDetail sd;
        using (a_scenario.ScenarioDetailLock.EnterWrite(out sd))
        {
            Transmissions.ScenarioDetailClearT clearT = new (a_scenario.Id);
            clearT.ClearCells = true;
            sd.PlantManager.Receive(clearT, sd, a_dataChanges);
        }

        return true;
    }
}