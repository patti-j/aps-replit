using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace PT.Common.Debugging;

/// <summary>
/// Debuging class to record timing for some of the UI functions.
/// It writes the logs to executable location like this:
/// I:\APSSet\APS\APS\APS\UI\bin\Debug\Timing.txt
/// It's written when UI is closed.
/// </summary>
public class Timing
{
    private static readonly StringBuilder m_entries = new ();

    private static string GetTimingLogPath()
    {
        string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        return Path.Combine(dir, "Timing.txt");
    }

    /// <summary>
    /// Pass in a PT.Common.Testing.Timing object that has a Name property. It stops it and adds a string with the
    /// Name and Length of the timer to a StringBuilder to be written to log later.
    /// </summary>
    /// <param name="a_timer"></param>
    [Conditional("DEBUG")]
    public static void Log(PT.Common.Testing.Timing a_timer)
    {
        a_timer.Stop();
        string msg = string.Format("{0}: {1}", a_timer.Name, TimeSpan.FromTicks(a_timer.Length).ToString()) + Environment.NewLine;
        m_entries.AppendLine(msg);
    }

    [Conditional("DEBUG")]
    public static void WriteToFile()
    {
        using (StreamWriter sw = new (GetTimingLogPath()))
        {
            sw.WriteLine(m_entries);
            sw.Flush();
        }
    }
}