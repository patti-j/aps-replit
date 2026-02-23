namespace PT.Scheduler.Simulation.Events;

//This path is no longer valid to schedule if it hasn't already started
internal class AlternatePathValidityEndEvent : AlternatePathEvent
{
    internal AlternatePathValidityEndEvent(long a_time, ManufacturingOrder a_mo, AlternatePath a_path)
        : base(a_time, a_mo, a_path)
    {
        
    }

    internal const int UNIQUE_ID = 1122;

    internal override int UniqueId => UNIQUE_ID;
}