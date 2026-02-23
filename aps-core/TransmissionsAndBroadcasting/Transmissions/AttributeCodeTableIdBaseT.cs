using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base class for all AttributeCodeTable related Transmissions.
/// </summary>
public abstract class AttributeCodeTableIdBaseT : AttributeCodeTableBaseT
{
    #region IPTSerializable Members
    public AttributeCodeTableIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            id = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        id.Serialize(writer);
    }

    public new const int UNIQUE_ID = 564;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected AttributeCodeTableIdBaseT() { }

    protected AttributeCodeTableIdBaseT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId)
    {
        id = tableId;
    }

    private readonly BaseId id;

    public BaseId Id => id;
}