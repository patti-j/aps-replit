namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Used to constrain the start of an Job.
/// </summary>
public abstract class JobEvent : EventBase
{
    public JobEvent(long a_time, Job a_job)
        : base(a_time)
    {
        m_job = a_job;
    }

    private Job m_job;

    public Job Job
    {
        get => m_job;

        set => m_job = value;
    }
}