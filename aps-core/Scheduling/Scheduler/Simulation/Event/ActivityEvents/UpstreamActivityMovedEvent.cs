namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// When an operation is moved the downstream operations will be constrained to start on or after
/// their current scheduled start date (unless Expedite Successors on Move is in effect.
/// This event constrains an operation to start on or after its current scheduled date.
/// </summary>
public class ScheduledDateBeforeMoveEvent : ReleaseEvent
{
    public ScheduledDateBeforeMoveEvent(long a_time, InternalActivity a_activity)
        : base(a_time, a_activity) { }

    internal const int UNIQUE_ID = 9;

    internal override int UniqueId => UNIQUE_ID;
}