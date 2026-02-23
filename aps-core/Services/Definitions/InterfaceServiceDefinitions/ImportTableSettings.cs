using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses.Webapp;
using PT.Transmissions;
using System.ComponentModel;
using System.Data;
using System.Text.RegularExpressions;

namespace PT.ImportDefintions;

/// <summary>
/// Extends the original MapStepSettingsClass to work with enabled Features
/// </summary>
[JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
public class ImportTableSettings : ICloneable
{
    private const string c_displayOnlyPrefix = "_";
    private const string c_blankNextExpression = "''";
    private const string c_select = "SELECT ";

    [JsonIgnore]
    internal NewImportSettings ImportSettings { get; set; }


    public ImportTableSettings() { }

    public ImportTableSettings(Type a_objectType, NewImportSettings a_aImportSettings, string a_tableName) 
    {
        ImportSettings = a_aImportSettings;
        TypeFullName = a_objectType.AssemblyQualifiedName;
        TypeName = a_objectType.Name;
        TableName = a_tableName;
        DistinctRows = true;
        AutoDelete = true;

        // TODO: Handle default differences from base
        FromjoinExpression = TableName;
        
        RebuildImportData();
    }


    public string TableName { get; set; }

    public string TypeName { get; set; }

    /// <summary>
    /// Full reference name of underyling data type so that the existing settings can rebuild 
    /// </summary>
    public string TypeFullName { get; set; }


    public bool AutoDelete { get; set; }

    public bool DistinctRows { get; set; }

    public string FromjoinExpression { get; set; } = "";

    public string WhereExpression { get; set; } = "";

    public MapInfoArray MapInfos { get; set; }

    // Contains the import 
    private string m_cachedStandardImportCommand { get; set; }

    protected void SetMapInfosForType(Type type, List<IntegrationFeatureBase> a_featureSubset = null)
    {
        MapInfos = GetMapInfosForType(type, a_featureSubset);
    }

    private MapInfoArray GetMapInfosForType(Type type, List<IntegrationFeatureBase> a_featureSubset)
    {
        MapInfoArray mapInfos = new MapInfoArray();

        //Transitioning to use datasets instead of readers.
        if (type.Name.EndsWith("Table"))
        {
            DataTable table = (DataTable)Activator.CreateInstance(type);
            mapInfos = GetMapInfosForTableType(table, a_featureSubset);
        }
        else //use reflection
        {
            mapInfos = new MapInfoArray();
            MapInfo map;

            int propCount = TypeDescriptor.GetProperties(type).Count;
            PropertyDescriptor pd;
            for (int i = 0; i < propCount; i++)
            {
                pd = TypeDescriptor.GetProperties(type).Sort()[i]; //.Sort(GetSortStringArray(type))[i]; //sort by default sort specified in the object
                if (!pd.IsReadOnly && pd.IsBrowsable)
                {
                    map = new MapInfo();
                    map.PropertyName = pd.Name;
                    map.PropertyType = pd.PropertyType.Name;
                    map.PropertyDescription = pd.Description;
                    //Store whether the property is required or not.
                    // Set associated features, from some big lookup table? We need a way to associate features and properties
                    RequiredAttribute requiredAttribute = (RequiredAttribute)pd.Attributes[typeof(RequiredAttribute)];
                    if (requiredAttribute != null)
                    {
                        map.Required = requiredAttribute.Required;
                    }
                    else
                    {
                        map.Required = false;
                    }
                    if (PropertyExistsInFeature(pd.Name, map, a_featureSubset))
                    {
                        mapInfos.Add(map);
                    }
                }
            }
        }

        return mapInfos;
    }

    /// <summary>
    /// Determines if a property is present in any enabled features.
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="a_mapInfo"></param>
    /// <param name="a_featureSubset">Limits the features to check for this property, in case we only want to look at a subset og them (e.g. to find a slimmed down version with just one feature and its table baseline)</param>
    /// <returns></returns>
    private bool PropertyExistsInFeature(string columnName, MapInfo a_mapInfo, List<IntegrationFeatureBase> a_featureSubset = null)
    {
        bool useFeatureSubset = !(a_featureSubset == null || a_featureSubset.Count == 0);

        // TODO: review this if we start having properties mapped to more than one feature

        foreach (IntegrationFeatureBase feature in ImportSettings.FeaturesSettings.AllFeatures.Values
                 .Where(feature => !useFeatureSubset || a_featureSubset.Any(subsetFeature => feature.FeatureName == subsetFeature.FeatureName)))
        {
            if (!feature.Enabled)
            {
                continue;
            }

            List<IntegrationProperty> featureProperties = feature.Properties;
            IntegrationProperty prop = featureProperties.FirstOrDefault(property => property.ColumnName == columnName && property.TableName == TableName);
            if (prop == null)
            {
                continue;
            }
            else
            {
                if (prop.SourceOption == EPropertySourceOption.FixedValue)
                {
                    a_mapInfo.SourceExpression = $"'{prop.FixedValue}'";
                    return true;
                }
                if (prop.SourceOption == EPropertySourceOption.FromTable)
                {
                    return true;
                } //currently properly handles keep value and from table, need to implement the rest TODO
                continue;
            }
        }
        return false;
    }

    /// <summary>
    /// Set the Property names from the table object using Reflection.
    /// </summary>
    protected MapInfoArray GetMapInfosForTableType(DataTable table, List<IntegrationFeatureBase> a_featureSubset = null)
    {
        MapInfoArray mapInfos = new MapInfoArray();
        MapInfo map;

        int columnCount = table.Columns.Count;

        for (int i = 0; i < columnCount; i++)
        {
            DataColumn col = table.Columns[i];
            map = new MapInfo();
            bool markedForImport = col.ExtendedProperties["Non-Importable"] == null && PropertyExistsInFeature(col.ColumnName, map, a_featureSubset);

            if (markedForImport) //Some fields are for use in the user interface as display only -- can't import them.
            {
                
                map.PropertyName = col.ColumnName; //Must match dataset col name
                map.PropertyType = col.DataType.Name;
                map.SourceExpression = string.IsNullOrEmpty(map.SourceExpression) ? col.ColumnName : map.SourceExpression; // For v2 integration, prop name always matches the column
                //map.PropertyDescription = pd.Description;
                //Store whether the property is required or not.
                map.Required = !col.AllowDBNull;
                mapInfos.Add(map);
            }
        }

        return mapInfos;
    }

    public void Update(ImportTableSettings a_newSettings)
    {
        ImportSettings = a_newSettings.ImportSettings;
        AutoDelete = a_newSettings.AutoDelete;
        DistinctRows = a_newSettings.DistinctRows;
        MapInfos = a_newSettings.MapInfos;
        RebuildImportData();
    }

    /// <summary>
    /// Updates import settings with incoming webapp configuration.
    /// </summary>
    /// <param name="a_featureDto"></param>
    /// <param name="a_newImportSettings"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Update(FeatureDTO a_featureDto, NewImportSettings a_newImportSettings)
    {
        DistinctRows = a_featureDto.Distinct ?? false;
        AutoDelete = a_featureDto.AutoDelete ?? false;
        RebuildImportData(a_newImportSettings);
    } 

    /// <summary>
    /// Rebuilds and caches standard SQL queries so that they don't have to be determined at import time
    /// </summary>
    public void RebuildImportData(NewImportSettings a_settings = null)
    {
        // Uncommenting this will allow you to see if the IntegrationFeatures are out of sync with the data sets, as this method is fired for every data table used in the import schema
        // TODO: this would be nice as part of a unit test someday!

        #if DEBUG
        //Test_IsFeatureDataSetInSync();
        #endif

        if (a_settings != null)
        {
            ImportSettings = a_settings;
        }

        SetMapInfosForType(Type.GetType(TypeFullName));

        // Cache standard import query
        m_cachedStandardImportCommand = GetCommandTextInternal(true);
    }


    public string GetCommandText(bool a_usingNewTableQuery, int a_rowLimit = 0, List<IntegrationFeatureBase> a_featureSubset = null, bool a_grabAllColumns = false)
    {
        bool isStandardImportQuery = a_usingNewTableQuery && a_rowLimit == 0 && (a_featureSubset == null || a_featureSubset.Count == 0);

        if (isStandardImportQuery && !string.IsNullOrEmpty(m_cachedStandardImportCommand) && a_grabAllColumns == false)
        {
            return m_cachedStandardImportCommand;
        }

        if (a_grabAllColumns == true)
        {
            return c_select + $" * FROM {TableName}";
        }

        string command = GetCommandTextInternal(a_usingNewTableQuery, a_rowLimit, a_featureSubset);
        if (isStandardImportQuery)
        {
            m_cachedStandardImportCommand = command;
        }

        return command;
    }

    /// <summary>
    /// Builds a sql command for the provided subset of features, rather than all enabled ones.
    /// This does not impact the configuration of the this <see cref="ImportTableSettings"/>.
    /// Since it differs from standard config, does not use the <see cref="m_cachedStandardImportCommand"/>.
    /// </summary>
    /// <param name="a_usingNewTableQuery"></param>
    /// <param name="a_rowLimit"></param>
    /// <returns></returns>
    public string GetCommandTextSubset(bool a_usingNewTableQuery, List<IntegrationFeatureBase> a_featureSubset = null, int a_rowLimit = 0)
    {
        string command = GetCommandTextInternal(a_usingNewTableQuery, a_rowLimit, a_featureSubset);

        return command;
    }


    public string GetCommandTextInternal(bool a_usingNewTableQuery, int a_rowLimit = 0, List<IntegrationFeatureBase> a_featureSubset = null)
    {
        if (!HasNonEmptySourceExpression())
        {
            return string.Empty;
        }

        MapInfoArray mappingsForQuery = MapInfos;
        if (!(a_featureSubset == null || a_featureSubset.Count == 0))
        {
            mappingsForQuery = GetMapInfosForType(Type.GetType(TypeFullName), a_featureSubset);
            if (!HasNonEmptySourceExpression(mappingsForQuery))
            {
                return string.Empty;
            }
        }

        string cText = c_select;

        if (DistinctRows)
        {
            cText += " DISTINCT ";
        }

        if (a_rowLimit > 0)
        {
            cText += $" TOP {a_rowLimit} ";
        }

        if (a_usingNewTableQuery)
        {
            string leftWrapChar = "";
            string rightWrapChar = "";
            if (ImportSettings?.ReservedWordsWrapper != null && ImportSettings.ReservedWordsWrapper?.Length >= 2)
            {
                leftWrapChar = ImportSettings.ReservedWordsWrapper.Substring(0, 1);
                rightWrapChar = ImportSettings.ReservedWordsWrapper.Substring(1, 1);
            }

            //FIELDS
            bool addedFirstField = false;
            for (int i = 0; i < mappingsForQuery.Count; i++)
            {
                MapInfo mapInfo = mappingsForQuery[i];
                // Check to see if enabled features contain that prop
                // Foreach (feature)
                // foreach feature.property ()
                // if(property.Name && property.TableName == the ones here)
                // Add this to sql command 

                string nextExpression = !string.IsNullOrWhiteSpace(mapInfo.SourceExpression) ? mapInfo.SourceExpression : mapInfo.PropertyName;

                if (!string.IsNullOrWhiteSpace(nextExpression))
                {
                    if (addedFirstField) //already have a field so need a comma
                    {
                        cText = cText + ",";
                    }

                    if (nextExpression.Trim().ToUpperInvariant() == mapInfo.PropertyName.Trim().ToUpperInvariant())
                    {
                        cText = $"{cText} {nextExpression}"; //Can't have AS with the same alias as the field name.  Returns error: Circular reference caused by alias '<field name>' in query definition's SELECT list.                             
                    }
                    else
                    {
                        nextExpression = RemoveAsFromExpression(nextExpression);
                        cText = $"{cText} {nextExpression} as {leftWrapChar}{mapInfo.PropertyName}{rightWrapChar}"; //For MISYS Prospect running Pervasive, removed brackets that were around {2}.  Was giving <<???>> error.
                    }

                    addedFirstField = true;
                }
            }

            //FROM
            string fromExpression = $" FROM {TableName}";

            //WHERE
            // string tempWhereExpression = "";
            // if (WhereExpression.Trim().Length > 0)
            // {
            //     tempWhereExpression = $" WHERE {WhereExpression.Trim()}";
            // }

            return cText + fromExpression; //+ tempWhereExpression;
        }
        else
        {
            string externalId = "";
            string plantExternalId = "";
            string departmentExternalId = "";
            string resourceExternalId = "";

            //FIELDS
            for (int i = 0; i < mappingsForQuery.Count; i++)
            {
                MapInfo mapInfo = mappingsForQuery[i];
                string nextExpression = mapInfo.SourceExpression;
                if (nextExpression.Trim().Length == 0)
                {
                    nextExpression = c_blankNextExpression; //Can't have blanks in sql
                }

                if (cText.Length > c_select.Length) //already have a field so need a comma
                {
                    cText = cText + ",";
                }

                cText = cText + " " + nextExpression;

                //Get values for ORDER BY clause for ResourceT Capability Assignment                        
                if (mapInfo.PropertyName == "ExternalId")
                {
                    externalId = nextExpression;
                }

                if (mapInfo.PropertyName == "PlantExternalId")
                {
                    plantExternalId = nextExpression;
                }

                if (mapInfo.PropertyName == "DepartmentExternalId")
                {
                    departmentExternalId = nextExpression;
                }

                if (mapInfo.PropertyName == "ResourceExternalId")
                {
                    resourceExternalId = nextExpression;
                }
            }

            string orderByExpression = "";

            orderByExpression = AddOrderBy(orderByExpression, externalId); //this has to be before the PlantExternalId, DeptExternalId, WarehouseEternalId or else it will be ingored since it's a substring of them.

            //ResourceT
            orderByExpression = AddOrderBy(orderByExpression, plantExternalId);
            orderByExpression = AddOrderBy(orderByExpression, departmentExternalId);
            orderByExpression = AddOrderBy(orderByExpression, resourceExternalId);

            orderByExpression = RemoveAsFromExpression(orderByExpression);

            string tempOrderBy = orderByExpression;
            orderByExpression = orderByExpression.Replace(",,", ","); //get rid of blanks in order by
            if (orderByExpression.Length > 0)
            {
                tempOrderBy = " ORDER BY " + orderByExpression + " ASC";
            }

            //FROM
            string fromExpression = " FROM " + TypeName;

            //WHERE
            // string tempWhereExpression = "";
            // if (WhereExpression.Trim().Length > 0)
            // {
            //     tempWhereExpression = " WHERE " + WhereExpression.Trim();
            // }

            //ORDER BY
            cText = cText + fromExpression /*+ tempWhereExpression */+ tempOrderBy;

            return cText;
        }
    }

    private string AddOrderBy(string originalExpression, string addOnExpression)
    {
        string output;
        if (addOnExpression == c_blankNextExpression)
        {
            output = originalExpression;
        }
        else
        {
            addOnExpression = RemoveAsFromExpression(addOnExpression); //can't have 'AS alias' in Order By clause
            if (originalExpression.Length > 0)
            {
                //Don't add it if it's already in the list since this is not allowed. Using "," below to avoid cases where the new field is a part of the name of some other field.
                if (originalExpression.Contains(addOnExpression))
                {
                    output = originalExpression;
                }
                else
                {
                    output = string.Format("{0},{1}", originalExpression, addOnExpression);
                }
            }
            else
            {
                output = addOnExpression;
            }
        }

        return output;
    }

    /// <summary>
    /// Returns the express minus the ' as alias' suffix if any.
    /// </summary>
    private string RemoveAsFromExpression(string expression)
    {
        string[] result = Regex.Split(expression, " AS ", RegexOptions.IgnoreCase);
        if (result[0].IndexOf("cast(", StringComparison.CurrentCultureIgnoreCase) == -1)
        {
            return result[0];
        }

        return expression;
    }

    public bool HasNonEmptySourceExpression(MapInfoArray a_mapInfos = null)
    {
        MapInfoArray mapInfos = a_mapInfos ?? MapInfos;
        if (mapInfos == null || mapInfos.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < mapInfos.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(mapInfos[i].SourceExpression))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Testing method to make sure the two sources of truth for schema fields (PT DB DataSets and IntegrationFeatures) are in sync -
    /// ie, that all fields are captured in both sets of data.
    /// The ImportTableSettings class acts as a bridge between these two sources of truth, and so allows easy testing.
    /// To test the full data set, this method should be run for every ImportTableSetting prop in NewImportSettings (since there's one per DataTable).
    /// This should be used for development purposes only, but has no side effects other than slowness.
    /// To use this, uncomment the debug call to it in 
    /// </summary>
    private void Test_IsFeatureDataSetInSync()
    {
        #if DEBUG
        List<IntegrationProperty> propsMissingFromDataTable = new (); // this likely means it was purposefully removed from the PT model
        List<string> propsMissingFromIntegrationFeatureClasses = new (); // this likely was forgotten
        List<(IntegrationProperty property, Type DatasetDataType)> propsWithMismatchedDataTypes = new (); // integration feature should be using dataset type


        // Use dummy feature settings with everything enabled (so if the prop is mapped in any feature, we'll know about it)
        ImportFeaturesSettings testImportFeastFeaturesSettings = new ImportFeaturesSettings();
        foreach (IntegrationFeatureBase feature in testImportFeastFeaturesSettings.AllFeatures.Values)
        {
            feature.Enabled = true;
        }

        // Find everything defined in a feature (TODO: include feature they are in)
        List<IntegrationProperty> allTablePropsInAllFeatures = testImportFeastFeaturesSettings.GetPropertiesUsingTable(TableName, true, prop => true).ToList();

        // Iterate over this class's assigned table, and the full list of features.
        // This has no side effects to the current class.
        DataTable table = (DataTable)Activator.CreateInstance(Type.GetType(TypeFullName));
        int columnCount = table.Columns.Count;

        for (int i = 0; i < columnCount; i++)
        {
            DataColumn col = table.Columns[i];
            bool markedForImport = col.ExtendedProperties["Non-Importable"] == null; // TODO: I don't see why we couldn't set this kind of prop in our data sets to map to a feature name

            if (markedForImport)
            {
                IntegrationProperty matchInFeatures = allTablePropsInAllFeatures.FirstOrDefault(prop => prop.ColumnName.Equals(col.ColumnName, StringComparison.OrdinalIgnoreCase));
                if (matchInFeatures == null)
                {
                    propsMissingFromIntegrationFeatureClasses.Add(col.ColumnName);
                }
                else
                {
                    // Check data type match
                    if (!TestHelper_MatchDataTypes(matchInFeatures.DataType, col.DataType))
                    {
                        propsWithMismatchedDataTypes.Add((matchInFeatures, col.DataType));
                    }

                    // Remove from overall list of props. TODO: this will break if a prop is in multiple features. I don't think that's supported right now, but keep an eye out
                    allTablePropsInAllFeatures.Remove(matchInFeatures);
                }
            }
        }

        // Any remaining props from features are 
        if (allTablePropsInAllFeatures.Any())
        {
            propsMissingFromDataTable.AddRange(allTablePropsInAllFeatures);
        }

        if (propsMissingFromDataTable.Any() ||
            propsMissingFromIntegrationFeatureClasses.Any() ||
            propsWithMismatchedDataTypes.Any())
        {
            // Out of sync! Steps need to be taken to get these aligned.
            // The person adding the fields to the data set is probably the best person to do this
            // (e.g. knowing what feature to add a missing prop to) but I suspect it can generally be guessed.
            throw new Exception($"Props are out of sync for table {TableName}");
        }

        #endif
    }

    /// <summary>
    ///  Matches DataSet column datatypes to corresponding EPropertyDataType enum vals. Currently for testing, but could be a real helper method.
    /// </summary>
    /// <param name="a_featureDataType"></param>
    /// <param name="a_dataSetDataType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private bool TestHelper_MatchDataTypes(EPropertyDataType a_featureDataType, Type a_dataSetDataType)
    {
        // TODO:
        switch (a_dataSetDataType)
        {
            case var t when t == typeof(bool):
                return a_featureDataType == EPropertyDataType.Boolean;
            case var t when t == typeof(byte):
                return a_featureDataType == EPropertyDataType.Byte;
            case var t when t == typeof(DateTime):
                return a_featureDataType == EPropertyDataType.DateTime;
            case var t when t == typeof(decimal):
                return a_featureDataType == EPropertyDataType.Decimal;
            case var t when t == typeof(double):
                return a_featureDataType == EPropertyDataType.Double;
            case var t when t == typeof(short):
                return a_featureDataType == EPropertyDataType.Short;
            case var t when t == typeof(int):
                return a_featureDataType == EPropertyDataType.Int;
            case var t when t == typeof(long):
                return a_featureDataType == EPropertyDataType.Long;
            case var t when t == typeof(string):
                return a_featureDataType == EPropertyDataType.String;
            default:
                throw new ArgumentException($"Data set contains an unhandled data type: {a_dataSetDataType}");
        }
    }

    #region ICloneable Members
        object ICloneable.Clone()
    {
        return Clone();
    }

    public ImportTableSettings Clone()
    {
        return (ImportTableSettings)MemberwiseClone();
    }
    #endregion
}