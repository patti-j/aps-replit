using System.Text;

namespace PT.Common.Text;

/// <summary>
/// Summary description for Text.
/// </summary>
public class TextUtil
{
    /// <summary>
    /// Returns the length of the string. If the string is null returns 0.
    /// </summary>
    public static int Length(string a_str)
    {
        if (a_str != null)
        {
            return a_str.Length;
        }

        return 0;
    }

    public static string NonNullString(string a_inString)
    {
        if (a_inString != null)
        {
            return a_inString;
        }

        return "";
    }

    /// <summary>
    /// Just like string's IndexOf except case insensitive.
    /// </summary>
    /// <param name="a_text">The text to search.</param>
    /// <param name="a_searchString">The text to search for.</param>
    /// <returns>0 or greater if</returns>
    public static int IndexOfCaseInsensitive(string a_text, string a_searchString)
    {
        string upperSearchString = a_searchString.ToUpper();
        string upperText = a_text.ToUpper();

        return upperText.IndexOf(upperSearchString);
    }

    /// <summary>
    /// Similar to string.compare except white space is trimmed from the front and rear of each string.
    /// </summary>
    /// <param name="a_str1"></param>
    /// <param name="a_str2"></param>
    /// <param name="a_ignoreCase"></param>
    /// <returns></returns>
    public static bool EqualsNoWS(string a_str1, string a_str2, bool a_ignoreCase)
    {
        if (a_str1 == null && a_str2 == null)
        {
            return true;
        }

        if (a_str1 == null)
        {
            return false;
        }

        if (a_str2 == null)
        {
            return false;
        }

        return string.Compare(a_str1.Trim(), a_str2.Trim(), a_ignoreCase) == 0;
    }

    /// <summary>
    /// Returns true of both strings lengths are greater than 0 and are equal.
    /// </summary>
    /// <param name="a_a"></param>
    /// <param name="a_b"></param>
    /// <returns></returns>
    public static bool EqualAndLengthsGreaterThanZero(string a_a, string a_b)
    {
        if (a_a != null && a_b != null)
        {
            if (a_a.Length > 0 && a_b.Length > 0)
            {
                return a_a == a_b;
            }
        }

        return false;
    }

    /// <summary>
    /// Use StringBuilder.AppendFormat with NewLine added.
    /// </summary>
    /// <param name="a_sb"></param>
    /// <param name="a_format"></param>
    /// <param name="a_parms"></param>
    public static void AppendFormatLine(StringBuilder a_sb, string a_format, params object[] a_parms)
    {
        a_sb.AppendFormat(a_format, a_parms);
        a_sb.AppendFormat(Environment.NewLine);
    }
}