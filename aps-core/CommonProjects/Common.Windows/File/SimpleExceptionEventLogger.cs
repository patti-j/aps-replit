using System.Diagnostics;
using System.Runtime.Versioning;

using PT.Common.File;

namespace PT.Common.Windows.File;

/// <summary>
/// Allows you to easily log basic error message to a text file at the base of the c-drive through the use of
/// static functions.
/// </summary>
[SupportedOSPlatform("windows")]
public class SimpleExceptionEventLogger : SimpleExceptionLogger
{
    #region Construction. You can't create instances of this class because all the members are static.
    private SimpleExceptionEventLogger() { }
    #endregion
    
    #region EventViewer logging
    public static EventLog s_eventLog = new (APS_EVENT_LOG_NAME);
    public static string s_serviceName;

    public static void RegisterEventSource(string a_source)
    {
        if (!EventLog.SourceExists(a_source))
        {
            // This event type won't work the first time this application runs on a computer. Subsequent runs will work.
            // That is if this block is entered and an attempt to log a message is made it won't work.
            // The next time the program runs, the event source will have completely registered with Windows and log attempts will work. 
            EventLog.CreateEventSource(a_source, APS_EVENT_LOG_NAME);
        }

        // Cache the service name so it can be referenced for other serverside attempts of Event Logging
        s_serviceName = a_source;
    }

    public static void LogMessageToEventLog(string a_message, PTEventId a_eventId, EventLogEntryType a_entryType = EventLogEntryType.Information)
    {
        if (EventLog.SourceExists(s_serviceName))
        {
            s_eventLog.Source = s_serviceName;
            s_eventLog.WriteEntry(a_message, a_entryType, (int)a_eventId);
        }
    }

    public static void LogExceptionToEventLog(Exception e, PTEventId a_eventId, string description = null, EventLogEntryType a_entryType = EventLogEntryType.Error)
    {
        if (EventLog.SourceExists(s_serviceName))
        {
            s_eventLog.Source = s_serviceName;
            string message = string.IsNullOrEmpty(description) ? e.ToString() : e + "\n" + description;
            s_eventLog.WriteEntry(message, a_entryType, (int)a_eventId);
        }
    }

    public static void LogStartMessage(string a_source)
    {
        LogMessageToEventLog($"Service: {a_source} started successfully", PTEventId.STARTED_SUCCESSFULLY);
    }

    public static void LogUnhandledException(string a_source, Exception e, TimeSpan a_uptimeDuration)
    {
        string msg = $"{e}\nUptime Duration: {a_uptimeDuration}";
        LogExceptionToEventLog(e, PTEventId.UNHANDLED_EXCEPTION, msg);
    }

    public static void LogStopMessage(string a_source, TimeSpan a_uptimeDuration, string a_reason = "")
    {
        string stopMsg = $"Service: {a_source} stopped successfully. Uptime Duration: {a_uptimeDuration}";

        if (!string.IsNullOrEmpty(a_reason))
        {
            stopMsg += $"\nReason: {a_reason}";
        }

        LogMessageToEventLog(stopMsg, PTEventId.STOPPED_SUCCESSFULLY);
    }

    public static void LogShutdownMessage(string a_source, TimeSpan a_uptimeDuration)
    {
        LogMessageToEventLog($"Service: {a_source} detected system shutdown. Uptime Duration: {a_uptimeDuration}", PTEventId.SYSTEM_SHUTDOWN);
    }
    #endregion
}