namespace PT.Scheduler.Simulation.Events;

internal class MoveIntersectionEvent : EventBase
{
    internal MoveIntersectionEvent(long a_time, InternalActivity a_intersectingActivity)
        : base(a_time)
    {
        IntersectingActivity = a_intersectingActivity;
    }

    internal readonly InternalActivity IntersectingActivity;

    internal const int UNIQUE_ID = 44;

    internal override int UniqueId => UNIQUE_ID;
}