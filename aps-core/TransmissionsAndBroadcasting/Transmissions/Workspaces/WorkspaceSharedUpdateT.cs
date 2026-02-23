namespace PT.Transmissions;

/// <summary>
/// Transmission for sending usersettings files to the server
/// </summary>
public class WorkspaceSharedUpdateT : UserBaseT
{
    public const int UNIQUE_ID = 1010;

    #region IPTSerializable Members
    public WorkspaceSharedUpdateT(IReader a_reader)
        : base(a_reader)
    {
        a_reader.Read(out m_workspaceName);
        a_reader.Read(out m_workspaceBytes);
        m_bools = new BoolVector32(a_reader);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_workspaceName);
        a_writer.Write(m_workspaceBytes);
        m_bools.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private BoolVector32 m_bools;
    private const short c_deleteIdx = 0;
    private const short c_overwriteIdx = 1;
    private const short c_resetOtherSettingsIdx = 2;

    public WorkspaceSharedUpdateT() { }

    public WorkspaceSharedUpdateT(string a_sharedWorkspaceName, byte[] a_userSettings)
    {
        m_workspaceName = a_sharedWorkspaceName;
        m_workspaceBytes = a_userSettings;
    }

    private readonly byte[] m_workspaceBytes;

    public byte[] WorkspaceBytes => m_workspaceBytes;

    private readonly string m_workspaceName;

    public string WorkspaceName => m_workspaceName;

    public bool Delete
    {
        get => m_bools[c_deleteIdx];
        set => m_bools[c_deleteIdx] = value;
    }

    public bool Overwrite
    {
        get => m_bools[c_overwriteIdx];
        set => m_bools[c_overwriteIdx] = value;
    }

    public bool ResetOtherSettings
    {
        get => m_bools[c_resetOtherSettingsIdx];
        set => m_bools[c_resetOtherSettingsIdx] = value;
    }
}