using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all internal PurchaseToStock related transmissions.
/// </summary>
public abstract class PurchaseToStockBaseT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 546;

    #region IPTSerializable Members
    public PurchaseToStockBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected PurchaseToStockBaseT() { }

    protected PurchaseToStockBaseT(BaseId scenarioId)
        : base(scenarioId) { }
}

/// <summary>
/// Base object for all internal PurchaseToStock related transmissions.
/// </summary>
public abstract class PurchaseToStockIdBaseT : PurchaseToStockBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 545;

    #region IPTSerializable Members
    public PurchaseToStockIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            purchaseToStockId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        purchaseToStockId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected PurchaseToStockIdBaseT() { }

    protected PurchaseToStockIdBaseT(BaseId scenarioId, BaseId purchaseToStockId)
        : base(scenarioId)
    {
        this.purchaseToStockId = purchaseToStockId;
    }

    public BaseId purchaseToStockId;
}