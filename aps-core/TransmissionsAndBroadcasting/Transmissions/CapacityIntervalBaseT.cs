using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base class for all CapacityInterval related transmissions.
/// </summary>
public abstract class CapacityIntervalBaseT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 36;

    #region IPTSerializable Members
    public CapacityIntervalBaseT(IReader reader)
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

    protected CapacityIntervalBaseT() { }

    protected CapacityIntervalBaseT(BaseId scenarioId)
        : base(scenarioId) { }
}