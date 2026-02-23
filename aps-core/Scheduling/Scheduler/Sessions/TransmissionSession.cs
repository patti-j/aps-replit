using System.Net;

using PT.APSCommon;
using PT.Common.Collections;
using PT.Common.Compression;
using PT.Common.Exceptions;
using PT.Common.Http;
using PT.Transmissions;

namespace PT.Scheduler.Sessions;

/// <summary>
/// Maintains transmissions for a connection.
/// </summary>
public class TransmissionSession : EncryptedSession
{
    /// <summary>
    /// Whether this session has started receiving transmissions related to a scenario sent
    /// </summary>
    private bool m_sessionIsLoggedIn;

    internal bool SessionIsLoggedIn => m_sessionIsLoggedIn;

    /// <summary>
    /// A set of transmissions received from the broadcaster and bound for the client.
    /// </summary>
    private readonly HashedQueue<TransmissionContainer> m_transmissions = new ();

    /// <summary>
    /// Construct a bidirectional connection
    /// </summary>
    public TransmissionSession(string a_sessionToken) : base(a_sessionToken)
    {
        SetCompression(ECompressionType.Normal);
    }

    /// <summary>
    /// Get the transmissions forwarded to this connection by the broadcaster.
    /// </summary>
    /// <returns>An array containing TransmissionContainers.</returns>
    public byte[] DequeueTransmissionContainer(string a_previousTransmissionId)
    {
        TransmissionContainer nextTransmission;
        DateTime utcNow = DateTime.UtcNow;

        lock (m_transmissions)
        {
            NotifyLastReceiveCallStarted(utcNow);

            if (m_transmissions.Count == 0)
            {
                return null;
            }

            nextTransmission = m_transmissions.Peek();
            if (nextTransmission.TransmissionId.ToString() == a_previousTransmissionId)
            {
                m_transmissions.Dequeue();
                if (m_transmissions.Count == 0)
                {
                    return null;
                }
                nextTransmission = m_transmissions.Peek();
            }
            else if (!string.IsNullOrEmpty(a_previousTransmissionId))
            {
                throw new ApiException() { StatusCode = (int)HttpStatusCode.Gone };
            }
        }

        byte[] bytes;
        using (BinaryMemoryWriter writer = GenerateWriter())
        {
            nextTransmission.Serialize(writer);
            bytes = writer.GetBuffer();
        }

        NotifyReceiveCallFinished(utcNow);

        return bytes;
    }

    internal void Add(Transmission a_t)
    {
        byte[] transmissionByteArray;
        using (BinaryMemoryWriter writer = GenerateWriter())
        {
            writer.Write(a_t.UniqueId);
            a_t.Serialize(writer);
            transmissionByteArray = writer.GetBuffer();
        }

        TransmissionContainer tc = new (transmissionByteArray, a_t);
        Add(tc);
    }

    private void Add(TransmissionContainer a_tc)
    {
        lock (m_transmissions)
        {
            if (!m_transmissions.Enqueue(a_tc))
            {
                throw new PTHandleableException("3111", new object[] {a_tc.TransmissionId.ToString()});
            }
        }
    }

    /// <summary>
    /// Used to flag that the session has been logged in and that its transmissions are synced up with the server session's transmissions.
    /// </summary>
    internal void SetSessionIsLoggedIn()
    {
        m_sessionIsLoggedIn = true;
    }

    public Transmission GetSerializedTransmission(byte[] a_data, IClassFactory a_classFactory)
    {
        using BinaryMemoryReader reader = GenerateReader(a_data);
        return (Transmission)a_classFactory.Deserialize(reader);
    }

    public int GetQueuedTransmissionsCount()
    {
        lock (m_transmissions)
        {
            return m_transmissions.Count;
        }
    }

    /// <summary>
    /// This function is used to sync up the transmissions between the server's ClientSession and the logging in user's ClientSession.
    /// It is called within the m_connectionLock. The function's purpose is to resolve an issue where the logging in user's client session
    /// is not synced up with the server session, which can cause a desync. 
    /// </summary>
    /// <param name="a_clientTransmissionSession">The transmission session for the client that's logging in</param>
    internal void SyncTransmissions(TransmissionSession a_clientTransmissionSession)
    {
        // Theoretically, we don't need to lock any of the m_transmissions usage in the function
        // because this function is called while holding the m_connectionsLock on ServerSessionManager
        // so the queues shouldn't be getting modified. 
        a_clientTransmissionSession.m_transmissions.Clear();
        if (m_transmissions.Count == 0)
        {
            return;
        }
        foreach (TransmissionContainer transmission in m_transmissions)
        {
            a_clientTransmissionSession.m_transmissions.Enqueue(transmission);
        }

        a_clientTransmissionSession.SetSessionIsLoggedIn();
    }
}