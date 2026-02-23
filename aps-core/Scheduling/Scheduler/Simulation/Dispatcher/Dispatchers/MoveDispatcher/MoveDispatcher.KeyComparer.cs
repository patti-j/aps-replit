namespace PT.Scheduler;

public partial class MoveDispatcherDefinition : DispatcherDefinition
{
    private class KeyComparer : IComparer<KeyAndActivity>
    {
        #region IComparer Members
        public int Compare(KeyAndActivity a_key1, KeyAndActivity a_key2)
        {
            Key k1 = (Key)a_key1.Key;
            Key k2 = (Key)a_key2.Key;

            // The enumeration's sequence has the order of how activities are selected built into it.
            int comparison = Comparer<int>.Default.Compare((int)k1.m_keyType, (int)k2.m_keyType);
            if (comparison != 0)
            {
                return comparison;
            }

            if (k1.m_activityNbr > k2.m_activityNbr) // Higher numbers come first
            {
                return -1;
            }

            if (k1.m_activityNbr < k2.m_activityNbr)
            {
                return 1;
            }

            if (k1.SimultaneousSequenceIndex > k2.SimultaneousSequenceIndex)
            {
                return 1;
            }

            if (k1.SimultaneousSequenceIndex < k2.SimultaneousSequenceIndex)
            {
                return -1;
            }

            if (k1.OriginalStartDateTicks > k2.OriginalStartDateTicks)
            {
                return -1;
            }

            if (k1.OriginalStartDateTicks < k2.OriginalStartDateTicks)
            {
                return 1;
            }
            // Consider adding the activities operation level here as part of the sort since it will be more reliable if jobs are edited with new activites added to the front
            // of the activity. But I think it's possible that lower level activities will always have a lower BaseId because significant changes result in the Job being 
            // completely recreated. resort to sorting activities by their creation number, which is their Id, BaseId value. Lower activities come first (those that were created first).

            //TODO: this is test AttributeCodeTable or a_key1 reminder
            //delete. It adds a default to sort the activities by activity baseId if all other sorts fail. 
            //Consider replacing this with operation level. 
            //    Lower level operations should be scheduled first.
            //    The only reason this is working with the sample data is because the lower level activities were assigned smaller numbers.


            if (k1.m_activityBaseIdValue > k2.m_activityBaseIdValue)
            {
                return 1;
            }

            if (k1.m_activityBaseIdValue < k2.m_activityBaseIdValue)
            {
                return -1;
            }

            return 0;
        }
        #endregion
    }
}