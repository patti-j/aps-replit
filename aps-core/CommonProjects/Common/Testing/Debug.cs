namespace PT.Common.Testing;

public class Debug
{
    /// <summary>
    /// When this value is set to true the system will detect duplicate serializations of a single
    /// object. An object should only ever be serialized 1 time when the system is being saved.
    /// You should only run this debugging feature with small amounts of data because it is
    /// very CPU intensive.
    /// </summary>
    public static readonly bool UsePTSerializationDuplicateErrorChecking = false;

    /// <summary>
    /// Checks that the specifiedy Type contains a Property with the specified name.
    /// </summary>
    public static void ValidatePropertyConstant(Type type, string propertyNameToValidate)
    {
        try
        {
            type.GetProperty(propertyNameToValidate);
        }
        catch
        {
            throw new CommonException(string.Format("Type {0} does not contain property {1}.", type.Name, propertyNameToValidate));
        }
    }
}