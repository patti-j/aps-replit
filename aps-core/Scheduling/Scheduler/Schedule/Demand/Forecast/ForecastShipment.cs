using System.ComponentModel;

using PT.APSCommon;

namespace PT.Scheduler.Demand;

/// <summary>
/// Specifies a qty of an Item to be shipped at a particular datetime.
/// </summary>
public class ForecastShipment : BaseIdObject
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 629;

    public ForecastShipment(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12049)
        {
            reader.Read(out requiredQty);
            reader.Read(out requiredDate);
            reader.Read(out m_warehouseExternalId);
        }
        else if (reader.VersionNumber >= 12000)
        {
            reader.Read(out requiredQty);
            reader.Read(out requiredDate);
        }
        else if (reader.VersionNumber >= 754)
        {
            reader.Read(out requiredQty);
            reader.Read(out requiredDate);
            reader.Read(out m_warehouseExternalId);
        }
        else if (reader.VersionNumber >= 233)
        {
            reader.Read(out requiredQty);
            reader.Read(out requiredDate);
        }

        #region old
        else
        {
            reader.Read(out requiredQty);

            DateTime tmp;
            reader.Read(out tmp);
            requiredDate = tmp.Ticks;
        }
        #endregion

        #if TEST
            CompareDesyncResults();
        #endif
    }

    public ForecastShipment(IReader reader, object OLD_VERSION_COMPATIBILITY)
        : base(BaseId.NULL_ID)
    {
        if (reader.VersionNumber >= 12049)
        {
            reader.Read(out requiredQty);
            reader.Read(out requiredDate);
            reader.Read(out m_warehouseExternalId);
        }
        else if (reader.VersionNumber >= 12000)
        {
            reader.Read(out requiredQty);
            reader.Read(out requiredDate);
        }
        else if (reader.VersionNumber >= 754)
        {
            reader.Read(out requiredQty);
            reader.Read(out requiredDate);
            reader.Read(out m_warehouseExternalId);
        }
        else if (reader.VersionNumber >= 233)
        {
            reader.Read(out requiredQty);
            reader.Read(out requiredDate);
        }

        #region old
        else
        {
            reader.Read(out requiredQty);

            DateTime tmp;
            reader.Read(out tmp);
            requiredDate = tmp.Ticks;
        }
        #endregion

        #if TEST
            CompareDesyncResults();
        #endif
    }

    internal void RestoreReferences(Forecast aFC)
    {
        _forecast = aFC;
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(requiredQty);
        writer.Write(requiredDate);
        writer.Write(m_warehouseExternalId);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion IPTSerializable

    internal ForecastShipment(BaseId aId, DateTime aRequiredDate, decimal aRequiredQty, string a_warehouseExternalId, Forecast aFC)
        : base(aId)
    {
        if (aFC == null)
        {
            throw new Exception("Null Forecast.");
        }

        _forecast = aFC;

        RequiredDate = aRequiredDate;
        RequiredQty = aRequiredQty;
        m_warehouseExternalId = a_warehouseExternalId;
    }

    private string m_warehouseExternalId;

    public string WarehouseExternalId
    {
        get => m_warehouseExternalId;
        private set => m_warehouseExternalId = value;
    }

    private Forecast _forecast;

    public Forecast Forecast => _forecast;

    private decimal requiredQty;

    /// <summary>
    /// The qty demanded.
    /// </summary>
    public decimal RequiredQty
    {
        get => requiredQty;
        internal set => requiredQty = value;
    }

    private long requiredDate;

    /// <summary>
    /// The date/time when the material must be in stock to satisfy the demand.
    /// </summary>
    public DateTime RequiredDate
    {
        get => new (requiredDate);
        private set => requiredDate = value.Ticks;
    }

    internal long RequireDateTicks => requiredDate;

    #region Consumption
    private decimal? m_consumedQtyCached;

    // sales orders that are consuming this ForecastShipment and the quantity consumed.
    public List<SalesOrderPart> ConsumingSalesOrderParts { get; private set; } = new ();

    internal void ClearConsumption()
    {
        m_consumedQtyCached = null;
        ConsumingSalesOrderParts.Clear();
    }

    /// <summary>
    /// Consume a_qty of this ForecastShipment by a_sod
    /// </summary>
    internal void ConsumeSalesOrder(SalesOrderLineDistribution a_sod, decimal a_qty)
    {
        m_consumedQtyCached = null;
        ConsumingSalesOrderParts.Add(new SalesOrderPart(a_sod, a_qty));
    }

    /// <summary>
    /// returns the sum of all consuming sales order parts
    /// </summary>
    public decimal GetConsumedQty()
    {
        if (m_consumedQtyCached != null)
        {
            return m_consumedQtyCached.Value;
        }

        if (ConsumingSalesOrderParts.Count == 0)
        {
            return 0;
        }

        m_consumedQtyCached = ConsumingSalesOrderParts.Sum(sodp => { return sodp.Qty; });
        return m_consumedQtyCached.Value;
    }

    /// <summary>
    /// returns the qty not yet consumed.
    /// </summary>
    public decimal GetUnconsumedQty()
    {
        return RequiredQty - GetConsumedQty();
    }
    #endregion

    public class SalesOrderPart
    {
        public SalesOrderLineDistribution SalesOrderDistribution { get; private set; }

        public decimal Qty { get; private set; }

        internal SalesOrderPart(SalesOrderLineDistribution a_sod, decimal a_qty)
        {
            SalesOrderDistribution = a_sod;
            Qty = a_qty;
        }
    }

    public override string ToString()
    {
        string s = string.Format("Date{0}; Qty:{1}", DateTimeHelper.ToLocalTimeFromUTCTicks(RequiredDate.Ticks), RequiredQty);
        return s;
    }
}