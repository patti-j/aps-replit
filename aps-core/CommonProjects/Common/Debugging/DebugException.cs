namespace PT.Common.Debugging;

/// <summary>
/// We can use this exception type for debug exceptions
/// </summary>
public class DebugException : Exception
{
    //Only throws exceptions in debug mode
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowInDebug()
    {
        throw new DebugException();
    }

    //Only throws exceptions in debug mode
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowInDebug(string a_message)
    {
        throw new DebugException(a_message);
    }

    //Only throws exceptions in debug mode
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowInDebug(string a_message, Exception a_innerException)
    {
        throw new DebugException(a_message, a_innerException);
    }

    //Only throws exceptions in Test mode
    [System.Diagnostics.Conditional("TEST")]
    public static void ThrowInTest(string a_message)
    {
        throw new DebugException(a_message);
    }

    //Only throws exceptions in Test mode
    [System.Diagnostics.Conditional("TEST")]
    public static void ThrowInTest(string a_message, Exception a_innerException)
    {
        throw new DebugException(a_message, a_innerException);
    }

    //Only throws exceptions in Test mode
    [System.Diagnostics.Conditional("RELEASE")]
    public static void ThrowInRelease(string a_message)
    {
        throw new DebugException(a_message);
    }

    //Only throws exceptions in Test mode
    [System.Diagnostics.Conditional("RELEASE")]
    public static void ThrowInRelease(string a_message, Exception a_innerException)
    {
        throw new DebugException(a_message, a_innerException);
    }

    public DebugException() { }

    public DebugException(string a_message)
        : base(a_message) { }

    public DebugException(string a_message, Exception a_innerException)
        : base(a_message, a_innerException) { }
}