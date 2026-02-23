using PT.Scheduler.Demand;

namespace PT.Scheduler.Simulation.Events;

internal abstract class TransferOrderEventBase : EventBase
{
    internal TransferOrderEventBase(long a_time, TransferOrderDistribution a_dist)
        : base(a_time)
    {
        Distribution = a_dist;
    }

    /// <summary>
    /// The TransferOrderDistribution of the event.
    /// </summary>
    internal readonly TransferOrderDistribution Distribution;
}