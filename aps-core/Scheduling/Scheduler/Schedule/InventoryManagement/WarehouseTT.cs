namespace PT.Scheduler;

/// <summary>
/// A helper class of WarehouseManager used to store warhouse to warehouse transfer times.
/// </summary>
internal class WarehouseTT : IPTSerializable
{
    private readonly string m_fromWHExtId;

    internal string FromWHExtId => m_fromWHExtId;

    private readonly string m_toWHExtId;

    internal string ToWHExtId => m_toWHExtId;

    private readonly long m_tt;

    internal long TT => m_tt;

    internal WarehouseTT(string a_fromWHExtId, string a_toWHExtId, long a_ttTicks)
    {
        m_fromWHExtId = a_fromWHExtId;
        m_toWHExtId = a_toWHExtId;
        m_tt = a_ttTicks;
    }

    public int UniqueId = 99999;

    int IPTSerializable.UniqueId => UniqueId;

    internal WarehouseTT(IReader a_reader)
    {
        a_reader.Read(out m_fromWHExtId);
        a_reader.Read(out m_toWHExtId);
        a_reader.Read(out m_tt);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_fromWHExtId);
        a_writer.Write(m_toWHExtId);
        a_writer.Write(m_tt);
    }
}