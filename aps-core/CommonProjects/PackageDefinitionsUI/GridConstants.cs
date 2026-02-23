using System.Reflection;

// ReSharper disable InconsistentNaming

namespace PT.PackageDefinitionsUI;

public static class GridConstants
{
    public static readonly string AttributeSuffixCode = "_ptattcode";
    public static readonly string AttributeSuffixNbr = "_ptattnbr";
    public static readonly string AttributeSuffixColor = "_ptattclr";
    public static readonly string UDFSuffix = "_ptudf";

    public const string ActivityPropPrefix = "Activity@";
    public const string BatchPropPrefix = "Batch@";
    public const string DepartmentPropPrefix = "Department@";
    public const string ForecastVisualizationPropPrefix = "ForecastVisualization@";
    public const string InventoryPropPrefix = "Inventory@";
    public const string StorageAreaPropPrefix = "StorageArea@";
    public const string ItemPropPrefix = "Item@";
    public const string JobPropPrefix = "Job@";
    public const string MaterialsPropPrefix = "Materials@";
    public const string MoPropPrefix = "MO@";
    public const string OperationPropPrefix = "Operation@";
    public const string PlantPropPrefix = "Plant@";
    public const string PoPropPrefix = "PurchaseOrder@";
    public const string ResourcePropPrefix = "Resource@";
    public const string ResourceBlockPropPrefix = "ResourceBlock@";
    public const string ResourceRequirementPropPrefix = "ResourceRequirement@";
    public const string WarehousePropPrefix = "Warehouse@";
    public const string SoPropPrefix = "SalesOrder@";
    public const string SoLinePropPrefix = "SalesLine@";
    public const string SoLineDistPropPrefix = "SalesDistribution@";
    public const string TemplatePropPrefix = "Template@";
    public const string UserPropPrefix = "User@";
    public const string CustomerPropPrefix = "Customer@";

    private static readonly List<string> s_constants;

    static GridConstants()
    {
        s_constants = typeof(GridConstants).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                           .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                                           .Select(constant => (string)constant.GetRawConstantValue())
                                           .ToList();
    }

    public static string RemoveSuffix(string a_key)
    {
        if (a_key.EndsWith(AttributeSuffixCode))
        {
            return a_key.Replace(AttributeSuffixCode, "");
        }

        if (a_key.EndsWith(AttributeSuffixNbr))
        {
            return a_key.Replace(AttributeSuffixNbr, "");
        }

        if (a_key.EndsWith(AttributeSuffixColor))
        {
            return a_key.Replace(AttributeSuffixColor, "");
        }

        if (a_key.EndsWith(UDFSuffix))
        {
            return a_key.Replace(UDFSuffix, "");
        }

        return a_key;
    }

    public static string RemovePropPrefix(string a_key)
    {
        if (a_key.Contains('@'))
        {
            foreach (string constant in s_constants)
            {
                if (a_key.StartsWith(constant))
                {
                    return a_key.Replace(constant, "");
                }
            }
        }

        return a_key;
    }

    public static string GetPrefixString(string a_key)
    {
        foreach (string constant in s_constants)
        {
            if (a_key.StartsWith(constant))
            {
                return constant.TrimEnd('@');
            }
        }

        return "";
    }

    public static string GetPrefix(string a_key)
    {
        foreach (string constant in s_constants)
        {
            if (a_key.StartsWith(constant))
            {
                return constant;
            }
        }

        return "";
    }
}