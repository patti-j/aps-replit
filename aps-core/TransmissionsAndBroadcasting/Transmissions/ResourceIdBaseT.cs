using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Resource related transmissions.
/// </summary>
public abstract class ResourceIdBaseT : ResourceBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 118;

    #region IPTSerializable Members
    public ResourceIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            machineId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        machineId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId machineId;

    protected ResourceIdBaseT() { }

    protected ResourceIdBaseT(BaseId scenarioId, BaseId plantId, BaseId departmentId, BaseId machineId)
        : base(scenarioId, plantId, departmentId)
    {
        this.machineId = machineId;
    }
}