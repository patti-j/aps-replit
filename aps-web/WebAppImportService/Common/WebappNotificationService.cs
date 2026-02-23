using Azure.Storage.Queues.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace WebAppImportService.Common
{
    public static class WebappNotificationService
    {
        private static readonly HttpClient _http = new HttpClient();

        public static async Task SendImportStatusUpdate(string identifier, string userEmail, double progress, string message)
        {
            try
            {
                var endpoint = Environment.GetEnvironmentVariable("WebAppsStatusUpdateEndpoint");
                var urlParams = $"?id={identifier}&userEmail={userEmail}&progress={progress}&message={message}";
                var req = new HttpRequestMessage(HttpMethod.Post, endpoint + urlParams);
                req.Headers.Add("Access-Token", Environment.GetEnvironmentVariable("NotificationEndpointAccessToken"));
                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to send status update to WebApp: {response.StatusCode}\n{response.Content}");
                }
            } catch(Exception e)
            {
                Console.WriteLine($"Failed to send status update to WebApp; {e.Message}\n{e.StackTrace}");
            }
        }
    }
}
