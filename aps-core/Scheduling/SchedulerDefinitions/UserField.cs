using System.Text;

using PT.APSCommon;
using PT.Common.Extensions;
using PT.Common.Localization;

namespace PT.SchedulerDefinitions;

public class UserField : IPTSerializable, IEquatable<UserField>
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 653;

    public UserField(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12531)
        {
            a_reader.Read(out m_externalId);
            a_reader.Read(out bool haveDataValue); //can be null
            if (haveDataValue)
            {
                a_reader.ReadBoxedPrimitiveAndCommonSystemStructs(out m_dataValue);
            }
        }
        else if (a_reader.VersionNumber >= 12503)
        {
            a_reader.Read(out m_externalId);
            a_reader.Read(out bool haveDataValue); //can be null
            if (haveDataValue)
            {
                a_reader.ReadBoxedPrimitiveAndCommonSystemStructs(out m_dataValue);
            }

            a_reader.Read(out string dataValueString);
        }
        else
        {
            bool display = true;
            string oldName = "Default";
            if (a_reader.VersionNumber >= 720)
            {
                a_reader.Read(out oldName);
                a_reader.Read(out bool haveDataValue); //can be null
                if (haveDataValue)
                {
                    a_reader.ReadBoxedPrimitiveAndCommonSystemStructs(out m_dataValue);
                }

                a_reader.Read(out int udfType);
                m_udfDataType = (UDFTypes)udfType;

                BoolVector32 boolVector32 = new BoolVector32(a_reader);
                display = boolVector32[0];
            }
            else if (a_reader.VersionNumber >= 380)
            {
                a_reader.Read(out oldName);
                a_reader.Read(out bool haveDataValue); //can be null
                if (haveDataValue)
                {
                    a_reader.ReadBoxedPrimitiveAndCommonSystemStructs(out m_dataValue);
                }

                a_reader.Read(out int udfType);
                m_udfDataType = (UDFTypes)udfType;
            }
            else if (a_reader.VersionNumber >= 333)
            {
                a_reader.Read(out oldName);
                a_reader.Read(out bool haveDataValue); //can be null
                if (haveDataValue)
                {
                    a_reader.ReadBoxedPrimitiveAndCommonSystemStructs(out m_dataValue);
                }

                a_reader.Read(out string typeName);
                Type dataType = Type.GetType(typeName);
                m_udfDataType = GetTypeFromOldType(dataType);
            }
            else if (a_reader.VersionNumber >= 331)
            {
                a_reader.Read(out oldName);
                a_reader.Read(out bool haveDataValue); //can be null
                if (haveDataValue)
                {
                    a_reader.ReadBoxedPrimitiveAndCommonSystemStructs(out m_dataValue);
                    Type dataType = m_dataValue.GetType();
                    m_udfDataType = GetTypeFromOldType(dataType);
                }
                else
                {
                    m_udfDataType = UDFTypes.String;
                }
            }
            else if (a_reader.VersionNumber >= 1)
            {
                a_reader.Read(out oldName);
                a_reader.ReadBoxedPrimitiveAndCommonSystemStructs(out m_dataValue);
                Type dataType = m_dataValue.GetType();
                m_udfDataType = GetTypeFromOldType(dataType);
            }

            m_externalId = oldName;
            if (!display)
            {
                m_externalId += " (Hidden)";
            }
        }
    }

    public virtual void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        a_writer.Write(m_externalId);
        a_writer.Write(m_dataValue != null);
        if (m_dataValue != null)
        {
            a_writer.WriteBoxedPrimitiveAndCommonSystemStructs(m_dataValue);
        }
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public UserField(string a_externalId, object a_dataValue, UDFTypes a_udfType)
    {
        ExternalId = a_externalId;
        m_udfDataType = a_udfType;

        if (a_dataValue != null && a_dataValue.GetType() != GetTypeFromUDFType(a_udfType))
        {
            throw new APSCommon.PTValidationException("2039", new object[] { a_externalId, a_dataValue.ToString(), a_udfType.ToString() });
        }
        m_dataValue = a_dataValue;
    }

    public static Type GetTypeFromUDFType(UDFTypes a_udfType)
    {
        switch (a_udfType)
        {
            case UDFTypes.String:
            case UDFTypes.Hyperlink:
                return typeof(string);
            case UDFTypes.Integer:
                return typeof(int);
            case UDFTypes.Long:
                return typeof(long);
            case UDFTypes.Decimal:
                return typeof(decimal);
            case UDFTypes.DateTime:
                return typeof(DateTime);
            case UDFTypes.Boolean:
                return typeof(bool);
            case UDFTypes.Double:

                return typeof(double);
            default:
                return typeof(TimeSpan);
        }
    }

    public static UDFTypes GetTypeFromOldType(Type a_oldType)
    {
        if (a_oldType == typeof(string))
        {
            return UDFTypes.String;
        }

        if (a_oldType == typeof(int))
        {
            return UDFTypes.Integer;
        }

        if (a_oldType == typeof(long))
        {
            return UDFTypes.Long;
        }

        if (a_oldType == typeof(decimal))
        {
            return UDFTypes.Decimal;
        }

        if (a_oldType == typeof(DateTime))
        {
            return UDFTypes.DateTime;
        }

        if (a_oldType == typeof(bool))
        {
            return UDFTypes.Boolean;
        }

        if (a_oldType == typeof(double))
        {
            return UDFTypes.Double;
        }

        if (a_oldType == typeof(TimeSpan))
        {
            return UDFTypes.TimeSpan;
        }

        throw new APSCommon.PTValidationException("2038", new object[] { a_oldType });
    }

    private string m_externalId;
    public string ExternalId
    {
        get { return m_externalId; }
        set { m_externalId = value; }
    }

    private object m_dataValue;
    public object DataValue
    {
        get { return m_dataValue; }
        set
        {
            if (value != null && 
                (value.GetType() != typeof(string)
                && value.GetType() != typeof(int)
                && value.GetType() != typeof(long)
                && value.GetType() != typeof(double)
                && value.GetType() != typeof(decimal)
                && value.GetType() != typeof(bool)
                && value.GetType() != typeof(DateTime)
                && value.GetType() != typeof(TimeSpan)))
            {
                throw new APSCommon.PTValidationException("2040");
            }

            m_dataValue = value;
        }
    }
    
    public enum UDFTypes
    {
        String,
        Hyperlink,
        Integer,
        Double,
        Long,
        Decimal,
        Boolean,
        DateTime,
        TimeSpan
    }

    public enum EUDFObjectType
    {
        Users,
        Plants,
        Departments,
        Resources,
        Cells,
        CapacityIntervals,
        ProductRules,
        ResourceConnectors,
        Items,
        Warehouses,
        SalesOrders,
        Forecasts,
        PurchasesToStock,
        TransferOrders,
        Jobs,
        ManufacturingOrders,
        ResourceOperations,
        Customers,
        Lots,
        StorageArea,
        ItemStorage,
        StorageAreaConnectors
    }

    //UDF TODO: Check if this can be removed since it will be on UserFieldDefinition class
    private UDFTypes m_udfDataType;
    public UDFTypes UDFDataType
    {
        get => m_udfDataType;
        set => m_udfDataType = value;
    }

    public Type DataType => GetTypeFromUDFType(UDFDataType);

    public static string UserFieldSuffix = " UDF";

    /// <summary>
    /// Separates the Name, DataType, Value in a single User Field.
    /// </summary>
    public static string USER_FIELDS_SEP_CHAR => "~";

    /// <summary>
    /// Separates individual User Fields.
    /// </summary>
    public static string USER_FIELDS_INTERFIELD_SEP_CHAR => ";";

    /// <summary>
    /// Get the string that is used to pass the user fields in a transmission.
    /// </summary>
    public static string GetUserFieldImportString(string a_fieldName, object a_value)
    {
        return string.Format("{1}{0}{2}", USER_FIELDS_SEP_CHAR, a_fieldName, a_value == null ? "" : a_value.ToString());
    }

    /// <summary>
    /// Get the string that is used to pass the user fields in a transmission.
    /// </summary>
    public string GetUserFieldImportString()
    {
        return GetUserFieldImportString(ExternalId, DataValue);
    }

    /// <summary>
    /// Read the UDF value from a serialized byte[]. The data type will be determined by the UDF type
    /// </summary>
    public void SetDataValueFromSerializedData(byte[] a_value)
    {
        using BinaryMemoryReader reader = new (a_value);

        switch (UDFDataType)
        {
            case UDFTypes.String:
            case UDFTypes.Hyperlink:
                reader.Read(out string stringValue);
                DataValue = stringValue;
                break;
            case UDFTypes.Integer:
                reader.Read(out int intValue);
                DataValue = intValue;
                break;
            case UDFTypes.Long:
                reader.Read(out long longValue);
                DataValue = longValue;
                break;
            case UDFTypes.Double:
                reader.Read(out double doubleValue);
                DataValue = doubleValue;
                break;
            case UDFTypes.Decimal:
                reader.Read(out decimal decimalValue);
                DataValue = decimalValue;
                break;
            case UDFTypes.Boolean:
                reader.Read(out bool boolValue);
                DataValue = boolValue;
                break;
            case UDFTypes.DateTime:
                reader.Read(out DateTime dateValue);
                DataValue = dateValue;
                break;
            case UDFTypes.TimeSpan:
                reader.Read(out TimeSpan timespanValue);
                DataValue = timespanValue;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public bool Equals(UserField a_other)
    {
        if (ReferenceEquals(null, a_other))
        {
            return false;
        }

        if (ReferenceEquals(this, a_other))
        {
            return true;
        }

        return m_externalId == a_other.m_externalId && Equals(m_dataValue, a_other.m_dataValue) && m_udfDataType == a_other.m_udfDataType;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((UserField)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(m_externalId, m_dataValue, (int)m_udfDataType);
    }

    public bool Update(UserField a_userField)
    {
        bool changed = false;
        if (m_dataValue != a_userField.DataValue)
        {
            m_dataValue = a_userField.DataValue;
            changed = true;
        }
        
        
        if (m_udfDataType != a_userField.UDFDataType)
        {
            m_udfDataType = a_userField.UDFDataType;
            changed = true;
        }

        return changed;
    }
}

public class UserFieldList : IPTSerializable
{
    private string m_condensedString;

    #region IPTSerializable Members
    public const int UNIQUE_ID = 654;

    public UserFieldList(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12531)
        {
            a_reader.Read(out bool parsed);
            if (parsed)
            {
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    Add(new UserField(a_reader));
                }
            }
            else
            {
                a_reader.Read(out m_condensedString);
            }
        }
        else if (a_reader.VersionNumber >= 12503)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                UserField userField = new UserField(a_reader);
                // UDFS are broken here since they no longer serialize the string data
            }
        }
        else
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                UserField oldUserField = new UserField(a_reader);
                if (Contains(oldUserField.ExternalId))
                {
                    // There were multiple badly named UDFs, just ignore them
                    continue;
                }
                Add(oldUserField);
            }
        }
    }

    public virtual void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif

        if (!string.IsNullOrEmpty(m_condensedString))
        {
            a_writer.Write(false);
            a_writer.Write(m_condensedString);
        }
        else
        {
            a_writer.Write(true);
            a_writer.Write(Count);
            for (int i = 0; i < Count; i++)
            {
                this[i].Serialize(a_writer);
            }
        }
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public UserFieldList() { }

    /// <summary>
    /// Create the UserFields from an encoded string.
    /// The format is: FieldName~FieldValue
    /// User fields are separated by ';'
    /// Values defining each field are separated by '~'
    /// Specify the text 'NULL' (no quotes) to preserve values in previously created user defined fields.
    /// </summary>
    /// <example>Width~5.2</example>
    public UserFieldList(string a_condensedString)
    {
        m_condensedString = a_condensedString;
    }

    public bool Empty => Count == 0 && string.IsNullOrEmpty(m_condensedString);

    public void ParseUserFieldList(IUserFieldDefinitionManager a_definitionManager)
    {
        if (string.IsNullOrEmpty(m_condensedString))
        {
            return;
        }
        
        //Split the condensed string by the separator character ;
        char[] fieldSepChar = ";".ToCharArray();
        string[] fields = m_condensedString.Split(fieldSepChar, StringSplitOptions.RemoveEmptyEntries);

        //Return early if there are no user fields
        if (fields.Length <= 0)
        {
            return;
        }

        //Loop through the user fields
        for (int fieldI = 0; fieldI < fields.Length; fieldI++)
        {
            //Split each user field to get the field values
            string fieldStr = fields[fieldI];
            string[] fieldValues = fieldStr.Split(UserField.USER_FIELDS_SEP_CHAR.ToCharArray(), StringSplitOptions.None);

            //String can specify just name and type and omit value to preserve internal values
            if (string.IsNullOrEmpty(fieldValues[0]))
            {
                //The data field is empty, verify if is missing or just empty
                string[] fieldValuesNoEmpty = fieldStr.Split(UserField.USER_FIELDS_SEP_CHAR.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (fieldValuesNoEmpty.Length < fieldValues.Length)
                {
                    //The field was actually missing. 
                    fieldValues[1] = null;
                }
            }

            string externalId;
            string dataStr = null;

            if (fieldValues.Length == 2)
            {
                externalId = fieldValues[0];
                dataStr = fieldValues[1];
            }
            else if (fieldValues.Length == 1)
            {
                externalId = fieldValues[0];
            }
            else
            {
                //TODO: Add a new error code
                string badFieldMsg = Localizer.GetErrorString("3072");
                throw new APSCommon.PTValidationException(String.Format("{0}  Field string imported: '{1}'", badFieldMsg, fieldStr));
            }

            if (a_definitionManager.GetUdfDataType(externalId) is IUserFieldDefinition def)
            {
                ParseUserFieldDataValueString(dataStr, def, out object udfDataValue);
                if (udfDataValue != null)
                {
                    Add(new UserField(externalId, udfDataValue, def.UDFDataType));
                }
            }
            else
            {
                throw new PTValidationException(string.Format("There is no definition found for UserField with ExternalId '{0}'", externalId));
            }
        }

        m_condensedString = null;
    }

    private static void ParseUserFieldDataValueString(string a_dataStr, IUserFieldDefinition a_udfDef, out object o_dataValue)
    {
        o_dataValue = null;

        try
        {
            switch (a_udfDef.UDFDataType)
            {
                case UserField.UDFTypes.String:
                case UserField.UDFTypes.Hyperlink:
                    o_dataValue = a_dataStr;
                    break;
                case UserField.UDFTypes.Integer:
                    o_dataValue = int.Parse(a_dataStr);
                    break;
                case UserField.UDFTypes.Long:
                    o_dataValue = long.Parse(a_dataStr);
                    break;
                case UserField.UDFTypes.Double:
                    o_dataValue = double.Parse(a_dataStr);
                    break;
                case UserField.UDFTypes.Decimal:
                    o_dataValue = decimal.Parse(a_dataStr);
                    break;
                case UserField.UDFTypes.Boolean:
                    o_dataValue = bool.Parse(a_dataStr);
                    break;
                case UserField.UDFTypes.DateTime:
                    o_dataValue = DateTime.Parse(a_dataStr);
                    break;
                case UserField.UDFTypes.TimeSpan:
                    o_dataValue = TimeSpan.Parse(a_dataStr);
                    break;
                default:
                    throw new APSCommon.PTValidationException("3071", new object[] { a_udfDef.ExternalId });
            }
        }
        catch (Exception e)
        {
            throw new APSCommon.PTValidationException("2778", new object[] { a_udfDef.ExternalId, e.Message });
        }
    }
    
    private SortedList<string, UserField> m_userFields = new();
    public SortedList<string, UserField> UserFields => m_userFields;

    public int Count => m_userFields.Count;

    public void Add(UserField a_userField)
    {
        if (m_userFields.TryGetValue(a_userField.ExternalId, out UserField userField))
        {
            m_userFields[a_userField.ExternalId] = a_userField;
        }
        else
        {
            m_userFields.Add(a_userField.ExternalId, a_userField);
        }
    }

    public void Add(UserFieldList a_userFields)
    {
        if (a_userFields == null)
        {
            return;
        }

        for (int i = 0; i < a_userFields.Count; i++)
        {
            if (Contains(a_userFields[i].ExternalId))
            {
                continue;
            }

            m_userFields.Add(a_userFields[i].ExternalId, a_userFields[i]);
        }
    }

    public bool Remove(string a_name)
    {
        bool removed = false;
        if (m_userFields.ContainsKey(a_name))
        {
            removed = m_userFields.Remove(a_name);
        }

        return removed;
    }

    public void Clear()
    {
        m_userFields.Clear();
    }

    public UserField this[int a_index] => m_userFields.Values[a_index];

    public bool Contains(string a_fieldExternalId)
    {
        return m_userFields.ContainsKey(a_fieldExternalId);
    }

    public UserField Find(string a_externalId)
    {
        if (m_userFields.TryGetValue(a_externalId, out UserField udf))
        {
            return udf;
        }

        return null;
    }

    public UserField FindByColumnName(string a_columnFieldNameWithUdfSuffix)
    {
        if (m_userFields.TryGetValue(a_columnFieldNameWithUdfSuffix.Replace(UserField.UserFieldSuffix, ""), out UserField udf))
        {
            return udf;
        }

        return null;
    }

    /// <summary>
    /// Returns a string that can be used to import the User Field List.
    /// </summary>
    public string GetUserFieldImportString()
    {
        //TODO: Restore this when possible
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < Count; i++)
        {
            if (i > 0)
            {
                builder.Append(UserField.USER_FIELDS_INTERFIELD_SEP_CHAR);
            }

            string externalId = m_userFields.Keys[i];
            builder.Append(m_userFields[externalId].GetUserFieldImportString());
        }

        return builder.ToString();
    }

    public bool Update(UserFieldList a_userFieldList)
    {
        bool changed = false;

        //Validate that the imported value data type is the same as the udf def type.

        //Remove fields that are no longer in the collection
        for (var i = UserFields.Count - 1; i >= 0; i--)
        {
            UserField userField = UserFields.GetValueAtIndex(i);
            if (!a_userFieldList.Contains(userField.ExternalId))
            {
                UserFields.RemoveAt(i);
                changed = true;
            }
        }
        
        //Update or add existing UDFs
        for (var i = 0; i < a_userFieldList.Count; i++)
        {
            UserField userField = a_userFieldList[i];
            if (!UserFields.TryGetValue(userField.ExternalId, out UserField existingUserField))
            {
                UserField newUserField = new UserField(userField.ExternalId, userField.DataValue, userField.UDFDataType);
                UserFields.Add(newUserField.ExternalId, userField);
                changed = true;
            }
            else
            {
                changed = existingUserField.Update(userField);
            }
        }

        return changed;
    }
}

public interface IUserFieldDefinitionManager
{
    IUserFieldDefinition GetUdfDataType(string a_defExternalId);
}