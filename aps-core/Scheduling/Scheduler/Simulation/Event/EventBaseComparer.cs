using PT.Common.Collections;
using PT.Scheduler.Simulation.Events;

namespace PT.Scheduler;

internal class EventBaseComparer : IPTCollectionsComparer<EventBase>
{
    public bool EqualTo(EventBase a_n1, EventBase a_n2)
    {
        return a_n1.Time == a_n2.Time;
    }

    public bool GreaterThan(EventBase a_n1, EventBase a_n2)
    {
        return a_n1.Time > a_n2.Time;
    }

    public bool GreaterThanOrEqual(EventBase a_n1, EventBase a_n2)
    {
        return a_n1.Time >= a_n2.Time;
    }

    public bool LessThan(EventBase a_n1, EventBase a_n2)
    {
        return a_n1.Time < a_n2.Time;
    }

    public bool LessThanOrEqual(EventBase a_n1, EventBase a_n2)
    {
        return a_n1.Time <= a_n2.Time;
    }

    public bool NotEqualTo(EventBase a_n1, EventBase a_n2)
    {
        throw new NotImplementedException();
    }
}