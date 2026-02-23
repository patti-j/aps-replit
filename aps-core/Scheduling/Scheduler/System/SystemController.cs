using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;

using Microsoft.IdentityModel.Tokens;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Compression;
using PT.Common.Encryption;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.Common.Http;
using PT.Common.Localization;
using PT.Scheduler.PackageDefs;
using PT.Scheduler.Sessions;
using PT.ServerManagerAPIProxy.APIClients;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.SystemDefinitions.Interfaces;
using PT.SystemServiceDefinitions;
using PT.SystemServiceProxy.APIClients;
using PT.Transmissions;
using PT.Transmissions.Interfaces;

namespace PT.Scheduler;

public class SystemController
{
    #region variables
    //TODO: Replace with interface and pass references
    public static ServerSessionManager ServerSessionManager;
    private static WebAppActionsClient s_webAppActionsClient;

    public static WebAppActionsClient WebAppActionsClient => s_webAppActionsClient;

    private static PTSystem s_liveSystem;
    public static PTSystem Sys => s_liveSystem;

    //TODO: Replace with interface and pass references
    private static ClientSession m_clientSession;

    [Obsolete("Use a reference to the client session instead of this static helper")]
    public static IClientSession ClientSession => m_clientSession;

    public static BaseId CurrentUserId { get; private set; }

    private static long s_creationTicks;
    public static long CreationTicks => s_creationTicks;

    private static long s_broadcasterUtcDateTimeInstanceId;

    public static long BroadcasterUtcDateTimeInstanceId => s_broadcasterUtcDateTimeInstanceId;

    internal static IPublishHelper PublishHelper { get; set; }

    public static IImportingService ImportingService { get; set; }
    
    public static EImportingType ImportingType { get; private set; }

    //Used to keep an object in reference if a server is passed in on server creation(i.e. system service server and controllers)
    private static IPTWebService m_ptServer;

    static SystemController()
    {
        ResetupStatics();
    }

    private static void ResetupStatics()
    {
        ServerSessionManager = null;
        s_liveSystem = null;
        m_clientSession = null;
        CurrentUserId = BaseId.ServerId; //To be overridden by clients on login setuser
        s_broadcasterUtcDateTimeInstanceId = 0;
    }

    #region LoginException
    public class LoginException : PTHandleableException, System.Runtime.Serialization.ISerializable
    {
        public LoginException(string message, object[] a_stringParams = null, bool a_appendHelpUrl = false)
            : base(message, a_stringParams, a_appendHelpUrl) { }

        public LoginException(string a_message, Exception a_innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = false) :
            base(a_message, a_innerException, a_stringParameters, a_appendHelpUrl) { }
    }
    #endregion

    public static DateTime StartDate { get; private set; }
    #endregion

    #region CONSTANTS
    private const string c_logInErrLogName = "LogInDialogErrors.log";
    private const string c_loginErrMsgTitle = "Login Error";
    #endregion

    /// <summary>
    /// Method to start up the System Service.
    /// This is called from all programs that run the system(UI(Internal), Test Server, System Service)
    /// </summary>
    public static ClientSession StartAPSServer(SystemActionsClient a_systemActionsClient, StartupValsAdapter a_constructorValues, string a_defaultErrorLogFileName, IImportingService a_importingService, IPublishHelper a_publishHelper, IPTWebService a_ptHttpServer, bool a_isConfigMode = false)
    {
        const string StartupInitErr = "An error occurred while calling StartupInit.";

        ISystemLogger errorReporter;
        try
        {
            ImportingService = a_importingService;
            PublishHelper = a_publishHelper;
            m_ptServer = a_ptHttpServer;

            //Set this early so that working directories can change the package directory appropriately
            PTSystem.Server = true;
            // Create the WebAppActionsClient immediately so we can log errors to WebApp if necessary
            s_webAppActionsClient = WebAppActionsClient.TryInitWebAppClient(a_constructorValues.WebAppEnv, a_constructorValues.InstanceId, a_constructorValues.ApiKey);
            RuntimeStatus.InitializeRuntime();
            WorkingDirectoryPaths directoryStructure = new (a_constructorValues);
            directoryStructure.OverrideStandardPaths(a_constructorValues);
            errorReporter = PTSystem.StartupInit(directoryStructure, a_constructorValues);
            PTSystem.CleanUnneededFiles(a_constructorValues);
            PublishHelper.InitializeErrorReporting(errorReporter);
        }
        catch (Exception e)
        {
            APSStartServiceLogging.APSLogError(StartupInitErr, e, a_defaultErrorLogFileName);

            throw e;
        }

        const string createBroadcasterErr = "An error occurred while trying to create the broadcaster.";
        StartDate = DateTime.UtcNow;

        // Config mode requires no setup from here - the intent is to run in this mode just long enough to finish setup, then restart in a normal mode.
        if (a_isConfigMode)
        {
            return m_clientSession;
        }

        try
        {
            byte[] symmetricKey = DataEncryption.GenerateEncryptionKey();
            //TODO: Disconnect System from broadcaster
            ServerSessionManager = new ServerSessionManager(StartDate.Ticks, a_constructorValues, errorReporter, ref s_liveSystem);

            StartWebServer();
            
            ServerSessionManager.Encrypt(symmetricKey);
            ServerSessionManager.SetCompression(ECompressionType.None); //The server does not need to compress data it sends to itself

            HashSet<BaseId> allScenarioIds;
            //TODO lite client: All scenarios are considered opened on the server right now. We want to change this at some point, so this HashSet should change accordingly
            using (s_liveSystem.ScenariosLock.EnterRead(out ScenarioManager scenarioManager))
            {
                allScenarioIds = new HashSet<BaseId>(scenarioManager.Scenarios.Select(a_scenario => a_scenario.Id));
            }
            m_clientSession = new ClientSession(a_systemActionsClient, ServerSessionManager.SessionToken, PTTransmissionBase.TransmissionSenderType.PTSystem, TimeSpan.FromSeconds(a_constructorValues.ClientTimeoutSeconds), allScenarioIds);
            m_clientSession.Encrypt(symmetricKey);
            m_clientSession.ReceiveEvent += ServerSessionManager.LiveSystem.GetReceiveDelegate();
            m_clientSession.ClientRestartRequiredEvent += ServerSessionManager.LiveSystem.OnClientRestartEvent;
            a_systemActionsClient.Authenticate(m_clientSession.SessionToken); //Since we didn't log in directly, set the authentication token
            s_liveSystem.SetClientSession(m_clientSession);

            s_broadcasterUtcDateTimeInstanceId = ServerSessionManager.BroadcasterUtcDateTimeInstanceId.Ticks;
            s_creationTicks = ServerSessionManager.CreationDate.Ticks;

            //Check to see if the system uses a DataModelActivation key.  This is used for offline activation.
            //If so, a timer is spawned and will validate the scenario data on a regular interval.
            //If the data differs enough the system is sent into readonly mode.  The user is able to undo back to valid state.
            s_liveSystem.InitiateDataModelActivationTimer();
        }
        catch (Exception e)
        {
            APSStartServiceLogging.APSLogError(createBroadcasterErr, e, a_defaultErrorLogFileName);

            throw e;
        }
        
        APSStartServiceLogging.APSStartServiceMessage(string.Format("Planet Together Service Started. *STARTED*. Start Date [{1}]: {0}.", StartDate, StartDate.Ticks), a_defaultErrorLogFileName);

        // Config mode requires no setup from here - the intent is to run in this mode just long enough to finish setup, then restart in a normal mode.
        if (a_isConfigMode)
        {
            return m_clientSession;
        }

        if (ServerSessionManager.StartType is EStartType.Recording or EStartType.UnitTest or EStartType.UnitTestBase or EStartType.Normal)
        {
            ServerSessionManager.PlayBack();
        }
        else if (ServerSessionManager.StartType == EStartType.Prune)
        {
            ServerSessionManager.Prune();
        }

        return m_clientSession;
    }

    /// <summary>
    /// Perform required initialization and start the http server.
    /// </summary>
    private static void StartWebServer()
    {
        using (ServerSessionManager.LiveSystem.PackageManagerLock.EnterRead(out IPackageManager pm))
        {
            m_ptServer.LoadPackageManager(pm);
        }

        m_ptServer.Start();
    }

    /// <summary>
    /// Method to stop APS Service.
    /// </summary>
    public static void StopAPSService(string a_reason)
    {
        DateTime stoppingDate = DateTime.Now;
        APSStartServiceLogging.APSStartServiceMessage(string.Format("The Planet Together Service is stopping. *STOPPING*. Started Date [{2}]: {1}. Stopping date: {0}.", stoppingDate, StartDate, StartDate.Ticks), null);
        string broadcasterDisposeErr = "An error occurred while trying to dispose of the broadcaster.";
        try
        {
            ServerSessionManager.Dispose();
            m_ptServer.Stop();
        }
        catch (Exception e)
        {
            APSStartServiceLogging.APSLogError(broadcasterDisposeErr, e, null);
            throw new PTException("4449", new object[] { "-70" }, false);
        }

        DateTime stoppedDate = DateTime.Now;
        APSStartServiceLogging.APSStartServiceMessage(string.Format("Planet Together Service Stopped. *STOPPED*. Started Date [{3}]: {0}; Stopping(start of shutdown): {1}; Stopped: {2}", StartDate, stoppingDate, DateTime.Now, StartDate.Ticks), null);
    }

    /// <summary>
    /// This is only used when the system is run externally from the client. No validation is done on the user ID.
    /// The client login process sets the current user based on the user ID returned when logging into the server.
    /// </summary>
    /// <param name="a_loggedInUserId"></param>
    /// <param name="o_language"></param>
    public static void SetUser(BaseId a_loggedInUserId)
    {
        using (Sys.UsersLock.EnterRead(out UserManager um))
        {
            User u = um.GetById(a_loggedInUserId);

            CurrentUserId = u.Id;

            //Update TimeZoneAdjuster w/ the user's TimeZoneInfo
            //Common.TimeZoneAdjuster.SetTimeZoneInfo(u.TimeZoneInformation); //TODO: This line is commented to prevent any possible side effects of setting the TimeZoneAdjuster to use the user's TimeZone. Remove when necessary. 

            Localizer.SetLocalizedLanguage(u.DisplayLanguage);
        }
    }

    /// <summary>
    /// This is used only when running the server internally. The server does not return the logged in user, so the user is set globally here.
    /// If the backdoor password is used, the specified user name will be ignored and the first administrator user will be returned.
    /// This was made public for test purposes for development and is intended to provide a way set the user when the APSServer is run within the client.
    /// Some reasons a developer might want to run the APS Service within the client include reducing memory and reducing the time and effort it takes to start the system.
    /// </summary>
    public static void SetUser(string a_userName, string a_password, LoginType a_loginType)
    {
        using (Sys.UsersLock.EnterRead(out UserManager um))
        {
            User u;
            switch (a_loginType)
            {
                case LoginType.User:
                    byte[] symmetricKey = m_clientSession.SymmetricKey;
                    byte[] passwordHash = StringHasher.Hash(a_password, symmetricKey);
                    if (APSCommon.Debug.PasswordOverrides.IsOverride(passwordHash, symmetricKey))
                    {
                        u = um.GetAdministrator();
                    }
                    else
                    {
                        u = um.GetUserByName(a_userName);
                    }

                    break;
                case LoginType.JWT:
                    //TODO: Load from the local user file store instead of trying to pull user data from the web app.

                    if (APSCommon.Debug.PasswordOverrides.IsOverride(a_userName))
                    {
                        u = um.GetAdministrator();
                    }
                    else
                    {
                        string userName = GetUserFromJwtPrincipalWithoutValidation(a_userName);
                        u = um.GetUserByName(userName);
                        string ssoValidationCertificateThumbprint = ServerSessionManager.ConstructorValues.SsoValidationCertificateThumbprint;
                        string nonstandardSsoDomain = ServerSessionManager.ConstructorValues.SsoDomain;
                        string nonstandardSsoClientId = ServerSessionManager.ConstructorValues.SsoClientId;
                        ValidateLoginUserFromJwtPrincipal(a_userName, ssoValidationCertificateThumbprint, nonstandardSsoDomain, nonstandardSsoClientId);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(a_loginType), a_loginType, null);
            }

            if (u == null)
            {
                SimpleExceptionLogger.LogException(c_logInErrLogName, new LoginException("2466"), c_loginErrMsgTitle);
                throw new LoginException("4447");
            }

            CurrentUserId = u.Id;
            //This is for running internally. UserDateTimeNow will throw an exception if PTDateTime.s_timeZoneInfo is null.
            u.LastLoginDate = PTDateTime.UtcNow.ToDateTime();

            //Update TimeZoneAdjuster w/ the user's TimeZoneInfo
            //Common.TimeZoneAdjuster.SetTimeZoneInfo(u.TimeZoneInformation); //TODO: This line is commented to prevent any possible side effects of setting the TimeZoneAdjuster to use the user's TimeZone. Remove when necessary. 

            Localizer.SetLocalizedLanguage(u.DisplayLanguage);
        }
    }

    internal static string GetUserFromJwtPrincipalWithoutValidation(string a_token)
    {
        JwtSecurityTokenHandler tokenHandler = new ();
        JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(a_token);
        if (jwtToken == null)
        {
            return string.Empty;
        }

        string userName = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        return userName;
    }

    internal static void ValidateLoginUserFromJwtPrincipal(string a_token, string a_validationCertificateThumbprint, string a_ssoDomain, string a_ssoClientId)
    {
        JwtSecurityTokenHandler tokenHandler = new ();
        JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(a_token);
        if (jwtToken == null)
        {
            throw new InvalidLogonException("SSO Login failed, the token is invalid");
        }
        
        JsonWebKeySet signingKeys = GetSigningKeysAsync(a_ssoDomain);

        TokenValidationParameters parameters = new ()
        {
            ValidIssuer = a_ssoDomain,
            ValidAudience = a_ssoClientId,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            IssuerSigningKey = signingKeys.Keys[0],
            ClockSkew = TimeSpan.Zero
        };

        // Allow tokens originating from the WebApp also, if instance is configured to work with it
        if (!string.IsNullOrEmpty(ServerSessionManager.ConstructorValues.WebAppClientId))
        {
            parameters.ValidAudiences = new[] { parameters.ValidIssuer, ServerSessionManager.ConstructorValues.WebAppClientId };
        }

        //Run validation, failure will result in an exception
        tokenHandler.ValidateToken(a_token, parameters, out SecurityToken securityToken);
    }

    static JsonWebKeySet GetSigningKeysAsync(string domain)
    {
        var jwksUrl = $"{domain}.well-known/jwks.json";

        using var http = new HttpClient();
        var json = http.GetStringAsync(jwksUrl).Result;

        JsonWebKeySet keys = new JsonWebKeySet(json);
        return keys;
    }

    /// <summary>
    /// Indicates whether a m_clientSession has been established with the System.
    /// </summary>
    public static bool Receiving => m_clientSession != null;

    public static string GetServerConnectionDescription()
    {
        System.Text.StringBuilder sb = new ();
        sb.AppendFormat("Service Start Time: {0}".Localize(), new DateTime(BroadcasterUtcDateTimeInstanceId).ToLocalTime()).AppendLine();
        sb.AppendFormat("Client Connection Start: {0}".Localize(), m_clientSession.CreationDate.ToLocalTime()).AppendLine();
        sb.AppendLine();
        sb.Append(m_clientSession.ToString().Localize());
        return sb.ToString();
    }

    /// <summary>
    /// Sub-Class for Error Logging.
    /// </summary>
    public class APSStartServiceLogging
    {
        /// <summary>
        /// Use this function to log non errors that you like to keep track of, such as when the service starts and stops.
        /// This log is stored in the folder where the service is run from.
        /// </summary>
        /// <param name="message"></param>
        public static void APSStartServiceMessage(string message, string defaultPath)
        {
            //Working directory may not be setup yet
            if (PTSystem.WorkingDirectory == null)
            {
                //If server log to default path
                if (!string.IsNullOrEmpty(defaultPath))
                {
                    Exception exception = new (message);
                    SimpleExceptionLogger.LogExceptionToStartupFolder(defaultPath, exception);
                }

                return;
            }

            SimpleExceptionLogger.LogMessage(PTSystem.WorkingDirectory.APSServiceLog, message);
        }

        /// <summary>
        /// Write errors with an exception with this fucntion.
        /// </summary>
        /// <param name="description">Some text to associate with the exception if you have any to add.</param>
        /// <param name="e">The details of the description will be pulled and added to the log.</param>
        public static void APSLogError(string description, Exception e, string defaultPath)
        {
            //Working directory may not be setup yet
            if (PTSystem.WorkingDirectory == null)
            {
                //If server log to default path
                if (!string.IsNullOrEmpty(defaultPath))
                {
                    Exception exception = new (description, e);
                    SimpleExceptionLogger.LogExceptionToStartupFolder(defaultPath, exception);
                }

                return;
            }

            SimpleExceptionLogger.LogException(PTSystem.WorkingDirectory.APSServiceErrorLog, e, description);
        }
    }

    public static void SetClientSystem(byte[] a_systemBytes, byte[] a_symmetricKey, ClientSession a_clientSession, StartupVals a_startupVals, (string serialCode, string token, EnvironmentType environmentType) a_licenseSessionInfo)
    {
        m_clientSession = a_clientSession;
        s_liveSystem = PTSystem.CreateSystem(a_systemBytes, a_symmetricKey, a_startupVals.ScenarioLimit, a_startupVals.EnableChecksumDiagnostics, a_licenseSessionInfo);
        Task.Run(SetImportingType);
    }

    public static void SetImportingType()
    {
        BoolResponse newImport = m_clientSession.MakeGetRequest<BoolResponse>("GetIsUsingNewImport", "api/Import");
        if (newImport.Content)
        {
            ImportingType = EImportingType.IntegrationV2;
        }
        else
        {
            ImportingType = EImportingType.IntegrationV1;
        }
    }

    internal static async Task StopReceiving()
    {
        if (m_clientSession != null)
        {
            await m_clientSession.StopReceiving();
        }
    }

    public static void StartReceiving()
    {
        m_clientSession.StartReceiving();
    }
}