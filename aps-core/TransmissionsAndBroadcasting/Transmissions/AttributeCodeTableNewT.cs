using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Create a new table.
/// </summary>
public class AttributeCodeTableNewT : AttributeCodeTableBaseT
{
    #region IPTSerializable Members
    public AttributeCodeTableNewT(IReader reader)
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

    public new const int UNIQUE_ID = 563;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public AttributeCodeTableNewT() { }

    public AttributeCodeTableNewT(BaseId scenarioId)
        : base(scenarioId) { }

    private AttributeCodeTable attributeCodeTable = new ();

    public AttributeCodeTable AttributeCodeTable
    {
        get => attributeCodeTable;
        set => attributeCodeTable = value;
    }
}