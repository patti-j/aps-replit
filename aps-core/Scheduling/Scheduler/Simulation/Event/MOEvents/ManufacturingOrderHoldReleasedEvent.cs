namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Used to constrain the start of an MO.
/// </summary>
public class ManufacturingOrderHoldReleasedEvent : ManufacturingOrderEvent
{
    public ManufacturingOrderHoldReleasedEvent(long a_time, ManufacturingOrder a_mO)
        : base(a_time, a_mO) { }

    internal const int UNIQUE_ID = 18;

    internal override int UniqueId => UNIQUE_ID;
}