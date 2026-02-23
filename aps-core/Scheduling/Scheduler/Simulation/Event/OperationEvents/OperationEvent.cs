namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Base event for operation events.
/// </summary>
public abstract class OperationEvent : EventBase
{
    public OperationEvent(long a_time, BaseOperation a_operation)
        : base(a_time)
    {
        m_operation = a_operation;
    }

    private readonly BaseOperation m_operation;

    public BaseOperation Operation => m_operation;

    public override string ToString()
    {
        return string.Format("{0} {1}", base.ToString(), m_operation.Name);
    }
}