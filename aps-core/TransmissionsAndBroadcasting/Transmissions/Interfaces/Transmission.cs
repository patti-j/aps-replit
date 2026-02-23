using System.Xml.Serialization;

namespace PT.Transmissions;

/// <summary>
/// A transmission sent to the broadcaster.
/// </summary>
public abstract class Transmission : IPTSerializable
{
    //public const int UNIQUE_ID = 19;

    #region PT Serialization
    protected Transmission(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 13000)
        {
            a_reader.Read(out m_transmissionNbr);
            a_reader.ReadDateTimeToOffsetConversion(out m_timeStamp);
            a_reader.Read(out byte[] idBytes);
            m_transmissionId = new Guid(idBytes);
        }
        else if (a_reader.VersionNumber >= 12008)
        {
            a_reader.Read(out ulong m_lastTransmissionNbr);
            a_reader.Read(out m_transmissionNbr);
            a_reader.ReadDateTimeToOffsetConversion(out m_timeStamp);
            a_reader.Read(out byte[] idBytes);
            m_transmissionId = new Guid(idBytes);
        }
        else if (a_reader.VersionNumber >= 12006)
        {
            a_reader.Read(out ulong m_lastTransmissionNbr);
            a_reader.Read(out m_transmissionNbr);
            a_reader.ReadDateTimeToOffsetConversion(out m_timeStamp);
            a_reader.Read(out DateTime m_instigatorPcDisplayTime);
        }
        else if (a_reader.VersionNumber >= 172)
        {
            a_reader.Read(out ulong m_lastTransmissionNbr);
            a_reader.Read(out m_transmissionNbr);
            a_reader.Read(out int m_connectionNbr);
            a_reader.ReadDateTimeToOffsetConversion(out m_timeStamp);
            a_reader.Read(out DateTime m_instigatorPcDisplayTime);
        }
        else if (a_reader.VersionNumber >= 129)
        {
            a_reader.Read(out ulong m_lastTransmissionNbr);
            a_reader.Read(out m_transmissionNbr);
            a_reader.Read(out int m_connectionNbr);
            a_reader.ReadDateTimeToOffsetConversion(out m_timeStamp);
        }
        else if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out ulong m_lastTransmissionNbr);
            a_reader.Read(out m_transmissionNbr);

            a_reader.Read(out ulong tempConnectionNbr);

            a_reader.ReadDateTimeToOffsetConversion(out m_timeStamp);
        }
    }

    public virtual void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        a_writer.Write(m_transmissionNbr);
        a_writer.Write(m_timeStamp);
        a_writer.Write(m_transmissionId.ToByteArray());
    }

    public abstract int UniqueId { get; }
    #endregion

    protected Transmission()
    {
        m_transmissionId = Guid.NewGuid();
    }

    protected Transmission(Transmission a_t)
    {
        m_transmissionNbr = a_t.m_transmissionNbr;
        m_timeStamp = a_t.m_timeStamp;
        m_transmissionId = a_t.m_transmissionId;
    }

    /// <summary>
    /// Within PT this value is constantly being incremented by each new transmission and is stored between saves and loads and so eventually ends up being quite high.
    /// When each transmission is received it is verified to be the next transmission in sequence. This value is used to make sure all the transmissions that are
    /// processed by the system are received in the proper order.
    /// This value is set by the broadcaster as it receives each transmission.
    /// Currently I know of only one way that this value can be incorrect. That is in the case where the StartupSystem that the client receives has already processed
    /// some of the messages in the connections transmission queue.
    /// This is possible because when clients connect first a connection is made allowing transmissions to be stored in it. Then a copy of the
    /// system is sent down from the server.
    /// </summary>
    private ulong m_transmissionNbr;

    [XmlIgnore]
    public ulong TransmissionNbr
    {
        get => m_transmissionNbr;
        set => m_transmissionNbr = value;
    }

    private DateTimeOffset m_timeStamp;

    /// <summary>
    /// This is the PT Server time, not display time.
    /// </summary>
    public DateTimeOffset TimeStamp => m_timeStamp;

    public virtual void SetTimeStamp(DateTimeOffset a_dateTime)
    {
        if (m_timeStamp.Ticks == 0)
        {
            m_timeStamp = a_dateTime;
        }
    }

    private readonly Guid m_transmissionId;

    /// <summary>
    /// A unique id for this transmission that gets set when the transmission is created. This will not change
    /// </summary>
    public Guid TransmissionId => m_transmissionId;

    public static byte[] Compress(Transmission a_t, Common.Compression.ECompressionType a_compressionType = Common.Compression.ECompressionType.Normal)
    {
        using (BinaryMemoryWriter writer = new (a_compressionType))
        {
            writer.Write(a_t.UniqueId);
            a_t.Serialize(writer);
            return writer.GetBuffer();
        }
    }

    public static Transmission Decompress(byte[] a_tba, IClassFactory a_classFactory)
    {
        using (BinaryMemoryReader bfr = new (a_tba))
        {
            return (Transmission)a_classFactory.Deserialize(bfr);
        }
    }

    public virtual string Description => DEFAULT_DESCRIPTION;

    public static readonly string DEFAULT_DESCRIPTION = "Updated an object in a specific Scenario.";
}