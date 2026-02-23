using System.Collections;

using PT.APSCommon;
using PT.Common.Exceptions;

namespace PT.Scheduler.Schedule.Customers;

public class CustomerCollection : IEnumerable<Customer>
{
    #region Declarations
    private readonly SortedList<BaseId, Customer> m_customers = new ();

    public class CustomerIntervalsCollectionException : PTException
    {
        public CustomerIntervalsCollectionException(string a_message) : base(a_message) { }
    }
    #endregion

    #region Construction
    #endregion

    #region Properties and Methods
    internal Customer Add(Customer a_customer)
    {
        m_customers.Add(a_customer.Id, a_customer);
        return a_customer;
    }

    internal void Remove(int a_index)
    {
        m_customers.RemoveAt(a_index);
    }

    internal void Remove(Customer a_customer)
    {
        m_customers.Remove(a_customer.Id);
    }

    public Customer this[int a_index] => m_customers.Values[a_index];

    public object GetRow(int a_index)
    {
        return m_customers.Values[a_index];
    }

    public int Count => m_customers.Count;

    public Customer Find(BaseId a_id)
    {
        return m_customers[a_id];
    }

    public bool Contains(BaseId a_id)
    {
        return m_customers.ContainsKey(a_id);
    }

    /// <summary>
    /// Get List of Customer ExternalIds
    /// </summary>
    /// <returns></returns>
    public string GetCustomerExternalIdsList()
    {
        if (Count == 0)
        {
            return "";
        }

        string customers = "";
        foreach (Customer customer in this)
        {
            customers += $"{customer.ExternalId}, ";
        }

        return customers.TrimEnd(',', ' ');
    }

    /// <summary>
    /// Get List of Customer Names. Uses Customer External Id if Name is not set
    /// </summary>
    /// <returns></returns>
    public string GetCustomerNamesList()
    {
        if (Count == 0)
        {
            return "";
        }

        string customers = "";
        foreach (Customer customer in this)
        {
            customers += string.IsNullOrEmpty(customer.Name) ? $"{customer.ExternalId}, " : $"{customer.Name}, ";
        }

        return customers.TrimEnd(',', ' ');
    }

    public IEnumerator<Customer> GetEnumerator()
    {
        foreach (Customer customer in m_customers.Values)
        {
            yield return customer;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion
}