using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NetsuiteIntegration.ResponseObjects;
using NetsuiteIntegration.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using static System.Net.WebRequestMethods;

namespace NetsuiteIntegration
{
    public interface INetSuiteApi
    {
        /// <summary>
        /// Creates an HttpRequestMessage with OAuth1 headers and default JSON accept/user-agent headers.
        /// </summary>
        HttpRequestMessage CreateRequest(HttpMethod a_method, string a_url);

        /// <summary>
        /// Downloads a saved search and returns a dynamic JSON token. 
        /// If the payload is shaped like { "results": [...] }, you can pass a_path = "results".
        /// </summary>
        Task<JToken> GetSavedSearchAsync(string a_url, string? a_arrayPath = null);

        /// <summary>
        /// Downloads a saved search and returns a flattened DataTable (dynamic).
        /// Useful for SqlBulkCopy / SQL dumps.
        /// </summary>
        Task<DataTable> GetSavedSearchTableAsync(string a_url, string a_tableName, string? a_arrayPath = null);

        /// <summary>
        /// Downloads a list and deserializes to a strongly-typed List&lt;T&gt;.
        /// Accepts either a root array or an envelope like { "results": [...] }.
        /// </summary>
        Task<List<T>> GetListAsync<T>(string a_url, string? a_arrayPath = null);
    }

    public class NetSuiteApiLibrary
    {
        private readonly NetSuiteApiConfig _config;
        private readonly HttpClient _http;

        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
            FloatParseHandling = FloatParseHandling.Decimal, // safer for quantities
            DateParseHandling = DateParseHandling.DateTime   // let Newtonsoft try to parse dates
        };

        public NetSuiteApiLibrary(NetSuiteApiConfig a_config, HttpClient a_httpClient)
        {
            _config = a_config ?? throw new ArgumentNullException(nameof(a_config));
            _http = a_httpClient ?? throw new ArgumentNullException(nameof(a_httpClient));
            _http.Timeout = TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// Creates an authenticated request with OAuth1 and default headers.
        /// </summary>
        public HttpRequestMessage CreateRequest(HttpMethod a_method, string a_url, string a_action, string json)
        {
            if (a_action == "Import")
            {
                HttpRequestMessage req = new HttpRequestMessage(a_method, a_url);

                // OAuth1 (current implementation/class)
                OAuth1HeaderGenerator oauth = new OAuth1HeaderGenerator(_config, a_method, a_url);
                req.Headers.Authorization = oauth.CreateAuthenticationHeaderValue();

                // Headers
                req.Headers.UserAgent.ParseAdd($"PT NetSuite Import ({_config.AccountId})");
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                return req;
            }
            else if (a_action == "Publish")
            {
                HttpRequestMessage req = new HttpRequestMessage(a_method, a_url);
                OAuth1HeaderGenerator oauth = new OAuth1HeaderGenerator(_config, a_method, a_url);
                req.Headers.Authorization = oauth.CreateAuthenticationHeaderValue();

                req.Content = new StringContent(json, Encoding.UTF8, "application/json");

                return req;
            }

            return null;
        }

        /// <summary>
        /// Downloads a saved search and returns a JToken. 
        /// If the payload is { "results": [...] }, you can pass a_arrayPath = "results",
        /// or the method will normalize it automatically.
        /// </summary>
        public async Task<JToken> GetSavedSearchAsync(string a_url, string a_action, string? a_arrayPath = null)
        {
            using HttpRequestMessage req = CreateRequest(HttpMethod.Get, a_url, a_action, "");
            using HttpResponseMessage resp = await _http.SendAsync(req);

            // Reuse Aha Utilities dynamic deserializer
            JToken token = await Utilities.GetResponseObject<JToken>(resp);

            // If an array path was provided, return that segment if found
            if (!string.IsNullOrWhiteSpace(a_arrayPath))
            {
                JToken? sel = token.SelectToken(a_arrayPath);
                if (sel != null) return sel;
            }

            // Normalize { results: [...] } into an array token
            if (token is JObject obj && obj["results"] is JArray arr) return arr;

            // Could be a root JArray or a single object
            return token;
        }

        /// <summary>
        /// Downloads a list payload and converts it into List&lt;T&gt;.
        /// Works with a root array or an envelope { "results": [...] }.
        /// </summary>
        public async Task<List<T>> GetListAsync<T>(string a_url, string? a_arrayPath = null)
        {
            JToken token = await GetSavedSearchAsync(a_url, "Import", a_arrayPath);

            // Ensure we have a JArray to deserialize
            JArray? arr = null;

            if (!string.IsNullOrWhiteSpace(a_arrayPath))
                arr = token.SelectToken(a_arrayPath) as JArray;

            if (arr == null && token is JObject obj && obj["results"] is JArray r)
                arr = r;

            if (arr == null)
                arr = token as JArray ?? new JArray(token); // wrap single object as array

            // Deserialize to List<T> using tolerant settings
            return arr.ToObject<List<T>>(JsonSerializer.CreateDefault(_jsonSettings))!;
        }
        public async Task<HttpResponseMessage> PublishJobUpdates(string a_json, string a_accountId, string a_publishUrl)
        {
            string sub = a_accountId.Replace("_", "-").ToLowerInvariant();
            string publishEndpointUrl = a_publishUrl;
            using HttpRequestMessage req = CreateRequest(HttpMethod.Post, publishEndpointUrl, "Publish", a_json);
            using HttpResponseMessage resp = await _http.SendAsync(req);

            return resp;
        }

        public async Task<JArray> GetArrayAsync(string a_url, string? a_arrayPath = null)
        {
            JToken token = await GetSavedSearchAsync(a_url, "Import", a_arrayPath);
            
            if (token is JObject obj && obj["results"] is JArray r) return r;
            
            return token as JArray ?? new JArray(token);
        }

        //SuiteQL Helpers
        public async Task<JArray> RunSuiteQlPagedAsync(string a_accountId, string a_sql, int a_pageLimit = 1000)
        {
            JArray all = new JArray();
            
            string nsBase = $"https://{a_accountId.Replace("_", "-").ToLowerInvariant()}.suitetalk.api.netsuite.com";


            for (int offset = 0; ; offset += a_pageLimit)
            {
                (string raw, JArray items) = await PostSuiteQlPageAsync(nsBase, a_sql, a_pageLimit, offset);
                if (items != null && items.Count > 0)
                    foreach (JToken it in items) all.Add(it);
                if (items == null || items.Count < a_pageLimit) break;
            }
            return all;
        }
        
        private async Task<(string rawJson, JArray items)> PostSuiteQlPageAsync(string a_nsBase, string a_sql, int a_limit, int a_offset)
        {
            string url = $"{a_nsBase}/services/rest/query/v1/suiteql?limit={a_limit}&offset={a_offset}";
            using HttpRequestMessage req = CreateRequest(HttpMethod.Post, url, "Import", json: "");
            req.Headers.TryAddWithoutValidation("Prefer", "transient");
            req.Content = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(new { q = a_sql }),
                Encoding.UTF8, "application/json");

            using HttpResponseMessage resp = await _http.SendAsync(req);
            string raw = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new Exception($"SuiteQL HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {raw}");

            JObject? obj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(raw);
            JArray items = obj?["items"] as JArray ?? new JArray();
            return (raw, items);
        }


    }
}
