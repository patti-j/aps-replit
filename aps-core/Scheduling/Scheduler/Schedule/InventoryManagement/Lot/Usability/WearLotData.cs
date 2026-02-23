namespace PT.Scheduler;

public partial class WearLotData : ILotData
{
    #region Serialization
    internal WearLotData(IReader a_reader)
    {
        a_reader.Read(out m_wearAmount);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_wearAmount);
    }

    public const int UNIQUE_ID = 748;

    public int UniqueId => UNIQUE_ID;
    #endregion

    internal WearLotData(int a_wearAmount)
    {
        WearAmount = a_wearAmount;
    }

    internal WearLotData()
    {
        WearAmount = -1;
    }

    private int m_wearAmount;

    public int WearAmount
    {
        get => m_wearAmount;
        private set => m_wearAmount = value;
    }

    public bool WearLimited => m_wearAmount != -1;
}