using PT.APSCommon;
using PT.Common.ObjectHelpers;
using PT.Scheduler;

namespace PT.SchedulerData.ObjectKeys;

public readonly struct ResourceKey : IEquatable<ResourceKey>
{
    public readonly BaseId PlantId;
    public readonly BaseId DepartmentId;
    public readonly BaseId ResourceId;

    public ResourceKey(BaseId a_plantId, BaseId a_departmentId, BaseId a_resourceId)
    {
        PlantId = a_plantId;
        DepartmentId = a_departmentId;
        ResourceId = a_resourceId;
    }

    public ResourceKey(BaseId a_plantId, BaseId a_departmentId)
    {
        PlantId = a_plantId;
        DepartmentId = a_departmentId;
        ResourceId = BaseId.NULL_ID;
    }

    public ResourceKey(BaseId a_plantId)
    {
        PlantId = a_plantId;
        DepartmentId = BaseId.NULL_ID;
        ResourceId = BaseId.NULL_ID;
    }

    public Resource GetResource(ScenarioDetail a_sd)
    {
        Plant plant = a_sd.PlantManager.GetById(PlantId);
        Department department = plant.Departments.GetById(DepartmentId);
        return department.Resources.GetById(ResourceId);
    }

    public bool Equals(ResourceKey a_other)
    {
        return ResourceId.Value == a_other.ResourceId.Value && DepartmentId.Value == a_other.DepartmentId.Value && PlantId.Value == a_other.PlantId.Value;
    }

    public override bool Equals(object a_other)
    {
        if (a_other is ResourceKey key)
        {
            return Equals(key);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCodeHelper.GetHashCode(PlantId, DepartmentId, ResourceId);
    }
}