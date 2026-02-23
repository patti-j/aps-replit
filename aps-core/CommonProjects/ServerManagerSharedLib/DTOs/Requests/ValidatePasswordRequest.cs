using System;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{

    public class ValidatePasswordRequest
    {
        public string Password { get; set; }
        public string UserName { get; set; }

        public ValidatePasswordRequest(string a_username, string a_password)
        {
            Password = a_password;
            UserName = a_username;
        }
    }
}