namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Summary description for RetryForCleanoutEvent.
/// </summary>
internal class RetryForCleanoutEvent : EventBase
{
    internal RetryForCleanoutEvent(long a_time)
        : base(a_time) { }

    internal const int UNIQUE_ID = 51;
    internal override int UniqueId => UNIQUE_ID;
}