using System.Drawing;

using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors.Mask;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;

using PT.APSCommon;
using PT.Common.Extensions;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler;

namespace PT.PackageDefinitionsUI.DataSources;

/// <summary>
/// An implementation of IGridDatasourceModule, and it also inherits from DevExpress's
/// UnboundSource. This class itself is not often used directly. Instead, most of the
/// grids' data source will actually be an EditableUnboundDataSourceModule which extends
/// this class and allows the user to edit the properties directly from the grid.
/// </summary>
/// <typeparam name="T">Grid object type key</typeparam>
public class UnboundDataSourceModule<T> : UnboundSource, IGridDataSourceModule
{
    public UnboundDataSourceModule(GridView a_gridView, IScenarioInfo a_scenarioInfo)
    {
        m_gridView = a_gridView;
        m_scenarioInfo = a_scenarioInfo;
        ObjectIds = new object();

        a_scenarioInfo.ScenarioActivated += ScenarioInfoOnScenarioActivated;

        m_columnEditDateTime = new RepositoryItemDateEdit();
        m_columnEditDateTime.Mask.MaskType = MaskType.DateTime;
        m_columnEditDateTime.Mask.EditMask = "g";
        m_columnEditDateTime.Mask.UseMaskAsDisplayFormat = true;

        m_columnEditDate = new RepositoryItemDateEdit();
        m_columnEditDate.Mask.MaskType = MaskType.DateTime;
        m_columnEditDate.Mask.EditMask = "d";
        m_columnEditDate.Mask.UseMaskAsDisplayFormat = true;

        m_columnEditCurrency = new RepositoryItemTextEdit();
        m_columnEditCurrency.Mask.MaskType = MaskType.Numeric;
        m_columnEditCurrency.Mask.EditMask = "c";
        m_columnEditCurrency.Mask.UseMaskAsDisplayFormat = true;

        m_columnEditTimeSpan = new RepositoryItemTimeSpanEdit();
        m_columnEditTimeSpan.TimeEditStyle = TimeEditStyle.SpinButtons;
        m_columnEditTimeSpan.MaskSettings.ShowAdvancedSettings = true;
        m_columnEditTimeSpan.MaskSettings.MaskExpression = "[d'.']hh':'mm':'ss['.'f]";
        m_columnEditTimeSpan.UseMaskAsDisplayFormat = true;

        m_gridView.GridControl.RepositoryItems.Add(m_columnEditDateTime);
        m_gridView.GridControl.RepositoryItems.Add(m_columnEditCurrency);
        m_gridView.GridControl.RepositoryItems.Add(m_columnEditTimeSpan);
        m_gridView.GridControl.RepositoryItems.Add(m_columnEditDate);
    }

    private void ScenarioInfoOnScenarioActivated(Scenario a_arg1, ScenarioDetail a_sd, ScenarioEvents a_arg3)
    {
        //ClearData
        Reset(a_sd);
    }

    /// <summary>
    /// The grid associated with the UnboundDataSourceModule
    /// </summary>
    protected readonly GridView m_gridView;

    /// <summary>
    /// Exposes various scenario information to this module.
    /// </summary>
    protected readonly IScenarioInfo m_scenarioInfo;

    /// <summary>
    /// The keys of m_lockDict are the columns FieldName, and the value is used
    /// to lock the column when its values are being changed.
    /// </summary>
    private readonly Dictionary<string, object> m_lockDict = new ();

    /// <summary>
    /// The keys of m_propertyDict are the property/column's FieldName, and the values are
    /// IObjectProperty. This is used to manipulate the property that underlies each column.
    /// </summary>
    protected readonly Dictionary<string, IObjectProperty> m_propertyDict = new ();

    // These two lists below are used when writing extensions to override or extend its corresponding
    // GridModule/DataSource
    protected List<IGridModuleExtensionElement> m_extensionModules = new ();
    protected List<IDataSourceExtensionElement> m_dataExtensionModules = new ();

    // These Repository Items are just default repository items given to a column
    // depending on its data type. The repository item determines how a column's cell will 
    // look and behave when a user clicks on it. 
    private readonly RepositoryItemDateEdit m_columnEditDateTime;
    private readonly RepositoryItemDateEdit m_columnEditDate;
    private readonly RepositoryItemTextEdit m_columnEditCurrency;
    private readonly RepositoryItemTimeSpanEdit m_columnEditTimeSpan;

    /// <summary>
    /// Matches row index to the object's key
    /// </summary>
    protected Dictionary<int, T> m_rowIdxToObjectKeyDict = new ();

    /// <summary>
    /// list of grid filters
    /// </summary>
    private List<(string, object)> m_gridFilter;

    public object ObjectIds { get; set; }

    public event Action<int> GridLoadComplete;
    public event Action GridLoadStart;
    public event Action FilterChanged;
    public event Action<string> LayoutChanged;

    protected void FireGridLoadStarted()
    {
        GridLoadStart?.Invoke();
    }

    protected void FireGridLoadComplete(int a_rowCount)
    {
        GridLoadComplete?.Invoke(a_rowCount);
    }

    protected void FireFilterChanged()
    {
        FilterChanged?.Invoke();
    }

    public void RegisterExtensionModule(IGridModuleExtensionElement a_extensionModule)
    {
        m_extensionModules.Add(a_extensionModule);
        m_extensionModules = m_extensionModules.OrderBy(a_module1 => a_module1.Priority).ToList();
    }

    public void RegisterExtensionModule(IDataSourceExtensionElement a_extensionModule)
    {
        m_dataExtensionModules.Add(a_extensionModule);
        m_dataExtensionModules = m_dataExtensionModules.OrderBy(a_module1 => a_module1.Priority).ToList();
    }

    /// <summary>
    /// Used by GridUtility.cs when initializing a grid. The list of IObjectProperty
    /// passed in will come from a PackageProject's package manager, and implementations
    /// of this function should handle how different types of IObjectProperty are turned
    /// into columns. For example, the PackageManager may have extra IObjectProperty's
    /// that the corresponding UnboundDataSourceModule does not need so LoadColumns()
    /// would just act as a filter in this case.
    /// </summary>
    /// <param name="a_objectProperties"></param>
    /// <exception cref="NotImplementedException"></exception>
    public virtual void LoadColumns(List<IObjectProperty> a_objectProperties)
    {
        //Must override
        throw new NotImplementedException();
    }

    // Can be override by DataSourceModules that inherit from this class if
    // specialized behavior/look are needed for the module's columns
    protected virtual GridColumn GenerateColumn(string a_prefix, IObjectProperty a_objectProperty, bool a_allowEdit)
    {
        GridColumn newColumn = new ();
        Dictionary<string, object> tagDict = new ();
        newColumn.Tag = tagDict;
        newColumn.Caption = a_objectProperty.PropertyName;
        newColumn.FieldName = a_prefix + a_objectProperty.PropertyName;
        newColumn.Visible = true;
        newColumn.UnboundDataType = a_objectProperty.PropertyType;

        if (a_objectProperty.PropertyType == typeof(PTColor))
        {
            newColumn.SortMode = ColumnSortMode.Value;
            newColumn.FilterMode = ColumnFilterMode.DisplayText;
            newColumn.OptionsColumn.AllowSort = DefaultBoolean.True;
            tagDict.Add("ValueType", typeof(PTColor));
        }
        else if (a_objectProperty.PropertyType == typeof(TimeSpan))
        {
            newColumn.ColumnEdit = m_columnEditTimeSpan;
            //if (!(a_objectProperty is ITimeSpanProperty))
            //{
            //    DebugException.ThrowInDebug(a_objectProperty.PropertyName + " Returns TimeSpan type but does not inherit TimeSpanProperty");
            //}
        }
        else if (a_objectProperty.PropertyType == typeof(long) ||
                 a_objectProperty.PropertyType == typeof(int) ||
                 a_objectProperty.PropertyType == typeof(decimal) ||
                 a_objectProperty.PropertyType == typeof(double))
        {
            #if TEST
                // The if condition above use to just include long when this 
                // compiler conditional was added. I know DateTime technically comes in the form of ticks
                // which is really just a long, but I'm not sure how we'd handle this. 
                // The extra conditions were added so that we could add thousand separators in. 

                //Convert this property to decimal time representation.
                throw new NotImplementedException();
            #endif

            newColumn.DisplayFormat.FormatType = FormatType.Numeric;
            newColumn.DisplayFormat.FormatString = "n2";
        }

        if (a_objectProperty.PropertyType == typeof(DateTime))
        {
            //Set the editor to show times
            if (a_objectProperty is IFormatColumnAsDateOnly)
            {
                newColumn.ColumnEdit = m_columnEditDate;
            }
            else
            {
                newColumn.ColumnEdit = m_columnEditDateTime;
            }

            newColumn.SortMode = ColumnSortMode.Default;
        }

        if (a_objectProperty is ICurrencyProperty)
        {
            newColumn.ColumnEdit = m_columnEditCurrency;
        }

        if (a_objectProperty is IImageProperty imageProperty)
        {
            RepositoryItemImageComboBox repositoryItemImageComboBox = imageProperty.GetColumnEditor();
            if (!m_gridView.GridControl.RepositoryItems.Contains(repositoryItemImageComboBox))
            {
                m_gridView.GridControl.RepositoryItems.Add(repositoryItemImageComboBox);
            }

            newColumn.ColumnEdit = repositoryItemImageComboBox;
        }

        if (a_objectProperty is IHasCustomFormattingProperty customFormatProperty)
        {
            newColumn.DisplayFormat.FormatType = customFormatProperty.PropertyFormatType;
            newColumn.DisplayFormat.FormatString = customFormatProperty.PropertyFormatString;
        }

        m_gridView.Columns.Add(newColumn);
        m_lockDict.AddIfNew(newColumn.FieldName, new object());
        m_propertyDict.AddIfNew(newColumn.FieldName, a_objectProperty);
        return newColumn;
    }

    #if DEBUG
    public new int Add()
    {
        //Don't use this, use AddObjectRow instead
        throw new NotImplementedException();
    }
    #endif

    protected void AddObjectRow(T a_objectKey)
    {
        if (GridFilter != null && GridFilter.Count > 0)
        {
            //For now only supports OR filters. Does not support nulls
            foreach ((string columnName, object value) filter in GridFilter)
            {
                if (m_propertyDict.TryGetValue(filter.columnName, out IObjectProperty property))
                {
                    LookUpValueStruct value = GetValue(a_objectKey, property);

                    if (value.ValueFound && value.Value.Equals(filter.value))
                    {
                        AddObject(a_objectKey);
                        break;
                    }
                }
            }
        }
        else
        {
            AddObject(a_objectKey);
        }
    }

    private void AddObject(T a_objectKey)
    {
        int add = base.Add();
        m_rowIdxToObjectKeyDict.Add(add, a_objectKey);
    }

    /// <summary>
    /// This function should help the DataSource determine where to get the property's value
    /// </summary>
    protected virtual LookUpValueStruct GetValue(T a_object, IObjectProperty a_property)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Some values come in batches or blocks, and this function should help the DataSource
    /// determine where to get the batch values from.
    /// </summary>
    protected virtual List<LookUpValueStruct> GetBatchValues(List<T> a_rowsToCalc, IObjectProperty a_property)
    {
        throw new NotImplementedException();
    }

    public virtual LookUpValueStruct GetCellValue(int a_rowIdx, string a_fieldName)
    {
        if (m_rowIdxToObjectKeyDict.TryGetValue(a_rowIdx, out T a_objectKey))
        {
            if (m_propertyDict.TryGetValue(a_fieldName, out IObjectProperty property))
            {
                return GetValue(a_objectKey, property);
            }
        }

        //TODO: Error
        return LookUpValueStruct.EmptyLookUpValue;
    }

    private void CacheBatch(List<int> a_rowsToCalc, string a_columnName)
    {
        //Translate row indices to objects
        List<T> keys = new (a_rowsToCalc.Count);
        foreach (int i in a_rowsToCalc)
        {
            if (m_rowIdxToObjectKeyDict.TryGetValue(i, out T key))
            {
                keys.Add(key);
            }
            else
            {
                throw new Exception("Datasource object cache error");
            }
        }

        if (m_propertyDict.TryGetValue(a_columnName, out IObjectProperty property))
        {
            GetBatchValues(keys, property);
        }
    }

    public void CacheColumn()
    {
        GridColumnReadOnlyCollection gridColumnReadOnlyCollection = m_gridView.SortedColumns;
        List<int> rowsToCalc = new ();
        for (int i = 0; i < m_gridView.DataRowCount; i++)
        {
            rowsToCalc.Add(i);
        }

        if (rowsToCalc.Count == 0)
        {
            return;
        }

        Parallel.ForEach(gridColumnReadOnlyCollection,
            c =>
            {
                object lockObject = m_lockDict[c.FieldName];
                lock (lockObject)
                {
                    CacheBatch(rowsToCalc, c.FieldName);
                }
            });
    }

    public void ChangedLayout(string a_layoutName)
    {
        LayoutChanged?.Invoke(a_layoutName);
    }

    public virtual bool IsCellReadOnly(int a_rowHandle, string a_fieldName)
    {
        //This should be overriden by derivative classes
        return false;
    }

    public void Reset(ScenarioDetail a_sd)
    {
        FireGridLoadStarted();
        m_rowIdxToObjectKeyDict.Clear();

        m_scenarioInfo.InvokeControl.Invoke(new Action(Clear));

        m_scenarioInfo.InvokeControl.Invoke(new Action(() => PopulateObjects(a_sd)));

        m_scenarioInfo.InvokeControl.Invoke(m_gridView.RefreshData);

        FireGridLoadComplete(m_rowIdxToObjectKeyDict.Count);
    }

    public void Reset(UserManager a_um)
    {
        FireGridLoadStarted();
        m_rowIdxToObjectKeyDict.Clear();

        m_scenarioInfo.InvokeControl.Invoke(new Action(Clear));

        m_scenarioInfo.InvokeControl.Invoke(new Action(() => PopulateObjects(a_um)));

        m_scenarioInfo.InvokeControl.Invoke(m_gridView.RefreshData);

        FireGridLoadComplete(m_rowIdxToObjectKeyDict.Count);
    }

    /// <summary>
    /// Should be overriden in each DataSourceModule that interacts with scenario data so
    /// that the module can be properly populated with the corresponding object type.
    /// </summary>
    /// <param name="a_sd"></param>
    protected virtual void PopulateObjects(ScenarioDetail a_sd)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Should be overriden in each DataSourceModule that interacts with Users
    /// </summary>
    /// <param name="a_um"></param>
    protected virtual void PopulateObjects(UserManager a_um)
    {
        throw new NotImplementedException();
    }

    public virtual void ProcessNewObjects(object a_objects)
    {
        ObjectIds = a_objects;
    }

    public List<T> GetPrimaryKeys(List<int> a_rowIndexList)
    {
        List<T> keys = new ();
        foreach (int i in a_rowIndexList)
        {
            if (m_rowIdxToObjectKeyDict.TryGetValue(i, out T resId))
            {
                keys.Add(resId);
            }
        }

        return keys;
    }

    public T GetPrimaryKey(int a_rowIndexList)
    {
        if (m_rowIdxToObjectKeyDict.TryGetValue(a_rowIndexList, out T key))
        {
            return key;
        }

        return default(T);
    }

    public object GetPrimaryKeyObject(int a_rowIndexList)
    {
        if (m_rowIdxToObjectKeyDict.TryGetValue(a_rowIndexList, out T key))
        {
            return key;
        }

        if (a_rowIndexList < 0)
        {
            return null;
        }

        throw new PTValidationException("Key not found");
    }

    public object GetPrimaryKeyObjects(List<int> a_rowIndexList)
    {
        HashSet<T> keys = new ();
        foreach (int i in a_rowIndexList)
        {
            if (m_rowIdxToObjectKeyDict.TryGetValue(i, out T resId))
            {
                keys.Add(resId);
            }
        }

        return keys;
    }

    //Since this function is not called often, just search through the dictionary. 
    public int GetRowByKeyObject(object a_key)
    {
        foreach (KeyValuePair<int, T> pair in m_rowIdxToObjectKeyDict)
        {
            if (pair.Value.Equals(a_key))
            {
                return pair.Key;
            }
        }

        return -1;
    }

    public List<int> GetRowsByKeyObjects(object a_keys)
    {
        List<int> rowIdxs = new ();
        HashSet<T> keys = (HashSet<T>)a_keys;

        foreach (KeyValuePair<int, T> pair in m_rowIdxToObjectKeyDict)
        {
            if (keys.Contains(pair.Value))
            {
                rowIdxs.Add(pair.Key);
                if (rowIdxs.Count == keys.Count)
                {
                    return rowIdxs;
                }
            }
        }

        return rowIdxs;
    }

    public IObjectProperty GetPropertyFromColumnKey(string a_key)
    {
        if (m_propertyDict.TryGetValue(a_key, out IObjectProperty property))
        {
            return property;
        }

        return null;
    }

    public int RowCount => m_rowIdxToObjectKeyDict.Count;

    public virtual string DataTypeKey => throw new NotImplementedException();

    public virtual string DataTypeName => throw new NotImplementedException();

    /// <summary>
    /// Some data sources will pull from multiple types of PTObject, and this
    /// value should be set to true in those cases.
    /// For example, the activities grid will pull from jobs, resources, and others
    /// </summary>
    public virtual bool MultiProperty => false;

    public string GridName { get; set; }

    public List<(string, object)> GridFilter
    {
        get => m_gridFilter;
        set
        {
            m_gridFilter = value;
            FireFilterChanged();
        }
    }
}