using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

internal class DeleteAllSalesOrdersModule : PruneScenario.IPruneScenarioModule
{
    public bool Prune(Scenario a_scenario, IScenarioDataChanges a_dataChanges)
    {
        using (a_scenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            if (sd.SalesOrderManager.Count > 0)
            {
                Transmissions.ScenarioDetailClearT clearT = new (a_scenario.Id);
                clearT.ClearSalesOrders = true;
                sd.Receive(clearT, a_dataChanges);
                return true;
            }
        }

        return false;
    }
}