using PT.Common.PTMath;

namespace PT.Scheduler.Simulation;

/// <summary>
/// [USAGE_CODE]: Used to specify a future interval of time on a resource reserved for a block.
/// </summary>
internal class BlockReservation : Interval
{
    /// <summary>
    /// Creae a reservation.
    /// </summary>
    /// <param name="a_act">The activity the reservation is for. </param>
    /// <param name="a_rr">The RR the act is for.</param>
    /// <param name="a_startTicks">The start of the block.</param>
    /// <param name="a_endTicks">the end of the block</param>
    /// <param name="a_si">The SI data used to determine the reservation.</param>
    internal BlockReservation(InternalActivity a_act, ResourceRequirement a_rr, long a_startTicks, long a_endTicks, SchedulableInfo a_si)
    {
        Activity = a_act;
        ResReq = a_rr;
        StartTicks = a_startTicks;
        EndTicks = a_endTicks;
        SchedulableInfo = a_si;
    }

    /// <summary>
    /// The act the reservation is for.
    /// </summary>
    internal readonly InternalActivity Activity;

    /// <summary>
    /// The RR the reservation is for.
    /// </summary>
    internal readonly ResourceRequirement ResReq;

    /// <summary>
    /// The start of the reserved interval.
    /// </summary>
    //internal readonly long StartTicks;

    /// <summary>
    /// The end of the reserved interval.
    /// </summary>
    //internal readonly long EndTicks;

    /// <summary>
    /// Scheduling information about the block.
    /// </summary>
    internal readonly SchedulableInfo SchedulableInfo;

    /// <summary>
    /// When added for BlockReservationSet, this value is set to the node containing the reservation.
    /// This is done to make removals fast.
    /// </summary>
    internal LinkedListNode<BlockReservation> BlockReservationSetNode { get; set; }

    /// <summary>
    /// Whether an interval intersects this block reservation.
    /// </summary>
    /// <param name="a_startIntervalTicks">The start of an interval.</param>
    /// <param name="a_endIntervalTicks">The end of the interval.</param>
    /// <returns>true if the specified interval and this reservation intersect.</returns>
    internal bool Intersects(long a_startIntervalTicks, long a_endIntervalTicks)
    {
        return Intersection(a_startIntervalTicks, a_endIntervalTicks, StartTicks, EndTicks);
    }

    public override string ToString()
    {
        string s = string.Format("{0} to {1}", DateTimeHelper.ToLocalTimeFromUTCTicks(StartTicks), DateTimeHelper.ToLocalTimeFromUTCTicks(EndTicks));
        return s;
    }
}