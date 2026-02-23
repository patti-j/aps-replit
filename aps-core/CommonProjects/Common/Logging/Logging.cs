using System.Diagnostics;

namespace PT.Common.Logging;

public class Logging
{
    /// <summary>
    /// Writes a timestamp and msg to the Debug log.
    /// </summary>
    /// <param name="text"></param>
    [Conditional("DEBUG")]
    public static void DebugWriteToLog(string text)
    {
        Debug.WriteLine("{0} - {1}", DateTime.Now.ToLongTimeString(), text);
    }
}