using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;

namespace ReportsWebApp.DB.Services
{
    public class IntegrationConfigService : IIntegrationConfigService
    {
        private readonly IServiceProvider _serviceProvider;

        public IntegrationConfigService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        private DbReportsContext GetDbContext()
        {
            return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DbReportsContext>();
        }

        public async Task<List<IntegrationConfig>> GetByCompanyIdAsync(int companyId)
        {
            await using var context = GetDbContext();
            return await context.IntegrationConfigs
                .Where(c => c.CompanyId == companyId)
                .Include(i => i.LastEditingUser)
                .ToListAsync();
        }

        public async Task<IntegrationConfig> UpdateAsync(IntegrationConfig config)
        {
            await using var context = GetDbContext();
            var existing = await context.IntegrationConfigs.Include(x => x.LastEditingUser).Where(x => x.Id == config.Id).SingleOrDefaultAsync();
            if (existing == null)
            {
                throw new InvalidOperationException("Attempted to update an IntegrationConfig that does not exist.");
            }
            
            existing.Name = config.Name;
            existing.LastEditedDate = config.LastEditedDate;
            existing.LastEditingUserId = config.LastEditingUserId;
            context.IntegrationConfigs.Update(existing);
            await context.SaveChangesAsync();
            var ret = await context.IntegrationConfigs.Include(x => x.LastEditingUser).Where(x => x.Id == existing.Id).SingleOrDefaultAsync();
            return ret;
        }

        public async Task DeleteAsync(IntegrationConfig config)
        {
            await using var context = GetDbContext();
            IntegrationConfig? existing = context.IntegrationConfigs.FirstOrDefault(c => c.Id == config.Id);
            if (existing == null) throw new InvalidOperationException("Attempted to delete an IntegrationConfig that does not exist.");
            context.IntegrationConfigs.Remove(existing);
            await context.SaveChangesAsync();
        }
    }
}
