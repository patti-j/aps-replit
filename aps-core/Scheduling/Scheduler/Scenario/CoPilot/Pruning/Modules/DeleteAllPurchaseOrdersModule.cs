using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

/// <summary>
/// Delete all purchase orders in the system
/// </summary>
internal class DeleteAllPurchaseOrdersModule : PruneScenario.IPruneScenarioModule
{
    public bool Prune(Scenario a_scenario, IScenarioDataChanges a_dataChanges)
    {
        using (a_scenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            if (sd.PurchaseToStockManager.Count > 0)
            {
                Transmissions.ScenarioDetailClearT clearT = new (a_scenario.Id);
                clearT.ClearPurchaseToStocks = true;
                sd.Receive(clearT, a_dataChanges);
                return true;
            }
        }

        return false;
    }
}