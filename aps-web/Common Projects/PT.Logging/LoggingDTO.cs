using PT.Logging.Entities;

namespace PT.Logging;

public class LogErrorRequest
{
    public long UserId { get; set; }
    // TODO: In future, we can probably directly send an ExceptionDescriptionInfo from Common, but will require handling PTSerializables on webapp side
    public ErrorLog ErrorLog { get; set; }
}

public class LogUserActionRequest
{
    public long UserId { get; set; }
    public AuditLog AuditLog { get; set; }

}

public class GetLogsRequest
{
    // LogSource is supposed to be what object generated the log originally (so user or instance/system)
    public int LogSource { get; set; }
    // LogKind is meant to be error vs user action logs
    public ELogKind ELogKind { get; set; }
    // LogType matches the TypeName prop originally written to error logs (but not current used for audits).
    // I think we should keep this a string (or int) so we don't need to update code to handle new log types in future.
    public string LogTypeName { get; set; }
    // More thought needs to be given as to how we retrieve logs, and the flexibility we want to give in this request in terms of how to filter the logs. 
    public int LogEntryCount { get; set; } // This is here so we don't need to grab all the log entries at once
    public bool Filtered { get; set; }
    public bool StartInUserText { get; set; }
    public long UserId { get; set; }
    public string InstanceId { get; set; }
    public string CompanyName { get; set; }
}

public enum ELogKind
{
    Error,
    Audit
}

public class GetLogsResponse
{
    public string LogEntries { get; set; } // What type should this be?
}