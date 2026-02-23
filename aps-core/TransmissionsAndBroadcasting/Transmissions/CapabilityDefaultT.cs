using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new Capability in the specified Scenario using default values.
/// </summary>
public class CapabilityDefaultT : CapabilityBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 31;

    #region IPTSerializable Members
    public CapabilityDefaultT(IReader reader)
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

    public CapabilityDefaultT() { }

    public CapabilityDefaultT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "Capability Created";
}