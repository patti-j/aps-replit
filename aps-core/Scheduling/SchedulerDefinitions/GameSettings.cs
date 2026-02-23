namespace PT.SchedulerDefinitions;

public class GameSettings
{
    public const int UNIQUE_ID = 794;

    #region IPTSerializable Members
    public GameSettings(IReader reader)
    {
        if (reader.VersionNumber >= 493)
        {
            reader.Read(out m_difficulty);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_difficulty);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public GameSettings() { }

    private int m_difficulty;

    public int Difficulty
    {
        get => m_difficulty;
        set => m_difficulty = value;
    }
}