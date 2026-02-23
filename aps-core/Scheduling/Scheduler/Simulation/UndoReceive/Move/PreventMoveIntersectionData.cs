using PT.SchedulerDefinitions;

namespace PT.Scheduler.Simulation.UndoReceive.Move;

// [USAGE_CODE] PreventMoveIntersectionData: An activity that will be constrained when the move is re-received. Or a move activity whose move date has been predetermined by a prior move re-apply.
/// <summary>
/// Calculated while receiving a move and used when simulating the re-received transmission to put earliest start constraints on activities.
/// For instance during a simulation it might be determined that it would be better off to delay scheduling	of an activity to fit around a
/// moved block. In particular, when an intersecting block whose UsageStart==Run can be scheduled right after the move block (the earliest
/// release date needs to be back calculated to squeeze everything in tightly).
/// This is also used to record the move times of blocks.
/// </summary>
internal struct ActivityReleaseTime
{
    /// <summary>
    /// A key to the activity is stored instead of a reference to the activity because the undo recreates SD
    /// which us composed of completely new objects copied from the original SD.
    /// </summary>
    internal readonly ActivityKey ActivityKey;

    internal readonly InternalActivity Activity;

    /// <summary>
    /// A earliest start constraint placed on the activity.
    /// </summary>
    internal readonly long ReleaseTicks;

    /// <summary>
    /// Specify a constrained activities key and earliest start.
    /// </summary>
    /// <param name="a_act"></param>
    /// <param name="a_releaseTicks"></param>
    internal ActivityReleaseTime(long a_releaseTicks, InternalActivity a_act)
    {
        ActivityKey ak = new (a_act.Job.Id, a_act.ManufacturingOrder.Id, a_act.Operation.Id, a_act.Id);
        ActivityKey = ak;
        Activity = a_act;
        ReleaseTicks = a_releaseTicks;
    }

    public override string ToString()
    {
        return string.Format("{ReleaseTicks={0}; Op Name={1}", DateTimeHelper.ToLocalTimeFromUTCTicks(ReleaseTicks), Activity.Operation.Name);
    }
}