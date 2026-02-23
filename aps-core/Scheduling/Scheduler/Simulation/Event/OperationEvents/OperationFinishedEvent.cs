namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// This event is setup for finished operations to make sure they aren't released to their successors too early.
/// They may be released early in the case where the operation is reported as finished but the clock hasn't advanced up to the point of the
/// reported finish.
/// </summary>
internal class OperationFinishedEvent : OperationEvent
{
    internal OperationFinishedEvent(long a_releaseTicks, long a_readyTicks, BaseOperation a_operation)
        : base(a_releaseTicks, a_operation)
    {
        OperationReadyTicks = a_readyTicks;
    }

    internal OperationFinishedEvent(OperationFinishedEvent a_evt, long a_releaseTicks, long a_readyTicks)
        : base(a_readyTicks, a_evt.Operation)
    {
        OperationReadyTicks = a_readyTicks;
    }

    internal readonly long OperationReadyTicks;

    internal const int UNIQUE_ID = 24;

    internal override int UniqueId => UNIQUE_ID;

    public override string ToString()
    {
        string ret = base.ToString() + "OperationReadyTicks=" + DateTimeHelper.ToLocalTimeFromUTCTicks(OperationReadyTicks);
        return ret;
    }
}