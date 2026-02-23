namespace PT.Scheduler.Simulation.Events;

internal class ReleaseToResourceEvent : ReleaseEvent
{
    internal ReleaseToResourceEvent(long a_time, InternalActivity a_activity, InternalResource a_resource)
        : base(a_time, a_activity)
    {
        Resource = a_resource;
    }

    internal InternalResource Resource { get; }

    internal override bool Cancelled => Activity.SimScheduled;

    internal const int UNIQUE_ID = 7;

    internal override int UniqueId => UNIQUE_ID;
}