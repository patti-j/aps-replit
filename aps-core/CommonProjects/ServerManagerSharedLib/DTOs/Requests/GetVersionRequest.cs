using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class GetVersionRequest
    {
        public InstanceKey InstanceKey { get; set; }
        public string NewInstanceName { get; set; }
    }
}
