using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class DeleteGroupUsersRequest
    {
        public InstanceKey InstanceKey { get; set; }
        public string ADGroup { get; set; }
        public List<ActiveDirectoryUser> users { get; set; }
    }
}
