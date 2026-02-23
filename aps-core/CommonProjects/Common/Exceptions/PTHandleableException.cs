namespace PT.Common.Exceptions;

/// <summary>
/// Summary description for PTHandleableException.
/// </summary>
public class PTHandleableException : PTException
{
    public PTHandleableException() : base(false) { }

    public PTHandleableException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
        : base(a_message, a_stringParameters, a_appendHelpUrl, false) { }

    public PTHandleableException(string a_message, Exception a_innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
        : base(a_message, a_innerException, a_stringParameters, a_appendHelpUrl, false) { }
}

public class EmailException : PTHandleableException
{
    public EmailException(string a_message) : base(a_message) { }

    public EmailException(string a_message, Exception a_innerException) : base(a_message, a_innerException) { }
}