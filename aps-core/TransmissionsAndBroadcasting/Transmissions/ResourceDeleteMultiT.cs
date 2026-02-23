using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Deletes multiple selected resources, optionally with empty Departments and Plants too.
/// </summary>
public class ResourceDeleteMultiT : ResourceIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 224;
    private List<ResourceKey> m_resourceKeys = new List<ResourceKey>();
    private bool m_deleteEmpty;

    public List<ResourceKey> ResourceKeys
    {
        get { return m_resourceKeys; }
        set { m_resourceKeys = value; }
    }

    public bool DeleteEmpty
    {
        get { return m_deleteEmpty; }
        set { m_deleteEmpty = value; }
    }

    #region IPTSerializable Members
    public ResourceDeleteMultiT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_deleteEmpty);
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                ResourceKey node = new(reader);
                m_resourceKeys.Add(node);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(DeleteEmpty);
        writer.Write(ResourceKeys);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceDeleteMultiT() { }

    public ResourceDeleteMultiT(BaseId scenarioId, BaseId plantId, BaseId departmentId, BaseId machineId)
        : base(scenarioId, plantId, departmentId, machineId) { }

    public override string Description => "Resources deleted";
}