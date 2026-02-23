using PT.APSCommon;

namespace PT.Transmissions.ResourceConnectors;

public class ResourceConnectorsDeleteT : ResourceConnectorsIdBaseT
{
    #region IPTSerializable Members
    public ResourceConnectorsDeleteT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public new const int UNIQUE_ID = 1101;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceConnectorsDeleteT() { }

    public ResourceConnectorsDeleteT(BaseId a_scenarioId, List<BaseId> a_resConnectorIdList)
        : base(a_scenarioId, a_resConnectorIdList) { }

    public override string Description => "Resource Connector deleted";
}