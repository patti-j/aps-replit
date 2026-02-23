using PT.SchedulerDefinitions;

namespace PT.Scheduler.Simulation.UndoReceive.Move;

// [USAGE_CODE] UndoReceiveMove: Whether to undo and re-receive a move transmission and extra data necessary to re-receive the transmission and produce a better result.
/// <summary>
/// Used by ScenarioDetail to indicate whether the received move transmission should be undone
/// and whether it should be re-received.
/// </summary>
internal class UndoReceiveMove : UndoReceive
{
    /// <summary>
    /// If the move is to be re-received, this may specify a different move time than the original move time.
    /// To check if a new move time is being specified, check the value of HasReReceiveMoveTicks
    /// </summary>
    internal long ReReceiveMoveTicks { get; set; }

    /// <summary>
    /// Whether the ReReceiveMoveTicks specifies a new time for the move.
    /// </summary>
    internal bool HasReReceiveMoveTicks => ReReceiveMoveTicks > 0;

    /// <summary>
    /// The SchedulableInfo at the time move activity was able to be scheduled.
    /// This is used to reapply the transmission at the time when the move is possible instead of the drop time.
    /// Reapplying allows blocks that intersect the move time to squeeze in around the move time more efficiently.
    /// </summary>
    internal SchedulableInfo SI { get; set; }

    internal enum ReapplyTypeEnum
    {
        /// <summary>
        /// Normal move processing. The move to resources are locked for the move activity.
        /// </summary>
        Regular,

        /// <summary>
        /// Move resources are not reserved. Other activities are allowed to schedule before the move activity. The move activity will fall in place
        /// whereever it can. Some reasons this may be done include: to consume unused capacity in the case where it's not possible for the move activity
        /// to schedule at the move time, or activities that use the move to resources supply material to the move activity and must be scheduled first.
        /// </summary>
        NonLockingMoveRelease
    }

    /// <summary>
    /// The previous type of the MoveT, if any.
    /// </summary>
    internal ReapplyTypeEnum ReapplyTypePrevious { get; set; }

    /// <summary>
    /// The current type of apply of the MoveT.
    /// </summary>
    internal ReapplyTypeEnum ReapplyType { get; set; }

    private int m_nbrOfReapplies;

    /// <summary>
    /// The number of times this transmission has been re-applied.
    /// </summary>
    internal int NbrOfReapplies => m_nbrOfReapplies;

    /// <summary>
    /// Call this function whenever the transmission is being re-applied.
    /// </summary>
    internal void IncrementReapplies()
    {
        ++m_nbrOfReapplies;
    }

    /// <summary>
    /// Reset the values of the object except for a few whose purpose is in-simulation.
    /// </summary>
    internal override void Reset()
    {
        base.Reset();

        ReapplyTypePrevious = ReapplyType;
        ReapplyType = ReapplyTypeEnum.Regular;

        ReReceiveMoveTicks = 0;

        SI = null;

        m_intersectorReleases.Clear();
    }

    #region Activities whose start dates should be constrained when the move is re-received.
    /// <summary>
    /// Before reapplying the transmission, simulation back calculated new reltease times of activities that
    /// intersected with the move date.
    /// </summary>
    private readonly List<ActivityReleaseTime> m_intersectorReleases = new ();

    /// <summary>
    /// During a simulation of a re-received transmission, constraints for processing re-received transmission are
    /// added for activities.
    /// This is
    /// Step 2
    /// New constraint times are stored that prevent intersection with the move time but allow the activity to be
    /// scheduled tightly with the moved blocks.
    /// </summary>
    /// <param name="a_backCalculatedReleaseTicks">The activity can't be scheduled to start before this date.</param>
    /// <param name="a_act">The activity to constrain.</param>
    internal void AddIntersectorRelease(long a_backCalculatedReleaseTicks, InternalActivity a_act)
    {
        m_intersectorReleases.Add(new ActivityReleaseTime(a_backCalculatedReleaseTicks, a_act));
    }

    /// <summary>
    /// Enumerate through the simulation constraints placed on activities during a re-receive of a transmission.
    /// </summary>
    /// <returns></returns>
    internal IEnumerator<ActivityReleaseTime> GetEnumeratorOfIntersectorReleases()
    {
        return m_intersectorReleases.GetEnumerator();
    }

    /// <summary>
    /// These are the times when each move activities are actually able to schedule.
    /// When a move is reapplied, these times are locked and each of these activities
    /// is fixed to the time that was determined it can schedule in a previous simulation.
    /// </summary>
    private readonly List<ActivityReleaseTime> m_knownMoveTicks = new ();

    /// <summary>
    /// When a move activity schedule, call this function.
    /// These are needed to re-apply the move to make sure each activity schedules exactly where it did
    /// the first time it scheduled in the re-apply process.
    /// </summary>
    /// <param name="a_schedTicks"></param>
    /// <param name="a_act"></param>
    internal void AddKnownMoveTicks(long a_schedTicks, InternalActivity a_act)
    {
        m_knownMoveTicks.Add(new ActivityReleaseTime(a_schedTicks, a_act));
    }

    /// <summary>
    /// These are actual times where move activities will schedule based on attempted simulations within the reapply MoveT process.
    /// When an activity is scheduled, its scheduled date is saved. Then if a reapply of the MoveT is necessary, the block whose
    /// move times are already known will be rescheduled back to where they're specified by this function.
    /// </summary>
    /// <param name="a_idx"></param>
    /// <param name="r_schedTicks">The time the activity scheduled in the re-apply process.</param>
    /// <returns>Whether the specified index exists.</returns>
    internal bool GetKnownMoveTicks(int a_idx, out ActivityReleaseTime r_schedTicks)
    {
        if (a_idx < m_knownMoveTicks.Count)
        {
            r_schedTicks = m_knownMoveTicks[a_idx];
            return true;
        }

        r_schedTicks = new ActivityReleaseTime();
        return false;
    }

    /// <summary>
    /// Whether the time the move activity will end up being scheduled has been determined.
    /// </summary>
    /// <param name="a_act">The activity whose status is being sought.</param>
    /// <returns>true if the time has been determined.</returns>
    internal bool HasKnownMoveTicks(InternalActivity a_act)
    {
        for (int actI = 0; actI < m_knownMoveTicks.Count; ++actI)
        {
            ActivityKey ak = new (a_act.Job.Id, a_act.ManufacturingOrder.Id, a_act.Operation.Id, a_act.Id);
            if (m_knownMoveTicks[actI].ActivityKey == ak)
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}