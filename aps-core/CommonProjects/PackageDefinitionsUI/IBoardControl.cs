namespace PT.PackageDefinitionsUI;

/// <summary>
/// The base interface for a Board Control. Most of the boards that interact with
/// scenario data will be implemented by TileBoardLayoutControl
/// <T>
/// where T is
/// some type of key related to the type of scenario data the board interacts with.
/// Many of the settings and ancillary functionalities can be interacted with
/// through their own, non-generic board control though.
/// </summary>
public interface IBoardControl
{
    /// <summary>
    /// The Name of the board. Many of the controls that
    /// implement IBoardControl will grab this from the Name property of the
    /// Windows form control. The main exception being the TileBoardLayoutControl
    /// which forms this by pre-pending a board constant to the BoardKey.
    /// </summary>
    string BoardControlName { get; }

    /// <summary>
    /// A string that is displayed on the board that is a localized value of
    /// the BoardKey. This can show up in different ways, but it's commonly the
    /// title of the board.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// A key associated with the board. There are a list of BoardKeys in Constants.cs,
    /// and the BoardKey is typically set in the board's corresponding BoardModules file.
    /// </summary>
    string BoardKey { get; set; }

    /// <summary>
    /// A bool that indicates whether or not the Board is visible.
    /// Setting this value will often trigger certain events being fired.
    /// </summary>
    bool BoardVisible { get; set; }

    /// <summary>
    /// A bool that indicates whether or not the board is active.
    /// A board being active just means it's been loaded, but doesn't
    /// necessarily mean it's visible. For example, if there are multiple
    /// boards in a single document group, only the selected board will be
    /// visible while the rest of the boards will be active, but not visible.
    /// Setting this value can also trigger certain events firing.
    /// </summary>
    bool BoardActive { get; set; }

    /// <summary>
    /// This function should reload the board. There are events related to changes in scenario
    /// data that will automatically call this function in some of the board/tile controllers.
    /// </summary>
    void ReloadBoard();

    /// <summary>
    /// This function needs to be called by ScenarioViewer to load User settings once a workspace has been
    /// activated
    /// </summary>
    void LoadUserSettings();

    /// <summary>
    /// The board is being unloaded, likely because the client is logging off
    /// Remove event listeners, timers, and anything that will trigger loading data
    /// </summary>
    void Unload();
}