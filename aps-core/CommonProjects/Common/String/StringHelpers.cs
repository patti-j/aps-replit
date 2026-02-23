using System.Text.RegularExpressions;

namespace PT.Common;

public class StringHelpers
{
    /// <summary>
    /// Whether a string is equal to any of the other string parameters.
    /// For example StringEqualsAnyOf("test", "t", "te", "test", "test 2")
    /// would return true because the string is equal to the 4th parameter.
    /// </summary>
    /// <param name="a_value">The string to test.</param>
    /// <param name="a_containStrings">Any number of strings to test for equality.</param>
    /// <returns></returns>
    public static bool StringEqualsAnyOf(string a_value, params string[] a_containStrings)
    {
        for (int i = 0; i < a_containStrings.Length; ++i)
        {
            if (string.Compare(a_containStrings[i], a_value, true) == 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Whether a string contains any of the other string parameters.
    /// For instance ContainsAnyString("The quick brown", "test", "The", "a")
    /// would return true because the first parmeter contains "The".
    /// </summary>
    /// <param name="a_value"></param>
    /// <param name="a_containStrings"></param>
    /// <returns></returns>
    public static bool ContainsAnyString(string a_value, params string[] a_containStrings)
    {
        for (int i = 0; i < a_containStrings.Length; ++i)
        {
            if (a_value.Contains(a_containStrings[i]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Converts camelCaseString into Title Case String. Works with acronyms and numbers.
    /// </summary>
    /// <param name="a_camelCaseString"></param>
    /// <returns></returns>
    public static string ToTitleCase(string a_camelCaseString)
    {
        if (string.IsNullOrEmpty(a_camelCaseString))
        {
            return string.Empty;
        }

        string[] splitString = Regex.Matches(a_camelCaseString, "([A-Z]+(?![a-z])|[A-Z][a-z]+|[0-9]+|[a-z]+)")
                                    .OfType<Match>()
                                    .Select(m => m.Value)
                                    .ToArray();

        return string.Join(" ", splitString);
    }
}