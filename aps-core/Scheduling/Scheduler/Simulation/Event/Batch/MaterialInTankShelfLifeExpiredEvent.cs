namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// When the shelf life of the material stored in a tank has expired, purge the inventory and release the tank for processing.
/// </summary>
internal class MaterialInTankShelfLifeExpiredEvent : EventBase
{
    /// <summary>
    /// </summary>
    /// <param name="a_inv">The inventory that will expire.</param>
    /// <param name="a_batch">The batch that produced the inventory.</param>
    /// <param name="a_tankRes">The tank the inventory is in.</param>
    /// <param name="a_expirationTime">The time when the inventory expires.</param>
    internal MaterialInTankShelfLifeExpiredEvent(Inventory a_inv, Batch a_batch, Resource a_tankRes, long a_expirationTime) :
        base(a_expirationTime)
    {
        Inventory = a_inv;
        Batch = a_batch;
        TankResource = a_tankRes;
    }

    /// <summary>
    /// The inventory that has expired.
    /// </summary>
    internal Inventory Inventory { get; set; }

    internal Batch Batch { get; set; }

    internal Resource TankResource { get; set; }

    internal const int UNIQUE_ID = 38;

    internal override int UniqueId => UNIQUE_ID;
}