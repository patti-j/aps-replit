using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Capability related transmissions.
/// </summary>
public abstract class CapabilityBaseT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 28;

    #region IPTSerializable Members
    public CapabilityBaseT(IReader reader)
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

    protected CapabilityBaseT() { }

    public CapabilityBaseT(BaseId scenarioId)
        : base(scenarioId) { }
}