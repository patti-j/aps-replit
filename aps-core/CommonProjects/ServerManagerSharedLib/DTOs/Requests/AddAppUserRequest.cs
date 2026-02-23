using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.Common.Http;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class AddAppUserRequest
    {
        public InstanceKey InstanceKey { get; set; }
        public string AppUserName { get; set; }
        public string AppUserPassword{ get; set; }
    }
}
