using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// Summary description for TransmissionException.
/// </summary>
public class TransmissionException : PTException
{
    public TransmissionException(string eMsg, object[] a_stringParameters, bool a_appendHelpUrl = true)
        : base(eMsg, a_stringParameters, a_appendHelpUrl) { }
}