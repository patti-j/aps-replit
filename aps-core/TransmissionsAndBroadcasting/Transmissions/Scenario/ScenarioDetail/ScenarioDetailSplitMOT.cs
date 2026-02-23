using PT.APSCommon;

namespace PT.Transmissions;

public class ScenarioDetailSplitMOT : ScenarioDetailMOBaseT
{
    #region IPTSerializable Members
    public ScenarioDetailSplitMOT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_splitQty);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(m_splitQty);
    }

    public const int UNIQUE_ID = 580;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailSplitMOT() { }

    public ScenarioDetailSplitMOT(BaseId a_scenarioId, BaseId a_jobId, BaseId a_moId, decimal a_splitQty)
        : base(a_scenarioId, a_jobId, a_moId)
    {
        SplitQty = a_splitQty;
    }

    private decimal m_splitQty;

    public decimal SplitQty
    {
        get => m_splitQty;
        private set => m_splitQty = value;
    }

    public override string Description => "ManufacturingOrder split";
}