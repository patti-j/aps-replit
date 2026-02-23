using PT.Scheduler.CoPilot.PruneScenario;

namespace PT.Scheduler.CoPilot.Pruning.ModuleGenerators;

/// <summary>
/// Creates modules for deleting helper ResourceRequirements for all Jobs in the Scenario
/// </summary>
internal class GenerateDeleteHelperResourceRequirementModules : IPruneScenarioModuleGenerator
{
    public List<IPruneScenarioModule> Generate(Scenario a_scenario)
    {
        List<IPruneScenarioModule> deleteHelperRRModules = new ();

        using (a_scenario.ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
        {
            foreach (Job job in sd.JobManager)
            {
                foreach (BaseOperation baseOp in job.GetOperations())
                {
                    if (baseOp is InternalOperation op)
                    {
                        for (int i = 0; i < op.ResourceRequirements.Count; i++)
                        {
                            if (i != op.ResourceRequirements.PrimaryResourceRequirementIndex)
                            {
                                deleteHelperRRModules.Add(new Modules.DeleteResourceRequirementModule(op, i));
                            }
                        }
                    }
                }
            }
        }

        return deleteHelperRRModules;
    }
}