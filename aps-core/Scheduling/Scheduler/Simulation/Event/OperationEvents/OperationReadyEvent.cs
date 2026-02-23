namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Indicates when an operation will be ready. These events are created when an optimization or
/// expedite is performed. They are also created when predecessor operations become ready.
/// When an operation is ready its activities may be added to some dispatchers or if it has been
/// finished then a predecessor ready event may be scheduled for its successor operations.
/// </summary>
public class OperationReadyEvent : OperationEvent
{
    public OperationReadyEvent(long a_time, BaseOperation a_operation, InternalOperation.LatestConstraintEnum a_latestConstraint)
        : base(a_time, a_operation)
    {
        LatestConstraint = a_latestConstraint;
    }

    public readonly InternalOperation.LatestConstraintEnum LatestConstraint;

    internal const int UNIQUE_ID = 25;

    internal override int UniqueId => UNIQUE_ID;
}