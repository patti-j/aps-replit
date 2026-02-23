using PT.APSCommon;

namespace PT.Transmissions.ResourceConnectors;

public class ResourceConnectorsIdBaseT : ResourceConnectorsBaseT
{
    #region IPTSerializable Members
    public ResourceConnectorsIdBaseT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                m_resConnectorIds.Add(new BaseId(a_reader));
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_resConnectorIds.Count);
        foreach (BaseId resConnectorId in m_resConnectorIds)
        {
            resConnectorId.Serialize(a_writer);
        }
    }

    public new const int UNIQUE_ID = 1097;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceConnectorsIdBaseT() { }

    public ResourceConnectorsIdBaseT(BaseId a_scenarioId, List<BaseId> a_resConnectorIdList)
        : base(a_scenarioId)
    {
        m_resConnectorIds = a_resConnectorIdList;
    }

    private readonly List<BaseId> m_resConnectorIds = new ();

    public List<BaseId> ResourceConnectorIds => m_resConnectorIds;
}