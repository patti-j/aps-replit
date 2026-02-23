namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// [USAGE_CODE]: Event to schedule a block reservation.
/// </summary>
internal class BlockReservationEvent : ResourceEvent
{
    /// <summary>
    /// Constructor. Many of the parameters are required only because the function that schedules a block on a resource needs them
    /// and they were readily available in AttemptToScheduleResource. Some parameters might be in a_reservation and removable.
    /// </summary>
    /// <param name="a_time">The time to sched the block.</param>
    /// <param name="a_resource">The resource to sched the block on.</param>
    /// <param name="a_tank">Whether the resource is a tank.</param>
    /// <param name="a_hasProductsToStoreInTank"></param>
    /// <param name="a_primaryResource">The primary res used in scheduling the block.</param>
    /// <param name="a_reservation">The reservation.</param>
    /// <param name="a_rri">The index of the RR being scheduled.</param>
    /// <param name="a_batchToJoin">The batch the activity will be a part of if any.</param>
    /// <param name="a_newBatch">A new batch if he activity won't be a part of an existing batch.</param>
    internal BlockReservationEvent(long a_time, Resource a_resource, Resource a_tank, bool a_hasProductsToStoreInTank, Resource a_primaryResource, BlockReservation a_reservation, int a_rri, ScenarioDetail.BatchToJoinTempStruct a_batchToJoin, Batch a_newBatch)
        : base(a_time, a_resource)
    {
        PrimaryResource = a_primaryResource;
        Reservation = a_reservation;
        BatchToJoin = a_batchToJoin;
        RRIdx = a_rri;
        NewBatch = a_newBatch;
        HasProductsToStoreInTank = a_hasProductsToStoreInTank;
    }

    internal readonly Resource PrimaryResource;
    internal readonly Resource Tank;
    internal readonly BlockReservation Reservation;
    internal readonly ScenarioDetail.BatchToJoinTempStruct BatchToJoin;

    internal InternalActivity Activity => Reservation.Activity;

    internal ResourceRequirement Requirement => Reservation.ResReq;

    internal readonly int RRIdx;
    internal readonly Batch NewBatch;
    internal readonly bool HasProductsToStoreInTank;

    internal const int UNIQUE_ID = 40;

    internal override int UniqueId => UNIQUE_ID;

    public override string ToString()
    {
        return DateTimeHelper.ToLocalTimeFromUTCTicks(Time) + "; " + Reservation;
    }
}