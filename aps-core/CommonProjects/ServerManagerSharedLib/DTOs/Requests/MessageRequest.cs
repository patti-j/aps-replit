using Newtonsoft.Json;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class MessageRequest
    {
        [JsonRequired]
        public string Message { get; set; }
        [JsonRequired]
        public bool Shutdown { get; set; }
        [JsonRequired]
        public bool ShutdownWarning { get; set; }
        [JsonRequired]
        public List<long> SelectedUsers { get; set; }
        [JsonRequired]
        public string UserName { get; set; }

        public MessageRequest(){}

        public MessageRequest(string a_message, bool a_shutdown, bool a_shutdownWarning, List<long> a_selectedUsers, string a_userName)
        {
            Message = a_message;
            Shutdown = a_shutdown;
            ShutdownWarning = a_shutdownWarning;
            SelectedUsers = a_selectedUsers;
            UserName = a_userName;
        }

        // How long the PT Instance takes between providing a shutdown warning to users and executing the shutdown
        public const int c_ClientShutdownWarningTimeInMS = 15000;
    }
}