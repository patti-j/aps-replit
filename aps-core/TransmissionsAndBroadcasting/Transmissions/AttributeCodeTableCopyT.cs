using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Copy a specific Table.
/// </summary>
public class AttributeCodeTableCopyT : AttributeCodeTableIdBaseT
{
    #region IPTSerializable Members
    public AttributeCodeTableCopyT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public new const int UNIQUE_ID = 567;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public AttributeCodeTableCopyT() { }

    public AttributeCodeTableCopyT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId, tableId) { }
}