using System.Drawing;
using System.Windows.Forms;

using DevExpress.Utils;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraBars.Docking2010;
using DevExpress.XtraBars.Docking2010.Views;
using DevExpress.XtraBars.Docking2010.Views.Widget;
using DevExpress.XtraBars.Navigation;

using PT.APSCommon.Extensions;
using PT.APSCommon.Packages;
using PT.APSCommon.Windows;
using PT.Common.Debugging;
using PT.ComponentLibrary;
using PT.ComponentLibrary.Extensions;
using PT.PackageDefinitions;
using PT.PackageDefinitionsUI;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.ScenarioControls.PackageHelpers;
using PT.Scheduler;
using PT.UIDefinitions;

using QueryControlEventArgs = DevExpress.XtraBars.Docking2010.Views.QueryControlEventArgs;

namespace PT.ScenarioControls.Tiles;

public sealed partial class TileBoardLayoutControl<T> : BoardControlBase, ITileBoard
{
    private readonly TileController<T> m_tileController;
    private readonly IMainForm m_mainForm;
    private readonly IScenarioInfo m_scenarioInfo;
    private bool m_loading;
    private static readonly object s_lock = new object();
    private DocumentManagerContainer m_docContainerSettings;

    private int m_tileOffsetMultiplier; // used to stagger the opening location of tiles
    private const int c_baseOffset = 10;

    public Color PaneColor
    {
        set => tileDocumentManager.PaneColor = value;
        private get => tileDocumentManager.PaneColor;
    }

    /// <summary>
    /// Designer Constructor
    /// </summary>
    public TileBoardLayoutControl()
    {
        InitializeComponent();
    }

    public TileBoardLayoutControl(IMainForm a_mainForm, IScenarioInfo a_scenarioInfo)
    {
        InitializeComponent();
        if (!IsHandleCreated)
        {
            CreateHandle();
        }

        m_mainForm = a_mainForm;
        m_scenarioInfo = a_scenarioInfo;
        m_scenarioInfo.ScenarioActivated += ScenarioInfoOnScenarioActivated;
        a_mainForm.UserPreferenceInfo.SettingSavedEvent += UserPreferenceInfoOnSettingSavedEvent;

        //Load object controls
        m_tileController = new TileController<T>(a_scenarioInfo);
        m_tileController.RegisterEvents();

        tileDocumentManager.View.DocumentClosing += new DocumentCancelEventHandler(tileView1_DocumentClosed);
        tileDocumentManager.View.EndDocking += ViewOnEndDocking;
        tileDocumentManager.View.DocumentActivated += ViewOnDocumentActivated;
        tileDocumentManager.View.BeginFloating += ViewOnBeginFloating;
        tileDocumentManager.View.QueryControl += ViewOnQueryControl;

        LoadTileSettings();
        accordionControlElement_Settings.ImageOptions.SvgImage = PtImageCache.GetImage("settingsGears");
        accordionControlElement_Tiles.ImageOptions.SvgImage = PtImageCache.GetImage("tileLayout");
        accordionControlElement_CloseAll.ImageOptions.SvgImage = PtImageCache.GetImage("close");
        accordionControlElement_InlineMenuMode.ImageOptions.SvgImage = PtImageCache.GetImage("tilesMain");

        AddSettingElementsSuperTips();

        OpenTileCheckEdit.Click += CheckEditOnClick;
        InlineMenuCheckEdit.Click += CheckEditOnClick;
        
        accordionControl1.CustomDrawElement += AccordionControl1_CustomDrawElement;
        accordionControlElement_Tiles.Click += AccordionControl1OnStateChanged;
        accordionControlElement_Settings.Click += AccordionControl1OnStateChanged;
        UpdateTilePreferences();
        m_tileOffsetMultiplier = 0;

        Localize();
    }

    private void AccordionControl1_CustomDrawElement(object sender, CustomDrawElementEventArgs e)
    {
        AccordionElementBaseViewInfo elementBaseViewInfo = e.ObjectInfo;
        if (isBoardOpen.TryGetValue(e.Element.Name, out bool opened) && opened)
        {
            using (SolidBrush brush = new (m_theme.BlueDark))
            {
                Rectangle rect = elementBaseViewInfo.ImageBounds;

                if (accordionControl1.OptionsMinimizing.State == AccordionControlState.Minimized)
                {
                    rect.Location = new Point((int)Math.Abs(rect.Left - (rect.Left * 0.6)), rect.Y);
                    rect.Width = 4;
                }
                else
                {
                    rect.Location = new Point((int)Math.Abs(rect.Left * 0.3), rect.Y);
                    rect.Width = 3;
                }
                
                e.Graphics.FillRectangle(brush, rect);
            }
        }
    }

    private void ViewOnBeginFloating(object a_sender, DocumentCancelEventArgs a_e)
    {
        //SaveUserSettings();
    }

    private void ViewOnDocumentActivated(object a_sender, DocumentEventArgs a_e)
    {
        //SaveUserSettings();
    }

    private void ViewOnEndDocking(object a_sender, DocumentEventArgs a_e)
    {
        //SaveUserSettings();
    }

    private void WorkspaceInfoOnCollectUnsavedSettings(WorkspaceSettingsCollector a_collector)
    {
        //The modules list will be emptied once we have loaded the tiles.
        if (SaveUserSettings())
        {
            a_collector.SaveSetting(new SettingData(m_docContainerSettings));
        }
    }

    /// <summary>
    /// Add tool tips to settings accordion elements
    /// </summary>
    private void AddSettingElementsSuperTips()
    {
        accordionControlElement_ShowMode.SuperTip = new SuperToolTip();
        accordionControlElement_ShowMode.SuperTip.Items.AddTitle("Open Mode".Localize());
        accordionControlElement_ShowMode.SuperTip.Items.Add("Open tiles undocked as movable windows".Localize());

        accordionControlElement_CloseAll.SuperTip = new SuperToolTip();
        accordionControlElement_CloseAll.SuperTip.Items.AddTitle("Close Tiles".Localize());
        accordionControlElement_CloseAll.SuperTip.Items.Add("Close open tiles".Localize());

        accordionControlElement_InlineMenuMode.SuperTip = new SuperToolTip();
        accordionControlElement_InlineMenuMode.SuperTip.Items.AddTitle("Toggle Menu Display Mode".Localize());
        accordionControlElement_InlineMenuMode.SuperTip.Items.Add("Toggle menu display mode between an overlay style and an inline style.".Localize());
    }

    private void UserPreferenceInfoOnSettingSavedEvent(ISettingsManager a_arg1, string a_settingKey)
    {
        if (a_settingKey == TilePreferences.GlobalSettingKey)
        {
            LoadTileSettings();
        }
    }

    private void LoadTileSettings()
    {
        TilePreferences tilePreferences = m_mainForm.UserPreferenceInfo.LoadSetting<TilePreferences>(TilePreferences.GlobalSettingKey);
        OpenTileCheckEdit.Checked = tilePreferences.OpenTileInWindow;
        InlineMenuCheckEdit.Checked = tilePreferences.InlineMenuDisplayMode;

        accordionControlElement_ShowMode.ImageOptions.SvgImage = PtImageCache.GetImage(OpenTileCheckEdit.Checked ? "pushPin" : "workspaces");
        accordionControl1.OptionsHamburgerMenu.DisplayMode = tilePreferences.InlineMenuDisplayMode ? AccordionControlDisplayMode.Inline : AccordionControlDisplayMode.Overlay;
    }

    private void CheckEditOnClick(object a_sender, ContextItemClickEventArgs a_e)
    {
        UpdateTilePreferences();
    }

    private AccordionCheckContextButton OpenTileCheckEdit => accordionControlElement_ShowMode.ContextButtons[0] as AccordionCheckContextButton;
    private AccordionCheckContextButton InlineMenuCheckEdit => accordionControlElement_InlineMenuMode.ContextButtons[0] as AccordionCheckContextButton;

    private void ScenarioInfoOnScenarioActivated(Scenario a_s, ScenarioDetail a_sd, ScenarioEvents a_se)
    {
        ReloadBoard();
    }


    private bool m_reloadAutomatically;

    private bool ReloadAutomatically
    {
        get => m_reloadAutomatically;
        set
        {
            m_reloadAutomatically = value;
            m_tileController.ReloadAutomatically = value;
        }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="a_disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool a_disposing)
    {
        if (a_disposing)
        {
            components?.Dispose();
            m_tileController.Dispose();
        }

        base.Dispose(a_disposing);
    }

    public void LoadTileModule(ITileModule a_tilesModule)
    {
        m_tilesModule.Add(a_tilesModule);
    }

    private List<ITileModule> m_tilesModule = new List<ITileModule>();
    private readonly Dictionary<TileInfo, ITileModule> m_unopenedTiles = new Dictionary<TileInfo, ITileModule>();

    private void GenerateTileInfos()
    {
        if (m_tilesModule?.Count > 0)
        {
            foreach (ITileModule module in m_tilesModule)
            {
                foreach (TileInfo tileInfo in module.GenerateTileInfos(m_scenarioInfo))
                {
                    m_unopenedTiles.Add(tileInfo, module); //we store the module so we can generate the tile later
                }
            }

            foreach (TileInfo tileInfo in m_unopenedTiles.Keys)
            {
                ITile primaryTile = null;
                if (tileInfo.Primary)
                {
                    if (primaryTile != null)
                    {
                        throw new DebugException($"This board has more than one primary tile. Existing primar: '{primaryTile.TileKey}'. Second primary '{tileInfo.TileKey}'");
                    }
                }

                LoadMenuItem(tileInfo);
            }

            m_tilesModule = null;

            LoadUserSettings();

            //Now we can start listening to the saved workspace events
            m_mainForm.WorkspaceInfo.CollectUnsavedSettings += WorkspaceInfoOnCollectUnsavedSettings;
        }
    }

    public void SimulationComplete()
    {
        m_tileController.Reload();
    }

    /// <returns>True if UserSettings have changed, false if not</returns>
    public bool SaveUserSettings()
    {
        lock (s_lock)
        {
            Dictionary<string, bool> tiles = new Dictionary<string, bool>();
            List<ITile> activeTileNames = m_tileController.GetActiveTiles();
            foreach (ITile tile in activeTileNames)
            {
                tiles.Add(tile.TileKey, true);
            }

            m_docContainerSettings = m_mainForm.WorkspaceInfo.LoadSetting<DocumentManagerContainer>(c_boardPrefix + BoardControlName);

            if (m_docContainerSettings.SettingKey == null)
            {
                m_docContainerSettings.SettingKey = c_boardPrefix + BoardControlName;
            }

            if (m_docContainerSettings.ActiveDocuments.Count == 0 && tiles.Count == 0)
            {
                return false;
            }

            byte[] data;
            try
            {
                data = tileDocumentManager.SaveLayout();
            }
            catch (Exception)
            {
                //TODO: DevExpress ticket T1251677
                //This can happen if the user is dragging a widget around during layout save
                return false;
            }

            if (IsEqualToActiveDocuments(tiles) &&
                IsEqualToTileLayoutBytes(data))
            {
                return false;
            }

            m_docContainerSettings.ActiveDocuments = tiles;
            m_docContainerSettings.TileLayoutBytes = data;
            m_docContainerSettings.SettingCaption = DisplayName + " tiles layout";

            return true;
        }
    }

    //TODO: IsEqualToActiveDocuments and IsEqualToTileLayoutBytes is also in ScenarioViewer.cs
    // They serve the same purpose and are almost identical code, just use different member variables.
    // It would be nice to get it so that both files could use the same function.
    private bool IsEqualToActiveDocuments(Dictionary<string, bool> a_tiles)
    {
        if (m_docContainerSettings.ActiveDocuments.Count != a_tiles.Count)
        {
            return false;
        }

        foreach ((string boardKey, bool isActive) in m_docContainerSettings.ActiveDocuments)
        {
            if (a_tiles.TryGetValue(boardKey, out bool otherIsActive))
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
        if (m_docContainerSettings.TileLayoutBytes == null &&
            a_data != null)
        {
            return false;
        }

        if (m_docContainerSettings.TileLayoutBytes != null &&
            a_data == null)
        {
            return false;
        }

        if (m_docContainerSettings.TileLayoutBytes == null &&
            a_data == null)
        {
            return true;
        }

        if (m_docContainerSettings.TileLayoutBytes != null &&
            a_data != null)
        {
            return m_docContainerSettings.TileLayoutBytes.SequenceEqual(a_data);
        }

        // The four cases above cover all possibility, but the compiler is forcing me to 
        // have a return statement here so I'm just returning false. 
        return false;
    }

    public void LoadUserSettings()
    {
        try
        {
            m_docContainerSettings = m_mainForm.WorkspaceInfo.LoadSetting<DocumentManagerContainer>(c_boardPrefix + BoardControlName);
            if (m_docContainerSettings.SettingKey == null)
            {
                m_docContainerSettings.SettingKey = c_boardPrefix + BoardControlName;
            }
        }
        catch
        {
            //Old update serializer. Create a new setting
            //TODO: Log this
            m_exceptionManager.LogSimpleException(new Exception("Unable to load boards layout in new version, default layout loaded.".Localize()));
            m_docContainerSettings = new DocumentManagerContainer();
            m_docContainerSettings.SettingKey = c_boardPrefix + BoardControlName;
        }

        for (int i = tileDocumentManager.View.Documents.Count - 1; i >= 0; i--)
        {
            Document doc = (Document)tileDocumentManager.View.Documents[i];
            CloseTile(doc);
        }

        for (int i = tileDocumentManager.View.FloatDocuments.Count - 1; i >= 0; i--)
        {
            Document doc = (Document)tileDocumentManager.View.FloatDocuments[i];
            CloseTile(doc);
        }

        if (m_docContainerSettings.ActiveDocuments.Count == 0)
        {
            //Activate the primary tile
            foreach (TileInfo tileInfo in m_unopenedTiles.Keys)
            {
                if (tileInfo.Primary)
                {
                    m_docContainerSettings.ActiveDocuments.Add(tileInfo.TileKey, true);
                    break;
                }
            }
        }

        foreach (string activeDocumentsKey in m_docContainerSettings.ActiveDocuments.Keys)
        {
            foreach (TileInfo unopenedTileInfo in m_unopenedTiles.Keys)
            {
                //Find the tileinfo for this document key
                if (unopenedTileInfo.TileKey == activeDocumentsKey)
                {
                    //Create the document so we can open the tile
                    MapTile(unopenedTileInfo);
                }
            }

            OpenTile(activeDocumentsKey, null);
        }

        if (m_docContainerSettings.TileLayoutBytes != null)
        {
            tileDocumentManager.LoadLayout(m_docContainerSettings.TileLayoutBytes);
        }

        ReloadAutomatically = true;

        Localize();
    }

    public void Unload()
    {
        m_tileController.Dispose();
    }

    public override void Localize()
    {
        UILocalizationHelper.LocalizeControlsRecursively(Controls);
    }

    private void MapTile(TileInfo a_tileInfo)
    {
        string caption = a_tileInfo.DisplayName;
        Document document = (Document)tileDocumentManager.View.AddDocument(caption, a_tileInfo.TileKey);
        document.Properties.AllowClose = a_tileInfo.Primary ? DefaultBoolean.False : DefaultBoolean.True;
        document.Maximized += DocumentOnMaximized;

        document.Tag = a_tileInfo.Primary;

        document.CustomHeaderButtons.BeginUpdate();
        CustomHeaderButton helpButton = new CustomHeaderButton("", ButtonStyle.PushButton);
        helpButton.SetImage("information");
        helpButton.ImageOptions.SvgImageSize = new Size(16, 16);
        helpButton.Tag = "Help";
        helpButton.SuperTip = new SuperToolTip();
        helpButton.SuperTip.Items.AddTitle("Online Help".Localize());
        helpButton.SuperTip.Items.Add(string.Format("Get more information about the {0} tile online".Localize(), caption));
        document.CustomHeaderButtons.Add(helpButton);

        CustomHeaderButton floatButton = new CustomHeaderButton("", ButtonStyle.PushButton);
        floatButton.SetImage("push_pin");
        floatButton.ImageOptions.SvgImageSize = new Size(16, 16);
        floatButton.Tag = "Float";
        floatButton.SuperTip = new SuperToolTip();
        floatButton.SuperTip.Items.AddTitle("Pin/Unpin Tile".Localize());
        floatButton.SuperTip.Items.Add("Tiles can be unpinned and moved outside of the board. Pinned tiles will stay docked within this board.".Localize());
        document.CustomHeaderButtons.Add(floatButton);
        document.CustomHeaderButtons.EndUpdate();
        document.CustomButtonClick += new ButtonEventHandler(TileMenuPopupHandler);
    }

    private void ViewOnQueryControl(object a_sender, QueryControlEventArgs a_e)
    {
        foreach (TileInfo tileInfo in m_unopenedTiles.Keys)
        {
            if (tileInfo.TileKey == a_e.Document.ControlName)
            {
                ITile generatedTile = m_unopenedTiles[tileInfo].GenerateTile(m_scenarioInfo, tileInfo);

                if (generatedTile == null)
                {
                    throw new PackageException(string.Format("Failed to generate tile {0}".Localize(), tileInfo.TileKey));
                }

                m_unopenedTiles.Remove(tileInfo);
                generatedTile.Primary = tileInfo.Primary;
                try
                {
                    generatedTile.Localize();
                    m_tileController.LoadTile(generatedTile);
                }
                catch (Exception e)
                {
                    m_exceptionManager.LogException(new PackageException($"Error loading Tile '{generatedTile.TileKey.Localize()}'"), true);
                }

                if (generatedTile is Control tileControl)
                {
                    tileControl.Name = tileInfo.TileKey;
                    generatedTile.EnsureHandleCreated();
                    a_e.Control = tileControl;

                    a_e.Document.ImageOptions.SvgImage = tileInfo.GetIconImage();
                    a_e.Document.ImageOptions.SvgImageSize = new Size(16, 16);
                }
                else
                {
                    DebugException.ThrowInDebug($"Tile {tileInfo.TileKey} must be Control.");
                }

                //It's being opened for the first time, activate it
                TileActivated(tileInfo.TileKey);

                break;
            }
        }
    }

    private void DocumentOnMaximized(object a_sender, EventArgs a_e)
    {
        //SaveUserSettings();
    }

    private void TileMenuPopupHandler(object a_sender, ButtonEventArgs a_e)
    {
        switch (a_e.Button.Properties.Tag.ToString())
        {
            case "Help":
                Document document = a_sender as Document;
                GetTileFromDocument(document)?.ShowHelp();
                break;
            case "Float":
                Document floatableDocument = a_sender as Document;
                if (!floatableDocument.IsFloating)
                {
                    FloatDocument(floatableDocument);
                }
                else
                {
                    ITile tileFromDocument = GetTileFromDocument(floatableDocument);
                    if (tileFromDocument != null)
                    {
                        DockTile(tileFromDocument);
                    }
                }

                break;
        }
    }

    private void tileView1_DocumentClosed(object a_sender, DocumentCancelEventArgs a_e)
    {
        if (a_e.Document is Document document)
        {
            CloseTile(document);
            accordionControl1.Invalidate();
        }

        a_e.Cancel = true;

        //TODO: Do from workspace instead
        //SaveUserSettings();
    }

    private void CloseAllTiles()
    {
        List<ITile> tiles = m_tileController.GetActiveTiles();
        foreach (ITile tile in tiles)
        {
            Document document = GetDocumentFromTile(tile);
            CloseTile(document);
        }

    }

    private void CloseFloatingTiles()
    {
        List<ITile> tiles = m_tileController.GetActiveTiles();
        foreach (ITile tile in tiles)
        {
            Document document = GetDocumentFromTile(tile);
            if (document.IsFloating)
            {
                CloseTile(document);
            }
        }

    }

    private void CloseTile(Document a_document)
    {
        ITile tile = GetTileFromDocument(a_document);
        if (tile == null)
        {
            // The control hasn't been opened yet.
            return;
        }

        if (!tile.Primary)
        {
            if (a_document.IsMaximized)
            {
                // Restore from maximized state before closing.
                tile.Maximized = true;
                tileDocumentManager.View.Controller.Restore(a_document);
            }
            else
            {
                tile.Maximized = false;
            }

            // Hide the tile using the tile controller to update internal state.
            m_tileController.HideTile(tile);
        }

        // Dock the tile after hiding if needed
        DockTile(tile);

        // Update the dictionary to indicate the tile is now closed
        if (isBoardOpen.ContainsKey(a_document.ControlName))
        {
            isBoardOpen[a_document.ControlName] = false;
        }
    }

    private void DockTile(ITile a_tile)
    {
        Document document = GetDocumentFromTile(a_tile);
        if (document.IsFloating)
        {
            tileDocumentManager.View.Controller.Dock(document);
        }
    }

    private ITile? GetTileFromDocument(Document a_document)
    {
        return a_document?.Control as ITile;
    }

    private Document GetDocumentFromTile(ITile a_tile)
    {
        foreach (BaseDocument document in tileDocumentManager.View.Documents)
        {
            if (document.ControlName == a_tile.TileKey)
            {
                return (Document)document;
            }
        }

        foreach (BaseDocument document in tileDocumentManager.View.FloatDocuments)
        {
            if (document.Control.Name == a_tile.TileKey)
            {
                return (Document)document;
            }
        }

        return null;
    }

    public ITile TileActivated(string a_tileKey)
    {
        ITile tile = m_tileController.ActivateTile(a_tileKey);
        if (tile == null)
        {
            //Tile was not activated, nothing to do.
            return null;
        }

        Document tileDocument = GetDocumentFromTile(tile);
        if (!tileDocument.IsFloating)
        {
            TilePreferences tilePreferences = m_mainForm.UserPreferenceInfo.LoadSetting<TilePreferences>(TilePreferences.GlobalSettingKey);
            if (tilePreferences.OpenTileInWindow)
            {
                //We can't float yet, the queryControl event hasn't returned
                BeginInvoke(() => FloatDocument(tileDocument));
            }
        }

        if (tile.Maximized)
        {
            tileDocumentManager.View.Controller.Maximize(tileDocument);
        }

        //Without this, sometimes the tile won't appear unless the dockmanager are is clicked
        tileDocumentManager.View.ActivateDocument(tile as Control);

        return tile;
    }

    private void FloatDocument(Document a_document)
    {
        Point startingLocation;
        if (IsDocumentOnScreen(a_document))
        {
            startingLocation = a_document.FloatLocation.Value;
        }
        else
        {
            int offset = m_tileOffsetMultiplier * c_baseOffset;
            startingLocation = Location + new Size(accordionControl1.Width + offset, accordionControl1.Height / 5 + offset);
            m_tileOffsetMultiplier++;
            m_tileOffsetMultiplier %= 7; //Need to reset the multiplier every so often in case we have a ton of tiles so they don't start off the screen

            // lucky number 7, no other meaning behind this
        }

        tileDocumentManager.View.Controller.Float(a_document, startingLocation);

        Size? floatSize = a_document.FloatSize;
        if (floatSize.Value is Size s)
        {
            if (s.Height <= m_mainForm.CurrentDpiScaler * 30)
            {
                ITile tile = GetTileFromDocument(a_document);
                if (tile != null)
                {
                    //The tile is not showing. Update the size and location
                    a_document.Form.Size = tile.GetDefaultFloatSize();
                }
            }
        }
    }

    /// <summary>
    /// Function name is self-explanatory. This will return true as long as the top left corner of the control is on the screen.
    /// </summary>
    /// <param name="a_document"></param>
    /// <returns>A bool that is true if the Document is on the screen, false if not</returns>
    private static bool IsDocumentOnScreen(Document a_document)
    {
        if (a_document?.FloatLocation == null)
        {
            return false;
        }

        Point documentCorner = a_document.FloatLocation.Value;
        Size floatSize = a_document.FloatSize ?? new Size();
        foreach (Screen screen in Screen.AllScreens)
        {
            Rectangle topLeft = new Rectangle(documentCorner.X, documentCorner.Y, a_document.Width, a_document.Height);
            Point bottomRightRectsTopLeftCorner = new Point(documentCorner.X - a_document.Width + floatSize.Width, documentCorner.Y - a_document.Height + floatSize.Height);
            Rectangle bottomRight = new Rectangle(bottomRightRectsTopLeftCorner.X, bottomRightRectsTopLeftCorner.Y, a_document.Width, a_document.Height);
            if (screen.WorkingArea.Contains(topLeft))
            {
                return true;
            }

            if (screen.WorkingArea.Contains(bottomRight))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Represents whether this board is open. The board may not be visible to the user (if on a background tab)
    /// </summary>
    public bool BoardVisible
    {
        get => Visible;
        set
        {
            if (value)
            {
                GenerateTileInfos();
                Task.Run(new Action(() => m_tileController.Enable()));
                Visible = true;
            }
            else
            {
                Visible = false;
                CloseFloatingTiles();
                Task.Run(new Action(() => m_tileController.Disable()));
            }
        }
    }

    private bool m_boardActive;

    /// <summary>
    /// Represents whether this board can be seen by the user
    /// </summary>
    public bool BoardActive
    {
        get => m_boardActive;
        set
        {
            m_boardActive = value;

            //Update the controller to know how to process tile updates
            m_tileController.SetForeground(value);
        }
    }

    private BaseDocument GetDocumentByKey(string a_documentKey)
    {
        foreach (BaseDocument document in tileDocumentManager.View.Documents)
        {
            if (document.ControlName == a_documentKey)
            {
                return document;
            }
        }

        foreach (BaseDocument viewFloatDocument in tileDocumentManager.View.FloatDocuments)
        {
            if (viewFloatDocument.ControlName == a_documentKey)
            {
                return viewFloatDocument;
            }
        }
        return null;
    }

    public void OpenTile(string a_tileKey, UINavigationEvent a_navEvent)
    {
        BaseDocument document = GetDocumentByKey(a_tileKey);
        if (document != null)
        {
            tileDocumentManager.View.Controller.Activate(document);
        }
        else
        {
            bool tileKeyMatchesTile = false;
            // Find the unopened tile info by key
            foreach (TileInfo unopenedTilesKey in m_unopenedTiles.Keys)
            {
                if (unopenedTilesKey.TileKey == a_tileKey)
                {
                    MapTile(unopenedTilesKey);
                    tileKeyMatchesTile = true;
                    break;
                }
            }

            if (!tileKeyMatchesTile)
            {
                return;
            }

            // Create the tile control
            document = GetDocumentByKey(a_tileKey);
            tileDocumentManager.View.Controller.Activate(document);
        }

        ITile tile = TileActivated(a_tileKey);
        
        // Update isBoardOpen dictionary to reflect that this tile is now open
        isBoardOpen[a_tileKey] = true;

        // Since a tile has been opened, minimize the accordion
        if (accordionControl1.OptionsMinimizing.State == AccordionControlState.Normal)
        {
            //accordionControl1.OptionsMinimizing.State = AccordionControlState.Minimized;
        }

        if (a_navEvent != null)
        {
            if (tile == null)
            {
                // The tile is already active
                ITile[] activeTiles = m_tileController.GetActiveTiles().Where(x => x.TileKey == a_tileKey).ToArray();
                if (activeTiles.Length > 0)
                {
                    tile = activeTiles[0];
                }
                else
                {
                    DebugException.ThrowInDebug($"Navigation Event attempted to open tile '{a_tileKey}' but it has not been loaded");
                }
            }

            if (tile is IUIEventTile uiTile)
            {
                Task.Run(new Action(() => uiTile.ProcessUIEventData(a_navEvent)));
            }
        }
    }

    public ITile GetPrimaryTile()
    {
        return m_tileController.GetPrimaryTile();
    }

    public void ReloadBoard()
    {
        m_tileController.Reload();
    }

    #region Tiles
    private void LoadMenuItem(TileInfo a_tile)
    {
        //Insert tile according to priority
        int position = 0;
        for (int i = 0; i < accordionControlElement_Tiles.Elements.Count; i++)
        {
            if (accordionControlElement_Tiles.Elements[i].Tag is int priority)
            {
                if (priority >= a_tile.Priority)
                {
                    position = i + 1;
                }
            }
        }

        AccordionControlElement accordionControlElement = accordionControl1.AddGroup(accordionControlElement_Tiles, a_tile.TileKey, a_tile.DisplayName, a_tile.GetIconImage(), a_tile.Description.Localize(), position, a_tile.Priority);
        
        if (a_tile.Primary)
        {
            accordionControlElement.Visible = false;
        }
    }

    private readonly Dictionary<string, bool> isBoardOpen = new Dictionary<string, bool>();

    private void accordionControl1_ElementClick(object sender, ElementClickEventArgs e)
    {
        // Check for special cases
        if (e.Element == accordionControlElement_CloseAll)
        {
            CloseAllTiles();
            accordionControl1.Invalidate();
            return;
        }

        if (e.Element == accordionControlElement_ShowMode)
        {
            OpenTileCheckEdit.Checked = !OpenTileCheckEdit.Checked;
            UpdateTilePreferences();
            return;
        }

        if (e.Element == accordionControlElement_InlineMenuMode)
        {
            InlineMenuCheckEdit.Checked = !InlineMenuCheckEdit.Checked;
            bool pinned = InlineMenuCheckEdit.Checked;

            accordionControl1.BeginUpdate();
            accordionControl1.OptionsHamburgerMenu.DisplayMode = pinned ? AccordionControlDisplayMode.Inline : AccordionControlDisplayMode.Overlay;
            accordionControl1.EndUpdate();
            accordionControl1.Refresh();

            UpdateTilePreferences();
            return;
        }

        string elementName = e.Element.Name;

        // Toggle tile open/close based on current state
        if (isBoardOpen.ContainsKey(elementName) && isBoardOpen[elementName])
        {
            // Tile is open, close it
            CloseTile((Document)GetDocumentByKey(elementName));
        }
        else
        {
            // Tile is closed, open it
            OpenTile(elementName, null);
        }
    }

    private void UpdateTilePreferences()
    {
        TilePreferences tilePreferences = m_mainForm.UserPreferenceInfo.LoadSetting<TilePreferences>(TilePreferences.GlobalSettingKey);
        tilePreferences.OpenTileInWindow = OpenTileCheckEdit.Checked;
        tilePreferences.InlineMenuDisplayMode = InlineMenuCheckEdit.Checked;

        accordionControlElement_ShowMode.ImageOptions.SvgImage = PtImageCache.GetImage(OpenTileCheckEdit.Checked ? "pushPin" : "workspaces");
        m_mainForm.UserPreferenceInfo.SaveSetting(tilePreferences);
    }

    private void AccordionControl1OnStateChanged(object a_sender, EventArgs a_e)
    {
        if (accordionControl1.OptionsMinimizing.State == AccordionControlState.Minimized && accordionControl1.ActiveGroup == accordionControlElement_Settings)
        {
            accordionControl1.ActiveGroup = accordionControlElement_Tiles;
        }
    }
    #endregion TILES
}