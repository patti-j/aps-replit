using System.Windows.Forms;

using DevExpress.Utils.Svg;
using DevExpress.XtraBars;

using PT.PackageDefinitions;
using PT.Transmissions;

namespace PT.PackageDefinitionsUI;

public interface ICustomDXBarMenuElement : IBarMenuElement
{
    BarButtonItem GenerateCustomMenu(BarManager a_barManager);
}

public interface IBarMenuElement : IBaseBarButtonElement
{
    PackageEnums.EBarMenuSortByType SortByType { get; }
}

public interface IScenarioBarMenuElement : IBarButtonElement { }

public interface IBarButtonElement : IBaseBarButtonElement
{
    /// <summary>
    /// The Menu bar button style
    /// </summary>
    BarButtonStyle ButtonStyle { get; }

    /// <summary>
    /// Sets the initial visibility of the menu bar button
    /// </summary>
    bool InitialVisibility { get; }

    /// <summary>
    /// Handle the button click
    /// </summary>
    void ButtonClicked();

    string RequiredUserPermission { get; }
}

public interface IBarControlElement : IBarElementUpdater
{
    /// <summary>
    /// The string key that associates this IBarControlElement to its parent Menu bar button dropdown
    /// </summary>
    string BarItemKey { get; }

    /// <summary>
    /// Whether this is a LayoutControl that manages it's own content or a single control
    /// </summary>
    bool IsLayoutControl { get; }

    /// <summary>
    /// The Control instance to display in the UI
    /// </summary>
    Control ControlInstance { get; }

    /// <summary>
    /// This can be used to hide the parent Menu bar button if certain conditions are met
    /// </summary>
    bool HideBarItem { get; }

    /// <summary>
    /// Whether this control needs to be laid out Vertically or Horizontally in the Menu bar button dropdown
    /// </summary>
    bool LayoutVertically { get; }

    /// <summary>
    /// Collects a transmission to save settings
    /// </summary>
    /// <returns></returns>
    PTTransmission Save();

    /// <summary>
    /// Initialize the control
    /// </summary>
    void Initialize();
}

public interface IBaseBarButtonElement : IBarElementUpdater, IDisposable
{
    /// <summary>
    /// The Menu bar button caption
    /// </summary>
    string Caption { get; }

    /// <summary>
    /// The Menu bar button SuperTip title
    /// </summary>
    string SuperTipTitle { get; }

    /// <summary>
    /// The Menu bar button SuperTip Content
    /// </summary>
    string SuperTipContent { get; }

    /// <summary>
    /// The Menu bar button SVG image
    /// </summary>
    SvgImage BarItemImage { get; }

    /// <summary>
    /// Whether to dock the Menu bar button on the right or left side of the toolbar
    /// </summary>
    bool DockRight { get; }
}

public interface IBarElementUpdater : IBarElement
{
    /// <summary>
    /// Can be used to update the Menu bar button
    /// </summary>
    /// <param name="a_item"></param>
    void UpdateButton(BarItem a_item);

    /// <summary>
    /// Event to refresh the Menu bar button
    /// </summary>
    event Action<IBarElement> DisplayRefresh;
}

public interface IBarElement : IPackageElement
{
    string ElementKey { get; }
    uint PositionPriority { get; }
}