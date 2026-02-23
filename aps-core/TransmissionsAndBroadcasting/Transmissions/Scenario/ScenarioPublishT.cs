namespace PT.Transmissions;

/// <summary>
/// Copy the Live Scenario and mark it as "Published".
/// </summary>
public class ScenarioPublishT : ScenarioBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 138;

    #region IPTSerializable Members
    public ScenarioPublishT(IReader reader)
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

    public ScenarioPublishT() { }
}