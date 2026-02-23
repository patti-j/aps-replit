using PT.Common.Compression;
using PT.SystemServiceDefinitions;

namespace PT.Scheduler.Sessions;

internal interface ICompressedSession
{
    public bool Compressed { get; }
    public void SetCompression(ECompressionType a_compressionType);
    public ECompressionType CompressionType { get; }
}

public class CompressedSession : BaseSession, ICompressedSession
{
    protected ECompressionType m_compressionType;

    internal CompressedSession(string a_sessionToken) : base(a_sessionToken) { }

    public bool Compressed => m_compressionType != ECompressionType.None;

    public void SetCompression(ECompressionType a_compressionType)
    {
        m_compressionType = a_compressionType;
    }

    public ECompressionType CompressionType => m_compressionType;

    protected override BinaryMemoryWriter GenerateWriter()
    {
        return new BinaryMemoryWriter(CompressionType);
    }
}