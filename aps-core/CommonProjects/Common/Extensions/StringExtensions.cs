using PT.Common.Localization;
using System.Collections;

namespace PT.Common;

public static class StringExtensions
{
    public static string Replace(this string a_s, char a_oldChar, char a_newChar, int a_length)
    {
        string fragment = a_s.Substring(0, a_length);
        fragment = fragment.Replace(a_oldChar, a_newChar);
        string updatedString = fragment + a_s.Substring(a_length);
        return updatedString;
    }

    public static string Pad(this string a_s, int a_length = 1, char a_padChar = ' ')
    {
        if (a_length < 1)
        {
            return a_s;
        }

        return a_s.PadLeft(a_length, a_padChar).PadRight(a_length, a_padChar);
    }

    /// <summary>
    /// Surrounds the string with double quotes.
    /// Redundant quotes are removed.
    /// </summary>
    /// <returns></returns>
    public static string Quotation(this string a_string)
    {
        if (a_string == null)
        {
            return "\"\"";
        }

        return $"\"{a_string.Trim('\"')}\"";
    }

    /// <summary>
    /// Surrounds the string with single quotes.
    /// Redundant quotes are removed.
    /// </summary>
    /// <returns></returns>
    public static string QuotationSingle(this string a_string)
    {
        if (a_string == null)
        {
            return "''";
        }

        return $"'{a_string.Trim('\'')}'";
    }

    /// <summary>
    /// Trims quotation marks from the string
    /// </summary>
    /// <returns></returns>
    public static string RemoveQuotation(this string a_string)
    {
        if (a_string == null)
        {
            return string.Empty;
        }

        return a_string.Trim('\"');
    }

    //Localization Extension for String
    public static string Localize(this string a_string)
    {
        return Localizer.GetString(a_string);
    }

    public static string Localize(this Enum a_enum)
    {
        return Localizer.GetString(a_enum.ToString());
    }
}