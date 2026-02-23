using PT.Common.Exceptions;

namespace PT.APSCommon.Exceptions;

public class ThumbprintMismatchException : PTException
{
    public ThumbprintMismatchException(string a_message = "", object[] a_stringParameters = null, bool a_appendHelpUrl = false)
        : base(a_message, a_stringParameters, a_appendHelpUrl) { }
}