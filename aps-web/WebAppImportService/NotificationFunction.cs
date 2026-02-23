using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using WebAppImportService.Models;

namespace FunctionApp3
{
    public class NotificationFunction
    {
	    private static readonly HttpClient _http = new HttpClient();

        [FunctionName("NotificationFunction")]
        public static async Task Run(
			[QueueTrigger("%NotificationQueueName%", Connection = "AzureSMBStorageConnectionString")] ImportMessage myQueueItem, ILogger log)
        {
            var endpoint = Environment.GetEnvironmentVariable("WebAppsNotificationEndpoint");
            var urlParams = $"?userEmail={myQueueItem.Sender}&text={myQueueItem.Message}&type=1";
            var req = new HttpRequestMessage(HttpMethod.Post, endpoint + urlParams);
			req.Headers.Add("Access-Token", Environment.GetEnvironmentVariable("NotificationEndpointAccessToken"));
            var response = await _http.SendAsync(req);
            if (response.IsSuccessStatusCode)
            {
	            log.LogInformation($"Sent Notification to Web App.");
			}
            else
            {
	            log.LogError($"Failed to send notification to Web App. Response code {response.StatusCode}: {response.Content}");
                throw new HttpRequestException($"Failed to send notification to Web App. Response code {response.StatusCode}: {response.Content}");
            }
        }

    }
}
