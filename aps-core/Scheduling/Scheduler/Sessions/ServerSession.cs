using PT.Common.Encryption;

namespace PT.Scheduler.Sessions;

public interface IServerSession
{
    public byte[] Handshake(string a_publicKey);
}

public class ServerSession : UserSession
{
    public ServerSession() : base(GenerateNewToken()) { }

    public byte[] Handshake(string a_publicKey)
    {
        using EncryptionHandshake handshake = new (a_publicKey);
        return handshake.Encrypt(m_symmetricKey);
    }
}