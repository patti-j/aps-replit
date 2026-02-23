namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Summary description for RetryForCleanoutEvent.
/// </summary>
internal class StorageAreaUsageEvent : EventBase
{
    private readonly StorageAreaConnector m_connector;
    private readonly StorageArea m_storageArea;
    private readonly bool m_inflow;
    
    internal StorageArea StorageArea => m_storageArea;
    internal bool Inflow => m_inflow;
    internal StorageAreaConnector StorageAreaConnector => m_connector;

    /// <summary>
    /// Storage Area usage expires
    /// </summary>
    internal StorageAreaUsageEvent(long a_time, StorageArea a_storageArea, bool a_inflow)
        : base(a_time)
    {
        m_storageArea = a_storageArea;
        m_inflow = a_inflow;
    }

    /// <summary>
    /// Storage Area Connector usage expires
    /// </summary>
    internal StorageAreaUsageEvent(long a_time, StorageAreaConnector a_connector)
        : base(a_time)
    {
        m_connector = a_connector;
    }

    internal const int UNIQUE_ID = 58;
    internal override int UniqueId => UNIQUE_ID;
}