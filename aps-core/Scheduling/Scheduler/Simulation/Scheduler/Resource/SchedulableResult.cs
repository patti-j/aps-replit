namespace PT.Scheduler.Simulation;

internal class SchedulableResult
{
    internal SchedulableResult(SchedulableSuccessFailureEnum a_result, OperationCapacityProfile a_ocp)
    {
        m_result = a_result;
        CapacityProfileResult = a_ocp;
    }

    internal SchedulableResult(SchedulableSuccessFailureEnum a_result, OperationCapacityProfile a_ocp, long a_retryTime)
    {
        m_result = a_result;
        CapacityProfileResult = a_ocp;
        m_retryTime = a_retryTime;
    }

    internal SchedulableResult(SchedulableInfo a_si, OperationCapacityProfile a_ocp)
    {
        m_si = a_si;
        m_result = SchedulableSuccessFailureEnum.Success;
        CapacityProfileResult = a_ocp;
    }

    internal SchedulableResult(long a_retryTime)
    {
        m_retryTime = a_retryTime;
        m_result = SchedulableSuccessFailureEnum.LackCapacityWithRetry;
        CapacityProfileResult = new OperationCapacityProfile();
    }

    internal SchedulableResult(FindCapacityResult a_capacityResult, OperationCapacityProfile a_ocp)
    {
        m_result = a_capacityResult.ResultStatus;
        m_findCapacityResult = a_capacityResult;
        m_leftSequenceValues = a_capacityResult.LeftSequenceValues;
        m_retryTime = a_capacityResult.NextStartTime;
        CapacityProfileResult = a_ocp;
    }

    internal SchedulableSuccessFailureEnum m_result;
    private readonly long m_retryTime;
    internal long RetryTime => m_retryTime;

    private readonly LeftNeighborSequenceValues m_leftSequenceValues;
    internal LeftNeighborSequenceValues LeftSequenceValues => m_leftSequenceValues;

    internal OperationCapacityProfile CapacityProfileResult;

    private FindCapacityResult m_findCapacityResult;
    internal FindCapacityResult FindCapacityResult => m_findCapacityResult;

    // Success variables
    internal SchedulableInfo m_si;

    public override string ToString()
    {
        string result;
        if (m_si != null)
        {
            result = m_si.ToString();
        }
        else
        {
            result = m_result.ToString();
        }

        return result;
    }
}