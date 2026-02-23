namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Used during a normal move to constrain the successors of the moved activity to schedule
/// after their left neighbors. They don't need to schedule immediately after their left
/// neighbors but can schedule no earlier than they are scheduled to complete.
/// </summary>
public class RightMovingNeighborReleaseEvent : ReleaseEvent
{
    public RightMovingNeighborReleaseEvent(long a_time, InternalActivity a_activity)
        : base(a_time, a_activity) { }

    internal const int UNIQUE_ID = 8;

    internal override int UniqueId => UNIQUE_ID;
}