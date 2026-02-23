namespace PT.Scheduler.Simulation.Dispatcher;

internal partial class ConstantCompositeDispatcher
{
    /// <summary>
    /// Used to compare objects of type StaticCompositeValue.
    /// NOTE, the AVL tree always enumerates from less than to greater than so these keys are reversed from how things are dispatched.
    /// </summary>
    private class ConstantCompositeKeyComparer : IComparer<ConstantCompositeKey>
    {
        //We use higher score to dispatch. The AVL tree always returns lower score first.
        //So these comparers are reversed so that a higher Composite is returned first from the tree.
        public int Compare(ConstantCompositeKey x, ConstantCompositeKey y)
        {
            if (x.Composite > y.Composite)
            {
                return -1;
            }

            if (x.Composite < y.Composite)
            {
                return 1;
            }

            // Smaller sequence IDs have priority
            // SimultaneousSequenceIndex is startTicks + index. So lower index means higher priority
            if (x.SimultaneousSequenceIndex > y.SimultaneousSequenceIndex)
            {
                return 1;
            }

            if (x.SimultaneousSequenceIndex < y.SimultaneousSequenceIndex)
            {
                return -1;
            }

            if (x.ObjectId > y.ObjectId)
            {
                return 1;
            }

            if (x.ObjectId < y.ObjectId)
            {
                return -1;
            }

            return 0;
        }
    }
}