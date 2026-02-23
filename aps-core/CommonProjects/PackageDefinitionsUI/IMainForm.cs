using System.Windows.Forms;

using PT.Common.Http;
using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.Interfaces;
using PT.Scheduler;
using PT.SystemDefinitions;
using PT.Transmissions.Interfaces;
using PT.UIDefinitions;
using PT.UIDefinitions.Interfaces;

namespace PT.PackageDefinitionsUI;

public interface IMainForm : IExceptionManager, IErrorLogViewManager, IUINavigationEventManager, IAutoSaveSettingsManager
{
    /// <summary>
    /// This Owner form is the top level form of the application
    /// It can be used for Invoking on the main thread, or as a parent Form, for example for a dialog
    /// </summary>
    /// <returns></returns>
    Form GetOwnerForm();

    IWorkspaceInfo WorkspaceInfo { get; }

    /// <summary>
    /// The active Scenario information and events.
    /// </summary>
    IScenarioInfo ScenarioInfo { get; }

    IUserPreferenceInfo UserPreferenceInfo { get; }
    IUsersInfo UsersInfo { get; }
    IMessageProvider MessageProvider { get; }

    /// <summary>
    /// Themes and branding texts.
    /// Use the brands ActiveTheme for all color references
    /// </summary>
    IBrand CurrentBrand { get; }

    /// <summary>
    /// The application is closing, you can use this value to check if events can be ignored
    /// </summary>
    bool ClosingSoIgnoreAllEvents { get; }

    /// <summary>
    /// Closes top level popup controls.
    /// Use this for closing menu items after an button or other UI item has been clicked.
    /// </summary>
    void CloseMenus();

    void ChangeTheme(string a_newTheme, bool a_initializing);

    /// <summary>
    /// Logged in instance info. Can be used for basic Instance data like name and version
    /// </summary>
    LoggedInInstanceInfo InstanceInfo { get; }

    decimal CurrentDpiScaler { get; }

    WorkingDirectory WorkingDirectory { get; }

    ISettingsManager SystemSettings { get; }

    /// <summary>
    /// This reference can be used to send transmissions to the server or call APIs
    /// </summary>
    IClientSession ClientSession { get; }

    /// <summary>
    /// Monitors the state and timing of the underlying http client connection
    /// This is used for tracking latency or other API errors.
    /// </summary>
    IConnectionStateManager ConnectionStateManager { get; }
}