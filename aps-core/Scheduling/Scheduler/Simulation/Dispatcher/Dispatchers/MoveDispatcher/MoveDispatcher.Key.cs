namespace PT.Scheduler;

public partial class MoveDispatcherDefinition : DispatcherDefinition
{
    internal class Key : IDispatchKey
    {
        /// <summary>
        /// The sequence the literals are defined in indicates their priority when selecting activities from the dispatcher.
        /// </summary>
        internal enum Type
        {
            /// <summary>
            /// The activity is in production or setting up.
            /// </summary>
            InProduction,

            /// <summary>
            /// The activity is a member of a batch that other activities are being moved into.
            /// </summary>
            MoveIntoBatch,

            /// <summary>
            /// The activity is being moved or being moved into an existing batch.
            /// </summary>
            MoveActivity,

            /// <summary>
            /// The batch isn't being moved. The sequence its scheduled in won't change.
            /// </summary>
            SequencedActivity
        }

        internal Key(Type a_keyType, long a_originalStartTicks, int a_activityNbr, long a_simultaneousSequenceIdx, long a_activityId)
        {
            m_keyType = a_keyType;
            m_activityNbr = a_activityNbr;
            m_activityBaseIdValue = a_activityId;
            SimultaneousSequenceIndex = a_simultaneousSequenceIdx;
            //Earlier dates should take priority so we want to use negative ticks
            OriginalStartDateTicks = -a_originalStartTicks;
        }

        internal Type m_keyType;
        internal int m_activityNbr;
        internal long m_activityBaseIdValue;
        internal long SimultaneousSequenceIndex;
        internal long OriginalStartDateTicks;

        public override string ToString()
        {
            string s;

            s = GetType().Name + "::" + m_keyType + "; " + m_activityNbr + "; " + SimultaneousSequenceIndex;

            return s;
        }
    }
}