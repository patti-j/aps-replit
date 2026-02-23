using System.Reflection;

using PT.APIDefinitions.RequestsAndResponses.AuditObjects;
using PT.APIDefinitions.RequestsAndResponses.Webapp;
using PT.APSCommon;
using PT.Common.Extensions;
using PT.Common.File;
using PT.SchedulerDefinitions;
using PT.ServerManagerAPIProxy.APIClients;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.SystemDefinitions
{
    /*
     * The Instance's ErrorReporter will send error WebApp, and also Sentry if the WebApp is configured for it.
     * Scheduling errors on the Instance should always be logged to either the WebApp or Sentry. Clients will 
     * not send most (if not all) scheduling errors to the Instance since the Instance should be processing
     * the same set of transmissions as the Clients, thus encountering the same scheduling errors. 
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
    public class InstanceLogger : ISystemLogger, IDisposable
    {
        private readonly bool m_isConfiguredForSentry;
        private readonly WebAppActionsClient m_client;
        private readonly IDisposable m_sentryDisposable;
        private readonly IErrorLogger m_sqlLogger;

        private readonly Queue<LogErrorRequest> m_webAppErrorQueue = new();
        private readonly object m_webAppLock = new();
        private bool m_webAppProcessing;

        private readonly Queue<string> m_sentryErrorQueue = new();
        private readonly object m_sentryLock = new();
        private bool m_sentryProcessing;

        private readonly Queue<(ExceptionDescriptionInfo, ELogClassification)> m_sqlErrorQueue = new();
        private readonly object m_sqlLock = new();
        private bool m_sqlProcessing;

        private readonly int m_retryIntervalSeconds;
        private readonly int m_maxRetry;

        private readonly bool m_sqlLoggingEnabled;
        private readonly bool m_webAppLoggingEnabled;

        // May need to change this constructor so that we create an Action<SentryOptions> from the startupVals
        // so that we don't need to have all the higher level projects (like PlanetTogetherClient or SystemWinService)
        // have a reference to the Sentry Nuget. 
        public InstanceLogger(WebAppActionsClient a_webAppActionsClient, Action<SentryOptions> a_sentryOptions, IErrorLogger a_sqlLogger, int a_retryIntervalSeconds)
        {
            if (a_webAppActionsClient != null)
            {
                m_client = a_webAppActionsClient;
                m_webAppLoggingEnabled = true;
            }

            if (a_sqlLogger != null)
            {
                m_sqlLogger = a_sqlLogger;
                m_sqlLoggingEnabled = true;
            }
            
            m_sentryDisposable =  SentrySdk.Init(a_sentryOptions);
            m_isConfiguredForSentry = SentrySdk.IsEnabled;

            m_maxRetry = 3;
            m_retryIntervalSeconds = a_retryIntervalSeconds;
        }
        public void Log(string a_description, DateTime a_eventTimestamp, ELogClassification a_classification = ELogClassification.Audit)
        {
            if (m_sqlLoggingEnabled)
            {
                m_sqlLogger.LogMessage(a_description, a_classification.ToString(), "", a_eventTimestamp.ToString(), "");
            }
        }

        public void LogException(Exception a_e, PTTransmission a_t, ELogClassification a_classification, bool a_logToSentry, string a_suggestion = "")
        {
            Task.Run(() => AddToErrorQueue(new ExceptionDescriptionInfo(a_e), a_t.Instigator, null, a_classification, a_logToSentry));
        }

        public void LogException(ExceptionDescriptionInfo a_descriptionInfo, ELogClassification a_classification, bool a_logToSentry = false)
        {
            Task.Run(() => AddToErrorQueue(a_descriptionInfo, BaseId.NULL_ID, null, a_classification, a_logToSentry));
        }

        public void LogException(Exception a_e, PTTransmission a_t, ScenarioExceptionInfo a_sei, ELogClassification a_classification, bool a_logToSentry, string a_suggestion = "")
        {
            Task.Run(() => AddToErrorQueue(new ExceptionDescriptionInfo(a_e), a_t.Instigator, a_sei, a_classification, a_logToSentry));
        }

        public void LogException(ApplicationExceptionList a_errs, PTTransmission a_t, ScenarioExceptionInfo a_sei, ELogClassification a_classification, bool a_logToSentry, string a_suggestion = "")
        {
            // Log the exceptions.
            ApplicationExceptionList.Node node = a_errs.First;
            while (node != null)
            {
                ExceptionDescriptionInfo exceptionDescriptionInfo = node.Data;
                Task.Run(() => AddToErrorQueue(exceptionDescriptionInfo, a_t.Instigator, a_sei, a_classification, a_logToSentry));
                node = node.Next;
            }
        }

        public void Log(string a_header, string a_log, ELogClassification a_classification = ELogClassification.Notifications)
        {
            if (!m_sqlLoggingEnabled)
            {
                return;
            }
            string fullMessage = string.Format("{0}{1}{2}{1}{3}", a_header, Environment.NewLine, SimpleExceptionLogger.NOTIFICATION_INFO_TEXT, a_log);
            m_sqlLogger?.LogMessage(fullMessage, a_classification.ToString(), "", PTDateTime.UtcNow.ToString(), "");
        }

        public void LogException(Exception a_e, ScenarioExceptionInfo a_sei, bool a_logToSentry = false, ELogClassification a_logClassification = ELogClassification.Misc)
        {
            Task.Run(() => AddToErrorQueue(new ExceptionDescriptionInfo(a_e), BaseId.NULL_ID, a_sei, a_logClassification, a_logToSentry));
        }
        
        /// <summary>
        /// Queues up the error request for later logging
        /// </summary>  
        /// <param name="a_request"></param>
        public void LogClientException(LogErrorRequest a_request)
        {
            Enum.TryParse(a_request.ErrorLog.LogType, out ELogClassification classification);
            Task.Run(() => AddToErrorQueue(a_request, new BaseId(a_request.UserId), null,classification,a_request.LogSentry));
        }

        public string GetLogContents(ELogClassification a_classification, bool a_filtered = false, bool a_startInUserText = false)
        {
            //Todo: Implement retrieval of WebApp Logs, Sql Logs
            return String.Empty;
        }

        public void ClearLog(ELogClassification a_classification)
        {
            //Todo: Implement clearing of WebApp Logs, Sql Logs
        }

        public string GetReportedTransmissionErrors(Guid a_transmissionGuid)
        {
            return string.Empty;
        }

        public event ExceptionOccurredDelegate ExceptionOccurredEvent;
        public event TransmissionFailureDelegate TransmissionFailureEvent;
        
        public void LogAudit(BaseId a_scenarioId, BaseId a_instigator, ScenarioDataChanges a_dataChanges)
        {
            if (a_dataChanges.HasChanges)
            {
                //m_client.LogAuditAction(new LogAuditRequest(){Instigator = a_instigator, AuditEntries = GetAllChangeBuckets(a_dataChanges).SelectMany(x => x.AuditEntries).ToList() });
            }
        }

        public void LogTransmission(TransmissionLog a_transmissionLog)
        {
            //Todo send log to web app
        }

        private static IEnumerable<IDataObjectChanges> GetAllChangeBuckets(ScenarioDataChanges a_scenarioDataChanges)
        {
            return typeof(ScenarioDataChanges)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => typeof(IDataObjectChanges).IsAssignableFrom(p.PropertyType))
                .Select(p => p.GetValue(a_scenarioDataChanges) as IDataObjectChanges)
                .Where(v => v != null);
        }

        /// <summary>
        /// Queues up errors to be logged to the appropriate destination 
        /// </summary>
        /// <typeparam name="T">This can either be a <para><see cref="LogErrorRequest"/>- typically sent from the client </para>or <para><see cref="ExceptionDescriptionInfo"/>- typically defined directly on the server</para></typeparam>
        /// <param name="a_errorData"></param>
        /// <param name="a_instigator"></param>
        /// <param name="a_sei"></param>
        /// <param name="a_classification"></param>
        /// <param name="a_logToSentry"></param>
        private void AddToErrorQueue<T>(T a_errorData, BaseId a_instigator, ScenarioExceptionInfo a_sei, ELogClassification a_classification, bool a_logToSentry)
        {
            ExceptionDescriptionInfo exceptionDescriptionInfo = new ExceptionDescriptionInfo();

            LogErrorRequest webAppRequest = null;
            string sentryMsg = String.Empty;
            bool sendToSentry = false;

            if (a_errorData is not LogErrorRequest && a_errorData is not ExceptionDescriptionInfo)
            {
                return;
            }

            if (a_errorData is LogErrorRequest request)
            {
                exceptionDescriptionInfo = new ExceptionDescriptionInfo(request.ErrorLog.TypeName, request.ErrorLog.Message, request.ErrorLog.StackTrace, request.ErrorLog.Source);
                webAppRequest = request;

                if (m_isConfiguredForSentry && request.LogSentry)
                {
                    sentryMsg = SimpleExceptionLogger.CreateErrorMsgString(exceptionDescriptionInfo, null, String.Empty);
                    sendToSentry = true;
                }
                
            } 
            else if (a_errorData is ExceptionDescriptionInfo info)
            {
                exceptionDescriptionInfo = info;
                ELogClassification classification = a_classification;
                BaseId instigator = a_instigator;
                sendToSentry = a_logToSentry;
                ScenarioExceptionInfo erExceptionInfo = a_sei;

                //TODO: Log SEI to the web app as well.
                webAppRequest = new LogErrorRequest(instigator.Value, info, classification.ToString(), "", DateTime.UtcNow, sendToSentry);

                if (m_isConfiguredForSentry && sendToSentry)
                {
                    sentryMsg = SimpleExceptionLogger.CreateErrorMsgString(exceptionDescriptionInfo, erExceptionInfo, String.Empty);
                }

            }

            if (m_webAppLoggingEnabled)
            {
                AddToWebApp(webAppRequest);
            }

            if (sendToSentry)
            {
                AddToSentry(sentryMsg);
            }

            if (m_sqlLoggingEnabled)
            {
                AddToSql(exceptionDescriptionInfo, a_classification);
            }
        }

        private void AddToAuditQueue( BaseId a_instigator, LogAuditRequest a_sei, ELogClassification a_classification = ELogClassification.Audit, bool a_logToSentry = false)
        {
            // TODO: Implement Audit Logging to WebApp, Sentry, SQL as needed.
        }

        /// <summary>
        /// Adds an error log request to the WebApp error queue and starts a background task to process the queue
        /// if no task is currently running.
        /// </summary>
        private void AddToWebApp(LogErrorRequest a_request)
        {
            if (a_request == null)
            {
                return;
            }

            lock (m_webAppLock)
            {
                m_webAppErrorQueue.Enqueue(a_request);

                if (!m_webAppProcessing)
                {
                    m_webAppProcessing = true;
                    Task.Factory.StartNew(TriggerWebAppProcessing, TaskCreationOptions.LongRunning);
                }
            }
        }

        /// <summary>
        /// Adds an error message to the Sentry error queue and starts a background task to process the queue
        /// if no task is currently running.
        /// </summary>
        private void AddToSentry(string a_errorData)
        {
            if (string.IsNullOrEmpty(a_errorData))
            {
                return;
            }

            lock (m_sentryLock)
            {
                m_sentryErrorQueue.Enqueue(a_errorData);

                if (!m_sentryProcessing)
                {
                    m_sentryProcessing = true;
                    Task.Factory.StartNew(TriggerSentryProcessing, TaskCreationOptions.LongRunning);
                }
            }
        }
        /// <summary>
        /// Adds an error to the SQL error queue and starts a background task to process the queue
        /// if no task is currently running.
        /// </summary>
        private void AddToSql(ExceptionDescriptionInfo a_request, ELogClassification a_classification)
        {
            if (a_request == null)
            {
                return;
            }

            lock (m_sqlLock)
            {
                m_sqlErrorQueue.Enqueue((a_request, a_classification));

                if (!m_sqlProcessing)
                {
                    m_sqlProcessing = true;
                    Task.Factory.StartNew(TriggerSqlProcessing, TaskCreationOptions.LongRunning);
                }
            }
        }
        /// <summary>
        /// Background task that dequeues and logs errors to the SQL database. Continues processing until the queue is empty.
        /// Includes basic retry mechanism (re-enqueue on failure or log to disk).
        /// </summary>
        private void TriggerSqlProcessing()
        {
            while (true)
            {
                ExceptionDescriptionInfo edi;
                ELogClassification errorClassification;
                lock (m_sqlLock)
                {
                    if (m_sqlErrorQueue.Count > 0)
                    {
                        (edi, errorClassification) = m_sqlErrorQueue.Dequeue();
                    }
                    else
                    {
                        m_sqlProcessing = false;
                        break;
                    }
                }

                try
                {
                    m_sqlLogger?.SetLogType(errorClassification.ToString());
                    m_sqlLogger?.LogException(edi, null);
                }
                catch (Exception e)
                {
                    //Something has gone wrong, hope for the best.
                    string workingDirectoryFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    File.WriteAllText(Path.Combine(workingDirectoryFolder, "FallBackErrors.log"), e.GetExceptionFull());
                }
            }
        }
        /// <summary>
        /// Background task that dequeues and sends error messages to Sentry. Continues processing until the queue is empty.
        /// Includes basic retry mechanism (re-enqueue on failure or log to disk).
        /// </summary>
        private async void TriggerSentryProcessing()
        {
            int logTry = 0;

            while (true)
            {
                string errorMsg;

                lock (m_sentryLock)
                {
                    if (m_sentryErrorQueue.Count > 0)
                    {
                        errorMsg = m_sentryErrorQueue.Dequeue();
                    }
                    else
                    {
                        m_sentryProcessing = false;
                        break;
                    }
                }

                try
                {
                    SentrySdk.CaptureMessage(errorMsg, SentryLevel.Error);
                }
                catch (Exception)
                {
                    if (logTry < m_maxRetry)
                    {
                        lock (m_sentryLock)
                        {
                            m_sentryErrorQueue.Enqueue(errorMsg);
                        }

                        await Task.Delay(TimeSpan.FromSeconds(m_retryIntervalSeconds));
                        logTry++;
                    }
                }
            }
        }
        /// <summary>
        /// Background task that dequeues and sends error log requests to the WebApp service. Continues processing until the queue is empty.
        /// Includes basic retry mechanism (re-enqueue on failure or log to disk).
        /// </summary>
        private async void TriggerWebAppProcessing()
        {
            while (true)
            {
                LogErrorRequest request;

                lock (m_webAppLock)
                {
                    if (m_webAppErrorQueue.Count > 0)
                    {
                        request = m_webAppErrorQueue.Dequeue();
                    }
                    else
                    {
                        m_webAppProcessing = false;
                        break;
                    }
                }

                try
                {
                    m_client.LogErrorToWebApp(request);
                }
                catch (Exception ex)
                {
                    //In the event of failure when attempting to log to the web app, log to the event viewer and requeue original error for logging later
                    //TODO: This was the only line preventing Scheduler project from not referencing windows
                    //SimpleExceptionEventLogger.LogExceptionToEventLog(ex, SimpleExceptionLogger.PTEventId.ERROR_MESSAGE, "Failed to Log Error to Web App".Localize());

                    lock (m_webAppErrorQueue)
                    {
                       m_webAppErrorQueue.Enqueue(request);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(m_retryIntervalSeconds));
                }
            }
        }


        public void Dispose()
        {
            m_sentryDisposable.Dispose();
        }
    }
}
