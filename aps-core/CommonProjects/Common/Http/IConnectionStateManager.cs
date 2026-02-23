namespace PT.Common.Http;

public class RoundTripConnectionInfo
{
    public RoundTripConnectionInfo()
    {
        Id = Guid.NewGuid();
    }

    public static RoundTripConnectionInfo BeginSend(string a_endpoint)
    {
        return new RoundTripConnectionInfo
        {
            Start = DateTime.UtcNow,
            Endpoint = a_endpoint
        };
    }

    public DateTime Start;
    public DateTime End;
    public Exception ExceptionResult;
    public string Endpoint;
    public readonly Guid Id;
    public TimeSpan Duration;
    public bool IsSuccessful => ExceptionResult == null;

    public void Success()
    {
        End = DateTime.UtcNow;
        Duration = End - Start;
    }

    public void Fail(Exception e)
    {
        End = DateTime.UtcNow;
        Duration = End - Start;
        ExceptionResult = e;
    }

    public override bool Equals(object obj)
    {
        return obj is RoundTripConnectionInfo info && info.Id == Id;
    }

    protected bool Equals(RoundTripConnectionInfo other)
    {
        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}

public interface IConnectionStateManager
{
    void Log(RoundTripConnectionInfo a_info);

    /// <summary>
    /// Whether an active connection is currently down (as of last request0.
    /// </summary>
    bool IsConnectionDown { get; }

    /// <summary>
    /// Checks if the State Manager deems the request can be prevented, typically in the case that <see cref="IsConnectionDown"/> is true.
    /// </summary>
    /// <param name="a_endpointName"></param>
    /// <param name="o_disconnectMessage"></param>
    /// <returns></returns>
    bool ShouldRequestBePrevented(string a_endpointName, out string o_disconnectMessage);

    /// <summary>
    /// This event is raised when Connection that was created during login
    /// has been dropped by the server. Therefore requiring logging into the
    /// system again.
    /// </summary>
    event Action<Exception> ConnectionDropped;

    event Action<int> ConnectionQuality;

    /// <summary>
    /// This event is raised when system service is not found or a call to system service has timed out.
    /// It usually signifies service being stopped or network connectivity issues. In
    /// this case, the client can continue trying, for example, to Receive,
    /// until connection is re-established.
    /// </summary>
    public event Action<Exception> SystemServiceUnavailableEvent;

    public event Action SystemServiceAvailableEvent;
}