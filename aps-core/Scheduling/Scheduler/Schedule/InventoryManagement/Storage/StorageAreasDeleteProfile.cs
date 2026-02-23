using System.Collections;

using PT.APSCommon;
using PT.Common.Extensions;

namespace PT.Scheduler.Schedule.Storage;

public class StorageAreasDeleteProfile : IEnumerable<StorageArea>
{
    private readonly bool m_clearingJobs;
    private readonly bool m_clearingPurchaseOrders;
    private readonly bool m_clearingItemStorages;
    private readonly Dictionary<BaseId, StorageArea> m_storageAreaCache = new ();
    private readonly Dictionary<BaseId, PTValidationException> m_validationExceptions = new ();

    public StorageAreasDeleteProfile(bool a_clearingJobs, bool a_clearingPurchaseOrders, bool a_clearingItemStorages)
    {
        m_clearingJobs = a_clearingJobs;
        m_clearingPurchaseOrders = a_clearingPurchaseOrders;
        m_clearingItemStorages = a_clearingItemStorages;
    }

    public StorageAreasDeleteProfile() { }

    public void Add(StorageArea a_storageArea)
    {
        m_storageAreaCache.Add(a_storageArea.Id, a_storageArea);
    }

    public StorageArea GetById(BaseId a_storageAreaId)
    {
        return m_storageAreaCache[a_storageAreaId];
    }

    public bool ContainsStorageArea(BaseId a_saId)
    {
        return m_storageAreaCache.ContainsKey(a_saId);
    }
    
    public bool Empty => m_storageAreaCache.Count == 0;

    public IEnumerator<StorageArea> GetEnumerator()
    {
        return m_storageAreaCache.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void AddValidationException(StorageArea a_storageArea, PTValidationException a_ptValidationException)
    {
        m_validationExceptions.AddIfNew(a_storageArea.Id, a_ptValidationException);
    }

    public bool HasError(BaseId a_storageAreaId)
    {
        return m_validationExceptions.ContainsKey(a_storageAreaId);
    }

    public PTValidationException GetException(BaseId a_storageAreaId)
    {
        return m_validationExceptions[a_storageAreaId];
    }

    public IEnumerable<PTValidationException> ValidationExceptions => m_validationExceptions.Values;

    public bool ClearingItemStorages
    {
        get { return m_clearingItemStorages; }
    }

    public bool ClearingJobs
    {
        get { return m_clearingJobs; }
    }

    public bool ClearingPurchaseOrders
    {
        get { return m_clearingPurchaseOrders; }
    }

    public IEnumerable<StorageArea> StorageAreasSafeToDelete()
    {
        foreach (StorageArea storageArea in m_storageAreaCache.Values)
        {
            if (!HasError(storageArea.Id))
            {
                yield return storageArea;
            }
        }
    }

    public IEnumerable<StorageArea> StorageAreasWithErrors()
    {
        foreach (StorageArea storageArea in m_storageAreaCache.Values)
        {
            if (HasError(storageArea.Id))
            {
                yield return storageArea;
            }
        }
    }
}