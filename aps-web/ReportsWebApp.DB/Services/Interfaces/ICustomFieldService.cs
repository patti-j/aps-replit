using ReportsWebApp.DB.Models;

public interface ICustomFieldService
{
    Task<List<Company>> GetCompaniesAsync();
    Task<CFGroup> CreateOrUpdateCFGroupAsync(CFGroup group);
    Task<List<CustomField>> GetCFsForCompanyAsync(int companyId);
    Task<CustomField> AddOrUpdateCFAsync(CustomField cf);
    Task DeleteCFAsync(CustomField cf);
}