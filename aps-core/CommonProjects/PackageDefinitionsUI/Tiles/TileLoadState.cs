namespace PT.PackageDefinitionsUI.Tiles;

/// <summary>
/// Keeps track of tile loading properties to keep the reload process in sync
/// </summary>
public class TileLoadState
{
    /// <summary>
    /// The error that occurred on loading
    /// </summary>
    private Exception m_loadError;

    public bool Error => m_loadError != null;

    /// <summary>
    /// The tile has begun loading
    /// </summary>
    public bool Loaded;

    /// <summary>
    /// Specify the error that occurred during loading
    /// </summary>
    /// <param name="a_loadError"></param>
    public void LoadError(Exception a_loadError)
    {
        m_loadError = a_loadError;
    }
}