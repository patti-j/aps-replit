namespace PT.Scheduler;

public class OperationCapacity : IPTSerializable
{
    internal OperationCapacity(long a_startTicks, long a_endTicks, long a_totalCapacityTicks, decimal a_efficiencyMultiplier, decimal a_cost, bool a_overtime)
    {
        StartTicks = a_startTicks;
        EndTicks = a_endTicks;
        TotalCapacityTicks = a_totalCapacityTicks;
        m_cost = a_cost;
        m_efficiencyMultiplier = a_efficiencyMultiplier;
        Overtime = a_overtime;
    }

    internal long StartTicks;
    internal long EndTicks;
    internal long TotalCapacityTicks;
    private readonly decimal m_cost;
    private readonly decimal m_efficiencyMultiplier = 1;
    private BoolVector32 m_bools;
    private const short c_overtimeIdx = 0;

    public DateTimeOffset StartDate => new DateTimeOffset(StartTicks, TimeSpan.Zero);
    public DateTimeOffset EndDate => new DateTimeOffset(EndTicks, TimeSpan.Zero);
    public TimeSpan TotalCapacity => new TimeSpan(TotalCapacityTicks);
    public decimal CapacityRatio => (TotalCapacityTicks * m_efficiencyMultiplier) / (EndTicks - StartTicks);
    public decimal Cost => m_cost;
    public TimeSpan Duration => TimeSpan.FromTicks(EndTicks - StartTicks);

    public bool Overtime
    {
        get => m_bools[c_overtimeIdx];
        private set => m_bools[c_overtimeIdx] = value;
    }

    internal OperationCapacity(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12541)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out StartTicks);
            a_reader.Read(out EndTicks);
            a_reader.Read(out TotalCapacityTicks);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_efficiencyMultiplier);
        } 
        else if (a_reader.VersionNumber >= 12518)
        {
            a_reader.Read(out StartTicks);
            a_reader.Read(out EndTicks);
            a_reader.Read(out TotalCapacityTicks);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_efficiencyMultiplier);
        }
        else if (a_reader.VersionNumber >= 12510)
        {
            a_reader.Read(out StartTicks);
            a_reader.Read(out EndTicks);
            a_reader.Read(out TotalCapacityTicks);
            a_reader.Read(out m_cost);
        }
        else if (a_reader.VersionNumber >= 12500)
        {
            a_reader.Read(out StartTicks);
            a_reader.Read(out EndTicks);
            a_reader.Read(out TotalCapacityTicks);
        }
        else if (a_reader.VersionNumber >= 12429)
        {
            a_reader.Read(out StartTicks);
            a_reader.Read(out EndTicks);
            a_reader.Read(out TotalCapacityTicks);
            a_reader.Read(out m_efficiencyMultiplier);
        }
        else if (a_reader.VersionNumber >= 12415)
        {
            a_reader.Read(out StartTicks);
            a_reader.Read(out EndTicks);
            a_reader.Read(out TotalCapacityTicks);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write(StartTicks);
        a_writer.Write(EndTicks);
        a_writer.Write(TotalCapacityTicks);
        a_writer.Write(m_cost);
        a_writer.Write(m_efficiencyMultiplier);
    }

    public int UniqueId => 1118;
}