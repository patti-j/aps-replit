using PT.Scheduler.Demand;

namespace PT.Scheduler.Simulation.Events;

internal class TransferOrderReceiveEvent : TransferOrderEventBase
{
    internal TransferOrderReceiveEvent(long a_time, TransferOrderDistribution a_dist)
        : base(a_time, a_dist)
    {
        ReceiveQty = a_dist.QtyOrdered - a_dist.QtyReceived;
    }

    /// <summary>
    /// The quantity being received.
    /// </summary>
    internal readonly decimal ReceiveQty;

    internal const int UNIQUE_ID = 33;

    internal override int UniqueId => UNIQUE_ID;
}