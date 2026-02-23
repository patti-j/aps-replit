using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Creates a new Cell in the specified Scenario using default values.
/// </summary>
public class CellDefaultT : CellBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 52;

    #region IPTSerializable Members
    public CellDefaultT(IReader reader)
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

    public CellDefaultT() { }

    public CellDefaultT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "Cell Created".Localize();
}