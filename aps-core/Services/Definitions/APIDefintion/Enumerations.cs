namespace PT.APIDefinitions
{
    public enum EPropertyDataType
    {
        Boolean,
        Byte,
        DateTime,
        Decimal,
        Double,
        Short,
        Int,
        Long,
        String,
        Unspecified = 999
    }

    public enum EPropertySourceOption
    {
        FromTable = 0,
        FixedValue = 1,
        KeepValue = 2,
        ClearValue = 3
    }

    public static class EPropertyDataTypeExtensions
    {
        public static string ToSqlType(this EPropertyDataType a_option)
        {
            switch (a_option)
            {
                case EPropertyDataType.Boolean:
                    return "bit";
                    break;
                case EPropertyDataType.Byte:
                    return "tinyint";
                    break;
                case EPropertyDataType.DateTime:
                    return "datetime";
                    break;
                case EPropertyDataType.Decimal:
                    return "decimal";
                    break;
                case EPropertyDataType.Double:
                    return "float";
                    break;
                case EPropertyDataType.Short:
                    return "smallint";
                    break;
                case EPropertyDataType.Int:
                    return "int";
                    break;
                case EPropertyDataType.Long:
                    return "bigint";
                    break;
                case EPropertyDataType.String:
                    return "nvarchar(max)";
                    break;
                case EPropertyDataType.Unspecified:
                    //TODO: what should be returned here if anything?
                    throw new Exception("Attempted to get sql type of an unknown data type"); 
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(a_option), a_option, null);
            }
        }
    }

}
