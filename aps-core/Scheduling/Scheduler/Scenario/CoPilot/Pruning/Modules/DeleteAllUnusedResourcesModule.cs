using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

internal class DeleteAllUnusedResourcesModule : PruneScenario.IPruneScenarioModule
{
    public bool Prune(Scenario a_scenario, IScenarioDataChanges a_dataChanges)
    {
        bool pruned = false;

        using (a_scenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            BaseId? lastDeletedPlant = null;

            for (int pIdx = sd.PlantManager.Count - 1; pIdx >= 0; pIdx--)
            {
                Plant p = sd.PlantManager[pIdx];
                for (int dIdx = p.DepartmentCount - 1; dIdx >= 0; dIdx--)
                {
                    Department d = p.Departments.GetByIndex(dIdx);
                    for (int rIdx = d.ResourceCount - 1; rIdx >= 0; rIdx--)
                    {
                        Resource r = d.Resources.GetByIndex(rIdx);
                        if (r.Blocks == null || r.Blocks.Count == 0)
                        {
                            d.Resources.Delete(r, a_dataChanges);
                            pruned = true;
                        }
                    }

                    if (d.ResourceCount == 0)
                    {
                        p.Departments.DeleteTry(d, a_dataChanges);
                        pruned = true;
                    }
                }

                if (p.DepartmentCount == 0)
                {
                    lastDeletedPlant = p.Id;
                    sd.PlantManager.Receive(sd, new Transmissions.PlantDeleteT(a_scenario.Id, p.Id), a_scenario, a_dataChanges);
                    pruned = true;
                }
            }

            if (lastDeletedPlant.HasValue)
            {
                sd.JobManager.AdjustEligiblePlants(new Transmissions.PlantDeleteT(a_scenario.Id, lastDeletedPlant.Value));
            }

            if (pruned)
            {
                sd.JobManager.ComputeEligibility();
            }
        }

        return pruned;
    }
}