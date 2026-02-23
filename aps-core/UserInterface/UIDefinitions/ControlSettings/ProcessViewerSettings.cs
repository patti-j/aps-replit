namespace PT.UIDefinitions.ControlSettings;

public class ProcessViewerSettings
{
    #region IPTSerializable Members
    public ProcessViewerSettings(IReader a_reader)
    {
        m_processFiles = new Dictionary<string, byte[]>();
        if (a_reader.VersionNumber >= 633)
        {
            int count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                string name;
                byte[] fileBytes;
                a_reader.Read(out name);
                a_reader.Read(out fileBytes);
                m_processFiles.Add(name, fileBytes);
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_processFiles.Count);
        foreach (KeyValuePair<string, byte[]> keyValuePair in m_processFiles)
        {
            a_writer.Write(keyValuePair.Key);
            a_writer.Write(keyValuePair.Value);
        }
    }
    #endregion

    private Dictionary<string, byte[]> m_processFiles;

    public ProcessViewerSettings()
    {
        m_processFiles = new Dictionary<string, byte[]>();
    }

    public Dictionary<string, byte[]> ProcessFiles
    {
        get => m_processFiles;
        set => m_processFiles = value;
    }
}