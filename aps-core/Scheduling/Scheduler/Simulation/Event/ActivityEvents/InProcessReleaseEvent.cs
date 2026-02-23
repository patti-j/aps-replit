namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Create this event for in-process activities.
/// </summary>
public class InProcessReleaseEvent : ReleaseEvent
{
    public InProcessReleaseEvent(long a_time, InternalActivity a_activity)
        : base(a_time, a_activity) { }

    internal const int UNIQUE_ID = 3;

    internal override int UniqueId => UNIQUE_ID;
}