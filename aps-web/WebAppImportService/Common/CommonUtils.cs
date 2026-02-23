using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net.NetworkInformation;

using System.Data.SqlClient;
//using Microsoft.Data.SqlClient;
using System.IO;
using Azure.Storage.Files.Shares;

using System.Security.Cryptography.X509Certificates;
//using Microsoft.Extensions.Configuration;

//using System.Runtime.InteropServices.JavaScript;
using OfficeOpenXml;
using OfficeOpenXml.Export.ToDataTable;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using WebAppImportService.Models;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using System.Collections;

namespace WebAppImportService.Common;
public static class CommonUtils
{
    public async static Task<string> ConvertExcelToTable(ImportMessage message)
    {
        await WebappNotificationService.SendImportStatusUpdate(message.FileName, message.Sender, 0, "Starting Import...");

        List<string> Errors = new List<string>();
        ShareFileClient file;
        try
        {
            // Get a reference to the file
            var storageConnectionString = Environment.GetEnvironmentVariable("AzureSMBStorageConnectionString");
            ShareClient share = new ShareClient(storageConnectionString, Environment.GetEnvironmentVariable("AzureStorage"));
            ShareDirectoryClient directory = share.GetDirectoryClient(Environment.GetEnvironmentVariable("AzureSMBStorageInDirectory"));
            file = directory.GetFileClient(message.FileName);

            //Note: A commercial license has been purchased. The license is not set directly in code for versions < 8.0. So the license key is not present in this project.
            //If upgrading to 8.0 or later, we will set the license like: ExcelPackage.License.SetCommercial("<Your License key>");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        } catch(Exception e)
        {
            Errors.Add($"Error acquiring file: {e.Message}");
            var errorString = string.Join(", ", Errors);
            await WebappNotificationService.SendImportStatusUpdate(message.FileName, message.Sender, 1, $"Error: {errorString}");
            return errorString;
        }

        using (var stream = file.OpenRead())
        using (var package = new ExcelPackage(stream))
        {
            string companyDbConnectionString;
            string connectionString;
            try
            {
                companyDbConnectionString = Environment.GetEnvironmentVariable("WebAppDBConnectionString");
                connectionString = GetImportDbConnectionString(companyDbConnectionString, message);
                if (string.IsNullOrEmpty(connectionString)) { throw new Exception("DB ConnectionString not found"); }
                //create log table 
                await WebappNotificationService.SendImportStatusUpdate(message.FileName, message.Sender, 0, "Updating Log Table...");

                if (!TableExists("_log_import", connectionString))
                {
                    string createLogTable = $"CREATE TABLE import._log_import (import_date DATETIME, file_name VARCHAR(50), transaction_id VARCHAR(50), import_user VARCHAR(50), trans_start DATETIME, trans_end DATETIME, import_details NVARCHAR(MAX));";
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandText = createLogTable;
                        command.ExecuteNonQuery();
                    }
                }
            } 
            catch (Exception e)
            {
                Errors.Add($"Error acquiring connection string: {e.Message}");
                var errorString = string.Join(", ", Errors);
                await WebappNotificationService.SendImportStatusUpdate(message.FileName, message.Sender, 1, $"Error: {errorString}");
                return errorString;
            }
            
            var importStartTime = DateTime.Now;
            //processing each worksheet of excel file
            string SheetName = string.Empty;
            string ColumnName;
            for (int i = 0; i < package.Workbook.Worksheets.Count; i++)
            {
                try
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[i]; // Assuming data is on the first worksheet
                    SheetName = worksheet.Name;
                    if (SheetName.ToLower() == "metadata") continue;
                    await WebappNotificationService.SendImportStatusUpdate(message.FileName, message.Sender, (double)i / package.Workbook.Worksheets.Count, $"Importing Table {SheetName}...");
                    // Extract table name from the sheet name
                    string tableName = worksheet.Name;
                    tableName = Regex.Replace(tableName, "[^a-zA-Z0-9]", "");
                    // Extract column names and data types from Excel file
                    //Somtimes the coloum names have some null value and the End.Coloumn does not return correct value.. checking here for correct column length
                    int EndColumn = worksheet.Dimension.End.Column;
                    for (int col = worksheet.Dimension.End.Column; col >= 1; col--)
                    {
                        if (worksheet.Cells[1, col].Value == null ||
                            worksheet.Cells[1, col].Value.ToString() == string.Empty ||
                            worksheet.Cells[1, col].Value.ToString() == "" ||
                            worksheet.Cells[1, col].Value.ToString() == null)
                            continue;
                        else
                        {
                            EndColumn = col;
                            break;
                        }
                    }

                    DataColumn[] columns = new DataColumn[EndColumn];
                    for (int col = worksheet.Dimension.Start.Column; col <= EndColumn; col++)
                    {
                        string columnName = worksheet.Cells[1, col].Value.ToString();
                        columnName = Regex.Replace(columnName, "[^a-zA-Z0-9]", "");
                        string dataType = worksheet.Cells[2, col].Value?.ToString();
                        columns[col - 1] = new DataColumn(columnName, GetSqlDbType(dataType));
                    }

                    // Create table in database
                    using (var connection = new SqlConnection(connectionString))
                    {
                        var startTime = DateTime.Now;
                        connection.Open();
                        var command = connection.CreateCommand();

                        //drop table if exists
                        if (TableExists(tableName, connectionString))
                        {
                            string dropTableSQL = $"DROP TABLE import.{tableName}";
                            command.CommandText = dropTableSQL;
                            command.ExecuteNonQuery();
                        }

                        // Generate CREATE TABLE SQL statement
                        string createTableSql = GenerateCreateTableSql(tableName, columns);
                        command.CommandText = createTableSql;
                        command.ExecuteNonQuery();

                        // Insert data into table
                        using (var bulkCopy = new SqlBulkCopy(connection))
                        {
                            bulkCopy.DestinationTableName = "import." + tableName;
                            for (int col = 0; col < columns.Length; col++)
                            {
                                bulkCopy.ColumnMappings.Add(col, columns[col].ColumnName);
                            }

                            int rowStart = worksheet.Dimension.Start.Row;
                            int rowEnd = worksheet.Dimension.End.Row;
                            int colStart = worksheet.Dimension.Start.Column;
                            int colEnd = EndColumn;
                            var option = ToDataTableOptions.Create();
                            option.FirstRowIsColumnNames = true;
                            option.AlwaysAllowNull = true;
                            //option.SkipNumberOfRowsStart = 1;
                            bulkCopy.WriteToServer(worksheet.Cells[rowStart, colStart, rowEnd, colEnd]
                                .ToDataTable(option));
                        }
                        //write to log table
                    }
                }
                catch (Exception ex)
                {
                    Errors.Add(ex.Message + " SheetName: "+ SheetName);
                }
            }

            var importEndTime = DateTime.Now;

            var details = Errors.Any() ? string.Join(";\n", Errors) : "success";
            try
            {
                if (details == "success")
                {
                    await WebappNotificationService.SendImportStatusUpdate(message.FileName, message.Sender, 1, "Import Complete");
                } else
                {
                    await WebappNotificationService.SendImportStatusUpdate(message.FileName, message.Sender, 1, "Error: " + details);
                }


                //inserting to log
                string insetToLogTable = $"INSERT INTO import._log_import (import_date , file_name , transaction_id , import_user , trans_start , trans_end , import_details )" +
                                         $"VALUES ('{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}','{message.OriginalFileName}','{Path.GetFileNameWithoutExtension(message.FileName)}','{message.Sender}','{importStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}','{importEndTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}','{details}' );";
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = insetToLogTable;
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Errors.Add(e.Message);
            }
        }

        if (Errors.Count > 0)
            return string.Join(", ", Errors);
        
        return null;
    }

    private static string GetImportDbConnectionString(string companyDbConnectionString, ImportMessage message)
    {
        string dBServerName = string.Empty;
        string dBName = string.Empty;
        string dBUserName = string.Empty;
        string dBPasswordKey = string.Empty;
        string dbConnectionString = string.Empty;
        string sql = $"SELECT DBServerName, DBName, DBUserName, DBPasswordKey FROM dbo.CompanyDbs WHERE CompanyId = '{message.CompanyId}' AND Environment = '{message.Environment}' AND DbType = 0" +
                     $" and Name = '{message.InstanceName}'";
        string connecstrionstringKey = string.Empty;
        try
        {
            using (SqlConnection connection = new SqlConnection(companyDbConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                dBServerName = reader.GetString(0);
                                dBName = reader.GetString(1);
                                dBUserName = reader.GetString(2);
                                dBPasswordKey = reader.GetString(3);
                            }
                        }
                    }
                }

                string dbPassword = CommonUtils.GetValueFromAzureKeyVault(dBPasswordKey);
                dbConnectionString =
                    $"Data Source={dBServerName};Initial Catalog={dBName};User ID={dBUserName};Password={dbPassword};encrypt=false";
            }

            return dbConnectionString;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    private static Type GetSqlDbType(string dataType)
    {
        if (dataType == null) return typeof(string);
        if (int.TryParse(dataType, out int intResult)) return typeof(int);
        if (decimal.TryParse(dataType, out decimal decimalResult)) return typeof(decimal);
        if (DateTime.TryParse(dataType, out DateTime dateTimeResult)) return typeof(DateTime);
        return typeof(string);
    }

    private static string GenerateCreateTableSql(string tableName, DataColumn[] columns)
    {
        string sql = $"CREATE TABLE import.{tableName} (\n";
        foreach (var column in columns)
        {
            sql += $"    {column.ColumnName} {GetSqlTypeString(column.DataType)},\n";
        }
        sql = sql.TrimEnd(',', '\n'); // Remove trailing comma and newline
        sql += "\n);";
        return sql;
    }

    private static string GetSqlTypeString(Type type)
    {
        if (type == typeof(int))
            return "INT";
        else if (type == typeof(string))
            return "NVARCHAR(MAX)";
        else if (type == typeof(bool))
            return "BIT";
        else if (type == typeof(float))
            return "FLOAT";
        else if (type == typeof(long))
            return "LONG";
        else if (type == typeof(DateTime))
            return "DATETIME";
        else if (type == typeof(decimal))
            return "DECIMAL";
        // Add more mappings as needed
        else
            throw new NotSupportedException($"Data type '{type}' is not supported.");
    }
    public static bool TableExists(string tableName, string connectionString)
    {
        bool tableExists = false;

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Query to check if the table exists
            string sql = "IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'import' AND  TABLE_NAME = @TableName) SELECT 1 ELSE SELECT 0";

            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@TableName", tableName);

                // Execute the query
                object result = command.ExecuteScalar();

                // Check if the result is not null and is equal to 1
                if (result != null && Convert.ToInt32(result) == 1)
                {
                    tableExists = true;
                }
            }
        }

        return tableExists;
    }

    internal static async Task PublishMessageToQueue(IQueueMessage myQueueMessage, string queueName)
    {
        var fileShareConnectionString = Environment.GetEnvironmentVariable("AzureSMBStorageConnectionString");
        QueueManager queueManager = new QueueManager(fileShareConnectionString, queueName);
        
        //if(myQueueMessage.IsSuccess)
        //    myQueueMessage.Message = $"Import Complete: FileName: {myQueueMessage.FileName}, User: {myQueueMessage.Sender}, Date: {DateTime.Now}";

        await queueManager.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(myQueueMessage))));
    }

    public static string GetValueFromAzureKeyVault(string key)
    {
        string kvURL = Environment.GetEnvironmentVariable("KeyVaultUrl");
        string tenentId = Environment.GetEnvironmentVariable("TenantId");
        string clientId = Environment.GetEnvironmentVariable("ClientId");
        string clientSecret = Environment.GetEnvironmentVariable("ClientSecret");

        var credential = new ClientSecretCredential(tenentId, clientId, clientSecret);

        var client = new SecretClient(new Uri(kvURL), credential);
        KeyVaultSecret secret = client.GetSecret(key);
        return secret.Value;

    }
	internal static async Task TriggerImport(TriggerImportMessage myQueueItem, ILogger log)
	{
		try
		{
			string ptUserName = string.Empty;
			string ptPassword = string.Empty;
			string ServerManagerUrl = String.Empty;
			string InstanceName = myQueueItem.InstanceName;
			bool valuesFound =
				GetTriggerImportDetailsFromDb(out ptUserName, out ptPassword, out ServerManagerUrl, myQueueItem);

			if (!valuesFound)
			{
				myQueueItem.IsSuccess = false;
				myQueueItem.Message =
					$"Trigger Import failed. Could not connect to DB";
				await PublishMessageToQueue(myQueueItem, Environment.GetEnvironmentVariable("NotificationQueueName"));
				log.LogInformation("Trigger Import failed. Could not connect to DB");
				return;
			}

			var InstanceCollection = Instance
				.CallServerManagerToGetInstance(ServerManagerUrl + "/api/public/getInstancesWithUser", ptUserName,
					ptPassword).Result;
            if (InstanceCollection == null) throw new Exception("Cannot get instance collection from ServerManager");
			var instance = InstanceCollection.Where(x => x.InstanceName.ToLower() == InstanceName.Trim().ToLower()).FirstOrDefault();
            if (instance == null) throw new Exception(InstanceName + " instance not found");
			var ptAuthToken = await Login(ptUserName, ptPassword, instance.SystemServiceUrl);

			GetScenarioResponse getScenarioResponse =
				await Instance.GetScenarios(ptUserName, ptPassword, ptAuthToken, instance.SystemServiceUrl);

			foreach (var scenario in
			         getScenarioResponse.Confirmations) // or just select one in particular, by name maybe?
			{
				var importResponse = await Instance.ImportScenario(ptUserName, ptPassword, ptAuthToken, scenario,
					instance.SystemServiceUrl);
				if (importResponse?.Exception ?? false)
				{
					string originalExceptionString = importResponse.FullExceptionText;
					string truncatedString = originalExceptionString.Length > 100
						? originalExceptionString.Substring(0, 100)
						: originalExceptionString;
					string encodedString = Uri.EscapeDataString(truncatedString);
					myQueueItem.IsSuccess = false;
					myQueueItem.Message =
						$"Import failed for scenario {scenario.ScenarioName}:ResponseCode={importResponse.ResponseCode} - {encodedString}";
					await PublishMessageToQueue(myQueueItem,
						Environment.GetEnvironmentVariable("NotificationQueueName"));
					log.LogInformation("Notification published to notification queue");
					//Console.WriteLine($"Import failed for scenario {scenario.ScenarioName}: {importResponse.FullExceptionText}");
				}
				else
				{
					myQueueItem.IsSuccess = true;
					myQueueItem.Message =
						$"Import successful for scenario {scenario.ScenarioName}:ResponseCode={importResponse.ResponseCode}";
					await PublishMessageToQueue(myQueueItem,
						Environment.GetEnvironmentVariable("NotificationQueueName"));
					//Console.WriteLine($"Import succeeded for scenario {scenario.ScenarioName}: {importResponse.ResponseCode}");
				}
			}
		}
		catch (Exception ex)
		{
			myQueueItem.IsSuccess = false;
			myQueueItem.Message =
				$"Trigger Import failed. Exception happened during Trigger import. " +ex.Message;
			await PublishMessageToQueue(myQueueItem, Environment.GetEnvironmentVariable("NotificationQueueName"));
			log.LogInformation("Trigger Import failed. Exception: " +ex.Message);
			return;
		}
	}

	private static bool GetTriggerImportDetailsFromDb(out string ptUserName, out string ptPassword, out string serverManagerUrl, TriggerImportMessage myQueueItem)
	{
		string query = $"SELECT ImportUserName, ImportUserPasswordKey, ServerManagerUrl FROM CompanyDbs where Name = '{myQueueItem.InstanceName}' and DbType = '0' and CompanyId = '{myQueueItem.CompanyId}' and Environment = '{myQueueItem.Environment}';";
		using (SqlConnection connection =
		       new SqlConnection(Environment.GetEnvironmentVariable("WebAppDBConnectionString")))
		{
			connection.Open();

			using (SqlCommand command = new SqlCommand(query, connection))
			{
				SqlDataReader reader = command.ExecuteReader();

				if (reader.HasRows)
				{
					while (reader.Read())
					{
						// Store result values in variables
						ptUserName = reader.GetString(0);
						ptPassword = GetValueFromAzureKeyVault(reader.GetString(1));
						serverManagerUrl = reader.GetString(2);
						return true;
					}
				}
			}
		}
		ptUserName = string.Empty;
        ptPassword = string.Empty;
        serverManagerUrl = string.Empty;
		return false;
	}

	static async Task<string> Login(string ptUserName, string ptPassword, string url)
	{
		// Handshake
		byte[] passwordHash = null;
		using (EncryptionHandshake handshake = new EncryptionHandshake())
		{
			string publicKey = handshake.GetEncryptionKey();
			byte[] symmetricKey;
			byte[] encryptedData = await Instance.CallHandshakeAPI(publicKey, url);
			symmetricKey = handshake.Decrypt(encryptedData);
			passwordHash = StringHasher.Hash(ptPassword, symmetricKey);
		}

		// Login/ get Session Auth token
		BasicLoginRequest request = new BasicLoginRequest
		{
			UserName = ptUserName,
			PasswordHash = passwordHash
		};
		UserLoginResponse loginResponse = await Instance.PostLogin(request, url);
		string sessionToken = loginResponse.SessionToken; // need this to authorize subsequent requests
		return sessionToken;
	}
}
