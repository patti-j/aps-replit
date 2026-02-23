namespace PT.APIDefinitions.RequestsAndResponses;

public class GetTransmissionResponse : IPTSerializable
{
    public const int UNIQUE_ID = 1200; // New API Serialization Ids begin at 12xx

    public GetTransmissionResponse() { }

    #region IPTSerializable Members
    public void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        a_writer.Write(RemainingTransmissions);
        a_writer.Write(TransmissionData);
    }

    public GetTransmissionResponse(IReader a_reader)
    {
        a_reader.Read(out m_remainingTransmissions);
        a_reader.Read(out m_transmissionData);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    private int m_remainingTransmissions;

    public int RemainingTransmissions
    {
        get => m_remainingTransmissions;
        set => m_remainingTransmissions = value;
    }

    private byte[] m_transmissionData;

    public byte[] TransmissionData
    {
        get => m_transmissionData;
        set => m_transmissionData = value;
    }
}