namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Used during operation move to place an activity on a dispatcher at the right time.
/// </summary>
public class MoveActivityTimeEvent : ReleaseEvent
{
    public MoveActivityTimeEvent(long a_time, InternalActivity a_activity, Resource a_moveRes)
        : base(a_time, a_activity)
    {
        m_moveRes = a_moveRes;
    }

    internal const int UNIQUE_ID = 5;

    internal override int UniqueId => UNIQUE_ID;

    /// <summary>
    /// If this value is set to null this means that the initial event failed because a non exact move occurred and a block was
    /// already scheduled at the move time. See the response to this event in ScenarioDetail.
    /// </summary>
    internal readonly Resource m_moveRes;
}