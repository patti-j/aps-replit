using PT.Common.Exceptions;
using PT.Transmissions;

namespace PT.CustomInterface;

/// <summary>
/// Interface to use for creating custom import and export functions.
/// </summary>
public interface IPTCustomInterface
{
    /// <summary>
    /// Trigger sending of transmissions to APS.
    /// </summary>
    void PerformImport(string workingDirectory);

    /// <summary>
    /// Whether to run the standard import after this import.
    /// </summary>
    bool RunStandardImport { get; }

    /// <summary>
    /// Fire this event to submit a Transmission to APS.
    /// </summary>
    event InterfaceDelegate.SendTransmissionHandler SendTransmissionEvent;

    /// <summary>
    /// Export the data.
    /// </summary>
    void PerformExport(string workingDirectory, Database.PtDbDataSet dataset);
}

public class InterfaceDelegate
{
    public delegate void SendTransmissionHandler(object sender, PTTransmission t);
}

public class IPTCustomInterfaceException : PTException
{
    public IPTCustomInterfaceException(string a_message, Exception innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = true)
        : base(a_message, innerException, a_stringParameters, a_appendHelpUrl) { }

    public IPTCustomInterfaceException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = true)
        : base(a_message, a_stringParameters, a_appendHelpUrl) { }
}

//TODO: Do we need this?
/// <summary>
/// Dummy needed to create instance with reflection.
/// </summary>
public class CustomInterface : IPTCustomInterface
{
    #region IPTCustomInterface Members
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