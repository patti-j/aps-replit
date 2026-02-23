namespace PT.Scheduler.Simulation.Customizations;

/// <summary>
/// Provides the schedulablility change helper classes to the PostSimStage customization.
/// Also provides the ability to signal an additonal simulation.
/// </summary>
public class PostSimStageChangeHelper
{
    private readonly SchedulabilityChangeHelper m_schedChangeHelper;

    public PostSimStageChangeHelper()
    {
        m_schedChangeHelper = new SchedulabilityChangeHelper();
    }

    public SchedulabilityChangeHelper SchedulabilityChangeHelper => m_schedChangeHelper;
}