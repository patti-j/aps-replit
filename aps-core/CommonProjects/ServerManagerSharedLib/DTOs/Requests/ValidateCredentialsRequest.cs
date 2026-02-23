using Newtonsoft.Json;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class ValidateCredentialsRequest
    {
        public string Password { get; set; }
        public string UserName { get; set; }

        public ValidateCredentialsRequest(string a_username, string a_password)
        {
            UserName = a_username;
            Password = a_password;
        }

        public ValidateCredentialsRequest()
        {
            
        }
    }
}