using PT.APSCommon;
using PT.SchedulerData.ObjectKeys;

namespace PT.PackageDefinitionsUI.KPI;

public enum EAdjustmentType
{
    /// <summary>
    /// The adjustment is a fixed point in time. For example a one time cost, instant increase, or count
    /// </summary>
    Singular,

    /// <summary>
    /// The adjustments represent a period of time in which the change value is valid. For exmaple a late job that is late for x time, or a resources bottleneck duration
    /// These adjustments value's will be counted in each bucket in which they span
    /// </summary>
    Concurrent,

    /// <summary>
    /// The adjustments represent a value averaged over a period of time. For example a setup cost over a duration, capacity hours over a duration, etc.
    /// These adjustments will be split into buckets proportionally
    /// </summary>
    Averaged,

    [Obsolete("Used as a placeholder. Replace with the correct type")]
    Default
}

public interface IKpiObjectsFilter
{
    bool HasSOandDistributionFilters { get; }
    bool HasPOFilters { get; }
    bool HasJobFilters { get; }
    bool HasResourceFilters { get; }
    int JobCount { get; }
    int SOandDistributionCount { get; }
    int POCount { get; }
    int ResourceCount { get; }
    IEnumerable<JobKey> JobIds { get; }
    IEnumerable<ResourceKey> ResourceIds { get; }
    IEnumerable<SalesOrderKey> SOandDistributionIds { get; }
    IEnumerable<BaseId> PurchaseOrderIds { get; }
    IEnumerable<JobKey> TemplateIds { get; }
    IEnumerable<ActivityKey> ActivityIds { get; }
    IEnumerable<InventoryKey> InventoryIds { get; }
    IEnumerable<MaterialRequirementKey> MaterialsIds { get; }
}

public interface IKpiAdjustment
{
    DateTime AdjustmentDate { get; }
    DateTime EndDate { get; }
    decimal Change { get; }

    object ObjectKey { get; }
}

public interface IKpiResult
{
    IEnumerable<IKpiAdjustment> AdjustmentCollection { get; }

    /// <summary>
    /// How the result adjustments should be interpreted
    /// </summary>
    EAdjustmentType AdjustmentType { get; }
}

/// <summary>
/// Stores a sorted adjustment list and calculates statistics
/// </summary>
public interface ICalculatedKpiResult : IKpiResult
{
    decimal Max { get; }
    decimal Min { get; }
    decimal Average { get; }
    decimal Total { get; }

    /// <summary>
    /// Whether there are any adjustments in the result
    /// </summary>
    bool IsEmpty { get; }
}

public interface IBucketedKpiResult
{
    IEnumerable<IKpiResultBucket> BucketedAdjustmentList { get; }
}

public interface IKpiResultBucket : ICalculatedKpiResult
{
    DateTime BucketStart { get; }
    DateTime BucketEnd { get; }
}