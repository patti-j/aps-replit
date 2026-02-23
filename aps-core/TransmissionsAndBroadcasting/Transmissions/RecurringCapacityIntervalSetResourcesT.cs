using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Sets the list of Resources connected to the Recurring Capacity Interval.
/// </summary>
public class RecurringCapacityIntervalSetResourcesT : RecurringCapacityIntervalIdBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public RecurringCapacityIntervalSetResourcesT(IReader reader)
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

    public new const int UNIQUE_ID = 475;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceKeyList resources;

    public RecurringCapacityIntervalSetResourcesT() { }

    public RecurringCapacityIntervalSetResourcesT(BaseId scenarioId, BaseId rciId, ResourceKeyList resources)
        : base(scenarioId, new List<BaseId> { rciId })
    {
        this.resources = resources;
    }

    public override string Description => "Capacity(Recurring) Resources assigned";
}