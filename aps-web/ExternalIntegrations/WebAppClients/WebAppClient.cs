using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

namespace ExternalIntegrations.WebAppClients
{
    public class WebAppDBDataConnectorProperties
    {
        public string IntegrationSettingsJSON { get; set; }
        public int? DataConnector { get; set; }        
    }

    public class ErpDatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string PublishSqlServerConnectionString { get; set; }
    }

    //public class WebAppDBroperties
    //{
    //    public string Username { get; set; }
    //    public string Password { get; set; }
    //    public string Endpoint { get; set; }
    //    public string APIToken { get; set; }
    //    public string DbConnectionString { get; set; }
    //}

    public class AhaProperties
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Endpoint { get; set; }
        public string APIToken { get; set; }
        public string DbConnectionString { get; set; }


    }

    public class NetsuiteCredentials
    {
        public string AccountId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TokenId { get; set; }
        public string TokenSecret { get; set; }
    }

    public class NetsuiteImportEndpoints
    {
        public List<string> URLs { get; set; }
    }

    public class NetsuitePublishEndpoint
    {
        public string URL { get; set; }
    }

    public class NetsuiteProperties
    {
        public NetsuiteCredentials Credentials { get; set; }
        public NetsuiteImportEndpoints ImportEndpoints { get; set; }
        public NetsuitePublishEndpoint PublishEndpoint { get; set; }

        public bool WorkOrders { get; set; }
        public bool BOMs { get; set; }
        public bool Routings { get; set; }
        public bool Items { get; set; }
        public bool PurchaseOrders { get; set; }
        public bool SalesOrders { get; set; }

        public string DbConnectionString { get; set; }     
    }

    public class WebAppClient
    {

        public async Task<TProps> WebAppProperties<TProps>(int a_companyId, int a_planningAreaId, string a_action)
        where TProps : class, new()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string webAppDBConnectionString = config.GetConnectionString("WebAppDBConnectionString")?.Replace(@"\\", @"\");
            
            WebAppDBDataConnectorProperties webAppDbResult = await GetWebAppDBProperties(webAppDBConnectionString, a_planningAreaId);            
            
            TProps properties = JsonConvert.DeserializeObject<TProps>(webAppDbResult.IntegrationSettingsJSON) ?? new TProps();           

            string dataConnectorSQLName = "";
            switch (a_action)
            {
                case "Import":
                    dataConnectorSQLName = "ImportConnectionString";
                    break;
                case "Publish":
                    dataConnectorSQLName = "PublishConnectionString";
                    break;
                default:
                    throw new ArgumentException("Unknown DB data connector lookup argument.");
                    break;
            }

            try
            {                
                string dbConnectionString = await GetDataConnectorConnectionString(
                webAppDBConnectionString, a_planningAreaId, dataConnectorSQLName);

                System.Reflection.PropertyInfo pi = typeof(TProps).GetProperty("DbConnectionString");
                if (pi != null && pi.CanWrite) pi.SetValue(properties, dbConnectionString);

                if (dbConnectionString == "")
                {
                    throw new Exception("No Db connection string found");
                }

            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }            

            return properties;
        }

        //public async Task<WebAppDBProperties> WebAppProperties(int a_companyId, int a_planningAreaId, string a_action)
        //{
        //    IConfigurationRoot config = new ConfigurationBuilder()
        //        .SetBasePath(Directory.GetCurrentDirectory())
        //        .AddJsonFile("appsettings.json")
        //        .Build();
        //    string webAppDBConnectionString = config.GetConnectionString("WebAppDBConnectionString");
        //    webAppDBConnectionString = webAppDBConnectionString.Replace(@"\\", @"\");

        //    WebAppDBProperties properties = new WebAppDBProperties();

        //    try
        //    {
        //        WebAppDBDataConnectorProperties webAppDbResult = await GetWebAppDBProperties(webAppDBConnectionString, a_planningAreaId, a_action);

        //        properties = JsonConvert.DeserializeObject<WebAppDBProperties>(webAppDbResult.IntegrationSettingsJSON);

        //        properties.DbConnectionString = await GetDataConnectorConnectionString(webAppDBConnectionString, a_planningAreaId, webAppDbResult.DataConnector);

        //        if (properties.DbConnectionString == "")
        //        {
        //            throw new Exception("No Db connection string found");
        //        }
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        Console.Error.WriteLine($"Error: {ex.Message}");
        //    }

        //    return properties;
        //}

        //TODO: Pass in Parameter that signals whether this is a Import or Publish
        public async Task<WebAppDBDataConnectorProperties> GetWebAppDBProperties(string a_connectionString, int a_planningAreaId)
        {
            WebAppDBDataConnectorProperties values = new WebAppDBDataConnectorProperties();

            try
            {
                if (a_connectionString == null)
                {
                    throw new ArgumentException("Connection string cannot be null.", nameof(a_connectionString));
                }                


                int? externalIntegrationId = null;

                string query = $"SELECT [ExternalIntegrationId] FROM [PlanningAreas] WHERE id = {a_planningAreaId}";
                using (SqlConnection connection = new SqlConnection(a_connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {                            

                            if (!reader.IsDBNull(reader.GetOrdinal("ExternalIntegrationId")))
                            {
                                externalIntegrationId = reader.GetInt32(reader.GetOrdinal("ExternalIntegrationId"));
                            }
                            else
                            {
                                externalIntegrationId = null; //No ExternalIntegration found
                            }
                        }
                    }
                }
                
                if (externalIntegrationId == null)
                {
                    throw (new ArgumentException($"No External Integration ID found for Planning area: {a_planningAreaId}")
                    {

                    });
                }

                query = $"SELECT SettingsJson FROM ExternalIntegrations WHERE id = {externalIntegrationId}";

                using (SqlConnection connection = new SqlConnection(a_connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            values.IntegrationSettingsJSON = reader["SettingsJson"].ToString();                            
                        }
                    }
                }
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine($"Argument error: {ex.Message}");
            }
            return values;
        }

        public async Task<string> GetDataConnectorConnectionString(string a_connectionString, int a_planningAreaId, string a_dataConnectorName)
        {
            string dbConnectionString = "";
            string? settingsJson = null;
            int? dataConnectorId = null;

            try
            {
                
                string paQuery = @"SELECT DataConnectorId, Settings FROM PlanningAreas WHERE Id = @PlanningAreaId";

                using (SqlConnection connection = new SqlConnection(a_connectionString))
                using (SqlCommand command = new SqlCommand(paQuery, connection))
                {
                    command.Parameters.AddWithValue("@PlanningAreaId", a_planningAreaId);
                    await connection.OpenAsync();

                    using SqlDataReader reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal("DataConnectorId")))
                            dataConnectorId = reader.GetInt32(reader.GetOrdinal("DataConnectorId"));

                        if (!reader.IsDBNull(reader.GetOrdinal("Settings")))
                            settingsJson = reader["Settings"].ToString();
                    }
                }
                
                if (dataConnectorId != null)
                {                    
                    if (a_dataConnectorName != "ImportConnectionString" &&
                        a_dataConnectorName != "PublishConnectionString")
                    {
                        throw new ArgumentException(
                            $"Invalid data connector column name: {a_dataConnectorName}",
                            nameof(a_dataConnectorName));
                    }

                    string dcQuery = $@"SELECT {a_dataConnectorName} FROM DataConnectors WHERE Id = @DataConnectorId";

                    using (SqlConnection connection = new SqlConnection(a_connectionString))
                    using (SqlCommand command = new SqlCommand(dcQuery, connection))
                    {
                        command.Parameters.AddWithValue("@DataConnectorId", dataConnectorId.Value);
                        await connection.OpenAsync();

                        object? result = await command.ExecuteScalarAsync();
                        dbConnectionString = result?.ToString() ?? "";
                    }

                    return dbConnectionString;
                }
                
                if (!string.IsNullOrWhiteSpace(settingsJson))
                {
                    ErpDatabaseSettings result = JsonConvert.DeserializeObject<ErpDatabaseSettings>(settingsJson);
                    if (result != null)
                    {              
                        dbConnectionString = result.ConnectionString ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }

            return dbConnectionString;
        }
    }
}
