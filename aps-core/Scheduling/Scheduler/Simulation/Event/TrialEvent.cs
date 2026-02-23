namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// If this is a trial version this is used to skip days. For instance don't allow
/// something to start anytime Thursday, Friday, or Sunday.
/// So any attempt to schedule on Thurday or Friday results in an event Saturday at midnight when
/// scheduling can being again. Or if attempting to schedule Sunday an event of this type occurs Monday
/// at midnight when scheduling can begin again.
/// </summary>
internal class TrialEvent : EventBase
{
    internal TrialEvent(long a_time)
        : base(a_time) { }

    internal const int UNIQUE_ID = 36;

    internal override int UniqueId => UNIQUE_ID;
}