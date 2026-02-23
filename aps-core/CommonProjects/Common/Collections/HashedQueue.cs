using System.Collections;

namespace PT.Common.Collections
{
    /// <summary>
    /// This class is a queue with an accompanying HashSet so that existence can be checked in constant time (assuming no collisions)
    /// </summary>
    public class HashedQueue<T> : IEnumerable
    {
        private readonly HashSet<T> m_objectHash = new ();
        private readonly Queue<T> m_queue = new ();

        /// <summary>
        /// Releases objects referenced by the queue or values and resets the count to 0.
        /// </summary>
        public void Clear()
        {
            m_objectHash.Clear();
            m_queue.Clear();
        }

        /// <summary>
        /// Add an element to the end of the queue. The element cannot be a duplicate.
        /// Returns true if the element was successfully added, and returns false otherwise. 
        /// </summary>
        /// <param name="a_data">The element to add to the end of the queue.</param>
        public bool Enqueue(T a_data)
        {
            if (!m_objectHash.Add(a_data))
            {
                return false;
            }

            m_queue.Enqueue(a_data);

            return true;
        }

        /// <summary>
        /// You must make sure the Count of this collection is greater than 0 before calling this function.
        /// Removes the object at the beginning of the queue and returns it.
        /// </summary>
        /// <returns>The object at the beginning of the queue.</returns>
        public T Dequeue()
        {
            T dataObject = m_queue.Dequeue();
            m_objectHash.Remove(dataObject);
            return dataObject;
        }

        /// <summary>
        /// You must make sure the Count of this collection is greater than 0 before calling this function.
        /// Returns the object at the beginning of the queue without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the queue.</returns>
        public T Peek()
        {
            return m_queue.Peek();
        }

        public int Count => m_objectHash.Count;

        public bool Contains(T a_item)
        {
            return m_objectHash.Contains(a_item);
        }

        /// <summary>
        /// Gets an enumerator from HashedQueue's underlying Queue so that it iterates in the order in which
        /// objects were added. 
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return m_queue.GetEnumerator();
        }
    }
}
