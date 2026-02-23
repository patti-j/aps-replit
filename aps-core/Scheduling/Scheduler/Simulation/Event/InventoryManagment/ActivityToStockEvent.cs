namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// </summary>
internal class UNUSED_Event : ReleaseEvent
{
    internal UNUSED_Event(long a_time, InternalActivity a_activity)
        : base(a_time, a_activity) { }

    internal const int UNIQUE_ID = 15;

    internal override int UniqueId => UNIQUE_ID;
}