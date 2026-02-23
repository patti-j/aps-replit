using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes the Capability.
/// </summary>
public class CapabilityDeleteT : CapabilityIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 33;

    #region IPTSerializable Members
    private readonly bool m_isMultiDelete;
    private readonly List<BaseId> m_machineCapabilityIds;
    public List<BaseId> CapabilityIdsToDelete => m_machineCapabilityIds;

    public CapabilityDeleteT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12107)
        {
            a_reader.Read(out m_isMultiDelete);
            if (IsMultiDelete)
            {
                m_machineCapabilityIds = new List<BaseId>();
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    m_machineCapabilityIds.Add(new BaseId(a_reader));
                }
            }
        }
        else if (a_reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(IsMultiDelete);
        if (IsMultiDelete)
        {
            writer.Write(m_machineCapabilityIds.Count);
            foreach (BaseId capabilityId in m_machineCapabilityIds)
            {
                capabilityId.Serialize(writer);
            }
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CapabilityDeleteT() { }

    public CapabilityDeleteT(BaseId scenarioId, BaseId machineCapabilityId)
        : base(scenarioId, machineCapabilityId)
    {
        m_isMultiDelete = false;
    }

    public CapabilityDeleteT(BaseId scenarioId, List<BaseId> machineCapabilityIds)
        : base(scenarioId, machineCapabilityIds.First())
    {
        m_isMultiDelete = true;
        m_machineCapabilityIds = machineCapabilityIds;
    }

    public override string Description => "Capability deleted";

    public bool IsMultiDelete => m_isMultiDelete;
}