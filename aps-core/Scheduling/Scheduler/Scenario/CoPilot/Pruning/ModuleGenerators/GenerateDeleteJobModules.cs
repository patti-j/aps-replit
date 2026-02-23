namespace PT.Scheduler.CoPilot.Pruning.ModuleGenerators;

//internal class GenerateDeleteJobModules : IPruneScenarioModuleGenerator
//{
//    public List<IPruneScenarioModule> Generate(Scenario a_scenario)
//    {
//        List<IPruneScenarioModule> deleteJobModules = new List<IPruneScenarioModule>();

//        ScenarioDetail sd;
//        using (a_scenario.ScenarioDetailLock.EnterRead(out sd))
//        {
//            foreach(Job job in sd.JobManager)
//            {
//                deleteJobModules.Add(new Modules.DeleteJobModule());
//            }
//        }

//        return deleteJobModules;
//    }
//}