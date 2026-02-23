using System.Collections;

namespace PT.Transmissions;

/// <summary>
/// A transmission container that can hold a series of transmissions. ScenarioBaseT descendants can be stored, including ERP transmissions
/// </summary>
public class PacketT : PTTransmission, IEnumerable<PTTransmission>
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 833;

    public PacketT(IReader a_reader, ObjectCreatorDelegate a_deserializeDelegate)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                AddT((PTTransmission)a_deserializeDelegate(a_reader));
            }

            a_reader.Read(out m_description);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_transmissionQueue.Count);

        foreach (PTTransmission ptTransmission in this)
        {
            a_writer.Write(ptTransmission.UniqueId);
            ptTransmission.Serialize(a_writer);
        }

        a_writer.Write(m_description);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public PacketT() { }

    private string m_description = "";
    private readonly Queue<PTTransmission> m_transmissionQueue = new ();

    public void AddT(PTTransmission a_t)
    {
        a_t.Instigator = Instigator;
        //Store the last description/ 
        m_description = a_t.Description;
        m_transmissionQueue.Enqueue(a_t);
    }

    public int Count => m_transmissionQueue.Count;

    public override string Description => m_description;

    public IEnumerator<PTTransmission> GetEnumerator()
    {
        return m_transmissionQueue.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override void SetTimeStamp(DateTimeOffset a_dateTime)
    {
        foreach (PTTransmission transmission in this)
        {
            transmission.SetTimeStamp(a_dateTime);
        }
    }
}