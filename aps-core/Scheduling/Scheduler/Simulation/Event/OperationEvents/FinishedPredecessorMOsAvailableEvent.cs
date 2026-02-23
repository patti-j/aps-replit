namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Indicates when predecessor MOs that have been finished are ready. Finished predecessor MOs might not be available due to TransferTime.
/// </summary>
internal class FinishedPredecessorMOsAvailableEvent : OperationEvent
{
    /// <summary>
    /// Indicate when finished predecessor MOs are ready.
    /// </summary>
    /// <param name="aFinishedPredecessorMOsAvailableTime">The time when the finished predecessors are ready.</param>
    /// <param name="successorOperation">The operation that the finished predecessor MOs are used in.</param>
    internal FinishedPredecessorMOsAvailableEvent(long a_finishedPredecessorMOsAvailableTime, BaseOperation a_successorOperation)
        : base(a_finishedPredecessorMOsAvailableTime, a_successorOperation) { }

    internal const int UNIQUE_ID = 21;

    internal override int UniqueId => UNIQUE_ID;
}