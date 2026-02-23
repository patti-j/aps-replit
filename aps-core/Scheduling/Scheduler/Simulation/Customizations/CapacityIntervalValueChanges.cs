namespace PT.Scheduler.Simulation.Customizations;

//Stores a capacity interval modified within a customization.
public class CapacityIntervalValueChanges : BaseObjectValueChanges
{
    private readonly CapacityInterval m_interval;

    public CapacityIntervalValueChanges(CapacityInterval a_capacityInterval)
        : base(a_capacityInterval)
    {
        m_interval = a_capacityInterval;
    }

    public CapacityInterval Interval => m_interval;
}