using PT.Scheduler.CoPilot.PruneScenario;

namespace PT.Scheduler.CoPilot.Pruning.ModuleGenerators;

internal class GenerateDeleteManufacturingOrderModules : IPruneScenarioModuleGenerator
{
    public List<IPruneScenarioModule> Generate(Scenario a_scenario)
    {
        List<IPruneScenarioModule> deleteMOModules = new ();

        ScenarioDetail sd;
        using (a_scenario.ScenarioDetailLock.EnterRead(out sd))
        {
            foreach (Job job in sd.JobManager)
            {
                if (job.ManufacturingOrders.Count > 1)
                {
                    foreach (ManufacturingOrder mo in job.ManufacturingOrders)
                    {
                        deleteMOModules.Add(new Modules.DeleteManufacturingOrderModule(mo));
                    }
                }
            }
        }

        return deleteMOModules;
    }
}