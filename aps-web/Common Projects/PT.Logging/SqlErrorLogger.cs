using System.Data;
using System.Text;

using PT.Common;
using PT.Common.Sql.SqlServer;
using PT.Logging;
using PT.Logging.Entities;
using PT.Logging.Interfaces;

namespace WebAPI.Services.Logging;

/// <summary>
/// Logs Errors to the configured SQL Database for a particular instance.
/// This logic is mostly ported from SQLErrorLogger.cs in core. Note that a related class, ErrorReporter, is responsible for ensuring the appropriate tables exist and have the appropriate schema for the instance.
/// TODO: While for MVP, we can rely on the instance starting up to ensure those tables exist, we'll probably want to move database management methods to the webapp.
/// </summary>
public class SqlErrorLogger : IErrorLogger, IAuditLogger
{
    const string c_errorLogsTableName = "InstanceLogs";
    const string c_auditlogsTableName = "AuditLogs";

    private readonly string c_insertErrorCommand = $"INSERT INTO {c_errorLogsTableName} " +
                                    "(InstanceName, SoftwareVersion, TypeName, Message, StackTrace, Source, " +
                                    "InnerExceptionMessage, InnerExceptionStackTrace, LogType, HeaderMessage, Timestamp) " +
                                    "VALUES (" +
                                    "@InstanceName, @SoftwareVersion, @TypeName, @Message, @StackTrace, @Source, " +
                                    "'', '', @LogType, @HeaderMessage, @TimeStamp)";

    private readonly string c_insertAuditCommand = $"INSERT INTO {c_auditlogsTableName} " +
                                     "(InstanceName, SoftwareVersion, Message, TimeStamp) " +
                                      "VALUES (" +
                                      "@InstanceName, @SoftwareVersion, @Message, @TimeStamp)";


    private readonly InstanceLoggingContext m_context;
    private readonly DatabaseConnections m_databaseConnection;

    public SqlErrorLogger(InstanceLoggingContext a_context)
    {
        m_context = a_context;
        m_databaseConnection = new DatabaseConnections(a_context.InstanceAuditLogConnectionString);
    }

    public void EnsureErrorLogConfigured()
    {
        // TODO: Code taken from ErrorReporter.cs. Since the instance doesn't need to care about logging any more, this can probably be the primary source going forward.
        if (!m_databaseConnection.IsValid())
        {
            throw new ArgumentException("Connection is not valid");
        }

        // TODO: going forward, we should have a mechanism to update any fields in an existing table to match the current schema (the below just creates it if non-existant).
        // TODO: This was what's been used so far, and it isn't certain we're going to continue storing to the db, so it's fine for now.
        // TODO: To add it, I suggest we either (a) utilize the DatabaseSynchronizer class from PT.Common (requires working with datasets, starting to go obsolete, but we already have functionality to update a schema regardless of current db state)
        // TODO: or (b) use EF (managing each subsequent change as a migration. These are more strict in where the db begins, but we could probably treat the schema below as almost certainly being what any existing db would look like, make that the first migration, and manually update the EFMigrations table if we find that structure.)

        string createErrorsLogTable = $@"IF OBJECT_ID('{c_errorLogsTableName}', 'U') IS NULL CREATE TABLE {c_errorLogsTableName} (InstanceName nvarchar(MAX),  SoftwareVersion nvarchar(MAX), TypeName nvarchar(MAX), Message nvarchar(MAX), StackTrace nvarchar(MAX), Source nvarchar(MAX),
                                                    InnerExceptionMessage nvarchar(MAX), InnerExceptionStackTrace nvarchar(MAX), LogType nvarchar(MAX), HeaderMessage nvarchar(MAX), Timestamp datetime NULL DEFAULT (GETDATE()))";

        m_databaseConnection.SendSQLTransaction(new[] { createErrorsLogTable });

        // The Webapp is not currently logging to this table, but it is part of the existing logging db schema. We could add it here for backwards compatability, but instances that use it should generate it on startup.
        //string createServiceLogsTable = @"IF OBJECT_ID('InstanceServiceLogs', 'U') IS NULL CREATE TABLE InstanceServiceLogs (InstanceName nvarchar(MAX),  SoftwareVersion nvarchar(MAX), Message nvarchar(MAX), EventType nvarchar(MAX), Reason nvarchar(MAX), TimeOfEvent nvarchar(MAX), DurationSinceStart nvarchar(MAX), Timestamp datetime NULL DEFAULT (GETDATE()))";
        //m_databaseConnection.SendSQLTransaction(new[] { createServiceLogsTable });
    }

    public async Task LogExceptionAsync(ErrorLog a_ex)
    {
        SqlCommandHelper command = BuildLogErrorCommand(a_ex);

        try
        {
            await m_databaseConnection.SendSQLTransactionAsync([command]);
        }
        catch (Exception ex)
        {
            // TODO: handle retry, log to sentry?
        }
    }

    private SqlCommandHelper BuildLogErrorCommand(ErrorLog a_ex)
    {
        SqlCommandHelper commandHelper = new SqlCommandHelper();
        commandHelper.CommandText = c_insertErrorCommand; // Inner exception content left blank as the message and stack trace show the entire tree

        commandHelper.AddParam("@InstanceName", m_context.InstanceName);
        commandHelper.AddParam("@SoftwareVersion", m_context.SoftwareVersion);
        commandHelper.AddParam("@TypeName", a_ex.TypeName.CleanString());
        commandHelper.AddParam("@Message", a_ex.Message.CleanString());
        commandHelper.AddParam("@StackTrace", a_ex.StackTrace.CleanString());
        commandHelper.AddParam("@Source", a_ex.Source.CleanString());
        commandHelper.AddParam("@LogType", a_ex.LogType.CleanString());
        commandHelper.AddParam("@HeaderMessage", a_ex.HeaderMessage.CleanString());
        commandHelper.AddParam("@TimeStamp", a_ex.TimeStamp.ToString());

        return commandHelper;
    }

    /// <summary>
    /// Aggregates multiple exceptions so they can all be logged in one SQlConnection.
    /// </summary>
    /// <param name="a_errs"></param>
    /// <returns></returns>
    public async Task LogExceptionsAsync(List<ErrorLog> a_errs)
    {
        SqlCommandHelper[] commands = new SqlCommandHelper[a_errs.Count];
        for (int i = 0; i < a_errs.Count; i++)
        {
            commands[i] = BuildLogErrorCommand(a_errs[i]);
        }

        try
        {
            await m_databaseConnection.SendSQLTransactionAsync(commands);
        }
        catch (Exception ex)
        {
            // TODO: handle retry, log to sentry?
        }

    }

    public void EnsureAuditLogConfigured()
    {
        if (!m_databaseConnection.IsValid())
        {
            throw new ArgumentException("Connection is not valid");
        }

        // TODO: going forward, we should have a mechanism to update any fields in an existing table to match the current schema (the below just creates it if non-existant).
        // TODO: This was what's been used so far, and it isn't certain we're going to continue storing to the db, so it's fine for now.
        // TODO: To add it, I suggest we either (a) utilize the DatabaseSynchronizer class from PT.Common (requires working with datasets, starting to go obsolete, but we already have functionality to update a schema regardless of current db state)
        // TODO: or (b) use EF (managing each subsequent change as a migration. These are more strict in where the db begins, but we could probably treat the schema below as almost certainly being what any existing db would look like, make that the first migration, and manually update the EFMigrations table if we find that structure.)

        string createAuditLogTable = $@"IF OBJECT_ID('{c_auditlogsTableName}', 'U') IS NULL CREATE TABLE {c_auditlogsTableName} (InstanceName nvarchar(MAX),  SoftwareVersion nvarchar(MAX), Message nvarchar(MAX), Timestamp datetime NULL DEFAULT (GETDATE()))";


        m_databaseConnection.SendSQLTransaction(new[] { createAuditLogTable });

    }

    public async Task LogAuditAsync(AuditLog a_auditDto)
    {
        SqlCommandHelper command = BuildLogAuditCommand(a_auditDto);

        try
        {
            await m_databaseConnection.SendSQLTransactionAsync([command]);
        }
        catch (Exception ex)
        {
            // TODO: handle retry, log to sentry?
        }
    }

    private SqlCommandHelper BuildLogAuditCommand(AuditLog a_auditDto)
    {
        SqlCommandHelper commandHelper = new SqlCommandHelper();
        commandHelper.CommandText = c_insertAuditCommand; // Inner exception content left blank as the message and stack trace show the entire tree

        commandHelper.AddParam("@InstanceName", m_context.InstanceName);
        commandHelper.AddParam("@SoftwareVersion", m_context.SoftwareVersion);
        commandHelper.AddParam("@Message", a_auditDto.Description.CleanString());
        commandHelper.AddParam("@TimeStamp", a_auditDto.TimeStamp.ToString());

        return commandHelper;
    }

    public async Task<string> GetLogContentsAsync(GetLogsRequest a_logsRequest)
    {
        string dbTableName;
        string contents = "";
        switch (a_logsRequest.ELogKind)
        {
            case ELogKind.Audit:
            {
                dbTableName = c_auditlogsTableName;
                string sqlCmd = $"SELECT TOP (1000) * FROM {dbTableName} WHERE InstanceName = '{m_context.InstanceName}' AND SoftwareVersion = '{m_context.SoftwareVersion}' " +
                                $" ORDER BY Timestamp DESC";
                if (!string.IsNullOrWhiteSpace(m_context.InstanceAuditLogConnectionString))
                {
                    try
                    {
                        DataTable result = m_databaseConnection.SelectSQLTable(sqlCmd);
                        contents = GetFullMessageFromAuditTable(result, a_logsRequest.Filtered, a_logsRequest.StartInUserText);
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }
                break;
            }
            case ELogKind.Error:
            default:
            {
                dbTableName = c_errorLogsTableName;
                string sqlCmd = $"SELECT TOP (1000) * FROM {dbTableName} WHERE InstanceName = '{m_context.InstanceName}' AND SoftwareVersion = '{m_context.SoftwareVersion}' AND LogType = '{a_logsRequest.LogTypeName.CleanString()}' " +
                                $" ORDER BY Timestamp DESC";
                if (!string.IsNullOrWhiteSpace(m_context.InstanceAuditLogConnectionString))
                {
                    try
                    {
                        DataTable result = m_databaseConnection.SelectSQLTable(sqlCmd);
                        contents = GetFullMessageFromErrorTable(result, a_logsRequest.Filtered, a_logsRequest.StartInUserText);
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }

                break;
            }
        }

        

        return contents;
    }

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



    /// <summary>
    /// Returns all the text in specified instance based on filtering requirement.
    /// </summary>
    /// <param name="a_exceptions">The datatable of database contents for specified instance.</param>
    /// <param name="a_filtered">If true then only the text for users is shown, filtering stack details, etc.</param>
    /// <returns></returns>
    private string GetFullMessageFromErrorTable(DataTable a_exceptions, bool a_filtered, bool a_startInUserText = false)
    {
        StringBuilder sb = new();

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

            sb.AppendFormat(Environment.NewLine + "{0}   {1}       PT Version={3}  PT Serialization Version={4} {2}",
                m_START_ENTRY_TEXT,
                PTDateTime.UtcNow,
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
    
    private string GetFullMessageFromAuditTable(DataTable a_table, bool a_filtered, bool a_startInUserText = false)
    {
        StringBuilder sb = new();

        foreach (DataRow row in a_table.Rows)
        {
            string instanceName = row[0].ToString();
            string softwareVersion = row[1].ToString();
            string message = row[2].ToString();
            string timestamp = row[3].ToString();

            sb.AppendFormat(Environment.NewLine + "{0}   {1}       PT Version={3}   {2}",
                m_START_ENTRY_TEXT,
                timestamp,
                m_TRAIL_TEXT,
                softwareVersion);

            if (!string.IsNullOrEmpty(message))
            {
                sb.Append(Environment.NewLine + m_MESSAGE_TEXT + Environment.NewLine);
                sb.Append(message);
            }

            //space between errors
            sb.Append(Environment.NewLine);
        }

        return sb.ToString();
    }


}