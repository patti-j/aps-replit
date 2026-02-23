using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

public class ProductionUnitsCleanoutTriggerTableUpdateT : ProductionUnitsCleanoutTriggerTableIdBaseT
{
    #region IPTSerializable Members
    public ProductionUnitsCleanoutTriggerTableUpdateT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            m_ProductionUnitsCleanoutTriggerTable = new ProductionUnitsCleanoutTriggerTable(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_ProductionUnitsCleanoutTriggerTable.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1083;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ProductionUnitsCleanoutTriggerTableUpdateT() { }

    public ProductionUnitsCleanoutTriggerTableUpdateT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId, tableId) { }

    private ProductionUnitsCleanoutTriggerTable m_ProductionUnitsCleanoutTriggerTable = new ();

    public ProductionUnitsCleanoutTriggerTable ProductionUnitsCleanoutTriggerTable
    {
        get => m_ProductionUnitsCleanoutTriggerTable;
        set => m_ProductionUnitsCleanoutTriggerTable = value;
    }
}