using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes the CapacityInterval.
/// </summary>
public class CapacityIntervalDeleteT : CapacityIntervalIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 42;

    #region IPTSerializable Members
    public CapacityIntervalDeleteT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CapacityIntervalDeleteT() { }

    public CapacityIntervalDeleteT(BaseId scenarioId, List<BaseId> a_capacityIntervalId)
        : base(scenarioId, a_capacityIntervalId) { }

    public override string Description => "Capacity Interval deleted";
}