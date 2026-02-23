using ReportsWebApp.DB.Models;

namespace ReportsWebApp.DB.Services.Interfaces;

public interface IDataConnectorService
{
    public DataConnector? GetDataConnector(int a_dataConnectorId);
    public bool SaveDataConnector(DataConnector a_dataConnector);
    public List<DataConnector> GetDataConnectorsForCompany(int CompanyId);
    public bool DeleteDataConnector(int a_dataConnectorId);
    Task<bool> SaveDataConnectorAsync(DataConnector a_dataConnector);

}