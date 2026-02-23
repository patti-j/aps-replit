using System.Threading.Tasks;
using System;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ExternalIntegrations
{
    public class ImportIntegrationExecutionPolicy
    {
        public record ImportIntegrationExecutionResult(bool a_useSeparateDatabases, string? a_importIntegrationUserAndPass);

        private readonly string _webAppDbConnectionString;

        public ImportIntegrationExecutionPolicy()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();

            _webAppDbConnectionString =
                config.GetConnectionString("WebAppDBConnectionString")?.Replace(@"\\", @"\")
                ?? throw new ArgumentNullException("WebAppDBConnectionString");
        }

        public async Task<ImportIntegrationExecutionResult> GetImportUserAndLogic(int a_companyId, int a_planningAreaId)
        {
            int? dataConnectorId = null;

            const string paQuery = @"SELECT DataConnectorId 
                                 FROM PlanningAreas 
                                 WHERE Id = @PlanningAreaId AND CompanyId = @CompanyId";

            using (SqlConnection conn = new SqlConnection(_webAppDbConnectionString))
            using (SqlCommand cmd = new SqlCommand(paQuery, conn))
            {
                cmd.Parameters.AddWithValue("@PlanningAreaId", a_planningAreaId);
                cmd.Parameters.AddWithValue("@CompanyId", a_companyId);

                await conn.OpenAsync();
                object result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                    dataConnectorId = Convert.ToInt32(result);
            }

            if (!dataConnectorId.HasValue)
                return new ImportIntegrationExecutionResult(false, null);

            string? importCreds = null;
            bool useSeparateDbs = false;

            const string dcQuery = @"SELECT ImportIntegrationUserAndPass, UseSeparateDatabases
                                 FROM DataConnectors
                                 WHERE Id = @DataConnectorId AND CompanyId = @CompanyId";

            using (SqlConnection conn = new SqlConnection(_webAppDbConnectionString))
            using (SqlCommand cmd = new SqlCommand(dcQuery, conn))
            {
                cmd.Parameters.AddWithValue("@DataConnectorId", dataConnectorId.Value);
                cmd.Parameters.AddWithValue("@CompanyId", a_companyId);

                await conn.OpenAsync();
                using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    importCreds = reader["ImportIntegrationUserAndPass"] as string;
                    if (reader["UseSeparateDatabases"] != DBNull.Value)
                        useSeparateDbs = Convert.ToBoolean(reader["UseSeparateDatabases"]);
                }
            }

            return new ImportIntegrationExecutionResult(useSeparateDbs, importCreds);
        }
    }

}