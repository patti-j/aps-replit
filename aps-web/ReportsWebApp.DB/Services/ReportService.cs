using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReportsWebApp.Common;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;

public class ReportService : IReportService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextFactory<DbReportsContext> _dbContextFactory;

    public ReportService(IServiceProvider serviceProvider, IDbContextFactory<DbReportsContext> dbContextFactory)
    {
        _serviceProvider = serviceProvider;
        _dbContextFactory = dbContextFactory;
    }
    private DbReportsContext GetDbContext()
    {
        return _dbContextFactory.CreateDbContext();
    }
    public List<Report> GetReports()
    {
        using var dbContext = GetDbContext();
        // Use Entity Framework Core or other data access methods to retrieve reports
        return dbContext.Reports.Include(r => r.Categories).ToList();
    }

    public List<Report> GetReports(int companyId)
    {
        using var dbContext = GetDbContext();
        // Use Entity Framework Core or other data access methods to retrieve reports
        var reports = dbContext.Reports
            .Include(r => r.Categories)
            .Include(r => r.PBIWorkspace)
            .Where(r => dbContext.Companies
                            .Where(c => c.Id == companyId)
                            .SelectMany(c => c.Workspaces)
                            .Any(w => w == r.PBIWorkspace))
            .ToList();

        return reports;
    }
    public List<Report> GetHomePageReports(int companyId, int userId)
    {
        using var dbContext = GetDbContext();
        //get roles of user
        var roles = dbContext.Roles.Include(g=>g.Categories)
              .ThenInclude(c=>c.Reports)
              .ThenInclude(r => r.PBIWorkspace)
              .Include(g=>g.Users)
              .Where(g => g.Users.Any(u => u.Id == userId && u.CompanyId == g.CompanyId));
        //get categories associated with roles and the reports with 'show on overview page' set to true
        var reports = roles.SelectMany(g => g.Categories).Distinct().SelectMany(c => c.Reports).Distinct()
            .Where(r => r.ShowOnOverview == true).ToList();
        
        return reports;
    }

    public async Task<Report> GetReportByPBIReportId(string pbiReportId)
    {
        using var dbContext = GetDbContext();
        var report = dbContext.Reports.Include(x => x.PBIWorkspace).FirstOrDefaultAsync(r => r.PBIReportId == pbiReportId);
        return await report;
    }
    public List<PBIWorkspace> GetWorkspaces(int companyId)
    {
        using var dbContext = GetDbContext();
        // Use Entity Framework Core or other data access methods to retrieve workspaces
        var workspaces = dbContext.PBIWorkspace.Where(x => x.CompanyId == companyId).ToList();

        return workspaces;
    }

    public async Task<Report> GetReportByIdAsync(int reportId)
    {
        using var dbContext1 = GetDbContext();
        return await dbContext1.Reports.Include(x => x.Categories).Include(x => x.PBIWorkspace).FirstAsync(x => x.Id == reportId);
    }

    public async Task<Report> SaveAsync(Report report)
    {
        using var dbContext = GetDbContext();
        if (report == null)
        {
            throw new ArgumentNullException(nameof(report));
        }
        if (report.Name == null)
        {
            throw new ArgumentNullException(nameof(report.Name));
        }
        if (report.PBIReportId == null)
        {
            throw new ArgumentNullException(nameof(report.PBIReportId));
        }
        if (report.PBIWorkspace == null)
        {
            throw new ArgumentNullException(nameof(report.PBIWorkspace));
        }
        if (report.PBIWorkspaceId == 0)
        {
            report.PBIWorkspaceId = report.PBIWorkspace.Id;
        }
        

        if (report.Id == 0)
        {
            // This is a new report; add it to the context
            var categories = report.Categories;

            report.Categories = null;
            report.PBIWorkspace = null;

            dbContext.Reports.Add(report);
            dbContext.SaveChanges();

            // This is an existing report; update it in the context
            var reportCategoriesDbSet = dbContext.Set<ReportCategory>("CategoryReport");
            var newRoles = categories.Select(x => new ReportCategory()
            {
                ReportsId = report.Id,
                CategoriesId = x.Id,
            }).ToList();
            reportCategoriesDbSet.AddRange(newRoles);
        }
        else
        {
            var categories = report.Categories;
            Report? dbReport = dbContext.Reports.FirstOrDefault(r => r.Id == report.Id);
            if (dbReport != null)
            {
                report.CopyPublicFieldsTo(dbReport);
            }

            dbReport.Categories = null;
            report.PBIWorkspace = null;

            dbContext.Reports.Update(dbReport);
            dbContext.SaveChanges();

            // This is an existing report; update it in the context
            var reportCategoriesDbSet = dbContext.Set<ReportCategory>("CategoryReport");
            var roles = reportCategoriesDbSet.Where(x => x.ReportsId == report.Id).ToList();
            reportCategoriesDbSet.RemoveRange(roles);
            var newRoles = categories.Select(x => new ReportCategory()
            {
                ReportsId = report.Id,
                CategoriesId = x.Id,
            }).ToList();
            reportCategoriesDbSet.AddRange(newRoles); 
        }

        dbContext.SaveChanges();
        return await GetReportByIdAsync(report.Id);
    }

    public async Task<bool> DeleteAsync(int reportId)
    {
        using var dbContext = GetDbContext();
        var report = await dbContext.Reports.FindAsync(reportId);

        if (report == null)
        {
            return false; // Report not found
        }
        report.Categories.Clear();
        var reportCategoriesDbSet = dbContext.Set<ReportCategory>("CategoryReport");
        var roles = reportCategoriesDbSet.Where(x => x.ReportsId == report.Id).ToList();
        reportCategoriesDbSet.RemoveRange(roles);
        report.PBIWorkspace = null;
        dbContext.Reports.Remove(report);
        await dbContext.SaveChangesAsync();
        return true; // Report successfully deleted
    }
}
