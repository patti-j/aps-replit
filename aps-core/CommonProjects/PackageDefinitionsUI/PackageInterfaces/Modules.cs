using System.Data;
using DevExpress.XtraReports.UI;

using PT.APSCommon;
using PT.PackageDefinitions;
using PT.Scheduler;

namespace PT.PackageDefinitionsUI.PackageInterfaces;

public interface IBoardModule : IPackageModule
{
    IBoardControl GenerateBoard(IScenarioInfo a_scenarioInfo);
    string BoardKey { get; }
}

public interface IMainBarModule : IPackageModule
{
    List<IBarMenuElement> GetMainBarMenus(IScenarioInfo a_scenarioInfo, UserManager a_um, UserManagerEvents a_userManagerEvents)
    {
        return new List<IBarMenuElement>();
    }

    List<IBarButtonElement> GetBarButtonElements(IScenarioInfo a_scenarioInfo, Scenario a_s, ScenarioDetail a_sd)
    {
        return new List<IBarButtonElement>();
    }

    List<IBarControlElement> GetBarMenuControls(IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd)
    {
        return new List<IBarControlElement>();
    }
}

public interface ITileModule : IPackageModule
{
    /// <summary>
    /// The board to load these tiles onto
    /// </summary>
    string BoardKey { get; }

    /// <summary>
    /// Information about the tile that will be used to generate the tile control later (once the tile is opened)
    /// </summary>
    List<TileInfo> GenerateTileInfos(IScenarioInfo a_scenarioInfo);

    /// <summary>
    /// Generate an individual tile by key. The tile has been opened.
    /// </summary>
    ITile GenerateTile(IScenarioInfo a_scenarioInfo, TileInfo a_tileInfo);
}

public interface IDataSetModule : IPackageModule
{
    DataSetInfo DataSetInfo { get; }

    DataSet GenerateDataSet();
}

public interface IReportModule : IPackageModule
{
    ReportInfo ReportInfo { get; }

    DataSet GenerateDataset(ScenarioDetail a_sd, List<BaseId> a_jobIds);
    XtraReport GetReport(DataSet a_ds);

    void PostReportShown(ScenarioDetail a_sd, List<BaseId> a_jobIds);
}

public interface IPropertyModule : IPackageModule
{
    List<IObjectProperty> GenerateProperties();
}

public interface ISettingsControlModule : IPackageModule
{
    SettingsInfo SettingsInfo { get; }

    List<ISettingsControl> GenerateSettingsControls(IScenarioInfo a_scenarioInfo);
}

public interface ISequenceFactorSettingsControlModule : IPackageModule
{
    List<ISequenceFactorSettingsControl> GenerateFactorSettingsControls(IMainForm a_mainForm);
}

public interface IImportModule : IPackageModule { }

public interface ISearchModule : IPackageModule
{
    List<ISearchElement> GenerateSearchControls();
}

public interface IInsightModule : IPackageModule
{
    List<IInsightElement> GenerateInsightElements(ScenarioDetail a_sd, Job a_job);
    List<IInsightElement> GenerateInsightElements(ScenarioDetail a_sd, ManufacturingOrder a_manufacturingOrder);
    List<IInsightElement> GenerateInsightElements(ScenarioDetail a_sd, InternalOperation a_iOp);
    List<IInsightElement> GenerateInsightElements(ScenarioDetail a_sd, InternalActivity a_act);
    List<IInsightElement> GenerateInsightElements(ScenarioDetail a_sd, Resource a_res);
    List<IInsightElement> GenerateInsightElements(ScenarioDetail a_sd, Scenario a_scenario);
}

public interface IPruningModule : IPackageModule
{
    List<IPruningElement> GeneratePruningElements(ScenarioDetail a_sd);
}