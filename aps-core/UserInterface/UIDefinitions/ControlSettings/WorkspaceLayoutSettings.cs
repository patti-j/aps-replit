namespace PT.UIDefinitions.ControlSettings;

public class WorkspaceLayoutSettings : IPTSerializable
{
    #region IPTSerializable Members
    private BoolVector32 m_bools;

    public WorkspaceLayoutSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 607)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_detailsSliderPosition);
        }
    }

    private const int c_DetailsCollapsedIdx = 0;

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write(m_detailsSliderPosition);
    }

    public int UniqueId => 1027;
    #endregion

    private int m_detailsSliderPosition;

    public WorkspaceLayoutSettings()
    {
        DetailsSliderPosition = 0;
    }

    public int DetailsSliderPosition
    {
        get => m_detailsSliderPosition;
        set => m_detailsSliderPosition = value;
    }

    public bool DetailsCollapsed
    {
        get => m_bools[c_DetailsCollapsedIdx];
        set => m_bools[c_DetailsCollapsedIdx] = value;
    }
}