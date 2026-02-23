namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of Department objects.
/// </summary>
public partial class DepartmentManager
{
    #region Diagnostics
    internal void PrintResultantCapacity()
    {
        for (int i = 0; i < Count; ++i)
        {
            Department d = this[i];
            d.PrintResultantCapacity();
        }
    }
    #endregion
}