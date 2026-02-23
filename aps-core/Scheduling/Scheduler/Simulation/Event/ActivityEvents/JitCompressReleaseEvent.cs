namespace PT.Scheduler.Simulation.Events;

internal class JitCompressReleaseEvent : ActivityReleasedEvent
{
    public JitCompressReleaseEvent(long a_time, InternalActivity a_activity)
        : base(a_time, a_activity) { }

    internal new const int UNIQUE_ID = 35;

    internal override int UniqueId => UNIQUE_ID;
}