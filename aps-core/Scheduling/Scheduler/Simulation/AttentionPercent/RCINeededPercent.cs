using System.Diagnostics;

using PT.Common.PTMath;

namespace PT.Scheduler;

internal partial class RCINeededPercentManager
{
    /// <summary>
    /// The specification of a percentage required of a RCI.
    /// </summary>
    internal class RCINeededPercent : Interval
    {
        internal RCINeededPercent(InternalActivity a_act, ResourceCapacityInterval a_RCI, decimal a_neededPercent, ResourceRequirement a_resourceRequirement, long a_startDate, long a_endDate)
        {
            Activity = a_act;
            RCI = a_RCI;
            PercentNeeded = a_neededPercent;
            ResourceRequirement = a_resourceRequirement;
            StartTicks = a_startDate;
            EndTicks = a_endDate;

            Validate();
        }

        /// <summary>
        /// The RCI a percent of attention is required of.
        /// </summary>
        internal readonly ResourceCapacityInterval RCI;

        /// <summary>
        /// The amount of attention required.
        /// </summary>
        internal readonly decimal PercentNeeded;

        internal readonly InternalActivity Activity;
        internal readonly ResourceRequirement ResourceRequirement;

        [Conditional("TEST")]
        private void Validate()
        {
            if (PercentNeeded < 0)
            {
                throw new Exception("Bad attention percent specified.");
            }
        }

        public override string ToString()
        {
            return string.Format("Operation={0}; Needed Percent={1}; RCI={2}", Activity.Operation.ExternalId, PercentNeeded, RCI);
        }
    }
}