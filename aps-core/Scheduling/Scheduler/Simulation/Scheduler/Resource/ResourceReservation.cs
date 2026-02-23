using PT.Common.PTMath;

namespace PT.Scheduler;

public class ResourceReservation : Interval
{
    /// <summary>
    /// Constructor for a primary resource
    /// </summary>
    /// <param name="a_si"></param>
    /// <param name="a_act"></param>
    /// <param name="a_keepSuccessor">Whether this reservation is for the same resource as the predecessor's reservation</param>
    internal ResourceReservation(SchedulableInfo a_si, InternalActivity a_act, Resource a_predecessorResource)
    {
        StartTicks = a_si.m_scheduledStartDate;
        EndTicks = a_si.m_finishDate;
        m_res = a_si.m_resource;
        m_ia = a_act;
        m_rrIndex = a_act.Operation.ResourceRequirements.PrimaryResourceRequirementIndex;
        m_primaryReservation = true;
        KeepSuccessor = a_si.m_resource == a_predecessorResource;
    }

    /// <summary>
    /// Constructor for a helper resource
    /// </summary>
    /// <param name="a_resource"></param>
    /// <param name="a_act"></param>
    /// <param name="a_rrIndex"></param>
    /// <param name="a_startDate"></param>
    /// <param name="a_endDate"></param>
    /// <param name="a_keepSuccessor">Whether this reservation is for the same resource as the predecessor's reservation</param>
    internal ResourceReservation(Resource a_resource, InternalActivity a_act, int a_rrIndex, long a_startDate, long a_endDate, bool a_keepSuccessor)
    {
        StartTicks = a_startDate;
        EndTicks = a_endDate;
        m_ia = a_act;
        m_res = a_resource;
        m_rrIndex = a_rrIndex;
        m_primaryReservation = m_rrIndex == a_act.Operation.ResourceRequirements.PrimaryResourceRequirementIndex;
        KeepSuccessor = a_keepSuccessor;
    }

    /// <summary>
    /// Constructor for moving a reservation
    /// </summary>
    /// <param name="a_resource"></param>
    /// <param name="a_act"></param>
    /// <param name="a_rrIndex"></param>
    /// <param name="a_startDate"></param>
    /// <param name="a_endDate"></param>
    /// <param name="a_keepSuccessor">Whether this reservation is for the same resource as the predecessor's reservation</param>
    internal ResourceReservation(ResourceReservation a_existingReservation, Resource a_newResource)
    {
        StartTicks = a_existingReservation.StartTicks;
        EndTicks = a_existingReservation.EndTicks;
        m_ia = a_existingReservation.m_ia;
        m_res = a_newResource;
        m_rrIndex = a_existingReservation.m_rrIndex;
        m_primaryReservation = a_existingReservation.m_primaryReservation;
        KeepSuccessor = a_existingReservation.KeepSuccessor;
    }

    internal readonly InternalActivity m_ia;
    internal readonly int m_rrIndex;
    internal Resource m_res;
    internal Resource ReservedResource => m_res;
    private readonly bool m_primaryReservation; //The reservation for the primary resource requirement
    internal bool PrimaryReservation => m_primaryReservation;
    internal bool KeepSuccessor;

    /// <summary>
    /// Whether the resource reservation finishes after the date.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    internal bool FinishesAfter(long a_dt)
    {
        return EndTicks > a_dt; // Everything from the start point up to but not including finishDate is part of the resource reservation.
    }

    /// <summary>
    /// Associates non-infinite resources with this.
    /// Associates the scheduled activity with this.
    /// </summary>
    internal void MakeFirm()
    {
        if (m_res.CapacityType != SchedulerDefinitions.InternalResourceDefs.capacityTypes.Infinite)
        {
            // Resource reservations aren't added to Infinite resources since batches assigned to infinites don't consume capacity.
            m_res.AddReservation(this);
        }

        m_ia.SetResourceReservationForRR(this);
        if (PrimaryReservation && !KeepSuccessor)
        {
            //This activity was reserved for a resource that the predecessor did not schedule on. Don't attempt to keep them together.
            m_ia.Operation.TryToEnforcePredecessorsKeepSuccessor = false;
            //TODO: Max Delay is only compatible with single predecessor / successor
            ((InternalOperation)m_ia.Operation.Predecessors[0].Successor.Operation).TryToEnforcePredecessorsKeepSuccessor = false;
        }
    }

    internal void Scheduled()
    {
        m_res.RemoveFutureReservation(this);
    }

    #region Debugging
    public override string ToString()
    {
        return string.Format("Resource={0}; StartDate={1}; FinishDate={2}; JobName={3}Operation.Name={4}", m_res.Name, StartTicks, EndTicks, m_ia.Job.Name, m_ia.Operation.Name);
    }
    #endregion
}