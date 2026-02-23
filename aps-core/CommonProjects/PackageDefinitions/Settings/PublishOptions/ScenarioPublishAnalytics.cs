namespace PT.PackageDefinitions.Settings.PublishOptions
{
    public class ScenarioPublishAnalytics :ISettingData, ICloneable, IPTSerializable, IEquatable<ScenarioPublishAnalytics>
    {
        #region BoolVector32
        private BoolVector32 m_boolVector;
        private const int c_publishToDashboardIdx = 1;
        #endregion

        #region
        /// <summary>
        /// Whether to upload the data to Dashboard Server for reporting purposes.
        /// </summary>
        public bool PublishToDashboard
        {
            get => m_boolVector[c_publishToDashboardIdx];
            set { m_boolVector[c_publishToDashboardIdx] = value; }
        }
        #endregion

        public ScenarioPublishAnalytics()
        {
            PublishToDashboard = false;
        }
        /// <summary>
        /// </summary>
        /// <param name="a_publishToDashboard">Publish to Analytics Database</param>
        public ScenarioPublishAnalytics(bool a_publishToDashboard)
        {
            PublishToDashboard = a_publishToDashboard;
        }
        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }

        public ScenarioPublishAnalytics Clone()
        {
            return (ScenarioPublishAnalytics)MemberwiseClone();
        }
        #endregion

        #region IPTSerializable Members
        public ScenarioPublishAnalytics(IReader a_reader)
        {
            m_boolVector = new BoolVector32(a_reader);
        }

        public void Serialize(IWriter a_writer)
        {
            m_boolVector.Serialize(a_writer);
        }

        public int UniqueId => 9806;
        #endregion

        /// <summary>
        /// Returns if settings are identical
        /// <param name="a_options"></param>
        /// <returns></returns>
        /// </summary>
        public bool Equals(ScenarioPublishAnalytics a_options)
        {
            if (a_options == null)
            {
                return false;
            }

            return m_boolVector.Equals(a_options.m_boolVector);
        }

        public string SettingKey => Key;
        public string SettingCaption => "Publish Analytics";
        public string Description => "Publish Analytics";
        public string SettingsGroup => SettingGroupConstants.SystemSettingsGroup;
        public string SettingsGroupCategory => SettingGroupConstants.Publish;
        public static string Key = "scenarioSetting_ScenarioPublishAnalytics";
    }
}
