using PT.APSCommon;

namespace PT.Transmissions.ResourceConnectors;

public class ResourceConnectorsDeleteAllT : ResourceConnectorsBaseT
{
    #region IPTSerializable Members
    public ResourceConnectorsDeleteAllT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public new const int UNIQUE_ID = 1102;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceConnectorsDeleteAllT() { }

    public ResourceConnectorsDeleteAllT(BaseId a_scenarioId)
        : base(a_scenarioId) { }

    public override string Description => "All Resource Connectors deleted";
}