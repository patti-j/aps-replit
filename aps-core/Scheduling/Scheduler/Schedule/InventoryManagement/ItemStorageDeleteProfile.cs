using System.Collections;

using PT.APSCommon;
using PT.Common.Extensions;

namespace PT.Scheduler.Schedule.InventoryManagement;

internal class ItemStorageDeleteProfile : IEnumerable<ItemStorage>
{
    private readonly bool m_clearJobs;
    private readonly bool m_clearPurchaseOrders;
    private readonly Dictionary<BaseId, ItemStorage> m_itemStorageCache = new();
    private readonly Dictionary<BaseId, HashSet<BaseId>> m_storageAreaItemCache = new();
    private readonly Dictionary<BaseId, PTValidationException> m_validationExceptions = new();
    
    public ItemStorageDeleteProfile(bool a_clearJobs, bool a_clearPurchaseOrders)
    {
        m_clearJobs = a_clearJobs;
        m_clearPurchaseOrders = a_clearPurchaseOrders;
    }

    public ItemStorageDeleteProfile()
    {
        
    }

    public void Add(ItemStorage a_itemStorage)
    {
        m_itemStorageCache.Add(a_itemStorage.Id, a_itemStorage);
        if (m_storageAreaItemCache.TryGetValue(a_itemStorage.StorageArea.Id, out HashSet<BaseId> itemIds))
        {
            itemIds.Add(a_itemStorage.Item.Id);
        }
        else
        {
            m_storageAreaItemCache.Add(a_itemStorage.StorageArea.Id, new HashSet<BaseId> { a_itemStorage.Item.Id });
        }
    }

    public bool ContainsItemStorage(BaseId a_itemStorageId)
    {
        return m_itemStorageCache.ContainsKey(a_itemStorageId);
    }

    public ItemStorage GetById(BaseId a_inventoryId)
    {
        return m_itemStorageCache[a_inventoryId];
    }

    public bool ContainsItems(BaseId a_storageAreaId, BaseId a_itemId)
    {
        if (m_storageAreaItemCache.TryGetValue(a_storageAreaId, out HashSet<BaseId> itemIds))
        {
            if (itemIds.Contains(a_itemId))
            {
                return true;
            }
        }

        return false;
    }

    public bool Empty => m_itemStorageCache.Count == 0;

    public IEnumerator<ItemStorage> GetEnumerator()
    {
        return m_itemStorageCache.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void AddValidationException(ItemStorage a_itemStorage, PTValidationException a_ptValidationException)
    {
        m_validationExceptions.AddIfNew(a_itemStorage.Id, a_ptValidationException);
    }

    public bool HasError(BaseId a_itemStorageId)
    {
        return m_validationExceptions.ContainsKey(a_itemStorageId);
    }

    public IEnumerable<PTValidationException> ValidationExceptions => m_validationExceptions.Values;

    public bool ClearJobs
    {
        get { return m_clearJobs; }
    }

    public bool ClearPurchaseOrders
    {
        get { return m_clearPurchaseOrders; }
    }

    public bool CanSafelyRemoveAll => m_validationExceptions.Values.Count == 0;

    public IEnumerable<ItemStorage> ItemStoragesSafeToDelete()
    {
        foreach (ItemStorage itemStorage in m_itemStorageCache.Values)
        {
            if (!HasError(itemStorage.Id))
            {
                yield return itemStorage;
            }
        }
    }

    public IEnumerable<ItemStorage> ItemStoragesWithErrors()
    {
        foreach (ItemStorage itemStorage in m_itemStorageCache.Values)
        {
            if (HasError(itemStorage.Id))
            {
                yield return itemStorage;
            }
        }
    }
}