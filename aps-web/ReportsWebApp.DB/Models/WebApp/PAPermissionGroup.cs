using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ReportsWebApp.DB.Models
{
    public class PAPermissionGroup : BaseEntity
    {
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public List<PAUserPermission> Permissions { get; set; } = new();

        public override string TypeDisplayName => "Planning Area Permission Group";
    }

    public class UserPermissionKeys
    {
        public static readonly string Scheduling = "Scheduling";
        public static readonly string Planning = "Planning";
        public static readonly string Publish = "Publish";
        public static readonly string AdvanceClock = "Advance Clock";
        public static readonly string ImportData = "Import Data";
        public static readonly string UndoRedo = "Undo/Redo";
        public static readonly string SchedulePlant = "Schedule Plant";
        public static readonly string ViewJobsInPlant = "View Jobs In Plant";
        public static readonly string ViewInventoryInPlant = "View Inventory In Plan";
        public static readonly string ControlAddIns = "Manage Add-Ins";
        public static readonly string UndoErpActions = "Undo ERP Actions";
        public static readonly string CreateNewScenarios = "Create Scenarios";
        public static readonly string RefreshMultipleScenarios = "Refresh Multiple Scenarios";
        public static readonly string MaintainCapacity = "Maintain Capacity";
        public static readonly string MaintainJobs = "Maintain Jobs";
        public static readonly string MaintainCustomers = "Maintain Customers";
        public static readonly string MaintainInventory = "Maintain Inventory";
        public static readonly string MaintainPurchaseOrders = "Maintain Purchase Orders";
        public static readonly string MaintainSalesOrders = "Maintain Sales Orders";
        public static readonly string MaintainManufacturingOrders = "Maintain Manufacturing Orders";
        public static readonly string MaintainResources = "Maintain Resources";
        public static readonly string MaintainForecasts = "Maintain Forecasts";
        public static readonly string MaintainOvertime = "Maintain Overtime";
        public static readonly string MrpOptimizeMainBarButton = "MRP Optimize";
        public static readonly string OptimizeScheduleMainBarButton = "Optimize Schedule";
        public static readonly string CompressMainBarButton = "Compress";
        public static readonly string RunReports = "Run Reports";
        public static readonly string ShowUnavailableUndoActions = "Show Unavailable Undo Actions";
        public static readonly string ReserveCTP = "Reserve CTP";
        public static readonly string ViewErrorLog = "View Error Log";
        public static readonly string DownloadScenarios = "Download Scenarios";
        public static readonly string SplitOp = "Split Operation";
        public static readonly string SplitJob = "Split Job";
        public static readonly string SplitMO = "Split MO";
        public static readonly string SplitJoin = "Join Job or Manufacturing Order";

        //Board Level
        public static readonly string BoardAnalytics = "AnalyticsBoard";
        public static readonly string BoardPublishToAnalytics = "PublishToAnalyticsBoard";
        public static readonly string BoardJobs = "JobsBoard";
        public static readonly string BoardCustomers = "CustomersBoard";
        public static readonly string BoardInventoryPlan = "InventoryPlanBoard";
        public static readonly string BoardTemplates = "RoutingTemplatesBoard";
        public static readonly string BoardActivities = "ActivitiesBoard";
        public static readonly string BoardDatabaseManager = "DatabaseManagerBoard";
        public static readonly string BoardUsers = "UsersBoard";
        public static readonly string BoardKPIs = "KPIBoard";
        public static readonly string BoardSystemOptions = "SystemOptionsBoard";
        public static readonly string BoardMappingWizard = "MappingWizardBoard";
        public static readonly string BoardWorkspaceManagement = "WorkspaceManagementBoard";
        public static readonly string LocalBoardWorkspaceManagement = "WorkspacesLocalManagement";
        public static readonly string SharedBoardWorkspaceManagement = "WorkspacesSharedManagement";
        public static readonly string BoardScenarioData = "ScenarioDataBoard";
        public static readonly string BoardScenarioHistory = "ScenarioHistoryBoard";
        public static readonly string BoardProcessFlow = "ProcessFlowBoard";
        public static readonly string BoardMetrics = "MetricsBoard";
        public static readonly string BoardForecast = "ForecastBoard";
        public static readonly string BoardMaterials = "MaterialsBoard";
        public static readonly string BoardSalesOrders = "SalesOrdersBoard";
        public static readonly string BoardPurchaseOrders = "PurchaseOrdersBoard";
        public static readonly string BoardGantt = "GanttBoard";
        public static readonly string BoardImportMapping = "ImportMappingBoard";
        public static readonly string BoardScenarioManagement = "ScenarioManagementBoard";
        public static readonly string BoardBufferManagement = "BufferManagementBoard";
        public static readonly string BoardReportDesigner = "ReportDesignerBoard";
        public static readonly string BoardSequencePlanning = "SequencePlanningBoard";
        public static readonly string BoardDataModelingAndTesting = "DataModelingAndTestingBoard";
        public static readonly string BoardCapacityPlanning = "CapacityPlanningBoard";
    }

    public static class PermissionsGroupConstants
    {
        public static readonly string Scheduling = "Scheduling";
        public static readonly string Planning = "Planning";
        public static readonly string Modeling = "Modeling";
        public static readonly string Import = "Import";
        public static readonly string PublishGroup = "Publish";
        public static readonly string UndoRedoGroup = "Undo Redo";
        public static readonly string PlantsGroup = "Plants";
        public static readonly string MiscellaneousGroup = "Miscellaneous";
        public static readonly string BoardsGroup = "Boards";
        public static readonly string CapacityGroup = "Capacity";
        public static readonly string MainBarGroup = "Main Bar Controls";
        public static readonly string Custom = "Custom";

        public static readonly List<string> DefaultGroups = new List<string>
            {
                Scheduling,
                Planning,
                Modeling,
                Import,
                PublishGroup,
                UndoRedoGroup,
                PlantsGroup,
                MiscellaneousGroup,
                BoardsGroup,
                CapacityGroup,
                MainBarGroup,
                Custom
            };

        public static List<PAUserPermission> DefaultPermissions = new() {

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_Metrics",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardMetrics,
                Caption = "Metrics Dashboard",
                Description = "User can access the Metrics Dashboard."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_Analytics",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardAnalytics,
                Caption = "Analytics",
                Description = "User can access the Analytics board."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_PublishToAnalytics",
                GroupKey = PermissionsGroupConstants.PublishGroup,
                PermissionKey = UserPermissionKeys.BoardPublishToAnalytics,
                Caption = "Publish to analytics",
                Description = "Analytics Dashboard Control is available."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_KPIsBoard",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardKPIs,
                Caption = "KPIs",
                Description = "User can access the KPI board"
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_Customer",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardCustomers,
                Caption = "Customer",
                Description = "User can access the Customers board."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_UndoRedo",
                GroupKey = PermissionsGroupConstants.UndoRedoGroup,
                PermissionKey = UserPermissionKeys.UndoRedo,
                Caption = "Undo-Redo",
                Description = "User can use the Undo-Redo controls."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_ControlAddIns",
                GroupKey = PermissionsGroupConstants.MiscellaneousGroup,
                PermissionKey = UserPermissionKeys.ControlAddIns,
                Caption = "Control Add-Ins",
                Description = "User can manage add-ins."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_UndoErpActions",
                GroupKey = PermissionsGroupConstants.UndoRedoGroup,
                PermissionKey = UserPermissionKeys.UndoErpActions,
                Caption = "Undo ERP Actions",
                Description = "User can undo ERP actions."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_Users",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardUsers,
                Caption = "Users",
                Description = "User can access the Users board."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_WorkspaceManagement",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardWorkspaceManagement,
                Caption = "Workspace Management",
                Description = "User can access the Workspace Management board."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_LocalWorkspaceManagement",
                GroupKey = PermissionsGroupConstants.MiscellaneousGroup,
                PermissionKey = UserPermissionKeys.LocalBoardWorkspaceManagement,
                Caption = "Local Workspace Management",
                Description = "User can edit local profiles if they have access workspace management board."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_SharedWorkspaceManagement",
                GroupKey = PermissionsGroupConstants.MiscellaneousGroup,
                PermissionKey = UserPermissionKeys.SharedBoardWorkspaceManagement,
                Caption = "Shared Workspace Management",
                Description = "User can also edit shared profiles if they have permission to edit local profiles."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_CompressMainBarButton",
                GroupKey = PermissionsGroupConstants.MainBarGroup,
                PermissionKey = UserPermissionKeys.CompressMainBarButton,
                Caption = "Compress",
                Description = "User can use the Compress button on the main bar."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_CompressMainBarButton",
                GroupKey = PermissionsGroupConstants.MiscellaneousGroup,
                PermissionKey = UserPermissionKeys.RunReports,
                Caption = "Run Reports",
                Description = "User can run reports."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_MaintainOvertime",
                GroupKey = PermissionsGroupConstants.CapacityGroup,
                PermissionKey = UserPermissionKeys.MaintainOvertime,
                Caption = "Maintain Overtime",
                Description = "User can edit Overtime settings."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_ShowUnavailableUndoActions",
                GroupKey = PermissionsGroupConstants.UndoRedoGroup,
                PermissionKey = UserPermissionKeys.ShowUnavailableUndoActions,
                Caption = "Show Unavailable Undo Actions",
                Description = "User can view unavailable actions in the Undo-Redo drop-down."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_ViewErrorLog",
                GroupKey = PermissionsGroupConstants.MiscellaneousGroup,
                PermissionKey = UserPermissionKeys.ViewErrorLog,
                Caption = "View Error Log",
                Description = "User can view the error log."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_MaintainCustomers",
                GroupKey = PermissionsGroupConstants.MiscellaneousGroup,
                PermissionKey = UserPermissionKeys.MaintainCustomers,
                Caption = "Maintain Customers",
                Description = "User can maintain/manage customers."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_ReserveCtp",
                GroupKey = PermissionsGroupConstants.Planning,
                PermissionKey = UserPermissionKeys.ReserveCTP,
                Caption = "Reserve CTP",
                Description = "User can reserve CTP."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_MaintainForecasts",
                GroupKey = PermissionsGroupConstants.Modeling, //Not sure if this group is appropriate for this permission.
                PermissionKey = UserPermissionKeys.MaintainForecasts,
                Caption = "Maintain Forecasts",
                Description = "User can maintain Forecasts data."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_MappingWizard",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardMappingWizard,
                Caption = "Import Mappings",
                Description = "User can access the Import Mappings board."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_DownloadScenarios",
                GroupKey = PermissionsGroupConstants.Modeling,
                PermissionKey = UserPermissionKeys.DownloadScenarios,
                Caption = "Download Scenarios",
                Description = "User can download Scenarios."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_Materials",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardMaterials,
                Caption = "Materials",
                Description = "User can access the Materials board"
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_MrpOptimizeMainBarButton",
                GroupKey = PermissionsGroupConstants.MainBarGroup,
                PermissionKey = UserPermissionKeys.MrpOptimizeMainBarButton,
                Caption = "MRP Optimize",
                Description = "User can run MRP optimize actions."
            },
            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_Scheduling",
                GroupKey = PermissionsGroupConstants.Scheduling,
                PermissionKey = UserPermissionKeys.Scheduling,
                Caption = "Perform Scheduling",
                Description = "User can access the Scheduling controls and perform scheduling actions"
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_Planning",
                GroupKey = PermissionsGroupConstants.Planning,
                PermissionKey = UserPermissionKeys.Planning,
                Caption = "Scenario Planning",
                Description = "User can access the Planning controls."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_Publish",
                GroupKey = PermissionsGroupConstants.PublishGroup,
                PermissionKey = UserPermissionKeys.Publish,
                Caption = "Publish",
                Description = "User can access the Publishing controls."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_AdvanceClock",
                GroupKey = PermissionsGroupConstants.Planning,
                PermissionKey = UserPermissionKeys.AdvanceClock,
                Caption = "Advance Clock",
                Description = "User may use the Advance Clock control to advance the schedule."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_ImportData",
                GroupKey = PermissionsGroupConstants.Import,
                PermissionKey = UserPermissionKeys.ImportData,
                Caption = "Refresh Planning Data",
                Description = "User can refresh planning data."
            },

            new PAUserPermission()
            {
                PackageObjectId = "plantPermissionElement_Schedule",
                GroupKey = PermissionsGroupConstants.PlantsGroup,
                PermissionKey = UserPermissionKeys.SchedulePlant,
                Caption = "Schedule Plant",
                Description = "User can schedule jobs in a given plant."
            },

            new PAUserPermission()
            {
                PackageObjectId = "plantPermissionElement_ViewJobs",
                GroupKey = PermissionsGroupConstants.PlantsGroup,
                PermissionKey = UserPermissionKeys.ViewJobsInPlant,
                Caption = "View jobs scheduled in plant",
                Description = "User can view jobs in a given plant."
            },

            new PAUserPermission()
            {
                PackageObjectId = "plantPermissionElement_ViewInventory",
                GroupKey = PermissionsGroupConstants.PlantsGroup,
                PermissionKey = UserPermissionKeys.ViewInventoryInPlant,
                Caption = "View inventory in plant warehouses",
                Description = "User can view warehouse inventory in a given plant."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_Jobs",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardJobs,
                Caption = "Jobs",
                Description = "User can access the Jobs board."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_InventoryPlan",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardInventoryPlan,
                Caption = "Inventory Plan",
                Description = "User can access the Inventory Plan board."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_Templates",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardTemplates,
                Caption = "Routing Templates",
                Description = "User can access the Routing Templates board."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_Activities",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardActivities,
                Caption = "Activities",
                Description = "User can access the Activities board."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_DatabaseManager",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardDatabaseManager,
                Caption = "Database Manager",
                Description = "User can access the Database Manager board."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_SystemOptions",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardSystemOptions,
                Caption = "System Options",
                Description = "User can access the System Settings board."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_ScenarioData",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardScenarioData,
                Caption = "Scenario Data",
                Description = "User can access the Scenario Data board."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_CapacityPlan",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardCapacityPlanning,
                Caption = "Capacity Plan",
                Description = "User can access the Capacity Plan board."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_SequencePlanning",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardSequencePlanning,
                Caption = "Sequence Planning",
                Description = "User can access the Sequence Planning board."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_OptimizeScheduleMainBarButton",
                GroupKey = PermissionsGroupConstants.MainBarGroup,
                PermissionKey = UserPermissionKeys.OptimizeScheduleMainBarButton,
                Caption = "Optimize Schedule",
                Description = "User can optimize the schedule"
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_MaintainJobs",
                GroupKey = PermissionsGroupConstants.Modeling,
                PermissionKey = UserPermissionKeys.MaintainJobs,
                Caption = "Maintain Jobs",
                Description = "User can maintain Job data."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_MaintainInventory",
                GroupKey = PermissionsGroupConstants.Modeling,
                PermissionKey = UserPermissionKeys.MaintainInventory,
                Caption = "Maintain Inventory",
                Description = "User can maintain Inventory data."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_MaintainResources",
                GroupKey = PermissionsGroupConstants.Modeling,
                PermissionKey = UserPermissionKeys.MaintainResources,
                Caption = "Maintain Resources",
                Description = "User can maintain Resource data."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_MaintainPurchaseOrders",
                GroupKey = PermissionsGroupConstants.Modeling,
                PermissionKey = UserPermissionKeys.MaintainPurchaseOrders,
                Caption = "Maintain Purchase Orders",
                Description = "User can maintain Purchase Orders."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_MaintainSalesOrders",
                GroupKey = PermissionsGroupConstants.Modeling,
                PermissionKey = UserPermissionKeys.MaintainSalesOrders,
                Caption = "Maintain Sales Orders",
                Description = "User can maintain Sales Orders."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_MaintainManufacturingOrders",
                GroupKey = PermissionsGroupConstants.Modeling,
                PermissionKey = UserPermissionKeys.MaintainManufacturingOrders,
                Caption = "Maintain Manufacturing Orders",
                Description = "User can maintain Manufacturing Orders."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_PurchaseOrders",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardPurchaseOrders,
                Caption = "Purchase Orders",
                Description = "User can access the Purchase Orders board"
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_SalesOrders",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardSalesOrders,
                Caption = "Sales Orders",
                Description = "User can access the Sales Orders board"
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_CreateNewScenarios",
                GroupKey = PermissionsGroupConstants.Scheduling,
                PermissionKey = UserPermissionKeys.CreateNewScenarios,
                Caption = "Can Create Scenarios",
                Description = "Can create new scenarios"
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_RefreshMultipleScenarios",
                GroupKey = PermissionsGroupConstants.Scheduling,
                PermissionKey = UserPermissionKeys.RefreshMultipleScenarios,
                Caption = "Multiple Scenario Data Refresh",
                Description = "Can import data on all scenarios"
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_MaintainCapacity",
                GroupKey = PermissionsGroupConstants.CapacityGroup,
                PermissionKey = UserPermissionKeys.MaintainCapacity,
                Caption = "Maintain Capacity",
                Description = "Can maintain capacity, make changes to capacity intervals, etc."
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_SplitOp",
                GroupKey = PermissionsGroupConstants.Scheduling,
                PermissionKey = UserPermissionKeys.SplitOp,
                Caption = "Split Operation",
                Description = "User can split Operations"
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_SplitJob",
                GroupKey = PermissionsGroupConstants.Scheduling,
                PermissionKey = UserPermissionKeys.SplitJob,
                Caption = "Split Job",
                Description = "User can split Jobs"
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_SplitMO",
                GroupKey = PermissionsGroupConstants.Scheduling,
                PermissionKey = UserPermissionKeys.SplitMO,
                Caption = "Split MO",
                Description = "User can split MO's"
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_SplitJoin",
                GroupKey = PermissionsGroupConstants.Scheduling,
                PermissionKey = UserPermissionKeys.SplitJoin,
                Caption = "Join Job or Manufacturing Order",
                Description = "User can Join multiple Jobs or Manufacturing Orders into a single Job/MO"
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_BufferManagement",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardBufferManagement,
                Caption = "Buffer Management",
                Description = "User can access the Buffer Management board"
            },

            new PAUserPermission()
            {
                PackageObjectId = "userPermissionElement_Gantt",
                GroupKey = PermissionsGroupConstants.BoardsGroup,
                PermissionKey = UserPermissionKeys.BoardGantt,
                Caption = "Gantt",
                Description = "User can access the Gantt board"
            }
        };
    }
}
