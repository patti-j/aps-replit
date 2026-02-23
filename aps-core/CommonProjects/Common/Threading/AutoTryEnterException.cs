namespace PT.Common;

/// <summary>
/// An exception thrown by PT.Common.ThreadLock.
/// </summary>
public class AutoTryEnterException : ApplicationException
{
    public AutoTryEnterException(string a_message) : base(a_message) { }

    public AutoTryEnterException(string a_message, bool a_writePending) : base(a_message)
    {
        WritePending = a_writePending;
    }

    public bool WritePending;
}