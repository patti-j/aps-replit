namespace WebAPI.RequestsAndResponses;

public class GetPlanningAreaDetailsResponse
{
    public string Name { get; set; }
    public string Key { get; set; }
    public string Version { get; set; }
    public string Environment { get; set; }
    public int? ActiveIntegrationId { get; set; }
}