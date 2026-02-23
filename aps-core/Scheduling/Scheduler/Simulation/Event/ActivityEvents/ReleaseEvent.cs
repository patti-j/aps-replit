namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// An activity can have a number of constraints that must be satisfied
/// before it is released. This event is a base for any type of constraint
/// holding back an activity.
/// </summary>
public abstract class ReleaseEvent : EventBase
{
    public ReleaseEvent(long a_time, InternalActivity a_activity)
        : base(a_time)
    {
        m_activity = a_activity;
    }

    private InternalActivity m_activity;

    /// <summary>
    /// The activity that this event is for.
    /// </summary>
    public InternalActivity Activity
    {
        get => m_activity;
        set => m_activity = value;
    }

    public override string ToString()
    {
        return string.Format("{0} {1}", base.ToString(), m_activity.Operation.Name);
    }
}