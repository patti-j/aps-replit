namespace PT.SchedulerDefinitions;

/// <summary>
/// Stores CoPilot settings for InsertNewJobs simulation. This is used in ScenarioManager and Modified in UI.
/// </summary>
public class InsertJobsSettings : IPTSerializable
{
    public const int UNIQUE_ID = 777;

    #region IPTSerializable Members
    public InsertJobsSettings(IReader reader)
    {
        if (reader.VersionNumber >= 476)
        {
            m_bools = new BoolVector32(reader);

            short enumInt;
            reader.Read(out enumInt);
            m_simResults = (SimulationResultsType)enumInt;
            reader.Read(out m_kpiToRun);
            reader.Read(out m_kpiThreshold);
            reader.Read(out m_minAttemptInterval);
        }
        else if (reader.VersionNumber >= 467)
        {
            m_bools = new BoolVector32(reader);

            short enumInt;
            reader.Read(out enumInt);
            m_simResults = (SimulationResultsType)enumInt;
            reader.Read(out m_kpiToRun);
            reader.Read(out m_kpiThreshold);
        }
        else if (reader.VersionNumber >= 453)
        {
            m_bools = new BoolVector32(reader);

            short enumInt;
            reader.Read(out enumInt);
            m_simResults = (SimulationResultsType)enumInt;
            reader.Read(out m_kpiToRun);
        }
    }

    public void Serialize(IWriter writer)
    {
        m_bools.Serialize(writer);
        writer.Write((short)m_simResults);
        writer.Write(m_kpiToRun);
        writer.Write(m_kpiThreshold);
        writer.Write(m_minAttemptInterval);
    }

    public InsertJobsSettings()
    {
        m_bools = new BoolVector32(false);
        m_simResults = SimulationResultsType.CreateScenario;
        m_kpiToRun = "    ";
    }

    /// <summary>
    /// Compares two settings to see if they are equal.
    /// </summary>
    /// <returns>True if the settings are different</returns>
    public bool CompareSettingsForChanges(InsertJobsSettings a_newSettings)
    {
        if (Enabled != a_newSettings.Enabled || KpiToRun != a_newSettings.KpiToRun || SimulationResultType != a_newSettings.SimulationResultType || InsertDoNotSchedule != a_newSettings.InsertDoNotSchedule || InsertExcludedDoNotSchedule != a_newSettings.InsertExcludedDoNotSchedule || InsertAfterMaxReleaseDate != a_newSettings.InsertAfterMaxReleaseDate || CheckAlternatePaths != a_newSettings.CheckAlternatePaths || NoAnchorDrift != a_newSettings.NoAnchorDrift || KpiThresholdValue != a_newSettings.KpiThresholdValue || MinimumAttemptInterval != a_newSettings.MinimumAttemptInterval)
        {
            return true;
        }

        return false;
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region MEMBERS
    //NOTE: Make sure to update CompareSettingsForChanges() when adding new members.

    private BoolVector32 m_bools;

    private const short c_EnabledIdx = 0;
    private const short c_DoNotSchedule = 1;
    private const short c_ScheduleExcluded = 2;
    private const short c_InsertAfterMaxReleaseDate = 3;
    private const short c_CheckAlternatePaths = 4;
    private const short c_UseKpiThreshold = 5;
    private const short c_NoAnchorDriftIdx = 6;

    /// <summary>
    /// Whether to use RuleSeek
    /// </summary>
    public bool Enabled
    {
        get => m_bools[c_EnabledIdx];
        set => m_bools[c_EnabledIdx] = value;
    }

    /// <summary>
    /// Whether to use insert jobs marked as Do Not Schedule
    /// </summary>
    public bool InsertDoNotSchedule
    {
        get => m_bools[c_DoNotSchedule];
        set => m_bools[c_DoNotSchedule] = value;
    }

    /// <summary>
    /// Whether to use insert excluded jobs marked as Do Not Schedule
    /// </summary>
    public bool InsertExcludedDoNotSchedule
    {
        get => m_bools[c_ScheduleExcluded];
        set => m_bools[c_ScheduleExcluded] = value;
    }

    /// <summary>
    /// Whether to not expedite before the max release date of MOs
    /// </summary>
    public bool InsertAfterMaxReleaseDate
    {
        get => m_bools[c_InsertAfterMaxReleaseDate];
        set => m_bools[c_InsertAfterMaxReleaseDate] = value;
    }

    /// <summary>
    /// Whether to calculate KPIs and check scheduled dates on each path in a job.
    /// </summary>
    public bool CheckAlternatePaths
    {
        get => m_bools[c_CheckAlternatePaths];
        set => m_bools[c_CheckAlternatePaths] = value;
    }

    /// <summary>
    /// Whether to not allow scheduled jobs to increase anchor drift
    /// </summary>
    public bool NoAnchorDrift
    {
        get => m_bools[c_NoAnchorDriftIdx];
        set => m_bools[c_NoAnchorDriftIdx] = value;
    }

    private double m_kpiThreshold;

    /// <summary>
    /// This % value used to calculate wheter a KPI is within an acceptable range
    /// </summary>
    public double KpiThresholdValue
    {
        get => m_kpiThreshold;
        set => m_kpiThreshold = value;
    }

    private SimulationResultsType m_simResults;

    /// <summary>
    /// How to report Results
    /// </summary>
    public SimulationResultsType SimulationResultType
    {
        get => m_simResults;
        set => m_simResults = value;
    }

    private string m_kpiToRun;

    /// <summary>
    /// Name of the KPI to calculate a score for during simulations
    /// </summary>
    public string KpiToRun
    {
        get => m_kpiToRun;
        set => m_kpiToRun = value;
    }

    private long m_minAttemptInterval;

    /// <summary>
    /// The minimum time time between expedite times. If there are times within this interval, one will be removed.
    /// </summary>
    public TimeSpan MinimumAttemptInterval
    {
        get => new (m_minAttemptInterval);
        set => m_minAttemptInterval = value.Ticks;
    }
    #endregion

    public enum SimulationResultsType { CreateScenario }
}