namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// This event indicates that a Produced lot has expired and the Storage should be removed
/// </summary>
internal class ShelfLifeEvent : EventBase
{
    private readonly Item m_item;

    internal ShelfLifeEvent(long a_time, List<Lot> a_expirableLots, Item a_item) : base(a_time)
    {
        m_item = a_item;
        ExpirableLots = a_expirableLots;
    }

    internal ShelfLifeEvent(long a_time, Lot a_expirableLot) : base(a_time)
    {
        m_item = a_expirableLot.Item;
        ExpirableLots = [a_expirableLot];
    }

    internal const int UNIQUE_ID = 47;

    internal override int UniqueId => UNIQUE_ID;

    internal List<Lot> ExpirableLots { get; }

    public Item Item => m_item;
}