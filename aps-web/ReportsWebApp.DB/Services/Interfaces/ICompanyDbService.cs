using ReportsWebApp.DB.Models;

public interface ICompanyDbService
{
    List<CompanyDb> GetCompanyDbs(int companyId, PADetails? pADetails = null);
    List<CompanyDb> GetImportDbs(int companyId);
    List<CompanyDb> GetTriggerImportEntities(int companyId);
    Task DeleteAsync(int instanceId);
    Task UpdateOneAsync(CompanyDb instance);
    List<ImportHistoryItem> GetImportHistory(CompanyDb instance);
    Task<DashtResource> GetRandomResource(CompanyDb instance);
    Task<List<DashtResource>> GetResources(CompanyDb instance, Scenario scenario);
    Task GetJobDetails(CompanyDb instance, Scenario scenario);
    Task GetConcurrenciesDetailsAsync(CompanyDb instance, Scenario scenario);
    Task GetMaterialsDetails(CompanyDb instance, Scenario scenario);
}