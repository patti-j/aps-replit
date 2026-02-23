namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// This event is used to check and retry scheduling activities now that the ItemStorage is emptied.
/// </summary>
internal class FutureStorageDisposalEvent : EventBase
{
    /// <summary>
    /// Occurs when a product goes into stock.
    /// </summary>
    /// <param name="a_time"></param>
    /// <param name="a_inventory">The inventory the product is going into.</param>
    /// <param name="a_product">The product that's going to inventory.</param>
    internal FutureStorageDisposalEvent(long a_time, ItemStorage a_itemItemStorageDisposed, DemandSource a_demandSource)
        : base(a_time)
    {
        m_itemStorage = a_itemItemStorageDisposed;
        m_demandSource = a_demandSource;
    }

    private readonly ItemStorage m_itemStorage;
    private readonly DemandSource m_demandSource;

    internal ItemStorage ItemStorage => m_itemStorage;

    internal const int UNIQUE_ID = 57;

    internal override int UniqueId => UNIQUE_ID;

    internal DemandSource DemandSource => m_demandSource;
}