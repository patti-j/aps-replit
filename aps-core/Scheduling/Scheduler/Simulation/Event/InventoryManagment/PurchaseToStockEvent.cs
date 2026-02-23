namespace PT.Scheduler.Simulation.Events;

internal class PurchaseToStockEvent : EventBase
{
    internal PurchaseToStockEvent(long a_time, PurchaseToStock a_pts) : base(a_time)
    {
        PurchaseToStock = a_pts;
    }

    internal const int UNIQUE_ID = 46;

    internal override int UniqueId => UNIQUE_ID;

    internal PurchaseToStock PurchaseToStock { get; private set; }
}