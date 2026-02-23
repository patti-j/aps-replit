using System.Drawing;
using System.Windows.Forms;

using DevExpress.Utils;
using DevExpress.Utils.Menu;
using DevExpress.XtraBars.Docking2010.Views;
using DevExpress.XtraBars.Docking2010.Views.Tabbed;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.APSCommon.Windows;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.Localization;
using PT.ComponentLibrary;
using PT.PackageDefinitions;
using PT.PackageDefinitionsUI;
using PT.PackageDefinitionsUI.Controls;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.PackageDefinitionsUI.Packages;
using PT.ScenarioControls.PackageHelpers;
using PT.Scheduler;
using PT.SchedulerData;
using PT.UIDefinitions;

namespace PT.UI.ScenarioViewer;

public partial class ScenarioViewer : PTBaseControl
{
    #region Declarations
    private readonly TabbedView m_tabbedView;
    private readonly IPackageManagerUI m_packageManager;
    private readonly IScenarioInfo m_scenarioInfo;
    private readonly IMainForm m_mainForm;
    private readonly Dictionary<string, IBoardControl> m_boardControls = new ();
    private readonly List<IBoardControl> m_openBoards = new ();
    private static readonly object s_boardLock = new ();
    private readonly OverlayForm m_scenarioOverlayForm;
    //TODO lite client: OverlayForm might need to be reworked to accommodate multiple scenarios being desynced at the same time
    #endregion

    internal ScenarioViewer(IMainForm a_mainForm, IScenarioInfo a_scenarioInfo, IPackageManagerUI a_packageManager)
    {
        PT.Common.Testing.Timing timer = new (true, "ScenarioViewer.ScenarioViewer");

        m_mainForm = a_mainForm;
        // UserSettingsManager can be obtained from m_mainForm.WorkspaceInfo if we ever need it
        m_packageManager = a_packageManager;
        m_scenarioInfo = a_scenarioInfo;
        InitializeComponent();

        if (!IsHandleCreated)
        {
            CreateHandle();
        }

        m_scenarioOverlayForm = new OverlayForm(this, m_mainForm.CurrentBrand.ActiveTheme, string.Empty);
        m_scenarioOverlayForm.Cancelled += ScenarioOverlayFormOnCancelled;

        //Load styles for design mode
        m_tabbedView = documentManager1.ViewCollection[0] as TabbedView;
        m_tabbedView.DocumentGroupProperties.ShowDocumentSelectorButton = false;
        m_tabbedView.DocumentActivated += TabbedViewOnDocumentActivated;
        m_tabbedView.DocumentClosing += new DocumentCancelEventHandler(m_tabbedView_DocumentClosed);
        m_tabbedView.BeginFloating += new DocumentCancelEventHandler(m_tabbedView_BeginFloating);
        m_tabbedView.Floating += new DocumentEventHandler(m_tabbedView_Floating);
        m_tabbedView.EndDocking += new DocumentEventHandler(m_tabbedView_DocumentDocked);

        m_listener = new ScenarioViewerListener(this, m_mainForm);
        m_tabbedView.PopupMenuShowing += PopupMenuShowing;
        //m_featureController.FeatureChangedEvent += EnforcePermissions;

        //Initialize gantt

        Timing.Log(timer);

        m_scenarioInfo.ScenarioClosed += CloseScenario;
        m_scenarioInfo.ScenarioActivated += ActivateScenario;
        m_mainForm.UiNavigationEvent += NavigationEventHandler;
        m_mainForm.WorkspaceInfo.CollectUnsavedSettings += WorkspaceInfoOnCollectUnsavedSettings;
        m_mainForm.WorkspaceInfo.WorkspaceActivated += WorkspaceInfoOnWorkspaceActivated;
        //m_tabbedView.Appearance.BackColor = PTAppearance.ScenarioViewerBackgroundColor;
    }

    private Document m_rightClickedDocument;

    private void PopupMenuShowing(object sender, PopupMenuShowingEventArgs a_e)
    {
        // Disabling and changing some text for the default context menu items
        DXMenuItem dxMenuItem = a_e.Menu.Find(BaseViewControllerCommand.ShowWindowsDialog);
        if (dxMenuItem != null)
        {
            dxMenuItem.Visible = false;
        }

        dxMenuItem = a_e.Menu.Find(BaseViewControllerCommand.CloseAll);
        if (dxMenuItem != null)
        {
            dxMenuItem.Caption = "Close All Boards".Localize();
        }

        a_e.Menu.Remove(BaseViewControllerCommand.CloseAllButThis);
        m_rightClickedDocument = (Document)a_e.GetDocument();
        if (m_rightClickedDocument != null)
        {
            a_e.Menu.Items.Insert(1, new DXMenuItem("Close All But This".Localize(), CloseAllButThisClicked));
        }
    }

    private void CloseAllButThisClicked(object sender, EventArgs a_e)
    {
        foreach (Document document in m_tabbedView.Documents)
        {
            if (document != m_rightClickedDocument && document.Parent == m_rightClickedDocument.Parent)
            {
                document.Control.Visible = false;
                IBoardControl board = m_openBoards.FirstOrDefault(x => x.DisplayName == document.Caption);

                if (board != null)
                {
                    m_openBoards.Remove(board);
                }
            }
        }

        UpdateAllowFloat();
        HandleActiveBoardsAndTabs(m_rightClickedDocument.Parent);
    }

    private void WorkspaceInfoOnCollectUnsavedSettings(WorkspaceSettingsCollector a_collector)
    {
        if (m_intializingBoards)
        {
            return;
        }

        if (SaveUserSettings())
        {
            a_collector.SaveSetting(new SettingData(m_dockManagerSettings));
        }
    }

    private void WorkspaceInfoOnWorkspaceActivated()
    {
        LoadUserSettings(false);
    }

    private void TabbedViewOnDocumentActivated(object a_sender, DocumentEventArgs a_e)
    {
        if (m_intializingBoards)
        {
            return;
        }

        Document doc = a_e.Document as Document;
        HandleActiveBoardsAndTabs(doc.Parent);
    }

    #region Event Handlers
    /// <summary>
    /// A navigation request was made. Determine the proper event handling
    /// </summary>
    private void NavigationEventHandler(UINavigationEvent a_event)
    {
        if (a_event.Key == "ToggleHeaders")
        {
            if (a_event.Data.TryGetValue("ToggleObject", out object toggle))
            {
                BoardTabsModeChangedHandler((PackageEnums.EBoardTabsMode)toggle);
                a_event.Handled = true;
            }

            return;
        }

        if (a_event.Key == ActivateBoardNavigationEvent.OpenBoardKey)
        {
            if (a_event.Data.TryGetValue(ActivateBoardNavigationEvent.BoardName, out object data))
            {
                string boardKey = (string)data;
                bool openInNewGroup = false;

                if (a_event.Data.TryGetValue(ActivateBoardNavigationEvent.DockNewGroup, out object newGroup))
                {
                    openInNewGroup = (bool)newGroup;
                }

                ActivateBoard(boardKey, openInNewGroup);
                IBoardControl pane = GetBoard(boardKey);

                if (a_event.Data.TryGetValue(ActivateTileNavigationEvent.OpenTile, out object openTile))
                {
                    if (!string.IsNullOrWhiteSpace((string)openTile))
                    {
                        if (pane is ITileBoard tilePane)
                        {
                            tilePane.OpenTile((string)openTile, a_event);
                        }
                    }
                }

                if (a_event.Data.TryGetValue(ActivateTileNavigationEvent.OpenMultipleTiles, out object openTiles))
                {
                    if (openTiles is List<string> tilesList)
                    {
                        if (pane is ITileBoard tileBoard)
                        {
                            foreach (string tile in tilesList)
                            {
                                tileBoard.OpenTile(tile, a_event);
                            }
                        }
                    }
                }

                a_event.Handled = true;
            }
        }
    }

    private PackageEnums.EBoardTabsMode BoardTabsMode { get; set; } = PackageEnums.EBoardTabsMode.Smart;

    private void BoardTabsModeChangedHandler(PackageEnums.EBoardTabsMode a_boardTabsMode)
    {
        BoardTabsMode = a_boardTabsMode;
        switch (a_boardTabsMode)
        {
            case PackageEnums.EBoardTabsMode.None:
            case PackageEnums.EBoardTabsMode.Show:
                ToggleTabHeaders();
                break;
            case PackageEnums.EBoardTabsMode.Smart:
                SmartToggleHeaders();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(a_boardTabsMode), a_boardTabsMode, null);
        }
    }

    private void m_tabbedView_DocumentDocked(object a_sender, DocumentEventArgs a_e)
    {
        Document doc = a_e.Document as Document;
        //IBoardControl pane = (IBoardControl)doc.Control;

        // pane.BoardActive = true;

        m_tabbedView.Controller.Activate(doc);

        foreach (DocumentGroup group in m_tabbedView.DocumentGroups)
        {
            if (group != doc.Parent && group.Visible)
            {
                Document remainingDoc = GetRemainingDocumentInGroup(group, doc);
                if (remainingDoc != null)
                {
                    IBoardControl paneToActivate = (IBoardControl)remainingDoc.Control;
                    remainingDoc.Control.Focus();
                    ActivateBoard(paneToActivate.BoardKey);
                }
            }
        }

        UpdateAllowFloat();
        HandleActiveBoardsAndTabs(doc.Parent);
    }

    private void m_tabbedView_DocumentClosed(object a_sender, DocumentCancelEventArgs a_e)
    {
        BaseDocument document = a_e.Document;
        Document theTabDocument = (Document)document;

        FocusDefaultActiveDocument(theTabDocument);

        IBoardControl board = (IBoardControl)document.Control;
        board.BoardVisible = false; //Set visibility first, it may trigger an activate event.
        a_e.Cancel = true;

        if (m_intializingBoards)
        {
            return;
        }

        lock (s_boardLock)
        {
            if (m_openBoards.Contains(board))
            {
                m_openBoards.Remove(board);
            }
        }

        if (!theTabDocument.IsFloating)
        {
            if (IsGroupEmpty(theTabDocument))
            {
                theTabDocument.Parent.Visible = false;
            }
            else
            {
                theTabDocument.Parent.Visible = true;
                Document remainingDoc = GetRemainingDocumentInGroup(theTabDocument.Parent, theTabDocument);
                remainingDoc.Control.Focus();
                m_tabbedView.Controller.Activate(remainingDoc);
            }
        }
        else
        {
            theTabDocument.Form.Hide();
            //m_tabbedView.Controller.Dock(theTabDocument, m_tabbedView.DocumentGroups[0]);
            //m_tabbedView.Controller.Activate(GetRemainingDocumentInGroup(m_tabbedView.DocumentGroups[0], theTabDocument));
        }

        UpdateAllowFloat();
        HandleActiveBoardsAndTabs(theTabDocument.Parent);
    }

    private void m_tabbedView_Floating(object a_sender, DocumentEventArgs a_e)
    {
        UpdateAllowFloat();
    }

    private void m_tabbedView_BeginFloating(object a_sender, DocumentCancelEventArgs a_e)
    {
        //make sure the document group has another document active, otherwise close that group
        if (a_e.Document is Document sourceDocument)
        {
            DocumentGroup group = sourceDocument.Parent;
            if (IsGroupEmpty(sourceDocument))
            {
                FocusDefaultActiveDocument(sourceDocument);
                group.Visible = false;
            }
            else
            {
                m_tabbedView.Controller.Activate(GetRemainingDocumentInGroup(group, sourceDocument));
            }

            HandleActiveBoardsAndTabs(group);
        }
    }
    #endregion

    private void FocusDefaultActiveDocument(Document a_triggerDocument)
    {
        foreach (DocumentGroup group in m_tabbedView.DocumentGroups)
        {
            if (group != a_triggerDocument.Parent)
            {
                //group.SelectedDocument.Control.Focus();
                m_tabbedView.Controller.Activate(group.SelectedDocument);
                break;
            }
        }
    }

    #region Document group validation
    /// <summary>
    /// Check if the group contains multiple visible documents
    /// </summary>
    /// <param name="a_group"></param>
    /// <returns></returns>
    private bool GroupHasMultipleDocuments(DocumentGroup a_group)
    {
        int docCount = 0;

        foreach (BaseDocument doc in m_tabbedView.Documents)
        {
            Document document = doc as Document;

            if (document?.Control is IBoardControl boardControl && boardControl.BoardVisible && document.Parent == a_group)
            {
                docCount++;

                if (docCount > 1)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Check if the group has no remaining documents
    /// </summary>
    /// <param name="a_doc"></param>
    /// <returns></returns>
    private bool IsGroupEmpty(Document a_doc)
    {
        List<BaseDocument> groupedDocs = new ();
        foreach (BaseDocument doc in m_tabbedView.Documents)
        {
            Document document = doc as Document;
            if (document?.Control is IBoardControl boardControl && boardControl.BoardVisible && document != a_doc && document.Parent == a_doc.Parent)
            {
                groupedDocs.Add(doc);
            }
        }

        return groupedDocs.Count == 0;
    }

    /// <summary>
    /// Get last document in the group
    /// </summary>
    /// <param name="a_group"></param>
    /// <param name="a_excludingDocument"></param>
    /// <returns></returns>
    private Document GetRemainingDocumentInGroup(DocumentGroup a_group, Document a_excludingDocument)
    {
        Document lastDocument = null;
        foreach (BaseDocument baseDocument in m_tabbedView.Documents)
        {
            Document tabDocument = (Document)baseDocument;
            if (tabDocument != a_excludingDocument && tabDocument.Parent == a_group)
            {
                if (tabDocument.Control is IBoardControl boardControl && boardControl.BoardVisible)
                {
                    //Keep the active pane in the group active.
                    if (boardControl.BoardActive)
                    {
                        return tabDocument;
                    }

                    lastDocument = tabDocument;
                }
            }
        }

        return lastDocument;
    }
    #endregion

    /// <summary>
    /// Prevents the last docked pane to be undocked
    /// </summary>
    private void UpdateAllowFloat()
    {
        if (m_intializingBoards)
        {
            return;
        }

        lock (s_boardLock)
        {
            int undockedBoardCount = 0;
            foreach (IBoardControl boardControl in m_openBoards)
            {
                if (m_tabbedView.Documents.TryGetValue((Control)boardControl, out BaseDocument baseDoc))
                {
                    if (!baseDoc.IsFloating)
                    {
                        undockedBoardCount++;
                    }
                }
            }

            bool allowFloat = m_openBoards.Count > 0 && undockedBoardCount > 1;
            m_tabbedView.DocumentProperties.AllowTabReordering = allowFloat;

            foreach (IBoardControl paneControl in m_openBoards)
            {
                if (m_tabbedView.Documents.TryGetValue((Control)paneControl, out BaseDocument baseDoc))
                {
                    if (baseDoc is Document doc)
                    {
                        //Allow closing tabs in the non primary tab group or in the primary group as long as more than one board is open
                        DocumentGroup group = doc.Parent;
                        bool notPrimaryGroupCanClose = group != m_tabbedView.DocumentGroups[0];
                        bool primaryGroupCanClose = !notPrimaryGroupCanClose && GroupHasMultipleDocuments(group);

                        doc.Properties.AllowFloat = allowFloat ? DefaultBoolean.True : DefaultBoolean.False;
                        doc.Properties.AllowClose = notPrimaryGroupCanClose || primaryGroupCanClose ? DefaultBoolean.True : DefaultBoolean.False;
                    }
                }
                else if (m_tabbedView.FloatDocuments.TryGetValue((Control)paneControl, out BaseDocument floatDoc))
                {
                    //floatDoc.Properties.AllowFloat = allowFloat ? DefaultBoolean.True : DefaultBoolean.False;
                    //floatDoc.Properties.AllowClose = allowFloat ? DefaultBoolean.True : DefaultBoolean.False;
                }
            }
        }
    }

    private void HandleActiveBoardsAndTabs(DocumentGroup a_group = null)
    {
        List<Document> visibleDocuments = new ();
        foreach (DocumentGroup group in m_tabbedView.DocumentGroups)
        {
            visibleDocuments.Add(group.SelectedDocument);
        }

        foreach (Document document in m_tabbedView.FloatDocuments)
        {
            visibleDocuments.Add(document);
        }

        foreach (BaseDocument tabbedViewDocument in m_tabbedView.Documents)
        {
            Document document = (Document)tabbedViewDocument;

            IBoardControl board = (IBoardControl)document.Control;
            board.BoardActive = visibleDocuments.Contains(document);
        }

        //SaveUserSettings();

        if (BoardTabsMode == PackageEnums.EBoardTabsMode.Smart)
        {
            SmartToggleHeaders(a_group);
        }
        else
        {
            ToggleTabHeaders();
        }
    }

    private void SmartToggleHeaders(DocumentGroup a_group = null)
    {
        m_tabbedView.BeginUpdate();

        if (a_group != null)
        {
            a_group.Properties.ShowTabHeader = GroupHasMultipleDocuments(a_group) ? DefaultBoolean.True : DefaultBoolean.False;
        }
        else
        {
            foreach (DocumentGroup group in m_tabbedView.DocumentGroups)
            {
                group.Properties.ShowTabHeader = GroupHasMultipleDocuments(group) ? DefaultBoolean.True : DefaultBoolean.False;
            }
        }

        m_tabbedView.EndUpdate();
    }

    private void ToggleTabHeaders()
    {
        bool toggle = BoardTabsMode == PackageEnums.EBoardTabsMode.Show;

        m_tabbedView.BeginUpdate();

        foreach (DocumentGroup group in m_tabbedView.DocumentGroups)
        {
            group.Properties.ShowTabHeader = toggle ? DefaultBoolean.True : DefaultBoolean.False;
        }

        m_tabbedView.EndUpdate();
    }

    private void MapBoard(IBoardControl a_board)
    {
        Document document = (Document)m_tabbedView.AddDocument((Control)a_board);
        document.ControlName = a_board.BoardControlName;
        document.Caption = a_board.DisplayName;
        document.Properties.AllowClose = DefaultBoolean.True;
        a_board.BoardVisible = false;
        //    document.CustomHeaderButtons.Add(m_tilesButton);
        //    document.CustomButtonClick += new ButtonEventHandler(TileMenuPopupHandler);
        //    document.AppearanceCaption.BackColor = PTAppearance.PrimaryTilePaneColor;
        //    document.AppearanceActiveCaption.BackColor = PTAppearance.PrimaryTilePaneColor;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="a_disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool a_disposing)
    {
        //if (m_propertiesControl != null)
        //{
        //    m_propertiesControl.CaptionUpdateEvent -= UpdateObjectPropertiesPaneText;
        //}

        //if (m_activitySchedulingGridControl != null)
        //{
        //    m_activitySchedulingGridControl.FindButtonClicked -= FindActivityHandler;
        //}

        //m_featureController.FeatureChangedEvent -= EnforcePermissions;

        if (a_disposing)
        {
            components?.Dispose();
            m_scenarioInfo.ScenarioClosed -= CloseScenario;
            m_scenarioInfo.ScenarioActivated -= ActivateScenario;
            m_mainForm.UiNavigationEvent -= NavigationEventHandler;
            m_mainForm.WorkspaceInfo.CollectUnsavedSettings -= WorkspaceInfoOnCollectUnsavedSettings;
            m_mainForm.WorkspaceInfo.WorkspaceActivated -= WorkspaceInfoOnWorkspaceActivated;
        }

        base.Dispose(a_disposing);
    }

    public override void Localize()
    {
        UILocalizationHelper.LocalizeControlsRecursively(Controls);
        //TODO: Add documentmanager
    }

    #region Open Scenario
    private readonly ScenarioViewerListener m_listener;

    public void CloseScenario(Scenario a_s, ScenarioEvents a_se)
    {
        m_listener.RemoveScenarioEventListeners(a_se);
    }

    public void ActivateScenario(Scenario a_s, ScenarioDetail a_sd, ScenarioEvents a_se)
    {
        m_listener.AddScenarioEventListeners(a_se);
        HideScenarioOverlay();
        LoadInitialScenario();
    }

    private bool m_initialScenarioLoaded;
    private bool m_intializingBoards;

    private void LoadInitialScenario()
    {
        if (m_initialScenarioLoaded)
        {
            return;
        }

        m_initialScenarioLoaded = true;

        try
        {
            LoadUserSettings(true);
        }
        catch (Exception e)
        {
            m_mainForm.LogException(new PTHandleableException("Failed to load scenario user settings"), true);
        }

        InitializeViewSettings();
    }
    #endregion Open Scenario

    private static void InvokeMessageProvider(string a_message)
    {
        m_messageProvider.ShowMessage(a_message);
    }

    #region Inventory Plan Control
    //private async void InventoryPlanControl_FindOperationClicked(string a_jobName, string a_operationName)
    //{
    //    using (BackgroundLock asyncLock = new BackgroundLock(m_scenarioId))
    //    {
    //        await asyncLock.RunLockCodeBackground(FindOperation, a_jobName, a_operationName));
    //    }
    //}

    //private void InventoryPlanControl_FindOperationMaterialsClicked(BaseId a_id)
    //{
    //    MaterialsGridFilterOpertions(new List<BaseId> { a_id });
    //}

    //TODO: Tiles: Add to search tile
    private void FindOperation(ScenarioManager a_sm, Scenario a_s, ScenarioDetail a_sd, object[] a_params)
    {
        string jobName = (string)a_params[0];
        string opName = (string)a_params[1];
        Job job = a_sd.JobManager.GetByName(jobName);
        if (job == null)
        {
            BeginInvoke(new Action(() => InvokeMessageProvider(string.Format("Job with name '{0}' was not found.".Localize(), jobName))));
            return;
        }

        InternalOperation op = null;
        foreach (ManufacturingOrder mo in job.ManufacturingOrders)
        {
            op = mo.OperationManager.Find(opName) as InternalOperation;
            if (op != null)
            {
                break;
            }
        }

        if (op == null)
        {
            BeginInvoke(new Action(() => InvokeMessageProvider(string.Format("Operation with Name '{0}' was not found in Job '{1}'".Localize(), opName, jobName))));
            return;
        }

        InternalActivity act = op.GetLeadActivity();
        if (act == null || !act.Scheduled)
        {
            BeginInvoke(new Action(() => InvokeMessageProvider(string.Format("Job '{1}' Operation '{0}' does not have an Activity that is scheduled.".Localize(), opName, jobName))));
            return;
        }

        Invoke(new Action(() =>
        {
            //ShowGanttPane(true);
            //m_ganttEventProcessor.FindActivity(act.PrimaryResourceRequirementBlock.GetKey(), act.PrimaryResourceRequirementBlock.ScheduledResource.GetResourceKey());
        }));
    }

    //Removed link between inventory plan and inventory plot. The plan already has a plot and both controls have their own event handlers
    //private void InventoryPlanControl_InventoryRowActivated(BaseId a_itemId, BaseId a_warehouseId, ScenarioUserRights a_userRights)
    //{
    //    BeginInvoke(new Action<BaseId, BaseId>((itemId, warehouseId) =>
    //    {
    //        PT.Common.Testing.Timing timer = new PT.Common.Testing.Timing(true, "ScenarioViewer.InventoryPlanControl_InventoryRowActivated");

    //        if (GetInventoryPlotControl(false) != null && GetInventoryPlotControl(false).Visible)
    //        {
    //            try
    //            {
    //                ScenarioManager sm;
    //                using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
    //                {
    //                    Scenario s = sm.Find(ScenarioId);
    //                    ScenarioDetail sd;
    //                    using (s.ScenarioDetailLock.TryEnterRead(out sd, 1000))
    //                    {
    //                        Warehouse warehouse = sd.WarehouseManager.GetById(a_warehouseId);
    //                        if (warehouse != null) //data can be stale
    //                        {
    //                            Inventory inventory = warehouse.Inventories[a_itemId];
    //                            if (inventory != null)
    //                            {
    //                                GetInventoryPlotControl(false).ShowInventory(sd, warehouse, inventory, sd.ClockDate, sd.GetPlanningHorizonEnd());
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //            catch (AutoTryEnterException)
    //            {
    //                MainForm.ShowBusyMessage();
    //            }
    //        }

    //        Utilities.Timing.Log(timer);
    //    }), new object[] { a_itemId, a_warehouseId });
    //}
    #endregion Inventory Plan Control

    #region Multi Storage Control
    //private StoragePlotMultiPanel multiStoragePlotControl;

    //internal StoragePlotMultiPanel GetMultiStoragePlotControl(bool createIfNull)
    //{
    //    if (multiStoragePlotControl == null & createIfNull)
    //    {
    //        multiStoragePlotControl = new StoragePlotMultiPanel(ScenarioId);
    //        multiStoragePlotControl.Dock = DockStyle.Fill;
    //        MultiStoragePanel.Controls.Add(multiStoragePlotControl);
    //    }
    //    //TODO: m_userSettingsControls.Add(multiStoragePlotControl);
    //    return multiStoragePlotControl;
    //}

    //internal void ShowMultiStoragePane(bool pinit)
    //{
    //    ShowPane(MultiStoragePanel, pinit);
    //}
    #endregion Multi Storage Control

    private IBoardControl GetBoard(string a_boardKey)
    {
        if (m_boardControls.TryGetValue(a_boardKey, out IBoardControl board))
        {
            return board;
        }

        using (new MultiLevelHourglass())
        {
            IBoardControl boardControl = m_packageManager.GeneratePane(m_scenarioInfo, a_boardKey);

            if (boardControl is ITileBoard tileBoard)
            {
                foreach (ITileModule module in m_packageManager.GetTileModules(boardControl.BoardKey))
                {
                    tileBoard.LoadTileModule(module);
                }
            }

            if (boardControl != null)
            {
                m_boardControls.Add(a_boardKey, boardControl);
            }

            return boardControl;
        }
    }

    private void ShowBoard(IBoardControl a_boardControl)
    {
        using (new MultiLevelHourglass())
        {
            lock (s_boardLock)
            {
                a_boardControl.BoardVisible = true;

                //if (m_activeBoards.Contains(a_boardControl))
                //{
                //    //Already shown, just activate
                //    ActivateBoard(a_boardControl.BoardKey, false);
                //    return;
                //}

                //Find the parent document group. If it is not visible, show it.
                if (m_tabbedView.Documents.TryGetValue((Control)a_boardControl, out BaseDocument baseDocument))
                {
                    DocumentGroup documentGroup = (baseDocument as Document).Parent;
                    if (!documentGroup.Visible)
                    {
                        documentGroup.Visible = true;
                    }
                }
                else if (m_tabbedView.FloatDocuments.TryGetValue((Control)a_boardControl, out BaseDocument floatDocument))
                {
                    floatDocument.Form.Show();
                }
            }

            UpdateAllowFloat();
            //SaveUserSettings();
        }
    }

    private void ActivateBoard(string a_key, bool a_newGroup = false)
    {
        using (new MultiLevelHourglass())
        {
            IBoardControl boardControl = GetBoard(a_key);
            if (boardControl == null)
            {
                return;
            }

            if (!boardControl.BoardVisible)
            {
                try
                {
                    ShowBoard(boardControl);
                }
                catch (Exception e)
                {
                    string msg = Localizer.GetErrorString("4450", new object[] { boardControl.DisplayName, e.GetExceptionFullMessage() }, true);
                    m_mainForm.MessageProvider.ShowMessageBox(new PTMessage(msg, "Unable To Show Board") { Classification = PTMessage.EMessageClassification.Warning });
                    return;
                }
            }

            if (m_tabbedView.Documents.TryGetValue((Control)boardControl, out BaseDocument baseDoc))
            {
                if (baseDoc is Document document)
                {
                    if (a_newGroup && GroupHasMultipleDocuments(document.Parent))
                    {
                        m_tabbedView.Controller.CreateNewDocumentGroup(document);
                    }

                    if (!m_intializingBoards)
                    {
                        document.Control.Focus();
                        HandleActiveBoardsAndTabs(document.Parent);
                    }
                }
            }

            lock (s_boardLock)
            {
                if (!m_openBoards.Contains(boardControl))
                {
                    m_openBoards.Add(boardControl);
                }
            }
        }
    }

    internal void ShowScenarioOverlay(string a_message)
    {
        m_scenarioOverlayForm.SetLoadingText(a_message);
        m_scenarioOverlayForm.ShowOverlay();
    }

    internal void HideScenarioOverlay()
    {
        m_scenarioOverlayForm.HideOverlay();
    }

    private void ScenarioOverlayFormOnCancelled()
    {
        //TODO lite client: Restart the program somehow here
    }

    #region Materials Pane
    private async void MaterialPlotRequestedHandler(BaseId a_itemId)
    {
        List<string> returnedItemName = new ();
        using (BackgroundLock asyncLock = new (m_scenarioInfo.ScenarioId))
        {
            await asyncLock.RunLockCode(this, FilterInventoryPlotByItem, a_itemId, returnedItemName);
            if (asyncLock.Status is BackgroundLock.EResultStatus.Error or BackgroundLock.EResultStatus.Canceled) { }
        }
        //if (returnedItemName.Count > 0)
        //{
        //    IPaneControl pane = GetPane("InventoryPlan");
        //    ITilePane tilePane = pane as ITilePane;
        //    ((IGridTile)tilePane.GetPrimaryTile()).ApplyColumnFilters("Item", returnedItemName, true);
        //    ActivatePane("InventoryPlan", false);
        //}
    }

    private void MaterialPlotRequestedHandler(List<string> a_itemIds)
    {
        //if (a_itemIds.Count > 0)
        //{
        //    IPaneControl pane = GetPane("InventoryPlan");
        //    ITilePane tilePane = pane as ITilePane;
        //    ((IGridTile)tilePane.GetPrimaryTile()).ApplyColumnFilters("Item", a_itemIds, true);
        //    ActivatePane("InventoryPlan", false);
        //}
    }

    private void MaterialsGridFilterOpertions(List<BaseId> a_operationIds)
    {
        //if (a_operationIds.Count > 0)
        //{
        //    IPaneControl pane = GetPane("Materials");
        //    ITilePane tilePane = pane as ITilePane;
        //    ((IGridTile)tilePane.GetPrimaryTile()).ApplyColumnFilters("OpId", a_operationIds, true);
        //    ShowPane("Materials");
        //}
    }

    private static void FilterInventoryPlotByItem(Scenario a_s, ScenarioDetail a_sd, params object[] a_params)
    {
        BaseId itemId = (BaseId)a_params[0];
        List<string> returnedItemName = (List<string>)a_params[1];
        Item item = a_sd.ItemManager.GetById(itemId);
        if (item != null)
        {
            returnedItemName.Add(item.Name);
        }
    }
    #endregion Materials Control

    #region Layouts
    private void LoadUserSettings(bool a_mapBoards)
    {
        m_intializingBoards = true;

        m_dockManagerSettings.SettingKey = "DockManager";
        m_dockManagerSettings = m_mainForm.WorkspaceInfo.LoadSetting(m_dockManagerSettings);
        List<IBoardControl> newBoards = new ();

        m_tabbedView.BeginUpdate();

        //Load document manager layout from stream
        if (m_dockManagerSettings.TileLayoutBytes != null)
        {
            if (a_mapBoards)
            {
                //Generate all panes initially. This will create the documents in the DocumentManager, but not load the tile controls.
                //This is required to load a layout successfully.
                List<IBoardModule> paneInfos = m_packageManager.GetPaneModules();
                m_dockManagerSettings.RemoveBoardsNoLongerLoaded(paneInfos);

                foreach (IBoardModule paneInfo in paneInfos)
                {
                    IBoardControl boardControl = GetBoard(paneInfo.BoardKey);
                    if (boardControl != null)
                    {
                        if (!m_dockManagerSettings.LoadedDocuments.Contains(boardControl.BoardKey))
                        {
                            m_dockManagerSettings.LoadedDocuments.Add(boardControl.BoardKey);
                            newBoards.Add(boardControl);
                        }
                        else
                        {
                            MapBoard(boardControl);
                        }
                    }
                }
            }

            foreach (IBoardControl newBoard in newBoards)
            {
                MapBoard(newBoard);
            }

            using (MemoryStream loadStream = new (m_dockManagerSettings.TileLayoutBytes))
            {
                documentManager1.View.RestoreLayoutFromStream(loadStream);
            }
        }
        else
        {
            if (a_mapBoards)
            {
                //Generate all panes initially. This will create the documents in the DocumentManager, but not load the tile controls.
                //This is required to load a layout successfully.
                List<IBoardModule> paneInfos = m_packageManager.GetPaneModules();

                foreach (IBoardModule paneInfo in paneInfos)
                {
                    IBoardControl boardControl = GetBoard(paneInfo.BoardKey);
                    if (boardControl != null)
                    {
                        m_dockManagerSettings.LoadedDocuments.Add(boardControl.BoardKey);
                        MapBoard(boardControl);
                    }
                }
            }
        }

        //Close all open docs (when switching workspaces)
        for (int i = m_tabbedView.FloatDocuments.Count - 1; i >= 0; i--)
        {
            Document doc = (Document)m_tabbedView.FloatDocuments[i];
            CloseBoard(doc, true);
        }

        for (int i = m_tabbedView.Documents.Count - 1; i >= 0; i--)
        {
            Document doc = (Document)m_tabbedView.Documents[i];
            CloseBoard(doc, false);
        }

        List<string> previouslyOpenBoards = m_dockManagerSettings.ActiveDocuments.Keys.ToList();
        foreach (string boardKey in previouslyOpenBoards)
        {
            if (!m_boardControls.ContainsKey(boardKey))
            {
                m_dockManagerSettings.ActiveDocuments.Remove(boardKey);
            }
            //TODO: Maybe save this setting if we remove a board
        }

        //If no active boards are loaded, try opening the Jobs Board
        if (m_dockManagerSettings.ActiveDocuments.Count == 0)
        {
            IBoardControl jobBoard = GetBoard(BoardKeys.Job);
            if (jobBoard != null)
            {
                m_dockManagerSettings.ActiveDocuments.Add(jobBoard.BoardKey, true);
            }
        }

        foreach ((string boardKey, bool boardActive) in m_dockManagerSettings.ActiveDocuments)
        {
            foreach (IBoardControl boardControl in m_boardControls.Values)
            {
                if (boardKey == boardControl.BoardKey)
                {
                    boardControl.BoardActive = boardActive;
                    try
                    {
                        boardControl.LoadUserSettings();
                    }
                    catch (Exception e)
                    {
                        m_mainForm.LogException(new PTHandleableException($"Failed to load board settings for {boardKey}"), false);
                    }

                    ActivateBoard(boardControl.BoardKey);
                    break;
                }
            }
        }

        foreach (string boardKey in m_dockManagerSettings.GetSelectedBoards())
        {
            Document doc = GetDocumentByKey(boardKey, out bool floating);
            if (doc != null)
            {
                doc.Control.Focus();
                m_tabbedView.Controller.Activate(doc);
            }
        }

        m_intializingBoards = false;

        HideEmptyDocGroups();
        UpdateAllowFloat();
        HandleActiveBoardsAndTabs();
        m_tabbedView.EndUpdate();
    }

    private void HideEmptyDocGroups()
    {
        foreach (DocumentGroup group in m_tabbedView.DocumentGroups)
        {
            bool isEmpty = true;
            foreach (Document doc in group.Items)
            {
                if (doc?.Control is IBoardControl boardControl && boardControl.BoardVisible)
                {
                    isEmpty = false;
                    break;
                }
            }

            if (isEmpty)
            {
                group.Visible = false;
            }
        }
    }

    private Document GetDocumentByKey(string a_boardKey, out bool o_floating)
    {
        o_floating = false;
        foreach (BaseDocument baseDoc in m_tabbedView.Documents)
        {
            if (baseDoc is Document doc)
            {
                if (doc.Control is IBoardControl boardControl && boardControl.BoardKey == a_boardKey)
                {
                    return doc;
                }
            }
        }

        foreach (BaseDocument baseDoc in m_tabbedView.FloatDocuments)
        {
            if (baseDoc is Document doc)
            {
                if (doc.Control is IBoardControl boardControl && boardControl.BoardKey == a_boardKey)
                {
                    o_floating = true;
                    return doc;
                }
            }
        }

        return null;
    }

    private DocumentManagerContainer m_dockManagerSettings = new ();

    /// <returns>True if there were setting changes, false if there were no setting changes</returns>
    private bool SaveUserSettings()
    {
        if (m_intializingBoards)
        {
            return false;
        }

        Dictionary<string, bool> activeBoards = new ();

        lock (s_boardLock)
        {
            m_openBoards.Sort((x, y) => x.BoardActive.CompareTo(y.BoardActive));
            foreach (IBoardControl board in m_openBoards)
            {
                activeBoards.Add(board.BoardKey, board.BoardActive);
            }

            byte[] data;
            using (MemoryStream s = new ())
            {
                documentManager1.View.SaveLayoutToStream(s);
                data = s.ToArray();
            }

            if (string.IsNullOrEmpty(m_dockManagerSettings.SettingKey))
            {
                m_dockManagerSettings.SettingKey = "DockManager";
            }

            if (m_dockManagerSettings.ActiveDocuments.Count == 0 && activeBoards.Count == 0)
            {
                return false;
            }

            if (IsEqualToActiveDocuments(activeBoards) &&
                m_dockManagerSettings.TileLayoutBytes != null &&
                m_dockManagerSettings.TileLayoutBytes.SequenceEqual(data))
            {
                return false;
            }

            m_dockManagerSettings.ActiveDocuments = activeBoards;
            m_dockManagerSettings.TileLayoutBytes = data;
            m_dockManagerSettings.SettingCaption = "Boards layout";
            m_mainForm.WorkspaceInfo.SaveSetting(m_dockManagerSettings);

            return true;
        }
    }

    private bool IsEqualToActiveDocuments(Dictionary<string, bool> a_boards)
    {
        if (m_dockManagerSettings.ActiveDocuments.Count != a_boards.Count)
        {
            return false;
        }

        foreach ((string boardKey, bool isActive) in m_dockManagerSettings.ActiveDocuments)
        {
            if (a_boards.TryGetValue(boardKey, out bool otherIsActive))
            {
                if (isActive != otherIsActive)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private bool IsEqualToTileLayoutBytes(byte[] a_data)
    {
        if (m_dockManagerSettings.TileLayoutBytes == null &&
            a_data != null)
        {
            return false;
        }

        if (m_dockManagerSettings.TileLayoutBytes != null &&
            a_data == null)
        {
            return false;
        }

        if (m_dockManagerSettings.TileLayoutBytes == null &&
            a_data == null)
        {
            return true;
        }

        if (m_dockManagerSettings.TileLayoutBytes != null &&
            a_data != null)
        {
            return m_dockManagerSettings.TileLayoutBytes.SequenceEqual(a_data);
        }

        // The four cases above cover all possibility, but the compiler is forcing me to 
        // have a return statement here so I'm just returning false. 
        return false;
    }
    #endregion Layouts

    #region Docking
    //private void InitializeControls()
    //{
    //    //Set floating locations and sizes if not already set.
    //    foreach (DockAreaPane da in ultraDockManager1.DockAreas)
    //    {
    //        if (da.Settings.AllowResize != DefaultableBoolean.False) // && da.FloatingLocation==new Point(1,1)) //Then user hasn't customized sizes so we can resize to a reasonable size.
    //        {
    //            Size s;
    //            if (da.DockedState == DockedState.Floating)
    //            {
    //                s = new Size(Convert.ToInt32(Screen.PrimaryScreen.WorkingArea.Width * .7), Convert.ToInt32(Screen.PrimaryScreen.WorkingArea.Height * .5));
    //            }
    //            else
    //            {
    //                s = new Size(Convert.ToInt32(Screen.PrimaryScreen.WorkingArea.Width * .3), Convert.ToInt32(Screen.PrimaryScreen.WorkingArea.Height * .3));
    //            }
    //            da.Size = s;

    //            //Center in the screen
    //            Point p = new Point(Convert.ToInt32(Screen.PrimaryScreen.WorkingArea.Width / 2 - s.Width / 2), Convert.ToInt32(Screen.PrimaryScreen.WorkingArea.Height / 2 - s.Height / 2));
    //            da.FloatingLocation = p;
    //        }
    //    }
    //}

    /// <summary>
    /// If the pane is off the screen, move it so it's visible.
    /// </summary>
    /// <param name="p"></param>
    //private void InsureVisible(DockAreaPane p)
    //{
    //    if (p.DockedState == DockedState.Floating)
    //    {
    //        bool onScreen = ValidateWindowOnScreen(Screen.PrimaryScreen, Screen.AllScreens, p.FloatingLocation, p.Size.Width);
    //        if (!onScreen)
    //        {
    //            FitWindowOnScreen(p, Screen.PrimaryScreen.WorkingArea);
    //        }
    //    }
    //}

    /// <summary>
    /// Checks if the pane can be seen and dragged within any screen.
    /// If not, it will move and resize the pane to be visible on the primary screen.
    /// </summary>
    /// <param name="a_primaryScreen"></param>
    /// <param name="a_screens"></param>
    /// <param name="a_location">Current pane location</param>
    /// <param name="a_width">Current pane width</param>
    public bool ValidateWindowOnScreen(Screen a_primaryScreen, Screen[] a_screens, Point a_location, int a_width)
    {
        foreach (Screen screen in a_screens)
        {
            //Test if the top left area of the window is on a screen
            Rectangle corner = new (a_location.X, a_location.Y, 100, 100);
            if (screen.WorkingArea.Contains(corner))
            {
                return true;
            }

            //Test if the top right area of the window is on a screen
            corner = new Rectangle(a_location.X + a_width - 100, a_location.Y, 100, 100);
            if (screen.WorkingArea.Contains(corner))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Fixes the pane position so that it can be drawn within the screen.
    /// </summary>
    /// <param name="a_screen"></param>
    //private static void FitWindowOnScreen(DockAreaPane a_p, System.Drawing.Rectangle a_screen)
    //{
    //    a_p.FloatingLocation = new Point(a_screen.X + 10, a_screen.Y + 10);
    //    Size newSize = new Size(Math.Max(a_p.Size.Width, 300), Math.Max(a_p.Size.Height, 300));
    //    a_p.Size = new Size(Math.Min(newSize.Width, (int)(a_screen.Width * .85)),  Math.Min(newSize.Height, (int)(a_screen.Height * .85)));
    //}

    //TODO: Tiles: remove
    internal void SetPropertyPaneObject(object o, bool a_refreshType)
    {
        if (Disposing) { }

        //GetPropertiesControl(true).SetSelectedObject(o, a_refreshType);
    }

    //private void ClearPropertyPaneObject()
    //{
    //    if (GetPropertiesControl(false) != null)
    //    {
    //        GetPropertiesControl(false).Clear();
    //        ultraDockManager1.PaneFromControl(PropertiesPanel).Text = Localizer.GetString("Properties");
    //    }
    //}
    #endregion

    /// <summary>
    /// Loads user settings, panes, and refreshes the undo list.
    /// </summary>
    private void InitializeViewSettings()
    {
        //UpdateGanttView();
    }

    private void CloseBoard(Document a_document, bool a_isFloatDoc)
    {
        if (a_document.Control is IBoardControl boardControl)
        {
            lock (s_boardLock)
            {
                if (m_openBoards.Contains(boardControl))
                {
                    m_openBoards.Remove(boardControl);
                }
            }
        }

        if (a_isFloatDoc)
        {
            m_tabbedView.Controller.Dock(a_document);
        }

        m_tabbedView.Controller.Close(a_document);


        //SaveUserSettings();
    }

    public void Unload()
    {
        foreach (IBoardControl boardControl in m_boardControls.Values)
        {
            boardControl.Unload();
        }
    }
}