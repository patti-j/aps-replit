namespace PT.Scheduler.Simulation.Events;

internal class ResourceReservationEvent : ResourceEvent
{
    internal ResourceReservationEvent(long a_time, Resource a_resource, ResourceReservation a_resRvn)
        : base(a_time, a_resource)
    {
        m_resRvn = a_resRvn;
    }

    private readonly ResourceReservation m_resRvn;

    internal ResourceReservation ResRvn => m_resRvn;

    internal const int UNIQUE_ID = 31;

    internal override int UniqueId => UNIQUE_ID;
}