using ReportsWebApp.DB.Models;

namespace ReportsWebApp.DB.Services.Interfaces;

public interface ICTPRequestService
{
    Task<List<CtpRequest>> GetCtpRequestsAsync(int PADetailsId);
    Task<bool> SaveCtpRequestAsync(CtpRequest request);
    Task<List<JobTemplate>> GetJobTemplatesAsync(int companyId);
    Task<bool> RemoveRequestAsync(CtpRequest request);
}