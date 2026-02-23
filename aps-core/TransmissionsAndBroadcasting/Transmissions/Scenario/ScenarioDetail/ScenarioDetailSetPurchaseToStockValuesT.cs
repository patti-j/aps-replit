using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Set Commitments and Priorities for select Jobs.
/// </summary>
public class ScenarioDetailSetPurchaseToStockValuesT : PurchaseToStockBaseT, IPTSerializable
{
    public new static readonly int UNIQUE_ID = 830;

    #region IPTSerializable Members
    public ScenarioDetailSetPurchaseToStockValuesT(IReader reader)
        : base(reader)
    {
        #region 654
        if (reader.VersionNumber >= 654)
        {
            m_poIds = new BaseIdList(reader);
            m_bools = new BoolVector32(reader);
            m_boolsSetValues = new BoolVector32(reader);
            if (QtySet)
            {
                reader.Read(out m_qty);
            }
        }
        #endregion 645
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_poIds.Serialize(writer);
        m_bools.Serialize(writer);
        m_boolsSetValues.Serialize(writer);
        if (QtySet)
        {
            writer.Write(m_qty);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private BoolVector32 m_bools;
    private BoolVector32 m_boolsSetValues;

    //Set Values
    private const short c_closedIdx = 0;
    private const short c_receivedIdx = 1;
    private const short c_qtyIdx = 2;
    private const short c_deleteIdx = 3;

    public ScenarioDetailSetPurchaseToStockValuesT() { }

    /// <summary>
    /// Set Commitments and Priorities for select Jobs.
    /// </summary>
    public ScenarioDetailSetPurchaseToStockValuesT(BaseId a_scenarioId, BaseIdList a_pos)
        : base(a_scenarioId)
    {
        m_poIds = a_pos;
    }

    private readonly BaseIdList m_poIds;

    public BaseIdList Purchases => m_poIds;

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

    public bool Received
    {
        get => m_bools[c_receivedIdx];
        set
        {
            m_bools[c_receivedIdx] = value;
            m_boolsSetValues[c_receivedIdx] = true;
        }
    }

    public bool ReceiveSet => m_boolsSetValues[c_receivedIdx];

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

    public override string Description => "Purchase Orders modified";
}