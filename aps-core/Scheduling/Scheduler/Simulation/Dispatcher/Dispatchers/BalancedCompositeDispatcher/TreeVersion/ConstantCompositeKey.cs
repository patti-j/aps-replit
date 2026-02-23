namespace PT.Scheduler.Simulation.Dispatcher;

internal partial class ConstantCompositeDispatcher
{
    /// <summary>
    /// Used to represent a key value in the set that contains the activities of this dispatcher.
    /// </summary>
    private readonly struct ConstantCompositeKey
    {
        private readonly decimal m_composite;

        internal decimal Composite
        {
            get
            {
                #if DEBUG
                ValidateStructureInitialized();
                #endif
                return m_composite;
            }
        }

        private readonly long m_id;
        private readonly long m_simultaneousSequenceIdx;

        internal long ObjectId => m_id;

        public long SimultaneousSequenceIndex => m_simultaneousSequenceIdx;

        internal ConstantCompositeKey(decimal a_composite, long a_id, long a_simultaneousSequenceIdx)
        {
            m_composite = a_composite;
            m_id = a_id;
            m_simultaneousSequenceIdx = a_simultaneousSequenceIdx;
            #if DEBUG
            m_set = true;
            #endif
        }

        public override string ToString()
        {
            #if DEBUG
            ValidateStructureInitialized();
            #endif
            return "Composite=" + m_composite + "; SequenceNbr=" + m_id;
        }

        #if DEBUG
        private readonly bool m_set;
        private void ValidateStructureInitialized()
        {
            if (!m_set)
            {
                throw new Exception("The Composite and SequenceNbr haven't been set.");
            }
        }
        #endif
    }
}