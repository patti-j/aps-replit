using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Signals the start of a new InsertJobs Simulation. Provides the ID of scenario to use.
/// </summary>
public class InsertJobsStartT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 775;

    #region IPTSerializable Members
    public InsertJobsStartT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 469)
        {
            m_listOfJobsToExpedite = new Scheduler.BaseIdList(a_reader);
            a_reader.Read(out m_type);
        }
        else if (a_reader.VersionNumber >= 464)
        {
            m_listOfJobsToExpedite = new Scheduler.BaseIdList(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_listOfJobsToExpedite.Serialize(a_writer);
        a_writer.Write(m_type);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public InsertJobsStartT() { }

    public InsertJobsStartT(BaseId a_scenarioId, SchedulerDefinitions.InsertJobsSimulationTypes a_insertType)
        : base(a_scenarioId)
    {
        SimulationType = a_insertType;
    }

    public InsertJobsStartT(BaseId a_scenarioId, Scheduler.BaseIdList a_listOfJobstToExpedite, SchedulerDefinitions.InsertJobsSimulationTypes a_insertType)
        : base(a_scenarioId)
    {
        m_listOfJobsToExpedite = a_listOfJobstToExpedite;
        SimulationType = a_insertType;
    }

    private Scheduler.BaseIdList m_listOfJobsToExpedite = new ();

    /// <summary>
    /// List of Job Ids that insert Jobs should use, ignoring other settings that specify which jobs to Insert.
    /// </summary>
    public Scheduler.BaseIdList ListOfJobsToExpedite
    {
        get => m_listOfJobsToExpedite;
        set => m_listOfJobsToExpedite = value;
    }

    private short m_type;

    /// The type of InsertJobs simulation to perform
    /// </summary>
    public SchedulerDefinitions.InsertJobsSimulationTypes SimulationType
    {
        get => (SchedulerDefinitions.InsertJobsSimulationTypes)m_type;
        set => m_type = (short)value;
    }

    public override string Description => "Insert Jobs started";
}