namespace PT.Scheduler.Simulation;

public interface IRequiredSpan
{
    long TimeSpanTicks { get; }

    bool Overrun { get; }

    decimal CapacityCost { get; }
}