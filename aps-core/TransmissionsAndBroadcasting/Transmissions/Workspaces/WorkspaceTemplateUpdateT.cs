namespace PT.Transmissions;

/// <summary>
/// Transmission for sending usersettings files to the server
/// </summary>
public class WorkspaceTemplateUpdateT : PTTransmission
{
    public static readonly int UNIQUE_ID = 802;

    #region IPTSerializable Members
    public WorkspaceTemplateUpdateT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 513)
        {
            a_reader.Read(out m_workspaceName);
            a_reader.Read(out m_zippedBytes);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_workspaceName);
        a_writer.Write(m_zippedBytes);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public WorkspaceTemplateUpdateT() { }

    public WorkspaceTemplateUpdateT(string a_sharedWorkspaceName, byte[] a_userSettings)
    {
        m_workspaceName = a_sharedWorkspaceName;
        m_zippedBytes = a_userSettings;
    }

    private readonly byte[] m_zippedBytes;

    public byte[] ZipFileBytes => m_zippedBytes;

    private readonly string m_workspaceName;

    public string WorkspaceName => m_workspaceName;
}