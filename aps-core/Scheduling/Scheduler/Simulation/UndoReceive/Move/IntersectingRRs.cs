using System.Collections;

namespace PT.Scheduler;

// [USAGE_CODE] IntersectingRRs: AActs and their RRs that intersect the move.
/// <summary>
/// Used to keep track of which RRs of an activity intersect with the move date.
/// </summary>
public partial class ScenarioDetail
{
    internal class IntersectingRRs : IEnumerable<IntersectingRR>
    {
        /// <summary>
        /// Specify the activity that intersects with the move date.
        /// </summary>
        /// <param name="a_act">The activity that intersects with the move date.</param>
        internal IntersectingRRs(InternalActivity a_act)
        {
            Activity = a_act;
        }

        /// <summary>
        /// The activity that intersects with a resource requirement.
        /// </summary>
        internal InternalActivity Activity { get; private set; }

        /// <summary>
        /// Specify a RR of the activity that intersects with the move date.
        /// </summary>
        /// <param name="a_rr">The resource requirement must be of the activity.</param>
        internal void AddRR(IntersectingRR a_rr)
        {
            m_intersectingRRs.Add(a_rr);
        }

        /// <summary>
        /// The set of RRs that intersect with the move date.
        /// </summary>
        private readonly List<IntersectingRR> m_intersectingRRs = new ();

        /// <summary>
        /// Iterate through the intersecting RRs.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IntersectingRR> GetEnumerator()
        {
            return m_intersectingRRs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}