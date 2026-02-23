using PT.PackageDefinitionsUI;
using PT.Scheduler;
using PT.SchedulerData;

namespace PT.ScenarioControls.Tiles;

public class TileProcessor<T>
{
    public TileProcessor(IScenarioTile<T> a_tile, IScenarioInfo a_scenarioInfo)
    {
        m_tile = a_tile;
        m_scenarioInfo = a_scenarioInfo;
        m_isScenarioDetailTile = a_tile is IScenarioDetailTile;
        m_isUserTile = a_tile is IUsersTile;
    }

    private List<T> m_selectedObjects;
    private List<T> m_loadedObjects;
    private bool m_foreground;
    private readonly object m_processLock = new ();
    private readonly object m_dataLock = new ();
    private bool m_waitingOnLock;
    private bool m_processingData;
    private readonly IScenarioInfo m_scenarioInfo;
    private bool m_reload;
    private bool m_reloading;
    private readonly IScenarioTile<T> m_tile;
    private readonly bool m_isScenarioDetailTile;
    private readonly bool m_isUserTile;

    private async Task LockAndProcessData()
    {
        if (m_isScenarioDetailTile || m_isUserTile)
        {
            if (!LockPreProcessing())
            {
                //we are already waiting on a lock and we don't need to reload.
                return;
            }
        }

        if (m_isScenarioDetailTile)
        {
            LockResult result = await m_scenarioInfo.DataLock.CreateNewLock().RunLockCodeBackground(LockAndProcessScenarioData);
            if (result.Status is EResultStatus.Error or EResultStatus.Canceled)
            {
                return;
            }
        }

        if (m_isUserTile)
        {
            await m_scenarioInfo.DataLock.CreateNewLock().RunLockCodeBackground(LockAndProcessUserData);
        }
    }

    private bool LockPreProcessing()
    {
        bool reload = false;
        lock (m_processLock)
        {
            if (m_waitingOnLock)
            {
                return false;
            }

            if (m_processingData)
            {
                reload = m_reload = true; //the objects changed after the load started
            }

            if (!reload)
            {
                m_waitingOnLock = true;
            }
        }

        if (reload)
        {
            //Can't cancel in process lock.
            CancelLoad();
        }

        return true;
    }

    private void LockAndProcessScenarioData(ScenarioManager a_sm, Scenario a_s, ScenarioDetail a_sd, params object[] a_params)
    {
        if (m_reloading)
        {
            return;
        }

        ReloadPreProcessing();

        //Reload individual objects
        ((IScenarioDetailTile)m_tile).Reload(a_sd, m_foreground);

        ReloadPostProcessing();
    }

    private void LockAndProcessUserData(UserManager a_um, params object[] a_params)
    {
        if (m_reloading)
        {
            return;
        }

        ReloadPreProcessing();

        //Reload individual objects
        ((IUsersTile)m_tile).Reload(a_um, m_foreground);

        ReloadPostProcessing();
    }

    private void ReloadPreProcessing()
    {
        lock (m_processLock)
        {
            m_reload = false;
            m_waitingOnLock = false;
            m_processingData = true;
            m_reloading = true;
        }
    }

    private void ReloadPostProcessing()
    {
        lock (m_processLock)
        {
            m_processingData = false;
            m_reloading = false;
            if (m_reload)
            {
                Task.Run(() => LockAndProcessData());
            }
        }
    }

    internal void ObjectsSelected(List<T> a_objects, bool a_activating = false)
    {
        m_selectedObjects = a_objects;
        bool reload = m_tile.ProcessSelectedObjects(m_selectedObjects);
        if (reload && !a_activating)
        {
            LockAndProcessData();
        }
    }

    internal void ObjectsLoaded(List<T> a_objects, bool a_activating = false)
    {
        m_loadedObjects = a_objects;
        bool reload = m_tile.ProcessLoadedObjects(m_loadedObjects);
        if (reload && !a_activating)
        {
            LockAndProcessData();
        }
    }

    internal Task Reload(bool a_foreground)
    {
        lock (m_dataLock)
        {
            m_reload = true;
            m_foreground = a_foreground;
        }

        return LockAndProcessData();
    }

    internal void CancelLoad()
    {
        m_tile.CancelLoad();
    }
}