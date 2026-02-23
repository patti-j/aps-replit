using PT.APSCommon;
using PT.Scheduler.Schedule.Demand;

namespace PT.Scheduler.Schedule;

/// <summary>
/// Define common properties of some type of material requirers, such as SalesOrderDistributions and
/// TransferOrderDistributions.
/// </summary>
internal interface IQtyRequirement : Simulation.ILotEligibility
{
    /// <summary>
    /// The item to supply.
    /// </summary>
    Item Item { get; }

    /// <summary>
    /// The amount of material to use.
    /// </summary>
    decimal QtyOpenToShip { get; }

    /// <summary>
    /// The actual time material is scheduled to ship.
    /// </summary>
    long ActualAvailableTicks { get; set; }

    /// <summary>
    /// The warehouse that must supply the material.
    /// </summary>
    Warehouse MustSupplyFromWarehouse { get; }

    BaseId Id { get; }
}