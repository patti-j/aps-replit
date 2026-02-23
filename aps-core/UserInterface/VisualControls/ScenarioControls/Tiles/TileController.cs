using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Debugging;
using PT.Common.Extensions;
using PT.PackageDefinitions;
using PT.PackageDefinitions.PackageInterfaces;
using PT.PackageDefinitionsUI;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ScenarioControls.Tiles;

public class TileController<T> : IDisposable
{
    public TileController(IScenarioInfo a_scenarioInfo)
    {
        m_scenarioInfo = a_scenarioInfo;
        m_activeTiles = new List<ITile>();
        m_inActiveTiles = new List<ITile>();
        m_processorMap = new Dictionary<ITile, TileProcessor<T>>();
    }

    private readonly List<ITile> m_activeTiles;
    private readonly List<ITile> m_inActiveTiles;
    private List<T> m_selectedObjects;
    private List<T> m_loadedObjects;
    private readonly IScenarioInfo m_scenarioInfo;
    private readonly Dictionary<ITile, TileProcessor<T>> m_processorMap;
    private readonly object m_dataLock = new ();
    private ITile m_primaryTile;
    private bool m_enabled;
    private bool m_outOfDate;

    private enum ETileLoadType { Simulation, User }

    private bool LoadTypeMatches(ITile a_tile, ETileLoadType a_type)
    {
        switch (a_type)
        {
            case ETileLoadType.Simulation:
                return a_tile is IScenarioDetailTile;
            case ETileLoadType.User:
                return a_tile is IUsersTile;
            default:
                throw new ArgumentOutOfRangeException(nameof(a_type), a_type, null);
        }
    }

    private void SimulationStartedHandler(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, BaseId a_instigatorId, DateTime a_timeStarted)
    {
        lock (m_dataLock)
        {
            if (!m_enabled || !m_reloadAutomatically)
            {
                NotifyOutOfDate();
                return;
            }

            foreach (ITile activeTile in m_activeTiles)
            {
                if (LoadTypeMatches(activeTile, ETileLoadType.Simulation))
                {
                    if (activeTile is IScenarioTile<T> scenarioTile)
                    {
                        scenarioTile.CancelLoad();
                    }

                    activeTile.EnterDataLock();
                }
            }
        }
    }

    private void SimulationCompleteHandler()
    {
        TriggerReload(ETileLoadType.Simulation);

        foreach (ITile activeTile in m_activeTiles)
        {
            if (LoadTypeMatches(activeTile, ETileLoadType.Simulation))
            {
                activeTile.ExitDataLock();
            }
        }
    }

    private void TriggerReload(ETileLoadType a_loadType)
    {
        lock (m_dataLock)
        {
            if (!m_enabled || !m_reloadAutomatically)
            {
                NotifyOutOfDate();
                return;
            }

            foreach (ITile activeTile in m_activeTiles)
            {
                if (activeTile is IScenarioTile<T> scenarioTile)
                {
                    //Only load primary and independent tiles
                    if (!scenarioTile.Primary && scenarioTile.SynchronizedToPrimaryTile)
                    {
                        continue;
                    }
                }

                if (LoadTypeMatches(activeTile, a_loadType))
                {
                    if (m_processorMap.TryGetValue(activeTile, out TileProcessor<T> processor))
                    {
                        processor.Reload(m_foreGround);
                    }
                }
            }
        }
    }

    private void NotifyOutOfDate()
    {
        foreach (ITile activeTile in m_activeTiles)
        {
            activeTile.NotifyOutOfDate();
        }

        lock (m_dataLock)
        {
            m_outOfDate = true;
        }
    }

    public void LoadTile(ITile a_tile)
    {
        ValidateTile(a_tile);
        UpdateManagedEvents(a_tile);
        m_inActiveTiles.Add(a_tile);
        if (a_tile is IScenarioTile<T> scenarioTile)
        {
            m_processorMap.Add(a_tile, new TileProcessor<T>(scenarioTile, m_scenarioInfo));
            scenarioTile.RequestReloadEvent += new Action<ITile>(ReloadRequestedHandler);
        }
    }

    private bool m_loadedSimEvents;
    private bool m_loadedUserEvents;

    public void RegisterEvents()
    {
        m_scenarioInfo.ScenarioActivated += ScenarioInfoOnScenarioActivated;
        m_scenarioInfo.UndoComplete += ScenarioInfoOnUndoComplete;
    }

    private void UpdateManagedEvents(ITile a_tile)
    {
        if (a_tile is IGridTile<T> tile)
        {
            tile.ObjectsSelectedEvent += TileSelectedObjectsHandler;
            tile.ObjectsLoadedEvent += TileLoadedObjectsHandler;
        }

        if (!m_loadedSimEvents && a_tile is IScenarioDetailTile)
        {
            m_loadedSimEvents = true;
            m_scenarioInfo.SimulationComplete += SimulationCompleteHandler;
            m_scenarioInfo.SimulationStart += SimulationStartedHandler;
        }
        else if (!m_loadedUserEvents && a_tile is IUsersTile)
        {
            m_scenarioInfo.DataChanged += ScenarioInfoOnDataChanged;
            m_loadedUserEvents = true;
        }
    }

    private void ScenarioInfoOnScenarioActivated(Scenario a_s, ScenarioDetail a_sd, ScenarioEvents a_se)
    {
        foreach (ITile activeTile in m_activeTiles)
        {
            UpdateTileReadonlyStatus(activeTile);
        }
    }

    private void ScenarioInfoOnUndoComplete(bool a_success)
    {
        Reload(m_activeTiles.Where(t => t is ITileRequiresReloadAfterUndo));
    }

    /// <summary>
    /// Notify the tile of readonly status based on the Scenario and User permissions
    /// </summary>
    /// <param name="a_tile"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void UpdateTileReadonlyStatus(ITile a_tile)
    {
        EUserAccess currentUserEditAccess = m_scenarioInfo.GetCurrentUserEditAccess(a_tile.RequiredPermissions.ToArray());
        switch (currentUserEditAccess)
        {
            case EUserAccess.None:
            case EUserAccess.ViewOnly:
                a_tile.EnterReadOnly();
                break;
            case EUserAccess.Edit:
                a_tile.ExitReadOnly();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ScenarioInfoOnDataChanged(IScenarioDataChanges a_changes)
    {
        if (a_changes.UserChanges.HasChanges)
        {
            TriggerReload(ETileLoadType.User);
        }
    }

    private void ReloadRequestedHandler(ITile a_tile)
    {
        if (!m_scenarioInfo.InvokeControl.InvokeRequired)
        {
            Task.Run(new Action(() => Reload(a_tile)));
        }
        else
        {
            Reload(a_tile);
        }
    }

    public void ActivateTile(ITile a_tile)
    {
        lock (m_dataLock)
        {
            m_activeTiles.Add(a_tile);
            if (m_inActiveTiles.Contains(a_tile))
            {
                m_inActiveTiles.Remove(a_tile);
            }

            a_tile.Shown = true;
            UpdateTileReadonlyStatus(a_tile);

            if (m_processorMap.TryGetValue(a_tile, out TileProcessor<T> processor))
            {
                Task.Run(new Action(() =>
                {
                    processor.ObjectsLoaded(m_loadedObjects, true);
                    processor.ObjectsSelected(m_selectedObjects, true);
                    processor.Reload(m_foreGround); //This might not be necessary if all tiles reload based on loaded or selected objects.
                }));
            }
        }
    }

    public void HideTile(ITile a_tile)
    {
        lock (m_dataLock)
        {
            bool removed = m_activeTiles.Remove(a_tile);
            a_tile.Shown = false;

            if (removed)
            {
                m_inActiveTiles.Add(a_tile);
            }
        }
    }

    private void ValidateTile(ITile a_tile)
    {
        //If IGridTile loads but fails this validation, could be because the T doesn't match what's defined on the Board interface
        if (a_tile is IGridTile<T> tile)
        {
            if (tile.Primary)
            {
                #if DEBUG
                if (m_primaryTile != null)
                {
                    DebugException.ThrowInDebug("Only one primary tile should be defined: " + a_tile.TileKey.Localize());
                }
                #endif
                m_primaryTile = tile;
            }
        }
        #if TEST
            else
            {
                
            }
        #endif
        #if DEBUG
        foreach (ITile tile1 in m_inActiveTiles)
        {
            if (tile1.TileKey == a_tile.TileKey)
            {
                DebugException.ThrowInDebug("Each tile must have a unique ControlName. Multiple names: " + a_tile.TileKey);
            }
        }
        #endif
    }

    private void ObjectsSelected(ITile a_sourceTile, List<T> a_objects)
    {
        if (a_objects == null)
        {
            DebugException.ThrowInDebug("Tile sent null objects selected");
            return;
        }

        lock (m_dataLock)
        {
            m_selectedObjects = a_objects;
            foreach (ITile activeTile in m_activeTiles)
            {
                if (activeTile == a_sourceTile)
                {
                    //Don't add the source tile.
                    continue;
                }

                if (m_processorMap.TryGetValue(activeTile, out TileProcessor<T> processor))
                {
                    processor.ObjectsSelected(m_selectedObjects);
                }
            }
        }
    }

    private void ObjectsLoaded(ITile a_sourceTile, List<T> a_objects)
    {
        if (a_objects == null)
        {
            DebugException.ThrowInDebug("Tile sent null objects selected");
            return;
        }

        lock (m_dataLock)
        {
            m_loadedObjects = a_objects;
            foreach (ITile activeTile in m_activeTiles)
            {
                if (activeTile == a_sourceTile)
                {
                    //Don't add the source tile.
                    continue;
                }

                if (m_processorMap.TryGetValue(activeTile, out TileProcessor<T> processor))
                {
                    processor.ObjectsLoaded(m_loadedObjects);
                }
            }
        }
    }

    private void TileSelectedObjectsHandler(ITile a_sourceTile, List<T> a_objectIds)
    {
        Task.Run(new Action(() => ObjectsSelected(a_sourceTile, a_objectIds)));
    }

    private void TileLoadedObjectsHandler(ITile a_sourceTile, List<T> a_objectIds)
    {
        Task.Run(new Action(() => ObjectsLoaded(a_sourceTile, a_objectIds)));
    }

    public void Reload()
    {
        Reload(m_activeTiles);
        lock (m_dataLock)
        {
            m_outOfDate = false;
        }
    }

    private void Reload(ITile a_tile)
    {
        List<ITile> tiles = new () { a_tile };
        Reload(tiles);
    }

    private void Reload(IEnumerable<ITile> a_tilesToLoad)
    {
        lock (m_dataLock)
        {
            foreach (ITile activeTile in a_tilesToLoad)
            {
                if (m_processorMap.TryGetValue(activeTile, out TileProcessor<T> processor))
                {
                    processor.Reload(m_foreGround);
                }
            }
        }
    }

    public List<ITile> GetTiles()
    {
        lock (m_dataLock)
        {
            List<ITile> tiles = m_inActiveTiles.ShallowCopy();
            tiles.AddRange(m_activeTiles);
            return tiles;
        }
    }

    public List<ITile> GetClosedTiles()
    {
        lock (m_dataLock)
        {
            return m_inActiveTiles.ShallowCopy();
        }
    }

    public List<ITile> GetActiveTiles()
    {
        lock (m_dataLock)
        {
            return m_activeTiles.ShallowCopy();
        }
    }

    private void CancelLoads()
    {
        lock (m_dataLock)
        {
            foreach (ITile activeTile in m_activeTiles)
            {
                if (m_processorMap.TryGetValue(activeTile, out TileProcessor<T> processor))
                {
                    processor.CancelLoad();
                }
            }
        }
    }

    public ITile ActivateTile(string a_tileKey)
    {
        lock (m_dataLock)
        {
            foreach (ITile inActiveTile in m_inActiveTiles)
            {
                if (inActiveTile.TileKey == a_tileKey)
                {
                    ActivateTile(inActiveTile);
                    return inActiveTile;
                }
            }
        }

        return null;
    }

    public ITile GetPrimaryTile()
    {
        return m_primaryTile;
    }

    public void HideAllTiles()
    {
        for (int i = m_activeTiles.Count - 1; i >= 0; i--)
        {
            HideTile(m_activeTiles[i]);
        }
    }

    private bool m_foreGround;

    public void SetForeground(bool a_foreGround)
    {
        lock (m_dataLock)
        {
            m_foreGround = a_foreGround;
        }
    }

    public void Enable()
    {
        bool reload = false;
        lock (m_dataLock)
        {
            if (!m_enabled)
            {
                m_enabled = true;
                reload = true;
            }
        }

        if (reload)
        {
            Reload();
        }
    }

    public void Disable()
    {
        bool cancel = false;
        lock (m_dataLock)
        {
            if (m_enabled)
            {
                m_enabled = false;
                cancel = true;
            }
        }

        if (cancel)
        {
            CancelLoads();
        }
    }

    private bool m_reloadAutomatically = true;

    public bool ReloadAutomatically
    {
        get
        {
            lock (m_dataLock)
            {
                return m_reloadAutomatically;
            }
        }
        set
        {
            bool outOfDate;
            lock (m_dataLock)
            {
                m_reloadAutomatically = value;
                outOfDate = m_outOfDate;
            }

            if (m_enabled && outOfDate)
            {
                //Objects selected may cancel load which can't be done on the main thread
                Task.Run(new Action(() =>
                {
                    ObjectsSelected(m_primaryTile, m_loadedObjects);
                    Reload();
                }));
            }
        }
    }

    public void Dispose()
    {
        m_scenarioInfo.ScenarioActivated -= ScenarioInfoOnScenarioActivated;
        m_scenarioInfo.UndoComplete -= ScenarioInfoOnUndoComplete;
        m_scenarioInfo.SimulationComplete -= SimulationCompleteHandler;
        m_scenarioInfo.SimulationStart -= SimulationStartedHandler;
        m_scenarioInfo.DataChanged -= ScenarioInfoOnDataChanged;
    }
}