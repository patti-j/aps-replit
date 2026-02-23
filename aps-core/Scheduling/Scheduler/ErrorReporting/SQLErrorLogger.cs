using System.Data;
using System.Text;

using Microsoft.Data.SqlClient;

using PT.Common.Exceptions;
using PT.Common.File;
using PT.Common.Sql.SqlServer;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler.ErrorReporting;

/// <summary>
/// Manages the logging of errors on the database.
/// Implements IErrorLogger Interface
/// string GetLogTitle();
/// void Clear();
/// string GetLogContents(bool a_filtered);
/// void LogException(Exception a_e, PTTransmission a_t, string a_suggestion);
/// void LogException(PT.Common.File.ExceptionDescriptionInfo a_edi, string a_message);
/// void LogExceptions(ApplicationExceptionList a_errs, PTTransmission a_t, string a_suggestion);
/// </summary>
public class SQLErrorLogger : IErrorLogger
{
    /// <summary>
    /// Where to store instance values and SQL connection data.
    /// </summary>
    private readonly string m_instanceName;

    private readonly string m_softwareVersion;
    private readonly string m_errorSQLConnection;
    private string m_logType;
    private readonly DatabaseConnections m_databaseConnection;

    /// <summary>
    /// Exception Headers
    /// </summary>
    private readonly string m_START_ENTRY_TEXT = "....... START ENTRY ";

    private readonly string m_TRAIL_TEXT = ".......";
    private readonly string m_MESSAGE_TEXT = "*** Message ***";
    private readonly string m_EXCEPTION_MESSAGE_TEXT = "***   Exception.Message   ***";
    private readonly string m_EXCEPTION_STACK_TRACE_TEXT = "*** Exception.StackTrace ***";
    private readonly string m_EXCEPTION_SOURCE_TEXT = "*** Exception.Source ***";
    private readonly string m_EXCEPTION_INNER_EXCEPTION_MESSAGE_TEXT = "*** Exception.InnerException.Message ***";
    private readonly string m_EXCEPTION_INNER_EXCEPTION_STACK_TRACE_TEXT = "*** Exception.InnerException.StackTrace ***";
    private readonly string m_INSERT_LOG = "INSERT INTO InstanceLogs (InstanceName, SoftwareVersion, TypeName, Message, StackTrace, Source, InnerExceptionMessage, InnerExceptionStackTrace, LogType, HeaderMessage) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')";
    private readonly string m_INSERT_SERVICE_LOG = "INSERT INTO InstanceServiceLogs (InstanceName, SoftwareVersion, Message, EventType, Reason, TimeOfEvent, DurationSinceStart) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')";

    public string GetLogTitle()
    {
        return m_logType;
    }

    public void SetLogType(string a_logType)
    {
        m_logType = a_logType;
    }
    /// <summary>
    /// Constructor for system error attributes
    /// </summary>
    /// <param name="a_instanceName"></param>
    /// <param name="a_softwareVersion"></param>
    /// <param name="a_sqlConnectionString"></param>
    /// <param name="a_logType"></param>
    public SQLErrorLogger(string a_instanceName, string a_softwareVersion, string a_sqlConnectionString, string a_logType) : this(a_instanceName, a_softwareVersion, a_sqlConnectionString)
    {
        m_logType = a_logType;
        m_databaseConnection = new DatabaseConnections(a_sqlConnectionString);
    }
    public SQLErrorLogger(string a_instanceName, string a_softwareVersion, string a_sqlConnectionString)
    {
        m_instanceName = a_instanceName.CleanString();
        m_softwareVersion = a_softwareVersion.CleanString();
        m_errorSQLConnection = a_sqlConnectionString;
        m_databaseConnection = new DatabaseConnections(a_sqlConnectionString);

        if (m_databaseConnection.IsValid())
        {
            CreateLoggingTables(m_databaseConnection);
        }
    }

    #region Exception Logging
    /// <summary>
    /// Checks database for and creates InstanceLogs table for error logging.
    /// </summary>
    /// <param name="a_databaseConnection"></param>
    private static void CreateLoggingTables(DatabaseConnections a_databaseConnection)
    {
        string createTable = @"IF OBJECT_ID('InstanceLogs', 'U') IS NULL CREATE TABLE InstanceLogs (InstanceName nvarchar(MAX),  SoftwareVersion nvarchar(MAX), TypeName nvarchar(MAX), Message nvarchar(MAX), StackTrace nvarchar(MAX), Source nvarchar(MAX),
                                                        InnerExceptionMessage nvarchar(MAX), InnerExceptionStackTrace nvarchar(MAX), LogType nvarchar(MAX), HeaderMessage nvarchar(MAX), Timestamp datetime NULL DEFAULT (GETDATE()))";

        a_databaseConnection.SendSQLTransaction(new[] { createTable });

        createTable = @"IF OBJECT_ID('InstanceServiceLogs', 'U') IS NULL CREATE TABLE InstanceServiceLogs (InstanceName nvarchar(MAX),  SoftwareVersion nvarchar(MAX), Message nvarchar(MAX), EventType nvarchar(MAX), Reason nvarchar(MAX), TimeOfEvent nvarchar(MAX), DurationSinceStart nvarchar(MAX), Timestamp datetime NULL DEFAULT (GETDATE()))";

        a_databaseConnection.SendSQLTransaction(new[] { createTable });
    }
    /// <summary>
    /// Log an error for the system that was triggered by a transmission.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="t"></param>
    public void LogException(Exception a_e, PTTransmission a_t, string a_suggestion, ScenarioExceptionInfo a_sei)
    {
        Log(a_e, a_t, a_suggestion, a_sei);
    }

    /// <summary>
    /// Log an error for the system to the database
    /// </summary>
    /// <param name="a_edi"></param>
    /// <param name="a_message"></param>
    public void LogException(ExceptionDescriptionInfo a_edi, string a_message)
    {
        string cmdAdd = string.Format(m_INSERT_LOG,
            m_instanceName,
            m_softwareVersion,
            a_edi.GetTypeName.CleanString(),
            a_edi.Message.CleanString(),
            a_edi.StackTrace.CleanString(),
            a_edi.Source.CleanString(),
            "",
            "",
            m_logType,
            a_message.CleanString()); // Inner exception content left blank as the message and stack trace show the entire tree
        try
        {
            m_databaseConnection.SendSQLTransaction(new[] { cmdAdd });
        }
        catch (Exception ex)
        {
            throw new PTException($"SQL Logger failed to add entry: {a_edi.Message}", ex);
        }
    }

    /// <summary>
    /// Log an error for the system.
    /// </summary>
    /// <param name="a_errs"></param>
    /// <param name="a_t"></param>
    /// <param name="a_suggestion"></param>
    public void LogExceptions(ApplicationExceptionList a_errs, PTTransmission a_t, string a_suggestion, ScenarioExceptionInfo a_sei)
    {
        string headerText = GetTransmissionExceptionHeader(a_t, a_suggestion);

        // Log the exceptions.
        ApplicationExceptionList.Node node = a_errs.First;
        while (node != null)
        {
            LogException(node.Data, headerText);
            node = node.Next;
        }
    }

    /// <summary>
    /// Log an exception that was triggered by a transmission.
    /// </summary>
    private void Log(Exception a_e, PTTransmission a_t, string a_suggestion, ScenarioExceptionInfo a_sei)
    {
        string errorText = GetTransmissionExceptionHeader(a_t, a_suggestion);

        // Log the exception.
        ExceptionDescriptionInfo edi = new (a_e);
        LogException(edi, errorText);
    }

    /// <summary>
    /// Build the header error description text.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="suggestion"></param> TODO: nothing currently uses this param, could be removed if no future intention
    /// <returns></returns>
    public static string GetTransmissionExceptionHeader(PTTransmission t, string suggestion)
    {
        // Create a description of the problem. 
        const string NONE = "<none>";

        string transmissionType = NONE;
        string transmissinoNbr = NONE;
        string instigator = NONE;
        string recordingPath = NONE;

        if (t != null)
        {
            transmissionType = t.GetType().ToString();
            transmissinoNbr = t.TransmissionNbr.ToString();
            instigator = t.Instigator.ToString();
            if (t.Recording)
            {
                recordingPath = t.RecordingFilePath;
            }
        }

        if (suggestion == null)
        {
            suggestion = "";
        }
        else if (suggestion.Trim() != "") //don't bother with the 'suggestion' text if no suggestion.
        {
            suggestion = "Suggestion: " + suggestion;
        }


        string errorText = "";
        if (transmissionType.Length > 0 && transmissionType != NONE)
        {
            errorText += string.Format("TransmissionType={0}.", transmissionType);
        }

        if (transmissinoNbr.Length > 0 && transmissinoNbr != NONE)
        {
            errorText += string.Format("TransmissionNbr={0}.", transmissinoNbr).TrimStart();
        }

        if (!string.IsNullOrEmpty(recordingPath) && recordingPath != NONE)
        {
            errorText += string.Format("RecordingFile={0}.", recordingPath);
        }

        if (instigator.Length > 0 && instigator != NONE)
        {
            errorText += string.Format("Instigator={0}.", instigator);
        }

        if (suggestion.Length > 0 && suggestion != NONE)
        {
            errorText += string.Format("Suggestion={0}.", suggestion);
        }

        return errorText;
    }
    #endregion Exception Logging

    #region Service Message Logging
    public void LogMessage(string a_message, string a_eventType, string a_reason, string a_timeOfEvent, string a_durationSinceStart)
    {
        string cmdAdd = string.Format(m_INSERT_SERVICE_LOG, m_instanceName, m_softwareVersion, a_message.CleanString(), a_eventType.CleanString(), a_reason.CleanString(), a_timeOfEvent, a_durationSinceStart);
        try
        {
            m_databaseConnection.SendSQLTransaction(new[] { cmdAdd });
        }
        catch (Exception ex)
        {
            string error = ex.Message;
        }
    }
    #endregion

    /// <summary>
    /// Clear the specified logs of matching instance name and version from database.
    /// </summary>
    /// <param name="a_logType"></param>
    public void Clear()
    {
        string cmdClear = $"DELETE FROM InstanceLogs WHERE InstanceName = '{m_instanceName}' AND SoftwareVersion = '{m_softwareVersion}' AND LogType = '{m_logType.CleanString()}'";
        try
        {
            m_databaseConnection.SendSQLTransaction(new[] { cmdClear });
        }
        catch (Exception ex)
        {
            string error = ex.Message;
        }
    }

    /// <summary>
    /// Retrieves all the text in the specified instance.
    /// </summary>
    /// <param name="a_filtered">If true then only the text for users is shown, filtering stack details, etc.</param>
    /// <returns></returns>
    public string GetLogContents(bool a_filtered, bool a_startInUserText = false)
    {
        string sqlCmd = $"SELECT TOP (1000) * FROM InstanceLogs WHERE InstanceName = '{m_instanceName}' AND SoftwareVersion = '{m_softwareVersion}' AND LogType = '{m_logType.CleanString()}' " +
                        $" ORDER BY Timestamp DESC";
        string contents = "";
        if (!string.IsNullOrWhiteSpace(m_errorSQLConnection))
        {
            try
            {
                DataTable result = m_databaseConnection.SelectSQLTable(sqlCmd);
                contents = GetFullMessage(result, a_filtered, a_startInUserText);
            }
            catch (SqlException ex)
            {
                return ex.Message;
            }
        }

        return contents;
    }

    /// <summary>
    /// Returns all the text in specified instance based on filtering requirement.
    /// </summary>
    /// <param name="a_exceptions">The datatable of database contents for specified instance.</param>
    /// <param name="a_filtered">If true then only the text for users is shown, filtering stack details, etc.</param>
    /// <returns></returns>
    private string GetFullMessage(DataTable a_exceptions, bool a_filtered, bool a_startInUserText = false)
    {
        StringBuilder sb = new ();

        foreach (DataRow row in a_exceptions.Rows)
        {
            string instanceName = row[0].ToString();
            string softwareVersion = row[1].ToString();
            string typeName = row[2].ToString();
            string exceptionMessage = row[3].ToString();
            string stackTrace = row[4].ToString();
            string source = row[5].ToString();
            string innerExpceptionMessage = row[6].ToString();
            string innerExpStackTrace = row[7].ToString();
            string logType = row[8].ToString();
            string headerMessage = row[9].ToString();
            DateTime timestamp = row["Timestamp"] is DateTime ? (DateTime)row["Timestamp"] : default;

            sb.AppendFormat(Environment.NewLine + "{0}   {1}       PT Version={3}  PT Serialization Version={4} {2}",
                m_START_ENTRY_TEXT,
                timestamp,
                m_TRAIL_TEXT,
                AssemblyVersionChecker.GetAssemblyVersion(),
                Serialization.VersionNumber);

            if (!string.IsNullOrEmpty(headerMessage))
            {
                sb.Append(Environment.NewLine + m_MESSAGE_TEXT + Environment.NewLine);
                sb.Append(headerMessage);
            }

            if (exceptionMessage.Length > 0)
            {
                sb.Append(Environment.NewLine + Environment.NewLine + m_EXCEPTION_MESSAGE_TEXT + Environment.NewLine);
                string message = typeName + ": " + Environment.NewLine + exceptionMessage + Environment.NewLine;
                sb.Append(message);
            }

            if (!a_filtered && stackTrace.Length > 0)
            {
                sb.Append(Environment.NewLine + m_EXCEPTION_STACK_TRACE_TEXT + Environment.NewLine);
                sb.Append(stackTrace);
            }

            if (!a_filtered && source.Length > 0)
            {
                sb.Append(Environment.NewLine + m_EXCEPTION_SOURCE_TEXT + Environment.NewLine);
                sb.Append(source);
            }

            if (innerExpceptionMessage.Length > 0)
            {
                sb.Append(Environment.NewLine + Environment.NewLine + m_EXCEPTION_INNER_EXCEPTION_MESSAGE_TEXT + Environment.NewLine);
                sb.Append(innerExpceptionMessage);
            }

            if (!a_filtered && innerExpStackTrace.Length > 0)
            {
                sb.Append(Environment.NewLine + Environment.NewLine + m_EXCEPTION_INNER_EXCEPTION_STACK_TRACE_TEXT + Environment.NewLine);
                sb.Append(innerExpStackTrace);
            }

            //space between errors
            sb.Append(Environment.NewLine);
        }

        return sb.ToString();
    }
}