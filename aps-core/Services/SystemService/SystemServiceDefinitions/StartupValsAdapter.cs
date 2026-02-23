using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.SystemServiceDefinitions;

/// <summary>
/// Extension of StartupVals designed to hold other cacheable values from system startup.
/// </summary>
public class StartupValsAdapter : StartupVals
{
    #region Additional Config Values
    // TODO: It might be nice to not need these additional values - they are only needed when a new IInstanceSettingsManager is needed at an irregular time, which we could try to remove.
    private string m_instanceDatabaseConnectionString;
    [Obsolete("Used to get instance settings pre-Webapp connection. Should be phased out once all devs can connect there.")]
    public string InstanceDatabaseConnectionString
    {
        get => m_instanceDatabaseConnectionString;
        set => m_instanceDatabaseConnectionString = value;
    }

    private string m_webappEnv;
    public string WebAppEnv 
    {
        get => m_webappEnv;
        set => m_webappEnv = value;
    }
    #endregion
    public StartupValsAdapter (){}

    public StartupValsAdapter(StartupVals a_startupVals, string a_webappEnv, string a_instanceDatabaseConnectionString, string a_apiKey)
    {
        //m_startupVals = a_startupVals;
        m_webappEnv = a_webappEnv;
        m_instanceDatabaseConnectionString = a_instanceDatabaseConnectionString;
        ApiKey = a_apiKey;

        m_devPackagePath = a_startupVals.DevPackagePath;
        m_workingDirectory = a_startupVals.WorkingDirectory;
        m_serviceName = a_startupVals.ServiceName;
        m_port = a_startupVals.Port;
        m_startType = a_startupVals.StartType;
        m_threadPriority = a_startupVals.ServiceThreadPriority;
        m_record = a_startupVals.Record;
        m_maxNbrSessionRecordingsToStore = a_startupVals.MaxNbrSessionRecordingsToStore;
        m_publishTimeZone = a_startupVals.PublishTimeZone;
        m_maxNbrSystemBackupsToStorePerSession = a_startupVals.MaxNbrSystemBackupsToStorePerSession;
        m_minutesBetweenSystemBackups = a_startupVals.MinutesBetweenSystemBackups;
        m_nonSequenced = a_startupVals.NonSequencedTransmissionPlayback;
        m_startingScenarioNbr = a_startupVals.StartingScenarioNumber;
        m_singleThreaded = a_startupVals.SingleThreaded;
        m_systemServiceUrl = a_startupVals.SystemServiceUrl;
        m_interfaceServiceUrl = a_startupVals.InterfaceServiceUrl;
        m_clientTimeoutSeconds = a_startupVals.ClientTimeoutSeconds;
        m_webApiTimeoutSeconds = a_startupVals.WebApiTimeoutSeconds;
        m_runningMassRecordings = a_startupVals.RunningMassRecordings;
        m_massRecordingsSessionId = a_startupVals.MassRecordingsSessionId;
        m_massRecordingsPlayerPath = a_startupVals.MassRecordingsPlayerPath;
        m_instanceName = a_startupVals.InstanceName;
        m_SoftwareVersion = a_startupVals.SoftwareVersion;
        m_logDbConnectionString = a_startupVals.LogDbConnectionString;
        m_logFolder = a_startupVals.LogFolder;
        m_keyFolder = a_startupVals.KeyFolder;
        SsoValidationCertificateThumbprint = a_startupVals.SsoValidationCertificateThumbprint;
        SsoDomain = a_startupVals.SsoDomain;
        SsoClientId = a_startupVals.SsoClientId;
        SecurityToken = a_startupVals.SecurityToken;
        ServerTimeZoneId = a_startupVals.ServerTimeZoneId;
        ScenarioLimit = a_startupVals.ScenarioLimit;
        SerialCode = a_startupVals.SerialCode;
        EnvironmentType = a_startupVals.EnvironmentType;
        SiteId = a_startupVals.SiteId;
        InstanceId = a_startupVals.InstanceId;
        CoPilotSettings = a_startupVals.CoPilotSettings;
        EnableChecksumDiagnostics = a_startupVals.EnableChecksumDiagnostics;
        IntegrationV2Connection = a_startupVals.IntegrationV2Connection;
        WebAppUrl = a_startupVals.WebAppUrl;
        WebAppClientId = a_startupVals.WebAppClientId;
        UseSentry = a_startupVals.UseSentry;
        SentryDsn = a_startupVals.SentryDsn;
        DisableHttps = a_startupVals.DisableHttps;
        // TODO: These have been traditionally returned from another call (GetInstanceSettingsEntity, which is needlessly bulky), but it is no longer needed.
        // TODO: Once all Core devs on Hydrogen are connecting with the Webapp, they can get this info from here and remove calls to the old method.
        //EnableDiagnostics = a_startupVals.EnableDiagnostics;
        //ServicePaths = a_startupVals.ServicePaths;
    }

    /// <summary>
    /// Gets whether the IntegrationV2 (and corresponding importer) should be loaded in and used vs the old one.
    /// Currently a WIP, this is set in dev here, but theoretically should be tied to a setting in the Server Manager - 
    /// whether the instance has configured a connection to pull v2 integrations from (similar to how db logging is enabled).
    /// Once that is in place, we can directly check against whether a valid connection can be established with the provided connection string.
    /// </summary>
    public bool NewImport =>
        #if DEBUG
        !string.IsNullOrEmpty(IntegrationV2Connection); // devs can LOCALLY set to true for now until SM release where this is settable is available. TODO: Make this the default when publically available
    #else
        !string.IsNullOrEmpty(IntegrationV2Connection); // will always be false until this is released and editable in SM.
    #endif
}