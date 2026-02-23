namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Used to constrain the start of an MO.
/// </summary>
public abstract class ManufacturingOrderEvent : EventBase
{
    public ManufacturingOrderEvent(long a_time, ManufacturingOrder a_mO)
        : base(a_time)
    {
        m_mo = a_mO;
    }

    private ManufacturingOrder m_mo;

    public ManufacturingOrder ManufacturingOrder
    {
        get => m_mo;

        set => m_mo = value;
    }
}