namespace PT.Common;

public struct LookUpValueStruct
{
    public static LookUpValueStruct EmptyLookUpValue;

    public LookUpValueStruct()
    {
        Value = null;
        ValueFound = false;
    }

    /// <summary>
    /// Use this constructor to specify that a value was found. The value could be null
    /// </summary>
    public LookUpValueStruct(object a_value)
    {
        Value = a_value;
        ValueFound = true;
    }

    /// <summary>
    /// Use this constructor to specify that a value was found, but the value was null
    /// </summary>
    public LookUpValueStruct(bool a_nullValue)
    {
        Value = null;
        ValueFound = a_nullValue;
    }

    public readonly object Value;
    public readonly bool ValueFound;
}