namespace PT.SchedulerDefinitions;

/// <summary>
/// Holds all diagnostic information for RuleSeek simulations.
/// </summary>
public class RuleSeekDiagnositcs : IPTSerializable
{
    public RuleSeekDiagnositcs()
    {
        RuleSeekStatus = CoPilotSimulationStatus.STOPPED;
        RuleSeekSimulationIterations = 0;
        m_simulationDurationTicks = 0;
        m_simulationStartTicks = 0;
        m_durationToFindBestScenario = 0;
    }

    public const int UNIQUE_ID = 787;

    #region IPTSerializable Members
    public RuleSeekDiagnositcs(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_ruleSeekStatus);
            reader.Read(out m_ruleSeekSimulationIterations);
            reader.Read(out m_simulationDurationTicks);
            reader.Read(out m_simulationStartTicks);
            reader.Read(out m_durationToFindBestScenario);
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(m_ruleSeekStatus);
        writer.Write(m_ruleSeekSimulationIterations);
        writer.Write(m_simulationDurationTicks);
        writer.Write(m_simulationStartTicks);
        writer.Write(m_durationToFindBestScenario);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region RuleSeek
    private int m_ruleSeekStatus;

    /// <summary>
    /// Current status of the RuleSeek simulation
    /// </summary>
    public CoPilotSimulationStatus RuleSeekStatus
    {
        get => (CoPilotSimulationStatus)m_ruleSeekStatus;
        set => m_ruleSeekStatus = (int)value;
    }

    private long m_ruleSeekSimulationIterations;

    /// <summary>
    /// Number of rule sets tried.
    /// </summary>
    public long RuleSeekSimulationIterations
    {
        get => m_ruleSeekSimulationIterations;
        set => m_ruleSeekSimulationIterations = value;
    }

    private long m_simulationDurationTicks;

    /// <summary>
    /// TimeSpan the simulation has been running.
    /// </summary>
    public TimeSpan SimulationDuration
    {
        get => new (m_simulationDurationTicks);
        set => m_simulationDurationTicks = value.Ticks;
    }

    private long m_durationToFindBestScenario;

    /// <summary>
    /// Duration from the latest scenario creation since the simulation started.
    /// </summary>
    public TimeSpan DurationToFindBestScenario
    {
        get => new (m_durationToFindBestScenario);
        set => m_durationToFindBestScenario = value.Ticks;
    }

    private long m_simulationStartTicks;

    /// <summary>
    /// DateTime the simulation started.
    /// </summary>
    public DateTime SimulationStartTime
    {
        get => new (m_simulationStartTicks);
        set => m_simulationStartTicks = value.Ticks;
    }
    #endregion
}