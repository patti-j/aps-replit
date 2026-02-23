using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes all Capabilitys in the specified Scenario (and all of their Resources).
/// </summary>
public class CapabilityDeleteAllT : CapabilityBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 32;

    #region IPTSerializable Members
    public CapabilityDeleteAllT(IReader reader)
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

    public CapabilityDeleteAllT() { }

    public CapabilityDeleteAllT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "All Capabilities deleted";
}