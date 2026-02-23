using PT.Common.Sql.SqlServer;

namespace PT.APIDefinitions.RequestsAndResponses;

public class StagingDBSchemaResponse
{
    public bool Success { get; set; }
    public DBIntegrationDTO? Integration { get; set; }
    public int IntegrationId { get; set; }
}