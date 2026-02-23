using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PT.APSCommon;
//using static PT.SchedulerData.BackgroundLock;

namespace PT.SchedulerData;

/// <summary>
/// This class is a WIP. It is supposed to be a wrapper around the background locks that can help reduce the total number of threads run in parallel.
/// The idea is to create a background lock and queue up actions so that each awaiter is awaiting the same task
/// From investigation we found that the backgound locks could get the data too quickly, causing lots of threads
///   So if the data locks are open, it might make sense to wait a few ms before running the action to wait for more requests to queue
///   Potentially add a static flag that can be enabled during app startup or other times there are lots of locks about to be created.
///   For example flag when a sim is complete, or app startup so the lock will wait for multiple locks to queue up, then disable the flag after some time.
/// </summary>
public class ScenarioDataLock
{
    public ScenarioDataLock(BaseId a_scenarioId)
    {
        m_scenarioId = a_scenarioId;
        //m_sdLock = new BackgroundLock(m_scenarioId);
    }

    private readonly BaseId m_scenarioId;

    //private BackgroundLock m_sdLock;

    public ThreadedBackgroundLock CreateNewLock()
    {
        return new ThreadedBackgroundLock(m_scenarioId);
    }

    /// <summary>
    /// Locks the appropriate objects and runs an action when the locks are obtained. Will attempt to show the Splash screen if needed.
    /// </summary>
    /// <param name="a_action">The function which requires the specified locked objects</param>
    //public Task<LockResult> RunLockCodeBackground(Action<Scenario, ScenarioDetail, object[]> a_action, params object[] a_functionArguments)
    //{
    //    BackgroundLockData data = new BackgroundLockData(a_action, a_functionArguments, true);
    //    return RunLockCode(data);
    //}

    //public Task<LockResult> RunLockCode(Action<Scenario, ScenarioDetail, object[]> a_action, params object[] a_functionArguments)
    //{
    //    BackgroundLockData data = new BackgroundLockData(a_action, a_functionArguments, false);
    //    return RunLockCode(data);
    //}

    //private Task<LockResult> RunLockCode(BackgroundLockData a_data)
    //{
    //    if (m_sdLock.RunLockCode(a_data))
    //    {
    //        return m_sdLock.LockTask;
    //    }

    //    m_sdLock = new BackgroundLock(m_scenarioId);
    //    m_sdLock.RunLockCode(a_data);
    //    return m_sdLock.LockTask;
    //}
}

public struct LockResult
{
    public LockResult(EResultStatus a_status)
    {
        Status = a_status;
    }

    public LockResult(EResultStatus a_status, Exception a_error)
    {
        Status = a_status;
        Error = a_error;
    }

    public EResultStatus Status;
    public Exception Error;
}

public enum EResultStatus
{
    NotStarted,
    Locking,
    Processing,
    Error,
    Canceled,
    Finished
}

