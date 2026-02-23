namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Indicates that all the Material needed to start an Operation.
/// </summary>
public class MaterialAvailableEvent : OperationEvent
{
    public MaterialAvailableEvent(long a_time, MaterialRequirement a_latestMR, BaseOperation a_consumingOperation)
        : base(a_time, a_consumingOperation)
    {
        m_latestMR = a_latestMR;
    }

    internal const int UNIQUE_ID = 23;

    internal override int UniqueId => UNIQUE_ID;

    internal MaterialRequirement m_latestMR;
}