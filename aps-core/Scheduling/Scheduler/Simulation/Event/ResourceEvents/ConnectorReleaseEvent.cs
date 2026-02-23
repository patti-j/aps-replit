namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Used to release a resource connector after it is no longer in use
/// </summary>
internal class ConnectorReleaseEvent : EventBase
{
    private readonly ResourceConnector m_connector;

    /// <summary>
    /// Specify exactly when all an activity's constraints have been satisfied.
    /// </summary>
    /// <param name="a_time">The time at which all the constraints on the activity have been satisfied.</param>
    /// <param name="a_connector">The connector that will become available again.</param>
    internal ConnectorReleaseEvent(long a_time, ResourceConnector a_connector)
        : base(a_time)
    {
        m_connector = a_connector;
    }

    internal const int UNIQUE_ID = 52;

    internal override int UniqueId => UNIQUE_ID;

    internal ResourceConnector Connector => m_connector;

    /// <summary>
    /// If this connector was used for material transfer, this is the inventory being transferred.
    /// </summary>
    internal Inventory AllocatedInventory;
}