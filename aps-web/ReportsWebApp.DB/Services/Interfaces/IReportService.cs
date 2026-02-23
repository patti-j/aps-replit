using ReportsWebApp.DB.Models;

public interface IReportService
{
    List<Report> GetReports();
    List<Report> GetReports(int companyId);
    List<Report> GetHomePageReports(int companyId, int userId);
    Task<Report> GetReportByPBIReportId(string pbiReportId);
    List<PBIWorkspace> GetWorkspaces(int companyId);
    Task<Report> GetReportByIdAsync(int reportId);
    Task<Report> SaveAsync(Report report);
    Task<bool> DeleteAsync(int reportId);
}