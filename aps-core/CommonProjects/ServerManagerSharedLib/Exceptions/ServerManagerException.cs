namespace PT.ServerManagerSharedLib.Exceptions;

public class ServerManagerException : Exception
{
    public ServerManagerException(string message, object[] a_stringParameters = null, bool a_appendHelpUrl = true) : base (string.Format(message, a_stringParameters ?? new object[]{} ))
    {
            
    }

    public ServerManagerException(string message, Exception innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = true) : base(string.Format(message, a_stringParameters ?? new object[] { }), innerException)
    {

    }
}

public class LicenseServerException : Exception
{
    public LicenseServerException(string message, object[] a_stringParameters = null, bool a_appendHelpUrl = true)
    { }

    public LicenseServerException(string message, Exception innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = true)
    { }
}