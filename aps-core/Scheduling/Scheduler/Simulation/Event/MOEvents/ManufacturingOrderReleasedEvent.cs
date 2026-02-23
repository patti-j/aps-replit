namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Used to constrain the start of an MO. Things that constrain an MO include the MO release date and the ready times of completed predecessor MOs.
/// </summary>
internal class ManufacturingOrderReleasedEvent : ManufacturingOrderEvent
{
    internal ManufacturingOrderReleasedEvent(long a_time, ManufacturingOrder a_manufacturingOrder, ManufacturingOrder.EffectiveReleaseDateType a_releaseType)
        : base(a_time, a_manufacturingOrder)
    {
        m_releaseType = a_releaseType;
    }

    private readonly ManufacturingOrder.EffectiveReleaseDateType m_releaseType;

    internal ManufacturingOrder.EffectiveReleaseDateType ReleaseType => m_releaseType;

    public override string ToString()
    {
        return string.Format("{0}: {1}: {2}", base.ToString(), ManufacturingOrder, m_releaseType.ToString());
    }

    internal const int UNIQUE_ID = 19;

    internal override int UniqueId => UNIQUE_ID;
}