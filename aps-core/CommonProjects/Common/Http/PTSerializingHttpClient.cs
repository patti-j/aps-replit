using System.Net.Http.Headers;

namespace PT.Common.Http;

/// <summary>
/// PTHttpClient that signals the server to use direct binary serialization of PTSerializable request and response content.
/// Only clients communicating with an APS System Server should implement this class.
/// The ability to parse such response content is in the PTHttpClient base class, so it can handle it in the unexpected event it arrives as such.
/// However, the server is set up so that unless it receives a PTSerializationEnabled request header, it should not return the content in that way.
/// </summary>
public class PTSerializingHttpClient : PTHttpClient
{
    public PTSerializingHttpClient(string a_controller, string a_serverAddress = "https://localhost:8000/api/")
        : base(a_controller, a_serverAddress) { }

    public PTSerializingHttpClient(string a_controller, string a_serverAddress, TimeSpan a_timeout, AuthenticationHeaderValue a_authHeaderValue)
        : base(a_controller, a_serverAddress, a_timeout, a_authHeaderValue) { }

    public PTSerializingHttpClient(string a_controller, string a_serviceName, string a_serverAddress)
        : base(a_controller, a_serviceName, a_serverAddress) { }

    public PTSerializingHttpClient(string a_controller, InstanceKey a_instanceKey, string a_serverAddress = "http://localhost/api/")
        : base(a_controller, a_instanceKey, a_serverAddress) { }

    public override void InitializeHttpClient(string a_controller, string a_serverAddress)
    {
        base.InitializeHttpClient(a_controller, a_serverAddress);
        SetupSerialization();
    }

    private void SetupSerialization()
    {
        m_httpClient.DefaultRequestHeaders.Add("PTSerializationEnabled", "true");
    }
}