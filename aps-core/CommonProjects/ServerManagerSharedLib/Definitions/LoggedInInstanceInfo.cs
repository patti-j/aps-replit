using PT.ServerManagerSharedLib.Utils;

namespace PT.ServerManagerSharedLib.Definitions
{
    public class LoggedInInstanceInfoAPI
    {
        /// <summary>
        /// This is returned from the server on an external connection. Don't return any sensitive information
        /// </summary>
        /// <param name="a_instance"></param>
        /// <param name="a_systemServiceUrl"></param>
        /// <param name="a_interfaceServiceUrl"></param>
        public LoggedInInstanceInfoAPI(string a_instanceName, string a_softwareVersion, string a_dashboardReportURL, string a_smtpServerAddress, int a_smtpServerPortNbr, 
                                    string a_smtpFromEmailAddress, string a_sqlSupportEmailToAddresses, int a_dashboardTimeout, string a_systemServiceUrl, string a_interfaceServiceUrl)
        {
            m_instanceName = a_instanceName;
            m_instanceVersion = a_softwareVersion;
            m_dashboardUrl = a_dashboardReportURL;
            IsAnalyticsConfigured = !string.IsNullOrEmpty(a_smtpServerAddress) && a_smtpServerPortNbr != 0 && !string.IsNullOrEmpty(a_smtpFromEmailAddress) && !string.IsNullOrEmpty(a_sqlSupportEmailToAddresses);
            IsSupportEmailSetup = !string.IsNullOrEmpty(a_smtpServerAddress) && a_smtpServerPortNbr != 0 && !string.IsNullOrEmpty(a_smtpFromEmailAddress) && !string.IsNullOrEmpty(a_sqlSupportEmailToAddresses);
            m_erpDatabaseConnectionString = string.Empty; //sensitive
            m_interfaceServiceUrl = a_interfaceServiceUrl;
            m_systemServiceUrl = a_systemServiceUrl;
            m_sqlConnectionString = string.Empty; //sensitive
            m_publishDatabaseConnectionString = string.Empty; //sensitive
            m_dashboardConnectionString = string.Empty; //sensitive
            m_analyticsTimeout = a_dashboardTimeout;

        }

        public void SetInternalInfo(string a_erpDatabaseConnectionString, string a_publishSqlServerConnectionString, string a_dashboardConnString,
                                                         string a_systemServiceKeyFolder, string a_systemServiceWorkingDirectory)
        {
            m_erpDatabaseConnectionString = Encryption.RemovePasswordPortionFromConnectionString(a_erpDatabaseConnectionString);
            m_sqlConnectionString = Encryption.RemovePasswordPortionFromConnectionString(a_publishSqlServerConnectionString);
            m_publishDatabaseConnectionString = a_publishSqlServerConnectionString;
            m_dashboardConnectionString = a_dashboardConnString;
            m_systemServiceKeyFolder = a_systemServiceKeyFolder;
            m_systemServiceWorkingDirectory = a_systemServiceWorkingDirectory;
        }

        public int UniqueId => 840;


        private bool m_IsAnalyticsConfigured;
        private bool m_IsSupportEmailSetup;
        private bool m_IsActiveDirectoryAllowed;

        public bool IsAnalyticsConfigured
        {
            get { return m_IsAnalyticsConfigured; }
            set { m_IsAnalyticsConfigured = value; }
        }

        public bool IsSupportEmailSetup
        {
            get { return m_IsSupportEmailSetup; }
            set { m_IsSupportEmailSetup = value; }
        }
        
        public bool IsActiveDirectoryAllowed
        {
            get { return m_IsActiveDirectoryAllowed; }
            set { m_IsActiveDirectoryAllowed = value; }
        }

        private string m_instanceName;
        public string InstanceName => m_instanceName;

        private string m_instanceVersion;
        public string InstanceVersion => m_instanceVersion;

        private string m_dashboardUrl;
        public string DashboardReportURL => m_dashboardUrl;

        private string m_erpDatabaseConnectionString;
        public string ErpDatabaseConnectionString => m_erpDatabaseConnectionString;

        private string m_publishDatabaseConnectionString;
        public string PublishDatabaseConnectionString => m_publishDatabaseConnectionString;
        
        private string m_dashboardConnectionString;
        public string DashboardConnectionString => m_dashboardConnectionString;

        private string m_sqlConnectionString;
        public string SqlConnectionString => m_sqlConnectionString;

        private string m_interfaceServiceUrl;
        public string InterfaceServiceUrl => m_interfaceServiceUrl;

        private string m_systemServiceUrl;
        public string SystemServiceUrl => m_systemServiceUrl;

        private string m_clientUpaterUrl;
        public string ClientUpdaterServiceUrl => m_clientUpaterUrl;
        
        private int m_analyticsTimeout;
        public int AnalyticsTimeout => m_analyticsTimeout;
        
        private string m_systemServiceKeyFolder;
        public string SystemServiceKeyFolder => m_systemServiceKeyFolder;

        private string m_systemServiceWorkingDirectory;
        public string SystemServiceWorkingDirectory => m_systemServiceWorkingDirectory;
    }
}
