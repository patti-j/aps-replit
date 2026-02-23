namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Setup hold until events for operations that are being held.
/// </summary>
internal class HoldUntilEvent : OperationEvent
{
    internal HoldUntilEvent(long a_holdUntilDate, BaseOperation a_operation, HoldEnum a_hold)
        : base(a_holdUntilDate, a_operation)
    {
        HoldType = a_hold;
    }

    internal readonly HoldEnum HoldType;

    internal const int UNIQUE_ID = 22;

    internal override int UniqueId => UNIQUE_ID;
}