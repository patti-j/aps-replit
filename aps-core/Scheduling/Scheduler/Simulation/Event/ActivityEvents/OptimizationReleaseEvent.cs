namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// This event only occurs during optimziations. It's a release time on outter unscheduled activities based
/// on the optimization start time.
/// Leaf activities that were scheduled before this time before the optimization are not subject to this limit.
/// This event is used to prevent new orders and rescheduled activities from being scheduled to the left of the
/// optimization time.
/// </summary>
public class OptimizationReleaseEvent : ReleaseEvent
{
    public OptimizationReleaseEvent(long a_time, InternalActivity a_activity)
        : base(a_time, a_activity) { }

    internal const int UNIQUE_ID = 6;

    internal override int UniqueId => UNIQUE_ID;
}