using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class RestAdminPasswordRequest
    {
        public InstanceKey InstanceKey { get; set; }
        public string NewPassword { get; set; }
        public long? UserId { get; set; }
    }
}
