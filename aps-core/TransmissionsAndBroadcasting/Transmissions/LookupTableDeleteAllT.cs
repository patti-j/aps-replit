using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Create a new table.
/// </summary>
public class LookupTableDeleteAllT : ScenarioIdBaseT
{
    #region IPTSerializable Members
    public LookupTableDeleteAllT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public const int UNIQUE_ID = 562;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public LookupTableDeleteAllT() { }

    public LookupTableDeleteAllT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "All Data Tables deleted";
}