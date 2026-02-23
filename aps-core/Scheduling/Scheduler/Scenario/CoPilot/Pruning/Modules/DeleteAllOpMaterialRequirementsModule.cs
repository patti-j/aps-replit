using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

/// <summary>
/// Deletes MaterialRequirements on all Jobs in the scenario
/// </summary>
internal class DeleteAllOpMaterialRequirementsModule : PruneScenario.IPruneScenarioModule
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
                        if (op.MaterialRequirements.Count > 0)
                        {
                            op.MaterialRequirements.Clear();
                            pruned = true;
                        }
                    }
                }
            }
        }

        return pruned;
    }
}