namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Once all the constraints on an activity have been satisfied this event should
/// be posted so that the activity can be added to the appropriate dispatchers.
/// </summary>
internal class HeadStartWindowRetryEvent : ReleaseEvent
{
    /// <summary>
    /// Specify exactly when all an activity's constraints have been satisfied.
    /// </summary>
    /// <param name="time">The time at which all the constraints on the activity have been satisfied.</param>
    /// <param name="activity">The activity whose constraints are all satisfied at the event's time.</param>
    internal HeadStartWindowRetryEvent(long a_time, InternalActivity a_activity)
        : base(a_time, a_activity) { }

    internal const int UNIQUE_ID = 59;

    internal override int UniqueId => UNIQUE_ID;
}