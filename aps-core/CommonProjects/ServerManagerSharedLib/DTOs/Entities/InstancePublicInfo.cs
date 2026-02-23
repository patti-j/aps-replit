using System;

using PT.ServerManagerSharedLib.Definitions;

namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    public class InstancePublicInfo
    {
        public InstancePublicInfo(){}

        public InstancePublicInfo(string a_instanceName, string a_softwareVersion, DateTime? a_dateTime, EnvironmentType a_environmentType, bool a_activeDirectoryAllowed, bool a_autoStart, bool a_ssoAllowed, string a_systemServiceUrl)
        {
            InstanceName = a_instanceName;
            SoftwareVersion = a_softwareVersion;
            CreationDate = a_dateTime;
            ActiveDirectoryAllowed = a_activeDirectoryAllowed;
            EnvironmentType = a_environmentType; 
            AutoStart = a_autoStart;
            SsoAllowed = a_ssoAllowed;
            SystemServiceUrl = a_systemServiceUrl;
        }

        public string SystemServiceUrl { get; set; }

        public string InstanceName { get; set; }

        public string SoftwareVersion { get; set; }

        public string SystemServiceName
        {
            get { return string.Format("PlanetTogether {0} System", InstanceId); }
        }

        public string InterfaceServiceName
        {
            get { return string.Format("PlanetTogether {0} Interface", InstanceId); }
        }

        public string AdminMessage { get; set; }

        public DateTime? CreationDate { get; set; }

        public EnvironmentType EnvironmentType { get; set; }

        public bool ActiveDirectoryAllowed { get; set; }

        public bool SsoAllowed { get; set; }
        public bool AllowPasswordSettings { get; set; }

        public bool AutoStart { get; set; }

        /// <summary>
        /// Readable identifier for an instance. The server manager enforces uniqueness among name/version combos, as do conventions on our cloud.
        /// However, anything needing a unique, unspoofable identifier should use <see cref="InstanceSettings.InstanceId"/>
        /// </summary>
        public string InstanceId
        {
            get { return $"{InstanceName} {SoftwareVersion}"; }
        }

        public Integration CurrentIntegration { get; set; }
    }

    public enum EnvironmentType
    {
        Dev,
        QA,
        Production
    }
}
