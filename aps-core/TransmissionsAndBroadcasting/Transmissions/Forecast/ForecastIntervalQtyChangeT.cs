using PT.APSCommon;

namespace PT.Transmissions.Forecast;

/// <summary>
/// Change the total qty for a particular inventory's Forecasts in a defined interval.
/// </summary>
public class ForecastIntervalQtyChangeT : ForecastBaseT, IPTSerializable
{
    public override string Description => "Forecast Quantity updated";

    public new const int UNIQUE_ID = 700;

    #region IPTSerializable Members
    public ForecastIntervalQtyChangeT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            itemId = new BaseId(reader);
            warehouseId = new BaseId(reader);
            reader.Read(out oldQty);
            reader.Read(out newQty);
            reader.Read(out intervalStartInclusive);
            reader.Read(out intervalEndInclusive);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        itemId.Serialize(writer);
        warehouseId.Serialize(writer);
        writer.Write(oldQty);
        writer.Write(newQty);
        writer.Write(intervalStartInclusive);
        writer.Write(intervalEndInclusive);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ForecastIntervalQtyChangeT() { }

    public ForecastIntervalQtyChangeT(BaseId aScenarioId, BaseId aItemId, BaseId aWarehouseId, decimal aOldQty, decimal aNewQty, DateTime aIntervalStartInclusive, DateTime aIntervalEndInclusive) : base(aScenarioId)
    {
        itemId = aItemId;
        warehouseId = aWarehouseId;
        oldQty = aOldQty;
        newQty = aNewQty;
        intervalStartInclusive = aIntervalStartInclusive;
        intervalEndInclusive = aIntervalEndInclusive;
    }

    private readonly BaseId itemId;

    public BaseId ItemId => itemId;

    private readonly BaseId warehouseId;

    public BaseId WarehouseId => warehouseId;

    private readonly decimal oldQty;

    public decimal OldQty => oldQty;

    private readonly decimal newQty;

    public decimal NewQty => newQty;

    private readonly DateTime intervalStartInclusive;

    public DateTime IntervalStartInclusive => intervalStartInclusive;

    private readonly DateTime intervalEndInclusive;

    public DateTime IntervalEndInclusive => intervalEndInclusive;
}