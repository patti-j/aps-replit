using PT.Scheduler.Demand;

namespace PT.Scheduler.Simulation.Events;

internal class TransferOrderShipEvent : TransferOrderEventBase
{
    internal TransferOrderShipEvent(long a_time, TransferOrderDistribution a_dist)
        : base(a_time, a_dist)
    {
        InitialProcessingForDemand = true;
    }

    internal const int UNIQUE_ID = 29;

    internal override int UniqueId => UNIQUE_ID;

    /// <summary>
    /// Whether this event should trigger an adjustment for the demand. This event may be reprocessed if the material isn't found and we won't want to keep adding demand adjustments.
    /// </summary>
    internal bool InitialProcessingForDemand;

    public override object Clone()
    {
        TransferOrderShipEvent copy = new (Time, Distribution);
        copy.InitialProcessingForDemand = InitialProcessingForDemand;
        return copy;
    }
}