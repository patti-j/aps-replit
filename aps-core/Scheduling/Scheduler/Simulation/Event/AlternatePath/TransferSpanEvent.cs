using PT.Scheduler.Simulation.Scheduler.AlternatePaths;

namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Used to apply an activity's transfer time.
/// </summary>
public class TransferSpanEvent : EventBase
{
    /// <summary>
    /// When transfer time has passed.
    /// </summary>
    /// <param name="time">The time the transfer time constraint is released.</param>
    /// <param name="activity">The association whose transfertime has passed.</param>
    internal TransferSpanEvent(long a_oldUntilDate, AlternatePath.Association a_association, TransferInfo a_transferInfo)
        : base(a_oldUntilDate)
    {
        m_association = a_association;
        m_transferInfo = a_transferInfo;
    }

    /// <summary>
    /// The association whose TransferSpan has been met.
    /// </summary>
    internal AlternatePath.Association TransferSpanAssociation => m_association;

    internal TransferInfo TransferInfo => m_transferInfo;

    private readonly AlternatePath.Association m_association;
    private readonly TransferInfo m_transferInfo;

    public override string ToString()
    {
        return string.Format("{0}: {1}: {2}", base.ToString(), m_association.Predecessor.Operation.Name, m_association.Successor.Operation.Name);
    }

    internal const int UNIQUE_ID = 11;
    internal override int UniqueId => UNIQUE_ID;
}