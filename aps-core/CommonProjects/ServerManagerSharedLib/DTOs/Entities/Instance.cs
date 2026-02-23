using System.Text.Json.Serialization;
namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    public class Instance
    {
        public int ID { get; set; }
        public EnvironmentType EnvironmentType { get; set; }
        public string Name { get; set; }
        public string ServerManagerName { get; set; }
        public string Version { get; set; }
        public string IntegrationCode { get; set; }
        [Obsolete("See ApsInstanceEntity.ServiceStatus")]
        public string Status { get; set; }
        [Obsolete("See ApsInstanceEntity.InstanceLicenseInfo")]
        public string LicenseStatus { get; set; }
        public string NbrUsers { get; set; }
        public string LastLogon { get; set; }
        public DateTime CreationDate { get; set; }

        private string serialCode;
        public string SerialCode
        {
            get { return serialCode.ToUpper(); }  
            set { serialCode = value.ToUpper(); } 
        }
        public int Port { get; set; }

        public ServicePaths ServicePaths { get; set; }
    }
}