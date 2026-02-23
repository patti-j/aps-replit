using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Put a Scenario in Online Mode.
/// </summary>
public class ScenarioDetailOnlineT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 726;

    #region IPTSerializable Members
    public ScenarioDetailOnlineT(IReader reader)
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

    public ScenarioDetailOnlineT() { }

    public ScenarioDetailOnlineT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "Switched to online mode";
}