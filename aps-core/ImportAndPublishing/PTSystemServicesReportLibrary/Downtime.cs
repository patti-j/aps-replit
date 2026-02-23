namespace PT.SystemServicesReportLibrary;

public class Downtime
{
    public enum DowntimeTypes { OFFLINE, MAINTENANCE, UPTIME }

    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string Reason { get; set; }
    public DateTime ServiceStartTime { get; set; }
    public TimeSpan DurationSinceServiceStart { get; set; }
    public EventViewerLog StoppedLog { get; set; }
    public bool RequiresManualInput { get; set; }
    public DowntimeTypes DowntimeType { get; set; }

    public override string ToString()
    {
        if (RequiresManualInput)
        {
            return $"Unexpected Shutdown. Windows EventID: {StoppedLog.Entry.EventID} , Reported At: {StoppedLog.Entry.TimeWritten}";
        }

        if (StoppedLog.Entry.EventID == 41)
        {
            return $"Start Time: {StartTime} , Reported At: {StoppedLog.Entry.TimeWritten} , Duration: {Duration}, Reason: {Reason ?? "N/A"}";
        }

        return $"Start Time: {StartTime} , End Time: {StartTime + Duration} , Duration: {Duration}, Reason: {Reason ?? "N/A"}";
    }
}