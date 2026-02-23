using PT.UIDefinitions.ControlSettings;

namespace PT.UIDefinitions.Interfaces;

public interface IUserSettingsManager
{
    SingleInventoryPlotSettings LoadSingleInventoryPlotSettings();
    void SaveSingleInventoryPlotSettings(SingleInventoryPlotSettings a_singleInventoryPlotSettings);

    InventoryPlanSettings LoadInventoryPlanSettings();
    void SaveInventoryPlanSettings(InventoryPlanSettings a_inventoryPlanSettings);

    CtpSettings LoadCtpSettings();
    void SaveCtpSettings(CtpSettings a_ctpSettings);

    WorkspaceLayoutSettings LoadWorkspaceLayoutSettings();
    void SaveWorkspaceLayoutSettings(WorkspaceLayoutSettings a_workspaceLayoutSettings);

    AnalyticsSettings LoadAnalyticsSettings();
    void SaveAnalyticsSettings(AnalyticsSettings a_analyticstSettings);

    AlertManagerSettings LoadAlertManagerSettings();
    void SaveAlertManagerSettings(AlertManagerSettings a_alertManagerSettings);

    CapacityPlanSettings LoadCapacityPlanSettings();
    void SaveCapacityPlanSettings(CapacityPlanSettings a_capacityPlanSettings);

    ProductRulesSettings LoadProductRulesSettings();
    void SaveProductRulesSettings(ProductRulesSettings a_productRulesSettings);

    DatabaseManagerSettings LoadDatabaseManagerSettings();
    void SaveDatabaseManagerSettings(DatabaseManagerSettings a_databaseManagerSettings);

    ProcessViewerSettings LoadProcessViewerSettings();
    void SaveProcessViewerSettings(ProcessViewerSettings a_processViewerSettings);

    MappingWizardSettings LoadMappingWizardSettings();
    void SaveMappingWizardSettings(MappingWizardSettings a_mappingWizardSettings);

    ScheduleReportSettings LoadScheduleReportSettings();
    void SaveScheduleReportSettings(ScheduleReportSettings a_scheduleReportSettings);
}