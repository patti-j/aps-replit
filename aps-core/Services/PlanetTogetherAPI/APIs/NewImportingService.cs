using Microsoft.Data.SqlClient;
using PT.APIDefinitions.RequestsAndResponses.Webapp;
using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.File;
using PT.Common.Http.Json;
using PT.ERPTransmissions;
using PT.ImportDefintions;
using PT.ImportDefintions.RequestsAndResponses;
using PT.PackageDefinitions.Settings;
using PT.Scheduler;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.Transmissions;
using System.Data;
using System.Text;

using PT.APIDefinitions.RequestsAndResponses;
using PT.Common.Exceptions;
using PT.Common.Sql.SqlServer;
using PT.ServerManagerAPIProxy.APIClients;

using DBIntegrationDTO = PT.Common.Sql.SqlServer.DBIntegrationDTO;

namespace PT.PlanetTogetherAPI.Importing;

public class NewImportingService : IImportingService
{
    private readonly ImportUtilities m_utility;

    private BaseId m_instigator;
    private Type m_typeToRun;
    private BaseId m_targetScenarioId;
    private int m_targetConfigId;
    private bool m_locked;
    private string m_lockedUserName;
    private DateTime m_lockedTime;
    private bool m_testOnly;
    private ScenarioExceptionInfo m_sei;
    private readonly Lockee m_lockee = new();

    private static SoftwareVersion s_currentSoftwareVersion => SystemController.ServerSessionManager.ProductVersion;

    private ImportStatusMessage m_importStatusMessage { get; set; }

    public NewImportingService(ImportUtilities a_utility)
    {
        m_utility = a_utility;
        //InstanceSettingsEntity instance = m_utility.GetInstance();

        Task.Run(UpdateOutdatedConfigs);
    }

    private void UpdateOutdatedConfigs()
    {
        while (SystemController.Sys == null || SystemController.WebAppActionsClient == null)
        {
            Task.Delay(1000).Wait();
        }

        Dictionary<BaseId, IntegrationConfigMappingSettings> scenarioIdToConfigLookup = LoadScenarioIntegrationSettings();

        List<IntegrationConfigOptionsDTO> allConfigOptions = SystemController.WebAppActionsClient.GetIntegrationConfigOptions();
        IEnumerable<IntegrationConfigOptionsDTO> configOptionsToUpgrade =
            allConfigOptions.Where(incomingConfig => scenarioIdToConfigLookup.Values.Any(usedConfig => usedConfig.IntegrationConfigId == incomingConfig.IntegrationConfigId) && // in use
                                                     NewImportSettings.c_CONFIG_VERSION_NUMBER.ToString() != incomingConfig.VersionNumber); // outdated

        Parallel.ForEach(configOptionsToUpgrade, configToUpgrade => HandleUpgradingConfig(allConfigOptions, configToUpgrade, scenarioIdToConfigLookup));

        // Update scenario settings on one thread. If one scenario's config is out of date, all (mapped) ones will be
        foreach (KeyValuePair<BaseId, IntegrationConfigMappingSettings> scenarioSetting in scenarioIdToConfigLookup)
        {
            if (scenarioSetting.Value.IntegrationConfigId != IntegrationConfigMappingSettings.c_NO_MAPPED_CONFIG_ID)
            {
                UpdateScenarioIntegrationMapping(scenarioSetting.Key.Value, scenarioSetting.Value.IntegrationConfigId);
            }
        }
    }

    private void HandleUpgradingConfig(List<IntegrationConfigOptionsDTO> a_allConfigOptions, IntegrationConfigOptionsDTO a_configToUpgrade, Dictionary<BaseId, IntegrationConfigMappingSettings> a_scenarioIdToConfigLookup)
    {
        IntegrationConfigDTO oldConfig = null;
        try
        {
            int upgradedConfigId;

            // It's possible another instance has already done this upgrade, and saved it to the webapp
            IntegrationConfigOptionsDTO existingUpgrade = a_allConfigOptions.FirstOrDefault(config => config.UpgradedFromConfigId == a_configToUpgrade.IntegrationConfigId &&
                                                                                                      int.Parse(config.VersionNumber) == NewImportSettings.c_CONFIG_VERSION_NUMBER);
            if (existingUpgrade != null)
            {
                upgradedConfigId = existingUpgrade.IntegrationConfigId;
            }
            else
            {
                // Update old configuration, then save it to the webapp
                oldConfig = SystemController.WebAppActionsClient.GetIntegrationConfig(a_configToUpgrade.IntegrationConfigId);
                IntegrationConfigDTO localUpgradedConfig = UpgradeConfiguration(oldConfig);
                upgradedConfigId = SystemController.WebAppActionsClient.UpgradeIntegrationConfig(localUpgradedConfig);
            }

            // Set new id, to be saved after parallel work is done.
            // No thread in this parallel loop will be accessing the same lookup key.
            foreach (KeyValuePair<BaseId, IntegrationConfigMappingSettings> scenarioSetting in a_scenarioIdToConfigLookup)
            {
                if (scenarioSetting.Value.IntegrationConfigId == oldConfig.Id)
                {
                    scenarioSetting.Value.IntegrationConfigId = upgradedConfigId;
                }
            }
        }
        catch (Exception ex)
        {
            string configIdentifier = oldConfig?.Name ?? $"Id {a_configToUpgrade.IntegrationConfigId}";
            string errorDesc = $"An error occurred upgrading configuration {configIdentifier}";
            m_utility.LogError(errorDesc, ex);
            DebugException.ThrowInDebug(errorDesc);
        }
    }

    public IntegrationConfigDTO UpgradeConfiguration(IntegrationConfigDTO a_integrationConfigDto)
    {
        NewImportSettings settings = new NewImportSettings();
        settings.InitializeSettings(a_integrationConfigDto);
        IntegrationConfigDTO newSettings = settings.ToConfigDto();
        newSettings.UpgradedFromConfigId = a_integrationConfigDto.Id;
        newSettings.VersionNumber = NewImportSettings.c_CONFIG_VERSION_NUMBER.ToString();
        return newSettings;
    }

    private class Lockee
    {
        public bool locked;
    }

    private void RunImportThread(NewImportSettings a_settings)
    {
        ApplicationExceptionList ael = new();

        try
        {
            if (PTSystem.EnableDiagnostics)
            {
                m_utility.LogMessage("Import Logging\nNewImportingService: running import thread.");
            }

            SendPerformImportStartedTransmission();
            ImporterV2 importer = new(a_settings, m_utility, m_importStatusMessage);
            if (m_typeToRun != null)
            {
                ael = importer.RunImport(m_testOnly, m_typeToRun, m_instigator, m_targetScenarioId, m_sei, m_targetConfigId);
            }
            else
            {
                ael = importer.RunImport(m_testOnly, m_instigator, m_targetScenarioId, m_sei, m_targetConfigId);
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

    private Task<DBIntegrationDTO> m_RetrieveStagingDBTask;
    private Task<bool> m_UploadStagingDBTask;
    private int? m_RetrievelId;
    public int? TriggerStagingDBSchemaRetrieve()
    {
        m_RetrieveStagingDBTask = Task.Run(RunStagingDBSchemaRetrieve);
        int val = 0;
        do
        {
            val = Random.Shared.Next();
        } while (val == m_RetrievelId);
        m_RetrievelId = val;
        return val;
    }

    private DBIntegrationDTO m_stagingDbUploadData;
    public int? ApplyIntegration(DBIntegrationDTO a_integration)
    {
        if (m_UploadStagingDBTask != null && !m_UploadStagingDBTask.IsCompleted)
        {
            return null;
        }
        m_stagingDbUploadData = a_integration;
        m_UploadStagingDBTask = Task.Run(RunStagingDBSchemaUpload);
        int val = 0;
        do
        {
            val = Random.Shared.Next();
        } while (val == m_RetrievelId);
        m_RetrievelId = val;
        return val;
    }


    private async Task<bool> RunStagingDBSchemaUpload()
    {
        if (m_stagingDbUploadData != null)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(m_utility.GetErpSettings().ConnectionString);
                DbSchemaExtractor.UploadSchemaToDb(connection, m_stagingDbUploadData);
                return true;
            }
            catch (Exception e)
            {
                //SystemController.Sys.ErrorReporterInstance.LogInterfaceException(e, null, "", null); this throws //TODO: review logging 
                DebugException.ThrowInDebug("An exception occured when applying a db schema", e);
            }
        }

        m_stagingDbUploadData = null;
        return false;
    }

    public StagingDBSchemaResponse QueryStagingDBSchemaRetrieve(int a_retrievalId)
    {
        if (m_RetrievelId != null)
        {
            if (m_RetrieveStagingDBTask != null)
            {
                if (m_RetrieveStagingDBTask.IsCompletedSuccessfully)
                {
                    DBIntegrationDTO res = m_RetrieveStagingDBTask.Result;
                    m_RetrieveStagingDBTask = null;
                    return new()
                    {
                        Integration = res,
                        Success = true
                    };
                }

                if (m_RetrieveStagingDBTask.IsFaulted)
                {
                    m_RetrieveStagingDBTask = null;
                    return new StagingDBSchemaResponse()
                    {
                        Success = false,
                    };
                }
            }
            else if (m_UploadStagingDBTask != null)
            {
                if (m_UploadStagingDBTask.IsCompletedSuccessfully)
                {
                    if (m_UploadStagingDBTask.Result)
                    {
                        m_UploadStagingDBTask = null;
                        return new StagingDBSchemaResponse()
                        {
                            Success = true, //this is applying an integration, so no integration is returned
                        };
                    }
                    else
                    {
                        m_UploadStagingDBTask = null;
                        return new StagingDBSchemaResponse()
                        {
                            Success = false,
                        };
                    }

                }

                if (m_UploadStagingDBTask.IsFaulted)
                {
                    m_UploadStagingDBTask = null;
                    return new StagingDBSchemaResponse()
                    {
                        Success = false,
                    };
                }
            }

            return null;
        }

        return new StagingDBSchemaResponse()
        {
            Success = false,
        };;
    }

    private async Task<DBIntegrationDTO> RunStagingDBSchemaRetrieve()
    {
        try
        {
            SqlConnection connection = new SqlConnection(m_utility.GetErpSettings().ConnectionString);
            return DbSchemaExtractor.RetrieveDBSchema(connection);
        }
        catch (Exception ex)
        {
            // TODO: log
            DebugException.ThrowInDebug($"An error occured during the db retrieval process: {ex}");
            return null;
        }
    }

    public async Task<bool> CreateIntegrationFromStagingDB(CreateIntegrationRequest a_request, string a_email)
    {
        DBIntegrationDTO integration = await RunStagingDBSchemaRetrieve();
        integration.IntegrationTableObjects.RemoveAll(o => !a_request.ObjectsToInclude.Any(r => r == o.ObjectName));
        integration.IntegrationViewObjects.RemoveAll(o => !a_request.ObjectsToInclude.Any(r => r == o.ObjectName));
        integration.IntegrationFunctionObjects.RemoveAll(o => !a_request.ObjectsToInclude.Any(r => r == o.ObjectName));
        integration.IntegrationStoredProcObjects.RemoveAll(o => !a_request.ObjectsToInclude.Any(r => r == o.ObjectName));

        integration.Name = a_request.IntegrationName;
        integration.Version = a_request.Version;
        integration.VersionNotes = a_request.VersionNotes;
        integration.CompanyId = a_request.CompanyId;

        try
        {
            SystemController.WebAppActionsClient.CreateIntegration(integration, a_email);
            return true;
        }
        catch (Exception e)
        {
            DebugException.ThrowInDebug(e.Message, e);
            return false;
        }
    }
    private void SendPerformImportStartedTransmission()
    {
        PerformImportStartedT t = new();
        t.ImportingInstigator = m_instigator;
        SystemController.ClientSession.SendClientAction(t);
    }

    private void SendPerformImportCompletedT(ApplicationExceptionList a_exceptions)
    {
        PerformImportCompletedT t = new();
        t.ImportingInstigator = m_instigator;
        t.Exceptions = a_exceptions;
        SystemController.ClientSession.SendClientAction(t);
    }

    private void SendStagingStartedTransmission()
    {
        RefreshStagingDataStartedT t = new();
        t.ImportingInstigator = m_instigator;
        SystemController.ClientSession.SendClientAction(t);
    }

    private void SendStagingCompletedT(ApplicationExceptionList a_exceptions)
    {
        RefreshStagingDataCompletedT t = new();
        t.ImportingInstigator = m_instigator;
        t.Exceptions = a_exceptions;
        SystemController.ClientSession.SendClientAction(t);
    }

    private IDbConnection GetConnection()
    {
        ErpDatabase erpSettings = m_utility.GetErpSettings();

        return new SqlConnection(erpSettings.ConnectionString);
    }

    public DataTableJson GetBrowseTable(string a_commandText, bool a_includeValidation = false, string a_tableName = null, NewImportSettings a_importSettings = null)
    {
        System.Data.Common.DbDataAdapter adapter = null;

        DataSet browseDataSet = new();
        using (IDbConnection conn = GetConnection())
        {
            if (conn is SqlConnection)
            {
                adapter = new SqlDataAdapter(a_commandText, (SqlConnection)conn);
            }
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

            SqlConnection.ClearAllPools();
            conn.Close();
        }


        DataTable table = browseDataSet.Tables[0];

        if (a_includeValidation && !string.IsNullOrEmpty(a_tableName))
        {
            Validate(a_tableName, table, a_importSettings);
        }

        DataTableJson dataTableJson = new();
        dataTableJson.ConstructTable(browseDataSet.Tables[0]);
        return dataTableJson;
    }

    public void RefreshStagingData(BaseId a_instigator, BaseId a_targetScenarioId)
    {
        ApplicationExceptionList ael = new ApplicationExceptionList();
        lock (m_lockee)
        {
            try
            {
                m_lockee.locked = true;
                m_instigator = a_instigator;
                SendStagingStartedTransmission();

                // ImportSettings are not needed for performing preimport, as it only uses instance-level settings. So, we can just pass null here (to avoid leading from db).
                // In future, we could potentially store pre-import-specific data in there and need to load it.
                ImporterV2 importer = new (m_utility, m_importStatusMessage);

                importer.PerformPreimport(a_instigator, a_targetScenarioId);
            }
            catch (Exception e)
            {
                //Add any exception to the list so it's displayed in the UI.
                ael.Add(e);
            }
            finally
            {
                m_lockee.locked = false;
            }
        }



        try
        {
            SendStagingCompletedT(ael);
        }
        catch (Exception e)
        {
            m_utility.LogImporterException(e);
        }
    }

    // TODO: It's unfortunate we tie validation to a single table at a time, since there's nothing inherent to the properties that requires this.
    // TODO: However, SqlDataAdapter doesn't give us a way to get the table name from returned columns. We could in future extract this from the sql command, but this would be messy and isn't currently needed.
    private void Validate(string a_tableName, DataTable table, NewImportSettings a_importSettings)
    {
        IEnumerable<IntegrationFeatureBase> featuresUsingProperties = new List<IntegrationFeatureBase>();

        if (table.Rows.Count > 0)
        {
            IEnumerable<IntegrationProperty> tableProperties = table.Columns.Cast<DataColumn>() // DataColumnCollection is ancient and doesn't implement generic IEnumerable
                                                                    .SelectMany(column => a_importSettings.FeaturesSettings.GetPropertiesByNameAndTableName(column.ColumnName, a_tableName))
                                                                    .Distinct();

            featuresUsingProperties = a_importSettings.FeaturesSettings.AllFeatures.Values
                                                      .Where(feature => feature.Properties.Intersect(tableProperties).Any());
        }

        foreach (DataRow row in table.Rows)
        {
            try
            {
                foreach (IntegrationFeatureBase feat in featuresUsingProperties)
                {
                    Result<object, Dictionary<string, string>> result = feat.ValidateRow(row);

                    if (result.IsErr)
                    {
                        Dictionary<string, string> error = result.UnwrapErr();
                        foreach (KeyValuePair<string,string> err in error)
                        {
                            string errorText = row.GetColumnError(err.Key) == "" ? err.Value : row.GetColumnError(err.Key) + "\n" + err.Value;
                            row.SetColumnError(err.Key, errorText);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                row.SetColumnError(table.Columns[0].ColumnName, e.Message);
            }
        }
    }

    public ImportSettings GetImportSettings()
    {
        InstanceSettingsEntity instance = m_utility.GetInstance();
        ImportSettingsSaver settingsSaver = new ImportSettingsSaver(instance.ServicePaths.InterfaceFileNameWithFullPath, m_utility);
        return settingsSaver.LoadSettings();
    }

    public void SaveImportSettings(ImportSettings a_settings)
    {
        throw new ArgumentException("Attempting to save old import settings when running new service. Please confirm your import configuration on the server.");
    }

    public NewImportSettings GetNewImportSettings(long a_scenarioId)
    {
        NewImportSettings settings = null;
        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
        {
            Scenario s = sm.Find(new BaseId(a_scenarioId));
            if (s == null)
            {
                return null;
            }

            // TODO: cache config so api call isn't needed each time
            IntegrationConfigMappingSettings integrationConfigMappingSettings = new IntegrationConfigMappingSettings();
            using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
            {
                integrationConfigMappingSettings = ss.ScenarioSettings.LoadSetting(integrationConfigMappingSettings);
            }

            if (integrationConfigMappingSettings != null && integrationConfigMappingSettings.IntegrationConfigId != IntegrationConfigMappingSettings.c_NO_MAPPED_CONFIG_ID)
            {
                IntegrationConfigDTO integrationConfigDto = SystemController.WebAppActionsClient.GetIntegrationConfig(integrationConfigMappingSettings.IntegrationConfigId);
                settings = new NewImportSettings();
                settings.InitializeSettings(integrationConfigDto);
            }
        }

        return settings;
    }

    public int SaveNewImportSettings(long a_scenarioId, NewImportSettings a_settings)
    {
        IntegrationConfigDTO configDto = a_settings.ToConfigDto();
        if (a_settings.WebAppId < 1)
        {
            int newConfigId = SystemController.WebAppActionsClient.CreateIntegrationConfig(configDto);

            // This is the first time saving this config, so assign it to the scenario
            UpdateScenarioIntegrationMapping(a_scenarioId, newConfigId);

            return newConfigId;
        }
        else
        {
            SystemController.WebAppActionsClient.UpdateIntegrationConfig(configDto);
            return configDto.Id;
        }
    }

    private static void UpdateScenarioIntegrationMapping(long a_scenarioId, int a_configId)
    {
        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm)) //Wait until a lock can be made.  Must open here.
        {
            Scenario s = sm.Find(new BaseId(a_scenarioId));

            if (s != null)
            {
                using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary scenarioSummary))
                {
                    IntegrationConfigMappingSettings integrationMappingSettings = new IntegrationConfigMappingSettings();
                    integrationMappingSettings = scenarioSummary.ScenarioSettings.LoadSetting(integrationMappingSettings);
                    integrationMappingSettings.IntegrationConfigId = a_configId;
                    scenarioSummary.ScenarioSettings.SaveSetting(integrationMappingSettings);
                }
            }
        }
    }

    private Dictionary<BaseId, IntegrationConfigMappingSettings> LoadScenarioIntegrationSettings()
    {
        Dictionary<BaseId, IntegrationConfigMappingSettings> scenarioIdToConfigLookup = new();
        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
        {

            IntegrationConfigMappingSettings configSetting = new();
            foreach (Scenario s in sm.Scenarios)
            {
                using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary summary))
                {
                    configSetting = summary.ScenarioSettings.LoadSetting(configSetting);
                    scenarioIdToConfigLookup.Add(s.Id, configSetting);
                }
            }
        }

        return scenarioIdToConfigLookup;
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
                        //Todo this is where we need to consume the config Id to get the proper settings'
                        //
                        //Todo we should also be able to accept test settings from the perform import request
                        m_instigator = new BaseId(a_request.Instigator);
                        m_typeToRun = a_request.TypeToRun != null ? Type.GetType(a_request.TypeToRun) : null;
                        m_testOnly = a_request.TestOnly;
                        m_targetScenarioId = new BaseId(a_request.SpecificScenarioId);
                        m_targetConfigId = a_request.SpecificConfigId;

                        NewImportSettings importSettings = a_request.NewTestSettings ?? GetImportConfig(a_request);

                        Thread thread = null;
                        ThreadStart threadStart = new(() => RunImportThread(importSettings));
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

    private static NewImportSettings GetImportConfig(PerformImportRequest a_request)
    {
        NewImportSettings settings;

        // Use configId if provided, otherwise load from provided scenario
        int currentImportConfigId = a_request.SpecificConfigId;
        if (currentImportConfigId == IntegrationConfigMappingSettings.c_NO_MAPPED_CONFIG_ID)
        {
            using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
            {
                Scenario s = sm.Find(new BaseId(a_request.SpecificScenarioId));
                if (s == null)
                {
                    throw new PTHandleableException("2714".Localize(), new object[] { a_request.SpecificScenarioId });
                }

                IntegrationConfigMappingSettings integrationConfigPerScenarioMappingSettings = new IntegrationConfigMappingSettings();

                using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                {
                    integrationConfigPerScenarioMappingSettings = ss.ScenarioSettings.LoadSetting(integrationConfigPerScenarioMappingSettings);
                }
                currentImportConfigId = integrationConfigPerScenarioMappingSettings.IntegrationConfigId;
            }
        }

        IntegrationConfigDTO integrationConfigDto = SystemController.WebAppActionsClient.GetIntegrationConfig(currentImportConfigId);
        settings = new NewImportSettings();
        settings.InitializeSettings(integrationConfigDto);

        return settings;
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

    public bool? ApplyIntegrationAndData(WebAppActionsClient.GetIntegrationDataResponse a_integrationAndData)
    {
        int? retrievalId = ApplyIntegration(a_integrationAndData.Integration);
        if (!retrievalId.HasValue)
        {
            return false;
        }

        m_UploadStagingDBTask.Wait();

        using SqlConnection connection = new SqlConnection(m_utility.GetErpSettings().ConnectionString);
        connection.Open();
        foreach (KeyValuePair<string,DataTableJson> kv in a_integrationAndData.TableData)
        {
            string tableName = kv.Key;
            bool result = InsertDataForTable(connection, tableName, kv.Value.GetDataTable(out TableDataConversionErrorMessage errorMessage));
            if (!result)
            {
                connection.Close();
                return false;
            }
        }
        connection.Close();
        return true;
    }

    private static bool InsertDataForTable(SqlConnection a_conn, string a_tableName, DataTable a_table)
    {
        if (a_table.Rows.Count == 0)
        {
            return true;
        }
        using (SqlTransaction transaction = a_conn.BeginTransaction())
        {
            try
            {
                using (var bulk = new SqlBulkCopy(a_conn, SqlBulkCopyOptions.Default, transaction))
                {
                    bulk.DestinationTableName = a_tableName;
                    bulk.BatchSize = 1000; //controls number of rows per batch
                    bulk.BulkCopyTimeout = 600; //in seconds

                    // Map columns explicitly since DataTable column order may differ
                    foreach (DataColumn column in a_table.Columns)
                    {
                        bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }

                    // Write all rows at once
                    bulk.WriteToServer(a_table);
                }

                transaction.Commit();
                return true;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return false;
            }
        }
    }
}