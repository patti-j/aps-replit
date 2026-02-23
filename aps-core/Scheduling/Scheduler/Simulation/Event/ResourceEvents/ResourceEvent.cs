namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Indicates that the specified Resource will be available to schedule another
/// Activity.  Occurs after an Activity is complete or an Offline period ends.
/// </summary>
public abstract class ResourceEvent : EventBase
{
    protected ResourceEvent(long a_time, Resource a_resource)
        : base(a_time)
    {
        m_resource = a_resource;
    }

    private Resource m_resource;

    internal Resource Resource
    {
        get => m_resource;
        set => m_resource = value;
    }

    public override string ToString()
    {
        if (m_resource != null)
        {
            return string.Format("{0} {1}", base.ToString(), m_resource.Name);
        }

        return string.Format("{0} {1}", base.ToString(), "null");
    }

    internal virtual void Clear()
    {
        m_resource = null;
    }
}