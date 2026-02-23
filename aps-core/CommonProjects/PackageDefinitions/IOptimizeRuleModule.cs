namespace PT.PackageDefinitions;

public interface IOptimizeRuleModule
{
    List<IOptimizeRuleElement> GenerateOptimizeRules(ISettingsManager a_scenarioSettings);
}

/// <summary>
/// Used to load optimize rule elements from packages without reference to Scheduler
/// </summary>
public interface IOptimizeRuleElement : IPackageElement { }