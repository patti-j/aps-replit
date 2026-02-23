namespace PT.Scheduler;

/// <summary>
/// Sorts the dispatcher by higher score == higher priority
/// </summary>
public partial class BalancedCompositeDispatcherDefinition
{
    private class KeyComparer : IComparer<KeyAndActivity>
    {
        public int Compare(KeyAndActivity x, KeyAndActivity y)
        {
            Key xx = (Key)x.Key;
            Key yy = (Key)y.Key;

            //Order by highest score
            if (xx.Composite > yy.Composite)
            {
                return -1;
            }

            if (xx.Composite < yy.Composite)
            {
                return 1;
            }

            // Smaller sequence IDs have priority
            // SimultaneousSequenceIndex is startTicks + index. So lower index means higher priority
            if (xx.SimultaneousSequenceIndex > yy.SimultaneousSequenceIndex)
            {
                return 1;
            }

            if (xx.SimultaneousSequenceIndex < yy.SimultaneousSequenceIndex)
            {
                return -1;
            }

            if (xx.ObjectId > yy.ObjectId)
            {
                return 1;
            }

            if (yy.ObjectId > xx.ObjectId)
            {
                return -1;
            }

            return 0;
        }
    }
}