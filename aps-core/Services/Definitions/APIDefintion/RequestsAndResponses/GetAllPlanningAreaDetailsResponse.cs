using PT.Common.Sql.SqlServer;

namespace PT.APIDefinitions.RequestsAndResponses;

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

public class GetPlanningAreaDetailsResponse
{
    public string Name { get; set; }
    public string Key { get; set; }
    public string Version { get; set; }
    public string Environment { get; set; }
    public int? ActiveIntegrationId { get; set; }
}