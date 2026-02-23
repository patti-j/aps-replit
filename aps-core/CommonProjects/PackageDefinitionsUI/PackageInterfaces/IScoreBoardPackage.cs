using System.Windows.Forms;

using PT.PackageDefinitions;
using PT.Scheduler;

namespace PT.PackageDefinitionsUI.PackageInterfaces;

public interface IScoreBoardPackage : IUIPackage
{
    IEnumerable<IScoreBoardModule> GetScoreBoardModules();
}

public interface IScoreBoardModule
{
    IEnumerable<IScoreBoardSummaryElement> GetScoreBoardSummaries(string a_boardKey);
    IEnumerable<IScoreBoardTrackerElement> GetScoreBoardTrackers();
}

public interface IScoreBoardSummaryElement : IPackageElement
{
    void InitializeTrackers(IEnumerable<IScoreBoardTrackerElement> a_trackers);
    void CalcSummaryScore(ScenarioDetail a_sd);

    Control SummaryControl { get; }
}

public interface IScoreBoardTrackerElement : IPackageElement
{
    decimal CalcSummaryContribution(ScenarioDetail a_sd);
}