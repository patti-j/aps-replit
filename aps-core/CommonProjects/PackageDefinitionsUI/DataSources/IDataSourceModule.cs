using System.Windows.Forms;

using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.PackageInterfaces;

namespace PT.PackageDefinitionsUI.DataSources;

public interface IDataSourceModule
{
    List<IDataSourceModuleElement> GenerateModules(IScenarioInfo a_scenarioInfo, IWorkspaceInfo a_settingsManager);
}

public interface IDataSourceModuleElement : IPackageElement
{
    List<IDataSourceModuleControl> ExtensionControls { get; }
}

public interface IDataSourceExtensionElement : IDataSourceModuleElement
{
    /// <summary>
    /// Allows overriding the object value
    /// </summary>
    /// <param name="a_property">Object property for column</param>
    /// <param name="a_propValue">Value returned for this column</param>
    /// <returns>Whether this value should not be modified further</returns>
    bool OverridePropValue(IObjectProperty a_property, ref LookUpValueStruct a_propValue);

    /// <summary>
    /// The order in which this extension will be provided the value to override in relation to other elements
    /// Lower value is given high priority.
    /// </summary>
    int Priority { get; }
}

public interface IDataSourceModuleControl
{
    /// <summary>
    /// The underlying control to add to the dataSource extensions area.
    /// Note: this control should already be sized correctly.
    /// </summary>
    Control ExtensionControl { get; }

    /// <summary>
    /// Priority determines how the controls will be ordered from their associated edge (left or right)
    /// Higher priority controls will be closer to the edge
    /// </summary>
    uint Priority { get; }
}