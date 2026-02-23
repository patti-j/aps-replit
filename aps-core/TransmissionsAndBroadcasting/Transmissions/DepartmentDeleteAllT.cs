using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes all Departments in the specified Scenario (and all of their Resources).
/// </summary>
public class DepartmentDeleteAllT : DepartmentBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 68;

    #region IPTSerializable Members
    public DepartmentDeleteAllT(IReader reader)
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

    public DepartmentDeleteAllT() { }

    public DepartmentDeleteAllT(BaseId scenarioId, BaseId plantId)
        : base(scenarioId, plantId) { }

    public override string Description => "Plant Departments deleted";
}