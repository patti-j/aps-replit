using System.Net;

using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.Common.Compression;
using PT.Common.Exceptions;
using PT.Common.Http;
using PT.Common.File;
using PT.Common.Localization;
using PT.ERPTransmissions;
using PT.SystemServiceDefinitions.Headers;
using PT.SystemServiceProxy.APIClients;
using PT.Transmissions;
using PT.Transmissions.Interfaces;

namespace PT.Scheduler.Sessions;

/// <summary>
/// Pulls transmissions out of a connection at a specified interval and passes them to interested listeners.
/// </summary>
public class ClientSession : UserSession, IClientSession
{
    #region Construction
    public ClientSession(SystemActionsClient a_webClient, string a_sessionToken, PTTransmissionBase.TransmissionSenderType a_clientType, TimeSpan a_clientTimeout, HashSet<BaseId> a_loadedScenarioIds)
        : base(a_sessionToken, a_loadedScenarioIds)
    {
        m_webClient = a_webClient;
        m_clientType = a_clientType;
        m_interval = c_maxRetrieveInterval;
        m_transmissionClassFactory = PTSystem.TrnClassFactory;
        SetCompression(ECompressionType.Normal);

        ConnectionTimeout = a_clientTimeout;
    }
    #endregion

    public void AttachStateManager(IConnectionStateManager a_stateManager)
    {
        m_webClient.AttachStateManager(a_stateManager);
    }

    public bool IsConnectionDown => m_webClient.IsConnectionDown;

    public event Action SystemBusyCannotSend;

    protected object m_lock = new ();

    /// <summary>
    /// Used to reconstruct transmissions from from compressed bytes.
    /// </summary>
    protected IClassFactory m_transmissionClassFactory;

    private readonly SystemActionsClient m_webClient;
    private readonly PTTransmissionBase.TransmissionSenderType m_clientType;

    /// <summary>
    /// How long to wait between performing receives.
    /// </summary>
    private int m_interval;

    private const int c_minRetrieveInterval = 150;
    private const int c_maxRetrieveInterval = 1500;

    /// <summary>
    /// This is the worker task that performs the receives; dispatching them to interested listeners.
    /// </summary>
    private Task m_receptor;

    private CancellationTokenSource m_receptorCancelToken;

    public T MakePostRequest<T>(string a_endpoint, object a_content, string a_route) where T : class
    {
        return m_webClient.MakePostRequest<T>(a_endpoint, a_content, a_route);
    }

    public T MakeGetRequest<T>(string a_endpointName, string a_route = null, params GetParam[] a_paramList) where T : class
    {
        return m_webClient.MakeGetRequest<T>(a_endpointName, a_route, a_paramList);
    }

    public bool IsScenarioLoadedForSession(BaseId a_scenarioId)
    {
        return LoadedScenarioIds.Contains(a_scenarioId);
    }

    #region ReceiveEvent. There must be at least 1 listener.
    /// <summary>
    /// Used to create an event that is fired once for each transmission that's received.
    /// </summary>
    public delegate void ReceiveDelegate(Transmission a_t);

    /// <summary>
    /// This event is fired for every transmission that's received.
    /// </summary>
    public event ReceiveDelegate ReceiveEvent;

    /// <summary>
    /// </summary>
    /// <param name="a_t"></param>
    protected void FireReceiveEvent(Transmission a_t)
    {
        if (ReceiveEvent == null)
        {
            throw new Exception("ReceiveEvent doesn't have any listeners. Transmissions will be lost.");
        }

        ReceiveEvent(a_t);

        lock (m_lock)
        {
            //Check and notify if there are any threads waiting on this transmission to be received
            if (m_awaitingTransmissionList.TryGetValue(a_t.TransmissionId, out CancellationTokenSource tokenSource))
            {
                tokenSource.Cancel();
            }
        }
    }

    public event Action<string> ClientRestartRequiredEvent;
    protected void FireClientRestartRequiredEvent(string a_restartMessage)
    {
        ClientRestartRequiredEvent?.Invoke(a_restartMessage);
    }
    #endregion

    #region Start and stop receiving; LogOff
    /// <summary>
    /// Creates a new thread and starts receiving transmissions on it.
    /// </summary>
    public void StartReceiving()
    {
        lock (m_lock)
        {
            if (!Receiving)
            {
                m_receptorCancelToken = new CancellationTokenSource();
                m_receptor = new Task(ContinuouslyReceiveTransmissions, m_receptorCancelToken.Token, TaskCreationOptions.LongRunning);
                m_receptor.Start();
            }
        }
    }

    private bool Receiving => m_receptor?.Status == TaskStatus.Running;

    /// <summary>
    /// Stop receiving transmissions and
    /// </summary>
    public async Task StopReceiving()
    {
        if (!Receiving)
        {
            //Not receiving
            return;
        }

        //Stop the receiving thread
        m_receptorCancelToken.Cancel();
        await m_receptor;
    }

    /// <summary>
    /// Stop receiving and log off the connection.
    /// </summary>
    public async Task LogOff()
    {
        await StopReceiving();
        m_webClient.LogOff(m_sessionToken);
    }
    #endregion

    #region Reception
    /// <summary>
    /// Stores the last processed transmission to pass to the server so we can verify the client is receiving transmissions in the correct order
    /// and nothing happened to the transmission during transit back from the server
    /// </summary>
    private string m_lastProcessedTransmissionId = string.Empty;

    private int Receive()
    {
        if (Monitor.TryEnter(this, 0))
        {
            try
            {
                TransmissionContainer transmissionContainer;
                GetTransmissionResponse transmissionData;
                lock (m_lock)
                {
                    transmissionData = m_webClient.GetNextTransmissionData(new GetTransmissionRequest() { LastProcessedTransmissionId = m_lastProcessedTransmissionId });

                    if (transmissionData == null)
                    {
                        m_lastProcessedTransmissionId = String.Empty;
                        return 0;
                    }
                }

                using (BinaryMemoryReader reader = GenerateReader(transmissionData.TransmissionData))
                {
                    transmissionContainer = new TransmissionContainer(reader);
                }

                m_lastProcessedTransmissionId = transmissionContainer.TransmissionId.ToString();

                Transmission t;
                using (BinaryMemoryReader reader = GenerateReader(transmissionContainer.TransmissionBytes))
                {
                    t = (Transmission)m_transmissionClassFactory.Deserialize(reader);
                }

                FireReceiveEvent(t);
                return transmissionData.RemainingTransmissions;
            }
            catch (ApiException apiException)
            {
                //System is reachable but getting errors
                if (apiException.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    //The server has removed this session
                    //TODO: Relogin and restore data
                    StopReceiving();
                    //TODO: Fire 'dead connection' event
                }
                else if (apiException.StatusCode == (int)HttpStatusCode.Gone)
                {
                    //The provided last transmission number is no longer on the server, we cannot correct this
                    FireClientRestartRequiredEvent(Localizer.GetErrorString("3081"));
                    throw new PTException("3081");
                }
            }
            catch (Exception e) when (e is WebException or HttpRequestException)
            {
                //System is not reachable
                LogWebException(e);
                throw;
            }
            catch (Exception e)
            {
                SimpleExceptionLogger.LogException(BroadcastingAlerts.BroadcastingLogFilePath, e, "An error occurred while trying to receive uncompressed transmissions.");
                throw;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        return 0;
    }

    /// <summary>
    /// Receive transmissions with or without the wait interval (based on "receiveTrigger") until the "receiving" signal is raised.
    /// </summary>
    private void ContinuouslyReceiveTransmissions()
    {
        try
        {
            while (true)
            {
                if (m_receptorCancelToken.IsCancellationRequested)
                {
                    if (m_clientType != PTTransmissionBase.TransmissionSenderType.PTSystem)
                    {
                        //Clients can exit immediately because the data state is not preserved
                        return;
                    }
                }

                int transmissionReceived = 1;
                try
                {
                    while (transmissionReceived > 0)
                    {
                        transmissionReceived = Receive();
                        Thread.Sleep(100);
                    }
                }
                catch (Exception e)
                {
                    //Notifications of issues will be tracked by the connection state manager. We can simply try again later
                    m_interval = c_maxRetrieveInterval;
                }

                if (m_receptorCancelToken.IsCancellationRequested && transmissionReceived == 0)
                {
                    //Nothing left to process
                    return;
                }

                if (transmissionReceived > 0)
                {
                    //We received an action that we may have been waiting on, no need to poll so quickly now
                    m_interval = c_maxRetrieveInterval;
                }

                // Wait the specified interval for the next receive unless
                // something has set the trigger and thereby specified that the receiver
                // should perform the next receive right away.
                Thread.Sleep(m_interval); //Wait on itself
            }
        }
        catch (Exception)
        {
            //Done receiving. All handling and logging should have already taken place.
            //End Tread
        }
    }

    /// <summary>
    /// Sends a sequenced transmission to the System. Returns true if transmission sent, false otherwise.
    /// </summary>
    /// <param name="transmission"></param>
    /// <returns></returns>
    public bool SendClientAction(PTTransmission a_t, bool a_registerAwait = false)
    {
        return SendClientAction(a_t, BaseId.NULL_ID, a_registerAwait);
    }
    public bool SendClientAction(PTTransmission a_t, BaseId a_instigatorOverride, bool a_registerAwait = false)
    {
        if (a_t is PacketT packetT && packetT.Count == 1)
        {
            //Don't obscure the only action with a packetT
            return SendClientAction(packetT.First(), a_instigatorOverride, a_registerAwait);
        }


        if (a_t is not ImportT)
        {
            if (a_instigatorOverride != BaseId.NULL_ID)
            {
                a_t.Instigator = a_instigatorOverride;
            }
            else
            {
                a_t.Instigator = SystemController.CurrentUserId;
            }
        }

        Broadcast(a_t, a_registerAwait);
        return true;
    }

    public Guid SendClientActionsPacket(params PTTransmission[] a_transmissions)
    {
        return SendClientActionsPacket((IEnumerable<PTTransmission>)a_transmissions);
    }

    public Guid SendClientActionsPacket(IEnumerable<PTTransmission> a_transmissions)
    {
        PacketT transmissionsPacket = new ();

        foreach (PTTransmission t in a_transmissions)
        {
            transmissionsPacket.AddT(t);
        }

        if (transmissionsPacket.Count == 0)
        {
            return Guid.Empty;
        }


        if (SendClientAction(transmissionsPacket))
        {
            return transmissionsPacket.TransmissionId;
        }

        return Guid.Empty;
    }

    private void Broadcast(PTTransmission a_t, bool a_registerForAwaiter)
    {
        a_t.TransmissionSender = m_clientType;

        lock (m_lock)
        {
            if (a_registerForAwaiter)
            {
                CancellationTokenSource cancelTokenSource = new ();
                m_awaitingTransmissionList.Add(a_t.TransmissionId, cancelTokenSource);
            }

            byte[] compressedT;
            using (BinaryMemoryWriter writer = GenerateWriter())
            {
                writer.Write(a_t.UniqueId);
                a_t.Serialize(writer);
                compressedT = writer.GetBuffer();
            }

            m_webClient.Broadcast(compressedT, m_sessionToken);
            if (a_t is TriggerImportT)
            {
                if (PTSystem.EnableDiagnostics)
                {
                    PTSystem.SystemLogger.Log("Import Logging", "TriggerImportT broadcasted");
                }
            }
            else if (a_t is ImportT)
            {
                if (PTSystem.EnableDiagnostics)
                {
                    PTSystem.SystemLogger.Log("Import Logging", "ImportT broadcasted");
                }
            }

            lock (m_lock)
            {
                //This connection is waiting on the server to send back the action, poll more quickly
                m_interval = c_minRetrieveInterval;
            }

            //else
            {
                //This is used for scenarioiReplaceT to preserve the sender of the original transmission.
                //TODO:
                //m_broadcaster.TransmissionReceived(compressedT, a_connectionNbrOverride);
            }
        }
    }
    #endregion

    //public override string ToString()
    //{
    //    StringBuilder sb = new StringBuilder();

    //    TimeSpan activeLengthTicks = DateTime.UtcNow - CreationDate;

    //    sb.Append("System Initialization info:").AppendLine();
    //    sb.AppendFormat("Time Waiting On System: {0}", new TimeSpan(m_initializationTiming.TimeWaitingForScenarioData)).AppendLine();
    //    sb.AppendFormat("Time Retrieving Scenario Data: {0}", new TimeSpan(m_initializationTiming.TimeTransferingScenarioData)).AppendLine();
    //    sb.AppendFormat("Scenario Data Size: {0}", m_initializationTiming.ScenarioBytesTransferred).AppendLine();
    //    sb.AppendFormat("Deserialization Time: {0}", new TimeSpan(m_initializationTiming.TimeToDeserializeSystemTicks)).AppendLine();
    //    sb.AppendFormat("Total Initialization Time: {0}", new TimeSpan(m_initializationTiming.TimeToLoadDeserializeAndPerformOtherInitializationsTicks)).AppendLine();

    //    if (m_initializationTiming.TimeoutIntervalTicks > 0 || m_initializationTiming.ContactIntervalTicks > 0)
    //    {
    //        if (m_initializationTiming.TimeoutIntervalTicks > 0)
    //        {
    //            sb.AppendFormat("Timeout Interval: {0}", new TimeSpan(m_initializationTiming.TimeoutIntervalTicks)).AppendLine();
    //        }

    //        if (m_initializationTiming.ContactIntervalTicks > 0)
    //        {
    //            sb.AppendFormat("Contact Interval: {0}", new TimeSpan(m_initializationTiming.ContactIntervalTicks)).AppendLine();
    //        }

    //        sb.AppendLine();
    //    }

    //    if (m_initializationTiming.ClientFullyLoaded > 0)
    //    {
    //        sb.Append("Client Initialization info:").AppendLine();
    //        sb.AppendFormat("Client Updater Finished: {0}", new TimeSpan(m_initializationTiming.ClientUpdaterFinished)).AppendLine();
    //        sb.AppendFormat("Workspaces Loaded: {0}", new TimeSpan(m_initializationTiming.ClientWorkspacesLoaded)).AppendLine();
    //        sb.AppendFormat("Workspaces Applied: {0}", new TimeSpan(m_initializationTiming.ClientWorkspacesApplied)).AppendLine();
    //        sb.AppendFormat("Scenarios Loaded: {0}", new TimeSpan(m_initializationTiming.ClientScneariosLoaded)).AppendLine();
    //        sb.AppendFormat("Client Initialization Time: {0}", new TimeSpan(m_initializationTiming.ClientFullyLoaded)).AppendLine();
    //        sb.AppendLine();
    //    }

    //    sb.Append("Activity info:").AppendLine();
    //    sb.AppendFormat("Connection Length: {0}", activeLengthTicks).AppendLine();
    //    sb.AppendFormat("Nbr Of Transmissions Sent: {0}", NbrOfTransmissionsSent).AppendLine();
    //    sb.AppendFormat("Nbr of Transmissions Received: {0}", NbrOfTransmissionsReceived).AppendLine();
    //    sb.AppendLine();
    //    sb.Append("Round Trip connection info:").AppendLine();
    //    sb.Append(m_roundTripConnectionData).AppendLine();
    //    sb.AppendLine();
    //    sb.Append("Intervals Between Round Trips info:").AppendLine();
    //    sb.Append(m_delayBetweenConnectionsData);
    //    if (m_initializationTiming.TimeoutIntervalTicks > 0)
    //    {
    //        sb.AppendLine();
    //        sb.AppendLine();
    //        sb.Append("Timeout and Max comparison:").AppendLine();
    //        long ttlMax = m_roundTripConnectionData.MaxTimeTicks + m_delayBetweenConnectionsData.MaxTimeTicks;
    //        long difference = m_initializationTiming.TimeoutIntervalTicks - ttlMax;
    //        sb.AppendFormat("Timeout-MaxRoundTrip-MaxInterval: {0}", new TimeSpan(difference)).AppendLine();
    //        double maxEstimatedPctToTimeout = ((double)ttlMax / m_initializationTiming.TimeoutIntervalTicks) * 100;
    //        sb.AppendFormat("Estimated % to Timeout: {0}", maxEstimatedPctToTimeout);
    //    }

    //    if (LastReceiveStartDate > LastReceiveFinishDate)
    //    {
    //        long lengthOfInProcessReceive = (DateTime.UtcNow - LastReceiveFinishDate).Ticks;
    //        long inProcessReceiveTimeoutDifference = m_initializationTiming.TimeoutIntervalTicks - lengthOfInProcessReceive;
    //        sb.AppendLine();
    //        sb.AppendFormat("In Process Receive Timeout Difference: {0}", new TimeSpan(inProcessReceiveTimeoutDifference)).AppendLine();
    //        double maxEstimatedPctToTimeout = ((double)lengthOfInProcessReceive / m_initializationTiming.TimeoutIntervalTicks) * 100;
    //        sb.AppendFormat("In Process Estimated % to Timeout: {0}", maxEstimatedPctToTimeout);
    //    }

    //    return sb.ToString();
    //}

    #region Exception logging helpers
    protected static void LogWebException(Exception a_e)
    {
        SimpleExceptionLogger.LogException(BroadcastingAlerts.SystemNetWebExceptionLogFilePath, a_e, WebExceptionCommonCauses);
    }

    public static string WebExceptionCommonCauses => "Common causes for this include: the PlanetTogether System service may have stopped in which case it can be restarted from the Control Panel | Services screen.";
    #endregion

    private readonly Dictionary<Guid, CancellationTokenSource> m_awaitingTransmissionList = new ();

    public async Task WaitForTransmissionReceive(Guid a_senderLastTransmissionNbr)
    {
        if (m_awaitingTransmissionList.TryGetValue(a_senderLastTransmissionNbr, out CancellationTokenSource tokenSource))
        {
            if (!tokenSource.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, tokenSource.Token); //Delay until cancel
                }
                catch (TaskCanceledException)
                {
                    //this is expected, it is how the task gets canceled
                }
                catch (Exception e)
                {
                    //This should not happen
                    return;
                }
            }

            m_awaitingTransmissionList.Remove(a_senderLastTransmissionNbr);
        }
    }
}