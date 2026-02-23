using PT.APSCommon;
using PT.Common.ObjectHelpers;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData.ObjectKeys;

public readonly struct ActivityKey : IEquatable<ActivityKey>
{
    public readonly BaseId JobId;
    public readonly BaseId MOId;
    public readonly BaseId OperationId;
    public readonly BaseId ActivityId;
    public readonly int RequirementId;
    public readonly BaseId BatchId;
    public readonly BaseId ResBlockId;
    public readonly BaseId PlantId;
    public readonly BaseId DepartmentId;
    public readonly BaseId ResourceId;

    public static ActivityKey NullKey = new (BaseId.NULL_ID, BaseId.NULL_ID, BaseId.NULL_ID, BaseId.NULL_ID);

    public ActivityKey(BlockKey a_blockKey) : this()
    {
        JobId = a_blockKey.JobId;
        MOId = a_blockKey.MOId;
        OperationId = a_blockKey.OperationId;
        ActivityId = a_blockKey.ActivityId;
        RequirementId = -1;
        BatchId = BaseId.NULL_ID;
        ResBlockId = BaseId.NULL_ID;
        PlantId = BaseId.NULL_ID;
        DepartmentId = BaseId.NULL_ID;
    }

    public ActivityKey(BaseId a_jobId, BaseId a_moId, BaseId a_operationId, BaseId a_activityId)
    {
        JobId = a_jobId;
        MOId = a_moId;
        OperationId = a_operationId;
        ActivityId = a_activityId;
        RequirementId = -1;
        BatchId = BaseId.NULL_ID;
        ResBlockId = BaseId.NULL_ID;
        PlantId = BaseId.NULL_ID;
        DepartmentId = BaseId.NULL_ID;
        ResourceId = BaseId.NULL_ID;
    }

    public ActivityKey(BaseId a_jobId, BaseId a_moId, BaseId a_operationId, BaseId a_activityId, int a_requirementId, BaseId a_batchId, BaseId a_resBlockId, BaseId a_plantId, BaseId a_departmentId, BaseId a_resourceId)
    {
        JobId = a_jobId;
        MOId = a_moId;
        OperationId = a_operationId;
        ActivityId = a_activityId;
        RequirementId = a_requirementId;
        BatchId = a_batchId;
        ResBlockId = a_resBlockId;
        PlantId = a_plantId;
        DepartmentId = a_departmentId;
        ResourceId = a_resourceId;
    }

    public ActivityKey(BaseId a_plantId, BaseId a_departmentId, BaseId a_resourceId)
    {
        JobId = BaseId.NULL_ID;
        MOId = BaseId.NULL_ID;
        OperationId = BaseId.NULL_ID;
        ActivityId = BaseId.NULL_ID;
        RequirementId = -1;
        BatchId = BaseId.NULL_ID;
        ResBlockId = BaseId.NULL_ID;
        PlantId = a_plantId;
        DepartmentId = a_departmentId;
        ResourceId = a_resourceId;
    }

    public bool Equals(ActivityKey a_other)
    {
        return OperationId.Value == a_other.OperationId.Value && JobId.Value == a_other.JobId.Value && ActivityId.Value == a_other.ActivityId.Value && MOId.Value == a_other.MOId.Value && BatchId.Value == a_other.BatchId.Value && ResBlockId.Value == a_other.ResBlockId.Value && PlantId.Value == a_other.PlantId.Value && DepartmentId.Value == a_other.DepartmentId.Value && ResourceId.Value == a_other.ResourceId.Value;
    }

    public override bool Equals(object a_other)
    {
        if (a_other is ActivityKey key)
        {
            return Equals(key);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCodeHelper.GetHashCode(JobId, MOId, OperationId);
    }
}