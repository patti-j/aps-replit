using WebAPI.Models.Integration;

namespace WebAPI.Models;

public class PlanningAreaPATag
{
    public int PlanningAreaId { get; set; }
    public PADetails PlanningArea { get; set; }
    public int PAGroupId { get; set; }
    public PlanningAreaTag PAGroup { get; set; }
}