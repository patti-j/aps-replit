namespace PT.Scheduler;
/// <summary>
/// Static class which handles the flags to cancel a simulation process.
/// <para></para>
/// <remarks>
/// This class was initially placed in the Diagnostics package, but encountered unusual issues related to static variables and DLL loading as scheduling packages were sort of loaded twice.
/// Due to this, both the PackageInfo and PackageInfoScheduling class treated this static class (CancelSimulationProcess) as two separate classes.
/// As a result, the static fields were being created/loaded twice, causing the static field "CancelSimulation" to be out of sync.
/// This led to inconsistency when the client set the "CancelSimulation" field versus when the scheduler used it (i.e. PackageInfo version - client versus PackageInfoScheduling version - the scheduler), 
/// making coordination between the two components unreliable.
/// </remarks>
/// </summary>
public static class CancelSimulationProcess
{
    private static bool s_cancelSimulationServer;
    private static bool s_cancelSimulationClient;
    /// <summary>
    /// Flag to indicate whether a simulation process should be cancelled
    /// </summary>
    public static bool CancelSimulation
    {
        get
        {
            if (PTSystem.Server)
            {
                return s_cancelSimulationServer;
            }

            return s_cancelSimulationClient;
        }
        private set
        {
            if (PTSystem.Server)
            {
                s_cancelSimulationServer = value;
            }
            else
            {
                s_cancelSimulationClient = value;
            }
        }
    }
    /// <summary>
    /// Sets the flag to cancel simulation
    /// </summary>
    public static void CancelProcess()
    {
        CancelSimulation = true;

    }
    /// <summary>
    /// Unsets the flag to cancel simulation
    /// </summary>
    public static void ResetCancellation()
    {
        CancelSimulation = false;
    }
}