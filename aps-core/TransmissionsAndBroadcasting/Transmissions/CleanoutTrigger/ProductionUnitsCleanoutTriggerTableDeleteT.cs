using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

public class ProductionUnitsCleanoutTriggerTableDeleteT : ProductionUnitsCleanoutTriggerTableIdBaseT
{
    #region IPTSerializable Members
    public ProductionUnitsCleanoutTriggerTableDeleteT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1082;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ProductionUnitsCleanoutTriggerTableDeleteT() { }

    public ProductionUnitsCleanoutTriggerTableDeleteT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId, tableId) { }

    public override string Description => "Production Units Cleanout Trigger Table deleted";
}