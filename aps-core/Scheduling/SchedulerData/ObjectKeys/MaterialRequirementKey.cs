using PT.APSCommon;
using PT.Common.ObjectHelpers;

namespace PT.SchedulerData.ObjectKeys;

public struct MaterialRequirementKey
{
    public readonly BaseId JobId;
    public readonly BaseId MOId;
    public readonly BaseId OperationId;
    public readonly BaseId ActivityId;
    public readonly BaseId RequirementId;
    public readonly BaseId PlantId;
    public readonly BaseId WarehouseId;
    public readonly BaseId ItemId;

    public MaterialRequirementKey(BaseId a_jobId, BaseId a_moId, BaseId a_operationId, BaseId a_activityId, BaseId a_requirementId, BaseId a_plantId, BaseId a_warehouseId, BaseId a_itemId)
    {
        JobId = a_jobId;
        MOId = a_moId;
        OperationId = a_operationId;
        ActivityId = a_activityId;
        RequirementId = a_requirementId;
        PlantId = a_plantId;
        WarehouseId = a_warehouseId;
        ItemId = a_itemId;
    }

    public override bool Equals(object a_other)
    {
        if (a_other is MaterialRequirementKey key)
        {
            return JobId.Value == key.JobId.Value && ItemId.Value == key.ItemId.Value && OperationId.Value == key.OperationId.Value && PlantId.Value == key.PlantId.Value && WarehouseId.Value == key.WarehouseId.Value && MOId.Value == key.MOId.Value && ActivityId.Value == key.ActivityId.Value && RequirementId.Value == key.RequirementId.Value;
        }

        return false;
    }

    public override int GetHashCode()
    {
        //Don't have to worry much about activities and other properties since these ids are unique
        return HashCodeHelper.GetHashCode(JobId, OperationId, WarehouseId, ItemId);
    }
}