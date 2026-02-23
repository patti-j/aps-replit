using System.Diagnostics;

namespace PT.SystemServicesReportLibrary;

public class EventViewerLog
{
    public EventLogEntry Entry { get; set; }
    public string FullSourceName { get; set; }
    public bool SystemLog { get; set; }
    public string WindowsErrorMessage { get; set; }
}