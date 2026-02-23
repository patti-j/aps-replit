using System.Net;

using PT.APSCommon.Extensions;
using PT.Common.Collections;
using PT.Common.Http;

namespace PT.UI;

/// <summary>
/// We assume that we are logged in
/// </summary>
internal class ConnectionStateManager : IConnectionStateManager
{
    const string c_sendTransmissionEndpointName = "SendTransmission";
    const string c_pollingServerEndpointName = "RetrieveNextAction";

    private readonly TimeSpan m_connectionTimeout;
    private readonly CircularQueue<RoundTripConnectionInfo> m_infoQueue;
    private const int c_maxInfosToStore = 1000;
    private readonly object m_lock = new ();
    private long m_totalActionsLogged;
    private DateTimeOffset m_lastAnalyzeTime = PTDateTime.MinValue;
    private DateTimeOffset m_lastSuccessfulTime = PTDateTime.MinValue;

    private bool m_reachedServerLastAttempt;
    private bool m_reachedServerAtLeastOnce; 
    private RoundTripConnectionInfo m_lastSuccessfulInfo;
    public bool IsConnected => m_reachedServerLastAttempt;

    internal ConnectionStateManager(TimeSpan a_connectionTimeout)
    {
        m_connectionTimeout = a_connectionTimeout;
        m_infoQueue = new CircularQueue<RoundTripConnectionInfo>(c_maxInfosToStore);
    }

    public void Log(RoundTripConnectionInfo a_info)
    {
        m_totalActionsLogged++;
        AddRoundTripConnection(a_info);

        if (a_info.Endpoint == c_sendTransmissionEndpointName)
        {
            LastUserBroadcast = a_info;
        }

        if (PTDateTime.UtcNow > m_lastAnalyzeTime.Add(TimeSpan.FromSeconds(5)))
        {
            //Time to check the data again
            Task.Run(AnalyzeConnectionData);
        }
    }

    /// <summary>
    /// This event is raised when Connection that was created during login
    /// has been dropped by the server. Therefore requiring logging into the
    /// system again.
    /// </summary>
    public event Action<Exception> ConnectionDropped;

    public event Action<int> ConnectionQuality;

    /// <summary>
    /// This event is raised when system service is not found or a call to system service has timed out.
    /// It usually signifies service being stopped or network connectivity issues. In
    /// this case, the client can continue trying, for example, to Receive,
    /// until connection is re-established.
    /// </summary>
    public event Action<Exception> SystemServiceUnavailableEvent;

    public event Action SystemServiceAvailableEvent;

    internal void AddRoundTripConnection(RoundTripConnectionInfo a_info)
    {
        lock (m_lock)
        {
            m_infoQueue.Enqueue(a_info);
        }

        if (a_info.ExceptionResult != null)
        {
            //TODO: Handle exceptions
            if (a_info.ExceptionResult is ApiException apiException)
            {
                if ((HttpStatusCode)apiException.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ConnectionDropped?.Invoke(a_info.ExceptionResult);
                }
            }
            else if (a_info.ExceptionResult is WebException or HttpRequestException)
            {
                if (m_lastSuccessfulInfo != null && DateTime.UtcNow - m_lastSuccessfulInfo.Start > m_connectionTimeout)
                {
                    //We have been dropped by the server
                    ConnectionDropped?.Invoke(a_info.ExceptionResult);
                }
                else if (m_reachedServerLastAttempt) //Only fire once until the connection is restored
                {
                    //We can't reach the server, but the connection might be restored.
                    SystemServiceUnavailableEvent?.Invoke(a_info.ExceptionResult);
                }

                m_reachedServerLastAttempt = false;
            }
        }
        else
        {
            if (!m_reachedServerLastAttempt)
            {
                SystemServiceAvailableEvent?.Invoke();
            }

            m_reachedServerAtLeastOnce = true;
            m_reachedServerLastAttempt = true;
            m_lastSuccessfulInfo = a_info;
        }

        //Process known events
        if (a_info.Endpoint is "LoginAsUser" or "LoginWithToken") { }
    }

    internal RoundTripConnectionInfo LastUserBroadcast;

    public bool IsConnectionDown => m_reachedServerAtLeastOnce && !m_reachedServerLastAttempt;

    public bool ShouldRequestBePrevented(string a_endpointName, out string o_disconnectMessage)
    {
        if (IsConnectionDown &&
            !a_endpointName.Equals(c_pollingServerEndpointName)) // always let standard traffic through to reestablish connection
        {
            o_disconnectMessage = string.Format("4488".Localize(), a_endpointName);
            return true;
        }
        else
        {
            o_disconnectMessage = null;
            return false;
        }
    }

    private void AnalyzeConnectionData()
    {
        RoundTripConnectionInfo[] infos;
        lock (m_lock)
        {
            infos = m_infoQueue.GetQueueCopy();
        }

        m_lastAnalyzeTime = PTDateTime.UtcNow;

        RoundTripConnectionInfo lastInfo = infos[^1];
        RoundTripConnectionInfo[] sortedInfos = infos.OrderBy(i => i.Duration).ToArray();
        RoundTripConnectionInfo longest = sortedInfos[^1];
        RoundTripConnectionInfo shortest = sortedInfos[0];
        double average = infos.Average(i => i.Duration.Ticks);
        if (ConnectionQuality != null)
        {
            //Percent of the timeout remaining.
            DateTime lastSuccessTime = PTDateTime.MinDateTime;
            if (m_lastSuccessfulInfo != null)
            {
                lastSuccessTime = m_lastSuccessfulInfo.End;
            }

            double percentOfTimeout = (DateTime.UtcNow - lastSuccessTime).Ticks / (double)m_connectionTimeout.Ticks;
            percentOfTimeout = Math.Max(0, 1 - percentOfTimeout);
            ConnectionQuality?.Invoke(Convert.ToInt32(percentOfTimeout * 100));
        }
    }
}