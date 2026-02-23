using PT.SchedulerDefinitions;

namespace PT.PackageDefinitions.Settings.PublishOptions
{
    public class ScenarioPublishAutomaticSettings : ISettingData, ICloneable, IEquatable<ScenarioPublishAutomaticSettings>
    {
        #region BoolVector32
        private BoolVector32 m_boolVector;
        private const short c_automaticPublishIdx = 1;
        #endregion

        #region Properties
        /// <summary>
        /// Whether to automatically publish this scenario after a delay
        /// </summary>
        public bool AutomaticPublish
        {
            get => m_boolVector[c_automaticPublishIdx];
            set
            {
                m_boolVector[c_automaticPublishIdx] = value;
            }
        }

        private TimeSpan m_automaticPublishDelay = TimeSpan.FromMinutes(10);

        /// <summary>
        /// The amount of time to wait after the last user action to start a publish
        /// </summary>
        public TimeSpan AutomaticPublishDelay
        {
            get => m_automaticPublishDelay;
            set
            {
                m_automaticPublishDelay = value;
            }
        }

        private EExportDestinations m_exportDestinations = EExportDestinations.BasedOnSystemOptions;

        /// <summary>
        /// The destination for automatic publishes
        /// </summary>
        public EExportDestinations ExportDestination
        {
            get => m_exportDestinations;
            set
            {
                m_exportDestinations = value;
            }
        }
        #endregion

        public ScenarioPublishAutomaticSettings()
        {
            AutomaticPublish = false;
        }

        /// <summary>
        /// </summary>
        /// <param name="a_automaticPublish">Automatic Publish</param>
        /// <param name="a_automaticPublishDelay"></param>
        /// <param name="a_exportDestinations"></param>
        public ScenarioPublishAutomaticSettings(bool a_automaticPublish, TimeSpan a_automaticPublishDelay,EExportDestinations a_exportDestinations)
        {
            AutomaticPublish = a_automaticPublish;
            AutomaticPublishDelay = a_automaticPublishDelay;
            ExportDestination = a_exportDestinations;
        }
        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }

        public ScenarioPublishAutomaticSettings Clone()
        {
            return (ScenarioPublishAutomaticSettings)MemberwiseClone();
        }
        #endregion

        #region IPTSerializable Members
        public ScenarioPublishAutomaticSettings(IReader a_reader)
        {
            m_boolVector = new BoolVector32(a_reader);
            a_reader.Read(out m_automaticPublishDelay);
            a_reader.Read(out int exportDestinations);
            m_exportDestinations = (EExportDestinations)exportDestinations;
        }

        public void Serialize(IWriter a_writer)
        {
            m_boolVector.Serialize(a_writer);
            a_writer.Write(m_automaticPublishDelay);
            a_writer.Write((int)m_exportDestinations);
        }

        public int UniqueId => 9807;
        #endregion

        /// <summary>
        /// Returns if settings are identical
        /// <param name="a_options"></param>
        /// <returns></returns>
        /// </summary>
        public bool Equals(ScenarioPublishAutomaticSettings a_options)
        {
            if (a_options == null)
            {
                return false;
            }

            return m_boolVector.Equals(a_options.m_boolVector);
        }

        public static string Key = "scenarioSetting_ScenarioPublishAutomaticSettings";
        public string SettingKey => Key;
        public string SettingCaption => "Publish Automatic Settings";
        public string Description => "Publish Automatic Settings";
        public string SettingsGroup => SettingGroupConstants.SystemSettingsGroup;
        public string SettingsGroupCategory => SettingGroupConstants.Publish;
    }
}
