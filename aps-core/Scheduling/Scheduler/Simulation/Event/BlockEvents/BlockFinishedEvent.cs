namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Indicates that the finish time of a block has just occurred.
/// This event was created for the sake of MultiTasking resources.
/// These resources need to trigger this event for the sake of other activiites
/// that may require a multitasking resource.
/// Currently this event is only created for every block scheduled on a multitasking resource.
/// </summary>
public class BlockFinishedEvent : EventBase
{
    public BlockFinishedEvent(long a_time, Block a_block)
        : base(a_time)
    {
        m_block = a_block;
    }

    private readonly Block m_block;

    /// <summary>
    /// The block this event is for.
    /// </summary>
    public Block Block => m_block;

    public override string ToString()
    {
        return string.Format("{0} {1}", base.ToString(), m_block.Id);
    }

    internal const int UNIQUE_ID = 12;

    internal override int UniqueId => UNIQUE_ID;
}