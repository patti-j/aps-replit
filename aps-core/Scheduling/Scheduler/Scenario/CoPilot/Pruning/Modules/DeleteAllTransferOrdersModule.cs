using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

internal class DeleteAllTransferOrdersModule : PruneScenario.IPruneScenarioModule
{
    public bool Prune(Scenario a_scenario, IScenarioDataChanges a_dataChanges)
    {
        using (a_scenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            if (sd.TransferOrderManager.Count > 0)
            {
                Transmissions.ScenarioDetailClearT clearT = new (a_scenario.Id);
                clearT.ClearTransferOrders = true;
                sd.Receive(clearT, a_dataChanges);
                return true;
            }
        }

        return false;
    }
}