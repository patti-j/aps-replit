using Microsoft.Data.SqlClient;
using PT.APSCommon;
using PT.Common.File;
using PT.Common.Http;
using PT.ERPTransmissions;
using PT.ImportDefintions;
using PT.PackageDefinitions.Settings;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.Transmissions;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Net.Http.Headers;

using PT.APIDefinitions.Integration;
using PT.Common.Exceptions;

using ImportRequest = PT.APIDefinitions.Integration;

namespace PT.PlanetTogetherAPI.Importing;

/// <summary>
/// Implementation of the Importer with two defining features:
/// (1) it takes a NewImportSettings object, which is structured to allow feature-based structuring. (It would be nice to use inheritance here, but the changes to ImportTableSetting vs MapStepSetting, and difficulties with serialized communication over API, made this difficult)
/// (2) It abstracts the steps taken as part of the import process into its own class, ImportStep. This could enable running them, in parallel in future.
/// </summary>
public class ImporterV2 : IImporter
{
    // \/ Import Steps/Phases \/ 
    //Customers
    //Users
    //Plants
    //Departments
    //Capabilities
    //Cells
    //Resources
    //Items Only
    //Warehouses
    //CapacityIntervals
    //RecurringCapacityIntervals
    //ProductRules
    //Attributes
    //Attributes Setup Table
    //Attributes Table
    //Cleanout Trigger Tables
    //Purchase Orders
    //Sales Orders
    //Forecasts
    //Transfer Orders
    //Jobs
    //ActivityUpdateT
    //
    private ImportUtilities m_utility;
    private NewImportSettings m_settings;
    private List<ImportStep> m_steps = new();

    private ImportStatusMessage m_importStatusMessage { get; set; }
    private readonly ImportStatusUpdater m_statusUpdater;
    private long m_scenarioId;
    private int m_configId;

    #region Instance Import Settings
    private string m_preImportUrl;
    private bool m_runPreImportSql;
    private string m_preImportSql;
    private string m_preImportProgramPath;
    private string m_preImportProgramArgs;
    private string m_postImportUrl;
    #endregion

    internal JobDataSet jobDataSet = null;
    internal HashSet<string> jobTActivityUniqueIds = new();

    public ImporterV2(NewImportSettings a_settings, ImportUtilities a_utility, ImportStatusMessage a_importStatusMessage) : this(a_utility, a_importStatusMessage)
    {
        m_settings = a_settings.Clone();

        m_steps.Add(new ImportCustomersStep(m_settings, true));
        m_steps.Add(new ImportPlantsStep(m_settings, true));
        m_steps.Add(new ImportDepartmentsStep(m_settings, true));
        m_steps.Add(new ImportCapabilitiesStep(m_settings, true));
        m_steps.Add(new ImportCellsStep(m_settings, true));
        m_steps.Add(new ImportResourcesStep(m_settings, true));
        m_steps.Add(new ImportResourceConnectorsStep(m_settings, true));
        //m_steps.Add(new ImportItemsOnlyStep(m_settings, false)); // TODO: this one is handled differently - only on "onetypeonly imports, which we need to support
        m_steps.Add(new ImportWarehousesStep(m_settings, true));
        m_steps.Add(new ImportCapacityIntervalsStep(m_settings, true));
        m_steps.Add(new ImportRecurringCapacityIntervalsStep(m_settings, true));
        m_steps.Add(new ImportProductRulesStep(m_settings, true));
        m_steps.Add(new ImportAttributesStep(m_settings, true));
        m_steps.Add(new ImportAttributesSetupTableStep(m_settings, true));
        m_steps.Add(new ImportAttributeCodeTableStep(m_settings, true));
        m_steps.Add(new ImportCleanoutTriggerTableStep(m_settings, true));
        m_steps.Add(new ImportCompatibilityCodeTableStep(m_settings, true));
        m_steps.Add(new ImportCompatibilityCodeTableStep.ImportPurchaseToStockStep(m_settings, true));
        m_steps.Add(new ImportCompatibilityCodeTableStep.ImportSalesOrdersStep(m_settings, true));
        m_steps.Add(new ImportCompatibilityCodeTableStep.ImportForecastsStep(m_settings, true));
        m_steps.Add(new ImportCompatibilityCodeTableStep.ImportTransferOrderStep(m_settings, true));
        m_steps.Add(new ImportJobsStep(m_settings, true));
        m_steps.Add(new ImportInternalActivityStep(m_settings, true));
        m_steps.Add(new ImportUDFStep(m_settings, true));
    }

    /// <summary>
    /// Slim constructor that allows pre-import to be performed. If running a full import, use <see cref="ImporterV2(NewImportSettings,ImportUtilities,ImportStatusMessage)"/>
    /// </summary>
    /// <param name="a_utility"></param>
    /// <param name="a_importStatusMessage"></param>
    public ImporterV2(ImportUtilities a_utility, ImportStatusMessage a_importStatusMessage)
    {
        m_utility = a_utility;
        m_importStatusMessage = a_importStatusMessage;
        m_statusUpdater = new ImportStatusUpdater();
    }

    public NewImportSettings ImportSettings
    {
        get => m_settings;
        set => m_settings = value;
    }

    public List<ImportStep> Steps => m_steps;

    public ApplicationExceptionList PerformImport(bool a_testOnly, bool a_oneTypeOnly, Type a_typeToInclude, BaseId a_instigator, BaseId a_targetScenarioId, ScenarioExceptionInfo a_sei, int a_targetConfigId)
    {
        if (PTSystem.EnableDiagnostics)
        {
            m_utility.LogMessage("Import Logging\nNewImportingService: PerformImport started.");
        }

        m_scenarioId = a_targetScenarioId.Value;
        m_configId = a_targetConfigId;
        ImportT importT = new ()
        {
            SpecificScenarioId = new BaseId(m_scenarioId),
            SpecificConfigId = m_configId
        };

        // TODO: Error handling is a bit messy to support the potential for running steps in parallel. It would be nice to have one ApplicationExceptionList throughout
        ConcurrentBag<ExceptionDescriptionInfo> errors = new ConcurrentBag<ExceptionDescriptionInfo>();

        try
        {
            LoadInstanceImportSettings();

            if (PTSystem.EnableDiagnostics)
            {
                m_utility.LogMessage("Import Logging\nNewImportingService: PerfomImport loaded instance import settings.");
            }
            StartProgress(a_instigator);

            PerformPreimport(a_instigator, a_targetScenarioId);

            if (PTSystem.EnableDiagnostics)
            {
                m_utility.LogMessage("Import Logging\nNewImportingService: PerformPreimport() called.");
            }

            HandleImportSteps(a_testOnly, errors, importT);
        }
        catch (ImportException interfaceErr)
        {
            errors.Add(new ExceptionDescriptionInfo(interfaceErr));
        }
        catch (Exception err)
        {
            errors.Add(new ExceptionDescriptionInfo(new ImportException("2852", err)));
        }
        finally
        {
            SendTransmission(importT);
            if (PTSystem.EnableDiagnostics)
            {
                m_utility.LogMessage("Import Logging\nNewImportingService: Sent ImportT.");
            }
        }

        try
        {
            PerformPostImport(a_instigator, a_targetScenarioId, errors);
        }
        catch (ImportException interfaceErr)
        {
            errors.Add(new ExceptionDescriptionInfo(interfaceErr));
        }
        catch (Exception err)
        {
            errors.Add(new ExceptionDescriptionInfo(new ImportException("2852", err)));
        }

        ApplicationExceptionList newErrors = LogImportErrors(a_sei, errors);
        SetProgressCompleted(newErrors);

        return newErrors;
    }

    private ApplicationExceptionList LogImportErrors(ScenarioExceptionInfo a_sei, ConcurrentBag<ExceptionDescriptionInfo> errors)
    {
        ApplicationExceptionList errList = new();
        foreach (ExceptionDescriptionInfo exception in errors)
        {
            errList.Add(exception);
        }
        if (errors.Count > 0)
        {
            ApplicationExceptionList.Node node = errList.First;
            int loggedCnt = 0;
            while (node != null && loggedCnt < 100) //NO sense filling the log with loads of exceptions.
            {
                m_utility.LogImporterException(node.Data, a_sei);
                node = node.Next;
                loggedCnt++;
            }
        }

        //TEMPORARARY UNTIL I CAN FIGURE OUT WHY THESE DAMN EXCEPTIONS WON'T SERIALIZE.
        ApplicationExceptionList newErrors = new();
        ApplicationExceptionList.Node nextNode = errList.First;
        while (nextNode != null)
        {
            newErrors.Add(nextNode.Data);
            nextNode = nextNode.Next;
        }

        return newErrors;
    }

    private void PerformPostImport(BaseId a_instigator, BaseId a_targetScenarioId, ConcurrentBag<ExceptionDescriptionInfo> errors)
    {
        try
        {
            Task postImportWebhook = Task.Run(() => RunPostImportWebhook(a_targetScenarioId.ToBaseType(), a_instigator));
            postImportWebhook.Wait();
        }
        catch (ImportException interfaceErr)
        {
            // TODO: This would be better capturing the entire error as in Importer
            errors.Add(new ExceptionDescriptionInfo(interfaceErr));
        }
        catch (Exception err)
        {
            errors.Add(new ExceptionDescriptionInfo(new ImportException("2852", err)));
        }
    }

    private void HandleImportSteps(bool a_testOnly, ConcurrentBag<ExceptionDescriptionInfo> errors, ImportT importT)
    {
        // Activity Step has to come after Jobs, so we'll do it last
        ImportStep activityStep = m_steps.FirstOrDefault(step => step is ImportInternalActivityStep);

        // TODO: Replace with Parallel if it works
        foreach (ImportStep a_step in m_steps.Except(new[] { activityStep }))
        {
            HandleStep(a_testOnly, a_step, errors, importT);
        }

        if (activityStep != null)
        {
            HandleStep(a_testOnly, activityStep, errors, importT);
        }
    }

    public void PerformPreimport(BaseId a_instigator, BaseId a_targetScenarioId)
    {
        Task webhookTask = Task.Run(() => RunPreImportWebhook(a_targetScenarioId.ToBaseType(), a_instigator));
        webhookTask.Wait();

        RunPreImportPrograms();

        RunPreImportSQL();
    }

    private void HandleStep(bool a_testOnly, ImportStep a_step, ConcurrentBag<ExceptionDescriptionInfo> errors, ImportT importT)
    {
        if (a_step.ShouldStepBeSkipped())
        {
            return;
        }

        SetProgress(a_step.ProgressStep, a_step.ProgressPoints);

        var transmission = a_step.GetTransmission(this);
        if (!transmission.IsOk)
        {
            ImportStep.ImportStepTError error = transmission.UnwrapErr();
            foreach (ExceptionDescriptionInfo m in error.Exceptions)
            {
                errors.Add(m);
            }

            return;
        }

        if (a_testOnly)
        {
            return;
        }

        lock (importT)
        {
            importT.Add(transmission.Unwrap());
        }
    }

    private void SendTransmission(PTTransmission t)
    {
        //The instigator must by ERP so that JobT data doesn't overwrite user/barcode status updates
        t.Instigator = BaseId.ERP_ID;

        //Create PT Connection
        try
        {
            t.TransmissionSender = PTTransmissionBase.TransmissionSenderType.PTInterface;
            SystemController.ClientSession.SendClientAction(t);
        }
        catch (Exception err)
        {
            ImporterException e = new("2354", new object[] { t.GetType().ToString(), err.Message });
            m_utility.LogImporterException(e);
            throw e;
        }
    }

    public IDbConnection OpenNewConnection()
    {
        IDbConnection connection = GetConnection();

        try
        {
            connection.Open();
            return connection;
        }
        catch (Exception err)
        {
            ImporterConnectionException e = new(connection.ConnectionString, err.Message);
            m_utility.LogImporterException(e);
            throw e;
        }
    }

    /// <summary>
    /// Runs a program prior to import (We can possibly remove this, URL is not set anywhere)
    /// </summary>
    private void RunPreImportPrograms()
    {
        if (string.IsNullOrEmpty(m_preImportProgramPath))
        {
            return;
        }

        try
        {
            SetProgress(ImportStatuses.EImportProgressStep.PreImportProgram, 300);

            Process p = new();
            p.StartInfo.FileName = m_preImportProgramPath;
            p.StartInfo.Arguments = m_preImportProgramArgs;
            p.StartInfo.WorkingDirectory = Common.Directory.DirectoryUtils.GetPathFromFullPath(m_preImportProgramPath);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            string stdOut = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                throw new PTException("2959", new object[] { p.StartInfo.FileName, p.ExitCode, stdOut });
            }
        }
        catch (Exception e)
        {
            throw new PTException("2960", e, new object[] { m_preImportProgramPath, m_preImportProgramArgs });
        }
    }

    /// <summary>
    /// Execute a SQL Statement on the same connection specified for import data.
    /// </summary>
    /// <param name="connection"></param>
    private void RunPreImportSQL()
    {
        if (m_runPreImportSql)
        {
            SetProgress(ImportStatuses.EImportProgressStep.PreImportSql, 300);
            using (IDbConnection connection = OpenNewConnection())
            {
                try
                {
                    IDbCommand cmd = null;
                    if (connection is SqlConnection)
                    {
                        cmd = new SqlCommand(m_preImportSql, (SqlConnection)connection);
                    }
                    //else if (connection is OracleConnection)
                    //    cmd = new OracleCommand(instance.PreImportSQL, (OracleConnection)connection);
                    //else if (connection is OdbcConnection)
                    //    cmd = new OdbcCommand(instance.PreImportSQL, (OdbcConnection)connection);
                    //else if (connection is OleDbConnection)
                    //    cmd = new OleDbCommand(instance.PreImportSQL, (OleDbConnection)connection);

                    if (cmd != null)
                    {
                        cmd.CommandType = CommandType.Text; //allows them to run whatever sql they want
                        cmd.CommandTimeout = 0; //wait till done
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception err)
                {
                    throw new ImportException("2850", err, new object[] { m_preImportSql });
                }
            }
        }
    }
    private async Task RunPreImportWebhook(long a_targetScenarioId, BaseId a_instigator)
    {
        try
        {
            if (!string.IsNullOrEmpty(m_preImportUrl))
            {
                Uri preImportUrl = new Uri(m_preImportUrl);
                string apiController = "";
                string apiEndpoint = "";
                
                string baseUrl = preImportUrl.GetLeftPart(UriPartial.Authority);
                
                bool isPt = preImportUrl.Host.Contains("pt-integrations", StringComparison.OrdinalIgnoreCase) || (preImportUrl.IsLoopback && preImportUrl.Port == 55971);

                if (isPt)
                {
                    // Only for pt-integrations: force /api/Rest/Import (preserve any instance prefix in the path)
                    string path = string.Join("", preImportUrl.Segments).Trim('/');
                    apiController = string.IsNullOrEmpty(path) ? "api/Rest" : $"{path}/api/Rest";
                    apiEndpoint = "Import";
                }
                else
                {                    
                    if (preImportUrl.Segments.Length > 1)
                    {                        
                        apiController = string.Join("", preImportUrl.Segments[0..^1]).Trim(' ', '/');
                        apiEndpoint = preImportUrl.Segments.Last().Trim('/');
                    }
                    else
                    {                     
                        apiController = "api/Rest";
                        apiEndpoint = "/Import";
                    }
                }

                SetProgress(ImportStatuses.EImportProgressStep.PreImportWebhook, 300);
                TimeSpan timeout = TimeSpan.FromSeconds(m_utility.GetBroadcasterConstructorValues().WebApiTimeoutSeconds);
                AuthenticationHeaderValue auth = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{a_instigator}:null"))
                );

                PTHttpClient ptClient = new(apiController, baseUrl, timeout, auth)
                {
                    ForceDeserializeResponseObject = true
                };

                ImportRequest.ImportRequest importRequest = new ImportRequest.ImportRequest
                {
                    ScenarioId = a_targetScenarioId,
                    StatusStep = EStatusStep.Started
                };
                
                await ptClient.MakePostRequest(apiEndpoint, importRequest);
            }
        }
        catch (Exception err)
        {
            throw new ImportException("2985", err, new object[] { m_preImportUrl });
        }
    }

    private async Task RunPostImportWebhook(long a_targetScenarioId, BaseId a_instigator)
    {
        try
        {
            if (!string.IsNullOrEmpty(m_postImportUrl))
            {
                Uri preExportUri = new Uri(m_postImportUrl);
                string apiController = "";
                string apiEndpoint = "";

                //If there was a specified endpoint, Segments will be > 1.
                if (preExportUri.Segments.Length > 1)
                {
                    apiController = string.Join("", preExportUri.Segments[0..^1]);
                    apiController = apiController.Trim(' ', '/');

                    apiEndpoint = preExportUri.Segments.Last();
                    if (!apiEndpoint.StartsWith('/'))
                    {
                        apiEndpoint = '/' + apiEndpoint;
                    }
                }
                else
                {
                    apiController = "api/Rest";
                    apiEndpoint = "/Import";
                }

                SetProgress(ImportStatuses.EImportProgressStep.PostImportWebhook, 300);
                TimeSpan timeout = TimeSpan.FromSeconds(m_utility.GetBroadcasterConstructorValues().WebApiTimeoutSeconds);
                PTHttpClient ptClient = new(apiController, m_postImportUrl, timeout, new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{a_instigator.ToString()}:null"))));
                ImportRequest.ImportRequest importRequest = new ();
                importRequest.ScenarioId = a_targetScenarioId;
                importRequest.StatusStep = EStatusStep.Completed;
                await ptClient.MakePostRequest(apiEndpoint, importRequest);
            }
        }
        catch (Exception err)
        {
            throw new ImportException("2986", err, new object[] { m_postImportUrl });
        }
    }

    public IDbConnection GetConnection()
    {
        ErpDatabase erpSettings = m_utility.GetErpSettings();
        try
        {
            return new SqlConnection(erpSettings.ConnectionString);
        }
        catch (Exception exc)
        {
            ImporterException e = new("2352", new object[] { erpSettings.ConnectionString, exc.Message });
            m_utility.LogImporterException(e);
            throw e;
        }
    }

    public IDbCommand GetCommand(IDbConnection connection, string sqlText)
    {
        IDbCommand cmd;
        cmd = new SqlCommand(sqlText, (SqlConnection)connection);
        cmd.CommandTimeout = 0;
        return cmd;
    }

    public ApplicationExceptionList RunImport(bool testonly, Type objectType, BaseId instigator, BaseId a_targetScenarioId, ScenarioExceptionInfo a_sei, int a_targetConfigId)
    {
        try
        {
            return PerformImport(testonly, true, objectType, instigator, a_targetScenarioId, a_sei, a_targetConfigId);
        }
        catch (Exception e)
        {
            m_utility.LogImporterException(e);
            throw;
        }
    }

    public ApplicationExceptionList RunImport(bool testonly, BaseId instigator, BaseId a_targetScenarioId, ScenarioExceptionInfo a_sei, int a_targetConfigId)
    {
        ApplicationExceptionList exceptions = new();
        try
        {
            bool runStandard = true;
            // TODO: Is this used? If so, need to reimplement
            //if (m_settings.UseCustomInterface)
            //{
            //    runStandard = PerformCustomImport(testonly);
            //}

            if (runStandard)
            {
                return PerformImport(testonly, false, null, instigator, a_targetScenarioId, a_sei, a_targetConfigId);
            }
        }
        catch (Exception e)
        {
            exceptions.Add(e);
        }

        return exceptions;
    }

    /// <summary>
    /// populates Activity Table of JobDataSet from db and returns it.
    /// </summary>
    /// <returns></returns>
    private JobDataSet.ActivityDataTable FetchActivityDataTable()
    {
        JobDataSet ds = new();

        using (IDbConnection connection = OpenNewConnection())
        {
            string actUpdateCmdStr = m_settings.InternalActivitySettings.GetCommandText(true);
            IDbCommand actUpdateCmd = GetCommand(connection, actUpdateCmdStr);
            ERPTransmission.FillDataTable(ds.Activity, actUpdateCmd, typeof(ActivityUpdateT).Name);
        }

        return ds.Activity;
    }

    /// <summary>
    /// Creates an ActivityUpdateT transmission.
    /// </summary>
    /// <param name="a_jobDataSet">If JobT was processed this is used to avoid querying db again (can be null)</param>
    /// <param name="a_jobTUniqueKeys">This contains unique keys for activities that were already added to JobT</param>
    /// <returns></returns>
    internal ActivityUpdateT GetActivityUpdateT(JobDataSet a_jobDataSet, HashSet<string> a_jobTUniqueKeys)
    {
        ActivityUpdateT actUpdateT = null;

        if (a_jobDataSet == null) // query db
        {
            actUpdateT = new ActivityUpdateT(FetchActivityDataTable());
        }
        else if (a_jobTUniqueKeys != null && a_jobTUniqueKeys.Count < a_jobDataSet.Activity.Rows.Count)
        {
            actUpdateT = new ActivityUpdateT();
            foreach (JobDataSet.ActivityRow row in a_jobDataSet.Activity.Rows)
            {
                if (!a_jobTUniqueKeys.Contains(ERPTransmissions.Activity.GetUniqueKey(row))) // don't want any that were included with JobT
                {
                    actUpdateT.Add(row);
                }
            }
        }

        return actUpdateT;
    }


    /// <summary>
    /// Populate JobDataSet from SQL and return.
    /// </summary>
    /// <returns></returns>
    internal JobDataSet FetchJobDataSet(ImporterV2 a_importer)
    {
        //Jobs
        IDbConnection jobConnection = null;
        IDbConnection moConnection = null;
        IDbConnection machineOperationConnection = null;
        IDbConnection internalActivityConnection = null;
        IDbConnection resourceRequirementConnection = null;
        IDbConnection requiredCapabilityConnection = null;
        IDbConnection successorMoConnection = null;
        IDbConnection materialConnection = null;
        IDbConnection pathConnection = null;
        IDbConnection pathNodeConnection = null;
        IDbConnection customerConnection = null;
        string database = "";

        try
        {
            using (IDbConnection connection = a_importer.OpenNewConnection())
            {
                database = connection.Database; //for error message
                string sqlText = m_settings.JobSettings.GetCommandText(true);
                IDbCommand jobCmd = GetCommand(connection, sqlText);
                sqlText = m_settings.MoSettings.GetCommandText(true);
                IDbCommand moCmd = GetCommand(connection, sqlText);
                sqlText = m_settings.ResourceOperationSettings.GetCommandText(true);
                IDbCommand opCmd = GetCommand(connection, sqlText);
                sqlText = m_settings.ResourceRequirementSettings.GetCommandText(true);
                IDbCommand resReqtCmd = GetCommand(connection, sqlText);
                sqlText = m_settings.RequiredCapabilitySettings.GetCommandText(true);
                IDbCommand resReqtCapCmd = GetCommand(connection, sqlText);
                sqlText = m_settings.InternalActivitySettings.GetCommandText(true);
                IDbCommand activityCmd = GetCommand(connection, sqlText);
                sqlText = m_settings.MaterialSettings.GetCommandText(true);
                IDbCommand matlCmd = GetCommand(connection, sqlText);
                sqlText = m_settings.ProductSettings.GetCommandText(true);
                IDbCommand prodCmd = GetCommand(connection, sqlText);
                sqlText = m_settings.OpAttributeSettings.GetCommandText(true);
                IDbCommand opAttributeCmd = GetCommand(connection, sqlText);
                sqlText = m_settings.SuccessorMoSettings.GetCommandText(true);
                IDbCommand sucMoCmd = GetCommand(connection, sqlText);
                sqlText = m_settings.PathSettings.GetCommandText(true);
                IDbCommand altPathCmd = GetCommand(connection, sqlText);
                sqlText = m_settings.PathNodeSettings.GetCommandText(true);
                IDbCommand altPathNodecmd = GetCommand(connection, sqlText);
                sqlText = m_settings.CustomerConnectionSettings.GetCommandText(true);
                IDbCommand customerCmd = GetCommand(connection, sqlText);
                bool customerCmdHasSourceInfo = m_settings.CustomerConnectionSettings.HasNonEmptySourceExpression();
                return JobT.FetchJobDataSet(jobCmd,
                    moCmd,
                    opCmd,
                    resReqtCmd,
                    resReqtCapCmd,
                    activityCmd,
                    matlCmd,
                    prodCmd,
                    opAttributeCmd,
                    sucMoCmd,
                    altPathCmd,
                    altPathNodecmd,
                    customerCmd,
                    m_settings.ResourceRequirementSettings.HasNonEmptySourceExpression(),
                    m_settings.MaterialSettings.HasNonEmptySourceExpression(),
                    m_settings.ProductSettings.HasNonEmptySourceExpression(),
                    m_settings.InternalActivitySettings.HasNonEmptySourceExpression(),
                    m_settings.OpAttributeSettings.HasNonEmptySourceExpression(),
                    m_settings.SuccessorMoSettings.HasNonEmptySourceExpression(),
                    m_settings.PathSettings.HasNonEmptySourceExpression(),
                    m_settings.CustomerSettings.HasNonEmptySourceExpression() && customerCmdHasSourceInfo,
                    m_settings.IncludeResourceOperations,
                    m_settings.IncludeManufacturingOrders);
            }
        }
        catch (Exception e)
        {
            //Can only throw serializable exceptions from here.
            //Were getting UniqueArrayListException here which caused serialization problem.

            ImportException ie = new(e.Message);
            if (database != "")
            {
                ie = new ImportException("2718", new object[] { database, e.Message });
            }
            else
            {
                ie = new ImportException(e.Message);
            }

            throw ie;
        }
        finally
        {
            if (jobConnection != null)
            {
                if (jobConnection.State != ConnectionState.Closed)
                {
                    jobConnection.Close();
                }

                jobConnection.Dispose();
            }

            if (moConnection != null)
            {
                if (moConnection.State != ConnectionState.Closed)
                {
                    moConnection.Close();
                }

                moConnection.Dispose();
            }

            if (machineOperationConnection != null)
            {
                if (machineOperationConnection.State != ConnectionState.Closed)
                {
                    machineOperationConnection.Close();
                }

                machineOperationConnection.Dispose();
            }

            if (resourceRequirementConnection != null)
            {
                if (resourceRequirementConnection.State != ConnectionState.Closed)
                {
                    resourceRequirementConnection.Close();
                }

                resourceRequirementConnection.Dispose();
            }

            if (internalActivityConnection != null)
            {
                if (internalActivityConnection.State != ConnectionState.Closed)
                {
                    internalActivityConnection.Close();
                }

                internalActivityConnection.Dispose();
            }

            if (requiredCapabilityConnection != null)
            {
                if (requiredCapabilityConnection.State != ConnectionState.Closed)
                {
                    requiredCapabilityConnection.Close();
                }

                requiredCapabilityConnection.Dispose();
            }

            //Cleanup connections that are optional
            if (successorMoConnection != null)
            {
                if (successorMoConnection.State != ConnectionState.Closed)
                {
                    successorMoConnection.Close();
                }

                successorMoConnection.Dispose();
            }

            if (materialConnection != null)
            {
                if (materialConnection.State != ConnectionState.Closed)
                {
                    materialConnection.Close();
                }

                materialConnection.Dispose();
            }

            if (pathConnection != null)
            {
                if (pathConnection.State != ConnectionState.Closed)
                {
                    pathConnection.Close();
                }

                pathConnection.Dispose();
            }

            if (pathNodeConnection != null)
            {
                if (pathNodeConnection.State != ConnectionState.Closed)
                {
                    pathNodeConnection.Close();
                }

                pathNodeConnection.Dispose();
            }

            SqlConnection.ClearAllPools();
        }

    }

    private void LoadInstanceImportSettings()
    {
        InstanceSettingsEntity instanceSettingsEntity = m_utility.GetInstance();
        m_preImportUrl = instanceSettingsEntity.PreImportURL;
        m_runPreImportSql = instanceSettingsEntity.RunPreImportSQL;
        m_preImportSql = instanceSettingsEntity.PreImportSQL;
        m_preImportProgramPath = instanceSettingsEntity.PreImportProgramPath;
        m_preImportProgramArgs = instanceSettingsEntity.PreImportProgramArgs;
        m_postImportUrl = instanceSettingsEntity.PostImportURL;
    }

    private void SetupProgressPoints()
    {
        double totalProgressPoints = 0;
        double currentProgressPoints = 0;

        if (!string.IsNullOrEmpty(m_preImportUrl))
        {
            totalProgressPoints += 300;
        }

        if (m_runPreImportSql)
        {
            totalProgressPoints += 300;
        }

        if (!string.IsNullOrEmpty(m_preImportProgramPath))
        {
            totalProgressPoints += 300;
        }

        foreach (ImportStep importStep in m_steps)
        {
            if (!importStep.ShouldStepBeSkipped())
            {
                totalProgressPoints += importStep.ProgressPoints;
            }
        }

        if (!string.IsNullOrEmpty(m_postImportUrl))
        {
            totalProgressPoints += 300;
        }

        m_statusUpdater.SetInitialProgressPoints(currentProgressPoints, totalProgressPoints, 100);
    }

    private void StartProgress(BaseId a_instigator)
    {
        SetupProgressPoints();

        ImportStatusMessage importStatusMessage = new ImportStatusMessage(a_instigator);
        importStatusMessage.ProgressStep = ImportStatuses.EImportProgressStep.Started;

        m_importStatusMessage = importStatusMessage;
    }

    private void SetProgress(ImportStatuses.EImportProgressStep a_progressStep, double a_progressPercent)
    {
        m_importStatusMessage.ProgressPercent = m_statusUpdater.UpdateProgressPoints(a_progressPercent);
        m_importStatusMessage.ProgressStep = a_progressStep;
    }

    private void SetProgressCompleted(ApplicationExceptionList exceptions)
    {
        m_importStatusMessage.ProgressStep = ImportStatuses.EImportProgressStep.Complete;
        m_importStatusMessage.CompletedTime = DateTime.UtcNow;
        m_importStatusMessage.Exceptions = exceptions;
    }
}
