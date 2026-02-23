using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Update an existing table.
/// </summary>
public class CompatibilityCodeTableUpdateT : CompatibilityCodeTableIdBaseT
{
    #region IPTSerializable Members
    public CompatibilityCodeTableUpdateT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            compatibilityCodeTable = new CompatibilityCodeTable(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        compatibilityCodeTable.Serialize(writer);
    }

    public new const int UNIQUE_ID = 5052;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CompatibilityCodeTableUpdateT() { }

    public CompatibilityCodeTableUpdateT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId, tableId) { }

    private CompatibilityCodeTable compatibilityCodeTable = new();

    public CompatibilityCodeTable CompatibilityCodeTable
    {
        get => compatibilityCodeTable;
        set => compatibilityCodeTable = value;
    }
}