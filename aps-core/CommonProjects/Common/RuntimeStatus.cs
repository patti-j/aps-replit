namespace PT.Common.Runtime;

/// <summary>
/// This class enables tracking whether the program is running
/// Useful for checking in control constructors to execute code when not in design time
/// </summary>
public static class RuntimeStatus
{
    private static bool s_isRuntime;

    public static void InitializeRuntime()
    {
        s_isRuntime = true;
    }

    public static bool IsRuntime => s_isRuntime;
}