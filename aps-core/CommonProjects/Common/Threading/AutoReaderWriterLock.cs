namespace PT.Common.Threading;

/// <summary>
/// A generic that wraps an object requiring thread safety. It allows access to object in either
/// Reader, UpgradableableReader or Writer mode.
/// It uses System.Threading.ReaderWriterLockSlim for underlying lock and PT.Common.AutoDisposer to
/// exit locks through IDisposable pattern.
/// ReaderWriterLockSlim documentation: https://msdn.microsoft.com/en-us/library/system.threading.readerwriterlockslim(v=vs.110).aspx
/// </summary>
/// <typeparam name="T">Type of the object (e.g. ScenarioDetail)</typeparam>
public class AutoReaderWriterLock<T> where T : class?
{
    private readonly ReaderWriterLockSlim m_lock;
    private T m_obj;
    private readonly Action m_readExiter;
    private readonly Action m_upgradeableReadExiter;
    private readonly Action m_writeExiter;
    private readonly Action m_fakeExiter;

    private SynchronizationContext m_mainThreadContext;

    public void SetMainThread()
    {
        m_mainThreadContext = SynchronizationContext.Current;
    }

    /// <summary>
    /// Change the object managed by this locker. You must have a write lock to execute this function.
    /// </summary>
    /// <param name="a_obj"></param>
    public void ChangeLockedObject(T a_obj)
    {
        if (!m_lock.IsWriteLockHeld)
        {
            // Add code to verify the caller has the write write lock.
            // This can be done as follows:
            // 1. When a write lock is obtained, store a reference to the ObjectAccess<T>
            // 2. Add a parameter to this function that accepts the ObjectAccess<T>.
            // 3. Verify the ObjectAccess matches the stored reference to the ObjectAccess<T>
            // 4. null the new reference created in 1 when the write lock is released.
            throw new Exception("A write lock must be obtained to change the locked object.");
        }

        m_obj = a_obj;
    }

    public AutoReaderWriterLock(T a_obj)
    {
        m_lock = new ReaderWriterLockSlim();
        m_readExiter = new Action(ExitRead);
        m_upgradeableReadExiter = new Action(ExitUpgradeableRead);
        m_writeExiter = new Action(ExitWrite);
        m_fakeExiter = new Action(() => { });

        m_obj = a_obj;
    }

    private bool IsAnyLockHeld()
    {
        return m_lock.IsReadLockHeld || m_lock.IsWriteLockHeld || m_lock.IsUpgradeableReadLockHeld;
    }

    #region Read
    private void ExitRead()
    {
        m_lock.ExitReadLock();
    }

    /// <summary>
    /// Attempts to obtain a Read lock, blocking the current thread until it does.
    /// A read lock requires no thread to have a write lock or be waiting to enter write mode.
    /// </summary>
    public AutoDisposer EnterRead(out T o_obj)
    {
        while (true)
        {
            try
            {
                return TryEnterRead(out o_obj, c_autoWriterTimeoutMS);
            }
            catch (AutoTryEnterException) { }
        }
    }

    /// <summary>
    /// Attempts to obtain a Read lock, blocking the current thread until it does.
    /// A read lock requires no thread to have a write lock or be waiting to enter write mode.
    /// </summary>
    public ObjectAccess<T> EnterRead()
    {
        while (true)
        {
            try
            {
                return TryEnterRead(c_autoWriterTimeoutMS);
            }
            catch (AutoTryEnterException) { }
        }
    }

    /// <summary>
    /// Attempts to obtain a Read lock for a_millisecondsTimeout, blocking the current thread until it does or times out.
    /// A read lock requires no thread to have a write lock or be waiting to enter write mode.
    /// </summary>
    public AutoDisposer TryEnterRead(out T o_obj, int a_millisecondsTimeout)
    {
        if (IsAnyLockHeld())
        {
            o_obj = m_obj;
            return new AutoDisposer(m_fakeExiter);
        }

        if (PendingWriteLock && SynchronizationContext.Current != m_mainThreadContext)
        {
            throw new AutoTryEnterException("2811: Thread is locked", true);
        }

        if (m_lock.TryEnterReadLock(a_millisecondsTimeout))
        {
            o_obj = m_obj;
            return new AutoDisposer(m_readExiter);
        }
        else
        {
            if (m_lock.WaitingWriteCount == 0 && !m_lock.IsWriteLockHeld)
            {
                //Thread.Sleep(100);
                if (m_lock.TryEnterReadLock(a_millisecondsTimeout))
                {
                    o_obj = m_obj;
                    return new AutoDisposer(m_readExiter);
                }
                else
                {
                    if (m_lock.WaitingWriteCount == 0 && !m_lock.IsWriteLockHeld)
                    {
                        //Thread.Sleep(100);
                        if (m_lock.TryEnterReadLock(a_millisecondsTimeout))
                        {
                            o_obj = m_obj;
                            return new AutoDisposer(m_readExiter);
                        }
                    }
                }
            }
        }

        throw new AutoTryEnterException("2811: Thread is locked");
    }

    /// <summary>
    /// Attempts to obtain a Read lock for a_millisecondsTimeout, blocking the current thread until it does or times out.
    /// A read lock requires no thread to have a write lock or be waiting to enter write mode.
    /// </summary>
    public AutoDisposer? TryEnterReadNoException(out T o_obj, out bool o_writePending, int a_millisecondsTimeout)
    {
        o_obj = null;
        o_writePending = false;

        if (IsAnyLockHeld())
        {
            o_obj = m_obj;
            return new AutoDisposer(m_fakeExiter);
        }

        if (PendingWriteLock && SynchronizationContext.Current != m_mainThreadContext)
        {
            o_writePending = true;
            return null;
        }

        if (m_lock.TryEnterReadLock(a_millisecondsTimeout))
        {
            o_obj = m_obj;
            return new AutoDisposer(m_readExiter);
        }
        else
        {
            if (m_lock.WaitingWriteCount == 0 && !m_lock.IsWriteLockHeld)
            {
                //Thread.Sleep(100);
                if (m_lock.TryEnterReadLock(a_millisecondsTimeout))
                {
                    o_obj = m_obj;
                    return new AutoDisposer(m_readExiter);
                }
                else
                {
                    if (m_lock.WaitingWriteCount == 0 && !m_lock.IsWriteLockHeld)
                    {
                        //Thread.Sleep(100);
                        if (m_lock.TryEnterReadLock(a_millisecondsTimeout))
                        {
                            o_obj = m_obj;
                            return new AutoDisposer(m_readExiter);
                        }
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Attempts to obtain a Read lock for a_millisecondsTimeout, blocking the current thread until it does or times out.
    /// A read lock requires no thread to have a write lock or be waiting to enter write mode.
    /// </summary>
    public ObjectAccess<T> TryEnterRead(int a_millisecondsTimeout)
    {
        if (IsAnyLockHeld())
        {
            return new ObjectAccess<T>(m_obj, m_fakeExiter);
        }

        if (PendingWriteLock && SynchronizationContext.Current != m_mainThreadContext)
        {
            throw new AutoTryEnterException("2811: Thread is locked", true);
        }

        if (m_lock.TryEnterReadLock(a_millisecondsTimeout))
        {
            return new ObjectAccess<T>(m_obj, m_readExiter);
        }
        else
        {
            if (m_lock.WaitingWriteCount == 0 && !m_lock.IsWriteLockHeld)
            {
                //Thread.Sleep(100);
                if (m_lock.TryEnterReadLock(a_millisecondsTimeout))
                {
                    return new ObjectAccess<T>(m_obj, m_readExiter);
                }
                else
                {
                    if (m_lock.WaitingWriteCount == 0 && !m_lock.IsWriteLockHeld)
                    {
                        //Thread.Sleep(100);
                        if (m_lock.TryEnterReadLock(a_millisecondsTimeout))
                        {
                            return new ObjectAccess<T>(m_obj, m_readExiter);
                        }
                    }
                }
            }
        }

        throw new AutoTryEnterException("2811: Thread is locked");
    }
    #endregion

    #region UpgradeableRead
    /** On UpgradeableRead:
     * This kind of lock is available and was implemented initially but since it
     * is not being used and doesn't look like a good fit in the current implementation
     * it is made private.
     */
    private void ExitUpgradeableRead()
    {
        m_lock.ExitUpgradeableReadLock();
    }

    /// <summary>
    /// Blocks if there is already a thread in upgradeable mode, if there are threads waiting to enter write mode, or if there is a single thread in write mode.
    /// Thread can enter Write mode (by EnterWrite or TryEnterWrite) without first exiting.
    /// </summary>
    private AutoDisposer EnterUpgradeableRead(out T o_obj)
    {
        if (IsAnyLockHeld())
        {
            o_obj = m_obj;
            return new AutoDisposer(m_fakeExiter);
        }

        m_lock.EnterUpgradeableReadLock();
        o_obj = m_obj;
        return new AutoDisposer(m_upgradeableReadExiter);
    }

    /// <summary>
    /// Blocks if there is already a thread in upgradeable mode, if there are threads waiting to enter write mode, or if there is a single thread in write mode.
    /// Thread can enter Write mode (by EnterWrite or TryEnterWrite) without first exiting.
    /// </summary>
    private ObjectAccess<T> EnterUpgradeableRead()
    {
        if (IsAnyLockHeld())
        {
            return new ObjectAccess<T>(m_obj, m_fakeExiter);
        }

        m_lock.EnterUpgradeableReadLock();
        return new ObjectAccess<T>(m_obj, m_upgradeableReadExiter);
    }

    /// <summary>
    /// Blocks if there is already a thread in upgradeable mode, if there are threads waiting to enter write mode, or if there is a single thread in write mode.
    /// Thread can enter Write mode (by EnterWrite or TryEnterWrite) without first exiting.
    /// </summary>
    private AutoDisposer TryEnterUpgradeableRead(out T o_obj, int a_millisecondsTimeout)
    {
        if (IsAnyLockHeld())
        {
            o_obj = m_obj;
            return new AutoDisposer(m_fakeExiter);
        }

        if (m_lock.TryEnterUpgradeableReadLock(a_millisecondsTimeout))
        {
            o_obj = m_obj;
            return new AutoDisposer(m_upgradeableReadExiter);
        }

        throw new AutoTryEnterException("2811: Thread is locked");
    }

    /// <summary>
    /// Blocks if there is already a thread in upgradeable mode, if there are threads waiting to enter write mode, or if there is a single thread in write mode.
    /// Thread can enter Write mode (by EnterWrite or TryEnterWrite) without first exiting.
    /// </summary>
    private ObjectAccess<T> TryEnterUpgradeableRead(int a_millisecondsTimeout)
    {
        if (IsAnyLockHeld())
        {
            return new ObjectAccess<T>(m_obj, m_fakeExiter);
        }

        if (m_lock.TryEnterUpgradeableReadLock(a_millisecondsTimeout))
        {
            return new ObjectAccess<T>(m_obj, m_upgradeableReadExiter);
        }

        throw new AutoTryEnterException("2811: Thread is locked");
    }
    #endregion

    #region Write
    private volatile bool m_writeLockHeld;

    /// <summary>
    /// whether any threads have a write lock on this object
    /// </summary>
    public bool IsWriteLockHeld => m_writeLockHeld;

    private void ExitWrite()
    {
        m_lock.ExitWriteLock();
        m_writeLockHeld = false;
    }

    private const int c_autoWriterTimeoutMS = 200;
    private const int c_autoWriteWaitMS = 1000;
    private bool m_pendingWrite;

    public bool WriteLockPending => m_pendingWrite;

    /// <summary>
    /// Attempts to get a write lock, blocking the current thread until it does.
    /// Write lock requires all other locks of any type to be released first.
    /// </summary>
    public AutoDisposer EnterWrite(out T o_obj)
    {
        if (m_lock.IsWriteLockHeld)
        {
            o_obj = m_obj;
            return new AutoDisposer(m_fakeExiter);
        }

        while (true)
        {
            if (!m_lock.IsReadLockHeld && m_lock.TryEnterWriteLock(c_autoWriterTimeoutMS))
            {
                m_pendingWrite = false;
                m_writeLockHeld = true;
                o_obj = m_obj;
                return new AutoDisposer(m_writeExiter);
            }

            m_pendingWrite = true;
            Thread.Sleep(c_autoWriteWaitMS);
        }
    }

    public bool PendingWriteLock => m_pendingWrite || m_lock.WaitingWriteCount > 0;

    /// <summary>
    /// Attempts to get a write lock, blocking the current thread until it does.
    /// Write lock requires all other locks of any type to be released first.
    /// </summary>
    public ObjectAccess<T> EnterWrite()
    {
        if (m_lock.IsWriteLockHeld)
        {
            return new ObjectAccess<T>(m_obj, m_fakeExiter);
        }

        while (true)
        {
            if (!m_lock.IsReadLockHeld && m_lock.TryEnterWriteLock(c_autoWriterTimeoutMS))
            {
                m_pendingWrite = false;
                m_writeLockHeld = true;
                return new ObjectAccess<T>(m_obj, m_writeExiter);
            }

            m_pendingWrite = true;
            Thread.Sleep(c_autoWriteWaitMS);
        }
    }

    /// <summary>
    /// Attempts to get a write lock for a_millisecondsTimeout, blocking the current thread until it obtains the lock or times out.
    /// Write lock requires all other locks of any type to be released first.
    /// </summary>
    public AutoDisposer TryEnterWrite(out T o_obj, int a_millisecondsTimeout)
    {
        if (m_lock.IsWriteLockHeld)
        {
            o_obj = m_obj;
            return new AutoDisposer(m_fakeExiter);
        }

        if (m_lock.TryEnterWriteLock(a_millisecondsTimeout))
        {
            o_obj = m_obj;
            m_writeLockHeld = true;
            return new AutoDisposer(m_writeExiter);
        }

        throw new AutoTryEnterException("2811: Thread is locked");
    }

    /// <summary>
    /// Attempts to get a write lock for a_millisecondsTimeout, blocking the current thread until it obtains the lock or times out.
    /// Write lock requires all other locks of any type be released first.
    public ObjectAccess<T> TryEnterWrite(int a_millisecondsTimeout)
    {
        if (m_lock.IsWriteLockHeld)
        {
            return new ObjectAccess<T>(m_obj, m_fakeExiter);
        }

        if (m_lock.TryEnterWriteLock(a_millisecondsTimeout))
        {
            m_writeLockHeld = true;
            return new ObjectAccess<T>(m_obj, m_writeExiter);
        }

        throw new AutoTryEnterException("2811: Thread is locked");
    }
    #endregion
}

/// <summary>
/// Wrap an Object of type T and provides access to it through the Instance Property.
/// Also stores a reference to an action that it will execute when disposed (i.e. exiting a lock)
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectAccess<T> : IDisposable
{
    private readonly T m_instance;
    private readonly Action m_exiter;

    public ObjectAccess(T a_instance, Action a_exiter)
    {
        m_instance = a_instance;
        m_exiter = a_exiter;
    }

    public T Instance => m_instance;

    public void Dispose()
    {
        m_exiter();
    }
}