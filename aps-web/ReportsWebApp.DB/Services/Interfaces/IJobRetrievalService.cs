using ReportsWebApp.DB.Models;

public interface IJobRetrievalService
{
    Task GetJobDetailsAsync(CompanyDb db, Scenario scenario);
    Task GetConcurrenciesDetailsAsync(CompanyDb db, Scenario scenario);
    Task GetMaterialsDetails(CompanyDb db, Scenario scenario);
}