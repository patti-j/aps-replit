using System.ComponentModel.DataAnnotations;

using ReportsWebApp.Shared;

namespace ReportsWebApp.Data;

public class AhaIntegrationModel
{
    public string Endpoint { get; set; } = "";
    public string Username { get; set; } = "";
    [Password]
    public string Password { get; set; } = "";
    [Password]
    public string APIToken { get; set; } = "";
}