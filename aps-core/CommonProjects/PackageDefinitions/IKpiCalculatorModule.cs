using System.Drawing;

namespace PT.PackageDefinitions;

public interface IKpiCalculatorModule
{
    List<IBaseKpiCalculatorElement> GetKpiCalculatorElements();
}

/// <summary>
/// Interface that must be implemented for all KPI Calculators.
/// </summary>
public interface IBaseKpiCalculatorElement : IPackageElement
{
    string Name { get; }
    string Description { get; }
    int Id { get; }
    Color PlotColor { get; }
    bool LowerIsBetter { get; }
    string FormatString { get; }

    decimal ChartScaleMultiplier { get; }
}