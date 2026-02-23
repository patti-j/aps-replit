namespace PT.Scheduler.Sessions;

internal interface IEncryptedSession
{
    public bool Encrypted { get; }
    public void Encrypt(byte[] a_symmetricKey);
    public byte[] SymmetricKey { get; }
}

public class EncryptedSession : CompressedSession, IEncryptedSession
{
    protected byte[] m_symmetricKey;

    public byte[] SymmetricKey => m_symmetricKey;

    public EncryptedSession(string a_sessionToken) : base(a_sessionToken) { }

    public bool Encrypted => m_symmetricKey != null;

    public void Encrypt(byte[] a_symmetricKey)
    {
        m_symmetricKey = a_symmetricKey;
    }

    protected override BinaryMemoryWriter GenerateWriter()
    {
        return new BinaryMemoryWriter(m_symmetricKey, CompressionType);
    }

    protected override BinaryMemoryReader GenerateReader(byte[] a_buffer)
    {
        return new BinaryMemoryReader(m_symmetricKey, a_buffer);
    }
}