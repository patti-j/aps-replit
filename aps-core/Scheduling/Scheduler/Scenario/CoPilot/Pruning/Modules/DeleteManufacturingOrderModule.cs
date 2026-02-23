using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

/// <summary>
/// Deletes a ResourceRequirement
/// </summary>
internal class DeleteManufacturingOrderModule : BaseManufacturingOrderModule, PruneScenario.IPruneScenarioModule
{
    public DeleteManufacturingOrderModule(ManufacturingOrder a_mo) : base(a_mo) { }

    public bool Prune(Scenario a_scenario, IScenarioDataChanges a_dataChanges)
    {
        try
        {
            Transmissions.ManufacturingOrderDeleteT moDelT = new (a_scenario.Id, JobId, ManufacturingOrderId);

            ScenarioDetail sd;
            using (a_scenario.ScenarioDetailLock.EnterWrite(out sd))
            {
                sd.Receive(moDelT, a_dataChanges);
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}