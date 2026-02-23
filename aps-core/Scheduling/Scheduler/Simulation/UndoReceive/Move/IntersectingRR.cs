namespace PT.Scheduler;

// [USAGE_CODE] IntersectingRR. A RR that intersects the move.
/// <summary>
/// Used to keep track of which RRs of an activity intersect with the move date.
/// </summary>
public partial class ScenarioDetail
{
    internal class IntersectingRR
    {
        internal IntersectingRR(long a_attemptedStartTicks, InternalActivity a_act, ResourceRequirement a_rr, int a_rrIdx, Resource a_res)
        {
            AttemptedStartTicks = a_attemptedStartTicks;
            Activity = a_act;
            RR = a_rr;
            RRIdx = a_rrIdx;
            Res = a_res;
        }

        internal readonly long AttemptedStartTicks;

        internal readonly InternalActivity Activity;

        /// <summary>
        /// The RR that intersects with the move date.
        /// </summary>
        internal readonly ResourceRequirement RR;

        internal readonly int RRIdx;

        /// <summary>
        /// The resource that RR intersects the move date on.
        /// </summary>
        internal readonly Resource Res;
    }
}