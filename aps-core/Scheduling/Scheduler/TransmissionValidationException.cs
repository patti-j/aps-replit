using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Summary description for CreateTransmissionException.
/// </summary>
public class TransmissionValidationException : APSCommon.PTValidationException
{
    public PTTransmission transmission;

    public TransmissionValidationException(PTTransmission transmission, string msg, object[] a_stringParameters = null, bool a_appendHelpUrl = true)
        : base(msg, a_stringParameters, a_appendHelpUrl)
    {
        this.transmission = transmission;
    }
}