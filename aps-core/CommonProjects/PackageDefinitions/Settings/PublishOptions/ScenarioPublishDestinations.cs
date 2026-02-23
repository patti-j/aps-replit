namespace PT.PackageDefinitions.Settings.PublishOptions
{
    public class ScenarioPublishDestinations :ISettingData, ICloneable, IPTSerializable,IEquatable<ScenarioPublishDestinations>
    {
        #region BoolVector32
        private BoolVector32 m_boolVector;
        private const int PublishToSqlIdx = 1;
        private const int PublishToCustomDllIdx = 2;
        private const int PublishToXmlIdx = 3;
        private const int PublishAllActivitesForMOIdx = 4;
        #endregion

        #region
        /// <summary>
        /// Whether to export to a SQL database.
        /// </summary>
        public bool PublishToSQL
        {
            get => m_boolVector[PublishToSqlIdx];
            set { m_boolVector[PublishToSqlIdx] = value; }
        }

        /// <summary>
        /// Whether to export to an XML file.
        /// </summary>
        public bool PublishToXML
        {
            get => m_boolVector[PublishToXmlIdx];
            set { m_boolVector[PublishToXmlIdx] = value; }
        }

        /// <summary>
        /// Whether to export to a custom dll.
        /// </summary>
        public bool PublishToCustomDll
        {
            get => m_boolVector[PublishToCustomDllIdx];
            set { m_boolVector[PublishToCustomDllIdx] = value; }
        }

        /// <summary>
        /// Whether to publish All Activities that are part of a Manufacturing Order if any of its Activities are Published.
        /// </summary>
        public bool PublishAllActivitesForMO
        {
            get => m_boolVector[PublishAllActivitesForMOIdx];
            set { m_boolVector[PublishAllActivitesForMOIdx] = value; }
        }

        #endregion

        public ScenarioPublishDestinations()
        {
            PublishAllActivitesForMO = false;
            PublishToCustomDll = false;
            PublishToSQL = false;
            PublishToXML = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a_publishAllActivitiesForMO">Publish all activities for a Manufacturing Order if any are published</param>
        /// <param name="a_publishToCustomDLL">Publish to a custom program</param>
        /// <param name="a_publishSQL">Publish to SQL</param>
        /// <param name="a_publishToXml">Publish to XML</param>
        public ScenarioPublishDestinations(bool a_publishAllActivitiesForMO, bool a_publishToCustomDLL, bool a_publishSQL,bool a_publishToXml)
        {
            PublishAllActivitesForMO = a_publishAllActivitiesForMO;
            PublishToCustomDll = a_publishToCustomDLL;
            PublishToSQL = a_publishSQL;
            PublishToXML = a_publishToXml;
        }
        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }

        public ScenarioPublishDestinations Clone()
        {
            return (ScenarioPublishDestinations)MemberwiseClone();
        }
        #endregion
        
        #region IPTSerializable Members
        public ScenarioPublishDestinations(IReader a_reader)
        {
            m_boolVector = new BoolVector32(a_reader);
        }

        public void Serialize(IWriter a_writer)
        {
            m_boolVector.Serialize(a_writer);
        }

        public int UniqueId => 9800;
        #endregion

        /// <summary>
        /// Returns if settings are identical
        /// <param name="a_options"></param>
        /// <returns></returns>
        /// </summary>
        public bool Equals(ScenarioPublishDestinations a_options)
        {
            if (a_options == null)
            {
                return false;
            }

            return m_boolVector.Equals(a_options.m_boolVector);
        }

        public string SettingKey => Key;
        public string SettingCaption => "Publish Destinations";
        public string Description => "Publish Destinations";
        public string SettingsGroup => SettingGroupConstants.SystemSettingsGroup;
        public string SettingsGroupCategory => SettingGroupConstants.Publish;
        public static string Key = "scenarioSetting_ScenarioPublishDestinations";
    }
}
