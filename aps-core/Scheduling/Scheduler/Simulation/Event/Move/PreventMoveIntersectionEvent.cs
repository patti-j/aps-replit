namespace PT.Scheduler.Simulation.Events;

// [USAGE_CODE] PreventMoveIntersectionEvent: Event to constrain the start of an activity when the move is re-received. The activity will be better scheduled around the moved blocks.
/// <summary>
/// Used to constrain the start time of an activity that intersects the move time.
/// When activities are scheduled any that intersect the move date are stored. Then when the move activity is scheduled
/// the best times to schedule the intersecting activities are determined.
/// Finally the schedule changes are undone and the transmission is re-received and these constraint events are created
/// to better schedule the activities.
/// </summary>
internal class PreventMoveIntersectionEvent : EventBase
{
    /// <summary>
    /// Specify the earliest the activity can start.
    /// </summary>
    /// <param name="a_time">The earliest the activity can start.</param>
    /// <param name="a_act">An activity that intersects the move date. This isn't the activity being moved.</param>
    internal PreventMoveIntersectionEvent(long a_time, InternalActivity a_act) : base(a_time)
    {
        Activity = a_act;
    }

    /// <summary>
    /// The activity whose start time is being constrained. This isn't the activity being moved.
    /// </summary>
    internal readonly InternalActivity Activity;

    internal const int UNIQUE_ID = 42;

    internal override int UniqueId => UNIQUE_ID;
}