namespace PT.Scheduler;

/// <summary>
/// A ResourceSpan for a ResourceReservation.
/// One of these is created for each ResourceReservation and is added to m_resourceSpans.
/// When a block is scheduled for the ResourceReservation, this object is then removed from m_resourceSpans
/// and replaced with a BlockResourceSpan.
/// </summary>
internal class ResourceReservationSpan : ResourceSpan
{
    internal ResourceReservationSpan(InternalActivity a_ia, long a_start, long a_end) : base(a_ia, a_start, a_end) { }
}