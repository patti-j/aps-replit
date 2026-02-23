using PT.ImportDefintions.RequestsAndResponses;

namespace PT.ImportDefintions;

public interface ISystemServiceClient
{
    public PerformImportResult RunImport(PerformImportRequest a_request);
}