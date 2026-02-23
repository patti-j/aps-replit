using System.Data;

using Microsoft.Data.SqlClient;

namespace PT.Common.Sql.SqlServer;

/// <summary>
/// This class builds the SQL string needed to create a table.
/// </summary>
public class SqlTableCreator
{
    #region Instance Methods
    public string Create(DataTable schema)
    {
        string sql = GetCreateSQL(schema);
        return sql;
    }
    #endregion

    #region Static Methods
    public static void DeleteSqlTable(SqlConnection a_sqlConn, DataTable a_table)
    {
        string deleteCmd = $"DROP TABLE [{a_table.TableName}]";
        using (SqlCommand cmd = new (deleteCmd, a_sqlConn))
        {
            cmd.ExecuteNonQuery();
        }
    }

    public static void CreateSqlTable(SqlConnection a_sqlConn, DataTable a_table)
    {
        using (SqlCommand cmd = new (GetCreateSQL(a_table), a_sqlConn))
        {
            cmd.ExecuteNonQuery();
        }
    }

    public static void InsertToSql(SqlConnection a_sqlConn, DataTable a_table)
    {
        SqlBulkCopy bulkInsert = new (a_sqlConn, SqlBulkCopyOptions.TableLock, null);
        bulkInsert.BulkCopyTimeout = (int)TimeSpan.FromMinutes(5).TotalSeconds;

        bulkInsert.DestinationTableName = a_table.TableName;
        bulkInsert.BatchSize = a_table.Rows.Count;

        //Create Mapping List. Although the dataset columns are the same in the Database, they are not always in the same order.
        // A mapping is needed so that BulkCopy can match the columns by name.
        List<SqlBulkCopyColumnMapping> mappingList = new ();
        foreach (DataColumn c in a_table.Columns)
        {
            mappingList.Add(new SqlBulkCopyColumnMapping(c.ColumnName, c.ColumnName));
        }

        //Clear the old mappings if needed.
        bulkInsert.ColumnMappings.Clear();
        foreach (SqlBulkCopyColumnMapping mapping in mappingList)
        {
            bulkInsert.ColumnMappings.Add(mapping);
        }

        //Send the data.
        bulkInsert.WriteToServer(a_table);
    }

    /// <summary>
    /// Returns the sql string needed to create a table
    /// </summary>
    public static string GetCreateSQL(DataTable schema)
    {
        string sql = "CREATE TABLE " + schema.TableName + " (\n";

        // columns
        foreach (DataColumn column in schema.Columns)
        {
            sql += "[" + column.ColumnName + "] " + GetSqlType(column.DataType, column.MaxLength);
            if (!column.AllowDBNull)
            {
                sql += " not null";
            }
            else
            {
                sql += " null";
            }

            //if (column.DefaultValue != null)
            //{
            //TODO: This may be useful for some tables. For publishing this isn't necessary
            //}
            sql += ",\n";
        }

        sql = sql.TrimEnd(',', '\n') + "\n";

        // primary keys
        string pk = "CONSTRAINT PK_" + schema.TableName + " PRIMARY KEY CLUSTERED (";
        if (schema.PrimaryKey.Length > 0)
        {
            // user defined keys
            foreach (DataColumn key in schema.PrimaryKey)
            {
                pk += "[" + key.ColumnName + "], ";
            }

            pk = pk.TrimEnd(',', ' ', '\n') + ")\n";
            sql += pk;
        }

        sql += ")";

        return sql;
    }

    /// <summary>
    /// Returns the T-SQL equivalent type. Varchar types will be set according to the column's provided "MaxLength" attribute.
    /// </summary>
    public static string GetSqlType(Type a_columnType, int a_maxLength)
    {
        if (a_columnType == typeof(string))
        {
            string length = a_maxLength == -1 // Sql default MaxLength
                ? "max"
                : a_maxLength.ToString();
            return $"nvarchar({length})";
        }

        if (a_columnType == typeof(int))
        {
            return "int";
        }

        if (a_columnType == typeof(long))
        {
            return "bigint";
        }

        if (a_columnType == typeof(bool))
        {
            return "bit";
        }

        if (a_columnType == typeof(DateTime))
        {
            return "DateTime";
        }

        if (a_columnType == typeof(double))
        {
            return "float";
        }

        if (a_columnType == typeof(decimal))
        {
            return "float";
        }

        if (a_columnType == typeof(TimeSpan))
        {
            return "DateTime";
        }

        throw new CommonException(string.Format("Unknown Column Type: {0}", a_columnType.Name));
    }
    #endregion
}