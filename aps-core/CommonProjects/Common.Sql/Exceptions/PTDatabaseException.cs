using PT.Common.Exceptions;

namespace PT.Common.Sql.Exceptions;

public class PTDatabaseException : PTHandleableException
{
    public PTDatabaseException(string msg)
        : base(msg) { }

    public PTDatabaseException(string msg, Exception e, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
        : base(msg, e, a_stringParameters, a_appendHelpUrl) { }
}