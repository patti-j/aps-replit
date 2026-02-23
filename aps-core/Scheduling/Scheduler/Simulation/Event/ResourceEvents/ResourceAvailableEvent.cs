namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Indicates that the specified Resource will be available to schedule another
/// Activity.  Occurrs after an Activity is complete or an Offline period ends.
/// </summary>
internal class ResourceAvailableEvent : ResourceEvent
{
    internal ResourceAvailableEvent(long a_time, Resource a_resource, LinkedListNode<ResourceCapacityInterval> a_node)
        : base(a_time, a_resource)
    {
        m_node = a_node;
        EventState = State.Unprocessed;
    }

    private LinkedListNode<ResourceCapacityInterval> m_node;

    internal LinkedListNode<ResourceCapacityInterval> Node => m_node;

    internal const int UNIQUE_ID = 27;

    internal override int UniqueId => UNIQUE_ID;

    public override string ToString()
    {
        if (Resource != null)
        {
            return base.ToString() + ";" + GetType().Name + "::" + DateTimeHelper.ToLongLocalTimeFromUTCTicks(Time) + "; " + Resource.Name + "(" + Resource.Id + ")";
        }

        return base.ToString() + ";" + GetType().Name + "::" + DateTimeHelper.ToLongLocalTimeFromUTCTicks(Time) + "; null";
    }

    internal override void Clear()
    {
        base.Clear();
        m_node = null;
    }

    internal enum State { Unprocessed, Processed, Cancelled }

    /// <summary>
    /// Whether the event should be ingnored. An example of when this may occur:
    /// 1. An event is queued to make the resource offline (for instance due to the end of an online period), the resource's next online period is scheduled to start at the start of the next online period.
    /// 2. While the resource is still online a block is scheduled on the resource that spans several online spans and a new resource next online time is determined (for instance at the end of the block).
    /// 3. The new resource next online event is scheduled.
    /// 4. At this point there are 2 online events scheudled for the resource. If they're both processed, the debuggin test code will find a problem with the state of the variables and will thrown an
    /// exception, if running  a debug build.
    /// 5. At the end of step 3, the first scheudled online event should have been cancelled to prevent both online events from being processed.
    /// </summary>
    internal State EventState { get; set; }
}