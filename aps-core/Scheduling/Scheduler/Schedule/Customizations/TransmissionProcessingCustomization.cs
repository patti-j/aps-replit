namespace PT.Scheduler.Simulation.Customizations.TransmissionPreprocessing;

/// <summary>
/// Holds data relevant to the customization for use by ScenarioDetail outside of the customization
/// </summary>
public class TransmissionHandlingResult
{
    public class ActivityChange
    {
        public ActivityChange(InternalActivity a_act)
        {
            Activity = a_act;
        }

        public InternalActivity Activity { get; private set; }

        public bool? Anchored { get; set; }

        public bool? Locked { get; set; }

        public SchedulerDefinitions.InternalActivityDefs.productionStatuses? ProductionStatus { get; set; }
    }

    public TransmissionHandlingResult()
    {
        m_actUpdateList = new List<ActivityChange>();
    }

    private readonly List<ActivityChange> m_actUpdateList;

    public List<ActivityChange> ActivityUpdateList => m_actUpdateList;
}