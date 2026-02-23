namespace PT.Scheduler;

internal struct IntervalProfile
{
    internal bool CleanOutSetups;
    internal bool PreventOperationsFromSpanning;
    internal bool CanStartActivity;
    internal bool RunSetup;
    internal bool RunProcessing;
    internal bool RunPostProcessing;
    internal bool RunCleanout;
    internal bool RunStoragePostProcessing;
    internal bool Overtime;
    internal bool UseOnlyWhenLate;
    internal string CapacityCode;

    internal IntervalProfile(bool a_cleanOutSetups, bool a_preventOperationsFromSpanning, bool a_canStartActivity,
                             bool a_runSetup, bool a_runProcessing, bool a_runPostProcessing, bool a_runCleanout,
                             bool a_runStoragePostProcessing, bool a_overtime, bool a_useOnlyWhenLate, string a_capacityCode)
    {
        CleanOutSetups = a_cleanOutSetups;
        PreventOperationsFromSpanning = a_preventOperationsFromSpanning;
        CanStartActivity = a_canStartActivity;
        RunSetup = a_runSetup;
        RunProcessing = a_runProcessing;
        RunPostProcessing = a_runPostProcessing;
        RunCleanout = a_runCleanout;
        RunStoragePostProcessing = a_runStoragePostProcessing;
        Overtime = a_overtime;
        UseOnlyWhenLate = a_useOnlyWhenLate;
        CapacityCode = a_capacityCode;
    }

    internal static IntervalProfile DefaultProfile = new IntervalProfile(false, false, true, true, true, true, true, true, false, false, string.Empty);
}

