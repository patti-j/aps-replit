using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

/// <summary>
/// Deletes Products on all Jobs in the scenario
/// </summary>
internal class DeleteAllOpHelperResourceRequirementsModule : PruneScenario.IPruneScenarioModule
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
                        InternalOperation op = mo.OperationManager.GetByIndex(i) as InternalOperation;
                        if (op != null && op.ResourceRequirements.Count > 1)
                        {
                            for (int rIdx = op.ResourceRequirements.Count - 1; rIdx >= 0; rIdx--)
                            {
                                if (rIdx != 0)
                                {
                                    op.ResourceRequirements.Requirements.RemoveByKey(op.ResourceRequirements.Requirements.GetByIndex(rIdx).Id);
                                    pruned = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        return pruned;
    }
}