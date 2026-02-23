using System.Collections.Specialized;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Web;

using Newtonsoft.Json;

using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace PT.Common.Http;

/// <summary>
/// Client used to communicate with server manager
/// </summary>
public class PTHttpClient
{
    protected HttpClient m_httpClient;

    //Software Name, Software Version
    public InstanceKey m_instanceKey;
    public string m_defaultController;
    private static readonly List<string> m_registeredThumbprints = new ();
    private IConnectionStateManager m_stateManager;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="a_controller"></param>
    /// <param name="a_serverAddress"></param>
    public PTHttpClient(string a_controller, string a_serverAddress = "https://localhost:8000/api/")
    {
        InitializeHttpClient(a_controller, a_serverAddress);
        m_httpClient.Timeout = TimeSpan.FromMinutes(15);
    }

    /// <summary>
    /// Configure timeout on creation
    /// </summary>
    /// <param name="a_controller"></param>
    /// <param name="a_serverAddress"></param>
    /// <param name="a_timeout"></param>
    /// <param name="a_authHeaderValue"></param>
    public PTHttpClient(string a_controller, string a_serverAddress, TimeSpan a_timeout, AuthenticationHeaderValue a_authHeaderValue)
    {
        InitializeHttpClient(a_controller, a_serverAddress);
        m_httpClient.Timeout = a_timeout;
        m_httpClient.DefaultRequestHeaders.Authorization = a_authHeaderValue;
    }

    public virtual void InitializeHttpClient(string a_controller, string a_serverAddress)
    {
        HttpClientHandler httpClientHandler = new ();
        httpClientHandler.ServerCertificateCustomValidationCallback += SelfSignedOverride;
        m_httpClient = new HttpClient(httpClientHandler) { BaseAddress = new Uri(a_serverAddress) };
        m_defaultController = a_controller;
        m_httpClient.DefaultRequestHeaders.Accept.Clear();
        m_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        m_httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        m_httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
    }

    /// <summary>
    /// Force deserialization of API response object. Need this if the response object includes an error message or messages on unsuccessful API calls.
    /// </summary>
    public bool ForceDeserializeResponseObject { get; set; }


    public PTHttpClient(string a_controller, string a_serviceName, string a_serverAddress) : this(a_controller, a_serverAddress)
    {
        m_instanceKey = new InstanceKey(a_serviceName);
    }

    public PTHttpClient(string a_controller, InstanceKey a_instanceKey, string a_serverAddress = "http://localhost/api/") : this(a_controller, a_serverAddress)
    {
        m_instanceKey = a_instanceKey;
    }

    protected void Authenticate(AuthenticationHeaderValue a_authHeader)
    {
        m_httpClient.DefaultRequestHeaders.Authorization = a_authHeader;
    }

    public static void RegisterOverrideThumbprint(string a_thumbprint)
    {
        if (!m_registeredThumbprints.Contains(a_thumbprint))
        {
            m_registeredThumbprints.Add(a_thumbprint);
        }
    }

    public static void UnregisterOverrideThumbprint(string a_thumbprint)
    {
        if (m_registeredThumbprints.Contains(a_thumbprint))
        {
            m_registeredThumbprints.Remove(a_thumbprint);
        }
    }

    private static bool SelfSignedOverride(HttpRequestMessage a_arg1, X509Certificate2? a_certificate, X509Chain? a_arg3, SslPolicyErrors a_arg4)
    {
        if (a_arg4 == SslPolicyErrors.None || m_registeredThumbprints.Contains("*"))
        {
            return true;
        }

        string thumbprint = a_certificate?.Thumbprint;

        if (thumbprint != null)
        {
            foreach (string registeredThumbprint in m_registeredThumbprints)
            {
                if (string.Compare(thumbprint, registeredThumbprint, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase) == 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    #region Utilities
    public static HttpRequestMessage GenerateMessage(string a_endpoint, object a_object, HttpMethod a_httpMethod)
    {
        
        HttpRequestMessage request = new (a_httpMethod, a_endpoint);

        // Don't serialize byte array payloads.
        if (a_object is byte[] byteArray)
        {
            request.Content = new ByteArrayContent(byteArray);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        }
        else
        {
            string content = System.Text.Json.JsonSerializer.Serialize(a_object, new JsonSerializerOptions());
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");
        }

        return request;
    }

    private static async Task ValidateResponse(HttpResponseMessage a_message)
    {
        using (Stream decompressedStream = await TryDecompressStream(a_message.Content))
        {
            if (!a_message.IsSuccessStatusCode)
            {
                //Error
                Task<string> content = StreamToStringAsync(decompressedStream);
                throw new ApiException
                {
                    StatusCode = (int)a_message.StatusCode,
                    Content = content.Result
                };
            }
        }
    }

    private static async Task<Stream> GetStreamFromHttpMessage(HttpResponseMessage a_message, bool a_forceDeserializeResponseObject)
    {
        using (Stream decompressedStream = await TryDecompressStream(a_message.Content))
        {
            if (a_message.IsSuccessStatusCode || a_forceDeserializeResponseObject)
            {
                if (decompressedStream == null)
                {
                    throw new ApiException
                    {
                        StatusCode = (int)a_message.StatusCode,
                        Content = "No content for successful response"
                    };
                }

                if (!decompressedStream.CanRead)
                {
                    throw new ApiException
                    {
                        StatusCode = (int)a_message.StatusCode,
                        Content = "Invalid content for successful response"
                    };
                }
            }

            //Error
            Task<string> content = StreamToStringAsync(decompressedStream);
            throw new ApiException
            {
                StatusCode = (int)a_message.StatusCode,
                Content = content.Result
            };
        }
    }

    private static async Task<T> GetResponseObject<T>(HttpResponseMessage a_message, bool a_forceDeserializeResponseObject) where T : class
    {
        using (Stream decompressedStream = await TryDecompressStream(a_message.Content))
        {
            if (a_message.IsSuccessStatusCode || a_forceDeserializeResponseObject)
            {
                if (decompressedStream == null)
                {
                    throw new ApiException
                    {
                        StatusCode = (int)a_message.StatusCode,
                        Content = "No content for successful response"
                    };
                }

                if (!decompressedStream.CanRead)
                {
                    throw new ApiException
                    {
                        StatusCode = (int)a_message.StatusCode,
                        Content = "Invalid content for successful response"
                    };
                }

                if (a_message.Content.Headers.ContentType?.ToString() == "application/octet-stream")
                {
                    if (typeof(IPTSerializable).IsAssignableFrom(typeof(T)) && a_message.Headers.Contains("PTSerializationEnabled"))
                    {
                        return HandlePTSerializable<T>(decompressedStream as MemoryStream);
                    }

                    if (typeof(T) == typeof(ByteResponse))
                    {
                        return new ByteResponse { Content = DeserializeBytesFromStream(decompressedStream) } as T;
                    }

                    if (typeof(T) == typeof(byte[]))
                    {
                        return DeserializeBytesFromStream(decompressedStream) as T;
                    }

                    throw new ApiException
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Content = "Endpoint requires a byte response"
                    };
                }

                return DeserializeJsonFromStream<T>(decompressedStream);
            }

            //Error
            Task<string> content = StreamToStringAsync(decompressedStream);
            string contentMessage = CleanDeserializedMessage(content.Result);
            throw new ApiException
            {
                StatusCode = (int)a_message.StatusCode,
                Content = contentMessage
            };
        }
    }

    private static T HandlePTSerializable<T>(Stream a_decompressedStream) where T : class
    {
        byte[] bytes = DeserializeBytesFromStream(a_decompressedStream);
        try
        {
            return Serialization.TryDeserialize<T>(bytes) as T;
        }
        catch (ArgumentException e)
        {
            throw new ApiException
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Content = e.Message
            };
        }
        catch (Exception e)
        {
            throw new ApiException
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Content = $"An error occurred handling a PTSerializable response: {e.Message}"
            };
        }
    }

    /// <summary>
    /// Decompress the stream if the HTTP Content is encoded, return the original stream otherwise
    /// </summary>
    /// <param name="a_content"></param>
    /// <returns></returns>
    private static async Task<Stream> TryDecompressStream(HttpContent a_content)
    {
        long? foo = a_content.Headers.ContentLength;
        string bar = a_content.Headers.ContentType?.ToString();
        string contentEncoding = "Not Encoded";
        if (a_content.Headers.ContentEncoding.Count > 0)
        {
            contentEncoding = string.Join(" ", a_content.Headers.ContentEncoding.ToArray());
        }

        Stream stream = await a_content.ReadAsStreamAsync();
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        //Support gzip compression
        if (contentEncoding.ToLower().Contains("gzip"))
        {
            GZipStream gzipDecompressedStream = new (stream, CompressionMode.Decompress);
            return gzipDecompressedStream;
        }

        //Support deflate compression
        if (contentEncoding.ToLower().Contains("deflate"))
        {
            DeflateStream deflateDecompressedStream = new (stream, CompressionMode.Decompress);
            return deflateDecompressedStream;
        }

        //Support br compression
        if (contentEncoding.ToLower().Contains("br"))
        {
            BrotliStream brotliDecompressedStream = new (stream, CompressionMode.Decompress);
            return brotliDecompressedStream;
        }

        return stream;
    }

    private static byte[] DeserializeBytesFromStream(Stream a_stream)
    {
        using (MemoryStream memoryStream = new ())
        {
            a_stream.CopyTo(memoryStream);
            byte[] result = memoryStream.ToArray();

            return result;
        }
    }

    private static T DeserializeJsonFromStream<T>(Stream a_stream)
    {
        using (StreamReader sr = new (a_stream))
        {
            using (JsonTextReader jtr = new (sr))
            {
                JsonSerializer js = new ();
                T searchResult = js.Deserialize<T>(jtr);
                return searchResult;
            }
        }
    }

    private static async Task<string> StreamToStringAsync(Stream a_stream)
    {
        if (a_stream != null)
        {
            using (StreamReader sr = new (a_stream))
            {
                return await Task.Run(() => sr.ReadToEndAsync());
            }
        }

        return null;
    }

    private static string CleanDeserializedMessage(string message)
    {
        message = message.TrimStart('"');
        message = message.TrimEnd('"');

        message = message.Replace("\\r\\n", Environment.NewLine);
        message = message.Replace("\\r", Environment.NewLine);
        message = message.Replace("\\n", Environment.NewLine);

        string cleanDeserializedMessage = message.TrimEnd(Environment.NewLine.ToCharArray()).Trim();
        return cleanDeserializedMessage;
    }



    public async Task<HttpResponseMessage> SendPostRequestAsync(string a_endpointName, object a_content = null, string a_route = null)
    {
        string absoluteRoute = BuildRequestUri(a_route, a_endpointName);
        HttpRequestMessage request = GenerateMessage(absoluteRoute, a_content ?? m_instanceKey, HttpMethod.Post);
        return await m_httpClient.PostAsync(request.RequestUri, request.Content).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> SendPutRequestAsync(string a_endpointName, object a_content = null, string a_route = null)
    {
        string absoluteRoute = BuildRequestUri(a_route, a_endpointName);
        HttpRequestMessage request = GenerateMessage(absoluteRoute, a_content ?? m_instanceKey, HttpMethod.Put);
        return await m_httpClient.PutAsync(request.RequestUri, request.Content).ConfigureAwait(false);
    }

    /// <summary>
    /// Take an endpoint, an optional content for the body of the post, and an option fully qualified route and send the post response async.
    /// Then deserialize the response object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a_endpointName"></param>
    /// <param name="a_content"></param>
    /// <param name="a_route"></param>
    /// <exception cref="AggregateException">Default exception</exception>
    /// <returns></returns>
    public async Task<T> MakePostRequestAsync<T>(string a_endpointName, object a_content = null, string a_route = null) where T : class
    {
        HttpResponseMessage response = await SendPostRequestAsync(a_endpointName, a_content, a_route).ConfigureAwait(false);

        return await GetResponseObject<T>(response, ForceDeserializeResponseObject).ConfigureAwait(false);
    }

    /// <summary>
    /// Take an endpoint, an optional content for the body of the post, and an option fully qualified route and send the post response async.
    /// </summary>
    /// <param name="a_endpointName"></param>
    /// <param name="a_content"></param>
    /// <param name="a_route"></param>
    /// <returns></returns>
    public async Task MakePostRequestAsync(string a_endpointName, object a_content = null, string a_route = null)
    {
        HttpResponseMessage response = await SendPostRequestAsync(a_endpointName, a_content, a_route).ConfigureAwait(false);
        ;
        await ValidateResponse(response).ConfigureAwait(false);
    }

    /// <summary>
    /// Take an endpoint, an optional content for the body of the post, and an option fully qualified route and send the post response.
    /// This will filter any AggregateExceptions that are caught and only throw a single one designated in the FilterException function
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a_endpointName"></param>
    /// <param name="a_content"></param>
    /// <param name="a_route"></param>
    /// <exception cref="ApiException"></exception>
    /// <exception cref="HttpRequestException"></exception>
    /// <exception cref="System.Security.Authentication.AuthenticationException"></exception>
    /// <exception cref="System.Net.Sockets.SocketException"></exception>
    /// <exception cref="Exception">Default exception</exception>
    /// <returns></returns>
    public T MakePostRequest<T>(string a_endpointName, object a_content = null, string a_route = null) where T : class
    {
        RoundTripConnectionInfo info = null;

        try
        {
            if (m_stateManager != null)
            {
                info = RoundTripConnectionInfo.BeginSend(a_endpointName);
                HandleBadConnectionState(a_endpointName, info);
            }

            T result = MakePostRequestAsync<T>(a_endpointName, a_content, a_route).Result;
            info?.Success();

            return result;
        }
        catch (AggregateException e)
        {
            Exception filteredException = FilterExceptions(e);
            info?.Fail(filteredException);
            throw filteredException;
        }
        finally
        {
            //Notify manager.
            m_stateManager?.Log(info);
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="a_endpointName"></param>
    /// <param name="a_content"></param>
    /// <param name="a_route"></param>
    public Task MakePostRequest(string a_endpointName, object a_content = null, string a_route = null)
    {
        try
        {
            return MakePostRequestAsync(a_endpointName, a_content, a_route);
        }
        catch (AggregateException e)
        {
            throw FilterExceptions(e);
        }
    }

    /// <summary>
    /// Take an endpoint, an optional content for the body of the put, and an option fully qualified route and send the put response.
    /// This will filter any AggregateExceptions that are caught and only throw a single one designated in the FilterException function
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a_endpointName"></param>
    /// <param name="a_content"></param>
    /// <param name="a_route"></param>
    /// <exception cref="ApiException"></exception>
    /// <exception cref="HttpRequestException"></exception>
    /// <exception cref="System.Security.Authentication.AuthenticationException"></exception>
    /// <exception cref="System.Net.Sockets.SocketException"></exception>
    /// <exception cref="Exception">Default exception</exception>
    /// <returns></returns>
    public T MakePutRequest<T>(string a_endpointName, object a_content = null, string a_route = null) where T : class
    {
        RoundTripConnectionInfo info = null;

        try
        {
            if (m_stateManager != null)
            {
                info = RoundTripConnectionInfo.BeginSend(a_endpointName);
                HandleBadConnectionState(a_endpointName, info);
            }

            T result = MakePutRequestAsync<T>(a_endpointName, a_content, a_route).Result;
            info?.Success();

            return result;
        }
        catch (AggregateException e)
        {
            Exception filteredException = FilterExceptions(e);
            info?.Fail(filteredException);
            throw filteredException;
        }
        finally
        {
            //Notify manager.
            m_stateManager?.Log(info);
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="a_endpointName"></param>
    /// <param name="a_content"></param>
    /// <param name="a_route"></param>
    public Task MakePutRequest(string a_endpointName, object a_content = null, string a_route = null)
    {
        try
        {
            return MakePutRequestAsync(a_endpointName, a_content, a_route);
        }
        catch (AggregateException e)
        {
            throw FilterExceptions(e);
        }
    }


    /// <summary>
    /// Take an endpoint, an optional content for the body of the put, and an option fully qualified route and send the put response async.
    /// Then deserialize the response object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a_endpointName"></param>
    /// <param name="a_content"></param>
    /// <param name="a_route"></param>
    /// <exception cref="AggregateException">Default exception</exception>
    /// <returns></returns>
    public async Task<T> MakePutRequestAsync<T>(string a_endpointName, object a_content = null, string a_route = null) where T : class
    {
        HttpResponseMessage response = await SendPutRequestAsync(a_endpointName, a_content, a_route).ConfigureAwait(false);

        return await GetResponseObject<T>(response, ForceDeserializeResponseObject).ConfigureAwait(false);
    }

    /// <summary>
    /// Take an endpoint, an optional content for the body of the put, and an option fully qualified route and send the put response async.
    /// </summary>
    /// <param name="a_endpointName"></param>
    /// <param name="a_content"></param>
    /// <param name="a_route"></param>
    /// <returns></returns>
    public async Task MakePutRequestAsync(string a_endpointName, object a_content = null, string a_route = null)
    {
        HttpResponseMessage response = await SendPutRequestAsync(a_endpointName, a_content, a_route).ConfigureAwait(false);

        await ValidateResponse(response).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> SendGetRequest(string a_endpointName, string a_route = null, params GetParam[] a_paramList)
    {
        string absoluteRoute = BuildRequestUri(a_route, a_endpointName);

        UriBuilder uriBuilder = new (absoluteRoute);
        NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Query);
        if (a_paramList != null)
        {
            foreach (GetParam getParam in a_paramList)
            {
                query[getParam.Name] = getParam.Value;
            }
        }

        uriBuilder.Query = query.ToString();
        absoluteRoute = uriBuilder.ToString();

        return await m_httpClient.GetAsync(absoluteRoute).ConfigureAwait(false);
    }

    /// <summary>
    /// Take in a list of parameters and make a get call to the desired endpoint
    /// Then deserialize the response object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a_endpointName"></param>
    /// <param name="a_route"></param>
    /// <param name="a_paramList"></param>
    /// <exception cref="AggregateException">Default exception</exception>
    /// <returns></returns>
    public async Task<T> MakeGetRequestAsync<T>(string a_endpointName, string a_route = null, params GetParam[] a_paramList) where T : class
    {
        HttpResponseMessage response = await SendGetRequest(a_endpointName, a_route, a_paramList).ConfigureAwait(false);
        return await GetResponseObject<T>(response, ForceDeserializeResponseObject).ConfigureAwait(false);
    }

    /// <summary>
    /// Take in a list of parameters and make a get call to the desired endpoint
    /// </summary>
    /// <param name="a_endpointName"></param>
    /// <param name="a_route"></param>
    /// <param name="a_paramList"></param>
    /// <returns></returns>
    public async Task MakeGetRequestAsync(string a_endpointName, string a_route = null, params GetParam[] a_paramList)
    {
        HttpResponseMessage response = await SendGetRequest(a_endpointName, a_route, a_paramList).ConfigureAwait(false);
        await ValidateResponse(response).ConfigureAwait(false);
    }

    /// <summary>
    /// Take in a list of parameters and make a get call to the desired endpoint
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a_endpointName"></param>
    /// <param name="a_route"></param>
    /// <param name="a_paramList"></param>
    /// <exception cref="ApiException"></exception>
    /// <exception cref="HttpRequestException"></exception>
    /// <exception cref="System.Security.Authentication.AuthenticationException"></exception>
    /// <exception cref="System.Net.Sockets.SocketException"></exception>
    /// <exception cref="Exception">Default exception</exception>
    /// <returns></returns>
    public T MakeGetRequest<T>(string a_endpointName, string a_route = null, params GetParam[] a_paramList) where T : class
    {
        try
        {
            return MakeGetRequestAsync<T>(a_endpointName, a_route, a_paramList).Result;
        }
        catch (AggregateException e)
        {
            throw FilterExceptions(e);
        }
    }
    #endregion

    //public Exception FilterExceptions(AggregateException a_aggregateException)
    //{
    //    foreach(Exception e in  a_aggregateException.InnerExceptions)
    //    {
    //        if (e is ApiException)
    //        {
    //            return e;
    //        }
    //    }

    //    foreach (Exception e in a_aggregateException.InnerExceptions)
    //    {
    //        if (e is HttpRequestException)
    //        {
    //            return e;
    //        }
    //    }

    //    foreach (Exception e in a_aggregateException.InnerExceptions)
    //    {
    //        if (e is System.Security.Authentication.AuthenticationException)
    //        {
    //            return e;
    //        }
    //    }

    //    foreach (Exception e in a_aggregateException.InnerExceptions)
    //    {
    //        if (e is System.Net.Sockets.SocketException)
    //        {
    //            return e;
    //        }
    //    }

    //    return a_aggregateException.InnerException;
    //}

    public Exception FilterExceptions(AggregateException a_aggregateException, params Type[] a_exceptions)
    {
        foreach (Type e in a_exceptions)
        {
            Exception[] matches = a_aggregateException.InnerExceptions.Where(x => x.GetType() == e).ToArray();
            if (matches.Length > 0)
            {
                return matches[0];
            }
        }

        return a_aggregateException.InnerException;
    }

    public Exception FilterExceptions(AggregateException a_aggregateException)
    {
        return FilterExceptions(a_aggregateException,
            typeof(ApiException),
            typeof(HttpRequestException),
            typeof(System.Security.Authentication.AuthenticationException),
            typeof(System.Net.Sockets.SocketException));
    }

    public InstanceKey GetInstanceNameAndVersionFromServiceName(string a_serviceName)
    {
        return new InstanceKey(a_serviceName);
    }

    /// <summary>
    /// Ensure provided route and endpoint arguments are structured as an expected uri.
    /// </summary>
    /// <param name="a_route"></param>
    /// <returns></returns>
    private string BuildRequestUri(string a_route, string a_endpointName)
    {
        a_route = string.IsNullOrEmpty(a_route) ? m_defaultController : a_route;
        a_route = PTHttpUtility.EnsureTrailingSlash(a_route);

        string absoluteRoute = PTHttpUtility.UriCombine(m_httpClient.BaseAddress.AbsoluteUri, a_route);

        string fullUri = PTHttpUtility.UriCombine(absoluteRoute, a_endpointName);
        return fullUri;
    }

    #region Connection State Management

    //TODO: This could be a list of objects
    public void AttachStateManager(IConnectionStateManager a_stateManager)
    {
        m_stateManager = a_stateManager;
    }
    /// <summary>
    /// Checks if established connection has been lost.
    /// If <see cref="AttachStateManager"/> has not been called, this will always return false.
    /// </summary>
    /// <returns></returns>
    public bool IsConnectionDown => m_stateManager?.IsConnectionDown ?? false;

    /// <summary>
    /// Using State Manager, pre-emptively stops HTTP traffic when the connection is down.
    /// This prevents the client from hanging attempting to call when the server won't respond.
    /// </summary>
    /// <param name="a_endpointName">Name of the endpoint being called. The State Manager should accept regular polling traffic
    /// to confirm if the connection is reestablished, but may block nonessential requests.</param>
    /// <param name="a_info">Stores connection data to be updated by this check.</param>
    private void HandleBadConnectionState(string a_endpointName, RoundTripConnectionInfo a_info)
    {
        // disconnectMessage is returned from outside of this PTCommon class so Localization can be used.
        if (m_stateManager.ShouldRequestBePrevented(a_endpointName, out string disconnectMessage))
        {
            ApiException preventedRequestE = new ApiException()
            {
                StatusCode = (int)HttpStatusCode.ServiceUnavailable,
                Content = disconnectMessage
            };
            a_info.Fail(preventedRequestE);
            throw preventedRequestE;
        }
    }
    #endregion

}