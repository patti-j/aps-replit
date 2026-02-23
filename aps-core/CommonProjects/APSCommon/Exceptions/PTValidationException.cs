using PT.Common.Exceptions;

namespace PT.APSCommon;

/// <summary>
/// Summary description for PTValidationException.
/// </summary>
public class PTValidationException : PTHandleableException
{
    public PTValidationException() { }

    public PTValidationException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
        : base(a_message, a_stringParameters, a_appendHelpUrl) { }

    public PTValidationException(string a_message, Exception a_innerException, bool a_appendHelpUrl = false, object[] a_stringParameters = null)
        : base(a_message, a_innerException, a_stringParameters, a_appendHelpUrl) { }
}