using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base class for all SetupCodeTable related Transmissions.
/// </summary>
public abstract class CompatibilityCodeTableIdBaseT : CompatibilityCodeTableBaseT
{
    #region IPTSerializable Members
    public CompatibilityCodeTableIdBaseT(IReader reader)
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

    public new const int UNIQUE_ID = 5051;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected CompatibilityCodeTableIdBaseT() { }

    protected CompatibilityCodeTableIdBaseT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId)
    {
        id = tableId;
    }

    private readonly BaseId id;

    public BaseId Id => id;
}