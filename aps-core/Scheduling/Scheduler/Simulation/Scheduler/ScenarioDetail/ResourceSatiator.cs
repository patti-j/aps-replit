using PT.Scheduler.Simulation;

namespace PT.Scheduler;

/// <summary>
/// Used to store information about which resource to use to satisfy a resource requirement.
/// </summary>
public class ResourceSatiator
{
    internal ResourceSatiator(Resource a_res, BlockReservation a_reservation)
    {
        Resource = a_res;
        Reservation = a_reservation;
    }

    internal ResourceSatiator(Resource a_res)
    {
        Resource = a_res;
    }

    internal ResourceSatiator(ResourceSatiator a_rs)
    {
        Resource = a_rs.Resource;
        Reservation = a_rs.Reservation;
    }

    /// <summary>
    /// Whether a reservation should be made on the resource instead of scheduling it at the current simulation clock.
    /// </summary>
    internal bool BlockReservation => Reservation != null;

    /// <summary>
    /// The resource to use to satisfy the resource requirement.
    /// </summary>
    public readonly Resource Resource;

    /// <summary>
    /// If set, the requirement is to be satisified after the simulation clock, this specifies when to schedule the block.
    /// </summary>
    internal readonly BlockReservation Reservation;

    public override string ToString()
    {
        return string.Format("res={0}; rvn={1}", Resource, Reservation);
    }
}