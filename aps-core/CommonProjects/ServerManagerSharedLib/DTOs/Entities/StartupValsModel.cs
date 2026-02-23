using Microsoft.Extensions.Logging;

using PT.ServerManagerSharedLib.Definitions;

namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    public class StartupValsModel : StartupVals
    {
        public StartupValsModel(APSInstanceEntity a_serverInstance)
        {
            MaxNbrSessionRecordingsToStore = a_serverInstance.Settings.SystemServiceSettings.MaxNbrSessionRecordingsToStore;
            MaxNbrSystemBackupsToStorePerSession = a_serverInstance.Settings.SystemServiceSettings.MaxNbrSystemBackupsToStorePerSession;
            MinutesBetweenSystemBackups = a_serverInstance.Settings.SystemServiceSettings.MinutesBetweenSystemBackups;
            NonSequencedTransmissionPlayback = a_serverInstance.Settings.SystemServiceSettings.NonSequencedTransmissionPlayback;
            Port = a_serverInstance.Settings.SystemServiceSettings.Port;
            Record = a_serverInstance.Settings.SystemServiceSettings.RecordSystem;
            SingleThreaded = a_serverInstance.Settings.SystemServiceSettings.SingleThreadedTransmissionProcessing;
            StartingScenarioNumber = a_serverInstance.Settings.SystemServiceSettings.StartingScenarioNumber;
            StartType = (EStartType)Enum.Parse(typeof(EStartType), a_serverInstance.Settings.SystemServiceSettings.SystemEStartType.ToString());
            WorkingDirectory = a_serverInstance.ServicePaths.SystemServiceWorkingDirectory;
            
            // This used to call ServerManager.GetSystemServiceURLForInstance to check for the latest "ComputerNameOrIP" Server Manager setting -
            // however, instances now receive an updated value as soon as that Server Manager setting changes. 
            SystemServiceUrl = a_serverInstance.PublicInfo.SystemServiceUrl; 

            ClientTimeoutSeconds = a_serverInstance.Settings.SystemServiceSettings.ClientTimeoutSeconds;
            WebApiTimeoutSeconds = a_serverInstance.Settings.SystemServiceSettings.WebApiTimeoutSeconds;
            ServiceName = a_serverInstance.PublicInfo.SystemServiceName;
            KeyFolder = a_serverInstance.Settings.SystemServiceSettings.KeyFolder;
            LogFolder = a_serverInstance.Settings.SystemServiceSettings.LogFolder;
            InstanceName = a_serverInstance.PublicInfo.InstanceName;
            SoftwareVersion = a_serverInstance.PublicInfo.SoftwareVersion;
            LogDbConnectionString = a_serverInstance.Settings.SystemServiceSettings.LogDbConnectionString;
            SsoValidationCertificateThumbprint = a_serverInstance.Settings.SsoValidationCertificateThumbprint;
            AllowSsoLogin = a_serverInstance.Settings.AllowSsoLogin;
            CoPilotSettings = a_serverInstance.Settings.CoPilotSettings;
            PublishSettings = a_serverInstance.Settings.PublishSettings;
            SecurityToken = a_serverInstance.Settings.SecurityToken;
            ServerTimeZoneId = a_serverInstance.Settings.ServerTimeZoneId;
            ScenarioLimit = a_serverInstance.Settings.ScenarioLimit;
            SerialCode = a_serverInstance.LicenseInfo.SerialCode;
            SiteId = a_serverInstance.LicenseInfo.SiteId;
            EnvironmentType = a_serverInstance.PublicInfo.EnvironmentType;
            InstanceId = a_serverInstance.Settings.InstanceId;
            //ApiKey = a_serverInstance.Settings.ApiKey;
            SsoDomain = a_serverInstance.SsoDomain;
            SsoClientId = a_serverInstance.SsoClientId;
            EnableDiagnostics = a_serverInstance.Settings.DiagnosticsEnabled;
            EnableChecksumDiagnostics = a_serverInstance.Settings.SystemServiceSettings.EnableChecksumDiagnostics;
            WebAppUrl = a_serverInstance.ServerWideSettings.WebAppUrl;
            WebAppClientId = a_serverInstance.ServerWideSettings.WebAppClientId;
            IntegrationV2Connection = a_serverInstance.Settings.IntegrationV2Connection;
            ServicePaths = a_serverInstance.ServicePaths;
            UseSentry = a_serverInstance.Settings.UseSentry;
            SentryDsn = a_serverInstance.Settings.SentryDsn;
            AutoLogoffTimeout = a_serverInstance.ServerWideSettings.AutoLogoffTimeout;
        }
        private static string GetUrl(string a_url)
        {
            UriBuilder uriBuilder = new UriBuilder(a_url) { Scheme = "https" };
            return uriBuilder.Uri.AbsoluteUri;
        }
    }
}