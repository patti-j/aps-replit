using PT.APSCommon;

namespace PT.Transmissions;

public class ScenarioDetailClearPastShortTermT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 800;

    #region IPTSerializable Members
    public ScenarioDetailClearPastShortTermT(IReader a_reader)
        : base(a_reader) { }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailClearPastShortTermT() { }

    public ScenarioDetailClearPastShortTermT(BaseId a_scenarioId)
        : base(a_scenarioId) { }

    public override string Description => "Scenario Cleared Past Short Term";
}