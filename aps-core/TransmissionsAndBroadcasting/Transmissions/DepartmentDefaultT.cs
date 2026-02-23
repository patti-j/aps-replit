using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Creates a new Department in the specified Scenario using default values.
/// </summary>
public class DepartmentDefaultT : DepartmentBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 67;

    #region IPTSerializable Members
    public DepartmentDefaultT(IReader reader)
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

    public DepartmentDefaultT() { }

    public DepartmentDefaultT(BaseId scenarioId, BaseId plantId)
        : base(scenarioId, plantId) { }

    public override string Description => "Department Created".Localize();
}