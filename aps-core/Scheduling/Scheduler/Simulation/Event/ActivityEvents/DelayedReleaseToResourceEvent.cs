namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// When processing the release of an activity to a resource the specifics of the resource might require the release be delayed.
/// For instance the following could cause the need to delay the release: JIT release, regular releases head start span, and usage of the department EndOfFronzenSpan which also affects the
/// EndOfStableSpan.
/// </summary>
internal class DelayedReleaseToResourceEvent : ReleaseEvent
{
    internal DelayedReleaseToResourceEvent(long a_time, InternalActivity a_act, InternalResource a_resource)
        : base(a_time, a_act)
    {
        Resource = a_resource;
    }

    /// <summary>
    /// The resource whose dispatcher you want to add the activity to.
    /// </summary>
    internal InternalResource Resource { get; }

    internal const int UNIQUE_ID = 4;

    internal override int UniqueId => UNIQUE_ID;

    public override string ToString()
    {
        return DateTimeHelper.ToLocalTimeFromUTCTicks(Time) + ":: Act:" + Activity.Operation.Name + ";" + Activity.Operation.ExternalId + ":: Res:" + Resource.Name + ";" + Resource.ExternalId;
    }
}