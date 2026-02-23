using PT.APIDefinitions.RequestsAndResponses.AuditObjects;
using PT.APIDefinitions.RequestsAndResponses.Webapp;
using PT.APSCommon;
using PT.Common.File;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.SystemDefinitions.Interfaces
{
    public interface ISystemLogger : ICommonLogger
    {
        /// <summary>
        /// Used to log individual actions that do not have exceptions,
        /// thus they are not errors so a_classification defaults to Audit.
        /// </summary>
        /// <param name="a_description"></param>
        /// <param name="a_classification"></param>
        public void Log(string a_description, DateTime a_eventTimestamp, ELogClassification a_classification = ELogClassification.Audit);

        /// <summary>
        /// Log a user action that caused an exception. This overload does not have ScenarioExceptionInfo so
        /// the exception is likely not related to a single scenario. 
        /// </summary>
        /// <param name="a_exception"></param>
        /// <param name="a_transmission"></param>
        /// <param name="a_classification"></param>
        /// <param name="a_logToSentry"></param>
        /// <param name="a_suggestion">An optional string that can provide suggestions regarding the cause of the exception or how to fix it</param>
        public void LogException(Exception a_exception, PTTransmission a_transmission, ELogClassification a_classification, bool a_logToSentry, string a_suggestion = "");

        /// <summary>
        /// Log a user action that caused an exception. 
        /// </summary>
        /// <param name="a_message"></param>
        /// <param name="a_classification"></param>
        /// <param name="a_descriptionInfo"></param>
        /// <param name="a_logToSentry"></param>
        public void LogException(ExceptionDescriptionInfo a_descriptionInfo, ELogClassification a_classification, bool a_logToSentry = false);
        /// <summary>
        /// Log a user action that caused an exception. A ScenarioExceptionInfo is expected for this overload so the
        /// error/exception should be related to a specific scenario.
        /// </summary>
        /// <param name="a_exception"></param>
        /// <param name="a_transmission"></param>
        /// <param name="a_scenarioExceptionInfo"></param>
        /// <param name="a_classification"></param>
        /// <param name="a_logToSentry"></param>
        /// <param name="a_suggestion">An optional string that can provide suggestions regarding the cause of the exception or how to fix it</param>
        public void LogException(Exception a_exception, PTTransmission a_transmission, ScenarioExceptionInfo a_scenarioExceptionInfo, ELogClassification a_classification, bool a_logToSentry, string a_suggestion = "");

        /// <summary>
        /// Log a user action that caused multiple exceptions. A ScenarioExceptionInfo is expected for this overload so the
        /// errors/exceptions should be related to a specific scenario. 
        /// </summary>
        /// <param name="a_exceptions"></param>
        /// <param name="a_transmission"></param>
        /// <param name="a_scenarioExceptionInfo"></param>
        /// <param name="a_classification"></param>
        /// <param name="a_logToSentry"></param>
        /// <param name="a_suggestion">An optional string that can provide suggestions regarding the cause of the exception or how to fix it</param>
        public void LogException(ApplicationExceptionList a_exceptions, PTTransmission a_transmission, ScenarioExceptionInfo a_scenarioExceptionInfo, ELogClassification a_classification, bool a_logToSentry, string a_suggestion = "");

        public void Log(string a_header, string a_logEntry, ELogClassification a_classification = ELogClassification.Notifications);

        void LogClientException(LogErrorRequest a_request)
        {
            //Typically only implemented on the instance Logger, nothing should happen if the Client Logger calls this
        }

        /// <summary>
        /// Get the complete error log of all logged exceptions for this instance.
        /// </summary>
        /// <param name="a_classification"></param>
        /// <param name="a_filtered">Whether to omit the stack trace and source details of the errors</param>
        /// <param name="a_startInUserText"></param>
        /// <returns></returns>
        string GetLogContents(ELogClassification a_classification, bool a_filtered = false, bool a_startInUserText = false);
        /// <summary>
        /// Cleans the content of the specified log using the classification
        /// </summary>
        /// <param name="a_classification"></param>
        void ClearLog(ELogClassification a_classification);

        string GetReportedTransmissionErrors(Guid a_transmissionGuid);
        /// <summary>
        /// Fired after an exception is logged.
        /// </summary>
        public event ExceptionOccurredDelegate ExceptionOccurredEvent;
        /// <summary>
        /// Fired after an exception occurs that has an associated transmission.
        /// </summary>
        public event TransmissionFailureDelegate TransmissionFailureEvent;

        /// <summary>
        /// Log an audit entry for scenario data changes.
        /// </summary>
        /// <param name="a_scenarioId">Audited Scenario Id</param>
        /// <param name="a_instigator">The Current System User.</param>
        /// <param name="a_dataChanges">The data changes that occurred.</param>
        void LogAudit(BaseId a_scenarioId, BaseId a_instigator, ScenarioDataChanges a_dataChanges);

        void LogTransmission(TransmissionLog a_transmissionLog);
    }

    /// <summary>
    /// A delegate for firing events after an exception is logged.
    /// </summary>
    public delegate void ExceptionOccurredDelegate(Exception e, bool a_fatal, bool a_redo);
    /// <summary>
    /// A delegate for firing events after an exception occurs that has an associated transmission.
    /// </summary>
    public delegate void TransmissionFailureDelegate(Exception a_e, PTTransmission a_t, SystemMessage a_message);
}
