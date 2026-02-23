using System.Reflection;

namespace PT.PackageDefinitions;

public interface ISettingData : IPTDeserializable
{
    string SettingKey { get; }
    string SettingCaption { get; }
    string Description { get; }
    string SettingsGroup { get; }
    string SettingsGroupCategory { get; }
}

//TODO: Turn these into classes that provide images and other data to load into the accordian control
public static class SettingGroupConstants
{
    /// <summary>
    /// Settings Group Constants
    /// </summary>
    public static readonly string GanttSettingsGroup = "Gantt";

    public static readonly string MetricsSettingsGroup = "Metrics";
    public static readonly string OptimizeAndCompressSettingsGroup = "Scheduling";
    public static readonly string ScenariosGroup = "Scenarios";
    public static readonly string SystemSettingsGroup = "System";
    public static readonly string LayoutSettingsGroup = "Layout";
    public static readonly string BoardsSettingsGroup = "Boards";
    public static readonly string PersonalSettingsGroup = "Personal";
    public static readonly string AutomaticActionGroup = "Automatic Actions";
    public static readonly string CoPilotSettingsGroup = "CoPilot";
    public static readonly string ReportingSettingsGroup = "Progress Reporting";
    public static readonly string ReportsSettingsGroup = "Reports";

    /// <summary>
    /// Settings Group Category Constants
    /// </summary>
    public static readonly string MetricCollection = "Metric Collection";

    public static readonly string BlockBorders = "Block Borders";
    public static readonly string BlockSettings = "Block Settings";

    public static readonly string Compress = "Compress Settings";
    public static readonly string CoPilot = "Global Settings";
    public static readonly string RuleSeek = "Sequence Planning";
    public static readonly string InsertJobs = "Insert Jobs";

    public static readonly string FormLayouts = "Form Layouts";

    public static readonly string GanttViewSettings = "Gantt Visual Settings";
    public static readonly string GridSettings = "Grid Settings";

    public static readonly string InventoryPlanSettings = "Inventory Plan Settings";

    public static readonly string MappingWizardSettings = "Mapping Wizard Settings";
    public static readonly string MetricsSettings = "Metrics Settings";
    public static readonly string MoveSettings = "Move Settings";
    public static readonly string ExpediteSettings = "Expedite Settings";
    public static readonly string MpsMrp = "MPS/MRP Settings";

    public static readonly string Notifications = "Notification Settings";

    public static readonly string Optimize = "Optimize Settings";
    public static readonly string OptimizeRules = "Optimize Rule Settings";

    public static readonly string Publish = "Publish Settings";

    public static readonly string ScenarioPermissions = "Scenario Permissions";
    public static readonly string ScenarioPlanningPreferences = "Scenario Preferences";
    public static readonly string ScenarioPlanningSettings = "Scenario Planning";
    public static readonly string SegmentSettings = "Segments";
    public static readonly string SimplifyGanttSettings = "Simplify Gantt Settings";
    public static readonly string SystemSettingsDataOptions = "Data Options";
    public static readonly string SystemSettingsPublishOptions = "Publish Options";
    public static readonly string SystemSettingsSchedulingOptions = "Scheduling Options";
    public static readonly string SystemSettingsUndoOptions = "Undo Options";
    public static readonly string SystemSettingsUserOptions = "User Options";
    public static readonly string SummarySettings = "Summary Settings";
    public static readonly string JobWatchGanttOptions = "Job Watch Gantt Options";
    public static readonly string PlantsGanttSettings = "Plant View Settings";

    public static readonly string TileSettings = "Tile Settings";

    public static readonly string WorkspaceViewSettings = "Workspace View Settings";
    public static readonly string AutomaticActionSettings = "Automatic Action Settings";

    public static readonly string BoundGridLayoutSettings = "BoundGrid Layout Settings";
    public static readonly string ActivityReportingSettings = "Activity Reporting Settings";

    public static readonly string CapacityPlanSettings = "Capacity Plan Settings";

    public static readonly string HoldSettings = "Hold Settings";
}

public static class SettingDataUtilities
{
    public static ISettingData ConstructSetting(ISettingData a_isSettingData, SettingData a_settingData)
    {
        if (a_isSettingData is null)
        {
            return null;
        }

        Type type = a_isSettingData.GetType();
        ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, binder: null, types: new[] { typeof(IReader) },
            modifiers: null);

        if (constructorInfo != null)
        {
            using (BinaryMemoryReader reader = new BinaryMemoryReader(a_settingData.Data))
            {
                return (ISettingData)constructorInfo.Invoke(new object[] { reader });
            }
        }

        return null;
    }
    public static T ConstructSetting<T>(SettingData a_data)
    {
        if (a_data != null)
        {
            using (BinaryMemoryReader reader = new (a_data.Data))
            {
                return (T)typeof(T).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IReader) }, null).Invoke(new object[] { reader });
            }
        }

        return (T)typeof(T).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null).Invoke(new object[] { });
    }

    public static T ConstructSetting<T, TParameterType>(SettingData a_data, TParameterType a_constructorParameter)
    {
        ConstructorInfo parameterConstructor;
        if (a_data != null)
        {
            using (BinaryMemoryReader reader = new (a_data.Data))
            {
                //Attempt to construct with the parameter
                parameterConstructor = typeof(T).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IReader), typeof(TParameterType) }, null);
                if (parameterConstructor != null)
                {
                    return (T)parameterConstructor.Invoke(new object[] { reader, a_constructorParameter });
                }

                return (T)typeof(T).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IReader) }, null).Invoke(new object[] { reader });
            }
        }

        //Attempt to construct with the parameter
        parameterConstructor = typeof(T).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(TParameterType) }, null);
        if (parameterConstructor != null)
        {
            return (T)parameterConstructor.Invoke(new object[] { a_constructorParameter });
        }

        //Generic no parameter constructor
        return (T)typeof(T).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null).Invoke(new object[] { });
    }
}

/// <summary>
/// For settings that have variable SettingsKey values, the setting key will be set when loaded
/// </summary>
public interface ISettingRequiresInitialization
{
    void Initialize(string a_settingKey);
}