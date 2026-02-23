using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Signals the start of a new RuleSeek Simulation. Provides the ID of scenario to use.
/// </summary>
public class ScenarioRuleSeekStartT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 771;

    #region IPTSerializable Members
    public ScenarioRuleSeekStartT(IReader a_reader) : base(a_reader)
    {
        //if (a_reader.VersionNumber > 12009)
        //{
        //    a_reader.Read(out int Value);
        //    a_reader.Read(out int MaxSimulations);
        //    a_reader.Read(out int CpuLimitPercentageTotal);
        //    a_reader.Read(out int CpuLimitPercentageGalaxy);
        //    a_reader.Read(out double CpuProcessorsScalar);
        //    a_reader.Read(out double MemoryScalar);
        //    a_reader.Read(out int UpdateInterval);
        //    a_reader.Read(out int DispatchInterval);

        //    m_coPilotPerformanceValues = new CoPilotPerformanceValues()
        //    {
        //        Value = Value,
        //        MaxSimulations = MaxSimulations,
        //        CpuLimitPercentageTotal = CpuLimitPercentageTotal,
        //        CpuLimitPercentageGalaxy = CpuLimitPercentageGalaxy,
        //        CpuProcessorsScalar = CpuProcessorsScalar,
        //        MemoryScalar = MemoryScalar,
        //        UpdateInterval = UpdateInterval,
        //        DispatchInterval = DispatchInterval,
        //    };
        //}
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        //a_writer.Write(m_coPilotPerformanceValues.Value);
        //a_writer.Write(m_coPilotPerformanceValues.MaxSimulations);
        //a_writer.Write(m_coPilotPerformanceValues.CpuLimitPercentageTotal);
        //a_writer.Write(m_coPilotPerformanceValues.CpuLimitPercentageGalaxy);
        //a_writer.Write(m_coPilotPerformanceValues.CpuProcessorsScalar);
        //a_writer.Write(m_coPilotPerformanceValues.MemoryScalar);
        //a_writer.Write(m_coPilotPerformanceValues.UpdateInterval);
        //a_writer.Write(m_coPilotPerformanceValues.DispatchInterval);
    }

    public override int UniqueId => UNIQUE_ID;

    //private readonly CoPilotPerformanceValues m_coPilotPerformanceValues;
    //public CoPilotPerformanceValues CoPilotPerformanceValues => m_coPilotPerformanceValues;
    #endregion

    public ScenarioRuleSeekStartT() { }

    public ScenarioRuleSeekStartT(BaseId a_scenarioId)
        : base(a_scenarioId) { }

    public override string Description => "RuleSeek started";
}