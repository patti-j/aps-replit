namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Summary description for SchedulabilityCustomizationEvent.
/// </summary>
public class SchedulabilityCustomizationEvent : EventBase
{
    public SchedulabilityCustomizationEvent(long a_time)
        : base(a_time) { }

    internal const int UNIQUE_ID = 13;
    internal override int UniqueId => UNIQUE_ID;
}