using System.ComponentModel;
using System.Drawing;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Database;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Company for whom product is destined.
/// </summary>
[Serializable]
public class Customer : BaseObject, ICloneable
{
    public new static readonly int UNIQUE_ID = 13;

    #region IPTSerializable Members
    public Customer(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out m_abcCode);
            a_reader.Read(out m_group);
            a_reader.Read(out m_priority);
            a_reader.Read(out m_colorCode);
            a_reader.Read(out m_region);
            a_reader.Read(out int value);
            m_customerType = (CustomerDefs.ECustomerType)value;
        }
        else if (a_reader.VersionNumber >= 494)
        {
            a_reader.Read(out m_abcCode);
            a_reader.Read(out m_group);
            a_reader.Read(out decimal _); //deprecated
            a_reader.Read(out m_priority);
            a_reader.Read(out int _); //deprecated
            a_reader.Read(out int _); //deprecated
            a_reader.Read(out decimal _); //deprecated
        }
        else if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out m_abcCode);
            a_reader.Read(out m_group);
            a_reader.Read(out decimal _); //deprecated
            a_reader.Read(out m_priority);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_abcCode);
        a_writer.Write(m_group);
        a_writer.Write(m_priority);
        a_writer.Write(m_colorCode);
        a_writer.Write(m_region);
        a_writer.Write((int)m_customerType);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    //Property names for DataTables.
    public class CustomerException : PTException
    {
        public CustomerException(string a_message)
            : base(a_message) { }
    }
    #endregion

    #region Construction
    internal Customer(BaseId a_id) : base(a_id)
    {
        base.Name = "New Customer".Localize();
    }

    internal Customer(BaseId a_id, PTObjectBase a_ptObject, UserFieldDefinitionManager a_udfManager) : base(a_id, a_ptObject, a_udfManager, UserField.EUDFObjectType.Customers) { }
    #endregion

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [Browsable(false)]
    public override string DefaultNamePrefix => "Customer";
    #endregion

    #region Properties
    private string m_abcCode = "";

    /// <summary>
    /// Indicates the relative customer importance in terms of ABC value for the customer from the ERP system.
    /// </summary>
    public string AbcCode
    {
        get => m_abcCode;
        set => m_abcCode = value;
    }

    private string m_group = "";

    /// <summary>
    /// Used to sort/group customers.
    /// </summary>
    public string GroupCode
    {
        get => m_group;
        set => m_group = value;
    }

    //decimal m_latePenaltyCost;
    ///// <summary>
    ///// Specifies the cost (either actual or estimated) per day of finishing this customer's orders late.  Can be used for comparing schedules and in simulation rules. 
    ///// </summary>
    //public decimal LatePenaltyCost
    //{
    //    get { return m_latePenaltyCost; }
    //    set { m_latePenaltyCost = value; }
    //}

    private int m_priority;

    /// <summary>
    /// Priority associated with this customer.  Can be used for prioritizing work by Customer.
    /// </summary>
    public int Priority
    {
        get => m_priority;
        set => m_priority = value;
    }

    private string m_region;

    /// <summary>
    /// Can be used to visually group customers by geographic region.
    /// </summary>
    public string Region
    {
        get => m_region;
        set => m_region = value;
    }

    private Color m_colorCode;

    public Color ColorCode
    {
        get => m_colorCode;
        set => m_colorCode = value;
    }

    //string m_primaryContact = "";
    //[DoNotAuditProperty] // Premarking in case later reintroduced
    //public string PrimaryContact
    //{
    //    get { return m_primaryContact; }
    //    set { m_primaryContact = value; }
    //}

    //string m_primaryContactEmail = "";
    //[DoNotAuditProperty] // Premarking in case later reintroduced
    //public string PrimaryContactEmail
    //{
    //    get { return m_primaryContactEmail; }
    //    set { m_primaryContactEmail = value; }
    //}

    //decimal m_reputation;
    //public decimal Reputation
    //{
    //    get { return m_reputation; }
    //    set { m_reputation = value; }
    //}

    //int m_reputationMin;
    //public int ReputationMin
    //{
    //    get { return m_reputationMin; }
    //    set { m_reputationMin = value; }
    //}

    //int m_reputationMax;
    //public int ReputationMax
    //{
    //    get { return m_reputationMax; }
    //    set { m_reputationMax = value; }
    //}

    private CustomerDefs.ECustomerType m_customerType = CustomerDefs.ECustomerType.Customer;

    public CustomerDefs.ECustomerType CustomerType
    {
        get => m_customerType;
        set => m_customerType = value;
    }
    #endregion

    internal void Update(UserFieldDefinitionManager a_udfManager, CustomerT.Customer a_sourceCustomer, CustomerT a_customerT)
    {
        base.Update(a_sourceCustomer, a_customerT, a_udfManager, UserField.EUDFObjectType.Customers);

        AbcCode = a_sourceCustomer.AbcCode;
        ColorCode = a_sourceCustomer.ColorCode;
        Priority = a_sourceCustomer.Priority;
        Region = a_sourceCustomer.Region;
        GroupCode = a_sourceCustomer.GroupCode;
        Name = a_sourceCustomer.Name;
        CustomerType = a_sourceCustomer.CustomerType;
    }

    public void PtDbPopulate(ref PtDbDataSet a_dataSet, PTDatabaseHelper a_dbHelper, PtDbDataSet.SchedulesRow a_schedulesRow)
    {
        a_dataSet.Customers.AddCustomersRow(
            a_schedulesRow,
            a_schedulesRow.InstanceId,
            Id.ToBaseType(),
            ExternalId,
            AbcCode,
            ColorUtils.ConvertColorToHexString(ColorCode),
            Priority,
            GroupCode,
            Region,
            Name,
            CustomerType.ToString());
    }

    #region Cloning
    public Customer Clone()
    {
        return (Customer)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion

    /// <summary>
    /// Sets the edit changes
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_edit"></param>
    /// <returns></returns>
    public void Edit(CustomerEdit a_edit)
    {
        base.Edit(a_edit);

        if (a_edit.AbcCodeSet)
        {
            AbcCode = a_edit.AbcCode;
        }

        if (a_edit.ColorCodeSet)
        {
            ColorCode = a_edit.ColorCode;
        }

        if (a_edit.GroupCodeSet)
        {
            GroupCode = a_edit.GroupCode;
        }

        if (a_edit.PrioritySet)
        {
            Priority = a_edit.Priority;
        }

        if (a_edit.RegionSet)
        {
            Region = a_edit.Region;
        }

        if (a_edit.CustomerTypeSet)
        {
            CustomerType = a_edit.CustomerType;
        }
    }
}
