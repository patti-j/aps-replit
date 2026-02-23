using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

/// <summary>
/// Create a new table.
/// </summary>
public class ProductionUnitsCleanoutTriggerTableNewT : ProductionUnitsCleanoutTriggerTableBaseT
{
    #region IPTSerializable Members
    public ProductionUnitsCleanoutTriggerTableNewT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12305)
        {
            cleanoutTriggerTable = new ProductionUnitsCleanoutTriggerTable(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        cleanoutTriggerTable.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1081;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ProductionUnitsCleanoutTriggerTableNewT() { }

    public ProductionUnitsCleanoutTriggerTableNewT(BaseId scenarioId)
        : base(scenarioId) { }

    private ProductionUnitsCleanoutTriggerTable cleanoutTriggerTable = new ();

    public ProductionUnitsCleanoutTriggerTable CleanoutTriggerTable
    {
        get => cleanoutTriggerTable;
        set => cleanoutTriggerTable = value;
    }

    public override string Description => "Production Units Cleanout Trigger Table created";
}