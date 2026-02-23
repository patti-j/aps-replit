using System.Data;

using Microsoft.Data.SqlClient;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.Common.Http.Json;
using PT.ERPTransmissions;
using PT.ImportDefintions;
using PT.ImportDefintions.RequestsAndResponses;
using PT.PackageDefinitions.Settings;
using PT.Scheduler;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.Transmissions;

namespace PT.PlanetTogetherAPI.Importing;

public class ImportingService : IImportingService
{
    private readonly ImportSettingsSaver m_settingsSaver;
    private ImportSettings m_settings;
    private readonly ImportUtilities m_utility;

    private ImportSettings m_testSettings;
    private BaseId m_instigator;
    private Type m_typeToRun;
    private BaseId m_targetScenarioId;
    private bool m_locked;
    private string m_lockedUserName;
    private DateTime m_lockedTime;
    private bool m_testOnly;
    private ScenarioExceptionInfo m_sei;
    private readonly Lockee m_lockee = new ();

    private ImportStatusMessage m_importStatusMessage;

    public ImportingService(ImportUtilities a_utility)
    {
        m_utility = a_utility;
        InstanceSettingsEntity instance = m_utility.GetInstance();
        m_settingsSaver = new ImportSettingsSaver(instance.ServicePaths.InterfaceFileNameWithFullPath, m_utility);
        m_settings = m_settingsSaver.LoadSettings();
    }

    private class Lockee
    {
        public bool locked;
    }

    private void Lock(string a_userName)
    {
        if (m_locked)
        {
            throw new InterfaceLockedException(m_lockedUserName, m_lockedTime);
        }

        m_locked = true;
        m_lockedUserName = a_userName;
        m_lockedTime = DateTime.UtcNow;
    }

    private void Unlock()
    {
        m_locked = false;
        m_lockedUserName = "";
    }

    private void RunImportThread()
    {
        ApplicationExceptionList ael = new ();

        try
        {
            if (PTSystem.EnableDiagnostics)
            {
                m_utility.LogMessage("Import Logging\nImportingService: running import thread.");
            }

            SendPerformImportStartedTransmission();
            Importer importer = new (m_testSettings, m_utility, m_importStatusMessage);
            if (m_typeToRun != null)
            {
                ael = importer.RunImport(m_testOnly, m_typeToRun, m_instigator, m_targetScenarioId, m_sei);
            }
            else
            {
                ael = importer.RunImport(m_testOnly, m_instigator, m_targetScenarioId, m_sei);
            }
        }
        catch (Exception e)
        {
            //Add any exception to the list so it's displayed in the UI.
            ael.Add(e);
        }
        finally
        {
            lock (m_lockee)
            {
                m_lockee.locked = false;
            }
        }

        try
        {
            SendPerformImportCompletedT(ael);
        }
        catch (Exception e)
        {
            m_utility.LogImporterException(e);
        }
    }

    private void SendPerformImportStartedTransmission()
    {
        PerformImportStartedT t = new ();
        t.ImportingInstigator = m_instigator;
        SystemController.ClientSession.SendClientAction(t);
    }

    private void SendPerformImportCompletedT(ApplicationExceptionList a_exceptions)
    {
        PerformImportCompletedT t = new ();
        t.ImportingInstigator = m_instigator;
        t.Exceptions = a_exceptions;
        SystemController.ClientSession.SendClientAction(t);
    }

    private IDbConnection GetConnection()
    {
        ErpDatabase erpSettings = m_utility.GetErpSettings();
        //if (erpSettings.ConnectionType == ErpDatabase.EConnectionTypes.ODBC)
        //{
        //    return new OdbcConnection(erpSettings.ConnectionString);
        //}
        //if (erpSettings.ConnectionType == ErpDatabase.EConnectionTypes.OLEDB)
        //{
        //    return new OleDbConnection(erpSettings.ConnectionString);
        //}
        //if (erpSettings.ConnectionType == ErpDatabase.EConnectionTypes.ORACLE)
        //{
        //    return new OracleConnection(erpSettings.ConnectionString);
        //}

        return new SqlConnection(erpSettings.ConnectionString);
    }

    public DataTableJson GetBrowseTable(string a_commandText, 
        bool _ = false, string __ = null, NewImportSettings ___ = null) // Params used for extended functionality in ImporterV2
    {
        System.Data.Common.DbDataAdapter adapter = null;

        DataSet browseDataSet = new ();
        using (IDbConnection conn = GetConnection())
        {
            if (conn is SqlConnection)
            {
                adapter = new SqlDataAdapter(a_commandText, (SqlConnection)conn);
            }
            //else if (conn is System.Data.Odbc.OdbcConnection)
            //{
            //    adapter = new System.Data.Odbc.OdbcDataAdapter(a_commandText, (System.Data.Odbc.OdbcConnection)conn);
            //}
            //else if (conn is System.Data.OleDb.OleDbConnection)
            //{
            //    adapter = new System.Data.OleDb.OleDbDataAdapter(a_commandText, (System.Data.OleDb.OleDbConnection)conn);
            //}
            //else if (conn is System.Data.OracleClient.OracleConnection)
            //{
            //    adapter = new System.Data.OracleClient.OracleDataAdapter(a_commandText, (System.Data.OracleClient.OracleConnection)conn);
            //}

            string tableName = "Browse Table";
            try
            {
                adapter.SelectCommand.CommandTimeout = 300; // 5 minutes
                adapter.Fill(browseDataSet, tableName);
            }
            catch (Exception e)
            {
                throw new PTException(string.Format("Error filling {0}.[NL]{1}".Localize(), tableName, e.Message));
            }

            DataTable table = browseDataSet.Tables[0];
            SqlConnection.ClearAllPools();
            conn.Close();
        }

        //Convert DataTable to custom DataTableJson object since
        //DataTable is incompatible with .NET JSON serializer
        DataTableJson dataTableJson = new ();
        dataTableJson.ConstructTable(browseDataSet.Tables[0]);
        return dataTableJson;
    }

    public ImportSettings GetImportSettings()
    {
        return m_settings;
    }

    public void SaveImportSettings(ImportSettings a_settings)
    {
        m_settingsSaver.SaveImportSettings(a_settings); //save to file

        //TODO: We have to reload from file since we are using legacy serialization and loading functions
        //When this is replaced to load from a database, improve this process.
        m_settings = a_settings.Clone();
    }

    public NewImportSettings GetNewImportSettings(long a_scenarioId)
    {
        DebugException.ThrowInDebug("Attempting to get new import settings when running old import service. Please confirm your import configuration on the server.");
        return null;
    }

    public int SaveNewImportSettings(long a_scenarioId, NewImportSettings a_settings)
    {
        DebugException.ThrowInDebug("Attempting to get new import settings when running old import service. Please confirm your import configuration on the server.");
        return IntegrationConfigMappingSettings.c_NO_MAPPED_CONFIG_ID;
    }

    public int RunImport(string a_userName, long a_instigator, int a_connection, long a_targetScenarioId)
    {
        Lock(a_userName);
        ApplicationExceptionList ael = new ();
        try
        {
            m_instigator = new BaseId(a_instigator);
            Importer importer = new (m_settings, m_utility, m_importStatusMessage);
            SendPerformImportStartedTransmission();
            ael = importer.RunImport(false, m_instigator, new BaseId(a_targetScenarioId), m_sei);
        }
        catch (ApplicationException e)
        {
            //Add any exception to the list so it's displayed in the UI.
            ael = new ApplicationExceptionList();
            ael.Add(e);
        }
        finally
        {
            Unlock();
            SendPerformImportCompletedT(ael);
        }

        return ael.Count;
    }

    /// <summary>
    /// Runs Import asynchronously.
    /// </summary>
    /// <param name="a_testOnly"></param>
    /// <param name="a_userName"></param>
    /// <param name="a_instigator"></param>
    /// <param name="a_connectionNbr"></param>
    /// <param name="a_specificScenarioId"></param>
    /// <param name="a_testSettings">optional</param>
    /// <param name="a_transmissionType">optional</param>
    /// <returns></returns>
    public PerformImportResult RunImport(PerformImportRequest a_request)
    {
        if (Monitor.TryEnter(m_lockee, 0))
        {
            try
            {
                if (m_lockee.locked)
                {
                    return PerformImportResult.Busy;
                }
                else
                {
                    m_lockee.locked = true;

                    try
                    {
                        m_testSettings = a_request.TestSettings ?? m_settings;
                        m_instigator = new BaseId(a_request.Instigator);
                        m_typeToRun = a_request.TypeToRun != null ? Type.GetType(a_request.TypeToRun) : null;
                        m_testOnly = a_request.TestOnly;
                        m_targetScenarioId = new BaseId(a_request.SpecificScenarioId);

                        Thread thread = null;
                        ThreadStart threadStart = new (RunImportThread);
                        thread = new Thread(threadStart);
                        thread.Start();

                        return PerformImportResult.Started;
                    }
                    catch (Exception)
                    {
                        m_lockee.locked = false;
                        return PerformImportResult.Failed;
                    }
                }
            }
            finally
            {
                Monitor.Exit(m_lockee);
            }
        }

        return PerformImportResult.Busy;
    }

    /// <summary>
    /// Gets the status of the current/last run import.
    /// </summary>
    /// <param></param>
    /// <returns>Details on the last import. If an import hasn't run since the system last started, this will return null.</returns>
    public ImportStatusMessage GetCurrentImportStatus()
    {
        return m_importStatusMessage;
    }
}