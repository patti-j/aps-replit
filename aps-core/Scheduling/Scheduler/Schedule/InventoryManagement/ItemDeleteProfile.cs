using System.Collections;

using PT.APSCommon;
using PT.Common.Extensions;

namespace PT.Scheduler.Schedule.InventoryManagement;

public class ItemDeleteProfile : IEnumerable<Item>
{
    private readonly Dictionary<BaseId, Item> m_itemsToDelete = new ();
    private readonly Dictionary<BaseId, PTValidationException> m_validationExceptions = new ();
    private readonly HashSet<BaseId> m_itemsWithInventoryInUse = new ();

    public void Add(Item a_item)
    {
        m_itemsToDelete.Add(a_item.Id, a_item);
    }

    public bool Empty => m_itemsToDelete.Count == 0;

    public bool ContainsItem(BaseId a_id)
    {
        return m_itemsToDelete.ContainsKey(a_id);
    }

    public IEnumerator<Item> GetEnumerator()
    {
        //Only return items that can be deleted
        foreach (Item item in m_itemsToDelete.Values)
        {
            if (!m_itemsWithInventoryInUse.Contains(item.Id))
            {
                yield return item;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void AddValidationException(Item a_item, PTValidationException a_ptValidationException)
    {
        m_validationExceptions.AddIfNew(a_item.Id, a_ptValidationException);
    }

    public bool HasError(BaseId a_itemId)
    {
        return m_validationExceptions.ContainsKey(a_itemId);
    }

    public IEnumerable<PTValidationException> ValidationExceptions => m_validationExceptions.Values;

    public IEnumerable<Item> ItemsSafeToDelete()
    {
        foreach (Item item in m_itemsToDelete.Values)
        {
            if (!m_itemsWithInventoryInUse.Contains(item.Id) && !HasError(item.Id))
            {
                yield return item;
            }
        }
    }

    public void AddInventoryInUse(Inventory a_withError)
    {
        //This inventory is in use so we cannot delete the item.
        m_itemsWithInventoryInUse.Add(a_withError.Item.Id);
    }
}