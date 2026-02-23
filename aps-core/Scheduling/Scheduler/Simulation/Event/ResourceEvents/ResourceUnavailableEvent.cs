using System.Text;

namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Indicates that the specified Resource will be unavailable for scheduling starting at that time.
/// Occurs when an Activity or Offline time starts.
/// </summary>
public class ResourceUnavailableEvent : ResourceEvent
{
    /// <summary>
    /// </summary>
    /// <param name="a_time">The time at which the resouce becomes available.</param>
    /// <param name="a_resource">The resource to make unavailable for any reason, such as the end of an online time, capacity usage, etc.</param>
    /// <param name="a_lastUsedInterval">
    /// If known, pass in the the resource's last interval that wasn't completely consumed. Otherwise pass in null. This may be used to make it faster to search for ht next
    /// unused interval.
    /// </param>
    /// <param name="a_nextAvailableResourceTimeTicks" If you know the next time the resource should be available, specify it or pass in -1 to indicate normal processing which is the resource will be made
    ///     available again at the start of the next availalbe online time.
    /// </param>
    internal ResourceUnavailableEvent(long a_time, Resource a_resource, LinkedListNode<ResourceCapacityInterval> a_lastUsedInterval, long a_nextAvailableResourceTimeTicks)
        : base(a_time, a_resource)
    {
        LastUsedCapacityIntervalNode = a_lastUsedInterval;
        NextAvailableResourceTimeTicks = a_nextAvailableResourceTimeTicks;
    }

    internal const int UNIQUE_ID = 28;

    internal override int UniqueId => UNIQUE_ID;

    /// <summary>
    /// This value can be null. If specified, it's the last interval that was used prior to the ResourceUnavailableEvent being scheudled.
    /// </summary>
    internal LinkedListNode<ResourceCapacityInterval> LastUsedCapacityIntervalNode;

    /// <summary>
    /// If this value isn't equal to -1 then it specifies the time the resource should be made available again.
    /// </summary>
    internal long NextAvailableResourceTimeTicks { get; private set; }

    public override string ToString()
    {
        StringBuilder sb = new ();
        string baseToString = base.ToString();
        sb.AppendFormat("ResourceUnavailableEvent:{0}", baseToString);
        return sb.ToString();
    }
}