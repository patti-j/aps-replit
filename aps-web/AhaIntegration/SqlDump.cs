using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Data.SqlClient;
using System.Data.SqlClient;
using SqlConnection = System.Data.SqlClient.SqlConnection;
using SqlCommand = System.Data.SqlClient.SqlCommand;
using SqlBulkCopy = System.Data.SqlClient.SqlBulkCopy;

namespace AhaIntegration
{
    public sealed class EncodingJsonConverter : JsonConverter<System.Text.Encoding>
    {
        public override System.Text.Encoding Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Decoding Encoding from JSON is not supported.");
        public override void Write(Utf8JsonWriter writer, System.Text.Encoding value, JsonSerializerOptions options)
            => writer.WriteStringValue(value?.WebName);
    }

    public static class SqlDump
    {
        public static async Task DumpAsync<T>(IEnumerable<T> a_rows, string a_connectionString, string a_tableName, string a_schema)
        {
            List<T> list = a_rows?.ToList() ?? new List<T>();

            DataTable dt = BuildDataTable(list, a_tableName);

            string ddl = CreateSqlTable(dt, a_schema);

            await using SqlConnection conn = new SqlConnection(a_connectionString);
            await conn.OpenAsync();

            await using (SqlCommand cmd = new SqlCommand(ddl, conn))
                await cmd.ExecuteNonQueryAsync();

            await InsertSqlData(dt, conn, a_schema);
        }

        // ----------------- Build DataTable -----------------
        private static DataTable BuildDataTable<T>(IEnumerable<T> a_rows, string a_tableName)
        {
            Type type = typeof(T);
            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.GetIndexParameters().Length == 0)
                            .ToArray();

            DataTable table = new DataTable(a_tableName);

            foreach (PropertyInfo p in props)
            {
                Type pt = p.PropertyType;
                Type u = Nullable.GetUnderlyingType(pt) ?? pt;

                Type colType;
                if (Nullable.GetUnderlyingType(pt) != null)
                    colType = typeof(string);
                else if (u.IsEnum)
                    colType = typeof(string);
                else if (IsCollection(u) || IsComplex(u) || u.Namespace?.StartsWith("System.Text") == true)
                    colType = typeof(string);
                else
                    colType = u;

                DataColumn col = table.Columns.Add(p.Name, colType);
                col.AllowDBNull = true;
            }

            JsonSerializerOptions jsonOpts = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            jsonOpts.Converters.Add(new EncodingJsonConverter());

            DefaultJsonTypeInfoResolver resolver = new DefaultJsonTypeInfoResolver();
            resolver.Modifiers.Add(typeInfo =>
            {
                if (typeInfo.Kind != JsonTypeInfoKind.Object) return;

                List<JsonPropertyInfo> toRemove = new List<JsonPropertyInfo>();
                foreach (JsonPropertyInfo prop in typeInfo.Properties)
                {
                    Type t = prop.PropertyType;
                    string ns = t.Namespace ?? string.Empty;

                    if (ns.StartsWith("System.Text", StringComparison.Ordinal) ||
                        t.FullName!.StartsWith("System.ReadOnlySpan", StringComparison.Ordinal))
                    {
                        toRemove.Add(prop);
                    }
                }

                foreach (JsonPropertyInfo pinfo in toRemove)
                    typeInfo.Properties.Remove(pinfo);
            });
            jsonOpts.TypeInfoResolver = resolver;

            // Insert rows
            foreach (T item in a_rows)
            {
                DataRow dr = table.NewRow();

                foreach (PropertyInfo p in props)
                {
                    object val = p.GetValue(item);
                    if (val == null) { dr[p.Name] = DBNull.Value; continue; }

                    Type pt = p.PropertyType;
                    Type u = Nullable.GetUnderlyingType(pt) ?? pt;

                    if (Nullable.GetUnderlyingType(pt) != null)
                    {
                        dr[p.Name] = val.ToString();
                    }
                    else if (u.IsEnum)
                    {
                        dr[p.Name] = val.ToString();
                    }
                    else if (IsCollection(u) || IsComplex(u) || u.Namespace?.StartsWith("System.Text") == true)
                    {
                        if (val is System.Text.Encoding enc)
                        {
                            dr[p.Name] = enc.WebName;
                        }
                        else if (val.GetType().Namespace?.StartsWith("System.Text") == true)
                        {
                            dr[p.Name] = val.ToString() ?? string.Empty;
                        }
                        else
                        {
                            dr[p.Name] = SafeSerializeToString(val, jsonOpts);
                        }
                    }
                    else
                    {
                        dr[p.Name] = val;
                    }
                }

                table.Rows.Add(dr);
            }

            return table;
        }

        // ----------------- Create Table -----------------
        private static string CreateSqlTable(DataTable a_dt, string a_schema)
        {
            List<string> cols = new List<string>();
            foreach (DataColumn col in a_dt.Columns)
            {
                string sqlType = GetSqlType(col.DataType);
                cols.Add($"[{col.ColumnName}] {sqlType} NULL");
            }

            string fullName = $"[{a_schema}].[{a_dt.TableName}]";
            return $@"IF OBJECT_ID('{fullName}','U') IS NOT NULL DROP TABLE {fullName};
            CREATE TABLE {fullName} ( {string.Join(",\n    ", cols)} );";
        }

        private static string GetSqlType(Type a_type)
        {
            a_type = Nullable.GetUnderlyingType(a_type) ?? a_type;

            if (a_type == typeof(string)) return "NVARCHAR(MAX)";
            if (a_type == typeof(int)) return "INT";
            if (a_type == typeof(long)) return "BIGINT";
            if (a_type == typeof(short)) return "SMALLINT";
            if (a_type == typeof(bool)) return "BIT";
            if (a_type == typeof(DateTime)) return "DATETIME2";
            if (a_type == typeof(DateTimeOffset)) return "DATETIMEOFFSET";
            if (a_type == typeof(decimal)) return "DECIMAL(38,10)";
            if (a_type == typeof(double)) return "FLOAT";
            if (a_type == typeof(float)) return "REAL";
            if (a_type == typeof(Guid)) return "UNIQUEIDENTIFIER";

            return "NVARCHAR(MAX)";
        }

        // ----------------- Bulk Insert -----------------
        private static async Task InsertSqlData(DataTable a_dt, SqlConnection a_conn, string a_schema)
        {
            using SqlBulkCopy bulk = new SqlBulkCopy(a_conn)
            {
                DestinationTableName = $"[{a_schema}].[{a_dt.TableName}]",
                BulkCopyTimeout = 0
            };

            foreach (DataColumn col in a_dt.Columns)
            {
                bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);
            }

            await bulk.WriteToServerAsync(a_dt);
        }

        private static bool IsCollection(Type a_t) =>
            a_t != typeof(string) && (a_t.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(a_t));

        private static bool IsComplex(Type a_t)
        {
            a_t = Nullable.GetUnderlyingType(a_t) ?? a_t;

            if (a_t.IsPrimitive || a_t.IsEnum ||
                a_t == typeof(string) ||
                a_t == typeof(decimal) ||
                a_t == typeof(DateTime) ||
                a_t == typeof(DateTimeOffset) ||
                a_t == typeof(Guid) ||
                a_t == typeof(bool))
            {
                return false;
            }

            return true;
        }

        private static string SafeSerializeToString(object a_value, JsonSerializerOptions a_jsonOpts)
        {
            if (a_value is System.Text.Encoding enc)
                return enc.WebName;

            string ns = a_value.GetType().Namespace ?? string.Empty;
            if (ns.StartsWith("System.Text", StringComparison.Ordinal))
                return a_value.ToString() ?? string.Empty;

            try
            {
                return JsonSerializer.Serialize(a_value, a_jsonOpts);
            }
            catch
            {
                return a_value.ToString() ?? string.Empty;
            }
        }
    }
}
