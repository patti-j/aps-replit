namespace PT.Scheduler;

internal abstract partial class ArrayListDispatcher
{
    internal class ArrayListDispatcherEnumerator : IndexableEnumerator
    {
        private readonly List<InternalActivity> m_dispatcher;

        internal ArrayListDispatcherEnumerator(List<InternalActivity> a_activities)
        {
            m_dispatcher = a_activities;
        }

        public override InternalActivity Current => m_dispatcher[m_currentIndex];

        public override bool MoveNext()
        {
            return ++m_currentIndex < m_dispatcher.Count;
        }
    }
}