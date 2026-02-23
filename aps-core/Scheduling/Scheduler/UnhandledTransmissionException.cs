namespace PT.Scheduler;

/// <summary>
/// Summary description for UnhandledTransmissionException.
/// </summary>
public class UnhandledTransmissionException : ApplicationException
{
    public UnhandledTransmissionException(string msg)
        : base(msg) { }
}