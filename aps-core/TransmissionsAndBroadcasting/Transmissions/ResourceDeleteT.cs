using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes the Resource.
/// </summary>
public class ResourceDeleteT : ResourceIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 117;

    #region IPTSerializable Members
    public ResourceDeleteT(IReader reader)
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

    public ResourceDeleteT() { }

    public ResourceDeleteT(BaseId scenarioId, BaseId plantId, BaseId departmentId, BaseId machineId)
        : base(scenarioId, plantId, departmentId, machineId) { }

    public override string Description => "Resource deleted";
}