using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Database;
using PT.ERPTransmissions;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of Customer objects.
/// </summary>
public class CustomerManager : ScenarioBaseObjectManager<Customer>, IPTSerializable
{
    #region IPTSerializable Members
    public CustomerManager(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_idGen)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Customer c = new (a_reader);
                Add(c);
            }
        }
    }

    public new const int UNIQUE_ID = 14;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    public class CustomerManagerException : PTException
    {
        public CustomerManagerException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }
    }
    #endregion

    #region Construction
    public CustomerManager(BaseIdGenerator a_idGen) : base(a_idGen) { }
    #endregion

    #region Customer Edit Functions
    private Customer AddDefault(CustomerDefaultT t, IScenarioDataChanges a_dataChanges)
    {
        Customer c = MakeNameUnique(new Customer(NextID()));
        ValidateAdd(c);
        a_dataChanges?.AuditEntry(new AuditEntry(c.Id, c), true);
        return Add(c);
    }

    private Customer AddCopy(CustomerCopyT a_t, IScenarioDataChanges a_dataChanges)
    {
        ValidateCopy(a_t);
        Customer c = GetById(a_t.originalId);
        Customer added = AddCopy(c, c.Clone(), NextID());
        a_dataChanges?.AuditEntry(new AuditEntry(added.Id, added), true);
        return added;
    }

    private void Delete(BaseId a_customerId, HashSet<BaseId> a_cannotDeleteTheseCustomers, IScenarioDataChanges a_dataChanges)
    {
        Customer c = GetById(a_customerId);
        if (c != null)
        {
            if (a_cannotDeleteTheseCustomers.Contains(a_customerId))
            {
                throw new ValidationException("4456", new object[] { a_customerId.ToString() });
            }

            a_dataChanges?.AuditEntry(new AuditEntry(c.Id, c), false, true);
            Remove(c.Id); //Now remove it from the Manager.
        }
    }

    // Just a function to prevent the default customer name always being "New Customer"
    // although it technically works for any customer that's passed in. 
    private Customer MakeNameUnique(Customer a_customer)
    {
        int counter = 1;
        bool nameIsUnique = false;
        string newCustomerName = a_customer.Name;
        while (!nameIsUnique)
        {
            nameIsUnique = true;
            foreach (Customer customer in this)
            {
                if (customer.Name == newCustomerName)
                {
                    nameIsUnique = false;
                    break;
                }
            }

            if (!nameIsUnique)
            {
                newCustomerName = a_customer.Name + $" {counter}";
                counter++;
            }
        }

        a_customer.Name = newCustomerName;
        return a_customer;
    }
    #endregion

    #region Transmissions
    private void ValidateAdd(Customer a_c)
    {
        if (Contains(a_c))
        {
            throw new CustomerManagerException("2735", new object[] { a_c.Id.ToString() });
        }
    }

    private void ValidateCopy(CustomerCopyT a_t)
    {
        ValidateExistence(a_t.originalId);
    }

    public void Receive(CustomerBaseT a_t, IScenarioDataChanges a_dataChanges)
    {
        Customer c;
        if (a_t is CustomerDefaultT customerDefaultT)
        {
            c = AddDefault(customerDefaultT, a_dataChanges);
            a_dataChanges.CustomerChanges.AddedObject(c.Id);
        }
        else if (a_t is CustomerCopyT customerCopyT)
        {
            c = AddCopy(customerCopyT, a_dataChanges);
            a_dataChanges.CustomerChanges.AddedObject(c.Id);
        }
        else if (a_t is CustomerDeleteT customerDeleteT)
        {
            HashSet<BaseId> inUseCustomerIds = GetInUseCustomerIds();
            for (int i = 0; i < customerDeleteT.CustomerIds.Count; i++)
            {
                BaseId customerId = customerDeleteT.CustomerIds[i];
                Delete(customerId, inUseCustomerIds, a_dataChanges);
                a_dataChanges.CustomerChanges.DeletedObject(customerId);
            }
        }
        else if (a_t is CustomerDeleteAllT)
        {
            HashSet<BaseId> inUseCustomerIds = GetInUseCustomerIds();
            for (int i = Count - 1; i >= 0; i--)
            {
                c = GetByIndex(i);
                Delete(c.Id, inUseCustomerIds, a_dataChanges);
                a_dataChanges.CustomerChanges.DeletedObject(c.Id);
            }
        }
    }

    /// <summary>
    /// Transmission receive for Customer GridEdits
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_t"></param>
    /// <param name="a_dataChanges"></param>
    /// <exception cref="PTHandleableException"></exception>
    public void Receive(ScenarioDetail a_sd, CustomerEditT a_t, IScenarioDataChanges a_dataChanges)
    {
        foreach (CustomerEdit edit in a_t)
        {
            Customer customer = null;
            if (edit.BaseIdSet)
            {
                customer = GetById(edit.Id);
            }
            else if (edit.ExternalIdSet)
            {
                customer = GetByExternalId(edit.ExternalId);
            }

            if (customer == null)
            {
                throw new PTHandleableException("Not found");
            }

            AuditEntry custAuditEntry = new AuditEntry(customer.Id, customer);
            customer.Edit(edit);
            a_dataChanges.CustomerChanges.UpdatedObject(customer.Id);
            a_dataChanges.AuditEntry(custAuditEntry);
        }
    }
    #endregion

    #region ERP Transmissions
    public void Receive(ScenarioDetail a_sd, UserFieldDefinitionManager a_udfManager, CustomerT a_t, IScenarioDataChanges a_dataChanges)
    {
        HashSet<BaseId> affectedCustomers = new ();

        for (int i = 0; i < a_t.Count; ++i)
        {
            CustomerT.Customer custNode = a_t[i];

            Customer customer;
            if (custNode.IdSet)
            {
                customer = GetById(custNode.Id);
                if (customer == null)
                {
                    throw new ValidationException("2275", new object[] { custNode.Id });
                }
            }
            else
            {
                customer = GetByExternalId(custNode.ExternalId);
            }

            if (customer == null)
            {
                customer = new Customer(NextID(), custNode, a_udfManager);
                Add(customer);
                a_dataChanges.CustomerChanges.AddedObject(customer.Id);
                a_dataChanges.AuditEntry(new AuditEntry(customer.Id, customer), true);
            }
            else
            {
                AuditEntry custAuditEntry = new AuditEntry(customer.Id, customer);
                customer.Update(a_udfManager, custNode, a_t);
                a_dataChanges.CustomerChanges.UpdatedObject(customer.Id);
                a_dataChanges.AuditEntry(custAuditEntry);
            }

            affectedCustomers.Add(customer.Id);
        }

        if (a_t.AutoDeleteMode)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                BaseId id = GetByIndex(i).Id;
                if (!affectedCustomers.Contains(id))
                {
                    Customer cust = GetByIndex(i);
                    a_dataChanges.AuditEntry(new AuditEntry(cust.Id, cust), false, true);
                    Remove(id);
                    a_dataChanges.CustomerChanges.DeletedObject(id);
                }
            }
        }
    }
    #endregion

    #region ICopyTable
    public override Type ElementType => typeof(Customer);
    #endregion

    #region Restore References
    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (Customer customer in this)
        {
            a_udfManager.RestoreReferences(customer, UserField.EUDFObjectType.Customers);
        }
    }
    #endregion

    public void PtDbPopulate(ref PtDbDataSet a_ds, PTDatabaseHelper a_dbHelper, PtDbDataSet.SchedulesRow a_schedulesRow)
    {
        foreach (Customer customer in this)
        {
            customer.PtDbPopulate(ref a_ds, a_dbHelper, a_schedulesRow);
        }
    }

    public void ClearCustomers(IScenarioDataChanges a_dataChanges)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            Customer customer = GetByIndex(i);
            ValidateExistence(customer.Id);
            a_dataChanges.CustomerChanges.DeletedObject(customer.Id);
            Remove(customer);
        }
    }

    /// <summary>
    /// Looks through the Jobs and Sales Orders of the scenario detail, and
    /// grabs all customer Ids that are linked to any of them. These customers
    /// cannot be deleted until their linkage is removed.
    /// </summary>
    /// <returns>A HashSet of customer Ids that correspond to the customers being used by a Job or Sales Order</returns>
    private HashSet<BaseId> GetInUseCustomerIds()
    {
        HashSet<BaseId> inUseCustomerIds = new ();
        // Just using a HashSet so we're not adding a bunch of duplicates, and  
        // it should make the lookup later a good bit faster if we have large data 


        foreach (Job job in m_scenarioDetail.JobManager)
        {
            foreach (Customer jobCustomer in job.Customers)
            {
                inUseCustomerIds.Add(jobCustomer.Id);
            }
        }

        foreach (SalesOrder salesOrder in m_scenarioDetail.SalesOrderManager)
        {
            if (salesOrder.Customer != null) // Sales Order might not have any customers
            {
                inUseCustomerIds.Add(salesOrder.Customer.Id);
            }
        }

        return inUseCustomerIds;
    }
}
