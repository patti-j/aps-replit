namespace PT.Scheduler;

/// <summary>
/// When MOs are updated, this is used to indicate any changes to the routings.
/// </summary>
internal class RoutingChanges
{
    //Causes for the routing to change.
    internal enum RoutingChangeCauses
    {
        ScheduledPathRemoved,
        ScheduledOperationRemoved,
        ScheduledOperationAdded,
        ScheduledOperationChangedType,
        ProductChanged,
        ScheduledPathChanged
    }

    #region Members
    /// <summary>
    /// The scehduled alternate path, operation type, or product type has changed
    /// </summary>
    internal bool ScheduledRoutingChanged;

    /// <summary>
    /// An alternate path that is not scheduled has changed
    /// </summary>
    internal bool AlternatePathChanged;

    /// <summary>
    /// The reason why the MO routing changed
    /// Currently not displayed to the user. Unscheduled jobs will show up in impact analysis.
    /// </summary>
    internal RoutingChangeCauses RoutingChangeCause;

    /// <summary>
    /// Details on why the MO routing changed
    /// </summary>
    internal string RoutingChangeDescription = "";

    internal void AddDescription(string a_msg)
    {
        RoutingChangeDescription += a_msg + " ";
    }
    #endregion
}