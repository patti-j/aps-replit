namespace PT.Scheduler.Demand;

/// <summary>
/// Specified WHERE???
/// Inventory object?
/// Overrriden by Warehouse or Plant?
/// Overrideby by Optimization Settings?   Maybe this sets the Warehouse/Plant settings for use by subsequent drag and drops, etc?
/// </summary>
public class AllocationRule
{
    private enum DemandTypes
    {
        SalesOrder,
        Estimate,
        Forecast,
        TransferOrder,
        Job
    }

    private List<DemandTypes> allocationSequence;

    /// <summary>
    /// Specifies how inventory is allocated to competing demands.
    /// DemandTypes higher on the list are allocated inventory before those lower on the list.
    /// DemandTypes not listed receive no allocations -- they are ignored during planning.
    /// Within a given type of demand (with the same RequiredAvailableDate), allocations are based on Sales Order Line Item Distribution Priority.
    /// </summary>
    private List<DemandTypes> AllocationSequence => allocationSequence;
}