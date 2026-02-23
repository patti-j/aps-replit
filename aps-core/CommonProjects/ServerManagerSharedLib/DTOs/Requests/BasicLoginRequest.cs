using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class BasicLoginRequest
    {
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }
    }

    public class TokenLoginRequest
    {
        public string Token { get; set; }

        public TokenLoginRequest(string a_token)
        {
            Token = a_token;
        }

        public TokenLoginRequest()
        {
            
        }
    }
}
