using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes all Cells in the specified Scenario (and all of their Resources).
/// </summary>
public class CellDeleteAllT : CellBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 53;

    #region IPTSerializable Members
    public CellDeleteAllT(IReader reader)
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

    public CellDeleteAllT() { }

    public CellDeleteAllT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "All Cells deleted";
}