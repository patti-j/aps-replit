using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

public class ProductionUnitsCleanoutTriggerTableCopyT : ProductionUnitsCleanoutTriggerTableIdBaseT
{
    #region IPTSerializable Members
    public ProductionUnitsCleanoutTriggerTableCopyT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1090;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ProductionUnitsCleanoutTriggerTableCopyT() { }

    public ProductionUnitsCleanoutTriggerTableCopyT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId, tableId) { }

    public override string Description => "Production Units Cleanout Trigger Table copied";
}