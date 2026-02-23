using PT.Common.Localization;

namespace PT.Common.Exceptions;

public class PTException : CommonException
{
    public PTException(bool a_logToSentry = true) : base(a_logToSentry)
    {
    }

    public PTException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = true, bool a_logToSentry = true)
        : base(Localizer.GetErrorString(a_message, a_stringParameters, a_appendHelpUrl), a_logToSentry)
    {
    }

    public PTException(string a_message, Exception a_innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = true, bool a_logToSentry = true)
        : base(Localizer.GetErrorString(a_message, a_stringParameters, a_appendHelpUrl), a_innerException, a_logToSentry)
    {
    }
}