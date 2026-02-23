using WebAPI.Models.Integration;

namespace WebAPI.RequestsAndResponses;

public class GetAllPlanningAreaDetailsResponse
{
    public List<PlanningAreaDetailsResponseCompany> Companies { get; set; }
}

public class PlanningAreaDetailsResponseCompany
{
    public string CompanyName { get; set; }
    public int CompanyId { get; set; }
    public List<GetPlanningAreaDetailsResponse> PlanningAreaDetails { get; set; }
    public List<DBIntegrationDTO> DBIntegrations { get; set; }
}