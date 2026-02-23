using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Text;

using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

using PT.Common.Debugging;

namespace PT.Common.Sql.SqlServer
{
    public class DbSchemaExtractor
    {
        public static void UploadSchemaToDb(SqlConnection a_connection, DBIntegrationDTO a_stagingDbUploadData)
        {
            a_connection.Open();
            SqlTransaction transaction = a_connection.BeginTransaction();
            try
            {
                List<string> createQueries = new();
                foreach (DBIntegrationObjectDTO table in a_stagingDbUploadData.IntegrationTableObjects)
                {
                    StringBuilder sb = new();
                    sb.Append($"DROP TABLE IF EXISTS {table.ObjectName};\n");
                    sb.Append(table.CreateCommand);
                    
                    createQueries.Add(sb.ToString());
                }
                foreach (DBIntegrationObjectDTO command in a_stagingDbUploadData.IntegrationViewObjects)
                {
                    createQueries.Add($"DROP VIEW IF EXISTS {command.ObjectName};\n"); //view creation cant be batched
                    createQueries.Add(command.CreateCommand + ";\n");
                }
                foreach (DBIntegrationObjectDTO command in a_stagingDbUploadData.IntegrationStoredProcObjects)
                {
                    createQueries.Add($"DROP PROCEDURE IF EXISTS {command.ObjectName};\n"); //sp creation cant be batched
                    createQueries.Add(command.CreateCommand + ";\n");
                }
                foreach (DBIntegrationObjectDTO command in a_stagingDbUploadData.IntegrationFunctionObjects)
                {
                    createQueries.Add($"DROP FUNCTION IF EXISTS {command.ObjectName};\n"); //function creation cant be batched
                    createQueries.Add(command.CreateCommand + ";\n");
                }
                foreach (string query in createQueries)
                {
                    using SqlCommand sqlCommand = a_connection.CreateCommand();
                    sqlCommand.Transaction = transaction;
                    sqlCommand.CommandText = query;
                    sqlCommand.ExecuteNonQuery();
                }
                transaction.Commit();
                a_connection.Close();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                a_connection.Close();
                throw e;
            }
        }

        public static DBIntegrationDTO RetrieveDBSchema(SqlConnection a_connection)
        {
            a_connection.Open();
            DBIntegrationDTO dbIntegrationDto = new DBIntegrationDTO();
            List<Exception> exceptions = new List<Exception>();

            DataTable tables = RetrieveTablesAndViews(a_connection);
            foreach (DataRow table in tables.Rows)
            {
                try
                {
                    RetrieveTableOrViewDetails(a_connection, table, dbIntegrationDto);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    DebugException.ThrowInDebug($"An error occured during the db retrieval process for object {table["TABLE_SCHEMA"]}.{table["TABLE_NAME"]}: {ex}");
                    continue; // skip adding this value; assume user lacks access
                }
            }

            List<string> storedProcedures = RetrieveStoredProcedures(a_connection);
            foreach (var procName in storedProcedures)
            {
                try
                {
                    RetrieveStoredProcedureDetails(a_connection, procName, dbIntegrationDto);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    DebugException.ThrowInDebug($"An error occured during the db retrieval process for stored procedure {procName}: {ex}");
                    continue; // skip adding this value; assume user lacks access
                }
            }

            List<string> funcNames = RetrieveFunctions(a_connection);
            foreach (string funcName in funcNames)
            {
                try
                {
                    RetrieveFunctionDetails(a_connection, funcName, dbIntegrationDto);
                }
                catch (Exception ex) // Something went wrong with this particular db object
                {
                    exceptions.Add(ex);
                    DebugException.ThrowInDebug($"An error occured during the db retrieval process for function {funcName}: {ex}");
                    continue; // skip adding this value; assume user lacks access
                }
            }
            a_connection.Close();

            CleanDbObjects(dbIntegrationDto);

            if (exceptions.Any())
            {
                // TODO: return aggregate exceptions
            }
            return dbIntegrationDto;
        }

        private static void CleanDbObjects(DBIntegrationDTO a_dbIntegrationDto)
        {
            a_dbIntegrationDto.IntegrationTableObjects.Sort((t1, t2) => t1.ObjectName.CompareTo(t2.ObjectName));
            a_dbIntegrationDto.IntegrationViewObjects.Sort((t1, t2) => t1.ObjectName.CompareTo(t2.ObjectName));
            a_dbIntegrationDto.IntegrationStoredProcObjects.Sort((t1, t2) => t1.ObjectName.CompareTo(t2.ObjectName));
            a_dbIntegrationDto.IntegrationFunctionObjects.Sort((t1, t2) => t1.ObjectName.CompareTo(t2.ObjectName));
        }

        private static void RetrieveTableOrViewDetails(SqlConnection a_connection, DataRow a_table, DBIntegrationDTO a_dbIntegrationDto)
        {
            using SqlCommand checkPermsCommand = a_connection.CreateCommand();
            checkPermsCommand.CommandText = $"SELECT HAS_PERMS_BY_NAME('[{a_table["TABLE_SCHEMA"]}].[{a_table["TABLE_NAME"]}]', 'OBJECT', 'VIEW DEFINITION')";
            int doWeHavePerms = (int)checkPermsCommand.ExecuteScalar();
            if (doWeHavePerms == 0)
            {
                return;
            }

            using SqlCommand cmd = a_connection.CreateCommand();
            if ((string)a_table["TABLE_TYPE"] == "VIEW")
            {
                cmd.CommandText = $"sys.sp_helptext";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@objname", SqlDbType.VarChar).Value = $"[{a_table["TABLE_SCHEMA"]}].[{a_table["TABLE_NAME"]}]";
                SqlDataReader tableReader = cmd.ExecuteReader();
                string createCmd = "";
                while (tableReader.Read())
                {
                    createCmd += tableReader.GetString(0);
                }

                a_dbIntegrationDto.IntegrationViewObjects.Add(new DBIntegrationObjectDTO
                {
                    ObjectName = $"[{a_table["TABLE_SCHEMA"]}].[{a_table["TABLE_NAME"]}]",
                    CreateCommand = createCmd.Replace("\n", " ").Replace("\r", "")
                });
                tableReader.Close();
            }
            else if ((string)a_table["TABLE_TYPE"] == "BASE TABLE")
            {
                cmd.CommandText = $"sys.sp_columns";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@table_name", SqlDbType.VarChar).Value = a_table["TABLE_NAME"].ToString();
                cmd.Parameters.Add("@table_owner", SqlDbType.VarChar).Value = a_table["TABLE_SCHEMA"].ToString();
                SqlDataReader tableReader = cmd.ExecuteReader();
                List<(string columnName, string typeName)> columns = new ();
                string tableName = $"[{a_table["TABLE_SCHEMA"]}].[{a_table["TABLE_NAME"]}]";
                while (tableReader.Read())
                {
                    ReadOnlyCollection<DbColumn> readOnlyCollection = tableReader.GetColumnSchema();
                    int columnOrd = readOnlyCollection.FirstOrDefault(c => c.ColumnName == "COLUMN_NAME").ColumnOrdinal.Value;
                    int typeOrd = readOnlyCollection.FirstOrDefault(c => c.ColumnName == "TYPE_NAME").ColumnOrdinal.Value;
                    columns.Add(($"[{(string)tableReader[columnOrd]}]", (string)tableReader[typeOrd]));
                }
                
                // This may be empty if the user lacks permissions to actually get any column data
                if (columns.IsNullOrEmpty())
                {
                    return;
                }

                StringBuilder sb = new ();
                sb.Append($"CREATE TABLE {tableName} (\n");
                foreach (var column in columns) //this isnt a great way to get the create script, ideally we would call an sp or something but apparently thats very hard to do with tables
                {
                    if (!string.IsNullOrEmpty(column.Item1))
                    {
                        //key is column name, value is sql data type
                        sb.Append($"{column.Item1} {column.Item2},\n");
                    }
                }
                sb.Append(");\n");
                
                a_dbIntegrationDto.IntegrationTableObjects.Add(new DBIntegrationObjectDTO()
                {
                    ObjectName = tableName,
                    CreateCommand = sb.ToString(),
                });
                tableReader.Close();
            }
        }

        private static DataTable RetrieveTablesAndViews(SqlConnection a_connection)
        {
            var tables = a_connection.GetSchema("Tables");
            return tables;
        }
        
        private static List<string> RetrieveFunctions(SqlConnection connection)
        {
            using SqlCommand funcCmd = connection.CreateCommand();
            funcCmd.CommandText = """
                                  SELECT o.name, 'schema' = SCHEMA_NAME(o.schema_id), m.definition, o.type_desc 
                                    FROM sys.sql_modules m 
                                  INNER JOIN sys.objects o 
                                          ON m.object_id=o.object_id
                                  WHERE o.type_desc like '%function%'
                                  """;
            SqlDataReader funcReader = funcCmd.ExecuteReader();
            List<string> funcNames = new List<string>();
            while (funcReader.Read())
            {
                funcNames.Add($"[{funcReader["schema"]}].[{funcReader["name"]}]");
            }
            funcReader.Close();
            return funcNames;
        }

        private static void RetrieveFunctionDetails(SqlConnection a_connection, string a_funcName, DBIntegrationDTO a_dbIntegrationDto)
        {
            using SqlCommand checkPermsCommand = a_connection.CreateCommand();
            checkPermsCommand.CommandText = $"SELECT HAS_PERMS_BY_NAME('{a_funcName}', 'OBJECT', 'EXECUTE')";
            int doWeHavePerms = (int)checkPermsCommand.ExecuteScalar();
            if (doWeHavePerms == 0)
            {
                return;
            }
            using SqlCommand cmd = a_connection.CreateCommand();
            cmd.CommandText = $"""
                           SELECT sm.object_id,
                              OBJECT_NAME(sm.object_id) AS object_name,
                              o.type,
                              o.type_desc,
                              sm.definition,
                              sm.uses_ansi_nulls,
                              sm.uses_quoted_identifier,
                              sm.is_schema_bound,
                              sm.execute_as_principal_id
                           FROM sys.sql_modules AS sm
                           JOIN sys.objects AS o ON sm.object_id = o.object_id
                           WHERE sm.object_id = OBJECT_ID('{a_funcName}')
                           ORDER BY o.type;
                           """;
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) //should only execute once
            {
                a_dbIntegrationDto.IntegrationFunctionObjects.Add(new ()
                {
                    ObjectName = a_funcName,
                    CreateCommand = reader["definition"].ToString(),
                });
            }
            reader.Close();
        }

        private static List<string> RetrieveStoredProcedures(SqlConnection a_connection)
        {
            using SqlCommand spCmd = a_connection.CreateCommand();
            spCmd.CommandText = $"select name, SCHEMA_NAME(schema_id)\n  from sys.procedures\n where is_ms_shipped = 0";
            SqlDataReader procReader = spCmd.ExecuteReader();
            List<string> SPs = new List<string>();
            while (procReader.Read())
            {
                SPs.Add($"[{procReader.GetString(1)}].[{procReader["name"].ToString()}]");
            }
            procReader.Close();
            return SPs;
        }

        private static void RetrieveStoredProcedureDetails(SqlConnection a_connection, string a_procName, DBIntegrationDTO a_dbIntegrationDto)
        {
            using SqlCommand checkPermsCommand = a_connection.CreateCommand();
            checkPermsCommand.CommandText = $"SELECT HAS_PERMS_BY_NAME('{a_procName}', 'OBJECT', 'EXECUTE')";
            int doWeHavePerms = (int)checkPermsCommand.ExecuteScalar();
            if (doWeHavePerms == 0)
            {
                return;
            }
            using SqlCommand cmd = a_connection.CreateCommand();
            cmd.CommandText = $"sys.sp_helptext";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@objname", SqlDbType.VarChar).Value = $"{a_procName}";
            SqlDataReader tableReader = cmd.ExecuteReader();
            StringBuilder sb = new StringBuilder();
            while (tableReader.Read())
            {
                sb.Append(tableReader.GetString(0));
            }
            string createCmd = sb.ToString();
            a_dbIntegrationDto.IntegrationStoredProcObjects.Add(new ()
            {
                ObjectName = a_procName,
                CreateCommand = createCmd
            });
            tableReader.Close();
        }
    }
}
