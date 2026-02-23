namespace PT.PackageDefinitions.Settings.PublishOptions
{
    public class ScenarioPublishNetChange :ISettingData, ICloneable, IEquatable<ScenarioPublishNetChange>
    {
        #region BoolVector32
        private const int c_enableNetChangePublishingIdx = 1;
        private const int c_runStoredProcedureAfterPublishIdx = 2;

        private BoolVector32 m_boolVector;
        /// <summary>
        /// Whether to calculate which Activity blocks have changed after every simulation.
        /// If checked then whenever the schedule is changed a stored procedure can be called to update an external system
        /// with all schedule changes.
        /// </summary>
        public bool NetChangePublishingEnabled
        {
            get => m_boolVector[c_enableNetChangePublishingIdx];
            set
            {
                m_boolVector[c_enableNetChangePublishingIdx] = value;
            }
        }

        /// <summary>
        /// Whether to run a stored procedure after performing a Net Change Publish.
        /// This can be used to call a stored procedure that performs additional actions with the publish data such as update an ERP system.
        /// </summary>
        public bool RunStoredProcedureAfterNetChangePublish
        {
            get => m_boolVector[c_runStoredProcedureAfterPublishIdx];
            set
            {
                m_boolVector[c_runStoredProcedureAfterPublishIdx] = value;
            }
        }
        private string m_netChangeStoredProcedureName = "APS_NetChangePublish";

        /// <summary>
        /// The name of the stored procedure to call in the ERP database after any schedule change occurs.
        /// This is only called if the Net-Change publishing is enabled.
        /// The stored procedure is passed two parameters:
        /// PublishDate: (read-only) DateTime value that corresponds to the current time at which the data was exported and the PublishDate column in the Schedules table.
        /// Message: (return value) VarChar 8,000 characters that can be set by the stored procedure as a displayed return message to the user.  Displayed in the Log Viewer.
        /// Return 0 to indicate success. (no message is displayed)
        /// Return 1 to indicate failure.
        /// Return 2 to request an undo back to before the action that triggered the Net Change export.
        /// </summary>
        public string NetChangeStoredProcedureName
        {
            get => m_netChangeStoredProcedureName;
            set
            {
                m_netChangeStoredProcedureName = value;
            }
        }
        #endregion

        public ScenarioPublishNetChange()
        {
            RunStoredProcedureAfterNetChangePublish = false;
            NetChangePublishingEnabled = false;
        }
        public ScenarioPublishNetChange(bool a_runStoredProcAfterNetChange,bool a_netChangePublishEnabled, string a_netChangeStoredProcName)
        {
            RunStoredProcedureAfterNetChangePublish = a_runStoredProcAfterNetChange;
            NetChangePublishingEnabled = a_netChangePublishEnabled;
            NetChangeStoredProcedureName = a_netChangeStoredProcName;
        }

      
        #region IPTSerializable Members
        public ScenarioPublishNetChange(IReader a_reader)
        {
            m_boolVector = new BoolVector32(a_reader);
            a_reader.Read(out m_netChangeStoredProcedureName);
        }
        public void Serialize(IWriter a_writer)
        {
            m_boolVector.Serialize(a_writer);
            a_writer.Write(m_netChangeStoredProcedureName);
        }

        public int UniqueId => 9804;
        #endregion
        /// <summary>
        /// Returns if settings are identical
        /// <param name="a_options"></param>
        /// <returns></returns>
        /// </summary>
        public bool Equals(ScenarioPublishNetChange a_options)
        {
            if (a_options == null)
            {
                return false;
            }

            return m_boolVector.Equals(a_options.m_boolVector) && m_netChangeStoredProcedureName == a_options.m_netChangeStoredProcedureName;
        }
        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }

        public ScenarioPublishNetChange Clone()
        {
            return (ScenarioPublishNetChange)MemberwiseClone();
        }
        #endregion

        public string SettingKey => Key;
        public string SettingCaption => "Publish Net Change";
        public string Description => "Publish Net Change";
        public string SettingsGroup => SettingGroupConstants.SystemSettingsGroup;
        public string SettingsGroupCategory => SettingGroupConstants.Publish;
        public static string Key = "scenarioSetting_ScenarioPublishNetChange";
    }
}
