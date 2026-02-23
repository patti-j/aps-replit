namespace PT.Transmissions;

/// <summary>
/// Contains a transmission and some information about it.
/// This can be serialized through rest APIs with Json or with IPTSerialization
/// </summary>
public class TransmissionContainer
{
    public TransmissionContainer(byte[] a_transmissionBytes, Transmission a_t)
    {
        m_transmissionBytes = a_transmissionBytes;
        m_transmissionNbr = a_t.TransmissionNbr;
        m_timeStamp = a_t.TimeStamp.ToDateTime();
        m_transmissionId = a_t.TransmissionId;
    }

    #region IPTSerializable Members
    public TransmissionContainer(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12427)
        {
            a_reader.Read(out m_transmissionBytes);
            a_reader.Read(out m_transmissionNbr);
            a_reader.Read(out m_connectionNbr);
            a_reader.Read(out m_timeStamp);
            a_reader.Read(out byte[] idBytes);
            m_transmissionId = new Guid(idBytes);
        }
        else if (a_reader.VersionNumber >= 12300)
        {
            a_reader.Read(out m_transmissionBytes);
            a_reader.Read(out m_transmissionNbr);
            a_reader.Read(out m_connectionNbr);
            a_reader.Read(out m_timeStamp);
        }
        else if (a_reader.VersionNumber >= 12214)
        {
            a_reader.Read(out m_transmissionBytes);
            a_reader.Read(out m_transmissionNbr);
            a_reader.Read(out m_connectionNbr);
            a_reader.Read(out m_timeStamp);
            a_reader.Read(out byte[] idBytes);
            m_transmissionId = new Guid(idBytes);
        }
        else if (a_reader.VersionNumber >= 129)
        {
            a_reader.Read(out m_transmissionBytes);
            a_reader.Read(out m_transmissionNbr);
            a_reader.Read(out m_connectionNbr);
            a_reader.Read(out m_timeStamp);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_transmissionBytes);
        a_writer.Write(m_transmissionNbr);
        a_writer.Write(m_connectionNbr);
        a_writer.Write(m_timeStamp);
        a_writer.Write(m_transmissionId.ToByteArray());
    }

    public const int UNIQUE_ID = 424;

    public int UniqueId => UNIQUE_ID;
    #endregion

    private byte[] m_transmissionBytes;

    /// <summary>
    /// The serialized compressed bytes of the transmission.
    /// </summary>
    public byte[] TransmissionBytes
    {
        get => m_transmissionBytes;
        set => m_transmissionBytes = value;
    }

    private ulong m_transmissionNbr;

    /// <summary>
    /// The number of the transmission as set by the broadcaster.
    /// </summary>
    public ulong TransmissionNbr
    {
        get => m_transmissionNbr;
        set => m_transmissionNbr = value;
    }

    // Is this even used? Should it just be deleted? 
    private readonly int m_connectionNbr;

    /// <summary>
    /// The number of the connection that sent this transmission.
    /// </summary>
    public int ConnectionNbr => m_connectionNbr;

    private readonly DateTime m_timeStamp;

    /// <summary>
    /// This is when the transmission passed through the broadcaster.
    /// </summary>
    public DateTime TimeStamp => m_timeStamp;

    private readonly Guid m_transmissionId;

    public Guid TransmissionId => m_transmissionId;

}