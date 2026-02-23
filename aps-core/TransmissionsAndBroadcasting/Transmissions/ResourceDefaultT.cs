using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Creates a new Resource in the specified Scenario using default values.
/// </summary>
public class ResourceDefaultT : ResourceBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 115;

    #region IPTSerializable Members
    public ResourceDefaultT(IReader reader)
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

    public ResourceDefaultT() { }

    public ResourceDefaultT(BaseId scenarioId, BaseId plantId, BaseId departmentId)
        : base(scenarioId, plantId, departmentId) { }

    public override string Description => "Resource Created".Localize();
}