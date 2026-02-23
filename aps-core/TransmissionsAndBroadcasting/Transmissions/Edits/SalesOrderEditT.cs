using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

public class SalesOrderEditT : ScenarioIdBaseT, IPTSerializable
{
    #region PT Serialization
    public static int UNIQUE_ID => 1044;

    public SalesOrderEditT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                SalesOrderEdit node = new (a_reader);
                m_soEdits.Add(node);
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                SalesOrderLineEdit node = new (a_reader);
                m_soLineEdits.Add(node);
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                SalesOrderLineDistributionEdit node = new (a_reader);
                m_soLineDistributionEdits.Add(node);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_soEdits);
        writer.Write(m_soLineEdits);
        writer.Write(m_soLineDistributionEdits);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public SalesOrderEditT() { }
    public SalesOrderEditT(BaseId a_scenarioId) : base(a_scenarioId) { }

    public void Validate()
    {
        HashSet<string> lineNumbersAlreadyAddedHash = new ();

        foreach (SalesOrderLineEdit soLineEdit in SalesOrderLineEdits)
        {
            soLineEdit.Validate();

            if (soLineEdit.LineNumberSet)
            {
                if (lineNumbersAlreadyAddedHash.Contains(soLineEdit.LineNumber))
                {
                    throw new PTValidationException("2117", new object[] { "", soLineEdit.LineNumber }); //TODO: Make a new error message
                }

                lineNumbersAlreadyAddedHash.Add(soLineEdit.LineNumber);
            }
        }
    }

    public override string Description => string.Format("Sales orders updated ({0})".Localize(), m_soEdits.Count);

    private readonly List<SalesOrderEdit> m_soEdits = new ();
    public List<SalesOrderEdit> SalesOrderEdits => m_soEdits;

    private readonly List<SalesOrderLineEdit> m_soLineEdits = new ();
    public List<SalesOrderLineEdit> SalesOrderLineEdits => m_soLineEdits;

    private readonly List<SalesOrderLineDistributionEdit> m_soLineDistributionEdits = new ();
    public List<SalesOrderLineDistributionEdit> SalesOrderLineDistributionEdits => m_soLineDistributionEdits;
}

public class SalesOrderEdit : PTObjectBaseEdit, IPTSerializable
{
    public BaseId SalesOrderId;

    #region PT Serialization
    public SalesOrderEdit(IReader a_reader)
        : base(a_reader)
    {
        #region 12000
        if (a_reader.VersionNumber >= 12000)
        {
            SalesOrderId = new BaseId(a_reader);

            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out m_customerExternalId);
            a_reader.Read(out m_estimate);
            a_reader.Read(out m_salesAmount);
            a_reader.Read(out m_salesOffice);
            a_reader.Read(out m_salesPerson);
            a_reader.Read(out m_planner);
            a_reader.Read(out m_project);
            a_reader.Read(out m_expirationDate);

            a_reader.Read(out int soLinesCount);
            for (int i = 0; i < soLinesCount; i++)
            {
                BaseId lineEditId = new (a_reader);
                SalesOrderLineEdit lineEdit = new (a_reader);
                m_salesOrderLines.Add(lineEditId, lineEdit);
            }
        }
        #endregion
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        SalesOrderId.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_setBools.Serialize(a_writer);

        a_writer.Write(m_customerExternalId);
        a_writer.Write(m_estimate);
        a_writer.Write(m_salesAmount);
        a_writer.Write(m_salesOffice);
        a_writer.Write(m_salesPerson);
        a_writer.Write(m_planner);
        a_writer.Write(m_project);
        a_writer.Write(m_expirationDate);

        a_writer.Write(m_salesOrderLines.Count);
        foreach (KeyValuePair<BaseId, SalesOrderLineEdit> lineEdit in m_salesOrderLines)
        {
            lineEdit.Key.Serialize(a_writer);
            lineEdit.Value.Serialize(a_writer);
        }
    }

    public override bool HasEdits => base.HasEdits || m_setBools.AnyFlagsSet;

    public new int UniqueId => 1045;
    #endregion

    public SalesOrderEdit(BaseId a_salesOrderId)
    {
        SalesOrderId = a_salesOrderId;
        m_externalId = null; //Clear other id without triggering IsSet
    }

    #region Shared Properties
    private readonly Dictionary<BaseId, SalesOrderLineEdit> m_salesOrderLines = new ();
    public Dictionary<BaseId, SalesOrderLineEdit> SalesOrderLineEdits => m_salesOrderLines;

    private BoolVector32 m_bools;
    private const int c_cancelledIdx = 0;
    private const int c_cancelAtExpireDateIdx = 1;

    private BoolVector32 m_setBools;
    private const int c_customerSetIdx = 0;
    private const int c_estimateSetIdx = 1;
    private const int c_salesAmountSetIdx = 2;
    private const int c_salesOfficeSetIdx = 3;
    private const int c_salesPersonSetIdx = 4;
    private const int c_plannerSetIdx = 5;
    private const int c_projectSetIdx = 6;
    private const int c_cancelledSetIdx = 7;
    private const int c_cancelAtExpirationDateIdx = 8;
    private const int c_expirationDateSetIdx = 9;

    private string m_customerExternalId;

    public string CustomerExternalId
    {
        get => m_customerExternalId;
        set
        {
            m_customerExternalId = value;
            m_setBools[c_customerSetIdx] = true;
        }
    }

    public bool CustomerSet => m_setBools[c_customerSetIdx];

    private bool m_estimate;

    /// <summary>
    /// If true then this is a quote, not a firm order.
    /// </summary>
    public bool Estimate
    {
        get => m_estimate;
        set
        {
            m_estimate = value;
            m_setBools[c_estimateSetIdx] = true;
        }
    }

    public bool EstimateSet => m_setBools[c_estimateSetIdx];

    private decimal m_salesAmount;

    public decimal SalesAmount
    {
        get => m_salesAmount;
        set
        {
            m_salesAmount = value;
            m_setBools[c_salesAmountSetIdx] = true;
        }
    }

    public bool SalesAmountSet => m_setBools[c_salesAmountSetIdx];

    private string m_salesOffice;

    /// <summary>
    /// Specifies the sales office or other physical location that created the demand.
    /// This has no effect on the Warehouse that satisfies the order.
    /// It is for reference only.
    /// </summary>
    public string SalesOffice
    {
        get => m_salesOffice;
        set
        {
            m_salesOffice = value;
            m_setBools[c_salesOfficeSetIdx] = true;
        }
    }

    public bool SalesOfficeSet => m_setBools[c_salesOfficeSetIdx];

    private string m_salesPerson;

    /// <summary>
    /// The employee in sales who is responsible for this demand.
    /// </summary>
    public string SalesPerson
    {
        get => m_salesPerson;
        set
        {
            m_salesPerson = value;
            m_setBools[c_salesPersonSetIdx] = true;
        }
    }

    public bool SalesPersonSet => m_setBools[c_salesPersonSetIdx];

    private string m_planner;

    /// <summary>
    /// The User responsible for planning this demand if planning by demand rather than by product or location.
    /// </summary>
    public string Planner
    {
        get => m_planner;
        set
        {
            m_planner = value;
            m_setBools[c_plannerSetIdx] = true;
        }
    }

    public bool PlannerSet => m_setBools[c_plannerSetIdx];

    private string m_project;

    /// <summary>
    /// Can be used for tracking multiple demands tied to one project.
    /// </summary>
    public string Project
    {
        get => m_project;
        set
        {
            m_project = value;
            m_setBools[c_projectSetIdx] = true;
        }
    }

    public bool ProjectSet => m_setBools[c_projectSetIdx];

    /// <summary>
    /// Whether the Sales Order has been cancelled either by setting this value explicitly or by the Expiration Date passsing.
    /// Once a Sales Order is cancelled, its demands are ignored.
    /// </summary>
    public bool Cancelled
    {
        get => m_bools[c_cancelledIdx];
        set
        {
            m_bools[c_cancelledIdx] = value;
            m_setBools[c_cancelledSetIdx] = true;
        }
    }

    public bool CancelledSet => m_setBools[c_cancelledSetIdx];

    /// <summary>
    /// If true then the Sales Order is marked as Cancelled when the PlanetTogether Clock passes the Expiration Date.
    /// </summary>
    public bool CancelAtExpirationDate
    {
        get => m_bools[c_cancelAtExpireDateIdx];
        set
        {
            m_bools[c_cancelAtExpireDateIdx] = value;
            m_setBools[c_cancelAtExpirationDateIdx] = true;
        }
    }

    public bool CancelAtExpirationDateSet => m_setBools[c_cancelAtExpirationDateIdx];

    private DateTime m_expirationDate = PTDateTime.MaxDateTime;

    /// <summary>
    /// If CancelAtExpirationDate is true then the Sales Order is marked as Cancelled when the PlanetTogether Clock passes this date.
    /// </summary>
    public DateTime ExpirationDate
    {
        get => m_expirationDate;
        set
        {
            m_expirationDate = value;
            m_setBools[c_expirationDateSetIdx] = true;
        }
    }

    public bool ExpirationDateSet => m_setBools[c_expirationDateSetIdx];
    #endregion Shared Properties
}