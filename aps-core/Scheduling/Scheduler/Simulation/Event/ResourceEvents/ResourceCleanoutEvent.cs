namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Indicates that the specified Resource will be available to schedule another
/// Activity.  Occurrs after an Activity is complete or an Offline period ends.
/// </summary>
internal class ResourceCleanoutEvent : ResourceEvent
{
    internal ResourceCleanoutEvent(long a_time, Resource a_resource, LinkedListNode<ResourceCapacityInterval> a_node)
        : base(a_time, a_resource)
    {
        m_node = a_node;
    }

    private readonly LinkedListNode<ResourceCapacityInterval> m_node;
    internal LinkedListNode<ResourceCapacityInterval> Node => m_node;

    internal const int UNIQUE_ID = 10;
    internal override int UniqueId => UNIQUE_ID;
}