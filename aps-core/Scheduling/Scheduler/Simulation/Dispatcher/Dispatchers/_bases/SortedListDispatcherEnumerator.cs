namespace PT.Scheduler;

/// <summary>
/// Enumerates the sorted list in order. An exception will occur if Current is called after MoveNext returns false.
/// This enumerator is only valid while the underlying collection is static. If the collection changes, you'll need to call reset.
/// This enumerator isn't thread safe.
/// </summary>
internal abstract partial class SortedListDispatcher : ArrayListDispatcher
{
    private class SortedListDispatcherEnumerator : IndexableEnumerator
    {
        private readonly SortedListDispatcher m_sortedListDispatcher;

        internal SortedListDispatcherEnumerator(SortedListDispatcher a_sortedListDispatcher)
        {
            m_sortedListDispatcher = a_sortedListDispatcher;
        }

        /// <summary>
        /// An exception will be thrown if Current is called after MoveNext() returns false.
        /// </summary>
        public override InternalActivity Current => m_sortedListDispatcher.m_compositeSort[m_currentIndex].Activity;

        public override bool MoveNext()
        {
            if (m_sortedListDispatcher.m_compositeSort == null)
            {
                // This happens if the resource isn't available when this is called.
                return false;
            }

            return ++m_currentIndex < m_sortedListDispatcher.m_compositeSort.Count;
        }
    }
}