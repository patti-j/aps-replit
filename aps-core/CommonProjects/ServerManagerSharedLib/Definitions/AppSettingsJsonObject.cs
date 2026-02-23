
namespace PT.ServerManagerWebService.Models
{
    public class AppSettingsJsonObject
    {
        public Logging Logging { get; set; }
        public Eventlog EventLog { get; set; }
        public string InstanceDataFolder { get; set; }
        //TODO: Why do we need these Auth0 values here?
        public string Auth0ClientKeySecret { get; set; }
        public string Auth0ClientId { get; set; }
        public string Auth0Domain { get; set; }
        public string ServerAuthToken { get; set; }
        public string WebAppEnv { get; set; }
    }

    public class Logging
    {
        public Loglevel LogLevel { get; set; }
    }

    public class Loglevel
    {
        public string Default { get; set; }
        public string Microsoft { get; set; }
        public string MicrosoftHostingLifetime { get; set; }
    }

    public class Eventlog
    {
        public Loglevel1 LogLevel { get; set; }
    }

    public class Loglevel1
    {
        public string Default { get; set; }
        public string MicrosoftHostingLifetime { get; set; }
    }

}
