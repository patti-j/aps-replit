using PT.APSCommon;
using PT.Common.File;
using PT.Common.Text;

namespace PT.APIDefinitions.RequestsAndResponses.Webapp;

/// <summary>
/// Logger request class which contains details about the exception thrown and the affected user's id
/// </summary>
public class LogErrorRequest
{
    /// <summary>
    /// Initializes a new instance of the LogErrorRequest class for use by the JSON Serializer.
    /// </summary>
    public LogErrorRequest() { }
    /// <summary>
    /// Logger request class which contains details about the exception thrown and the affected user's id
    /// </summary>
    public LogErrorRequest(long a_userId, ExceptionDescriptionInfo a_exInfo, string a_logType, string a_headerMessage, DateTime a_errorTimeStamp, bool a_logToSentry)
    {
        UserId = a_userId;
        ErrorLog = new LogErrorDTO(a_exInfo, a_logType, a_headerMessage, a_errorTimeStamp);
        LogSentry = a_logToSentry;
    }
    public LogErrorRequest(long a_userId, Exception a_exception, string a_logType, string a_headerMessage, DateTime a_errorTimeStamp, bool a_logToSentry)
    {
        UserId = a_userId;
        ErrorLog = new LogErrorDTO(a_exception, a_logType, a_headerMessage, a_errorTimeStamp);
        LogSentry = a_logToSentry;
    }
    /// <summary>
    /// The Id of the user affected by the exception
    /// </summary>
    public long UserId { get; set; }
    
    public LogErrorDTO ErrorLog { get; set; }
    public bool LogSentry { get; set; }
}

public class LogUserActionRequest
{
    public long UserId { get; set; }
    public string ActionDescription { get; set; }
    public DateTime TimeStamp { get; set; }
}

public class LogAuditRequest
{
    public long Instigator { get; set; }

    public long ScenarioId { get; set; }

    public IEnumerable<AuditEntry> AuditEntries { get; set; }
}

/// <summary>
/// DTO class representing an error.
/// TODO: When the webapp api is set up to be able to handle PTSerializables, we can probably remove this class and directly use an ExceptionDescriptionInfo
/// </summary>
public class LogErrorDTO
{
    public string TypeName { get; set; }
    public string StackTrace { get; set; }
    public string Source { get; set; }
    //public string InnerExceptionMessage { get; set; } // Not used in current logging implementation
    //public string InnerExceptionStackTrace { get; set; }
    public string LogType { get; set; }

    // This requires access to a PTTransmission object to generate.
    // To keep things simpler now/ reduce the amount we transfer over API, generate this on the software instance
    // You can pull the method out of SQLErrorLogger
    public string HeaderMessage { get; set; }
    public string Message { get; set; }
    public DateTime TimeStamp { get; set; }

    public LogErrorDTO() { }

    public LogErrorDTO(ExceptionDescriptionInfo a_exceptionDescription, string a_logType, string a_headerMessage, DateTime? a_timeStamp = null)
    {
        TypeName = a_exceptionDescription.GetTypeName;
        Message = a_exceptionDescription.Message;
        HeaderMessage = a_headerMessage;
        StackTrace = a_exceptionDescription.StackTrace;
        Source = a_exceptionDescription.Source;
        LogType = a_logType;
        TimeStamp = a_timeStamp ?? DateTime.UtcNow;
    }

    public LogErrorDTO(Exception a_e, string a_logType, string a_headerMessage, DateTime? a_timeStamp)
        : this(new ExceptionDescriptionInfo(a_e), a_logType, a_headerMessage,a_timeStamp) { }
}

public class LogUserActionDTO
{
    public long UserId { get; set; }
    public string ActionDescription { get; set; }
}