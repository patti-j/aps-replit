using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;

using PT.Common.Collections;
using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler;
using PT.Transmissions;

namespace PT.PackageDefinitionsUI.DataSources;

/// <summary>
/// Extends the unbound datasource for editable grid properties
/// </summary>
/// <typeparam name="T">Grid object type key</typeparam>
public class EditableUnboundDataSourceModule<T> : UnboundDataSourceModule<T>, IEditableGridDataSourceModule
{
    public EditableUnboundDataSourceModule(GridView a_gridView, IScenarioInfo a_scenarioInfo, IUserPreferenceInfo a_preferenceInfo) : base(a_gridView, a_scenarioInfo)
    {
        m_gridReloadOnEditMode = a_preferenceInfo.LoadSetting(new PTCorePreferences()).GridReloadOnEditMode;
        a_preferenceInfo.SettingSavedEvent += preferenceInfo_SettingSavedEvent;
    }

    private void preferenceInfo_SettingSavedEvent(ISettingsManager a_settingsManager, string a_key)
    {
        m_gridReloadOnEditMode = a_settingsManager.LoadSetting(new PTCorePreferences()).GridReloadOnEditMode;
    }

    public void CancelEdits()
    {
        m_modifiedCells.Clear();
    }

    public bool PendingChanges => m_modifiedCells.Count > 0;
    public event Action DataChanged;
    public event Action<bool> EditModeChanged;

    private bool m_gridReloadOnEditMode;
    private bool m_editModeEnabled;

    public bool EditModeEnabled
    {
        get => m_editModeEnabled;
        set
        {
            m_editModeEnabled = value;
            EditModeChanged?.Invoke(m_editModeEnabled);
        }
    }

    public event Action RequestReload;
    /// <summary>
    /// This flag allows the grid to be reloaded when set to true regardless of whether the user's preferences prevents reloading while in edit mode.
    /// <remarks>Typical use case is when the user wants the grid reloaded on demand (i.e Clicked on the reload button on the grid)</remarks>
    /// </summary>
    private bool m_allowReloadOverride;
    public void FireReloadRequest()
    {
        m_allowReloadOverride = true;
        RequestReload?.Invoke();
    }
    public virtual PTTransmission? SaveEdits(ScenarioDetail a_sd)
    {
        //Typed inheritors should use modified cells collection to build a transmission and use all edit properties to set the values
        CancelEdits();
        return null;
    }

    protected DictionaryCollection<T, PropertyValueChange> GetModifiedCellKeys()
    {
        DictionaryCollection<T, PropertyValueChange> modifiedValues = new ();
        foreach (KeyValuePair<T, Dictionary<string, PropertyValueChange>> keyValuePair in m_modifiedCells)
        {
            T primaryKey = keyValuePair.Key;
            List<PropertyValueChange> changes = keyValuePair.Value.Values.ToList();
            modifiedValues.Add(primaryKey, changes);
        }

        return modifiedValues;
    }

    protected class PropertyValueChange
    {
        public IObjectProperty Property;
        public object Value;

        internal PropertyValueChange(IObjectProperty a_property, object a_value)
        {
            Property = a_property;
            Value = a_value;
        }
    }

    /// <summary>
    /// Keep track of modified cells
    /// </summary>
    private readonly Dictionary<T, Dictionary<string, PropertyValueChange>> m_modifiedCells = new ();

    private readonly Dictionary<string, IGridEditProperty> m_readOnlyProperties = new ();
    public Dictionary<string, IGridEditProperty> ReadOnlyProperties => m_readOnlyProperties;

    /// <summary>
    /// If the cell has been modified by the user, return that value instead of the data model actual value
    /// </summary>
    /// <param name="a_rowIdx"></param>
    /// <param name="a_fieldName"></param>
    /// <returns></returns>
    public override LookUpValueStruct GetCellValue(int a_rowIdx, string a_fieldName)
    {
        T primaryKey = GetPrimaryKey(a_rowIdx);
        if (m_modifiedCells.TryGetValue(primaryKey, out Dictionary<string, PropertyValueChange> rowChanges))
        {
            if (rowChanges.TryGetValue(a_fieldName, out PropertyValueChange valueChange))
            {
                return new LookUpValueStruct(valueChange.Value);
            }
        }

        //Value not modified, go get the value from the datasource
        return base.GetCellValue(a_rowIdx, a_fieldName);
    }

    /// <summary>
    /// An editable grid cell changed. Store the modified cell and value
    /// </summary>
    /// <param name="a_dataRowIndex"></param>
    /// <param name="a_column"></param>
    /// <param name="a_value"></param>
    public void SetCellValue(int a_dataRowIndex, GridColumn a_column, object a_value)
    {
        T primaryKey = GetPrimaryKey(a_dataRowIndex);
        PropertyValueChange change = new (GetPropertyFromColumnKey(a_column.FieldName), a_value);
        if (m_modifiedCells.TryGetValue(primaryKey, out Dictionary<string, PropertyValueChange> existingValues))
        {
            //See if we are reverting a change
            if (existingValues.TryGetValue(a_column.FieldName, out PropertyValueChange existingChange))
            {
                //This cell has been changed before.
                LookUpValueStruct lookUpValueStruct = base.GetCellValue(a_dataRowIndex, a_column.FieldName);
                if (lookUpValueStruct.ValueFound)
                {
                    IObjectProperty property = GetPropertyFromColumnKey(a_column.FieldName);
                    if (CompareValues(property.PropertyType, lookUpValueStruct.Value, a_value))
                    {
                        //We changed back to the original value
                        existingValues.Remove(a_column.FieldName);
                        return;
                    }
                }
            }

            existingValues[a_column.FieldName] = change;
        }
        else
        {
            Dictionary<string, PropertyValueChange> propertyValueChanges = new ();
            propertyValueChanges.Add(a_column.FieldName, change);
            m_modifiedCells.Add(primaryKey, propertyValueChanges);
        }

        DataChanged?.Invoke();
    }

    /// <summary>
    /// Checks if a cell was modified.
    /// </summary>
    /// <param name="a_dataRowHandle">The DataSource index of the modified cell row.</param>
    /// <param name="a_columnFieldName">The column field name of the modified cell.</param>
    /// <returns></returns>
    public bool CellModified(int a_dataRowHandle, string a_columnFieldName)
    {
        try
        {
            object primaryKeyObject = GetPrimaryKeyObject(a_dataRowHandle);
            if (primaryKeyObject == null)
            {
                return false;
            }

            if (!m_modifiedCells.TryGetValue((T)primaryKeyObject, out Dictionary<string, PropertyValueChange> propertyChanges))
            {
                return false;
            }

            if (!propertyChanges.TryGetValue(a_columnFieldName, out PropertyValueChange _))
            {
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            //PTValidation will be thrown if GetPrimaryKeyObject cannot find the key. The keys list is cleared on Reload. 
            return false;
        }
    }

    public new void Reset(ScenarioDetail a_sd)
    {
        if (EditModeEnabled && !m_gridReloadOnEditMode && !m_allowReloadOverride)
        {
            return;
        }

        base.Reset(a_sd);
        m_allowReloadOverride = false;
    }

    public new void Reset(UserManager a_um)
    {
        if (EditModeEnabled && !m_gridReloadOnEditMode && !m_allowReloadOverride)
        {
            return;
        }

        base.Reset(a_um);
        m_allowReloadOverride = false;
    }

    private static bool CompareValues(Type a_type, object a_object1, object a_object2)
    {
        if ((a_object1 == null && a_object2 != null) || (a_object1 != null && a_object2 == null))
        {
            return false;
        }

        if (a_object1 == null && a_object2 == null)
        {
            return true;
        }

        if (a_type == typeof(bool))
        {
            return (bool)a_object1 == (bool)a_object2;
        }

        if (a_type == typeof(decimal))
        {
            return (decimal)a_object1 == (decimal)a_object2;
        }

        if (a_type == typeof(int))
        {
            return (int)a_object1 == (int)a_object2;
        }

        if (a_type == typeof(double))
        {
            return (double)a_object1 == (double)a_object2;
        }

        if (a_type == typeof(string))
        {
            return (string)a_object1 == (string)a_object2;
        }

        if (a_type == typeof(Inventory))
        {
            return ((Inventory)a_object1).Id == ((Inventory)a_object2).Id;
        }

        return false;
    }

    /// <summary>
    /// Override generation to create editable columns for editable properties
    /// </summary>
    /// <param name="a_prefix"></param>
    /// <param name="a_objectProperty"></param>
    /// <param name="a_allowEdit"></param>
    /// <returns></returns>
    protected override GridColumn GenerateColumn(string a_prefix, IObjectProperty a_objectProperty, bool a_allowEdit)
    {
        GridColumn newColumn = base.GenerateColumn(a_prefix, a_objectProperty, a_allowEdit);

        if (a_allowEdit)
        {
            //For editable properties, generate a typed column editor for the grid
            if (a_objectProperty is IGridEditProperty editProperty)
            {
                editProperty.GenerateRepositoryItem();
                RepositoryItem item = editProperty.RepositoryItemInstance;
                m_gridView.GridControl.RepositoryItems.Add(item);
                newColumn.ColumnEdit = item;
                newColumn.Tag = editProperty;
                newColumn.Visible = true;
                newColumn.SortMode = ColumnSortMode.DisplayText;
            }
            else
            {
                newColumn.OptionsColumn.ReadOnly = m_editModeEnabled;
            }
        }

        return newColumn;
    }

}