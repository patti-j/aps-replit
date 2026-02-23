namespace PT.Scheduler.Simulation.Events;

internal class MinAgeEvent : EventBase
{
    internal MinAgeEvent(long a_time, Item a_item) : base(a_time)
    {
        Item = a_item;
    }

    internal const int UNIQUE_ID = 48;

    internal override int UniqueId => UNIQUE_ID;

    internal Item Item { get; private set; }

    internal QtyNode QtyNodeExpirable { get; private set; }
}