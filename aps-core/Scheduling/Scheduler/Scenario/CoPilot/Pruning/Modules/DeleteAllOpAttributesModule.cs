using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

/// <summary>
/// Deletes Attributes on all Jobs in the scenario
/// </summary>
internal class DeleteAllOpAttributesModule : PruneScenario.IPruneScenarioModule
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
                        if (op.Attributes.Count > 0)
                        {
                            op.Attributes.Clear();
                            pruned = true;
                        }
                    }
                }
            }
        }

        return pruned;
    }
}