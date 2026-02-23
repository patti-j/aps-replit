using Microsoft.Data.SqlClient;
using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.File;
using PT.Common.Localization;
using PT.Common.Sql.SqlServer;
using PT.Common.Threading;
using PT.ERPTransmissions;
using PT.PackageDefinitions.Settings;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using PT.Transmissions2;
using System.Data;
using PT.APIDefinitions.RequestsAndResponses.AuditObjects;

namespace PT.Scheduler;

internal class TransmissionLogger
{
    private ISystemLogger m_systemLogger;
    private string m_logDBConnectionString = "";

    public string LogDBConnectionString
    {
        get => m_logDBConnectionString;
        set => m_logDBConnectionString = value;
    }

    private BaseId m_instanceId;

    private string InstanceId
    {
        get => m_logDBConnectionString;
        set => m_logDBConnectionString = value;
    }

    public bool IsValidConnection { get; private set; }

    private string m_instanceName;
    private string m_softwareVersion;
    public TransmissionLogger(ISystemLogger a_systemLogger)
    {
        m_systemLogger = a_systemLogger;
    }

    public void SetTransmissionLoggingVals(string a_connectionString, string a_instanceName, string a_softwareVersion)
    {
        LogDBConnectionString = a_connectionString;
        m_instanceName = a_instanceName.CleanString();
        m_softwareVersion = a_softwareVersion.CleanString();

        CheckAndCreateTransmissionLogTable();
    }

    private void CheckAndCreateTransmissionLogTable()
    {
        if (string.IsNullOrWhiteSpace(LogDBConnectionString) ||
            !new DatabaseConnections(LogDBConnectionString).IsValid())
        {
            // AuditDb not in use; calls to LogTransmissionToSQL will not run.
            IsValidConnection = false;
            return;
        }

        using (SqlConnection conn = new (LogDBConnectionString))
        {
            DataTable logTable = CreateTransmissionLoggerDataTable();
            try
            {
                conn.Open();
                SyncLogTableSchema(logTable, conn);

                IsValidConnection = true;
                return;
            }
            catch (SqlException e)
            {
                PTException ptException = new PTException("4074", new object[] { e.Server, "TransmissionLogger", e.Message });
                m_systemLogger.LogException(ptException.GenerateDescriptionInfo(), ELogClassification.PtSystem, true);
            }
            catch (Exception e)
            {
                PTException ptException = new PTException("4074", new object[] { "", "TransmissionLogger", e.Message });
                m_systemLogger.LogException(ptException.GenerateDescriptionInfo(), ELogClassification.PtSystem, true);
            }

            IsValidConnection = false;
        }
    }

    private static void SyncLogTableSchema(DataTable logTable, SqlConnection conn)
    {
        DataSet dataSet = new DataSet();
        dataSet.Tables.Add(logTable);
        DatabaseSynchronizer.AlterDbStructureToMatchDataSet(conn, dataSet, false);
    }

    private static DataTable CreateTransmissionLoggerDataTable()
    {
        DataTable logTable = new () { TableName = "TransmissionLog" };
        logTable.Columns.Add("InstanceName", typeof(string));
        logTable.Columns.Add("InstanceVersion", typeof(string));
        logTable.Columns.Add("ScenarioName", typeof(string));
        logTable.Columns.Add("Username", typeof(string));
        logTable.Columns.Add("Description", typeof(string));
        logTable.Columns.Add("ScenarioType", typeof(string));
        logTable.Columns.Add("TransmissionType", typeof(string));
        logTable.Columns.Add("Details", typeof(string));
        logTable.Columns.Add("Timestamp", typeof(DateTime));
        logTable.Columns.Add("TransmissionNumber", typeof(long));
        return logTable;
    }

    /// <summary>
    /// Logs transmission data to SQL if Logger successfully initialized.
    /// </summary>
    /// <param name="t"></param>
    public void LogTransmissionToSQL(PTTransmission t)
    {
        if (!IsValidConnection)
        {
            // Could not connect to the database on initialization.
            return;
        }

        TransmissionLog logEntry = null;

        //Insert a row for 1 scenario
        if (t is ScenarioIdBaseT st)
        {
            using (ObjectAccess<ScenarioManager> sm = SystemController.Sys.ScenariosLock.EnterRead())
            {
                Scenario scenario;
                switch (st.Destination)
                {
                    case ScenarioIdBaseT.EDestinations.BasedOnScenarioId:
                        scenario = sm.Instance.Find(st.ScenarioId);
                        break;
                    case ScenarioIdBaseT.EDestinations.ToLiveScenario:
                    default:
                        scenario = sm.Instance.GetFirstProductionScenario();
                        break;
                }

                string scenarioType;
                using (scenario.ScenarioSummaryLock.TryEnterRead(out ScenarioSummary ss, AutoExiter.THREAD_TRY_WAIT_MS))
                {
                    scenarioType = ss.ScenarioSettings.LoadSetting(new ScenarioPlanningSettings()).Production ? "Production" : "What-If";
                }

                logEntry = new TransmissionLog(scenario.Id, scenario.Name, t.Instigator, t.Description, scenarioType, t.GetType().ToString(), t.TimeStamp.ToDateTime(), t.TransmissionNbr);
            }
        }
        //Insert a row for 1 scenario
        else if (t is IScenarioIdBaseT ist)
        {
            using (ObjectAccess<ScenarioManager> sm = SystemController.Sys.ScenariosLock.EnterRead())
            {
                Scenario scenario = sm.Instance.Find(ist.ScenarioId);
                
                string scenarioType;
                using (scenario.ScenarioSummaryLock.TryEnterRead(out ScenarioSummary ss, AutoExiter.THREAD_TRY_WAIT_MS))
                {
                    scenarioType = ss.ScenarioSettings.LoadSetting(new ScenarioPlanningSettings()).Production ? "Production" : "What-If";
                }

                logEntry = new TransmissionLog(scenario.Id, scenario.Name, t.Instigator, t.Description, scenarioType, t.GetType().ToString(), t.TimeStamp.ToDateTime(), t.TransmissionNbr);
            }
        }
        //Insert a row for each scenario
        else if (t is ERPTransmission)
        {
            using (ObjectAccess<ScenarioManager> sm = SystemController.Sys.ScenariosLock.EnterRead())
            {
                for (int i = 0; i < sm.Instance.LoadedScenarioCount; i++)
                {
                    Scenario s = sm.Instance.GetByIndex(i);

                    string scenarioType;
                    using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                    {
                        scenarioType = ss.ScenarioSettings.LoadSetting(new ScenarioPlanningSettings()).Production ? "Production" : "What-If";
                    }

                    logEntry = new TransmissionLog(s.Id, s.Name, t.Instigator, t.Description, scenarioType, t.GetType().ToString(), t.TimeStamp.ToDateTime(), t.TransmissionNbr);
                }
            }
        }
        //Insert a single row for no specific scenario
        else if (t is SystemStateSwitchT || t is SystemMessageT || t is WorkspaceSharedDeleteT || t is WorkspaceTemplateUpdateT)
        {
            logEntry = new TransmissionLog(BaseId.NULL_ID, "", t.Instigator, t.Description, "", t.GetType().ToString(), t.TimeStamp.ToDateTime(), t.TransmissionNbr);
        }
        else if (t is UserBaseT)
        {
            logEntry = new TransmissionLog(BaseId.NULL_ID, "", t.Instigator, t.Description, "", t.GetType().ToString(), t.TimeStamp.ToDateTime(), t.TransmissionNbr);
        }

        if (logEntry != null)
        {
            m_systemLogger.LogTransmission(logEntry);
        }
    }
}