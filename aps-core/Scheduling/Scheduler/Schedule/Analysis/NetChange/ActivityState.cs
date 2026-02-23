namespace PT.Scheduler.Schedule.Analysis.NetChange;

public class ActivityState
{
    public ActivityState(InternalActivity aIa)
    {
        ia = aIa;
    }

    public InternalActivity ia;

    public List<ResourceRequirementState> startStates = new ();
    public List<ResourceRequirementState> endStates = new ();

    internal bool EqualResourceRequirementStates()
    {
        if (startStates.Count != endStates.Count)
        {
            return false;
        }

        for (int rrsI = 0; rrsI < startStates.Count; ++rrsI)
        {
            if (!startStates[rrsI].Eq(endStates[rrsI]))
            {
                return false;
            }
        }

        return true;
    }
}