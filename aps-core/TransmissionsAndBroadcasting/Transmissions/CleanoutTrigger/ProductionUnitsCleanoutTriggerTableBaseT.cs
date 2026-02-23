using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

public class ProductionUnitsCleanoutTriggerTableBaseT : ScenarioIdBaseT
{
    #region IPTSerializable Members
    public ProductionUnitsCleanoutTriggerTableBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12305) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public const int UNIQUE_ID = 1077;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ProductionUnitsCleanoutTriggerTableBaseT() { }

    public ProductionUnitsCleanoutTriggerTableBaseT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "Production Units Cleanout Trigger Table updated";
}