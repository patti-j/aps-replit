namespace PT.PackageDefinitions.Settings.PublishOptions
{
    public class ScenarioPublishHistory :ISettingData, ICloneable, IPTSerializable, IEquatable<ScenarioPublishHistory>
    {
        #region BoolVector32
        private BoolVector32 m_boolVector;
        private const int c_enableHistoryIdx = 0;

        #endregion

        public ScenarioPublishHistory()
        {
            EnableHistory = false;
        }
        /// <summary>
        /// Whether to keep any Scenario history.
        /// </summary>
        public bool EnableHistory
        {
            get => m_boolVector[c_enableHistoryIdx];
            set
            {
                m_boolVector[c_enableHistoryIdx] = value;
            }
        }
        private TimeSpan m_publishHorizonSpan = TimeSpan.FromDays(365);

        /// <summary>
        /// Only activity that starts before Clock plus this value is exported.
        /// </summary>
        public TimeSpan PublishHorizonSpan
        {
            get => m_publishHorizonSpan;
            set
            {
                m_publishHorizonSpan = value;
            }
        }

        private TimeSpan m_historyHorizonSpan = TimeSpan.FromDays(1);

        /// <summary>
        /// Activity that starts after Clock plus this value is deleted.
        /// </summary>
        public TimeSpan HistoryHorizonSpan
        {
            get => m_historyHorizonSpan;
            set
            {
                m_historyHorizonSpan = value;
            }
        }

        private TimeSpan m_historyMaxAge = TimeSpan.FromDays(7);

        /// <summary>
        /// Scenarios with a PublishDate older than the current PC date minus this value are deleted.
        /// </summary>
        public TimeSpan HistoryMaxAge
        {
            get => m_historyMaxAge;
            set
            {
                m_historyMaxAge = value;
            }
        }

        private TimeSpan m_whatIfHistoryMaxAge = TimeSpan.FromDays(2);

        /// <summary>
        /// What-If Scenarios with a PublishDate older than the current PC date minus this value are deleted.
        /// </summary>
        public TimeSpan WhatIfHistoryMaxAge
        {
            get => m_whatIfHistoryMaxAge;
            set
            {
                m_whatIfHistoryMaxAge = value;
            }
        }

        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }

        public ScenarioPublishHistory Clone()
        {
            return (ScenarioPublishHistory)MemberwiseClone();
        }
        #endregion

        #region IPTSerializable Members
        public ScenarioPublishHistory(IReader a_reader)
        {
            a_reader.Read(out m_historyHorizonSpan);
            a_reader.Read(out m_historyMaxAge);
            a_reader.Read(out m_publishHorizonSpan);
            a_reader.Read(out m_whatIfHistoryMaxAge);
            m_boolVector = new BoolVector32(a_reader);
        }

        public ScenarioPublishHistory(bool a_enableHistory, TimeSpan a_historyHorizonSpan, TimeSpan a_historyMaxAge, TimeSpan a_whatIfHistoryMaxAge)
        {
            EnableHistory = a_enableHistory;
            m_historyHorizonSpan = a_historyHorizonSpan;
            m_historyMaxAge = a_historyMaxAge;
            m_whatIfHistoryMaxAge = a_whatIfHistoryMaxAge;
        }

        public void Serialize(IWriter a_writer)
        {
            a_writer.Write(m_historyHorizonSpan);
            a_writer.Write(m_historyMaxAge);
            a_writer.Write(m_publishHorizonSpan);
            a_writer.Write(m_whatIfHistoryMaxAge);
            m_boolVector.Serialize(a_writer);
        }

        public int UniqueId => 9801;
        #endregion
        /// <summary>
        /// Returns True if settings are identical
        /// <param name="a_options"></param>
        /// <returns></returns>
        /// </summary>
        public bool Equals(ScenarioPublishHistory a_options)
        {
            if (a_options == null)
            {
                return false;
            }

            return m_boolVector.Equals(a_options.m_boolVector) &&
                   PublishHorizonSpan == a_options.PublishHorizonSpan &&
                   HistoryHorizonSpan == a_options.HistoryHorizonSpan &&
                   HistoryMaxAge == a_options.HistoryMaxAge &&
                   WhatIfHistoryMaxAge == a_options.WhatIfHistoryMaxAge;
        }
        public string SettingKey => Key;
        public string SettingCaption => "Publish History";
        public string Description => "Publish History";
        public string SettingsGroup => SettingGroupConstants.SystemSettingsGroup;
        public string SettingsGroupCategory => SettingGroupConstants.Publish;
        public static string Key = "scenarioSetting_ScenarioPublishScheduleHistory";
    }
}
