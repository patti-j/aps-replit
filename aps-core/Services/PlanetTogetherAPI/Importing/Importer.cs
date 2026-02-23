using System.Data;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;

using Microsoft.Data.SqlClient;

using PT.APIDefinitions.Integration;
using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.Common.Http;
using PT.Common.Localization;
using PT.CustomInterface;
using PT.ERPTransmissions;
using PT.ImportDefintions;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.Transmissions;

using ImportRequest = PT.APIDefinitions.Integration.ImportRequest;

namespace PT.PlanetTogetherAPI.Importing;

/// <summary>
/// Summary description for Importer.
/// </summary>
public class Importer
{
    private readonly ImportSettings m_settings;
    private readonly ImportUtilities m_utility;
    private ImportStatusMessage m_importStatusMessage;
    private readonly ImportStatusUpdater m_statusUpdater;

    #region Instance Import Settings
    private string m_preImportUrl;
    private bool m_runPreImportSql;
    private string m_preImportSql;
    private string m_preImportProgramPath;
    private string m_preImportProgramArgs;
    private string m_postImportUrl;
    #endregion

    private long m_scenarioId;

    public Importer(ImportSettings a_settings, ImportUtilities a_utility, ImportStatusMessage a_importStatusMessage)
    {
        m_utility = a_utility;
        m_statusUpdater = new ImportStatusUpdater();
        m_settings = a_settings.Clone(); //To prevent modifications of the original settings.
        m_importStatusMessage = a_importStatusMessage;
    }

    /// <summary>
    /// Creates a connection of the correct type as determined by the ConnectionDef.
    /// </summary>
    public IDbConnection GetConnection()
    {
        ErpDatabase erpSettings = m_utility.GetErpSettings();
        try
        {
            //if (erpSettings.ConnectionType == ErpDatabase.EConnectionTypes.ODBC)
            //    return new OdbcConnection(erpSettings.ConnectionString);
            //else if (erpSettings.ConnectionType == ErpDatabase.EConnectionTypes.OLEDB)
            //    return new OleDbConnection(erpSettings.ConnectionString);
            ////else if (erpSettings.ConnectionType == ErpDatabase.EConnectionTypes.ORACLE)
            //    return new OracleConnection(erpSettings.ConnectionString);
            return new SqlConnection(erpSettings.ConnectionString);
        }
        catch (Exception exc)
        {
            ImporterException e = new ("2352", new object[] { erpSettings.ConnectionString, exc.Message });
            m_utility.LogImporterException(e);
            throw e;
        }
    }

    public IDbCommand GetCommand(IDbConnection connection, string sqlText)
    {
        IDbCommand cmd;
        //if (connection is OdbcConnection)
        //    cmd = new OdbcCommand(sqlText, (OdbcConnection)connection);
        //else if (connection is OleDbConnection)
        //    cmd = new OleDbCommand(sqlText, (OleDbConnection)connection);
        //else if (connection is SqlConnection)
        cmd = new SqlCommand(sqlText, (SqlConnection)connection);
        //else if (connection is OracleConnection)  
        //    cmd = new OracleCommand(sqlText, (OracleConnection)connection);
        //else
        //{
        //ImporterException e = new ImporterException("2353", new object[] { connection.GetType().Name });
        //m_utility.LogImporterException(e);
        //throw e;
        //}
        cmd.CommandTimeout = 0;
        return cmd;
    }

    public ApplicationExceptionList RunImport(bool testonly, Type objectType, BaseId instigator, BaseId a_targetScenarioId, ScenarioExceptionInfo a_sei)
    {
        try
        {
            return PerformImport(testonly, true, objectType, instigator, a_targetScenarioId, a_sei);
        }
        catch (Exception e)
        {
            m_utility.LogImporterException(e);
            throw;
        }
    }

    public ApplicationExceptionList RunImport(bool testonly, BaseId instigator, BaseId a_targetScenarioId, ScenarioExceptionInfo a_sei)
    {
        ApplicationExceptionList exceptions = new ();
        try
        {
            bool runStandard = true;
            if (m_settings.UseCustomInterface)
            {
                runStandard = PerformCustomImport(testonly);
            }

            if (runStandard)
            {
                return PerformImport(testonly, false, null, instigator, a_targetScenarioId, a_sei);
            }
        }
        catch (Exception e)
        {
            exceptions.Add(e);
        }

        return exceptions;
    }

    #region Custom Importer
    private const string CUSTOM_IMPORTER_TYPE_NAME = "PT.CustomInterface.CustomInterface";

    private bool PerformCustomImport(bool testOnly)
    {
        //Note PTSystem does not load like the system, license key is never loaded.
        //if (!PTSystem.LicenseKey.IncludeAddins)
        //{
        //    errors.Add(new PT.APSCommon.AuthorizationException("InterfaceCustomization.", PT.APSCommon.AuthorizationType.LicenseKey, LicenseKey.c_includeAddinsTag, false.ToString()));
        //    return errors;
        //}

        string ExePath = Environment.GetCommandLineArgs()[0];
        string exeFolder = Path.GetDirectoryName(ExePath);

        string customInterfaceFile = Path.Combine(exeFolder, "CustomInterface.dll");
        if (File.Exists(customInterfaceFile))
        {
            //SimpleExceptionLogger.LogMessage(ImportLogFilePath, "Found CustomInterface.dll");
            try
            {
                IPTCustomInterface customization = null;
                Assembly dllAssy = Assembly.LoadFrom(customInterfaceFile);
                foreach (Type t in dllAssy.GetExportedTypes())
                {
                    try
                    {
                        Type[] interfaces = t.GetInterfaces();
                        if (interfaces.ToList().Contains(typeof(IPTCustomInterface)))
                        {
                            ConstructorInfo ctor = t.GetConstructor(new Type[] { });
                            customization = (IPTCustomInterface)ctor.Invoke(new object[] { });
                            //SimpleExceptionLogger.LogMessage(ImportLogFilePath, $"Loaded CustomInterface.dll type: '{customization.GetType()}'");
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        throw new PTValidationException("CustomInterface.dll did not contain a public class implementing IPTCustomInterface with a public constructor with no parameters");
                    }
                }

                if (customization == null)
                {
                    //TODO: Update this message, the namespace is not important anymore
                    throw new PTValidationException("2033", new object[] { customInterfaceFile, CUSTOM_IMPORTER_TYPE_NAME });
                }

                customization.SendTransmissionEvent += new InterfaceDelegate.SendTransmissionHandler(customImporter_SendTransmissionEvent);
                customization.PerformImport(exeFolder);
                return customization.RunStandardImport;
            }
            catch (Exception e)
            {
                string msg = string.Format("Error loading or executing Custom Importer file: {0}.  {1}\r\n{2}", customInterfaceFile, e.Message, e.StackTrace);
                if (e.InnerException != null)
                {
                    msg = string.Format("{0}\r\nInner Exception: {1}, \r\n{2}", msg, e.InnerException.Message, e.InnerException.StackTrace);
                }

                throw new ImportException(msg);
            }
        }

        throw new ImportException("2599", new object[] { customInterfaceFile });
    }

    private void customImporter_SendTransmissionEvent(object sender, PTTransmission t)
    {
        SendTransmission(t);
    }
    #endregion Custom Importer

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

            Process p = new ();
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
            throw new PTException("2960", e, new object[] { m_preImportProgramPath, m_preImportProgramArgs});
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

    private async Task RunPreImportWebhook(long a_targetScenarioId, BaseId a_instigator, string a_preImportUrl)
    {
        try
        {
            if (!string.IsNullOrEmpty(a_preImportUrl))
            {
                Uri preImportUrl = new Uri(a_preImportUrl);
                string apiController = "";
                string apiEndpoint = "";

                // baseUrl must be scheme + host + (optional) port only
                string baseUrl = preImportUrl.GetLeftPart(UriPartial.Authority);

                // PT-style if it's a PT integrations host or my local API on port 55971 (avoids external false positives)
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
                    // For ALL other hosts: respect the URL as-is (no forced /api/Rest/Import)
                    if (preImportUrl.Segments.Length > 1)
                    {
                        // Example: http://localhost:1880/api/aps -> controller="api", endpoint="aps"
                        apiController = string.Join("", preImportUrl.Segments[0..^1]).Trim(' ', '/');
                        apiEndpoint = preImportUrl.Segments.Last().Trim('/'); 
                    }
                    else
                    {
                        // Example: http://localhost:1880 (root) -> post to root
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

                ImportRequest importRequest = new ImportRequest
                {
                    ScenarioId = a_targetScenarioId,
                    StatusStep = EStatusStep.Started
                };

                // IMPORTANT: endpoint must be relative; empty string posts to base/controller
                await ptClient.MakePostRequest(apiEndpoint, importRequest);
            }
        }
        catch (Exception err)
        {
            throw new ImportException("2985", err, new object[] { a_preImportUrl });
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

                SetProgress(ImportStatuses.EImportProgressStep.PostImportWebhook,300);

                TimeSpan timeout = TimeSpan.FromSeconds(m_utility.GetBroadcasterConstructorValues().WebApiTimeoutSeconds);
                PTHttpClient ptClient = new (apiController, m_postImportUrl, timeout, new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{a_instigator.ToString()}:null"))));
                ImportRequest importRequest = new ();
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

    /// <summary>
    /// Use this to create an InterfaceException when object type (Job, Item) that had the error is known.
    /// </summary>
    /// <param name="a_err">inner exception to use</param>
    /// <param name="a_objectType">type of the object that had the exception</param>
    /// <returns>Exception to include in errors list</returns>
    private ImportException GetTypeSpecificInterfaceException(Exception a_err, string a_objectType)
    {
        try
        {
            a_objectType = Localizer.GetString(a_objectType);
        }
        catch { } // forget it, use English

        return new ImportException("2851", a_err, new object[] { a_objectType });
    }

    private ApplicationExceptionList PerformImport(bool a_testOnly, bool a_oneTypeOnly, Type a_typeToInclude, BaseId a_instigator, BaseId a_targetScenarioId, ScenarioExceptionInfo a_sei)
    {
        ApplicationExceptionList errors = new (); //Contains a list of any exceptions encountered while creating the PT objects.            

        ImportT importT = new ();
        importT.SpecificScenarioId = a_targetScenarioId;
        importT.Instigator = a_instigator;
        m_scenarioId = a_targetScenarioId.Value;

        try
        {
            LoadInstanceImportSettings();
            if (PTSystem.EnableDiagnostics)
            {
                m_utility.LogMessage("Import Logging\nImporter: PerfomImport loaded instance import settings.");
            }
            SetupProgressPoints(a_oneTypeOnly, a_typeToInclude);
            StartProgress(a_instigator);

            Task webhookTask = Task.Run(() => RunPreImportWebhook(a_targetScenarioId.ToBaseType(), a_instigator, m_preImportUrl));
            webhookTask.Wait();
            if (PTSystem.EnableDiagnostics)
            {
                m_utility.LogMessage("Import Logging\nImporter: PreImportWebHook ran.");
            }

            RunPreImportPrograms();

            if (PTSystem.EnableDiagnostics)
            {
                m_utility.LogMessage("Import Logging\nImporter: Pre import programs ran.");
            }

            RunPreImportSQL();

            if (PTSystem.EnableDiagnostics)
            {
                m_utility.LogMessage("Import Logging\nImporter: Pre import sql command executed");
            }

            //UserFields
            if ((!a_oneTypeOnly && m_settings.IncludeUserFields) || (a_oneTypeOnly && a_typeToInclude == typeof(UserFieldDefinitionT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.UserFields, 100);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        if (m_settings.UserFieldSettings.HasNonEmptySourceExpression())
                        {
                            string sqlTable = m_settings.UserFieldSettings.GetCommandText(true);
                            IDbCommand cmdTable = GetCommand(connection, sqlTable);
                            UserFieldDefinitionT userFieldDefinitionT = new ();

                            userFieldDefinitionT.Fill(cmdTable);
                            userFieldDefinitionT.AutoDeleteMode = m_settings.UserFieldSettings.AutoDelete;
                            if (!a_testOnly)
                            {
                                importT.Add(userFieldDefinitionT);
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "User Field"));
                    }
                }
            }

            //Customers
            if ((!a_oneTypeOnly && m_settings.IncludeCustomers) || (a_oneTypeOnly && a_typeToInclude == typeof(CustomerT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Customers, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string sqlTable = m_settings.CustomerSettings.GetCommandText(true);
                        IDbCommand cmdTable = GetCommand(connection, sqlTable);
                        CustomerT customerT = new();

                        customerT.Fill(cmdTable);
                        customerT.AutoDeleteMode = m_settings.CustomerSettings.AutoDelete;
                        if (!a_testOnly)
                        {
                            importT.Add(customerT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Customer"));
                    }
                }
            }
            //Users
            if ((!a_oneTypeOnly && m_settings.IncludeUsers) || (a_oneTypeOnly && a_typeToInclude == typeof(UserT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Users, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string sqlText = m_settings.UserSettings.GetCommandText(true);
                        IDbCommand command = GetCommand(connection, sqlText);
                        UserT userT = new();
                        userT.AutoDeleteMode = m_settings.UserSettings.AutoDelete;
                        userT.Fill(command);
                        if (!a_testOnly)
                        {
                            importT.Add(userT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "User"));
                    }
                }
            }

            //Plants
            if ((!a_oneTypeOnly && m_settings.IncludePlants) || (a_oneTypeOnly && a_typeToInclude == typeof(PlantT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Plants, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string sqlText = m_settings.PlantSettings.GetCommandText(true);
                        IDbCommand command = GetCommand(connection, sqlText);

                        PlantT plantT = new ();
                        plantT.AutoDeleteMode = m_settings.PlantSettings.AutoDelete;
                        plantT.Fill(command);
                        if (!a_testOnly)
                        {
                            importT.Add(plantT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Plant"));
                    }
                }
            }

            //Departments
            if ((!a_oneTypeOnly && m_settings.IncludeDepartments) || (a_oneTypeOnly && a_typeToInclude == typeof(DepartmentT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Departments, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string sqlText = m_settings.DepartmentSettings.GetCommandText(true);
                        IDbCommand command = GetCommand(connection, sqlText);
                        DepartmentT deptT = new ();
                        deptT.AutoDeleteMode = m_settings.DepartmentSettings.AutoDelete;
                        deptT.Fill(command);
                        if (!a_testOnly)
                        {
                            importT.Add(deptT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Departments"));
                    }
                }
            }

            //Capabilities
            if ((!a_oneTypeOnly && m_settings.IncludeCapabilities) || (a_oneTypeOnly && a_typeToInclude == typeof(CapabilityT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Capabilities, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string sqlText = m_settings.CapabilitySettings.GetCommandText(true);
                        IDbCommand command = GetCommand(connection, sqlText);
                        CapabilityT capT = new ();
                        capT.Fill(command);
                        capT.AutoDeleteMode = m_settings.CapabilitySettings.AutoDelete;
                        if (!a_testOnly)
                        {
                            importT.Add(capT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Capability"));
                    }
                }
            }

            //Cells
            if ((!a_oneTypeOnly && m_settings.IncludeCells) || (a_oneTypeOnly && a_typeToInclude == typeof(CellT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Cells, 50);
                using (IDbConnection machineConnection = OpenNewConnection())
                {
                    try
                    {
                        string cellSqlText = m_settings.CellsSettings.GetCommandText(true);
                        IDbCommand cellsCmd = GetCommand(machineConnection, cellSqlText);

                        CellT cellT = new ();

                        cellT.Fill(cellsCmd);
                        cellT.AutoDeleteMode = m_settings.CellsSettings.AutoDelete;
                        if (!a_testOnly)
                        {
                            importT.Add(cellT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Cell"));
                    }
                }
            }

            //Resources
            if ((!a_oneTypeOnly && m_settings.IncludeResources) || (a_oneTypeOnly && a_typeToInclude == typeof(ResourceT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Resources, 50);
                using (IDbConnection machineConnection = OpenNewConnection())
                {
                    try
                    {
                        string machSqlText = m_settings.MachineSettings.GetCommandText(true);
                        IDbCommand machCommand = GetCommand(machineConnection, machSqlText);

                        string machCapabAssSqlText = m_settings.CapabilityAssignmentSettings.GetCommandText(true);
                        string allowedHelpersSqlText = m_settings.AllowedHelperResourcesSettings.GetCommandText(true);
                        IDbCommand machCapabAssCommand = GetCommand(machineConnection, machCapabAssSqlText);
                        IDbCommand allowedHelpersCommand = GetCommand(machineConnection, allowedHelpersSqlText);

                        ResourceT resourceT = new ();
                        resourceT.AutoDeleteMode = m_settings.MachineSettings.AutoDelete;
                        resourceT.AutoDeleteCapabilityAssociations = m_settings.CapabilityAssignmentSettings.AutoDelete;
                        resourceT.AutoDeleteAllowedHelpers = m_settings.AllowedHelperResourcesSettings.AutoDelete;
                        resourceT.UpdateAllowedHelpers = m_settings.IncludeAllowedHelperResources;
                        resourceT.Fill(machCommand, machCapabAssCommand, m_settings.IncludeCapabilityAssignments, m_settings.IncludeAllowedHelperResources, allowedHelpersCommand);
                        if (!a_testOnly)
                        {
                            importT.Add(resourceT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Resource"));
                    }
                }
            }

            if ((!a_oneTypeOnly && m_settings.IncludeResourceConnectors) || (a_oneTypeOnly && a_typeToInclude == typeof(ResourceConnectorsT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.ResourceConnectors, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string sqlTable = m_settings.ResourceConnectorsSettings.GetCommandText(true);
                        IDbCommand cmdTables = GetCommand(connection, sqlTable);
                        string sqlResourceConnection = m_settings.ResourceConnectionSettings.GetCommandText(true);
                        IDbCommand cmdResourceConnection = GetCommand(connection, sqlResourceConnection);
                        ResourceConnectorsT resConnectorsT = new();
                        resConnectorsT.Fill(cmdTables, cmdResourceConnection, errors);
                        resConnectorsT.AutoDeleteMode = m_settings.ResourceConnectorsSettings.AutoDelete;
                        resConnectorsT.AutoDeleteConnections = m_settings.ResourceConnectionSettings.AutoDelete;
                        if (!a_testOnly)
                        {
                            importT.Add(resConnectorsT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Resource Connectors"));
                    }
                }
            }

            //Items Only
            if (a_oneTypeOnly && a_typeToInclude == typeof(ItemT))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Items, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string sqlText = m_settings.ItemSettings.GetCommandText(true);
                        IDbCommand command = GetCommand(connection, sqlText);

                        WarehouseT warehouseT = new ();
                        warehouseT.AutoDeleteItems = m_settings.ItemSettings.AutoDelete;
                        warehouseT.Fill(null, null, null, null, command, null, null, null,null,null,null,null, null);
                        if (!a_testOnly)
                        {
                            importT.Add(warehouseT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Item"));
                    }
                }
            }

            //Warehouses
            if ((!a_oneTypeOnly && m_settings.IncludeWarehouses) || (a_oneTypeOnly && a_typeToInclude == typeof(WarehouseT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Warehouses, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    using (IDbConnection plantWarehouseConnection = OpenNewConnection())
                    {
                        string warehouseSqlText = m_settings.WarehouseSettings.GetCommandText(true);
                        IDbCommand warehouseCommand = GetCommand(connection, warehouseSqlText);

                        string plantWarehouseSqlText = m_settings.PlantWarehouseSettings.GetCommandText(true);
                        IDbCommand plantWarehouseCommand = GetCommand(plantWarehouseConnection, plantWarehouseSqlText);

                        IDbCommand inventoryCommand = null;
                        IDbCommand itemsCommand = null;
                        IDbCommand lotsCommand = null;
                        IDbCommand storageAreaCommand = null;
                        IDbCommand itemStorageCommand = null;
                        IDbCommand itemStorageLotCommand = null;
                        IDbCommand storageAreaConnectorCommand = null;
                        IDbCommand storageAreaConnectorsInCommand = null;
                        IDbCommand storageAreaConnectorsOutCommand = null;
                        IDbCommand resourceStorageAreaConnectorsInCommand = null;
                        IDbCommand resourceStorageAreaConnectorsOutCommand = null;

                        IDbConnection inventoryConnection = null;
                        IDbConnection itemsConnection = null;
                        IDbConnection lotsConnection = null;
                        IDbConnection storageAreaConnection = null;
                        IDbConnection itemStorageConnection = null;
                        IDbConnection itemStorageLotConnection = null;
                        IDbConnection storageAreaConnectorConnection = null;
                        IDbConnection storageAreaConnectorsInConnection = null;
                        IDbConnection storageAreaConnectorsOutConnection = null;
                        IDbConnection resourceStorageAreaConnectorsInConnection = null;
                        IDbConnection resourceStorageAreaConnectorsOutConnection = null;
                        try
                        {
                            WarehouseT warehouseT = new ();
                            warehouseT.AutoDeleteMode = m_settings.WarehouseSettings.AutoDelete;

                            //Inventories
                            if (m_settings.IncludeInventories)
                            {
                                SetProgress(ImportStatuses.EImportProgressStep.Inventory, 50);
                                inventoryConnection = OpenNewConnection();
                                string inventorySqlText = m_settings.InventorySettings.GetCommandText(true);
                                inventoryCommand = GetCommand(inventoryConnection, inventorySqlText);
                                warehouseT.AutoDeleteInventories = m_settings.InventorySettings.AutoDelete;

                                //Lots
                                if (m_settings.IncludeLots)
                                {
                                    SetProgress(ImportStatuses.EImportProgressStep.Lots, 50);
                                    lotsConnection = OpenNewConnection();
                                    string lotsSqlText = m_settings.LotsSettings.GetCommandText(true);
                                    lotsCommand = GetCommand(lotsConnection, lotsSqlText);
                                    warehouseT.AutoDeleteLots = m_settings.LotsSettings.AutoDelete;
                                }
                            }

                            //Items
                            if (m_settings.IncludeItems)
                            {
                                SetProgress(ImportStatuses.EImportProgressStep.Items, 50);
                                itemsConnection = OpenNewConnection();
                                string itemsSqlText = m_settings.ItemSettings.GetCommandText(true);
                                itemsCommand = GetCommand(itemsConnection, itemsSqlText);
                                warehouseT.AutoDeleteItems = m_settings.ItemSettings.AutoDelete;
                            }

                            //Storage Area
                            if (m_settings.IncludeStorageArea)
                            {
                                storageAreaConnection = OpenNewConnection();
                                string storageAreaSqlText = m_settings.StorageAreaSettings.GetCommandText(true);
                                storageAreaCommand = GetCommand(storageAreaConnection, storageAreaSqlText);
                                warehouseT.AutoDeleteStorageAreas = m_settings.StorageAreaSettings.AutoDelete;
                            }
                            
                            //Item Storage
                            if (m_settings.IncludeItemStorage)
                            {
                                itemStorageConnection = OpenNewConnection();
                                string itemStorageSqlText = m_settings.ItemStorageSettings.GetCommandText(true);
                                itemStorageCommand = GetCommand(itemStorageConnection, itemStorageSqlText);
                                warehouseT.AutoDeleteItemStorage = m_settings.ItemStorageSettings.AutoDelete;
                            } 
                            
                            //Item Storage Lots
                            if (m_settings.IncludeItemStorageLots)
                            {
                                itemStorageLotConnection = OpenNewConnection();
                                string itemStorageLotSqlText = m_settings.ItemStorageLotsSettings.GetCommandText(true);
                                itemStorageLotCommand = GetCommand(itemStorageLotConnection, itemStorageLotSqlText);
                                warehouseT.AutoDeleteItemStorageLots = m_settings.ItemStorageLotsSettings.AutoDelete;
                            }

                            //Storage Area Connectors
                            if (m_settings.IncludeStorageAreaConnectors)
                            {
                                storageAreaConnectorConnection =OpenNewConnection();
                                string storageAreaConnectorSqlText = m_settings.StorageAreaConnectorSettings.GetCommandText(true);
                                storageAreaConnectorCommand = GetCommand(storageAreaConnectorConnection, storageAreaConnectorSqlText);
                                warehouseT.AutoDeleteStorageAreaConnectors = m_settings.StorageAreaConnectorSettings.AutoDelete;

                                //Storage Area Connectors In
                                if (m_settings.IncludeStorageAreaConnectorIn)
                                {
                                    storageAreaConnectorsInConnection = OpenNewConnection();
                                    string storageAreaConnectorsInSqlText = m_settings.StorageAreaConnectorInSettings.GetCommandText(true);
                                    storageAreaConnectorsInCommand = GetCommand(storageAreaConnectorsInConnection, storageAreaConnectorsInSqlText);
                                    warehouseT.AutoDeleteStorageAreaConnectorsIn = m_settings.StorageAreaConnectorInSettings.AutoDelete;
                                }

                                //Storage Area Connectors Out
                                if (m_settings.IncludeStorageAreaConnectorOut)
                                {
                                    storageAreaConnectorsOutConnection = OpenNewConnection();
                                    string storageAreaConnectorsOutSqlText = m_settings.StorageAreaConnectorOutSettings.GetCommandText(true);
                                    storageAreaConnectorsOutCommand = GetCommand(storageAreaConnectorsOutConnection, storageAreaConnectorsOutSqlText);
                                    warehouseT.AutoDeleteStorageAreaConnectorsOut = m_settings.StorageAreaConnectorOutSettings.AutoDelete;

                                }

                                //Resource Storage Area Connectors In
                                if (m_settings.IncludeResourceStorageAreaConnectorIn)
                                {
                                    resourceStorageAreaConnectorsInConnection = OpenNewConnection();
                                    string resourceStorageAreaConnectorsInSqlText = m_settings.ResourceStorageAreaConnectorInSettings.GetCommandText(true);
                                    resourceStorageAreaConnectorsInCommand = GetCommand(resourceStorageAreaConnectorsInConnection, resourceStorageAreaConnectorsInSqlText);
                                    warehouseT.AutoDeleteResourceStorageAreaConnectorIn = m_settings.ResourceStorageAreaConnectorInSettings.AutoDelete;
                                }

                                //Resource Storage Area Connectors Out
                                if (m_settings.IncludeResourceStorageAreaConnectorOut)
                                {
                                    resourceStorageAreaConnectorsOutConnection = OpenNewConnection();
                                    string resourceStorageAreaConnectorsOutSqlText = m_settings.ResourceStorageAreaConnectorOutSettings.GetCommandText(true);
                                    resourceStorageAreaConnectorsOutCommand = GetCommand(resourceStorageAreaConnectorsOutConnection, resourceStorageAreaConnectorsOutSqlText);
                                    warehouseT.AutoDeleteResourceStorageAreaConnectorOut = m_settings.ResourceStorageAreaConnectorOutSettings.AutoDelete;
                                }
                            }

                            warehouseT.Fill(warehouseCommand, plantWarehouseCommand, inventoryCommand, lotsCommand, itemsCommand, storageAreaCommand, itemStorageCommand, itemStorageLotCommand, storageAreaConnectorCommand, storageAreaConnectorsInCommand, storageAreaConnectorsOutCommand, resourceStorageAreaConnectorsInCommand, resourceStorageAreaConnectorsOutCommand);

                            if (!a_testOnly)
                            {
                                importT.Add(warehouseT);
                            }
                        }
                        catch (Exception err)
                        {
                            errors.Add(GetTypeSpecificInterfaceException(err, "Warehouse and Inventory"));
                        }
                        finally
                        {
                            //Need to dispose of manually since not in Using clause.
                            if (inventoryConnection != null)
                            {
                                if (inventoryConnection.State != ConnectionState.Closed)
                                {
                                    inventoryConnection.Close();
                                }

                                inventoryConnection.Dispose();
                            }

                            if (itemsConnection != null)
                            {
                                if (itemsConnection.State != ConnectionState.Closed)
                                {
                                    itemsConnection.Close();
                                }

                                itemsConnection.Dispose();
                            }

                            if (lotsConnection != null)
                            {
                                if (lotsConnection.State != ConnectionState.Closed)
                                {
                                    lotsConnection.Close();
                                }

                                lotsConnection.Dispose();
                            }
                        }
                    }
                }
            }

            //CapacityIntervals
            if ((!a_oneTypeOnly && m_settings.IncludeCapacityIntervals) || (a_oneTypeOnly && a_typeToInclude == typeof(CapacityIntervalT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Capacity, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string ciSqlTxt = m_settings.CapacityIntervalSettings.GetCommandText(true);
                        string resSqlTxt = m_settings.CapacityIntervalResourceSettings.GetCommandText(true);
                        IDbCommand ciCmd = GetCommand(connection, ciSqlTxt);
                        IDbCommand resCmd = GetCommand(connection, resSqlTxt);
                        CapacityIntervalT cit = new ();
                        cit.AutoDeleteMode = m_settings.CapacityIntervalSettings.AutoDelete;
                        cit.AutoDeleteResourceAssociations = m_settings.CapacityIntervalResourceSettings.AutoDelete;
                        cit.Fill(ciCmd, resCmd);
                        if (!a_testOnly)
                        {
                            importT.Add(cit);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Capacity Interval"));
                    }
                }
            }

            //RecurringCapacityIntervals
            if ((!a_oneTypeOnly && m_settings.IncludeRecurringCapacityIntervals) || (a_oneTypeOnly && a_typeToInclude == typeof(RecurringCapacityIntervalT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Capacity, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string sqlText = m_settings.RecurringCapacityIntervalSettings.GetCommandText(true);
                        string resSqlTxt = m_settings.CapacityIntervalResourceSettings.GetCommandText(true);

                        IDbCommand command = GetCommand(connection, sqlText);
                        IDbCommand resCmd = GetCommand(connection, resSqlTxt);
                        RecurringCapacityIntervalT rciT = new ();

                        rciT.AutoDeleteMode = m_settings.RecurringCapacityIntervalSettings.AutoDelete;
                        rciT.AutoDeleteResourceAssociations = m_settings.CapacityIntervalResourceSettings.AutoDelete;
                        rciT.Fill(command, resCmd);
                        if (!a_testOnly)
                        {
                            importT.Add(rciT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Recurring Capacity Interval"));
                    }
                }
            }

            //ProductRules
            if ((!a_oneTypeOnly && m_settings.IncludeProductRules) || (a_oneTypeOnly && a_typeToInclude == typeof(ProductRulesT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.ProductRules, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string sqlText = m_settings.ProductRulesSettings.GetCommandText(true);
                        IDbCommand command = GetCommand(connection, sqlText);
                        ProductRulesT prt = new ();
                        prt.Fill(command);
                        prt.AutoDeleteMode = m_settings.ProductRulesSettings.AutoDelete;
                        if (!a_testOnly)
                        {
                            importT.Add(prt);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Product Rule"));
                    }
                }
            }

            //Attributes
            if ((!a_oneTypeOnly && m_settings.IncludeAttributes) || (a_oneTypeOnly && a_typeToInclude == typeof(PTAttributeT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Attributes, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string sqlText = m_settings.AttributeSettings.GetCommandText(true);
                        IDbCommand command = GetCommand(connection, sqlText);
                        PTAttributeT ptAttributeT = new ();
                        ptAttributeT.Fill(command);
                        ptAttributeT.AutoDeleteMode = m_settings.AttributeSettings.AutoDelete;
                        if (!a_testOnly)
                        {
                            importT.Add(ptAttributeT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Attribute"));
                    }
                }
            }

            //Attributes Setup Table
            if ((!a_oneTypeOnly && m_settings.IncludeAttributeSetupTables) || (a_oneTypeOnly && a_typeToInclude == typeof(LookupAttributeNumberRangeT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Attributes, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string sqlTable = m_settings.SetupTableAttSettings.GetCommandText(true);
                        IDbCommand cmdTable = GetCommand(connection, sqlTable);
                        string sqlAtt = m_settings.SetupTableAttNameSettings.GetCommandText(true);
                        IDbCommand cmdAtt = GetCommand(connection, sqlAtt);
                        string sqlFrom = m_settings.SetupTableAttFromSettings.GetCommandText(true);
                        IDbCommand cmdFrom = GetCommand(connection, sqlFrom);
                        string sqlTo = m_settings.SetupTableAttToSettings.GetCommandText(true);
                        IDbCommand cmdTo = GetCommand(connection, sqlTo);
                        string sqlRes = m_settings.SetupTableAttResourceSettings.GetCommandText(true);
                        IDbCommand cmdRes = GetCommand(connection, sqlRes);
                        LookupAttributeNumberRangeT prt = new ();
                        prt.Fill(cmdTable, cmdAtt, cmdFrom, cmdTo, cmdRes);
                        prt.AutoDeleteMode = m_settings.SetupTableAttSettings.AutoDelete;
                        if (!a_testOnly)
                        {
                            importT.Add(prt);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Attribute Number Range Table"));
                    }
                }
            }

            // AttributeCode Table
            if ((!a_oneTypeOnly && m_settings.IncludeAttributeCodeTables) || (a_oneTypeOnly && a_typeToInclude == typeof(LookupAttributeCodeTableT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Attributes, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string sqlTable = m_settings.AttributeCodeTableSetting.GetCommandText(true);
                        IDbCommand cmdTable = GetCommand(connection, sqlTable);
                        string sqlAtt = m_settings.AttributeCodeTableAttributeExternalIdSetting.GetCommandText(true);
                        IDbCommand cmdAtt = GetCommand(connection, sqlAtt);
                        string sqlCodes = m_settings.AttributeCodeTableAttributeCodesSetting.GetCommandText(true);
                        IDbCommand cmdCodes = GetCommand(connection, sqlCodes);
                        string sqlRes = m_settings.AttributeCodeTableAssignedResourcesSetting.GetCommandText(true);
                        IDbCommand cmdRes = GetCommand(connection, sqlRes);
                        LookupAttributeCodeTableT prt = new ();
                        prt.Fill(cmdTable, cmdAtt, cmdCodes, cmdRes);
                        prt.AutoDeleteMode = m_settings.AttributeCodeTableSetting.AutoDelete;
                        if (!a_testOnly)
                        {
                            importT.Add(prt);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Attribute Code Table"));
                    }
                }
            }

            // Cleanout Trigger Tables
            if ((!a_oneTypeOnly && m_settings.IncludeCleanoutTriggerTables) || (a_oneTypeOnly && a_typeToInclude == typeof(CleanoutTriggerTablesT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.CleanoutIntervals, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        bool importOpCountCleanoutTriggersHasValidSelectAndFromExpressions = true;
                        bool importProdUnitsCleanoutTriggersHasValidSelectAndFromExpressions = true;
                        bool importTimeCleanoutTriggersHasValidSelectAndFromExpressions = true;

                        string sqlTable = m_settings.CleanoutTriggerTablesSettings.GetCommandText(true);
                        IDbCommand cmdTables = GetCommand(connection, sqlTable);
                    
                        string sqlCleanoutTblAssignedResources = m_settings.CleanoutTriggerTablesAssignedResourcesSettings.GetCommandText(true);
                        IDbCommand cmdCleanoutTblAssignedResources = GetCommand(connection, sqlCleanoutTblAssignedResources);
                    
                        string sqlOpCountCleanoutTriggers = m_settings.OperationCountCleanoutTriggersSettings.GetCommandText(true);
                        IDbCommand cmdOpCountCleanoutTriggers = GetCommand(connection, sqlOpCountCleanoutTriggers);
                        if (!m_settings.OperationCountCleanoutTriggersSettings.HasNonEmptySourceExpression()
                            && m_settings.OperationCountCleanoutTriggersSettings.FromjoinExpression.Trim() == string.Empty)
                        {
                            importOpCountCleanoutTriggersHasValidSelectAndFromExpressions = false;
                        }

                        string sqlProdUnitsCleanoutTriggers = m_settings.ProductionUnitCleanoutTriggersSettings.GetCommandText(true);
                        IDbCommand cmdProdUnitsCleanoutTriggers = GetCommand(connection, sqlProdUnitsCleanoutTriggers);
                        if (!m_settings.ProductionUnitCleanoutTriggersSettings.HasNonEmptySourceExpression()
                            && m_settings.ProductionUnitCleanoutTriggersSettings.FromjoinExpression.Trim() == string.Empty)
                        {
                            importProdUnitsCleanoutTriggersHasValidSelectAndFromExpressions = false;
                        }

                        string sqlTimeCleanoutTriggers = m_settings.TimeCleanoutTriggersSettings.GetCommandText(true);
                        IDbCommand cmdTimeCleanoutTriggers = GetCommand(connection, sqlTimeCleanoutTriggers);
                        if (!m_settings.TimeCleanoutTriggersSettings.HasNonEmptySourceExpression()
                            && m_settings.TimeCleanoutTriggersSettings.FromjoinExpression.Trim() == string.Empty)
                        {
                            importTimeCleanoutTriggersHasValidSelectAndFromExpressions = false;
                        }

                        CleanoutTriggerTablesT prt = new();
                        prt.Fill(cmdTables, cmdCleanoutTblAssignedResources, cmdOpCountCleanoutTriggers, importOpCountCleanoutTriggersHasValidSelectAndFromExpressions, cmdTimeCleanoutTriggers, importTimeCleanoutTriggersHasValidSelectAndFromExpressions, cmdProdUnitsCleanoutTriggers, importProdUnitsCleanoutTriggersHasValidSelectAndFromExpressions, errors);
                        prt.AutoDelete = m_settings.CleanoutTriggerTablesSettings.AutoDelete;
                        prt.AutoDeleteOperationCountCleanoutTriggers = m_settings.OperationCountCleanoutTriggersSettings.AutoDelete;
                        prt.AutoDeleteProductionUnitsCleanoutTriggers = m_settings.ProductionUnitCleanoutTriggersSettings.AutoDelete;
                        prt.AutoDeleteTimeCleanoutTriggers = m_settings.TimeCleanoutTriggersSettings.AutoDelete;
                        if (!a_testOnly)
                        {
                            importT.Add(prt);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Cleanout Triggers Tables"));
                    }
                }
            }

            // Item Cleanout Tables
            if ((!a_oneTypeOnly && m_settings.IncludeStorageAreaItemCleanoutTables) || (a_oneTypeOnly && a_typeToInclude == typeof(LookupItemCleanoutTableT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.CleanoutIntervals, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string sqlTable = m_settings.StorageAreaItemCleanoutTablesSettings.GetCommandText(true);
                        IDbCommand cmdTables = GetCommand(connection, sqlTable);

                        string sqlCleanoutTblAssignedResources = m_settings.StorageAreaCleanAssignmentSettings.GetCommandText(true);
                        IDbCommand cmdCleanoutTblAssignedResources = GetCommand(connection, sqlCleanoutTblAssignedResources);

                        string sqlOpCountCleanoutTriggers = m_settings.StorageAreaItemCleanoutsSettings.GetCommandText(true);
                        IDbCommand cmdItemCleanouts = GetCommand(connection, sqlOpCountCleanoutTriggers);
                        
                        LookupItemCleanoutTableT tableT = new();
                        tableT.Fill(cmdTables, cmdItemCleanouts, cmdCleanoutTblAssignedResources, errors);
                        tableT.AutoDelete = m_settings.StorageAreaItemCleanoutTablesSettings.AutoDelete;
                        if (!a_testOnly)
                        {
                            importT.Add(tableT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Item Cleanout Tables"));
                    }
                }
            }

            // Compatibility Code Tables
            if ((!a_oneTypeOnly && m_settings.IncludeCompatibilityCodeTables) || (a_oneTypeOnly && a_typeToInclude == typeof(CompatibilityCodeTableT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Compatibility, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string sqlTable = m_settings.CompatibilityCodeTablesSettings.GetCommandText(true);
                        IDbCommand cmdTables = GetCommand(connection, sqlTable);
                        string sqlCompatibilityCodesAssignedResources = m_settings.CompatibilityCodeTablesAssignedResourcesSettings.GetCommandText(true);
                        IDbCommand cmdCompatibilityCodesTblAssignedResources = GetCommand(connection, sqlCompatibilityCodesAssignedResources);
                        string sqlCompatibilityCodes = m_settings.CompatibilityCodesSettings.GetCommandText(true);
                        IDbCommand cmdCompatibilityCodes = GetCommand(connection, sqlCompatibilityCodes);
                        CompatibilityCodeTableT prt = new();
                        prt.Fill(cmdTables,cmdCompatibilityCodes, cmdCompatibilityCodesTblAssignedResources);
                        prt.AutoDeleteMode = m_settings.CompatibilityCodeTablesSettings.AutoDelete;
                        if (!a_testOnly)
                        {
                            importT.Add(prt);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Compatibility Code Tables"));
                    }
                }
            }

            //Purchases To Stock
            if ((!a_oneTypeOnly && m_settings.IncludePurchasesToStock) || (a_oneTypeOnly && a_typeToInclude == typeof(PurchaseToStockT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.PurchaseToStock, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string sqlText = m_settings.PurchaseToStockSettings.GetCommandText(true);
                        IDbCommand command = GetCommand(connection, sqlText);
                        PurchaseToStockT purchaseT = new ();
                        purchaseT.Fill(command, PurchaseToStockDefs.EMaintenanceMethod.ERP);
                        purchaseT.AutoDeleteMode = m_settings.PurchaseToStockSettings.AutoDelete;
                        if (!a_testOnly)
                        {
                            importT.Add(purchaseT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Purchase Order"));
                    }
                }
            }

            //Sales Orders
            if ((!a_oneTypeOnly && m_settings.IncludeSalesOrders) || (a_oneTypeOnly && a_typeToInclude == typeof(SalesOrderT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.SalesOrders, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string soSqlText = m_settings.SalesOrderSettings.GetCommandText(true);
                        IDbCommand soCommand = GetCommand(connection, soSqlText);
                        string soLineSqlText = m_settings.SalesOrderLineSettings.GetCommandText(true);
                        IDbCommand soLineCommand = GetCommand(connection, soLineSqlText);
                        string soLineDistSqlText = m_settings.SalesOrderLineDistSettings.GetCommandText(true);
                        IDbCommand soLineDistCommand = GetCommand(connection, soLineDistSqlText);

                        SalesOrderT soT = new ();
                        soT.Fill(soCommand, soLineCommand, soLineDistCommand);
                        soT.AutoDeleteMode = m_settings.SalesOrderSettings.AutoDelete;
                        if (!a_testOnly)
                        {
                            importT.Add(soT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Sales Order"));
                    }
                }
            }

            //Forecasts
            if ((!a_oneTypeOnly && m_settings.IncludeForecasts) || (a_oneTypeOnly && a_typeToInclude == typeof(ForecastT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Forecasts, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string forecastSqlText = m_settings.ForecastSettings.GetCommandText(true);
                        IDbCommand forecastCommand = GetCommand(connection, forecastSqlText);
                        string shipmentSqlText = m_settings.ForecastShipmentSettings.GetCommandText(true);
                        IDbCommand shipmentCommand = GetCommand(connection, shipmentSqlText);
                        ForecastT forecastT = new ();
                        forecastT.Fill(forecastCommand, shipmentCommand, errors);
                        forecastT.AutoDeleteMode = m_settings.ForecastSettings.AutoDelete;
                        if (!a_testOnly)
                        {
                            importT.Add(forecastT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Forecast"));
                    }
                }
            }

            //Transfer Order
            if ((!a_oneTypeOnly && m_settings.IncludeTransferOrders) || (a_oneTypeOnly && a_typeToInclude == typeof(TransferOrderT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.TransferOrders, 50);
                using (IDbConnection connection = OpenNewConnection())
                {
                    try
                    {
                        string transferOrderSqlText = m_settings.TransferOrderSettings.GetCommandText(true);
                        IDbCommand transferOrderCommand = GetCommand(connection, transferOrderSqlText);
                        string transferOrderDistSqlText = m_settings.TransferOrderDistributionSettings.GetCommandText(true);
                        IDbCommand transferOrderDistCommand = GetCommand(connection, transferOrderDistSqlText);
                        TransferOrderT transferOrderT = new ();
                        transferOrderT.Fill(transferOrderCommand, transferOrderDistCommand, JobDefs.EMaintenanceMethod.ERP);
                        transferOrderT.AutoDeleteMode = m_settings.TransferOrderSettings.AutoDelete;
                        if (!a_testOnly)
                        {
                            importT.Add(transferOrderT);
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(GetTypeSpecificInterfaceException(err, "Transfer Order"));
                    }
                }
            }


            JobDataSet jobDataSet = null;
            HashSet<string> jobTActivityUniqueIds = new (); // this holds ERPTransmissions.Activity.GetUniqueKey() for every Activity that's added to JobT. Used to determine if ActivityUpdateT should be sent.

            //Jobs
            if ((!a_oneTypeOnly && m_settings.IncludeJobs) || (a_oneTypeOnly && a_typeToInclude == typeof(JobT)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Jobs, 50);
                JobT jobT = new ();
                jobT.AutoDeleteMode = m_settings.JobSettings.AutoDelete;
                try
                {
                    jobDataSet = FetchJobDataSet();
                    jobT.Fill(ref errors, jobDataSet, !m_settings.IncludePaths, jobTActivityUniqueIds);
                    jobT.AutoDeleteCustomerAssociations = m_settings.CustomerConnectionSettings.AutoDelete;
                    jobT.IncludeCustomerAssociations = m_settings.CustomerConnectionSettings.HasNonEmptySourceExpression();
                    jobT.AutoDeleteOperationAttributes = m_settings.OpAttributeSettings.AutoDelete;

                    if (!a_testOnly)
                    {
                        importT.Add(jobT);
                        if (jobT.Count <= 0)
                        {
                            errors.Add(new ImportException("2719"));
                        }
                    }
                }
                catch (ImportException interfaceErr)
                {
                    errors.Add(interfaceErr);
                }
                catch (Exception err)
                {
                    errors.Add(GetTypeSpecificInterfaceException(err, "Job"));
                }
            }

            // ActivityUpdateT
            if ((!a_oneTypeOnly && m_settings.IncludeInternalActivity) || (a_oneTypeOnly && a_typeToInclude == typeof(JobT.InternalActivity)))
            {
                SetProgress(ImportStatuses.EImportProgressStep.Activities, 50);
                try
                {
                    ActivityUpdateT actUpdateT = GetActivityUpdateT(jobDataSet, jobTActivityUniqueIds);
                    if (actUpdateT != null && actUpdateT.Count > 0)
                    {
                        actUpdateT.Validate();
                        importT.Add(actUpdateT);
                    }
                }
                catch (Exception err)
                {
                    errors.Add(GetTypeSpecificInterfaceException(err, "Activity Update"));
                }
            }
        }
        catch (ImportException interfaceErr)
        {
            errors.Add(interfaceErr);
        }
        catch (Exception err)
        {
            errors.Add(new ImportException("2852", err));
        }
        finally
        {
            // Send transmission
            SendTransmission(importT);
            if (PTSystem.EnableDiagnostics)
            {
                m_utility.LogMessage("Import Logging\nImportingService: Sent ImportT.");
            }
        }

        try
        {
            Task postImportWebhook = Task.Run(() => RunPostImportWebhook(a_targetScenarioId.ToBaseType(), a_instigator));
            postImportWebhook.Wait();
        }
        catch (ImportException interfaceErr)
        {
            errors.Add(interfaceErr);
        }
        catch (Exception err)
        {
            errors.Add(new ImportException("2852", err));
        }

        SetProgressCompleted(errors);

        ///Log any errors
        if (errors.Count > 0)
        {
            ApplicationExceptionList.Node node = errors.First;
            int loggedCnt = 0;
            while (node != null && loggedCnt < 100) //NO sense filling the log with loads of exceptions.
            {
                m_utility.LogImporterException(node.Data, a_sei);
                node = node.Next;
                loggedCnt++;
            }
        }

        //TEMPORARARY UNTIL I CAN FIGURE OUT WHY THESE DAMN EXCEPTIONS WON'T SERIALIZE.
        ApplicationExceptionList newErrors = new ();
        ApplicationExceptionList.Node nextNode = errors.First;
        while (nextNode != null)
        {
            newErrors.Add(nextNode.Data);
            nextNode = nextNode.Next;
        }

        return newErrors;
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

    private void SetupProgressPoints(bool a_oneTypeOnly, Type a_typeToInclude)
    {
        double totalProgressPoints = 0; // Starting at one step ensures the last step   
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

        if ((!a_oneTypeOnly && m_settings.IncludeUserFields) || (a_oneTypeOnly && a_typeToInclude == typeof(UserFieldDefinitionT)))
        {
            totalProgressPoints += 100;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeCustomers) || (a_oneTypeOnly && a_typeToInclude == typeof(CustomerT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeUsers) || (a_oneTypeOnly && a_typeToInclude == typeof(UserT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludePlants) || (a_oneTypeOnly && a_typeToInclude == typeof(PlantT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeDepartments) || (a_oneTypeOnly && a_typeToInclude == typeof(DepartmentT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeCapabilities) || (a_oneTypeOnly && a_typeToInclude == typeof(CapabilityT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeCells) || (a_oneTypeOnly && a_typeToInclude == typeof(CellT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeResources) || (a_oneTypeOnly && a_typeToInclude == typeof(ResourceT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeResourceConnectors) || (a_oneTypeOnly && a_typeToInclude == typeof(ResourceConnectorsT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeWarehouses) || (a_oneTypeOnly && a_typeToInclude == typeof(WarehouseT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeInventories) || (a_oneTypeOnly && a_typeToInclude == typeof(WarehouseT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeLots) || (a_oneTypeOnly && a_typeToInclude == typeof(WarehouseT)))
        {
            totalProgressPoints += 50;
        }
        
        if ((!a_oneTypeOnly && m_settings.IncludeItems) || (a_oneTypeOnly && a_typeToInclude == typeof(WarehouseT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeCapacityIntervals) || (a_oneTypeOnly && a_typeToInclude == typeof(CapacityIntervalT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeRecurringCapacityIntervals) || (a_oneTypeOnly && a_typeToInclude == typeof(RecurringCapacityIntervalT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeProductRules) || (a_oneTypeOnly && a_typeToInclude == typeof(ProductRulesT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeAttributes) || (a_oneTypeOnly && a_typeToInclude == typeof(PTAttributeT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeAttributeSetupTables) || (a_oneTypeOnly && a_typeToInclude == typeof(LookupAttributeNumberRangeT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeAttributeCodeTables) || (a_oneTypeOnly && a_typeToInclude == typeof(LookupAttributeCodeTableT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeCleanoutTriggerTables) || (a_oneTypeOnly && a_typeToInclude == typeof(CleanoutTriggerTablesT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeCompatibilityCodeTables) || (a_oneTypeOnly && a_typeToInclude == typeof(CompatibilityCodeTableT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludePurchasesToStock) || (a_oneTypeOnly && a_typeToInclude == typeof(PurchaseToStockT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeSalesOrders) || (a_oneTypeOnly && a_typeToInclude == typeof(SalesOrderT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeForecasts) || (a_oneTypeOnly && a_typeToInclude == typeof(ForecastT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeTransferOrders) || (a_oneTypeOnly && a_typeToInclude == typeof(TransferOrderT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeJobs) || (a_oneTypeOnly && a_typeToInclude == typeof(JobT)))
        {
            totalProgressPoints += 50;
        }

        if ((!a_oneTypeOnly && m_settings.IncludeInternalActivity) || (a_oneTypeOnly && a_typeToInclude == typeof(JobT.InternalActivity)))
        {
            totalProgressPoints += 50;
        }

        if (!string.IsNullOrEmpty(m_postImportUrl))
        {
            totalProgressPoints += 300;
        }

        m_statusUpdater.SetInitialProgressPoints(currentProgressPoints, totalProgressPoints, 100);
    }

    private void StartProgress(BaseId a_instigator)
    {
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
        m_importStatusMessage.ProgressPercent = 100;
        m_importStatusMessage.Exceptions = exceptions;
    }

    /// <summary>
    /// Populate JobDataSet from SQL and return.
    /// </summary>
    /// <returns></returns>
    private JobDataSet FetchJobDataSet()
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
            using (IDbConnection connection = OpenNewConnection())
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
                    m_settings.IncludeResourceRequirements,
                    m_settings.IncludeMaterials,
                    m_settings.IncludeProducts,
                    m_settings.IncludeInternalActivity,
                    m_settings.IncludeOpAttributes,
                    m_settings.IncludeSuccessorMOs,
                    m_settings.IncludePaths,
                    m_settings.IncludeCustomers && customerCmdHasSourceInfo,
                    m_settings.IncludeResourceOperations,
                    m_settings.IncludeManufacturingOrders);
            }
        }
        catch (Exception e)
        {
            //Can only throw serializable exceptions from here.
            //Were getting UniqueArrayListException here which caused serialization problem.

            ImportException ie = new (e.Message);
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

    /// <summary>
    /// populates Activity Table of JobDataSet from db and returns it.
    /// </summary>
    /// <returns></returns>
    private JobDataSet.ActivityDataTable FetchActivityDataTable()
    {
        JobDataSet ds = new ();

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
    private ActivityUpdateT GetActivityUpdateT(JobDataSet a_jobDataSet, HashSet<string> a_jobTUniqueKeys)
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

    private IDbConnection OpenNewConnection()
    {
        IDbConnection connection = GetConnection();

        try
        {
            connection.Open();
            if (PTSystem.EnableDiagnostics)
            {
                m_utility.LogMessage("Import Logging\nImporter.OpenNewConnection called.");
            }
            return connection;
        }
        catch (Exception err)
        {
            ImporterConnectionException e = new (connection.ConnectionString, err.Message);
            m_utility.LogImporterException(e);
            throw e;
        }
    }

    private IDataReader GetReader(IDbCommand command, string errText)
    {
        try
        {
            command.CommandTimeout = 0; //so it will wait indefinitely and not timeout for slow queries
            return command.ExecuteReader();
        }
        catch (Exception err)
        {
            string errMsg = string.Format("Error while executing query for {0} from Database {1}. The error was: {2}.  \r\nThe SQL was: {3}", errText, command.Connection.Database, err.Message, command.CommandText);
            ImporterCommandException e = new (command.CommandText, errMsg);

            m_utility.LogImporterException(e);
            throw e;
        }
    }

    private void SendTransmission(PTTransmission t)
    {
        //The instigator must by ERP so that JobT data doesn't overwrite user/barcode status updates
        if (t is not ImportT)
        {
            t.Instigator = BaseId.ERP_ID;
        }

        //Create PT Connection
        try
        {
            t.TransmissionSender = PTTransmissionBase.TransmissionSenderType.PTInterface;
            SystemController.ClientSession.SendClientAction(t);
        }
        catch (Exception err)
        {
            ImporterException e = new ("2354", new object[] { t.GetType().ToString(), err.Message });
            m_utility.LogImporterException(e);
            throw e;
        }
    }
}