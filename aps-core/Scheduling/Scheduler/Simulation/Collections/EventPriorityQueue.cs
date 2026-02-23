using PT.Common.Collections;
using PT.Scheduler.Simulation.Events;

namespace PT.Scheduler;

/// <summary>
/// The purpose of this class is to give simulation events more than one field to sort on.
/// In addition to time,
/// </summary>
internal class EventPriorityQueueNode
{
    internal long m_time;
    internal long m_sequenceNbr;
    internal EventBase m_event;

    public override string ToString()
    {
        return string.Format("{0}; Sequence # {1}; Event {2}", m_time, m_sequenceNbr, m_event);
    }

    public string de()
    {
        return ToString();
    }
}

internal class EventPriorityQueueNodeComparer : IPTCollectionsComparer<EventPriorityQueueNode>
{
    bool IPTCollectionsComparer<EventPriorityQueueNode>.LessThan(EventPriorityQueueNode a_n1, EventPriorityQueueNode a_n2)
    {
        if (a_n1.m_time < a_n2.m_time)
        {
            return true;
        }

        if (a_n1.m_time > a_n2.m_time)
        {
            return false;
        }

        return a_n1.m_sequenceNbr < a_n2.m_sequenceNbr;
    }

    bool IPTCollectionsComparer<EventPriorityQueueNode>.LessThanOrEqual(EventPriorityQueueNode a_n1, EventPriorityQueueNode a_n2)
    {
        if (a_n1.m_time < a_n2.m_time)
        {
            return true;
        }

        if (a_n1.m_time > a_n2.m_time)
        {
            return false;
        }

        return a_n1.m_sequenceNbr <= a_n2.m_sequenceNbr;
    }

    bool IPTCollectionsComparer<EventPriorityQueueNode>.GreaterThan(EventPriorityQueueNode a_n1, EventPriorityQueueNode a_n2)
    {
        if (a_n1.m_time > a_n2.m_time)
        {
            return true;
        }

        if (a_n1.m_time < a_n2.m_time)
        {
            return false;
        }

        return a_n1.m_sequenceNbr > a_n2.m_sequenceNbr;
    }

    bool IPTCollectionsComparer<EventPriorityQueueNode>.GreaterThanOrEqual(EventPriorityQueueNode a_n1, EventPriorityQueueNode a_n2)
    {
        if (a_n1.m_time > a_n2.m_time)
        {
            return true;
        }

        if (a_n1.m_time < a_n2.m_time)
        {
            return false;
        }

        return a_n1.m_sequenceNbr >= a_n2.m_sequenceNbr;
    }

    bool IPTCollectionsComparer<EventPriorityQueueNode>.EqualTo(EventPriorityQueueNode a_n1, EventPriorityQueueNode a_n2)
    {
        return a_n1.m_time == a_n2.m_time && a_n1.m_sequenceNbr == a_n2.m_sequenceNbr;
    }

    bool IPTCollectionsComparer<EventPriorityQueueNode>.NotEqualTo(EventPriorityQueueNode a_n1, EventPriorityQueueNode a_n2)
    {
        return a_n1.m_time != a_n2.m_time || a_n1.m_sequenceNbr != a_n2.m_sequenceNbr;
    }
}