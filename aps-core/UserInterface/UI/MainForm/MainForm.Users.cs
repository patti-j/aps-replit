using System.Collections;
using System.Windows.Forms;

using DevExpress.XtraBars;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.APSCommon.Serialization;
using PT.APSCommon.Windows;
using PT.Common.Debugging;
using PT.Common.Localization;
using PT.ComponentLibrary;
using PT.ComponentLibrary.Forms;
using PT.PackageDefinitions;
using PT.PackageDefinitionsUI;
using PT.PackageDefinitionsUI.Controls;
using PT.PackageDefinitionsUI.Controls.BaseControls;
using PT.PackageDefinitionsUI.Packages.UserWorkspaces;
using PT.PackageDefinitionsUI.UserSettings;
using PT.Scheduler;
using PT.SchedulerData;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.PermissionTemplates;
using PT.SchedulerExtensions.User;
using PT.Transmissions;
using PT.Transmissions.User;
using PT.UI.Managers;
using PT.UI.Packages;
using PT.UI.UserSettings;
using PT.UI.Utilities;
using PT.UIDefinitions;
//TODO: User Collaboration Controls
//using PT.ScenarioControls.Dialogs.Chat;
using Timer = System.Windows.Forms.Timer;

namespace PT.UI;

partial class MainForm
{
    private BarSubItem r_workspacePopupMenu => m_barManager_Main.Items["barSubItem_Workspaces"] as BarSubItem;

    /// <summary>
    /// This is used so that default settings are not saved before the user saved settings are loaded.
    /// This could happen when loading and a tab open/activated event is fired.
    /// </summary>
    private bool m_initialUserSettingsLoaded;
    //        private bool m_ignoreToolClicks;

    //Load Toolbar file if previously saved.
    private async Task LoadUserLayoutSettings()
    {
        PT.Common.Testing.Timing timer = new (true, "MainForm.LoadUserLayoutSettings");

        m_initialUserSettingsLoaded = true;

        // DON'T -- removes zoom editors on load for some reason.saver.LoadLayout(this.ganttViewer.ToolbarManager, "GanttViewer");
        //            m_ignoreToolClicks = false;

        await AfterLayoutLoaded();
        Timing.Log(timer);
    }

    // work that should override layout settings such as applying security and undo/redo's enabled state
    private async Task AfterLayoutLoaded()
    {
        // Apply LicenseKey limitations. Calling it here so they're not overriden by layout definitions.
        await Task.Run(() => EnforceReadonly(PTSystem.ReadOnly));
        //m_security.Enforce();
    }

    private void VisualFactoryTrigger(string a_workspaceName, DateTime a_nextWorkspaceSwitch)
    {
        //TODO: NewUI
        //m_mainStatusBar.UpdateVisualFactoryStatus(a_workspaceName, a_nextWorkspaceSwitch);
        WorkspaceSwitch(a_workspaceName);
    }

    private void WorkspaceSwitch(string a_workspaceName)
    {
        using (new MultiLevelHourglass())
        {
            m_workspaceController.SwitchWorkspace(a_workspaceName);
            LoadUserLayoutSettings();
        }
    }

    #region User Settings
    private UserPreferenceInfo m_userPreferences;

    public IUserPreferenceInfo UserPreferenceInfo => m_userPreferences;

    /// <summary>
    /// Load user settings that only need to be loaded once and are not part of workspaces
    /// </summary>
    private void LoadUserSettings()
    {
        PT.Common.Testing.Timing timer = new (true, "MainForm.LoadUserSettings");

        using (SystemController.Sys.UsersLock.EnterRead(out UserManager um))
        {
            User user = um.GetById(SystemController.CurrentUserId);
            bool needToSyncUserOptimizeSettings = false;
            GenericSettingSaver settingSaver;
            if (user.UserPreferenceInfo == null)
            {
                settingSaver = new GenericSettingSaver();
                needToSyncUserOptimizeSettings = true;
            }
            else
            {
                using (BinaryMemoryReader reader = new (user.UserPreferenceInfo))
                {
                    needToSyncUserOptimizeSettings = reader.VersionNumber < 12031;
                    settingSaver = new GenericSettingSaver(reader);
                }
            }
            m_userPreferences = new UserPreferenceInfo(m_packageManager, settingSaver);

            if (needToSyncUserOptimizeSettings)
            {
                OptimizeSettingsPresets newOptimizeSettingsPresets = new ();
                newOptimizeSettingsPresets.AddOptimizePreset(OptimizeSettingsPresets.DefaultPresetKey, user.OptimizeSettings.Clone());
                newOptimizeSettingsPresets.AddCompressPreset(OptimizeSettingsPresets.DefaultPresetKey, user.CompressSettings.Clone());

                m_userPreferences.SaveSetting(newOptimizeSettingsPresets);
            }

            //Initialize Time Zone
            TimeZoneInfo tzi;
            if (CommandLineArguments.TimeZoneOffset.ArgumentFound)
            {
                int.TryParse(CommandLineArguments.TimeZoneOffset.Value, out int offSet);
                tzi = TimeZoneAdjuster.GetTimeZoneInfoBasedOnOffset(offSet);
            }
            else
            {
                PTCorePreferences preference = m_userPreferences.LoadSetting(new PTCorePreferences());
                tzi = TimeZoneInfo.FindSystemTimeZoneById(preference.TimeZone);
                TimeZoneAdjuster.SetUseDaylightSavingAdjustment(preference.UseDaylightSavingAdjustment);
            }

            TimeZoneAdjuster.SetTimeZoneInfo(tzi);

            m_usersInfo = new UsersContext(this);
            m_autoSaveSettingManager = new PTAutoSaveSettingManager(this);

            //Load Position
            MainFormLayout layout = m_userPreferences.LoadSetting<MainFormLayout>(MainFormLayout.Key);
            StartPosition = FormStartPosition.Manual;
            BaseResizableForm.LoadFormLayout(this, layout.Layout, true);

            SkipLoadingToolbarSettings = user.UserChangedLanguage;
            Localizer.SetLocalizedLanguage(user.DisplayLanguage);
        }

        BaseResizableForm.SetSettingsManager(WorkspaceInfo);
        BaseResizableForm.SetDefaultParentForm(this);
        Timing.Log(timer);
    }

    /// <summary>
    /// This function stores user settings locally to the client so that they can be transfered to another scenario
    /// The actually data will be serialized on scenario copy or system shutdown.
    /// </summary>
    private async Task StoreAllUserSettings(bool a_sendToServer)
    {
        PT.Common.Testing.Timing timer = new (true, "MainForm.StoreUserSettingsLocallyOnly");

        if (m_initialUserSettingsLoaded)
        {
            //Store various user settings
            m_workspaceController.SaveActiveWorkspace();

            if (a_sendToServer)
            {
                //Save MainForm Position
                FormLayout currentLayout = new ("MainForm");
                BaseResizableForm.Update(currentLayout, this);
                m_userPreferences.SaveSetting(new MainFormLayout(currentLayout));

                //This serializes all user settings to files. All settings must be updated before this is called or they will not be written.
                Dictionary<string, byte[]> userWorkspaces = m_workspaceController.SerializeWorkspaces();
                //Send various user settings
                UserSettingsChangeT settingsT = new (SystemController.CurrentUserId);
                settingsT.UserWorkspaces = userWorkspaces;
                settingsT.CurrentWorkspaceName = m_workspaceInfo.WorkspaceName;
                settingsT.UserPreferences = m_userPreferences.SerializeSettings();

                settingsT.UserChangedLanguage = m_userChangedLanguage;
                settingsT.SkinName = DevExpress.LookAndFeel.UserLookAndFeel.Default.SkinName;

                m_clientSession.SendClientAction(settingsT, true);

                if (CommandLineArguments.ClientType == CommandLineArgumentsHelper.EClientType.Internal)
                {
                    await m_clientSession.WaitForTransmissionReceive(settingsT.TransmissionId);
                    await Task.Delay(TimeSpan.FromMilliseconds(500)); //Wait for UM to process the transmission
                }
            }
        }


        Timing.Log(timer);
    }

    public void Localize()
    {
        Invoke(new Action(() => SplashManager.UpdateSplashDescription("Localizing...")));
        //Don't need to localize the caption 
        UILocalizationHelper.LocalizeControlsRecursively(Controls);
    }
    #endregion

    internal void DefaultUser(User u, UserManager um, UserDefaultT t)
    {
        //try
        //{
        //    NewUser(u, t.Instigator);
        //}
        //catch (Exception e)
        //{
        //    UnhandledExceptionHandler(e);
        //}
    }

    internal void CopyUser(User u, UserManager um, UserCopyT t)
    {
        try
        {
            NewUser(u, t.Instigator);
        }
        catch (Exception e)
        {
            UnhandledException(e);
        }
    }

    internal void NewUser(User a_u, BaseId a_instigator)
    {
        //TableManager.AddTableRow((BaseObject)a_u);

        ////Make this the active row and put the name cell in edit mode so it's easy to see and rename.
        //if (a_instigator == SystemController.CurrentUserId && FormValid(m_usersDialog))
        //{
        //    m_usersDialog.ScrollToBottom(true); //for other objects use this: dConfigurator1.SelectCurrentGridLastRow();
        //}

        ////Update User ValueLists in grids.
        //RefreshUndoUserValueLists();
    }

    internal async void UpdateUsers(IScenarioDataChanges a_userChanges)
    {
        if (a_userChanges.HasChanges)
        {
            using (BackgroundLock asyncLock = new (BaseId.NULL_ID))
            {
                await asyncLock.RunLockCode(this, UpdateUsers, a_userChanges);
            }
        }
    }

    private void UpdateUsers(UserManager a_um, params object[] a_params)
    {
        if (a_params[0] is IScenarioDataChanges changes && changes.UserChanges.HasChanges)
        {
            foreach (BaseId baseId in changes.UserChanges.Updated)
            {
                //Only update languages for the current user
                if (SystemController.CurrentUserId == baseId)
                {
                    User user = a_um.GetById(baseId);

                    //Need to reset the language and localizer for the new language
                    Localizer.SetLocalizedLanguage(user.DisplayLanguage);
                    m_userChangedLanguage = true; // TODO: this shouldn't be needed anymore due to a localization change
                }
            }

            foreach (BaseId baseId in changes.UserChanges.Deleted)
            {
                if (baseId == SystemController.CurrentUserId) //Action was performed on current user.
                {
                    m_messageProvider.ShowMessageBox(new PTMessage("The active User has been deleted so the application will exit", "Exiting") { Classification = PTMessage.EMessageClassification.Error }, true);
                    s_userDeleted = true;
                    s_closeImmediately = true;
                    Close();
                }
            }
        }
    }

    internal async void UserPermissionsUpdated()
    {
        using (BackgroundLock asyncLock = new (BaseId.NULL_ID))
        {
            await asyncLock.RunLockCodeBackground(UserPermissionsUpdateAsync);
            if (asyncLock.Status is BackgroundLock.EResultStatus.Error or BackgroundLock.EResultStatus.Canceled) { }
        }
    }

    private void UserPermissionsUpdateAsync(ScenarioManager a_sm, UserManager a_um, params object[] a_params)
    {
        User user = a_um.GetById(SystemController.CurrentUserId);

        UserPermissionSet permissionSets = a_um.GetUserPermissionSetById(user.UserPermissionSetId);
        PlantPermissionSet plantPermissionSet = a_um.GetPlantPermissionSetById(user.PlantPermissionsId);
        Scenario scenario = a_sm.Find(m_scenarioInfo.ScenarioId);
        using (scenario.ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
        {
            UpdateViewerPermissions(permissionSets, plantPermissionSet, sd);
        }
    }

    [Obsolete("Listeners should be converted to IScenarioDataChanges")]
    internal void UpdateUsers(UserManager um, PTTransmission a_t, ArrayList addedUsers, ArrayList changedUsers, ArrayList deletedUsers)
    {
        //try
        //{
        //    if (a_t is UserT u && u.RestartUser)
        //    {
        //        foreach (UserT.User user in u.Nodes)
        //        {
        //            if (user.Id == SystemController.CurrentUserId)
        //            {
        //                using (ClosingDialog dlg = new ClosingDialog("Another user has made a settings change that requires a restart.  You will restart in 10 seconds.", false, true))
        //                {
        //                    dlg.TotalTime = TimeSpan.FromSeconds(10);
        //                    DialogResult result = dlg.ShowDialog();
        //                }

        //                m_restartOnClose = true;
        //                s_closeImmediately = true;
        //                Close();
        //                return;
        //            }
        //        }
        //    }

        //    for (int i = 0; i < changedUsers.Count; i++)
        //    {
        //        UpdateUsersHelper(um, (User)changedUsers[i]);
        //    }

        //    for (int i = 0; i < deletedUsers.Count; i++)
        //    {
        //        UpdateUsersHelper(um, (User)deletedUsers[i]);
        //    }

        //    //TODO: V12
        //    //m_usersDialog?.ScrollToBottom(false); //This is to be sure there's an active row after delete all.

        //    //TODO: V12
        //    m_scenarioContainer.ScenarioViewer.OptimizeSettingsChangedHandler();

        //    //Update User ValueLists in grids.
        //    //RefreshUndoUserValueLists(um);
        //}
        //catch (Exception e)
        //{
        //    UnhandledExceptionHandler(e);
        //}
    }

    internal void UserAdminLogOff(User a_u, UserAdminLogOffT a_t)
    {
        if (a_t.Contains(SystemController.CurrentUserId))
        {
            s_userLoggedOut = true;
            UIClosingDialogEvent closeDialog = new ("You have been signed out by an administrator.", 10, false, true);
            FireNavigationEvent(closeDialog);
        }
    }

    private bool m_userChangedLanguage;

    //TODO: User Collaboration Controls
    /// <summary>
    /// Reference to the collaboration form.  Check to be sure it's valid with FormValid() before accessing.
    /// </summary>
    //private static CollaborationForm CollaborationForm
    //{
    //    //TODO: V12 chat
    //    get
    //    {
    //        if (!FormValid(s_collaborationForm))
    //        {
    //            //CollaborationForm = new CollaborationForm();
    //            //s_collaborationForm.CollaborationCtrl.Populate();
    //        }

    //        return s_collaborationForm;
    //    }
    //    set => s_collaborationForm = value;
    //}

    //internal static void ShowCollaborationFormDialog()
    //{
    //    CollaborationForm.Show();
    //}
    private void LoadWorkspaceTemplateFromOlderVersion(string a_defaultWorkspaceName)
    {
        //TODO: Is this implemented in v12?
//#if DEBUG
//            GridControl.OverrideLayoutSaveAutomatically = true;
//#endif
//            try
//            {
//                s_layoutSaver.ApplyTemplate(a_defaultWorkspaceName);
//                return;
//            }
//            catch (Exception)
//            {
//                //Keep going
//            }

//            try
//            {
//                DialogResult result = m_messageProvider.ShowMessageBox(new PTMessage($"Default workspace '{a_defaultWorkspaceName}' could not be loaded. Would you like to find and use a template form an older version?", "Default workspace not found") { Classification = PTMessage.EMessageClassification.Question }, true, this);
//                if (result == DialogResult.Yes)
//                {
//                    //Attempt to copy template files from an older version
//                    System.IO.DirectoryInfo workingDir = new System.IO.DirectoryInfo(ClientWorkingFolderFlag.WorkingFolderPath);
//                    DirectoryInfo root = workingDir.Parent.Parent;
//                    DirectoryInfo[] directoryInfos = root.GetDirectories("*.*.*.*");
//                    System.Collections.Generic.List<SoftwareVersion> versionsList = new System.Collections.Generic.List<SoftwareVersion>();
//                    foreach (DirectoryInfo directoryInfo in directoryInfos)
//                    {
//                        try
//                        {
//                            //Must have a usersettings folder
//                            DirectoryInfo[] infos = directoryInfo.GetDirectories(workingDir.Name);
//                            if (infos.Length == 1)
//                            {
//                                DirectoryInfo[] usersettingsDir = infos[0].GetDirectories(a_defaultWorkspaceName, SearchOption.AllDirectories);
//                                if (usersettingsDir.Length == 1)
//                                {
//                                    SoftwareVersion version = new SoftwareVersion(directoryInfo.Name);
//                                    versionsList.Add(version);
//                                }
//                            }
//                        }
//                        catch (Exception)
//                        {
//                        }
//                    }

//                    //sort versions to get the most recent previous version
//                    if (versionsList.Count == 0)
//                    {
//                        m_messageProvider.ShowMessageBox(new PTMessage($"There were no older versions to copy from! '{a_defaultWorkspaceName}' could not be loaded.", "Older versions not found") { Classification = PTMessage.EMessageClassification.Information }, true, this);
//                    }
//                    else
//                    {
//                        bool versionFound = false;
//                        versionsList.Sort(SoftwareVersion.CompareVersionStringsInt);
//                        SoftwareVersion currentVersion = PTAssemblyVersionChecker.GetServerProductVersion();
//                        for (var i = versionsList.Count - 1; i >= 0; i--)
//                        {
//                            SoftwareVersion version = versionsList[i];

//                            if (version.CompareTo(currentVersion) < 0)
//                            {
//                                //Older version
//                                PT.Common.File.FileUtils.DeleteDirectoryRecursivelyWithRetry(Path.Combine(ClientWorkingFolderFlag.WorkingFolderPath, "UserSettings"));
//                                PT.Common.Directory.DirectoryUtils.CopyDirectory(Path.Combine(root.FullName, version.ToSimpleVersion().ToString(), workingDir.Name, "UserSettings"), Path.Combine(workingDir.Parent.FullName, workingDir.Name, "UserSettings"), true);
//                                m_messageProvider.ShowMessageBox(new PTMessage($"Loaded workspace '{a_defaultWorkspaceName}' from version '{version.ToSimpleVersion()}.", "Template loaded not found") { Classification = PTMessage.EMessageClassification.Information }, true, this);
//                                versionFound = true;
//                                break;
//                            }
//                        }

//                        if (!versionFound)
//                        {
//                            m_messageProvider.ShowMessageBox(new PTMessage($"There were no older versions to copy from! '{a_defaultWorkspaceName}' could not be loaded.", "Older versions not found") { Classification = PTMessage.EMessageClassification.Information }, true, this);
//                        }
//                    }
//                }

//                try
//                {
//                    s_layoutSaver.ApplyTemplate(a_defaultWorkspaceName);
//                }
//                catch (Exception e)
//                {
//                    m_messageProvider.ShowMessageBox(new PTMessage($"Failed to load older workspace. An older version was found but there was an error loading the workspace template file.", "Error") { Classification = PTMessage.EMessageClassification.Information }, true, this);
//                }
//            }
//            catch (Exception e)
//            {
//                m_messageProvider.ShowMessageBox(new PTMessage($"Failed to load older workspace. Something went wrong with the debug code. Check LoadWorkspaceTemplateFromOlderVersion().", "Error") { Classification = PTMessage.EMessageClassification.Information }, true, this);
//            }
    }

    #region AutoLogout
    private Timer m_activeUserTimer;
    private TimeSpan m_autoLogoutTimespan = TimeSpan.Zero;

    internal void SetupActiveUserTimer()
    {
        m_activeUserTimer = new Timer();
        m_activeUserTimer.Interval = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
        m_activeUserTimer.Tick += CheckLastActions;
        m_activeUserTimer.Start();
    }

    private void UpdateAutoLogoutTimespan(TimeSpan a_timeout)
    {
        if (a_timeout >= TimeSpan.Zero)
        {
            m_autoLogoutTimespan = a_timeout;
        }
        else
        {
            m_autoLogoutTimespan = TimeSpan.Zero;
        }
    }

    internal bool UserIsActive { get; private set; } = true;

    private void CheckLastActions(object sender, EventArgs e)
    {
        try
        {
            m_activeUserTimer.Stop();
            DateTime timeOfLastAction = PTDateTime.Max(PTBaseControl.LastAction, PTStyledForm.LastAction);
            if (m_connectionStateManager.LastUserBroadcast != null)
            {
                //Check control, style, and broadcaster LastAction variables to see which is the latest action.  Use that to compare to m_autoLogoutTimespan
                timeOfLastAction = PTDateTime.Max(timeOfLastAction, m_connectionStateManager.LastUserBroadcast.Start);
            }

            TimeSpan timeSinceLastAction = DateTime.UtcNow - timeOfLastAction;
            UserIsActive = timeSinceLastAction < TimeSpan.FromMinutes(10);
            if (!s_userLoggedOut && m_autoLogoutTimespan > TimeSpan.Zero && timeSinceLastAction > m_autoLogoutTimespan)
            {
                UIClosingDialogEvent closeDialog = new ("You are being signed out for inactivity.  Press cancel to keep working.", 60, true, false);
                FireNavigationEvent(closeDialog);
            }

            //Backup Workspace
            m_workspaceController.SaveActiveWorkspace();

            //TODO: Get the auto-save time from a user settings. It current is hardcoded to be 5 mins
            if (m_workspaceInfo.LastSaveDate < DateTime.UtcNow - TimeSpan.FromMinutes(5) + TimeSpan.FromMilliseconds(m_activeUserTimer.Interval))
            {
                //Auto-save the workspace if enabled
                PTCorePreferences corePreferences = m_workspaceInfo.LoadSetting(new PTCorePreferences());
                if (corePreferences.AutoBackupWorkspace)
                {
                    string workspaceName = WorkspaceInfo.WorkspaceName;
                    WorkspaceManager workspaceManager = (WorkspaceManager)m_workspaceInfo;
                    if (corePreferences.UseAutoSaveWorkspaceSuffix)
                    {
                        workspaceName += corePreferences.AutoSaveWorkspaceSuffix;
                    }

                    //Send backup to the server.
                    workspaceManager.BackupActiveWorkspace(workspaceName);
                    workspaceManager.BackupActiveWorkspaceToDisk(corePreferences.BackupWorkspaceFileName);
                }
            }
        }
        finally
        {
            m_activeUserTimer.Start();
        }
    }
    #endregion

    internal void ShowSupportEmailDialog(UserManager a_um, params object[] a_params)
    {
        if (a_params.Length == 2 && a_params[0] is string emailMessage && a_params[1] is string parentControlKey)
        {
            User user = a_um.GetById(SystemController.CurrentUserId);

            if (user == null)
            {
                return;
            }

            SupportEmailDialog emailDialog = new (parentControlKey, user.GetReadableName(), emailMessage, this);
            emailDialog.Show();
        }
    }

    #region Updater
    //private static void RunClientUpdater()
    //{
    //    //TODO: V12 do we want to support client updater?
    //    //if (!CommandLineArguments.SkipClientUpdater.ArgumentFound)
    //    //{
    //    //    SetSplashIfVisible("Updating Client...");
    //    //    try
    //    //    {
    //    //        ClientUpdaterFileManager = new Managers.ClientUpdaterFileRetriever(LoggedInInstance.ClientUpdaterServiceUrl);
    //    //        ClientUpdaterFileManager.RetrieveFiles(ClientWorkingFolderFlag.WorkingFolderPath);
    //    //    }
    //    //    catch (Exception e)
    //    //    {
    //    //        LogException(e);
    //    //        //PT.UIDefinitions.MultiLevelHourglass.TurnOff(true);
    //    //        //Suppressing this message for trial users and because the user cannot do anything about this error. 
    //    //        //MessageBox.Show(PT.APSCommon.Localization.Localizer.GetErrorString("2802", new object[] { e.Message }), "Error retrieving Client Updater files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    //    //        //PT.UIDefinitions.MultiLevelHourglass.TurnOn(); //turn back on since it will get scenarios now
    //    //    }
    //    //}
    //}

    private void RunClientUpdater()
    {
        if (!CommandLineArguments.SkipClientUpdater.ArgumentFound)
        {
            try
            {
                SplashManager.UpdateSplashDescription("Updating Client...".Localize());

                //Create the client updater manager so that it retrieves the files from the server
                ClientUpdaterFileRetriever clientUpdaterFileRetriever = new (LoggedInInstance.InstanceName, LoggedInInstance.InstanceVersion, LoggedInInstance.SystemServiceUrl);
                clientUpdaterFileRetriever.RetrieveFiles(ClientWorkingFolderFlag.WorkingFolderPath);
            }
            catch (Exception e)
            {
                LogException(e);
            }
        }
    }
    #endregion
}