namespace WebAPI.Models
{
    public class ActivateCodeMessage
    {
        public string Code { get; set; }
        public string Thumbprint { get; set; }
        public string AuthToken { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string ApiPort { get; set; }
        public string ServerManagerPath { get; set; }
        public string SsoClientId { get; set; }
        public string SsoDomain { get; set; }
        public string SystemId { get; set; }


        public ActivateCodeMessage(string code, string thumbprint, string authToken, string version, string serverManagerPath, string apiPort, string ssoClientId, string ssoDomain, string systemId)
        {
            Code = code;
            Thumbprint = thumbprint;
            AuthToken = authToken;
            Name = Environment.MachineName;
            Version = version;
            ServerManagerPath = serverManagerPath;
            ApiPort = apiPort;
            SsoClientId = ssoClientId;
            SsoDomain = ssoDomain;
            SystemId = systemId;
        }
    }
}
