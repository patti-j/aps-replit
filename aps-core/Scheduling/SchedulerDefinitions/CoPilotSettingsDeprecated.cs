namespace PT.SchedulerDefinitions;

/// <summary>
/// Stores System wide settings for RuleSeek. This is used in ScenarioManager.
/// </summary>
[Obsolete("This class has been deprecated and only exists for compatibility. The new class, CoPilotSettings, is defined in the ServerManagerSharedLib.")]
public class CoPilotSettingsDeprecated : IPTSerializable
{
    public const int UNIQUE_ID = 791;

    #region IPTSerializable Members
    public CoPilotSettingsDeprecated(IReader reader)
    {
        if (reader.VersionNumber >= 460)
        {
            m_bools = new BoolVector32(reader);
            m_performanceValues = new CoPilotPerformanceValuesDeprecated(reader);
        }

        #region 458
        else if (reader.VersionNumber >= 458)
        {
            m_bools = new BoolVector32(reader);
            long obsolete_IdleDuration;
            reader.Read(out obsolete_IdleDuration);
            m_performanceValues = new CoPilotPerformanceValuesDeprecated(reader);
        }
        #endregion
    }

    public void Serialize(IWriter writer)
    {
        m_bools.Serialize(writer);
        writer.Write(m_performanceValues.Value);
    }

    public CoPilotSettingsDeprecated()
    {
        m_bools = new BoolVector32(false);
        m_performanceValues = new CoPilotPerformanceValuesDeprecated();
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region MEMBERS
    private BoolVector32 m_bools;

    private const short c_EnabledIdx = 0;

    /// <summary>
    /// Whether to use Any parts of CoPilot automatically.
    /// Simulations could be started manually even if this is false.
    /// </summary>
    public bool Enabled
    {
        get => m_bools[c_EnabledIdx];
        set => m_bools[c_EnabledIdx] = value;
    }

    /// <summary>
    /// Values used to evaulate system performance and limit the number of maximum simultaneous simulations.
    /// </summary>
    private readonly CoPilotPerformanceValuesDeprecated m_performanceValues;

    public CoPilotPerformanceValuesDeprecated PerformanceValues => m_performanceValues;
    #endregion
}