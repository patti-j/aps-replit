using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

/// <summary>
/// Deletes a ResourceRequirement
/// </summary>
internal class DeleteAlternatePathModule : BaseManufacturingOrderModule, PruneScenario.IPruneScenarioModule
{
    private readonly BaseId m_pathId;

    public DeleteAlternatePathModule(ManufacturingOrder a_mo, AlternatePath a_path) : base(a_mo)
    {
        m_pathId = a_path.Id;
    }

    public bool Prune(Scenario a_scenario, IScenarioDataChanges a_dataChanges)
    {
        try
        {
            AlternatePath path = null;
            ManufacturingOrder mo = FindManufacturingOrder(a_scenario);
            for (int i = 0; i < mo.AlternatePaths.Count; i++)
            {
                path = mo.AlternatePaths[i];
                if (path.Id == m_pathId)
                {
                    break;
                }
            }

            if (path != null)
            {
                FindManufacturingOrder(a_scenario).AlternatePaths.Remove(path);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}