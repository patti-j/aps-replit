using System.Windows.Forms;

using PT.Scheduler;

namespace PT.PackageDefinitionsUI;

public interface ISearchElement : IDisposable
{
    Control ControlInstance { get; }
    bool Search(IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, UserManager a_um, string a_searchString);
}