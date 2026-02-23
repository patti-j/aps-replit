namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Used in clock advance to set an earliest date when an activity can be started.
/// It is also used as an event to release activities at the right point in time,
/// for instance when a Move occurs the operations that aren't being moved are
/// scheduled to be released at the time they were originally scheduled.
/// </summary>
public class ClockReleaseEvent : ReleaseEvent
{
    public ClockReleaseEvent(long a_time, InternalActivity a_activity)
        : base(a_time, a_activity) { }

    internal const int UNIQUE_ID = 2;

    internal override int UniqueId => UNIQUE_ID;
}