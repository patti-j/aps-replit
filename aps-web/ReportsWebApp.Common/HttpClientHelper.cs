using System.Text.Json;

namespace ReportsWebApp.Common
{
    /// <summary>
    /// Helper Class to assist with configuring Http Client for communication within PT
    /// TODO: We should eventually figure out how the PTHttpClient in PT.Common could be shared in the webapps -
    /// TODO: it's currently built for a specific windows architecture, but it shouldn't need to be.
    /// </summary>
    public class HttpClientHelper
    {
        /// <summary>
        /// Builds an <see cref="HttpClient"/> that is configured with the required Thumbprint and Auth to communicate with a Server Manager.
        /// </summary>
        /// <param name="a_serverSslToken"></param>
        /// <param name="a_serverAuthKey"></param>
        /// <returns></returns>
        public static HttpClient GetClientForServer(string a_serverSslToken, string a_serverAuthKey)
        {
            var handler = SetupHttpClientHandler(a_serverSslToken);

            var clientForServer = new HttpClient(handler);

            clientForServer.DefaultRequestHeaders.Add("Authorization", $"ServerManagerToken {a_serverAuthKey}");

            return clientForServer;
        }

        private static HttpClientHandler SetupHttpClientHandler(string a_serverSslToken)
        {
            HttpClientHandler handler = new HttpClientHandler();

            // Register the client to accept traffic from this server
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert == null)
                {
                    return false;
                }

                return cert.GetCertHashString().Equals(a_serverSslToken, StringComparison.OrdinalIgnoreCase);
            };
            return handler;
        }

        /// <summary>
        /// Creates a full endpoint url, cleaning up any unintended structural issues.
        /// </summary>
        /// <param name="a_serverUrl"></param>
        /// <param name="a_endpointName"></param>
        /// <returns></returns>
        // TODO: could take both route and endpoint
        public static Uri BuildRequestUri(string a_serverUrl, string a_endpointName)
        {
            // Clean up potential double slashes
            a_serverUrl = a_serverUrl.TrimEnd('/');
            a_endpointName = a_endpointName.TrimStart('/');

            var fullUrl = $"{a_serverUrl}/{a_endpointName}";
            return new Uri(fullUrl);
        }

        /// <summary>
        /// Deserializes server response into the provided generic type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<T> GetResponseObject<T>(HttpResponseMessage response)
        {
            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {

                if (response.IsSuccessStatusCode)
                {
                    if (stream == null)
                    {
                        //throw new ApiException
                        //{
                        //    StatusCode = (int)response.StatusCode,
                        //    Content = "No content for successful response"
                        //};
                        Console.WriteLine("No content for successful response");
                    }

                    if (!stream.CanRead)
                    {
                        //throw new ApiException
                        //{
                        //    StatusCode = (int)response.StatusCode,
                        //    Content = "Invalid content for successful response"
                        //};
                        Console.WriteLine("Invalid content for successful response");
                    }

                    return DeserializeJsonFromStream<T>(stream);
                }

                //Error
                Task<string> task = StreamToStringAsync(stream);
                //throw new ApiException
                //{
                //    StatusCode = (int)response.StatusCode,
                //    Content = task.Result
                //};
                var error = $"Handshake unsuccessful. Status code {(int)response.StatusCode}: {task.Result}";
                Console.WriteLine(error);
                throw new Exception(error);
            }
        }
        private static T DeserializeJsonFromStream<T>(Stream a_stream)
        {
            using (var sr = new StreamReader(a_stream))
            {
                var json = sr.ReadToEnd();
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }
        }

        private static async Task<string> StreamToStringAsync(Stream a_stream)
        {
            if (a_stream != null)
            {
                using (StreamReader sr = new StreamReader(a_stream))
                {
                    return await Task.Run(() => sr.ReadToEndAsync());
                }
            }

            return null;
        }

    }
}
