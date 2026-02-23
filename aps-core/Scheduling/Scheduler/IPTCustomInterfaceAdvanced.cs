using PT.Scheduler;
using PT.Transmissions;

namespace PT.CustomInterface;

/// <summary>
/// Interface to use for creating custom import programs.
/// This can be used, for example, to import from a particularly formatted Excel spreadsheet.
/// This Advanced version exposes Scheduler objects and so should only be used by PlanetTogether personnel.
/// </summary>
public interface IPTCustomInterfaceAdvanced : IPTCustomInterface
{
    /// <summary>
    /// Export the data.
    /// </summary>
    void PerformExport(string workingDirectory, ScenarioDetail sd);

    /// <summary>
    /// Perform any custom actions upon receipt of a Transmission to the Live Scenario.
    /// This can be used, for example, to export status or history information to an external system.
    /// </summary>
    /// <param name="workingDirectory"></param>
    /// <param name="sd"></param>
    /// <param name="t"></param>
    void LiveScenarioReceivedTransmission(string workingDirectory, ScenarioDetail sd, ScenarioBaseT t);
}

//Dummy needed to create instance with reflection.
public class CustomInterfaceAdvanced : IPTCustomInterfaceAdvanced
{
    #region IPTCustomInterface Members
    public void PerformExport(string workingDirectory, ScenarioDetail sd)
    {
        // TODO
    }

    public void LiveScenarioReceivedTransmission(string workingDirectory, ScenarioDetail sd, ScenarioBaseT t)
    {
        // TODO
    }

    public void PerformImport(string workingDirectory)
    {
        // TODO:  Add CustomInterface.PerformImport implementation
    }

    public bool RunStandardImport => false;

    public event InterfaceDelegate.SendTransmissionHandler SendTransmissionEvent;

    public void PerformExport(string workingDirectory, Database.PtDbDataSet dataset)
    {
        // TODO
    }
    #endregion
}