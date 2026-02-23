using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Put a Scenario in Offline Mode.
/// </summary>
public class ScenarioDetailOfflineT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 725;

    #region IPTSerializable Members
    public ScenarioDetailOfflineT(IReader reader)
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

    public ScenarioDetailOfflineT() { }

    public ScenarioDetailOfflineT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "Switched to offline mode";
}