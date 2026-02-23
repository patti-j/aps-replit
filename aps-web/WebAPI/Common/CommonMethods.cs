using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;

using WebAPI.DAL;
using WebAPI.Models;
using WebAPI.Models.Integration;

namespace WebAPI.Common
{
	public class CommonMethods
	{
        internal static string GetRecordsAsJson(StringValues tableName, StringValues queryString, StringValues limit, StringValues offset, StringValues orderBy, string connectionString)
		{
			//if limit, offset, querystring header values are not set, we set here default values
            int rowsToReturn = 1000;
            if (limit.Count == 0) rowsToReturn = 1000;
            else if (int.TryParse(limit.ToString(), out int result))
				rowsToReturn = result;
            
			if (offset.Count == 0) offset = "0";
			if (queryString.Count !=0) queryString = "where "+ queryString;
            string orderByColumnName = string.Empty;
			if(orderBy.Count == 0 || string.IsNullOrEmpty(orderBy.ToString())) orderByColumnName = "1";
			else orderByColumnName = orderBy.ToString();

			string sqlQuery = $"select * from {tableName} {queryString} order by {orderByColumnName} offset {offset} rows fetch next {rowsToReturn} rows only";
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();

				using (var command = new SqlCommand(sqlQuery, connection))
				{
					// Create a data adapter to execute the query and fill a dataset
					using (var adapter = new SqlDataAdapter(command))
					{
						var dataSet = new DataSet();
						adapter.Fill(dataSet);

						// Convert the first table in the dataset to JSON
						if (dataSet.Tables.Count > 0)
						{
							DataTable dataTable = dataSet.Tables[0];
							string json = DataTableToJson(dataTable, rowsToReturn);
							return json;
						}
					}
				}
			}
			return null;
		}
        internal static string GetRecordsAsJsonV2(StringValues tableName, StringValues queryString, StringValues limit, StringValues offset, StringValues orderBy, string connectionString)
        {
            //if limit, offset, querystring header values are not set, we set here default values
            int rowsToReturn = 1000;
            if (limit.Count == 0) rowsToReturn = 1000;
            else if (int.TryParse(limit.ToString(), out int result))
                rowsToReturn = result;

            if (offset.Count == 0) offset = "0";
            if (queryString.Count !=0) queryString = "where "+ queryString;
            string orderByColumnName = string.Empty;
            if (orderBy.Count == 0 || string.IsNullOrEmpty(orderBy.ToString())) orderByColumnName = "1";
            else orderByColumnName = orderBy.ToString();

            string sqlQuery = $"select * from {tableName} {queryString} order by {orderByColumnName} offset {offset} rows fetch next {rowsToReturn} rows only";
			string sqlQueryForTableRowCount = $"select count(*) from {tableName}";
			int tableRowCount = 0 ;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQueryForTableRowCount, connection))
                {
					tableRowCount = (int)command.ExecuteScalar();
                }
                using (var command = new SqlCommand(sqlQuery, connection))
                {
                    // Create a data adapter to execute the query and fill a dataset
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var dataSet = new DataSet();
                        adapter.Fill(dataSet);

                        // Convert the first table in the dataset to JSON
                        if (dataSet.Tables.Count > 0)
                        {
                            DataTable dataTable = dataSet.Tables[0];
                            string json = DataTableToJsonV2(dataTable, rowsToReturn, tableRowCount);
                            return json;
                        }
                    }
                }
            }
            return null;
        }
        internal static string DataTableToJson(DataTable table, int limitCount)
        {
            List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
            int rowCount = 1000;
			if(limitCount < 1000) rowCount = limitCount;
            if (limitCount > table.Rows.Count)
				rowCount = table.Rows.Count;
			else rowCount = limitCount;

            for (int i = 0; i < rowCount; i++)
            {
                var dataRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    dataRow[col.ColumnName] = table.Rows[i][col];
                }
                dataList.Add(dataRow);
            }

            return JsonConvert.SerializeObject(dataList, Formatting.Indented);
        }

        internal static string DataTableToJsonV2(DataTable table, int limitCount, int tableRowCount)
        {
            List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
            int rowCount = 1000;
            if (limitCount < 1000) rowCount = limitCount;
            if (limitCount > table.Rows.Count)
                rowCount = table.Rows.Count;
            else rowCount = limitCount;

            for (int i = 0; i < rowCount; i++)
            {
                var dataRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    dataRow[col.ColumnName] = table.Rows[i][col];
                }

                dataList.Add(dataRow);
            }

            var result = JsonConvert.SerializeObject(new { TotalRowCount = tableRowCount , Result = dataList }, Formatting.Indented);
            return result;
        }
        internal static bool UpdateTable(string requestBody, StringValues tableName, StringValues queryString, string connectionString, out string m_errorString)
		{
			var jsonObjects = JsonConvert.DeserializeObject<List<dynamic>>(requestBody);
			m_errorString = string.Empty;
			string updateTableSql = string.Empty;
			string setCommand = " set ";
			if (jsonObjects == null || jsonObjects.Count == 0)
			{
				m_errorString = "Json body not found in the request";
				return false;
			}
			dynamic jsonObject = jsonObjects[0];
			foreach (var property in jsonObject.Properties())
			{
				string columnName = property.Name;
				string columnValue = property.Value.ToString();
				setCommand = setCommand + columnName + "= '" + columnValue + "',";
			}

			setCommand = setCommand.Substring(0, setCommand.Length - 1);
			string whereCondition = " where " + queryString;

			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					//drop table if exists
					updateTableSql = $"Update {tableName} {setCommand} {whereCondition}";
					SqlCommand createTableCommand = new SqlCommand(updateTableSql, connection);
					createTableCommand.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				m_errorString = ex.Message + " Query:"+ updateTableSql;
				return false;
			}

			return true;
		}

        internal static bool ValidateBasicApiRequest(out string m_errorString, IHeaderDictionary requestHeaders, CompanyDBService a_companyDbService)
        {
            bool gotAllRequiredHeaderValues = true;
            StringValues CompanyId = string.Empty;
            StringValues CompanyAPIKey = string.Empty;
            string errorString = string.Empty;

            if (!requestHeaders.TryGetValue("CompanyId", value: out CompanyId))
            {
                errorString = "CompanyId missing in header.";
                gotAllRequiredHeaderValues = false;
            }

            if (!requestHeaders.TryGetValue("CompanyAPIKey", out CompanyAPIKey))
            {
                errorString = errorString + " CompanyAPIKey missing in header.";
                gotAllRequiredHeaderValues = false;
            }
            else if (int.TryParse(CompanyId, out int companyId))
            {
                if (!a_companyDbService.VerifyCompanyApiKey(companyId, CompanyAPIKey))
                {
                    errorString = errorString + "Company ApiKey does not match";
					gotAllRequiredHeaderValues = false;
                }
            }
            else
            {
                errorString = errorString + " CompanyId is not an integer.";
                gotAllRequiredHeaderValues = false;
            }
           
            if (gotAllRequiredHeaderValues)
            {
                m_errorString = "Success";
                return true;
            }
            else
            {
                m_errorString = errorString;
                return false;
            }
        }

        internal static bool ValidateIntegrationApiRequest(out string m_errorString, IHeaderDictionary requestHeaders, CompanyDBService companyDbService)
		{
			bool gotAllRequiredHeaderValues = true;
			//StringValues CompanyId = string.Empty;
			//StringValues CompanyAPIKey = string.Empty;
			StringValues InstanceName = string.Empty;
			StringValues TableName = string.Empty;
			string errorString;

            if (!ValidateBasicApiRequest(out errorString, requestHeaders, companyDbService))
            {
                m_errorString = errorString;
                return false;
            }
            if (!requestHeaders.TryGetValue("InstanceName", out InstanceName))
            {
                errorString = errorString + " InstanceName missing in header.";
                gotAllRequiredHeaderValues = false;
            }
            if (!requestHeaders.TryGetValue("TableName", out TableName))
			{
				errorString = errorString + " TableName missing in header.";
				gotAllRequiredHeaderValues = false;
			}

			if (gotAllRequiredHeaderValues)
			{
				m_errorString = "Success";
				return true;
			}
			else
			{
				m_errorString = errorString;
				return false;
			}
		}

		public static ConcurrentDictionary<string, string> ApiKeyToPAId = new ();
        
		/// <summary>
		/// Validate API calls made from the context of one particular instance, using its InstanceIdentifier
		/// TODO: Before release, we'd like this to include a second "Password" component, so that
		/// TODO: the password value can change while maintaining identity
		/// </summary>
		/// <param name="requestHeaders"></param>
		/// <param name="instanceService"></param>
		/// <param name="o_instanceCompany"></param>
		/// <param name="o_instanceId"></param>
		/// <returns></returns>
		public static bool ValidateInstanceApiRequest(IHeaderDictionary requestHeaders, CompanyDBService instanceService, out Company o_instanceCompany, out string o_instanceId)

        {
            o_instanceCompany = null;
            o_instanceId = null;
            StringValues InstanceId = string.Empty;

			if (!requestHeaders.TryGetValue("InstanceTokenScheme", value: out InstanceId))
			{
				return false;
            }

			// \/ UNCOMMENT WHEN READY TO ENFORCE REQUIRING AN API KEY \/ TODO: API KEYS

			//if (!requestHeaders.TryGetValue("PAApiKey", out StringValues apiKey))
			//{
			//	return false;
			//}

			//string Key = HttpUtility.UrlDecode(apiKey.ToString());
			//if (!ValidatePaApiKey(Key))
			//{
			//	Task.Delay(7).Wait(); //fake processing
			//	return false;
			//}

			//if (!ApiKeyToPAId.ContainsKey(Key))
			//{
			//	PADetails? pa = instanceService.GetPAWithApiKey(Key);
			//	if (pa == null)
			//	{
			//		return false;
			//	}
			//	ApiKeyToPAId.TryAdd(Key, pa.PlanningAreaKey);
			//}
			//else
			//{
			//	ApiKeyToPAId.TryGetValue(Key, out string paKey);
			//	if (InstanceId.ToString() != paKey)
			//	{
			//		return false;
			//	}
			//}

			o_instanceId = InstanceId.ToString();
			//PADetails planningArea = instanceService.GetPlanningAreaWithCompany(o_instanceId);
			//if (planningArea == null || planningArea.ApiKey != Key)
			//{
			//	return false;
			//}

			//return planningArea.Company != null;

			o_instanceCompany = instanceService.GetCompanyForInstanceId(InstanceId).Result;
			
			return o_instanceCompany != null;
		}

		public static void UpdatePaApiKey(UpdatePaApiKeyEvent paEvent)
		{
			ApiKeyToPAId.AddOrUpdate(paEvent.ApiKey, (key =>
			{
				return key;
			}), ((key, old) =>
			{
				return key;
			}));
		}
		
		public static bool ValidatePaApiKey(string a_apiKey)
		{
			byte[] bytes = Convert.FromBase64String(a_apiKey);
			byte xorByte = 0;
			byte addByte = 0;
            
			for (int i = 0; i < 12; i++)
			{
				xorByte ^= bytes[i];
				addByte += bytes[i];
			}

			byte op = (byte)(xorByte ^ addByte);
			UInt16 final = 0;
            
			for (int i = 0; i < 12; i++)
			{
				final ^= (ushort)((((ushort)bytes[i] * op) + op) ^ ushort.MaxValue);
			}

			if (bytes[12] == xorByte &&
			    bytes[13] == addByte &&
			    bytes[14] == (byte)(final >> 8) &&
			    bytes[15] == (byte)(final & 0xff))
			{
				return true;
			}
			return false;
		}

        /// <summary>
        /// Validate API calls made from the context of one particular instance, using its InstanceIdentifier
        /// TODO: Before release, we'd like this to include a second "Password" component, so that
        /// TODO: the password value can change while maintaining identity
        /// </summary>
        /// <param name="requestHeaders"></param>
        /// <param name="instanceService"></param>
        /// <param name="o_instanceCompany"></param>
        /// <param name="o_instanceId"></param>
        /// <returns></returns>
        public static bool ValidateInstancePublicApiRequest(IHeaderDictionary requestHeaders, CompanyDBService instanceService, out PADetails? o_pa)
        {
            o_pa = null;
            StringValues apiKey = string.Empty;
            StringValues paKey = string.Empty;

            if (!requestHeaders.TryGetValue("ApiKey", value: out apiKey))
            {
                return false;
            }
            if (!requestHeaders.TryGetValue("PlanningAreaKey", value: out paKey))
            {
                return false;
            }

            var pa = instanceService.GetPlanningArea(paKey);

            if (pa == null || pa.ApiKey != apiKey)
            {
                return false;
            }

            o_pa = pa;
            return true;
        }

        /// <summary>
        /// Validate API calls made from the context of the Server Manager. These calls have elevated ability to manage multiple instances on that server.
        /// </summary>
        /// <param name="requestHeaders"></param>
        /// <param name="dbService"></param>
        /// <param name="o_serverCompany">The company that manages the never (not the company/companies that use servers on it)</param>
        /// <param name="o_server"></param>
        /// <returns></returns>
        public static bool ValidateServerApiRequest(IHeaderDictionary requestHeaders, CompanyDBService dbService, out Company o_serverCompany, out CompanyServer o_server)
        {
            o_serverCompany = null;
            o_server = null;
            StringValues serverAuthToken = string.Empty;

            if (!requestHeaders.TryGetValue("ServerTokenScheme", value: out serverAuthToken))
            {
                return false;
            }

            CompanyServer server = dbService.GetServer(serverAuthToken).Result;
            if (server == null)
            {
                return false;
            }

            if (server.ManagingCompany == null)
            {
                return false;
            }

            o_serverCompany = server.ManagingCompany;
            o_server = server;

            return true;
        }

        public static string GetCompanyDBConnectionString(string CompanyId, CompanyDBService companyDbService, string CompanyAPIKey, string InstanceName)
		{
			var key = companyDbService.GetCompanyKey(CompanyId);
			if (key == string.Empty) throw new Exception("Company API key not found");
			if (key != null && key != CompanyAPIKey) throw new Exception("Company API key does not match");
			string connectionStringError = string.Empty;
			var connectionString = companyDbService.GetConnectionString(InstanceName, CompanyId, out connectionStringError);
			if (connectionString == string.Empty || connectionString == "" || connectionString == null)
				throw new Exception("ConnectionString not found for instance " + InstanceName + " " +
				                    connectionStringError);
			return connectionString;
		}

		public static bool CreateCustomTable(string requestBody, string tableName, out string m_errorString, string connectionString)
		{
			DataTable dataTable = new DataTable();
			var jsonObjects = JsonConvert.DeserializeObject<List<dynamic>>(requestBody);
			// Add columns to the DataTable based on the JSON object
			dynamic jsonObject = jsonObjects[0] ?? null;
			foreach (var property in jsonObject.Properties())
			{
				string columnName = property.Name;
				string columnType = property.Value.ToString();

				Type dataType = GetTypeFromColumnType(columnType);
				dataTable.Columns.Add(columnName, dataType);
			}
			// Create a table in the database
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				//drop table if exists
				var command = connection.CreateCommand();
				if (TableExists(tableName, connectionString))
				{
					string dropTableSQL = $"DROP TABLE {tableName}";
					command.CommandText = dropTableSQL;
					command.ExecuteNonQuery();
				}

				string createTableSql = $"CREATE TABLE {tableName} (";
				foreach (DataColumn column in dataTable.Columns)
				{
					createTableSql += $"{column.ColumnName} {GetSqlTypeFromType(column.DataType)}, ";
				}
				createTableSql = createTableSql.TrimEnd(',', ' ') + ")";
				SqlCommand createTableCommand = new SqlCommand(createTableSql, connection);
				createTableCommand.ExecuteNonQuery();
			}
			m_errorString = "";
			return true;
		}

		public static bool ImportToCustomTable(string requestBody, string tableName, out string m_errorString, string connectionString)
		{
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var jsonObjects = JsonConvert.DeserializeObject<List<dynamic>>(requestBody);
                DataTable dataTable = GetTableSchema(connection, tableName);
                using (var bulkCopy = new SqlBulkCopy(connectionString))
                {
                    for (int i = 0; i < jsonObjects.Count; i++)
                    {
                        DataRow row = dataTable.NewRow();
                        dynamic obj = jsonObjects[i] ?? null;
                        if (obj != null)
                        {
                            foreach (var property in obj.Properties())
                            {
                                string columnName = property.Name;
                                object value = property.Value.ToString();
                                row[columnName] = value;
                            }
                            dataTable.Rows.Add(row);
                        }
                    }
                    bulkCopy.DestinationTableName = tableName;
                    bulkCopy.WriteToServer(dataTable);
                }
            }
			m_errorString = "";
			return true;
		}
        public static DataTable GetTableSchema(SqlConnection connection, string tableName)
        {
            // Create an empty DataTable to hold the schema
            DataTable schemaTable = new DataTable();

            // Use a SqlDataAdapter to fill the schema from the SQL Server table
            string query = $"SELECT * FROM {tableName} WHERE 1 = 0"; // This will return an empty result set, but with the schema
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
            {
                adapter.FillSchema(schemaTable, SchemaType.Source);
            }

            return schemaTable;
        }
        public static bool TableExists(string tableName, string connectionString)
		{
            bool tableExists = false;
            //tablename wil be in the format [schema].[tablename] So we need to split this.

            // Define a regular expression to match content inside square brackets
            string pattern = @"\[(.*?)\]";
            MatchCollection matches = Regex.Matches(tableName, pattern);

            // Create an array to store the results
            string[] splitValue = new string[matches.Count];
            // Loop through each match and extract the text
            for (int i = 0; i < matches.Count; i++)
            {
                splitValue[i] = matches[i].Groups[1].Value;  // Groups[1] contains the captured text inside brackets
            }

            using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();

				// Query to check if the table exists
				string sql = "IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tablename AND TABLE_SCHEMA = @schema) SELECT 1 ELSE SELECT 0";

				using (var command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@tablename", splitValue[1]);
					command.Parameters.AddWithValue("@schema", splitValue[0]);

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
		private static Type GetTypeFromColumnType(string columnType)
		{
			switch (columnType.ToLower())
			{
				case "int":
					return typeof(int);
				case "varchar":
					return typeof(string);
				case "bool":
					return typeof(bool);
				case "float":
					return typeof(float);
				case "long":
					return typeof(long);
				case "datetime":
					return typeof(DateTime);
				case "string":
					return typeof(string);
				case "double":
					return typeof(float);
				case "decimal":
					return typeof(decimal);
				// Add other types as needed
				default:
					throw new ArgumentException($"Unsupported column type: {columnType}");
			}
		}
		private static string GetSqlTypeFromType(Type type)
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
				return "BIGINT";
			else if (type == typeof(DateTime))
				return "DATETIME";
			else if (type == typeof(decimal))
				return "DECIMAL";
			// Add more mappings as needed
			else
				throw new NotSupportedException($"Data type '{type}' is not supported.");
		}

		internal static bool DeleteAllTableData(StringValues tableName, out string m_errorString, string connectionString)
		{
			m_errorString = string.Empty;
			bool success = false;
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				//drop table if exists
				var command = connection.CreateCommand();
				if (TableExists(tableName, connectionString))
				{
					string deleteTableSQL = $"DELETE FROM {tableName}";
					command.CommandText = deleteTableSQL;
					command.ExecuteNonQuery();
					success = true;
				}
				else
				{
					m_errorString = "Table not found";
					success = false; ;
				}
			}
			return success;
		}

		internal static bool DeleteTableData(string requestBody, StringValues tableName, out string m_errorString, string connectionString)
		{
			DataTable dataTable = new DataTable();
			var jsonObjects = JsonConvert.DeserializeObject<List<dynamic>>(requestBody);
			dynamic jsonObject = jsonObjects[0] ?? null;
			m_errorString = string.Empty;

			for (int i = 0; i < jsonObjects.Count; i++)
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					dynamic obj = jsonObjects[i] ?? null;
					string whereCondition = " where ";
					if (obj != null)
					{
						foreach (var property in obj.Properties())
						{
							string columnName = property.Name;
							object value = property.Value.ToString();
							whereCondition = whereCondition + columnName +" = '" + value +"' and ";
						}

						whereCondition = whereCondition.Substring(0, whereCondition.Length - 4);
						string deleteQuery = "DELETE FROM " + tableName + whereCondition;
						using (SqlCommand command = new SqlCommand(deleteQuery, connection))
						{
							int rowsAffected = command.ExecuteNonQuery();
							if (rowsAffected == 0) m_errorString = m_errorString + "No results returned for query: " +whereCondition;
						}
					}

				}
			}

			return m_errorString == string.Empty;
		}

		public static string GetValueFromAzureKeyVault(string key, out string keyvalutError)
		{
			try
			{
				string c = Directory.GetCurrentDirectory();
				IConfigurationRoot configuration =
					new ConfigurationBuilder().SetBasePath(c).AddJsonFile("appsettings.json").Build();
				//string kvURL = configuration["KeyValultUrl"];
				string kvURL = configuration["KeyVaultUrl"];
				string tenentId = configuration["TenantId"];
				string clientId = configuration["ClientId"];
				string clientSecret = configuration["ClientSecret"];

				var credential = new ClientSecretCredential(tenentId, clientId, clientSecret);

				var client = new SecretClient(new Uri(kvURL), credential);
				KeyVaultSecret secret = client.GetSecret(key);
				keyvalutError = string.Empty;
				return secret.Value;
			}
			catch (Exception ex)
			{
				keyvalutError = ex.Message;
				return string.Empty;
			}
		}

		internal static async Task<bool> TriggerImport(HttpRequest request, CompanyDBService companyDbService)
		{
			bool gotAllRequiredHeaderValues = true;
			bool importSuccess = false;
			string errorString = string.Empty;
			string ptUserName = string.Empty;
			string ptPassword = string.Empty;
			string ServerManagerUrl = String.Empty;
			StringValues InstanceName = request.Headers["InstanceName"];
			StringValues CompanyId = request.Headers["CompanyId"];
			StringValues ScenarioName = string.Empty;
            try
            {
                if (!request.Headers.TryGetValue("ScenarioName", value: out ScenarioName))
                {
                    errorString = "ScenarioName not found in header";
                    gotAllRequiredHeaderValues = false;
                }

                if (!gotAllRequiredHeaderValues)
                {
                    throw new Exception(errorString);
                }

                companyDbService.GetTriggerImportDetails(out ptUserName, out ptPassword, out ServerManagerUrl,
                    int.Parse(CompanyId.ToString()), InstanceName.ToString());

                var InstanceCollection = Instance
                                         .CallServerManagerToGetInstance(ServerManagerUrl + "/api/public/getInstancesWithUser", ptUserName,
                                             ptPassword).Result;
                if (InstanceCollection.Count == 0) throw new Exception("No instances found");
                var instance = InstanceCollection.Where(x => x.InstanceName.ToLower() == InstanceName.ToString().ToLower()).FirstOrDefault();
                if (instance == null)
                {
                    throw new Exception("Cannot find instance");
                }

                var ptAuthToken = await Login(ptUserName, ptPassword, instance.SystemServiceUrl);

                GetScenarioResponse getScenarioResponse =
                    await Instance.GetScenarios(ptUserName, ptPassword, ptAuthToken, instance.SystemServiceUrl);

                var scenario = getScenarioResponse.Confirmations.Where(s => s.ScenarioName == ScenarioName).FirstOrDefault();

                if (scenario == null)
                {
                    throw new Exception("Scenario not found");
                }

                var importResponse = await Instance.ImportScenario(ptUserName, ptPassword, ptAuthToken, scenario,
                    instance.SystemServiceUrl);
                if (importResponse?.Exception ?? false)
                {
                    string originalExceptionString = importResponse.FullExceptionText;
                    string truncatedString = originalExceptionString.Length > 100
                        ? originalExceptionString.Substring(0, 100)
                        : originalExceptionString;

                    throw new Exception($"Import failed for scenario {scenario.ScenarioName}:ResponseCode={importResponse.ResponseCode} " +
                                        $"- {truncatedString}");
                }
            }
            catch (TaskCanceledException ex)
            {
                throw new TaskCanceledException();
            }

            catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
			return true;
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

        public static string GetInstanceSchemaMetaDataAsJson(StringValues schema, string connectionString)
        {
            string sqlQuery = $"SELECT t.NAME AS TableName, s.Name AS SchemaName, p.rows AS RowCounts " +
                              $"FROM sys.tables t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id " +
                              $"INNER JOIN sys.partitions p ON t.object_id = p.object_id " +
                              $"LEFT JOIN sys.dm_db_index_usage_stats u ON t.object_id = u.object_id AND u.database_id = DB_ID() " +
                              $"WHERE s.Name = '{schema}' AND p.index_id IN (0, 1) " +
                              $"ORDER BY t.NAME ASC;";
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(sqlQuery, connection))
                {
                    // Create a data adapter to execute the query and fill a dataset
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var dataSet = new DataSet();
                        adapter.Fill(dataSet);

                        // Convert the first table in the dataset to JSON
                        if (dataSet.Tables.Count > 0)
                        {
                            DataTable dataTable = dataSet.Tables[0];
                            string json = DataTableToJson(dataTable, 1000);
                            return json;
                        }
                    }
                }
            }
            return null;
        }
    }
}
