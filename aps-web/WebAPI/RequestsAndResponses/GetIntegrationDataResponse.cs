using System.Data;

using WebAPI.Models.Integration;

namespace WebAPI.RequestsAndResponses;

public class GetIntegrationDataResponse
{
    public DBIntegrationDTO Integration { get; set; }
    public Dictionary<string, DataTableJson> TableData { get; set; } = new (); //maps table name to data in table
}




// \/ \/ \/ this will be in some common package later TODO move to common package when we have it 
public class DataTableJson
{
    /// <summary>
    /// Custom DataTable object used for JSON serializing/deserializing
    /// </summary>
    public DataTableJson() { }

    /// <summary>
    /// Construct custom table object that can be JSON serialized
    /// </summary>
    /// <param name="a_dataTable"></param>
    public void ConstructTable(DataTable a_dataTable)
    {
        //Generate Columns
        Columns = new List<DataColumnJson>(a_dataTable.Columns.Count);
        for (int i = 0; i < a_dataTable.Columns.Count; i++)
        {
            DataColumn col = a_dataTable.Columns[i];
            Columns.Add(new DataColumnJson
            {
                ColumnName = col.ColumnName,
                ColumnType = col.DataType.FullName
                // Type.GetType(string) will return null with just col.DataType.Name
            });
        }

        //Generate Rows
        DataRows = new List<DataRowJson>(a_dataTable.Rows.Count);
        foreach (DataRow row in a_dataTable.Rows)
        {
            DataRowJson dtJson = new (row.ItemArray.Length);
            dtJson.ConstructRow(row);
            DataRows.Add(dtJson);
        }
    }

    /// <summary>
    /// Generate a DataTable object from the custom JSON table
    /// </summary>
    /// <returns></returns>
    public DataTable GetDataTable(out TableDataConversionErrorMessage a_tableErrorMessage)
    {
        DataTable dt = new ();
        a_tableErrorMessage = new TableDataConversionErrorMessage();
        for (int i = 0; i < Columns.Count; i++)
        {
            DataColumnJson dataColumnJson = Columns[i];
            dt.Columns.Add(new DataColumn(dataColumnJson.ColumnName, Type.GetType(dataColumnJson.ColumnType) ?? typeof(string)));
        }

        for (int i = 0; i < DataRows.Count; i++)
        {
            DataRowJson rowJson = DataRows[i];
            DataRow dataRow = dt.NewRow();
            TableDataConversionErrorMessage.RowDataConversionErrorMessage rowErrorMessage = rowJson.PopulateDataRow(dataRow, dt.Columns, i);
            if (!rowErrorMessage.IsEmpty)
            {
                a_tableErrorMessage.RowMessages.Add(rowErrorMessage);
            }

            dt.Rows.Add(dataRow);
        }

        return dt;
    }

    public List<DataColumnJson> Columns { get; set; }
    public List<DataRowJson> DataRows { get; set; }
}

public class DataRowJson
{
    /// <summary>
    /// Custom DataRow object that can be JSON serialized/deserialized
    /// </summary>
    public DataRowJson() { }

    public DataRowJson(int a_count)
    {
        Values = new string[a_count];
        Errors = new string[a_count];
    }

    /// <summary>
    /// Construct a custom DataRow object that can be JSON serialized/deserialized
    /// </summary>
    /// <param name="a_dataRow"></param>
    public void ConstructRow(DataRow a_dataRow)
    {
        for (int i = 0; i < a_dataRow.Table.Columns.Count; i++)
        {
            object rowItem = a_dataRow.ItemArray[i];
            if (rowItem?.GetType() == typeof(DBNull))
            {
                Values[i] = null;
            }
            else
            {
                Values[i] = Convert.ToString(rowItem);
                if (a_dataRow.HasErrors)
                {
                    Errors[i] = a_dataRow.GetColumnError(i);
                }
            }
        }
    }

    /// <summary>
    /// Populate DataRow from the custom JSON row. We had to use strings because objects
    /// aren't serializable so all the values of Values are typeof(string). If the stored
    /// value cannot be parsed into a valid value for the type, then it is converted and
    /// </summary>
    /// <param name="a_dataRow">The row being populated</param>
    /// <param name="a_columns">
    /// The columns of the table being populated, used to determine
    /// the type to convert the string into
    /// </param>
    /// <param name="a_rowIndex"></param>
    public TableDataConversionErrorMessage.RowDataConversionErrorMessage PopulateDataRow(DataRow a_dataRow, DataColumnCollection a_columns, int a_rowIndex)
    {
        TableDataConversionErrorMessage.RowDataConversionErrorMessage rowErrorMessage = new (a_rowIndex);
        for (int i = 0; i < Values.Length; i++)
        {
            #region "Parsing Values for the Row"
            bool unableToParseString = false;
            if (Values[i] == null)
            {
                a_dataRow[i] = DBNull.Value;
            }
            else if (a_columns[i].DataType == typeof(bool))
            {
                if (bool.TryParse(Values[i], out bool boolean))
                {
                    a_dataRow[i] = boolean;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = false;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(byte))
            {
                if (byte.TryParse(Values[i], out byte aByte))
                {
                    a_dataRow[i] = aByte;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = byte.MinValue;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(char))
            {
                if (char.TryParse(Values[i], out char c))
                {
                    a_dataRow[i] = c;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = char.MinValue;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(DateTime))
            {
                if (DateTime.TryParse(Values[i], out DateTime dateTime))
                {
                    a_dataRow[i] = dateTime;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = DateTime.MinValue;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(DateTimeOffset))
            {
                if (DateTimeOffset.TryParse(Values[i], out DateTimeOffset dateTimeOffset))
                {
                    a_dataRow[i] = dateTimeOffset;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = DateTimeOffset.MinValue;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(decimal))
            {
                if (decimal.TryParse(Values[i], out decimal dec))
                {
                    a_dataRow[i] = dec;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = 0.0;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(double))
            {
                if (double.TryParse(Values[i], out double dbl))
                {
                    a_dataRow[i] = dbl;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = 0.0;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(Guid))
            {
                if (Guid.TryParse(Values[i], out Guid guid))
                {
                    a_dataRow[i] = guid;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = Guid.Empty;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(short))
            {
                if (short.TryParse(Values[i], out short int16))
                {
                    a_dataRow[i] = int16;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = 0;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(int))
            {
                if (int.TryParse(Values[i], out int int32))
                {
                    a_dataRow[i] = int32;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = 0;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(long))
            {
                if (long.TryParse(Values[i], out long int64))
                {
                    a_dataRow[i] = int64;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = 0;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(object))
            {
                if (a_columns[i].AllowDBNull)
                {
                    a_dataRow[i] = DBNull.Value;
                }

                // I think the dt.NewRow call that should be creating this row will
                // handle setting a non-error throwing value here
                unableToParseString = true;
            }
            else if (a_columns[i].DataType == typeof(sbyte))
            {
                if (sbyte.TryParse(Values[i], out sbyte sByte))
                {
                    a_dataRow[i] = sByte;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = 0;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(float))
            {
                if (float.TryParse(Values[i], out float single))
                {
                    a_dataRow[i] = single;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = 0.0;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(string))
            {
                a_dataRow[i] = Values[i];
            }
            else if (a_columns[i].DataType == typeof(TimeSpan))
            {
                if (TimeSpan.TryParse(Values[i], out TimeSpan timeSpan))
                {
                    a_dataRow[i] = timeSpan;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = TimeSpan.MinValue;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(ushort))
            {
                if (ushort.TryParse(Values[i], out ushort uInt16))
                {
                    a_dataRow[i] = uInt16;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = ushort.MinValue;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(uint))
            {
                if (uint.TryParse(Values[i], out uint uInt32))
                {
                    a_dataRow[i] = uInt32;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = uint.MinValue;
                    }

                    unableToParseString = true;
                }
            }
            else if (a_columns[i].DataType == typeof(ulong))
            {
                if (ulong.TryParse(Values[i], out ulong uInt64))
                {
                    a_dataRow[i] = uInt64;
                }
                else
                {
                    if (a_columns[i].AllowDBNull)
                    {
                        a_dataRow[i] = DBNull.Value;
                    }
                    else
                    {
                        a_dataRow[i] = ulong.MinValue;
                    }

                    unableToParseString = true;
                }
            }
            #endregion

            else
            {
                const string c_stringToFillForSupportedTypes = "Unsupported data type. Could not convert {0} into a {1} at column {2}. ";
                // Will end up with a 'localized' $"Unsupported data type. Could not convert {Value} into a {TypeName} at column {Caption}"
                TableDataConversionErrorMessage.ColumnDataConversionErrorMessage columnMessage = new (c_stringToFillForSupportedTypes, Values[i], a_columns[i].DataType.ToString(), a_columns[i].Caption);
                rowErrorMessage.ColumnMessages.Add(columnMessage);
            }
            
            a_dataRow.SetColumnError(i, Errors[i]);

            if (unableToParseString)
            {
                const string c_stringToFillForParsingError = "Could not convert '{0}' into a {1} at column {2}. ";
                TableDataConversionErrorMessage.ColumnDataConversionErrorMessage columnMessage = new (c_stringToFillForParsingError, Values[i], a_columns[i].DataType.ToString(), a_columns[i].Caption);
                rowErrorMessage.ColumnMessages.Add(columnMessage);
            }
        }

        return rowErrorMessage;
    }

    public string[] Values { get; set; }
    public string[] Errors { get; set; }
}

public class DataColumnJson
{
    /// <summary>
    /// Custom DataColumn object that can be JSON serialized/deserialized
    /// </summary>
    public DataColumnJson() { }

    public string ColumnName { get; set; }
    public string ColumnType { get; set; }
}

/// <summary>
/// DataTableJson.cs does not have access to the various PT localizers so
/// various conversion error messages could not be localized in this file.
/// This class is just used to pass the information back to the code that constructs
/// the table in a format that can be localized using the same entry
/// in the dictionary
/// </summary>
public class TableDataConversionErrorMessage
{
    public TableDataConversionErrorMessage()
    {
        RowMessages = new List<RowDataConversionErrorMessage>();
    }

    public IList<RowDataConversionErrorMessage> RowMessages;
    public bool IsEmpty => RowMessages.Count == 0;

    public class RowDataConversionErrorMessage
    {
        public RowDataConversionErrorMessage(int a_rowIndex)
        {
            RowIndex = a_rowIndex;
            ColumnMessages = new List<ColumnDataConversionErrorMessage>();
        }

        public int RowIndex;
        public IList<ColumnDataConversionErrorMessage> ColumnMessages;
        public bool IsEmpty => ColumnMessages.Count == 0;
    }

    /// <summary>
    /// </summary>
    public class ColumnDataConversionErrorMessage
    {
        public ColumnDataConversionErrorMessage(string a_stringToFill, string a_value, string a_typeName, string a_columnCaption)
        {
            StringToFill = a_stringToFill;
            TypeName = a_typeName;
            ColumnCaption = a_columnCaption;
            Value = a_value;
        }

        public string StringToFill;
        public string TypeName;
        public string ColumnCaption;
        public string Value;
    }
}