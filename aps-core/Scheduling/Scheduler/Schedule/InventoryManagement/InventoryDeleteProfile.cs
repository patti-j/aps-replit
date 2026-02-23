using System.Collections;

using PT.APSCommon;
using PT.Common.Extensions;

namespace PT.Scheduler.Schedule.InventoryManagement;

public class InventoryDeleteProfile : IEnumerable<Inventory>
{
    private readonly bool m_clearingJobs;
    private readonly bool m_clearingPurchaseOrders;
    private readonly bool m_clearingTransferOrders;
    private readonly bool m_clearingItemStorages;
    private readonly Dictionary<BaseId, Inventory> m_inventoryCache = new ();
    private readonly Dictionary<BaseId, HashSet<BaseId>> m_warehouseItemCache = new ();
    private readonly Dictionary<BaseId, PTValidationException> m_validationExceptions = new ();

    public InventoryDeleteProfile(bool a_clearingJobs, bool a_clearingPurchaseOrders, bool a_clearingTransferOrders, bool a_clearingItemStorages)
    {
        m_clearingJobs = a_clearingJobs;
        m_clearingPurchaseOrders = a_clearingPurchaseOrders;
        m_clearingTransferOrders = a_clearingTransferOrders;
        m_clearingItemStorages = a_clearingItemStorages;
    }

    public InventoryDeleteProfile() {}

    public void Add(Inventory a_inventory)
    {
        m_inventoryCache.Add(a_inventory.Id, a_inventory);
        if (m_warehouseItemCache.TryGetValue(a_inventory.Warehouse.Id, out HashSet<BaseId> itemIds))
        {
            itemIds.Add(a_inventory.Item.Id);
        }
        else
        {
            m_warehouseItemCache.Add(a_inventory.Warehouse.Id, new HashSet<BaseId> { a_inventory.Item.Id });
        }
    }

    public bool ContainsInventory(BaseId a_inventoryId)
    {
        return m_inventoryCache.ContainsKey(a_inventoryId);
    }

    public Inventory GetById(BaseId a_inventoryId)
    {
        return m_inventoryCache[a_inventoryId];
    }

    public bool ContainsInventory(BaseId a_warehouseId, BaseId a_itemId)
    {
        if (m_warehouseItemCache.TryGetValue(a_warehouseId, out HashSet<BaseId> itemIds))
        {
            if (itemIds.Contains(a_itemId))
            {
                return true;
            }
        }

        return false;
    }

    public bool Empty => m_inventoryCache.Count == 0;

    public IEnumerator<Inventory> GetEnumerator()
    {
        return m_inventoryCache.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void AddValidationException(Inventory a_inventory, PTValidationException a_ptValidationException)
    {
        m_validationExceptions.AddIfNew(a_inventory.Id, a_ptValidationException);
    }

    public bool HasError(BaseId a_inventoryId)
    {
        return m_validationExceptions.ContainsKey(a_inventoryId);
    }

    public IEnumerable<PTValidationException> ValidationExceptions => m_validationExceptions.Values;

    public bool ClearingJobs
    {
        get { return m_clearingJobs; }
    }

    public bool ClearingPurchaseOrders
    {
        get { return m_clearingPurchaseOrders; }
    }

    public bool ClearingTransferOrders
    {
        get { return m_clearingTransferOrders; }
    }

    public bool ClearingItemStorages
    {
        get { return m_clearingItemStorages; }
    }

    public IEnumerable<Inventory> InventoriesSafeToDelete()
    {
        foreach (Inventory inventory in m_inventoryCache.Values)
        {
            if (!HasError(inventory.Id))
            {
                yield return inventory;
            }
        }
    }

    public IEnumerable<Inventory> InventoriesWithErrors()
    {
        foreach (Inventory inventory in m_inventoryCache.Values)
        {
            if (HasError(inventory.Id))
            {
                yield return inventory;
            }
        }
    }
}