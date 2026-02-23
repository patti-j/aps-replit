using System.Drawing;

using PT.Common.Localization;

namespace PT.PackageDefinitions;

/// <summary>
/// A user control that is managed by a TileController and displayed by a TilePaneLayoutControl.
/// </summary>
public interface ITile : IPackageElement, IDisposable, ILocalizable
{
    /// <summary>
    /// Whether this is the primary tile in the tile layout collection.
    /// Typically this is the grid control
    /// </summary>
    bool Primary { get; set; }

    /// <summary>
    /// The Control.Key value. It is used to synchronize document layouts with tiles to initially load
    /// </summary>
    string TileKey { get; }

    /// <summary>
    /// Whether this tile is loaded in a visible document.
    /// It must set the Control.Visible property to the specified value. This is used to synchronize UI documents
    /// </summary>
    bool Shown { set; }

    /// <summary>
    /// The user permissions for this tile don't allow editing scenario model data
    /// </summary>
    void EnterReadOnly();

    /// <summary>
    /// The user permissions for this tile allow editing scenario model data
    /// </summary>
    void ExitReadOnly();

    /// <summary>
    /// The UI should not attempt to send transmissions or access the SD until reloaded
    /// This is typically called when a simulation has started
    /// </summary>
    void EnterDataLock();

    /// <summary>
    /// The UI may now send transmissions and process data
    /// </summary>
    void ExitDataLock();

    /// <summary>
    /// This control's data may be out of date. A change to the data happened, but the tile is not going to be reloaded
    /// </summary>
    void NotifyOutOfDate();

    /// <summary>
    /// Each tile should have an interface to show help
    /// </summary>
    void ShowHelp();

    /// <summary>
    /// Sets document color based on module.
    /// </summary>
    Color DocumentColor { get; set; }

    /// <summary>
    /// Specifies how this tile should be categorized
    /// </summary>
    string TileCategory { get; }

    Size GetDefaultFloatSize();

    /// <summary>
    /// Maximized state
    /// </summary>
    bool Maximized { get; set; }

    /// <summary>
    /// The permissions required to perform any actions on this tile that modify scenario data.
    /// The user will need all required permissions.
    /// </summary>
    IEnumerable<string> RequiredPermissions => Enumerable.Empty<string>();

    /// <summary>
    /// This is a validation method to make sure the tile Handle has been created.
    /// The recommended place to CreateHandle() is after InitializeComponents() in the constructor.
    /// </summary>
    void EnsureHandleCreated();
}