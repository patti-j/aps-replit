using System.Windows.Forms;

using PT.APSCommon;
using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI;

public interface IScenarioListenerElement : IDisposable
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    void AttachScenarioInfo(IScenarioInfo a_scenarioInfo);
}

public interface IUsersListenerElement : IDisposable
{
    void AttachUsersInfoAndInvokeControl(IUsersInfo a_usersInfo, Control a_invokeControl);
}

/// <summary>
/// Defines a property that returns changes to other properties
/// </summary>
public interface IImpactProperty : IPackageElement
{
    void ValuesChanged(IValueCache a_cache);
}

/// <summary>
/// Returned as the type from IImpactProperties
/// </summary>
public enum EImpactChange { NoData, NoImpact, PositiveImpact, NegativeImpact }

/// <summary>
/// The change in value since the last update
/// </summary>
public enum EImpactComparisonResult
{
    NoChange,
    Increase,
    Decrease,
    Initialized,
    NotFound
}

public interface IValueCache
{
    EImpactComparisonResult CalculateChange(BaseId a_baseId);
    IComparable GetCurrentValue(BaseId a_baseId);
    IComparable GetPreviousValue(BaseId a_baseId);
}