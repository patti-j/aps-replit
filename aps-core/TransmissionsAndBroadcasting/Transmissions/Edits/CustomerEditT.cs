using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public class CustomerEditT : ScenarioIdBaseT, IPTSerializable, IEnumerable<CustomerEdit>
{
    #region PT Serialization
    private readonly List<CustomerEdit> m_customerEdits = new ();
    public static int UNIQUE_ID => 1050;

    public CustomerEditT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                CustomerEdit node = new (a_reader);
                m_customerEdits.Add(node);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_customerEdits);
    }

    public override int UniqueId => UNIQUE_ID;

    public CustomerEditT() { }
    public CustomerEditT(BaseId a_scenarioId) : base(a_scenarioId) { }
    public CustomerEdit this[int i] => m_customerEdits[i];

    public void Validate()
    {
        foreach (CustomerEdit cusomerEdit in m_customerEdits)
        {
            cusomerEdit.Validate();
        }
    }

    public override string Description => string.Format("Customers updated ({0})".Localize(), m_customerEdits.Count);

    public IEnumerator<CustomerEdit> GetEnumerator()
    {
        return m_customerEdits.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(CustomerEdit a_pts)
    {
        m_customerEdits.Add(a_pts);
    }
    #endregion
}

public class CustomerEdit : PTObjectBaseEdit, IPTSerializable
{
    public const int UNIQUE_ID = 793;

    public CustomerEdit(IReader a_reader)
        : base(a_reader)
    {
        #region 12000
        if (a_reader.VersionNumber >= 12000)
        {
            m_setBools = new BoolVector32(a_reader);
            a_reader.Read(out m_abcCode);
            a_reader.Read(out m_group);
            a_reader.Read(out m_priority);
            a_reader.Read(out m_region);
            a_reader.Read(out m_colorCode);
            a_reader.Read(out int val);
            m_customerType = (CustomerDefs.ECustomerType)val;
        }
        #endregion
    }

    public CustomerEdit(BaseId a_cusId)
    {
        Id = a_cusId;
        m_externalId = null; //Clear other id without triggering IsSet
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_setBools.Serialize(a_writer);
        a_writer.Write(m_abcCode);
        a_writer.Write(m_group);
        a_writer.Write(m_priority);
        a_writer.Write(m_region);
        a_writer.Write(m_colorCode);
        a_writer.Write((int)m_customerType);
    }

    public int UniqueId => UNIQUE_ID;

    #region Properties
    public override bool HasEdits => base.HasEdits || m_setBools.AnyFlagsSet;

    private BoolVector32 m_setBools;

    private const short c_abcCodeSetIdx = 0;
    private const short c_groupCodeSetIdx = 1;
    private const short c_latePenaltyCostSetIdx = 2;
    private const short c_prioritySetIdx = 3;
    private const short c_regionSetIdx = 4;
    private const short c_colorCodeSetIdx = 5;
    private const short c_primaryContactSetIdx = 6;
    private const short c_primaryContractEmailSetIdx = 7;
    private const short c_nameSetIdx = 8;
    private const short c_externalIdSetIdx = 9;
    private const short c_reputationSetIdx = 10;
    private const short c_reputationMinSetIdx = 11;
    private const short c_reputationMaxSetIdx = 12;
    private const short c_customerTypeSetIdx = 13;

    public bool AbcCodeSet => m_setBools[c_abcCodeSetIdx];
    public bool GroupCodeSet => m_setBools[c_groupCodeSetIdx];
    public bool PrioritySet => m_setBools[c_prioritySetIdx];
    public bool RegionSet => m_setBools[c_regionSetIdx];
    public bool ColorCodeSet => m_setBools[c_colorCodeSetIdx];

    public bool PrimaryContactSet => m_setBools[c_primaryContactSetIdx];

    //public bool NameIsSet => m_setBools[c_nameSetIdx];
    //public bool ExternalIdSet => m_setBools[c_externalIdSetIdx];
    public bool ReputationSet => m_setBools[c_reputationSetIdx];
    public bool ReputationMinSet => m_setBools[c_reputationMinSetIdx];
    public bool ReputationMaxSet => m_setBools[c_reputationMaxSetIdx];
    public bool CustomerTypeSet => m_setBools[c_customerTypeSetIdx];
    public bool LatePenaltyCostSet => m_setBools[c_latePenaltyCostSetIdx];
    public bool PrimaryContractEmailSet => m_setBools[c_primaryContractEmailSetIdx];

    private string m_abcCode = "";

    /// <summary>
    /// Indicates the relative customer importance in terms of ABC value for the customer from the ERP system.
    /// </summary>
    public string AbcCode
    {
        get => m_abcCode;
        set
        {
            m_abcCode = value;
            m_setBools[c_abcCodeSetIdx] = true;
        }
    }

    private string m_group = "";

    /// <summary>
    /// Used to sort/group customers.
    /// </summary>
    public string GroupCode
    {
        get => m_group;
        set
        {
            m_group = value;
            m_setBools[c_groupCodeSetIdx] = true;
        }
    }

    //decimal m_latePenaltyCost = 0;
    ///// <summary>
    ///// Specifies the cost (either actual or estimated) per day of finishing this customer's orders late.  Can be used for comparing schedules and in simulation rules. 
    ///// </summary>
    //public decimal LatePenaltyCost
    //{
    //    get { return m_latePenaltyCost; }
    //    set
    //    {
    //        m_latePenaltyCost = value;
    //        m_boolsPropertiesSet[c_latePenaltyCostSetIdx] = true;
    //    }
    //}

    private int m_priority;

    /// <summary>
    /// Priority associated with this customer.  Can be used for prioritizing work by Customer.
    /// </summary>
    public int Priority
    {
        get => m_priority;
        set
        {
            m_priority = value;
            m_setBools[c_prioritySetIdx] = true;
        }
    }

    private string m_region;

    /// <summary>
    /// Can be used to visually group customers by geographic region.
    /// </summary>
    public string Region
    {
        get => m_region;
        set
        {
            m_region = value;
            m_setBools[c_regionSetIdx] = true;
        }
    }

    private System.Drawing.Color m_colorCode;

    public System.Drawing.Color ColorCode
    {
        get => m_colorCode;
        set
        {
            m_colorCode = value;
            m_setBools[c_colorCodeSetIdx] = true;
        }
    }

    //string m_primaryContact;
    //public string PrimaryContact
    //{
    //    get { return m_primaryContact; }
    //    set
    //    {
    //        m_primaryContact = value;
    //        m_boolsPropertiesSet[c_primaryContactSetIdx] = true;
    //    }
    //}

    //string m_primaryContactEmail;
    //public string PrimaryContactEmail
    //{
    //    get { return m_primaryContactEmail; }
    //    set
    //    {
    //        m_primaryContactEmail = value;
    //        m_boolsPropertiesSet[c_primaryContractEmailSetIdx] = true;
    //    }
    //}

    //decimal m_reputation;
    //public decimal Reputation
    //{
    //    get { return m_reputation; }
    //    set
    //    {
    //        m_reputation = value;
    //        m_boolsPropertiesSet[c_reputationSetIdx] = true;
    //    }
    //}

    //int m_reputationMin;
    //public int ReputationMin
    //{
    //    get { return m_reputationMin; }
    //    set
    //    {
    //        m_reputationMin = value;
    //        m_boolsPropertiesSet[c_reputationMinSetIdx] = true;
    //    }
    //}

    //int m_reputationMax;
    //public int ReputationMax
    //{
    //    get { return m_reputationMax; }
    //    set
    //    {
    //        m_reputationMax = value;
    //        m_boolsPropertiesSet[c_reputationMaxSetIdx] = true;
    //    }
    //}

    private CustomerDefs.ECustomerType m_customerType = CustomerDefs.ECustomerType.Customer;

    public CustomerDefs.ECustomerType CustomerType
    {
        get => m_customerType;
        set
        {
            m_customerType = value;
            m_setBools[c_customerTypeSetIdx] = true;
        }
    }
    #endregion

    public void Validate() { }
}