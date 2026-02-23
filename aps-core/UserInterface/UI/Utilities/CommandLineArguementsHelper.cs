using PT.APSCommon.ProgramArguments;

namespace PT.UI.Utilities;

/// <summary>
/// Used to parse the command line into arguments specified within this class.
/// Right now this only works in debug mode due to the Click Once security settings; the settings
/// prevent command line arguments from being sent to the program. This projects security settings were altered to allow the arguments to be passed in the debug build.
/// The user interface takes the following command line parameters:
/// ServerURI: address of the server manager.
/// InstanceName: name of the server instance.
/// SoftwareVersion: The version of the software.
/// UserName: The APS user name in the instance.
/// Password: The APS user name's password.
/// SkipLoginScreen: Skips the login screen.
/// CreateInstanceOfServer: Starts a local instance of APS system.
/// Value: Internal will run system and client in once process.
/// SkipClientUpdater: Allows you to skip running the client updater.
/// NoSplash: Skips the splash screen
/// ADLogin: Changes the default login method to use Active Directory.
/// Value "Current" will use the current user's credentials
/// Value "Specify" will set the active directory login type to allow the user to specify credentials. (Used with SkipLoginScreen will login with last used user/password)
/// These are valid only when CreateInstanceOfServer is set to run an external instance:
/// CloseAfterStarting (CAS): Used for the local server
/// ShowTransmissionReceived (STR): Show each transmission receieved on the server window
/// ShowTransmissionProcessed (STP): show each transmission processed on the server window
/// StartPlayback (SP): Start running immediately
/// Topmost (TM): Set server window state to topmost
/// ServerPath: Localserver location when using CreateInstanceOfserver. This can be omitted when running DebugMode.
/// This can be the full path (won't work with spaces in the path name)
/// or the desired version of the server to run represented by a runmode (debug, release, ...)
/// TaskbarId: Sets the APPModelID to a specific value so that it can be grouped with other apps on the task bar that have the same Id.
/// DefaultWorkspace: When the UI is loaded, the specified workspace will be loaded instead of the normal default
/// TimeZoneOffset: Set offset value from UTC time
/// Below are examples of command line parameters in use:
/// \InstanceName:Debug \SoftwareVersion:0000.0.0.0 \UserName:admin \Password: \SkipLoginScreen: \CreateInstanceOfServer:Shared \skipClientUpdater: \NoSplash:
/// \ServerURI:http://localhost:7990 \InstanceName:Debug \SoftwareVersion:0000.00.00.0 \UserName:admin \Password: \SkipLoginScreen: \skipClientUpdater: \NoSplash: \EndQuickly:
/// \CreateInstanceOfServer:External \ServerPath:Debug \ShowTransmissionReceived: \ShowTransmissionProcessed: \StartPlayback: \DefaultWorkspace:Dev1
/// They all follow the same format and may or may not require a value after the command line argument name.
/// </summary>
public class CommandLineArgumentsHelper
{
    private readonly List<Argument> m_argList;

    public CommandLineArgumentsHelper(string[] args)
    {
        InitArguments();
        ArgumentParser p = new ();
        m_argList = new List<Argument>
        {
            SystemServiceUri,
            InstanceName,
            SoftwareVersion,
            InstanceDataConnectionString,
            UserName,
            Password,
            CreateInstanceOfServer,
            SkipClientUpdater,
            NoSplash,
            CloseAfterStarting,
            StartPlayback,
            Topmost,
            TaskbarId,
            DefaultWorkspace,
            TimeZoneOffset,
            CertificateThumbprint,
            JWTLogin,
            ServerManagerURI,
            LoadDevPackages,
            InstanceConfigPath,
            StartUpType,
        };

        #if DEBUG
        m_argList.Add(KeyFile);
        m_argList.Add(EndQuickly);
        m_argList.Add(ShowTransmissionReceived);
        m_argList.Add(ShowTransmissionProcessed);
        m_argList.Add(ServerPath);
        #endif
        Errors = p.ParseAndGatherErrors(args, true, m_argList.ToArray());
    }

    public List<PTArgumentException> Errors { get; }

    private void InitArguments()
    {
        SystemServiceUri = new Argument("SystemServiceUri", EValueAfterNameRequirement.Optional);
        InstanceName = new Argument("InstanceName", EValueAfterNameRequirement.Required);
        SoftwareVersion = new Argument("SoftwareVersion", EValueAfterNameRequirement.Required);
        InstanceDataConnectionString = new Argument("InstanceDataConnectionString", string.Empty, EValueAfterNameRequirement.Optional);
        UserName = new Argument("UserName", EValueAfterNameRequirement.Required);
        Password = new Argument("Password", EValueAfterNameRequirement.Optional);
        SkipLoginScreen = new Argument("SkipLoginScreen", EValueAfterNameRequirement.NoValue);
        CreateInstanceOfServer = new Argument("CreateInstanceOfServer", EValueAfterNameRequirement.Optional, "CIS");
        SkipClientUpdater = new Argument("SkipClientUpdater", EValueAfterNameRequirement.NoValue);
        NoSplash = new Argument("NoSplash", EValueAfterNameRequirement.NoValue);
        EndQuickly = new Argument("EndQuickly", EValueAfterNameRequirement.NoValue);
        //Use SSO
        //UseActiveDirectoryLogin = new Argument("ADLogin", EValueAfterNameRequirement.Required);
        TaskbarId = new Argument("TaskbarId", EValueAfterNameRequirement.Required);
        DefaultWorkspace = new Argument("DefaultWorkspace", EValueAfterNameRequirement.Required);
        TimeZoneOffset = new Argument("TimeZoneOffset", EValueAfterNameRequirement.Optional);
        JWTLogin = new Argument("JWToken", EValueAfterNameRequirement.Required);

        LoadDevPackages = new Argument("LoadDevPackages", EValueAfterNameRequirement.Optional);
        KeyFile = new Argument("KeyFile", EValueAfterNameRequirement.Optional);

        CertificateThumbprint = new Argument("CertificateThumbprint", EValueAfterNameRequirement.Required);

        //Optional Parameters for starting the localserver
        CloseAfterStarting = new Argument("CloseAfterStarting", EValueAfterNameRequirement.NoValue, "CAS");
        ShowTransmissionReceived = new Argument("ShowTransmissionReceived", EValueAfterNameRequirement.NoValue, "STR");
        ShowTransmissionProcessed = new Argument("ShowTransmissionProcessed", EValueAfterNameRequirement.NoValue, "STP");
        StartPlayback = new Argument("StartPlayback", EValueAfterNameRequirement.NoValue, "SP");
        Topmost = new Argument("Topmost", EValueAfterNameRequirement.NoValue, "TM");

        //Localserver location when using CreateInstanceOfserver
        ServerPath = new Argument("ServerPath", EValueAfterNameRequirement.Optional);

        ServerManagerURI = new Argument("ServerManagerURI", EValueAfterNameRequirement.Optional);
        InstanceConfigPath = new Argument("InstanceConfigPath", a_defaultValue: string.Empty, EValueAfterNameRequirement.Optional);
        
        StartUpType = new Argument("StartupType", EValueAfterNameRequirement.Required, "StartUpType");

        c_createInstanceOfServerValueShared = "Internal";
        c_createInstanceOfServerValueProcess = "External";
    }

    internal enum EClientType { Internal, External }

    internal EClientType ClientType
    {
        get
        {
            if (!CreateInstanceOfServer.ArgumentFound)
            {
                return EClientType.External;
            }

            if (CreateInstanceOfServer.Value == c_createInstanceOfServerValueShared)
            {
                return EClientType.Internal;
            }

            if (CreateInstanceOfServer.Value == c_createInstanceOfServerValueProcess)
            {
                return EClientType.External;
            }

            //The default for any other value
            return EClientType.External;
        }
    }

    // These are the command line arguments this program can accept. 
    internal Argument SystemServiceUri { get; private set; }
    internal Argument ServerManagerURI { get; private set; }
    internal Argument InstanceName { get; private set; }
    internal Argument SoftwareVersion { get; private set; }
    internal Argument InstanceDataConnectionString { get; private set; }
    internal Argument UserName { get; private set; }
    internal Argument Password { get; private set; }
    internal Argument SkipLoginScreen { get; private set; }
    internal Argument CreateInstanceOfServer { get; private set; }
    internal Argument SkipClientUpdater { get; private set; }
    internal Argument NoSplash { get; private set; }

    internal Argument EndQuickly { get; private set; }

    //We no longer support AD. Use SSO
    //internal Argument UseActiveDirectoryLogin { get; private set; }
    internal Argument JWTLogin { get; private set; }
    internal Argument TaskbarId { get; private set; }
    internal Argument DefaultWorkspace { get; private set; }
    internal Argument TimeZoneOffset { get; private set; }
    internal Argument CertificateThumbprint { get; private set; }

    //Optional parameters for starting the localserver
    internal Argument CloseAfterStarting { get; private set; }
    internal Argument ShowTransmissionReceived { get; private set; }
    internal Argument ShowTransmissionProcessed { get; private set; }
    internal Argument StartPlayback { get; private set; }
    internal Argument Topmost { get; private set; }
    internal Argument LoadDevPackages { get; private set; }
    internal Argument KeyFile { get; private set; }

    //Localserver location when using CreateInstanceOfserver
    internal Argument ServerPath { get; private set; }

    internal Argument InstanceConfigPath { get; private set; }
    internal Argument StartUpType { get; private set; }

    internal string c_createInstanceOfServerValueShared { get; private set; }
    internal string c_createInstanceOfServerValueProcess { get; private set; }

    internal string CreateArgumentString()
    {
        return ArgumentParser.GetArgumentString(m_argList.ToArray());
    }
}