namespace PT.Scheduler.Simulation.Events;

// [USAGE_CODE] UsageEvent: When a resource might be available again.
/// <summary>
/// Created for the ResourceRequirement usage functionality to specify when a resource might be
/// available.
/// For instance if a resource isn't available due to a Reservation or BlockReservation
/// an event will be created at the end of the reservation.
/// </summary>
internal class UsageEvent : EventBase
{
    /// <summary>
    /// Specify a time to try scheduling again.
    /// </summary>
    /// <param name="a_tryAgainTicks">When to try scheduling again.</param>
    /// <param name="a_act">The activity that failed to schedule.</param>
    internal UsageEvent(long a_tryAgainTicks, InternalActivity a_act)
        : base(a_tryAgainTicks)
    {
        Activity = a_act;
    }

    /// <summary>
    /// The activity that couldn't be scheduled.
    /// </summary>
    internal readonly InternalActivity Activity;

    internal const int UNIQUE_ID = 41;

    internal override int UniqueId => UNIQUE_ID;

    public override string ToString()
    {
        return "UsageEvent:" + DateTimeHelper.ToLongLocalTimeFromUTCTicks(Time);
    }
}