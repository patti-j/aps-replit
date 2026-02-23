using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Cell related transmissions.
/// </summary>
public abstract class CellBaseT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 49;

    #region IPTSerializable Members
    public CellBaseT(IReader reader)
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

    protected CellBaseT() { }

    protected CellBaseT(BaseId scenarioId)
        : base(scenarioId) { }
}