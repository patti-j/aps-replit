using System.Data;

using PT.ERPTransmissions;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Creates a new Customer in the specified Scenario using default values.
/// </summary>
public class CustomerT : ERPMaintenanceTransmission<CustomerT.Customer>
{
    public override string Description => "Customer updated";

    public new const int UNIQUE_ID = 60;

    #region IPTSerializable Members
    public CustomerT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 494)
        {
            a_reader.Read(out int count);
            for (int custCount = 0; custCount < count; custCount++)
            {
                Add(new Customer(a_reader));
            }
        }
        else if (a_reader.VersionNumber >= 1)
        {
            Customer customer = new (a_reader);
            Add(customer);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CustomerT() { }

    public new Customer this[int i] => Nodes[i];

    public void Fill(IDbCommand a_customerCmd)
    {
        PtImportDataSet ds = new ();
        FillTable(ds.Customer, a_customerCmd);

        Fill(ds);
    }

    private void Fill(PtImportDataSet a_ds)
    {
        foreach (PtImportDataSet.CustomerRow customerRow in a_ds.Customer)
        {
            Add(new Customer(customerRow));
        }
    }

    public class Customer : PTObjectBase, IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 793;

        public Customer(IReader a_reader) : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12000)
            {
                m_boolsPropertiesSet = new BoolVector32(a_reader);
                a_reader.Read(out m_abcCode);
                a_reader.Read(out m_group);
                a_reader.Read(out m_priority);
                a_reader.Read(out m_region);
                a_reader.Read(out m_colorCode);
                a_reader.Read(out int val);
                m_customerType = (CustomerDefs.ECustomerType)val;
            }
            else if (a_reader.VersionNumber >= 1)
            {
                m_boolsPropertiesSet = new BoolVector32(a_reader);
                if (m_boolsPropertiesSet[c_nameSetIdx])
                {
                    a_reader.Read(out string name);
                    Name = name;
                }

                if (m_boolsPropertiesSet[c_externalIdSetIdx])
                {
                    a_reader.Read(out string externalId);
                    ExternalId = externalId;
                }

                if (m_boolsPropertiesSet[c_abcCodeSetIdx])
                {
                    a_reader.Read(out m_abcCode);
                }

                if (m_boolsPropertiesSet[c_groupCodeSetIdx])
                {
                    a_reader.Read(out m_group);
                }

                if (m_boolsPropertiesSet[c_latePenaltyCostSetIdx])
                {
                    a_reader.Read(out decimal _); //
                }

                if (m_boolsPropertiesSet[c_prioritySetIdx])
                {
                    a_reader.Read(out m_priority);
                }

                if (m_boolsPropertiesSet[c_regionSetIdx])
                {
                    a_reader.Read(out m_region);
                }

                if (m_boolsPropertiesSet[c_colorCodeSetIdx])
                {
                    a_reader.Read(out m_colorCode);
                }

                if (m_boolsPropertiesSet[c_primaryContactSetIdx])
                {
                    a_reader.Read(out string _); //deprecated
                }

                if (m_boolsPropertiesSet[c_primaryContractEmailSetIdx])
                {
                    a_reader.Read(out string _); //deprecated
                }

                if (m_boolsPropertiesSet[c_reputationSetIdx])
                {
                    a_reader.Read(out decimal _); //deprecated
                }

                if (m_boolsPropertiesSet[c_reputationMinSetIdx])
                {
                    a_reader.Read(out int _); //deprecated
                }

                if (m_boolsPropertiesSet[c_reputationMaxSetIdx])
                {
                    a_reader.Read(out int _); //deprecated
                }
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);
            m_boolsPropertiesSet.Serialize(a_writer);
            a_writer.Write(m_abcCode);
            a_writer.Write(m_group);
            a_writer.Write(m_priority);
            a_writer.Write(m_region);
            a_writer.Write(m_colorCode);
            a_writer.Write((int)m_customerType);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public Customer() { }

        public Customer(PtImportDataSet.CustomerRow a_customerRow) : base(a_customerRow.ExternalId)
        {
            if (!a_customerRow.IsNameNull())
            {
                Name = a_customerRow.Name;
            }

            if (!a_customerRow.IsDescriptionNull())
            {
                Description = a_customerRow.Description;
            }

            if (!a_customerRow.IsNotesNull())
            {
                Notes = a_customerRow.Notes;
            }

            if (!a_customerRow.IsUserFieldsNull())
            {
                SetUserFields(a_customerRow.UserFields);
            }

            if (!a_customerRow.IsAbcCodeNull())
            {
                AbcCode = a_customerRow.AbcCode;
            }

            if (!a_customerRow.IsColorCodeNull())
            {
                ColorCode = ColorUtils.GetColorFromHexString(a_customerRow.ColorCode);
            }

            if (!a_customerRow.IsGroupCodeNull())
            {
                GroupCode = a_customerRow.GroupCode;
            }

            if (!a_customerRow.IsPriorityNull())
            {
                Priority = a_customerRow.Priority;
            }

            if (!a_customerRow.IsRegionNull())
            {
                Region = a_customerRow.Region;
            }

            if (!a_customerRow.IsCustomerTypeNull())
            {
                CustomerType = (CustomerDefs.ECustomerType)Enum.Parse(typeof(CustomerDefs.ECustomerType), a_customerRow.CustomerType);
            }
        }

        public Customer(string a_externalId, string a_name, string a_description, string a_notes, string a_userFields) : base(a_externalId, a_name, a_description, a_notes, a_userFields) { }

        #region Properties
        private BoolVector32 m_boolsPropertiesSet;
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
                m_boolsPropertiesSet[c_abcCodeSetIdx] = true;
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
                m_boolsPropertiesSet[c_groupCodeSetIdx] = true;
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
                m_boolsPropertiesSet[c_prioritySetIdx] = true;
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
                m_boolsPropertiesSet[c_regionSetIdx] = true;
            }
        }

        private System.Drawing.Color m_colorCode;

        public System.Drawing.Color ColorCode
        {
            get => m_colorCode;
            set
            {
                m_colorCode = value;
                m_boolsPropertiesSet[c_colorCodeSetIdx] = true;
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
                m_boolsPropertiesSet[c_customerTypeSetIdx] = true;
            }
        }
        #endregion
    }
}