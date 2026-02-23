namespace PT.Scheduler.Simulation;

internal class ResourceReservationResult
{
    internal ResourceReservationResult(ResourceReservation a_resRv, SchedulableResult a_schedulableResult)
    {
        ResRv = a_resRv;
        SchedulableResult = a_schedulableResult;
    }

    internal ResourceReservationResult(long a_nextPlausibleStartTicks)
    {
        NextPlausibleStartTicks = a_nextPlausibleStartTicks;
    }

    internal bool Reservation => ResRv != null;

    internal readonly ResourceReservation ResRv;
    internal readonly long NextPlausibleStartTicks;
    internal readonly SchedulableResult SchedulableResult;
    internal SchedulableInfo Si => SchedulableResult.m_si;
}