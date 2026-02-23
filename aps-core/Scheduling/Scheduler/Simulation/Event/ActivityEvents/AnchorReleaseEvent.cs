namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Used to hold an activity back until its anchor time has been reached.
/// </summary>
public class AnchorReleaseEvent : ReleaseEvent
{
    /// <summary>
    /// Specify exactly when all an activity's constraints have been satisfied.
    /// </summary>
    /// <param name="time">The time at which all the constraints on the activity have been satisfied.</param>
    /// <param name="activity">The activity whose constraints are all satisfied at the event's time.</param>
    public AnchorReleaseEvent(long a_time, InternalActivity a_activity)
        : base(a_time, a_activity) { }

    internal const int UNIQUE_ID = 1;

    internal override int UniqueId => UNIQUE_ID;
}