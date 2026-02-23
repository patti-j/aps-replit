using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Cell related transmissions.
/// </summary>
public abstract class CellIdBaseT : CellBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 55;

    #region IPTSerializable Members
    public CellIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            cellId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        cellId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId cellId;

    protected CellIdBaseT() { }

    protected CellIdBaseT(BaseId scenarioId, BaseId cellId)
        : base(scenarioId)
    {
        this.cellId = cellId;
    }
}