using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base class for all AttributeCodeTable related Transmissions.
/// </summary>
public abstract class AttributeCodeTableBaseT : ScenarioIdBaseT
{
    #region IPTSerializable Members
    public AttributeCodeTableBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public const int UNIQUE_ID = 568;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected AttributeCodeTableBaseT() { }

    protected AttributeCodeTableBaseT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "Attribute Code Table updated";
}