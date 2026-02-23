using PT.APSCommon.Extensions;
using PT.Common.IO;
using PT.PackageDefinitions;
using PT.Scheduler;
using PT.SchedulerDefinitions;

public class PTCorePreferences : ICloneable, ISettingData, ISetBoolsInitializer
{
    #region IPTSerializable
    public PTCorePreferences(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12516)
        {
            m_boolVector1 = new BoolVector32(a_reader);
            m_boolVector2 = new BoolVector32(a_reader);
            a_reader.Read(out m_skinName);
            a_reader.Read(out m_closeDelay);
            a_reader.Read(out int value);
            m_boardTabsMode = (PackageEnums.EBoardTabsMode)value;
            a_reader.Read(out m_lastThemeLoaded);
            a_reader.Read(out m_numberOfKpiTiles);
            a_reader.Read(out m_autoSaveWorkspaceSuffix);
            a_reader.Read(out value);
            m_publishReminder = (UserDefs.publishOnExitReminders)value;
            a_reader.Read(out m_timeZoneId);
            a_reader.Read(out m_backupWorkspaceFileName);
            a_reader.Read(out m_exportDirectoryId);
        }
        else if (a_reader.VersionNumber >= 12317)
        {
            m_boolVector1 = new BoolVector32(a_reader);
            m_boolVector2 = new BoolVector32(a_reader);
            a_reader.Read(out m_skinName);
            a_reader.Read(out m_closeDelay);
            a_reader.Read(out int value);
            m_boardTabsMode = (PackageEnums.EBoardTabsMode)value;
            a_reader.Read(out m_lastThemeLoaded);
            a_reader.Read(out m_numberOfKpiTiles);
            a_reader.Read(out m_autoSaveWorkspaceSuffix);
            a_reader.Read(out value);
            m_publishReminder = (UserDefs.publishOnExitReminders)value;
            a_reader.Read(out m_timeZoneId);
            a_reader.Read(out m_backupWorkspaceFileName);
        }
        else if (a_reader.VersionNumber >= 12031)
        {
            m_boolVector1 = new BoolVector32(a_reader);
            m_boolVector2 = new BoolVector32(a_reader);
            a_reader.Read(out m_skinName);
            a_reader.Read(out m_closeDelay);
            a_reader.Read(out int value);
            m_boardTabsMode = (PackageEnums.EBoardTabsMode)value;
            a_reader.Read(out m_lastThemeLoaded);
            a_reader.Read(out m_numberOfKpiTiles);
            a_reader.Read(out m_autoSaveWorkspaceSuffix);
            a_reader.Read(out value);
            m_publishReminder = (UserDefs.publishOnExitReminders)value;
            a_reader.Read(out m_timeZoneId);
        }
        else
        {
            if (a_reader.VersionNumber >= 12017)
            {
                m_boolVector1 = new BoolVector32(a_reader);
                m_boolVector2 = new BoolVector32(a_reader);
                a_reader.Read(out m_skinName);
                a_reader.Read(out m_closeDelay);
                a_reader.Read(out int value);
                m_boardTabsMode = (PackageEnums.EBoardTabsMode)value;
                a_reader.Read(out m_lastThemeLoaded);
                a_reader.Read(out m_numberOfKpiTiles);
                a_reader.Read(out m_autoSaveWorkspaceSuffix);
                a_reader.Read(out value);
                m_publishReminder = (UserDefs.publishOnExitReminders)value;
                a_reader.Read(out m_timeZoneId);
            }
            else
            {
                m_timeZoneId = TimeZoneInfo.Local.Id;
                if (a_reader.VersionNumber >= 12014)
                {
                    m_boolVector1 = new BoolVector32(a_reader);
                    m_boolVector2 = new BoolVector32(a_reader);
                    a_reader.Read(out m_skinName);
                    a_reader.Read(out m_closeDelay);
                    a_reader.Read(out int value);
                    m_boardTabsMode = (PackageEnums.EBoardTabsMode)value;
                    a_reader.Read(out m_lastThemeLoaded);
                    a_reader.Read(out m_numberOfKpiTiles);
                    a_reader.Read(out value);
                    m_publishReminder = (UserDefs.publishOnExitReminders)value;
                }
                else if (a_reader.VersionNumber >= 12013)
                {
                    m_boolVector1 = new BoolVector32(a_reader);
                    m_boolVector2 = new BoolVector32(a_reader);
                    a_reader.Read(out m_skinName);
                    a_reader.Read(out m_closeDelay);
                    a_reader.Read(out int value);
                    m_boardTabsMode = (PackageEnums.EBoardTabsMode)value;
                    a_reader.Read(out m_lastThemeLoaded);
                    a_reader.Read(out m_numberOfKpiTiles);
                }
                else if (a_reader.VersionNumber >= 12004)
                {
                    m_boolVector1 = new BoolVector32(a_reader);
                    a_reader.Read(out m_skinName);
                    a_reader.Read(out m_closeDelay);
                    a_reader.Read(out int value);
                    m_boardTabsMode = (PackageEnums.EBoardTabsMode)value;
                    a_reader.Read(out m_lastThemeLoaded);
                    a_reader.Read(out m_numberOfKpiTiles);
                }
                else if (a_reader.VersionNumber >= 11000)
                {
                    m_boolVector1 = new BoolVector32(a_reader);
                    a_reader.Read(out m_skinName);
                    a_reader.Read(out m_closeDelay);
                    a_reader.Read(out int value);
                    m_boardTabsMode = (PackageEnums.EBoardTabsMode)value;
                    a_reader.Read(out m_lastThemeLoaded);
                }
                else if (a_reader.VersionNumber >= 656)
                {
                    m_boolVector1 = new BoolVector32(a_reader);
                    a_reader.Read(out m_skinName);
                }
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_boolVector1.Serialize(a_writer);
        m_boolVector2.Serialize(a_writer);
        a_writer.Write(m_skinName);
        a_writer.Write(m_closeDelay);
        a_writer.Write((int)m_boardTabsMode);
        a_writer.Write(m_lastThemeLoaded);
        a_writer.Write(m_numberOfKpiTiles);
        a_writer.Write(m_autoSaveWorkspaceSuffix);
        a_writer.Write((int)m_publishReminder);
        a_writer.Write(m_timeZoneId);
        a_writer.Write(m_backupWorkspaceFileName);
        a_writer.Write(m_exportDirectoryId);
    }

    public int UniqueId => UNIQUE_ID;

    public const int UNIQUE_ID = 824;
    #endregion

    public PTCorePreferences()
    {
        //Default
        //TODO: These defaults should use the boolvector references so we don't set the IsSet properties.
        HideInactiveAlerts = true;
        ConfirmDelete = true;
        EnableQuickActions = false;
        UseScenarioOptimizeSettings = true;
        UseScenarioCompressSettings = true;
        ShowFullLastRefreshDate = true;
        AutoBackupWorkspace = true;
        UseAutoSaveWorkspaceSuffix = false;
        AutoSaveWorkspaceSuffix = "_auto".Localize();
        ExportAutoSaveWorkspace = false;
        m_timeZoneId = TimeZoneInfo.Local.Id;
        AutoSaveUserWorkspaceSettings = true;
        PublishReminder = UserDefs.publishOnExitReminders.AskUser;
        BackupWorkspaceFileName = PTSystem.WorkingDirectory.DefaultWorkspaceBackup;
        GridReloadOnEditMode = true;
    }

    #region Bools 1
    private BoolVector32 m_boolVector1;
    private const short c_confirmDelete = 0;
    private const short c_confirmDeleteSet = 1;
    private const short c_hideInactiveAlertsIdx = 2;
    private const short c_hideInactiveAlertsSetIdx = 3;
    private const short c_alwaysShowMoveCursorIdx = 4;
    private const short c_alwaysShowMoveCursorSetIdx = 5;
    private const short c_showOtherUserActionMessagesIdx = 6;
    private const short c_showOtherUserActionMessagesSetIdx = 7;
    private const short c_useScenarioOptimizeSettingsIdx = 8;
    private const short c_useScenarioOptimizeSettingsSetIdx = 9;
    private const short c_useScenarioCompressSettingsIdx = 10;
    private const short c_useScenarioCompressSettingsSetIdx = 11;
    private const short c_showAdvancedUserSettingsIdx = 12;

    private const short c_showAdvancedUserSettingsSetIdx = 13;

    //private const short c_showCompleteMessageForOptimizeIdx = 14; //Removed 12.0.29, part of notification slides
    //private const short c_showCompleteMessageForOptimizeSetIdx = 15; //Removed 12.0.29
    //private const short c_showCompleteMessageForImportIdx = 16; //Removed 12.0.29
    //private const short c_showCompleteMessageForImportSetIdx = 17; //Removed 12.0.29
    private const short c_useDaylightSavingAdjustmentIdx = 18;
    private const short c_useDaylightSavingAdjustmentSetIdx = 19;
    private const short c_showPaneHeadersIdx = 20;
    private const short c_showPaneHeadersIsSetIdx = 21;
    private const short c_enableQuickActionsIdx = 22;
    private const short c_enableQuickActionsIsSetIdx = 23;
    private const short c_boardTabsModeIsSetIdx = 24;
    private const short c_lastThemeLoadedIsSetIdx = 25;
    private const short c_prependSetupOnMoveIdx = 26;
    private const short c_prependSetupOnMoveSetIdx = 27;
    private const short c_showFullLastRefreshDateIdx = 28;
    private const short c_showFullLastRefreshDateIsSetIdx = 29;
    private const short c_photoSensitivityModeIdx = 30;
    private const short c_photoSensitivityModeIsSetIdx = 31;
    //This bool vector is full!!

    public bool ConfirmDelete
    {
        get => m_boolVector1[c_confirmDelete];
        set
        {
            m_boolVector1[c_confirmDelete] = value;
            m_boolVector1[c_confirmDeleteSet] = true;
        }
    }

    public bool ConfirmDeleteSet => m_boolVector1[c_confirmDeleteSet];

    public bool HideInactiveAlerts
    {
        get => m_boolVector1[c_hideInactiveAlertsIdx];
        set
        {
            m_boolVector1[c_hideInactiveAlertsIdx] = value;
            m_boolVector1[c_hideInactiveAlertsSetIdx] = true;
        }
    }

    public bool HideInactiveAlertsSet => m_boolVector1[c_hideInactiveAlertsSetIdx];

    /// <summary>
    /// Whether to show the multi move cursor even when dragging only a single block
    /// </summary>
    public bool AlwaysShowMoveCursor
    {
        get => m_boolVector1[c_alwaysShowMoveCursorIdx];
        set
        {
            m_boolVector1[c_alwaysShowMoveCursorIdx] = value;
            m_boolVector1[c_alwaysShowMoveCursorSetIdx] = true;
        }
    }

    public bool AlwaysShowMoveCursorSet => m_boolVector1[c_alwaysShowMoveCursorSetIdx];

    public bool ShowOtherUserActionMessages
    {
        get => m_boolVector1[c_showOtherUserActionMessagesIdx];
        set
        {
            m_boolVector1[c_showOtherUserActionMessagesIdx] = value;
            m_boolVector1[c_showOtherUserActionMessagesSetIdx] = true;
        }
    }

    public bool ShowOtherUserActionMessagesSet => m_boolVector1[c_showOtherUserActionMessagesSetIdx];

    /// <summary>
    /// Whether the User is using the shared Scenario Optimize Settings as opposed to his own settings for performing Optimizations.
    /// Same as UseSharedScenarioOptimizeSettings - If True will trigger use of system setttings, if False will trigger use of user's setttings
    /// </summary>
    public bool UseScenarioOptimizeSettings
    {
        get => m_boolVector1[c_useScenarioOptimizeSettingsIdx];
        set
        {
            m_boolVector1[c_useScenarioOptimizeSettingsIdx] = value;
            m_boolVector1[c_useScenarioOptimizeSettingsSetIdx] = true;
        }
    }

    public bool UseScenarioOptimizeSettingsSet => m_boolVector1[c_useScenarioOptimizeSettingsSetIdx];

    /// <summary>
    /// Whether the User is using the shared Scenario Compress Settings or his own settings for performing Optimizations.
    /// Same as UseSharedScenarioCompressSettings - If True will trigger use of system setttings, if False will trigger use of user's setttings
    /// </summary>
    public bool UseScenarioCompressSettings
    {
        get => m_boolVector1[c_useScenarioCompressSettingsIdx];
        set
        {
            m_boolVector1[c_useScenarioCompressSettingsIdx] = value;
            m_boolVector1[c_useScenarioCompressSettingsSetIdx] = true;
        }
    }

    public bool UseScenarioCompressSettingsSet => m_boolVector1[c_useScenarioCompressSettingsSetIdx];

    public bool ShowAdvancedUserSettings
    {
        get => m_boolVector1[c_showAdvancedUserSettingsIdx];
        set
        {
            m_boolVector1[c_showAdvancedUserSettingsIdx] = value;
            m_boolVector1[c_showAdvancedUserSettingsSetIdx] = true;
        }
    }

    public bool ShowAdvancedUserSettingsSet => m_boolVector1[c_showAdvancedUserSettingsSetIdx];

    public bool ShowPaneHeaders
    {
        get => m_boolVector1[c_showPaneHeadersIdx];
        set => m_boolVector1[c_showPaneHeadersIdx] = value;
    }

    public bool ShowShowPaneHeadersSet
    {
        get => m_boolVector1[c_showPaneHeadersIsSetIdx];
        private set => m_boolVector1[c_showPaneHeadersIsSetIdx] = value;
    }

    public bool EnableQuickActions
    {
        get => m_boolVector1[c_enableQuickActionsIdx];
        set => m_boolVector1[c_enableQuickActionsIdx] = value;
    }

    public bool EnableQuickActionsSet
    {
        get => m_boolVector1[c_enableQuickActionsIsSetIdx];
        private set => m_boolVector1[c_enableQuickActionsIsSetIdx] = value;
    }

    private PackageEnums.EBoardTabsMode m_boardTabsMode = PackageEnums.EBoardTabsMode.None;

    public PackageEnums.EBoardTabsMode BoardTabsMode
    {
        get => m_boardTabsMode;
        set
        {
            m_boardTabsMode = value;
            BoardTabsModeIsSet = true;
        }
    }

    public bool BoardTabsModeIsSet
    {
        get => m_boolVector1[c_boardTabsModeIsSetIdx];
        set => m_boolVector1[c_boardTabsModeIsSetIdx] = value;
    }

    private string m_lastThemeLoaded;

    public string LastThemeLoaded
    {
        get => m_lastThemeLoaded;
        set
        {
            m_lastThemeLoaded = value;
            LastThemeLoadedIsSet = true;
        }
    }

    public bool LastThemeLoadedIsSet
    {
        get => m_boolVector1[c_lastThemeLoadedIsSetIdx];
        set => m_boolVector1[c_lastThemeLoadedIsSetIdx] = value;
    }

    /// <summary>
    /// Whether to Attempt to prevent a block from ending later by prepending any new
    /// setup added to it when the block to its left is moved.
    /// </summary>
    public bool PrependSetupOnMove
    {
        get => m_boolVector1[c_prependSetupOnMoveIdx];
        set
        {
            m_boolVector1[c_prependSetupOnMoveIdx] = value;
            m_boolVector1[c_prependSetupOnMoveSetIdx] = true;
        }
    }

    public bool PrependSetupOnMoveSet => m_boolVector1[c_prependSetupOnMoveSetIdx];

    public bool ShowFullLastRefreshDate
    {
        get => m_boolVector1[c_showFullLastRefreshDateIdx];
        set
        {
            m_boolVector1[c_showFullLastRefreshDateIdx] = value;
            m_boolVector1[c_showFullLastRefreshDateIsSetIdx] = true;
        }
    }

    public bool ShowFullLastRefreshDateSet => m_boolVector1[c_showFullLastRefreshDateIsSetIdx];

    public bool PhotoSensitivityMode
    {
        get => m_boolVector1[c_photoSensitivityModeIdx];
        set
        {
            m_boolVector1[c_photoSensitivityModeIdx] = value;
            m_boolVector1[c_photoSensitivityModeIsSetIdx] = true;
        }
    }

    public bool PhotoSensitivityModeIsSet => m_boolVector1[c_photoSensitivityModeIsSetIdx];

    public bool UseDaylightSavingAdjustment
    {
        get => m_boolVector1[c_useDaylightSavingAdjustmentIdx];
        set
        {
            m_boolVector1[c_useDaylightSavingAdjustmentIdx] = value;
            m_boolVector1[c_useDaylightSavingAdjustmentSetIdx] = true;
        }
    }

    public bool UseDaylightSavingAdjustmentIsSet => m_boolVector1[c_useDaylightSavingAdjustmentSetIdx];
    #endregion

    #region Bools 2
    private BoolVector32 m_boolVector2;
    private const short c_autoSaveWorkspaceIsSetIdx = 0;
    private const short c_autoSaveWorkspaceSuffixIsSetIdx = 1;
    private const short c_exportAutoSaveWorkspaceIdx = 2;
    private const short c_exportAutoSaveWorkspaceIsSetIdx = 3;
    private const short c_publishReminderIsSetIdx = 4;
    private const short c_useAutoSaveWorkspaceSuffixIdx = 5;
    private const short c_useAutoSaveWorkspaceSuffixIsSetIdx = 6;
    private const short c_useAutoSaveUserAndSystemSettingsIdx = 7;
    private const short c_useAutoSaveUserAndSystemSettingsIsSetIdx = 8;
    private const short c_timeZoneIsSet = 9;
    private const short c_autoSaveWorkspaceIdx = 10;
    private const short c_syncActivityGridLayoutsIdx = 11;
    private const short c_syncActivityGridLayoutsIsSetIdx = 12;
    private const short c_undoRedoPopupConfirmationIdx = 13;
    private const short c_undoRedoPopupConfirmationIsSetIdx = 14;
    private const short c_undoIndividualActionsIdx = 15;
    private const short c_undoIndividualActionsIsSetIdx = 16;
    private const short c_backupWorkspaceFileNameIsSet = 17;
    private const short c_exportDirectoryPathIsSet = 18;
    private const short c_reloadOnEditModeIdx= 19;
    private const short c_reloadOnEditModeIsSet = 20;
    private const short c_integrationStudioSQLPathIsSet = 21;

    public bool AutoBackupWorkspace
    {
        get => m_boolVector2[c_autoSaveWorkspaceIdx];
        set
        {
            m_boolVector2[c_autoSaveWorkspaceIdx] = value;
            m_boolVector2[c_autoSaveWorkspaceIsSetIdx] = true;
        }
    }

    public bool AutoSaveWorkspaceSet => m_boolVector2[c_autoSaveWorkspaceIsSetIdx];

    private string m_backupWorkspaceFileName = string.Empty;

    public string BackupWorkspaceFileName
    {
        get => m_backupWorkspaceFileName;
        set
        {
            m_backupWorkspaceFileName = value;
            m_boolVector2[c_backupWorkspaceFileNameIsSet] = true;
        }
    }

    private string m_autoSaveWorkspaceSuffix;

    public string AutoSaveWorkspaceSuffix
    {
        get => m_autoSaveWorkspaceSuffix;
        set
        {
            m_autoSaveWorkspaceSuffix = value;
            m_boolVector2[c_autoSaveWorkspaceSuffixIsSetIdx] = true;
        }
    }

    public bool AutoSaveWorkspaceSuffixSet => m_boolVector2[c_autoSaveWorkspaceSuffixIsSetIdx];

    public bool UseAutoSaveWorkspaceSuffix
    {
        get => m_boolVector2[c_useAutoSaveWorkspaceSuffixIdx];
        set
        {
            m_boolVector2[c_useAutoSaveWorkspaceSuffixIdx] = value;
            m_boolVector2[c_useAutoSaveWorkspaceSuffixIsSetIdx] = true;
        }
    }

    public bool UseAutoSaveWorkspaceSuffixIsSet => m_boolVector2[c_useAutoSaveWorkspaceSuffixIsSetIdx];

    public bool ExportAutoSaveWorkspace
    {
        get => m_boolVector2[c_exportAutoSaveWorkspaceIdx];
        set
        {
            m_boolVector2[c_exportAutoSaveWorkspaceIdx] = value;
            m_boolVector2[c_exportAutoSaveWorkspaceIsSetIdx] = true;
        }
    }

    public bool ExportAutoSaveWorkspaceSet => m_boolVector2[c_exportAutoSaveWorkspaceIsSetIdx];

    public bool PublishReminderIsSet
    {
        get => m_boolVector2[c_publishReminderIsSetIdx];
        private set => m_boolVector2[c_publishReminderIsSetIdx] = value;
    }

    public bool AutoSaveUserAndSystemSettingsIsSet => m_boolVector2[c_useAutoSaveUserAndSystemSettingsIsSetIdx];

    public bool AutoSaveUserWorkspaceSettings
    {
        get => m_boolVector2[c_useAutoSaveUserAndSystemSettingsIdx];
        set
        {
            m_boolVector2[c_useAutoSaveUserAndSystemSettingsIdx] = value;
            m_boolVector2[c_useAutoSaveUserAndSystemSettingsIsSetIdx] = true;
        }
    }

    public bool SynchActivityGridLayouts
    {
        get => m_boolVector2[c_syncActivityGridLayoutsIdx];
        set
        {
            m_boolVector2[c_syncActivityGridLayoutsIdx] = value;
            m_boolVector2[c_syncActivityGridLayoutsIsSetIdx] = true;
        }
    }

    public bool SynchActivityGridLayoutsIsSet => m_boolVector2[c_syncActivityGridLayoutsIsSetIdx];

    public bool UndoRedoConfirmation
    {
        get => m_boolVector2[c_undoRedoPopupConfirmationIdx];
        set
        {
            m_boolVector2[c_undoRedoPopupConfirmationIdx] = value;
            m_boolVector2[c_undoRedoPopupConfirmationIsSetIdx] = true;
        }
    }

    public bool UndoRedoConfirmationIsSet => m_boolVector2[c_undoRedoPopupConfirmationIsSetIdx];

    public bool UndoIndividualActions
    {
        get => m_boolVector2[c_undoIndividualActionsIdx];
        set
        {
            m_boolVector2[c_undoIndividualActionsIdx] = value;
            m_boolVector2[c_undoIndividualActionsIsSetIdx] = true;
        }
    }

    public bool UndoIndividualActionsIsSet => m_boolVector2[c_undoIndividualActionsIsSetIdx];
    /// <summary>
    /// Flag to indicate if Grid Data sources are allowed to reload while in edit mode
    /// </summary>
    public bool GridReloadOnEditMode
    {
        get => m_boolVector2[c_reloadOnEditModeIdx];
        set
        {
            m_boolVector2[c_reloadOnEditModeIdx] = value;
            m_boolVector2[c_reloadOnEditModeIsSet] = true;
        }
    }

    public bool GridReloadOnEditModeIsSet => m_boolVector2[c_reloadOnEditModeIsSet];
    #endregion

    private string m_skinName;

    public string SkinName
    {
        get => m_skinName;
        set => m_skinName = value;
    }

    private TimeSpan m_closeDelay = TimeSpan.FromSeconds(5);

    public TimeSpan CloseDelay
    {
        get => m_closeDelay;
        set => m_closeDelay = value;
    }

    private int m_numberOfKpiTiles = 4;

    public int NumberOfKpiTiles
    {
        get => m_numberOfKpiTiles;
        set => m_numberOfKpiTiles = value;
    }

    private string m_timeZoneId;

    public string TimeZone
    {
        get => m_timeZoneId;
        set
        {
            m_timeZoneId = value;
            m_boolVector2[c_timeZoneIsSet] = true;
        }
    }

    public bool TimeZoneIsSet => m_boolVector2[c_timeZoneIsSet];

    private UserDefs.publishOnExitReminders m_publishReminder;

    /// <summary>
    /// Controls whether the User should be reminded when exiting to Publish the schedule if it has changed since the last Publish.
    /// This value does not apply unless the user is a Master Scheduler.
    /// </summary>
    public UserDefs.publishOnExitReminders PublishReminder
    {
        get => m_publishReminder;
        set
        {
            m_publishReminder = value;
            PublishReminderIsSet = true;
        }
    }

    private string m_exportDirectoryId;

    public string ExportDirectoryPath
    {
        get => m_exportDirectoryId;
        set
        {
            m_exportDirectoryId = value;
            m_boolVector2[c_exportDirectoryPathIsSet] = true;
        }
    }

    public bool ExportDirectoryPathIsSet => m_boolVector2[c_exportDirectoryPathIsSet];

    private string m_integrationStudioSQLId;

    public string IntegrationStudioSQLPath
    {
        get => m_integrationStudioSQLId;
        set
        {
            m_integrationStudioSQLId = value;
            m_boolVector2[c_integrationStudioSQLPathIsSet] = true;
        }
    }

    public bool IntegrationStudioSQLPathIsSet => m_boolVector2[c_integrationStudioSQLPathIsSet];

    public void Update(PTCorePreferences a_newPreferences)
    {
        if (a_newPreferences.ConfirmDeleteSet)
        {
            ConfirmDelete = a_newPreferences.ConfirmDelete;
        }

        if (a_newPreferences.HideInactiveAlertsSet)
        {
            HideInactiveAlerts = a_newPreferences.HideInactiveAlerts;
        }

        if (a_newPreferences.AlwaysShowMoveCursorSet)
        {
            AlwaysShowMoveCursor = a_newPreferences.AlwaysShowMoveCursor;
        }

        if (a_newPreferences.ShowOtherUserActionMessagesSet)
        {
            ShowOtherUserActionMessages = a_newPreferences.ShowOtherUserActionMessages;
        }

        if (a_newPreferences.UseScenarioOptimizeSettingsSet)
        {
            UseScenarioOptimizeSettings = a_newPreferences.UseScenarioOptimizeSettings;
        }

        if (a_newPreferences.UseScenarioCompressSettingsSet)
        {
            UseScenarioCompressSettings = a_newPreferences.UseScenarioCompressSettings;
        }

        if (a_newPreferences.ShowAdvancedUserSettingsSet)
        {
            ShowAdvancedUserSettings = a_newPreferences.ShowAdvancedUserSettings;
        }

        if (a_newPreferences.ShowShowPaneHeadersSet)
        {
            ShowPaneHeaders = a_newPreferences.ShowPaneHeaders;
        }

        if (a_newPreferences.EnableQuickActionsSet)
        {
            EnableQuickActions = a_newPreferences.EnableQuickActions;
        }

        if (a_newPreferences.BoardTabsModeIsSet)
        {
            BoardTabsMode = a_newPreferences.BoardTabsMode;
        }

        if (a_newPreferences.LastThemeLoadedIsSet)
        {
            LastThemeLoaded = a_newPreferences.LastThemeLoaded;
        }

        if (a_newPreferences.PrependSetupOnMoveSet)
        {
            PrependSetupOnMove = a_newPreferences.PrependSetupOnMove;
        }

        if (a_newPreferences.ShowFullLastRefreshDateSet)
        {
            ShowFullLastRefreshDate = a_newPreferences.ShowFullLastRefreshDate;
        }

        if (a_newPreferences.PhotoSensitivityModeIsSet)
        {
            PhotoSensitivityMode = a_newPreferences.PhotoSensitivityMode;
        }

        if (a_newPreferences.AutoSaveWorkspaceSet)
        {
            AutoBackupWorkspace = a_newPreferences.AutoBackupWorkspace;
        }

        if (a_newPreferences.AutoSaveWorkspaceSuffixSet)
        {
            AutoSaveWorkspaceSuffix = a_newPreferences.AutoSaveWorkspaceSuffix;
        }

        if (a_newPreferences.ExportAutoSaveWorkspaceSet)
        {
            ExportAutoSaveWorkspace = a_newPreferences.ExportAutoSaveWorkspace;
        }

        if (a_newPreferences.PublishReminderIsSet)
        {
            m_publishReminder = a_newPreferences.PublishReminder;
        }

        if (a_newPreferences.UseAutoSaveWorkspaceSuffixIsSet)
        {
            UseAutoSaveWorkspaceSuffix = a_newPreferences.UseAutoSaveWorkspaceSuffix;
        }

        if (a_newPreferences.AutoSaveUserAndSystemSettingsIsSet)
        {
            AutoSaveUserWorkspaceSettings = a_newPreferences.AutoSaveUserWorkspaceSettings;
        }

        if (a_newPreferences.TimeZoneIsSet)
        {
            TimeZone = a_newPreferences.TimeZone;
        }

        if (a_newPreferences.UseDaylightSavingAdjustmentIsSet)
        {
            UseDaylightSavingAdjustment = a_newPreferences.UseDaylightSavingAdjustment;
        }

        if (a_newPreferences.SynchActivityGridLayoutsIsSet)
        {
            SynchActivityGridLayouts = a_newPreferences.SynchActivityGridLayouts;
        }

        if (a_newPreferences.UndoRedoConfirmationIsSet)
        {
            UndoRedoConfirmation = a_newPreferences.UndoRedoConfirmation;
        }

        if (a_newPreferences.UndoIndividualActionsIsSet)
        {
            UndoIndividualActions = a_newPreferences.UndoIndividualActions;
        }

        if (a_newPreferences.ExportDirectoryPathIsSet)
        {
            ExportDirectoryPath = a_newPreferences.ExportDirectoryPath;
        }

        if (a_newPreferences.GridReloadOnEditModeIsSet)
        {
            GridReloadOnEditMode = a_newPreferences.GridReloadOnEditMode;
        }

        if (a_newPreferences.IntegrationStudioSQLPathIsSet)
        {
            IntegrationStudioSQLPath = a_newPreferences.IntegrationStudioSQLPath;
        }
    }

    public object Clone()
    {
        PTCorePreferences clone = new ();
        clone.m_boolVector1 = new BoolVector32(m_boolVector1);
        clone.m_boolVector2 = new BoolVector32(m_boolVector2);
        clone.CloseDelay = CloseDelay;
        clone.AutoSaveWorkspaceSuffix = AutoSaveWorkspaceSuffix;
        clone.m_publishReminder = PublishReminder;
        return clone;
    }

    public string SettingKey => Key;
    public static string Key => "UserPreferences_PTCorePreferences";
    public string Description => "User Preferences";
    public string SettingsGroup => SettingGroupConstants.PersonalSettingsGroup;
    public string SettingsGroupCategory => "User Preferences";
    public string SettingCaption => "Preferences";

    public void InitializeSetBools()
    {
        //TODO: Clear all of the IsSet indexes in both bool vectors
        m_boolVector1[c_photoSensitivityModeIsSetIdx] = false;

        m_boolVector2[c_autoSaveWorkspaceIsSetIdx] = false;
    }
}