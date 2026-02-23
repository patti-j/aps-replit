using System.Net.Http.Headers;
using System.Text;

using Newtonsoft.Json;

namespace PT.APIDefinitions;

public static class ApiClientUtilities
{
    //Utility function to create bytearray for posts
    public static ByteArrayContent PreparePostContent(object obj)
    {
        string paramContent = JsonConvert.SerializeObject(obj);
        byte[] buffer = Encoding.UTF8.GetBytes(paramContent);
        ByteArrayContent byteContent = new (buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return byteContent;
    }

    public static HttpClient SetUpApiClient(int a_port)
    {
        string apiServer = $"https://localhost:{a_port}";
        HttpClient Client = new ();
        Client.BaseAddress = new Uri(apiServer);
        return Client;
    }

    public static HttpRequestMessage PreparePutContent<T>(T a_request, Uri a_uri)
    {
        string content = JsonConvert.SerializeObject(a_request);
        HttpRequestMessage request = new (HttpMethod.Put, a_uri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(content, Encoding.UTF8);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        return request;
    }
}