namespace PT.Scheduler;

/// <summary>
/// Calling Current after MoveNext() returns false will result in an exception.
/// This enumerator is only valid while the underlying collection is static. If the collection changes, you can call Reset() to make it valid again.
/// This enumerator isn't thread safe.
/// </summary>
public abstract partial class ReadyActivitiesDispatcher
{
    public abstract class Enumerator : IEnumerator<InternalActivity>, System.Collections.IEnumerator
    {
        /// <summary>
        /// Throws an exception if called after MoveNext() returns false.
        /// </summary>
        public abstract InternalActivity Current { get; }

        public virtual void Dispose() { }

        /// <summary>
        /// Throws an exception if called after MoveNext() returns false.
        /// </summary>
        object System.Collections.IEnumerator.Current => Current;

        /// <summary>
        /// If the collection is modified the enumerator will become invalid and will need to be Reset.
        /// Once MoveNext returns false, Current will become invalid and attempting to use it may result in an Excpetion.
        /// MoveNext() will also return false if the resource if the resource wasn't available for scheudling when the enumerator was created.
        /// </summary>
        /// <returns></returns>
        public abstract bool MoveNext();

        /// <summary>
        /// Use reset to start the enumeration from the beggining. You should also use this to reset the enumerator after the collection being enumerated has changed.
        /// </summary>
        public abstract void Reset();
    }

    /// <summary>
    /// This class was designed for enumerators whose collections are indexable, such as List<> and ArrayList.
    /// It extends the Enumerator class with a protected indexer member and overrides Reset() to set
    /// m_currentIndex to -1. You must call this classes constructor for this class to start at the correct index.
    /// </summary>
    public abstract class IndexableEnumerator : Enumerator
    {
        /// <summary>
        /// Increment this value or use it to access the current member.
        /// </summary>
        protected int m_currentIndex;

        /// <summary>
        /// This consturctor must be called or the protected m_currentIndex won't be initialized to it's initial value.
        /// </summary>
        internal IndexableEnumerator()
        {
            Reset();
        }

        /// <summary>
        /// Call this method to reset m_currentIndex
        /// </summary>
        public override void Reset()
        {
            m_currentIndex = -1;
        }
    }
}