using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Set Commitments and Priorities for select Jobs.
/// </summary>
public class ScenarioDetailMaterialUpdateT : MaterialIdBaseT, IPTSerializable
{
    public new static readonly int UNIQUE_ID = 834;

    #region IPTSerializable Members
    public ScenarioDetailMaterialUpdateT(IReader reader)
        : base(reader)
    {
        #region 654
        if (reader.VersionNumber >= 654)
        {
            m_bools = new BoolVector32(reader);
            m_boolsSetValues = new BoolVector32(reader);
            if (QtySet)
            {
                reader.Read(out m_issuedQty);
            }
        }
        #endregion 645
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_bools.Serialize(writer);
        m_boolsSetValues.Serialize(writer);
        if (QtySet)
        {
            writer.Write(m_issuedQty);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private BoolVector32 m_bools;
    private BoolVector32 m_boolsSetValues;

    //Set Values
    private const short c_qtySetIdx = 0;
    private const short c_issueFromInventoryIdx = 1;

    public ScenarioDetailMaterialUpdateT() { }

    /// <summary>
    /// Set Commitments and Priorities for select Jobs.
    /// </summary>
    public ScenarioDetailMaterialUpdateT(BaseId a_scenarioId, List<(BaseId a_jobId, BaseId a_moId, BaseId a_opId, BaseId a_materialId)> a_ids)
        : base(a_scenarioId, a_ids) { }

    private decimal m_issuedQty;

    public decimal IssuedQty
    {
        get => m_issuedQty;
        set
        {
            m_issuedQty = value;
            m_boolsSetValues[c_qtySetIdx] = true;
        }
    }

    public bool QtySet => m_boolsSetValues[c_qtySetIdx];

    public bool IssueFromInventory
    {
        get => m_bools[c_issueFromInventoryIdx];
        set
        {
            m_bools[c_issueFromInventoryIdx] = value;
            m_boolsSetValues[c_issueFromInventoryIdx] = true;
        }
    }

    public bool IssueFromInventorySet => m_boolsSetValues[c_issueFromInventoryIdx];

    public override string Description => "Materials Issued";
}