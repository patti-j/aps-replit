namespace PT.Common.Localization;

public class SafeString
{
    /// <summary>
    /// Returns a formatted string if the arguments sent match the curly braces in the format.
    /// If an error is encountered then the original format string is returned along with an error message embedded in the result string.
    /// This is used to catch error caused by translations mistakes in the curly braces.
    /// </summary>
    /// <param name="a_stringToFormat"></param>
    /// <param name="a_args"></param>
    /// <returns></returns>
    public static string Format(string a_stringToFormat, params object[] a_args)
    {
        if (a_args == null)
        {
            return a_stringToFormat;
        }

        try
        {
            return string.Format(a_stringToFormat, a_args);
        }
        catch
        {
            int argCount = a_args.Length;
            return string.Format("{0} {1} {2}", a_stringToFormat, Localizer.GetString("0001"), argCount);
        }
    }
}