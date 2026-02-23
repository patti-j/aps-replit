namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Used to constrain the start of an Job.
/// </summary>
public class JobHoldReleasedEvent : JobEvent
{
    public JobHoldReleasedEvent(long a_time, Job a_job)
        : base(a_time, a_job) { }

    internal const int UNIQUE_ID = 17;

    internal override int UniqueId => UNIQUE_ID;
}