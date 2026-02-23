using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.PackageInterfaces;

namespace PT.ScenarioControls.PackageHelpers;

public class DocumentManagerContainer : ISettingData
{
    /// <summary>
    /// Dictionary of Documents containing Boards that were open in the Workspace and whether they were visible in the Document group
    /// </summary>
    public Dictionary<string, bool> ActiveDocuments;

    /// <summary>
    /// List of mapped and loaded Documents containing Boards
    /// </summary>
    public List<string> LoadedDocuments;

    /// <summary>
    /// The serialized byte array that represents the Boards Layout
    /// </summary>
    public byte[] TileLayoutBytes;

    private string m_settingKey;

    public DocumentManagerContainer(IReader a_reader)
    {
        ActiveDocuments = new Dictionary<string, bool>();
        LoadedDocuments = new List<string>();

        a_reader.Read(out m_settingKey);
        a_reader.Read(out int count);
        for (int i = 0; i < count; i++)
        {
            a_reader.Read(out string boardName);
            a_reader.Read(out bool selected);
            ActiveDocuments.Add(boardName, selected);
        }

        a_reader.Read(out count);
        for (int i = 0; i < count; i++)
        {
            a_reader.Read(out string boardName);
            LoadedDocuments.Add(boardName);
        }

        a_reader.Read(out TileLayoutBytes);
    }

    public DocumentManagerContainer()
    {
        ActiveDocuments = new Dictionary<string, bool>();
        LoadedDocuments = new List<string>();
        TileLayoutBytes = null;
    }

    public DocumentManagerContainer(List<string> a_visiblePanes, byte[] a_layoutBytes, string a_settingKey)
    {
        TileLayoutBytes = a_layoutBytes;
        m_settingKey = a_settingKey;
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_settingKey);

        a_writer.Write(ActiveDocuments.Count);
        foreach (KeyValuePair<string, bool> pair in ActiveDocuments)
        {
            a_writer.Write(pair.Key);
            a_writer.Write(pair.Value);
        }

        a_writer.Write(LoadedDocuments.Count);
        foreach (string docName in LoadedDocuments)
        {
            a_writer.Write(docName);
        }

        a_writer.Write(TileLayoutBytes);
    }

    /// <summary>
    /// Remove all Documents that were previously mapped and loaded if they are no longer present in existing packages
    /// or were loaded from packages that are no longer there.
    /// </summary>
    /// <param name="a_paneInfos"></param>
    public void RemoveBoardsNoLongerLoaded(List<IBoardModule> a_paneInfos)
    {
        for (int i = LoadedDocuments.Count - 1; i >= 0; i--)
        {
            if (!a_paneInfos.Any(x => x.BoardKey == LoadedDocuments[i]))
            {
                LoadedDocuments.RemoveAt(i);
            }
        }
    }

    public IEnumerable<string> GetSelectedBoards()
    {
        foreach ((string boardKey, bool boardActive) in ActiveDocuments)
        {
            if (boardActive)
            {
                yield return boardKey;
            }
        }
    }

    public int UniqueId => 1029;

    public string SettingKey
    {
        get => m_settingKey;
        set => m_settingKey = value;
    }

    public string Description => "Board Layout Settings";
    public string SettingsGroup => SettingGroupConstants.LayoutSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.WorkspaceViewSettings;
    public string SettingCaption { get; set; }
}