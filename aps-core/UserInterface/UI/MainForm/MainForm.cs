#region Usings
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.XtraBars.ToolbarForm;
using DevExpress.XtraReports.UI;
using PT.APSCommon;
using PT.APSCommon.Windows;
using PT.APSCommon.Windows.Extensions;
using PT.Common.Debugging;
using PT.Common.Extensions;
using PT.Common.Http;
using PT.ComponentLibrary;
using PT.ComponentLibrary.Forms;
using PT.GanttDotNet.Labels;
using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.Interfaces;
using PT.PackageDefinitions.Settings;
using PT.PackageDefinitionsUI;
using PT.PackageDefinitionsUI.Controls;
using PT.PackageDefinitionsUI.Controls.BaseControls;
using PT.PackageDefinitionsUI.Controls.Grids;
using PT.PackageDefinitionsUI.Controls.Legacy.Reporting;
using PT.PackageDefinitionsUI.Controls.MessageControl;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.PackageDefinitionsUI.Packages.UserSettings;
using PT.PackageDefinitionsUI.Packages.UserWorkspaces;
using PT.ScenarioControls.PackageHelpers;
using PT.Scheduler;
using PT.Scheduler.PackageDefs;
using PT.Scheduler.Sessions;
using PT.SchedulerData;
using PT.SchedulerDefinitions;
using PT.Transmissions;
using PT.Transmissions.Interfaces;
using PT.Transmissions.User;
using PT.UI.Dialogs.CommandWindow;
using PT.UI.Dialogs.Login;
using PT.UI.Managers;
using PT.UI.Packages;
using PT.UI.Utilities;
using PT.UIDefinitions;
using PT.UIDefinitions.Interfaces;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using PT.Common.Localization;
using PT.SystemDefinitions;
#endregion

namespace PT.UI;

public partial class MainForm : ToolbarForm, IErrorLogViewManager, IMainForm, ILocalizeController
{
    #region Declarations
    private IMessageProvider m_messageProvider;
    private bool m_allowDisplay;

    //internal ConnectionChecker ConnectionChecker;

    private readonly PackageManager m_packageManager;

    //TODO: remove this once HandleException can be made non-static
    private static IBrand m_brand;
    internal SplashScreenManagement SplashManager;
    private IUsersInfo m_usersInfo;

    private List<IMainInterfaceModule> m_mainInterfaceModules;
    private readonly ClientSession m_clientSession;

    public IClientSession ClientSession => m_clientSession;
    private Localizer m_localizer;
    private readonly ConnectionStateManager m_connectionStateManager;
    public IConnectionStateManager ConnectionStateManager => m_connectionStateManager;
    private List<INavigationListenerElement> m_navigationListeners;

    private PTAutoSaveSettingManager m_autoSaveSettingManager;
    private readonly ICommonLogger m_errorLogger;
    #endregion

    public MainForm()
    {
        InitializeComponent();
        s_mainForm = this;
        CurrentDpiScaler = (decimal)DeviceDpi / 96;
        MultiLevelHourglass.UseWaitCursor += MultiLevelHourglassOnUseWaitCursor;
        MultiLevelHourglass.TurnOn(1);

        if (CommandLineArguments.CertificateThumbprint.ArgumentFound)
        {
            PTHttpClient.RegisterOverrideThumbprint(CommandLineArguments.CertificateThumbprint.Value);
        }

        m_localizer = new Localizer(this);

        //Application.AddMessageFilter(this);
        //FormClosed += (s, e) => Application.RemoveMessageFilter(this);
        DpiChanged += OnDpiChanged;

        LoginProcedure loginProcedure = new (CommandLineArguments);

        //Initialize data from command line args
        bool loadDevPackages = CommandLineArguments.LoadDevPackages.ArgumentFound;
        string devPackagesPath = loadDevPackages ? CommandLineArguments.LoadDevPackages.Value : string.Empty;
        #if DEBUG
        string keyFile = CommandLineArguments.KeyFile.ArgumentFound && !string.IsNullOrEmpty(CommandLineArguments.KeyFile.Value) ? CommandLineArguments.KeyFile.Value : string.Empty;
        #endif
        //First set the working directory so any errors can be logged.

        try
        {
            ClientWorkingFolderFlag = new ClientWorkingFolder(loginProcedure.InstanceName);
            #if DEBUG
            WorkingDirectoryPaths folderStructure = new (ClientWorkingFolderFlag.WorkingSessionFolderPath, devPackagesPath, keyFile);
            #else
                WorkingDirectoryPaths folderStructure = new WorkingDirectoryPaths(ClientWorkingFolderFlag.WorkingSessionFolderPath, devPackagesPath);

            #endif
            m_workingDirectory = new WorkingDirectory(folderStructure);
            //TODO: This is bad, and should not be necessary, but some package stuff uses static PT.WorkingDirectory references.
            s_uiErrorLogger = PTSystem.StartupInit(folderStructure);

            loginProcedure.Init(s_uiErrorLogger);
        }
        catch (Exception e)
        {
            //Can't log yet so show error immediately.
            string msg = Localizer.GetErrorString("2437", new object[] { e.Message, e.StackTrace }, true);
            PTMessage message = new ();
            message.LoadFromException(e);
            message.Message = msg; //override the default message with the full message
            message.Title = "Parameter Error";
            using (PTMessageForm messageForm = new (true))
            {
                messageForm.Show(message);
            }

            ExitImmediately();
        }

        //Create handle so tasks can invoke
        if (!IsHandleCreated)
        {
            CreateHandle();
        }

        if (CommandLineArguments.ClientType == CommandLineArgumentsHelper.EClientType.External)
        {
            try
            {
                //External login
                m_clientSession = LoginClient(loginProcedure);
                PTStyledForm.SetClientSession(m_clientSession);
                PTStyledForm.SetInstanceInfo(InstanceInfo.InstanceName, InstanceInfo.InstanceVersion);
            }
            catch (Exception e)
            {
                PTMessage message = new (e.GetExceptionFullMessage(), "Unable to log in".Localize());
                using (PTMessageForm messageForm = new (true, this))
                {
                    messageForm.Show(message);
                }

                Environment.Exit(0);
            }
        }

        if (CommandLineArguments.ClientType == CommandLineArgumentsHelper.EClientType.Internal)
        {
            try
            {
                m_clientSession = LoginServer(loginProcedure, devPackagesPath);
                PTStyledForm.SetClientSession(m_clientSession);
                PTStyledForm.SetInstanceInfo(InstanceInfo.InstanceName,InstanceInfo.InstanceVersion);
            }
            catch (Exception e)
            {
                PTMessage message = new (e.GetExceptionFullMessage(), "Unable to log in".Localize());
                using (PTMessageForm messageForm = new (true, this))
                {
                    messageForm.Show(message);
                }

                Environment.Exit(0);
            }
        }

        m_connectionStateManager = new ConnectionStateManager(m_clientSession.ConnectionTimeout);

        //connectionStateManager
        m_clientSession.AttachStateManager(m_connectionStateManager);
        m_connectionStateManager.ConnectionDropped += connectionChecker_DeadConnectionEvent;

        //Reinitialize the error logger to the one created by the system startup.
        m_errorLogger = new UILogger(m_clientSession, m_clientSession.UserId);
        s_uiErrorLogger = m_errorLogger;

        //Sync package files
        PackageAssemblyLoader packageLoader = new (m_workingDirectory.PackagesPath, Path.Combine(m_workingDirectory.LogFolder, ServerSessionManager.PackageErrorFile));
        try
        {
            loginProcedure.SynchronizePackagesWithServer(packageLoader, m_workingDirectory);
            packageLoader.ValidateAndCreatePackageList();
        }
        catch (Exception err)
        {
            PTMessage message = new ("Error while synchronizing packages with server.".Localize(),
                "Unable to log in".Localize());
            using (PTMessageForm messageForm = new (true))
            {
                messageForm.Show(message);
            }

            throw new PTValidationException(string.Format($"Error while synchronizing packages with server: {err.Message}"), err);
        }

        //Load packages
        m_packageManager = new PackageManager(this);
        m_packageManager.LoadPackageAssemblies(m_workingDirectory.PackagesPath, PTSystem.LicenseKey.PackageLicenseOptions, packageLoader.GetValidatedAndLicensedPackageInfos().ToList(), loadDevPackages);
        if (!m_packageManager.GetLoadedPackageInfos().Any())
        {
            HashSet<int> licensedPackages = m_packageManager.GetPackagesLicensingInfo();
            string errorMessage = licensedPackages.Count == 0
                ? "System license is not valid. Package information is not available.".Localize()
                : string.Format("All required packages to run client were not loaded. Please check error logs at {0}".Localize(), m_workingDirectory.LogFolder);

            PTMessage message = new (errorMessage, "Unable to log in".Localize());
            using (PTMessageForm messageForm = new (true))
            {
                messageForm.Show(message);
            }

            throw new PTValidationException(errorMessage);
        }

        try
        {
            //Init any overrides
            m_packageManager.InitializePackages();

            //Load primary modules
            m_packageManager.LoadPrimaryElements();
        }
        catch (Exception err)
        {
            PTMessage message = new (err.Message, "Unable to log in".Localize());
            using (PTMessageForm messageForm = new (true))
            {
                messageForm.Show(message);
            }

            Environment.Exit(0);
        }

        InitializeBrand();

        PTStyledForm.SetBrand(m_brand);
        m_messageProvider = new MessageProviderUtility(this, MainFormInstance, this);
        MessageProvider = m_messageProvider;
        PTBaseControl.SetMessageProvider(m_messageProvider);
        BaseResizableForm.SetMessageProvider(MessageProvider);

        PTBaseControl.SetSkin(m_brand.ActiveTheme);

        //Show splash screen immediately if there were no errors creating the system.
        SplashManager = new SplashScreenManagement(this, this, m_brand, InstanceInfo.InstanceName);
        if (!CommandLineArguments.NoSplash.ArgumentFound)
        {
            SplashManager.ShowExternalSplash();
        }

        loginProcedure.SetSplashScreen(SplashManager);

        //External system startup
        if (CommandLineArguments.ClientType == CommandLineArgumentsHelper.EClientType.External)
        {
            Exception getDataResult = StartExternalSystem(loginProcedure, SplashManager);
            if (getDataResult != null)
            {
                PTMessage message = new (getDataResult.GetExceptionFullMessage(), "Unable to retrieve system data".Localize());
                using (PTMessageForm messageForm = new (true, this))
                {
                    messageForm.Show(message);
                }

                Environment.Exit(0);
            }
        }
        ////Send any logged errors that occurred before the client working directory was established
        foreach (Exception exception in s_unreportedExceptions)
        {
            LogException(exception);
        }

        s_unreportedExceptions.Clear(); //this is no longer used after this point

        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
        using (SystemController.Sys.UsersLock.EnterRead(out UserManager um))
        {
            m_packageManager.LoadUserUDFProperties(sm.UserFieldDefinitionManager, um);
        }

        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
        {
            for (int i = 0; i < sm.LoadedScenarioCount; i++)
            {
                Scenario s = sm.GetByIndex(i);
                using (s.ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
                {
                    m_packageManager.LoadUDFAndAttributeProperties(sd, sm.UserFieldDefinitionManager);
                }
            }
        }

        if (!PTSystem.LicenseKey.SDK && loadDevPackages)
        {
            PTMessage message = new ("Invalid parameter: LoadDevPackages".Localize(), "Invalid parameter");
            using (PTMessageForm messageForm = new (true))
            {
                messageForm.Show(message);
            }

            Environment.Exit(0);
        }

        if (!PTSystem.LicenseKey.SDK && CommandLineArguments.CreateInstanceOfServer.ArgumentFound)
        {
            PTMessage message = new ("Invalid parameter: CreateInstanceOfServer".Localize(), "Invalid parameter");
            using (PTMessageForm messageForm = new (true))
            {
                messageForm.Show(message);
            }

            Environment.Exit(0);
        }

        //Initialize controls with support settings
        PTMessageControl.SetSupportDetails(LoggedInInstance.IsSupportEmailSetup, this);

        //TODO: V12 Do we need these here?
        //Update strings
        m_readonlyMsg = "System is now in read-only mode. Any changes from this point on will not be saved. This is caused when license cannot be validated.".Localize();
        m_activeMsg = "System is no longer is in read-only mode.".Localize();

        SplashManager.UpdateSplashDescription("Loading Main Form...".Localize());
        //Set ThreadLock Reference
        SystemController.Sys.UsersLock.SetMainThread();
        SystemController.Sys.UserManagerEventsLock.SetMainThread();
        SystemController.Sys.ScenariosLock.SetMainThread();
    }

    private void InitializeBrand()
    {
        try
        {
            m_brand = m_packageManager.GetBrandModule().GetBrand();

            // get the icon for this branding
            Icon = m_brand.Icon;
            Text = m_brand.LongName;
        }
        catch (Exception err)
        {
            LogException(err);

            PTMessage message = new ("Error: Brand Package could not be found.", "Unable to log in".Localize());
            using (PTMessageForm messageForm = new (true))
            {
                messageForm.Show(message);
            }

            Environment.Exit(0);
        }
    }

    private static WorkingDirectory m_workingDirectory;
    public WorkingDirectory WorkingDirectory => m_workingDirectory;

    private ISettingsManager m_systemSettings;
    public ISettingsManager SystemSettings => m_systemSettings;

    /// <summary>
    /// This function will delay showing the main form until it is mostly loaded.
    /// This prevents showing the user a mostly blank unresponsive form that covers their screen
    /// </summary>
    protected override void SetVisibleCore(bool a_value)
    {
        if (m_allowDisplay)
        {
            base.SetVisibleCore(a_value);
            //SplashManager?.BringSplashToFront(); //This caused too many splash screen jumps
            //Localize();
        }
        else
        {
            //We are not ready to display the form yet.
            m_allowDisplay = true;
            InitializeForm();
            Show();
        }
    }

    private async void InitializeForm()
    {
        if (!RuntimeStatus.IsRuntime) //DesignMode only works in forms, not usercontrols!  Use this instead.
        {
            return;
        }

        //ConnectionChecker = new ConnectionChecker(this);

        PT.Common.Testing.Timing timer = new (true, "MainForm.MainForm2_Load");

        try
        {
            PTBaseControl.SetExceptionManager(this);
            BackgroundLock.SetExceptionManager(this);
            ThreadedBackgroundLock.SetInvokeControl(this);
            ThreadedBackgroundLock.SetExceptionManager(this);

            m_systemSettings = SystemController.Sys.SystemSettings;

            SplashManager.UpdateSplashDescription("Loading Events...".Localize());
            StartListeningToSchedulerEvents();
            //Load UI Icons

            SplashManager.UpdateSplashDescription("Loading Images...".Localize());
            ImageLists.LoadImages(ClientWorkingFolderFlag.WorkingFolderPath, m_messageProvider);

            //TODO:  UPDATE CLIENT FILES
            //RunClientUpdater();

            SplashManager.UpdateSplashDescription("Loading User Settings...".Localize());
            //Before loading the user settings, set the default workspace if specified
            //s_layoutSaver = new SettingsSaver(ClientWorkingFolderFlag.WorkingFolderPath, this, this);

            //TODO: Possible only load panes here.
            LoadWorkspaceController();
            LoadUserSettings();

            m_packageManager.LoadModules();

            //Initialize Statics
            LabelScriptGenerator.InitProperties(m_packageManager.GetObjectProperties(), m_workspaceInfo);

            LoadListeners();

            long loadWorkspaceDuration = timer.Length;
            ReportManager.SetReportsWorkingPath(Path.Combine(ClientWorkingFolderFlag.WorkingFolderPath, "Reports"));

            long applyWorkspaceDuration = timer.Length;

            SplashManager.UpdateSplashDescription("Loading User Data...".Localize());
            FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
            Shown += OnShown;

            //Start listening for dead connection events
            //ConnectionChecker.DeadConnectionEvent += new ConnectionChecker.DeadConnectionHander(connectionChecker_DeadConnectionEvent);
            //ConnectionChecker.SystemServiceUnavailableEvent += ConnectionChecker_SystemServiceUnavailableEvent;
            //ConnectionChecker.SystemServiceAvailableEvent += ConnectionChecker_SystemServiceAvailableEvent;

            SplashManager.UpdateSplashDescription("Loading Appearances...".Localize());
            //No Longer Used
            //Hashtable configSettings = (Hashtable)ConfigurationSettings.GetConfig("appearance");          

            Text = string.Format("{0}  |  {1}  {2}".Localize(), CurrentBrand.Name, InstanceInfo.InstanceName, InstanceInfo.InstanceVersion);

            m_clientSession.SystemBusyCannotSend += () => m_messageProvider.ShowBusyMessage();

            MultiLevelHourglass.VerifyCursor();

            //Set the form's default floating location
            int width = (int)(Screen.PrimaryScreen.WorkingArea.Width * .8);
            int left = (int)(Screen.PrimaryScreen.WorkingArea.Width * .1);
            int height = (int)(Screen.PrimaryScreen.WorkingArea.Height * .8);
            int top = (int)(Screen.PrimaryScreen.WorkingArea.Height * .1);
            Size = new Size(width, height);
            Location = new Point(left, top);

            BoundGridControl.SetGridModules(m_packageManager.GetGridModules());

            LoadScenarios();

            APSUICommandWindow.Initialize(m_scenarioInfo);
            long loadScenariosDuration = timer.Length;

            await LoadMainInterfaceModules();

            InitializeToolbarItems();

            await LoadUserLayoutSettings();

            //TODO: Remove this once gantt base controls are revamped
            GanttDotNet.GanttDataSource.SetGanttModules(m_packageManager.GetGanttModules());

            //TODO: Reports
            //ReportManager reportManager = new ReportManager(this, m_messageProvider);
            //reportManager.SetBrand(CurrentBrand);
            //System.Collections.Generic.List<BarButtonItem> newReports = reportManager.LoadReportTools();

            //Turn on the receiver to start polling for UI messages now that the system is loaded.  
            //  This should be the last thing done to avoid deadlocks with the user manager when it's trying
            //  to respond to events being triggered by transmissions while the UI is trying to access usermanager.
            //SystemController.StartSystemReceiver();

            s_busyMessageTimer.Tick += new EventHandler(busyMessageTimer_Tick);

            MultiLevelHourglass.TurnOff(1);

            timer.Stop();
            long fullLoadDuration = timer.Length;
            //PTSystem.Receiver.InitializationTiming.ClientUpdaterFinished = 0; //TODO: Update
            //PTSystem.Receiver.InitializationTiming.ClientWorkspacesLoaded = loadWorkspaceDuration;
            //PTSystem.Receiver.InitializationTiming.ClientWorkspacesApplied = applyWorkspaceDuration;
            //PTSystem.Receiver.InitializationTiming.ClientScneariosLoaded = loadScenariosDuration;
            //PTSystem.Receiver.InitializationTiming.ClientFullyLoaded = fullLoadDuration;

            #if BEMA
                //Bema Memory check
                //TODO: Find a way to make this a system option?
                m_memoryValidationTimer.Interval = 5 * 60 * 1000;
#if DEBUG
                m_memoryValidationTimer.Interval = 5000;
#endif
                m_memoryValidationTimer.Tick += MemoryValidationTimerOnTick;
                m_memoryValidationTimer.Start();
            #endif
            SetupActiveUserTimer();
            UpdateAutoLogoutTimespan(LoggedInInstance.AutoLogoffTimeout);
            ValidateGanttLayouts();
            //Localize(); //TODO
            
            SplashManager.HideExternalSplash();
        }
        catch (Exception err)
        {
            try
            {
                LogException(err);
                m_messageProvider.ShowMessageBoxError(new Exception("2439", err), true, "Error loading Client".Localize(), this);
            }
            catch (Exception e)
            {
                LogException(e);
            }

            ExitImmediately();
        }
        finally
        {
            Timing.Log(timer);
        }
    }
    /// <summary>
    /// Sends a  navigation event to trigger the validation of Gantt Layouts
    /// </summary>
    private void ValidateGanttLayouts()
    {
        UINavigationEvent clientLoaded = new("ValidateGanttLayouts");
        FireNavigationEvent(clientLoaded);
    }
    private void OnShown(object a_sender, EventArgs a_e)
    {
        //The UI will open in front of the splash screen. Re-open it
        //Note that this will again bring the splash in front of other apps
        SplashManager?.BringSplashToFront();
    }

    /// <summary>
    /// Load listener elements and store them sorted by priority
    /// </summary>
    private void LoadListeners()
    {
        m_navigationListeners = new List<INavigationListenerElement>();
        List<INavigationListenerModule> notificationModules = m_packageManager.GetNavigationListenerModules();
        foreach (INavigationListenerModule notificationModule in notificationModules)
        {
            m_navigationListeners.AddRange(notificationModule.GetNotificationElements());
        }

        m_navigationListeners = m_navigationListeners.OrderBy(e => e.Priority).ToList();
    }

    private void LoadWorkspaceController()
    {
        //TODO: Potentially load from packages. We would need a basic default if package not loaded
        //List<IMainWorkspaceManagerModule> mainWorkspaceManagerModules = m_packageManager.GetMainWorkspaceManagerModules();
        //foreach (IMainWorkspaceManagerModule mainWorkspaceManagerModule in mainWorkspaceManagerModules)
        //{
        //    m_workspaceController = mainWorkspaceManagerModule.GetWorkspaceController();
        //}
        m_workspaceController = new WorkspaceManager(this);
        m_workspaceInfo = m_workspaceController.GetWorkspaceInfo(); //m_workspaceController.GetWorkspaceInfo();
        m_workspaceManagerHelper = new WorkspaceManagerHelper(this, m_scenarioInfo);
    }

    private List<ISystemClosingElement> m_systemClosingElements;

    private async Task LoadMainInterfaceModules()
    {
        m_systemClosingElements = new List<ISystemClosingElement>();
        List<IMainInterfaceModule> mainInterfaceModules = m_packageManager.GetMainInterfaceModules();
        foreach (IMainInterfaceModule module in mainInterfaceModules)
        {
            m_systemClosingElements.AddRange(module.GetSystemClosingElements());
            List<INavigationElement> navElements = module.GetNavigationElements();
            foreach (INavigationElement navElement in navElements)
            {
                navElement.InitializeNavigation(this, m_scenarioInfo, this);
            }
        }

        m_systemClosingElements = m_systemClosingElements.OrderBy(a_element => a_element.Priority).ToList();

        await LoadMainMenus();
    }

    private ClientSession LoginClient(LoginProcedure a_loginProcedure)
    {
        try
        {
            return a_loginProcedure.LoginClient();
        }
        catch (Exception e)
        {
            MultiLevelHourglass.TurnOff(true);
            LogException(e);
            throw;
        }
        finally
        {
            SplashManager?.UpdateSplashWarning();
            LoggedInInstance = a_loginProcedure.ConnectedInstanceInfo;
            LastLoginSetttings = CommandLineArguments;
        }
    }

    private ClientSession LoginServer(LoginProcedure a_loginProcedure, string a_devPackagesPath)
    {
        try
        {
            return a_loginProcedure.LoginServer(a_devPackagesPath);
        }
        catch (Exception e)
        {
            MultiLevelHourglass.TurnOff(true);
            LogException(e);
            throw;
        }
        finally
        {
            if (SystemController.Receiving)
            {
                SplashManager?.UpdateSplashWarning();
                LoggedInInstance = a_loginProcedure.ConnectedInstanceInfo;
                LastLoginSetttings = CommandLineArguments;
            }
            else
            {
                ClientWorkingFolderFlag.Dispose();
            }
        }
    }

    private Exception StartExternalSystem(LoginProcedure a_loginProcedure, SplashScreenManagement a_splashManager)
    {
        try
        {
            MessageProviderUtility messageProvider = new (this, GetOwnerForm(), null);
            return a_loginProcedure.StartExternalSystem(messageProvider);
        }
        catch (Exception e)
        {
            MultiLevelHourglass.TurnOff(true);
            LogException(e);
            return e;
        }
        finally
        {
            if (SystemController.Receiving)
            {
                SplashManager?.UpdateSplashWarning();
            }
            else
            {
                ClientWorkingFolderFlag.Dispose();
            }
        }
    }

    //private bool m_restoreWindowLayout;

    //private void BrowseReports(string folder)
    //{
    //    string fileName = "";
    //    try
    //    {
    //        using (OpenFileDialog openReportDlg = new OpenFileDialog())
    //        {
    //            openReportDlg.AddExtension = true;
    //            openReportDlg.CheckFileExists = true;
    //            openReportDlg.CheckPathExists = true;
    //            openReportDlg.DefaultExt = "*rpt";
    //            openReportDlg.Filter = "Crystal / Microsoft |*.rpt;*.rdlc;*.*";
    //            openReportDlg.InitialDirectory = folder;
    //            openReportDlg.ShowDialog();
    //            if (openReportDlg.FileName != "")
    //            {
    //                fileName = openReportDlg.FileName;
    //                MultiLevelHourglass.TurnOff(true);
    //                Process.Start(openReportDlg.FileName);
    //            }

    //            //Refresh the tools; may have renamed, copied, deleted reports.
    //            MultiLevelHourglass.TurnOn();
    //            ReportManager rptMgr = new ReportManager();
    //            //TODO: rptMgr.LoadReportTools(ultraToolbarsManager1);
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        MultiLevelHourglass.TurnOff(true);
    //        if (fileName.EndsWith(".rpt"))
    //        {
    //            ShowMessageBox(e.Message + "Crystal Reports must be installed for editing these reports.".Localize(), "Can't Open Crystal Report".Localize(), MessageBoxButtons.OK, MessageBoxIcon.Warning, false);
    //        }
    //        else if (fileName.EndsWith(".rdlc"))
    //        {
    //            ShowMessageBox(e.Message + "Visual Studio must be installed for editing Microsoft reports.".Localize(), "Can't open Microsoft Report".Localize(), MessageBoxButtons.OK, MessageBoxIcon.Warning, false);
    //        }
    //        else
    //        {
    //            ShowMessageBox(e.Message + "This report type is not supported.  Only Crystal Reports and Microsoft Reports are supported.".Localize(), "Can't open report".Localize(), MessageBoxButtons.OK, MessageBoxIcon.Warning, false);
    //        }
    //    }
    //}

    /// <summary>
    /// Alerts the user of the new exception.
    /// </summary>
    internal void NewException(Exception a_e, bool a_fatal, bool a_redo)
    {
        try
        {
            //Error was already logged by the ErrorReporter.
            //Show if the cause wans't caused by a redo playback.
            if (!a_redo)
            {
                //For fatal errors, show the error message on this thread so it doesn't get shutdown by the error logger until the user closes the dialog.
                //Don't show threading issues since there is nothing the user can do about it.
                if (a_fatal && a_e is not InvalidOperationException)
                {
                    m_messageProvider.ShowMessageBoxError(a_e, true);
                }

                UINavigationEvent errorLogEvent = new ("ErrorLogsUpdated");
                errorLogEvent.Data.Add("NewError", null);

                FireNavigationEvent(errorLogEvent);
            }
        }
        catch (Exception err)
        {
            UnhandledException(err);
        }
    }

    #region Form Events
    private static bool s_closeImmediately;
    private static bool s_userDeleted;
    private static bool s_userLoggedOut;

    /// <summary>
    /// If this is true, then when the form is being closed for any reason, the closing procedure is skipped and the form
    /// is allowed to close.
    /// </summary>
    private bool m_allowFormClose;

    private bool m_restartOnClose;

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (m_allowFormClose ||
            e.CloseReason == CloseReason.WindowsShutDown //OS shutting down, no need to prevent it
            ||
            IsDisposed ||
            Disposing // We should not have gotten here, but too late to prompt
            ||
            !m_connectionStateManager.IsConnected) //No connection, no need to prompt
        {
            if (m_restartOnClose)
            {
                try
                {
                    Process.Start(Application.ExecutablePath, LastLoginSetttings.CreateArgumentString());
                }
                catch (Exception err)
                {
                    LogException(err);
                }
            }

            //All processing has been complete, allow the form to close normally
            return;
        }

        //Don't close yet.
        e.Cancel = true;

        //Trigger followup actions that may lead to again closing the form
        PromptToExit();
    }

    private async void PromptToExit()
    {
        //Wrap everything in try/catch/finally to be sure app exits.
        try
        {
            bool skipExitPrompts = m_restartOnClose //Don't prompt user, just save settings and exit
                                   ||
                                   s_closeImmediately ||
                                   s_userLoggedOut //User logged out by admin action, no prompts
                                   ||
                                   s_userDeleted //User deleted by admin, no prompts
                                   ||
                                   CommandLineArguments.EndQuickly.ArgumentFound; //For internal dev quick exists

            if (!skipExitPrompts)
            {
                EExitResult exitResult = EExitResult.FirstPrompt;
                foreach (ISystemClosingElement closingElement in m_systemClosingElements)
                {
                    exitResult = closingElement.ShowExitPrompt(exitResult);
                    if (exitResult == EExitResult.ExitImmediately)
                    {
                        break;
                    }

                    if (exitResult == EExitResult.CancelExit)
                    {
                        return;
                    }

                    if (exitResult == EExitResult.ContinueNoPrompt)
                    {
                        exitResult = EExitResult.FirstPrompt;
                    }
                }
            }

            foreach (ISystemClosingElement systemClosingElement in m_systemClosingElements)
            {
                systemClosingElement.Exiting();
            }

            await LogOutMainForm().ConfigureAwait(true);

            //Mainform won't wait for async handlers. Retry the close now that we are done logging out.
            CloseForm();
        }
        catch (Exception err)
        {
            //MainForm.LogException(err);
            m_messageProvider.ShowMessageBoxError(err, true, "Error during Shutdown".Localize());
        }
        finally
        {
            Timing.WriteToFile();
        }
    }

    private void CloseForm()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(Close));
        }
        else
        {
            Close();
        }
    }

    private async Task LogOutMainForm()
    {
        try
        {
            UINavigationEvent prepareCloseEvent = new (UINavigationEvent.PrepareLogout);
            FireNavigationEvent(prepareCloseEvent);

            m_listener.RemoveAllListeners(SystemController.Sys.SystemLoggerInstance);

            //Detach scenario events to avoid errors on shutdown
            m_scenarioController.UnloadAllScenarios();
            m_autoSaveSettingManager.Stop();
            m_closingSoIgnoreAllEvents = true;
            m_activeUserTimer?.Stop();
            s_busyMessageTimer.Stop();
            Hide(); //for quicker shutdown

            try
            {
                //Save things that are PTService independent first in case the server has shut down.
                if (!s_userDeleted)
                {
                    await StoreAllUserSettings(true).ConfigureAwait(true);
                }
                //Localizer.CommitNewResourcesToLocalizationFiles(); // No more calls to m_resourcemanager.GetString() -- Call this function to re-create Localization files before shutdown
            }
            catch (Exception err)
            {
                LogException(err);
            }

            try
            {
                //Dispose all boards and tiles and events
                m_scenarioViewerContainer.Unload();

                if (SystemController.Receiving)
                {
                    await Logout().ConfigureAwait(true);
                }

                if (CommandLineArguments.ClientType == CommandLineArgumentsHelper.EClientType.Internal)
                {
                    // This will also cause a LogOut.
                    // A window is shown because it might take a while to shut down the system.
                    APSCommandWindow cmdWindow = null;
                    try
                    {
                        cmdWindow = new APSCommandWindow(false, m_clientSession);
                        cmdWindow.Show();
                        cmdWindow.WriteLine();
                        cmdWindow.WriteLine("Stopping APS Service...", true);

                        SystemController.StopAPSService("User Logout/Shutdown");
                    }
                    finally
                    {
                        cmdWindow?.Close();
                    }
                }
            }
            catch (Exception err)
            {
                LogException(err);
            }
        }
        catch (Exception err)
        {
            m_messageProvider.ShowMessageBoxError(err, true, "Error during Shutdown".Localize());
        }
        finally
        {
            ClientWorkingFolderFlag?.Dispose();

            m_allowFormClose = true;
        }
    }
    #endregion

    private void busyMessageTimer_Tick(object sender, EventArgs e)
    {
        s_busyMessageTimer.Stop();
    }

    private MainFormListener m_listener;

    private void StartListeningToSchedulerEvents()
    {
        m_listener = new MainFormListener(this, SystemController.Sys.SystemLoggerInstance); //start listening for scheduler changes.
    }

    internal void NewTransmissionFailure(Exception e, PTTransmission t, SystemMessage message)
    {
        try
        {
            //Only show stack trace in debug mode.
            #if DEBUG
            string msg = string.Format("{0}{2}{2}{1}", message.MessageText, e.StackTrace, Environment.NewLine);
            #else
                string msg = message.MessageText;
            #endif
            m_messageProvider.ShowMessage(msg);
        }
        catch (Exception err)
        {
            UnhandledException(err);
        }
    }

    //Update the RuleSeek status statusbar
    internal void RuleSeekStatusUpdateHandling(ScenarioManager a_sm, CoPilotStatusUpdateT a_t)
    {
        string updateMessage = "CoPilot Status: ";

        switch (a_t.Status)
        {
            case CoPilotStatusUpdateT.CoPilotStatusUpdateValues.RuleSeekStarted:
                updateMessage += "Rule Seek Started";
                break;
            case CoPilotStatusUpdateT.CoPilotStatusUpdateValues.InsertJobsStarted:
                updateMessage += "InsertJobs Started";
                break;
            case CoPilotStatusUpdateT.CoPilotStatusUpdateValues.InsertJobsStopped:
                updateMessage += "InsertJobs Finished";
                break;
            case CoPilotStatusUpdateT.CoPilotStatusUpdateValues.Progress:
                //RuleSeek TODO
                updateMessage += "RuleSeek in progress";
                break;
            case CoPilotStatusUpdateT.CoPilotStatusUpdateValues.Enabled:
                //RuleSeek TODO
                updateMessage += "Enabled";
                break;
            case CoPilotStatusUpdateT.CoPilotStatusUpdateValues.Error:
                switch (a_t.ErrorStatus)
                {
                    case CoPilotStatusUpdateT.CoPilotErrorValues.FailedToStart:
                        updateMessage += "Failed To Start";
                        break;
                    case CoPilotStatusUpdateT.CoPilotErrorValues.SimulationException:
                        updateMessage += "Error during simulation";
                        break;
                    case CoPilotStatusUpdateT.CoPilotErrorValues.Unknown:
                        updateMessage += "Error. Attempting to restart simulation";
                        break;
                    case CoPilotStatusUpdateT.CoPilotErrorValues.InsertJobsNotStarted:
                        updateMessage += "InsertJobs is not running.";
                        break;
                    default:
                        updateMessage += "Error";
                        break;
                }

                break;
        }

        //TODO: NewUI
        //s_statusBar.RuleSeekPanelStatusText = Localizer.GetString(updateMessage);
    }

    //Update the RuleSeek status statusbar
    internal void RuleSeekSimulationStopHandling(ScenarioManager a_sm, RuleSeekEndReasons a_reason)
    {
        //TODO: V12 NewUI

        string statusMessage = "CoPilot Status: Idle";
        switch (a_reason)
        {
            case RuleSeekEndReasons.LiveScenarioChanged:
                statusMessage += " (Live Scenario Changed)";
                break;
            case RuleSeekEndReasons.ScenarioAction:
                statusMessage += " (Scenario Action)";
                break;
            case RuleSeekEndReasons.Startup:
                statusMessage += " (System Startup)";
                break;
            case RuleSeekEndReasons.Error:
                statusMessage += " (Error)";
                break;
            case RuleSeekEndReasons.ERP:
                statusMessage += " (ERP Change)";
                break;
            case RuleSeekEndReasons.Dissabled:
                statusMessage = "CoPilot Status: Off";
                break;
        }
    }

    private readonly string m_readonlyMsg;
    private readonly string m_activeMsg;

    internal async void SystemStateSwitched(SystemStateSwitchT a_t)
    {
        await Task.Run(() => EnforceReadonly(a_t.MakeReadOnly));
    }

    /// <summary>
    /// Indicates wheter the UI is displaying readonly status
    /// </summary>
    private bool m_inReadOnly;

    private void EnforceReadonly(bool a_readonlyMode)
    {
        if (m_inReadOnly == a_readonlyMode)
        {
            //Same state. Nothing to change
            return;
        }

        m_inReadOnly = a_readonlyMode;

        EnterReadonlyHelper readOnlyHelper = new (this, m_scenarioInfo, m_packageManager);
        readOnlyHelper.ProcessReadonlyStateChange(a_readonlyMode);
    }

    internal void PopupMessage(string message, bool showAllUsers, BaseId showForThisUser)
    {
        try
        {
            if (showAllUsers || showForThisUser == SystemController.CurrentUserId)
            {
                m_messageProvider.ShowMessage(message);
            }
        }
        catch (Exception e)
        {
            UnhandledException(e);
        }
    }
    internal async void RestartRequiredHandler(ClientUserRestartT a_t)
    {
        try
        {
            List<BaseId> affectedUsers = a_t.AffectedUsers;
            BaseId currentUserId = SystemController.CurrentUserId;

            if (!affectedUsers.Contains(currentUserId))
            {
                return;
            }

            UINavigationEvent navigationEvent = new ("ShowNotificationSlide");

            if (a_t.RestartClient)
            {
                navigationEvent.Data.Add("ShowRestartSlide", a_t.RestartMessage);

                if (a_t.Instigator != currentUserId)
                {
                    FireNavigationEvent(navigationEvent);
                    await Task.Delay(10000);
                }

                m_restartOnClose = true;
                CloseForm();
            }
            else
            {
                navigationEvent.Data.Add("ShowRestartOptionalSlide", a_t.RestartMessage);
                FireNavigationEvent(navigationEvent);
            }

            //if (a_t.Instigator != currentUserId)
            //{
            //    if (a_t.RestartClient)
            //    {

            //        navigationEvent.Data.Add("ShowRestartTile", a_t.RestartMessage);

            //        FireNavigationEvent(navigationEvent);
            //        //m_messageProvider.ShowMessageBox(new PTMessage(a_t.RestartMessage, "Restart Required".Localize()) { Classification = PTMessage.EMessageClassification.Information }, true);

            //        await Task.Delay(10000);

            //        m_restartOnClose = true;
            //        Close();
            //    }
            //    else
            //    {
            //        navigationEvent.Data.Add("ShowRestartOptionalTile", a_t.RestartMessage);
            //        //TODO: Restore;
            //        //AddNewNotification(new RestartNotification(this, a_t.RestartMessage));
            //    }
            //}
            //else
            //{
            //    if (a_t.RestartClient)
            //    {
            //        m_restartOnClose = true;
            //        Close();
            //    }
            //    else
            //    {
            //        //TODO: Restore;
            //        //AddNewNotification(new RestartNotification(this, a_t.RestartMessage));
            //    }
            //}
        }
        catch (Exception e)
        {
            UnhandledException(e);
        }
    }

    internal void RestartServiceRequiredHandler(PackageUpdateT a_t)
    {
        try
        {
            DialogResult rslt = ScenarioInfo.MessageProvider.ShowMessageBox(new PTMessage("Package changes made that require a client restart. Would you like to restart now?".Localize(), "Restart Required".Localize()) { Classification = PTMessage.EMessageClassification.Question }, true);

            if (rslt == DialogResult.Yes)
            {
                m_restartOnClose = true;
                CloseForm();
            }
        }
        catch (Exception e)
        {
            UnhandledException(e);
        }
    }

    internal async void InstanceMessageReceivedHandler(InstanceMessageT a_t)
    {
        List<long> affectedUsers = a_t.AffectedUsers;
        BaseId currentUserId = SystemController.CurrentUserId;

        if (affectedUsers!= null &&
            !affectedUsers.Contains(currentUserId.Value))
        {
            return;
        }

        if (!string.IsNullOrEmpty(a_t.Message))
        {
            UINavigationEvent navigationEvent = new ("ShowNotificationSlide");
            navigationEvent.Data.Add("ShowInstanceMessageSlide", a_t.Message);
            navigationEvent.Data.Add("InstanceAction", a_t.ShutdownWarning);
            FireNavigationEvent(navigationEvent);
        }

        if (a_t.Shutdown)
        {
            if (a_t.ShutdownWarning)
            {
                await Task.Delay(TimeSpan.FromSeconds(15));
            }

            m_restartOnClose = false;
            s_closeImmediately = true;
            CloseForm();
        }
    }

    private void MainForm_HelpRequested(object sender, HelpEventArgs a_hlpevent)
    {
        "".ShowHelp();
    }

    public void ShowErrorLog(short a_focusTabIdx = 2) //Default to system tab
    {
        UINavigationEvent errorMessageDialogEvent = new(UINavigationEvent.OpenErrorMessageDialog);
        errorMessageDialogEvent.Data.Add("isSupportEmailSetup", LoggedInInstance.IsSupportEmailSetup);
        errorMessageDialogEvent.Data.Add("ErrorReporter", SystemController.Sys.SystemLoggerInstance);
        errorMessageDialogEvent.Data.Add("FocusTabIdx", a_focusTabIdx);
        FireNavigationEvent(errorMessageDialogEvent);
    }
    public string GetLoggedErrorMessage(Guid a_transmissionId)
    {
        string loggedError = SystemController.Sys.SystemLoggerInstance.GetReportedTransmissionErrors(a_transmissionId);
        return loggedError;
    }

    public Form GetOwnerForm()
    {
        return this;
    }

    public IUsersInfo UsersInfo => m_usersInfo;

    public IMessageProvider MessageProvider { get; private set; }

    public IBrand CurrentBrand => m_brand;

    public IScenarioInfo ScenarioInfo => m_scenarioInfo;

    public void CloseMenus()
    {
        m_barManager_Main.CloseMenus();
    }

    public void ChangeTheme(string a_newTheme, bool a_initializing)
    {
        //Don't need to show overlay form on startup
        if (!a_initializing)
        {
            m_overlayForm = new OverlayForm(this, m_brand.ActiveTheme, string.Format("Applying the '{0}' theme...".Localize(), a_newTheme.Localize()));
        }

        Skin skin = CommonSkins.GetSkin(UserLookAndFeel.Default.ActiveLookAndFeel);

        //Change State
        UserLookAndFeel.Default.SetSkinStyle(skin.Name, a_newTheme);
        LookAndFeelHelper.ForceDefaultLookAndFeelChanged();

        CurrentBrand.ActiveTheme.FireThemeChangedEvent();

        //Overlay form not displayed on startup
        if (!a_initializing)
        {
            //Wait 6 seconds to allow the re-drawing to finish before hiding the overlay
            Thread.Sleep(6000);
            m_overlayForm?.HideOverlay();
        }
    }

    public LoggedInInstanceInfo InstanceInfo => LoggedInInstance;

    public event Action<UINavigationEvent> UiNavigationEvent;

    /// <summary>
    /// Wait for UIEvent to be handled.
    /// </summary>
    /// <param name="a_navigationEvent"></param>
    /// <returns></returns>
    public async Task<bool> FireNavigationEventUntilHandled(UINavigationEvent a_navigationEvent)
    {
        int maxTries = 5;
        while (!a_navigationEvent.Handled && maxTries > 0)
        {
            FireNavigationEvent(a_navigationEvent);
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            maxTries--;
        }


        return a_navigationEvent.Handled;
    }

    public async void FireNavigationEvent(UINavigationEvent a_navigationEvent)
    {
        //Override specific events
        if (a_navigationEvent.Key == "CopyScenario")
        {
            MultiLevelHourglass.TurnOn(77);
            ScenarioCopyT t = new (m_scenarioInfo.ScenarioId);
            m_clientSession.SendClientAction(t);
            return;
        }

        if (a_navigationEvent.Key == "ToggleHeaders")
        {
            if (a_navigationEvent.Data.TryGetValue("ToggleObject", out object toggle))
            {
                BoardTabsMode = (PackageEnums.EBoardTabsMode)toggle;
            }
        }
        else if (a_navigationEvent.Key == UINavigationLogoutEvent.LogoutKey)
        {
            MultiLevelHourglass.TurnOn(102);
            if (a_navigationEvent.Data.TryGetValue(UINavigationLogoutEvent.RestartClientKey, out object restartOnClose))
            {
                if ((bool)restartOnClose)
                {
                    m_restartOnClose = true;
                }
            }

            if (a_navigationEvent.Data.TryGetValue(UINavigationLogoutEvent.CloseImmediatelyKey, out object closeImmediately))
            {
                if ((bool)closeImmediately)
                {
                    s_closeImmediately = true;
                }
            }

            if (a_navigationEvent.Data.TryGetValue(UINavigationLogoutEvent.UserLoggedOutValueKey, out object noPrompt))
            {
                if ((bool)noPrompt)
                {
                    s_userLoggedOut = true;
                }
            }
            #if DEBUG
            //For testing
            if (PtImageCache.ReportMissingIcons())
            {
                Invoke(new Action(() => MessageBox.Show("Missing images logged to 'C:\\Temp\\MissingImages.txt'", "Missing Images")));
            }
            #endif


            #if Localization
               if (Localizer.ReportMissingTranslations())
                {
                    Invoke(new Action(() => MessageBox.Show("Missing translations logged to 'C:\\Temp\\newEntries.txt'", "Missing Translations")));
                }
            #endif

            CloseForm();
            MultiLevelHourglass.TurnOff(102);
        }
        else if (a_navigationEvent.Key == UIWorkspaceEvent.ImportKey)
        {
            if (a_navigationEvent.Data.TryGetValue(UIWorkspaceEvent.ImportKey, out object workspaceInfo))
            {
                (Dictionary<string, SettingData> SettingDictionary, string WorkspaceName, bool Overwrite, bool ResestOtherSettings, List<BaseId> UserIds, bool ApplyImmediately) tuple
                    = (ValueTuple<Dictionary<string, SettingData>, string, bool, bool, List<BaseId>, bool>)workspaceInfo;
                m_workspaceController.ImportWorkspace(tuple.SettingDictionary, tuple.WorkspaceName, tuple.Overwrite, tuple.ResestOtherSettings, tuple.UserIds, tuple.ApplyImmediately);
                a_navigationEvent.Handled = true;
            }
        }
        else if (a_navigationEvent.Key == UIWorkspaceEvent.SwitchWorkspace)
        {
            if (a_navigationEvent.Data.TryGetValue(UIWorkspaceEvent.WorkspaceName, out object ws) && ws is string workspaceName)
            {
                WorkspaceSwitch(workspaceName);
            }
        }
        else if (a_navigationEvent.Key == UIWorkspaceEvent.CopyWorkspace)
        {
            if (a_navigationEvent.Data.TryGetValue(UIWorkspaceEvent.WorkspaceName, out object source) &&
                source is string sourceWorkspace &&
                a_navigationEvent.Data.TryGetValue(UIWorkspaceEvent.NewWorkspaceName, out object newName) &&
                newName is string newWorkspaceName)
            {
                int num = 0;
                while (!m_workspaceController.CopyWorkspace(sourceWorkspace, newWorkspaceName) && num < 5)
                {
                    num++;
                }
            }
        }
        else if (a_navigationEvent.Key == UIWorkspaceEvent.RenameWorkspace)
        {
            if (a_navigationEvent.Data.TryGetValue(UIWorkspaceEvent.WorkspaceName, out object name))
            {
                (string WorkspaceToRename, string NewName) renameTuple = (ValueTuple<string, string>)name;
                m_workspaceController.RenameWorkspace(renameTuple.WorkspaceToRename, renameTuple.NewName);
            }
        }
        else if (a_navigationEvent.Key == UIWorkspaceEvent.DeleteWorkspace)
        {
            if (a_navigationEvent.Data.TryGetValue(UIWorkspaceEvent.WorkspaceName, out object ws) && ws is string workspaceName)
            {
                m_workspaceController.DeleteWorkspace(workspaceName);
            }
        }
        else if (a_navigationEvent.Key == UIEmailEvent.SendSupportEmail)
        {
            if (a_navigationEvent.Data.TryGetValue(UIEmailEvent.EmailMessage, out object message) && message is string emailMessage)
            {
                if (a_navigationEvent.Data.TryGetValue(UIEmailEvent.ControlUsed, out object parentControlKey) && parentControlKey is string parentKey)
                {
                    using (BackgroundLock asyncLock = new (BaseId.NULL_ID))
                    {
                        await asyncLock.RunLockCode(this, ShowSupportEmailDialog, emailMessage, parentKey);
                        if (asyncLock.Status is BackgroundLock.EResultStatus.Error or BackgroundLock.EResultStatus.Canceled)
                        {
                            //continue events through other listeners
                        }
                    }
                }
            }
        }
        else if (a_navigationEvent.Key == UINavigationEvent.OpenDebugConsole)
        {
            APSUICommandWindow cmdWindow = new (string.Empty, m_clientSession);
            cmdWindow.Size = new Size(250, 120);
            cmdWindow.Show();
        }
        else if (a_navigationEvent.Key == UIOpenReportEvent.OpenReportForm)
        {
            UIOpenReportEvent reportEvent = a_navigationEvent as UIOpenReportEvent;
            if (reportEvent.Data.TryGetValue(UIOpenReportEvent.ReportName, out object reportNameObj) && reportNameObj is string reportName && !string.IsNullOrEmpty(reportName))
            {
                if (m_packageManager.GetReport(reportName) is IReportModule reportModule)
                {
                    using (BackgroundLock asyncLock = new (ScenarioInfo.ScenarioId))
                    {
                        await asyncLock.RunLockCode(this, ShowReportForm, reportModule, reportEvent);
                        if (asyncLock.Status is BackgroundLock.EResultStatus.Error or BackgroundLock.EResultStatus.Canceled)
                        {
                            //continue events through other listeners
                        }
                    }

                }
            }
        }
        //Pass to listener elements
        foreach (INavigationListenerElement element in m_navigationListeners)
        {
            element.ProcessNavigationEvent(a_navigationEvent);
            if (a_navigationEvent.Handled)
            {
                return;
            }
        }

        //Pass event to other controls
        UiNavigationEvent?.Invoke(a_navigationEvent);
    }

    private void ShowReportForm(Scenario a_s, ScenarioDetail a_sd, params object[] a_params)
    {
        IReportModule reportModule = (IReportModule)a_params[0];
        UIOpenReportEvent openReportEvent = (UIOpenReportEvent)a_params[1];
        ReportForm reportForm = new(this, ScenarioInfo, MessageProvider, reportModule.ReportInfo.ReportName, false);

        if (openReportEvent.Data.TryGetValue(UIOpenReportEvent.JobIds, out object jobIdsObj) && jobIdsObj is List<BaseId> jobIds)
        {
            DataSet reportDataSet = reportModule.GenerateDataset(a_sd, jobIds);

            if (reportModule.GetReport(reportDataSet) is XtraReport report)
            {
                reportForm.InitReport(reportModule.ReportInfo.ReportName, report);
                reportForm.ShowDialog();

                reportModule.PostReportShown(a_sd, jobIds);
            }
        }
    }

    /// <summary>
    /// Generic unhandled exception message. Can be called from background threads.
    /// Logs the error and shows a popup message.
    /// </summary>
    void IExceptionManager.UnhandledException(Exception a_e)
    {
        UnhandledException(a_e);
    }

    private void HandleScenarioDesync()
    {
        //try
        //{
        //    SystemServiceProxy.SystemProxy systemProxy = new SystemServiceProxy.SystemProxy(LoggedInInstance.SystemServiceUrl);
        //    var sysClient = systemProxy.GetSystemServiceClient();
        //    string details;
        //    long nbrOfSimulations;
        //    decimal resourceJobOperationCombos;
        //    int scenarioByteLength;
        //    decimal startAndEndSums;
        //    Stream scenarioBytesStream;
        //    int blockCount = sysClient.GetScenario(m_scenarioInfo.ScenarioId.Value, out details, out nbrOfSimulations, out resourceJobOperationCombos, out scenarioByteLength, out startAndEndSums, out scenarioBytesStream);
        //    byte[] scenarioBytes;
        //    using (MemoryStream writeStream = new MemoryStream())
        //    {
        //        scenarioBytesStream.CopyTo(writeStream);
        //        scenarioBytes = writeStream.ToArray();
        //    }

        //    scenarioBytesStream.Dispose();
        //    ScenarioManager sm;
        //    using (SystemController.Sys.ScenariosLock.EnterWrite(out sm))
        //    {
        //        sm.ReloadScenario(m_scenarioInfo.ScenarioId, scenarioBytes, startAndEndSums, resourceJobOperationCombos, blockCount, details, nbrOfSimulations);
        //    }
        //}
        //catch (Exception e)
        //{
        //    LogException(e);
        //    throw;
        //}
    }

    private PackageEnums.EBoardTabsMode BoardTabsMode { get; set; }

    protected override bool ProcessCmdKey(ref Message a_msg, Keys a_keyData)
    {
        if (a_keyData == (Keys.Shift | Keys.Tab))
        {
            if (BoardTabsMode == PackageEnums.EBoardTabsMode.None)
            {
                BoardTabsMode = PackageEnums.EBoardTabsMode.Show;
            }
            else if (BoardTabsMode == PackageEnums.EBoardTabsMode.Show)
            {
                BoardTabsMode = PackageEnums.EBoardTabsMode.None;
            }

            TabSettings tabSettings = m_workspaceInfo.LoadSetting<TabSettings>(TabSettings.s_settingKey);
            tabSettings.TabMode = BoardTabsMode;
            m_workspaceInfo.SaveSetting(tabSettings);

            UINavigationEvent newEvent = new ("ToggleHeaders");
            newEvent.Data.Add("ToggleObject", BoardTabsMode);
            UiNavigationEvent?.Invoke(newEvent);
            return true;
        }

        return base.ProcessCmdKey(ref a_msg, a_keyData);
    }

    internal void ShowDataActivationWarning(DataActivationWarningT a_t)
    {
        m_messageProvider.ShowMessage("The scenario differs from the one your key was activated against.  You are at risk of readonly mode.  If you are placed in readonly mode you can undo to get back to a valid state.");
    }

    public decimal CurrentDpiScaler { get; private set; }

    private void OnDpiChanged(object a_sender, DpiChangedEventArgs a_e)
    {
        int eDeviceDpiOld = a_e.DeviceDpiOld;
        int eDeviceDpiNew = a_e.DeviceDpiNew;
        CurrentDpiScaler = (decimal)DeviceDpi / 96;
    }

    private void MultiLevelHourglassOnUseWaitCursor(bool a_useWaitCursor)
    {
        UseWaitCursor = a_useWaitCursor;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        m_brand?.Dispose();
        Application.ThreadException -= new ThreadExceptionEventHandler(Application_ThreadException);
        AppDomain.CurrentDomain.UnhandledException -= new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

        base.Dispose(disposing);
    }

    #region Memory Management
    #if BEMA
        readonly Timer m_memoryValidationTimer = new Timer();

        private void MemoryValidationTimerOnTick(object a_sender, EventArgs a_e)
        {
            try
            {
                m_memoryValidationTimer.Stop();
                VerifyMemoryIntegrity();
                m_memoryValidationTimer.Start();
            }
            catch (Exception e)
            {
            }
        }

        private void VerifyMemoryIntegrity()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect(); //Maybe don't need?
            long bytes = GC.GetTotalMemory(true);
            long workingSet64 = Process.GetCurrentProcess().WorkingSet64;
            bool memoryTooHigh = false;

            if (workingSet64 < 1024L * 1024L * 1024L)
            {
                //If the program is using less than 1 GB, ignore the ratio
                return;
            }
            if (workingSet64 > Convert.ToInt64(2.5 * 1024L * 1024L * 1024L))
            {
                //Force restart if using more than 2.5 GB of memory
                memoryTooHigh = true;
            }
            
            double ratio = workingSet64 / (double)bytes;

            if (ratio > 2.6) //2.6 is for bema, but this is too low for general
            {
                //Windows has allocated more memory that the program thinks it's using
                //Prompt the user for restart
                using (RestartPromptDialog dlg = new RestartPromptDialog())
                {
                    if (ratio > 3.5 || memoryTooHigh)
                    {
                        dlg.ForceRestart = true;
                        dlg.ShowDialog();
                        m_restartOnClose = true;
                        TrySendMemoryLog($"Verified Memory: using '{workingSet64}' bytes. Raitio: {ratio}. Client forced to restart.");
                        Close();
                    }
                    else
                    {
                        dlg.ForceRestart = false;
                        dlg.ShowDialog();
                        if (dlg.DialogResult == DialogResult.OK)
                        {
                            m_restartOnClose = true;
                            TrySendMemoryLog($"Verified Memory: using '{workingSet64}' bytes. Raitio: {ratio}. Restarting.");
                            Close();
                        }
                        else if (dlg.DialogResult == DialogResult.Ignore)
                        {
                            TrySendMemoryLog($"Verified Memory: using '{workingSet64}' bytes. Raitio: {ratio}. User prevented restart.");
                            m_memoryValidationTimer.Interval = Math.Min(m_memoryValidationTimer.Interval * 2, Convert.ToInt32(TimeSpan.FromMinutes(30).TotalMilliseconds));
                        }
                    }
                }
            }
        }

        private static void TrySendMemoryLog(string a_message)
        {
            string userName = "Unknown";
            try
            {
                using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, 3000))
                {
                    userName = um.GetById(SystemController.CurrentUserId).GetReadableName();
                }
            }
            catch (AutoTryEnterException)
            {
            }

            UserErrorT errorT = new UserErrorT(SystemController.CurrentUserId, new Exception(a_message), $"Memory validation check for User: {userName}");
            errorT.sequenced = false;
            UIBroadcaster.BroadcastImmediately(errorT);
        }
    #endif
    #endregion

    #region LOCALIZATION
    /// This section manages localizing controls once their handle has been created
    private readonly object m_iLocalizableLock = new ();

    private readonly List<ILocalizable> m_localizableList = new ();
    private bool m_localizeInProcess;

    public void RegisterLocalizableControl(ILocalizable a_control)
    {
        lock (m_iLocalizableLock)
        {
            m_localizableList.Add(a_control);
        }

        //Don't trigger another process if it's already running
        if (!m_localizeInProcess)
        {
            TriggerLocalize();
        }
    }

    /// <summary>
    /// Start a new task to localize all the controls that have registered so far
    /// </summary>
    private async void TriggerLocalize()
    {
        m_localizeInProcess = true;
        List<ILocalizable> controlsToLocalize = null;
        while (true)
        {
            lock (m_iLocalizableLock)
            {
                if (m_localizableList.Count > 0)
                {
                    controlsToLocalize = m_localizableList.ShallowCopy();
                    m_localizableList.Clear();
                }
            }

            if (controlsToLocalize != null && controlsToLocalize.Count > 0)
            {
                await Task.Run(new Action(() => LocalizeControlsWhenHandleCreated(controlsToLocalize)));
                //Keep checking
            }
            else
            {
                //Nothing to localize
                break;
            }
        }

        m_localizeInProcess = false;
    }

    /// <summary>
    /// Localize all ILocalizable and controls that have a handle.
    /// All controls that weren't localized will be re-added to the queue
    /// </summary>
    /// <param name="a_controlsToLocalize"></param>
    private void LocalizeControlsWhenHandleCreated(List<ILocalizable> a_controlsToLocalize)
    {
        //Localize on the main thread
        Invoke(new Action(() =>
        {
            for (int i = a_controlsToLocalize.Count - 1; i >= 0; i--)
            {
                ILocalizable localizable = a_controlsToLocalize[i];
                if (localizable is Control control)
                {
                    if (control.IsHandleCreated)
                    {
                        //Wait until the control is initialized to speed up opening the UI
                        localizable.Localize();
                        a_controlsToLocalize.RemoveAt(i);
                    }
                }
                else
                {
                    localizable.Localize();
                    a_controlsToLocalize.RemoveAt(i);
                }
            }
        }));

        //Any controls not initialized will get added back and we will try again later
        if (a_controlsToLocalize.Count > 0)
        {
            lock (m_iLocalizableLock)
            {
                m_localizableList.AddRange(a_controlsToLocalize);
                Task.Delay(TimeSpan.FromMilliseconds(150)); //Don't retry too quickly
            }
        }
    }
    #endregion

    public void AddPendingChanges(string a_instigatingEntityName, Func<PTTransmission> a_autoSaveDelegate)
    {
        m_autoSaveSettingManager.AddPendingChanges(a_instigatingEntityName, a_autoSaveDelegate);
    }
}