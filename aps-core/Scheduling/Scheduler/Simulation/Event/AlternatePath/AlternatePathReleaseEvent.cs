namespace PT.Scheduler.Simulation.Events;

internal class AlternatePathReleaseEvent : AlternatePathEvent
{
    internal AlternatePathReleaseEvent(long a_time, ManufacturingOrder a_mo, AlternatePath a_path, InternalOperation.LatestConstraintEnum a_latestConstraint)
        : base(a_time, a_mo, a_path)
    {
        LatestConstraint = a_latestConstraint;
    }

    internal InternalOperation.LatestConstraintEnum LatestConstraint { get; set; }

    internal const int UNIQUE_ID = 37;

    internal override int UniqueId => UNIQUE_ID;
}