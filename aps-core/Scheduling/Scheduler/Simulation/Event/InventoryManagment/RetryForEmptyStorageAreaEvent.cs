namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// This event is used to retry an activity that failed to schedule due no empty storage areas being available.
/// </summary>
internal class RetryForEmptyStorageAreaEvent : EventBase
{
    /// <summary>
    /// Occurs when a product goes into stock.
    /// </summary>
    internal RetryForEmptyStorageAreaEvent(long a_time, InternalActivity a_activity, IEnumerable<InternalResource> a_previousDispatchers)
        : base(a_time)
    {
        m_activity = a_activity;
        m_previousDispatchers = a_previousDispatchers;
    }

    private readonly InternalActivity m_activity;
    private readonly IEnumerable<InternalResource> m_previousDispatchers;

    internal InternalActivity Activity => m_activity;
    internal IEnumerable<InternalResource> PreviousDispatchers => m_previousDispatchers;

    internal const int UNIQUE_ID = 56;

    internal override int UniqueId => UNIQUE_ID;
}