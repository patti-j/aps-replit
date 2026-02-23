using LazyCache;

using PT.APSCommon;
using PT.APSCommon.Exceptions;
using PT.Scheduler;

namespace PT.PlanetTogetherAPI.Cacheing;

internal class CtpItemsCacheManager : ScenarioDataCacheManager<InventoryLookup>
{
    private const string c_cacheKeyPrefix = "ctpItems";
    private object m_lock = new ();

    public CtpItemsCacheManager(IAppCache a_cache, int a_timeout) : base(a_cache, a_timeout) { }

    protected override string GetCacheKey(long a_scenarioId) => $"{c_cacheKeyPrefix}.{a_scenarioId}";

    public override InventoryLookup GetOrLoadCache(long a_scenarioId)
    {
        InventoryLookup inventoryLookup = m_cache.Get<InventoryLookup>(GetCacheKey(a_scenarioId));

        if (inventoryLookup == null)
        {
            inventoryLookup = GetScenarioAndLoad(a_scenarioId, false);
        }

        return inventoryLookup;
    }

    public override InventoryLookup LoadCache(long a_scenarioId)
    {
        return GetScenarioAndLoad(a_scenarioId, true);
    }

    public override void ClearCache(long a_scenarioId)
    {
        m_cache.Remove(GetCacheKey(a_scenarioId));
    }

    public override bool ContainsData(long a_scenarioId)
    {
        return m_cache.TryGetValue<InventoryLookup>(GetCacheKey(a_scenarioId), out _);
    }

    protected override InventoryLookup GetScenarioAndLoad(long a_scenarioId, bool a_forceRefresh)
    {
        InventoryLookup cacheData = null;

        using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, m_loadingTimeoutMs))
        {
            Scenario scenario = sm.Find(new BaseId(a_scenarioId));

            if (scenario == null)
            {
                throw new PTNoScenarioException($"No scenario exists for Id {a_scenarioId}");
            }

            using (scenario.ScenarioDetailLock.TryEnterRead(out ScenarioDetail sd, m_loadingTimeoutMs))
            {
                lock (m_lock)
                {
                    if (a_forceRefresh)
                    {
                        m_cache.Add(GetCacheKey(a_scenarioId), GetInventoryWithContainingWarehouses(sd));
                        cacheData = m_cache.Get<InventoryLookup>(GetCacheKey(a_scenarioId));
                    }
                    else
                    {
                        cacheData = m_cache.GetOrAdd(GetCacheKey(a_scenarioId), _ => GetInventoryWithContainingWarehouses(sd));
                    }
                }
            }
        }

        return cacheData;
    }

    // I didn't see an existing way to get a list of items with their related Warehouses through any of the relevant entities/ managers / ServerSessionManager
    // - happy to adopt something existing if I missed it (or move this to one of those classes)
    private InventoryLookup GetInventoryWithContainingWarehouses(ScenarioDetail a_sd)
    {
        InventoryLookup inventoriesLookup = new InventoryLookup();

        foreach (Warehouse warehouse in a_sd.WarehouseManager)
        {
            using (IEnumerator<Inventory> enumerator = warehouse.Inventories.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Inventory inventory = enumerator.Current;
                    long inventoryId = inventory.Id.Value;

                    // CTP only needs items in templates. TODO if this method becomes used more generally, remove this from the method and filter after
                    if (inventory.HaveTemplateManufacturingOrderId)
                    {
                        if (!inventoriesLookup.Lookup.ContainsKey(inventoryId))
                        {
                            inventoriesLookup.Lookup.Add(inventoryId, new(inventory, new List<Warehouse>()));
                        }

                        // Include current warehouse with inventory
                        inventoriesLookup.Lookup[inventoryId].WarehousesContainingItem.Add(warehouse);
                    }
                }
            }
        }

        return inventoriesLookup;
    }
}