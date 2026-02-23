using PT.Scheduler;

namespace PT.SchedulerData;

public static class SuccessorMoData
{
    public static bool IsConstraining(this SuccessorMO a_successorMO)
    {
        return a_successorMO.SuccessorManufacturingOrder != null;
    }
}