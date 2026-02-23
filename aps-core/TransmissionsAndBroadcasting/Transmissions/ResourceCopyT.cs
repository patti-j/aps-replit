using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Creates a new Resource by copying the specified Machine.
/// </summary>
public class ResourceCopyT : ResourceBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 114;

    #region IPTSerializable Members
    public ResourceCopyT(IReader reader)
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

    public BaseId originalId; //Id of the Machine to copy.

    public ResourceCopyT() { }

    public ResourceCopyT(BaseId scenarioId, BaseId plantId, BaseId departmentId, BaseId originalId)
        : base(scenarioId, plantId, departmentId)
    {
        this.originalId = originalId;
    }

    public override string Description => "Resource Copied".Localize();
}