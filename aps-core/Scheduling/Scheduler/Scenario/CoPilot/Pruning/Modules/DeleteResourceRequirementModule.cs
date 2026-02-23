using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

/// <summary>
/// Deletes a ResourceRequirement
/// </summary>
internal class DeleteResourceRequirementModule : BaseOperationModule, PruneScenario.IPruneScenarioModule
{
    private readonly int ResourceRequirementIdx;

    public DeleteResourceRequirementModule(InternalOperation a_op, int a_rrIdx) : base(a_op)
    {
        ResourceRequirementIdx = a_rrIdx;
    }

    public bool Prune(Scenario a_scenario, IScenarioDataChanges a_dataChanges)
    {
        InternalOperation op = FindOperation(a_scenario) as InternalOperation;

        if (op != null)
        {
            op.ResourceRequirements.Requirements.RemoveByKey(op.ResourceRequirements.Requirements.GetByIndex(ResourceRequirementIdx).Id);
            return true;
        }

        return false;
    }
}