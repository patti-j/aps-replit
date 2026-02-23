using PT.Scheduler.Simulation.Events;

namespace PT.Scheduler;

/// <summary>
/// A collection of data needed to determine the priority of dispatching
/// </summary>
internal struct ResourceDispatchData : IEquatable<ResourceDispatchData>
{
    public ResourceAndEventPair ResourcePair;
    public decimal Score;
    public bool InProcess;
    public InternalActivity ActivityToSchedule;

    public bool Equals(ResourceDispatchData a_other)
    {
        return ResourcePair.Equals(a_other.ResourcePair) && ActivityToSchedule.Id.Equals(a_other.ActivityToSchedule.Id);
    }

    public override bool Equals(object a_obj)
    {
        return a_obj is ResourceDispatchData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ResourcePair.Resource.Id, ActivityToSchedule.Id);
    }
}

/// <summary>
/// A resource and it's associated available node event
/// </summary>
internal struct ResourceAndEventPair : IEquatable<ResourceAndEventPair>
{
    internal ResourceAndEventPair(Resource a_resource, ResourceAvailableEvent a_resourceAvailableEvent)
    {
        Resource = a_resource;
        ResourceAvailableEvent = a_resourceAvailableEvent;
    }

    public Resource Resource;
    public ResourceAvailableEvent ResourceAvailableEvent;

    public bool Equals(ResourceAndEventPair a_other)
    {
        return Resource.Id.Equals(a_other.Resource.Id);
    }

    public override bool Equals(object a_obj)
    {
        return a_obj is ResourceAndEventPair other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Resource.Id);
    }
}