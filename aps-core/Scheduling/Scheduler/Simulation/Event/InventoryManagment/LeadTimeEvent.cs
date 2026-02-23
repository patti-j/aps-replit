namespace PT.Scheduler.Simulation.Events;

/// For Inventory MaterialRequirements lead-time events are created.
/// There is no specific action necessary when the event occurs.
/// It exists simply to make sure the activity has the opportunity to schedule at the lead-time,
/// but it is possible that the activity has already scheduled due to Inventory becoming
/// available earlier.  The Inventory may have been made available either through existing stock,
/// Purchase To Stocks, or Activities with material destined for stock.
public class LeadTimeEvent : EventBase
{
    public LeadTimeEvent(long a_ime, MaterialRequirement a_mR, Warehouse a_warehouse, bool a_customizedLeadTime)
        : base(a_ime)
    {
        m_mr = a_mR;
        m_warehouse = a_warehouse;
        m_customizedLeadTime = a_customizedLeadTime;
        Item = a_mR.Item;
    }

    public LeadTimeEvent(long a_ime, Item a_item, Warehouse a_warehouse)
        : base(a_ime)
    {
        Item = a_item;
        m_warehouse = a_warehouse;
    }

    private readonly MaterialRequirement m_mr;
    private Warehouse m_warehouse;
    private readonly bool m_customizedLeadTime;

    /// <summary>
    /// Whether this MR specific lead time event is different from the standard inventory release events
    /// </summary>
    public bool CustomizedLeadTime => m_customizedLeadTime;

    internal Item Item;
    internal MaterialRequirement MaterialRequirement => m_mr;

    internal const int UNIQUE_ID = 14;

    internal override int UniqueId => UNIQUE_ID;

}