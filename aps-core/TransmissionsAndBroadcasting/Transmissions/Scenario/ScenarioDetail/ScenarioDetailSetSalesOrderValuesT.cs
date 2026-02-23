using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Set Commitments and Priorities for select Jobs.
/// </summary>
public class ScenarioDetailSetSalesOrderValuesT : ScenarioIdBaseT, IPTSerializable
{
    public static readonly int UNIQUE_ID = 831;

    #region IPTSerializable Members
    public ScenarioDetailSetSalesOrderValuesT(IReader reader)
        : base(reader)
    {
        m_soIds = new List<BaseId>();
        m_distributionIds = new List<BaseId>();

        #region 654
        if (reader.VersionNumber >= 654)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_soIds.Add(new BaseId(reader));
            }

            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_distributionIds.Add(new BaseId(reader));
            }

            m_bools = new BoolVector32(reader);
            m_boolsSetValues = new BoolVector32(reader);
            if (QtySet)
            {
                reader.Read(out m_qty);
            }

            m_warehouseOverride = new BaseId(reader);
        }
        #endregion 645
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_soIds.Count);
        foreach (BaseId baseId in m_soIds)
        {
            baseId.Serialize(writer);
        }

        writer.Write(m_distributionIds.Count);
        foreach (BaseId baseId in m_distributionIds)
        {
            baseId.Serialize(writer);
        }

        m_bools.Serialize(writer);
        m_boolsSetValues.Serialize(writer);
        if (QtySet)
        {
            writer.Write(m_qty);
        }

        m_warehouseOverride.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private BoolVector32 m_bools;
    private BoolVector32 m_boolsSetValues;

    //Set Values
    private const short c_closedIdx = 0;
    private const short c_shippedIdx = 1;
    private const short c_qtyIdx = 2;
    private const short c_deleteIdx = 3;

    public ScenarioDetailSetSalesOrderValuesT() { }

    /// <summary>
    /// Set Commitments and Priorities for select Jobs.
    /// </summary>
    public ScenarioDetailSetSalesOrderValuesT(BaseId a_scenarioId, List<BaseId> a_orderIds, List<BaseId> a_distributionIds)
        : base(a_scenarioId)
    {
        m_soIds = a_orderIds;
        m_distributionIds = a_distributionIds;
    }

    private readonly List<BaseId> m_soIds;

    public List<BaseId> SalesOrders => m_soIds;

    private readonly List<BaseId> m_distributionIds;

    public List<BaseId> Distributions => m_distributionIds;

    private BaseId m_warehouseOverride = BaseId.NULL_ID;

    public BaseId WarehouseOverride
    {
        get => m_warehouseOverride;
        set => m_warehouseOverride = value;
    }

    public bool Closed
    {
        get => m_bools[c_closedIdx];
        set
        {
            m_bools[c_closedIdx] = value;
            m_boolsSetValues[c_closedIdx] = true;
        }
    }

    public bool ClosedSet => m_boolsSetValues[c_closedIdx];

    public bool Delete
    {
        get => m_bools[c_deleteIdx];
        set
        {
            m_bools[c_deleteIdx] = value;
            m_boolsSetValues[c_deleteIdx] = true;
        }
    }

    public bool DeleteSet => m_boolsSetValues[c_deleteIdx];

    public bool Shipped
    {
        get => m_bools[c_shippedIdx];
        set
        {
            m_bools[c_shippedIdx] = value;
            m_boolsSetValues[c_shippedIdx] = true;
        }
    }

    public bool ShippedSet => m_boolsSetValues[c_shippedIdx];

    private decimal m_qty;

    public decimal Qty
    {
        get => m_qty;
        set
        {
            m_qty = value;
            m_boolsSetValues[c_qtyIdx] = true;
        }
    }

    public bool QtySet => m_boolsSetValues[c_qtyIdx];

    public override string Description => "Sales Orders modified";
}