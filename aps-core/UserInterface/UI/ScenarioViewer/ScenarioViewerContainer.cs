using System.Windows.Forms;

using PT.Common.Localization;
using PT.ComponentLibrary;
using PT.ComponentLibrary.Extensions;
using PT.PackageDefinitions;
using PT.PackageDefinitionsUI;
using PT.PackageDefinitionsUI.Packages;
using PT.PackageDefinitionsUI.UserSettings;
using PT.Scheduler;
using PT.UIDefinitions;

namespace PT.UI.ScenarioViewer;

/// <summary>
/// Holds a single Scenario Viewer.  The purpose of this control is to add the Scenario Status bar beneath the docking setup of the ScenarioViewer
/// and hold the alerts manager control.
/// </summary>
public partial class ScenarioViewerContainer : PTBaseControl, ILocalizable
{
    private readonly IPackageManagerUI m_packageManager;
    private readonly ScenarioViewer m_scenarioViewer;
    private readonly IMainForm m_mainForm;
    private readonly IScenarioInfo m_scenarioInfo;
    private NotificationsSettings m_notificationSettings = new ();
    private bool m_initialized;
    /// <summary>
    /// Flag to indicate the state of the notification panel is fixed to prevent it from automatically getting shown/hidden
    /// <remarks>This was introduced to prevent Grids from redrawing while users are actively editing and action occurs requiring a notification slide to be displayed</remarks>>
    /// </summary>
    private bool m_panelFixed;

    /// <summary>
    /// For designer, DO NOT USE.
    /// </summary>
    public ScenarioViewerContainer()
    {
        if (RuntimeStatus.IsRuntime)
        {
            throw new NotImplementedException();
        }

        InitializeComponent();
    }

    public override void Localize()
    {
        UILocalizationHelper.LocalizeControlsRecursively(Controls);
    }

    public ScenarioViewer ScenarioViewer => m_scenarioViewer;

    public ScenarioViewerContainer(IMainForm a_mainForm, IScenarioInfo a_scenarioInfo, IPackageManagerUI a_packageManager)
    {
        m_mainForm = a_mainForm;
        m_mainForm.WorkspaceInfo.SettingSavedEvent += WorkspaceInfoOnSettingSavedEvent;
        m_scenarioInfo = a_scenarioInfo;
        m_packageManager = a_packageManager;
        InitializeComponent();

        if (!IsHandleCreated)
        {
            CreateHandle();
        }

        a_scenarioInfo.ScenarioActivated += ScenarioInfoOnScenarioActivated;
        m_mainForm.UiNavigationEvent += UINavigationEvent;

        m_scenarioViewer = new ScenarioViewer(a_mainForm, m_scenarioInfo, m_packageManager);
        m_scenarioViewer.Localize();

        panel_flyoutDocking.Visible = false;

        simpleButton_Show.SetImage("open");
        simpleButton_Hide.SetImage("close");
        simpleButton_NotificationSettings.SetImage("settingsGears");
    }

    private void UINavigationEvent(UINavigationEvent a_navigation)
    {
        if (a_navigation.Key == "FixedNotificationKey")
        {
            if (a_navigation.Data.TryGetValue("editMode", out object editMode))
            {
                m_panelFixed = (bool)editMode;
            }

            if (m_notificationSettings.NotificationsBarOpenType == NotificationsSettings.ENotificationsBarOpenType.OpenAutomatically)
            {
                if (m_panelFixed && !panelControl_NotificationDock.Visible)
                {
                    panelControl_NotificationDock.Show();
                }
            }
        }
    }

    private void WorkspaceInfoOnSettingSavedEvent(ISettingsManager a_settingsManager, string a_settingsKey)
    {
        if (a_settingsKey == m_notificationSettings.SettingKey)
        {
            LoadUserSettings();
        }
    }

    private void ScenarioInfoOnScenarioActivated(Scenario a_arg1, ScenarioDetail a_arg2, ScenarioEvents a_arg3)
    {
        OpenScenario();
    }

    private void OpenScenario()
    {
        if (!m_initialized)
        {
            m_initialized = true;
            LoadUserSettings();
            m_scenarioViewerPanel.Controls.Add(m_scenarioViewer);
            m_scenarioViewer.Dock = DockStyle.Fill;

            List<INotificationModule> modules = m_packageManager.GetNotificationModules();

            stackPanel_NotificationSlides.SuspendLayout();

            foreach (INotificationModule module in modules)
            {
                List<INotificationElement> notificationElements = module.GetNotificationElements(m_scenarioInfo);
                foreach (INotificationElement element in notificationElements)
                {
                    AddSlide(element);
                }
            }

            stackPanel_NotificationSlides.ResumeLayout();

            if (m_notificationSettings.HideSimBarAutomatically)
            {
                HideBarIfNoSlidesAreVisible();
            }
        }

        //TODO: Events
        //PT.Common.Testing.Timing timer = new PT.Common.Testing.Timing(true, "ScenarioTabs.InitPanelIfNecessary - " + m_scenarioInfo.ScenarioId);
        //ScenarioViewer.ScenarioDesynchronized += new ScenarioViewer.ScenarioDesynchronizeDelegate(sv_ScenarioDesynchronized);
    }

    private void LoadUserSettings()
    {
        m_notificationSettings = m_mainForm.WorkspaceInfo.LoadSetting(m_notificationSettings);
    }

    public void AddSlide(INotificationElement a_element)
    {
        a_element.RemoveNotification += ElementOnRemoveNotification;
        a_element.ShowNotification += ElementOnShowNotification;

        Control notificationTile = a_element.NotificationSlide;
        notificationTile.Tag = a_element.ElementKey;
        notificationTile.Visible = false;

        stackPanel_NotificationSlides.Controls.Add(a_element.NotificationSlide);
    }

    private void ElementOnShowNotification(INotificationElement a_notificationElement, bool a_showNotificationsBar)
    {
        ToggleNotificationVisibility(a_notificationElement, true);

        if (!panelControl_NotificationDock.Visible)
        {
            switch (m_notificationSettings.NotificationsBarOpenType)
            {
                case NotificationsSettings.ENotificationsBarOpenType.OpenAutomatically:
                    if (a_showNotificationsBar && !m_panelFixed)
                    {
                        stackPanel_NotificationSlides.SuspendLayout();
                        panelControl_NotificationDock.Show();
                        stackPanel_NotificationSlides.ResumeLayout();
                    }

                    break;
                case NotificationsSettings.ENotificationsBarOpenType.OpenWithFlyoutButton:
                    panel_flyoutDocking.Visible = true;
                    break;
            }
        }
    }

    private void ElementOnRemoveNotification(INotificationElement a_notificationElement)
    {
        ToggleNotificationVisibility(a_notificationElement, false);

        if (m_notificationSettings.HideSimBarAutomatically)
        {
            HideBarIfNoSlidesAreVisible();
        }
    }

    private void HideBarIfNoSlidesAreVisible()
    {
        if (m_panelFixed)
        {
            return;
        }

        foreach (Control control in stackPanel_NotificationSlides.Controls)
        {
            if (control.Visible)
            {
                return;
            }
        }

        panelControl_NotificationDock.Hide();
    }

    private void ToggleNotificationVisibility(INotificationElement a_notificationElement, bool a_visible)
    {
        stackPanel_NotificationSlides.SuspendLayout();

        foreach (Control control in stackPanel_NotificationSlides.Controls)
        {
            if ((string)control.Tag == a_notificationElement.ElementKey)
            {
                if (a_visible)
                {
                    control.Visible = true;
                    control.BringToFront();

                    //Make sure that the queued action slide is always the leftmost element
                    if ((string)control.Tag != NotificationElementKeys.QueuedActionsNotificationElementKey &&  GetNotificationControlByKey(NotificationElementKeys.QueuedActionsNotificationElementKey) is Control queuedActionsElement && queuedActionsElement.Visible)
                    {
                        queuedActionsElement.BringToFront();
                    }
                }
                else
                {
                    control.Visible = false;
                }
            }
        }

        stackPanel_NotificationSlides.ResumeLayout();
    }

    private Control GetNotificationControlByKey(string a_key)
    {
        foreach (Control control in stackPanel_NotificationSlides.Controls)
        {
            if ((string)control.Tag == a_key)
            {
                return control;
            }
        }

        return null;
    }

    private void simpleButton_NotificationSettings_Click(object sender, EventArgs a_e)
    {
        UINavigationEvent uiEvent = new ActivateBoardNavigationEvent(BoardKeys.WorkspaceSettings, SettingGroupConstants.Notifications);
        m_mainForm.FireNavigationEvent(uiEvent);
    }

    private void simpleButton_Hide_Click(object sender, EventArgs e)
    {
        panelControl_NotificationDock.Hide();
    }

    private void simpleButton_Show_Click(object sender, EventArgs e)
    {
        panelControl_NotificationDock.Show();
        panel_flyoutDocking.Visible = false;
    }

    public void Unload()
    {
        m_scenarioViewer.Unload();
    }

    internal void ShowScenarioOverlay(string a_message)
    {
        m_scenarioViewer.ShowScenarioOverlay(a_message);
    }

    internal void HideScenarioOverlay()
    {
        m_scenarioViewer.HideScenarioOverlay();
    }
}