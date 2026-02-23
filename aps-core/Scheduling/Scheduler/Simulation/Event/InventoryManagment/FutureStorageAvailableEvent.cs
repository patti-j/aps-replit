namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Summary description for PurchaseToStockEvent.
/// </summary>
internal class FutureStorageAvailableEvent : EventBase
{
    /// <summary>
    /// Occurs when a product goes into stock.
    /// </summary>
    /// <param name="a_time"></param>
    /// <param name="a_inventory">The inventory the product is going into.</param>
    /// <param name="a_product">The product that's going to inventory.</param>
    internal FutureStorageAvailableEvent(long a_time, InternalActivity a_activity, IEnumerable<InternalResource> a_previousDispatchers)
        : base(a_time)
    {
        m_activity = a_activity;
        m_previousDispatchers = a_previousDispatchers;
    }

    private readonly InternalActivity m_activity;
    private readonly IEnumerable<InternalResource> m_previousDispatchers;

    internal InternalActivity Activity => m_activity;
    internal IEnumerable<InternalResource> PreviousDispatchers => m_previousDispatchers;

    internal const int UNIQUE_ID = 54;

    internal override int UniqueId => UNIQUE_ID;
}