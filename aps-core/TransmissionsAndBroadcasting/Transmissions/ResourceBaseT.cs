using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Resource related transmissions.
/// </summary>
public abstract class ResourceBaseT : BaseResourceT, IPTSerializable
{
    public new const int UNIQUE_ID = 111;

    #region IPTSerializable Members
    public ResourceBaseT(IReader reader)
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

    protected ResourceBaseT() { }

    protected ResourceBaseT(BaseId scenarioId, BaseId plantId, BaseId departmentId)
        : base(scenarioId, plantId, departmentId) { }
}