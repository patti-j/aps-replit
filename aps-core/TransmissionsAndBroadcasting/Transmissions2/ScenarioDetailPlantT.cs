using PT.APSCommon;
using PT.ERPTransmissions;

namespace PT.Transmissions2;

public class ScenarioDetailPlantT : PlantT, IScenarioIdBaseT, IPTSerializable
{
    private readonly BaseId scenarioId;

    public ScenarioDetailPlantT() { }

    public ScenarioDetailPlantT(BaseId aScenarioId)
    {
        scenarioId = aScenarioId;
    }

    #region IScenarioIdBaseT Members
    public BaseId ScenarioId => scenarioId;
    #endregion

    #region IPTSerializable Members
    public ScenarioDetailPlantT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            scenarioId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        scenarioId.Serialize(writer);
    }

    public new const int UNIQUE_ID = 679;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public override string Description => "Plant Updated";
}