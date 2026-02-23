using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class UserLoginResponse
    {
        public long UserId { get; set; }
        public string SessionToken { get; set; }
    }
}
