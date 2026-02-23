using LZ4;
using PT.Common.Compression;
using System.IO.Compression;

namespace PT.Common;

/// <summary>
/// Summary description for FileDeserializer.
/// </summary>
public class BinaryFileWriter : BinaryStreamWriter, IDisposable
{
    private readonly FileStream m_fs;

    #region compression
    private readonly ECompressionType m_compressionType;

    public ECompressionType CompressionType => m_compressionType;

    private readonly Stream m_compressStream;
    #endregion

    private MemoryStream m_highCompressionStream;
    private BinaryWriter m_originalFileWriter;

    public BinaryFileWriter(string a_path, ECompressionType a_compressionType = ECompressionType.None)
    {
        //Try a few times to create the file in case it is in use by another program.
        int retry = 5;
        while (retry > 0)
        {
            try
            {
                m_fs = new FileStream(a_path, FileMode.Create);
                break;
            }
            catch (IOException)
            {
                Thread.Sleep(500);
                retry--;
            }
        }

        m_bw = new BinaryWriter(m_fs);
        m_compressionType = a_compressionType;

        m_bw.Write(Serialization.VersionNumber);
        m_bw.Write((int)m_compressionType);

        var assemblyVersion = AssemblyVersionChecker.GetAssemblyVersion();
        m_bw.Write(assemblyVersion.Major);
        m_bw.Write(assemblyVersion.Minor);
        m_bw.Write(assemblyVersion.Hotfix);
        m_bw.Write(assemblyVersion.Revision);

        switch (m_compressionType)
        {
            case ECompressionType.None:
                return;
            case ECompressionType.Fast:
                m_compressStream = new LZ4Stream(m_fs, LZ4StreamMode.Compress, false);
                break;
            case ECompressionType.Normal:
                m_compressStream = new BrotliStream(m_fs, CompressionLevel.Optimal, true);
                break;
            case ECompressionType.High:
                m_compressStream = new BrotliStream(m_fs, CompressionLevel.SmallestSize, true);
                break;
            case ECompressionType.Full:
                throw new NotImplementedException("High Compression is not ready");
                //TODO: This seems to work, but the reader gets an error reading the byte[] parts
                m_highCompressionStream = new MemoryStream();
                m_originalFileWriter = m_bw;
                m_bw = new BinaryWriter(m_highCompressionStream);
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        m_bw = new BinaryWriter(m_compressStream);
    }

    public override void Close()
    {
        base.Close();
        m_compressStream?.Close();
        m_highCompressionStream?.Close();
        m_fs.Close();
    }

    public override void Flush()
    {
        base.Flush();
        m_compressStream?.Flush();
        m_highCompressionStream?.Flush();
        m_fs.Flush();
    }

    private bool m_disposed;

    public void Dispose()
    {
        if (!m_disposed)
        {
            FinalizeWrite();
            m_compressStream?.Dispose();
            m_highCompressionStream?.Dispose();
            m_fs.Dispose();
            m_bw.Dispose();
            m_originalFileWriter?.Dispose();
            m_disposed = true;
        }
    }

    public void FinalizeWrite()
    {
        if (m_compressionType == ECompressionType.Full)
        {
            m_highCompressionStream.Flush();
            m_highCompressionStream.Position = 0;
            List<byte[]> compressedBytes = Compression.LZMA.SevenZipHelper.Compress(m_highCompressionStream);
            m_originalFileWriter.Write(compressedBytes.Count);
            foreach (byte[] bytes in compressedBytes)
            {
                m_originalFileWriter.Write(bytes);
            }
        }
    }
}