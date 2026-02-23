using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Sets the list of Resources connected to the Capacity Interval.
/// </summary>
public class CapacityIntervalSetResourcesT : CapacityIntervalIdBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public CapacityIntervalSetResourcesT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 42)
        {
            resources = new ResourceKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        resources.Serialize(writer);
    }

    public new const int UNIQUE_ID = 474;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceKeyList resources;

    public CapacityIntervalSetResourcesT() { }

    public CapacityIntervalSetResourcesT(BaseId scenarioId, BaseId capacityIntervalId, ResourceKeyList resources)
        : base(scenarioId, new List<BaseId> { capacityIntervalId })
    {
        this.resources = resources;
    }

    public override string Description => "Capacity Interval Resources assigned";
}