namespace PT.Scheduler.Simulation.Events;

internal class BatchOperationReadyEvent : OperationEvent
{
    internal BatchOperationReadyEvent(long a_time, BaseOperation a_op)
        : base(a_time, a_op) { }

    internal const int UNIQUE_ID = 34;

    internal override int UniqueId => UNIQUE_ID;
}