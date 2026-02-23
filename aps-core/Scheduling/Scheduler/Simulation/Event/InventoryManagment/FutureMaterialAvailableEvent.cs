namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Summary description for PurchaseToStockEvent.
/// </summary>
internal class FutureMaterialAvailableEvent : EventBase
{
    /// <summary>
    /// Occurs when a product goes into stock.
    /// </summary>
    /// <param name="a_time"></param>
    /// <param name="a_inventory">The inventory the product is going into.</param>
    /// <param name="a_product">The product that's going to inventory.</param>
    internal FutureMaterialAvailableEvent(long a_time, InternalActivity a_activity, Plant.FindMaterialResult a_materialRetryInfo)
        : base(a_time)
    {
        m_activity = a_activity;
        m_materialRetryInfo = a_materialRetryInfo;
    }

    private readonly InternalActivity m_activity;
    private readonly Plant.FindMaterialResult m_materialRetryInfo;

    internal InternalActivity Activity => m_activity;
    internal Plant.FindMaterialResult MaterialRetryInfo => m_materialRetryInfo;

    internal const int UNIQUE_ID = 50;

    internal override int UniqueId => UNIQUE_ID;
}