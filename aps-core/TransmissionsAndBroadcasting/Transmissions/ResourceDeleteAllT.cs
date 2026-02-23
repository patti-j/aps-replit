using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes all Resources in the specified Scenario (and all of their Resources).
/// </summary>
public class ResourceDeleteAllT : ResourceBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 116;

    #region IPTSerializable Members
    public ResourceDeleteAllT(IReader reader)
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

    public ResourceDeleteAllT() { }

    public ResourceDeleteAllT(BaseId scenarioId, BaseId plantId, BaseId departmentId)
        : base(scenarioId, plantId, departmentId) { }

    public override string Description => "Resources deleted";
}