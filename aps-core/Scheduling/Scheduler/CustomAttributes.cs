namespace PT.Scheduler;

/// <summary>
/// Used by properties that require drop-down grids to display/choose values.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ListSourceAttribute : Attribute
{
    /// <remarks>
    /// Note that when adding new enum values, they must also be added in:
    /// GridControl.GetDropDownDataSource and
    /// GridControl.GetDropDownDataType and
    /// GridControl.GetChildObjectManager
    /// </remarks>
    public enum ListSources
    {
        Plant = 0,
        Department,
        Resource,
        User,
        Capability,
        BalancedCompositeDispatcherDefinition,
        Cell
    }

    /// <summary>
    /// Is used to determine which DataTable to use to populate the drop-down grid.
    /// </summary>
    /// <param name="listSource"></param>
    /// <param name="allowMultipleSelections"></param>
    public ListSourceAttribute(ListSources listSource, bool allowMultipleSelections)
    {
        this.listSource = listSource;
        this.allowMultipleSelections = allowMultipleSelections;
    }

    protected ListSources listSource;

    public ListSources ListSource => listSource;

    /// <summary>
    /// Whether multiple selections can be made in the dropdown list, as opposed to a single selection.
    /// </summary>
    protected bool allowMultipleSelections;

    public bool AllowMultipleSelections => allowMultipleSelections;
}

/// <summary>
/// Used by Properties that must be included in DataTables.  Prevents the property from being excluded when it's marked as non-browsable.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class PartOfKeyAttribute : Attribute
{
    public PartOfKeyAttribute(bool partOfKey)
    {
        this.partOfKey = partOfKey;
    }

    /// <summary>
    /// If part of key then this field will be added to DataTables but may be hidden.
    /// </summary>
    protected bool partOfKey;

    public bool PartOfKey => partOfKey;
}