using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new Department by copying the specified Department.
/// </summary>
public class DepartmentCopyT : DepartmentBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 66;

    #region IPTSerializable Members
    public DepartmentCopyT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            originalId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        originalId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId originalId; //Id of the Department to copy.

    public DepartmentCopyT() { }

    public DepartmentCopyT(BaseId scenarioId, BaseId plantId, BaseId originalId)
        : base(scenarioId, plantId)
    {
        this.originalId = originalId;
    }

    public override string Description => "Department copied";
}