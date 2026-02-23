using System.Diagnostics;

namespace MassRecordings;

public class MRUtilities
{
    public void KillMRProcesses()
    {
        foreach (Process process in Process.GetProcessesByName("MassRecordingPlayer"))
        {
            process.Kill();
        }
    }
}