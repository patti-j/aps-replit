using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData.ObjectKeys;

public readonly struct JobKey : IEquatable<JobKey>
{
    public readonly BaseId JobId;
    public readonly BaseId MOId;
    public readonly BaseId OperationId;
    public readonly BaseId ActivityId;
    public readonly BaseId MaterialRequirementId;

    public JobKey(BaseId a_jobId)
    {
        JobId = a_jobId;
        MOId = BaseId.NULL_ID;
        OperationId = BaseId.NULL_ID;
        ActivityId = BaseId.NULL_ID;
        MaterialRequirementId = BaseId.NULL_ID;
    }

    public JobKey(BlockKey a_blockKey) : this()
    {
        JobId = a_blockKey.JobId;
        MOId = a_blockKey.MOId;
        OperationId = a_blockKey.OperationId;
        ActivityId = a_blockKey.ActivityId;
        MaterialRequirementId = BaseId.NULL_ID;
    }

    public JobKey(BaseId a_jobId, BaseId a_moId)
    {
        JobId = a_jobId;
        MOId = a_moId;
        OperationId = BaseId.NULL_ID;
        ActivityId = BaseId.NULL_ID;
        MaterialRequirementId = BaseId.NULL_ID;
    }

    public JobKey(BaseId a_jobId, BaseId a_moId, BaseId a_operationId)
    {
        JobId = a_jobId;
        MOId = a_moId;
        OperationId = a_operationId;
        ActivityId = BaseId.NULL_ID;
        MaterialRequirementId = BaseId.NULL_ID;
    }

    public JobKey(BaseId a_jobId, BaseId a_moId, BaseId a_operationId, BaseId a_activityId)
    {
        JobId = a_jobId;
        MOId = a_moId;
        OperationId = a_operationId;
        ActivityId = a_activityId;
        MaterialRequirementId = BaseId.NULL_ID;
    }

    public JobKey(BaseId a_jobId, BaseId a_moId, BaseId a_operationId, BaseId a_activityId, BaseId a_materialRequirementId)
    {
        JobId = a_jobId;
        MOId = a_moId;
        OperationId = a_operationId;
        ActivityId = a_activityId;
        MaterialRequirementId = a_materialRequirementId;
    }

    public bool Equals(JobKey a_other)
    {
        return JobId.Value == a_other.JobId.Value && OperationId.Value == a_other.OperationId.Value && MOId.Value == a_other.MOId.Value && ActivityId.Value == a_other.ActivityId.Value && MaterialRequirementId.Value == a_other.MaterialRequirementId.Value;
    }

    public override bool Equals(object a_other)
    {
        if (a_other is JobKey key)
        {
            return Equals(key);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(JobId, MOId, OperationId, ActivityId, MaterialRequirementId);
    }
}