using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Reverting a PurchaseToStock back under control of the ERP.
/// </summary>
public class PurchaseToStockRevertT : PurchaseToStockIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 547;

    #region IPTSerializable Members
    public PurchaseToStockRevertT(IReader reader)
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

    public PurchaseToStockRevertT() { }

    public PurchaseToStockRevertT(BaseId scenarioId, BaseId ptsId)
        : base(scenarioId, ptsId) { }

    public override string Description => "Purchase Order updated";
}