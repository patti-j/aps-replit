using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    [DataContract(Name = "InstanceLicenseInfo", Namespace = "http://www.planettogether.com")]
    public class InstanceLicenseInfo
    {
        public InstanceLicenseInfo()
        {
            SetCpuId();
        }

        public InstanceLicenseInfo(string a_serialCode) : this()
        {
            SerialCode = a_serialCode;
        }

        [DataMember]
        public long SessionId { get; set; }

        [DataMember]
        public string CpuId { get; set; }

        [DataMember]
        public string SerialCode { get; set; }

        /// <summary>
        /// The Company's Site this instance is associated with, as identified in the v12 LicenseManager DB
        /// </summary>
        [DataMember]
        public string SiteId { get; set; }

        [DataMember]
        public bool ReadOnly { get; set; }

        private void SetCpuId()
        {
            try
            {
            }
            catch (Exception)
            {
                //It is possible that the cpuId cannot be calculated.
                CpuId = "Error";
            }
        }

        public void Update(string a_newSerialCode, string a_siteId)
        {
            SerialCode = a_newSerialCode;
            SiteId = a_siteId;
            SetCpuId();
        }
    }
}
