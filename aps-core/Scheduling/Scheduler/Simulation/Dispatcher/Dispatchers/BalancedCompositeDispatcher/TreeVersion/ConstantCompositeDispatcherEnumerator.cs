using PT.Common.Collections;

namespace PT.Scheduler.Simulation.Dispatcher;

/// <summary>
/// Use this dispatcher when the composite value never changes. Some examples of values that don't change include: JobNeedDate, Priority, and Hot.
/// Values related to setup may cause the composite value to change as blocks are scheduled on a resource.
/// Calling Current after MoveNext() returns false will result in an exception.
/// This enumerator is only valid while the underlying collection is static. If the collection changes, you'll need to call reset.
/// This enumerator isn't thread safe.
/// </summary>
internal partial class ConstantCompositeDispatcher : ReadyActivitiesDispatcher
{
    private class ConstantCompositeDispatcherEnumerator : Enumerator
    {
        private readonly AVLTree<ConstantCompositeKey, KeyAndActivity>.Enumerator m_treeEtr;

        internal ConstantCompositeDispatcherEnumerator(ConstantCompositeDispatcher a_dispatcher)
        {
            m_treeEtr = a_dispatcher.m_tree.GetEnumerator();
        }

        /// <summary>
        /// Throws an exception if called after MoveNext() returns false.
        /// </summary>
        public override InternalActivity Current => m_treeEtr.Current.Value.Activity;

        /// <summary>
        /// Once this returns false, calling Current will result in an exception.
        /// </summary>
        /// <returns></returns>
        public override bool MoveNext()
        {
            return m_treeEtr.MoveNext();
        }

        /// <summary>
        /// Use reset to start the enumeration from the begining. You should also use this to reset the enumerator after the collection being enumerated has changed.
        /// </summary>
        public override void Reset()
        {
            m_treeEtr.Reset();
        }
    }
}