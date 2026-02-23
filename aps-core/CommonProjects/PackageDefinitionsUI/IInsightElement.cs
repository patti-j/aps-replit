using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI;

public interface IInsightElement : IPackageElement
{
    /// <summary>
    /// Unique insight element key
    /// </summary>
    string InsightElementKey { get; }

    /// <summary>
    /// Viewable insight based on the calculated value
    /// </summary>
    string CalculatedInsight { get; }

    /// <summary>
    /// Unsigned integer used to set a sorting order
    /// </summary>
    uint Priority { get; }

    /// <summary>
    /// Type of the value calculated by the insight
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Calculated value
    /// </summary>
    object Value { get; }
}