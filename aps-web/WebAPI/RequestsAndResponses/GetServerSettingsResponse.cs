using WebAPI.Models;

namespace WebAPI.RequestsAndResponses;

public class GetServerSettingsResponse
{
    public GetServerSettingsResponse(CompanyServer a_server)
    {
        ServerSettings = a_server;

    }

    public CompanyServer ServerSettings { get; set; }
}