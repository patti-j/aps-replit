namespace PT.ServerManagerSharedLib.DTOs.Requests
{

    public class ResetCredentialsRequest
    {
        public long UserIdVal { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public bool ResetPwdOnNextLogin { get; set; }
        public bool ResetAdmin = false;
        public string UserName { get; set; }

        public ResetCredentialsRequest() { }

        public ResetCredentialsRequest(long a_userIdVal, string a_currentPassword, string a_newPassword, bool a_resetPwdOnNextLogin, string a_username)
        {
            UserIdVal = a_userIdVal;
            CurrentPassword = a_currentPassword;
            NewPassword = a_newPassword;
            ResetPwdOnNextLogin = a_resetPwdOnNextLogin;
            UserName = a_username;
        }

        public ResetCredentialsRequest(string a_newPassword)
        {
            CurrentPassword = "";
            UserName = "";
            ResetPwdOnNextLogin = false;
            UserIdVal = 0;

            NewPassword = a_newPassword;
            ResetAdmin = true;
        }
    }
}