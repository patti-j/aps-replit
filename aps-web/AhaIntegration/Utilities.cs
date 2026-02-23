using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace AhaIntegration
{
    internal class Utilities
    {
        internal static async Task<T> GetResponseObject<T>(HttpResponseMessage a_message)
        {
            string json = await a_message.Content.ReadAsStringAsync();
            using (Stream stream = await a_message.Content.ReadAsStreamAsync())
            {
                if (a_message.IsSuccessStatusCode)
                {
                    return DeserializeJsonFromStream<T>(stream);
                }

                //Error
                string content = await StreamToStringAsync(stream);
                throw new ApiException
                {
                    StatusCode = (int)a_message.StatusCode,
                    Content = content
                };
            }
        }

        private static T DeserializeJsonFromStream<T>(Stream a_stream)
        {
            if (a_stream == null || a_stream.CanRead == false)
            {
                return default(T);
            }

            using (StreamReader sr = new StreamReader(a_stream))
            {
                using (var jtr = new JsonTextReader(sr))
                {
                    var js = new JsonSerializer();
                    var searchResult = js.Deserialize<T>(jtr);
                    return searchResult;
                }
            }
        }

        private static async Task<string> StreamToStringAsync(Stream a_stream)
        {
            string content = null;

            if (a_stream != null)
            {
                using (StreamReader sr = new StreamReader(a_stream))
                {
                    content = await sr.ReadToEndAsync();
                }
            }

            return content;
        }
    }
}
