using PT.Common.Compression;

namespace PT.Transmissions;

/// <summary>
/// Provides a new CoPilot settings for the ScenarioManager
/// Only serializes the settings if they have been updated.
/// </summary>
public class PruneScenarioT : ScenarioBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 815;

    #region IPTSerializable Members
    public PruneScenarioT(IReader reader) : base(reader)
    {
        m_transmissionList = new List<byte[]>();
        if (reader.VersionNumber >= 608)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                byte[] bytes;
                reader.Read(out bytes);
                m_transmissionList.Add(bytes);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(m_transmissionList.Count);
        foreach (byte[] bytes in m_transmissionList)
        {
            writer.Write(bytes);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private readonly List<byte[]> m_transmissionList;

    public List<byte[]> PTTransmissionBytesList => m_transmissionList;

    public PruneScenarioT() { }

    public PruneScenarioT(List<PTTransmission> a_transmissions)
    {
        m_transmissionList = new List<byte[]>();
        foreach (PTTransmission transmission in a_transmissions)
        {
            using (BinaryMemoryWriter writer = new (ECompressionType.Fast))
            {
                writer.Write(transmission.UniqueId);
                transmission.Serialize(writer);
                m_transmissionList.Add(writer.GetBuffer());
            }
        }
    }
}