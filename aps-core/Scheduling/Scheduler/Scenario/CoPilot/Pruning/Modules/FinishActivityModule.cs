using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

/// <summary>
/// Deletes a ResourceRequirement
/// </summary>
internal class FinishActivityModule : BaseOperationModule, PruneScenario.IPruneScenarioModule
{
    private readonly BaseId m_actId;

    public FinishActivityModule(BaseOperation a_op, BaseActivity a_act) : base(a_op)
    {
        m_actId = a_act.Id;
    }

    public bool Prune(Scenario a_scenario, IScenarioDataChanges a_dataChanges)
    {
        try
        {
            BaseOperation op = FindOperation(a_scenario);
            Transmissions.InternalActivityFinishT actFinishT = new (a_scenario.Id, JobId, ManufacturingOrderId, OperationId, m_actId);

            using (a_scenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
            {
                sd.Receive(actFinishT, a_dataChanges);
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}