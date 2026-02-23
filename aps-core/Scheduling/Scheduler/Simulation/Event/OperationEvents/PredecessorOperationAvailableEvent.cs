namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Indicatest that a Predecessor Operation will be available for the SuccessorOperations
/// </summary>
public class PredecessorOperationAvailableEvent : EventBase
{
    public PredecessorOperationAvailableEvent(long a_earliestReleaseTime, AlternatePath.Association a_association, long a_predecessorReleaseTime)
        : base(a_earliestReleaseTime)
    {
        m_association = a_association;
        m_predecessorReleaseTime = a_predecessorReleaseTime;
    }

    /// <summary>
    /// This is some constraint on when the Manufacturing Order can be scheduled. For instance it may be the clock or the simulation start time.
    /// </summary>
    internal long EarliestReleaseTime => Time;

    private readonly long m_predecessorReleaseTime;

    /// <summary>
    /// When the material from a predecessor operation becomes available.
    /// Depending on the operation's time attributes, this could be:
    /// the end of processing,
    /// the end of material post processing,
    /// or the end of Resource Transfer Time.
    /// Operations consume time in this order 1. setup time, 2. processing, 3. material post processing time, 4. Resource Transfer Time.
    /// </summary>
    internal long PredecessorReleaseTime => m_predecessorReleaseTime;

    private readonly AlternatePath.Association m_association;

    /// <summary>
    /// The association between the predecessor operation and the successor operation being released.
    /// </summary>
    internal AlternatePath.Association Association => m_association;

    internal const int UNIQUE_ID = 26;

    internal override int UniqueId => UNIQUE_ID;

    public override string ToString()
    {
        return string.Format("{0}; Job: {1}; Pred: {2}; Suc: {3}", DateTimeHelper.ToLocalTimeFromUTCTicks(Time), m_association.Predecessor.Operation.Job.Name, m_association.Predecessor.Operation.Name, m_association.Successor.Operation.Name);
    }
}