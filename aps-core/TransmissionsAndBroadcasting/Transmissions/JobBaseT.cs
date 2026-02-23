using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Job related transmissions.
/// </summary>
public abstract class JobBaseT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 73;

    #region IPTSerializable Members
    public JobBaseT(IReader reader)
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

    protected JobBaseT() { }

    protected JobBaseT(BaseId scenarioId)
        : base(scenarioId) { }
}