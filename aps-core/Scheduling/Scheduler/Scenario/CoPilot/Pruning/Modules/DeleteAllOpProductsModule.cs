using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

/// <summary>
/// Deletes Products on all Jobs in the scenario
/// </summary>
internal class DeleteAllOpProductsModule : PruneScenario.IPruneScenarioModule
{
    public bool Prune(Scenario a_scenario, IScenarioDataChanges a_dataChanges)
    {
        bool pruned = false;

        using (a_scenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            foreach (Job j in sd.JobManager)
            {
                foreach (ManufacturingOrder mo in j.ManufacturingOrders)
                {
                    for (int i = 0; i < mo.OperationCount; i++)
                    {
                        BaseOperation op = mo.OperationManager.GetByIndex(i);
                        if (op.Products.Count > 0)
                        {
                            op.Products.Clear();
                            pruned = true;
                        }
                    }
                }
            }
        }

        return pruned;
    }
}