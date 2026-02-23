using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.ServerManagerSharedLib.Helpers;

namespace PT.ServerManagerSharedLib.Definitions
{
    /// <summary>
    /// Defines the properties of the APS instance for serialization purposes.
    /// </summary>
    public class APSInstanceEntity : IExtensibleDataObject
    {
        protected const string c_integrationJsonFileName = "Integration.json";

        protected InstanceSettings m_settings = new InstanceSettings(Guid.NewGuid());
        protected InstancePublicInfo m_instancePublicInfo;
        protected InstanceLicenseInfo m_instanceLicenseInfo;
        protected ServicePaths m_servicePaths;
        protected InstanceLogHelper m_instanceLogHelper;
        protected ServerWideInstanceSettings m_serverWideSettings;
        protected bool m_isBackup;
        protected bool m_isActive;
        public string ApiKey;

        [JsonIgnore] [System.Text.Json.Serialization.JsonIgnore]
        public InstanceKey key
        {
            get
            {
                return new InstanceKey(m_instancePublicInfo.InstanceName, m_instancePublicInfo.SoftwareVersion);
            }
        }

        [DataMember]
        public InstanceSettings Settings
        {
            get { return m_settings; }
            set { m_settings = value; }
        }

        [DataMember]
        public InstancePublicInfo PublicInfo
        {
            get { return m_instancePublicInfo; }
            set { m_instancePublicInfo = value; }
        }

        [DataMember]
        public InstanceLicenseInfo LicenseInfo
        {
            get { return m_instanceLicenseInfo; }
            set { m_instanceLicenseInfo = value; }
        }

        [DataMember]
        public InstanceLogHelper LogHelper
        {
            get { return m_instanceLogHelper; }
            set { m_instanceLogHelper = value; }
        }

        [DataMember]
        public ServicePaths ServicePaths
        {
            get { return m_servicePaths; }
            set { m_servicePaths = value; }
        }

        [DataMember]
        public bool IsBackup
        {
            get { return m_isBackup; }
            set { m_isBackup = value; }
        }

        [DataMember]
        public bool IsActive
        {
            get { return m_isBackup; }
            set { m_isBackup = value; }
        }

        public ServerWideInstanceSettings ServerWideSettings
        {
            get => m_serverWideSettings;
            set => m_serverWideSettings = value;
        }

        public ExtensionDataObject ExtensionData { get; set; }

        /// <summary>
        /// SsoDomain in use. Will use the one defined for the instance if not null, or the serverwide one if not null, or an appropriate default
        /// </summary>
        public string SsoDomain => SsoConstants.GetSsoDomain(Settings.SystemServiceSettings, ServerWideSettings);


        /// <summary>
        /// SsoClientId in use. Will use the one defined for the instance if not null, or the serverwide one if not null, or an appropriate default
        /// </summary>
        public string SsoClientId => SsoConstants.GetSsoClientId(Settings.SystemServiceSettings, ServerWideSettings);
    }
}
