using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes all CapacityIntervals in the specified Scenario.
/// </summary>
public class CapacityIntervalDeleteAllT : CapacityIntervalBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 41;

    #region IPTSerializable Members
    public CapacityIntervalDeleteAllT(IReader reader)
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

    public CapacityIntervalDeleteAllT() { }

    public CapacityIntervalDeleteAllT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "All Capacity deleted";
}