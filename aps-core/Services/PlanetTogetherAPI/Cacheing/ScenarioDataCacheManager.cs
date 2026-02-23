using LazyCache;
using PT.Scheduler;
using static PT.PlanetTogetherAPI.Controllers.CtpController;

namespace PT.PlanetTogetherAPI.Cacheing;

/// <summary>
/// Handles the cacheing and access of Scenario Data used by data APIs across controllers.
/// </summary>
// Use this class to separate scenario data access from its delivery.
internal interface IScenarioDataCacheManager<T>
{
    /// <summary>
    /// Loads data into the cache for a scenario. Will not replace existing cache data unless <see cref="a_replaceCacheIfPresent"/> is true;
    /// </summary>
    /// <param name="a_scenarioId"></param>
    /// <exception cref="AutoTryEnterException">If data is locked for longer than a configured timeout.</exception>
    T LoadCache(long a_scenarioId);

    /// <summary>
    /// Gets all cached data of type T for the scenario, to be queried/ mapped afterward.
    /// </summary>
    /// <param name="a_scenarioId"></param>
    /// <exception cref="AutoTryEnterException">If data must be loaded and is locked for longer than a configured timeout.</exception>
    T GetOrLoadCache(long a_scenarioId);

    /// <summary>
    /// Clears the cache of all data of type T for the scenario.
    /// </summary>
    /// <param name="a_scenarioId"></param>
    void ClearCache(long a_scenarioId);

    bool ContainsData(long a_scenarioId);
}

internal abstract class ScenarioDataCacheManager<T> : IScenarioDataCacheManager<T>
{
    protected readonly IAppCache m_cache;

    protected int m_loadingTimeoutMs;

    protected abstract string GetCacheKey(long a_scenarioId);


    public ScenarioDataCacheManager(IAppCache a_cache, int a_timeout)
    {
        m_cache = a_cache;
        m_loadingTimeoutMs = a_timeout;
    }

    public abstract T LoadCache(long a_scenarioId);

    public abstract T GetOrLoadCache(long a_scenarioId);

    public abstract void ClearCache(long a_scenarioId);

    public abstract bool ContainsData(long a_scenarioId);

    protected abstract InventoryLookup GetScenarioAndLoad(long a_scenarioId, bool a_forceRefresh);
}

/// <summary>
/// Wrapper for related data needed for constructing a <see cref="Inventory"/> dto.
/// </summary>
internal class InventoryLookup
{
    internal Dictionary<long, (Inventory Inventory, List<Warehouse> WarehousesContainingItem)> Lookup = new();
}
