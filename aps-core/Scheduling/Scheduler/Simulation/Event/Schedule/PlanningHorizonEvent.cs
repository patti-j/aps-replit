namespace PT.Scheduler.Simulation.Events;

internal class PlanningHorizonEvent : EventBase
{
    internal PlanningHorizonEvent(long a_time)
        : base(a_time) { }

    internal const int UNIQUE_ID = 30;

    internal override int UniqueId => UNIQUE_ID;
}