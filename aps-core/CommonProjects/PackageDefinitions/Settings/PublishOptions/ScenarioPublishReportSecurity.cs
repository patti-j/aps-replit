namespace PT.PackageDefinitions.Settings.PublishOptions
{
    public class ScenarioPublishReportSecurity :ISettingData, ICloneable, IPTSerializable, IEquatable<ScenarioPublishReportSecurity>
    {
        #region BoolVector32
        private BoolVector32 m_boolVector;
        private const int c_reportsUseIntegratedSecurityIdx = 1;
        /// <summary>
        /// Sets the Report Viewer's login to use Integrated Security instead of database level security.
        /// </summary>
        public bool ReportsUseIntegratedSecurity
        {
            get => m_boolVector[c_reportsUseIntegratedSecurityIdx];
            set
            {
                m_boolVector[c_reportsUseIntegratedSecurityIdx] = value;
            }
        }
        #endregion
        private string m_reportSecurityUserName = "";

        /// <summary>
        /// The user name passed to reports that are NOT using Integrated Security.
        /// </summary>
        public string ReportSecurityUserName
        {
            get => m_reportSecurityUserName;
            set
            {
                m_reportSecurityUserName = value;
            }
        }

        private string m_reportSecurityPassword = "";

        /// <summary>
        /// The password passed to reports that are NOT using Integrated Security.
        /// </summary>
        public string ReportSecurityPassword
        {
            get => m_reportSecurityPassword;
            set
            {
                m_reportSecurityPassword = value;
            }
        }

        public ScenarioPublishReportSecurity()
        {
            ReportsUseIntegratedSecurity = false;
        }
        public ScenarioPublishReportSecurity(bool a_reportUseIntegratedSecurity, string a_reportSecurityUsername, string a_reportSecurityPassword)
        {
            ReportsUseIntegratedSecurity = a_reportUseIntegratedSecurity;
            ReportSecurityUserName = a_reportSecurityUsername; 
            ReportSecurityPassword = a_reportSecurityPassword;
        }

        public ScenarioPublishReportSecurity(IReader a_reader)
        {
            a_reader.Read(out m_reportSecurityUserName);
            a_reader.Read(out m_reportSecurityPassword);

            m_boolVector = new BoolVector32(a_reader);
        }
        public void Serialize(IWriter a_writer)
        {
            a_writer.Write(m_reportSecurityUserName);
            a_writer.Write(m_reportSecurityPassword);
            
            m_boolVector.Serialize(a_writer);
        }

        public int UniqueId => 9805;
        /// <summary>
        /// Returns if settings are identical
        /// <param name="a_options"></param>
        /// <returns></returns>
        /// </summary>
        public bool Equals(ScenarioPublishReportSecurity a_options)
        {
            if (a_options == null)
            {
                return false;
            }

            return m_boolVector.Equals(a_options.m_boolVector) && 
                   a_options.ReportSecurityPassword == m_reportSecurityPassword &&
                   a_options.ReportSecurityUserName == m_reportSecurityUserName;
        }
        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }

        public ScenarioPublishReportSecurity Clone()
        {
            return (ScenarioPublishReportSecurity)MemberwiseClone();
        }
        #endregion

        public string SettingKey => Key;
        public string SettingCaption => "Publish Report Security";
        public string Description => "Publish Reports";
        public string SettingsGroup => SettingGroupConstants.SystemSettingsGroup;
        public string SettingsGroupCategory => SettingGroupConstants.Publish;
        public static string Key = "scenarioSetting_ScenarioPublishReportSecurity";
    }
}
