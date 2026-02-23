namespace PT.Scheduler.Simulation.Customizations;

/// <summary>
/// Provides change helper classes for the SimulationInitialization customization
/// </summary>
public class SchedulabilitySimulationInitializationHelper
{
    private readonly SchedulabilityChangeHelper m_schedulabilityChangeHelper;

    public SchedulabilitySimulationInitializationHelper()
    {
        m_schedulabilityChangeHelper = new SchedulabilityChangeHelper();
    }

    public SchedulabilityChangeHelper SchedulabilityChanger => m_schedulabilityChangeHelper;

    internal void Execute()
    {
        m_schedulabilityChangeHelper.Execute();
    }
}