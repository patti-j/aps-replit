using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Update an existing table.
/// </summary>
public class AttributeCodeTableUpdateT : AttributeCodeTableIdBaseT
{
    #region IPTSerializable Members
    public AttributeCodeTableUpdateT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            attributeCodeTable = new AttributeCodeTable(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        attributeCodeTable.Serialize(writer);
    }

    public new const int UNIQUE_ID = 569;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public AttributeCodeTableUpdateT() { }

    public AttributeCodeTableUpdateT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId, tableId) { }

    private AttributeCodeTable attributeCodeTable = new ();

    public AttributeCodeTable AttributeCodeTable
    {
        get => attributeCodeTable;
        set => attributeCodeTable = value;
    }
}