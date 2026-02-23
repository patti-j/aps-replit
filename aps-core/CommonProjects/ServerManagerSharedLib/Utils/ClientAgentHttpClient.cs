using System.Diagnostics;

using Microsoft.Extensions.Logging;

using PT.Common.Http;

namespace PT.ServerManagerSharedLib.Utils
{
    // TODO: Add logging to this class.
    // TODO: We could split this up along the same lines as the InstanceSettingsManager. Not strictly necessary,
    // TODO: since not all public methods are authorized unless you provide Server Auth, but would help clean up public interface
    public class ClientAgentHttpClient : PTHttpClient
    {
        private const string c_API_KEY = "Pl@n3t10geth3r";
        private const string c_instanceTokenSchemeName = "InstanceTokenScheme";
        private const string c_serverTokenSchemeName = "ServerTokenScheme";

        /// <summary>
        /// Initializes an Http Client with the default Utility Endpoint, for the production WebApp
        /// </summary>
        public ClientAgentHttpClient(string baseUrl) : base("api/Notification/", baseUrl)
        {
            SetDefaultHeaders();
            m_httpClient.BaseAddress = new Uri(baseUrl);
        }

        private void SetDefaultHeaders()
        {
            m_httpClient.DefaultRequestHeaders.Add("AzureAPIKey", c_API_KEY);
        }

        /// <summary>
        /// Ensure Client is configured to send requests using the Server Auth token, allow for certain elevated requests
        /// </summary>
        /// <param name="a_serverAuthToken"></param>
        public void AddServerManagerAuthorization(string a_serverAuthToken)
        {
            m_httpClient.DefaultRequestHeaders.Add(c_serverTokenSchemeName, a_serverAuthToken);
        }

        /// <summary>
        /// Ensure Client is configured to send requests using the Instance Auth token, designed to allow requests specific to the calling instance
        /// </summary>
        /// <param name="a_serverAuthToken"></param>
        public void AddInstanceAuthorization(string a_serverAuthToken)
        {
            m_httpClient.DefaultRequestHeaders.Add(c_instanceTokenSchemeName, a_serverAuthToken);
        }

        public Uri BaseAddress => m_httpClient.BaseAddress;

        public async Task<bool> SendNotification(string launchId, string email, int code, Serilog.ILogger log)
        {
            var urlParams = $"?id={launchId}&userEmail={email}&status={code}";
            try
            {
                await MakePostRequestAsync("/NotifyClientLaunchStatus" + urlParams, null);
                return true;
            }
            catch (Exception ex)
            {
                log.Warning($"Failed to notify WebApp of event type {code}:\n{ex}");
                return false;
            }
        }

        public async Task<byte[]?> GetClientAgentZip(string version)
        {
            byte[] response;
            var urlParams = $"?version={version}";
            response = await MakePostRequestAsync<byte[]>("/GetClientAgent" + urlParams);

            return response;
        }
    }
}
