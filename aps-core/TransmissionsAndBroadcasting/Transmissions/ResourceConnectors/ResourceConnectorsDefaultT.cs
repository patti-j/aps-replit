using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions.ResourceConnectors;

/// <summary>
/// Create a new ResourceConnector.
/// </summary>
public class ResourceConnectorsDefaultT : ResourceConnectorsBaseT
{
    #region IPTSerializable Members
    public ResourceConnectorsDefaultT(IReader a_reader)
        : base(a_reader)
    {
        a_reader.Read(out int count);
        for (int i = 0; i < count; i++)
        {
            FromResources.Add(new BaseId(a_reader));
        }

        a_reader.Read(out count);
        for (int i = 0; i < count; i++)
        {
            ToResources.Add(new BaseId(a_reader));
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(FromResources.Count);

        foreach (BaseId resId in FromResources)
        {
            resId.Serialize(a_writer);
        }

        a_writer.Write(ToResources.Count);

        foreach (BaseId resExternalKey in ToResources)
        {
            resExternalKey.Serialize(a_writer);
        }
    }

    public new const int UNIQUE_ID = 1098;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceConnectorsDefaultT() { }

    public ResourceConnectorsDefaultT(BaseId a_scenarioId)
        : base(a_scenarioId) { }

    public override string Description => "Resource Connector created";

    internal void AddConnection(BaseId a_resId, ConnectorDefs.EConnectionDirection a_connDirection)
    {
        switch (a_connDirection)
        {
            case ConnectorDefs.EConnectionDirection.FromResource:
                FromResources.Add(a_resId);
                break;
            case ConnectorDefs.EConnectionDirection.ToResource:
                ToResources.Add(a_resId);
                break;
        }

        ValidateConnection(a_resId);
    }

    private void ValidateConnection(BaseId a_resId)
    {
        if (FromResources.Contains(a_resId) && ToResources.Contains(a_resId))
        {
            throw new PTValidationException("4484", new object[] { a_resId });
        }
    }

    public List<BaseId> FromResources = new();
    public List<BaseId> ToResources = new();
}