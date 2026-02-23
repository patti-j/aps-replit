using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using System.Data;
using Dapper;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReportsWebApp.Shared;

public class CompanyDbService : ICompanyDbService
{
    private readonly DbReportsContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppInsightsLogger _logger;

    public CompanyDbService(DbReportsContext dbContext, IConfiguration configuration, IServiceProvider serviceProvider, IAppInsightsLogger logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _serviceProvider=serviceProvider;
        _logger = logger;
    }
    private DbReportsContext GetDbContext()
    {
        // Resolve a new scoped DbContext using the IServiceProvider
        return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DbReportsContext>();
    }
    public List<CompanyDb> GetCompanyDbs(int companyId, PADetails? pADetails = null)
    {
        using var dbContext1 = GetDbContext();
        return dbContext1.CompanyDbs
            .Where(x => x.CompanyId == companyId && (pADetails == null || (x.Name == pADetails.Name && x.Environment == pADetails.Environment && x.DbType == EDbType.Publish)))
            .ToList();
    }

    public List<CompanyDb> GetImportDbs(int companyId)
    {
        return _dbContext.CompanyDbs.Where(x => x.CompanyId == companyId && x.DbType == EDbType.Import).ToList();
    }

    public List<CompanyDb> GetTriggerImportEntities(int companyId)
    {
        return _dbContext.CompanyDbs.Where(x => x.CompanyId == companyId && x.DbType == EDbType.Import && x.ImportUserName != String.Empty && x.ImportUserPasswordKey != String.Empty).ToList();
    }
    public async Task DeleteAsync(int instanceId)
    {
        var item = await _dbContext.CompanyDbs.FirstAsync(x => x.Id == instanceId);
        _dbContext.CompanyDbs.Remove(item);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateOneAsync(CompanyDb instance)
    {
        _dbContext.Update(instance);
        await _dbContext.SaveChangesAsync();
    }

    public List<ImportHistoryItem>? GetImportHistory(CompanyDb instance)
    {
        string connectionString;
        try
        {
            string dbPassword = GetValueFromAzureKeyVault(instance.DBPasswordKey ?? "");
            connectionString = $"Data Source={instance.DBServerName};Initial Catalog={instance.DBName};User ID={instance.DBUserName};Password={dbPassword};MultipleActiveResultSets=true;Encrypt=False";
        } catch (Exception ex)
        {
            _logger.LogError(ex);
            return null;
        }
        var items = new List<ImportHistoryItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var readSql = $"select [import_date], [file_name], [import_user], [transaction_id], [import_details] from import._log_import order by [import_date] desc";
                    SqlCommand createTableCommand = new SqlCommand(readSql, connection);

                    var reader = createTableCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        var item = (IDataRecord)reader;

                        items.Add(new ImportHistoryItem
                        {
                            ImportDate = GetValue<DateTime>(item, "import_date"),
                            FileName = GetValue<string>(item, "file_name"),
                            ImportUser = GetValue<string>(item, "import_user"),
                            TransactionId = item.IsDBNull(3) ? null : GetValue<string>(item, "transaction_id"),
                            Details = item.IsDBNull(4) ? null : GetValue<string>(item, "import_details")
                        });
                    }
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex);
                return new List<ImportHistoryItem>();
            }
		
        return items;
    }
    public async Task<DashtResource> GetRandomResource(CompanyDb instance)
    {
        string dbPassword = GetValueFromAzureKeyVault(instance.DBPasswordKey);
        string connectionString = $"Data Source={instance.DBServerName};Initial Catalog={instance.DBName};User ID={instance.DBUserName};Password={dbPassword};MultipleActiveResultSets=true;Encrypt=True";
        using (var connection = new SqlConnection(connectionString))
        {
            const string query = "SELECT TOP 1 * FROM [publish].[DASHt_Resources] ORDER BY NEWID()";
            await connection.OpenAsync();
            return await connection.QueryFirstAsync<DashtResource>(query);
        }
    }
    
    public async Task<List<DashtResource>> GetResources(CompanyDb instance, Scenario scenario)
    {
        string dbPassword = GetValueFromAzureKeyVault(instance.DBPasswordKey);
        string connectionString = $"Data Source={instance.DBServerName};Initial Catalog={instance.DBName};User ID={instance.DBUserName};Password={dbPassword};MultipleActiveResultSets=true;Encrypt=True";
        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT * FROM [publish].[DASHt_Resources] " +
                        "WHERE [NewScenarioId] = @ScenarioId";
            var items = await connection.QueryAsync<DashtResource>(query, new { ScenarioId = scenario.Id});
            return items.ToList();
        }
    }
    private string ConstructConnectionString(CompanyDb instance)
    {
        string connectionString = $"Server={(string.IsNullOrEmpty(instance.DBServerName) ? "(local)\\SQLEXPRESS" : instance.DBServerName)};Database={instance.DBName};Trusted_Connection=True;Encrypt=False;MultipleActiveResultSets=True;";
        if (!string.IsNullOrEmpty(instance.DBPasswordKey))
        {
            string dbPassword = GetValueFromAzureKeyVault(instance.DBPasswordKey);
            connectionString += $"Password={dbPassword}";
        }
        return connectionString;
    }

    public async Task GetJobDetails(CompanyDb instance, Scenario scenario)
    {
        string dbPassword = GetValueFromAzureKeyVault(instance.DBPasswordKey);
        string connectionString = $"Data Source={instance.DBServerName};Initial Catalog={instance.DBName};User ID={instance.DBUserName};Password={dbPassword};MultipleActiveResultSets=true;Encrypt=True";

        //string connectionString = GetValueFromAzureKeyVault(instance.ConnectionStringKey);
        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT * FROM [publish].[DASHt_Planning]  " +
                        "WHERE [NewScenarioId] = @ScenarioId";
            var items = await connection.QueryAsync<DashtPlanning>(query, new { ScenarioId = scenario.Id });

            //query = "SELECT * FROM [publish].[DASHt_Planning] WHERE JobName LIKE 'WO-3-001' ";
            scenario.DetailDashtPlanning = items.ToList();
        }
    }
    public async Task GetConcurrenciesDetailsAsync(CompanyDb instance, Scenario scenario)
    {
        string dbPassword = GetValueFromAzureKeyVault(instance.DBPasswordKey);
        string connectionString = $"Data Source={instance.DBServerName};Initial Catalog={instance.DBName};User ID={instance.DBUserName};Password={dbPassword};MultipleActiveResultSets=true;Encrypt=True";
        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT * FROM [publish].[DASHt_CapacityPlanning_ShiftsCombined] " +
                        "WHERE [NewScenarioId] = @ScenarioId";
            var items = await connection.QueryAsync<DashtCapacityPlanningShiftsCombined>(query, new { ScenarioId = scenario.Id });
            scenario.DetailDashtCapacityPlanningShiftsCombined = items.ToList();
        }
    }
    public async Task GetMaterialsDetails(CompanyDb instance, Scenario scenario)
    {
        string dbPassword = GetValueFromAzureKeyVault(instance.DBPasswordKey);
        string connectionString = $"Data Source={instance.DBServerName};Initial Catalog={instance.DBName};User ID={instance.DBUserName};Password={dbPassword};MultipleActiveResultSets=true;Encrypt=True";

        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT * FROM [publish].[DASHt_Materials] " +
                        "WHERE [NewScenarioId] = @ScenarioId";
            var items = await connection.QueryAsync<Dasht_Materials>(query, new { ScenarioId = scenario.Id });
            scenario.DetailDasht_Materials = items.ToList();
        }
    }
    // TODO: This could be extracted to an extention class
    private T? GetValue<T>(IDataRecord record, string key)
    {
        var value = record[key];
        if (value == DBNull.Value)
        {
            return default;
        } 
        else
        {
            return (T)value;
        }
    }

    private string GetValueFromAzureKeyVault(string key)
    {
        string kvURL = _configuration["KeyValultUrl"];
        string tenentId = _configuration["TenantId"];
        string clientId = _configuration["ClientId"];
        string clientSecret = _configuration["ClientSecret"];

        var credential = new ClientSecretCredential(tenentId, clientId, clientSecret);

        var client = new SecretClient(new Uri(kvURL), credential);
        KeyVaultSecret secret = client.GetSecret(key);
        return secret.Value;
    }
}
