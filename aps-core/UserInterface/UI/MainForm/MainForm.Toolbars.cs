using System.Windows.Forms;

using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.APSCommon.Windows;
using PT.Common.Extensions;
using PT.PackageDefinitions;
using PT.PackageDefinitions.Extensions;
using PT.PackageDefinitions.PackageInterfaces;
using PT.PackageDefinitionsUI;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.PackageDefinitionsUI.Packages;
using PT.ScenarioControls;
using PT.Scheduler;
using PT.SchedulerData;
using PT.SchedulerDefinitions.PermissionTemplates;
using PT.UI.Packages;
using PT.UIDefinitions;

namespace PT.UI;

partial class MainForm
{
    private BarItemManager m_barItemManager;

    private async Task LoadMainMenus()
    {
        BarManager.HighlightMode = BarDropdownHighlightMode.Separate;
        m_barItemManager = new BarItemManager(m_barManager_Main, MainFormInstance);
        m_barManager_Main.AllowCustomization = false;
        //Load Menus
        List<IBarMenuElement> mainMenuElements = new ();
        List<IMainBarModule> barModules = m_packageManager.GetMainBarModules();
        using (SystemController.Sys.UsersLock.EnterRead(out UserManager um))
        {
            using (SystemController.Sys.UserManagerEventsLock.EnterRead(out UserManagerEvents ume))
            {
                foreach (IMainBarModule module in barModules)
                {
                    try
                    {
                        mainMenuElements.AddRange(module.GetMainBarMenus(m_scenarioInfo, um, ume));
                    }
                    catch (Exception e)
                    {
                        LogException(new PackageException(module, "Error loading Main Bar Menus"), true);
                    }
                }
            }
        }

        using (BackgroundLock asyncLock = new (m_scenarioInfo.ScenarioId))
        {
            await asyncLock.RunLockCode(this, AddScenarioButtons, mainMenuElements, barModules);
            if (asyncLock.Status == BackgroundLock.EResultStatus.Error)
            {
                m_messageProvider.ShowMessageBox("Failed to load main menus".Localize(), "Error Loading Packages", true, false, this);
                ExitImmediately();
            }
        }
    }

    /// <summary>
    /// Turn scenario menu bar visibility on and off based on scenario permissions
    /// </summary>
    /// This function is no longer used, but maybe it'll be useful again in the future.
    private void EnableScenarioMenu()
    {
        bool enable = m_scenarioInfo.GetCurrentUserEditAccess() == EUserAccess.Edit;
        m_scenarioBar.Visible = enable;
        m_barManager_Main.AllowShowToolbarsPopup = enable;
    }

    private void AddScenarioButtons(Scenario a_s, ScenarioDetail a_sd, ScenarioEvents a_se, params object[] a_params)
    {
        List<IBarMenuElement> mainMenuElements = a_params[0] as List<IBarMenuElement>;
        List<IMainBarModule> barModules = a_params[1] as List<IMainBarModule>;
        Dictionary<string, List<IBarControlElement>> verticalMenuControls = new ();
        Dictionary<string, List<IBarControlElement>> horizontalMenuControls = new ();
        List<IBarControlElement> controlElements = new ();

        List<IBarButtonElement> scenarioBarButtons = new ();
        //Load ScenarioButtons
        foreach (IMainBarModule barModule in barModules)
        {
            try
            {
                scenarioBarButtons.AddRange(barModule.GetBarButtonElements(m_scenarioInfo, a_s, a_sd));
            }
            catch (Exception e)
            {
                LogException(new PackageException(barModule, "Error loading Bar Button Elements. " + e.GetExceptionFullMessage()), true);
            }
        }

        scenarioBarButtons.Sort((x, y) => x.PositionPriority.CompareTo(y.PositionPriority));

        //Get and collect bar controls for each menu item
        foreach (IMainBarModule barModule in barModules)
        {
            try
            {
                controlElements.AddRange(barModule.GetBarMenuControls(m_scenarioInfo, a_sd));
            }
            catch (Exception e)
            {
                LogException(new PackageException(barModule, "Error loading Bar Menu Controls. " + e.GetExceptionFullMessage()), true);
            }
        }

        //Override Core elements if necessary
        controlElements.ProcessOverrides();
        controlElements.Sort((x, y) => x.ElementKey.CompareTo(y.ElementKey));

        foreach (IBarControlElement controlElement in controlElements)
        {
            m_barItemManager.AttachControlRefreshEvent(controlElement);

            if (controlElement.LayoutVertically)
            {
                if (verticalMenuControls.TryGetValue(controlElement.BarItemKey, out List<IBarControlElement> controlList))
                {
                    controlList.Add(controlElement);
                }
                else
                {
                    verticalMenuControls.Add(controlElement.BarItemKey, new List<IBarControlElement> { controlElement });
                }
            }
            else
            {
                if (horizontalMenuControls.TryGetValue(controlElement.BarItemKey, out List<IBarControlElement> controlList))
                {
                    controlList.Add(controlElement);
                }
                else
                {
                    horizontalMenuControls.Add(controlElement.BarItemKey, new List<IBarControlElement> { controlElement });
                }
            }
        }

        mainMenuElements.Sort((x, y) => x.PositionPriority.CompareTo(y.PositionPriority));
        ConvertMenuToBarItems(mainMenuElements, verticalMenuControls);
        AddButtonsToBar(scenarioBarButtons, verticalMenuControls, horizontalMenuControls);
    }

    private void ConvertMenuToBarItems(IEnumerable<IBarMenuElement> a_menuElements, Dictionary<string, List<IBarControlElement>> a_menuControls)
    {
        foreach (IBarMenuElement element in a_menuElements)
        {
            if (element is ICustomDXBarMenuElement dxElement)
            {
                BarButtonItem item = m_barItemManager.GenerateCustomButton(dxElement);
                AddItemToBar(m_mainBar, item);
            }
            else
            {
                if (!a_menuControls.TryGetValue(element.ElementKey, out List<IBarControlElement> controlList))
                {
                    controlList = new List<IBarControlElement>();
                }

                BarButtonItem menuButton;

                if (controlList.Count == 1 && controlList[0].IsLayoutControl)
                {
                    IBarControlElement barControlElement = controlList[0];
                    menuButton = m_barItemManager.GenerateLayoutControlMenu(element, barControlElement.ControlInstance);
                    barControlElement.UpdateButton(menuButton);
                }
                else
                {
                    menuButton = m_barItemManager.GenerateMenu(element, controlList);
                }

                if (menuButton != null)
                {
                    AddItemToBar(m_mainBar, menuButton);
                }
            }
        }
    }

    private void AddButtonsToBar(IEnumerable<IBarButtonElement> a_scenarioBarButtons,
                                 Dictionary<string, List<IBarControlElement>> a_verticalMenuControls,
                                 Dictionary<string, List<IBarControlElement>> a_horizontalMenuControls)
    {
        foreach (IBarButtonElement scenarioBarButton in a_scenarioBarButtons)
        {
            BarButtonItem menuButton;

            if (scenarioBarButton.ButtonStyle == BarButtonStyle.DropDown)
            {
                if (!a_verticalMenuControls.TryGetValue(scenarioBarButton.ElementKey, out List<IBarControlElement> controlList))
                {
                    controlList = new List<IBarControlElement>();
                }

                if (!a_horizontalMenuControls.TryGetValue(scenarioBarButton.ElementKey, out List<IBarControlElement> horizontalControlList))
                {
                    horizontalControlList = new List<IBarControlElement>();
                }

                if (controlList.Count == 1 && controlList[0].IsLayoutControl)
                {
                    menuButton = m_barItemManager.GenerateLayoutControlMenu(scenarioBarButton, controlList[0].ControlInstance);
                }
                else
                {
                    menuButton = m_barItemManager.GenerateButton(scenarioBarButton);
                    controlList.Sort((x, y) => x.PositionPriority.CompareTo(y.PositionPriority));
                    m_barItemManager.AttachPopupControlContainer(menuButton, controlList, horizontalControlList);
                }
            }
            else
            {
                menuButton = m_barItemManager.GenerateButton(scenarioBarButton);
            }

            AddItemToBar(m_scenarioBar, menuButton);
        }
    }

    private void AddItemToBar(Bar a_scenarioBar, BarItem a_barItem)
    {
        m_barManager_Main.Items.Add(a_barItem);
        a_scenarioBar.ItemLinks.Add(a_barItem);
    }

    private const string c_workspaceButtonPrefix = "ptws_";

    private void FireSearchNavigationEvent(object a_sender)
    {
        string searchString = "";

        if (a_sender is TextEdit textEdit)
        {
            searchString = textEdit.Text;
        }

        //searchString = (string)barEditItem_GlobalSearch.EditValue;
        UINavigationEvent newEvent = new ActivateTileNavigationEvent(BoardKeys.ScenarioManagement, TileKeys.SearchTile);

        FireNavigationEvent(newEvent);
    }

    private void barButtonItem_Find_ItemClick(object sender, ItemClickEventArgs e)
    {
        FireSearchNavigationEvent(sender);
    }

    private void GlobalSearchEnterPressedHandler(object a_sender, KeyEventArgs a_e)
    {
        if (a_e.KeyCode == Keys.Enter)
        {
            FireSearchNavigationEvent(a_sender);
        }
    }

    /// <summary>
    /// Awaits a lock on scenario objects and then updates toolbar items.
    /// </summary>
    private void InitializeToolbarItems()
    {
        //TODO: NewUI
        //m_barAndDockingController.AppearancesRibbon.FormCaption.Font = PTAppearance.SelectedFont;
        //m_barAndDockingController.AppearancesRibbon.Item.Font = PTAppearance.LargeFont;
        //m_barAndDockingController.AppearancesRibbon.ItemHovered.Font = PTAppearance.LargeFont;
        //m_barAndDockingController.AppearancesRibbon.ItemPressed.Font = PTAppearance.LargeFont;
        //m_barAndDockingController.AppearancesRibbon.ItemDisabled.Font = PTAppearance.LargeFont;
        //m_barAndDockingController.AppearancesRibbon.PageCategory.Font = PTAppearance.LargeFont;
        //m_barAndDockingController.AppearancesBar.StatusBar.Font = PTAppearance.LargeFont;

        //barEditItem_GlobalSearch.Edit.KeyDown += GlobalSearchEnterPressedHandler;

        //Localize and Style
        foreach (BarItem item in m_barManager_Main.Items)
        {
            item.Caption = item.Caption.Localize(); //string.Format("{0}_{1}", prefix, ribbonTab.Key);
            if (item.SuperTip != null)
            {
                foreach (BaseToolTipItem toolTipItem in item.SuperTip.Items)
                {
                    if (toolTipItem is ToolTipTitleItem)
                    {
                        ((ToolTipTitleItem)toolTipItem).Text = ((ToolTipTitleItem)toolTipItem).Text.Localize();
                    }
                    else if (toolTipItem is ToolTipItem)
                    {
                        ((ToolTipItem)toolTipItem).Text = ((ToolTipItem)toolTipItem).Text.Localize();
                    }
                }
            }
        }
    }

    public async Task OpenConnectionStatusAsync()
    {
        using (AsyncLock asyncLock = new (this, SystemController.CurrentUserId, AsyncLock.ESplashVisibility.AsNeeded, "Loading Optimize Settings...".Localize()))
        {
            await asyncLock.RunLockCode(this, OpenConnectionStatus);
        }
    }

    private void OpenConnectionStatus(UserManager a_um, params object[] a_params)
    {
        User user = a_um.GetById(SystemController.CurrentUserId);
        UserPermissionSet permissionSet = a_um.GetUserPermissionSetById(user.UserPermissionSetId);

        if (permissionSet.AdministerUsers)
        {
            //MainStatusBar.OpenConnectionStatus();
        }
        else
        {
            m_messageProvider.ShowMessageBox(new PTMessage(SystemController.GetServerConnectionDescription(), "Connection Diagnostics".Localize()) { Classification = PTMessage.EMessageClassification.Information }, false, true, this);
        }
    }

    public bool SkipLoadingToolbarSettings;

    public static void Default_StyleChanged(object a_sender, EventArgs a_e)
    {
        switch (DevExpress.LookAndFeel.UserLookAndFeel.Default.SkinName)
        {
            case "Office 2016 Dark":

                break;
        }
    }
}