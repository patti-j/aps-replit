using DevExpress.XtraGrid.Columns;

using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler;
using PT.Transmissions;

namespace PT.PackageDefinitionsUI.DataSources;

public interface IEditableGridDataSourceModule : IGridDataSourceModule
{
    void SetCellValue(int a_rowHandle, GridColumn a_column, object a_value);
    bool CellModified(int a_dataRowHandle, string a_columnFieldName);
    bool PendingChanges { get; }
    bool EditModeEnabled { get; set; }
    event Action RequestReload;
    event Action DataChanged;
    event Action<bool> EditModeChanged;
    void CancelEdits();
    PTTransmission? SaveEdits(ScenarioDetail a_sd);
    void FireReloadRequest();
}

public interface IGridDataSourceModule
{
    event Action<int> GridLoadComplete;
    event Action GridLoadStart;
    event Action FilterChanged;
    event Action<string> LayoutChanged;

    List<(string, object)> GridFilter { get; set; }

    LookUpValueStruct GetCellValue(int a_rowIdx, string a_columnName);

    int RowCount { get; }

    void Reset(ScenarioDetail a_sd);
    void Reset(UserManager a_um);

    void CacheColumn();
    void ChangedLayout(string a_layoutName);

    bool IsCellReadOnly(int a_rowHandle, string a_fieldName);

    //Represents the type of object the data source represents.
    string DataTypeKey { get; }

    string DataTypeName { get; }

    bool MultiProperty { get; }

    string GridName { get; }

    //The non typed key
    object GetPrimaryKeyObject(int a_rowIndex);
    object GetPrimaryKeyObjects(List<int> a_rowIndex);
    int GetRowByKeyObject(object a_key);
    List<int> GetRowsByKeyObjects(object a_keys);

    IObjectProperty GetPropertyFromColumnKey(string a_key);

    void RegisterExtensionModule(IGridModuleExtensionElement a_extensionModule);
}

public interface IGridTypedDataSourceModule<T> : IGridDataSourceModule
{
    List<T> GetPrimaryKeys(List<int> a_rowIndexList);
    T GetPrimaryKey(int a_rowIndexList);
    int GetRowByKey(T a_key);
    List<int> GetRowsByKeys(List<T> a_keys);
}