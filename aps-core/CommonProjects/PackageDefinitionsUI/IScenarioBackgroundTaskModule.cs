using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI;

public interface IScenarioBackgroundTaskModule
{
    List<IScenarioBackgroundTaskElement> GetScenarioBackgroundTaskElements(IScenarioInfo a_scenarioInfo);
}

public interface IScenarioBackgroundTaskElement : IPackageElement { }