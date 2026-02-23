using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

public class DeleteAllSetupTablesModule : PruneScenario.IPruneScenarioModule
{
    public bool Prune(Scenario a_scenario, IScenarioDataChanges a_dataChanges)
    {
        using (a_scenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            if (sd.FromRangeSetManager.Count > 0 || sd.AttributeCodeTableManager.Count > 0)
            {
                Transmissions.ScenarioDetailClearT clearT = new (a_scenario.Id);
                clearT.ClearAttributeNumberRangeTables = true;
                clearT.ClearAttributeCodeTables = true;
                sd.Receive(clearT, a_dataChanges);
                return true;
            }
        }

        return false;
    }
}