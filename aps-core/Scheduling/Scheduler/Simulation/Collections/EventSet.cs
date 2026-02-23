using PT.Common.Collections;
using PT.Scheduler.Simulation.Events;

namespace PT.Scheduler;

/// <summary>
/// Holds a priority queue for simulation.
/// </summary>
internal class EventSet
{
    private int m_jobEventsCount;
    private long m_nextSequenceNbr;
    private PriorityQueue<EventPriorityQueueNode> m_pq;

    internal EventSet()
    {
        m_pq = new PriorityQueue<EventPriorityQueueNode>(new EventPriorityQueueNodeComparer());
    }

    internal int Count => m_pq.Count;

    internal int JobEventsCount => m_jobEventsCount;

    public void Clear()
    {
        m_jobEventsCount = 0;
        m_nextSequenceNbr = 0;
        m_pq = new PriorityQueue<EventPriorityQueueNode>(new EventPriorityQueueNodeComparer());

        #if TEST
			addedEvents = new PT.Common.File.TextFile();
			count = 0;
        #endif
    }

    #if TEST
		PT.Common.File.TextFile addedEvents;
		int count;
    #endif

    internal void AddEvent(EventBase a_simEvent)
    {
        EventPriorityQueueNode node = new ();
        node.m_time = a_simEvent.Time;

        #if TEST
			DateTime dt = new DateTime(simEvent.Time);

			bool added = false;
			if(simEvent is JITReleaseEvent)
			{
				++count;
				JITReleaseEvent jitRel = (JITReleaseEvent)simEvent;
				string description = string.Format("{0} {1} {2} {3} {4}",
					simEvent.Time.ToString().PadLeft(20, ' '),
					dt.ToLongDateString().PadLeft(30, ' '),
					dt.ToLongTimeString().PadLeft(11, ' '),
					simEvent.ToString(),
					jitRel.Activity.de);

				addedEvents.AppendText(description);

				added = true;
			}

			if(!added)
			{
				string description = string.Format("{0} {1} {2} {3}",
					simEvent.Time.ToString().PadLeft(20, ' '),
					dt.ToLongDateString().PadLeft(30, ' '),
					dt.ToLongTimeString().PadLeft(11, ' '),
					simEvent.ToString());

				addedEvents.AppendText(description);
			}
        #endif
        node.m_sequenceNbr = m_nextSequenceNbr;
        ++m_nextSequenceNbr;

        node.m_event = a_simEvent;

        if (initializingInsertionMode)
        {
            m_pq.InitialInsert(node);
        }
        else
        {
            m_pq.Insert(node);
        }

        if (NonJobTypeEvent(a_simEvent)) { }
        else
        {
            m_jobEventsCount++;
        }
    }

    internal void AddEvents(IEnumerable<EventBase> a_events)
    {
        foreach (EventBase e in a_events)
        {
            AddEvent(e);
        }
    }

    private bool NonJobTypeEvent(EventBase a_simEvent)
    {
        return a_simEvent.UniqueId == ResourceUnavailableEvent.UNIQUE_ID || a_simEvent.UniqueId == ResourceAvailableEvent.UNIQUE_ID || a_simEvent.UniqueId == PlanningHorizonEvent.UNIQUE_ID;
    }

    #if TEST
		internal void WriteAdds()
		{
			addedEvents.WriteFile(Path.Combine(WorkingDirectory.Errors, "PriorityQueueInsertionSequence.txt"));
		}
    #endif

    private bool initializingInsertionMode;

    /// <summary>
    /// Whether the queue is in initial insertion mode. If so then the elements in the queue haven't been arranged.
    /// </summary>
    internal bool InitialInsertionMode => initializingInsertionMode;

    /// <summary>
    /// Turn on initial insertion mode.
    /// </summary>
    internal void InitialInsertionModeStart()
    {
        initializingInsertionMode = true;
    }

    internal void InitialInsertionModeComplete()
    {
        #if DEBUG
        if (!initializingInsertionMode)
        {
            throw new Exception("The Priority Queue wasn't in Initial Insertion mode.");
        }
        #endif

        m_pq.InitialInsertionComplete();
        initializingInsertionMode = false;
    }

    internal EventBase PeekMin()
    {
        return m_pq.PeekMin().m_event;
    }

    internal EventBase DeleteMin()
    {
        #if DEBUG
        if (initializingInsertionMode)
        {
            throw new Exception("DeleteMin() called too early.");
        }
        #endif
        EventPriorityQueueNode node = m_pq.DeleteMin();
        EventBase simEvent = node.m_event;

        if (!NonJobTypeEvent(simEvent))
        {
            m_jobEventsCount--;
        }

        return simEvent;
    }

    internal void Validate()
    {
        m_pq.Validate();
    }

    /// <summary>
    /// For debugging purposes. Get a copy of the elements array.
    /// </summary>
    /// <returns></returns>
    private string[] EventsToString()
    {
        string[] eventsToString = m_pq.GetElementsArray();
        return eventsToString;
    }

    public override string ToString()
    {
        return base.ToString() + " Contains " + Count + " elements.";
    }

    /// <summary>
    /// Whether the event set contains an event.
    /// </summary>
    /// <param name="a_e">The event to search for.</param>
    /// <returns>true if the set contains the event.</returns>
    internal bool Contains(EventBase a_e)
    {
        IEnumerable<EventPriorityQueueNode> etrn = m_pq;
        IEnumerator<EventPriorityQueueNode> etr = etrn.GetEnumerator();
        while (etr.MoveNext())
        {
            EventPriorityQueueNode n = etr.Current;
            if (n != null)
            {
                if (ReferenceEquals(n.m_event, a_e))
                {
                    return true;
                }
            }
        }

        return false;
    }
}