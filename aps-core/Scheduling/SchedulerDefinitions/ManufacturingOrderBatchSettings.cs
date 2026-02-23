namespace PT.SchedulerDefinitions;

public class ManufacturingOrderBatchSettings
{
    /// <summary>
    /// Determines how manufacturing orders are batched.
    /// </summary>
    public enum EManufacturingOrderBatchMethods
    {
        /// <summary>
        /// The batch window is the earliest need date plus the batch window span.
        /// </summary>
        ByNeedDate,

        /// <summary>
        /// The batch windows is the earliest release date plus the batch window span.
        /// </summary>
        ByReleaseDate,

        /// <summary>
        /// The need date of the batch is the earliest need date among its members.
        /// A manufacturing order can be added to the batch if its release date is less than (earliest need date)-(batch windows span).
        /// </summary>
        ByNeedWhereBatchEligibilityIsDeterminedByReleaseDateBeingEarlierThanNeedDate
    }
}