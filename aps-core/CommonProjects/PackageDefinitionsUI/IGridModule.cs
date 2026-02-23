using System.Windows.Forms;

using DevExpress.XtraGrid.Views.Grid;

using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.DataSources;

namespace PT.PackageDefinitionsUI;

/// <summary>
/// This module creates and maintains settings for all grid extensions
/// </summary>
public interface IGridModule : IPackageModule
{
    List<IGridModuleElement> GenerateDataSourcedModules(IScenarioInfo a_scenarioInfo, GridView a_gridView, IWorkspaceInfo a_settingsManager, IGridDataSourceModule a_gridDataSource);

    List<IGridModuleElement> GenerateBaseModules(IScenarioInfo a_scenarioInfo, GridView a_gridView)
    {
        return new List<IGridModuleElement>(0);
    }
}

public interface IGridModuleElement : IPackageElement, IDisposable
{
    List<IGridModuleControl> ExtensionControls { get; }
}

public interface IGridModuleExtensionElement : IGridModuleElement
{
    /// <summary>
    /// Allows overriding the object value
    /// </summary>
    /// <param name="a_columnKey">Grid column property key</param>
    /// <param name="a_cellValue">Value returned for this column</param>
    /// <returns>Whether this value should not be modified further</returns>
    bool OverrideCellValue(string a_columnKey, ref object a_cellValue);

    /// <summary>
    /// The order in which this extension will be provided the value to override in relation to other elements
    /// Lower value is given high priority.
    /// </summary>
    int Priority { get; }
}

public interface IGridModuleControl
{
    /// <summary>
    /// The underlying control to add to the grid extensions area.
    /// Note: this control should already be sized correctly.
    /// </summary>
    List<Control> ExtensionControls { get; }

    /// <summary>
    /// Priority determines how the controls will be ordered from their associated edge (left or right)
    /// Higher priority controls will be closer to the edge
    /// </summary>
    uint Priority { get; }

    /// <summary>
    /// Whether to dock the control on the right side instead of the left
    /// </summary>
    DockStyle DockPosition => DockStyle.Top;

    /// <summary>
    /// Fire to change extension control visibility
    /// </summary>
    event Action<IGridModuleControl, bool> UpdateVisibility;
}