namespace PT.Scheduler.Simulation.Events;

internal class AttemptToScheduleForResRvnEvent : EventBase
{
    internal AttemptToScheduleForResRvnEvent(long a_time)
        : base(a_time) { }

    internal const int UNIQUE_ID = 32;

    internal override int UniqueId => UNIQUE_ID;
}