using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Department related transmissions.
/// </summary>
public abstract class DepartmentIdBaseT : DepartmentBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 70;

    #region IPTSerializable Members
    public DepartmentIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            departmentId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        departmentId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId departmentId;

    protected DepartmentIdBaseT() { }

    protected DepartmentIdBaseT(BaseId scenarioId, BaseId plantId, BaseId departmentId)
        : base(scenarioId, plantId)
    {
        this.departmentId = departmentId;
    }
}