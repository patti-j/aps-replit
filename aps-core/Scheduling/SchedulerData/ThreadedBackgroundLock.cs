using System.Windows.Forms;

using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Scheduler;
using PT.UIDefinitions.Interfaces;

namespace PT.SchedulerData;

/// <summary>
/// The New background lock. Use this instead of BackgroundLock
/// </summary>
public class ThreadedBackgroundLock : IDisposable
{
    protected readonly BaseId m_scenarioId;
    protected bool m_canceled;
    private static IExceptionManager s_exceptionManager;

    private const int c_threadWaitInitialMs = 100;
    private const int c_threadWaitMaxMs = 2250;
    private int m_keyWaitMs = c_threadWaitInitialMs;
    private System.Timers.Timer m_autoCancelTimer;

    public static void SetInvokeControl(Control a_control)
    {
        BackgroundLockData.SetInvokeControl(a_control);
    }

    private bool ContinueLocking
    {
        get => Status != EResultStatus.Canceled && Status != EResultStatus.Error && Status != EResultStatus.Finished;
    }

    public EResultStatus Status;
    private Exception m_backgroundLockException;

    /// <summary>
    /// Handles background processing of the PT locks. The calling thread should call the lock functions on a new task so the thread will not be blocked while waiting for lock
    /// </summary>
    /// <param name="a_scenarioId">Scenario Id in which to lock</param>
    public ThreadedBackgroundLock(BaseId a_scenarioId)
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
    private static readonly object m_orderLock = new();
    private static readonly Dictionary<string, List<int>> m_lockOrderList = new();

    //If this lock would like to maintain an ordering amongst other locks with the same key
    public void AttachLockToQueue(string a_key)
    {
        lock (m_lockOrderList)
        {
            m_key = a_key;
            m_order = GetMaxOrderingForKey(a_key) + 1;
            AddLockOrdering(a_key, m_order);
        }
    }

    //Return the newest key
    private int GetMaxOrderingForKey(string a_key)
    {
        lock (m_orderLock)
        {
            if (m_lockOrderList.TryGetValue(a_key, out List<int> list))
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

        lock (m_orderLock)
        {
            if (m_lockOrderList.TryGetValue(a_key, out List<int> list))
            {
                list.Add(a_order);
            }
            else
            {
                m_lockOrderList.Add(a_key, new List<int> { a_order });
            }
        }
    }

    private void PopLockOrdering(string a_key)
    {
        lock (m_orderLock)
        {
            if (m_lockOrderList.TryGetValue(a_key, out List<int> list))
            {
                list.Remove(m_order);
                if (list.Count == 0)
                {
                    m_lockOrderList.Remove(a_key);
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
            lock (m_orderLock)
            {
                if (m_lockOrderList.TryGetValue(m_key, out List<int> orderList))
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
        m_keyWaitMs = Math.Max(m_keyWaitMs + 20, c_threadWaitMaxMs);
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
        if (Status != EResultStatus.Error && Status != EResultStatus.Canceled && Status != EResultStatus.Finished)
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

    private class BackgroundLockData
    {
        protected BackgroundLockData(bool a_runInBackground)
        {
            m_runInBackground = a_runInBackground;
        }

        private bool m_runInBackground;
        protected object[] m_functionArguments;

        public bool LockScenarioManager { get; protected set; }
        public bool LockScenario { get; protected set; }
        public bool LockScenarioDetail { get; protected set; }
        public bool LockScenarioEvents { get; protected set; }
        public bool LockScenarioSummary { get; protected set; }
        public bool LockScenarioUndoEvents { get; protected set; }
        public bool LockScenarioUndoSets { get; protected set; }
        public bool LockUserManager { get; protected set; }
        public bool LockUserManagerEvents { get; protected set; }
        public bool ScenarioDataRequired => LockScenarioManager
                                            || LockScenario
                                            || LockScenarioDetail
                                            || LockScenarioEvents
                                            || LockScenarioSummary
                                            || LockScenarioUndoEvents
                                            || LockScenarioUndoSets;

        private static Control s_actionObject;

        internal void InvokeAction(LockedObjects a_lockedObjects)
        {
            if (m_runInBackground)
            {
                Action(a_lockedObjects);
            }
            else
            {
                s_actionObject.Invoke(new Action(() => Action(a_lockedObjects)));
            }
        }

        protected virtual void Action(LockedObjects a_lockedObjects)
        {
            throw new NotImplementedException();
        }

        public static void SetInvokeControl(Control a_control)
        {
            s_actionObject = a_control;
        }
    }

    private class ScenarioDetailLockData : BackgroundLockData
    {
        private readonly Action<Scenario, ScenarioDetail, object[]> m_action;

        internal ScenarioDetailLockData(Action<Scenario, ScenarioDetail, object[]> a_action, object[] a_functionArguments, bool a_runInBackground)
            : base(a_runInBackground)
        {
            m_action = a_action;
            m_functionArguments = a_functionArguments;
            LockScenario = true;
            LockScenarioDetail = true;
        }

        protected override void Action(LockedObjects a_lockedObjects)
        {
            m_action(a_lockedObjects.Scenario, a_lockedObjects.ScenarioDetail, m_functionArguments);
        }
    }

    private class ScenarioManagerDetailLockData : BackgroundLockData
    {
        private readonly Action<ScenarioManager, Scenario, ScenarioDetail, object[]> m_action;

        internal ScenarioManagerDetailLockData(Action<ScenarioManager, Scenario, ScenarioDetail, object[]> a_action, object[] a_functionArguments, bool a_runInBackground)
            : base(a_runInBackground)
        {
            m_action = a_action;
            m_functionArguments = a_functionArguments;
            LockScenarioManager = true;
            LockScenario = true;
            LockScenarioDetail = true;
        }

        protected override void Action(LockedObjects a_lockedObjects)
        {
            m_action(a_lockedObjects.ScenarioManager, a_lockedObjects.Scenario, a_lockedObjects.ScenarioDetail, m_functionArguments);
        }
    }

    private class ScenarioDetailAndEventsLockData : BackgroundLockData
    {
        private readonly Action<Scenario, ScenarioDetail, ScenarioEvents, object[]> m_action;

        internal ScenarioDetailAndEventsLockData(Action<Scenario, ScenarioDetail, ScenarioEvents, object[]> a_action, object[] a_functionArguments, bool a_runInBackground)
            : base(a_runInBackground)
        {
            m_action = a_action;
            m_functionArguments = a_functionArguments;
            LockScenario = true;
            LockScenarioDetail = true;
            LockScenarioEvents = true;
        }

        protected override void Action(LockedObjects a_lockedObjects)
        {
            m_action(a_lockedObjects.Scenario, a_lockedObjects.ScenarioDetail, a_lockedObjects.ScenarioEvents, m_functionArguments);
        }
    }

    private class ScenarioDetailAndUserManagerLockData : BackgroundLockData
    {
        private readonly Action<Scenario, ScenarioDetail, UserManager, object[]> m_action;

        internal ScenarioDetailAndUserManagerLockData(Action<Scenario, ScenarioDetail, UserManager, object[]> a_action, object[] a_functionArguments, bool a_runInBackground)
            : base(a_runInBackground)
        {
            m_action = a_action;
            m_functionArguments = a_functionArguments;
            LockScenario = true;
            LockScenarioDetail = true;
            LockUserManager = true;
        }

        protected override void Action(LockedObjects a_lockedObjects)
        {
            m_action(a_lockedObjects.Scenario, a_lockedObjects.ScenarioDetail, a_lockedObjects.UserManager, m_functionArguments);
        }
    }

    private class UserManagerLockData : BackgroundLockData
    {
        private readonly Action<UserManager, object[]> m_action;

        internal UserManagerLockData(Action<UserManager, object[]> a_action, object[] a_functionArguments, bool a_runInBackground)
            : base(a_runInBackground)
        {
            m_action = a_action;
            m_functionArguments = a_functionArguments;
            LockUserManager = true;
        }

        protected override void Action(LockedObjects a_lockedObjects)
        {
            m_action(a_lockedObjects.UserManager, m_functionArguments);
        }
    }

    private class ScenarioManagerUserManagerLockData : BackgroundLockData
    {
        private readonly Action<ScenarioManager, UserManager, object[]> m_action;

        internal ScenarioManagerUserManagerLockData(Action<ScenarioManager, UserManager, object[]> a_action, object[] a_functionArguments, bool a_runInBackground)
            : base(a_runInBackground)
        {
            m_action = a_action;
            m_functionArguments = a_functionArguments;
            LockUserManager = true;
            LockScenarioManager = true;
        }

        protected override void Action(LockedObjects a_lockedObjects)
        {
            m_action(a_lockedObjects.ScenarioManager, a_lockedObjects.UserManager, m_functionArguments);
        }
    }

    private class UserManagerEventsLockData : BackgroundLockData
    {
        private readonly Action<UserManagerEvents, object[]> m_action;

        internal UserManagerEventsLockData(Action<UserManagerEvents, object[]> a_action, object[] a_functionArguments, bool a_runInBackground)
            : base(a_runInBackground)
        {
            m_action = a_action;
            m_functionArguments = a_functionArguments;
            LockUserManagerEvents = true;
        }

        protected override void Action(LockedObjects a_lockedObjects)
        {
            m_action(a_lockedObjects.UserManagerEvents, m_functionArguments);
        }
    }

    private class ScenarioSummaryLockData : BackgroundLockData
    {
        private readonly Action<ScenarioSummary, object[]> m_action;

        internal ScenarioSummaryLockData(Action<ScenarioSummary, object[]> a_action, object[] a_functionArguments, bool a_runInBackground)
            : base(a_runInBackground)
        {
            m_action = a_action;
            m_functionArguments = a_functionArguments;
            LockScenarioSummary = true;
        }

        protected override void Action(LockedObjects a_lockedObjects)
        {
            m_action(a_lockedObjects.ScenarioSummary, m_functionArguments);
        }
    }

    private class ScenarioUndoEventsLockData : BackgroundLockData
    {
        private readonly Action<Scenario, Scenario.UndoSets> m_action;

        internal ScenarioUndoEventsLockData(Action<Scenario, Scenario.UndoSets> a_action, bool a_runInBackground)
            : base(a_runInBackground)
        {
            m_action = a_action;
            LockScenario = true;
            LockScenarioUndoSets = true;
        }

        protected override void Action(LockedObjects a_lockedObjects)
        {
            m_action(a_lockedObjects.Scenario, a_lockedObjects.ScenarioUndoSets);
        }
    }

    private class ScenarioEventsAndUndoEventsLockData : BackgroundLockData
    {
        private readonly Action<Scenario, ScenarioEvents, ScenarioUndoEvents> m_action;

        internal ScenarioEventsAndUndoEventsLockData(Action<Scenario, ScenarioEvents, ScenarioUndoEvents> a_action, bool a_runInBackground)
            : base(a_runInBackground)
        {
            m_action = a_action;
            LockScenario = true;
            LockScenarioEvents = true;
            LockScenarioUndoEvents = true;
        }

        protected override void Action(LockedObjects a_lockedObjects)
        {
            m_action(a_lockedObjects.Scenario, a_lockedObjects.ScenarioEvents, a_lockedObjects.ScenarioUndoEvents);
        }
    }

    private class ScenarioLockData : BackgroundLockData
    {
        private readonly Action<Scenario> m_action;

        internal ScenarioLockData(Action<Scenario> a_action, bool a_runInBackground)
            : base(a_runInBackground)
        {
            m_action = a_action;
            LockScenario = true;
        }

        protected override void Action(LockedObjects a_lockedObjects)
        {
            m_action(a_lockedObjects.Scenario);
        }
    }

    private class ScenarioManagerLockData : BackgroundLockData
    {
        private readonly Action<ScenarioManager> m_action;

        internal ScenarioManagerLockData(Action<ScenarioManager> a_action, bool a_runInBackground)
            : base(a_runInBackground)
        {
            m_action = a_action;
            LockScenarioManager = true;
        }

        protected override void Action(LockedObjects a_lockedObjects)
        {
            m_action(a_lockedObjects.ScenarioManager);
        }
    }

    private class LockedObjects
    {
        internal ScenarioManager ScenarioManager;
        internal Scenario Scenario;
        internal ScenarioDetail ScenarioDetail;
        internal ScenarioSummary ScenarioSummary;
        internal ScenarioEvents ScenarioEvents;
        internal ScenarioUndoEvents ScenarioUndoEvents;
        internal Scenario.UndoSets ScenarioUndoSets;
        internal UserManager UserManager;
        internal UserManagerEvents UserManagerEvents;

        public void Dispose()
        {
            ScenarioManager = null;
            Scenario = null;
            ScenarioDetail = null;
            ScenarioSummary = null;
            ScenarioEvents = null;
            ScenarioUndoEvents = null;
            ScenarioUndoSets = null;
            UserManager = null;
            UserManagerEvents = null;
        }
    }

    private LockResult LockCode(BackgroundLockData a_data)
    {
        WaitForQueue();
        InitializeLock();
        Status = EResultStatus.Locking;
        LockedObjects lockedObjects = new LockedObjects();

        while (true)
        {
            try
            {
                //Scenario Manager or Scenario Data
                if (a_data.ScenarioDataRequired)
                {
                    if (SystemController.Sys.ScenariosLock.IsWriteLockHeld)
                    {
                        InvokeSplashScreen();
                    }

                    using AutoDisposer smLock = SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, GetKeyWaitDuration());
                    lockedObjects.ScenarioManager = sm;
                    if (a_data.LockScenario)
                    {
                        Scenario s = sm.Find(m_scenarioId);
                        lockedObjects.Scenario = s;

                        //Scenario Detail
                        if (a_data.LockScenarioDetail)
                        {
                            if (s.ScenarioDetailLock.IsWriteLockHeld)
                            {
                                InvokeSplashScreen();
                            }
                            
                            using AutoDisposer sdLock = s.ScenarioDetailLock.TryEnterRead(out ScenarioDetail sd, GetKeyWaitDuration());
                            lockedObjects.ScenarioDetail = sd;
                        }

                        if (a_data.LockScenarioSummary)
                        {
                            if (s.ScenarioSummaryLock.IsWriteLockHeld)
                            {
                                InvokeSplashScreen();
                            }

                            using AutoDisposer sdLock = s.ScenarioSummaryLock.TryEnterRead(out ScenarioSummary ss, GetKeyWaitDuration());
                            lockedObjects.ScenarioSummary = ss;
                        }

                        if (a_data.LockScenarioEvents)
                        {
                            if (s.ScenarioEventsLock.IsWriteLockHeld)
                            {
                                InvokeSplashScreen();
                            }

                            using AutoDisposer sdLock = s.ScenarioEventsLock.TryEnterRead(out ScenarioEvents se, GetKeyWaitDuration());
                            lockedObjects.ScenarioEvents = se;
                        }

                        if (a_data.LockScenarioUndoEvents)
                        {
                            if (s.ScenarioUndoEventsLock.IsWriteLockHeld)
                            {
                                InvokeSplashScreen();
                            }

                            using AutoDisposer sdLock = s.ScenarioUndoEventsLock.TryEnterRead(out ScenarioUndoEvents sue, GetKeyWaitDuration());
                            lockedObjects.ScenarioUndoEvents = sue;
                        }

                        if (a_data.LockScenarioUndoSets)
                        {
                            if (s.UndoSetsLock.IsWriteLockHeld)
                            {
                                InvokeSplashScreen();
                            }

                            using AutoDisposer sdLock = s.UndoSetsLock.TryEnterRead(out Scenario.UndoSets undoSets, GetKeyWaitDuration());
                            lockedObjects.ScenarioUndoSets = undoSets;
                        }
                    }
                }

                if (a_data.LockUserManager)
                {
                    using AutoDisposer umLock = SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, GetKeyWaitDuration());
                    lockedObjects.UserManager = um;
                }

                if (a_data.LockUserManagerEvents)
                {
                    using AutoDisposer umeLock = SystemController.Sys.UserManagerEventsLock.TryEnterRead(out UserManagerEvents ume, GetKeyWaitDuration());
                    lockedObjects.UserManagerEvents = ume;
                }

                Status = EResultStatus.Processing;

                InvokeDisableCancel();
                a_data.InvokeAction(lockedObjects);

                Status = EResultStatus.Finished;
                return new LockResult(Status);
            }
            catch (AutoTryEnterException e)
            {
                if (m_canceled)
                {
                    Status = EResultStatus.Canceled;
                    return new LockResult(Status);
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
                return new LockResult(Status);
            }
            catch (Exception error)
            {
                LogExceptions(error);
                return new LockResult(Status, m_backgroundLockException);
            }
            finally
            {
                lockedObjects.Dispose();
                PopLockOrdering(m_key);
            }
        }
    }

    private Task<LockResult> RunLockCodeTask(BackgroundLockData a_data)
    {
        return Task.Factory.StartNew(() => LockCode(a_data), TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCode(Action<ScenarioManager, Scenario, ScenarioDetail, object[]> a_action, params object[] a_functionArguments)
    {
        ScenarioManagerDetailLockData lockData = new(a_action, a_functionArguments, false);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCodeBackground(Action<ScenarioManager, Scenario, ScenarioDetail, object[]> a_action, params object[] a_functionArguments)
    {
        ScenarioManagerDetailLockData lockData = new(a_action, a_functionArguments, true);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCode(Action<Scenario, ScenarioDetail, object[]> a_action, params object[] a_functionArguments)
    {
        ScenarioDetailLockData lockData = new(a_action, a_functionArguments, false);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and runs an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCodeBackground(Action<Scenario, ScenarioDetail, object[]> a_action, params object[] a_functionArguments)
    {
        ScenarioDetailLockData lockData = new(a_action, a_functionArguments, true);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    private void LockCode(Control a_actionObject, Action<Scenario, ScenarioDetail, object[]> a_action, params object[] a_functionArguments)
    {
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
            PopLockOrdering(m_key);
        }
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCode(Action<Scenario, ScenarioDetail, ScenarioEvents, object[]> a_action, params object[] a_functionArguments)
    {
        ScenarioDetailAndEventsLockData lockData = new(a_action, a_functionArguments, false);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and runs an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCodeBackground(Action<Scenario, ScenarioDetail, ScenarioEvents, object[]> a_action, params object[] a_functionArguments)
    {
        ScenarioDetailAndEventsLockData lockData = new(a_action, a_functionArguments, true);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCode(Action<Scenario, ScenarioDetail, UserManager, object[]> a_action, params object[] a_functionArguments)
    {
        ScenarioDetailAndUserManagerLockData lockData = new(a_action, a_functionArguments, false);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCodeBackground(Action<Scenario, ScenarioDetail, UserManager, object[]> a_action, params object[] a_functionArguments)
    {
        ScenarioDetailAndUserManagerLockData lockData = new(a_action, a_functionArguments, true);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCode(Action<Scenario, Scenario.UndoSets> a_action)
    {
        ScenarioUndoEventsLockData lockData = new(a_action, false);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCodeBackground(Action<Scenario, Scenario.UndoSets> a_action)
    {
        ScenarioUndoEventsLockData lockData = new(a_action, true);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCode(Action<Scenario, ScenarioEvents, ScenarioUndoEvents> a_action)
    {
        ScenarioEventsAndUndoEventsLockData lockData = new(a_action, false);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCodeBackground(Action<Scenario, ScenarioEvents, ScenarioUndoEvents> a_action)
    {
        ScenarioEventsAndUndoEventsLockData lockData = new(a_action, true);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCode(Action<UserManager, object[]> a_action, params object[] a_functionArguments)
    {
        UserManagerLockData lockData = new(a_action, a_functionArguments, false);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCodeBackground(Action<UserManager, object[]> a_action, params object[] a_functionArguments)
    {
        UserManagerLockData lockData = new(a_action, a_functionArguments, true);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCode(Action<Scenario> a_action)
    {
        ScenarioLockData lockData = new(a_action, false);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCodeBackground(Action<Scenario> a_action)
    {
        ScenarioLockData lockData = new(a_action, true);
        return RunLockCodeTask(lockData);
    }


    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCode(Action<ScenarioManager> a_action)
    {
        ScenarioManagerLockData lockData = new(a_action, false);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCodeBackground(Action<ScenarioManager> a_action)
    {
        ScenarioManagerLockData lockData = new(a_action, true);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_actionObject">Control on which to invoke the action</param>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCode(Control a_actionObject, Action<ScenarioManager, UserManager, object[]> a_action, params object[] a_functionArguments)
    {
        ScenarioManagerUserManagerLockData lockData = new(a_action, a_functionArguments, false);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCodeBackground(Action<ScenarioManager, UserManager, object[]> a_action, params object[] a_functionArguments)
    {
        ScenarioManagerUserManagerLockData lockData = new(a_action, a_functionArguments, true);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and invokes an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCode(Action<UserManagerEvents, object[]> a_action, params object[] a_functionArguments)
    {
        UserManagerEventsLockData lockData = new(a_action, a_functionArguments, false);
        return RunLockCodeTask(lockData);
    }

    public Task<LockResult> RunLockCodeBackground(Action<UserManagerEvents, object[]> a_action, params object[] a_functionArguments)
    {
        UserManagerEventsLockData lockData = new(a_action, a_functionArguments, true);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and runs an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCode(Action<ScenarioSummary, object[]> a_action, params object[] a_functionArguments)
    {
        ScenarioSummaryLockData lockData = new(a_action, a_functionArguments, false);
        return RunLockCodeTask(lockData);
    }

    /// <summary>
    /// Locks the appropriate objects and runs an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    public Task<LockResult> RunLockCodeBackground(Action<ScenarioSummary, object[]> a_action, params object[] a_functionArguments)
    {
        ScenarioSummaryLockData lockData = new(a_action, a_functionArguments, true);
        return RunLockCodeTask(lockData);
    }

    public class BackgroundLockException : PTHandleableException
    {
        public BackgroundLockException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }

        public BackgroundLockException(string a_message, Exception a_innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_innerException, a_stringParameters, a_appendHelpUrl) { }
    }
}