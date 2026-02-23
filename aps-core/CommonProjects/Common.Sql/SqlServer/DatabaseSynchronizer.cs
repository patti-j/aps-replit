using System.Data;

using Microsoft.Data.SqlClient;

using PT.Common.Sql.Exceptions;

namespace PT.Common.Sql.SqlServer;

public class DatabaseSynchronizer
{

    /// <summary>
    /// Validates if the publish database schema is the same as the dataset being published. Takes into account missing tables, missing columns, and extra columns.
    /// </summary>
    /// <param name="a_openConnection">Active connection with SQL database to alter.</param>
    /// <param name="a_ds">Data set to conform the database to.</param>
    /// <param name="a_removeMissingCols">Whether column removals will be reconciled. Defaults to true. Setting to false may help maintain compatibility if multiple versions connect to the same db.</param>
    /// <exception cref="PTDatabaseException"></exception>
    public static void AlterDbStructureToMatchDataSet(SqlConnection a_openConnection, DataSet a_ds, bool a_removeMissingCols = true)
    {
        for (int dsTableI = 0; dsTableI < a_ds.Tables.Count; dsTableI++)
        {
            DataTable dsTable = a_ds.Tables[dsTableI];
            Dictionary<string, ColumnInfo> columnNames = GetColumnNames(a_openConnection, dsTable.TableName);
            if (columnNames == null)
            {
                //the table doesn't exist. Create it from the dataset
                string createTableSQL = SqlTableCreator.GetCreateSQL(dsTable);
                using (SqlCommand sqlCmd = a_openConnection.CreateCommand())
                {
                    sqlCmd.CommandText = createTableSQL;
                    sqlCmd.CommandType = CommandType.Text;

                    try
                    {
                        sqlCmd.ExecuteNonQuery();
                        continue;
                    }
                    catch (Exception e)
                    {
                        //Table doesn't exist
                        throw new PTDatabaseException("2912", e, new object[] { dsTable.TableName });
                    }
                }
            }

            List<ColumnInfo> columnsToAdd = new ();
            List<ColumnInfo> columnsToRemove = new ();

            //Find columns that are missing in the target database
            foreach (DataColumn column in dsTable.Columns)
            {
                ColumnInfo missingColumnInfo;
                if (!columnNames.TryGetValue(column.ColumnName, out missingColumnInfo))
                {
                    ColumnInfo columnInfoToAdd = new (column.ColumnName);
                    columnInfoToAdd.ColumnType = column.DataType;
                    columnInfoToAdd.ColumnMaxLength = column.MaxLength;
                    columnInfoToAdd.Nullable = column.AllowDBNull;
                    columnsToAdd.Add(columnInfoToAdd);
                }
            }

            if (a_removeMissingCols)
            {
                //Find columns that are extra in the target database
                foreach (KeyValuePair<string, ColumnInfo> columnInfo in columnNames)
                {
                    if (columnInfo.Value.AutoIndex)
                    {
                        continue;
                    }

                    if (!dsTable.Columns.Contains(columnInfo.Key))
                    {
                        columnsToRemove.Add(columnInfo.Value);
                    }
                }
            }

            SynchronizeColumns(a_openConnection, dsTable, dsTable.TableName, columnsToAdd, columnsToRemove);
        }
    }

    /// <summary>
    /// From lists of missing and extra columns, alters the sql database to match.
    /// </summary>
    /// <param name="a_con"></param>
    /// <param name="a_tableName"></param>
    /// <param name="a_columnsToAdd"></param>
    /// <param name="a_columnsToRemove"></param>
    private static void SynchronizeColumns(SqlConnection a_con, DataTable a_table, string a_tableName, List<ColumnInfo> a_columnsToAdd, List<ColumnInfo> a_columnsToRemove)
    {
        using (SqlCommand sqlCmd = a_con.CreateCommand())
        {
            sqlCmd.CommandType = CommandType.Text;
            string currentColumnName = "";
            try
            {
                //Add missing columns
                foreach (ColumnInfo columnToAdd in a_columnsToAdd)
                {
                    currentColumnName = columnToAdd.ComparisonName;
                    string type = SqlTableCreator.GetSqlType(columnToAdd.ColumnType, columnToAdd.ColumnMaxLength);
                    sqlCmd.CommandText = "Alter table " + a_tableName + " add " + columnToAdd.SQLName + " " + type;

                    if (!columnToAdd.Nullable)
                    {
                        object defaultValue = GetDefaultValue(columnToAdd);

                        string defaultSqlLiteral = ToSqlLiteral(defaultValue, columnToAdd.ColumnType);

                        sqlCmd.CommandText += $" NOT NULL DEFAULT ({defaultSqlLiteral})";
                    }

                    sqlCmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw new PTDatabaseException("2910", e, new object[] { a_tableName, currentColumnName });
            }

            //It's possible that SynchronizeColumns will want to remove columns that were a part of this table's PK constraint. In that case, we want to drop the current PK, remove the columns, and remake the PK constraint.
            //This method will update the a_columnsToRemove collection for any primary key column removed.
            SynchronizePrimaryKeyConstraint(a_con, a_table, a_tableName, a_columnsToRemove);

            try
            {
                //Remove extra columns
                foreach (ColumnInfo columnToRemove in a_columnsToRemove)
                {
                    currentColumnName = columnToRemove.ComparisonName;
                    sqlCmd.CommandText = "alter table " + a_tableName + " drop column " + columnToRemove.SQLName;
                    sqlCmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw new PTDatabaseException("2911", e, new object[] { a_tableName, currentColumnName });
            }
        }
    }

    private static void SynchronizePrimaryKeyConstraint(SqlConnection a_con, DataTable a_table, string a_tableName, List<ColumnInfo> a_columnsToRemove)
    {
        if (a_columnsToRemove.Count == 0)
        {
            return;
        }

        using (SqlCommand sqlCmd = a_con.CreateCommand())
        {
            sqlCmd.CommandType = CommandType.Text;
            string currentColumnName = "";

            try
            {
                bool primaryConstraintDropped = false;

                //We won't be able to drop the column if it was previously used as a primary key column. We need to drop the foreign key constraint, drop the columns, and then recreate the primary key.

                // Get primary key column names for this table
                HashSet<string> primaryKeyColumnsInDb = GetPrimaryKeyColumns(a_con, a_tableName);
                if (primaryKeyColumnsInDb.Count == 0)
                {
                    return;
                }

                //Check if any of the columns to remove are part of the former PrimaryKey constraint
                for (int i = a_columnsToRemove.Count - 1; i >= 0; i--)
                {
                    ColumnInfo columnToRemove = a_columnsToRemove[i];
                    currentColumnName = columnToRemove.ComparisonName;

                    if (primaryKeyColumnsInDb.Contains(currentColumnName))
                    {
                        //Drop the primary key constraint, we only need to do this once per table.
                        if (!primaryConstraintDropped)
                        {
                            primaryConstraintDropped = DropPrimaryKeyConstraint(a_con, a_tableName);
                        }

                        //Drop the column
                        sqlCmd.CommandText = "ALTER TABLE " + a_tableName + " DROP COLUMN " + columnToRemove.SQLName + ";";
                        sqlCmd.ExecuteNonQuery();
                        a_columnsToRemove.RemoveAt(i);
                    }
                }

                //Recreate the Primary Key constraint if it was deleted.
                if (primaryConstraintDropped)
                {
                    HashSet<string> currentPrimaryKeyColumns = new HashSet<string>(
                        a_table.PrimaryKey.Select(c => c.ColumnName),
                        StringComparer.OrdinalIgnoreCase
                    );

                    sqlCmd.CommandText = "ALTER TABLE " + a_tableName + " ADD CONSTRAINT PK_" + a_tableName + " PRIMARY KEY (" + string.Join(',', currentPrimaryKeyColumns) + ");";
                    sqlCmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw new PTDatabaseException("3101", e, new object[] { a_tableName });
            }
        }
    }
    private static string ToSqlLiteral(object value, Type columnType)
    {
        if (value == null)
        {
            return "NULL";
        }

        if (columnType == typeof(string))
        {
            return $"N'{value.ToString()!.Replace("'", "''")}'";
        }

        if (columnType == typeof(bool))
        {
            return ((bool)value) ? "1" : "0";
        }

        if (columnType == typeof(DateTime))
        {
            return $"'{((DateTime)value):yyyy-MM-ddTHH:mm:ss.fff}'";
        }

        // numeric types
        return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)!;
    }
    private static object GetDefaultValue(ColumnInfo a_column)
    {
        Type type = a_column.ColumnType;

        // Nullable<T> → unwrap
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(decimal) || type == typeof(double) || type == typeof(float))
        {
            return 0;
        }

        if (type == typeof(bool))
        {
            return false;
        }

        if (type == typeof(DateTime))
        {
            return DateTime.UtcNow;
        }

        if (type == typeof(Guid))
        {
            return Guid.Empty;
        }

        if (type == typeof(string))
        {
            return string.Empty;
        }

        throw new NotSupportedException(
            $"No default value defined for column '{a_column.SQLName}' of type '{type.Name}'.");
    }

    private static HashSet<string> GetPrimaryKeyColumns(SqlConnection a_conn, string a_tableName)
    {
        HashSet<string> primaryKeyColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            using (SqlCommand cmd = a_conn.CreateCommand())
            {
                cmd.CommandText = @"
            SELECT c.name AS ColumnName
            FROM sys.indexes i
                INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                INNER JOIN sys.tables t ON i.object_id = t.object_id
            WHERE i.is_primary_key = 1 AND t.name = @tableName";

                cmd.Parameters.AddWithValue("@tableName", a_tableName);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        primaryKeyColumns.Add(reader.GetString(0));
                    }
                }
            }
        }
        catch (Exception ee)
        {
            throw new PTDatabaseException("3103", ee, new object[] { a_tableName });
        }

        return primaryKeyColumns;
    }

    private static bool DropPrimaryKeyConstraint(SqlConnection a_conn, string a_tableName)
    {

        try
        {
            string constraintName = "";

            using (var cmd = a_conn.CreateCommand())
            {
                //First get the primary key constraint name for the table. We could probably assume this to be 'PK_[TABLE NAME]' although, we might want to do this to be safe.
                cmd.CommandText = @"SELECT kc.name
                                    FROM sys.key_constraints kc
                                    JOIN sys.tables t ON kc.parent_object_id = t.object_id
                                    WHERE kc.type = 'PK' AND t.name = '" + a_tableName + "';";

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    constraintName = result.ToString();

                    //If we have the constraint name, go ahead and drop it.
                    if (!string.IsNullOrEmpty(constraintName))
                    {
                        cmd.CommandText = "ALTER TABLE " + a_tableName + " DROP CONSTRAINT " + constraintName + ";";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        catch (Exception ee)
        {
            throw new PTDatabaseException("3103", ee, new object[] { a_tableName });
        }

        return true;
    }



    private static Dictionary<string, ColumnInfo> GetColumnNames(SqlConnection conn, string tableName)
    {
        Dictionary<string, ColumnInfo> result = new ();
        DataTable dataTable;
        using (SqlCommand sqlCmd = conn.CreateCommand())
        {
            sqlCmd.CommandText = "select * from " + tableName; // No data wanted, only schema
            sqlCmd.CommandType = CommandType.Text;

            try
            {
                using (SqlDataReader sqlDR = sqlCmd.ExecuteReader(CommandBehavior.KeyInfo))
                {
                    dataTable = sqlDR.GetSchemaTable();
                }
            }
            catch (Exception)
            {
                //Table doesn't exist
                return null;
            }
        }

        foreach (DataRow row in dataTable.Rows)
        {
            ColumnInfo colInfo = new ((string)row["ColumnName"]);
            colInfo.AutoIndex = (bool)row["IsIdentity"] || (bool)row["IsAutoIncrement"];
            colInfo.ColumnType = (Type)row["DataType"];
            result.Add(colInfo.ComparisonName, colInfo);
        }

        return result;
    }

    public class ColumnInfo
    {
        public ColumnInfo(string a_columnName)
        {
            if (a_columnName.StartsWith("["))
            {
                m_name = m_name.Trim('[', ']');
            }
            else
            {
                m_name = a_columnName;
            }
        }

        private readonly string m_name;

        public string ComparisonName => m_name;

        public string SQLName
        {
            get
            {
                if (!m_name.StartsWith("["))
                {
                    return "[" + m_name + "]";
                }

                return m_name;
            }
        }

        public bool Nullable { get; set; } = true;
        public bool AutoIndex;
        public Type ColumnType;
        public int ColumnMaxLength = -1; // Sql default for unlimited columns
    }
}