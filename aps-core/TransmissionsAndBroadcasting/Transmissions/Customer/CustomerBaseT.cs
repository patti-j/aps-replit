using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Customer related transmissions.
/// </summary>
public abstract class CustomerBaseT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 57;

    #region IPTSerializable Members
    public CustomerBaseT(IReader reader)
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

    protected CustomerBaseT() { }

    protected CustomerBaseT(BaseId scenarioId)
        : base(scenarioId) { }
}