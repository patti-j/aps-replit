namespace PT.Common;

/// <summary>
/// Help manage locking resources by thread. Meant to be used within a using clause. Stores a reference to the current thread and nulls it when an AutoDisposer is called.
/// Its primary purpose is to allow a client thread access to a resource a worker thread has locked. For instance
/// The anticpicated usage is with Thread lock as follows:
/// 1. a worker thread locks the referenced object (m_lockee).
/// 2. The worker thrad calls an event.
/// 3. The listener Invokes the client thread.
/// 4. The client thread (user interface thread) AutoShareLock()s the thread.
/// 5. The client thread completes processing.
/// 6. The client thread releases the AutoShareLock().
/// 7. The worker thread releases the lock on the referenced object (m_lockee).
/// </summary>
public class LockSharer
{
    /// <summary>
    /// The current thread this LockSharer is associated with.
    /// </summary>
    private Thread m_sharee;

    /// <summary>
    /// To work this function must be called within a using clause. It's purpose is to make sure only a single thread shares the lock created with ThreadLock.
    /// </summary>
    /// <returns>An object that will null the thread referenced by this class when its Dispose method is called.</returns>
    public AutoDisposer AutoShareLock()
    {
        lock (this)
        {
            if (m_sharee != null)
            {
                throw new ApplicationException("AutoShareLock cannot be called because a Thread has already been registered on it.");
            }

            m_sharee = Thread.CurrentThread;
        }

        return new AutoDisposer(new Action(UnshareLock));
    }

    /// <summary>
    /// Clear the referenced thread. Also make sure the thread clearing the lock is the thread that is sharing the lock.
    /// </summary>
    private void UnshareLock()
    {
        lock (this)
        {
            if (Thread.CurrentThread != m_sharee)
            {
                throw new ApplicationException(
                    "UnshareLock cannot be called on a different thread than the thread that has already been registered against it.");
            }

            m_sharee = null;
        }
    }

    /// <summary>
    /// Tells whether the CurrentThread is the sharing thread.
    /// </summary>
    public bool Sharing
    {
        get
        {
            lock (this)
            {
                if (m_sharee == null)
                {
                    return false;
                }

                return Thread.CurrentThread == m_sharee;
            }
        }
    }
}