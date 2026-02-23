namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// </summary>
public class QtyToStockEvent : EventBase
{
    /// <summary>
    /// Occurs when a product goes into stock.
    /// </summary>
    /// <param name="a_time"></param>
    /// <param name="a_inventory">The inventory the product is going into.</param>
    /// <param name="a_product">The product that's going to inventory.</param>
    public QtyToStockEvent(long a_time, Warehouse a_warehouse, Product a_product, Item a_item)
        : base(a_time)
    {
        Warehouse = a_warehouse;
        Product = a_product;
        Item = a_item;
    }

    /// <summary>
    /// Could be null
    /// </summary>
    internal Warehouse Warehouse { get; }

    /// <summary>
    /// The product going to stock, if there is one.
    /// </summary>
    internal Product Product { get; }

    internal const int UNIQUE_ID = 16;

    internal override int UniqueId => UNIQUE_ID;

    /// <summary>
    /// Should not be null
    /// </summary>
    internal Item Item { get; }
}