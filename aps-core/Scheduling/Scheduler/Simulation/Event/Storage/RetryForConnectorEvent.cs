namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Summary description for RetryForCleanoutEvent.
/// </summary>
internal class RetryForConnectorEvent : EventBase
{
    internal RetryForConnectorEvent(long a_time)
        : base(a_time) { }

    internal const int UNIQUE_ID = 53;
    internal override int UniqueId => UNIQUE_ID;
}