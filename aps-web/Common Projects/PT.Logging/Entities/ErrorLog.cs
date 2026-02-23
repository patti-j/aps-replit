using PT.Common.File;

namespace PT.Logging.Entities
{
    public class ErrorLog
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

        public ErrorLog() { }

        public ErrorLog(ExceptionDescriptionInfo a_exceptionDescription, string a_logType, DateTime? a_timeStamp)
        {
            TypeName = a_exceptionDescription.GetTypeName;
            Message = a_exceptionDescription.Message;
            StackTrace = a_exceptionDescription.StackTrace;
            Source = a_exceptionDescription.Source;
            LogType = a_logType;
            TimeStamp = a_timeStamp ?? DateTime.UtcNow;
        }

        public ErrorLog(Exception a_e, DateTime a_timeStamp, string a_logType)
            : this(new ExceptionDescriptionInfo(a_e), a_logType, a_timeStamp) { }
    }
}
