using PT.APSCommon;

namespace PT.Transmissions;

public class SalesOrderLineEdit : PTObjectBaseEdit, IPTSerializable
{
    #region PT Serialization
    public SalesOrderLineEdit(IReader a_reader)
        : base(a_reader)
    {
        #region 12000
        if (a_reader.VersionNumber >= 12000)
        {
            m_setBools = new BoolVector32(a_reader);

            SalesOrderLineId = new BaseId(a_reader);
            SalesOrderId = new BaseId(a_reader);

            a_reader.Read(out m_lineNumber);
            a_reader.Read(out m_unitPrice);
            a_reader.Read(out m_itemExternalId);

            a_reader.Read(out int lineDistCount);
            for (int i = 0; i < lineDistCount; i++)
            {
                BaseId lineDistId = new (a_reader);
                m_lineDistributions.Add(lineDistId, new SalesOrderLineDistributionEdit(a_reader));
            }
        }
        #endregion
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_setBools.Serialize(a_writer);

        SalesOrderLineId.Serialize(a_writer);
        SalesOrderId.Serialize(a_writer);

        a_writer.Write(m_lineNumber);
        a_writer.Write(m_unitPrice);
        a_writer.Write(m_itemExternalId);

        a_writer.Write(m_lineDistributions.Count);
        foreach (KeyValuePair<BaseId, SalesOrderLineDistributionEdit> distEdit in m_lineDistributions)
        {
            distEdit.Key.Serialize(a_writer);
            distEdit.Value.Serialize(a_writer);
        }
    }

    public new int UniqueId => 1046;
    #endregion

    public SalesOrderLineEdit(BaseId a_salesOrderId, BaseId a_soLineId)
    {
        SalesOrderLineId = a_soLineId;
        SalesOrderId = a_salesOrderId;
        m_externalId = null; //Clear other id without triggering IsSet
    }

    internal void Validate()
    {
        //Sales order line validation
        if (LineNumberSet && string.IsNullOrEmpty(LineNumber))
        {
            throw new PTValidationException("2118");
        }

        if (ItemExternalIdSet && string.IsNullOrWhiteSpace(ItemExternalId))
        {
            throw new PTValidationException("2119");
        }
    }

    public override bool HasEdits => base.HasEdits || m_setBools.AnyFlagsSet;

    #region Shared Properties
    public readonly BaseId SalesOrderLineId;
    public readonly BaseId SalesOrderId;

    private BoolVector32 m_setBools;
    private const int c_itemExternalIdSetIdx = 0;
    private const int c_lineNumberSetIdx = 1;
    private const int c_unitPriceSetIdx = 2;

    private readonly Dictionary<BaseId, SalesOrderLineDistributionEdit> m_lineDistributions = new ();

    public Dictionary<BaseId, SalesOrderLineDistributionEdit> LineDistributions => m_lineDistributions;

    private string m_itemExternalId;

    /// <summary>
    /// The item being ordered.
    /// </summary>
    public string ItemExternalId
    {
        get => m_itemExternalId;
        set
        {
            m_itemExternalId = value;
            m_setBools[c_itemExternalIdSetIdx] = true;
        }
    }

    public bool ItemExternalIdSet => m_setBools[c_itemExternalIdSetIdx];

    private string m_lineNumber;

    /// <summary>
    /// The identifier for a line item within the Sales Order.
    /// </summary>
    public string LineNumber
    {
        get => m_lineNumber;
        set
        {
            m_lineNumber = value;
            m_setBools[c_lineNumberSetIdx] = true;
        }
    }

    public bool LineNumberSet => m_setBools[c_lineNumberSetIdx];

    private decimal m_unitPrice;

    /// <summary>
    /// The sale price per unit.  This can be used to maximize sales revenue.
    /// </summary>
    public decimal UnitPrice
    {
        get => m_unitPrice;
        set
        {
            m_unitPrice = value;
            m_setBools[c_unitPriceSetIdx] = true;
        }
    }

    public bool UnitPriceSet => m_setBools[c_unitPriceSetIdx];
    #endregion
}