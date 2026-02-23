namespace PT.Common;

/// <summary>
/// References an object and provides various methods to create thread locks on it.
/// </summary>
public class ThreadLock
{
    /// <summary>
    /// The object to make thread locks on.
    /// </summary>
    private object m_lockee;

    /// <summary>
    /// This can be used to help share a thread lock between 2 threads. For instance, if a worker thread locks an object but needs to call an event to cause the client to redraw,
    /// then then client would use the member function AutoShareLock to allow the lock client to bypass the lock held by the worker thread.
    /// *** This does mean the client needs to know the worker thread probably does have a lock on an object. Consider getting rid of this.
    /// The anticpicated usage is:
    /// 1. a worker thread locks the referenced object (m_lockee).
    /// 2. The worker thrad calls an event.
    /// 3. The listener Invokes the client thread.
    /// 4. The client thread (user interface thread) AutoShareLock()s the thread.
    /// 5. The client thread completes processing.
    /// 6. The client thread releases the AutoShareLock().
    /// 7. The worker thread releases the lock on the referenced object (m_lockee).
    /// </summary>
    private readonly LockSharer m_lockSharer = new ();

    /// <summary>
    /// The object to make thread locks on.
    /// </summary>
    /// <param name="a_lockee"></param>
    public ThreadLock(object a_lockee)
    {
        m_lockee = a_lockee;
    }

    /// <summary>
    /// Change the object thread locks are made on.
    /// </summary>
    /// <param name="a_lockee"></param>
    public void ChangeLockee(object a_lockee)
    {
        lock (this)
        {
            object o = m_lockee;
            Monitor.Enter(m_lockee);

            try
            {
                m_lockee = a_lockee;
            }
            finally
            {
                Monitor.Exit(o);
            }
        }
    }

    /// <summary>
    /// If the thread is already locked, return an AutoDisposer to track which thread is sharing the lock.
    /// </summary>
    /// <returns></returns>
    public AutoDisposer AutoShareLock()
    {
        if (Monitor.TryEnter(m_lockee))
        {
            Monitor.Exit(m_lockee);
            return null;
        }

        return m_lockSharer.AutoShareLock();
    }

    /// <summary>
    /// Puts a Monitor.Enter lock on the referenced object if the lock isn't being shared.
    /// </summary>
    /// <param name="o_lockee">A reference to the locked object. You must remember to call Exit().</param>
    public void Enter(out object o_lockee)
    {
        if (!m_lockSharer.Sharing)
        {
            Monitor.Enter(m_lockee);
        }

        o_lockee = m_lockee;
    }

    /// <summary>
    /// Performs a Monitor.TryEntry on the referenced object.
    /// </summary>
    /// <param name="o_lockee">A reference to the locked object if successful.</param>
    /// <param name="a_ms">Give up if this many milliseoncds pass without obtaining the lock.</param>
    /// <returns>Whether the lock was successful.</returns>
    public bool TryEnter(out object o_lockee, int a_ms)
    {
        if (m_lockSharer.Sharing || Monitor.TryEnter(m_lockee, a_ms))
        {
            o_lockee = m_lockee;
            return true;
        }

        o_lockee = null;
        return false;
    }

    /// <summary>
    /// Must be used within a using clause. Locks the referenced object and automatically releases the lock after the using clause.
    /// </summary>
    /// <param name="o_lockee">The referenced object.</param>
    /// <returns>An object that will automatically release the locks when its Dispose method is called.</returns>
    public AutoExiter AutoEnter(out object o_lockee)
    {
        Enter(out o_lockee);
        return new AutoExiter(o_lockee, m_lockSharer);
    }

    /// <summary>
    /// Must be used within a using clause. Locks the referenced object and automatically releases the lock after the using clause.
    /// Throws AutoTryEnterException if the lock can't be obtained within the maximum number of milliseconds.
    /// </summary>
    /// <param name="o_lockee">The referenced object.</param>
    /// <param name="a_ms">Give up if this many milliseoncds pass without obtaining the lock.</param>
    /// <returns>An object that will automatically release the locks when its Dispose method is called.</returns>
    public AutoExiter AutoTry(out object o_lockee, int a_ms)
    {
        if (TryEnter(out o_lockee, a_ms))
        {
            return new AutoExiter(o_lockee, m_lockSharer);
        }

        throw new AutoTryEnterException("2811: Thread is locked");
    }

    /// <summary>
    /// Calls MOnitor.Exit if the Sharing thread isn't active. Also nulls the reference to the object.
    /// </summary>
    /// <param name="r_lockee">Nulls this value out to help make sure you don't accidentally use an unlocked object.</param>
    public void Exit(ref object r_lockee)
    {
        if (!m_lockSharer.Sharing)
        {
            Monitor.Exit(r_lockee);
        }

        r_lockee = null;
    }
}