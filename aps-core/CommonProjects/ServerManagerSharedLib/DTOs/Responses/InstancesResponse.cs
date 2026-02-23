using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class InstancesResponse
    {
        public List<InstanceResponse> Instances { get; set; } = new List<InstanceResponse>();
    }

    public class InstanceResponse
    {
        public InstancePublicInfo PublicInfo { get; set; }

        public ServiceStatus Status { get; set; }

        public string InstanceIdentifier { get; set; }

        public int Port { get; set; }

        public string SsoDomain { get; set; }

        public string SsoClientId { get; set; }
    }
}
