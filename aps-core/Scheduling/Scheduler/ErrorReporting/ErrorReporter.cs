//using Microsoft.Data.SqlClient;
//using PT.APIDefinitions.RequestsAndResponses.Webapp;
//using PT.APSCommon;
//using PT.APSCommon.Extensions;
//using PT.Common.Extensions;
//using PT.Common.File;
//using PT.Common.Http;
//using PT.ImportDefintions;
//using PT.Scheduler.ErrorReporting;
//using PT.Scheduler.Sessions;
//using PT.SchedulerDefinitions;
//using PT.SystemDefinitions;
//using PT.SystemDefinitions.Interfaces;
//using PT.Transmissions;
//using System.Collections.Concurrent;

//using PT.Common.Exceptions;
//using PT.Common.Sql.SqlServer;

//namespace PT.Scheduler;

///// <summary>
///// Use this class to report exceptions.
///// It keeps a copy of the last few exceptions and writes exceptions to the appropriate log file.
///// </summary>
//public class ErrorReporter : IErrorReporter
//{
//    private readonly HashSet<string> m_ptExceptionCollection = new HashSet<string>();
//    #region Construction
//    public ErrorReporter(WorkingDirectory a_systemWorkingDirectory)
//    {
//        CreateLoggers(a_systemWorkingDirectory, string.Empty, string.Empty, string.Empty);
//    }

//    public ErrorReporter(WorkingDirectory a_systemWorkingDirectory, string a_sqlConnectionString, string a_instanceName, string a_softwareVersion)
//    {
//        CreateLoggers(a_systemWorkingDirectory, a_sqlConnectionString, a_instanceName, a_softwareVersion);
//    }

//    /// <summary>
//    /// Creates error loggers to add to database or write to disk if no database connection.
//    /// </summary>
//    /// <param name="a_sqlConnectionString"></param>
//    /// <param name="a_instanceName"></param>
//    /// <param name="a_softwareVersion"></param>
//    private void CreateLoggers(WorkingDirectory a_systemWorkingDirectory, string a_sqlConnectionString, string a_instanceName, string a_softwareVersion)
//    {
//        DatabaseConnections connections = new (a_sqlConnectionString);
//        if (!connections.IsValid())
//        {
//            CreateFileLoggers(a_systemWorkingDirectory);
//        }
//        else
//        {
//            try
//            {
//                CreateDatabase(a_sqlConnectionString);
//                CreateSQLLoggers(a_sqlConnectionString, a_instanceName, a_softwareVersion);
//            }
//            catch (SqlException ex)
//            {
//                CreateFileLoggers(a_systemWorkingDirectory);

//                SimpleExceptionEventLogger.LogExceptionToEventLog(new PTException($"An error occured when initializing connection to Audit DB on Server '{ex.Server}': {ex}"), // Localization hasn't been loaded yet
//                    SimpleExceptionLogger.PTEventId.EXCEPTION);
//            }
//        }
//    }

//    /// <summary>
//    /// Writes error logs to database.
//    /// </summary>
//    /// <param name="a_sqlConnectionString"></param>
//    /// <param name="a_instanceName"></param>
//    /// <param name="a_softwareVersion"></param>
//    private void CreateSQLLoggers(string a_sqlConnectionString, string a_instanceName, string a_softwareVersion)
//    {
//        m_fatal = new SQLErrorLogger(a_instanceName, a_softwareVersion, a_sqlConnectionString, "Exceptions");
//        m_externalInterface = new SQLErrorLogger(a_instanceName, a_softwareVersion, a_sqlConnectionString, "ExternalInterface");
//        m_misc = new SQLErrorLogger(a_instanceName, a_softwareVersion, a_sqlConnectionString, "Misc");
//        m_ptInterface = new SQLErrorLogger(a_instanceName, a_softwareVersion, a_sqlConnectionString, "Interface");
//        m_ptSystem = new SQLErrorLogger(a_instanceName, a_softwareVersion, a_sqlConnectionString, "System");
//        m_ptUser = new SQLErrorLogger(a_instanceName, a_softwareVersion, a_sqlConnectionString, "User");
//        m_ui = new SQLErrorLogger(a_instanceName, a_softwareVersion, a_sqlConnectionString, "ErrorsReportedByClients");
//        m_desync = new SQLErrorLogger(a_instanceName, a_softwareVersion, a_sqlConnectionString, "Desyncs");
//        m_key = new SQLErrorLogger(a_instanceName, a_softwareVersion, a_sqlConnectionString, "Key");
//        m_schedulingWarnings = new SQLErrorLogger(a_instanceName, a_softwareVersion, a_sqlConnectionString, "SchedulingWarnings");
//        m_notifications = new SQLErrorLogger(a_instanceName, a_softwareVersion, a_sqlConnectionString, "Notifications");
//        m_api = new SQLErrorLogger(a_instanceName, a_softwareVersion, a_sqlConnectionString, "Api");
//        m_apiDiagnostics = new SQLErrorLogger(a_instanceName, a_softwareVersion, a_sqlConnectionString, "ApiDiagnostics");
//        m_ptService = new SQLErrorLogger(a_instanceName, a_softwareVersion, a_sqlConnectionString, "Service");
//        m_failedLogin = new SQLErrorLogger(a_instanceName, a_softwareVersion, a_sqlConnectionString, "StartupSystemRetrievalInvalidLogons");
//    }

//    /// <summary>
//    /// Writes error logs to disk.
//    /// </summary>
//    private void CreateFileLoggers(WorkingDirectory a_systemWorkingDirectory)
//    {
//        m_fatal = new FileLogger(a_systemWorkingDirectory.LogFolder, "Exceptions");
//        m_externalInterface = new FileLogger(a_systemWorkingDirectory.LogFolder, "ExternalInterface");
//        m_misc = new FileLogger(a_systemWorkingDirectory.LogFolder, "Misc");
//        m_ptInterface = new FileLogger(a_systemWorkingDirectory.LogFolder, "Interface");
//        m_ptSystem = new FileLogger(a_systemWorkingDirectory.LogFolder, "System");
//        m_ptUser = new FileLogger(a_systemWorkingDirectory.LogFolder, "User");
//        m_ui = new FileLogger(a_systemWorkingDirectory.LogFolder, "ErrorsReportedByClients");
//        m_desync = new FileLogger(a_systemWorkingDirectory.LogFolder, "Desyncs");
//        m_key = new FileLogger(a_systemWorkingDirectory.LogFolder, "Key");
//        m_schedulingWarnings = new FileLogger(a_systemWorkingDirectory.LogFolder, "SchedulingWarnings");
//        m_notifications = new FileLogger(a_systemWorkingDirectory.LogFolder, "Notifications");
//        m_api = new FileLogger(a_systemWorkingDirectory.LogFolder, "Api");
//        m_apiDiagnostics = new FileLogger(a_systemWorkingDirectory.LogFolder, "ApiDiagnostics");
//        m_ptService = new FileLogger(a_systemWorkingDirectory.LogFolder, "Service");
//        m_failedLogin = new FileLogger(a_systemWorkingDirectory.LogFolder, "StartupSystemRetrievalInvalidLogons");
//    }

//    /// <summary>
//    /// Checks database for and creates InstanceLogs table for error logging.
//    /// </summary>
//    /// <param name="a_sqlConnectionString"></param>
//    private static void CreateDatabase(string a_sqlConnectionString)
//    {
//        DatabaseConnections databaseConnection = new (a_sqlConnectionString);
//        if (!databaseConnection.IsValid())
//        {
//            return;
//        }

//        string createTable = @"IF OBJECT_ID('InstanceLogs', 'U') IS NULL CREATE TABLE InstanceLogs (InstanceName nvarchar(MAX),  SoftwareVersion nvarchar(MAX), TypeName nvarchar(MAX), Message nvarchar(MAX), StackTrace nvarchar(MAX), Source nvarchar(MAX),
//                                                        InnerExceptionMessage nvarchar(MAX), InnerExceptionStackTrace nvarchar(MAX), LogType nvarchar(MAX), HeaderMessage nvarchar(MAX), Timestamp datetime NULL DEFAULT (GETDATE()))";

//        databaseConnection.SendSQLTransaction(new[] { createTable });

//        createTable = @"IF OBJECT_ID('InstanceServiceLogs', 'U') IS NULL CREATE TABLE InstanceServiceLogs (InstanceName nvarchar(MAX),  SoftwareVersion nvarchar(MAX), Message nvarchar(MAX), EventType nvarchar(MAX), Reason nvarchar(MAX), TimeOfEvent nvarchar(MAX), DurationSinceStart nvarchar(MAX), Timestamp datetime NULL DEFAULT (GETDATE()))";

//        databaseConnection.SendSQLTransaction(new[] { createTable });
//    }
//    #endregion

//    #region Declarations
//    private IErrorLogger m_fatal;
//    private IErrorLogger m_externalInterface;

//    private IErrorLogger m_misc;
//    private IErrorLogger m_ptInterface;
//    private IErrorLogger m_ptSystem;
//    private IErrorLogger m_ptUser;
//    private IErrorLogger m_ui;
//    private IErrorLogger m_desync;
//    private IErrorLogger m_key;
//    private IErrorLogger m_schedulingWarnings;
//    private IErrorLogger m_notifications;
//    private IErrorLogger m_api;
//    private IErrorLogger m_apiDiagnostics;
//    private IErrorLogger m_ptService;
//    private IErrorLogger m_failedLogin;

//    private readonly ConcurrentDictionary<Guid, string> m_transmissionErrorCache = new ();
//    #endregion

//    #region Path Properties
//    /// <summary>
//    /// The full path to the Fatal log. This is the log that errors that bring down the system are written to.
//    /// </summary>
//    public string FatalPath => m_fatal.GetLogTitle();

//    /// <summary>
//    /// Full path to the ExternalInterface log. This is the log written to by external systems.
//    /// </summary>
//    public string ExternalInterfacePath => m_externalInterface.GetLogTitle();

//    /// <summary>
//    /// The full path to the Misc log.
//    /// These are issues that should be almost insignificant unless something was incorrectly labeled as insignificant by throwing a PTHandleableException or PTValidationException when a different exception
//    /// was appropriate.
//    /// </summary>
//    public string MiscPath => m_misc.GetLogTitle();

//    /// <summary>
//    /// Full path to the PTInterface log. These are warnings created while processing transmissions resulting from running the PTInterface.
//    /// </summary>
//    public string PTInterfacePath => m_ptInterface.GetLogTitle();

//    /// <summary>
//    /// Full path to the PTSystem log. These are warning created while processing transmissions generated by PT.
//    /// </summary>
//    public string PTSystemPath => m_ptSystem.GetLogTitle();

//    /// <summary>
//    /// Full path to the PTUser log. These are warnings created while processing transmissions generated by users running the PlanetTogether user interface.
//    /// </summary>
//    public string PTUserPath => m_ptUser.GetLogTitle();

//    public string SchedulingWarningsPath => m_schedulingWarnings.GetLogTitle();

//    public string NotificationsPath => m_notifications.GetLogTitle();

//    public string ApiPath => m_api.GetLogTitle();
//    #endregion

//    #region Public log readers
//    /// <summary>
//    /// Returns all the information in the file log.
//    /// </summary>
//    public string FatalLogContents => m_fatal.GetLogContents(false);

//    public string FatalLogContentsFiltered => m_fatal.GetLogContents(true);

//    /// <summary>
//    /// Returns all the information in the file log.
//    /// </summary>
//    public string ExternalInterfaceLogContents => m_externalInterface.GetLogContents(false);

//    public string ExternalInterfaceLogContentsFiltered => m_externalInterface.GetLogContents(true);

//    public string MiscLogContents => m_misc.GetLogContents(false);

//    public string MiscLogContentsFiltered => m_misc.GetLogContents(true);

//    /// <summary>
//    /// Returns all the information in the file log.
//    /// </summary>
//    public string PTInterfaceLogContents => m_ptInterface.GetLogContents(false);

//    public string PTInterfaceLogContentsFiltered => m_ptInterface.GetLogContents(true);

//    /// <summary>
//    /// Returns all the information in the file log.
//    /// </summary>
//    public string PTSystemLogContents => m_ptSystem.GetLogContents(false);

//    public string SchedulingWarningsContents => m_schedulingWarnings.GetLogContents(false);

//    public string NotificationsContents => m_notifications.GetLogContents(false);

//    public string PTSystemLogContentsFiltered => m_ptSystem.GetLogContents(true);

//    /// <summary>
//    /// Returns all the information in the file log.
//    /// </summary>
//    public string PTUserLogContents => m_ptUser.GetLogContents(false);

//    public string PTUserLogContentsFiltered => m_ptUser.GetLogContents(true);

//    public string SchedulingWarningContentsFiltered => m_schedulingWarnings.GetLogContents(true);

//    public string NotificationsContentsFiltered => m_notifications.GetLogContents(true, true);

//    public string ApiContents => m_api.GetLogContents(false);
//    #endregion

//    #region Public methods to clear logs.
//    /// <summary>
//    /// Clear all the text from this log.
//    /// </summary>
//    public void ClearFatalLog()
//    {
//        m_fatal.Clear();
//    }

//    /// <summary>
//    /// Clear all the text from this log.
//    /// </summary>
//    public void ClearExternalInterfaceLog()
//    {
//        m_externalInterface.Clear();
//    }

//    /// <summary>
//    /// Clears all the text from this log.
//    /// </summary>
//    public void ClearMiscLog()
//    {
//        m_misc.Clear();
//    }

//    /// <summary>
//    /// Clear all the text from this log.
//    /// </summary>
//    public void ClearPTInterfaceLog()
//    {
//        m_ptInterface.Clear();
//    }

//    /// <summary>
//    /// Clear all the text from this log.
//    /// </summary>
//    public void ClearPTSystemLog()
//    {
//        m_ptSystem.Clear();
//    }

//    /// <summary>
//    /// Clear all the text from this log.
//    /// </summary>
//    public void ClearPTUserLog()
//    {
//        m_ptUser.Clear();
//    }

//    public void ClearAPILog()
//    {
//        m_api.Clear();
//    }

//    /// <summary>
//    /// Clear logs: Fatal, ExternalInterface, PTInterface, PTSystem, and PTUser.
//    /// </summary>
//    public void ClearAllLogs()
//    {
//        ClearFatalLog();
//        ClearExternalInterfaceLog();
//        ClearPTInterfaceLog();
//        ClearPTSystemLog();
//        ClearPTUserLog();
//        ClearSchedulingWarnings();
//        ClearNotifications();
//    }

//    public void ClearSchedulingWarnings()
//    {
//        m_schedulingWarnings.Clear();
//    }

//    public void ClearNotifications()
//    {
//        m_notifications.Clear();
//    }
//    #endregion

//    #region Events: ExceptionEvent and TransmissionFailureEvent.
//    /// <summary>
//    /// A delegate for firing events after an exception is logged.
//    /// </summary>
//    public delegate void ExceptionOccurredDelegate(Exception e, bool a_fatal, bool a_redo);

//    /// <summary>
//    /// Fired after an exception is logged.
//    /// </summary>
//    public event ExceptionOccurredDelegate ExceptionOccurredEvent;

//    private void FireExceptionOccurredDelegate(Exception a_e, bool a_fatal, bool a_redo)
//    {
//        ExceptionOccurredEvent?.Invoke(a_e, a_fatal, a_redo);
//    }

//    private void FireInterfaceExceptionOccuredDelegate(bool a_redo)
//    {
//        ExceptionOccurredEvent?.Invoke(new PTHandleableException(), false, a_redo);
//    }

//    /// <summary>
//    /// A delegate for firing events after an exception occurs that has an associated transmission.
//    /// </summary>
//    public delegate void TransmissionFailureDelegate(Exception a_e, PTTransmission a_t, SystemMessage a_message);

//    /// <summary>
//    /// Fired after an exception occurs that has an associated transmission.
//    /// </summary>
//    public event TransmissionFailureDelegate TransmissionFailureEvent;

//    /// <summary>
//    /// Reports to the user interface that a transmission has failed.
//    /// </summary>
//    /// <param name="a_t"></param>
//    /// <param name="a_message"></param>
//    private void FireTransmissionFailureDelegate(Exception a_e, PTTransmission a_t, SystemMessage a_message)
//    {
//        if (TransmissionFailureEvent != null)
//        {
//            TransmissionFailureEvent(a_e, a_t, a_message);
//        }
//    }
//    #endregion

//    #region Writes the exception information to the appropriate xml error log.
//    /// <summary>
//    /// Writes the exception to the appropriate log.
//    /// If the exception is fatal this function will terminate the system.
//    /// Any Exception that is not inherited from PTMiscHandlableException is fatal.
//    /// </summary>
//    /// <param name="a_e"></param>
//    /// <param name="a_sei"></param>
//    /// <param name="a_logToSentry"></param>
//    public void LogException<T>(T a_e, ScenarioExceptionInfo a_sei)
//    {
//        ScenarioExceptionInfo sei = a_sei ?? new ScenarioExceptionInfo();
//        LogException(a_e, null, "", sei);
//    }

//    /// <summary>
//    /// Writes the exception to the appropriate log.
//    /// If the exception is fatal this function will terminate the system.
//    /// Any Exception that is not inherited from PTMiscHandlableException is fatal.
//    /// </summary>
//    /// <param name="a_e"></param>
//    /// <param name="a_t">Can be NULL.</param>
//    public void LogException<T>(T a_e, PTTransmission a_t, ScenarioExceptionInfo a_sei)
//    {
//        ScenarioExceptionInfo sei = a_sei ?? new ScenarioExceptionInfo();
//        LogException(a_e, a_t, "", sei);
//    }

//    /// <summary>
//    /// Writes the exception to the appropriate log.
//    /// If the exception is fatal this function will terminate the system.
//    /// Any Exception that is not inherited from PTMiscHandlableException is fatal.
//    /// </summary>
//    /// <param name="a_e"></param>
//    /// <param name="a_t">Can be NULL.</param>
//    /// <param name="a_suggestion">Some extra text you would like in the log entry. Can be NULL or empty.</param>
//    public void LogException<T>(T a_e, PTTransmission a_t, string a_suggestion, ScenarioExceptionInfo a_sei)
//    {
//        //Typically, we may want to run the rest of the logic if the exception happens in a dev environment.
//        //Not enforcing that logic since logging to sentry isn't quite complete

//        if (!(a_e is EmailException)) // emails can't be sent, don't try again, just log it locally.
//        {
//            SendLogEmail(a_e as Exception, a_t, a_suggestion);
//        }

//        if (a_t != null)
//        {
//            m_transmissionErrorCache.GetOrAdd(a_t.TransmissionId, (a_e as Exception).GetExceptionFullMessage());
//        }

//        bool fatal = false;
//        ScenarioExceptionInfo sei = a_sei ?? new ScenarioExceptionInfo();
//        //Handle multiple exceptions thrown together
//        if (a_e is AggregateException agggegate)
//        {
//            foreach (Exception innerException in agggegate.InnerExceptions)
//            {
//                fatal |= LogExceptionDetails(innerException, a_t, sei, a_suggestion);
//            }
//        }
//        else
//        {
//            fatal = LogExceptionDetails(a_e as Exception, a_t, sei, a_suggestion);
//        }

//        //Alert the UI if this user sent the transmission or if this was not transmission related.
//        if (a_t == null || a_t.TransmissionSender != PTTransmissionBase.TransmissionSenderType.PTUser)
//        {
//            // Notify the UI that an exception was logged.
//            FireExceptionOccurredDelegate(a_e as Exception, fatal, a_t is ScenarioBaseT && ((ScenarioBaseT)a_t).ReplayForUndoRedo);
//        }
//    }
//    private bool LogExceptionDetails(Exception a_e, PTTransmission a_t, ScenarioExceptionInfo a_sei, string a_suggestion)
//    {
//        // Pull the transmission out of PTValidationExceptions if needed.
//        if (a_t == null && a_e is TransmissionValidationException)
//        {
//            TransmissionValidationException tve = (TransmissionValidationException)a_e;
//            a_t = tve.transmission;
//        }

//        // Determine which error logger to use.
//        IErrorLogger el = GetLogger(a_e.GetType().FullName, a_t);

//        // Log the exception.
//        if (el != m_fatal && a_t != null && !a_t.LogErrorToUsualLogFile)
//        {
//            el = m_misc;
//        }

//        el.LogException(a_e, a_t, a_suggestion, a_sei);

//        return el == m_fatal;
//    }

//    private IErrorLogger GetLogger(string a_exceptionName, PTTransmission a_t)
//    {
//        if (a_exceptionName == typeof(PTHandleableException).FullName)
//        {
//            if (a_t != null)
//            {
//                return GetLogger(a_t);
//            }

//            if (a_exceptionName == typeof(ImportException).FullName)
//            {
//                return m_ptInterface;
//            }

//            if (a_exceptionName == typeof(AuthorizationException).FullName)
//            {
//                return m_key;
//            }

//            if (a_exceptionName == typeof(ScenarioDetail.SchedulingWarningException).FullName)
//            {
//                return m_schedulingWarnings;
//            }

//            if (a_exceptionName == typeof(ImportApiException).FullName)
//            {
//                return m_api;
//            }

//            return m_ptSystem; //JMC Added to handle null transmission
//        }

//        if (a_exceptionName == typeof(InvalidLogonBaseException).FullName)
//        {
//            return m_failedLogin;
//        }

//        return m_fatal;
//    }

//    private void SendLogEmail(Exception a_e, PTTransmission a_t, string a_suggestion)
//    {
//        if (SystemController.Sys != null)
//        {
//            SystemController.Sys.SendLogEmail(a_e, a_t, a_suggestion);
//        }
//    }

//    public void LogUIException(ExceptionDescriptionInfo a_edi, string a_message)
//    {
//        //Typically, we may want to run the rest of the logic if the exception happens in a dev environment.
//        //Not enforcing that logic since logging to sentry isn't quite complete

//        m_ui.LogException(a_edi, a_message);
//    }

//    public void LogDesyncException(ExceptionDescriptionInfo a_edi, string a_message)
//    {
//        //Typically, we may want to run the rest of the logic if the exception happens in a dev environment.
//        //Not enforcing that logic since logging to sentry isn't quite complete

//        m_desync.LogException(a_edi, a_message);
//    }

//    public void LogInterfaceExceptions(ApplicationExceptionList a_errs, PTTransmission a_t, string a_suggestion, ScenarioExceptionInfo a_sei)
//    {
//        ScenarioExceptionInfo sei = a_sei ?? new ScenarioExceptionInfo();

//        //Typically, we may want to run the rest of the logic if the exception happens in a dev environment.
//        //Not enforcing that logic since logging to sentry isn't quite complete

//        m_transmissionErrorCache.GetOrAdd(a_t.TransmissionId, a_errs.GetExceptionFullMessage());
//        m_ptInterface.LogExceptions(a_errs, a_t, a_suggestion, sei);
//        FireInterfaceExceptionOccuredDelegate(a_t is ScenarioBaseT && ((ScenarioBaseT)a_t).ReplayForUndoRedo);
//    }

//    public void LogSystemExceptions(ApplicationExceptionList a_errs, PTTransmission a_t, string a_suggestion, ScenarioExceptionInfo a_sei)
//    {
//        ScenarioExceptionInfo sei = a_sei ?? new ScenarioExceptionInfo();

//        //Typically, we may want to run the rest of the logic if the exception happens in a dev environment.
//        //Not enforcing that logic since logging to sentry isn't quite complete


//        m_transmissionErrorCache.GetOrAdd(a_t.TransmissionId, a_errs.GetExceptionFullMessage());
//        m_ptSystem.LogExceptions(a_errs, a_t, a_suggestion, sei);
//    }

//    public void LogInterfaceException(Exception a_e, PTTransmission a_t, string a_suggestion, ScenarioExceptionInfo a_sei)
//    {
//        ScenarioExceptionInfo sei = a_sei ?? new ScenarioExceptionInfo();
//        //Typically, we may want to run the rest of the logic if the exception happens in a dev environment.
//        //Not enforcing that logic since logging to sentry isn't quite complete


//        m_transmissionErrorCache.GetOrAdd(a_t.TransmissionId, a_e.GetExceptionFullMessage());
//        m_ptInterface.LogException(a_e, a_t, a_suggestion, sei);
//        FireInterfaceExceptionOccuredDelegate(a_t is ScenarioBaseT && ((ScenarioBaseT)a_t).ReplayForUndoRedo);
//    }

//    public void LogServiceMessage(string a_message, string a_eventType, string a_reason, string a_timeOfEvent, string a_durationSinceStart)
//    {
//        m_ptService.LogMessage(a_message, a_eventType, a_reason, a_timeOfEvent, a_durationSinceStart);
//    }

//    public void LogNotification(string a_message, string a_eventType, string a_reason, string a_timeOfEvent, string a_durationSinceStart)
//    {
//        m_notifications.LogMessage(a_message, a_eventType, a_reason, a_timeOfEvent, a_durationSinceStart);
//    }
//    #endregion

//    #region Log helpers
//    private IErrorLogger GetLogger(PTTransmissionBase a_t)
//    {
//        IErrorLogger el;

//        switch (a_t.TransmissionSender)
//        {
//            case PTTransmissionBase.TransmissionSenderType.ExternalInterface:
//                el = m_externalInterface;
//                break;

//            case PTTransmissionBase.TransmissionSenderType.PTInterface:
//                el = m_ptInterface;
//                break;

//            case PTTransmissionBase.TransmissionSenderType.PTSystem:
//                el = m_ptSystem;
//                break;

//            case PTTransmissionBase.TransmissionSenderType.PTUser:
//                el = m_ptUser;
//                break;

//            default:
//                throw new PTException("An exception occurred while trying to log a different exception. The TransmissionSender type is unknown.");
//        }

//        return el;
//    }

//    public string GetReportedTransmissionErrors(Guid a_transmissionId)
//    {
//        if (m_transmissionErrorCache.TryGetValue(a_transmissionId, out string errorMessage))
//        {
//            return errorMessage;
//        }

//        return null;
//    }
//    #endregion

//    [Obsolete("This method should be removed as it only works with file loggers")]
//    public void LogApiInfo(string a_info)
//    {
//        if (m_apiDiagnostics is FileLogger fileLogger)
//        {
//            fileLogger.LogMessage(a_info);
//        }
//    }

//    #region User Session log in/out
//    public void LogSessionError(Exception a_exception)
//    {
//        m_ptUser.LogException(a_exception, null, null, null);
//    }

//    public void LogSessionStart(UserSession a_userSession)
//    {
//        string logMessage = string.Format("Session created for user with Id {0}.".Localize(), a_userSession.UserId);

//        m_ptUser.LogMessage(logMessage, "", "", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"), "");
//    }

//    public void LogSessionEnd(UserSession a_userSession)
//    {
//        string durationSinceStart = (DateTime.UtcNow - a_userSession.CreationDate).ToString(@"dd\.hh\:mm\:ss");
//        string messageStart = string.Format("Session ended for user with Id {0}.".Localize(), a_userSession.UserId);
//        string logoutReason = a_userSession.SessionLifecycle.GetLogoutReason(a_userSession.ReceiveTimingMember.NbrOfReceiveCalls);
//        string logMessage = $"{messageStart} {logoutReason}";

//        m_ptUser.LogMessage(logMessage, "", "", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"), durationSinceStart);
//    }
//    #endregion
//}