using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Summary description for a Transmission for moving PurchaseToStocks.
/// </summary>
public class PurchaseToStockMoveT : PurchaseToStockIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 544;

    #region IPTSerializable Members
    public PurchaseToStockMoveT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out newStart);
            reader.Read(out moveToDock);
            reader.Read(out newDockNbr);

            newWarehouseId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(newStart);
        writer.Write(moveToDock);
        writer.Write(newDockNbr);

        newWarehouseId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public PurchaseToStockMoveT() { }

    public PurchaseToStockMoveT(BaseId scenarioId, BaseId ptsId, DateTime newStart, bool moveToDock, int newDockNbr, BaseId newWarehouseId)
        : base(scenarioId, ptsId)
    {
        this.newStart = newStart.Ticks;
        this.moveToDock = moveToDock;
        this.newDockNbr = newDockNbr;
        this.newWarehouseId = newWarehouseId;

        ReportAsEvent = false; //we have a special event to handle move failures
    }

    public long newStart;
    public bool moveToDock;
    public int newDockNbr;
    public BaseId newWarehouseId;

    public override string Description => "Purchase Order updated";
}