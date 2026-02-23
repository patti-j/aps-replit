namespace PT.APIDefinitions.RequestsAndResponses;

public class CreateIntegrationRequest
{
    public List<string> ObjectsToInclude { get; set; } = new ();
    public string IntegrationName { get; set; }
    public string Version { get; set; }
    public string VersionNotes { get; set; }
    public int? CompanyId { get; set; }
}