using System.Net;
using System.Windows.Forms;

using Microsoft.Extensions.Configuration;

using PT.APSCommon;
using PT.APSCommon.ProgramArguments;
using PT.Common.Debugging;
using PT.Common.Encryption;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.Common.Http;
using PT.Common.Windows.File;
using PT.PlanetTogetherAPI;
using PT.PlanetTogetherAPI.Importing;
using PT.PlanetTogetherAPI.Server;
using PT.Scheduler;
using PT.Scheduler.PackageDefs;
using PT.Scheduler.Sessions;
using PT.SchedulerData;
using PT.ServerManagerAPIProxy;
using PT.ServerManagerSharedLib.Data;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.ServerManagerSharedLib.Helpers;
using PT.SystemDefinitions;
using PT.SystemServiceDefinitions;
using PT.SystemServiceDefinitions.Headers;
using PT.SystemServiceProxy.APIClients;
using PT.Transmissions;
using PT.UI.Utilities;
using PT.UIDefinitions;
using PT.UIDefinitions.Interfaces;

using AssemblyVersionChecker = PT.Common.AssemblyVersionChecker;
using InstanceKey = PT.Common.Http.InstanceKey;
using LicenseKeyJson = PT.SchedulerDefinitions.LicenseKeyJson;
using SoftwareVersion = PT.Common.SoftwareVersion;

namespace PT.UI.Dialogs.Login;

internal class LoginProcedure
{
    internal string SystemServiceUri;
    internal string InstanceName;
    internal string InstanceVersion;
    internal string UserName;
    internal string Password;

    private IMessageProvider m_messageProvider;

    private LoginType m_loginType = LoginType.User;

    private readonly bool m_showCommandWindow;
    private APSCommandWindow m_cmdWindow = new (false, null);

    /// <summary>
    /// The Instance that was successfully logged into.  Null if not yet logged in.
    /// </summary>
    internal LoggedInInstanceInfo ConnectedInstanceInfo { get; private set; }

    internal SystemActionsClient ClientSideSystemClient { get; private set; }

    private ClientSession m_clientSession;

    internal LoginProcedure(CommandLineArgumentsHelper a_commandLineArguments)
    {
        InstanceName = a_commandLineArguments.InstanceName.Value;
        InstanceVersion = a_commandLineArguments.SoftwareVersion.Value;
        SystemServiceUri = a_commandLineArguments.SystemServiceUri.ArgumentFound ? a_commandLineArguments.SystemServiceUri.Value : "https://localhost:4001";
        UserName = a_commandLineArguments.UserName.ArgumentFound ? a_commandLineArguments.UserName.Value : string.Empty;
        Password = a_commandLineArguments.Password.ArgumentFound ? a_commandLineArguments.Password.Value ?? string.Empty : string.Empty;
        //Should we show the debug window?
        if ((a_commandLineArguments.CreateInstanceOfServer.ArgumentFound && a_commandLineArguments.CreateInstanceOfServer.Value == a_commandLineArguments.c_createInstanceOfServerValueShared) || a_commandLineArguments.NoSplash.ArgumentFound)
        {
            m_showCommandWindow = true;
        }
    }

    /// <summary>
    /// Creates a proxy to ServerManager and stores the instance
    /// </summary>
    /// <returns>Whether the instance was retrieved</returns>
    private void InitializeProxy()
    {
        //The window will be closed by the login dialog.
        if (m_showCommandWindow)
        {
            m_cmdWindow?.Show();
            m_cmdWindow?.WriteLine();
        }

        m_cmdWindow?.WriteLine("Getting proxy to system...", true);
        ClientSideSystemClient = new SystemActionsClient(InstanceName, InstanceVersion, SystemServiceUri);
    }

    private SplashScreenManagement m_splashManager;

    internal void SetSplashScreen(SplashScreenManagement a_splash)
    {
        m_splashManager = a_splash;
    }

    internal ClientSession LoginClient()
    {
        //validate connection settings
        if (SystemServiceUri.Trim().Length == 0)
        {
            throw new SystemController.LoginException("2424");
        }

        if (InstanceName.Trim().Length == 0)
        {
            throw new SystemController.LoginException("2425");
        }

        try
        {
            //Validate Login Type
            //if (m_loginType is LoginType.ActiveDirectoryCredentials or LoginType.ActiveDirectoryGUID)
            //{
            //    if (!ConnectedInstanceInfo.IsActiveDirectoryAllowed)
            //    {
            //        throw new SystemController.LoginException("2826", new object[] { ConnectedInstanceInfo.InstanceName }, true);
            //    }
            //}


            MultiLevelHourglass.TurnOn(2);
            m_cmdWindow?.WriteLine(string.Format("Obtaining instance: {0}; Version: {1}", InstanceName, InstanceVersion), true);

            //External
            //Login and set user only. The system will be started later after packages are retrieved.
            if (MainForm.CommandLineArguments.ClientType == CommandLineArgumentsHelper.EClientType.External)
            {
                InitializeProxy();
                m_cmdWindow?.WriteLine("Logging in...", true);

                string userName = UserName;
                string password = Password;
                UpdateCurrentUserCredentials(ref userName, ref password);

                byte[] symmetricKey;
                using (EncryptionHandshake handshake = new ())
                {
                    string publicKey = handshake.GetEncryptionKey();
                    byte[] encryptedData = ClientSideSystemClient.Handshake(publicKey);
                    symmetricKey = handshake.Decrypt(encryptedData);
                }

                UserLoginResponse sessionResponse = ClientSideSystemClient.Login(userName, password, m_loginType, symmetricKey);

                //External systems must get the license key from the server.
                string licenseKeyJsonString = ClientSideSystemClient.GetLicenseKeyForInstance();
                LicenseKeyJson licenseKeyJson = LicenseKeyJson.DeserializeToObjectFromString(licenseKeyJsonString);
                PTSystem.LicenseKey.LoadFromJsonObject(licenseKeyJson.SerialCode, licenseKeyJson);

                byte[] connectedInstanceInfo = ClientSideSystemClient.GetLoggedInInstanceData();
                using (BinaryMemoryReader reader = new (symmetricKey, connectedInstanceInfo))
                {
                    ConnectedInstanceInfo = new LoggedInInstanceInfo(reader);
                    //TODO:  After moving MappingWizard to a new process. Remove this set
                    ConnectedInstanceInfo.SystemServiceUrl = SystemServiceUri;
                }

                HashSet<BaseId> loadedScenarioIds = new HashSet<BaseId>();
                foreach (long loadedScenarioIdAsLong in sessionResponse.LoadedScenarioIds)
                {
                    loadedScenarioIds.Add(new BaseId(loadedScenarioIdAsLong));
                }

                m_clientSession = new ClientSession(ClientSideSystemClient, sessionResponse.SessionToken, PTTransmissionBase.TransmissionSenderType.PTUser, ConnectedInstanceInfo.ClientTimeout, loadedScenarioIds);
                m_clientSession.SetUser(new BaseId(sessionResponse.UserId));
                m_clientSession.Encrypt(symmetricKey);

                m_splashManager?.UpdateSplashLocalization();
            }
            //success
        }
        catch (ApiException apiException)
        {
            switch ((HttpStatusCode)apiException.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "Client server api mismatch.".Localize()));
                    break;
                case HttpStatusCode.InternalServerError:
                    LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "Server is unable to process the request.".Localize()));
                    break;
                case HttpStatusCode.BadRequest:
                    LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, apiException.Content));
                    break;
                case HttpStatusCode.Unauthorized:
                    LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "Unauthorized".Localize()));
                    break;
                default:
                    LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri,
                        string.IsNullOrEmpty(apiException.Content) ?
                            $"Status {apiException.StatusCode}." :
                            apiException.Content));
                    break;
            }
        }
        catch (System.Security.Authentication.AuthenticationException)
        {
            LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "The client security method could not be validated.".Localize()));
        }
        catch (System.Net.Sockets.SocketException socketException) //get this if the port is wrong or the service isn't running or the firewall is blocking
        {
            LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "The server could not be reached.".Localize(), socketException));
        }
        catch (TaskCanceledException)
        {
            LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "The session timed out before login could be completed.".Localize()));
        }
        catch (PTException ptE)
        {
            m_errorReporter.LogException(new FailedLogonException("", ptE), null, ptE.LogToSentry);
            throw ptE;
        }
        catch (System.ServiceModel.EndpointNotFoundException endPointE)
        {
            throw new PTValidationException("2803", endPointE);
        }
        catch (Exception e)
        {
            //If exception is unknown and not handled
            LogAndFailWithException(new PTValidationException("2804", e), true);
        }
        finally
        {
            MultiLevelHourglass.TurnOff(2);

            m_cmdWindow?.Close();
            m_cmdWindow = null;
        }

        return m_clientSession;
    }

    private void LogAndFailWithException(Exception a_e, bool a_logToSentry = false)
    {
        m_errorReporter.LogException(a_e, null, a_logToSentry);
        throw a_e;
    }

    internal ClientSession LoginServer(string a_devPackagesPath)
    {
        //validate connection settings
        if (SystemServiceUri.Trim().Length == 0)
        {
            throw new SystemController.LoginException("2424");
        }

        if (InstanceName.Trim().Length == 0)
        {
            throw new SystemController.LoginException("2425");
        }


        //Validate Login Type
        //if (m_loginType is LoginType.ActiveDirectoryCredentials or LoginType.ActiveDirectoryGUID)
        //{
        //    if (!ConnectedInstanceInfo.IsActiveDirectoryAllowed)
        //    {
        //        throw new SystemController.LoginException("2826", new object[] { ConnectedInstanceInfo.InstanceName }, true);
        //    }
        //}

        try
        {
            MultiLevelHourglass.TurnOn(2);

            //Internal
            if (MainForm.CommandLineArguments.ClientType == CommandLineArgumentsHelper.EClientType.Internal)
            {
                //TODO: If running internal, verify server isn't already running.
                //m_splashManager.UpdateSplashDescription("Connecting To Server...".Localize()); //set specifically here, because the system doesn't exist when it is first shown from the login dialog.

                string instanceName = MainForm.CommandLineArguments.InstanceName.Value;
                string softwareVersion = MainForm.CommandLineArguments.SoftwareVersion.Value;

                InstanceKey instanceKey = new InstanceKey(instanceName, softwareVersion);
                SimpleExceptionEventLogger.RegisterEventSource(instanceKey.GetServiceName());

                LoadConfigurationForInternal(instanceKey);

                IInstanceSettingsManager instanceSettingsManager;

                try
                {
                    instanceSettingsManager = InstanceSettingManagerFactory.CreateInstanceSettingsManagerForInstance(m_configuration["InstanceDataConnectionString"], Environment.MachineName, m_configuration["InstanceIdentifier"], m_configuration["ApiKey"], m_configuration["WebAppEnv"]);
                }
                catch (Exception ex)
                {
                    SimpleExceptionEventLogger.LogExceptionToEventLog(new SystemController.LoginException("7013", new object[] {ex.Message}), SimpleExceptionLogger.PTEventId.FAILED_TO_START);
                    throw;
                }

                m_cmdWindow?.WriteLine("Getting configuration data...", true);

                // TODO: Add an endpoint that get all of this info in one call, to reduce multiple calls out to webapp. This would have to be added after all devs are on webapp.
                // InstanceDataFolder must be obtained before other instance data, as their construction relies on the static Paths class.
                Paths.InstanceDataFolder = instanceSettingsManager.GetServerManagerFolder(instanceName, softwareVersion);
                StartupValsAdapter startupVals = new (instanceSettingsManager.GetStartupVals(instanceName, softwareVersion), m_configuration["WebAppEnv"], m_configuration["InstanceDataConnectionString"], m_configuration["ApiKey"]);
                InstanceSettingsEntity instanceSettingsEntity = instanceSettingsManager.GetInstanceSettingsEntity(instanceName, softwareVersion);
                startupVals.DevPackagePath = a_devPackagesPath;

                m_cmdWindow?.WriteLine("Starting the APS System within the client...", true);

                //Bring up the system service controllers
                PTHttpServer systemServer = new (new UriBuilder(SystemServiceUri), instanceSettingsEntity.EnableDiagnostics, instanceSettingsManager.GetCertificateThumbprint(instanceName, softwareVersion), new StaticAuthorizer(), false, m_errorReporter);
                ImportUtilities importUtilities = new (startupVals.ServiceName, instanceSettingsEntity, instanceSettingsManager);

                InitializeProxy(); //Must initialize after the web server starts up.

                IImportingService importingService = startupVals.NewImport
                    ? new NewImportingService(importUtilities)
                    : new ImportingService(importUtilities);

                if (MainForm.CommandLineArguments.StartUpType.ArgumentFound)
                {
                    if (Enum.TryParse(MainForm.CommandLineArguments.StartUpType.Value, out EStartType overrideType))
                    {
                        startupVals.StartType = overrideType;
                    }
                    else
                    {
                        throw new PTArgumentException($"Invalid StartupType for parameter {MainForm.CommandLineArguments.StartUpType.Name}");
                    }
                }

                m_clientSession = SystemController.StartAPSServer(ClientSideSystemClient, startupVals, null, importingService, new PublishHelper(instanceSettingsEntity, startupVals), systemServer);

                m_cmdWindow?.WriteLine("The APS System has started", true);

                //Get connected instance info using encryption key from system generated client session
                byte[] connectedInstanceInfo = ClientSideSystemClient.GetLoggedInInstanceData();
                using (BinaryMemoryReader reader = new (m_clientSession.SymmetricKey, connectedInstanceInfo))
                {
                    ConnectedInstanceInfo = new LoggedInInstanceInfo(reader);
                    //TODO:  After moving MappingWizard to a new process. Remove this set
                    ConnectedInstanceInfo.SystemServiceUrl = SystemServiceUri;
                }
                string tempUserName = UserName;
                string tempPassword = Password;
                UpdateCurrentUserCredentials(ref tempUserName, ref tempPassword);
                SystemController.SetUser(tempUserName, tempPassword, m_loginType);
                Task.Run(SystemController.SetImportingType);
            }
            
            //success
        }
        catch (ApiException apiException)
        {
            switch ((HttpStatusCode)apiException.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "Client server api mismatch.".Localize()));
                    break;
                case HttpStatusCode.InternalServerError:
                    LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "Server is unable to process the request.".Localize()));
                    break;
                case HttpStatusCode.BadRequest:
                    LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "Invalid data.".Localize()));
                    break;
                case HttpStatusCode.Unauthorized:
                    LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, $"Login is not authorized"));
                    break;
                default:
                    LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri,
                        string.IsNullOrEmpty(apiException.Content) ?
                            $"Status {apiException.StatusCode}." :
                            apiException.Content));
                    break;
            }
        }
        catch (System.Security.Authentication.AuthenticationException)
        {
            LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "The client security method could not be validated.".Localize()));
        }
        catch (System.Net.Sockets.SocketException socketException) //get this if the port is wrong or the service isn't running or the firewall is blocking
        {
            LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "The server could not be reached.".Localize(), socketException));
        }
        catch (TaskCanceledException)
        {
            LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "The session timed out before login could be completed."));
        }
        catch (PTException ptE)
        {
            m_errorReporter.LogException(new FailedLogonException("", ptE), null, ptE.LogToSentry);
            throw ptE;
        }
        catch (System.ServiceModel.EndpointNotFoundException endPointE)
        {
            throw new PTValidationException("2803", endPointE);
        }
        catch (Exception e)
        {
            //If exception is unknown and not handled
            LogAndFailWithException(new PTValidationException("2804", e), true);
        }
        finally
        {
            MultiLevelHourglass.TurnOff(2);

            m_cmdWindow?.Close();
            m_cmdWindow = null;
        }

        return m_clientSession;
    }
    /// <summary>
    /// Attempts login and displays any necessary error messages.
    /// </summary>
    private void UpdateCurrentUserCredentials(ref string r_userName, ref string r_password)
    {
        if (MainForm.CommandLineArguments.JWTLogin.ArgumentFound)
        {
            r_userName = MainForm.CommandLineArguments.JWTLogin.Value;
            r_password = string.Empty;
            m_loginType = LoginType.JWT;
        }
    }
    /// <summary>
    /// Loads configuration file, useable on server only (ie, when running internal)
    /// </summary>
    /// <param name="a_instanceKey"></param>
    private void LoadConfigurationForInternal(InstanceKey a_instanceKey)
    {
        // TODO: If we change our instance file structure so that instances no longer have version-specific directories (see ARC-112), this will need to be updated.
        string instanceFullName = $"{a_instanceKey.InstanceName} {a_instanceKey.SoftwareVersion}";
        string configPathFromStartArgs = GetConfigPathFromStartArgs();
        string pathToConfigFile = configPathFromStartArgs ?? StartupInstanceSettingsHelper.GetPathToConfigFile(instanceFullName);

        m_configuration = new ConfigurationBuilder()
                          .SetBasePath(Path.GetDirectoryName(pathToConfigFile))
                          .AddJsonFile(Path.GetFileName(pathToConfigFile), optional: true, reloadOnChange: true)
                          //.AddCommandLine(a_args) // ConfigurationBuilder cares about order of sources - e.g. CommandLine values will overwrite any matching keys in the JsonFile.
                          .Build();
    }

    /// <summary>
    /// Load configuration from a specific source other than the standard one.
    /// In a development environment, the default is to use a configuration in the SolutionMiscFiles directory of the repo;
    /// a reasonable alternative would be to set InstanceConfigPath:C:\ProgramData\PlanetTogether\Debug 0.0\System", (adjusting for your setup) where production config would be.
    /// </summary>
    /// <returns></returns>
    private string GetConfigPathFromStartArgs()
    {
        string instanceConfigPathArg = MainForm.CommandLineArguments.InstanceConfigPath.Value;

        if (!string.IsNullOrEmpty(instanceConfigPathArg) && 
            !instanceConfigPathArg.Contains(".json"))
        {
            DebugException.ThrowInDebug("The InstanceConfigPath arg should include the file name to load (default 'configuration.json').");
        }

        if (string.IsNullOrEmpty(instanceConfigPathArg))
        {
            // This is normal; can return null here and use StartupInstanceSettingsHelper.GetPathToConfigFile() 
            return null;
        }

        return instanceConfigPathArg;
    }

    // External system refers to the PTSystem that a client has for its own local scheduling actions
    internal Exception StartExternalSystem(IMessageProvider a_messageProvider)
    {
        m_messageProvider = a_messageProvider;

        m_splashManager?.UpdateSplashDescription("Retrieving Scenarios...".Localize());

        try
        {
            // In the event that the session times out while calling GetSystem below, we want to stop waiting for a response from the API.
            // This client can't directly know if this has occurred, but we can at least stop if the full timeout has been reached).
            ClientSideSystemClient.SetTimeout(m_clientSession.ConnectionTimeout);
            byte[] systemBytes = ClientSideSystemClient.GetSystem(m_clientSession.SessionToken);
            SoftwareProductVersion serverSoftwareVersion = ClientSideSystemClient.GetServerProductVersion(m_clientSession.SessionToken);
            SoftwareVersion serverProductVersion = new (serverSoftwareVersion.Major, serverSoftwareVersion.Minor, serverSoftwareVersion.Hotfix, serverSoftwareVersion.Revision);

            try
            {
                PTAssemblyVersionChecker.CheckAssemblies(serverProductVersion);

                //Progress bar
                //byte[] systemBytes = null;
                //using (MemoryStream writeStream = new MemoryStream())
                //{
                //    int chunkSize = getSystemResponse.SystemBytesLength / 100;
                //    chunkSize = Math.Max(chunkSize, 10 * 1000000); // minimum of 10 MB
                //    chunkSize = Math.Min(chunkSize, 200 * 1000000); // maximum of 200 MB
                //    byte[] buffer = new byte[chunkSize];

                //    do
                //    {
                //        // read bytes from input stream
                //        int bytesRead = getSystemResponse.SystemBytes.Read(buffer, 0, chunkSize);
                //        if (bytesRead == 0) break;

                //        // write bytes to output stream
                //        writeStream.Write(buffer, 0, bytesRead);

                //        // report progress from time to time
                //        m_splashManager.UpdateSplashDescription(string.Format("Retrieving scenario data {0}%".Localize(), writeStream.Position / getSystemResponse.SystemBytesLength * 100));
                //    } while (true);
                //    systemBytes = writeStream.ToArray();
                //}
                m_cmdWindow?.WriteLine("Getting broadcaster values from Server Manager...", true);
                StartupValsAdapter startupVals = ClientSideSystemClient.GetStartupVals();
                (string serialCode, string token, EnvironmentType environmentType) licenseSessionInfo = (startupVals.SerialCode, startupVals.SecurityToken, startupVals.EnvironmentType);

                SystemController.SetClientSystem(systemBytes, m_clientSession.SymmetricKey, m_clientSession, startupVals, licenseSessionInfo);
                SystemController.SetUser(m_clientSession.UserId);
                m_clientSession.ReceiveEvent += SystemController.Sys.GetReceiveDelegate();
                m_clientSession.ClientRestartRequiredEvent += SystemController.Sys.OnClientRestartEvent;

                SoftwareVersion productVersion = PTAssemblyVersionChecker.GetServerProductVersion();
                if (!AssemblyVersionChecker.ValidateBuildVersion(productVersion, serverProductVersion))
                {
                    string msg = string.Format("The version of the Client application that is running on this machine does not match the version of the APS System Service on the server.  The version on the server is {0}.  The version of this client is {1}.", serverProductVersion, productVersion);
                    return new PTAssemblyVersionChecker.PTAssemblyVersionCheckerException(msg);
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }
        catch (ApiException apiException)
        {
            switch ((HttpStatusCode)apiException.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "Client server api mismatch.".Localize()));
                    break;
                case HttpStatusCode.InternalServerError:
                    LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "Server is unable to process the request.".Localize()));
                    break;
                case HttpStatusCode.BadRequest:
                    LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "Invalid data.".Localize()));
                    break;
                case HttpStatusCode.Unauthorized:
                    // TODO: It would be nice if session expiry could be delayed until startup completes, to avoid this
                    LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "The session expired before startup could complete. If this continues, the Client Timeout setting may need to be increased.".Localize()));
                    break;
                default:
                    LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, 
                        string.IsNullOrEmpty(apiException.Content) ? 
                            $"Status {apiException.StatusCode}." : 
                            apiException.Content));
                    break;
            }
        }
        catch (System.Security.Authentication.AuthenticationException)
        {
            LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "The client security method could not be validated.".Localize()));
        }
        catch (System.Net.Sockets.SocketException socketException) //get this if the port is wrong or the service isn't running or the firewall is blocking
        {
            LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "The server could not be reached.".Localize(), socketException));
        }
        catch (TaskCanceledException)
        {
            LogAndFailWithException(new InvalidLogonBaseException(InstanceName, InstanceVersion, SystemServiceUri, "The session timed out before login could be completed.".Localize()));
        }
        catch (PTException ptE)
        {
            m_errorReporter.LogException(new FailedLogonException("", ptE), null, ptE.LogToSentry);
            return ptE;
        }
        catch (System.ServiceModel.EndpointNotFoundException endPointE)
        {
            return new PTValidationException("2803", endPointE);
        }        
        catch (Exception e)
        {
            //If exception is unknown and not handled
            LogAndFailWithException(new PTValidationException("2804", e), true);
        }

        return null;
    }

    private string m_retrievingScenarioTextLocalized = string.Empty;
    private int m_lastRetrieveScenarioProgress = -1;

    private void SystemServiceProxy_LoginProgressChanged(int a_percentComplete)
    {
        int done = a_percentComplete;
        if (m_lastRetrieveScenarioProgress >= done)
        {
            //Reduce ui thread invokes
            return;
        }

        m_lastRetrieveScenarioProgress = done;

        if (string.IsNullOrEmpty(m_retrievingScenarioTextLocalized))
        {
            m_retrievingScenarioTextLocalized = "Retrieving Scenarios... {0} %".Localize();
        }

        if (done >= 100)
        {
            m_splashManager.UpdateSplashDescription("Initializing Scenario...".Localize());
        }
        else
        {
            m_splashManager.UpdateSplashDescription(string.Format(m_retrievingScenarioTextLocalized, done));
        }
    }

    //TODO: Maybe use this?
    private void loginControl_BrowseClientDataClicked(object sender, EventArgs e)
    {
        try
        {
            using (new MultiLevelHourglass())
            {
                System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", ClientWorkingFolder.ClientDataFolder);
            }
        }
        catch (Exception err)
        {
            MessageBox.Show(err.Message);
        }
    }

    /// <summary>
    /// Delete packages on client that are not on server
    /// </summary>
    private static void RemoveMissingPackages(IEnumerable<AssemblyPackageInfo> a_packageVersionsOnClient, IEnumerable<AssemblyPackageInfo> a_packagesOnServer)
    {
        IEnumerable<AssemblyPackageInfo> packagesToRemove = a_packageVersionsOnClient.Where(clientPackage => a_packagesOnServer.Count(serverPackage => serverPackage.AssemblyFileName == clientPackage.AssemblyFileName && serverPackage.Version == clientPackage.Version) == 0);

        foreach (AssemblyPackageInfo packageToRemove in packagesToRemove)
        {
            try
            {
                Common.Directory.DirectoryUtils.Delete(Path.GetDirectoryName(packageToRemove.PathOnDisk));
            }
            catch
            {
                //TODO: Add logging
            }
        }
    }

    /// <summary>
    /// Syncs client packages with server.
    /// </summary>
    internal void SynchronizePackagesWithServer(IPackageAssemblyLoader a_assemblyLoader, WorkingDirectory a_workingDirectory)
    {
        IEnumerable<AssemblyPackageInfo> clientPackageInfos = a_assemblyLoader.GetLatestPackageInfos();

        IEnumerable<AssemblyPackageInfo> packagesVersionsOnServer = ClientSideSystemClient.GetPackagesOnServer();

        //Delete packages no longer on the server from the client's disk
        RemoveMissingPackages(clientPackageInfos, packagesVersionsOnServer);

        IEnumerable<AssemblyPackageInfo> packagesToGetFromServer = packagesVersionsOnServer.Where(serverPackage => clientPackageInfos.Count(clientPackage => clientPackage.AssemblyFileName == serverPackage.AssemblyFileName && clientPackage.Version == serverPackage.Version) == 0);

        foreach (AssemblyPackageInfo packageVersionToGet in packagesToGetFromServer)
        {
            PackedAssembly packedAssembly = ClientSideSystemClient.GetPackedPackageAssembly(packageVersionToGet);


            string clientPackagePath = Path.Combine(a_assemblyLoader.PackagesOnDiskPath, packedAssembly.PackageInfo.AssemblyTitleName, packedAssembly.PackageInfo.Version, packedAssembly.PackageInfo.AssemblyFileName);

            if (File.Exists(clientPackagePath))
            {
                //Remove old binary
                File.Delete(clientPackagePath);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(clientPackagePath));
            }

            FileUtils.SaveBinaryFile(clientPackagePath, packedAssembly.PackageBytes);
        }
    }

    /// <summary>
    /// Validates package assembly files are present in package path. Cleans (deletes) package reference if empty.
    /// </summary>
    /// <param name="a_packagePath"></param>
    private static bool ValidateDlls(string a_packagePath)
    {
        //confirm dlls present
        if (Directory.GetFiles(a_packagePath, "PT.*Package*.dll", SearchOption.AllDirectories).Length > 0)
        {
            return true;
        }

        Common.Directory.DirectoryUtils.Delete(a_packagePath);
        return false;
    }

    private ICommonLogger m_errorReporter;
    private IConfigurationRoot m_configuration;

    public void Init(ICommonLogger a_commonLogger)
    {
        m_errorReporter = a_commonLogger;
    }
}