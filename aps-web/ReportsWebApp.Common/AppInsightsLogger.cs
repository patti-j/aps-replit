using Microsoft.ApplicationInsights;


namespace ReportsWebApp.Shared
{
    public interface IAppInsightsLogger
    {
        TelemetryClient TelemetryClient { get; set; }
        void Log(string message, string user = "unknown", string location = "unknown");
        void LogEvent(string eventName, string user = "unknown", params KeyValuePair<string, string>[] properties);
        string LogError(Exception e, string user = "unknown", string location = "unknown");
    }

    public class AppInsightsLogger : IAppInsightsLogger
    {
        public TelemetryClient TelemetryClient { get; set; }

        public AppInsightsLogger(TelemetryClient client) {
            TelemetryClient = client;
        }

        public void Log(string message, string user = "unknown", string location = "unknown")
        {
            var props = new Dictionary<string, string>();
            if (user != null)
            {
                props.Add("User", user);
            }
            props.Add("Location", location);
            TelemetryClient.TrackTrace(message, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information, props);
        }

        public void LogEvent(string eventName, string user = "unknown", params KeyValuePair<string, string>[] properties)
        {
            var props = properties.ToDictionary(x => x.Key, x => x.Value);
            if (user != null)
            {
                props.Add("User", user);
            }
            TelemetryClient.TrackEvent(eventName, props);
        }

        public string LogError(Exception e, string user = "unknown", string location = "unknown")
        {
            var correlationId = GetCorrelationId();

            var props = new Dictionary<string, string>();
            if (user != null)
            {
                props.Add("User", user);
            }
            props.Add("CorrelationId", correlationId);
            props.Add("Location", location);

            TelemetryClient.TrackException(e, props);
            return correlationId;
        }

        private string GetCorrelationId() {
            // Generate a string of random number as letters, avoiding
            // vowels or vowel-like glyphs to avoid generating real words
            const string chars = "BCDFGHJKLMNPQRSTVWXZ2356789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }
    }
}