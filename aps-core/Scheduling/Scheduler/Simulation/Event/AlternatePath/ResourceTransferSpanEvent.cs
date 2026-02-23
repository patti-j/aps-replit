namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Used to implement resource transfer span between operations.
/// </summary>
public class ResourceTransferSpanEvent : EventBase
{
    public ResourceTransferSpanEvent(long a_time, AlternatePath.Association a_association)
        : base(a_time)
    {
        m_association = a_association;
    }

    private readonly AlternatePath.Association m_association;

    public AlternatePath.Association Association => m_association;

    public override string ToString()
    {
        return string.Format("{0}: {1}: {2}", base.ToString(), m_association.Predecessor.Operation.Name, m_association.Successor.Operation.Name);
    }

    internal const int UNIQUE_ID = 10;

    internal override int UniqueId => UNIQUE_ID;
}