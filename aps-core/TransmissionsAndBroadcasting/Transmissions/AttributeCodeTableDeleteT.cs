using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Delete a specific table.
/// </summary>
public class AttributeCodeTableDeleteT : AttributeCodeTableIdBaseT
{
    #region IPTSerializable Members
    public AttributeCodeTableDeleteT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public new const int UNIQUE_ID = 565;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public AttributeCodeTableDeleteT() { }

    public AttributeCodeTableDeleteT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId, tableId) { }
}