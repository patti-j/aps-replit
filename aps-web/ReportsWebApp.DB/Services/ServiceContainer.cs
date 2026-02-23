using Microsoft.JSInterop;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;
using ReportsWebApp.DB.Services.SchedulerHelpers;

namespace ReportsWebApp.DB.Services
{
    public class ServiceContainer
    {    
        /// <summary>
        /// Stores the time zone ID (e.g., "America/New_York") detected from the browser or selected in user profile.
        /// </summary>
        public string UserTimeZone { get; set; } = "UTC"; // Default fallback
        public IJSRuntime JSRuntime { get; }
        public HierarchyFilterService HierarchyFilterService { get; }
        public ISchedulerFavoritesService FavoritesService { get; }
        public ICosmosDbService<GanttFavoriteData> CosmosDbService { get; }
        public PopupManagerService PopupManager { get; }
        public IUserService UserService { get; }
        public ICompanyService CompanyService { get; }
        public ScopedAppStateService ScopedAppStateService { get; }        
        public ICompanyDbService CompanyDbService { get; }
        public ShortcutService ShortcutService { get; }
        public IPlanningAreaDataService PlanningAreaDataService { get; }
        public ICTPRequestService CTPRequestService { get; }
        public IServerManagerService ServerManagerService { get; }
        public IExternalIntegrationService ExternalIntegrationService { get; }
        public IPlanningAreaAssignmentPropagationService PlanningAreaAssignmentPropagationService { get; }
        public IDataConnectorService DataConnectorService { get; }
        public IGanttService GanttService { get; }
        public IGanttDataService GanttDataService { get; }
        
        public ServiceContainer(
            IJSRuntime jsRuntime,
            HierarchyFilterService hierarchyFilterService,
            ISchedulerFavoritesService favoritesService,
            ICosmosDbService<GanttFavoriteData> cosmosDbService,
            PopupManagerService popupManager,
            IUserService userService,
            ICompanyDbService companyDbService,
            ShortcutService shortcutService,
            IPlanningAreaDataService planningAreaDataService,
            ICTPRequestService cTPRequestService,
            IServerManagerService serverManagerService,
            ICompanyService companyService,
            ScopedAppStateService scopedAppStateService,
            IExternalIntegrationService externalIntegrationService,
            IPlanningAreaAssignmentPropagationService planningAreaAssignmentPropagationService,
            IDataConnectorService dataConnectorService,
            IGanttService ganttService,
            IGanttDataService ganttDataService)
        {
            JSRuntime = jsRuntime;
            GanttDataService = ganttDataService;
            HierarchyFilterService = hierarchyFilterService;
            FavoritesService = favoritesService;
            CosmosDbService = cosmosDbService;
            PopupManager = popupManager;
            UserService = userService;
            CompanyDbService = companyDbService;
            ShortcutService = shortcutService;
            PlanningAreaDataService = planningAreaDataService;
            CTPRequestService = cTPRequestService;
            ServerManagerService = serverManagerService;
            CompanyService = companyService;
            ScopedAppStateService = scopedAppStateService;
            ExternalIntegrationService = externalIntegrationService;
            DataConnectorService = dataConnectorService;
            GanttService = ganttService;
        }
    }

}
