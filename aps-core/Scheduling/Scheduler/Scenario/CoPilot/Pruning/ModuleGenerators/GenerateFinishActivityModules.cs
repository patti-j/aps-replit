using PT.Scheduler.CoPilot.PruneScenario;

namespace PT.Scheduler.CoPilot.Pruning.ModuleGenerators;

internal class GenerateFinishActivityModules : IPruneScenarioModuleGenerator
{
    public List<IPruneScenarioModule> Generate(Scenario a_scenario)
    {
        List<IPruneScenarioModule> finishActMods = new ();

        ScenarioDetail sd;
        using (a_scenario.ScenarioDetailLock.EnterRead(out sd))
        {
            foreach (Job job in sd.JobManager)
            {
                foreach (ManufacturingOrder mo in job.ManufacturingOrders)
                {
                    for (int i = 0; i < mo.OperationManager.Count; i++)
                    {
                        InternalOperation op = mo.OperationManager.GetByIndex(i) as InternalOperation;
                        if (op == null)
                        {
                            continue;
                        }

                        for (int aIdx = 0; aIdx < op.Activities.Count; aIdx++)
                        {
                            BaseActivity act = op.Activities.GetByIndex(aIdx);
                            finishActMods.Add(new Modules.FinishActivityModule(op, act));
                        }
                    }
                }
            }
        }

        return finishActMods;
    }
}