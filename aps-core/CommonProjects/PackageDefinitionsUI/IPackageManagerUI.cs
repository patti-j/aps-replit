using DevExpress.Utils.Svg;

using PT.APSCommon.Extensions;
using PT.APSCommon.Packages;
using PT.APSCommon.Windows;
using PT.Common.Localization;
using PT.PackageDefinitions.PackageInterfaces;
using PT.PackageDefinitionsUI.DataSources;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler.PackageDefs;

namespace PT.PackageDefinitionsUI;

public interface IPackageManagerUI : IPackageManager
{
    /// <summary>
    /// Gets Tile modules associated with a specific Board
    /// </summary>
    /// <param name="a_paneKey"></param>
    /// <returns></returns>
    List<ITileModule> GetTileModules(string a_paneKey);

    /// <summary>
    /// Gets Object Properties that have been collected from all Object Property modules
    /// </summary>
    /// <returns></returns>
    List<IObjectProperty> GetObjectProperties();

    /// <summary>
    /// Gets all Gantt Modules
    /// </summary>
    /// <returns></returns>
    List<IGanttModule> GetGanttModules();

    /// <summary>
    /// Gets all grid modules
    /// </summary>
    /// <returns></returns>
    List<IGridModule> GetGridModules();

    /// <summary>
    /// Gets all Data Source modules
    /// </summary>
    /// <returns></returns>
    List<IDataSourceModule> GetDataSourceModules();

    /// <summary>
    /// Unused TODO: Remove?
    /// </summary>
    /// <returns></returns>
    List<ILocalizationLanguageModule> GetLanguageModules();

    /// <summary>
    /// Unused TODO: Remove?
    /// </summary>
    /// <param name="a_key"></param>
    /// <returns></returns>
    IReportModule GetReport(string a_key);

    /// <summary>
    /// Searches Board Modules for specific Board to generate
    /// TODO: Refactor, Board not Pane
    /// </summary>
    /// <param name="a_scenarioInfo"></param>
    /// <param name="a_key"></param>
    /// <returns></returns>
    IBoardControl GeneratePane(IScenarioInfo a_scenarioInfo, string a_key);

    /// <summary>
    /// Gets all Settings Control modules
    /// </summary>
    /// <returns></returns>
    List<ISettingsControlModule> GetSettingsControlModules();

    /// <summary>
    /// Gets all Sequencing Factor Settings Control modules
    /// TODO: Refactor, Sequencing not Optimize
    /// </summary>
    /// <returns></returns>
    List<ISequenceFactorSettingsControlModule> GetOptimizeFactorSettingsControlModules();

    /// <summary>
    /// Unused TODO: Remove?
    /// </summary>
    /// <returns></returns>
    List<IDataSetModule> GetDataSetModules();

    /// <summary>
    /// Gets all Search modules
    /// </summary>
    /// <returns></returns>
    List<ISearchModule> GetSearchModules();

    /// <summary>
    /// Gets all Insight modules
    /// </summary>
    /// <returns></returns>
    List<IInsightModule> GetInsightModules();

    /// <summary>
    /// Gets all Pruning modules
    /// </summary>
    /// <returns></returns>
    List<IPruningModule> GetPruningModules();

    /// <summary>
    /// Gets all Board modules
    /// TODO: Refactor, Board not  Pane
    /// </summary>
    /// <returns></returns>
    List<IBoardModule> GetPaneModules();

    /// <summary>
    /// Gets all User Preference modules
    /// </summary>
    /// <returns></returns>
    List<IUserPreferencesModule> GetPreferenceModules();

    /// <summary>
    /// Gets all Notification modules
    /// </summary>
    /// <returns></returns>
    List<INotificationModule> GetNotificationModules();

    /// <summary>
    /// Gets all Main Interface modules (Main Interface level System Closing and Navigation elements)
    /// </summary>
    /// <returns></returns>
    List<IMainInterfaceModule> GetMainInterfaceModules();

    /// <summary>
    /// Unused
    /// </summary>
    /// <returns></returns>
    List<IMainWorkspaceManagerModule> GetMainWorkspaceManagerModules();

    /// <summary>
    /// Gets all Score Board modules for Impact tiles
    /// </summary>
    /// <returns></returns>
    List<IScoreBoardModule> GetScoreBoardModules();

    /// <summary>
    /// Gets HashSet of Packages Ids for Packages that require licensing
    /// </summary>
    /// <returns></returns>
    HashSet<int> GetPackagesLicensingInfo();

    /// <summary>
    /// Gets all Scenario Background Task element modules
    /// </summary>
    /// <returns></returns>
    List<IScenarioBackgroundTaskModule> GetScenarioBackgroundTaskModules();

    /// <summary>
    /// Gets all Object Action modules
    /// </summary>
    /// <returns></returns>
    List<IObjectActionModule> GetObjectActionModules();
}

public class TileInfo : IComparable<TileInfo>, IEquatable<TileInfo>
{
    public readonly string PaneKey;
    public readonly string TileKey;
    public readonly bool Primary;
    public readonly string DisplayName; //Currently is just TileKey.Localize();
    public string Description;

    /// <summary>
    /// The order the tile is shown on the tile menu
    /// </summary>
    public readonly int Priority;

    /// <summary>
    /// Return an image representing this tile
    /// It will be used in the UI
    /// </summary>
    /// <returns></returns>
    public SvgImage GetIconImage()
    {
        return PtImageCache.GetImage(m_imageKey);
    }

    private readonly string m_imageKey;

    public TileInfo(string a_paneKey, string a_tileKey, int a_priority, string a_imageKey, string a_description, bool a_primary = false)
    {
        m_imageKey = a_imageKey;
        PaneKey = a_paneKey;
        TileKey = a_tileKey;
        Primary = a_primary;
        DisplayName = a_tileKey.Localize();
        Priority = a_priority;
        Description = a_description;
    }

    public TileInfo(string a_paneKey, string a_tileKey, int a_priority, string a_imageKey, string a_description, string a_displayName, bool a_primary = false)
        : this(a_paneKey, a_tileKey, a_priority, a_imageKey, a_description, a_primary)
    {
        DisplayName = a_displayName;
    }

    public TileInfo(string a_paneKey, string a_tileKey, string a_displayName, string a_imageKey)
    {
        m_imageKey = a_imageKey;
        PaneKey = a_paneKey;
        TileKey = a_tileKey;
        Primary = false;
        DisplayName = a_displayName;
        Priority = 100;
        Description = string.Empty;
    }

    public int CompareTo(TileInfo a_other)
    {
        if (ReferenceEquals(this, a_other))
        {
            return 0;
        }

        if (ReferenceEquals(null, a_other))
        {
            return 1;
        }

        return string.Compare(TileKey, a_other.TileKey, StringComparison.Ordinal);
    }

    public bool Equals(TileInfo a_other)
    {
        if (ReferenceEquals(null, a_other))
        {
            return false;
        }

        if (ReferenceEquals(this, a_other))
        {
            return true;
        }

        return TileKey == a_other.TileKey;
    }

    public override bool Equals(object a_obj)
    {
        if (ReferenceEquals(null, a_obj))
        {
            return false;
        }

        if (ReferenceEquals(this, a_obj))
        {
            return true;
        }

        if (a_obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((TileInfo)a_obj);
    }

    public override int GetHashCode()
    {
        return TileKey != null ? TileKey.GetHashCode() : 0;
    }
}

public class ReportInfo
{
    public ReportInfo(string a_reportName)
    {
        ReportName = a_reportName;
    }

    public string ReportName;
}

public class SettingsInfo
{
    public SettingsInfo(ESettingsCategory a_category)
    {
        Category = a_category;
    }

    public ESettingsCategory Category;

    public enum ESettingsCategory { User, System }
}

public class DataSetInfo
{
    public DataSetInfo(EDataSetCategory a_category)
    {
        DataSetCategory = a_category;
    }

    public EDataSetCategory DataSetCategory;

    public enum EDataSetCategory { JobsDataSet, PTDataSet, WhereUsedDataSet, CustomDataSet }
}