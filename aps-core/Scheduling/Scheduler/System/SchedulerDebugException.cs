using PT.Common.Debugging;

namespace PT.Scheduler;

internal class SchedulerDebugException : DebugException
{
    //Only throws exceptions in debug mode and when not running MR
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfNotMassRecordings()
    {
        if (!PTSystem.RunningMassRecordings)
        {
            ThrowInDebug();
        }
    }

    //Only throws exceptions in debug mode and when not running MR
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfNotMassRecordings(string a_message)
    {
        if (!PTSystem.RunningMassRecordings)
        {
            ThrowInDebug(a_message);
        }
    }

    //Only throws exceptions in debug mode and when running MR
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfRunningMassRecordings()
    {
        if (PTSystem.RunningMassRecordings)
        {
            ThrowInDebug();
        }
    }

    //Only throws exceptions in debug mode and when running MR
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfRunningMassRecordings(string a_message)
    {
        if (PTSystem.RunningMassRecordings)
        {
            ThrowInDebug(a_message);
        }
    }
}