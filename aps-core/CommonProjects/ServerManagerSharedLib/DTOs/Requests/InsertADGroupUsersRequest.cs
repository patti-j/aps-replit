using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.ServerManagerSharedLib.Definitions;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class InsertADGroupUsersRequest
    {
        public string SharedKey { get; set; }
        public List<ActiveDirectoryUser> users { get; set; }
        public string AccessLevel { get; set; }
        public string AdGroup { get; set; }
    }
}
