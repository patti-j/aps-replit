namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Indicates the ready time of a predecessor MO. The predecessor task releases its successor MO.
/// at the task level or operation level.
/// </summary>
public class PredecessorMOAvailableEvent : EventBase
{
    #region Construction
    public PredecessorMOAvailableEvent(long a_time, ManufacturingOrder a_predecessor, BaseOperation a_successor)
        : base(a_time)
    {
        m_predecessorMO = a_predecessor;
        m_successorOperation = a_successor;
        m_successorMO = null;
    }

    public PredecessorMOAvailableEvent(long a_time, ManufacturingOrder a_predecessor, ManufacturingOrder a_successor)
        : base(a_time)
    {
        m_predecessorMO = a_predecessor;
        m_successorOperation = null;
        m_successorMO = a_successor;
    }
    #endregion

    private readonly ManufacturingOrder m_predecessorMO;

    /// <summary>
    /// The MO that has made material available for the successor MO.
    /// </summary>
    internal ManufacturingOrder PredecessorMO => m_predecessorMO;

    private readonly ManufacturingOrder m_successorMO;

    /// <summary>
    /// The MO that depends on the predecessor MOs material.
    /// This field is set to NULL if the predecessor constrains the successor at the operation level.
    /// </summary>
    internal ManufacturingOrder SuccessorMO => m_successorMO;

    private readonly BaseOperation m_successorOperation;

    /// <summary>
    /// The successor operation that depends on the predecessor MOs material.
    /// This field is set to NULL if the predecessor constrains the successor at the MO level.
    /// </summary>
    internal BaseOperation SuccessorOperation => m_successorOperation;

    public override string ToString()
    {
        return string.Format("{0}: {1}: {2}: {3}", base.ToString(), m_predecessorMO.Name, m_successorMO.Name, m_successorOperation.Name);
    }

    internal const int UNIQUE_ID = 20;

    internal override int UniqueId => UNIQUE_ID;
}