using PT.PackageDefinitionsUI.PackageInterfaces;

namespace PT.PackageDefinitionsUI;

public enum EValueComparisonType { HigherIsBetter, LowerIsBetter, None }

public enum EKpiResultType
{
    /// <summary>
    /// This result is a number
    /// </summary>
    Number,

    /// <summary>
    /// This result is a percent value
    /// </summary>
    Percent,

    /// <summary>
    /// This result is a dollar value
    /// </summary>
    Dollar,

    [Obsolete("Used as a placeholder. Replace with the correct type")]
    Default
}

public interface IKpiProperty : IObjectProperty
{
    EValueComparisonType ValueComparisonType { get; }

    EKpiResultType ResultDisplayType { get; }
}