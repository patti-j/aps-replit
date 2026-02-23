using PT.Common.Collections;
using PT.Scheduler.Simulation.Events;

namespace PT.Scheduler;

/// <summary>
/// This is a MainResourceSet with events for a particular stage.
/// The events are used when the stage is simulated.
/// </summary>
internal class StageSet : MainResourceSet
{
    internal StageSet()
    {
        EventBaseComparer comparer = new ();
        m_stateEvents = new PriorityQueue<EventBase>(comparer);
    }

    private List<EventBase> m_stageEvents = new ();
    private readonly PriorityQueue<EventBase> m_stateEvents;

    internal int EventCount => m_stateEvents.Count;

    internal void AddEvent(EventBase e)
    {
        m_stateEvents.Insert(e);
    }

    internal EventBase GetNextEvent()
    {
        return m_stateEvents.DeleteMin();
    }
}