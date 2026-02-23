namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Used to signal that the Move time has been reached. Initially added to release the move reservations in case the move activities couldn't be released at the move time and cause the releases to be
/// performed.
/// </summary>
internal class MoveTicksEvent : EventBase
{
    internal MoveTicksEvent(long a_moveTicks, InternalActivity a_act) :
        base(a_moveTicks)
    {
        Activity = a_act;
    }

    internal readonly InternalActivity Activity;

    internal const int UNIQUE_ID = 39;

    internal override int UniqueId => UNIQUE_ID;
}