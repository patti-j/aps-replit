using PT.APSCommon;

namespace PT.Transmissions.ResourceConnectors;

/// <summary>
/// Base class for all ResourceConnectors related Transmissions.
/// </summary>
public class ResourceConnectorsBaseT : ScenarioIdBaseT
{
    #region IPTSerializable Members
    public ResourceConnectorsBaseT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12305) { }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public const int UNIQUE_ID = 1096;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceConnectorsBaseT() { }

    public ResourceConnectorsBaseT(BaseId a_scenarioId)
        : base(a_scenarioId) { }

    public override string Description => "Resource Connector updated";
}