using ReportsWebApp.DB.Models;

public class JobRetrievalService : IJobRetrievalService
{
    private readonly ICompanyDbService m_companyDbService;

    public JobRetrievalService(ICompanyDbService companyDbService)
    {
        m_companyDbService = companyDbService;
    }

    public async Task GetJobDetailsAsync(CompanyDb db, Scenario scenario)
    {
         await m_companyDbService.GetJobDetails(db, scenario);
    }
    public async Task GetConcurrenciesDetailsAsync(CompanyDb db, Scenario scenario)
    {
         await m_companyDbService.GetConcurrenciesDetailsAsync(db, scenario);
    }
    public async Task GetMaterialsDetails(CompanyDb db, Scenario scanerio)
    {
         await m_companyDbService.GetMaterialsDetails(db, scanerio);
    }
}
