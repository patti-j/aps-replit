using PT.APSCommon.Extensions;
using PT.ComponentLibrary;

namespace PT.ScenarioControls.Tiles;

/// <summary>
/// The basic implementation of IBoardControl that many BoardControls
/// will inherit from.
/// </summary>
public partial class BoardControlBase : PTBaseControl
{
    public BoardControlBase()
    {
        InitializeComponent();
    }

    public string BoardControlName => Name;

    public string DisplayName => m_boardKey.Localize();

    /// <summary>
    /// This is prepended to the board key when storing settings related to the board.
    /// It is also prepended to the BoardKey to set the value of the Board's Name.
    /// </summary>
    protected const string c_boardPrefix = "IBoardControl_";

    private string m_boardKey;

    public string BoardKey
    {
        get => m_boardKey;
        set
        {
            m_boardKey = value;
            Name = c_boardPrefix + value;
        }
    }
}