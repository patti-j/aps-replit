using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PT.Common.Debugging;
using PT.Common.Extensions;
using PT.Common.Sql.SqlServer;
using PT.Common.Testing;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.ServerManagerSharedLib.DTOs.Responses;
using PT.ServerManagerSharedLib.Utils;
using System.Data;
using System.Text;
using Db = PT.ServerManagerSharedLib.Data.SettingsDbConstants;

namespace PT.ServerManagerSharedLib.Data
{
    /// <summary>
    /// Manages instance-level settings for an instance, stored in a local database file.
    /// </summary>
    public class DbInstanceSettingsManager : IMultiInstanceSettingsManager
    {
        /// <summary>
        /// Connection string to instance settings database. It's expected that there should be one of these per company.
        /// </summary>
        private readonly string m_settingsDbConnectionString;

        /// <summary>
        /// The Server making this call, be it for a particular instance, or all instances on the server (ie those managed by the Server Manager).
        /// </summary>
        private readonly string m_serverName;

        /// <summary>
        /// Initializes management of instances for a particular server.
        /// </summary>
        /// <param name="a_settingsDbConnectionString"></param>
        /// <param name="a_serverName">The unique identifier of the server the instances run on</param>
        public DbInstanceSettingsManager(string a_settingsDbConnectionString, string a_serverName)
        {
            m_serverName = a_serverName;
            m_settingsDbConnectionString = a_settingsDbConnectionString;

            //EnsureSettingsSourceExists(a_serverManagerPath);
        }

        /// <summary>
        /// Attempts to connect to the database and creates it if required.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public bool EnsureConnection()
        {
            SqlDatabaseConnectionHelper connectionHelper = new SqlDatabaseConnectionHelper(m_settingsDbConnectionString);
            if (!connectionHelper.IsValid())
            {
                throw new ArgumentException($"Instance Database Connection String not valid. ");
            }

            Exception error = null;

            using (var connection = new SqlConnection(m_settingsDbConnectionString))
            {
                try
                {
                    connection.Open();
                }
                catch (Exception e)
                {
                    error = new Exception($"An error occurred connecting to the instance database : {e}");
                }
                finally
                {
                    connection.Close();
                }

                if (error != null)
                {
                    throw error;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks to see if database version meets version of the running application (Server Manager or Instance, which use same versioning).
        /// If the DB is older, add any missing columns.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void EnsureDbVersion()
        {
            SoftwareVersion dbVersion = new SoftwareVersion("0.0");

            SoftwareVersion currentVersion = AssemblyVersionChecker.GetEntryAssemblyVersionSafe();

            if (currentVersion.Equals(new SoftwareVersion()))
            {
                throw new Exception($"Unable to find assembly version: current {currentVersion}.");
            }

            using var connection = new SqlConnection(m_settingsDbConnectionString);
            try
            {
                APS_InstancesDataSet dataSet = new APS_InstancesDataSet();
                APS_InstancesDataSet.ConfigurationDataTable dbConfigTable = dataSet.Configuration;

                connection.Open();

                bool configTableExists = EnsureConfigTableExists(dataSet, connection);
                if (configTableExists)
                {
                    dbVersion = GetDbVersion(dataSet, connection, dbConfigTable, dbVersion);
                }

                if (!configTableExists || !dbVersion.MeetsMinimum(currentVersion))
                {
                    MigrateDatabase(connection, dataSet, currentVersion);
                }
            }
            catch (Exception err)
            {
                throw new Exception($"Error upgrading database from version {dbVersion.ToString()} to {currentVersion.ToString()}.", err);
            }
        }

        private SoftwareVersion GetDbVersion(APS_InstancesDataSet a_dataSet, SqlConnection a_connection, APS_InstancesDataSet.ConfigurationDataTable a_dbConfigTable, SoftwareVersion a_dbVersion)
        {

            FillConfigurationDataTable(a_dataSet, a_connection);

            if (a_dbConfigTable.Rows.Count > 0)
            {
                string versionString = a_dbConfigTable.Rows[0][a_dbConfigTable.DbVersionColumn.ColumnName].ToString();
                a_dbVersion = new SoftwareVersion(versionString);
            }

            return a_dbVersion;
        }

        private void MigrateDatabase(SqlConnection a_connection, APS_InstancesDataSet a_dataSet, SoftwareVersion a_currentVersion)
        {
            DatabaseSynchronizer.AlterDbStructureToMatchDataSet(a_connection, a_dataSet);

            UpdateDbVersion(a_connection, a_dataSet, a_currentVersion);
        }

        private bool EnsureConfigTableExists(APS_InstancesDataSet a_dataSet, SqlConnection a_connection)
        {
            SqlCommand configTableCheck = new SqlCommand($"SELECT Count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{a_dataSet.Configuration.TableName}'", a_connection);
            int executeScalar = Convert.ToInt32(configTableCheck.ExecuteScalar());
            return executeScalar == 1;
        }

        private void FillConfigurationDataTable(APS_InstancesDataSet a_dataSet, SqlConnection a_connection)
        {
            APS_InstancesDataSet.ConfigurationDataTable dbConfigTable = a_dataSet.Configuration;

            using (SqlDataAdapter da = new SqlDataAdapter($"SELECT {dbConfigTable.DbVersionColumn.ColumnName} FROM {dbConfigTable.TableName}", a_connection))
            {
                da.Fill(dbConfigTable);
            }
        }

        private void UpdateDbVersion(SqlConnection a_connection, APS_InstancesDataSet a_dataSet, SoftwareVersion currentVersion)
        {
            // Clear existing value(s)
            SqlDatabaseConnectionHelper connectionHelper = new SqlDatabaseConnectionHelper(m_settingsDbConnectionString);
            connectionHelper.ExecuteScalar($"DELETE FROM {a_dataSet.Configuration.TableName}");

            // Add new version
            APS_InstancesDataSet.ConfigurationDataTable configTable = a_dataSet.Configuration;
            a_dataSet.Configuration.Rows.Clear();

            DataRow dataRow = configTable.NewRow();
            dataRow[configTable.DbVersionColumn] = currentVersion;
            configTable.Rows.Add(dataRow);

            SqlTableCreator.InsertToSql(a_connection, configTable);
        }


        /// <summary>
        /// Creates new instance settings file. This puts the connection string in a fixed place, so the instance can connect to the database without any other knowledge of its settings.
        /// </summary>
        /// <param name="instance"></param>
        public void CreateInstanceConnectionFile(APSInstanceEntity a_instance, string a_connectionString)
        {
            string instanceSettingsDirectory = a_instance.ServicePaths.SystemInstanceSettingsDirectory;

            if (!Directory.Exists(instanceSettingsDirectory))
            {
                Directory.CreateDirectory(instanceSettingsDirectory);
            }

            using FileStream fs = File.Open(a_instance.ServicePaths.SystemInstanceConnectionFilePath, FileMode.Create);
            {
                using StreamWriter sw = new StreamWriter(fs);
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        JsonSerializer serializer = new() { Formatting = Formatting.Indented };

                        serializer.Serialize(writer, new { InstanceDataConnectionString = a_connectionString });
                    }
                }
            }
        }


        public APSInstanceEntity GetInstance(string a_instanceName, string a_instanceVersion)
        {
            APSInstanceEntity instance = GetInstanceFromDb(a_instanceName, a_instanceVersion);

            return instance;
        }

        private APSInstanceEntity GetInstanceFromDb(string a_instanceName, string a_instanceVersion)
        {
            SqlScriptHelper sql = new SqlScriptHelper();
            string nameParam = sql.SetSqlParam("@instanceName", a_instanceName);
            string versionParam = sql.SetSqlParam("@instanceVersion", a_instanceVersion);

            // The database stores the Instance in a single JSON string (to match existing .json config management, and to leverage existing management code).
            // We first pull in and deserialize that data, then make the relevant update.
            sql.Script = $"SELECT instance.{Db.c_settingsCol}, server.Name as ServerName, server.* " +
                         $"FROM {Db.c_instanceTable} instance " +
                         $"RIGHT JOIN {Db.c_serverTable} server " +
                         $"ON instance.{Db.c_instanceServerCol}=server.{Db.c_serverNameCol} " +
                         $"WHERE instance.{Db.c_instanceNameCol}={nameParam} " +
                         $"AND instance.{Db.c_versionCol}={versionParam} " +
                         $"AND instance.{Db.c_instanceServerCol}='{m_serverName}'";

            SqlDatabaseConnectionHelper connectionHelper = new SqlDatabaseConnectionHelper(m_settingsDbConnectionString);
            APSInstanceEntity instance = connectionHelper.GetFullInstanceDataFromDb(sql.Script, sql.SqlParams);

            //Commenting out the below code because when throwing exception, the React UI was not updating. Bugs 26371, 26372
            //if (instance == null)
            //{
            //    throw new KeyNotFoundException($"Could not find data for instance {a_instanceName} {a_instanceVersion} " +
            //                                   $"on server {m_serverName} in {Db.c_instanceTable} table.");
            //}

            return instance;
        }

        public void SaveInstance(APSInstanceEntity a_instance)
        {
            SqlScriptHelper insertSql = BuildInstanceInsertSql(new List<APSInstanceEntity>() { a_instance });

            SqlDatabaseConnectionHelper connectionHelper = new SqlDatabaseConnectionHelper(m_settingsDbConnectionString);
            connectionHelper.Insert(insertSql.Script, insertSql.SqlParams);
        }

        public bool DeleteInstance(string _, string a_instanceName, string a_instanceVersion)
        {
            SqlScriptHelper sql = new SqlScriptHelper();
            string nameParam = sql.SetSqlParam("@instanceName", a_instanceName);
            string versionParam = sql.SetSqlParam("@instanceVersion", a_instanceVersion);

            sql.Script = $"DELETE FROM {Db.c_instanceTable} " +
                         $"WHERE {Db.c_instanceNameCol}= {nameParam} " +
                         $"AND {Db.c_versionCol}={versionParam} " +
                         $"AND {Db.c_instanceServerCol}='{m_serverName}' " +
                         $"SELECT @@ROWCOUNT";
            SqlDatabaseConnectionHelper connectionHelper = new SqlDatabaseConnectionHelper(m_settingsDbConnectionString);
            object deleteResponse = connectionHelper.ExecuteScalar(sql.Script, sql.SqlParams);
            int.TryParse(deleteResponse.ToString(), out int rowsAffected);
            if (rowsAffected == 0)
            {
                throw new KeyNotFoundException($"Could not find data to delete for instance {a_instanceName} {a_instanceVersion} " +
                                               $"on server {m_serverName} in {Db.c_instanceTable} table.");
            }

            return true;

        }

        /// <summary>
        /// Delete all instances for a particular server.
        /// </summary>
        /// <returns></returns>
        public bool DeleteAllInstances()
        {
            SqlScriptHelper sql = new SqlScriptHelper();

            sql.Script = $"DELETE FROM {Db.c_instanceTable} " +
                         $"WHERE {Db.c_instanceServerCol}='{m_serverName}';";

            SqlDatabaseConnectionHelper connectionHelper = new SqlDatabaseConnectionHelper(m_settingsDbConnectionString);
            object deleteResponse = connectionHelper.ExecuteScalar(sql.Script, sql.SqlParams);

            return true;
        }


        public IEnumerable<APSInstanceEntity> GetInstances()
        {
            List<APSInstanceEntity> instances = new List<APSInstanceEntity>();

            string query = $"SELECT {Db.c_settingsCol} FROM {Db.c_instanceTable} " +
                           $"WHERE {Db.c_instanceServerCol}='{m_serverName}'";

            SqlDatabaseConnectionHelper connectionHelper = new SqlDatabaseConnectionHelper(m_settingsDbConnectionString);
            return connectionHelper.GetDeserializedJsonRows<APSInstanceEntity>(query);
        }

        /// <summary>
        /// Handles saving instances in a single bulk transaction.
        /// </summary>
        /// <param name="a_instances"></param>
        public void SaveInstances(IEnumerable<APSInstanceEntity> a_instances)
        {
            if (!a_instances.Any())
            {
                return;
            }

            try
            {
                SqlScriptHelper bulkInsertSql = BuildInstanceInsertSql(a_instances);

                SqlDatabaseConnectionHelper connectionHelper = new SqlDatabaseConnectionHelper(m_settingsDbConnectionString);
                connectionHelper.Insert(bulkInsertSql.Script, bulkInsertSql.SqlParams); // handles transaction

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public InstanceSettingsEntity GetInstanceSettingsEntity(string a_instanceName, string a_instanceVersion)
        {
            APSInstanceEntity apsInstanceEntity = GetInstance(a_instanceName, a_instanceVersion);
            return new InstanceSettingsEntity(apsInstanceEntity);
        }

        public StartupVals GetStartupVals(string a_instanceName, string a_instanceVersion)
        {
            APSInstanceEntity apsInstanceEntity = GetInstance(a_instanceName, a_instanceVersion);
            return new StartupValsModel(apsInstanceEntity);
        }

        public string GetCertificateThumbprint(string a_instanceName, string a_instanceVersion)
        {
            string thumbprint = null;
            try
            {
                thumbprint = GetServerSettings().Thumbprint;
            }
            catch
            {
                // Old schema before Settings table was added.
                try
                {
                    SqlScriptHelper sql = new SqlScriptHelper();
                    string nameParam = sql.SetSqlParam("@instanceName", a_instanceName);
                    string versionParam = sql.SetSqlParam("@instanceVersion", a_instanceVersion);

                    sql.Script = $"SELECT {Db.c_oldInstanceThumbprintCol} FROM {Db.c_instanceTable} " +
                                 $"WHERE {Db.c_instanceTable}.{Db.c_instanceNameCol} ={nameParam} " +
                                 $"AND {Db.c_versionCol}={versionParam} " +
                                 $"AND {Db.c_instanceServerCol}='{m_serverName}'";

                    SqlDatabaseConnectionHelper connectionHelper = new SqlDatabaseConnectionHelper(m_settingsDbConnectionString);
                    thumbprint = connectionHelper.ExecuteScalar(sql.Script, sql.SqlParams).ToString();
                }
                catch
                {
                    // Try using cert utility below
                }
            }

            return thumbprint;
        }

        public ErpDatabase GetErpDatabaseSettings(string a_instanceName, string a_instanceVersion)
        {
            return new ErpDatabase(GetInstance(a_instanceName, a_instanceVersion).Settings.ErpDatabaseSettings);
        }

        public ServerWideInstanceSettings GetServerSettings()
        {
            SqlScriptHelper sql = new SqlScriptHelper();

            sql.Script = $"SELECT * FROM {Db.c_serverTable} " +
                         $"WHERE {Db.c_serverNameCol}='{m_serverName}'";

            SqlDatabaseConnectionHelper connectionHelper = new SqlDatabaseConnectionHelper(m_settingsDbConnectionString);
            return connectionHelper.GetServerSettings(sql.Script, sql.SqlParams);
        }

        public string GetServerManagerFolder(string a_instanceName, string a_instanceVersion)
        {
            string path = null;
            try
            {
                path = GetServerSettings().ServerManagerPath;
            }
            catch
            {
                try
                {
                    // Old schema before Settings table was added.
                    SqlScriptHelper sql = new SqlScriptHelper();
                    string nameParam = sql.SetSqlParam("@instanceName", a_instanceName);
                    string versionParam = sql.SetSqlParam("@instanceVersion", a_instanceVersion);

                    sql.Script = $"SELECT {Db.c_serverPathCol} FROM {Db.c_instanceTable} " +
                                 $"WHERE {Db.c_instanceTable}.{Db.c_instanceNameCol} ={nameParam} " +
                                 $"AND {Db.c_versionCol}={versionParam} " +
                                 $"AND {Db.c_instanceServerCol}='{m_serverName}'";

                    SqlDatabaseConnectionHelper connectionHelper = new SqlDatabaseConnectionHelper(m_settingsDbConnectionString);
                    path = connectionHelper.ExecuteScalar(sql.Script, sql.SqlParams).ToString();
                }
                catch
                {
                    // Try using default below
                }
            }

            if (path.IsNullOrEmpty())
            {
                path = Paths.ServerManagerPath;
            }

            return path;
        }

        //public void BackUpSettings(string a_backupPostfix)
        //{
        //    SqlDatabaseConnectionHelper connectionHelper = new SqlDatabaseConnectionHelper(m_settingsDbConnectionString);
        //    connectionHelper.BackUpDatabase(a_backupPostfix);
        //}

        /// <summary>
        /// Writes Server-level config data to the database
        /// </summary>
        /// <param name="a_thumbprint"></param>
        /// <param name="a_serverManagerPath"></param>
        public void SaveServerwideSettings(ServerSettingsDto a_settings)
        {
            SqlScriptHelper sql = new SqlScriptHelper();
            string serverPathParam = sql.SetSqlParam("@serverManagerPath", a_settings.ServerManagerPath ?? string.Empty);
            string thumbprintParam = sql.SetSqlParam("@thumbprintParam", a_settings.Thumbprint ?? string.Empty);
            string ssoDomainParam = sql.SetSqlParam("@ssoDomainParam", a_settings.SsoDomain ?? string.Empty);
            string ssoClientParam = sql.SetSqlParam("@ssoClientParam", a_settings.SsoClientId ?? string.Empty);
            string webAppUrlParam = sql.SetSqlParam("@webAppUrlParam", a_settings.WebAppUrl ?? string.Empty);
            string webAppClientIdParam = sql.SetSqlParam("@webAppClientIdParam", a_settings.WebAppClientId ?? string.Empty);

            StringBuilder insertBuilder = new StringBuilder();

            insertBuilder.AppendLine($"MERGE {Db.c_serverTable} AS target ");
            insertBuilder.AppendLine($"USING( ");
            insertBuilder.AppendLine($"VALUES ('{m_serverName}', {serverPathParam}, {thumbprintParam}, {ssoDomainParam}, {ssoClientParam}, {webAppUrlParam}, {webAppClientIdParam}) ");
            insertBuilder.AppendLine(") ");

            insertBuilder.AppendLine($"AS source ({Db.c_serverNameCol}, {Db.c_serverPathCol}, {Db.c_serverThumbprintCol}, {Db.c_ssoDomainCol}, {Db.c_ssoClientIdCol}, {Db.c_webAppUrlCol}, {Db.c_webAppClientIdCol}) ");
            insertBuilder.AppendLine($"ON target.{Db.c_serverNameCol} = source.{Db.c_serverNameCol} ");

            insertBuilder.AppendLine("WHEN MATCHED THEN");
            insertBuilder.AppendLine("UPDATE SET");

            // Map all (non-key) columns to update
            insertBuilder.AppendLine($"target.{Db.c_serverPathCol} = source.{Db.c_serverPathCol}, ");
            insertBuilder.AppendLine($"target.{Db.c_serverThumbprintCol} = source.{Db.c_serverThumbprintCol}, ");
            insertBuilder.AppendLine($"target.{Db.c_ssoDomainCol} = source.{Db.c_ssoDomainCol}, ");
            insertBuilder.AppendLine($"target.{Db.c_ssoClientIdCol} = source.{Db.c_ssoClientIdCol}, ");
            insertBuilder.AppendLine($"target.{Db.c_webAppUrlCol} = source.{Db.c_webAppUrlCol}, ");
            insertBuilder.AppendLine($"target.{Db.c_webAppClientIdCol} = source.{Db.c_webAppClientIdCol} ");

            insertBuilder.AppendLine("WHEN NOT MATCHED THEN");
            insertBuilder.AppendLine($"INSERT ({Db.c_serverNameCol}, {Db.c_serverPathCol}, {Db.c_serverThumbprintCol}, {Db.c_ssoDomainCol}, {Db.c_ssoClientIdCol}, {Db.c_webAppUrlCol}, {Db.c_webAppClientIdCol}) ");
            insertBuilder.AppendLine($"VALUES ('{m_serverName}', {serverPathParam}, {thumbprintParam}, {ssoDomainParam}, {ssoClientParam}, {webAppUrlParam}, {webAppClientIdParam}); ");

            // Legacy handling for older instances (12.1.4.x) that use this nuget from when these columns were on the instance table, but using the latest Server Manager. When no one is using 12.1.4 anymore, gut this ruthlessly
            // TODO: Do we want to add this to every save instance query? It might be better to release an update for 12.1.4 using this latest nuget, but it would break all previous 12.1.4 versions (upon changing these values/ clearing db)
            insertBuilder.AppendLine("DECLARE @sqlLegacyThumbprint NVARCHAR(MAX);");
            insertBuilder.AppendLine("SET @sqlLegacyThumbprint = '';");
            insertBuilder.AppendLine("IF EXISTS (");
            insertBuilder.AppendLine("    SELECT 1 ");
            insertBuilder.AppendLine("    FROM INFORMATION_SCHEMA.COLUMNS ");
            insertBuilder.AppendLine($"    WHERE TABLE_NAME = '{Db.c_instanceTable}' AND COLUMN_NAME = '{Db.c_oldInstanceThumbprintCol}'");
            insertBuilder.AppendLine(")");
            insertBuilder.AppendLine("BEGIN");
            insertBuilder.AppendLine("    SET @sqlLegacyThumbprint = '");
            insertBuilder.AppendLine("        UPDATE " + Db.c_instanceTable);
            insertBuilder.AppendLine("        SET " + Db.c_oldInstanceThumbprintCol + " = @NewValue");
            insertBuilder.AppendLine("        WHERE " + Db.c_instanceServerCol + " = '' " + m_serverName + "'' ';"); // Replace <your_condition> with your actual condition
            insertBuilder.AppendLine("END");
            insertBuilder.AppendLine($"EXEC sp_executesql @sqlLegacyThumbprint, N'@NewValue VARCHAR(100)', @NewValue = {thumbprintParam};");

            insertBuilder.AppendLine("DECLARE @sqlLegacyServerPath NVARCHAR(MAX);");
            insertBuilder.AppendLine("SET @sqlLegacyServerPath = '';");
            insertBuilder.AppendLine("IF EXISTS (");
            insertBuilder.AppendLine("    SELECT 1 ");
            insertBuilder.AppendLine("    FROM INFORMATION_SCHEMA.COLUMNS ");
            insertBuilder.AppendLine($"    WHERE TABLE_NAME = '{Db.c_instanceTable}' AND COLUMN_NAME = '{Db.c_oldInstanceServerManagerPathColumn}'");
            insertBuilder.AppendLine(")");
            insertBuilder.AppendLine("BEGIN");
            insertBuilder.AppendLine("    SET @sqlLegacyServerPath = '");
            insertBuilder.AppendLine("        UPDATE " + Db.c_instanceTable);
            insertBuilder.AppendLine("        SET " + Db.c_oldInstanceServerManagerPathColumn + " = @NewValue");
            insertBuilder.AppendLine("        WHERE " + Db.c_instanceServerCol + " = '' " + m_serverName + "'' ';"); // Replace <your_condition> with your actual condition
            insertBuilder.AppendLine("END");
            insertBuilder.AppendLine($"EXEC sp_executesql @sqlLegacyServerPath, N'@NewValue VARCHAR(100)', @NewValue = {serverPathParam};");

            SqlDatabaseConnectionHelper connectionHelper = new SqlDatabaseConnectionHelper(m_settingsDbConnectionString);

            sql.Script = insertBuilder.ToString();
            connectionHelper.Insert(sql.Script, sql.SqlParams);
        }

        public void SetAsBackup(APSInstanceEntity newVersion, APSInstanceEntity oldVersion)
        {
        }

        private SqlScriptHelper BuildInstanceInsertSql(IEnumerable<APSInstanceEntity> a_instances)
        {
            SqlScriptHelper sql = new();
            StringBuilder insertBuilder = new StringBuilder();

            insertBuilder.AppendLine($"MERGE {Db.c_instanceTable} AS target " +
                                    $"USING( " +
                                    $"VALUES ");

            List<APSInstanceEntity> apsInstanceEntities = a_instances.ToList();
            for (int i = 0; i < apsInstanceEntities.Count; i++)
            {
                APSInstanceEntity instance = apsInstanceEntities[i];

                string serializedInsert = JsonConvert.SerializeObject(instance);

                string instanceSettingsParam = sql.SetSqlParam($"@instanceSettings{i}", serializedInsert);
                string instanceNameParam = sql.SetSqlParam($"@instanceName{i}", instance.PublicInfo.InstanceName);
                string instanceVersionParam = sql.SetSqlParam($"@instanceVersion{i}", instance.PublicInfo.SoftwareVersion);

                insertBuilder.AppendLine($"({instanceNameParam}, {instanceVersionParam}, {instanceSettingsParam}, '{m_serverName}'), ");
            }

            insertBuilder.Remove(insertBuilder.Length - 4, 2); // Remove last comma

            insertBuilder.AppendLine(") ");
            insertBuilder.AppendLine($"AS source ({Db.c_instanceNameCol}, {Db.c_versionCol}, {Db.c_settingsCol}, {Db.c_instanceServerCol}) ");
            insertBuilder.AppendLine($"ON target.{Db.c_instanceNameCol} = source.{Db.c_instanceNameCol} ");
            insertBuilder.AppendLine($"AND target.{Db.c_versionCol} = source.{Db.c_versionCol} ");
            insertBuilder.AppendLine($"AND target.{Db.c_instanceServerCol} = source.{Db.c_instanceServerCol} ");

            insertBuilder.AppendLine("WHEN MATCHED THEN");
            insertBuilder.AppendLine("UPDATE SET");
            // Map all (non-key) columns to update
            insertBuilder.AppendLine($"target.{Db.c_settingsCol} = source.{Db.c_settingsCol} ");

            insertBuilder.AppendLine("WHEN NOT MATCHED THEN");
            insertBuilder.AppendLine($"INSERT ({Db.c_instanceNameCol}, {Db.c_versionCol}, {Db.c_settingsCol}, {Db.c_instanceServerCol}) ");
            insertBuilder.AppendLine($"VALUES (source.{Db.c_instanceNameCol}, source.{Db.c_versionCol}, source.{Db.c_settingsCol}, source.{Db.c_instanceServerCol});");

            sql.Script = insertBuilder.ToString();
            return sql;
        }
    }

    /// <summary>
    ///  Constants for DB schema
    /// </summary>
    class SettingsDbConstants
    {
        internal const string c_dbName = "InstanceData.db";

        // Instance Table
        internal const string c_instanceTable = "InstanceSettings";
        internal const string c_instanceNameCol = "Name";
        internal const string c_versionCol = "Version";
        internal const string c_instanceServerCol = "Server";
        internal const string c_settingsCol = "Settings";

        // Old Instance columns that existed before the Server table. Need to keep this alive so old instances don't break when they try to get these values.
        internal const string c_oldInstanceThumbprintCol = "Thumbprint";
        internal const string c_oldInstanceServerManagerPathColumn = "ServerManagerPath";

        // Server Table
        internal const string c_serverTable = "ServerSettings";
        internal const string c_serverNameCol = "Name";
        internal const string c_serverPathCol = "ServerManagerPath";
        internal const string c_serverThumbprintCol = "Thumbprint";
        internal const string c_ssoDomainCol = "SsoDomain";
        internal const string c_ssoClientIdCol = "SsoClientId";
        internal const string c_webAppUrlCol = "WebAppUrl";
        internal const string c_webAppClientIdCol = "WebAppClientId";
    }
}
