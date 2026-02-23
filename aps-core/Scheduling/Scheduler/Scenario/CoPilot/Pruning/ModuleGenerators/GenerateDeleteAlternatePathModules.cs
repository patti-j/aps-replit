using PT.Scheduler.CoPilot.PruneScenario;

namespace PT.Scheduler.CoPilot.Pruning.ModuleGenerators;

internal class GenerateDeleteAlternatePathModules : IPruneScenarioModuleGenerator
{
    public List<IPruneScenarioModule> Generate(Scenario a_scenario)
    {
        List<IPruneScenarioModule> delPathMods = new ();

        ScenarioDetail sd;
        using (a_scenario.ScenarioDetailLock.EnterRead(out sd))
        {
            foreach (Job job in sd.JobManager)
            {
                foreach (ManufacturingOrder mo in job.ManufacturingOrders)
                {
                    if (mo.AlternatePaths.Count > 1)
                    {
                        for (int i = 0; i < mo.AlternatePaths.Count; i++)
                        {
                            delPathMods.Add(new Modules.DeleteAlternatePathModule(mo, mo.AlternatePaths[i]));
                        }
                    }
                }
            }
        }

        return delPathMods;
    }
}