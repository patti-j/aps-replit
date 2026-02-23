using PT.Scheduler.Demand;
using Serilog.Events;

namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// This event should occur when
/// </summary>
internal class SalesOrderLineDistributionEvent : EventBase
{
    /// <summary>
    /// </summary>
    /// <param name="a_requiredAvailTicks">The required available time.</param>
    /// <param name="a_sod"></param>
    internal SalesOrderLineDistributionEvent(long a_requiredAvailTicks, SalesOrderLineDistribution a_sod) :
        base(a_requiredAvailTicks)
    {
        SalesOrderLineDistribution = a_sod;
        InitialProcessingForDemand = true;
    }

    internal SalesOrderLineDistributionEvent(SalesOrderLineDistributionEvent a_e) :
        base(a_e.Time)
    {
        SalesOrderLineDistribution = a_e.SalesOrderLineDistribution;
    }

    /// <summary>
    /// Whether this event should trigger an adjustment for the demand. This event may be reprocessed if the material isn't found and we won't want to keep adding demand adjustments.
    /// </summary>
    internal bool InitialProcessingForDemand;

    /// <summary>
    /// The SalesOrderLineDistribution this is for.
    /// </summary>
    internal SalesOrderLineDistribution SalesOrderLineDistribution { get; private set; }

    public override object Clone()
    {
        SalesOrderLineDistributionEvent copy = new (Time, SalesOrderLineDistribution);
        copy.InitialProcessingForDemand = InitialProcessingForDemand;
        return copy;
    }

    internal const int UNIQUE_ID = 49;

    internal override int UniqueId => UNIQUE_ID;
}