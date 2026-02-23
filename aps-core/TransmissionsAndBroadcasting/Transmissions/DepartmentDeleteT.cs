using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes the Department (and all of its Resources).
/// </summary>
public class DepartmentDeleteT : DepartmentIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 69;

    #region IPTSerializable Members
    public DepartmentDeleteT(IReader reader)
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

    public DepartmentDeleteT() { }

    public DepartmentDeleteT(BaseId scenarioId, BaseId plantId, BaseId departmentId)
        : base(scenarioId, plantId, departmentId) { }

    public override string Description => "Department deleted";
}