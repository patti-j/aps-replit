namespace PT.Scheduler.Simulation;

internal enum SupplyTypeEnum
{
    /// <summary>
    /// material created by and activity.
    /// </summary>
    Activity,

    /// <summary>
    /// On-hand inventory.
    /// </summary>
    Inventory,

    /// <summary>
    /// Material from a purchase to stock.
    /// </summary>
    PurchaseToStock,

    /// <summary>
    /// Material from a transfer order.
    /// </summary>
    TransferOrder,

    /// <summary>
    /// Material from a sales order.
    /// </summary>
    SalesOrder,

    /// <summary>
    /// The product of an activity.
    /// </summary>
    Product
}