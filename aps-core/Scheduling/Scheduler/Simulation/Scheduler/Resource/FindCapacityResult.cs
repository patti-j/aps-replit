using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PT.Common.Debugging;
using PT.Scheduler.Simulation;

namespace PT.Scheduler;

internal class FindCapacityResult
{
    /// <summary>
    /// Success, or the reason capacity wasn't found
    /// </summary>
    internal SchedulableSuccessFailureEnum ResultStatus;

    /// <summary>
    /// The calendar end date of the capacity
    /// </summary>
    internal long FinishDate;

    /// <summary>
    /// Capacity broken down by cycle for use in transfers
    /// </summary>
    internal CapacityUsageProfile CapacityUsageProfile;

    /// <summary>
    /// The end date of the obstacle blocking capacity to retry afterward next time
    /// </summary>
    internal long NextStartTime;

    internal LeftNeighborSequenceValues LeftSequenceValues;

    /// <summary>
    /// Success
    /// </summary>
    /// <param name="a_finishDate"></param>
    /// <param name="a_ocp"></param>
    internal FindCapacityResult(long a_finishDate, CapacityUsageProfile a_ocp)
    {
        #if DEBUG
        if (a_ocp == null)
        {
            //We need this for UI enumeration, so don't allow null values.
            throw new DebugException("Capacity Usage Profile cannot be null");
        }
        #endif

        ResultStatus = SchedulableSuccessFailureEnum.Success;
        FinishDate = a_finishDate;
        CapacityUsageProfile = a_ocp;
    }

    internal FindCapacityResult(SchedulableSuccessFailureEnum a_reason, long a_retryTime, CapacityUsageProfile a_ocp)
    {
        #if DEBUG
        if (a_ocp == null)
        {
            //We need this for UI enumeration, so don't allow null values.
            throw new DebugException("Capacity Usage Profile cannot be null");
        }
        #endif

        ResultStatus = a_reason;
        NextStartTime = a_retryTime;
        CapacityUsageProfile = a_ocp;
    }

    /// <summary>
    /// No capacity was found
    /// </summary>
    /// <param name="a_reason"></param>
    /// <param name="a_retryTime"></param>
    internal FindCapacityResult(SchedulableSuccessFailureEnum a_reason, long a_retryTime)
    {
        ResultStatus = a_reason;
        NextStartTime = a_retryTime;
        CapacityUsageProfile = new CapacityUsageProfile();
    }

    /// <summary>
    /// No capacity was found
    /// </summary>
    /// <param name="a_reason"></param>
    /// <param name="a_leftSequenceValues"></param>
    internal FindCapacityResult(SchedulableSuccessFailureEnum a_reason, LeftNeighborSequenceValues a_leftSequenceValues)
    {
        ResultStatus = a_reason;
        LeftSequenceValues = a_leftSequenceValues;
        NextStartTime = a_leftSequenceValues.EndDate;
        CapacityUsageProfile = new CapacityUsageProfile();
    }

    internal void SetLeftSequenceValues(LeftNeighborSequenceValues a_sequenceValues)
    {
        LeftSequenceValues = a_sequenceValues;
        NextStartTime = a_sequenceValues.EndDate;
    }
}
