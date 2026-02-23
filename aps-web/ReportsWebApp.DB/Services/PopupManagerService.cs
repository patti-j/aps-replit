namespace ReportsWebApp.DB.Services
{
    public class PopupManagerService
    {
        private bool showActivityPopup = false;
        private bool showFavoritesPopup = false;
        private bool showKeyboardShortcutsPopup = false;
        private bool showResourcesPopup = false;
        private bool showHierarchyFilterPopup = false;
        private bool showPlanningAreaSelector = false;
        private bool showCTPRequestsPopup = false;
        private bool showAddCTPRequestPopup = false;
        private bool showZoomPanel = false;

        public event Action OnChange;
        
        public bool ShowActivityPopup
        {
            get => showActivityPopup;
            set { showActivityPopup = value; NotifyStateChanged(); }
        }
        public bool ShowFavoritesPopup
        {
            get => showFavoritesPopup;
            set { showFavoritesPopup = value; NotifyStateChanged(); }
        }
        public bool ShowKeyboardShortcutsPopup
        {
            get => showKeyboardShortcutsPopup;
            set { showKeyboardShortcutsPopup = value; NotifyStateChanged(); }
        }
        public bool ShowCTPRequestsPopup
        {
            get => showCTPRequestsPopup;
            set { showCTPRequestsPopup = value; NotifyStateChanged(); }
        }
        public bool ShowZoomPanel
        {
            get => showZoomPanel;
            set { showZoomPanel = value; NotifyStateChanged(); }
        }
        public bool ShowAddCTPRequestPopup
        {
            get => showAddCTPRequestPopup;
            set { showAddCTPRequestPopup = value; NotifyStateChanged(); }
        }

        public bool ShowResourcesPopup
        {
            get => showResourcesPopup;
            set { showResourcesPopup = value; NotifyStateChanged(); }
        }

        public bool ShowHierarchyFilterPopup
        {
            get => showHierarchyFilterPopup;
            set { showHierarchyFilterPopup = value; NotifyStateChanged(); }
        }
        public bool ShowPlanningAreaSelector
        {
            get => showPlanningAreaSelector;
            set { showPlanningAreaSelector = value; NotifyStateChanged(); }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public async Task OnShowActivityPopup()
        {
            ResetPopups();
            ShowActivityPopup = true;
        }

        public async Task OnShowFavoritesPopup()
        {
            ResetPopups();
            ShowFavoritesPopup = true;
        }
        public async Task OnShowKeyboardShortcutsPopup()
        {
            ResetPopups();
            ShowKeyboardShortcutsPopup = true;
        }
        public async Task OnShowAddCTPRequestPopup()
        {
            ResetPopups();
            ShowAddCTPRequestPopup = true;
        }        
        public async Task OnShowZoomPanel()
        {
            ResetPopups();
            ShowZoomPanel = true;
        }
        public async Task OnShowCTPRequestsPopup()
        {
            ResetPopups();
            ShowCTPRequestsPopup = true;
        }

        public async Task OnShowHierarchyFilterPopup()
        {
            ResetPopups();
            ShowHierarchyFilterPopup = true;
        }

        public async Task OnShowResourceManagementPopup()
        {
            ResetPopups();
            ShowResourcesPopup = true;
        }
        public void ResetPopups()
        {
            ShowActivityPopup = false;
            ShowFavoritesPopup = false;
            ShowResourcesPopup = false;
            ShowKeyboardShortcutsPopup = false;
            ShowHierarchyFilterPopup = false;
        }
    }
}
