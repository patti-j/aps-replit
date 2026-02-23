using PT.PackageDefinitions;
using PT.Scheduler;
using PT.UIDefinitions;

namespace PT.PackageDefinitionsUI;

/// <summary>
/// A user control that is managed by a TileController and displayed by a TilePaneLayoutControl.
/// </summary>
public interface IScenarioTile<T> : ITile
{
    /// <summary>
    /// When fired, this tile is requesting to be reloaded with the latest SD data
    /// Use this when the control is modified from an external event and needs to reload when the tile controller would not noramlly reload tiles
    /// </summary>
    event Action<IScenarioTile<T>> RequestReloadEvent;

    /// <summary>
    /// New objects were selected by the primary tile
    /// This is a generic object so that different tiles collections can use different collections of objects.
    /// </summary>
    /// <param name="a_objects"></param>
    bool ProcessSelectedObjects(IEnumerable<T> a_objects);

    bool ProcessLoadedObjects(List<T> a_objects);

    /// <summary>
    /// This tile won't be reloaded automatically on data model changes. The primary tile will trigger a reload from ProcessSelectedObjects or ProcessLoadedObjects
    /// </summary>
    bool SynchronizedToPrimaryTile => false;

    /// <summary>
    /// Cancel the current load, a new Reload will follow
    /// </summary>
    void CancelLoad();
}

//Different types of scenario loading

public interface IScenarioDetailTile : ITile
{
    /// <summary>
    /// Reload the control and exit ReadOnly.
    /// This function should also manage checking for Cancel
    /// </summary>
    void Reload(ScenarioDetail a_sd, bool a_foreground);
}

public interface IUsersTile : ITile
{
    /// <summary>
    /// Reload the control and exit ReadOnly.
    /// This function should also manage checking for Cancel
    /// </summary>
    void Reload(UserManager a_um, bool a_foreground);
}

public interface IUIEventTile
{
    void ProcessUIEventData(UINavigationEvent a_event);
}

/// <summary>
/// Any tile with this interface will be automatically reloaded after an undo finishes
/// </summary>
public interface ITileRequiresReloadAfterUndo { }