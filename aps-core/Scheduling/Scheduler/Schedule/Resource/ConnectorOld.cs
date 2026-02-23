using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// Specify a flow relationship between two Resources.
/// A Resource can (optionally) have one or more Connectors to downstream Resources to specify where successor operations can be scheduled.
/// Connectors are often used in process manufacturing when there is a physical conveyance such as a pipe or belt that carries product between Resources.
/// If Connectors are specified for a Resource then success operations to operations scheduled on the Resource will only be scheduled to one of the connected Resources.
/// There is the option to allow drag-and-drop to non-connected Resources in which case a Flag will be shown to warn the user of the perhaps undesired condition.
/// </summary>
[Obsolete("Replaced by ResourceConnector")]
public class ConnectorOld : IComparable<ConnectorOld>, IKey<BaseId>, IPTSerializable
{
    public const int UNIQUE_ID = 663;

    public BaseId Id;

    public ConnectorOld(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12312)
        {
            Id = new BaseId(a_reader);
            a_reader.Read(out long m_fixedTransitSpan);
            a_reader.Read(out short connectorType);
        }
        else if (a_reader.VersionNumber >= 455)
        {
            Id = new BaseId(a_reader);
            a_reader.Read(out long m_fixedTransitSpan);
        }
    }
    
    public int CompareTo(ConnectorOld a_other)
    {
        if (ReferenceEquals(this, a_other))
        {
            return 0;
        }

        if (ReferenceEquals(null, a_other))
        {
            return 1;
        }

        return 0;
    }

    public bool Equals(BaseId a_other)
    {
        return true;
    }

    public BaseId GetKey()
    {
        return BaseId.NULL_ID;
    }

    public void Serialize(IWriter a_writer)
    {
        throw new NotImplementedException();
    }

    public int UniqueId => 0;
}