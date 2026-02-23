using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.SystemDefinitions;

public class LoggedInInstanceInfo : IPTSerializable
{
    private LoggedInInstanceInfo() { }

    /// <summary>
    /// This is returned from the server on an external connection. Don't return any sensitive information
    /// </summary>
    /// <param name="a_instance"></param>
    /// <param name="a_systemServiceUrl"></param>
    /// <param name="a_interfaceServiceUrl"></param>
    public LoggedInInstanceInfo(string a_instanceName,
                                string a_softwareVersion,
                                string a_dashboardReportURL,
                                string a_smtpServerAddress,
                                int a_smtpServerPortNbr,
                                string a_smtpFromEmailAddress,
                                string a_sqlSupportEmailToAddresses,
                                int a_dashboardTimeout,
                                string a_systemServiceUrl,
                                string a_interfaceServiceUrl)
    {
        m_instanceName = a_instanceName;
        m_instanceVersion = a_softwareVersion;
        m_dashboardUrl = a_dashboardReportURL;
        IsAnalyticsConfigured = !string.IsNullOrEmpty(a_smtpServerAddress) && a_smtpServerPortNbr != 0 && !string.IsNullOrEmpty(a_smtpFromEmailAddress) && !string.IsNullOrEmpty(a_sqlSupportEmailToAddresses);
        IsSupportEmailSetup = !string.IsNullOrEmpty(a_smtpServerAddress) && a_smtpServerPortNbr != 0 && !string.IsNullOrEmpty(a_smtpFromEmailAddress) && !string.IsNullOrEmpty(a_sqlSupportEmailToAddresses);
        m_erpDatabaseConnectionString = string.Empty; //sensitive
        m_interfaceServiceUrl = a_interfaceServiceUrl;
        m_systemServiceUrl = a_systemServiceUrl;
        m_publishSqlConnectionString = string.Empty; //sensitive
        m_analyticsConnectionString = string.Empty; //sensitive
        m_analyticsTimeout = a_dashboardTimeout;
    }

    //Used to convert from APIInstanceClassLibrary LoggedInInfo to SchedulerDefinitions LoggedInInstanceInfo
    public LoggedInInstanceInfo(InstanceSettingsEntity a_loggedInInfo)
    {
        IsAnalyticsConfigured = !string.IsNullOrEmpty(a_loggedInInfo.SmtpServerAddress) && a_loggedInInfo.SmtpServerPort != 0 && !string.IsNullOrEmpty(a_loggedInInfo.SmtpFromEmail) && !string.IsNullOrEmpty(a_loggedInInfo.SupportEmailToAddresses);
        IsSupportEmailSetup = !string.IsNullOrEmpty(a_loggedInInfo.SmtpServerAddress) && a_loggedInInfo.SmtpServerPort != 0 && !string.IsNullOrEmpty(a_loggedInInfo.SmtpFromEmail) && !string.IsNullOrEmpty(a_loggedInInfo.SupportEmailToAddresses);
        IsActiveDirectoryAllowed = a_loggedInInfo.AllowAcitiveDirectory;
        m_instanceName = a_loggedInInfo.Name;
        m_instanceVersion = a_loggedInInfo.Version;
        m_dashboardUrl = a_loggedInInfo.DashboardUrl;
        m_erpDatabaseConnectionString = a_loggedInInfo.ErpDatabaseConnectionString;
        m_publishSqlConnectionString = a_loggedInInfo.PublishDbConnectionString;
        m_analyticsConnectionString = a_loggedInInfo.AnalyticsDbConnectionString;
        m_analyticsTimeout = a_loggedInInfo.DashTimeout;
        m_systemServiceKeyFolder = a_loggedInInfo.ServicePaths.SystemServiceKeyFolder;
        m_systemServiceWorkingDirectory = a_loggedInInfo.ServicePaths.SystemServiceWorkingDirectory;
        m_clientTimeoutSeconds = a_loggedInInfo.ClientTimeoutSeconds;
        m_autoLogOffTimeoutTicks = a_loggedInInfo.ServerWideSettings.AutoLogoffTimeout;
    }

    public static LoggedInInstanceInfo SetInstanceInfo(string a_instanceName, string a_instanceVersion, string a_systemServiceUrl)
    {
        LoggedInInstanceInfo info = new ()
        {
            m_instanceName = a_instanceName,
            m_instanceVersion = a_instanceVersion,
            m_systemServiceUrl = a_systemServiceUrl
        };

        return info;
    }

    public LoggedInInstanceInfo(IReader a_reader)
    {
        m_bools = new BoolVector32(a_reader);
        a_reader.Read(out m_instanceName);
        a_reader.Read(out m_instanceVersion);
        a_reader.Read(out m_dashboardUrl);
        a_reader.Read(out m_erpDatabaseConnectionString);
        //No longer used
        //a_reader.Read(out m_sqlConnectionString);
        a_reader.Read(out m_interfaceServiceUrl);
        a_reader.Read(out m_systemServiceUrl);
        a_reader.Read(out m_clientUpaterUrl);
        a_reader.Read(out m_publishSqlConnectionString);
        a_reader.Read(out m_analyticsConnectionString);
        a_reader.Read(out m_analyticsTimeout);
        a_reader.Read(out m_systemServiceKeyFolder);
        a_reader.Read(out m_systemServiceWorkingDirectory);
        a_reader.Read(out m_clientTimeoutSeconds);
        a_reader.Read(out m_autoLogOffTimeoutTicks);
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write(InstanceName);
        a_writer.Write(InstanceVersion);
        a_writer.Write(DashboardReportURL);
        a_writer.Write(ErpDatabaseConnectionString);
        //No longer used
        //a_writer.Write(SqlConnectionString);
        a_writer.Write(InterfaceServiceUrl);
        a_writer.Write(SystemServiceUrl);
        a_writer.Write(ClientUpdaterServiceUrl);
        a_writer.Write(m_publishSqlConnectionString);
        a_writer.Write(m_analyticsConnectionString);
        a_writer.Write(m_analyticsTimeout);
        a_writer.Write(m_systemServiceKeyFolder);
        a_writer.Write(m_systemServiceWorkingDirectory);
        a_writer.Write(m_clientTimeoutSeconds);
        a_writer.Write(m_autoLogOffTimeoutTicks);
    }

    public int UniqueId => 840;

    private BoolVector32 m_bools;

    private const short c_IsAnalyticsConfiguredIdx = 0;
    private const short c_IsSupportEmailSetupIdx = 1;
    private const short c_IsActiveDirectoryAllowedIdx = 2;

    public bool IsAnalyticsConfigured
    {
        get => m_bools[c_IsAnalyticsConfiguredIdx];
        set => m_bools[c_IsAnalyticsConfiguredIdx] = value;
    }

    public bool IsSupportEmailSetup
    {
        get => m_bools[c_IsSupportEmailSetupIdx];
        set => m_bools[c_IsSupportEmailSetupIdx] = value;
    }

    public bool IsActiveDirectoryAllowed
    {
        get => m_bools[c_IsActiveDirectoryAllowedIdx];
        set => m_bools[c_IsActiveDirectoryAllowedIdx] = value;
    }

    private string m_instanceName;
    public string InstanceName => m_instanceName;

    private string m_instanceVersion;
    public string InstanceVersion => m_instanceVersion;

    private readonly string m_dashboardUrl;
    public string DashboardReportURL => m_dashboardUrl;

    private string m_erpDatabaseConnectionString;
    public string ErpDatabaseConnectionString => m_erpDatabaseConnectionString;

    private string m_publishSqlConnectionString;
    public string PublishSqlConnectionString => m_publishSqlConnectionString;

    private string m_analyticsConnectionString;
    public string AnalyticsConnectionString => m_analyticsConnectionString;

    //No longer used. Lept not to break serialization.  Remove before public release?
    private readonly string m_interfaceServiceUrl;
    public string InterfaceServiceUrl => m_interfaceServiceUrl;

    //No longer used. Left not to break serialization.  Remove before public release?
    private string m_systemServiceUrl;

    public string SystemServiceUrl
    {
        get => m_systemServiceUrl;
        set => m_systemServiceUrl = value;
    }

    private readonly string m_clientUpaterUrl;
    public string ClientUpdaterServiceUrl => m_clientUpaterUrl;

    private readonly int m_analyticsTimeout;
    public int AnalyticsTimeout => m_analyticsTimeout;

    private string m_systemServiceKeyFolder;
    public string SystemServiceKeyFolder => m_systemServiceKeyFolder;

    private string m_systemServiceWorkingDirectory;

    public string SystemServiceWorkingDirectory => m_systemServiceWorkingDirectory;

    private readonly int m_clientTimeoutSeconds;
    public TimeSpan ClientTimeout => TimeSpan.FromSeconds(m_clientTimeoutSeconds);
    
    private readonly long m_autoLogOffTimeoutTicks;

    public TimeSpan AutoLogoffTimeout => TimeSpan.FromTicks(m_autoLogOffTimeoutTicks);
}