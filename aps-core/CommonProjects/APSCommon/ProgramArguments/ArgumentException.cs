using PT.Common.Exceptions;

namespace PT.APSCommon.ProgramArguments;

public class PTArgumentException : PTHandleableException
{
    public PTArgumentException(string a_msg)
        : base(a_msg) { }
}

public class ArgumentUnknownException : PTArgumentException
{
    public ArgumentUnknownException(string a_msg)
        : base(a_msg) { }
}