using PT.Common.File;

namespace PT.APSCommon
{
    public interface ICommonLogger
    {
        void LogException(Exception a_e, ScenarioExceptionInfo a_sei, bool a_logToSentry = false, ELogClassification a_log = ELogClassification.Misc);
    }
    // We may need to split up Audit into further categories
    public enum ELogClassification
    {
        Audit, // Indicates that the log is meant for auditing purposes and not an error. We may want to add more classifications for audits in the future. 
        Fatal, // All these other classifications right now indicate some type of error occurred. 
        ExternalInterface,
        Misc,
        PtInterface,
        PtSystem,
        PtUser,
        UI,
        Desync,
        Key,
        SchedulingWarning,
        Notifications,
        API,
        ApiDiagnostics,
        PtService,
        Login
    }
}
