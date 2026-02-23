using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Scheduler.TransmissionDispatchingAndReception;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Dispatches transmissions. Transmissions are dispatched on separate threads than the thread that delivered the transmission to this object.
/// </summary>
internal class Dispatcher : IPTSerializable
{
    #region IPTSerializable Members
    internal Dispatcher(IReader a_reader, EDispatcherOwner a_dispatcherOwner)
    {
        m_owner = a_dispatcherOwner;
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int count);
            m_transmissions = new Queue<PTTransmission>();
            TransmissionClassFactory factory = PTSystem.TrnClassFactory;
            for (int i = 0; i < count; i++)
            {
                PTTransmission t = (PTTransmission)factory.Deserialize(a_reader);
                m_transmissions.Enqueue(t);
            }
        }

    }

    public void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        Monitor.Enter(m_transmissions);

        try
        {
            a_writer.Write(m_transmissions.Count);
            foreach (PTTransmissionBase t in m_transmissions)
            {
                a_writer.Write(t.UniqueId);
                t.Serialize(a_writer);
            }
        }
        finally
        {
            Monitor.Exit(m_transmissions);
        }
    }

    public const int UNIQUE_ID = 413;

    public int UniqueId => UNIQUE_ID;
    #endregion

    private readonly Queue<PTTransmission> m_transmissions; // A queue of Broadcaster.Transmission objects.
    // Might not need this
    internal int TransmissionsCount
    {
        get
        {
            int count; 
            Monitor.Enter(m_transmissions);
            count = m_transmissions.Count;
            Monitor.Exit(m_transmissions);
            return count;
        }
    }

    private Task m_dispatchTask; // A thread that doles out the transmissions to the listening object 1 by 1.
    private readonly EDispatcherOwner m_owner;

    private bool Dispatching => m_dispatchTask?.Status == TaskStatus.Running;

    private CancellationTokenSource m_dispatchCancelToken; 

    internal event Action<PTTransmission, int> TransmissionReceived;

    internal Dispatcher(EDispatcherOwner a_dispatcherOwner)
    {
        m_owner = a_dispatcherOwner;
        m_transmissions = new Queue<PTTransmission>();
    }

    /// <summary>
    /// This function is run on a worker thread. It dispatches the transmissions in the queue. It completes when it finds no more transmissions
    /// left (right before dispatching the last transmission).
    /// to the listener.
    /// </summary>
    private void DispatchTransmissions()
    {
        try
        {
            // This lock prevents more than one thread at a time from dispatching transmissions. So transmissions
            // will be processed in the order they were received.
            lock (this)
            {
                bool dispatching;

                do
                {
                    if (m_dispatchCancelToken.IsCancellationRequested)
                    {
                        //Stop dispatching, but transmissions should still be received and enqueued into m_transmissions
                        return;
                    }

                    PTTransmission t;
                    Monitor.Enter(m_transmissions);
                    try
                    {
                        if (m_transmissions.Count == 0)
                        {
                            return;
                        }

                        t = m_transmissions.Dequeue();
                    }
                    finally
                    {
                        Monitor.Exit(m_transmissions);
                    }

                    if (TransmissionReceived == null)
                    {
                        throw new PTException($"{m_owner.ToString()} Dispatcher.TransmissionReceived does not have any event handlers registered. The transmission being lost has Guid: {t.TransmissionId} | Description: {t.Description} | TransmissionNbr: {t.TransmissionNbr}");
                    }
                    // The lock is released while processing the transmission
                    TransmissionReceived?.Invoke(t, m_transmissions.Count);

                    // Additional transmissions may have been enqueued while the transmission is being processed
                    Monitor.Enter(m_transmissions);
                    dispatching = m_transmissions.Count > 0;
                    Monitor.Exit(m_transmissions);
                } while (dispatching);
            }
        }
        catch (Exception e)
        {
            throw new PTException("Connection Exception", e);
        }
    }

    /// <summary>
    /// As transmissions are received they are added to the transmission queue in this object
    /// one by one. A different thread within this object will dispatch the transmissions
    /// in the sequence that are received.
    /// </summary>
    /// <param name="a_t"></param>
    internal void Receive(PTTransmission a_t)
    {
        if (SkipQueue(a_t))
        {
            TransmissionReceived?.Invoke(a_t, 0);
            return;
        }

        Monitor.Enter(m_transmissions);

        try
        {
            m_transmissions.Enqueue(a_t);
            if (!Dispatching &&
                (m_dispatchCancelToken == null || !m_dispatchCancelToken.IsCancellationRequested) &&
                // null means the Dispatch task has never been started while not having cancellation requested means
                // the task was able to complete and dispatch all transmissions in the queue
                m_transmissions.Count > 0) // TODO lite client: Come back and think about this condition a bit more
            {
                StartDispatching();
            }
        }
        finally
        {
            Monitor.Exit(m_transmissions);
        }
    }
    /// <summary>
    /// Determines if transmission is allowed to jump the dispatcher queue.
    /// </summary>
    /// <param name="a_t"></param>
    /// <returns></returns>
    private bool SkipQueue(PTTransmission a_t)
    {
        return (a_t is ScenarioReplaceT && !PTSystem.Server && !Dispatching) || (a_t is ScenarioStartUndoT || (PTSystem.Server && a_t is ScenarioReplaceT t && t.InstigatorTransmissionId == ScenarioUndoT.UNIQUE_ID));
    }

    internal void StartDispatching()
    {
        if (!Dispatching)
        {
            if (m_dispatchCancelToken == null || m_dispatchCancelToken.IsCancellationRequested)
            {
                m_dispatchCancelToken = new CancellationTokenSource();
            }
            m_dispatchTask = new Task(DispatchTransmissions, m_dispatchCancelToken.Token, TaskCreationOptions.LongRunning);
            m_dispatchTask.Start();
        }
    }

    internal void CancelTransmissionsDispatching()
    {
        if (!Dispatching || m_dispatchCancelToken.IsCancellationRequested)
        {
            return;
        }

        m_dispatchCancelToken.Cancel();
    }

    internal PTTransmission DequeueTransmission()
    {
        Monitor.Enter(m_transmissions);
        // Should this dequeue be in a try-catch?
        PTTransmission transmission = m_transmissions.Dequeue();
        Monitor.Exit(m_transmissions);
        return transmission;
    }

    /// <summary>
    /// By the end of this function call all of the transmissions within this object will have been dispatched.
    /// This function was intended to be used when the system data is serialized. For it to work correctly
    /// the following things must have occurred in this order:
    /// 1. The thread that receives transmissions must have been stopped and flushed. (we should expect no additional transmissions to be necessary for PTSystem).
    /// 2. PTSystem's Dispatcher must have been flushed.
    /// 3. All the scenario's Dispatchers must be flushed (in any order).
    /// </summary>
    internal void Flush()
    {
        bool waiting;

        do
        {
            Thread.Sleep(1);

            // First check whether there are any transmissions that need to be processed. 
            // We know that no additional transmissions will be added to the queue since 
            // steps 1, 2, & 3 above were followed.
            // The only thing the transmission queue may end up being used for at this point
            // is for pulling transmissions off the queue for processing.
            Monitor.Enter(m_transmissions);

            try
            {
                // Keep waiting if there are more transmissions waiting to be processed.
                waiting = m_transmissions.Count > 0;
            }
            finally
            {
                Monitor.Exit(m_transmissions);
            }

            // The lock verifies that a thread isn't processing transmissions in DispatchTransmissions().
            // For instance, there could be 0 transmissions in queue, but the last one could still be 
            // in the process of being dispatched; this object is locked as long as transmissions are 
            // being dispatched in DispatchTransmissions().
            if (!waiting)
            {
                lock (this)
                {
                    ++m_flushCount;
                }
            }
        } while (waiting);
    }

    /// <summary>
    /// Builds a list of QueuedTransmissionData objects based on the current list of transmissions in the dispatcher.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_userManager"></param>
    /// <returns></returns>
    public List<QueuedTransmissionData> GetQueueDescriptions(ScenarioDetail a_sd)
    {
        List<QueuedTransmissionData> descriptions = new List<QueuedTransmissionData>();
        Monitor.Enter(m_transmissions);
        try
        {
            foreach (PTTransmission transmission in m_transmissions)
            {
                string actionDescription = transmission.Description;

                //Update the action description for moves and expedites, same as in the undo-redo controls. 
                if (transmission is ScenarioDetailMoveT moveT)
                {
                    int count = moveT.Count();

                    Resource resource = a_sd.PlantManager.GetResource(moveT.ToResourceKey);
                    if (resource == null)
                    {
                        continue;
                    }

                    if (count > 1)
                    {
                        actionDescription = string.Format("{0} activities moved to {1}".Localize(), count, resource.Name);
                    }

                    foreach (MoveBlockKeyData data in moveT)
                    {
                        //Only 1 block
                        BlockKey blockKey = data.BlockKey;
                        if (a_sd.JobManager.GetById(blockKey.JobId) is Job job)
                        {
                            if (job.ManufacturingOrders.GetById(blockKey.MOId) is ManufacturingOrder mo)
                            {
                                if (mo.OperationManager[blockKey.OperationId] is BaseOperation op)
                                {
                                    actionDescription = string.Format("Job {0} Op {1} moved to {2}".Localize(), job.Name, op.Name, resource.Name);
                                }
                            }
                        }
                    }
                }
                else if (transmission is ScenarioDetailExpediteMOsT expediteT)
                {
                    if (expediteT.MOs.Count > 1)
                    {
                        actionDescription = string.Format("{0} jobs expedited".Localize(), expediteT.MOs.Count);
                    }
                    else if (expediteT.MOs.Count == 1)
                    {
                        //Single expedite
                        MOKeyList.Node moNode = expediteT.MOs.First;
                        Job job = a_sd.JobManager.GetById(moNode.Data.JobId);
                        if (job == null)
                        {
                            continue;
                        }

                        if (job.ManufacturingOrders.Count == 1)
                        {
                            actionDescription = string.Format("{0} expedited".Localize(), job.Name);
                        }
                        else
                        {
                            ManufacturingOrder manufacturingOrder = job.ManufacturingOrders.GetById(moNode.Data.MOId);
                            if (manufacturingOrder != null)
                            {
                                actionDescription = string.Format("Job {0} MO {1} expedited".Localize(), job.Name, manufacturingOrder.Name);
                            }
                        }
                    }
                    else
                    {
                        actionDescription = "No Manufacturing Orders were expedited.".Localize();
                    }
                }
                else if (transmission is ScenarioDetailExpediteJobsT expediteJobsT)
                {
                    if (expediteJobsT.Jobs.Count > 1)
                    {
                        actionDescription = string.Format("{0} jobs expedited".Localize(), expediteJobsT.Jobs.Count);
                    }
                    else if (expediteJobsT.Jobs.Count == 1)
                    {
                        //Single expedite
                        Job job = a_sd.JobManager.GetById(expediteJobsT.Jobs.GetFirst());
                        if (job != null)
                        {
                            actionDescription = string.Format("{0} expedited".Localize(), job.Name);
                        }
                    }
                    else
                    {
                        actionDescription = "No jobs were expedited.".Localize();
                    }
                }

                descriptions.Add(new QueuedTransmissionData(transmission.Instigator, actionDescription ?? "Description unavailable".Localize(), transmission.TimeStamp));
            }
        }
        finally
        {
            Monitor.Exit(m_transmissions);
        }

        return descriptions;
    }

    /// <summary>
    /// This variable's main purpose is to ensure that there isn't an empty statement within a lock
    /// so it doesn't get optimized out by the compiler when the code is built in Release.
    /// The reason for the lock is described where this variable gets incremented. 
    /// </summary>
    private long m_flushCount;

    public bool DoesTransmissionExist(Guid a_lastReceivedTransmissionId)
    {
        // The code that calls this function has a lock on m_transmissions
        bool transmissionExists = false;
        foreach (PTTransmission transmission in m_transmissions)
        {
            if (transmission.TransmissionId == a_lastReceivedTransmissionId)
            {
                transmissionExists = true;
                break;
            }
        }
        return transmissionExists;
    }

    
    /// <summary>
    /// Merges transmissions from a previous dispatcher into the current transmission queue.
    /// If the current queue is not empty, only transmissions with a timestamp later than the 
    /// first (oldest) queued transmission are added. If the queue is empty, transmissions with 
    /// timestamps greater than the provided scenario's last received timestamp are enqueued. 
    /// Duplicate transmissions (based on transmission ID) are skipped. 
    /// </summary>
    /// <param name="a_previousDispatcher">The dispatcher from which to dequeue and merge transmissions.</param>
    /// <param name="a_scenarioLastReceivedTransmissionTimeTicks"> The reference timestamp used when the current transmission queue is empty. </param>
    internal void MergeTransmissions(Dispatcher a_previousDispatcher, long a_scenarioLastReceivedTransmissionTimeTicks)
    {
        Monitor.Enter(m_transmissions);
        try
        {
            long transmissionAfter;
            if (m_transmissions.Count > 0)
            {
                PTTransmission topCurrentTransmission = m_transmissions.Peek();
                transmissionAfter = topCurrentTransmission.TimeStamp.Ticks;
            }
            else
            {
                transmissionAfter = a_scenarioLastReceivedTransmissionTimeTicks;
            }


            while (a_previousDispatcher.TransmissionsCount > 0)
            {
                PTTransmission dequeuedTransmission = a_previousDispatcher.DequeueTransmission();

                if (!DoesTransmissionExist(dequeuedTransmission.TransmissionId) && dequeuedTransmission.TimeStamp.Ticks > transmissionAfter)
                {
                    m_transmissions.Enqueue(dequeuedTransmission);
                }
            }
        }
        finally
        {
            Monitor.Exit(m_transmissions);
        }
    }
}