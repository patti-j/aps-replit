namespace PT.Transmissions;

/// <summary>
/// Transmission for sending usersettings files to the server
/// </summary>
public class WorkspaceSharedDeleteT : PTTransmission
{
    public static readonly int UNIQUE_ID = 801;

    #region IPTSerializable Members
    public WorkspaceSharedDeleteT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 513)
        {
            a_reader.Read(out m_sharedWorkspaceToDelete);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_sharedWorkspaceToDelete);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public WorkspaceSharedDeleteT() { }

    public WorkspaceSharedDeleteT(string a_sharedWorkspaceName)
    {
        m_sharedWorkspaceToDelete = a_sharedWorkspaceName;
    }

    private readonly string m_sharedWorkspaceToDelete;

    public string SharedWorkspaceToDelete => m_sharedWorkspaceToDelete;
}