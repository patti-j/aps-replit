using WebAPI.Models;

namespace WebAPI.RequestsAndResponses;

public class GetPlanningAreaResponse
{
    public string Settings { get; set; }

    // TODO: When the PlanningArea Settings are split up out of the json, we'll no longer need to send these settings separately
    public ServerWideInstanceSettings ServerWideInstanceSettings { get; set; }
    public bool IsActive { get; set; }
    public bool IsBackup { get; set; }
}

/// <summary>
/// Holds instance settings that are common to all instances managed by a particular Server Manager, to be applied to all such managed instances.
/// </summary>
public class ServerWideInstanceSettings
{
    public ServerWideInstanceSettings(){}

    public ServerWideInstanceSettings(CompanyServer a_server)
    {
        ServerManagerPath = a_server.ServerManagerPath;
        Thumbprint = a_server.Thumbprint;
        SsoDomain = a_server.SsoDomain;
        SsoClientId = a_server.SsoClientId;
    }

    public string ServerManagerPath { get; set; }

    public string Thumbprint { get; set; }

    public string SsoDomain { get; set; }

    public string SsoClientId { get; set; }
}