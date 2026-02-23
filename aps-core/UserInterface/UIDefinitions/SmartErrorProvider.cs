using System.Windows.Forms;

namespace PT.UIDefinitions;

/// <summary>
/// Summary description for SmartErrorProvider.
/// </summary>
public class SmartErrorProvider : ErrorProvider
{
    public void SetWrappableError(Control control, string error)
    {
        SetError(control, WrapString(error));
    }

    private const int LINE_LENGTH = 100;
    private static readonly string RETURN_CHAR = Environment.NewLine;

    private static string WrapString(string input)
    {
        if (input.Length <= LINE_LENGTH)
        {
            return input;
        }

        int cur_char = 0;
        string returnString = "";
        while (cur_char < input.Length)
        {
            int GET_LENGTH = Math.Min(LINE_LENGTH, input.Length - cur_char);
            returnString = returnString + input.Substring(cur_char, GET_LENGTH) + RETURN_CHAR;
            cur_char = cur_char + LINE_LENGTH;
        }

        return returnString;
    }
}