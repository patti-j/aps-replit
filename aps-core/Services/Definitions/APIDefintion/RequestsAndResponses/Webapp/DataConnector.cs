using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PT.APIDefinitions.RequestsAndResponses.Webapp;

public class DataConnector
{
    public int Id { get; set; }
    
    public string Name { get; set; } = "";
    
    public int CompanyId { get; set; }
    
    public string ConnectionString { get; set; } = "";
}