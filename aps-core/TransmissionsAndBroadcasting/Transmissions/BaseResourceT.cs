using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Resources.
/// </summary>
public abstract class BaseResourceT : DepartmentIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 27;

    #region IPTSerializable Members
    public BaseResourceT(IReader reader)
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

    protected BaseResourceT() { }

    protected BaseResourceT(BaseId scenarioId, BaseId plantId, BaseId departmentId)
        : base(scenarioId, plantId, departmentId) { }
}