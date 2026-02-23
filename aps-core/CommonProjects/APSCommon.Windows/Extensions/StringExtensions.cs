namespace PT.APSCommon.Windows.Extensions;

public static class StringExtensions
{
    //Opens a webpage to the localized help url
    public static void ShowHelp(this string a_string)
    {
        LocalizerUIHelper.ShowHelp(a_string);
    }
}