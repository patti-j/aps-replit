namespace PT.Scheduler;

/// <summary>
/// Backwards compatible serialization for TransmissionSequencer. Sequenced feature has been removed.
/// </summary>
internal class TransmissionSequencer
{
    #region IPTSerializable Members
    internal TransmissionSequencer(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12020)
        {
            a_reader.Read(out ulong m_lastTransmissionNbr);
            a_reader.Read(out Guid m_lastTransmissionId);
        }
        else if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out ulong m_lastTransmissionNbr);
        }
    }

    public const int UNIQUE_ID = 343;

    public int UniqueId => UNIQUE_ID;
    #endregion
}