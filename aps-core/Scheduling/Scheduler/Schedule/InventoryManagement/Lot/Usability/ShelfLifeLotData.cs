namespace PT.Scheduler;

public partial class ShelfLifeLotData : ILotData
{
    #region Serialization
    internal ShelfLifeLotData(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12529)
        {
            a_reader.Read(out m_expirationTicks);
            a_reader.Read(out m_expirationWarningTicks);
        }
        else
        {
            a_reader.Read(out m_expirationTicks);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_expirationTicks);
        a_writer.Write(m_expirationWarningTicks);
    }

    public const int UNIQUE_ID = 747;

    public int UniqueId => UNIQUE_ID;
    #endregion

    internal ShelfLifeLotData(long a_expirationDateTicks, long a_expirationWarningDateTicks)
    {
        ExpirationTicks = a_expirationDateTicks;
        ExpirationWarningTicks = a_expirationWarningDateTicks;
    }

    internal ShelfLifeLotData()
    {
        ExpirationTicks = PTDateTime.InvalidDateTime.Ticks;
    }

    private long m_expirationTicks;

    internal long ExpirationTicks
    {
        get => m_expirationTicks;
        private set => m_expirationTicks = value;
    }

    public DateTime ExpirationDate => new (m_expirationTicks);

    public bool Expirable => m_expirationTicks != PTDateTime.InvalidDateTime.Ticks;

    private long m_expirationWarningTicks;

    internal long ExpirationWarningTicks
    {
        get => m_expirationWarningTicks;
        private set => m_expirationWarningTicks = value;
    }

    public DateTime ExpirationWarningDate => new(m_expirationWarningTicks);
    public bool UseWarning => m_expirationWarningTicks != PTDateTime.InvalidDateTime.Ticks && m_expirationWarningTicks != m_expirationTicks;
}