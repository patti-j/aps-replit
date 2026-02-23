using PT.APSCommon;
using PT.PackageDefinitions;

namespace PT.UIDefinitions;

public interface IUINavigationEventManager
{
    void FireNavigationEvent(UINavigationEvent a_navigationEvent);

    /// <summary>
    /// Fire the navigation event on repeat until handled
    /// </summary>
    /// <param name="a_navigationEvent"></param>
    /// <param name="a_handled"></param>
    Task<bool> FireNavigationEventUntilHandled(UINavigationEvent a_navigationEvent);

    /// <summary>
    /// A UI Navigation event has been sent
    /// </summary>
    event Action<UINavigationEvent> UiNavigationEvent;
}

/// <summary>
/// This class represents a structure of available UI navigation actions. For example opening a pane, activating a tile,
/// or specifying objects were selected.
/// </summary>
public class UINavigationEvent
{
    protected readonly string m_key;
    protected readonly Dictionary<string, object> m_data;
    public static readonly string ActivateScenarioKey = "ActivateScenario";
    public static readonly string PrepareLogout = "PrepareLogout";
    public static readonly string HideControlKey = "HideControl";
    public static readonly string DisableControlKey = "DisableControl";
    public static readonly string WatchJobsKey = "WatchJobs";
    public static readonly string PurgeCapacityIntervalsKey = "PurgeCapacityIntervals";
    public static readonly string OpenDebugConsole = "OpenDebugConsole";
    public static readonly string OpenJobDialog = "OpenJobDialog";
    public static readonly string OpenUserPreferencesDialog = "OpenUserPreferencesDialog";
    public static string OpenErrorMessageDialog = "OpenErrorMessageDialog";

    public bool Handled;

    public UINavigationEvent(string a_key)
    {
        m_key = a_key;
        m_data = new Dictionary<string, object>();
    }

    public string Key => m_key;

    public Dictionary<string, object> Data => m_data;
}

public class UIOpenReportEvent : UINavigationEvent
{
    public static readonly string OpenReportForm = "OpenReportForm";
    public static readonly string ReportName = "ReportName";
    public static readonly string JobIds = "JobIds";

    public UIOpenReportEvent(string a_reportName) : base(OpenReportForm)
    {
        m_data.Add(ReportName, a_reportName);
    }
}

/// <summary>
/// Navigation event for handling support e-mails.
/// </summary>
public class UIEmailEvent : UINavigationEvent
{
    public static readonly string EmailMessage = "Message";
    public static readonly string SendSupportEmail = "SendSupportEmail";
    public static readonly string SupportEmailSent = "SupportEmailSent";
    public static readonly string ControlUsed = "ControlUsed";

    public UIEmailEvent(string a_emailKey, string a_controlKey) : base(a_emailKey)
    {
        m_data.Add(ControlUsed, a_controlKey);
    }
}

/// <summary>
/// Navigation event for handling support e-mails.
/// </summary>
public class UINavigationLogoutEvent : UINavigationEvent
{
    public static readonly string LogoutKey = "Logout";
    public static readonly string RestartClientKey = "RestartClient";
    public static readonly string CloseImmediatelyKey = "CloseImmediately";
    public static readonly string UserLoggedOutValueKey = "UserLoggedOut"; //For when the UI needs to close and not prompt anymore

    /// <summary>
    /// UI navigation event to logout of the client
    /// </summary>
    public UINavigationLogoutEvent() : base(LogoutKey) { }
}

/// <summary>
/// Navigation event for handling support e-mails.
/// </summary>
public class UIClosingDialogEvent : UINavigationEvent
{
    public static readonly string CloseDialog = "OpenCloseDialog";
    public static readonly string MessageKey = "Message";
    public static readonly string AllowCancelKey = "AllowCancel";
    public static readonly string AllowExitKey = "AllowExit";
    public static readonly string ExitTimeKey = "ExitTime";

    /// <summary>
    /// UI navigation event to open the UI closing dialog
    /// </summary>
    /// <param name="a_message">Closing message</param>
    /// <param name="a_exitTime">Closing timer time</param>
    /// <param name="a_allowCancel">Show the button to cancel closing</param>
    /// <param name="a_allowExit">Show the button to exit immediately</param>
    public UIClosingDialogEvent(string a_message, short a_exitTime, bool a_allowCancel, bool a_allowExit) : base(CloseDialog)
    {
        m_data.Add(MessageKey, a_message);
        m_data.Add(AllowCancelKey, a_allowCancel);
        m_data.Add(AllowExitKey, a_allowExit);
        m_data.Add(ExitTimeKey, a_exitTime);
    }

    #region Quick Getters
    public string Message
    {
        get
        {
            if (m_data.TryGetValue(MessageKey, out object message))
            {
                return Convert.ToString(message);
            }

            return string.Empty;
        }
    }

    public bool AllowCancel
    {
        get
        {
            if (m_data.TryGetValue(AllowCancelKey, out object allowCancel))
            {
                return Convert.ToBoolean(allowCancel);
            }

            return true;
        }
    }

    public bool AllowExit
    {
        get
        {
            if (m_data.TryGetValue(AllowExitKey, out object allowExit))
            {
                return Convert.ToBoolean(allowExit);
            }

            return true;
        }
    }

    public short ExitTime
    {
        get
        {
            if (m_data.TryGetValue(ExitTimeKey, out object exitTime))
            {
                return Convert.ToInt16(exitTime);
            }

            return 30;
        }
    }
    #endregion
}

/// <summary>
/// Quick navigation event creation for performing a workspace action
/// </summary>
public class UIWorkspaceEvent : UINavigationEvent
{
    public static readonly string SwitchWorkspace = "SwitchWorkspace";
    public static readonly string CopyWorkspace = "CopyWorkspace";
    public static readonly string DeleteWorkspace = "DeleteWorkspace";
    public static readonly string WorkspaceName = "WorkspaceName";
    public static readonly string NewWorkspaceName = "NewWorkspaceName";
    public static readonly string ImportKey = "ImportWorkspace";
    public static readonly string ImportCompleteKey = "ImportCompleteKey";
    public static readonly string ExportCompleteKey = "ExportCompleteKey";
    public static string RenameWorkspace = "RenameWorkspace";
    public static string RenameWorkspaceComplete = "RenameWorkspaceCompleteKey";

    public UIWorkspaceEvent(string a_actionKey, Dictionary<string, SettingData> a_settingsDictionary, string a_workspaceName, bool a_overwrite, bool a_resetOtherSettings, List<BaseId> a_userIds, bool a_applyImmediately) : base(a_actionKey)
    {
        m_data.Add(a_actionKey, (a_settingsDictionary, a_workspaceName, a_overwrite, a_resetOtherSettings, a_userIds, a_applyImmediately));
    }

    public UIWorkspaceEvent(string a_actionKey) : base(a_actionKey) { }
}

/// <summary>
/// Quick navigation event creation for opening a board
/// </summary>
public class ActivateBoardNavigationEvent : UINavigationEvent
{
    public static readonly string OpenBoardKey = "OpenBoard";
    public static readonly string BoardName = "BoardName";
    public static readonly string OpenSettings = "OpenSettings";
    public static readonly string DockNewGroup = "DockStyle";

    /// <summary>
    /// Activate a board. Docking in new group optional
    /// </summary>
    /// <param name="a_boardName"></param>
    /// <param name="a_newGroup"></param>
    public ActivateBoardNavigationEvent(string a_boardName, bool a_newGroup = false) : base(OpenBoardKey)
    {
        m_data.Add(BoardName, a_boardName);
        m_data.Add(DockNewGroup, a_newGroup);
    }

    /// <summary>
    /// Activate a settings board specifying a settings group
    /// </summary>
    /// <param name="a_boardName"></param>
    /// <param name="a_settings"></param>
    public ActivateBoardNavigationEvent(string a_boardName, string a_settings) : base(OpenBoardKey)
    {
        m_data.Add(BoardName, a_boardName);
        m_data.Add(OpenSettings, a_settings);
    }
}

/// <summary>
/// Quick navigation event creation for activating a tile
/// </summary>
public class ActivateTileNavigationEvent : ActivateBoardNavigationEvent
{
    public static readonly string OpenTile = "OpenTile";
    public static readonly string OpenMultipleTiles = "OpenMultipleTiles";

    public ActivateTileNavigationEvent(string a_boardName, string a_tileKey) : base(a_boardName)
    {
        m_data.Add(OpenTile, a_tileKey);
    }

    public ActivateTileNavigationEvent(string a_boardName, string a_tileKey, object a_selectedObjects) : base(a_boardName)
    {
        m_data.Add(OpenTile, a_tileKey);
        m_data.Add("SelectedObjects", a_selectedObjects);
    }

    public ActivateTileNavigationEvent(string a_boardName, List<string> a_tiles) : base(a_boardName)
    {
        m_data.Add(OpenMultipleTiles, a_tiles);
    }
}