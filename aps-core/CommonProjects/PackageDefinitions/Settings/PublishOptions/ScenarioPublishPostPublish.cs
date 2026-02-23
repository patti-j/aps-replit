namespace PT.PackageDefinitions.Settings.PublishOptions
{
    public class ScenarioPublishPostPublish :ISettingData, ICloneable, IPTSerializable, IEquatable<ScenarioPublishPostPublish>
    {
        #region BoolVector32
        private BoolVector32 m_boolVector;

        private const int c_runStoredProcedureAfterPublishIdx = 1;
        private const int c_runMicrosoftProjectStoredProcedureAfterPublishIdx = 2;
        private const int c_runProgramAfterPublishIdx = 3;
        /// <summary>
        /// Whether to run a stored procedure in the ERP database after a Scenario is Published to the SQL database.
        /// </summary>
        public bool RunStoredProcedureAfterPublish
        {
            get => m_boolVector[c_runStoredProcedureAfterPublishIdx];
            set
            {
                m_boolVector[c_runStoredProcedureAfterPublishIdx] = value;
            }
        }

        private string m_postPublishStoredProcedureName = "APS_Publish";

        /// <summary>
        /// The name of the stored procedure to call in the ERP database after publishing the scenario to the SQL database.
        /// This is only called if Run Stored Procedure After Publish option is enabled.
        /// The stored procedure is passed two parameters:
        /// PublishDate: (read-only) DateTime value that corresponds to the current time at which the data was exported and the PublishDate column in the Schedules table.
        /// Message: (return value) VarChar 8,000 characters that can be set by the stored procedure as a displayed return message to the user.  Displayed in the Log Viewer.
        /// Return 0 to indicate success. (no message is displayed)
        /// Return 1 to indicate failure.
        /// </summary>
        public string PostPublishStoredProcedureName
        {
            get => m_postPublishStoredProcedureName;
            set
            {
                m_postPublishStoredProcedureName = value;
            }
        }
        /// <summary>
        /// Whether to run a program after performing a Full or Net-Change Publish.
        /// This can be used to call an executable or batch file, etc. that performs additional actions with the published data such as update an ERP system.
        /// </summary>
        public bool RunProgramAfterPublish
        {
            get => m_boolVector[c_runProgramAfterPublishIdx];
            set
            {
                m_boolVector[c_runProgramAfterPublishIdx] = value;
            }
        }

        private string m_runProgramPath = "";

        /// <summary>
        /// The full path to the exe or other file to call after a publish.
        /// </summary>
        public string RunProgramPath
        {
            get => m_runProgramPath;
            set
            {
                m_runProgramPath = value;
            }
        }

        private string m_runProgramCommandLine = "";

        /// <summary>
        /// The optional command line to pass to the program run after publishes.
        /// </summary>
        public string RunProgramCommandLine
        {
            get => m_runProgramCommandLine;
            set
            {
                m_runProgramCommandLine = value;
            }
        }

        /// <summary>
        /// If true then a stored procedure in the PlanetTogether database is run to copy scenario data to tables that can be used to import to Microsoft Project.
        /// </summary>
        public bool RunMicrosoftProjectStoredProcedureAfterPublish
        {
            get => m_boolVector[c_runMicrosoftProjectStoredProcedureAfterPublishIdx];
            set
            {
                m_boolVector[c_runMicrosoftProjectStoredProcedureAfterPublishIdx] = value;
            }
        }
        #endregion
        public ScenarioPublishPostPublish(bool a_runMicrosoftProject, bool a_runProgram, bool a_runStoredProc, string a_postPublishStoredProcedureName,string a_runProgramPath, string a_runProgramCommandLine)
        {
            RunMicrosoftProjectStoredProcedureAfterPublish = a_runMicrosoftProject;
            RunProgramAfterPublish = a_runProgram;
            RunStoredProcedureAfterPublish = a_runStoredProc;
            RunProgramPath = a_runProgramPath;
            RunProgramCommandLine = a_runProgramCommandLine;
            PostPublishStoredProcedureName = a_postPublishStoredProcedureName;
        }
        public ScenarioPublishPostPublish()
        {
            RunMicrosoftProjectStoredProcedureAfterPublish = false;
            RunProgramAfterPublish = false;
            RunStoredProcedureAfterPublish = false;
        }
       
        #region IPTSerializable Members
        public ScenarioPublishPostPublish(IReader a_reader)
        {
            m_boolVector = new BoolVector32(a_reader);
            a_reader.Read(out m_postPublishStoredProcedureName);
            a_reader.Read(out m_runProgramPath);
            a_reader.Read(out m_runProgramCommandLine);
        }
        public void Serialize(IWriter a_writer)
        {
            m_boolVector.Serialize(a_writer);
            a_writer.Write(m_postPublishStoredProcedureName);
            a_writer.Write(m_runProgramPath);
            a_writer.Write(m_runProgramCommandLine);
        }

        public int UniqueId => 9803;
        #endregion
        /// <summary>
        /// Returns if settings are identical
        /// <param name="a_options"></param>
        /// <returns></returns>
        /// </summary>
        public bool Equals(ScenarioPublishPostPublish a_options)
        {
            if (a_options == null)
            {
                return false;
            }

            return m_boolVector.Equals(a_options.m_boolVector) && m_postPublishStoredProcedureName == a_options.m_postPublishStoredProcedureName
                                                               && m_runProgramCommandLine == a_options.m_runProgramCommandLine
                                                               && m_runProgramPath == a_options.m_runProgramPath;
        }
        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }

        public ScenarioPublishPostPublish Clone()
        {
            return (ScenarioPublishPostPublish)MemberwiseClone();
        }
        #endregion

        public string SettingKey => Key;
        public string SettingCaption => "Publish Post Publish Stored Procedures";
        public string Description => "Publish Post Publish Stored Procedures";
        public string SettingsGroup => SettingGroupConstants.SystemSettingsGroup;
        public string SettingsGroupCategory => SettingGroupConstants.Publish;
        public static string Key = "scenarioSetting_ScenarioPublishPostPublish";
    }
}
