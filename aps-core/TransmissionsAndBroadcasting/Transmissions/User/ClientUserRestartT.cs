using PT.APSCommon;

namespace PT.Transmissions.User;

/// <summary>
/// Send transmission to alert users of system restart requirement
/// </summary>
public class ClientUserRestartT : PTTransmission
{
    public const int UNIQUE_ID = 832;

    public override int UniqueId => UNIQUE_ID;

    #region IPTSerializable Members
    public ClientUserRestartT(IReader a_reader) : base(a_reader)
    {
        a_reader.Read(out m_restartClient);
        a_reader.Read(out m_restartMessage);

        a_reader.Read(out int count);
        for (int i = 0; i < count; i++)
        {
            m_affectedUsers.Add(new BaseId(a_reader));
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_restartClient);
        a_writer.Write(m_restartMessage);

        a_writer.Write(m_affectedUsers.Count);
        foreach (BaseId userId in m_affectedUsers)
        {
            userId.Serialize(a_writer);
        }
    }
    #endregion

    /// <summary>
    /// Whether to restart the client immediately
    /// </summary>
    private bool m_restartClient;

    public bool RestartClient
    {
        get => m_restartClient;
        set => m_restartClient = value;
    }

    /// <summary>
    /// List of affected user ids
    /// </summary>
    private List<BaseId> m_affectedUsers = new ();

    public List<BaseId> AffectedUsers
    {
        get => m_affectedUsers;
        set => m_affectedUsers = value;
    }

    /// <summary>
    /// Message to display to affected user
    /// </summary>
    private string m_restartMessage;

    public string RestartMessage
    {
        get => m_restartMessage;
        set => m_restartMessage = value;
    }

    public ClientUserRestartT() { }

    public ClientUserRestartT(List<BaseId> a_affectedUsers)
    {
        m_affectedUsers = a_affectedUsers;
    }
}