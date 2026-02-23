using PT.PackageDefinitions;

namespace PT.Scheduler.PackageDefs;

/// <summary>
/// Interface that must be implemented for all KPI Calculators.
/// </summary>
public interface IKpiCalculatorElement : IBaseKpiCalculatorElement
{
    KPI.EValueDisplayTypes ValueDisplayType { get; }

    decimal Calculate(ScenarioDetail a_sd);
}