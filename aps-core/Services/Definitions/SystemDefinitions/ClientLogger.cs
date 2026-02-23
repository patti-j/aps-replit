using System.Collections.Concurrent;
using System.Text.Json;

using PT.APIDefinitions.RequestsAndResponses.AuditObjects;
using PT.APIDefinitions.RequestsAndResponses.Webapp;
using PT.APSCommon;
using PT.Common.Extensions;
using PT.Common.File;
using PT.Common.Http;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using PT.Transmissions.Interfaces;

namespace PT.SystemDefinitions
{
    /*
     * The Client's ErrorReporter will send errors to the instance, then the instance will pass the error
     * to the WebApp and/or Sentry depending on configuration. We shouldn't have to send scheduling errors
     * over to the instance though since the instance should encounter the same scheduling error due to
     * the fact that all transmissions are routed to the instance before being broadcast to all clients.
     * Thus, the instance and all clients should be processing the same set of transmissions with the
     * same result. The main exception would be in the case of desyncs, which should be handled
     * with the desync diagnostics process. 
     */

    // TODO Logging:
    /*
     * Need to add code so that we log to disk if making API requests fail.
     * There's code in the ErrorReporter that instantiates various IErrorReporter, but I don't think
     * we should be creating all of them immediately on startup like we currently do since
     * they may not be used at all during the program's lifetime (especially on the client).
     * There are some issues with referencing WorkingDirectory since WorkingDirectory is currently
     * in the Scheduler project so we should probably move WorkingDirectory into the SystemDefinitions project. 
     */
    public class ClientLogger : ISystemLogger
    {
        private readonly BaseId m_currentUserId; //This value is passed into the API request so that the Instance can track the source of the logging request
        private readonly string m_loggingFolderPath;
        private readonly ConcurrentDictionary<Guid, string> m_transmissionErrorCache = new();

        public event ExceptionOccurredDelegate ExceptionOccurredEvent;
        public event TransmissionFailureDelegate TransmissionFailureEvent;
        public void LogAudit(BaseId a_scenarioId, BaseId a_instigator, ScenarioDataChanges a_dataChanges)
        {
            //Todo send collection of audit entries per instigator
        }

        public void LogTransmission(TransmissionLog a_transmissionLog)
        {
            //Todo send log to server
        }

        public ClientLogger(BaseId a_currentUserId, string a_loggingFolderPath)
        {
            m_currentUserId = a_currentUserId;
            m_loggingFolderPath = a_loggingFolderPath;
        }

        public void LogException(Exception a_exception, ScenarioExceptionInfo a_sei, bool a_logToSentry, ELogClassification a_logClassification = ELogClassification.Fatal)
        {
            FileLogger fileLogger = new FileLogger(m_loggingFolderPath, a_logClassification.ToString());
            fileLogger.LogException(a_exception, null, "", a_sei);

            //Alert the UI if this user sent the transmission or if this was not transmission related.
            FireExceptionOccurredDelegate(a_exception, a_logToSentry, false);
        }
        private void FireExceptionOccurredDelegate(Exception a_e, bool a_fatal, bool a_redo)
        {
            ExceptionOccurredEvent?.Invoke(a_e, a_fatal, a_redo);
        }
        public void Log(string a_description, DateTime a_eventTimestamp, ELogClassification a_classification = ELogClassification.Audit)
        {
            LogUserActionRequest request = new LogUserActionRequest();
            request.ActionDescription = a_description;
            request.TimeStamp = a_eventTimestamp;
            request.UserId = m_currentUserId.Value;

            FileLogger file = new FileLogger(m_loggingFolderPath, a_classification.ToString());
            file.LogMessage(a_description,a_classification.ToString(), "", "","");
        }
        public void LogException(Exception a_e, PTTransmission a_t, ELogClassification a_classification, bool a_logToSentry, string a_suggestion = "")
        {
            FileLogger fileLogger = new FileLogger(m_loggingFolderPath, a_classification.ToString());
            fileLogger.LogException(a_e, a_t, a_suggestion, null);
            
            if (a_t != null)
            {
                m_transmissionErrorCache.GetOrAdd(a_t.TransmissionId, a_e .GetExceptionFullMessage());
            }

            //Alert the UI if this user sent the transmission or if this was not transmission related.
            if (a_t == null || a_t.TransmissionSender != PTTransmissionBase.TransmissionSenderType.PTUser)
            {
                // Notify the UI that an exception was logged.
                FireExceptionOccurredDelegate(a_e, a_logToSentry, a_t is ScenarioBaseT && ((ScenarioBaseT)a_t).ReplayForUndoRedo);
            }
        }

        public void LogException(ExceptionDescriptionInfo a_descriptionInfo, ELogClassification a_classification, bool a_logToSentry = false)
        {
            FileLogger fileLogger = new FileLogger(m_loggingFolderPath, ELogClassification.Fatal.ToString());
            fileLogger.LogException(a_descriptionInfo, string.Empty);

            //Alert the UI if this user sent the transmission or if this was not transmission related.
            FireExceptionOccurredDelegate(new Exception(a_descriptionInfo.Message), a_logToSentry, false);
        }

        public void LogException(Exception a_e, PTTransmission a_t, ScenarioExceptionInfo a_sei, ELogClassification a_classification, bool a_logToSentry, string a_suggestion = "")
        {
            FileLogger fileLogger = new FileLogger(m_loggingFolderPath, a_classification.ToString());
            fileLogger.LogException(a_e, a_t, a_suggestion, a_sei);

            if (a_t != null)
            {
                m_transmissionErrorCache.GetOrAdd(a_t.TransmissionId, a_e.GetExceptionFullMessage());
            }

            //Alert the UI if this user sent the transmission or if this was not transmission related.
            if (a_t == null || a_t.TransmissionSender != PTTransmissionBase.TransmissionSenderType.PTUser)
            {
                // Notify the UI that an exception was logged.
                FireExceptionOccurredDelegate(a_e, a_logToSentry, a_t is ScenarioBaseT && ((ScenarioBaseT)a_t).ReplayForUndoRedo);
            }
        }

        public void LogException(ApplicationExceptionList a_errs, PTTransmission a_t, ScenarioExceptionInfo a_sei, ELogClassification a_classification, bool a_logToSentry, string a_suggestion = "")
        {
            string headerMessage = FileLogger.GetTransmissionExceptionHeader(a_t, a_suggestion);

            FileLogger fileLogger = new FileLogger(m_loggingFolderPath, a_classification.ToString());

            ApplicationExceptionList.Node node = a_errs.First;
            while (node != null)
            {
                ExceptionDescriptionInfo exceptionDescriptionInfo = node.Data;
                fileLogger.LogException(exceptionDescriptionInfo, headerMessage);

                if (a_t != null)
                {
                    m_transmissionErrorCache.GetOrAdd(a_t.TransmissionId, exceptionDescriptionInfo.Message);
                }

                node = node.Next;
            }

            //Alert the UI if this user sent the transmission or if this was not transmission related.
            if (a_t == null || a_t.TransmissionSender != PTTransmissionBase.TransmissionSenderType.PTUser)
            {
                // Notify the UI that an exception was logged.
                FireExceptionOccurredDelegate(null, a_logToSentry, a_t is ScenarioBaseT && ((ScenarioBaseT)a_t).ReplayForUndoRedo);
            }
        }

        public void Log(string a_header, string a_log, ELogClassification a_classification = ELogClassification.Notifications)
        {
            string fullMessage = string.Format("{0}{1}{2}{1}{3}", a_header, Environment.NewLine, SimpleExceptionLogger.NOTIFICATION_INFO_TEXT, a_log);

            FileLogger log = new FileLogger(m_loggingFolderPath, a_classification.ToString());
            log.LogMessage(fullMessage);

        }

        public string GetLogContents(ELogClassification a_classification, bool a_filtered = false, bool a_startInUserText = false)
        {
           FileLogger fl = new FileLogger(m_loggingFolderPath, a_classification.ToString());
           return fl.GetLogContents(a_filtered, a_startInUserText);
        }

        public void ClearLog(ELogClassification a_classification)
        {
            //Clear all logs by classification
            FileLogger fl = new FileLogger(m_loggingFolderPath, a_classification.ToString());
            fl.Clear();
        }
        public string GetReportedTransmissionErrors(Guid a_transmissionGuid)
        {
            if (m_transmissionErrorCache.TryGetValue(a_transmissionGuid, out string errorMessage))
            {
                return errorMessage;
            }

            return null;
        }
    }
}
