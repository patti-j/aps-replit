namespace PT.Transmissions;

/// <summary>
/// Transmission for messaging from Instance Manager.
/// </summary>
public class InstanceMessageT : PTTransmission
{
    public const int UNIQUE_ID = 1013;
    public override int UniqueId => UNIQUE_ID;

    public InstanceMessageT() { }

    public InstanceMessageT(IReader a_reader) : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out m_message);
            a_reader.Read(out m_shutdown);
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out long userId);
                m_affectedUsers.Add(userId);
            }

            a_reader.Read(out m_shutdownWarning);
        }
    }

    private string m_message;
    private bool m_shutdown;
    private List<long> m_affectedUsers = new ();
    private bool m_shutdownWarning;

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(Message);
        a_writer.Write(Shutdown);
        a_writer.Write(AffectedUsers.Count);
        foreach (long userId in AffectedUsers)
        {
            a_writer.Write(userId);
        }

        a_writer.Write(ShutdownWarning);
    }

    public InstanceMessageT(string a_message, bool a_shutdown, List<long> a_loggedInUsers, bool a_shutdownWarning)
    {
        Message = a_message;
        Shutdown = a_shutdown;
        AffectedUsers = a_loggedInUsers;
        ShutdownWarning = a_shutdownWarning;
    }

    public string Message
    {
        get => m_message;
        set => m_message = value;
    }

    public bool Shutdown
    {
        get => m_shutdown;
        set => m_shutdown = value;
    }

    public bool ShutdownWarning
    {
        get => m_shutdownWarning;
        set => m_shutdownWarning = value;
    }

    public List<long> AffectedUsers
    {
        get => m_affectedUsers;
        set => m_affectedUsers = value;
    }
}