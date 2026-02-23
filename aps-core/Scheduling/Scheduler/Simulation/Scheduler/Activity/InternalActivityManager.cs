namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of InternalActivity objects.
/// </summary>
public partial class InternalActivityManager
{
    internal void AbsorbReportedValues(InternalActivityManager a_absorbOp)
    {
        InternalActivity ia = GetByIndex(0);
        InternalActivity absorbIA = a_absorbOp.GetByIndex(0);
        ia.AbsorbReportedValues(absorbIA);
    }

    internal bool AnyFinishedActivities()
    {
        for (int actI = 0; actI < Count; ++actI)
        {
            InternalActivity ia = (InternalActivity)Activities.GetByIndex(actI);

            if (ia.Finished)
            {
                return true;
            }
        }

        return false;
    }

    internal IEnumerable<InternalActivity> UnfinishedActivities()
    {
        for (int actI = 0; actI < Count; ++actI)
        {
            InternalActivity ia = (InternalActivity)Activities.GetByIndex(actI);

            if (!ia.Finished)
            {
                yield return ia;
            }
        }
    }
}