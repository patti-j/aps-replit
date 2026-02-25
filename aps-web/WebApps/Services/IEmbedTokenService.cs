namespace ReportsWebApp.Services
{
    public interface IEmbedTokenService
    {
        string GenerateEmbedToken(string email, int companyId, bool hasAIAnalyticsRole, bool isCompanyAdmin);
    }
}