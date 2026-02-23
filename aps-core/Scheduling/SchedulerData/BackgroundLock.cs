using System.Windows.Forms;

using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Scheduler;
using PT.UIDefinitions.Interfaces;

namespace PT.SchedulerData;

public class BackgroundLock : IDisposable
{
    protected readonly BaseId m_scenarioId;
    protected bool m_canceled;
    private static IExceptionManager s_exceptionManager;

    private const int c_threadWaitInitialMs = 100;
    private const int c_threadWaitMaxMs = 2250;
    private int m_keyWaitMs = c_threadWaitInitialMs;
    private System.Timers.Timer m_autoCancelTimer;

    private bool ContinueLocking
    {
        get => Status != EResultStatus.Canceled && Status != EResultStatus.Error && Status != EResultStatus.Finished;
    }

    public enum EResultStatus
    {
        NotStarted,
        Locking,
        Processing,
        Error,
        Canceled,
        Finished,
        Skipped,
    }

    public EResultStatus Status;
    private Exception m_backgroundLockException;

    /// <summary>
    /// Handles background processing of the PT locks. The calling thread should call the lock functions on a new task so the thread will not be blocked while waiting for lock
    /// </summary>
    /// <param name="a_scenarioId">Scenario Id in which to lock</param>
    public BackgroundLock(BaseId a_scenarioId)
    {
        m_scenarioId = a_scenarioId;
    }

    /// <summary>
    /// Sets member variables for a new lock attempt
    /// </summary>
    private void InitializeLock()
    {
        m_keyWaitMs = c_threadWaitInitialMs;
    }

    #region Lock Queue Functions and Members
    private string m_key = "";
    private int m_order;
    private static readonly object m_keyLock = new ();
    private static readonly Dictionary<string, List<int>> s_lockOrderList = new ();
    private static readonly HashSet<string> s_exclusiveSet = new ();
    private bool m_skipDueToExclusivity;
    private string m_exclusiveKey;

    //If this lock would like to maintain an ordering amongst other locks with the same key
    public void AttachLockToQueue(string a_key)
    {
        lock (s_lockOrderList)
        {
            m_key = a_key;
            m_order = GetMaxOrderingForKey(a_key) + 1;
            AddLockOrdering(a_key, m_order);
        }
    }

    /// <summary>
    /// If another lock with this key is waiting to process, do not queue another one.
    /// </summary>
    /// <param name="a_key"></param>
    public void KeepExclusive(string a_key)
    {
        lock (m_keyLock)
        {
            if (!s_exclusiveSet.Add(a_key))
            {
                m_skipDueToExclusivity = true;
            }
            else
            {
                m_exclusiveKey = a_key;
            }
        }
    }

    private void RemoveExclusiveKey()
    {
        if (string.IsNullOrEmpty(m_exclusiveKey))
        {
            return;
        }

        lock (m_keyLock)
        {
            s_exclusiveSet.Remove(m_exclusiveKey);
        }
    }

    //Return the newest key
    private int GetMaxOrderingForKey(string a_key)
    {
        lock (m_keyLock)
        {
            if (s_lockOrderList.TryGetValue(a_key, out List<int> list))
            {
                return list.Max();
            }

            //Key is not in the list yet
            return -1;
        }
    }

    private static void AddLockOrdering(string a_key, int a_order)
    {
        if (string.IsNullOrEmpty(a_key))
        {
            DebugException.ThrowInDebug("You should not be here!");
        }

        lock (m_keyLock)
        {
            if (s_lockOrderList.TryGetValue(a_key, out List<int> list))
            {
                list.Add(a_order);
            }
            else
            {
                s_lockOrderList.Add(a_key, new List<int> { a_order });
            }
        }
    }

    private void CleanupHandling()
    {
        PopLockOrdering();
        RemoveExclusiveKey();
    }

    private void PopLockOrdering()
    {
        if (string.IsNullOrEmpty(m_key))
        {
            return;
        }

        lock (m_keyLock)
        {
            if (s_lockOrderList.TryGetValue(m_key, out List<int> list))
            {
                list.Remove(m_order);
                if (list.Count == 0)
                {
                    s_lockOrderList.Remove(m_key);
                }
            }
        }
    }

    private void WaitForQueue()
    {
        if (string.IsNullOrEmpty(m_key))
        {
            return;
        }

        while (true)
        {
            int minQueueValue = -1;
            lock (m_keyLock)
            {
                if (s_lockOrderList.TryGetValue(m_key, out List<int> orderList))
                {
                    minQueueValue = orderList.Min();
                }
            }

            if (minQueueValue == m_order)
            {
                return;
            }

            Thread.Sleep(200);
        }
    }
    #endregion

    public void AutoCancel(TimeSpan a_timeSpan)
    {
        m_autoCancelTimer = new System.Timers.Timer(a_timeSpan.TotalMilliseconds);

        m_autoCancelTimer.Elapsed += (a_sender, a_args) =>
        {
            m_canceled = true;
            m_autoCancelTimer.Dispose();
        };

        m_autoCancelTimer.Start();
    }

    /// <summary>
    /// Get the current druation to wait for a lock
    /// This value steadily increases to reduce overhead of exceptions and loop code for long waits
    /// </summary>
    /// <returns></returns>
    private int GetKeyWaitDuration()
    {
        m_keyWaitMs = Math.Max(m_keyWaitMs + 30, c_threadWaitMaxMs);
        return m_keyWaitMs;
    }

    /// <summary>
    /// If write pending, to reduce the number of exceptions,
    /// wait double the current wait value. Don't exceed the max wait
    /// </summary>
    private void WaitForWriteLockRelease()
    {
        Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(m_keyWaitMs * 2, c_threadWaitMaxMs)));
    }

    private bool CheckForExclusivitySkip()
    {
        if (m_skipDueToExclusivity)
        {
            Status = EResultStatus.Skipped;
            return true;
        }

        return false;
    }

    public static void SetExceptionManager(IExceptionManager a_exceptionManager)
    {
        s_exceptionManager = a_exceptionManager;
    }

    protected virtual void InvokeSplashScreen() { }

    protected virtual void InvokeDisableCancel() { }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    public virtual void Dispose()
    {
        m_autoCancelTimer?.Dispose();
        if (Status != EResultStatus.Error 
            && Status != EResultStatus.Canceled 
            && Status != EResultStatus.Skipped 
            && Status != EResultStatus.Finished)
        {
            DebugException.ThrowInDebug("Should not be disposing while the locking thread has not finished.");
        }
    }

    private void LogExceptions(Exception a_e)
    {
        m_backgroundLockException = new BackgroundLockException("4446", a_e);
        Status = EResultStatus.Error;

        s_exceptionManager.LogSimpleException(a_e);
    }

    /// <summary>
    /// Get Scenario by Scenario Id
    /// </summary>
    /// <param name="a_sm"></param>
    /// <returns></returns>
    private Scenario FindScenario(ScenarioManager a_sm)
    {
        Scenario s = a_sm.Find(m_scenarioId);
        if (s == null)
        {
            throw new ObjectDisposedException("Scenario");
        }

        return s;
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCode(Control a_actionObject, Action<ScenarioManager, Scenario, ScenarioDetail, object[]> a_action, params object[] a_functionArguments)
    {
        return Task.Factory.StartNew(() => { LockCode(a_actionObject, a_action, a_functionArguments); }, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Locks the appropriate objects and runs an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCodeBackground(Action<ScenarioManager, Scenario, ScenarioDetail, object[]> a_action, params object[] a_functionArguments)
    {
        return RunLockCode(null, a_action, a_functionArguments);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void LockCode(Control a_actionObject, Action<ScenarioManager, Scenario, ScenarioDetail, object[]> a_action, params object[] a_functionArguments)
    {
        if (CheckForExclusivitySkip())
        {
            return;
        }
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        while (true)
        {
            try
            {
                Scenario s;
                if (SystemController.Sys.ScenariosLock.IsWriteLockHeld)
                {
                    InvokeSplashScreen();
                }

                using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, GetKeyWaitDuration()))
                {
                    s = sm.Find(m_scenarioId);
                    if (s.ScenarioDetailLock.IsWriteLockHeld)
                    {
                        InvokeSplashScreen();
                    }

                    using (s.ScenarioDetailLock.TryEnterRead(out ScenarioDetail sd, GetKeyWaitDuration()))
                    {
                        Status = EResultStatus.Processing;
                        InvokeDisableCancel();

                        if (a_actionObject != null)
                        {
                            if (!a_actionObject.IsDisposed)
                            {
                                a_actionObject.Invoke(new Action(() => a_action(sm, s, sd, a_functionArguments)));
                            }
                        }
                        else
                        {
                            a_action(sm, s, sd, a_functionArguments);
                        }
                    }
                }

                Status = EResultStatus.Finished;
                return;
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (e.WritePending)
                {
                    WaitForWriteLockRelease();
                }
            }
            catch (ObjectDisposedException)
            {
                //This scenario is being disposed. There is nothing else to do here.
                Status = EResultStatus.Canceled;
                return;
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return;
            }
            finally
            {
                CleanupHandling();
            }
        }
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCode(Control a_actionObject, Action<Scenario, ScenarioDetail, object[]> a_action, params object[] a_functionArguments)
    {
        return Task.Factory.StartNew(() => { LockCode(a_actionObject, a_action, a_functionArguments); }, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Locks the appropriate objects and runs an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCodeBackground(Action<Scenario, ScenarioDetail, object[]> a_action, params object[] a_functionArguments)
    {
        return RunLockCode(null, a_action, a_functionArguments);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void LockCode(Control a_actionObject, Action<Scenario, ScenarioDetail, object[]> a_action, params object[] a_functionArguments)
    {
        if (CheckForExclusivitySkip())
        {
            return;
        }
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        bool writeLockInProgress = false;

        AutoDisposer sLockDisposer = null;
        AutoDisposer sdLockDisposer = null;
        try
        {
            while (ContinueLocking)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (writeLockInProgress)
                {
                    WaitForWriteLockRelease();
                }

                if (SystemController.Sys.ScenariosLock.IsWriteLockHeld)
                {
                    InvokeSplashScreen();
                }

                sLockDisposer = SystemController.Sys.ScenariosLock.TryEnterReadNoException(out ScenarioManager sm, out writeLockInProgress, GetKeyWaitDuration());
                if (sLockDisposer == null)
                {
                    continue;
                }

                Scenario s = FindScenario(sm);
                if (s.ScenarioDetailLock.IsWriteLockHeld)
                {
                    InvokeSplashScreen();
                }

                sdLockDisposer = s.ScenarioDetailLock.TryEnterReadNoException(out ScenarioDetail sd, out writeLockInProgress, GetKeyWaitDuration());
                if (sdLockDisposer == null)
                {
                    sLockDisposer?.Dispose();
                    continue;
                }

                Status = EResultStatus.Processing;
                InvokeDisableCancel();

                if (a_actionObject != null)
                {
                    if (a_actionObject.IsHandleCreated)
                    {
                        if (!a_actionObject.IsDisposed)
                        {
                            a_actionObject.Invoke(new Action(() => { a_action(s, sd, a_functionArguments); }));
                        }
                    }
                    else
                    {
                        DebugException.ThrowInTest($"{a_actionObject} control's Handle has not been created an is attempting to run async");
                    }
                }
                else
                {
                    a_action(s, sd, a_functionArguments);
                }

                Status = EResultStatus.Finished;
            }
        }
        catch (ObjectDisposedException)
        {
            //This scenario is being disposed. There is nothing else to do here.
            Status = EResultStatus.Canceled;
        }
        catch (Exception error)
        {
            LogExceptions(error);
        }
        finally
        {
            sLockDisposer?.Dispose();
            sdLockDisposer?.Dispose();
            CleanupHandling();
        }
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCode(Control a_actionObject, Action<Scenario, ScenarioDetail, ScenarioEvents, object[]> a_action, params object[] a_functionArguments)
    {
        return Task.Factory.StartNew(() => { LockCode(a_actionObject, a_action, a_functionArguments); }, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Locks the appropriate objects and runs an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCodeBackground(Action<Scenario, ScenarioDetail, ScenarioEvents, object[]> a_action, params object[] a_functionArguments)
    {
        return RunLockCode(null, a_action, a_functionArguments);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void LockCode(Control a_actionObject, Action<Scenario, ScenarioDetail, ScenarioEvents, object[]> a_action, params object[] a_functionArguments)
    {
        if (CheckForExclusivitySkip())
        {
            return;
        }
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        while (true)
        {
            try
            {
                Scenario s;
                if (SystemController.Sys.ScenariosLock.IsWriteLockHeld)
                {
                    InvokeSplashScreen();
                }

                using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, GetKeyWaitDuration()))
                {
                    s = FindScenario(sm);
                    if (s.ScenarioDetailLock.IsWriteLockHeld)
                    {
                        InvokeSplashScreen();
                    }

                    using (s.ScenarioDetailLock.TryEnterRead(out ScenarioDetail sd, GetKeyWaitDuration()))
                    {
                        if (s.ScenarioEventsLock.IsWriteLockHeld)
                        {
                            InvokeSplashScreen();
                        }

                        using (s.ScenarioEventsLock.TryEnterRead(out ScenarioEvents se, GetKeyWaitDuration()))
                        {
                            Status = EResultStatus.Processing;
                            InvokeDisableCancel();

                            if (a_actionObject != null)
                            {
                                if (!a_actionObject.IsDisposed)
                                {
                                    a_actionObject.Invoke(new Action(() => a_action(s, sd, se, a_functionArguments)));
                                }
                            }
                            else
                            {
                                a_action(s, sd, se, a_functionArguments);
                            }
                        }
                    }
                }

                Status = EResultStatus.Finished;
                return;
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (e.WritePending)
                {
                    WaitForWriteLockRelease();
                }
            }
            catch (ObjectDisposedException)
            {
                //This scenario is being disposed. There is nothing else to do here.
                Status = EResultStatus.Error;
                return;
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return;
            }
            finally
            {
                CleanupHandling();
            }
        }
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCode(Control a_actionObject, Action<Scenario, ScenarioDetail, UserManager, object[]> a_action, params object[] a_functionArguments)
    {
        return Task.Factory.StartNew(() => { LockCode(a_actionObject, a_action, a_functionArguments); }, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCodeBackground(Action<Scenario, ScenarioDetail, UserManager, object[]> a_action, params object[] a_functionArguments)
    {
        return RunLockCode(null, a_action, a_functionArguments);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void LockCode(Control a_actionObject, Action<Scenario, ScenarioDetail, UserManager, object[]> a_action, params object[] a_functionArguments)
    {
        if (CheckForExclusivitySkip())
        {
            return;
        }
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        while (true)
        {
            try
            {
                Scenario s;
                if (SystemController.Sys.ScenariosLock.IsWriteLockHeld)
                {
                    InvokeSplashScreen();
                }

                using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, GetKeyWaitDuration()))
                {
                    s = FindScenario(sm);
                    if (s.ScenarioDetailLock.IsWriteLockHeld)
                    {
                        InvokeSplashScreen();
                    }

                    using (s.ScenarioDetailLock.TryEnterRead(out ScenarioDetail sd, GetKeyWaitDuration()))
                    {
                        if (SystemController.Sys.UsersLock.IsWriteLockHeld)
                        {
                            InvokeSplashScreen();
                        }

                        using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, GetKeyWaitDuration()))
                        {
                            Status = EResultStatus.Processing;
                            InvokeDisableCancel();

                            if (a_actionObject != null)
                            {
                                if (!a_actionObject.IsDisposed)
                                {
                                    a_actionObject.Invoke(new Action(() => a_action(s, sd, um, a_functionArguments)));
                                }
                            }
                            else
                            {
                                a_action(s, sd, um, a_functionArguments);
                            }
                        }
                    }
                }

                Status = EResultStatus.Finished;
                return;
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (e.WritePending)
                {
                    WaitForWriteLockRelease();
                }
            }
            catch (ObjectDisposedException)
            {
                //This scenario is being disposed. There is nothing else to do here.
                Status = EResultStatus.Error;
                return;
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return;
            }
            finally
            {
                CleanupHandling();
            }
        }
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCode(Control a_actionObject, Action<Scenario, Scenario.UndoSets> a_action)
    {
        return Task.Factory.StartNew(() => { LockCode(a_actionObject, a_action); }, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCodeBackground(Action<Scenario, Scenario.UndoSets> a_action)
    {
        return RunLockCode(null, a_action);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void LockCode(Control a_actionObject, Action<Scenario, Scenario.UndoSets> a_action)
    {
        if (CheckForExclusivitySkip())
        {
            return;
        }
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        while (true)
        {
            try
            {
                Scenario s;
                if (SystemController.Sys.ScenariosLock.IsWriteLockHeld)
                {
                    InvokeSplashScreen();
                }

                using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, GetKeyWaitDuration()))
                {
                    s = FindScenario(sm);
                    if (s.UndoSetsLock.IsWriteLockHeld)
                    {
                        InvokeSplashScreen();
                    }

                    using (s.UndoSetsLock.TryEnterRead(out Scenario.UndoSets undoSets, GetKeyWaitDuration()))
                    {
                        Status = EResultStatus.Processing;
                        InvokeDisableCancel();

                        if (a_actionObject != null)
                        {
                            if (!a_actionObject.IsDisposed)
                            {
                                a_actionObject.Invoke(new Action(() => a_action(s, undoSets)));
                            }
                        }
                        else
                        {
                            a_action(s, undoSets);
                        }
                    }
                }

                Status = EResultStatus.Finished;
                return;
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (e.WritePending)
                {
                    WaitForWriteLockRelease();
                }
            }
            catch (ObjectDisposedException)
            {
                //This scenario is being disposed. There is nothing else to do here.
                Status = EResultStatus.Error;
                return;
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return;
            }
            finally
            {
                CleanupHandling();
            }
        }
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCode(Control a_actionObject, Action<Scenario, ScenarioEvents, ScenarioUndoEvents> a_action)
    {
        return Task.Factory.StartNew(() => { LockCode(a_actionObject, a_action); }, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCodeBackground(Action<Scenario, ScenarioEvents, ScenarioUndoEvents> a_action)
    {
        return RunLockCode(null, a_action);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void LockCode(Control a_actionObject, Action<Scenario, ScenarioEvents, ScenarioUndoEvents> a_action)
    {
        if (CheckForExclusivitySkip())
        {
            return;
        }
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        while (true)
        {
            try
            {
                Scenario s;
                if (SystemController.Sys.ScenariosLock.IsWriteLockHeld)
                {
                    InvokeSplashScreen();
                }

                using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, GetKeyWaitDuration()))
                {
                    s = FindScenario(sm);
                    using (s.ScenarioEventsLock.TryEnterWrite(out ScenarioEvents se, GetKeyWaitDuration()))
                    {
                        using (s.ScenarioUndoEventsLock.TryEnterWrite(out ScenarioUndoEvents sue, GetKeyWaitDuration()))
                        {
                            Status = EResultStatus.Processing;

                            if (a_actionObject != null)
                            {
                                if (!a_actionObject.IsDisposed)
                                {
                                    a_actionObject.Invoke(new Action(() => a_action(s, se, sue)));
                                }
                            }
                            else
                            {
                                a_action(s, se, sue);
                            }
                        }
                    }
                }

                Status = EResultStatus.Finished;
                return;
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (e.WritePending)
                {
                    WaitForWriteLockRelease();
                }
            }
            catch (ObjectDisposedException)
            {
                //This scenario is being disposed. There is nothing else to do here.
                Status = EResultStatus.Error;
                return;
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return;
            }
            finally
            {
                CleanupHandling();
            }
        }
    }

    //NOTE: Read lock on ScenarioEvents is not supported. It needs to use write lock
    ///// <summary>
    ///// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    ///// </summary>
    ///// <param name="a_actionObject">Control on which to invoke the action</param>
    ///// <param name="a_action">The function which requires the specified locked objects</param>
    //public void RunLockCode(Control a_actionObject, Action<Scenario, ScenarioEvents> a_action)
    //{
    //    LockCode(a_actionObject, a_action);
    //}

    ///// <summary>
    ///// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    ///// </summary>
    ///// <param name="a_action">The function which requires the specified locked objects</param>
    //public void RunLockCodeBackground(Action<Scenario, ScenarioEvents> a_action)
    //{
    //    LockCode(null, a_action);
    //}

    ///// <summary>
    ///// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    ///// </summary>
    ///// <param name="a_actionObject">Control on which to invoke the action</param>
    ///// <param name="a_action">The function which requires the specified locked objects</param>
    //private void LockCode(Control a_actionObject, Action<Scenario, ScenarioEvents> a_action)
    //{
    //    InitializeLock();
    //    Status = EResultStatus.Locking;
    //    while (true)
    //    {
    //        try
    //        {
    //            Scenario s;
    //            ScenarioEvents se;
    //            ScenarioManager sm;
    //            if (SystemController.Sys.ScenariosLock.IsWriteLockHeld)
    //            {
    //                InvokeSplashScreen();
    //            }
    //            using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, GetKeyWaitDuration()))
    //            {
    //                s = FindScenario(sm);
    //                if (s.ScenarioEventsLock.IsWriteLockHeld)
    //                {
    //                    InvokeSplashScreen();
    //                }
    //                using (s.ScenarioEventsLock.TryEnterRead(out se, GetKeyWaitDuration()))
    //                {
    //                    Status = EResultStatus.Processing;
    //                    InvokeDisableCancel();
    //                    a_actionObject.Invoke(new Action(() => a_action(s, se)));
    //                }
    //            }
    //            Status = EResultStatus.Finished;
    //            return;
    //        }
    //        catch (AutoTryEnterException e)
    //        {
    //            if (m_canceled)
    //            {
    //                Status = EResultStatus.Canceled;
    //                return;
    //            }
    //            if (e.WritePending)
    //            {
    //                WaitForWriteLockRelease();
    //            }
    //        }
    //    }
    //}

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCode(Control a_actionObject, Action<UserManager, object[]> a_action, params object[] a_functionArguments)
    {
        return Task.Factory.StartNew(() => { LockCode(a_actionObject, a_action, a_functionArguments); }, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCodeBackground(Action<UserManager, object[]> a_action, params object[] a_functionArguments)
    {
        return RunLockCode(null, a_action, a_functionArguments);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void LockCode(Control a_actionObject, Action<UserManager, object[]> a_action, params object[] a_functionArguments)
    {
        if (CheckForExclusivitySkip())
        {
            return;
        }
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        while (true)
        {
            try
            {
                if (SystemController.Sys.UsersLock.IsWriteLockHeld)
                {
                    InvokeSplashScreen();
                }

                using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, GetKeyWaitDuration()))
                {
                    Status = EResultStatus.Processing;
                    InvokeDisableCancel();

                    if (a_actionObject != null)
                    {
                        if (!a_actionObject.IsDisposed)
                        {
                            a_actionObject.Invoke(new Action(() => a_action(um, a_functionArguments)));
                        }
                    }
                    else
                    {
                        a_action(um, a_functionArguments);
                    }
                }

                Status = EResultStatus.Finished;
                return;
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (e.WritePending)
                {
                    WaitForWriteLockRelease();
                }
            }
            catch (ObjectDisposedException)
            {
                //This scenario is being disposed. There is nothing else to do here.
                Status = EResultStatus.Error;
                return;
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return;
            }
            finally
            {
                CleanupHandling();
            }
        }
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCode(Control a_actionObject, Action<Scenario> a_action)
    {
        return Task.Factory.StartNew(() => { LockCode(a_actionObject, a_action); }, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCodeBackground(Action<Scenario> a_action)
    {
        return RunLockCode(null, a_action);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void LockCode(Control a_actionObject, Action<Scenario> a_action)
    {
        if (CheckForExclusivitySkip())
        {
            return;
        }
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        while (true)
        {
            try
            {
                Scenario s;
                if (SystemController.Sys.ScenariosLock.IsWriteLockHeld)
                {
                    InvokeSplashScreen();
                }

                using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, GetKeyWaitDuration()))
                {
                    s = FindScenario(sm);
                    Status = EResultStatus.Processing;
                    InvokeDisableCancel();

                    if (a_actionObject != null)
                    {
                        if (!a_actionObject.IsDisposed)
                        {
                            a_actionObject.Invoke(new Action(() => a_action(s)));
                        }
                    }
                    else
                    {
                        a_action(s);
                    }
                }

                Status = EResultStatus.Finished;
                return;
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (e.WritePending)
                {
                    WaitForWriteLockRelease();
                }
            }
            catch (ObjectDisposedException)
            {
                //This scenario is being disposed. There is nothing else to do here.
                Status = EResultStatus.Error;
                return;
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return;
            }
            finally
            {
                CleanupHandling();
            }
        }
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunWriteLockCode(Control a_actionObject, Action<Scenario> a_action)
    {
        return Task.Run(() => WriteLockCode(a_actionObject, a_action));
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunWriteLockCodeBackground(Action<Scenario> a_action)
    {
        return RunWriteLockCode(null, a_action);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void WriteLockCode(Control a_actionObject, Action<Scenario> a_action)
    {
        if (CheckForExclusivitySkip())
        {
            return;
        }
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        while (true)
        {
            try
            {
                Scenario s;
                using (SystemController.Sys.ScenariosLock.TryEnterWrite(out ScenarioManager sm, GetKeyWaitDuration()))
                {
                    s = FindScenario(sm);

                    Status = EResultStatus.Processing;

                    if (a_actionObject != null)
                    {
                        if (!a_actionObject.IsDisposed)
                        {
                            a_actionObject.Invoke(new Action(() => a_action(s)));
                        }
                    }
                    else
                    {
                        a_action(s);
                    }
                }

                Status = EResultStatus.Finished;
                return;
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (e.WritePending)
                {
                    WaitForWriteLockRelease();
                }
            }
            catch (ObjectDisposedException)
            {
                //This scenario is being disposed. There is nothing else to do here.
                Status = EResultStatus.Error;
                return;
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return;
            }
            finally
            {
                CleanupHandling();
            }
        }
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunWriteLockCode(Control a_actionObject, Action<Scenario, ScenarioEvents> a_action)
    {
        return Task.Run(() => WriteLockCode(a_actionObject, a_action));
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunWriteLockCodeBackground(Action<Scenario, ScenarioEvents> a_action)
    {
        return RunWriteLockCode(null, a_action);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void WriteLockCode(Control a_actionObject, Action<Scenario, ScenarioEvents> a_action)
    {
        if (CheckForExclusivitySkip())
        {
            return;
        }
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        while (true)
        {
            try
            {
                Scenario s;
                using (SystemController.Sys.ScenariosLock.TryEnterWrite(out ScenarioManager sm, GetKeyWaitDuration()))
                {
                    s = FindScenario(sm);
                    using (s.ScenarioEventsLock.TryEnterWrite(out ScenarioEvents se, GetKeyWaitDuration()))
                    {
                        Status = EResultStatus.Processing;
                        if (a_actionObject != null)
                        {
                            if (!a_actionObject.IsDisposed)
                            {
                                a_actionObject.Invoke(new Action(() => a_action(s, se)));
                            }
                        }
                        else
                        {
                            a_action(s, se);
                        }
                    }
                }

                Status = EResultStatus.Finished;
                return;
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (e.WritePending)
                {
                    WaitForWriteLockRelease();
                }
            }
            catch (ObjectDisposedException)
            {
                //This scenario is being disposed. There is nothing else to do here.
                Status = EResultStatus.Error;
                return;
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return;
            }
            finally
            {
                CleanupHandling();
            }
        }
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public void RunWriteLockCode(Control a_actionObject, Action<Scenario, ScenarioEvents, ScenarioUndoEvents> a_action)
    {
        WriteLockCode(a_actionObject, a_action);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public void RunWriteLockCodeBackground(Action<Scenario, ScenarioEvents, ScenarioUndoEvents> a_action)
    {
        WriteLockCode(null, a_action);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void WriteLockCode(Control a_actionObject, Action<Scenario, ScenarioEvents, ScenarioUndoEvents> a_action)
    {
        if (CheckForExclusivitySkip())
        {
            return;
        }
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        while (true)
        {
            try
            {
                using (SystemController.Sys.ScenariosLock.TryEnterWrite(out ScenarioManager sm, GetKeyWaitDuration()))
                {
                    Scenario s = FindScenario(sm);
                    using (s.ScenarioEventsLock.TryEnterWrite(out ScenarioEvents se, GetKeyWaitDuration()))
                    {
                        using (s.ScenarioUndoEventsLock.TryEnterWrite(out ScenarioUndoEvents sue, GetKeyWaitDuration()))
                        {
                            Status = EResultStatus.Processing;
                            if (a_actionObject != null)
                            {
                                if (!a_actionObject.IsDisposed)
                                {
                                    a_actionObject.Invoke(new Action(() => a_action(s, se, sue)));
                                }
                            }
                            else
                            {
                                a_action(s, se, sue);
                            }
                        }
                    }
                }

                Status = EResultStatus.Finished;
                return;
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (e.WritePending)
                {
                    WaitForWriteLockRelease();
                }
            }
            catch (ObjectDisposedException)
            {
                //This scenario is being disposed. There is nothing else to do here.
                Status = EResultStatus.Error;
                return;
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return;
            }
            finally
            {
                CleanupHandling();
            }
        }
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCode(Control a_actionObject, Action<ScenarioManager> a_action)
    {
        return Task.Factory.StartNew(() => { LockCode(a_actionObject, a_action); }, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCodeBackground(Action<ScenarioManager> a_action)
    {
        return RunLockCode(null, a_action);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void LockCode(Control a_actionObject, Action<ScenarioManager> a_action)
    {
        if (CheckForExclusivitySkip())
        {
            return;
        }
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        while (true)
        {
            try
            {
                if (SystemController.Sys.ScenariosLock.IsWriteLockHeld)
                {
                    InvokeSplashScreen();
                }

                using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, GetKeyWaitDuration()))
                {
                    Status = EResultStatus.Processing;
                    InvokeDisableCancel();

                    if (a_actionObject != null)
                    {
                        if (!a_actionObject.IsDisposed)
                        {
                            a_actionObject.Invoke(new Action(() => a_action(sm)));
                        }
                    }
                    else
                    {
                        a_action(sm);
                    }
                }

                Status = EResultStatus.Finished;
                return;
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (e.WritePending)
                {
                    WaitForWriteLockRelease();
                }
            }
            catch (ObjectDisposedException)
            {
                //This scenario is being disposed. There is nothing else to do here.
                Status = EResultStatus.Error;
                return;
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return;
            }
            finally
            {
                CleanupHandling();
            }
        }
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunWriteLockCodeBackground(Action<UserManagerEvents> a_action)
    {
        return Task.Run(() => WriteLockCode(a_action));
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void WriteLockCode(Action<UserManagerEvents> a_action)
    {
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        while (true)
        {
            try
            {
                using (SystemController.Sys.UserManagerEventsLock.TryEnterWrite(out UserManagerEvents ume, GetKeyWaitDuration()))
                {
                    Status = EResultStatus.Processing;
                    try
                    {
                        a_action(ume);
                    }
                    catch (Exception err)
                    {
                        throw new BackgroundLockException("4446", err);
                    }
                }

                Status = EResultStatus.Finished;
                return;
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (e.WritePending)
                {
                    WaitForWriteLockRelease();
                }
            }
            catch (ObjectDisposedException)
            {
                //This scenario is being disposed. There is nothing else to do here.
                Status = EResultStatus.Error;
                return;
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return;
            }
            finally
            {
                CleanupHandling();
            }
        }
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCode(Control a_actionObject, Action<ScenarioManager, UserManager, object[]> a_action, params object[] a_functionArguments)
    {
        return Task.Factory.StartNew(() => { LockCode(a_actionObject, a_action, a_functionArguments); }, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCode(Control a_actionObject, Action<UserManagerEvents, object[]> a_action, params object[] a_functionArguments)
    {
        return Task.Factory.StartNew(() => { LockCode(a_actionObject, a_action, a_functionArguments); }, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCodeBackground(Action<ScenarioManager, UserManager, object[]> a_action, params object[] a_functionArguments)
    {
        return RunLockCode(null, a_action, a_functionArguments);
    }

    public Task RunLockCodeBackground(Action<UserManagerEvents, object[]> a_action, params object[] a_functionArguments)
    {
        return RunLockCode(null, a_action, a_functionArguments);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void LockCode(Control a_actionObject, Action<ScenarioManager, UserManager, object[]> a_action, params object[] a_functionArguments)
    {
        if (CheckForExclusivitySkip())
        {
            return;
        }
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        while (true)
        {
            try
            {
                if (SystemController.Sys.ScenariosLock.IsWriteLockHeld)
                {
                    InvokeSplashScreen();
                }

                using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, GetKeyWaitDuration()))
                {
                    Status = EResultStatus.Processing;
                    InvokeDisableCancel();
                    if (SystemController.Sys.UsersLock.IsWriteLockHeld)
                    {
                        InvokeSplashScreen();
                    }

                    using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, GetKeyWaitDuration()))
                    {
                        if (a_actionObject != null)
                        {
                            if (!a_actionObject.IsDisposed)
                            {
                                a_actionObject.Invoke(new Action(() => a_action(sm, um, a_functionArguments)));
                            }
                        }
                        else
                        {
                            a_action(sm, um, a_functionArguments);
                        }
                    }
                }

                Status = EResultStatus.Finished;
                return;
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (e.WritePending)
                {
                    WaitForWriteLockRelease();
                }
            }
            catch (ObjectDisposedException)
            {
                //This scenario is being disposed. There is nothing else to do here.
                Status = EResultStatus.Error;
                return;
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return;
            }
            finally
            {
                CleanupHandling();
            }
        }
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void LockCode(Control a_actionObject, Action<UserManagerEvents, object[]> a_action, object[] a_functionArguments)
    {
        if (CheckForExclusivitySkip())
        {
            return;
        }
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        while (true)
        {
            try
            {
                if (SystemController.Sys.ScenariosLock.IsWriteLockHeld)
                {
                    InvokeSplashScreen();
                }

                using (SystemController.Sys.UserManagerEventsLock.TryEnterRead(out UserManagerEvents ume, GetKeyWaitDuration()))
                {
                    if (a_actionObject != null)
                    {
                        if (!a_actionObject.IsDisposed)
                        {
                            a_actionObject.Invoke(new Action(() => a_action(ume, a_functionArguments)));
                        }
                    }
                    else
                    {
                        a_action(ume, a_functionArguments);
                    }
                }

                Status = EResultStatus.Finished;
                return;
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (e.WritePending)
                {
                    WaitForWriteLockRelease();
                }
            }
            catch (ObjectDisposedException)
            {
                //This scenario is being disposed. There is nothing else to do here.
                Status = EResultStatus.Error;
                return;
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return;
            }
            finally
            {
                CleanupHandling();
            }
        }
    }

    /// <summary>
    /// Locks the appropriate objects and runs an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCode(Control a_actionObject, Action<ScenarioSummary, object[]> a_action, params object[] a_functionArguments)
    {
        return Task.Factory.StartNew(() => { LockCode(a_actionObject, a_action, a_functionArguments); }, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Locks the appropriate objects and runs an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task RunLockCodeBackground(Action<ScenarioSummary, object[]> a_action, params object[] a_functionArguments)
    {
        return RunLockCode(null, a_action, a_functionArguments);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void LockCode(Control a_actionObject, Action<ScenarioSummary, object[]> a_action, params object[] a_functionArguments)
    {
        if (CheckForExclusivitySkip())
        {
            return;
        }
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        while (true)
        {
            try
            {
                if (SystemController.Sys.ScenariosLock.IsWriteLockHeld)
                {
                    InvokeSplashScreen();
                }

                using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, GetKeyWaitDuration()))
                {
                    Scenario s = FindScenario(sm);
                    using (s.ScenarioSummaryLock.TryEnterRead(out ScenarioSummary ss, GetKeyWaitDuration()))
                    {
                        if (a_actionObject != null)
                        {
                            if (!a_actionObject.IsDisposed)
                            {
                                a_actionObject.Invoke(new Action(() => a_action(ss, a_functionArguments)));
                            }
                        }
                        else
                        {
                            a_action(ss, a_functionArguments);
                        }
                    }
                }

                Status = EResultStatus.Finished;
                return;
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return;
                }

                if (e.WritePending)
                {
                    WaitForWriteLockRelease();
                }
            }
            catch (ObjectDisposedException)
            {
                //This scenario is being disposed. There is nothing else to do here.
                Status = EResultStatus.Error;
                return;
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return;
            }
            finally
            {
                CleanupHandling();
            }
        }
    }

    public class BackgroundLockException : PTHandleableException
    {
        public BackgroundLockException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }

        public BackgroundLockException(string a_message, Exception a_innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_innerException, a_stringParameters, a_appendHelpUrl) { }
    }
}