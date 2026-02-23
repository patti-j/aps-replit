using System.Net.Http.Headers;

using PT.Common.Http;

using WebAPI.Models;

namespace WebAPI.DAL
{
    public class WebAppApiService
    {
        private PTHttpClient client;

        public WebAppApiService(IConfiguration config)
        {
            client = new PTHttpClient("api/Notification", config["WebAppUrl"], TimeSpan.FromSeconds(10), AuthenticationHeaderValue.Parse(config["NotificationEndpointAccessToken"]));
        }

        public async Task SendStatusUpdateAsync(PlanningAreaStatusList updates)
        {
            SignalRMessagePublisher.SendToWebApp("PAStatusUpdate", updates);
        }

        public async Task SendServerAgentShutdownAsync(string token)
        {
            SignalRMessagePublisher.SendToWebApp("ServerAgentShutdown", token);
        }

        public async Task SendGetLogsUpdateAsync(string events)
        {
            var resp = await client.SendPostRequestAsync("LogsUpdate", events);
        }
    }
}
