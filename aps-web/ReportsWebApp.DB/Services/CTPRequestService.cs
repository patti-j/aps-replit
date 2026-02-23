using Microsoft.EntityFrameworkCore;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;
using ReportsWebApp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Services
{
    public class CTPRequestService : ICTPRequestService
    {
        private readonly DbReportsContext _dbContext;
        private readonly IAppInsightsLogger _logger; 

        public CTPRequestService(DbReportsContext dbContext, IAppInsightsLogger logger) 
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<CtpRequest>> GetCtpRequestsAsync(int PADetailsId)
        {
            return await _dbContext.CtpRequests.Where(c => c.PADetailsId == PADetailsId).ToListAsync();
        }

        public async Task<bool> SaveCtpRequestAsync(CtpRequest request)
        {
            try
            {
                if (request.Id  < 1)
                {
                    await _dbContext.CtpRequests.AddAsync(request);
                }
                else
                {
                    _dbContext.CtpRequests.Update(request);
                }
                await _dbContext.SaveChangesAsync();
                _logger.Log("CTP request saved successfully", "System", "SaveCtpRequestAsync");
                return true;
            }
            catch (Exception ex)
            {
                var correlationId = _logger.LogError(ex, "System", "SaveCtpRequestAsync");
                Console.WriteLine($"Error saving CTP request. Correlation ID: {correlationId}");
                return false;
            }
        }

        public async Task<List<JobTemplate>> GetJobTemplatesAsync(int companyId)
        {
            return await _dbContext.JobTemplates.Where(j => j.CompanyId == companyId).ToListAsync();
        }

        public async Task<bool> RemoveRequestAsync(CtpRequest request)
        {
            try
            {
                _dbContext.CtpRequests.Remove(request);
                await _dbContext.SaveChangesAsync();
                _logger.Log("CTP request removed successfully", "System", "RemoveCtpRequestAsync");
                return true;
            }
            catch (Exception ex)
            {
                var correlationId = _logger.LogError(ex, "System", "RemoveCtpRequestAsync");
                Console.WriteLine($"Error removing CTP request. Correlation ID: {correlationId}");
                return false;
            }
        }
    }
}
