using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Department related transmissions.
/// </summary>
public abstract class DepartmentBaseT : PlantIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 64;

    #region IPTSerializable Members
    public DepartmentBaseT(IReader reader)
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

    protected DepartmentBaseT() { }

    protected DepartmentBaseT(BaseId scenarioId, BaseId plantId)
        : base(scenarioId, plantId) { }
}