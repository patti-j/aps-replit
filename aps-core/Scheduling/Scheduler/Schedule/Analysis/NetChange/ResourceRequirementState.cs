namespace PT.Scheduler.Schedule.Analysis.NetChange;

public class ResourceRequirementState
{
    public ResourceRequirementState(ResourceBlock rb)
    {
        StartTicks = rb.StartTicks;
        FinishTicks = rb.EndTicks;
        Res = rb.ScheduledResource;
    }

    public ResourceRequirementState()
    {
        StartTicks = -1;
        FinishTicks = -1;
        Res = null;
    }

    public bool Eq(ResourceRequirementState rrs)
    {
        return StartTicks == rrs.StartTicks && FinishTicks == rrs.FinishTicks && ReferenceEquals(Res, rrs.Res);
    }

    public readonly long StartTicks;
    public readonly long FinishTicks;
    public readonly InternalResource Res;
}