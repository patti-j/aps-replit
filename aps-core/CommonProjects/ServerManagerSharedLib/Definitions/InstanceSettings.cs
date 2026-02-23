using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.ServerManagerSharedLib.Utils;

namespace PT.ServerManagerSharedLib.Definitions
{
    [DataContract]
    public class InstanceSettings
    {
        public InstanceSettings()
        {
        }

        public InstanceSettings(Guid newGuid)
        {
            InstanceId = newGuid.ToString();
        }

        DateTime m_creationDate;

        /// <summary>
        /// This is the insertion time when the instance is born.
        /// </summary>
        [DataMember]
        public DateTime CreationDate
        {
            get { return m_creationDate; }
            set { m_creationDate = value; }
        }

        string m_ScenarioFile;

        /// <summary>
        /// This is the insertion time when the instance is born.
        /// </summary>
        [DataMember]
        public string ScenarioFile
        {
            get { return m_ScenarioFile; }
            set { m_ScenarioFile = value; }
        }

        int m_startServicesTimeoutSeconds = 120;

        /// <summary>
        /// This is the time in minutes ServerManager allows for all services to start before cancelling the start operation.
        /// </summary>
        [DataMember]
        [DefaultValue(120)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]

        public int StartServicesTimeoutSeconds
        {
            get { return m_startServicesTimeoutSeconds; }
            set
            {
                if (value < 0)
                {
                    m_startServicesTimeoutSeconds = 0;
                }
                else
                {
                    m_startServicesTimeoutSeconds = value;
                }
            }
        }

        string m_dashboardConnStr = "";
        /// <summary>
        /// Connection string used to publish data to Dashboard Server
        /// </summary>
        [DataMember]
        public string DashboardConnString
        {
            get { return string.IsNullOrEmpty(m_dashboardConnStr) ? string.Empty : Encryption.Decrypt(m_dashboardConnStr); }
            set { m_dashboardConnStr = Encryption.Encrypt(value); }
        }

        /// <summary>
        /// A sql command to run after publishing. This can be used to complement or customize data with ERP data.
        /// </summary>
        [DataMember]
        public string DashboardSqlToRun { get; set; } = "";

        int m_dashboardTimeout = 120; // this is the default from BulkCopy
        /// <summary>
        /// number of seconds to allow for each table in publish database to be published to Dashboard
        /// </summary>
        [DataMember]
        public int DashboardTimeout
        {
            get { return m_dashboardTimeout; }
            set { m_dashboardTimeout = Math.Max(0, value); }
        }

        [DataMember]
        int m_scenarioLimit = 5; 
        [DataMember]
        public int ScenarioLimit
        {
            get { return m_scenarioLimit; }
            set { m_scenarioLimit = Math.Max(1, value); }
        }

        int m_dashboardPublishFreq = 0;
        /// <summary>
        /// If 0, automatic publish is off. Otherwise, APS will trigger a new publish to Dashboard every so many minutes.
        /// </summary>
        [DataMember]
        public int DashboardPublishFrequency
        {
            get { return m_dashboardPublishFreq; }
            set { m_dashboardPublishFreq = Math.Max(0, value); }
        }

        const string c_defaultDashboardReportURL = "http://dashboard.planettogether.com";

        /// <summary>
        /// Address to where dashboard reports reside
        /// </summary>
        [DataMember]
        public string DashboardReportURL { get; set; } = c_defaultDashboardReportURL;

        [DataMember]
        public string AnalyticsURL { get; set; } = "";

        /// <summary>
        /// Address to where dashboard reports reside
        /// </summary>
        [DataMember]
        public string PreImportURL { get; set; } = "";


        /// <summary>
        /// Address to where dashboard reports reside
        /// </summary>
        [DataMember]
        public string PostImportURL { get; set; } = "";


        /// <summary>
        /// Address to where dashboard reports reside
        /// </summary>
        [DataMember]
        public string PreExportURL { get; set; } = "";


        /// <summary>
        /// Address to where dashboard reports reside
        /// </summary>
        [DataMember]
        public string PostExportURL { get; set; } = "";

        /// <summary>
        /// this controls whether data from this instance can be sent to PlanetTogether.
        /// </summary>
        [DataMember]
        public bool DiagnosticsEnabled { get; set; } = false;

        /// <summary>
        /// GUID uniquely identifying the instance.
        /// </summary>
        [DataMember]
        public string InstanceId { get; set; }

        [JsonIgnore]
        [Obsolete("Use ServerTimeZoneId to store string TimeZoneInfo.Id, and ServerTimeZone to get readonly full TimeZoneInfo.")]
        public string TimeZone => ServerTimeZoneId;

        /// <summary>
        /// Stores TimeZoneInfo.Id, for easy translation to JS frontend and the db.
        /// Used to construct <see cref="ServerTimeZone"/>
        /// </summary>
        [DataMember]
        public string ServerTimeZoneId { get; set; } // has to be public for serialization

        /// <summary>
        /// Returns the TimeZoneInfo data associated with the set TimeZone Id string in ServerTimeZoneId.
        /// </summary>
        /// <returns></returns>
        public TimeZoneInfo GetServerTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(ServerTimeZoneId);
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }

        /// <summary>
        /// Connection string to access the database of Integration Configurations under the new model.
        /// </summary>
        [DataMember]
        public string IntegrationV2Connection { get; set; }


        #region LogEmail
        public bool SendLogEmails
        {
            get { return !string.IsNullOrEmpty(LogEmailToAddresses) && !string.IsNullOrEmpty(LogEmailToAddresses); }
        }

        /// <summary>
        /// Network address to the SMTP server to use to send emails.
        /// </summary>
        public string SmtpServerAddress { get; set; }

        public int SmtpServerPortNbr { get; set; } = 25;

        /// <summary>
        /// Network address to the SMTP server to use to send emails.
        /// </summary>
        public string SmtpFromEmailAddress { get; set; }

        public bool SmtpUseSsl { get; set; }

        public string SmtpUsername { get; set; }

        public string SmtpEncryptedPassword { get; set; }

        /// <summary>
        /// The email to use when sending log emails.
        /// </summary>
        string DefaultLogEmailFromAddress => "public@planettogether.com";

        /// <summary>
        /// Specifies to whom emails should be sent when logs are automatically e-mailed.
        /// Separate multiple e-mails addresses with semi-colon.
        /// </summary>
        public string LogEmailToAddresses { get; set; }

        public string LogEmailSubjectPrefix { get; set; }


        public string SupportEmailToAddresses { get; set; }

        public string SupportEmailSubjectPrefix { get; set; }
        [DataMember]
        public string SecurityToken { get; set; }

        #endregion Email

        #region Service Settings
        [DataMember]
        public SystemServiceSettings SystemServiceSettings { get; set; }
        [DataMember]
        public InterfaceServiceSettings InterfaceServiceSettings { get; set; }
        [DataMember]
        public ErpDatabaseSettings ErpDatabaseSettings { get; set; }
        //TODO: Is this used?  If not remove.
        [DataMember]
        public ApiSettings ApiSettings { get; set; }

        public string ServerManagerName { get; set; }
        [DataMember]
        public PackageSettings PackageSettings { get; set; }
        [DataMember]
        public string SsoValidationCertificateThumbprint { get; set; }
        [DataMember]
        public bool AllowSsoLogin { get; set; }

        [DataMember]
        public CoPilotSettings CoPilotSettings { get; set; } = new CoPilotSettings();

        [DataMember]
        public PublishSettings PublishSettings { get; set; } = new PublishSettings();

        public ServerWideInstanceSettings ServerWideSettings { get; set; }

        [DataMember]
        public bool UseSentry { get; set; } = true;

        /// <summary>
        /// Dsn value for our PT Sentry environment for logging core errors. Provided as a constant by WebApp API.
        /// </summary>
        [DataMember]
        public string SentryDsn { get; set; } // Resolved by webapp api
        #endregion ServiceSettings
    }
}
