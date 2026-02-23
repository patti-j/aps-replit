using PT.PackageDefinitionsUI.PackageInterfaces;

namespace PT.PackageDefinitionsUI.DataSources;

public class DataSourceUtilities
{
    public static void InitializeDataSourceExtensions<T>(IPackageManagerUI a_packageManager, IMainForm a_mainForm, UnboundDataSourceModule<T> a_dataSource)
    {
        //Attach dataSource extensions
        List<IDataSourceModule> dataSourceModules = a_packageManager.GetDataSourceModules();
        foreach (IDataSourceModule dataSourceModule in dataSourceModules)
        {
            List<IDataSourceModuleElement> dataSourceModuleElements = dataSourceModule.GenerateModules(a_mainForm.ScenarioInfo, a_mainForm.WorkspaceInfo);
            foreach (IDataSourceModuleElement dataSourceModuleElement in dataSourceModuleElements)
            {
                if (dataSourceModuleElement is IDataSourceExtensionElement extension)
                {
                    a_dataSource.RegisterExtensionModule(extension);
                }
            }
        }
    }

    public static void LoadDataSourceExtensions(IPackageManagerUI a_packageManager, IMainForm a_mainForm, IScenarioInfo a_scenarioInfo, ref List<IDataSourceExtensionElement> a_extensionModules)
    {
        //Load dataSource extensions
        List<IDataSourceModule> dataSourceModules = a_packageManager.GetDataSourceModules();

        foreach (IDataSourceModule dataSourceModule in dataSourceModules)
        {
            List<IDataSourceModuleElement> dataSourceModuleElements = dataSourceModule.GenerateModules(a_scenarioInfo, a_mainForm.WorkspaceInfo);
            foreach (IDataSourceModuleElement dataSourceModuleElement in dataSourceModuleElements)
            {
                if (dataSourceModuleElement is IDataSourceExtensionElement extension)
                {
                    a_extensionModules.Add(extension);
                }
            }
        }
    }

    public static object GetOverrideValue(IObjectProperty a_property, object propValue, List<IDataSourceExtensionElement> a_extensionModules)
    {
        if (propValue is List<LookUpValueStruct> lookUpValueList)
        {
            for (int i = 0; i < lookUpValueList.Count; i++)
            {
                foreach (IDataSourceExtensionElement extensionModule in a_extensionModules)
                {
                    LookUpValueStruct value = lookUpValueList[i];
                    bool keepChanges = extensionModule.OverridePropValue(a_property, ref value);
                    if (keepChanges)
                    {
                        lookUpValueList[i] = value;
                        break;
                    }
                }
            }
        }
        else if (propValue is LookUpValueStruct lookUpValue)
        {
            foreach (IDataSourceExtensionElement extensionModule in a_extensionModules)
            {
                bool keepChanges = extensionModule.OverridePropValue(a_property, ref lookUpValue);
                if (keepChanges)
                {
                    break;
                }
            }
        }

        return propValue;
    }
}