namespace PT.Scheduler;

public partial class WearRequirement : IPTSerializable
{
    #region Serialization
    internal WearRequirement(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 427)
        {
            a_reader.Read(out m_maxWearAmount);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(MaxWearAmount);
    }

    private const int UNIQUE_ID = 746;

    public int UniqueId => UNIQUE_ID;
    #endregion

    /// <summary>
    /// Default no constraint
    /// </summary>
    internal WearRequirement()
    {
        m_maxWearAmount = int.MaxValue;
    }

    internal WearRequirement(int a_maxWearAmount)
    {
        MaxWearAmount = a_maxWearAmount;
    }

    private int m_maxWearAmount;

    /// <summary>
    /// Only material whose Wear is less than or equal to this amount is eligible for use.
    /// </summary>
    public int MaxWearAmount
    {
        get => m_maxWearAmount;
        private set => m_maxWearAmount = value;
    }
}