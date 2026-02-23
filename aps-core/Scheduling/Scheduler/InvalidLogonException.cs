using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// Summary description for InvalidLogonException.
/// </summary>
public class InvalidLogonBaseException : PTException
{
    private const string c_errorMessage = "Failed to connect to the Instance '{0}', Version '{1}' at IP/Server='{2}'.\r\n{3}";

    public InvalidLogonBaseException(string msg, object[] a_params = null, bool a_appendHelpUrl = false)
        : base(msg, a_params, a_appendHelpUrl) { }

    public InvalidLogonBaseException(string msg, Exception a_e, object[] a_params = null, bool a_appendHelpUrl = false)
        : base(msg, a_e, a_params, a_appendHelpUrl) { }

    public InvalidLogonBaseException(string a_instanceName, string a_softwareVersion, string a_systemUrl, string a_message)
        : base(string.Format(c_errorMessage, a_instanceName, a_softwareVersion, a_systemUrl, a_message)) { }

    public InvalidLogonBaseException(string a_instanceName, string a_softwareVersion, string a_systemUrl, string a_message, object[] a_params = null, bool a_appendHelpUrl = false)
        : base(string.Format(c_errorMessage, a_instanceName, a_softwareVersion, a_systemUrl, a_message), a_params, a_appendHelpUrl) { }

    public InvalidLogonBaseException(string a_instanceName, string a_softwareVersion, string a_systemUrl, string a_message, Exception a_e, object[] a_params = null, bool a_appendHelpUrl = false)
        : base(string.Format(c_errorMessage, a_instanceName, a_softwareVersion, a_systemUrl, a_message), a_e, a_params, a_appendHelpUrl) { }
}

/// <summary>
/// This exception type includes information that should not be displayed to an unauthenticated user. Ths info should be logged instead.
/// </summary>
public class InvalidLogonException : InvalidLogonBaseException
{
    public InvalidLogonException(string msg, object[] a_params = null, bool a_appendHelpUrl = false)
        : base(msg, a_params, a_appendHelpUrl) { }
}

/// <summary>
/// This exception type includes information that can be displayed to an unauthenticated user. For example invalid login credentials, failed login attempts, etc.
/// </summary>
public class FailedLogonException : InvalidLogonBaseException
{
    public FailedLogonException(string msg, Exception a_e, object[] a_params = null, bool a_appendHelpUrl = false)
        : base(msg, a_e, a_params, a_appendHelpUrl) { }

    public FailedLogonException(string msg, object[] a_params = null, bool a_appendHelpUrl = false)
        : base(msg, a_params, a_appendHelpUrl) { }
}

public class NoMoreUserLicensesException : InvalidLogonBaseException
{
    internal NoMoreUserLicensesException(string msg, object[] a_params = null, bool a_appendHelpUrl = true)
        : base(msg, a_params, a_appendHelpUrl) { }
}