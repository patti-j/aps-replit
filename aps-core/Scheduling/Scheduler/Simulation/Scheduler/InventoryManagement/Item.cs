using PT.Scheduler.Simulation.Events;

namespace PT.Scheduler;

public partial class Item
{
    /// <summary>
    /// ItemManager will initialize this value to a unique value among items starting from 0.
    /// This is used by ScenarioDetail as the index of a stage value in an array.
    /// </summary>
    internal int StageArrayIndex { get; set; }

    /// <summary>
    /// The set of materials for demands such as Purchase Orders and transfer orders that
    /// couldn't be fullfilled at their need dates. The events for these demands
    /// are stored in this set. When material becomes available, these events are added
    /// back to the event queue to try to fullfill them.
    /// </summary>
    private readonly List<Simulation.Events.EventBase> m_retryMatlEvents = new ();

    /// <summary>
    /// The activities that failed to schedule due to storage constraints
    /// </summary>
    private readonly List<Simulation.Events.EventBase> m_retryStorageEvents = new();

    /// <summary>
    /// Add an unfulfilled demand to the retry set.
    /// </summary>
    /// <param name="a_evt"></param>
    internal void AddRetryMatlEvt(Simulation.Events.EventBase a_evt)
    {
        m_retryMatlEvents.Add(a_evt);
    }

    /// <summary>
    /// Add an unfulfilled demand to the retry set.
    /// </summary>
    /// <param name="a_evt"></param>
    internal void AddRetryStorageEvent(Simulation.Events.EventBase a_evt)
    {
        m_retryStorageEvents.Add(a_evt);
    }

    /// <summary>
    /// Clear the retry set. You should do this after you've added the events back to the event queue.
    /// </summary>
    internal void ClearRetryMatlEvtSet()
    {
        m_retryMatlEvents.Clear();
    }
    
    /// <summary>
    /// Clear the retry set. You should do this after you've added the events back to the event queue.
    /// </summary>
    internal void ClearRetryStorageEventSet()
    {
        m_retryStorageEvents.Clear();
    }

    /// <summary>
    /// Get an enumerator to the retry events.
    /// </summary>
    /// <returns></returns>
    internal IEnumerable<EventBase> GetRetryReqMatlEvtSetEnumerator()
    {
        foreach (EventBase retryEvent in m_retryMatlEvents)
        {
            yield return retryEvent;
        }
    }
    
    /// <summary>
    /// Get an enumerator to the retry events.
    /// </summary>
    /// <returns></returns>
    internal IEnumerable<EventBase> GetRetryStorageEventSet()
    {
        return m_retryStorageEvents;
    }

    internal void ResetSimulationStateVariables()
    {
        m_minAges = new HashSet<long>();

        m_retryMatlEvents.Clear();
        m_retryStorageEvents.Clear();

        SaveExpiredMaterial = false;
    }

    /// <summary>
    /// All of the MinAges of all the requirements for this item.
    /// This is filled prior to simulation and is used to create events
    /// that allow scheduling at the times the materials become aged enough to
    /// satisfy material requirements.
    /// </summary>
    private HashSet<long> m_minAges;

    /// <summary>
    /// Add a MinAge to this item. This function must be called for every requirement that specifies a MinAge.
    /// </summary>
    /// <param name="a_ageSpanTicks"></param>
    internal void AddMinAge(long a_ageSpanTicks)
    {
        m_minAges.Add(a_ageSpanTicks);
    }

    /// <summary>
    /// Whether any material requirements have Min Age requirements.
    /// </summary>
    internal bool HasMinAges => m_minAges.Count > 0;

    /// <summary>
    /// Enumerare the MinAges for this item.
    /// </summary>
    /// <returns></returns>
    internal HashSet<long>.Enumerator GetMinAgesEnumerator()
    {
        return m_minAges.GetEnumerator();
    }

    /// <summary>
    /// Whether there is at least one demand source for this item that can use expired material
    /// </summary>
    internal bool SaveExpiredMaterial;
}