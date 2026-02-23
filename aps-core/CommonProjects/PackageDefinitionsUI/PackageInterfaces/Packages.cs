using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.DataSources;
using PT.PackageDefinitionsUI.Interfaces;

namespace PT.PackageDefinitionsUI.PackageInterfaces;

public interface IUIPackage : IPackage
{
    IMainForm MainForm { set; }
    IPackageManagerUI PackageManager { set; }
}

public interface IPanePackage : ITilePackage
{
    List<IBoardModule> GetPaneModules();
}

public interface IMainBarExtensionPackage : IUIPackage
{
    List<IMainBarModule> GetMainBarModules();
}

public interface ITilePackage : IUIPackage
{
    List<ITileModule> GetTileModules();
}

public interface IReportPackage : IUIPackage
{
    List<IReportModule> GetReportModules();
}

public interface IPropertyPackage : IUIPackage
{
    List<IPropertyModule> GetPropertyModules();
}

public interface IGanttPackage : IUIPackage
{
    List<IGanttModule> GetGanttModules();
}

public interface ISettingsControlPackage : IUIPackage
{
    List<ISettingsControlModule> GetSettingsControlModules();
}

public interface IOptimizeFactorSettingsControlPackage : IUIPackage
{
    List<ISequenceFactorSettingsControlModule> GetFactorSettingsControlModules();
}

public interface IDataSetPackage : IUIPackage
{
    List<IDataSetModule> GetDataSetModules();
}

public interface IImportPackage : IUIPackage
{
    List<IImportModule> GetImportModules();
}

public interface ISearchPackage : IUIPackage
{
    List<ISearchModule> GetSearchModules();
}

public interface IInsightPackage : IUIPackage
{
    List<IInsightModule> GetInsightModules();
}

public interface IPruningPackage : IUIPackage
{
    List<IPruningModule> GetPruningModules();
}

public interface IGridPackage : IUIPackage
{
    List<IGridModule> GetGridModules();
}

public interface IDataSourcePackage : IUIPackage
{
    List<IDataSourceModule> GetDataSourceModules();
}

/// <summary>
/// Contains modules for displaying UI notifications
/// </summary>
public interface INotificationPackage : IUIPackage
{
    List<INotificationModule> GetNotificationModules()
    {
        return new List<INotificationModule>(0);
    }

    List<INavigationListenerModule> GetNavigationListenerModules()
    {
        return new List<INavigationListenerModule>(0);
    }
}

/// <summary>
/// This package contains branding material
/// </summary>
public interface IBrandPackage : IUIPackage, ILicensedPackage
{
    IBrandModule GetBrandModule();
}

public interface IBrandModule : IPackageModule
{
    IBrand GetBrand();
}

public interface IScenarioBackgroundTaskPackage : IUIPackage
{
    List<IScenarioBackgroundTaskModule> GetScenarioBackgroundTaskModules();
}

public interface IObjectActionPackage : IUIPackage
{
    List<IObjectActionModule> GetObjectActionModules();
}