using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace NetsuiteIntegration.Utils
{
    public class OAuth1HeaderGenerator
    {
        private readonly NetSuiteApiConfig _config;
        private readonly HttpMethod _httpMethod;
        private readonly string _requestUrl;

        public OAuth1HeaderGenerator(NetSuiteApiConfig a_apiConfig, HttpMethod a_httpMethod, string a_requestUrl)
        {
            _config = a_apiConfig ?? throw new ArgumentNullException(nameof(a_apiConfig));
            _httpMethod = a_httpMethod ?? throw new ArgumentNullException(nameof(a_httpMethod));
            _requestUrl = a_requestUrl ?? throw new ArgumentNullException(nameof(a_requestUrl));
        }

        /// <summary>Creates the OAuth Authorization header value.</summary>
        public AuthenticationHeaderValue CreateAuthenticationHeaderValue()
        {
            return new AuthenticationHeaderValue(
                "OAuth",
                GetAuthenticationHeaderValueParameter(GenerateNonce(), GenerateTimeStamp())
            );
        }

        public string GetAuthenticationHeaderValueParameter(string a_nonce, string a_timestamp)
        {
            string signature = GenerateSignature(a_nonce, a_timestamp);

            OrderedDictionary headerParams = new OrderedDictionary
            {
                { "realm", _config.AccountId },
                { "oauth_token", _config.TokenId },
                { "oauth_consumer_key", _config.ClientId },
                { "oauth_nonce", a_nonce },
                { "oauth_timestamp", a_timestamp },
                { "oauth_signature_method", "HMAC-SHA256" },
                { "oauth_version", "1.0" },
                { "oauth_signature", signature }
            };

            string combined = CombineOAuthHeaderParams(headerParams);
            Debug.WriteLine("CreateAuthenticationHeaderValue: combinedOauthParams: " + combined);
            return combined;
        }

        private string CombineOAuthHeaderParams(OrderedDictionary a_parameters)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;

            foreach (object? key in a_parameters.Keys)
            {
                if (!first) sb.Append(", ");
                string value = Uri.EscapeDataString(a_parameters[key]!.ToString()!);
                sb.Append($"{key}=\"{value}\"");
                first = false;
            }
            return sb.ToString();
        }

        public string GenerateSignature(string a_nonce, string a_timestamp)
        {
            string baseString = GenerateSignatureBaseString(a_nonce, a_timestamp);
            string key = GenerateSignatureKey();

            Debug.WriteLine("GenerateSignature: baseString: " + baseString);
            Debug.WriteLine("GenerateSignature: key: " + key);

            using HMACSHA256 hmac = new HMACSHA256(Encoding.ASCII.GetBytes(key));
            byte[] hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(baseString));
            return Convert.ToBase64String(hash);
        }

        public string GenerateSignatureKey()
        {
            return CombineKeyParams(new List<string>
            {
                _config.ClientSecret,
                _config.TokenSecret
            });
        }

        public string GenerateSignatureBaseString(string a_nonce, string a_timestamp)
        {
            Uri uri = new Uri(_requestUrl);
            string path = uri.GetLeftPart(UriPartial.Path);
            string prefix = _httpMethod.ToString() + "&" + Uri.EscapeDataString(path) + "&";

            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "oauth_consumer_key", _config.ClientId },
                { "oauth_nonce", a_nonce },
                { "oauth_signature_method", "HMAC-SHA256" },
                { "oauth_timestamp", a_timestamp },
                { "oauth_token", _config.TokenId },
                { "oauth_version", "1.0" }
            };

            // Include query string parameters
            string query = uri.Query.TrimStart('?');
            if (!string.IsNullOrEmpty(query))
            {
                foreach (string pair in query.Split('&'))
                {
                    string[] kv = pair.Split('=');
                    if (kv.Length == 2)
                        parameters[kv[0]] = Uri.UnescapeDataString(kv[1]);
                }
            }

            string normalized = CombineBaseStringParams(parameters);
            return prefix + Uri.EscapeDataString(normalized);
        }

        private string CombineBaseStringParams(Dictionary<string, string> a_parameters)
        {
            List<string> items = new List<string>();
            foreach (KeyValuePair<string, string> kv in a_parameters)
                items.Add($"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}");

            items.Sort();

            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (string s in items)
            {
                if (!first) sb.Append('&');
                sb.Append(s);
                first = false;
            }
            return sb.ToString();
        }

        private string CombineKeyParams(List<string> a_parameters)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (string p in a_parameters)
            {
                if (!first) sb.Append('&');
                sb.Append(Uri.EscapeDataString(p));
                first = false;
            }
            return sb.ToString();
        }

        public string GenerateTimeStamp()
        {
            double seconds = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            return Convert.ToInt64(seconds).ToString();
        }

        public string GenerateNonce()
        {
            return new Random().Next(123400, 9_999_999).ToString();
        }
    }
}
