using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.ServerManagerSharedLib.Definitions
{
    public class Auth0Config
    {
        public string domain { get; set; }
        public string clientId { get; set; }
        public string clientSecretKey { get; set; }
    }
}
