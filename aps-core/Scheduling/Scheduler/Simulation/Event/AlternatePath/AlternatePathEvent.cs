namespace PT.Scheduler.Simulation.Events;

internal abstract class AlternatePathEvent : EventBase
{
    internal AlternatePathEvent(long a_time, ManufacturingOrder a_mo, AlternatePath a_path)
        : base(a_time)
    {
        ManufacturingOrder = a_mo;
        AlternatePath = a_path;
    }

    internal ManufacturingOrder ManufacturingOrder { get; private set; }

    internal AlternatePath AlternatePath { get; private set; }

    public override string ToString()
    {
        return string.Format("{0}; MO: {1}; Path: {2}", base.ToString(), ManufacturingOrder, AlternatePath);
    }
}