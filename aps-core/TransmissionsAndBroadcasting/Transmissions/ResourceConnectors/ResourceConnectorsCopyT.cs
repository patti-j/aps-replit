using PT.APSCommon;

namespace PT.Transmissions.ResourceConnectors;

public class ResourceConnectorsCopyT : ResourceConnectorsIdBaseT
{
    #region IPTSerializable Members
    public ResourceConnectorsCopyT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public new const int UNIQUE_ID = 1100;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceConnectorsCopyT() { }

    public ResourceConnectorsCopyT(BaseId a_scenarioId, List<BaseId> a_resConnectorIdList)
        : base(a_scenarioId, a_resConnectorIdList) { }

    public override string Description => "Resource Connector copied";
}