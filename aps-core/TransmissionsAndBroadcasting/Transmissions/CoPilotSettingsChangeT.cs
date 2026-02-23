using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Provides a new CoPilot settings for the ScenarioManager
/// Only serializes the settings if they have been updated.
/// </summary>
public class CoPilotSettingsChangeT : ScenarioBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 767;

    #region IPTSerializable Members
    public CoPilotSettingsChangeT(IReader reader) : base(reader)
    {
        if (reader.VersionNumber >= 12009)
        {
            bool readSettings;
            reader.Read(out readSettings);
            if (readSettings)
            {
                m_ruleSeekSettings = new RuleSeekSettings(reader);
                m_ruleSeekSettingsChanged = true;
            }

            reader.Read(out readSettings);
            if (readSettings)
            {
                m_insertJobsSettings = new InsertJobsSettings(reader);
                m_insertJobsSettingsChanged = true;
            }
        }
        else if (reader.VersionNumber >= 458)
        {
            bool readSettings;
            reader.Read(out readSettings);
            if (readSettings)
            {
                new CoPilotSettingsDeprecated(reader);
            }

            reader.Read(out readSettings);
            if (readSettings)
            {
                m_ruleSeekSettings = new RuleSeekSettings(reader);
                m_ruleSeekSettingsChanged = true;
            }

            reader.Read(out readSettings);
            if (readSettings)
            {
                m_insertJobsSettings = new InsertJobsSettings(reader);
                m_insertJobsSettingsChanged = true;
            }
        }
        else if (reader.VersionNumber >= 452)
        {
            m_ruleSeekSettings = new RuleSeekSettings(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        if (m_ruleSeekSettings != null)
        {
            writer.Write(true);
            m_ruleSeekSettings.Serialize(writer);
        }
        else
        {
            writer.Write(false);
        }

        if (m_insertJobsSettings != null)
        {
            writer.Write(true);
            m_insertJobsSettings.Serialize(writer);
        }
        else
        {
            writer.Write(false);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CoPilotSettingsChangeT() { }

    /// <summary>
    /// Bool representing if the settings have changed and should be updated
    /// If this is false, then the settings are null
    /// </summary>
    private bool m_ruleSeekSettingsChanged;

    public bool RuleSeekSettingsChanged => m_ruleSeekSettingsChanged;

    /// <summary>
    /// Bool representing if the settings have changed and should be updated
    /// If this is false, then the settings are null
    /// </summary>
    private bool m_insertJobsSettingsChanged;

    public bool InsertJobsSettingsChanged => m_insertJobsSettingsChanged;

    /// <summary>
    /// New settings that should be used. Will be null if RuleSeekSettingsChanged is false
    /// </summary>
    private RuleSeekSettings m_ruleSeekSettings;

    public RuleSeekSettings RuleSeekSettings
    {
        get => m_ruleSeekSettings;
        set
        {
            m_ruleSeekSettings = value;
            m_ruleSeekSettingsChanged = true;
        }
    }

    /// <summary>
    /// New settings that should be used. Will be null if InsertJobsSettingsChanged is false
    /// </summary>
    private InsertJobsSettings m_insertJobsSettings;

    public InsertJobsSettings InsertJobsSettings
    {
        get => m_insertJobsSettings;
        set
        {
            m_insertJobsSettings = value;
            m_insertJobsSettingsChanged = true;
        }
    }
}